

/****************************************************************
 *                                                              *
 * Copyright (c) Nuvoton Technology Corp. All rights reserved.  *
 *                                                              *
 ****************************************************************/
#include <string.h>
#include "NUC1xx.h"
#include "System/SysInfra.h"
#include "Driver/DrvUSB.h"
#include "HIDDevice.h"

#define DBG_PRINTF(...)     //printf(__VA_ARGS__)

extern BOOL bUsbDataReady, bUartDataReady;
extern void my_memcpy(void *dest, void *src, int32_t size);
extern void Delay(uint32_t delayCnt);
extern void SysTimerDelay(uint32_t us);

static S_USBINFRA_CTRL_CALLBACK_ENTRY asCtrlCallbackEntry[];

//size 27
__align(4) uint8_t g_HID_au8ReportDescriptor[] = {


   //每行开始的第一字节为该条目的前缀，前缀的格式为：
 //D7~D4：bTag。D3~D2：bType；D1~D0：bSize。以下分别对每个条目注释。
 
 //这是一个全局（bType为1）条目，将用途页选择为普通桌面Generic Desktop Page。
 //后面跟1字节数据（bSize为1），后面的字节数就不注释了，自己根据bSize来判断。
 0x05, 0x01, // USAGE_PAGE (Generic Desktop)
 
 //这是一个局部（bType为2）条目，用途选择为0x00。在普通桌面页中，
 //该用途是未定义的，如果使用该用途来开集合，那么系统将不会把它
 //当作标准系统设备，从而就成了一个用户自定义的HID设备。
 0x09, 0x00, // USAGE (0)
 
 //这是一个主条目（bType为0）条目，开集合，后面跟的数据0x01表示
 //该集合是一个应用集合。它的性质在前面由用途页和用途定义为
 //用户自定义。
 0xa1, 0x01, // COLLECTION (Application)

 //这是一个全局条目，说明逻辑值最小值为0。
 0x15, 0x00, //     LOGICAL_MINIMUM (0)
 
 //这是一个全局条目，说明逻辑值最大为255。
 0x25, 0xff, //     LOGICAL_MAXIMUM (255)
 
 //这是一个局部条目，说明用途的最小值为0。
 0x19, 0x00, //     USAGE_MINIMUM (1)
 
 //这是一个局部条目，说明用途的最大值255。
 0x29, 0xff, //     USAGE_MAXIMUM (255) 
 
 //这是一个全局条目，说明数据域的数量为64个。
 0x95, 0x40, //     REPORT_COUNT (64)
 
 //这是一个全局条目，说明每个数据域的长度为8bit，即1字节。
 0x75, 0x08, //     REPORT_SIZE (8)
 
 //这是一个主条目，说明有8个长度为8bit的数据域做为输入。
 0x81, 0x02, //     INPUT (Data,Var,Abs)
 
 //这是一个局部条目，说明用途的最小值为0。
 0x19, 0x00, //     USAGE_MINIMUM (0)
 
 //这是一个局部条目，说明用途的最大值255。
 0x29, 0xff, //     USAGE_MAXIMUM (255) 
 
 //这是一个主条目。定义输出数据（8字节，注意前面的全局条目）。
 0x91, 0x02, //   OUTPUT (Data,Var,Abs)
 
 //下面这个主条目用来关闭前面的集合。bSize为0，所以后面没数据。
 0xc0        // END_COLLECTION

};

#define HID_REPORT_DESCRIPTOR_SIZE \
	sizeof (g_HID_au8ReportDescriptor) / sizeof (g_HID_au8ReportDescriptor)[0]

__align(4) uint8_t g_HID_au8DeviceDescriptor[] =
{
	LEN_DEVICE,							// bLength
	DESC_DEVICE,						// bDescriptorType
	0x10, 0x01,							// bcdUSB
	0x00,								// bDeviceClass
	0x00,								// bDeviceSubClass
	0x00,								// bDeviceProtocol
	HID_MAX_PACKET_SIZE_EP0,			// bMaxPacketSize0
	
	// idVendor
	0x0416 & 0x00FF,
	(0x0416 & 0xFF00) >> 8,
	
	// idProduct
	0xA316 & 0x00FF,
	(0xA316 & 0xFF00) >> 8,
	0x00, 0x00,							// bcdDevice
	0x01,								// iManufacture
	0x02,								// iProduct
	0x00,								// iSerialNumber
	0x01								// bNumConfigurations
};

__align(4) uint8_t g_HID_au8ConfigDescriptor[] =
{
	LEN_CONFIG,							// bLength
	DESC_CONFIG,						// bDescriptorType
	LEN_CONFIG_AND_SUBORDINATE & 0x00FF,// wTotalLength
	(LEN_CONFIG_AND_SUBORDINATE & 0xFF00) >> 8, // 0x3B & 0x00FF,
	0x01,								// bNumInterfaces
	0x01,								// bConfigurationValue
	0x00,								// iConfiguration
	0xC0,								// bmAttributes
	0x32,								// MaxPower

	// I/F descr: HID
	LEN_INTERFACE,						// bLength
	DESC_INTERFACE,						// bDescriptorType
	0x01,								// bInterfaceNumber
	0x00,								// bAlternateSetting
	0x02,								// bNumEndpoints
	0x03,								// bInterfaceClass
	0x00,//0x01							// bInterfaceSubClass	01:Boot Interface subclass
	0x00,//0x01							// bInterfaceProtocol	01:Keyboard
	0x00,								// iInterface

	// HID Descriptor
	LEN_HID,							// Size of this descriptor in UINT8s.
	DESC_HID,							// HID descriptor type.
	0x10, 0x01, 						// HID Class Spec. release number.
	0x00,								// H/W target country.
	0x01,								// Number of HID class descriptors to follow.
	DESC_HID_RPT,						// Dscriptor type.
	
	// Total length of report descriptor.
	HID_REPORT_DESCRIPTOR_SIZE & 0x00FF,		//keyboard report descriptor size bigger than mouse.
	(HID_REPORT_DESCRIPTOR_SIZE & 0xFF00) >> 8,
	
	// EP Descriptor: interrupt in.
	LEN_ENDPOINT,						// bLength
	DESC_ENDPOINT,						// bDescriptorType
	0x81,								// bEndpointAddress
	0x03,								// bmAttributes->bulk
	
	// wMaxPacketSize
	HID_MAX_PACKET_SIZE_EP1 & 0x00FF,
	(HID_MAX_PACKET_SIZE_EP1 & 0xFF00) >> 8,
	0x02,//0x0A,		// bInterval
	
	// EP Descriptor: interrupt out.
 	0x07,// bLength
 	0x05,// bDescriptorType
 
 	//bEndpointAddress字段。端点的地址。我们使用D12的输出端点1。
 	//D7位表示数据方向，输出端点D7为0。所以输出端点2的地址为0x02。
 	0x02,
 	//bmAttributes字段。D1~D0为端点传输类型选择。
 	//该端点为中断端点。中断端点的编号为3。其它位保留为0。
 	0x03,//0x03,
 
 	//wMaxPacketSize字段。该端点的最大包长。端点1的最大包长为16字节。
 	//注意低字节在先。
 	HID_MAX_PACKET_SIZE_EP1 & 0x00FF,
	(HID_MAX_PACKET_SIZE_EP1 & 0xFF00) >> 8,
 
 	//bInterval字段。端点查询的时间，我们设置为1个帧时间，即1ms。
 	0x01//0x0A
	
	
};

__align(4) const uint8_t g_HID_au8StringLang[4] =
{
	4,				// bLength
	DESC_STRING,	// bDescriptorType
	0x09, 0x04
};

__align(4) const S_USBINFRA_STRING_DESC g_HID_sVendorStringDesc =
{
	16,
	DESC_STRING,
	{'N', 'U', 'V', 'O', 'T', 'O', 'N'}
};

__align(4) const S_USBINFRA_STRING_DESC g_HID_sProductStringDesc =
{
	16,
	DESC_STRING,
	{'W', 'P', 'M', ' ', 'U', 'S', 'B'}
};

__align(4) const uint8_t g_HID_au8StringSerial[26] =
{
	26,				// bLength
	DESC_STRING,	// bDescriptorType
	//'B', 0, '0', 0, '2', 0, '0', 0, '0', 0, '6', 0, '0', 0, '9', 0, '2', 0, '1', 0, '1', 0, '4', 0

	'B', 0, '0', 0, '2', 0, '0', 0, '0', 0, '8', 0, '0', 0, '3', 0, '2', 0, '1', 0, '1', 0, '5', 0
};


S_HID_DEVICE g_HID_sDevice;

/*
	if ATTR[HSIZE_8] = 1: byte access, please using WordsCpy(dest, src, size) for data copy
*/
void WordsCpy(void *dest, void *src, int32_t size)
{
    uint8_t *pu8Src, *pu8Dest;
    int32_t i;
    
    pu8Dest = (uint8_t *)dest;
    pu8Src  = (uint8_t *)src;
    
    for(i=0;i<size;i++)
        pu8Dest[i] = pu8Src[i]; 
}

#if 0
/*************************************************************************/
/*                                                                       */
/* DESCRIPTION                                                           */
/*      get data from u32EPAddr's out USB SRAM buffer                    */
/*                                                                       */
/* INPUTS                                                                */
/*      None                                                             */
/*                                                                       */
/* OUTPUTS                                                               */
/*      None                                                             */
/*                                                                       */
/* RETURN                                                                */
/*      received data's size                                             */
/*                                                                       */
/*************************************************************************/
static UINT32 HID_CtrlGetOutData()
{
	UINT32 u32Size = 0, trigCnt;
	S_HID_DEVICE *pHidDev = &g_HID_sDevice;
	UINT8* buffer = pHidDev->buffer + pHidDev->u32Size;
	
	
	if(pHidDev->u32Size_want > 0)
	{
		u32Size = _DRVUSB_GET_EP_DATA_SIZE(1);
		my_memcpy(buffer, (UINT8*)HID_USB_BUF_0, u32Size);
		pHidDev->u32Size += u32Size;
		pHidDev->u32Size_want -= u32Size;
		
		if(pHidDev->u32Size_want > HID_MAX_PACKET_SIZE_EP0)
			trigCnt = HID_MAX_PACKET_SIZE_EP0;
		else
			trigCnt = pHidDev->u32Size_want;
		
		pHidDev->u32TriggerSize = trigCnt;

		//_DRVUSB_SET_EP_BUF(1,(UINT32)HID_USB_BUF_0);
		_DRVUSB_TRIG_EP(1,trigCnt);
	}
	else if(pHidDev->pfnOutFinish)
		pHidDev->pfnOutFinish(0);
	
	

	return u32Size;
}
#endif


/*************************************************************************/
/*                                                                       */
/* DESCRIPTION                                                           */
/*     trigger ready flag for sending data, only using for CTRL pipe     */
/*     after receive IN token from host, USB will send the data          */
/*     if u8Buffer == NULL && u32Size == 0 then send DATA1 always        */
/*     else DATA0 and DATA1 by turns                                     */
/*     Note : except ctrl pipe, I don't think other pipe can use         */
/*     HID_DataIn(NULL, 0)                                    */
/*                                                                       */
/* INPUTS                                                                */
/*      u32EPAddr     EP address, send data from it                      */
/*      u8Buffer     data buffer                                         */
/*      u32Size     data size                                            */
/*                                                                       */
/* OUTPUTS                                                               */
/*      None                                                             */
/*                                                                       */
/* RETURN                                                                */
/*      0           Success                                              */
/*		Otherwise	error												 */
/*                                                                       */
/*************************************************************************/
int32_t HID_DataIn(const uint8_t * u8Buffer, uint32_t u32Size)
{
	S_HID_DEVICE *psDevice =&g_HID_sDevice;
	uint32_t cpyCnt = u32Size;
	
	//EPNumber = HID_GetEPNumber(u32EPAddr, EP_INPUT);
	
	if(u8Buffer && u32Size)
	{
		if(u32Size > HID_MAX_PACKET_SIZE_EP0)
			cpyCnt = HID_MAX_PACKET_SIZE_EP0;

		my_memcpy((uint8_t*)HID_USB_BUF_0, (void*)u8Buffer, cpyCnt);
		//psDevice->u32Size = u32Size - cpyCnt;
		psDevice->buffer = (uint8_t *)(u8Buffer + cpyCnt);
	}
	//else
	//	psDevice->u32Size = 0;
	
	//_DRVUSB_SET_EP_BUF(EPNumber,(UINT32)psDevice->sEpCrl[EPNumber].u8SramBuffer);
	
	if(u8Buffer == NULL && u32Size == 0)
		psDevice->abData0[0] = FALSE;
	else
		psDevice->abData0[0] = !psDevice->abData0[0];
//printf("data0 %d %d, %d, %d\n", u32EPAddr, g_UsbInfra_sDevice.abData0[u32EPAddr], cpyCnt, EPNumber);
	
	_DRVUSB_SET_EP_TOG_BIT(0, psDevice->abData0[0]);
	_DRVUSB_TRIG_EP(0,cpyCnt);
	
	return E_SUCCESS;
}


/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_UsbBusResetCallback()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/       

void HID_UsbBusResetCallback(void* pVoid)
{

	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) pVoid;

    DBG_PRINTF("%s\n",__FUNCTION__);

	_DRVUSB_SET_FADDR(0x00);
	
	// Disable IN NAK interrupt.
	_DRVUSB_SET_STS(0x00);	
	_DRVUSB_SET_SETUP_BUF(HID_USB_BUF_SETUP);

	// Configure EP0 for EP0 CTRL IN.
	_DRVUSB_SET_CFGP(0,0x01);
	_DRVUSB_SET_CFG(0,CFG_CTRL_IN_EP0_SETTING);
	_DRVUSB_SET_EP_BUF(0,HID_USB_BUF_0);
	
	// Configure EP1 for EP0 CTRL OUT.
	_DRVUSB_SET_CFGP(1,0x01);
	_DRVUSB_SET_CFG(1,CFG_CTRL_OUT_EP0_SETTING);
	_DRVUSB_SET_EP_BUF(1,HID_USB_BUF_0);

	psDevice->u8UsbAddress = 0;
	psDevice->u8UsbConfiguration = 0;
}

__inline void HID_CtrlSetupAck()
{
	uint32_t i;
	//int eUsbState;
	S_USBINFRA_CTRL_CALLBACK_ENTRY *psEntry = 0;
	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) &g_HID_sDevice;

    DBG_PRINTF("%s\n",__FUNCTION__);

#if 0//clyu
	_HID_CLR_CTRL_READY();

	// check if after DEFAULT state (RESET)
	eUsbState = g_HID_sDevice.eUsbState;
	if (eUsbState < eUSBINFRA_USB_STATE_DEFAULT)
	{
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();
		return;
	}
#endif
	//g_HID_sDevice.au8Setup = g_HID_ar8UsbBufSetup;
	my_memcpy(g_HID_sDevice.au8Setup, (void*)g_HID_ar8UsbBufSetup, 8);

	for (i = 0; i < psDevice->u8CtrlCallbackEntryCnt; i++)
	{
		psEntry = asCtrlCallbackEntry + i;
		if (/*psEntry->u8RequestType == (psDevice->au8Setup[0] & 0x60) &&*/
		        psEntry->u8Request == psDevice->au8Setup[1])
		{
			//psEntryRet->sCtrlCallbackEntry.pfnCtrlSetupCallback(psEntryRet->sCtrlCallbackEntry.pVoid);
			psEntry->pfnCtrlSetupCallback(psDevice);
			return;
		}
	}

	_HID_CLR_CTRL_READY_AND_TRIG_STALL();
	return;
}

//======================================================
__inline void HID_CtrlDataInAck()
{
	uint32_t i;
	S_USBINFRA_CTRL_CALLBACK_ENTRY *psEntry = 0;
	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) &g_HID_sDevice;

	for (i = 0; i < psDevice->u8CtrlCallbackEntryCnt; i++)
	{
		psEntry = asCtrlCallbackEntry + i;
		if (/*psEntry->u8RequestType == (psDevice->au8Setup[0] & 0x60) &&*/
		        psEntry->u8Request == psDevice->au8Setup[1])
		{
			//psEntry->pfnCtrlDataInCallback(psDevice);
			if(psEntry->u8Request == SET_ADDRESS)
				HID_CtrlDataInSetAddress(psDevice);
			else
				HID_CtrlDataInDefault(psDevice);
			return;
		}
	}
	return;
}

//======================================================
__inline void HID_CtrlDataOutAck(
)
{
	uint32_t i;
	S_USBINFRA_CTRL_CALLBACK_ENTRY *psEntry = 0;
	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) &g_HID_sDevice;

	for (i = 0; i < psDevice->u8CtrlCallbackEntryCnt; i++)
	{
		psEntry = asCtrlCallbackEntry + i;
		if (/*psEntry->u8RequestType == (psDevice->au8Setup[0] & 0x60) &&*/
		        psEntry->u8Request == psDevice->au8Setup[1])
		{
			//psEntry->pfnCtrlDataOutCallback(psDevice);
			HID_CtrlDataOutDefault(psDevice);
			return;
		}
	}
	return;
}



/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupSetAddress()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void HID_CtrlSetupSetAddress(
    void* pVoid
)
{
	int eUsbState;

	eUsbState = g_HID_sDevice.eUsbState;
	if (eUsbState == eUSBINFRA_USB_STATE_DEFAULT)
	{
		g_HID_sDevice.u8UsbAddress = g_HID_sDevice.au8Setup[2];
		_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
		_DRVUSB_TRIG_EP(0,0x00);

		g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_ADDRESS;
	}
	else
	{
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();
	}
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupClearSetFeature()            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlSetupClearSetFeature(
    void* pVOid
)
{
	// Device
	if ((g_HID_sDevice.au8Setup[0] == 0x00) && (g_HID_sDevice.au8Setup[2] == FEATURE_DEVICE_REMOTE_WAKEUP));
	
	// Interface
	else if (g_HID_sDevice.au8Setup[0] == 0x01);
	
	// Endpoint
	else if ((g_HID_sDevice.au8Setup[0] == 0x02) && (g_HID_sDevice.au8Setup[2] == FEATURE_ENDPOINT_HALT))
	{
		if ((g_HID_sDevice.au8Setup[4] &0xF) == INT_IN_EP_NUM)
		{
			// Endpoint 1
			_DRVUSB_CLEAR_EP_DSQ(2);
			if (g_HID_sDevice.au8Setup[1] == CLEAR_FEATURE)
			{
				_DRVUSB_CLEAR_EP_STALL(2);
			}
			else
			{
				_DRVUSB_TRIG_EP_STALL(2);
			}
		}
		else
		{
			_HID_CLR_CTRL_READY_AND_TRIG_STALL();
		}
	}
	else
	{
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();
	}
	_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
	_DRVUSB_TRIG_EP(0,0x00);
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupGetConfiguration()	           	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlSetupGetConfiguration(
    void* pVoid
)
{
	g_HID_ar8UsbBuf0[0] = g_HID_sDevice.u8UsbConfiguration;
	_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
	_DRVUSB_TRIG_EP(0,0x01);
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupGetStatus()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlSetupGetStatus(
    void* pVoid
)
{
	uint8_t au8Buf[4];

	if (g_HID_sDevice.au8Setup[0] == 0x80)
	{
		// Device
		au8Buf[0] = 0x01;	// Self-Powered
	}
	else if (g_HID_sDevice.au8Setup[0] == 0x81)
	{
		// Interface
		au8Buf[0] = 0x00;
	}
	else if (g_HID_sDevice.au8Setup[0] == 0x82)
	{
		// Endpoint
		if ((g_HID_sDevice.au8Setup[4] & 0xF) == 0x01)
		{
			// Interrupt-In Endpoint
			au8Buf[0] = (_DRVUSB_GET_CFGP(2) & CFGPx_STALL) ? 1 : 0;
		}
		else
		{
			_HID_CLR_CTRL_READY_AND_TRIG_STALL();
			return;
		}
	}
	else
	{
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();
		return;
	}
	au8Buf[1] = 0x00;
	g_HID_ar8UsbBuf0[0] = au8Buf[0];
	g_HID_ar8UsbBuf0[1] = au8Buf[1];

	_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
	_DRVUSB_TRIG_EP(0,0x02);
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupGetInterface()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlSetupGetInterface(
    void* pVoid
)
{
	g_HID_ar8UsbBuf0[0] = 0x00;

	_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
	_DRVUSB_TRIG_EP(0,0x01);
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupSetInterface()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlSetupSetInterface(
    void* pVoid
)
{
	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) pVoid;

	HID_Reset(psDevice);
	HID_Start(psDevice);

	_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
	_DRVUSB_TRIG_EP(0,0x00);
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_CtrlSetupGetDescriptor()	           	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     	Invocated when Get_Descriptor SETUP ACK happens.	                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void HID_CtrlSetupGetDescriptor(
    void* pVoid
)
{
	uint16_t u16Len;
	
	//u16Len = g_HID_sDevice.au8Setup[7];
	//u16Len <<= 8;
	//u16Len += g_HID_sDevice.au8Setup[6];
	u16Len = inps(g_HID_sDevice.au8Setup + 6);
//printf("Get descriptor %d\n", g_HID_sDevice.au8Setup[3]);
	switch (g_HID_sDevice.au8Setup[3])
	{
	//***********************/
	// Get Device Descriptor
	//***********************/
	case DESC_DEVICE:
	{
		//_SYSINFRA_SET16_LE(g_HID_au8DeviceDescriptor + 8, g_HID_sDevice.sVendorInfo.u16VendorId);
		//_SYSINFRA_SET16_LE(g_HID_au8DeviceDescriptor + 10, g_HID_sDevice.sVendorInfo.u16ProductId);

		if(u16Len > LEN_DEVICE)
		    u16Len = LEN_DEVICE;

		HID_DataIn(g_HID_au8DeviceDescriptor, u16Len);

		break;
	}
	//***********************/
	// Get Configuration Descriptor
	//***********************/
	case DESC_CONFIG:	
	{
		if(u16Len > g_HID_au8ConfigDescriptor[2])
		    u16Len = g_HID_au8ConfigDescriptor[2];

		HID_DataIn(g_HID_au8ConfigDescriptor, u16Len);

		break;
	}		
	//***********************/
	// Get HID Descriptor
	//***********************/
	case DESC_HID:
	{
		if(u16Len > LEN_HID)
		    u16Len = LEN_HID;
		HID_DataIn(g_HID_au8ConfigDescriptor +
	    					LEN_CONFIG + LEN_INTERFACE, u16Len);
		break;
	}
	//***********************/
	// Get Report Descriptor
	//***********************/
	case DESC_HID_RPT:	
	{
		if(u16Len > HID_REPORT_DESCRIPTOR_SIZE)
		    u16Len = HID_REPORT_DESCRIPTOR_SIZE;

		HID_DataIn(g_HID_au8ReportDescriptor, u16Len);

		break;
	}	
	//***********************/
	// Get String Descriptor
	//***********************/
	case DESC_STRING:
	{
		// Get Language
		if (g_HID_sDevice.au8Setup[2] == 0)
		{
			if(u16Len > 4)
			    u16Len = 4;
			HID_DataIn(g_HID_au8StringLang, u16Len);

		}
		else
		{
			// Get String Descriptor
			switch (g_HID_sDevice.au8Setup[2])
			{
			case 1:
			{
				if(u16Len > g_HID_sVendorStringDesc.byLength)
				    u16Len = g_HID_sVendorStringDesc.byLength;

				HID_DataIn((const uint8_t *)&g_HID_sVendorStringDesc, u16Len);
				break;
			}
			case 2:
			{
				if(u16Len > g_HID_sProductStringDesc.byLength)
				    u16Len = g_HID_sProductStringDesc.byLength;

				HID_DataIn((const uint8_t *)&g_HID_sProductStringDesc, u16Len);

				break;
			}
			case 3:
			{
				if(u16Len > g_HID_au8StringSerial[0])
				    u16Len = g_HID_au8StringSerial[0];

				HID_DataIn(g_HID_au8StringSerial, u16Len);

				break;
			}

			default:
				// Not support. Reply STALL.
				_HID_CLR_CTRL_READY_AND_TRIG_STALL();
			}
		}
		break;
	}
	default:
		// Not support. Reply STALL.
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();

	}   
}
/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_UsbBusResetCallback()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     	Invocated when Set_Configuration SETUP ACK happens.                                                */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void HID_CtrlSetupSetConfiguration(
    void* pVoid
)
{
	S_HID_DEVICE *psDevice = (S_HID_DEVICE *) pVoid;

	if (g_HID_sDevice.au8Setup[2] == 0)
	{
		// USB address state.
		g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_ADDRESS;
		g_HID_sDevice.u8UsbConfiguration = g_HID_sDevice.au8Setup[2];
		// Trigger next Control In DATA1 Transaction.
		_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
		_DRVUSB_TRIG_EP(0,0x00);
	}
	else if (g_HID_sDevice.au8Setup[2] == g_HID_au8ConfigDescriptor[5])
	{
		// USB configured state.
		//USB_SetUsbState(eUSBINFRA_USB_STATE_CONFIGURED);
		g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_CONFIGURED;
		
		HID_Reset(psDevice);
		HID_Start(psDevice);

		g_HID_sDevice.u8UsbConfiguration = g_HID_sDevice.au8Setup[2];
		// Trigger next Control In DATA1 Transaction.
		_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
		_DRVUSB_TRIG_EP(0,0x00);
	}
	else
	{
		// Not support. Reply STALL.
		_HID_CLR_CTRL_READY_AND_TRIG_STALL();
	}
}

/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_UsbBusResetCallback()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlDataInSetAddress(
    void* pVoid
)
{
	if (g_HID_sDevice.u8UsbAddress == 0x00)
	{
		//USB_SetUsbState(eUSBINFRA_USB_STATE_DEFAULT);
		g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_DEFAULT;
	}
	else
	{
		//USB_SetUsbState(eUSBINFRA_USB_STATE_ADDRESS);
		g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_ADDRESS;
		_DRVUSB_SET_FADDR(g_HID_sDevice.u8UsbAddress);
	}
}
/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_UsbBusResetCallback()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlDataInDefault(
    void* pVoid
)
{

    DBG_PRINTF("%s\n", __FUNCTION__);
	
	if (g_HID_sDevice.au8Setup[0] & 0x80)
	{
	#if 0	
		if(g_HID_sDevice.u32Size == 0)
        {
            HID_DataIn(0, 0);
        }
        else
			HID_DataIn(g_HID_sDevice.buffer, g_HID_sDevice.u32Size);
	#endif
		_DRVUSB_TRIG_EP(1,0x00);//prepare to receive OUT token

	}
//	else
//		g_HID_sDevice.u32Size_want = 0;
	
	
}
/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_UsbBusResetCallback()	            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  
void HID_CtrlDataOutDefault(
    void* pVoid
)
{
	//UINT32 u32Size;
    DBG_PRINTF("%s\n", __FUNCTION__);

	if ((g_HID_sDevice.au8Setup[0] & 0x80) == 0)
	{
		_DRVUSB_SET_EP_TOG_BIT(0,FALSE);
		_DRVUSB_TRIG_EP(0,0x00);

	#if 0//no ctrl data out for ISP
		u32Size = HID_CtrlGetOutData();
	
		if((g_HID_sDevice.u32Size_want==0) || 
			(g_HID_sDevice.u32TriggerSize > u32Size))
			HID_DataIn(NULL, 0);
	#endif
	}
	else
	{
		//g_HID_sDevice.u32Size = 0;
	}
	
	
}

static S_USBINFRA_CTRL_CALLBACK_ENTRY asCtrlCallbackEntry[] =
{
	{/*REQ_STANDARD,*/ SET_ADDRESS, HID_CtrlSetupSetAddress/*, HID_CtrlDataInSetAddress, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ CLEAR_FEATURE, HID_CtrlSetupClearSetFeature/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ SET_FEATURE, HID_CtrlSetupClearSetFeature/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ GET_CONFIGURATION, HID_CtrlSetupGetConfiguration/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ GET_STATUS, HID_CtrlSetupGetStatus/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ GET_INTERFACE, HID_CtrlSetupGetInterface/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ SET_INTERFACE, HID_CtrlSetupSetInterface/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ GET_DESCRIPTOR, HID_CtrlSetupGetDescriptor/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/},
	{/*REQ_STANDARD,*/ SET_CONFIGURATION, HID_CtrlSetupSetConfiguration/*, HID_CtrlDataInDefault, HID_CtrlDataOutDefault, &g_HID_sDevice*/}
};


/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_Open()	                             	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

int32_t
HID_Open(
)
{
	//USB init
	NVIC_DisableIRQ(USBD_IRQn);
	/* Enable USB device clock */
    SYSCLK->APBCLK.USBD_EN = 1;

     /* Reset USB device */
	//outp32(IPRST, UDC_RST);				// Reset
    SYS->IPRSTC2.USBD_RST = 1;
	SysTimerDelay(100);//3us
    SYS->IPRSTC2.USBD_RST = 0;
	//outp32(IPRST, inp32(IPRST) & ~UDC_RST);


     /* Select USB divider source */
    SYSCLK->CLKDIV.USB_N = 0;

    _DRVUSB_ENABLE_USB();
    //outp32(ATTR, 0x10);
	//outp32(USBD->DRVSE0, 1);	// SE0 off
	USBD->DRVSE0.DRVSE0 = 1;
    SysTimerDelay(150000);//Delay(0x100);
	USBD->DRVSE0.DRVSE0 = 0;
	//outp32(USBD->DRVSE0, 0);	// SE0 off
	
	//HID_Open();
	g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_DETACHED;
	
	//g_HID_sDevice.sEpCrl = sEpDescription;
	// Register Control callback functions.
	
	//g_HID_sDevice.asCtrlCallbackEntry = asCtrlCallbackEntry;
	g_HID_sDevice.u8CtrlCallbackEntryCnt = sizeof(asCtrlCallbackEntry) / sizeof(asCtrlCallbackEntry[0]);


	return 0;
}




/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_Reset()	                            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void HID_Reset(
    S_HID_DEVICE *psDevice
)
{
	_DRVUSB_SET_CFG(2,CFG_INT_IN_EP1_SETTING);
	_DRVUSB_SET_CFGP(2,CFGPx_CFGP);
	
	_DRVUSB_SET_CFG(3, CFGx_EPT_OUT|0x02);
	_DRVUSB_SET_CFGP(3, CFGPx_CFGP);

}


/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_Start()	                            	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void
HID_Start(
    S_HID_DEVICE *psDevice
)
{
	//memset(psDevice->au8IntInBuffer, 0, HID_INT_BUFFER_SIZE);
	//my_memcpy(
	//    (VOID*) g_HID_ar8UsbBuf1,
	//    psDevice->au8IntInBuffer,
	//    HID_INT_BUFFER_SIZE
	//);
	
	_DRVUSB_SET_EP_BUF(2,HID_USB_BUF_1);
	_DRVUSB_TRIG_EP(2,HID_MAX_PACKET_SIZE_EP1);
	
	_DRVUSB_SET_EP_BUF(3,HID_USB_BUF_2);
	_DRVUSB_TRIG_EP(3,HID_MAX_PACKET_SIZE_EP1);
}


/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      HID_IntInCallback()	                    	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  

void
HID_IntInCallback(
)
{
	S_HID_DEVICE* psDevice = (S_HID_DEVICE*) &g_HID_sDevice;
	
	//printf("bInDataReady = %d\n", bInDataReady);
	
	my_memcpy(
	    (void*) g_HID_ar8UsbBuf1,
   		psDevice->au8IntInBuffer,
	    HID_INT_BUFFER_SIZE
	);
	//memset(psDevice->au8IntInBuffer, 0, HID_INT_BUFFER_SIZE);
	
	_DRVUSB_TRIG_EP(2,HID_MAX_PACKET_SIZE_EP1);
	
}

void HID_IntOutCallback()
{
	S_HID_DEVICE* psDevice = (S_HID_DEVICE*) &g_HID_sDevice;

	//interrupt is 64 invariable
	my_memcpy(psDevice->au8IntOutBuffer, (uint8_t*)HID_USB_BUF_2, HID_MAX_PACKET_SIZE_EP1);
	
	//ParseCmd(psDevice->au8IntOutBuffer, u32Len);
	
	bUsbDataReady = TRUE;
	
	//_DRVUSB_TRIG_EP(3,HID_MAX_PACKET_SIZE_EP1);
	
}


static uint32_t CountTrailingZero(uint32_t x)
{
    uint32_t i;
   
    if(x)
    {
        i = 0;
        while((x & (1 << i)) == 0)
        {
            i++;
        }
    }
    else
    {
        i = 32;
    }
     
    return i;
    
}



/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* FUNCTION                                                                                                */
/*      UsbInfra_PreDispatchEvent()	              	                                                       */
/*                                                                                                         */
/* DESCRIPTION                                                                                             */
/*     										                                                               */		
/*                                                                                                         */
/* INPUTS                                                                                                  */
/*      None											       		                                       */
/*                                                                                                         */
/* OUTPUTS                                                                                                 */
/*      None                            				                                                   */
/*                                                                                                         */
/* RETURN                                                                                                  */
/*      None				                                                                               */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/  


void USB_ParseEvent(void)
{
	uint32_t u32EPNum;
	uint32_t u32EPSettingNum;
	uint32_t u32PacketType;
	uint32_t u32FLODET = inp32(USBD_FLODETB);//_DRVUSB_CLEAR_WAKEUP_INT();
	uint32_t u32EVF = _DRVUSB_GET_EVF();
	uint32_t u32ATTR = inp32(USBD_ATTR);//_DRVUSB_CLEAR_BUS_INT();
	uint32_t u32STS = _DRVUSB_GET_STS();
	
    //printf("USB event %x, %x, %x, %x\n", u32EVF, u32ATTR, u32FLODET, g_HID_sDevice.eUsbState);
	//if (u32EVF & IEF_WAKEUP)
	//{
		// Clear wakeup event.
		_DRVUSB_SET_EVF(EVF_WAKEUP);
		// Pre-dispatch wakeup event.
		//UsbInfra_PreDispatchWakeupEvent(&g_UsbInfra_sDevice);
	//}
	//else 
	if (u32EVF & IEF_FLD)
	{
		// Clear float-detection event.		
		_DRVUSB_SET_EVF(EVF_FLD);
		// Pre-dispatch float-detection event.
		if (u32FLODET & 1)
		{
			// attached
			if (g_HID_sDevice.eUsbState == eUSBINFRA_USB_STATE_DETACHED)
			{
				g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_ATTACHED;
				_DRVUSB_ENABLE_USB(); // enable USB & PHY
				//UsbInfra_EnableUsb(psDevice);
			}
		}
		else
		{
			// detached
			g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_DETACHED;
			_DRVUSB_DISABLE_USB();// disable USB & PHY
			//UsbInfra_DisableUsb(psDevice);
		}
	}
	else if (u32EVF & IEF_BUS)
	{
		// Clear bus event.		
		_DRVUSB_SET_EVF(EVF_BUS);
		// Pre-dispatch bus event.
		//UsbInfra_PreDispatchBusEvent(&g_UsbInfra_sDevice);

		if (g_HID_sDevice.eUsbState == eUSBINFRA_USB_STATE_DETACHED)
		{
			// Clear all pending events on USB attach/detach to
			// handle the scenario that the time sequence of event happening
			// is different from that of event handling.
        	//printf("BUS event - Detached.\n");
			return;
		}

		if (u32ATTR & ATTR_USBRST)
		{
        	//printf("Bus event: Usb reset\n");
			// reset
			_DRVUSB_ENABLE_USB(); // enable PHY
			g_HID_sDevice.eUsbState = eUSBINFRA_USB_STATE_DEFAULT;
			//psDevice->u16MiscEventFlags |= USBINFRA_EVENT_FLAG_BUS_RESET;
			HID_UsbBusResetCallback(&g_HID_sDevice);
		}
		else if (u32ATTR & ATTR_SUSPEND)
		{
    	   //printf("Bus event: Usb suspend\n");
			// suspend
			_DRVUSB_DISABLE_PHY(); // disable PHY
			//outp32(ATTR, 0x380);
        
        	if (g_HID_sDevice.eUsbState >= eUSBINFRA_USB_STATE_ATTACHED)
			{
				g_HID_sDevice.eUsbState |= eUSBINFRA_USB_STATE_SUSPENDED;
			}
			//psDevice->u16MiscEventFlags |= USBINFRA_EVENT_FLAG_BUS_SUSPEND;

		}
		else if (u32ATTR & ATTR_RESUME)
		{
	       //printf("Bus event: Usb resume\n");
			// resume
			_DRVUSB_ENABLE_USB(); // enable PHY
			if (g_HID_sDevice.eUsbState >= eUSBINFRA_USB_STATE_ATTACHED)
			{
				g_HID_sDevice.eUsbState &= ~eUSBINFRA_USB_STATE_SUSPENDED;
			}
			//psDevice->u16MiscEventFlags |= USBINFRA_EVENT_FLAG_BUS_RESUME;
		}
	}
	else if (u32EVF & IEF_USB)
	{
		// Clear USB events individually instead of in total.
		// Otherwise, incoming USB events may be cleared mistakenly.
		// Pre-dispatch USB event.
		//UsbInfra_PreDispatchEPEvent(&g_UsbInfra_sDevice);
		if (g_HID_sDevice.eUsbState == eUSBINFRA_USB_STATE_DETACHED)
		{
			// Clear all pending events on USB attach/detach to
			// handle the scenario that the time sequence of event happening
			// is different from that of event handling.
		
			return;
		}

		//printf("PreDispatch: USB event %x\n", u32EVF);
		
		// Only care EP events and Setup event
		u32EVF &= (EVF_EPTF0 | EVF_EPTF1 | EVF_EPTF2 | EVF_EPTF3 | EVF_EPTF4 | EVF_EPTF5 | EVF_SETUP);

		if (u32EVF & EVF_SETUP)
		{
			//psDevice->u16MiscEventFlags |= USBINFRA_EVENT_FLAG_SETUP;
			u32EVF &= ~EVF_SETUP;
		
			_DRVUSB_SET_EVF(EVF_SETUP);
			//printf("setup\n");
			g_HID_sDevice.abData0[0] = TRUE;
			HID_CtrlSetupAck();
		}

		while (1)
		{
			u32EPSettingNum = CountTrailingZero(u32EVF);
			if (u32EPSettingNum >= 32)
			{
				break;
			}
			// Clear this EP event as pre-handled.
			u32EVF &= ~(1 << u32EPSettingNum);
		
			_DRVUSB_SET_EVF(1 << u32EPSettingNum);

			u32EPSettingNum -= 16; // 0 ~ 5
			u32EPNum = (_DRVUSB_GET_CFG(u32EPSettingNum) & CFGx_EPT);		
		
			u32PacketType = (  (u32STS >> (4 + u32EPSettingNum * 3))  & STS_STS);
			
			//printf("u32PacketType=%x\n", u32PacketType);
		
			if ((u32PacketType == STS_IN_ACK) || (u32PacketType == STS_IN_NAK))
			{
				// Set this EP event as non-handled.
				if(u32EPNum == 0)//ctrl in
					HID_CtrlDataInAck();
				else if(u32EPNum == 1)//interrupt in
					HID_IntInCallback();					
			}
			else if ((u32PacketType == STS_OUT0_ACK) || (u32PacketType == STS_OUT1_ACK))
			{
				// Set this EP event as non-handled.
				//psDevice->u16EPEventFlags |= (1 << (u32EPNum + 6));
				if (u32PacketType == STS_OUT1_ACK)
					g_HID_sDevice.abData0[u32EPNum] = FALSE;
				else
					g_HID_sDevice.abData0[u32EPNum] = TRUE;
				if(u32EPNum == 0)//ctrl out
					HID_CtrlDataOutAck();
				else if(u32EPNum == 2)//interrupt out
					HID_IntOutCallback();
			}
		}
	}
}




