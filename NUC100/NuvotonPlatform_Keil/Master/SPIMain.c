/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#include <stdio.h>
#include <string.h>
#include "NUC1xx.h"

#include "Driver/DrvSPI.h"
#include "Driver/DrvGPIO.h"
#include "cmd.h"


static SPI_T * SPI = SPI0;
static E_DRVSPI_PORT eSpiPort = eDRVSPI_PORT0;

#define V6M_AIRCR				0xE000ED0CUL
#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL

void SysTimerDelay(uint32_t us);

#define USE_INTERRUPT	1

#if USE_INTERRUPT
static volatile BOOL SPI_Int_Flag = TRUE;
#endif

#if (!USE_INTERRUPT)
#define RETRY_CNT	10//4

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

/*---------------------------------------------------------------------------------------------------------*/
/*  SPI (Master) Callback Function									                                   */
/*---------------------------------------------------------------------------------------------------------*/
#if USE_INTERRUPT
void SPI_Callback_Master(uint32_t userData)
{
	SPI_Int_Flag = TRUE;
}
#endif
#if USE_INTERRUPT
BOOL SPI_MasterSendData()
{
	int i;
	BOOL result = TRUE;

	SPI_Int_Flag = FALSE;
	for(i = 0; i<PACKET_SIZE; i++)
	{		
		SPI->TX[0] = sendbuf[i];
		SPI->CNTRL.GO_BUSY = 1;/*SPI go*/
		while(!SPI_Int_Flag);/*wait send finish*/
		SysTimerDelay(10);
		SPI_Int_Flag = FALSE;
	}

	return result;
}

BOOL SPI_MasterRcvData()
{
	int i;
	BOOL result = TRUE;
	
	SPI_Int_Flag = FALSE;
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.GO_BUSY = 1;
		while(!SPI_Int_Flag);
		rcvbuf[i] = SPI->RX[0];
		SysTimerDelay(10);
		SPI_Int_Flag = FALSE;
	}
	
	return result;
}
#else
BOOL SPI_MasterSendData()
{
	int i=0, cnt;
	BOOL result = TRUE;

#if 0	
	//send
	for(i = 0; i<PACKET_SIZE; i+=2)
	{
		SPI->CNTRL.IF = 1;
		SPI->TX[0] = sendbuf[i];
		SPI->TX[1] = sendbuf[i+1];
		SysTimerDelay(1);//1us
		SPI->CNTRL.GO_BUSY = 1;
		//while(SPI->CNTRL.GO_BUSY);
		while(SPI->CNTRL.IF==0);
		SysTimerDelay(1);//1us
	}
#else
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.IF = 1;/*clear interrupt flag*/
		SPI->TX[0] = sendbuf[i];
		SPI->CNTRL.GO_BUSY = 1;/*SPI go*/
		//while(SPI->CNTRL.IF==0);/*wait send finish*/
		WaitIF_Timeout();
		if(SPI->CNTRL.IF==0)
		{
			printf("send timeout\n");
			result = FALSE;
			break;
		}
		SysTimerDelay(10);//5us
	}
#endif
	return result;
}

BOOL SPI_MasterRcvData()
{
	int i=0, cnt;
	BOOL result = TRUE;
	
#if 0
	//rcv
	for(i = 0; i<PACKET_SIZE; i+=2)
	{
		SPI->CNTRL.GO_BUSY = 1;
		while(SPI->CNTRL.GO_BUSY);
		rcvbuf[i] = SPI->RX[0];
		rcvbuf[i+1] = SPI->RX[1];
	}
#else
	for(i = 0; i<PACKET_SIZE; i++)
	{
		SPI->CNTRL.IF = 1;/*clear interrupt flag*/
		SPI->CNTRL.GO_BUSY = 1;/*SPI go*/
		//while(SPI->CNTRL.IF==0);/*wait Rcv finish*/
		WaitIF_Timeout();
		if(SPI->CNTRL.IF==0)
		{
			printf("rcv timeout\n");
			result = FALSE;
			break;
		}
		rcvbuf[i] = SPI->RX[0];
		SysTimerDelay(10);//1us
	}
#endif
	
	return result;
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

//0.5us
void SysTimerDelay(uint32_t us)
{
    SysTick->LOAD = us * 11; /* using 22M cpu clock*/
    SysTick->VAL   =  (0x00);
    SysTick->CTRL = (1 << SYSTICK_CLKSOURCE) | (1<<SYSTICK_ENABLE);//using cpu clock

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);
}

/*
* init I2C slave
*/
static __inline void SPIInit(void)
{
	/* Configure SPI0 as a master, 32-bit transaction */
	DrvSPI_Open(eSpiPort, eDRVSPI_MASTER, eDRVSPI_TYPE4, 8);//eDRVSPI_TYPE1
	//SPI->CNTRL.TX_NUM = 1;//two tranceiver in one transfer
	/* Enable the automatic slave select function of SS0. */
	DrvSPI_EnableAutoCS(eSpiPort, eDRVSPI_SS0);
	/* Set the active level of slave select. */
	DrvSPI_SetSlaveSelectActiveLevel(eSpiPort, eDRVSPI_ACTIVE_LOW_FALLING);
	/* SPI clock rate 3MHz */
	DrvSPI_SetClock(eSpiPort, 2000000, 0);
	
#if USE_INTERRUPT
	DrvSPI_EnableInt(eSpiPort, SPI_Callback_Master, 0);
#endif	
}


BOOL SendData()
{
	BOOL Result;
	
	gcksum = Checksum(sendbuf, PACKET_SIZE);
	
	Result = SPI_MasterSendData();
	
	return Result;
}

BOOL RcvData()
{
	BOOL Result;
	unsigned short lcksum;
	uint8_t *pBuf;
	
	SysTimerDelay(100000);//50ms
	Result = SPI_MasterRcvData();
	
	if(Result == FALSE)
		return Result;
	
	pBuf = rcvbuf;
	WordsCpy(&lcksum, pBuf, 2);
	pBuf += 4;
	
	if(inpw(pBuf) != g_packno)
	{
		printf("g_packno=%d rcv %d\n", g_packno, inpw(pBuf));
		Result = FALSE;
	}
	else
	{
		if(lcksum != gcksum)
		{
			printf("gcksum=%x lcksum=%x\n", gcksum, lcksum);
			Result = FALSE;
		}
		g_packno++;
		
	}
	return Result;
}


int32_t main()
{
    extern uint32_t SystemFrequency;
  
    UNLOCKREG();
 
   	SYSCLK->PWRCON.OSC22M_EN = 1;
   	/* Waiting for 22M Xtal stable */
   	SysTimerDelay(5000);
   	FMC->ISPCON.ISPEN = 1;
   	UartInit();
   	SYSCLK->CLKSEL0.HCLK_S = 4;//4=22M;0=12M
    SystemFrequency = 22000000;
    
    printf("SPI Master %d, %d\n", &imageBegin, &imageEnd);
    
	/* Process USB event by interrupt */    
	SPIInit();
	
	CmdUpdateAprom(FALSE);

	return 0;
}







































































































