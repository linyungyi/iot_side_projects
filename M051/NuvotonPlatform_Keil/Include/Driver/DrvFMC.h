/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright(c) 2009 Nuvoton Technology Corp. All rights reserved.                                         */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#ifndef _DRVFMC_H
#define _DRVFMC_H

#include "M05xx.h"


/*---------------------------------------------------------------------------------------------------------*/
/*  Define Version number								                                                   */
/*---------------------------------------------------------------------------------------------------------*/
#define DRVFMC_MAJOR_NUM	1
#define DRVFMC_MINOR_NUM	00
#define DRVFMC_BUILD_NUM	1

/*---------------------------------------------------------------------------------------------------------*/
/*  Version define with SysInfra				                                                           */
/*---------------------------------------------------------------------------------------------------------*/
#define DRVFMC_VERSION_NUM     _SYSINFRA_VERSION(DRVFMC_MAJOR_NUM, DRVFMC_MINOR_NUM, DRVFMC_BUILD_NUM)
							   
/*---------------------------------------------------------------------------------------------------------*/
/*  Define Error Code									                                                   */
/*---------------------------------------------------------------------------------------------------------*/
// E_DRVFMC_ERR_ISP_FAIL  		ISP Failed when illegal condition occurs
#define E_DRVFMC_ERR_ISP_FAIL   _SYSINFRA_ERRCODE(TRUE, MODULE_ID_DRVFMC, 1)


/*---------------------------------------------------------------------------------------------------------*/
/* Define parameter                                                                                        */
/*---------------------------------------------------------------------------------------------------------*/
#define CONFIG0         0x00300000

/*---------------------------------------------------------------------------------------------------------*/
/*  Flash Boot Selector 								                                                   */
/*---------------------------------------------------------------------------------------------------------*/
typedef enum {APROM = 0, LDROM = 1} E_FMC_BOOTSELECT;

/*---------------------------------------------------------------------------------------------------------*/
/* Define FMC functions prototype                                                                          */
/*---------------------------------------------------------------------------------------------------------*/
void 	 DrvFMC_EnableISP(int32_t i32Enable);
void 	 DrvFMC_BootSelect(E_FMC_BOOTSELECT boot);
void 	 DrvFMC_EnableLDUpdate(int32_t i32Enable);
void 	 DrvFMC_EnableConfigUpdate(int32_t i32Enable);
void 	 DrvFMC_EnablePowerSaving(int32_t i32Enable);
void 	 DrvFMC_EnableLowSpeedMode(int32_t i32Enable);
int32_t  DrvFMC_ReadCID(uint32_t * u32data);
int32_t  DrvFMC_ReadDID(uint32_t * u32data);
int32_t  DrvFMC_ReadPID(uint32_t * u32data);
int32_t  DrvFMC_Write(uint32_t u32addr, uint32_t u32data);
int32_t  DrvFMC_Read(uint32_t u32addr, uint32_t * u32data);
int32_t  DrvFMC_Erase(uint32_t u32addr);
int32_t  DrvFMC_WriteConfig(uint32_t u32data0);
uint32_t DrvFMC_ReadDataFlashBaseAddr(void);
E_FMC_BOOTSELECT DrvFMC_GetBootSelect(void);

#endif

