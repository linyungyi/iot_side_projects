/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
/*
	using I2C(P3.4 SDA,P3.5 SCL) as Master
*/
#include <stdio.h>
#include <string.h>
#include "M05xx.h"

#include "Driver/DrvI2C.h"
#include "Driver/DrvGPIO.h"
#include "cmd.h"


#define V6M_AIRCR				0xE000ED0CUL
#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL



//__align(4) uint8_t aprom_buf[PAGE_SIZE];
//uint8_t FileBuffer[FILE_BUFFER];
//#pragma arm section rwdata = ".bss"
static uint8_t Device_Addr0;

#define USE_INTERRUPT	1

#if USE_INTERRUPT
static volatile BOOL I2C_Read = TRUE;
static volatile BOOL Send_Result = TRUE;
static volatile BOOL Rcv_Result = TRUE;
static uint8_t Send_Cnt = 0;
static uint8_t Rcv_Cnt = 0;
static volatile uint8_t EndFlag = 0;
#endif

//static unsigned int g_packno = 1;
//static unsigned short gcksum

void SysTimerDelay(uint32_t us);

#define RETRY_CNT	10//4

#define START_SENT	    0x08
#define REPEAT_START_ACK	0x10
#define SLAW_SENT_ACK	0x18
#define DATA_SENT_ACK	0x28
#define SLAR_SENT_ACK	0x40
#define DATA_RCV_ACK	0x50

#if (!USE_INTERRUPT)
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
/*  I2C (Master) Callback Function									                                   */
/*---------------------------------------------------------------------------------------------------------*/
#if USE_INTERRUPT
void I2C_Callback_Master(uint32_t status)
{
	if (status == 0x08)					   	/* START has been transmitted and prepare SLA+W */
	{
		if(I2C_Read)
			DrvI2C_WriteData(Device_Addr0<<1 | 0x01);
		else
			DrvI2C_WriteData(Device_Addr0<<1);
		DrvI2C_Ctrl(0, 0, 1, 0);
	}	
	else if (status == 0x18)				/* SLA+W has been transmitted and ACK has been received */
	{
		DrvI2C_WriteData(sendbuf[Send_Cnt++]);
		DrvI2C_Ctrl(0, 0, 1, 0);
	}
	else if (status == 0x20)				/* SLA+W has been transmitted and NACK has been received */
	{
		DrvI2C_Ctrl(1, 1, 1, 0);
	}
	else if (status == 0x28)				/* DATA has been transmitted and ACK has been received */
	{
		if (Send_Cnt < PACKET_SIZE)
		{
			DrvI2C_WriteData(sendbuf[Send_Cnt++]);
			DrvI2C_Ctrl(0, 0, 1, 0);
		}
		else
		{
			DrvI2C_Ctrl(0, 1, 1, 0);
			EndFlag = 1;
		}		
	}
	else if (status == 0x10)				/* Repeat START has been transmitted */
	{
		DrvI2C_WriteData(Device_Addr0<<1 | 0x01);
		DrvI2C_Ctrl(0, 0, 1, 0);
	}
	else if (status == 0x40)				/* SLA+W has been transmitted and ACK has been received */
	{
		DrvI2C_Ctrl(0, 0, 1, 1);
	}
	else if (status == 0x48)				/* SLA+W has been transmitted and NACK has been received */
	{
		DrvI2C_Ctrl(1, 1, 1, 0);
	}
	else if (status == 0x50)				/* DATA has been transmitted and ACK has been received */
	{
		if (Rcv_Cnt < (PACKET_SIZE))
		{
			rcvbuf[Rcv_Cnt++] = DrvI2C_ReadData();
			DrvI2C_Ctrl(0, 0, 1, 1);
		}
		else
		{
			DrvI2C_Ctrl(0, 1, 1, 0);
			EndFlag = 1;
		}		
	}
	else
	{
		if(I2C_Read)
			Rcv_Result = FALSE;
		else
			Send_Result = FALSE;
		EndFlag = 1;
		printf("Status 0x%x is NOT processed\n", status);
	}			
}
#endif

#if USE_INTERRUPT	  // Use interrupt.
BOOL I2C_MasterSendData()
{
	BOOL result;
	
	EndFlag = 0;
	Send_Cnt = 0;
	I2C_Read = FALSE;
	/* I2C as master sends START signal */
	DrvI2C_Ctrl(1, 0, 1, 0);

	while (EndFlag == 0);
	result = Send_Result;
	Send_Result = TRUE;
	
	return result;
}

BOOL I2C_MasterRcvData()
{
	BOOL result;
	
	EndFlag = 0;
	Rcv_Cnt = 0;
	I2C_Read = TRUE;
	/* I2C as master sends START signal */
	DrvI2C_Ctrl(1, 0, 1, 0);

	while (EndFlag == 0);
	result = Rcv_Result;
	Rcv_Result = TRUE;

	return result;
}
#else	 // Use Polling.
BOOL I2C_MasterSendData()
{
	int i, cnt;
	BOOL result = TRUE;
	
	/* I2C0 as master sends START signal */
	DrvI2C_Ctrl(1, 0, 1, 0);
	WaitSI_Timeout();
	//while( I2C0->CON.SI == 0);
	
	//start signal sent; I2C0->STATUS == 0x08
	//and then send SLA+W to slave
	DrvI2C_WriteData(Device_Addr0<<1);
	DrvI2C_Ctrl(0, 0, 1, 0);//send SLA+W and receive AA
	WaitSI_Timeout();
	
	//SLA+W sent, ACK received
	if((I2C->CON.SI == 0) || (I2C->STATUS != 0x18))
	{
		result = FALSE;
		goto out;
	}

	i = 0;
	do
	{
		I2C->DATA = sendbuf[i];
		DrvI2C_Ctrl(0, 0, 1, 0);//send data and receive AA
		WaitSI_Timeout();
		
		//SI don't set or NACK received
		if((I2C->CON.SI == 0) || (I2C->STATUS != 0x28)) break;
		//printf("i %d\n", i);
		i++;
		
	}while(i<PACKET_SIZE);

out:
	DrvI2C_Ctrl(0, 1, 1, 0);//send STOP
	cnt = 0;	
	//while( I2C0->CON.STO == 1);

	while(I2C->CON.STO == 1)
	{
		SysTimerDelay(30);
		cnt++;
		if(cnt >= RETRY_CNT)
			break;
	}

	
	return result;
}

BOOL I2C_MasterRcvData()
{
	int i, cnt;
	BOOL result = TRUE;

	/* I2C0 as master sends START signal */
	DrvI2C_Ctrl(1, 0, 1, 0);
	WaitSI_Timeout();
	//while( I2C0->CON.SI == 0);
	
	//(I2C0->STATUS == 0x08) || (I2C0->STATUS == 0x10)
	DrvI2C_WriteData(Device_Addr0<<1 | 0x01);//SLA + R
	DrvI2C_Ctrl(0, 0, 1, 0);//send SLA+R and receive AA
	WaitSI_Timeout();
		
	if((I2C->CON.SI == 0) || (I2C->STATUS != 0x40)) 
	{
		result = FALSE;
		goto out;
	}
	
	i = 0;
	//for(i = 0; i<PACKET_SIZE; i++)
	do
	{
		DrvI2C_Ctrl(0, 0, 1, 1);//rcv data and send AA
		WaitSI_Timeout();
		
		//don't care NACK/ACK, the data is OK for me
		rcvbuf[i] = I2C->DATA;
		i++;
	}while(i<PACKET_SIZE);
out:	
	DrvI2C_Ctrl(0, 1, 1, 0);//send STOP
	//while( I2C0->CON.STO == 1);
	cnt = 0;
	while(I2C->CON.STO == 1)
	{
		SysTimerDelay(30);
		cnt++;
		if(cnt >= RETRY_CNT)
			break;
	}
	
	return result;
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
    //BaudRateCalculator(SystemCoreClock, 115200, &UART0->BAUD);
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
    SysTick->LOAD = us * 11; /* using 22MHz cpu clock*/
    SysTick->VAL   =  (0x00);
    SysTick->CTRL = (1 << SysTick_CTRL_CLKSOURCE_Pos) | (1<<SysTick_CTRL_ENABLE_Pos);

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);
}

BOOL SendData()
{
	BOOL Result;
	
	gcksum = Checksum(sendbuf, PACKET_SIZE);
	
	Result = I2C_MasterSendData();
	
	return Result;
}

BOOL RcvData()
{
	BOOL Result;
	unsigned short lcksum;
	uint8_t *pBuf;
	
	SysTimerDelay(100000);//50ms
	
	Result = I2C_MasterRcvData();
	
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

	/* Open I2C0 and I2C1, and set clock = 100Kbps */
	DrvI2C_Open(100000);
	

	/* Get I2C0 clock */
	u32data = DrvI2C_GetClockFreq();
	printf("I2C clock %d Hz\n", u32data);

#if USE_INTERRUPT
	/* Enable I2C interrupt and set corresponding NVIC bit */
	DrvI2C_EnableInt();
		
	/* Install I2C call back function for write and read data to or from slave */
	DrvI2C_InstallCallback(I2CFUNC, I2C_Callback_Master);
#endif
	
	Device_Addr0 = 0x36;

#if 0
	/* Uninstall I2C call back function for write data to slave */
	DrvI2C_UninstallCallBack(I2CFUNC);
	
	/* Disable I2C interrupt and clear corresponding NVIC bit */
	DrvI2C_DisableInt();

	/* Close I2C */
	DrvI2C_Close();
#endif
	
}



int32_t main()
{
    extern uint32_t SystemFrequency;
  
    UNLOCKREG();
	
	//SYSCLK->PWRCON.XTL12M_EN = 1;
   	SYSCLK->PWRCON.OSC22M_EN = 1;
   	/* Waiting for 12M Xtal stable */
   	SysTimerDelay(5000);
   	FMC->ISPCON.ISPEN = 1;
   	UartInit();
   	SYSCLK->CLKSEL0.HCLK_S = 4;//22M
	SystemCoreClock = 22000000;
    	
   	printf("I2C Master %d, %d\n", &imageBegin, &imageEnd);
    	
	/* Process USB event by interrupt */    
	I2CInit();
	
	CmdUpdateAprom(FALSE);
	
	while(1);

}







































































































