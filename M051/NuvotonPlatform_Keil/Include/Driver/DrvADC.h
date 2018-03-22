/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright(c) 2009 Nuvoton Technology Corp. All rights reserved.                                         */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#ifndef __DRVADC_H__
#define __DRVADC_H__


/*---------------------------------------------------------------------------------------------------------*/
/* Include related headers                                                                              */
/*---------------------------------------------------------------------------------------------------------*/
#include "M05xx.h"
#include "System/SysInfra.h"

/*---------------------------------------------------------------------------------------------------------*/
/* Macro, type and constant definitions                                                                    */
/*---------------------------------------------------------------------------------------------------------*/
/* version definition with SysInfra */
#define	DRVADC_MAJOR_NUM 1
#define	DRVADC_MINOR_NUM 00
#define	DRVADC_BUILD_NUM 1
#define DRVADC_VERSION_NUM    _SYSINFRA_VERSION(DRVADC_MAJOR_NUM, DRVADC_MINOR_NUM, DRVADC_BUILD_NUM)

/* error code definition */
#define E_DRVADC_ARGUMENT           _SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVADC, 1)
#define E_DRVADC_CHANNELNUM         _SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVADC, 2)

typedef int32_t	ERRCODE;
typedef enum {ADC_SINGLE_END, ADC_DIFFERENTIAL} ADC_INPUT_MODE;
typedef enum {ADC_SINGLE_OP, ADC_SINGLE_CYCLE_OP, ADC_CONTINUOUS_OP} ADC_OPERATION_MODE;
typedef enum {EXT_12MHZ=0, INT_PLL=1, INT_RC22MHZ=2} ADC_CLK_SRC;
typedef enum {LOW_LEVEL=0, HIGH_LEVEL=1, FALLING_EDGE=2, RISING_EDGE=3} ADC_EXT_TRI_COND;
typedef enum {EXT_INPUT_SIGNAL, INT_BANDGAP, INT_TEMPERATURE_SENSOR} ADC_CH7_SRC;
typedef enum {LESS_THAN, GREATER_OR_EQUAL} ADC_COMP_CONDITION;
typedef void (DRVADC_ADC_CALLBACK)(uint32_t u32UserData);
typedef void (DRVADC_ADCMP0_CALLBACK)(uint32_t u32UserData);
typedef void (DRVADC_ADCMP1_CALLBACK)(uint32_t u32UserData);

#define _DRVADC_CONV() (ADC->ADCR.ADST=1)

/*---------------------------------------------------------------------------------------------------------*/
/* Define Function Prototypes                                                                              */
/*---------------------------------------------------------------------------------------------------------*/
void DrvADC_Open(ADC_INPUT_MODE InputMode, ADC_OPERATION_MODE OpMode, uint8_t u8ChannelSelBitwise, ADC_CLK_SRC ClockSrc, uint8_t u8AdcDivisor);
void DrvADC_Close(void);
void DrvADC_SetAdcChannel(uint8_t u8ChannelSelBitwise, ADC_INPUT_MODE InputMode);
void DrvADC_ConfigAdcChannel7(ADC_CH7_SRC Ch7Src);
void DrvADC_SetAdcInputMode(ADC_INPUT_MODE InputMode);
void DrvADC_SetAdcOperationMode(ADC_OPERATION_MODE OpMode);
void DrvADC_SetAdcClkSrc(ADC_CLK_SRC ClockSrc);
void DrvADC_SetAdcDivisor(uint8_t u8AdcDivisor);
void DrvADC_EnableAdcInt(DRVADC_ADC_CALLBACK callback, uint32_t u32UserData);
void DrvADC_DisableAdcInt(void);
void DrvADC_EnableAdcmp0Int(DRVADC_ADCMP0_CALLBACK callback, uint32_t u32UserData);
void DrvADC_DisableAdcmp0Int(void);
void DrvADC_EnableAdcmp1Int(DRVADC_ADCMP1_CALLBACK callback, uint32_t u32UserData);
void DrvADC_DisableAdcmp1Int(void);
uint32_t DrvADC_GetConversionRate(void);
void DrvADC_ExtTriggerEnable(ADC_EXT_TRI_COND TriggerCondition);
void DrvADC_ExtTriggerDisable(void);
void DrvADC_StartConvert(void);
void DrvADC_StopConvert(void);
uint32_t DrvADC_IsConversionDone(void);
uint32_t DrvADC_GetConversionData(uint8_t u8ChannelNum);
void DrvADC_PdmaEnable(void);
uint32_t DrvADC_IsDataValid(uint8_t u8ChannelNum);
uint32_t DrvADC_IsDataOverrun(uint8_t u8ChannelNum);
ERRCODE DrvADC_Adcmp0Enable(uint8_t u8CmpChannelNum, ADC_COMP_CONDITION CmpCondition, uint16_t u16CmpData, uint8_t CmpMatchCount);
ERRCODE DrvADC_Adcmp1Enable(uint8_t u8CmpChannelNum, ADC_COMP_CONDITION CmpCondition, uint16_t u16CmpData, uint8_t CmpMatchCount);
void DrvADC_Adcmp0Disable(void);
void DrvADC_Adcmp1Disable(void);
void DrvADC_SelfCalEnable (void);
uint32_t DrvADC_IsCalDone(void);
uint32_t DrvADC_GetVersion (void);

#endif
