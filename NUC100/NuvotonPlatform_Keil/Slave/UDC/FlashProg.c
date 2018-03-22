/*---------------------------------------------------------------------------------------------------------*/
/*                                                                                                         */
/* Copyright (c) Nuvoton Technology Corp. All rights reserved.                                             */
/*                                                                                                         */
/*---------------------------------------------------------------------------------------------------------*/

/*---------------------------------------------------------------------------------------------------------*/
/* Includes of system headers                                                                              */
/*---------------------------------------------------------------------------------------------------------*/
#include "NUC1xx.h"
#include "FMC.h"
 #undef inp32
#undef outp32
#include "UDC.h"


/*---------------------------------------------------------------------------------------------------------*/
/* Macro, type and constant definitions                                                                    */
/*---------------------------------------------------------------------------------------------------------*/

#define FLASH_PAGE_SIZE         512

extern uint32_t g_SecurityLockBit;
extern uint32_t g_apromSize;

uint8_t u8FormatData[62] = 
{
    0xEB, 0x3C, 0x90, 0x4D, 0x53, 0x44, 0x4F, 0x53,
    0x35, 0x2E, 0x30, 0x00, 0x02, 0x01, 0x06, 0x00,
    0x02, 0x00, 0x02, 0xA8, 0x00, 0xF8, 0x01, 0x00,
    0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x29, 0xB9,
    0xC1, 0xAA, 0x42, 0x4E, 0x4F, 0x20, 0x4E, 0x41,
    0x4D, 0x45, 0x20, 0x20, 0x20, 0x20, 0x46, 0x41,
    0x54, 0x31, 0x32, 0x20, 0x20, 0x20
};


void FMC_ReadPage(uint32_t u32startAddr, uint32_t * u32buff)
{
    uint32_t i;
       
    for (i = 0; i < FLASH_PAGE_SIZE/4; i++)
    {
        u32buff[i] = 0;
    }                

    if (u32startAddr == 0x00000000)
    {
        my_memcpy((uint8_t *)u32buff, u8FormatData, 62);
        
        u32buff[FLASH_PAGE_SIZE/4-1] = 0xAA550000;            
    }
    else
    {
        if ( (u32startAddr == (FAT_SECTORS * 512)) || (u32startAddr == ((FAT_SECTORS+1) * 512)) )
        {
            u32buff[0] = 0x00FFFFF8;
        }
    }
}


//void DataFlashRead(uint32_t addr, uint32_t buffer)
//{
    /* This is low level read function of USB Mass Storage */
        
//    FMC_ReadPage(addr, (uint32_t *)buffer);
//}


void FMC_ProgramPage(uint32_t u32startAddr, uint32_t * u32buff)
{
    uint32_t i;
    
    for (i = 0; i < FLASH_PAGE_SIZE/4; i++)
    {
        FMC_Write(u32startAddr + i*4, u32buff[i]);
    }    
}

void DataFlashWrite(uint32_t addr, uint32_t buffer)
{
    /* This is low level write function of USB Mass Storage */
        
    if ((addr >= DATA_SECTOR_ADDRESS) && (addr < (DATA_SECTOR_ADDRESS+g_apromSize)))
    {
        addr -= DATA_SECTOR_ADDRESS;
        
        if(addr == 0)
        {
        	uint32_t regcnf0, regcnf1;
        	
			FMC_Read(Config0, &regcnf0);
			FMC_Read(Config1, &regcnf1);
				
			g_SecurityLockBit = regcnf0 & 0x2;
			
			if (g_SecurityLockBit == 0)//security lock enable
        	{	
        	    if(regcnf0 & 0x4)//erase data flash
				{
					EraseAP(FALSE, 0, (g_apromSize < 0x20000)?0x20000:g_apromSize);//erase all aprom including data flash
				}
				else
					EraseAP(TRUE, 0, 0);//don't erase data flash
        	}
        }

        if (g_SecurityLockBit)//security lock not set
        {
        	FMC_Erase(addr);
            FMC_ProgramPage(addr, (uint32_t *) buffer);
        }
        else//security lock enable
        {
            FMC_ProgramPage(addr, (uint32_t *) buffer);
        }    
    }                
}              

