#include <stdio.h>
#include <string.h>
#include "NUC1xx.h"

#include "Driver/DrvI2C.h"
#include "Driver/DrvGPIO.h"
#include "cmd.h"

#define CMD_UPDATE_APROM	0x000000A0
#define CMD_UPDATE_CONFIG	0x000000A1
#define CMD_READ_CONFIG		0x000000A2
#define CMD_ERASE_ALL		0x000000A3
#define CMD_SYNC_PACKNO		0x000000A4
#define CMD_GET_FWVER		0x000000A6
#define CMD_APROM_SIZE		0x000000AA
#define CMD_RUN_APROM		0x000000AB
#define CMD_RUN_LDROM		0x000000AC
#define CMD_RESET			0x000000AD

#define CMD_GET_DEVICEID	0x000000B1

#define CMD_PROGRAM_WOERASE 	0x000000C2
#define CMD_PROGRAM_WERASE 	 	0x000000C3
#define CMD_READ_CHECKSUM 	 	0x000000C8
#define CMD_WRITE_CHECKSUM 	 	0x000000C9
#define CMD_GET_FLASHMODE 	 	0x000000CA

#define APROM_MODE	1
#define LDROM_MODE	2

extern uint8_t imageBegin, imageEnd;
__align(4) uint8_t rcvbuf[PACKET_SIZE];
__align(4) uint8_t sendbuf[PACKET_SIZE];
__align(4) uint8_t aprom_buf[PAGE_SIZE];
uint8_t FileBuffer[FILE_BUFFER];

unsigned int g_packno = 1;
unsigned short gcksum;


void WordsCpy(void *dest, void *src, int32_t size)
{
    uint8_t *pu8Src, *pu8Dest;
    int32_t i;
    
    pu8Dest = (uint8_t *)dest;
    pu8Src  = (uint8_t *)src;
    
    for(i=0;i<size;i++)
        pu8Dest[i] = pu8Src[i]; 
}

uint16_t Checksum (unsigned char *buf, int len)
{
    int i;
    uint16_t c;

    for (c=0, i=0; i < len; i++) {
        c += buf[i];
    }
    return (c);
}

static uint16_t CalCheckSum(uint8_t *buf, uint32_t len)
{
	int i;
	uint16_t lcksum = 0;
	
	for(i = 0; i < len; i+=PAGE_SIZE)
	{
		WordsCpy(aprom_buf, buf + i, PAGE_SIZE);
		if(len - i >= PAGE_SIZE)
			lcksum += Checksum(aprom_buf, PAGE_SIZE);
		else
			lcksum += Checksum(aprom_buf, len - i);
	}
    
    return lcksum;
    
}


BOOL CmdSyncPackno(int flag)
{
	BOOL Result;
	unsigned long cmdData;
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_SYNC_PACKNO;//CMD_UPDATE_APROM
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	WordsCpy(sendbuf+8, &g_packno, 4);
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;

	Result = RcvData();
	
	return Result;
}

BOOL CmdFWVersion(int flag, unsigned int *fwver)
{
	BOOL Result;
	unsigned long cmdData;
	unsigned int lfwver;
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_GET_FWVER;
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;

	Result = RcvData();
	if(Result)
	{
		WordsCpy(&lfwver, rcvbuf+8, 4);
		*fwver = lfwver;
	}
	
	return Result;
}


BOOL CmdGetDeviceID(int flag, unsigned int *devid)
{
	BOOL Result;
	unsigned long cmdData;
	unsigned int ldevid;
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_GET_DEVICEID;
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;

	Result = RcvData();
	if(Result)
	{
		WordsCpy(&ldevid, rcvbuf+8, 4);
		*devid = ldevid;
	}
	
	return Result;
}

BOOL CmdGetConfig(int flag, unsigned int *config)
{
	BOOL Result;
	unsigned long cmdData;
	unsigned int lconfig[2];
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_READ_CONFIG;
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;

	Result = RcvData();
	if(Result)
	{
		WordsCpy(&lconfig[0], rcvbuf+8, 4);
		WordsCpy(&lconfig[1], rcvbuf+12, 4);
		config[0] = lconfig[0];
		config[1] = lconfig[1];
	}
	
	return Result;
}

//uint32_t def_config[2] = {0xFFFFFF7F, 0x0001F000};
//CmdUpdateConfig(FALSE, def_config)
BOOL CmdUpdateConfig(int flag, uint32_t *conf)
{
	BOOL Result;
	unsigned long cmdData;
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_UPDATE_CONFIG;
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	WordsCpy(sendbuf+8, conf, 8);
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;

	Result = RcvData();
	
	return Result;
}

//for the commands
//CMD_RUN_APROM
//CMD_RUN_LDROM
//CMD_RESET
//CMD_ERASE_ALL
//CMD_GET_FLASHMODE
//CMD_WRITE_CHECKSUM
BOOL CmdRunCmd(uint32_t cmd, uint32_t *data)
{
	BOOL Result;
	uint32_t cmdData;
	
	//sync send&recv packno
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = cmd;
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	if(cmd == CMD_WRITE_CHECKSUM)
	{
		WordsCpy(sendbuf+8, &data[0], 4);
		WordsCpy(sendbuf+12, &data[1], 4);
	}
	g_packno++;
	
	Result = SendData();
	if(Result == FALSE)
		return Result;
	
	if((cmd == CMD_ERASE_ALL) || (cmd == CMD_GET_FLASHMODE) 
			|| (cmd == CMD_WRITE_CHECKSUM))
	{
		if(cmd == CMD_WRITE_CHECKSUM)
			SysTimerDelay(400000);//0.2s
		Result = RcvData();
		if(Result)
		{
			if(cmd == CMD_GET_FLASHMODE)
			{
				WordsCpy(&cmdData, rcvbuf+8, 4);
				*data = cmdData;
			}
		}
		
	}
	else if((cmd == CMD_RUN_APROM) || (cmd == CMD_RUN_LDROM) 
			|| (cmd == CMD_RESET))
		SysTimerDelay(1000000);//0.5s
	
	return Result;
}

BOOL CmdUpdateAprom(int flag)
{
	BOOL Result;
	unsigned int devid, config[2], i, mode;
	unsigned long readcn, sendcn, cmdData, startaddr, totallen, pos;
	unsigned short lcksum, get_cksum;
	
	g_packno = 1;
	
	Result = CmdSyncPackno(flag);
	if(Result == FALSE)
	{
		printf("send Sync Packno cmd fail\n");
		goto out;
	}
	Result = CmdRunCmd(CMD_GET_FLASHMODE, &mode);
	if(mode != LDROM_MODE)
	{
		printf("change to LDROM ");
		CmdRunCmd(CMD_RUN_LDROM, NULL);
		Result = CmdSyncPackno(flag);
		if(Result == FALSE)
		{
			printf("ldrom Sync Packno cmd fail\n");
			goto out;
		}
	
		Result = CmdRunCmd(CMD_GET_FLASHMODE, &mode);
		if(mode != LDROM_MODE)
		{
			printf("fail\n");
			goto out;
		}
		else
			printf("ok\n");
	}
	
	CmdGetDeviceID(flag, &devid);
	printf("DeviceID: %x\n", devid);
			
	CmdGetConfig(flag, config);
	printf("config0: %x\n", config[0]);
	printf("config1: %x\n", config[1]);

	/** send updata aprom command**/
	memset(sendbuf, 0, PACKET_SIZE);
	cmdData = CMD_UPDATE_APROM;//CMD_UPDATE_APROM
	WordsCpy(sendbuf+0, &cmdData, 4);
	WordsCpy(sendbuf+4, &g_packno, 4);
	g_packno++;
	//start address
	startaddr = 0;
	WordsCpy(sendbuf+8, &startaddr, 4);
	
	// Try to obtain hFile's size 
	totallen = &imageEnd - &imageBegin;//GetFileSize (hFileHandle, NULL); 
	WordsCpy(sendbuf+12, &totallen, 4);
			
	//read data from aprom.bin
	pos = 0;
	WordsCpy(FileBuffer, (char*)&imageBegin-1, FILE_BUFFER);
	pos += FILE_BUFFER-1;
	
	readcn = FILE_BUFFER;
	sendcn = PACKET_SIZE - 16;
	WordsCpy(sendbuf+16, FileBuffer, sendcn);

			
	//send CMD_UPDATE_APROM
	Result = SendData();
	if(Result == FALSE)
		goto out;
	
	for(i = 0; i < 16; i++)
		SysTimerDelay(1000000);//0.5s
	
	printf("rcv data2\n");
	
	Result = RcvData();
	if(Result == FALSE)
		goto out;
	
	while(1)
	{
				
		//调用WriteFile函数发送数据
		while(sendcn < readcn)
		{
			sendbuf[0] = 0x00;
			cmdData = 0x00000000;//continue
			WordsCpy(sendbuf+0, &cmdData, 4);
			WordsCpy(sendbuf+4, &g_packno, 4);
			g_packno++;
			WordsCpy(sendbuf+8, FileBuffer+sendcn, PACKET_SIZE-8);
			//printf("send image\n");
			Result = SendData();
			if(Result == FALSE)
				goto out;
			SysTimerDelay(100000);//50ms
			Result = RcvData();
			if(Result == FALSE)
				goto out;
			sendcn += PACKET_SIZE-8;
			if((sendcn < readcn) && (readcn - sendcn < PACKET_SIZE-8))
			{
				WordsCpy(FileBuffer, FileBuffer + sendcn, readcn - sendcn);
				sendcn = readcn - sendcn;
				break;
			}
			
		}
		if(sendcn >= readcn)
			sendcn = 0;
		readcn = 0;
		if(pos + FILE_BUFFER - sendcn > totallen)
			readcn = totallen - pos;
		else
			readcn = FILE_BUFFER - sendcn;
		if(readcn)
			WordsCpy(FileBuffer + sendcn, (char*)&imageBegin+pos, readcn);
		pos += readcn;
				
		if(sendcn == 0)
		{
			if(readcn == 0)
				break;
		}
		else
			readcn += sendcn;
		
		sendcn = 0;
				//AddToInfOut("01发送成功 "+itos(written,16)+" 字节");
			
	}
	printf("get checksum\n");
	WordsCpy(&get_cksum, rcvbuf+8, 2);
	lcksum = CalCheckSum((uint8_t*)&imageBegin-1, totallen);
	if(Result == TRUE)
	{
		if(lcksum == get_cksum)
		{	
			config[0] = totallen;
			config[1] = lcksum;
			printf("write ck %x %x\n", totallen, lcksum);
			Result = CmdRunCmd(CMD_WRITE_CHECKSUM, config);	
			if(Result == TRUE)
				printf("update success\n");
			else
				printf("update fail\n");
		}
		else
			printf("check cksum error, %x(should be %x)\n", lcksum, get_cksum);
	}
	else
		printf("Fail\n");
	
	printf("update finished\n");
out:	
	return Result;
}

