#include <stdio.h>
//#include "wbtypes.h"

#define COM_PORT   0

#define GP_BASE       (0x50004000)
//#define GCR_BASE      (0x50000000)
#define INT_BA        (0x50000300)
#define INT_BA        (0x50000300)
#define UART_BA       (0x40050000)
#define CLK_BASE      (0x50000200)

#define REG_GPA_PMD             (GP_BASE+0x00)    //Mode Register 
#define REG_GPA_DOUT			(GP_BASE+0x08)    //Bus Status Register#define REG_GPB_PMD             (GP_BASE+0x40)    //Mode Register 
#define REG_GPB_DOUT			(GP_BASE+0x48)    //Bus Status Register
#define REG_GPB_PIN			    (GP_BASE+0x50)    //Interrupt Enable Register 

#define DEBUG   1
#define REG_IPRSTC2     (GCR_BASE+0xC)

#define INPUT       0
#define OUTPUT      1
#define ENABLE	1
#define DISABLE	0

/*----- Define the CAN0 registers -----*/
#define REG_UART0_RBR           (UART_BA+0x00)   //Mode Register 
#define REG_UART0_THR		    (UART_BA+0x00)    //Command Register 
#define REG_UART0_IER			(UART_BA+0x04)    //Bus Status Register
#define REG_UART0_FCR			(UART_BA+0x08)    //Interrupt Status Register 
#define REG_UART0_LCR		    (UART_BA+0x0C)    //Interrupt Enable Register 
#define REG_UART0_MCR			(UART_BA+0x10)    //Bit Timing Register
#define REG_UART0_MSR			(UART_BA+0x14)    //Bit Timing Register
#define REG_UART0_FSR		    (UART_BA+0x18)    //Error Capture Register
#define REG_UART0_ISR		    (UART_BA+0x1c)    //Receiver Error Counter Register 
#define REG_UART0_TOR			(UART_BA+0x20)    //Trasmit Error Counter Register
#define REG_UART0_BAUD			(UART_BA+0x24)    //Trasmit Error Counter Register
#define REG_UART0_IRCR			(UART_BA+0x28)    //Trasmit Error Counter Register
#define REG_UART0_LIN_BCNT		(UART_BA+0x2C)    //Trasmit Error Counter Register
#define REG_UART0_FUN_SEL		(UART_BA+0x30)    //Trasmit Error Counter Register


#define REG_UART1_RBR           (UART_BA+0x100000)    //Mode Register 
#define REG_UART1_THR		    (UART_BA+0x100000)    //Command Register 
#define REG_UART1_IER			(UART_BA+0x100004)    //Bus Status Register
#define REG_UART1_FCR			(UART_BA+0x100008)    //Interrupt Status Register 
#define REG_UART1_LCR		    (UART_BA+0x10000C)    //Interrupt Enable Register 
#define REG_UART1_MCR			(UART_BA+0x100010)    //Bit Timing Register
#define REG_UART1_MSR			(UART_BA+0x100014)    //Bit Timing Register
#define REG_UART1_FSR		    (UART_BA+0x100018)    //Error Capture Register
#define REG_UART1_ISR		    (UART_BA+0x10001C)    //Receiver Error Counter Register 
#define REG_UART1_TOR			(UART_BA+0x100020)    //Trasmit Error Counter Register
#define REG_UART1_BAUD			(UART_BA+0x100024)    //Trasmit Error Counter Register
#define REG_UART1_IRCR			(UART_BA+0x100028)    //Trasmit Error Counter Register
#define REG_UART1_LIN_BCNT		(UART_BA+0x10002C)    //Trasmit Error Counter Register
#define REG_UART1_FUN_SEL		(UART_BA+0x100030)    //Trasmit Error Counter Register

     
#define COM_RX           (UART_BA+COM_PORT*0x100000)    //Mode Register 
#define COM_TX		      (UART_BA+COM_PORT*0x100000)    //Command Register 
#define COM_IER			  (UART_BA+COM_PORT*0x100000+0x04)    //Bus Status Register
#define COM_FCR			  (UART_BA+COM_PORT*0x100000+0x08)    //Interrupt Status Register 
#define COM_LCR		      (UART_BA+COM_PORT*0x100000+0x0C)    //Interrupt Enable Register 
#define COM_MCR			  (UART_BA+COM_PORT*0x100000+0x10)    //Bit Timing Register
#define COM_MSR			  (UART_BA+COM_PORT*0x100000+0x14)    //Bit Timing Register
#define COM_FSR		      (UART_BA+COM_PORT*0x100000+0x18)    //Error Capture Register
#define COM_ISR		      (UART_BA+COM_PORT*0x100000+0x1C)    //Receiver Error Counter Register 
#define COM_TOR			  (UART_BA+COM_PORT*0x100000+0x20)    //Trasmit Error Counter Register
#define COM_BAUD		  (UART_BA+COM_PORT*0x100000+0x24)    //Trasmit Error Counter Register
#define COM_IRCR		  (UART_BA+COM_PORT*0x100000+0x28)    //Trasmit Error Counter Register
#define COM_LIN_BCNT	  (UART_BA+COM_PORT*0x100000+0x2C)    //Trasmit Error Counter Register
#define COM_FUN_SEL		  (UART_BA+COM_PORT*0x100000+0x30)    //Trasmit Error Counter Register

/*----- Input / Output function -----*/
#define outp(port,value)	*((volatile unsigned char *)(port))=(value)
#define inp(port)			*((volatile unsigned char *)(port))
//#define outpw(port,value)	*((volatile unsigned int *)(port))=(value)
//#define	inpw(port)			*((volatile unsigned int *)(port))
#define outph(x,y)  		*((volatile unsigned short *)(x)) = (y)
#define inph(port)  			*((volatile unsigned short *)(port))


int8_t GetChar(void);
int32_t sysGetNum(void);
void UART0_IRQHandler(void);
void UART1_IRQHandler(void);


