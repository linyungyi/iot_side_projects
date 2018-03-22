/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#include <stdio.h>
#include <string.h>
#include "M05xx.h"

#include "Driver/DrvUART.h"
#include "Driver/DrvGPIO.h"
//#include "UART_register.h"
#include "FMC.h"

//#define SUPPORT_WRITECKSUM
//#define USING_GPIO
#define USING_AUTODETECT

#define CMD_UPDATE_APROM	0x000000A0
#define CMD_UPDATE_CONFIG	0x000000A1
#define CMD_READ_CONFIG		0x000000A2
#define CMD_ERASE_ALL		0x000000A3
#define CMD_SYNC_PACKNO		0x000000A4
#define CMD_GET_FWVER		0x000000A6
#define CMD_RUN_APROM		0x000000AB
#define CMD_RUN_LDROM		0x000000AC
#define CMD_RESET			0x000000AD
#define CMD_CONNECT			0x000000AE
#define CMD_DISCONNECT		0x000000AF

#define CMD_GET_DEVICEID	0x000000B1

#define CMD_UPDATE_DATAFLASH 	0x000000C3
#define CMD_WRITE_CHECKSUM 	 	0x000000C9
#define CMD_GET_FLASHMODE 	 	0x000000CA

#define CMD_RESEND_PACKET       0x000000FF

#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL

#define PACKET_SIZE	64

#define DISCONNECTED	0
#define CONNECTING		1
#define CONNECTED		2

static uint8_t volatile	bufhead;
static uint8_t volatile	g_connStatus;// = DISCONNECTED;
__align(4) static uint8_t uart_rcvbuf[64];
__align(4) static uint8_t uart_sendbuf[64];
__align(4) static uint8_t aprom_buf[PAGE_SIZE];
BOOL bUsbDataReady, bUartDataReady;
BOOL bUsbInReady, bUpdateApromCmd;
uint32_t g_apromSize, g_dataFlashAddr, g_dataFlashSize;
volatile uint32_t g_pdid, g_timecnt;
#ifdef SUPPORT_WRITECKSUM
static uint32_t g_ckbase = (0x20000 - 8);
static void CheckCksumBase(void);
#endif

void Delay(uint32_t delayCnt);
void my_memcpy(void *dest, void *src, int32_t size);
extern void WordsCpy(void *dest, void *src, int32_t size);

void NMI_Handler(void)
{
	SYS->BODCR.BOD_INTF = 1;
}


/*
* send data for UART
*/
static __inline void PutString()
{
	int i;
	
	for(i = 0; i<PACKET_SIZE; i++)
	{
		//while( UART0->FSR.TE == 0);
		while(UART0->FSR.TX_POINTER >= 14);
		UART0->DATA = uart_sendbuf[i];
	}
}

/*
* UART0 interrupt handler. handle receive only
*/
void UART0_IRQHandler(void)
{  
	/*----- Determine interrupt source -----*/
	
		if (inpw(&UART0->ISR/*COM_ISR*/) & 0x11) //RDA FIFO interrupt or RDA timeout interrupt 
   		{
   			while(((inpw(&UART0->FSR/*COM_FSR*/) & (0x4000)) == 0) && (bufhead < 64))//RX fifo not empty
				uart_rcvbuf[bufhead++] = inpw(&UART0->DATA/*COM_RX*/);
		}
	
		if(bufhead == 64)
		{
			bUartDataReady = TRUE;
			bufhead = 0;
		}

}


static __inline void UartInit(void)
{
    /* Multi-Function Pin: Enable UART0:Tx Rx */
    SYS->P3_MFP.MFP0_RXD0 		= 1;
	SYS->P3_MFP.ALT0_RXD0 		= 0;

	SYS->P3_MFP.MFP1_TXD0 		= 1;
	SYS->P3_MFP.ALT1_TXD0 		= 0;

    /* Configure GCR to reset UART0 */
    SYS->IPRSTC2.UART0_RST = 1;
    SYS->IPRSTC2.UART0_RST = 0;

    /* Enable UART clock */
    SYSCLK->APBCLK.UART0_EN = 1;

    /* Select UART clock source */
    SYSCLK->CLKSEL1.UART_S = 2;//0:12M; 2:22.1184M

    /* Data format */
    UART0->LCR.WLS = 3;

	outpw(&UART0->FCR, 0x32);//14
	UART0->TOR.TOIC = 0x40;

    /* Configure the baud rate */
    //BaudRateCalculator(SystemFrequency, 115200, &UART0->BAUD);
#if 0//12M
    //*((__IO uint32_t *)&UART0->BAUD) = 0x3F000066;//115200
    *((__IO uint32_t *)&UART0->BAUD) = 0xB;//57600
#else //22.1184M
	 *((__IO uint32_t *)&UART0->BAUD) = 0x2F00000A;//115200
	 //*((__IO uint32_t *)&UART0->BAUD) = 0x3F00017B;//0x16//57600
#endif

    NVIC_SetPriority (UART0_IRQn, 2);
    NVIC_EnableIRQ(UART0_IRQn);
    
    //UART0->IER.RDA_IEN = 1;UART0->IER.TOC_IEN = 1;UART0->IER.TOC_EN = 1;//Enable Timeout Counter
	outpw(&UART0->IER, 0x811);

}

void Delay(uint32_t delayCnt)
{
    while(delayCnt--)
    {
        __NOP();
        __NOP();
    }
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

int ParseCmd(unsigned char *buffer, uint8_t len, BOOL bUSB)
{
	static uint32_t StartAddress, StartAddress_bak, TotalLen, TotalLen_bak, LastDataLen, g_packno = 1;
	uint8_t *response;
	uint16_t cksum, lcksum;
	uint32_t	lcmd, packno, srclen, i, regcnf0, security;
	unsigned char *pSrc;
	static uint32_t	gcmd;
	
	
	response = uart_sendbuf;

	pSrc = buffer;
	srclen = len;
	//cksum = Checksum(pSrc, srclen);
	lcmd = inpw(pSrc);
	packno = inpw(pSrc + 4);
	outpw(response+4, 0);
	if((lcmd)&&(lcmd!=CMD_RESEND_PACKET))
		gcmd = lcmd;
	
	pSrc += 8;
	srclen -= 8;
	
	ReadData(Config0, Config0 + 8, (uint32_t*)(response+8));
	regcnf0 = *(uint32_t*)(response + 8);
	security = regcnf0 & 0x2;

	if(lcmd == CMD_SYNC_PACKNO)
	{
		g_packno = inpw(pSrc);
	}
	if((packno != g_packno) && ((lcmd != CMD_CONNECT))) //Connection state, packno always == 1
		goto out;

	if(lcmd == CMD_GET_FWVER)
		response[8] = FW_VERSION;//version 2.3
	//else if(lcmd == CMD_READ_CONFIG)
	//{
		//FMC_Read(Config0, (uint32_t*)(response+8));
		//FMC_Read(Config1, (uint32_t*)(response+12));
	//}
	else if(lcmd == CMD_GET_DEVICEID)
	{
		//outpw(response+8, g_pdid);
		outpw(response+8, SYS->PDID);
	}
	else if(lcmd == CMD_RUN_APROM || lcmd == CMD_RUN_LDROM || lcmd == CMD_RESET)
	{
		outpw(&SYS->RSTSRC, 3);//clear bit
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
		outpw(&SCB->AIRCR, (V6M_AIRCR_VECTKEY_DATA | V6M_AIRCR_SYSRESETREQ));

		/* Trap the CPU */
		while(1);
	}
	else if(lcmd == CMD_CONNECT)
	{
		g_connStatus = CONNECTED;
		g_packno = 1;
	}
	else if(lcmd == CMD_DISCONNECT)
	{
		g_connStatus = DISCONNECTED;
	}
		
	else if((lcmd == CMD_UPDATE_APROM) || (lcmd == CMD_ERASE_ALL))
	{
		
		if((regcnf0 & 0x4) && ((security == 0)||(lcmd == CMD_ERASE_ALL)))//erase APROM + data flash
		{
			EraseAP(FALSE, 0, (g_apromSize < 0x20000)?0x20000:g_apromSize);//erase all aprom including data flash
		}
		else
			EraseAP(TRUE, 0, 0);//don't erase data flash
		if(lcmd == CMD_ERASE_ALL)
		{
    		*(uint32_t*)(response + 8) = regcnf0|0x02;
    		UpdateConfig((uint32_t*)(response + 8), NULL);
		}
		else
			bUpdateApromCmd = TRUE;
	}


#ifdef SUPPORT_WRITECKSUM
	else if(lcmd == CMD_WRITE_CHECKSUM)//write cksum to aprom last
	{
		cktotallen = inpw(pSrc);
		lcksum = inpw(pSrc+4);
		CheckCksumBase();
		ReadData(g_ckbase & 0xFFE00, g_ckbase, (uint32_t*)aprom_buf);
		outpw(aprom_buf+PAGE_SIZE - 8, cktotallen);
		outpw(aprom_buf+PAGE_SIZE - 4, lcksum);
		FMC_Erase(g_ckbase & 0xFFE00);
		WriteData(g_ckbase & 0xFFE00, g_ckbase + 8, (uint32_t*)aprom_buf);
	}
#endif
	else if(lcmd == CMD_GET_FLASHMODE)
	{
		//return 1: APROM, 2: LDROM
		outpw(response+8, (inpw(&FMC->ISPCON)&0x2)? 2 : 1);
	}
	
	if((lcmd == CMD_UPDATE_APROM) || (lcmd == CMD_UPDATE_DATAFLASH))
	{
		if(lcmd == CMD_UPDATE_DATAFLASH)
		{
			StartAddress = g_dataFlashAddr;
		
			if(g_dataFlashSize)//g_dataFlashAddr
				EraseAP(FALSE, g_dataFlashAddr, g_dataFlashAddr + g_dataFlashSize);
			else goto out;
		}
		else
		{
			StartAddress = 0;
		}
		
		//StartAddress = inpw(pSrc);
		TotalLen = inpw(pSrc+4);
		pSrc += 8;
		srclen -= 8;
		StartAddress_bak = StartAddress;
		TotalLen_bak = TotalLen;

		//return 0;
	}
	else if(lcmd == CMD_UPDATE_CONFIG)
	{
		if((security == 0) && (!bUpdateApromCmd))//security lock
			goto out;
		
		UpdateConfig((uint32_t*)(pSrc), (uint32_t*)(response+8));
		GetDataFlashInfo(&g_dataFlashAddr, &g_dataFlashSize);

		goto out;
	}
	else if(lcmd == CMD_RESEND_PACKET)
	{  	   
	   StartAddress -= LastDataLen;
	   TotalLen += LastDataLen;
		if((StartAddress & 0xFFE00) >= Config0)
			goto out;
		ReadData(StartAddress & 0xFFE00, StartAddress, (uint32_t*)aprom_buf);
		FMC_Erase(StartAddress & 0xFFE00);
		WriteData(StartAddress & 0xFFE00, StartAddress, (uint32_t*)aprom_buf);
		if((StartAddress%PAGE_SIZE) >= (PAGE_SIZE-LastDataLen))
	    	FMC_Erase((StartAddress & 0xFFE00)+PAGE_SIZE);
		goto out;

	}
	
	if((gcmd == CMD_UPDATE_APROM) || (gcmd == CMD_UPDATE_DATAFLASH))
	{
		if(TotalLen >= srclen)
			TotalLen -= srclen;
		else{
			srclen = TotalLen;//prevent last package from over writing
			TotalLen = 0;
		}

		WriteData(StartAddress, StartAddress + srclen, (uint32_t*)pSrc);
		//memset(pSrc, 0, srclen);
		{
			uint32_t *p = (uint32_t*)pSrc;
			for(i = 0; i<srclen/4; i++)
				p[i] = 0;
		}
		
		ReadData(StartAddress, StartAddress + srclen, (uint32_t*)pSrc);
		StartAddress += srclen;
		LastDataLen =  srclen;
		
		if(TotalLen == 0)
		{
			lcksum = CalCheckSum(StartAddress_bak, TotalLen_bak);
			outps(response + 8, lcksum);
		}
	}
out:
	cksum = Checksum(buffer, len);
	//NVIC_DisableIRQ(USBD_IRQn);
	outps(response, cksum);
	++g_packno;
	outpw(response+4, g_packno);
	g_packno++;
	//NVIC_EnableIRQ(USBD_IRQn);
	//TotalLen -= srclen;

	return 0;
}

//0: timeout cleared by interrrupt
//1: timeout happen
int SysTimerDelay(uint32_t us)
{
    SysTick->LOAD = us * 11; /* using 22MHz cpu clock*/
    SysTick->VAL   =  (0x00);
    SysTick->CTRL = (1 << SysTick_CTRL_CLKSOURCE_Pos) 
				| (1<<SysTick_CTRL_ENABLE_Pos);//using cpu clock

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);

    return 1;
}

#ifdef SUPPORT_WRITECKSUM
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

uint8_t CheckCmd()
{
	int i;
	for(i = 0; i < bufhead; i++)
	{
		if(uart_rcvbuf[i] == 0xA4)//CMD_SYNC_PACKNO
			return TRUE;
	}
	return FALSE;
}
#endif


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


#define GET_ISPPIN_STATUS()	(PORT2->PIN & (1 << 2))//p2.2 for M05x

int32_t main()
{
    extern uint32_t SystemFrequency;
    volatile uint32_t pinst;
    uint8_t volatile	bufhead_bak=0;
#ifdef SUPPORT_WRITECKSUM
    uint32_t totallen, cksum;
#endif
  
    UNLOCKREG();
	
	SYSCLK->PWRCON.OSC22M_EN = 1;

    /* Waiting for 12M Xtal stalble */
    SysTimerDelay(5000);
    FMC->ISPCON.ISPEN = 1;

	g_apromSize = GetApromSize();
	GetDataFlashInfo(&g_dataFlashAddr, &g_dataFlashSize);
	
    g_pdid = SYS->PDID;
  
   	pinst = GET_ISPPIN_STATUS();//p2.2 for M05x
   

#if defined(USING_GPIO)
# ifdef SUPPORT_WRITECKSUM
	CheckCksumBase();
    
    FMC_Read(g_ckbase, &totallen);
	FMC_Read(g_ckbase+4, &cksum);

    if((pinst == 0) || ((inpw(&SYS->RSTSRC)&0x3) == 0x00)
    	|| (totallen > g_apromSize) || (CalCheckSum(0x0, totallen) != cksum))//if GPIO low or SYSRESETREQ reset or checksum error, run ISP
# else
	if((pinst == 0) || ((inpw(&SYS->RSTSRC)&0x3) == 0x00))//if GPIO low or SYSRESETREQ reset, run ISP
# endif

#else
	if(1)
#endif
    {
    	UartInit();
    }
    else//change to APROM
    {
_APROM:
		//if((inpw(&SYS->RSTSRC)&0x3) == 0x1)
		{ 	//after power on
			outpw(&SYS->RSTSRC, 3);//clear bit
	   		outpw(&FMC->ISPCON, inpw(&FMC->ISPCON) & 0xFFFFFFFC);
	   		outpw(&SCB->AIRCR, (V6M_AIRCR_VECTKEY_DATA | V6M_AIRCR_SYSRESETREQ));//SYSRESETREQ
		}
		
		/* Trap the CPU */
		while(1);
    }

    while(1)
    {
    	
#if defined(USING_AUTODETECT)
		if(g_connStatus == DISCONNECTED)
		{
			uint32_t	lcmd;

    		//timeout 25ms
    		SysTick->LOAD = 25000 * 22; /* using 22MHz cpu clock*/
    		SysTick->VAL   =  (0x00);
    		SysTick->CTRL = (1 << SysTick_CTRL_CLKSOURCE_Pos) 
					| (1<<SysTick_CTRL_ENABLE_Pos);//using cpu clock


    		while(1)
    		{
	    		if((bufhead >= 4) || (bUartDataReady == TRUE))
				{
    				lcmd = inpw(uart_rcvbuf);
					if(lcmd == CMD_CONNECT) 
					{
						break;
					}
					else
					{
						bUartDataReady = FALSE;
						bufhead = 0;
					}
				}

    			if((SysTick->CTRL & (1 << 16)) != 0)//timeout
    				goto _APROM;

    		}
    	}
#endif
    	
    	if(bUartDataReady == TRUE)
    	{
    		g_timecnt = 0;
    		ParseCmd(uart_rcvbuf, PACKET_SIZE, FALSE);
    		PutString();
    		bUartDataReady = FALSE;
    	}
#if defined(USING_GPIO)		
		pinst = GET_ISPPIN_STATUS();
    	if(pinst != 0)//P2.2 high
    		goto _APROM;
#endif
    	//timeout happen; but byte is less than 64 bytes; host goes wrong
		if(bufhead > 0)
		{
			if(g_timecnt == 0)
				bufhead_bak = bufhead;
			SysTimerDelay(1);//0.5us
			g_timecnt++;
			if(g_timecnt > 2000)//1ms
			{
				g_timecnt = 0;
				if(bufhead_bak == bufhead)
				{
					//g_connStatus = DISCONNECTED;
					bufhead = 0;
				}
			}
		}
	}

}








































































































