/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#include "UDC.h"


#define	V6M_AIRCR_VECTKEY_DATA	0x05FA0000UL
#define V6M_AIRCR_SYSRESETREQ	0x00000004UL

/*----------------------------------------------------------------------------
  MAIN function
 *----------------------------------------------------------------------------*/
int32_t main(void)
{
    volatile uint32_t u32INTSTS;
    
    UNLOCKREG();
    
    /* Check if GPB15 is low */
    if ( (inp32(GPIOB_BASE + 0x10) & BIT15) != 0)
    {
        /* Boot from AP */
        outpw(&SYS->RSTSRC, 3);//clear bit
	   	outpw(&FMC->ISPCON, inpw(&FMC->ISPCON) & 0xFFFFFFFC);
	   	outpw(&SCB->AIRCR, (V6M_AIRCR_VECTKEY_DATA | V6M_AIRCR_SYSRESETREQ));//SYSRESETREQ
        while(1);
    }

    /* Enable 12M Crystal */
    SYSCLK->PWRCON.XTL12M_EN = 1;
    RoughDelay(0x2000);                     

    /* Enable PLL */
    outp32(&SYSCLK->PLLCON, 0xC22E);
    RoughDelay(0x2000);

    /* Switch HCLK source to PLL */
    SYSCLK->CLKSEL0.HCLK_S = 2;
    
    /* Initialize USB Device function */

    /* Enable PHY to send bus reset event */
    _DRVUSB_ENABLE_USB();

    outp32(&USBD->DRVSE0, 0x01);
    RoughDelay(1000);
    outp32(&USBD->DRVSE0, 0x00);
    RoughDelay(1000);

    /* Disable PHY */
    _DRVUSB_DISABLE_USB();

    /* Enable USB device clock */
    outp32(&SYSCLK->APBCLK, BIT27);

    /* Reset IP */	
    outp32(&SYS->IPRSTC2, BIT27);
    outp32(&SYS->IPRSTC2, 0x0);	

    _DRVUSB_ENABLE_USB();
    outp32(&USBD->DRVSE0, 0x01);
    RoughDelay(1000);
    outp32(&USBD->DRVSE0, 0x00);

	g_u8UsbState = USB_STATE_DETACHED;
	_DRVUSB_TRIG_EP(1, 0x08);
	UsbFdt();

    /* Initialize mass storage device */
    udcFlashInit();  
      
    /* Start USB Mass Storage */

    /* Handler the USB ISR by polling */
	while(1)
	{
	    u32INTSTS = _DRVUSB_GET_EVF();

        if (u32INTSTS & EVF_FLD)
	    {
	        /* Handle the USB attached/detached event */
		    UsbFdt();
	    }
        else if(u32INTSTS & EVF_BUS)
	    {
	        /* Handle the USB bus event: Reset, Suspend, and Resume */
		    UsbBus();
	    }
        else if(u32INTSTS & EVF_USB)
	    {
	        /* Handle the USB Protocol/Clase event */
		    UsbUsb(u32INTSTS);
        }
	}
}



