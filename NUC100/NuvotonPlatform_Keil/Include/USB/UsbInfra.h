/****************************************************************
 *                                                              *
 * Copyright (c) Nuvoton Technology Corp. All rights reserved.  *
 *                                                              *
 ****************************************************************/
 
#ifndef __USBINFRA_H__
#define __USBINFRA_H__

#include "NUC1xx.h"
#include "System/SysInfra.h"
#include "Driver/DrvUSB.h"


#ifdef __cplusplus
extern "C"
{
#endif

#define EP_INPUT	0x80
#define EP_OUTPUT	0x00
#define MAX_EP_NUM		6

//***************************************************
// 		USB REQUEST
//***************************************************
// Request Type
#define	REQ_STANDARD		0x00
#define	REQ_CLASS			0x20
#define	REQ_VENDOR			0x40

// Request
#define	GET_STATUS			0x00
#define	CLEAR_FEATURE		0x01
//#define					0x02
#define	SET_FEATURE			0x03
//#define					0x04
#define	SET_ADDRESS			0x05
#define	GET_DESCRIPTOR		0x06
#define	SET_DESCRIPTOR		0x07
#define	GET_CONFIGURATION	0x08
#define	SET_CONFIGURATION	0x09
#define	GET_INTERFACE		0x0A
#define	SET_INTERFACE		0x0B
#define	SYNC_FRAME			0x0C

//***************************************************
// USB Descriptor Type
//***************************************************
#define	DESC_DEVICE			0x01
#define	DESC_CONFIG			0x02
#define	DESC_STRING			0x03
#define	DESC_INTERFACE		0x04
#define	DESC_ENDPOINT		0x05
#define	DESC_QUALIFIER		0x06
#define	DESC_OTHERSPEED		0x07
// HID
#define DESC_HID 			0x21
#define DESC_HID_RPT 		0x22

//***************************************************
// USB Descriptor Length
//***************************************************
#define	LEN_DEVICE			18
#define	LEN_CONFIG			9
#define	LEN_INTERFACE		9
#define	LEN_ENDPOINT		7
// HID
#define LEN_HID				0x09

// USB Endpoint Type
#define	EP_ISO				0x01
#define	EP_BULK				0x02
#define	EP_INT				0x03

//***************************************************
// USB Feature Selector
//***************************************************
#define	FEATURE_DEVICE_REMOTE_WAKEUP	0x01
#define	FEATURE_ENDPOINT_HALT			0x00

//***************************************************
// USB Device state
//***************************************************

	typedef enum
	{
		eUSBINFRA_USB_STATE_DETACHED 	= 0,
		eUSBINFRA_USB_STATE_ATTACHED 	= BIT0,
		eUSBINFRA_USB_STATE_POWERED 	= eUSBINFRA_USB_STATE_ATTACHED + BIT1,
		eUSBINFRA_USB_STATE_DEFAULT 	= eUSBINFRA_USB_STATE_POWERED + BIT2,
		eUSBINFRA_USB_STATE_ADDRESS 	= eUSBINFRA_USB_STATE_DEFAULT + BIT3,
		eUSBINFRA_USB_STATE_CONFIGURED 	= eUSBINFRA_USB_STATE_ADDRESS + BIT4,
		eUSBINFRA_USB_STATE_SUSPENDED 	= BIT5,

	} E_USBINFRA_USB_STATE;

//***************************************************
// USB Vender_info descriptor structure
//***************************************************
typedef struct
{
	uint8_t byLength;
	uint8_t byDescType;
	uint16_t 	au16UnicodeString[16];
	
} S_USBINFRA_STRING_DESC;

//***************************************************
// Typedef USB Callback function
//***************************************************
typedef void (*PFN_USBINFRA_ATTACH_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_DETACH_CALLBACK)(void*);

typedef void (*PFN_USBINFRA_BUS_RESET_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_BUS_SUSPEND_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_BUS_RESUME_CALLBACK)(void*);

typedef void (*PFN_USBINFRA_SETUP_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_EP_IN_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_EP_OUT_CALLBACK)(void*);

typedef void (*PFN_USBINFRA_CTRL_SETUP_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_CTRL_DATA_IN_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_CTRL_DATA_OUT_CALLBACK)(void*);
//***************************************************
// Typedef ISR USB Callback function
//***************************************************
typedef void (*PFN_USBINFRA_ISR_ATTACH_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_ISR_DETACH_CALLBACK)(void*);

typedef void (*PFN_USBINFRA_ISR_BUS_RESET_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_ISR_BUS_SUSPEND_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_ISR_BUS_RESUME_CALLBACK)(void*);

typedef void (*PFN_USBINFRA_ISR_SETUP_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_ISR_EP_IN_CALLBACK)(void*);
typedef void (*PFN_USBINFRA_ISR_EP_OUT_CALLBACK)(void*);
//***************************************************
// USB Control callback function structure
//***************************************************
typedef struct
{
	//UINT8 u8RequestType;
	uint8_t u8Request;
	PFN_USBINFRA_CTRL_SETUP_CALLBACK 	pfnCtrlSetupCallback;
	//PFN_USBINFRA_CTRL_DATA_IN_CALLBACK	pfnCtrlDataInCallback;
	//PFN_USBINFRA_CTRL_DATA_OUT_CALLBACK	pfnCtrlDataOutCallback;
	//void*								pVoid;
} S_USBINFRA_CTRL_CALLBACK_ENTRY;


#define MAX_EP_NUM		6

#define	HID_ATTACH			0
#define	HID_DETACH			1
#define	HID_BUS_RESET		2
#define	HID_BUS_SUSPEND		3
#define	HID_BUS_RESUME		4
#define	HID_SETUP			5

#define	USBINFRA_EVENT_FLAG_ATTACH			BIT0
#define	USBINFRA_EVENT_FLAG_DETACH			BIT1
#define	USBINFRA_EVENT_FLAG_BUS_RESET		BIT2
#define	USBINFRA_EVENT_FLAG_BUS_SUSPEND		BIT3
#define	USBINFRA_EVENT_FLAG_BUS_RESUME		BIT4
#define	USBINFRA_EVENT_FLAG_SETUP			BIT5

//***************************************************
// Define USBINFRA READ/WRITE USB BUFFER
//***************************************************

	#define _USBINFRA_READ_USB_BUF(UsbBuf, Mem, Len) \
		memcpy ((void *) (Mem), (void *) (UsbBuf), (Len))
	
	#define _USBINFRA_WRITE_USB_BUF(UsbBuf, Mem, Len) \
		memcpy ((void *) (UsbBuf), (void *) (Mem), (Len))


	void USB_ParseEvent(void);
	
	void USB_Close(void);

#ifdef __cplusplus
}
#endif

#endif // __USBINFRA_H__

















