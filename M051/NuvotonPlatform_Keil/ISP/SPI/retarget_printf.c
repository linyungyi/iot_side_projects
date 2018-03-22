/*
 *-----------------------------------------------------------------------------
 * The confidential and proprietary information contained in this file may
 * only be used by a person authorised under and to the extent permitted
 * by a subsisting licensing agreement from ARM Limited.
 *
 *            (C) COPYRIGHT 2009 ARM Limited.
 *                ALL RIGHTS RESERVED
 *
 * This entire notice must be reproduced on all copies of this file
 * and copies of this file may only be made by a person if such person is
 * permitted to do so under the terms of a subsisting license agreement
 * from ARM Limited.
 *
 *      SVN Information
 *
 *      Checked In          : $Date: Thu Jul  2 18:34:07 2009 $
 *
 *      Revision            : $Revision: 1.1 $
 *
 *      Release Information : Cortex-M0-AT510-r0p0-01rel0
 *-----------------------------------------------------------------------------
 */

//
// printf retargetting functions
//

#include <stdio.h>
#include <rt_misc.h>
#include <stdint.h>
#include "M05xx.h"


#if defined ( __CC_ARM   )
#if (__ARMCC_VERSION < 400000)
#else
// Insist on keeping widthprec, to avoid X propagation by benign code in C-lib
#pragma import _printf_widthprec
#endif
#endif


// Routine to write a char - specific to Cortex-M0 Integration Kit
void char_write(int ch)
{
#if 0
    while(UART1->FSR.TX_FULL == 1);
	UART1->DATA = ch;
    if(ch == '\n')
    {
        while(UART1->FSR.TX_FULL == 1);
        UART1->DATA = '\r';
    }

#else
    while(UART0->FSR.TX_FULL == 1);
	UART0->DATA = ch;
    if(ch == '\n')
    {
        while(UART0->FSR.TX_FULL == 1);
        UART0->DATA = '\r';
    }
#endif
}

int32_t char_read(void)
{
    int32_t ch;
#if 0
    while(UART1->FSR.RX_EMPTY == 1);
	ch = UART1->DATA;

#else
    while(UART0->FSR.RX_EMPTY == 1);
	ch = UART0->DATA;
#endif
    return ch;
}


//
// C library retargetting
//

struct __FILE { int handle; /* Add whatever you need here */ };
FILE __stdout;
FILE __stdin;


void _ttywrch(int ch)
{
  char_write(ch);
  return;
}

int fputc(int ch, FILE *f)
{
  char_write(ch);
  return ch;
}

int fgetc(FILE *f)
{
  int ch;
  ch = char_read();
  return ch;
}


int ferror(FILE *f) {
  return EOF;
}
