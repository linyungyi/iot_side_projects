/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
/*
	using I2C(P3.4 SDA,P3.5 SCL) as slave
*/
#include <stdio.h>
#include <string.h>
#include "M05xx.h"

#include "Driver/DrvI2C.h"
#include "Driver/DrvGPIO.h"
//#include "UART_register.h"
#include "FMC.h"

#define CMD_UPDATE_APROM	0x000000A0
#define CMD_UPDATE_CONFIG	0x000000A1
#define CMD_READ_CONFIG		0x000000A2
#define CMD_ERASE_ALL		0x000000A3
#define CMD_SYNC_PACKNO		0x000000A4
#define CMD_GET_FWVER		0x000000A6
#define CMD_RUN_APROM		0x000000AB
#define CMD_RUN_LDROM		0x000000AC
#define CMD_RESET			0x000000AD

#define CMD_GET_DEVICEID	0x000000B1

#define CMD_WRITE_CHECKSUM 	 	0x000000C9
#define CMD_GET_FLASHMODE 	 	0x000000CA


#define V6M_AIRCR				0xE000ED0CUL
#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL

#define PACKET_SIZE	64//32

#define USE_INTERRUPT	1

__align(4) static uint8_t rcvbuf[PACKET_SIZE];
__align(4) static uint8_t sendbuf[PACKET_SIZE];
__align(4) static uint8_t aprom_buf[PAGE_SIZE];
//#pragma arm section rwdata = ".bss"
BOOL bI2CDataReady;
uint32_t g_apromSize, g_dataFlashAddr, g_dataFlashSize;
uint32_t def_config[2] = {0xFFFFFF7F, 0x0001F000};
volatile uint32_t g_pdid;
static uint32_t g_ckbase = (0x20000 - 8);

#if USE_INTERRUPT
static volatile BOOL I2C_Read = FALSE;
static uint8_t Send_Cnt = 0;
static uint8_t Rcv_Cnt = 0;
static volatile uint8_t EndFlag = 0;
#endif

void SysTimerDelay(uint32_t us);
static void CheckCksumBase(void);

#if (!USE_INTERRUPT)
#define RETRY_CNT	10//4

#define WaitSI_Timeout() \
	cnt = 0;\
	while(I2C->CON.SI == 0)\
	{\
		SysTimerDelay(30);\
		cnt++;\
		if(cnt >= RETRY_CNT)\
			break;\
	}
#endif

/*---------------------------------------------------------------------------------------------------------*/
/*  I2C (Slave) Callback Function									                                   */
/*---------------------------------------------------------------------------------------------------------*/
#if USE_INTERRUPT
void I2C_Callback_Slave(uint32_t status)
{	
	if ((status==0xA8) || (status==0xB0))	/* Own SLA+W has been received and ACK has been return */
	{
		I2C_Read = FALSE;
		Send_Cnt = 0;
		DrvI2C_WriteData(sendbuf[Send_Cnt++]);
		DrvI2C_Ctrl(0, 0, 1, 1);
	}
	else if (status == 0xB8)				/* Data byte has been transmitted and ACK has been received */
	{
		if (Send_Cnt < (PACKET_SIZE))
		{
			DrvI2C_WriteData(sendbuf[Send_Cnt++]);
			DrvI2C_Ctrl(0, 0, 1, 1);
		}
		else
		{
			DrvI2C_Ctrl(0, 0, 1, 1);
			EndFlag = 1;
		}
	}
	if ((status==0x60) || (status==0x68))	/* Own SLA+W has been received and ACK has been return */
	{
		I2C_Read = TRUE;
		Rcv_Cnt = 0;
		bI2CDataReady = TRUE;
		DrvI2C_Ctrl(0, 0, 1, 1);
	}
	else if (status == 0x80)				/* Data byte has been received and ACK has been received */
	{
		if (Rcv_Cnt < (PACKET_SIZE-1))
		{
			rcvbuf[Rcv_Cnt++] = DrvI2C_ReadData();
			DrvI2C_Ctrl(0, 0, 1, 1);
		}
		else
		{
			rcvbuf[Rcv_Cnt++] = DrvI2C_ReadData();
			DrvI2C_Ctrl(0, 0, 1, 1);
			EndFlag = 1;
		}
	}
	else if (status == 0xA0)				/* A STOP or repeated START has been received while still addressed as SLA/REC */
	{
		DrvI2C_Ctrl(0, 0, 1, 1);
	}
	else
	{
		if(I2C_Read)
			bI2CDataReady = FALSE;
		EndFlag = 1;
		//printf("Status 0x%x is NOT processed\n", status);
	}			
}
#endif
#if USE_INTERRUPT
static void I2C_SlaveRcvSendData()
{
	EndFlag = 0;
	
	/* I2C0 as slave set AA*/
	DrvI2C_Ctrl(0, 0, 1, 1);

	while(EndFlag == 0);
}
#else
static void I2C_SlaveRcvSendData()
{
	int i, cnt;
	
	do{
		/* I2C as slave set AA*/
		DrvI2C_Ctrl(0, 0, 1, 1);
		while( I2C->CON.SI == 0);
	}while((I2C->STATUS != 0xA8) && (I2C->STATUS != 0xB0)
			&& (I2C->STATUS != 0x60) && (I2C->STATUS != 0x68));//wait SLA+R/SLA+W again
	
	if((I2C->STATUS == 0xA8) || (I2C->STATUS == 0xB0))/* SLA+R has been received and ACK has been returned */
	{
		//printf("start sent\n");
		
		//for(i = 0; i<PACKET_SIZE; i++)
		i = 0;
		do
		{
			I2C->DATA = sendbuf[i];
			DrvI2C_Ctrl(0, 0, 1, 1);//send data and receive AA
			//while( I2C0->CON.SI == 0);
			WaitSI_Timeout();
			
			////Timeout or NACK received, break and don't resent
			if((I2C->CON.SI == 0) || (I2C->STATUS != 0xB8))
				break;
			i++;
		}while(i<PACKET_SIZE);
	}
	else if((I2C->STATUS == 0x60) || (I2C->STATUS == 0x68))/* SLA+W has been received and ACK has been returned */
	{
		//printf("start recv\n");
		bI2CDataReady = TRUE;

		i = 0;
		do
		{
			DrvI2C_Ctrl(0, 0, 1, 1);//rcv data and send AA
			WaitSI_Timeout();
			//printf("i %d\n", i);
			if((I2C->CON.SI == 0) || (I2C->STATUS != 0x80))
			{
				bI2CDataReady = FALSE;
				break;
			}
			rcvbuf[i] = I2C->DATA;
			i++;
		}while(i<PACKET_SIZE);
	}
	//wait stop signal
	DrvI2C_Ctrl(0, 0, 1, 1);
	WaitSI_Timeout();
}
#endif

static __inline void UartInit(void)
{
    /* Multi-Function Pin: Enable UART0:Tx Rx */
    outpw(&SYS->P3_MFP, (inpw(&SYS->P3_MFP) & ~(0x3<<8)) | (0x3));
	outpw(&SYS->P0_MFP, (inpw(&SYS->P0_MFP) | (0x3<<10)) & ~(0x3<<2));

    /* Configure GCR to reset UART0 */
    SYS->IPRSTC2.UART0_RST = 1;
    SYS->IPRSTC2.UART0_RST = 0;

    /* Enable UART clock */
    SYSCLK->APBCLK.UART0_EN = 1;

    /* Select UART clock source */
    SYSCLK->CLKSEL1.UART_S = 2;//0:12M; 2:22.1184M

    /* Data format */
    UART0->LCR.WLS = 3;
	//UART0->FCR.RFR = 1;//reset rx fifo
	//UART0->FCR.RFITL = 5;//46
	outpw(&UART0->FCR, 0x52);
	UART0->TOR.TOIC = 0x40;

    /* Configure the baud rate */
#if 0    
    //if SYSCLK->CLKSEL1.UART_S = 0
    //BaudRateCalculator(SystemFrequency, 115200, &UART0->BAUD);
    //*((__IO uint32_t *)&UART0->BAUD) = 0x3F000066;//115200
    *((__IO uint32_t *)&UART0->BAUD) = 0xB;//57600
#else
    //if SYSCLK->CLKSEL1.UART_S = 2//22.1184M
    *((__IO uint32_t *)&UART0->BAUD) = 0x2F00000A;//115200
    //*((__IO uint32_t *)&UART0->BAUD) = 0x16;//57600
#endif
    
    //NVIC_SetPriority (UART0_IRQn, 2);
    //NVIC_EnableIRQ(UART0_IRQn);
    
    //UART0->IER.RDA_IEN = 1;UART0->IER.TOC_IEN = 1;UART0->IER.TOC_EN = 1;//Enable Timeout Counter
	//outpw(&UART0->IER, 0x811);
}

void SysTimerDelay(uint32_t us)
{
    SysTick->LOAD = us * 11; /* using 22/2MHz cpu clock*/
    SysTick->VAL   =  (0x00);
    SysTick->CTRL = (1 << SysTick_CTRL_CLKSOURCE_Pos) | (1<<SysTick_CTRL_ENABLE_Pos);//using cpu clock

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);
}

/*
* init I2C slave
*/
static __inline void I2CInit(void)
{
	uint32_t u32data;

    /* Set I2C I/O */
    outpw(&SYS->P3_MFP, (inpw(&SYS->P3_MFP) | (0x3<<12)) & ~(0x3<<4));

	//set to IO_QUASI mode
	outpw((uint32_t)&PORT3->PMD, inpw((uint32_t)&PORT3->PMD) & ~(0xF00));
    outpw((uint32_t)&PORT3->PMD, inpw((uint32_t)&PORT3->PMD) | (0xF00));

	/* Open I2C, and set clock = 100Kbps */
	DrvI2C_Open(100000);
	

	/* Get I2C clock */
	u32data = DrvI2C_GetClockFreq();
	printf("I2C clock %d Hz\n", u32data);

	/* Set I2C0 slave addresses */
	DrvI2C_SetAddress(0, 0x36, 0);
	
	/* Set AA bit, I2C as slave */
	//DrvI2C_Ctrl(0, 0, 1, 1);
		
	/* Enable I2C interrupt and set corresponding NVIC bit */
	//DrvI2C_EnableInt();
		
	/* Install I2C call back function for slave */
	//DrvI2C_InstallCallback(I2CFUNC, I2C_Callback_Slave);
#if USE_INTERRUPT		
	/* Enable I2C0 interrupt and set corresponding NVIC bit */
	DrvI2C_EnableInt();
		
	/* Install I2C0 call back function for slave */
	DrvI2C_InstallCallback(I2CFUNC, I2C_Callback_Slave);
#endif

#if 0
	/* Uninstall I2C call back function for write data to slave */
	DrvI2C_UninstallCallBack(I2CFUNC);
	
	/* Disable I2C interrupt and clear corresponding NVIC bit */
	DrvI2C_DisableInt();

	/* Close I2C*/
	DrvI2C_Close();
#endif
	
}

static uint16_t Checksum (unsigned char *buf, int len)
{
    int i;
    uint16_t c;

    for (c=0, i=0; i < len; i++) {
        c += buf[i];
    }
    return (c);
}

static uint16_t CalCheckSum(uint32_t start, uint32_t len)
{
	int i;
	uint16_t lcksum = 0;
	
	for(i = 0; i < len; i+=PAGE_SIZE)
	{
		ReadData(start + i, start + i + PAGE_SIZE, (uint32_t*)aprom_buf);
		if(len - i >= PAGE_SIZE)
			lcksum += Checksum(aprom_buf, PAGE_SIZE);
		else
			lcksum += Checksum(aprom_buf, len - i);
	}
    
    return lcksum;
    
}

static int ParseCmd(unsigned char *buffer, uint8_t len, BOOL flag)
{
	static uint32_t g_packno = 1;
	uint8_t *response ;
	uint16_t cksum, lcksum;
	uint32_t	lcmd, packno, srclen, cktotallen, ckstart, i;
	unsigned char *pSrc;
	static uint32_t	gcmd;
	

	response = sendbuf;
	pSrc = buffer;
	srclen = len;

	lcmd = inpw(pSrc);
	packno = inpw(pSrc + 4);
	outpw(response+4, 0);
	if(lcmd)
		gcmd = lcmd;

	pSrc += 8;
	srclen -= 8;
	
	if(lcmd == CMD_SYNC_PACKNO)
	{
		g_packno = inpw(pSrc);
	}
	if(packno != g_packno)
		goto out;
//printf("aprom %x\n", lcmd);

	if(lcmd == CMD_GET_FWVER)
		response[8] = FW_VERSION;//version 2.3
	else if(lcmd == CMD_READ_CONFIG)
	{
		ReadData(Config0, Config0 + 8, (uint32_t*)(response+8));
	}
	else if(lcmd == CMD_GET_DEVICEID)
	{
		outpw(response+8, SYS->PDID);
	}


	else if(lcmd == CMD_RUN_APROM || lcmd == CMD_RUN_LDROM || lcmd == CMD_RESET)
	{
		SYS->RSTSRC.RSTS_POR=1;//clear bit
		SYS->RSTSRC.RSTS_RESET=1;//clear bit
		/* Set BS */		
		if(lcmd == CMD_RUN_APROM)
		{
			i = (inpw(&FMC->ISPCON) & 0xFFFFFFFC);
		}
		else if(lcmd == CMD_RUN_LDROM)
		{
			i = (inpw(&FMC->ISPCON) & 0xFFFFFFFC);
			i |= 0x00000002;
		}
		else
		{
			i = (inpw(&FMC->ISPCON) & 0xFFFFFFFE);//ISP disable
		}

		outpw(&FMC->ISPCON, i);
		outpw(V6M_AIRCR, (V6M_AIRCR_VECTKEY_DATA | V6M_AIRCR_SYSRESETREQ));

		/* Trap the CPU */
		while(1);
	}
	
	if(lcmd == CMD_WRITE_CHECKSUM)
	{
		cktotallen = inpw(pSrc);
		lcksum = inpw(pSrc+4);
		//printf("cktotallen=%x,lcksum=%x\n",cktotallen, lcksum);
		printf("cksum\n");
		CheckCksumBase();
		ReadData(g_ckbase & 0xFFE00, g_ckbase, (uint32_t*)aprom_buf);
		outpw(aprom_buf+PAGE_SIZE - 8, cktotallen);
		outpw(aprom_buf+PAGE_SIZE - 4, lcksum);
		FMC_Erase(g_ckbase & 0xFFE00);
		WriteData(g_ckbase & 0xFFE00, g_ckbase + 8, (uint32_t*)aprom_buf);
	}
	else if(lcmd == CMD_GET_FLASHMODE)
	{
		//return 1: APROM, 2: LDROM
		outpw(response+8, (inpw(&FMC->ISPCON)&0x2)? 2 : 1);
	}
	
out:
	cksum = Checksum(buffer, len);
	outph(response, cksum);
	++g_packno;
	outpw(response+4, g_packno);
	g_packno++;

	return 0;
}

static void CheckCksumBase()
{
	//skip data flash
	unsigned int aprom_end = g_apromSize, data;
	int result;
	
	result = FMC_Read(0x1F000 - 8, &data);
	if(result == 0)//128K flash
    {
    	FMC_Read(Config0, &data);
    	if((data&0x01)==0)//DFEN enable
    	{
    		FMC_Read(Config1, &aprom_end);
    	}
    	g_ckbase = aprom_end - 8;
    	return;
    }
    else// less than 128K
    {
    	aprom_end = 0x10000;//64K
    	do{
		    result = FMC_Read(aprom_end - 8, &data);
		    if(result == 0)
		    	break;
    		aprom_end = aprom_end/2;
    	}while(aprom_end > 4096);

		g_ckbase = aprom_end - 8;
	}
}

//the smallest of APROM size is 2K 
static uint32_t GetApromSize()
{
	uint32_t size = 0x800, data;
	int result;
	
	do{
		result = FMC_Read(size, &data);
		if(result < 0)
	    {
    		return size;
		}
		else 
			size *= 2;
	}while(1);
}



int32_t main()
{
    extern uint32_t SystemFrequency;
  
    UNLOCKREG();

    g_pdid = SYS->PDID;
   
	//SYSCLK->PWRCON.XTL12M_EN = 1;
   	SYSCLK->PWRCON.OSC22M_EN = 1;
   	/* Waiting for 22M Xtal stable */
   	SysTimerDelay(5000);
   	FMC->ISPCON.ISPEN = 1;
   	g_apromSize = GetApromSize();
	GetDataFlashInfo(&g_dataFlashAddr, &g_dataFlashSize);
   	UartInit();
   	SYSCLK->CLKSEL0.HCLK_S = 4;//22M
    
    printf("I2C APROM\n");
	/* Process USB event by interrupt */    
	I2CInit();

    while(1)
    {
    	I2C_SlaveRcvSendData();
    	if(bI2CDataReady == TRUE)
    	{
    		ParseCmd(rcvbuf, PACKET_SIZE, TRUE);
    		bI2CDataReady = FALSE;
    	}
    	
    }

}







































































































