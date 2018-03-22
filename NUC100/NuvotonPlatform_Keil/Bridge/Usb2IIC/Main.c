/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#include <stdio.h>
#include <string.h>
#include "NUC1xx.h"

#include "Driver/DrvUSB.h"
#include "Driver/DrvGPIO.h"
#include "Driver/DrvI2C.h"
#include "USB/HIDDevice.h"
#include "USB/USBInfra.h"
#include "FMC.h"

//#define DBG_PRINTF          printf
#define DBG_PRINTF(...)          


#define CMD_UPDATE_APROM	0x000000A0
#define CMD_UPDATE_CONFIG	0x000000A1
#define CMD_READ_CONFIG		0x000000A2
#define CMD_ERASE_ALL		0x000000A3
#define CMD_SYNC_PACKNO		0x000000A4
#define CMD_GET_FWVER		0x000000A6
#define CMD_APROM_SIZE		0x000000AA
#define CMD_RUN_APROM		0x000000AB
#define CMD_RUN_LDROM		0x000000AC
#define CMD_RESET			0x000000AD
#define CMD_CONNECT			0x000000AE
#define CMD_DISCONNECT		0x000000AF

#define CMD_GET_DEVICEID	0x000000B1

#define CMD_PROGRAM_WOERASE 	0x000000C2
#define CMD_PROGRAM_WERASE 	 	0x000000C3
#define CMD_READ_CHECKSUM 	 	0x000000C8
#define CMD_WRITE_CHECKSUM 	 	0x000000C9
#define CMD_GET_FLASHMODE 	 	0x000000CA

#define CMD_RESEND_PACKET       0x000000FF

#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL

#define CONNECTING		1
#define CONNECTED		2
#define DISCONNECTED	3


#define PACKET_SIZE     64

#define RETRY_CNT	1000000//4


#define WaitSI_Timeout() \
	cnt = 0;\
	while(I2C0->CON.SI == 0)\
	{\
		SysTimerDelay(30);\
		cnt++;\
		if(cnt >= RETRY_CNT)\
			break;\
	}


E_USBINFRA_USB_STATE eUsbState;
static uint8_t volatile	bufhead;
static uint8_t volatile	g_connStatus = DISCONNECTED;
__align(4) static uint8_t rcvbuf[PACKET_SIZE];
__align(4) static uint8_t sendbuf[PACKET_SIZE];

volatile BOOL bUsbDataReady, bUartDataReady;
volatile BOOL bUsbInReady;
uint32_t g_apromSize=0x20000;
volatile uint32_t g_pdid, g_timecnt;

void Delay(uint32_t delayCnt);
void my_memcpy(void *dest, void *src, int32_t size);
extern void WordsCpy(void *dest, void *src, int32_t size);

// I2C
static uint8_t Device_Addr0;


/*
* USB interrupt handler
*/
void USBD_IRQHandler(void)
{
    //printf("USB relative events\n");
	USB_ParseEvent();
}

/*
* init HID usb
*/
static void UsbHid(void)
{
	//USB_Init();
	HID_Open();	
	
    NVIC_SetPriority (USBD_IRQn, 2);//(1<<__NVIC_PRIO_BITS) - 2);
    NVIC_EnableIRQ(USBD_IRQn);

	/* Enable float-detection interrupt. */
	_DRVUSB_ENABLE_FLD_INT();

	/* Enable USB-related interrupts. */
	_DRVUSB_ENABLE_MISC_INT(IEF_WAKEUP | IEF_WAKEUPEN | IEF_FLD | IEF_USB | IEF_BUS);

	
}



//unit is 0.5us
void SysTimerDelay(uint32_t us)
{
#ifndef UART_ONLY
    SysTick->LOAD = us * 24; /* using 48MHz cpu clock*/
#else
	SysTick->LOAD = us * 11; /* using 22MHz cpu clock*/
#endif
    SysTick->VAL   =  (0x00);
    SysTick->CTRL = (1 << SYSTICK_CLKSOURCE) | (1<<SYSTICK_ENABLE);//using cpu clock

    /* Waiting for down-count to zero */
    while((SysTick->CTRL & (1 << 16)) == 0);
}


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
    SYSCLK->CLKSEL1.UART_S = 0;//0:12M; 2:22.1184M

    /* Data format */
    UART0->LCR.WLS = 3;
	//UART0->FCR.RFR = 1;//reset rx fifo
	//UART0->FCR.RFITL = 5;//46
	outpw(&UART0->FCR, 0x52);
	UART0->TOR = 0x40;

    /* Configure the baud rate */
#if 1    
    //if SYSCLK->CLKSEL1.UART_S = 0
    //BaudRateCalculator(SystemFrequency, 115200, &UART0->BAUD);
    *((__IO uint32_t *)&UART0->BAUD) = 0x3F000066;//115200
    //*((__IO uint32_t *)&UART0->BAUD) = 0xB;//57600
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

BOOL I2C_MasterSendData()
{
	int i, cnt;
	BOOL result = TRUE;
	
	/* I2C0 as master sends START signal */
	DrvI2C_Ctrl(I2C_PORT0, 1, 0, 1, 0);
	WaitSI_Timeout();
	//while( I2C0->CON.SI == 0);
	
	//start signal sent; I2C0->STATUS == 0x08
	//and then send SLA+W to slave
	DrvI2C_WriteData(I2C_PORT0, Device_Addr0<<1);
	DrvI2C_Ctrl(I2C_PORT0, 0, 0, 1, 0);//send SLA+W and receive AA
	WaitSI_Timeout();
	
	//SLA+W sent, ACK received
	if((I2C0->CON.SI == 0) || (I2C0->STATUS != 0x18))
	{
		result = FALSE;
		goto out;
	}

	i = 0;
	do
	{
		I2C0->DATA = sendbuf[i];
		DrvI2C_Ctrl(I2C_PORT0, 0, 0, 1, 0);//send data and receive AA
		WaitSI_Timeout();
		
		//SI don't set or NACK received
		if((I2C0->CON.SI == 0) || (I2C0->STATUS != 0x28)) break;
		//printf("i %d\n", i);
		i++;
		
	}while(i<PACKET_SIZE);

out:
	DrvI2C_Ctrl(I2C_PORT0, 0, 1, 1, 0);//send STOP
	cnt = 0;	
	//while( I2C0->CON.STO == 1);

	while(I2C0->CON.STO == 1)
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
	DrvI2C_Ctrl(I2C_PORT0, 1, 0, 1, 0);
	WaitSI_Timeout();
	//while( I2C0->CON.SI == 0);
	
	//(I2C0->STATUS == 0x08) || (I2C0->STATUS == 0x10)
	DrvI2C_WriteData(I2C_PORT0, Device_Addr0<<1 | 0x01);//SLA + R
	DrvI2C_Ctrl(I2C_PORT0, 0, 0, 1, 0);//send SLA+R and receive AA
	WaitSI_Timeout();
		
	if((I2C0->CON.SI == 0) || (I2C0->STATUS != 0x40)) 
	{
		result = FALSE;
		goto out;
	}
	
	i = 0;
	//for(i = 0; i<PACKET_SIZE; i++)
	do
	{
		DrvI2C_Ctrl(I2C_PORT0, 0, 0, 1, 1);//rcv data and send AA
		WaitSI_Timeout();
		
		//don't care NACK/ACK, the data is OK for me
		rcvbuf[i] = I2C0->DATA;
		i++;
	}while(i<PACKET_SIZE);
out:	
	DrvI2C_Ctrl(I2C_PORT0, 0, 1, 1, 0);//send STOP
	//while( I2C0->CON.STO == 1);
	cnt = 0;
	while(I2C0->CON.STO == 1)
	{
		SysTimerDelay(30);
		cnt++;
		if(cnt >= RETRY_CNT)
			break;
	}
	
	return result;
}


/*
* init I2C slave
*/
static __inline void I2CInit(void)
{
	uint32_t u32data, u32HCLK;
	
	u32HCLK = SystemFrequency;//DrvSYS_GetHCLK() * 1000;


    /* Set I2C I/O */
    SYS->GPAMFP.I2C0_SDA 	=1;
	SYS->GPAMFP.I2C0_SCL 	=1;
	//set to IO_QUASI mode
	outpw((uint32_t)&GPIOA->PMD , inpw((uint32_t)&GPIOA->PMD) & ~(0xF0000));
    outpw((uint32_t)&GPIOA->PMD, inpw((uint32_t)&GPIOA->PMD) | (0xF0000));

	/* Open I2C0 and I2C1, and set clock = 100Kbps */
	DrvI2C_Open(I2C_PORT0, u32HCLK, 100000);
	

	/* Get I2C0 clock */
	u32data = DrvI2C_GetClock(I2C_PORT0, u32HCLK);
	printf("I2C0 clock %d Hz\n", u32data);

	/* Enable I2C0 interrupt and set corresponding NVIC bit */
	//DrvI2C_EnableInt(I2C_PORT0);
		
	/* Install I2C0 call back function for write data to slave */
	//DrvI2C_InstallCallback(I2C_PORT0, I2CFUNC, I2C0_Callback_Rx);
	
	Device_Addr0 = 0x36;

#if 0
	/* Uninstall I2C0 call back function for write data to slave */
	DrvI2C_UninstallCallBack(I2C_PORT0, I2CFUNC);
	
	/* Disable I2C0 interrupt and clear corresponding NVIC bit */
	DrvI2C_DisableInt(I2C_PORT0);

	/* Close I2C0 and I2C1 */
	DrvI2C_Close(I2C_PORT0);
#endif
	
}

#if 0

int ParseCmd(unsigned char *buffer, uint8_t len, BOOL bUSB)
{
	static uint32_t StartAddress, TotalLen, LastDataLen, g_packno = 1;
	uint8_t *response = g_HID_sDevice.au8IntInBuffer;
	uint16_t cksum, lcksum;
	uint32_t	lcmd, packno, srclen, cktotallen, ckstart, i;
	unsigned char *pSrc;
	static uint32_t	gcmd;
	
	if(!bUSB)
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
	
	if(lcmd == CMD_SYNC_PACKNO)
	{
		g_packno = inpw(pSrc);
	}
	if((packno != g_packno) && ((lcmd != CMD_CONNECT))) //Connection state, packno always == 1
		goto out;
	__NOP();
	__NOP();
	if(lcmd == CMD_GET_FWVER)
		response[8] = 0x21;//version 2.1
	else if(lcmd == CMD_READ_CONFIG)
	{
		FMC_Read(Config0, (uint32_t*)(response+8));
		FMC_Read(Config1, (uint32_t*)(response+12));
	}
	else if(lcmd == CMD_GET_DEVICEID)
	{
		//outpw(response+8, g_pdid);
		outpw(response+8, SYS->PDID);
	}
	else if(lcmd == CMD_APROM_SIZE)
	{
		g_apromSize = inpw(pSrc);
		if(g_apromSize < 0x1000)//4K
			g_apromSize = 0x20000;
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
		
	if((lcmd == CMD_UPDATE_APROM) || 
		(lcmd == CMD_UPDATE_CONFIG) || 
		(lcmd == CMD_ERASE_ALL))
	{
		if(lcmd == CMD_ERASE_ALL)
		{
			EraseAP(FALSE, 0, 0x20000);
			FMC->ISPCON.CFGUEN = 1;//enable config update
			FMC_Erase(Config0);
			FMC_Write(Config0, 0xFFFFFF7F);//boot from ISP
    		FMC_Write(Config1, 0x0001F000);
		}
		else
			EraseAP(TRUE, 0, 0);
	}
	
	if(lcmd == CMD_READ_CHECKSUM)
	{
		ckstart = inpw(pSrc);
		cktotallen = inpw(pSrc+4);
		lcksum = CalCheckSum(ckstart, cktotallen);
		outps(response + 8, lcksum);
	}
	else if(lcmd == CMD_GET_FLASHMODE)
	{
		//return 1: APROM, 2: LDROM
		outpw(response+8, (inpw(&FMC->ISPCON)&0x2)? 2 : 1);
	}
	
	else if((lcmd == CMD_UPDATE_APROM) || (lcmd == CMD_PROGRAM_WOERASE)
		|| (lcmd == CMD_PROGRAM_WERASE))
	{
		StartAddress = inpw(pSrc);
		TotalLen = inpw(pSrc+4);
		pSrc += 8;
		srclen -= 8;
		if(lcmd == CMD_PROGRAM_WERASE)
			EraseAP(FALSE, StartAddress, StartAddress + TotalLen);
		//printf("StartAddress=%x,TotalPadLen=%d\n",StartAddress, TotalPadLen);
		//return 0;
	}
	else if(lcmd == CMD_UPDATE_CONFIG)
	{
		UpdateConfig((uint32_t*)(pSrc), (uint32_t*)(response+8));
		//outps(response, cksum);
		__NOP();
		__NOP();
		goto out;
	}
	else if(lcmd == CMD_RESEND_PACKET)
	{  	   
	   StartAddress -= LastDataLen;
	   TotalLen += LastDataLen;
	   ReadData(StartAddress & 0xFFE00, StartAddress, (uint32_t*)aprom_buf);
	   FMC_Erase(StartAddress & 0xFFE00);
	   WriteData(StartAddress & 0xFFE00, StartAddress, (uint32_t*)aprom_buf);
	   if((StartAddress%PAGE_SIZE) >= (PAGE_SIZE-LastDataLen))
	     FMC_Erase((StartAddress & 0xFFE00)+PAGE_SIZE);
	   goto out;

	}
	
	if((gcmd == CMD_UPDATE_APROM) || (gcmd == CMD_PROGRAM_WOERASE) 
		|| (gcmd == CMD_PROGRAM_WERASE))
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

#endif

static BOOL bIsUpdate;
static uint32_t gu32TotalLength;

int ByPassCmd(unsigned char *buffer, uint8_t len)
{
    int32_t status;
    uint32_t cmd;

    /* Host didn't get previous status back, return error */
    if(bUsbInReady == TRUE)
        return FALSE;

    status = TRUE;

    memcpy(sendbuf, buffer, PACKET_SIZE);
    status = I2C_MasterSendData();
    if(!status) return status;
    
    cmd = inp32(buffer);
    
    DBG_PRINTF("cmd:%x\n", cmd);

    if((cmd == 0) && bIsUpdate && gu32TotalLength && 0) 
    {
        /* data phase of aprom update */
        DBG_PRINTF("pkt:%d  len:%d\n", inp32(&sendbuf[4]), gu32TotalLength);
        gu32TotalLength -= 56;

        if(gu32TotalLength <= 0)
        {
            bIsUpdate = 0;
            SysTimerDelay(100000);//50ms

            /* Get update status */
            status = I2C_MasterRcvData();
            if(!status) return status;
            memcpy(g_HID_sDevice.au8IntInBuffer, rcvbuf, PACKET_SIZE);
        	my_memcpy((void*) g_HID_ar8UsbBuf1, rcvbuf, HID_INT_BUFFER_SIZE);
        	
        	_DRVUSB_TRIG_EP(2,HID_MAX_PACKET_SIZE_EP1);
        
            bUsbInReady = TRUE;
            
            return TRUE;
        }    
    }
    else
    {

    
        /* Delay according to different commands */
        if((cmd == CMD_SYNC_PACKNO) || 
           (cmd == CMD_GET_FWVER) ||
           (cmd == CMD_READ_CHECKSUM) ||
           (cmd == CMD_GET_DEVICEID) ||
           (cmd == CMD_READ_CONFIG) ||
           (cmd == CMD_UPDATE_CONFIG) ||
           (cmd == CMD_APROM_SIZE) ||
           (cmd == CMD_ERASE_ALL) ||
           (cmd == CMD_GET_FLASHMODE) ||
          0)
        {
            //while(bUsbInReady == TRUE);
                    
    
        }
        else if((cmd == CMD_WRITE_CHECKSUM) || 
              0)
        {
            SysTimerDelay(400000);//0.2s
        }
        else if((cmd == CMD_RUN_APROM) || 
                (cmd == CMD_RUN_LDROM) || 
                (cmd == CMD_RESET) || 
              0)
        {
            //SysTimerDelay(1000000);//0.5s

            /* No ack */
            return TRUE;
        }
        else if(cmd == CMD_UPDATE_APROM)
        {
    
            bIsUpdate = 1;
            gu32TotalLength =  inp32(&sendbuf[0xc]);
            DBG_PRINTF("Write AP, size:%d\n", gu32TotalLength);
            gu32TotalLength -= 48;

         	//for(i = 0; i < 16; i++)
        	//	SysTimerDelay(1000000);//0.5s
    
       }
        else
        {
            //SysTimerDelay(100000);//50ms
        }
    
        status = I2C_MasterRcvData();
        if(!status) while(1);
        //if(!status) return status;
        memcpy(g_HID_sDevice.au8IntInBuffer, rcvbuf, PACKET_SIZE);
    	my_memcpy((void*) g_HID_ar8UsbBuf1, rcvbuf, HID_INT_BUFFER_SIZE);
    	
    	_DRVUSB_TRIG_EP(2,HID_MAX_PACKET_SIZE_EP1);
    
        bUsbInReady = TRUE;
    
        DBG_PRINTF("Rcv: %x\n", inp32(rcvbuf));
    
    }

    return status;
}

int32_t main()
{
    extern uint32_t SystemFrequency;
    volatile uint32_t pinst;
    uint8_t volatile	bufhead_bak;
	
    UNLOCKREG();

	SYSCLK->PWRCON.XTL12M_EN = 1;

    /* Waiting for 12M/22M Xtal stalble */
    SysTimerDelay(5000);
    SYSCLK->CLKSEL0.HCLK_S = 4;//4:22M
    FMC->ISPCON.ISPEN = 1;


    /* Enable PLL */
	SYSCLK->PLLCON.OE = 0;
	SYSCLK->PLLCON.PD = 0;
    //Delay(1000);
    SysTimerDelay(100);

	SYSCLK->CLKSEL0.HCLK_S = 2;
    SysTimerDelay(100);

	/* The PLL must be 48MHz x N times when using USB */
    //SystemFrequency = 48000000;



    bUsbDataReady = 0;
    bUartDataReady = 0;
    bUsbInReady = 0;
    bIsUpdate = 0;
    gu32TotalLength = 0;

    UartInit();



	I2CInit();

   
	/* Process USB event by interrupt */    
	UsbHid();

    DBG_PRINTF(" >> ISP to I2C converter <<\n");

    while(1)
    {
    
	   	if(bUsbDataReady == TRUE)
    	{
    		//ParseCmd(g_HID_sDevice.au8IntOutBuffer, HID_MAX_PACKET_SIZE_EP1, TRUE);
    		ByPassCmd(g_HID_sDevice.au8IntOutBuffer, HID_MAX_PACKET_SIZE_EP1);
            
            bUsbDataReady = FALSE;
			_DRVUSB_TRIG_EP(3,HID_MAX_PACKET_SIZE_EP1);
    	}
	}

}

void my_memcpy(void *dest, void *src, int32_t size)
{
	WordsCpy(dest, src, size);
}







































































































