/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright(c) 2009 Nuvoton Technology Corp. All rights reserved.                                         */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#ifndef __DRVCAN_H__
#define __DRVCAN_H__


#include "NUC1xx.h"

/*---------------------------------------------------------------------------------------------------------*/
/*  Define Version number								                                                   */
/*---------------------------------------------------------------------------------------------------------*/
#define DRVCAN_MAJOR_NUM	1
#define DRVCAN_MINOR_NUM	00
#define DRVCAN_BUILD_NUM	1

/*---------------------------------------------------------------------------------------------------------*/
/*  Version define with SysInfra				                                                           */
/*---------------------------------------------------------------------------------------------------------*/
#define DRVCAN_VERSION_NUM     _SYSINFRA_VERSION(DRVCAN_MAJOR_NUM,DRVCAN_MINOR_NUM,DRVCAN_BUILD_NUM) 
								 
/*---------------------------------------------------------------------------------------------------------*/
/*  Define Error Code									                                                   */
/*---------------------------------------------------------------------------------------------------------*/
// E_DRVCAN_ERR_TIMEOUT  				Time out
// E_DRVCAN_ERR_PORT_INVALID		 	Wrong port
// E_DRVCAN_ARGUMENT                	Wrong Argument (Wrong UART Port)
#define E_DRVCAN_ERR_TIMEOUT   				_SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVCAN, 1)
#define E_DRVCAN_ERR_PORT_INVALID	  		_SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVCAN, 2)
#define E_DRVCAN_ERR_ARGUMENT               _SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVCAN, 3)


typedef void (PFN_DRVCAN_CALLBACK)(uint32_t userData);

/*---------------------------------------------------------------------------------------------------------*/
/* Port Number                                                                                             */
/*---------------------------------------------------------------------------------------------------------*/
#define DRVCAN_PORT0		0x0000
#define DRVCAN_PORT1		0x4000

typedef enum 
{
	CAN_PORT0 = DRVCAN_PORT0, 
	CAN_PORT1 = DRVCAN_PORT1
} CAN_PORT;


#define MODE_TX  0
#define MODE_RX  1

/*---------------------------------------------------------------------------------------------------------*/
/* Interrupt Flag        								                                                   */
/*---------------------------------------------------------------------------------------------------------*/
typedef enum 
{
	INT_RI  =  BIT0,	     /* Recevie Interrupt */
	INT_TI  =  BIT1,		 /* Transmit Interrupt */
	INT_WUI	=  BIT4,
	INT_ALI	=  BIT6,
	INT_BEI =  BIT7

} DRVCAN_INTFLAG;

/*---------------------------------------------------------------------------------------------------------*/
/* Error Capture        								                                                   */
/*---------------------------------------------------------------------------------------------------------*/
typedef enum 
{
	DRVCAN_ERRSTUFF =0,    /* Stuff Error 			*/
	DRVCAN_ERRFORM  =1,    /* Form Error 			*/
	DRVCAN_ERRCRC   =2,    /* CRC Error 			*/
	DRVCAN_ERRACK   =3,    /* Acknowledge Error  	*/
	DRVCAN_ERRBIT   =4     /* Bit Error 			*/
} DRVCAN_ERRFLAG;


/*---------------------------------------------------------------------------------------------------------*/
/*  Define FORMAT	// 0 - STANDARD, 	1- EXTENDED IDENTIFIER						                       */
/*---------------------------------------------------------------------------------------------------------*/
#define STANDARD_FORMAT     0
#define EXTENDED_FORMAT		1


/*---------------------------------------------------------------------------------------------------------*/
/*  Define TYPE		// 0 - DATA FRAME, 	1- REMOTE FRAME								                       */
/*---------------------------------------------------------------------------------------------------------*/
#define DATA_TYPE     	0
#define REMOTE_TYPE		1


/*---------------------------------------------------------------------------------------------------------*/
/*  Define Mask 																	                       */
/*---------------------------------------------------------------------------------------------------------*/
#define AMR_ALLPASS  0x0
#define AMR_ALLMASK  0xFFFFFFFF


/*---------------------------------------------------------------------------------------------------------*/
/*  Define BUS BITRATE																                       */
/*---------------------------------------------------------------------------------------------------------*/
#define BITRATE_100K 100
#define BITRATE_500K 500
#define BITRATE_1MB  1000

/*---------------------------------------------------------------------------------------------------------*/
/*  Define CAN Bit Time Setting Table                                                                      */
/*---------------------------------------------------------------------------------------------------------*/
/* Prototype : BITRATE_0000K_XXM                                                                           */
/*			   0000:CAN Bus speed XX:CAN Bus clock														   */
/* example   : BITRATE_1000K_12M                                                                           */
/*---------------------------------------------------------------------------------------------------------*/
#define BITRATE_100K_6M 			0x3420
#define BITRATE_500K_6M 			0x3510
#define BITRATE_1000K_6M  			0x4000
#define BITRATE_100K_12M 			0x4120
#define BITRATE_500K_12M 			0x4350
#define BITRATE_1000K_12M  			0x2150	     
#define BITRATE_100K_24M 			0x5790
#define BITRATE_500K_24M 			0x5980
#define BITRATE_1000K_24M  			0x6190
#define BITRATE_100K_48M 			0x7390
#define BITRATE_500K_48M 			0x7520
#define BITRATE_1000K_48M  			0x7690

/*---------------------------------------------------------------------------------------------------------*/
/*  Define CAN initialization data structure                                                               */
/*---------------------------------------------------------------------------------------------------------*/
typedef struct DRVCAN_STRUCT		  
{
    uint32_t        id;				/* ID identifier */
    uint32_t    	u32cData[2];	/* Data field */
    uint8_t   	    u8cLen;			/* Length of data field in bytes */
    uint8_t			u8cFormat;	  	/* 0 - STANDARD, 	1- EXTENDED IDENTIFIER */
    uint8_t	        u8cType;		/* 0 - DATA FRAME, 	1- REMOTE FRAME */
	uint8_t	        u8cOverLoad;	/* 0 - Disable, 	1- Enable */
}STR_CAN_T;


 
/*---------------------------------------------------------------------------------------------------------*/
/* Define CAN functions prototype                                                                          */
/*---------------------------------------------------------------------------------------------------------*/
 void    DrvCAN_Init(void);
 int32_t DrvCAN_Open(CAN_PORT port,uint32_t u32Reg);
 int32_t DrvCAN_Close(CAN_PORT port);
 int32_t DrvCAN_DisableInt(CAN_PORT port,int32_t u32InterruptFlag);
 int32_t DrvCAN_EnableInt(CAN_PORT port,int32_t u32InterruptFlag,PFN_DRVCAN_CALLBACK pfncallback);
 int32_t DrvCAN_GetErrorStatus(CAN_PORT port,DRVCAN_ERRFLAG u32ErrorFlag);
 int32_t DrvCAN_WaitReady(CAN_PORT port);
 int32_t DrvCAN_WriteMsg(CAN_PORT port,STR_CAN_T *Msg);
 int32_t DrvCAN_SetAcceptanceFilter(CAN_PORT port,int32_t id_Filter );
 int32_t DrvCAN_SetMaskFilter(CAN_PORT port,int32_t id_Filter );
 int32_t DrvCAN_GetVersion(void);
 int32_t DrvCAN_GetClock(void);
 STR_CAN_T DrvCAN_ReadMsg(CAN_PORT port);
 int32_t DrvCAN_SetBusTiming(CAN_PORT port,int8_t i8SynJumpWidth,int16_t i16TimeSeg1,int8_t i8TimeSeg2,int8_t SampPtNo);
 void DrvCAN_ReTransmission(int32_t bIsEnable);
#endif													 








