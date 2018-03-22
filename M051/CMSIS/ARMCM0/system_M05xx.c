/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright(c) 2009 Nuvoton Technology Corp. All rights reserved.                                         */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
#include <stdint.h>
#include "M05xx.h"



/*----------------------------------------------------------------------------
  Clock Definitions
 *----------------------------------------------------------------------------*/
uint32_t SystemCoreClock  = __HSI;   /*!< System Clock Frequency (Core Clock) */

uint32_t gau32ClkSrcTbl[] = {__XTAL, __HSI, __IRC10K, __IRC22M};

/*----------------------------------------------------------------------------
  Clock functions
 *----------------------------------------------------------------------------*/
void SystemCoreClockUpdate (void)            /* Get Core Clock Frequency      */
{
  /* Determine clock frequency according to clock register values             */

  /* NEEDS TO BE IMPLEMENTED */

}

/*---------------------------------------------------------------------------------------------------------*/
/* Function: SystemInit                                                                                    */
/*                                                                                                         */
/* Parameters:                                                                                             */
/*      None                                                                                               */
/*                                                                                                         */
/* Returns:                                                                                                */
/*      None                                                                                               */
/*                                                                                                         */
/* Description:                                                                                            */
/*      The necessary initializaiton of systerm.                                                           */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/
void SystemInit (void)
{

}

