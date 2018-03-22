// MyHidLib.h : MyHidLib DLL 的主要標頭檔
//

#pragma once

#ifndef __AFXWIN_H__
	#error "對 PCH 包含此檔案前先包含 'stdafx.h'"
#endif

#include "resource.h"		// 主要符號


// CMyHidLibApp
// 這個類別的實作請參閱 MyHidLib.cpp
//

#define _DEBUG

/////////////////////////////////////////
#define FW_VER_NUM         0x23        //

#define MAX_PACKET 64
#define FILE_BUFFER	2048
#define MAX_BIN_FILE_SIZE (128*1024)

#define CMD_GET_VERSION     0x000000A6
#define CMD_UPDATE_APROM	0x000000A0
#define CMD_SYNC_PACKNO		0x000000A4
#define CMD_UPDATE_CONFIG   0x000000A1
#define CMD_UPDATE_DATAFLASH 0x000000C3
//#define CMD_UPDATE_DATA1	0x000000C2
//#define CMD_UPDATE_DATA2	0x000000C3
#define CMD_READ_CHECKSUM 	0x000000C8
#define CMD_ERASE_ALL 	    0x000000A3

#define CMD_READ_CONFIG     0x000000A2
#define CMD_APROM_SIZE      0x000000AA
#define CMD_GET_DEVICEID    0x000000B1

#define CMD_WRITE_CHECKSUM  0x000000C9
#define CMD_GET_FLASHMODE   0x000000CA
#define CMD_RUN_APROM       0x000000AB
#define CMD_RUN_LDROM       0x000000AC

#define CMD_RESEND_PACKET   0x000000FF
#define CMD_CONNECT         0x000000AE
#define CMD_DISCONNECT      0x000000AF

#define PORT_USB 1
#define PORT_COM 2

#define MODE_APROM 1
#define MODE_LDROM 2

#define SERIAL_NUC1XX 1
#define SERIAL_M05X   2

#define ERR_CODE_LOST_PACKET      -1
#define ERR_CODE_CHECKSUM_ERROR   -2
#define ERR_CODE_TIME_OUT         -3
#define ERR_CODE_COM_ERROR_OPEN   -4

#define ERROR_CODE_DEV_NOT_FOUND  -10
#define ERROR_CODE_INVALID_HANDLE -11
#define ERROR_CODE_SENDING		  -12
#define ERROR_CODE_FILESIZE2BIG	  -13
#define ERROR_CODE_HEX2BIN		  -14


/////////////////////////////////////////

typedef struct{
	    INT  nCmdTotalNum;
        INT  nCmdNum;
		DWORD aCmdList[10];
}MY_CMD_LIST;

typedef struct{
	UINT uIndex;
	UINT uChipID;
	UINT uRamSize;
	TCHAR cChipName[128];
	UINT uFlashSize;
	UINT uCodeFlashSize;
	UINT uDataFlashSize;
	UINT uDataFlashStartAddr;
	//Any more...

}MY_CHIP_TYPE;

typedef struct{
	UINT uCodeFileType; // 0:bin 1:hex
	UINT uCodeFileStartAddr; //hex only
    UINT uCodeFileSize;
	UINT16 uCodeFileCheckSum;
	FILETIME   ftLastCodeFileWriteTime;


	UINT uDataFileType;
	UINT uDataFileStartAddr;
	UINT uDataFileSize;
	UINT16 uDataFileCheckSum;
	FILETIME   ftLastDataFileWriteTime;
}MY_FILE_INFO_TYPE;

class CMyHidLibApp : public CWinApp
{
public:
	CMyHidLibApp();

// 覆寫
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()

public:
	BOOL OpenDeviceUsb();
	BOOL InitDevice();
	BOOL ReadData();
	BOOL WriteData();
	void CloseDeviceUsb();
	BOOL CmdToDo(DWORD cmd);
	INT  getErrorCode();
	INT  setErrorCode(INT code);

	//CString MyDevPathName;
	BOOL UpdateConfig();
	BOOL CmdSyncPackno();
	BOOL EraseAllChip();
	BOOL CmdChipConnection();
	BOOL DetectDevice();
	BOOL CmdWriteAndReadOne(DWORD cmd);
	void RunApRom();
	BOOL CmdGetCheckSum(int start, int len, unsigned short *cksum);
	BOOL CmdSetCheckSum(unsigned short checksum, int len);

	BOOL setHexConfig(DWORD conf0,DWORD conf1);
	BOOL getHexConfig(DWORD* conf0,DWORD* conf1);
	BOOL getChipLocked();
	BOOL isDataInSending();
	BOOL GetFileInfo(LPCTSTR filename,BOOL bCodeFile);
	BOOL HexToBin(LPCTSTR filename,UINT nMaxBufSize,BOOL bCodeFile,MY_FILE_INFO_TYPE *fileInfo);
	UINT getDataFileSize();

private:

	static friend UINT TransferThread(LPVOID pParam);
	static friend void writeLog(char* string);
	static friend void writeLog(CString string);

	CString itos(INT value, INT radix);
	unsigned short Checksum (unsigned char *buf, int len);
	unsigned long CMyHidLibApp::HexStringToDec(TCHAR *buf, UINT len);


};
