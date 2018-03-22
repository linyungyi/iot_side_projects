#ifndef __CMD_H__
#define __CMD_H__


#define BOOL  uint8_t
#define PAGE_SIZE                      0x00000200     /* Page size */

#define PACKET_SIZE	64//32
#define FILE_BUFFER	128

extern uint8_t imageBegin, imageEnd;
extern uint8_t rcvbuf[PACKET_SIZE];
extern uint8_t sendbuf[PACKET_SIZE];
extern unsigned int g_packno;
extern unsigned short gcksum;

BOOL SendData(void);
BOOL RcvData(void);
void SysTimerDelay(uint32_t us);//unit=0.5us
uint16_t Checksum (unsigned char *buf, int len);
void WordsCpy(void *dest, void *src, int32_t size);

BOOL CmdSyncPackno(int flag);
BOOL CmdGetCheckSum(int flag, int start, int len, unsigned short *cksum);
BOOL CmdGetDeviceID(int flag, unsigned int *devid);
BOOL CmdGetConfig(int flag, unsigned int *config);
BOOL CmdPutApromSize(int flag, unsigned int apsize);
BOOL CmdEraseAllChip(int flag);
BOOL CmdUpdateAprom(int flag);

#endif//__CMD_H__
