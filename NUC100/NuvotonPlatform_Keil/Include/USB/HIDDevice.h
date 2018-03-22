/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#ifndef __HIDDEVICE_H__
#define __HIDDEVICE_H__

#include "NUC1xx.h"
#include "System/SysInfra.h"
#include "USB/USBInfra.h"



#ifdef  __cplusplus
extern "C"
{
#endif

#define HID_MAJOR_NUM	1
#define HID_MINOR_NUM	00
#define HID_BUILD_NUM	1

  
// E_HID_BUFFER_OVERRUN                 Allocated buffer is not enough.
// E_HID_CTRL_REG_TAB_FULL              Control register table is full.
// E_HID_EXCEED_INT_IN_PACKET_SIZE      Report size must be less than packet size of interrupt.
// E_HID_INVALID_EP_NUM                 Invalid EP number.
// E_HID_MUST_LESS_THAN_PACKET0_SIZE    Data size in control must be less than packet0 size.
// E_HID_NULL_POINTER                   NULL pointer.
// E_HID_UNDEFINE                       Undefined error.
// E_HID_INVALID_REG_NUM				  			Invaild register unmber
   
#define E_HID_UNDEFINE                      _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 0)
#define E_HID_NULL_POINTER                  _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 1)
#define E_HID_BUFFER_OVERRUN                _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 2)
#define E_HID_INVALID_EP_NUM                _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 3)   
#define E_HID_MUST_LESS_THAN_PACKET0_SIZE   _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 4)
#define E_HID_EXCEED_INT_IN_PACKET_SIZE     _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 5)
#define E_HID_CTRL_REG_TAB_FULL             _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 6)
#define E_HID_INVALID_REG_NUM               _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 7)
//#define E_DRVUSB_INVALID_EP_NUM              _SYSINFRA_ERRCODE(TRUE, MODULE_ID_HID, 8)
   
   
  
_SYSINFRA_VERSION_DEF(HID, HID_MAJOR_NUM, HID_MINOR_NUM, HID_BUILD_NUM)


#define INT_IN_EP_NUM	0x01

#define	CFG_CTRL_IN_EP0_SETTING		(CFGx_STALL_CTL | CFGx_EPT_IN | 0)
#define	CFG_CTRL_OUT_EP0_SETTING	(CFGx_STALL_CTL | CFGx_EPT_OUT | 0)
#define CFG_INT_IN_EP1_SETTING		(CFGx_EPT_IN | INT_IN_EP_NUM)

   
// Max packet size of EP0
#define	HID_MAX_PACKET_SIZE_EP0		64
// Max packet size of EP1
#define HID_MAX_PACKET_SIZE_EP1		64//8

#define USB_SRAM    USB_SRAM_BASE
// For Contorl In and Control Out
#define	HID_USB_BUF_0			(USB_SRAM+0x00)
// For Interrupt In
#define	HID_USB_BUF_1			(HID_USB_BUF_0+HID_MAX_PACKET_SIZE_EP0)
// For Control Setup
#define	HID_USB_BUF_SETUP		(HID_USB_BUF_1+HID_MAX_PACKET_SIZE_EP1)
// For Interrupt Out
#define	HID_USB_BUF_2		(HID_USB_BUF_SETUP+HID_MAX_PACKET_SIZE_EP1)


#define g_HID_ar8UsbBufSetup 	((__IO uint8_t *) HID_USB_BUF_SETUP)
#define g_HID_ar8UsbBuf0 		((__IO uint8_t *) HID_USB_BUF_0)
#define g_HID_ar8UsbBuf1 		((__IO uint8_t *) HID_USB_BUF_1)




#define LEN_CONFIG_AND_SUBORDINATE (LEN_CONFIG+LEN_INTERFACE+LEN_HID+LEN_ENDPOINT*2)
#define HID_INT_BUFFER_SIZE	64

typedef BOOL (*PFN_DRVUSB_COMPARE)(uint8_t);
	
	extern const uint8_t g_HID_au8KeyboardReport[];
	extern uint8_t g_HID_au8KeyboardReportDescriptor[];
	extern const uint32_t g_HID_u32KeyboardReportDescriptorSize;
	
	typedef struct
	{
		//bit7 is directory bit, 1: input; 0: output
		uint32_t u32EPAddr;
		uint32_t u32MaxPacketSize; 
		uint8_t *u8SramBuffer;
	}S_HID_EP_CTRL;
	
	typedef struct
	{
		uint8_t au8IntInBuffer[HID_INT_BUFFER_SIZE];
		uint8_t au8IntOutBuffer[HID_INT_BUFFER_SIZE];
		uint8_t au8Setup[8];
		
		/*E_USBINFRA_USB_STATE*/uint8_t eUsbState;
		BOOL abData0[MAX_EP_NUM];//index is EP address

		//S_USBINFRA_CTRL_CALLBACK_ENTRY 	*asCtrlCallbackEntry;//[64];
		uint8_t 	u8CtrlCallbackEntryCnt;//[64];
		

		uint8_t u8UsbAddress;
		uint8_t u8UsbConfiguration;
		
		// End user Vender setting data
		uint8_t *buffer;			//buffer to send or receive
		uint32_t u32Size;			//size to send or total received actually
		//S_HID_EP_CTRL *sEpCrl;
		uint32_t u32Size_want;			//size that want to receive
		uint32_t u32TriggerSize;			//Trigger size that want to receive, if actually received less than u32TriggerSize, it means OUT finished
		PFN_DRVUSB_COMPARE pfnOutFinish;
		
	} S_HID_DEVICE;

extern S_HID_DEVICE g_HID_sDevice;


//static inline
__inline void _HID_CLR_CTRL_READY_AND_TRIG_STALL()
{
	_DRVUSB_CLEAR_EP_READY_AND_TRIG_STALL(0);
	_DRVUSB_CLEAR_EP_READY_AND_TRIG_STALL(1);
}

//static inline
__inline void _HID_CLR_CTRL_READY()
{
	_DRVUSB_CLEAR_EP_READY(0);
	_DRVUSB_CLEAR_EP_READY(1);
}

void HID_CtrlSetupAck(void);
void HID_CtrlDataInAck(void);
void HID_CtrlDataOutAck(void);
void HID_CtrlSetupSetAddress(void* pVoid);
void HID_CtrlSetupClearSetFeature(void* pVoid);
void HID_CtrlSetupGetConfiguration(void* pVoid);
void HID_CtrlSetupGetStatus(void* pVoid);
void HID_CtrlSetupGetInterface(void* pVoid);
void HID_CtrlSetupSetInterface(void* pVoid);
void HID_CtrlSetupGetDescriptor(void* pVoid);
void HID_CtrlSetupSetConfiguration(void* pVoid);
void HID_CtrlDataInSetAddress(void* pVoid);
void HID_CtrlDataInSetInterface(void* pVoid);
void HID_CtrlDataInDefault(void* pVoid);
void HID_CtrlDataOutDefault(void* pVoid);


int32_t HID_Open(void);
void HID_Close(void);

void HID_CtrlDataInDefault(void* pVoid);
void HID_CtrlDataOutDefault(void* pVoid);
void HID_Reset(S_HID_DEVICE *psDevice);
void HID_Start(S_HID_DEVICE *psDevice);
void HID_UsbBusResetCallback(void* pVoid);
void HID_IntOutCallback(void);
void HID_IntInCallback(void);

#ifdef  __cplusplus
}
#endif

#endif //__HIDDEVICE_H__








