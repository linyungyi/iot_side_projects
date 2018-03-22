/**************************************************
 *
 * Part one of the system initialization code, contains low-level
 * initialization, plain thumb variant.
 *
 * Copyright 2010 IAR Systems. All rights reserved.
 *
 * $Revision: 34539 $
 *
 **************************************************/

;
; The modules in this file are included in the libraries, and may be replaced
; by any user-defined modules that define the PUBLIC symbol _program_start or
; a user defined start symbol.
; To override the cstartup defined in the library, simply add your modified
; version to the workbench project.
;
; The vector table is normally located at address 0.
; When debugging in RAM, it can be located in RAM, aligned to at least 2^6.
; The name "__vector_table" has special meaning for C-SPY:
; it is where the SP start value is found, and the NVIC vector
; table register (VTOR) is initialized to this address if != 0.
;
; Cortex-M version
;

        MODULE  ?cstartup

        ;; Forward declaration of sections.
        SECTION CSTACK:DATA:NOROOT(3)

        SECTION .intvec:CODE:NOROOT(2)
        
        EXTERN  __iar_program_start
        EXTERN  SystemInit
        PUBLIC  __vector_table

        DATA
__vector_table
        DCD     sfe(CSTACK)                 ; Top of Stack
        DCD     Reset_Handler               ; Reset Handler
        DCD     NMI_Handler                 ; NMI Handler
        DCD     HardFault_Handler           ; Hard Fault Handler
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     SVC_Handler                 ; SVCall Handler
        DCD     0xFFFFFFFF                   ; Reserved
        DCD     0xFFFFFFFF                  ; Reserved
        DCD     PendSV_Handler              ; PendSV Handler
        DCD     SysTick_Handler             ; SysTick Handler
        
                        ; External Interrupts

        DCD     BOD_IRQHandler              ; Brownout low voltage detected interrupt                 
        DCD     WDT_IRQHandler              ; Watch Dog Timer interrupt                              
        DCD     EINT0_IRQHandler            ; External signal interrupt from PB.14 pin                
        DCD     EINT1_IRQHandler            ; External signal interrupt from PB.15 pin                
        DCD     GPIOP0P1_IRQHandler         ; External signal interrupt from P0[15:0] / P1[13:0]     
        DCD     GPIOP2P3P4_IRQHandler       ; External interrupt from P2[15:0]/P3[15:0]/P4[15:0]     
        DCD     PWMA_IRQHandler             ; PWM0 or PWM2 interrupt                                 
        DCD     PWMB_IRQHandler             ; PWM1 or PWM3 interrupt                                 
        DCD     TMR0_IRQHandler             ; Timer 0 interrupt                                      
        DCD     TMR1_IRQHandler             ; Timer 1 interrupt                                      
        DCD     TMR2_IRQHandler             ; Timer 2 interrupt                                      
        DCD     TMR3_IRQHandler             ; Timer 3 interrupt                                      
        DCD     UART0_IRQHandler            ; UART0 interrupt                                        
        DCD     UART1_IRQHandler            ; UART1 interrupt                                        
        DCD     SPI0_IRQHandler             ; SPI0 interrupt                                         
        DCD     SPI1_IRQHandler             ; SPI1 interrupt                                         
        DCD     SPI2_IRQHandler             ; SPI2 interrupt                                         
        DCD     SPI3_IRQHandler             ; SPI3 interrupt                                         
        DCD     I2C_IRQHandler              ; I2C interrupt                                         
        DCD     Default_Handler             ; Reserved
        DCD     Default_Handler             ; Reserved
        DCD     Default_Handler             ; Reserved                                         
        DCD     Default_Handler             ; Reserved
        DCD     Default_Handler             ; Reserved
        DCD     Default_Handler             ; Reserved
        DCD     ACMP_IRQHandler             ; Analog Comparator-0 or Comaprator-1 interrupt          
        DCD     Default_Handler							; Reserved
        DCD     Default_Handler 						; Reserved
        DCD     PWRWU_IRQHandler            ; Clock controller interrupt for chip wake up from power-
        DCD     ADC_IRQHandler              ; ADC interrupt                                          
        DCD     Default_Handler             ; Reserved
        DCD     RTC_IRQHandler              ; Real time clock interrupt                              

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;
;; Default interrupt handlers.
;;
      PUBWEAK Reset_Handler
      PUBWEAK NMI_Handler       
      PUBWEAK HardFault_Handler 
      PUBWEAK SVC_Handler       
      PUBWEAK PendSV_Handler    
      PUBWEAK SysTick_Handler   
      PUBWEAK BOD_IRQHandler   
      PUBWEAK WDT_IRQHandler   
      PUBWEAK EINT0_IRQHandler 
      PUBWEAK EINT1_IRQHandler 
      PUBWEAK GPIOP0P1_IRQHandler  
      PUBWEAK GPIOP2P3P4_IRQHandler 
      PUBWEAK PWMA_IRQHandler  
      PUBWEAK PWMB_IRQHandler  
      PUBWEAK TMR0_IRQHandler  
      PUBWEAK TMR1_IRQHandler  
      PUBWEAK TMR2_IRQHandler  
      PUBWEAK TMR3_IRQHandler  
      PUBWEAK UART0_IRQHandler 
      PUBWEAK UART1_IRQHandler 
      PUBWEAK SPI0_IRQHandler  
      PUBWEAK SPI1_IRQHandler  
;      PUBWEAK SPI2_IRQHandler  
;      PUBWEAK SPI3_IRQHandler  
      PUBWEAK I2C_IRQHandler  
;      PUBWEAK I2C1_IRQHandler  
;      PUBWEAK CAN0_IRQHandler   
;      PUBWEAK USBD_IRQHandler   
;      PUBWEAK PS2_IRQHandler   
      PUBWEAK ACMP_IRQHandler  
;      PUBWEAK PDMA_IRQHandler 
;      PUBWEAK I2S_IRQHandler
      PUBWEAK PWRWU_IRQHandler  
      PUBWEAK ADC_IRQHandler    
      PUBWEAK RTC_IRQHandler    

        THUMB
        SECTION .text:CODE:REORDER(2)
Reset_Handler
                LDR     R0, =SystemInit
                BLX     R0
;*************Add by Nuvoton***************

;****************************************** 
               LDR     R0, =__iar_program_start
               BX      R0
              SECTION .text:CODE:REORDER(2)
NMI_Handler       
HardFault_Handler
        LDR    R0, [R13, #24]        ; Get previous PC
        LDRH   R1, [R0]              ; Get instruction
        LDR    R2, =0xBEAB           ; The sepcial BKPT instruction
        CMP    R1, R2                ; Test if the instruction at previous PC is BKPT
        BNE    HardFault_Handler_Ret ; Not BKPT

        ADDS   R0, #4                ; Skip BKPT and next line
        STR    R0, [R13, #24]        ; Save previous PC

        BX     LR
HardFault_Handler_Ret
        B      .

SVC_Handler       
PendSV_Handler    
SysTick_Handler   
BOD_IRQHandler   
WDT_IRQHandler   
EINT0_IRQHandler 
EINT1_IRQHandler 
GPIOP0P1_IRQHandler
GPIOP2P3P4_IRQHandler
PWMA_IRQHandler  
PWMB_IRQHandler  
TMR0_IRQHandler  
TMR1_IRQHandler  
TMR2_IRQHandler  
TMR3_IRQHandler  
UART0_IRQHandler
UART1_IRQHandler 
SPI0_IRQHandler  
SPI1_IRQHandler  
SPI2_IRQHandler  
SPI3_IRQHandler  
I2C_IRQHandler
;I2C1_IRQHandler
;CAN0_IRQHandler
;USBD_IRQHandler
;PS2_IRQHandler
ACMP_IRQHandler  
;PDMA_IRQHandler
;I2S_IRQHandler
PWRWU_IRQHandler
ADC_IRQHandler    
RTC_IRQHandler    
Default_Handler          
        B Default_Handler 
        
; int SH_DoCommand(int n32In_R0, int n32In_R1, int *pn32Out_R0);
; Input
;	R0,n32In_R0: semihost register 0
;	R1,n32In_R1: semihost register 1
; Output
;	R2,*pn32Out_R0: semihost register 0
; Return
;	0: No ICE debug
;	1: ICE debug
SH_DoCommand	
        EXPORT SH_DoCommand
        BKPT   0xAB                  ; Wait ICE or HardFault
                                     ; ICE will step over BKPT directly
                                     ; HardFault will step BKPT and the next line
        B      SH_ICE
SH_HardFault                         ; Captured by HardFault
        MOVS   R0, #0                ; Set return value to 0
        BX     lr                    ; Return
SH_ICE                               ; Captured by ICE
        ; Save return value
        CMP    R2, #0
        BEQ    SH_End
        STR    R0, [R2]              ; Save the return value to *pn32Out_R0
SH_End
        MOVS   R0, #1                ; Set return value to 1
        BX     lr                    ; Return

        END
