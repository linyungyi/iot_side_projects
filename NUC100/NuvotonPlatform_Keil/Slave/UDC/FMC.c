#include <stdio.h>
#include "NUC1xx.h"
//#include "Platform.h"
#include "Driver/DrvUSB.h"
#include "Driver/DrvGPIO.h"
#include "USB/HIDDevice.h"
#include "USB/USBInfra.h"
#include "FMC.h"

extern uint32_t g_apromSize;

// APROM 128K
static uint32_t APROM_END;//0x00020000


int FMC_Write(unsigned int address, unsigned int data)
{
    unsigned int Reg;
    
    outp32(ISPCMD, ISP_Program);
    outp32(ISPADR, address);
    outp32(ISPDAT, data);
    outp32(ISPTRG, ISPGO); 
    
    __ISB();
    
    Reg = inp32(FISPCON);
    if (Reg & ISPFF)
    {
    	outp32(FISPCON, Reg);
    	return -1;
    }
    return 0;
}

int FMC_Read(unsigned int address, unsigned int * data)
{
    unsigned int Reg;

    outp32(ISPCMD, ISP_Read);
    outp32(ISPADR, address);
    outp32(ISPDAT, 0x00000000);
	outp32(ISPTRG, ISPGO); 
    
    __ISB();

    Reg = inp32(FISPCON);
    if (Reg & ISPFF)
    {
    	outp32(FISPCON, Reg);
    	return -1;
    }
 
	*data = inp32(ISPDAT);

    return 0;
}


int FMC_Erase(unsigned int address)
{
    unsigned int Reg;
    
    outp32(ISPCMD, ISP_PageErase);
    outp32(ISPADR, address);
    outp32(ISPTRG, ISPGO); 

	__ISB();
    
    Reg = inp32(FISPCON);
    if (Reg & ISPFF)
    {
    	outp32(FISPCON, Reg);
    	return -1;
    }
    return 0;
}


void ReadData(unsigned int addr_start, unsigned int addr_end, unsigned int* data)    // Read data from flash
{
    unsigned int rLoop;

    for ( rLoop = addr_start; rLoop < addr_end; rLoop += 4 ) 
    {     
		FMC_Read(rLoop, data);
		data++;
    }
    return;
}

void WriteData(unsigned int addr_start, unsigned int addr_end, unsigned int *data)  // Write data into flash
{
    unsigned int wLoop;
    
    for ( wLoop = addr_start; wLoop < addr_end; wLoop+=4 ) 
    {
        FMC_Write(wLoop, *data);
        data++;
    }
}



//bAprom == TRUE erase all aprom besides data flash
void EraseAP(BOOL bAprom, unsigned int addr_start, unsigned int addr_end)
{
    unsigned int eraseLoop;
    unsigned int erase_end;

    APROM_END = g_apromSize;
    if(APROM_END >= 0x20000)
    {
    	FMC_Read(Config0, &erase_end);
    	if((erase_end&0x01)==0)//DFEN enable
    	{
    		FMC_Read(Config1, &APROM_END);
    	}
    }
    if(APROM_END >= 0xFFFFF || APROM_END < 0x1000)//avoid config1 value from error
    	APROM_END = 0x20000;
    
    if(bAprom == TRUE)
    {
    	eraseLoop = APROM_BASE;
    	erase_end = APROM_END;
    }
    else
    {
    	eraseLoop = addr_start;
    	erase_end = addr_end;
    }
    for ( ; eraseLoop < erase_end; eraseLoop += PAGE_SIZE )
    {
        FMC_Erase(eraseLoop);
    }           
    return;
}

void UpdateConfig(unsigned int *data, unsigned int *res)
{
	
	FMC->ISPCON.CFGUEN = 1;//enable config update
    FMC_Erase(Config0);
    //FMC_Write(Config0, 0xFFFFFF7F); // Disable Lock bit
    //FMC_Write(Config1, 0x0001F000);
    
   	FMC_Write(Config0, *data);
   	FMC_Write(Config1, *(data+1));
        
    FMC_Read(Config0, res);
    FMC_Read(Config1, res+1);
    
    FMC->ISPCON.CFGUEN = 0;//disable config update

}
