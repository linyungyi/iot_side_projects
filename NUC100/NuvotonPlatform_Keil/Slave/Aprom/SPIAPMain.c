/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
/*
	using SPI0(GPC0=SS00,GPC1=CLK,GPC2=MISO0,GPC3=MOSI0) as slave
*/
#include <stdio.h>
#include <string.h>
#include "NUC1xx.h"

#include "Driver/DrvSPI.h"
#include "Driver/DrvGPIO.h"
//#include "UART_register.h"
#include "FMC.h"

static SPI_T * SPI = SPI0;
static E_DRVSPI_PORT eSpiPort = eDRVSPI_PORT0;

#define CMD_READ_CONFIG		0x000000A2
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

#define PACKET_SIZE	64

#define USE_INTERRUPT	1

__align(4) static uint8_t rcvbuf[PACKET_SIZE];
__align(4) static uint8_t sendbuf[PACKET_SIZE];
__align(4) static uint8_t aprom_buf[PAGE_SIZE];
//#pragma arm section rwdata = ".bss"
BOOL bSPIDataReady;
uint32_t g_apromSize, g_dataFlashAddr, g_dataFlashSize;
uint32_t def_config[2] = {0xFFFFFF7F, 0x0001F000};
volatile uint32_t g_pdid;
static uint32_t g_ckbase = (0x20000 - 8);

#if USE_INTERRUPT
static volatile BOOL SPI_Int_Flag = TRUE;
#endif

void SysTimerDelay(uint32_t us);
static void CheckCksumBase(void);

#if (!USE_INTERRUPT)
#define RETRY_CNT	2000000//5s

#define WaitIF_Timeout() \
	cnt = 0;\
	while(SPI->CNTRL.IF == 0)\
	{\
		SysTimerDelay(1);\
		cnt++;\
		if(cnt >= RETRY_CNT)\
			break;\
	}
#endif

#if USE_INTERRUPT
void SPI_Slave_Callback(void)
{
	SPI_Int_Flag = TRUE;
}
#endif
#if USE_INTERRUPT
static void SPI_SlaveSendData()
{
	int i=0;

	SPI_Int_Flag = FALSE;
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->TX[0] = sendbuf[i];
		SPI->CNTRL.GO_BUSY = 1;
		while(!SPI_Int_Flag);
		SPI_Int_Flag = FALSE;
	}
}

static void SPI_SlaveRcvData()
{
	int i=0;

	SPI_Int_Flag = FALSE;
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.GO_BUSY = 1;
		while(!SPI_Int_Flag);
		rcvbuf[i] = SPI->RX[0];
		SPI_Int_Flag = FALSE;
	}

	if(i == PACKET_SIZE)
		bSPIDataReady = TRUE;
}
#else
static void SPI_SlaveSendData()
{
	int i=0, cnt;
#if 0	
	//send
	for(i = 0; i<PACKET_SIZE; i+=2)
	{
		SPI->CNTRL.IF = 1;
		SPI->TX[0] = sendbuf[i];
		SPI->TX[1] = sendbuf[i+1];
		SysTimerDelay(1);//0.5us
		SPI->CNTRL.GO_BUSY = 1;
		//while(SPI->CNTRL.GO_BUSY);
		while(SPI->CNTRL.IF == 0);
	}
#else
	//printf("send %x %x\n", sendbuf[0], sendbuf[1]);
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.IF = 1;
		SPI->TX[0] = sendbuf[i];
		SysTimerDelay(1);//0.5us
		SPI->CNTRL.GO_BUSY = 1;
		//while(SPI->CNTRL.IF == 0);

		WaitIF_Timeout();
		if(SPI->CNTRL.IF == 0)
		{
			printf("send timeout %d\n", i);
			break;
		}
	}
	//printf("send %d\n", i);
#endif
}

static void SPI_SlaveRcvData()
{
	int i=0, cnt;

#if 0	
	//rcv
	for(i = 0; i<PACKET_SIZE; i+=2)
	{
		SPI->CNTRL.IF = 1;
		SPI->CNTRL.GO_BUSY = 1;
		//while(SPI->CNTRL.GO_BUSY);
		while(SPI->CNTRL.IF == 0);
		rcvbuf[i] = SPI->RX[0];
		rcvbuf[i+1] = SPI->RX[1];
	}
#else
	//printf("rcv\n");
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.IF = 1;
		SPI->CNTRL.GO_BUSY = 1;
		//while(SPI->CNTRL.GO_BUSY);
		if(i == 0)
			while(SPI->CNTRL.IF == 0);
		else
		{
			WaitIF_Timeout();
			if(SPI->CNTRL.IF == 0)
			{
				printf("rcv timout %d\n", i);
				break;
			}
		}
		rcvbuf[i] = SPI->RX[0];
		//SysTimerDelay(1);//1us
	}
	//printf("end %d\n", i);
#endif
	if(i == PACKET_SIZE)
		bSPIDataReady = TRUE;
}
#endif

static __inline void UartInit(void)
{
    /* Multi-Function Pin: Enable UART0:Tx Rx */
    //SYS->GPBMFP.UART0_RX = 1;
    //SYS->GPBMFP.UART0_TX = 1;
    outpw(&SYS->GPBMFP, 0x03);

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
	UART0->TOR = 0x40;

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
    SysTick->CTRL = (1 << SYSTICK_CLKSOURCE) | (1<<SYSTICK_ENABLE);//using cpu clock

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);
}

/*
* init SPI slave
*/
void SPI_Callback(uint32_t u32UserData)
{
	//SPI_INT_Flag = 1;
}

static __inline void SPIInit(void)
{
	
	/* Configure SPI as a slaver, 32-bit transaction */
	DrvSPI_Open(eSpiPort, eDRVSPI_SLAVE, eDRVSPI_TYPE7, 8);//eDRVSPI_TYPE1
	//SPI->CNTRL.TX_NUM = 1;//two tranceiver in one transfer
	/* Set the trigger mode of slave select pin. */
	//DrvSPI_SetTriggerMode(eSpiPort, eDRVSPI_LEVEL_TRIGGER);   /* level trigger */
	DrvSPI_SetTriggerMode(eSpiPort, eDRVSPI_EDGE_TRIGGER);
	/* Set the active level of slave select. */
	DrvSPI_SetSlaveSelectActiveLevel(eSpiPort, eDRVSPI_ACTIVE_LOW_FALLING);

#if USE_INTERRUPT	
	/* Enable the SPI1 interrupt and install the callback function. */
	DrvSPI_EnableInt(eSpiPort, SPI_Slave_Callback, 0);
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
		SYS->RSTSRC.RSTS_PAD=1;//clear bit
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
    SYSCLK->PWRCON.XTL12M_EN = 1;
    SYSCLK->PWRCON.OSC22M_EN = 1;
    /* Waiting for 22M Xtal stable */
    SysTimerDelay(5000);
    /* ISPEN must be set before FMC_Read */
    FMC->ISPCON.ISPEN = 1;
    g_apromSize = GetApromSize();
	GetDataFlashInfo(&g_dataFlashAddr, &g_dataFlashSize);
		
    UartInit();
    SYSCLK->CLKSEL0.HCLK_S = 4;//4=22M;0=12M
    SystemFrequency = 22000000;
    
    printf("SPI APROM\n");
	/* Process USB event by interrupt */    
	SPIInit();
    

    while(1)
    {
    	SPI_SlaveRcvData();
    	if(bSPIDataReady == TRUE)
    	{
    		ParseCmd(rcvbuf, PACKET_SIZE, TRUE);
    		bSPIDataReady = FALSE;
    	}
    
    	SPI_SlaveSendData();
    	
    }

}







































































































