// MyHidLib.cpp : ﹚竡 DLL ﹍て盽Α
//

#include "stdafx.h"
#include "MyHidLib.h"
#include <stdio.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//HID................................
#include "dbt.h"

extern "C" {
#include "hidsdi.h"
#include "setupapi.h"
}

static UINT g_packno = 1;
static int g_debug = 0;
static DWORD g_ErrorCode = 0;

HANDLE hUsbHandle;
BOOL DataInSending;
BOOL bDetecting;
BOOL MyDevFound;

CWinThread * pReadReportThread;
CWinThread * pWriteReportThread;

OVERLAPPED WriteOverlapped;
OVERLAPPED ReadOverlapped;

UCHAR WriteReportBuffer[256];
UCHAR ReadReportBuffer[256];

DWORD m_curCmd;
unsigned short gcksum;

MY_CMD_LIST m_sMyCmdList;

DWORD m_hexConfig0;
DWORD m_hexConfig1;
DWORD m_hexConfig0_saved;
DWORD m_hexConfig1_saved;

BOOL bIsChipLocked;
BYTE m_IspVersion;
MY_CHIP_TYPE m_sMyChipType;
MY_FILE_INFO_TYPE m_sMyFileInfo;

//128k Bin
UCHAR CodeFileBuffer[MAX_BIN_FILE_SIZE];
UCHAR DataFileBuffer[MAX_BIN_FILE_SIZE];


// 癸 App About ㄏノ CAboutDlg 癸杠よ遏
DEV_BROADCAST_DEVICEINTERFACE DevBroadcastDeviceInterface;
//end of HID................................

//
//TODO: 狦硂 DLL 琌笆篈癸 MFC DLL 硈挡
//		ê或眖硂 DLL 蹲ヴ穦㊣
//		MFC ず场ㄧΑ常ゲ斗ㄧΑ秨繷 AFX_MANAGE_STATE
//		エ栋
//
//		ㄒ:
//
//		extern "C" BOOL PASCAL EXPORT ExportedFunction()
//		{
//			AFX_MANAGE_STATE(AfxGetStaticModuleState());
//			// 矪タ盽ㄧΑ砰
//		}
//
//		硂エ栋﹚璶瞷–
//		ㄧΑい镑㊣ MFC ず场硂種帝
//		ウゲ斗琌ㄧΑず材朝瓃Α
//		ゲ斗ヴン跑计玡
//		ウ篶ㄧΑ穦玻ネ癸 MFC
//		DLL ず场㊣
//
//		叫把綷 MFC м砃矗ボ 33 ㎝ 58 い
//		冈灿戈
//


// CMyHidLibApp

BEGIN_MESSAGE_MAP(CMyHidLibApp, CWinApp)
END_MESSAGE_MAP()


// CMyHidLibApp 篶

CMyHidLibApp::CMyHidLibApp()
{
	// TODO: 篶祘Α絏
	// 盢┮Τ璶﹍砞﹚ InitInstance い
}


// 度Τ CMyHidLibApp ン

CMyHidLibApp theApp;


// CMyHidLibApp ﹍砞﹚

BOOL CMyHidLibApp::InitInstance()
{
	CWinApp::InitInstance();

	return TRUE;
}

BOOL CMyHidLibApp::InitDevice()
{
	//GUID HidGuid;

	//MyDevPathName=_T("");
	MyDevFound=FALSE;
	//MyVid=8888;MyPid=0006;MyPvn=0100;

	hUsbHandle = INVALID_HANDLE_VALUE;

	DataInSending=FALSE;
	m_curCmd = 0;
	//bCodeFlagErrorColorEnable = FALSE;
	//bDataFlagErrorColorEnable = FALSE;
	bIsChipLocked = FALSE;

	memset(&m_sMyFileInfo,0,sizeof(MY_FILE_INFO_TYPE));
	memset(&m_sMyChipType,0,sizeof(MY_CHIP_TYPE));
	lstrcpy(m_sMyChipType.cChipName,_T("Unknown"));

	//bDetecting = FALSE;


	WriteOverlapped.Offset=0;
	WriteOverlapped.OffsetHigh=0;
	WriteOverlapped.hEvent=CreateEvent(NULL,TRUE,FALSE,NULL);
	
	ReadOverlapped.Offset=0;
	ReadOverlapped.OffsetHigh=0;
	ReadOverlapped.hEvent=CreateEvent(NULL,TRUE,FALSE,NULL);

	
	pWriteReportThread=AfxBeginThread(TransferThread,
	this,
	THREAD_PRIORITY_NORMAL,
	0,
	CREATE_SUSPENDED,
	NULL);

	if(pWriteReportThread!=NULL)
	{
		pWriteReportThread->ResumeThread();
	}
	

	/*
	HidD_GetHidGuid(&HidGuid);
	DevBroadcastDeviceInterface.dbcc_size=sizeof(DevBroadcastDeviceInterface);
	DevBroadcastDeviceInterface.dbcc_devicetype=DBT_DEVTYP_DEVICEINTERFACE;
	DevBroadcastDeviceInterface.dbcc_classguid=HidGuid;
	
	RegisterDeviceNotification(m_hWnd,
		&DevBroadcastDeviceInterface,
		DEVICE_NOTIFY_WINDOW_HANDLE);
	*/

	m_sMyChipType.uDataFlashStartAddr = 0x1F000;

	return true;
}

BOOL CMyHidLibApp::OpenDeviceUsb()
{

	GUID HidGuid;
	HDEVINFO hDevInfoSet;
	DWORD MemberIndex;
	SP_DEVICE_INTERFACE_DATA DevInterfaceData;
	BOOL Result;
	DWORD RequiredSize;
	PSP_DEVICE_INTERFACE_DETAIL_DATA	pDevDetailData;
	HANDLE hDevHandle;
	HIDD_ATTRIBUTES DevAttributes;
	CString MyDevPathName;

	//InitDeviceUsb();
	Result = false;

	MyDevFound=FALSE;
	hUsbHandle=INVALID_HANDLE_VALUE;

	DevInterfaceData.cbSize=sizeof(DevInterfaceData);
	DevAttributes.Size=sizeof(DevAttributes);


	HidD_GetHidGuid(&HidGuid);

	
	hDevInfoSet=SetupDiGetClassDevs(&HidGuid,
		NULL,
		NULL,
		DIGCF_DEVICEINTERFACE|DIGCF_PRESENT);
	
	MemberIndex=0;

	while(1)
	{

		Result=SetupDiEnumDeviceInterfaces(hDevInfoSet,
			NULL,
			&HidGuid,
			MemberIndex,
			&DevInterfaceData);
		

		if(Result==FALSE) break;
		
		MemberIndex++;
		
		Result=SetupDiGetDeviceInterfaceDetail(hDevInfoSet,
			&DevInterfaceData,
			NULL,
			NULL,
			&RequiredSize,
			NULL);

		pDevDetailData=(PSP_DEVICE_INTERFACE_DETAIL_DATA)malloc(RequiredSize);
		if(pDevDetailData==NULL) 
		{
			//MessageBox(_T("No enough memory!"));
			SetupDiDestroyDeviceInfoList(hDevInfoSet);
			return FALSE;
		}
		
		pDevDetailData->cbSize=sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
		
		Result=SetupDiGetDeviceInterfaceDetail(hDevInfoSet,
			&DevInterfaceData,
			pDevDetailData,
			RequiredSize,
			NULL,
			NULL);
		

		MyDevPathName=pDevDetailData->DevicePath;
		free(pDevDetailData);
		

		if(Result==FALSE) continue;
		
		hDevHandle=CreateFile(MyDevPathName, 
			NULL,
			FILE_SHARE_READ|FILE_SHARE_WRITE, 
			NULL,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			NULL);
		
		if(hDevHandle!=INVALID_HANDLE_VALUE)
		{
			Result=HidD_GetAttributes(hDevHandle,
				&DevAttributes);
			
			CloseHandle(hDevHandle);
			
			if(Result==FALSE) continue;
						
			if(DevAttributes.VendorID==0x0416 && 
				DevAttributes.ProductID==0xA316){
						//MyDevFound=TRUE; 

						hUsbHandle=CreateFile(MyDevPathName, 
							GENERIC_READ|GENERIC_WRITE,
							FILE_SHARE_READ|FILE_SHARE_WRITE, 
							NULL,
							OPEN_EXISTING,
							FILE_ATTRIBUTE_NORMAL|FILE_FLAG_OVERLAPPED,
							NULL);
						if(hUsbHandle==INVALID_HANDLE_VALUE){		
							//AddToInfOut(_T("Failed to access the USB device!"),1,1);
							continue;
						}
						
						//AddToInfOut(_T("USB found!"),1,1);
						Result = TRUE;
						DataInSending=FALSE; 
						
						break;
			}
		}

		else continue;
	}

	if(!Result)
		return FALSE;

	SetupDiDestroyDeviceInfoList(hDevInfoSet);

	return Result;

}

void CMyHidLibApp::CloseDeviceUsb()
{

    BOOL bResult;

	if(hUsbHandle!=INVALID_HANDLE_VALUE)
	{
		if(MyDevFound)
		  RunApRom();
		bResult = CloseHandle(hUsbHandle);
		hUsbHandle=INVALID_HANDLE_VALUE;
	}
	

	DataInSending=FALSE;
	bDetecting = FALSE;
	//bDetectingSaved = FALSE;
	g_packno = 1;
	g_debug = 0;

	MyDevFound=FALSE;	
	//bIsConfigLoad = FALSE;

	m_sMyCmdList.nCmdNum = 0;
	m_sMyCmdList.nCmdTotalNum = 0;
}

BOOL CMyHidLibApp::setHexConfig(DWORD conf0,DWORD conf1)
{
	m_hexConfig0 = conf0;
	m_hexConfig1 = conf1;
	return true;
}

BOOL CMyHidLibApp::getHexConfig(DWORD* conf0,DWORD* conf1)
{
	*conf0 = m_hexConfig0_saved;
	*conf1 = m_hexConfig0_saved;
	return true;
}

BOOL CMyHidLibApp::isDataInSending()
{
	return DataInSending;
}


BOOL CMyHidLibApp::getChipLocked()
{
	return bIsChipLocked;
}

void CMyHidLibApp::RunApRom()
{
	unsigned long cmdData;

	//CMD_RUN_APROM   
    memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_RUN_APROM;
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	g_packno++;

	WriteData();

}

BOOL CMyHidLibApp::CmdSyncPackno()
{
	BOOL Result;
	unsigned long cmdData;
	
	//sync send&recv packno
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_SYNC_PACKNO;//CMD_UPDATE_APROM
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	memcpy(WriteReportBuffer+9, &g_packno, 4);
	g_packno++;
	
	Result = WriteData();
	if(Result == FALSE)
		return Result;

	Result = ReadData();
	
	return Result;
}

BOOL CMyHidLibApp::UpdateConfig()
{
	BOOL Result;
	unsigned long cmdData;
	DWORD rcvConfig0,rcvConfig1;

	Result = CmdSyncPackno();
	if(Result == FALSE)
	{
		/*
		AddToInfOut(_T("Send sync packno cmd fail"),1,1);
		m_writeProgress.SetPos(0);
		*/
		return FALSE;
	}

	//m_writeProgress.SetPos(10);

	/** send updata config command**/
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_UPDATE_CONFIG;//CMD_UPDATE_CONFIG
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	g_packno++;


	//config 0
	memcpy(WriteReportBuffer+9, &m_hexConfig0, 4);
	//config 1
	memcpy(WriteReportBuffer+13, &m_hexConfig1, 4);


	//send CMD_UPDATE_CONFIG
	Result = WriteData();
	if(Result == FALSE)
		return FALSE;
	//m_writeProgress.SetPos(15);

	Result = ReadData();
	if(Result == FALSE)
		return FALSE;

	memcpy(&rcvConfig0,ReadReportBuffer+9,4);
	memcpy(&rcvConfig1,ReadReportBuffer+13,4);

	if( (rcvConfig0!=m_hexConfig0) || (rcvConfig1!=m_hexConfig1) )
	{
		//AddToInfOut(_T("Recieved Config0:0x")+itos(rcvConfig0,16)+_T(" Config1:0x")+itos(rcvConfig1,16),FALSE,TRUE);

		return FALSE;
	}

	return TRUE;

}

BOOL CMyHidLibApp::EraseAllChip()
{
	BOOL Result;
	unsigned long cmdData;

	Result = CmdSyncPackno();
	if(Result == FALSE)
	{
		return FALSE;
	}	

	/** send updata config command**/
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_ERASE_ALL;
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	g_packno++;

	Result = WriteData();
	if(Result == FALSE)
		return FALSE;

	Result = ReadData();
	if(Result == FALSE)
		return FALSE;

	return TRUE;
}

BOOL CMyHidLibApp::CmdWriteAndReadOne(DWORD cmd)
{
	m_curCmd = cmd;
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	memcpy(WriteReportBuffer+1, &cmd, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	g_packno++;

	if(WriteData() == FALSE)
		return FALSE;
			
	if(ReadData() == FALSE)
		return FALSE;

	return TRUE;

}

BOOL CMyHidLibApp::CmdChipConnection()
{

	BOOL Result;
	unsigned long cmdData;
	CString tmpStr;
	DWORD ret;
	BOOL bDetectResult;

    SleepEx(100,TRUE); //Waitting ISP Firmware Working

//first:
//CMD_SYNC_PACKNO
    //AddToInfOut(_T("Getting flash mode..."),1,1);
	Result = CmdSyncPackno();
	if(Result == FALSE)
	{
		Result = CmdSyncPackno();
		if(Result == FALSE)
			goto fail;
	}
       
//CMD_GET_VERSION	
	//AddToInfOut(_T("Getting ISP version..."),1,1);
	Result = CmdSyncPackno();
	if(Result == FALSE)
       goto fail;

	Result = CmdWriteAndReadOne(CMD_GET_VERSION);
	if(Result == FALSE)
		goto fail;

	memcpy(&m_IspVersion,ReadReportBuffer+9,1); //ISP version

	if(m_IspVersion == 0) //ISP FW version
		goto fail;

	/*if(m_IspVersion != FW_VER_NUM)
	{
		tmpStr.Format(_T("Firmware not match!\nV%x.%x is required, but current FW version is V%x.%x\n\nContinue?"),FW_VER_NUM>>4,FW_VER_NUM&0xF,m_IspVersion>>4,m_IspVersion&0xF);
        if(IDCANCEL == MessageBox(tmpStr,_T("Warning"),MB_OKCANCEL) )
			goto fail;
	}*/
	
	//tmpStr.Format(_T("F/W Ver:%x.%x"),m_IspVersion>>4,m_IspVersion&0xF);
	//pDlg->AddToInfOut(tmpStr,1,1);	
	//SetDlgItemText(IDC_TXT_FW_NO,tmpStr);
		


//CMD_GET_DEVICEID
	//AddToInfOut(_T("Getting device ID..."),1,1);
	Result = CmdWriteAndReadOne(CMD_GET_DEVICEID);
	if(Result == FALSE)
		goto fail;

	memcpy(&m_sMyChipType.uChipID,ReadReportBuffer+9,4); //Device ID
	
	//tmpStr.Format(_T("Device ID:%08X"),m_sMyChipType.uChipID);
	//pDlg->AddToInfOut(tmpStr,1,1);

	if(m_sMyChipType.uChipID == 0)
		goto fail;
	//tmpStr.Format(_T("0x%08X"),m_sMyChipType.uChipID);
	//SetDlgItemText(IDC_EDIT_PART_NO,tmpStr);

//CMD_READ_CONFIG
	//AddToInfOut(_T("Getting Config..."),1,1);
	Result = CmdWriteAndReadOne(CMD_READ_CONFIG);
	if(Result == FALSE)
		goto fail;

	memcpy(&m_hexConfig0_saved,ReadReportBuffer+9,4); //Config0
	memcpy(&m_hexConfig1_saved,ReadReportBuffer+13,4); //Config0
	
	m_hexConfig0 = m_hexConfig0_saved;
	m_hexConfig1 = m_hexConfig1_saved;


//Check if locked
	if(m_hexConfig0 & (1<<1))
	    bIsChipLocked = FALSE;
	else
		bIsChipLocked = TRUE;

	//bIsChipLocked = TRUE; //test

	/*if(bIsChipLocked)
	{
		pDlg->AddToInfOut(_T("Chip is locked"),1,1);
		//SetDlgItemText(IDC_TXT_CONFIG0,_T(""));
		//SetDlgItemText(IDC_TXT_CONFIG1,_T(""));
	}*/

	//tmpStr.Format(_T("Config0:%08X \r\nConfig1:%08X"),m_hexConfig0_saved,m_hexConfig1_saved);
	//pDlg->AddToInfOut(tmpStr,0,1);

	/*
	tmpStr.Format(_T("%08X"),m_hexConfig0);
	SetDlgItemText(IDC_TXT_CONFIG0,tmpStr);
	tmpStr.Format(_T("%08X"),m_hexConfig1);
	SetDlgItemText(IDC_TXT_CONFIG1,tmpStr);
	*/

	//Result = UpdateChipInfo();
	//if(Result == FALSE)
	//	goto fail;


#if 0

	if((Result)&&(m_IspVersion<0x23))
	{
       //AddToInfOut(_T("Sending APROM size..."),0,0);	
	   m_curCmd = CMD_APROM_SIZE;
	   memset(WriteReportBuffer, 0, MAX_PACKET+1);	  
	   cmdData = CMD_APROM_SIZE;
	   memcpy(WriteReportBuffer+1, &cmdData, 4);
	   memcpy(WriteReportBuffer+5, &g_packno, 4);
	   memcpy(WriteReportBuffer+9, &m_sMyChipType.uFlashSize, 4);
	   g_packno++;
	
	   Result = WriteData(bUSB);
	   if(Result == FALSE){
		   AddToInfOut(_T("False"),1,1);
		   goto fail;
	   }

	   Result = ReadData(bUSB);
	   if(Result)
	       AddToInfOut(_T("OK"),1,1);
	   else{
		   AddToInfOut(_T("False"),1,1);
		   goto fail;
	   }


	}
#endif


	//if(!wcsncmp(m_sMyChipType.cChipName,_T("M05"),3))
	//	m_ChipSerial = SERIAL_M05X;

	MyDevFound=TRUE;

/*
	ret = GetFileAttributes(m_strDataFilePath);
	if(!((ret == INVALID_FILE_ATTRIBUTES) || (ret & FILE_ATTRIBUTE_DIRECTORY)))
	{
		m_ctlEditDataFile.SetWindowText(m_strDataFilePath);;
		m_sMyFileInfo.uDataFileCheckSum = 0;
		m_sMyFileInfo.uDataFileSize = 0;
        Result = GetFileInfo(m_strDataFilePath,FALSE,&m_sMyFileInfo);

		CheckFileSize(2);

		if(Result)
		  ShowBinFile(DataFileBuffer,FALSE,m_sMyFileInfo.uDataFileSize);
		else
		  m_text_dataflash.SetWindowText(_T(""));
	}


	ret = GetFileAttributes(m_strCodeFilePath);
	if(!((ret == INVALID_FILE_ATTRIBUTES) || (ret & FILE_ATTRIBUTE_DIRECTORY)))
	{
		m_ctlEditCodeFile.SetWindowText(m_strCodeFilePath);;
		m_sMyFileInfo.uCodeFileCheckSum = 0;
		m_sMyFileInfo.uCodeFileSize = 0;
        Result = GetFileInfo(m_strCodeFilePath,TRUE,&m_sMyFileInfo);


		CheckFileSize(1);//比较文件大小和芯片flash大小，并在UI上显示
		if(Result)
			ShowBinFile(CodeFileBuffer,TRUE,m_sMyFileInfo.uCodeFileSize);
		else
			m_text_aprom.SetWindowText(_T(""));
	}

	ChangeBurenMode(m_uBurnMode_last);
*/

	return TRUE;


fail:
	//if((g_ErrorCode != ERR_CODE_TIME_OUT)&&(bDetectResult))
	//  MessageBox(_T("Connect failed"),_T("Error"),MB_OK);

	MyDevFound=FALSE;
    CloseDeviceUsb();

    return FALSE;
}

BOOL CMyHidLibApp::CmdGetCheckSum(int start, int len, unsigned short *cksum)
{
	BOOL Result;
	unsigned long cmdData;
	unsigned short lcksum;
	
	//sync send&recv packno
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_READ_CHECKSUM;
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	memcpy(WriteReportBuffer+9, &start, 4);
	memcpy(WriteReportBuffer+13, &len, 4);
	g_packno++;
	
	Result = WriteData();
	if(Result == FALSE)
		return Result;

	Result = ReadData();
	if(Result)
	{
		memcpy(&lcksum, ReadReportBuffer+9, 2);

		*cksum = lcksum;
	}
	
	return Result;
}

BOOL CMyHidLibApp::CmdSetCheckSum(unsigned short checksum, int len)
{
	BOOL Result;
	unsigned long cmdData;
	
	//sync send&recv packno
	memset(WriteReportBuffer, 0, MAX_PACKET+1);
	cmdData = CMD_WRITE_CHECKSUM;
	memcpy(WriteReportBuffer+1, &cmdData, 4);
	memcpy(WriteReportBuffer+5, &g_packno, 4);
	memcpy(WriteReportBuffer+9, &len, 4);
	memcpy(WriteReportBuffer+13, &checksum, 4);
	g_packno++;
	
	Result = WriteData();
	if(Result == FALSE)
		return Result;

	Result = ReadData();	
	return Result;
}

UINT TransferThread(LPVOID pParam)
{

#ifdef _DEBUG
	writeLog("TransferThread start.\n");
#endif

	CMyHidLibApp *pApp = (CMyHidLibApp *)pParam;

	BOOL Result, bUSB;
	UINT nCurProgress = 0;
	CString Str;
	unsigned long readcn, sendcn, sendPacketSize, cmdData;
	unsigned short get_cksum;

	UCHAR *tranBuf;
	UINT   tranBufStartAddr;
	UINT   tranBufSize;
	UINT16 tranBufCheckSum;

	while(1)
	{
		if(DataInSending==TRUE)
		{

			/*
			if(m_sMyCmdList.nCmdTotalNum)
			{
			  Str.Format(_T("%d/%d"),pMainDlg->m_sMyCmdList.nCmdTotalNum-pMainDlg->m_sMyCmdList.nCmdNum+1,pMainDlg->m_sMyCmdList.nCmdTotalNum);
			  pMainDlg->SetDlgItemText(IDC_TXT_CMD_NUM,Str);
			}
			*/

			if( m_curCmd == CMD_UPDATE_CONFIG )
			{
				Result = pApp->UpdateConfig();
				DataInSending=FALSE;

				if(Result == FALSE){
					m_sMyCmdList.nCmdNum = 1; 
				}
				else{
					
					if(!(m_hexConfig0 & (1<<1))){ //Chip locked
						bIsChipLocked = TRUE;
					}

					m_hexConfig0_saved = m_hexConfig0;
					m_hexConfig1_saved = m_hexConfig1;

				}

				//goto next;
				goto out;
				
			}
			else if(m_curCmd == CMD_ERASE_ALL)
			{
				Result = pApp->EraseAllChip();
				if(Result == TRUE){
					bIsChipLocked = FALSE;
					//Erase后恢复默认值
					//pMainDlg->m_hexConfig0 = CONFIG0_DEFAULT_VALUE;
					//pMainDlg->m_hexConfig1 = CONFIG1_DEFAULT_VALUE;

			        memcpy(&m_hexConfig0, ReadReportBuffer+9, 4);
					memcpy(&m_hexConfig1, ReadReportBuffer+13, 4);
					
					m_hexConfig0_saved = m_hexConfig0;
					m_hexConfig1_saved = m_hexConfig1;
					//UpdateSizeInfo(FALSE);
				}
				goto out;
			}
			else if(m_curCmd == CMD_GET_VERSION)
			{
				Result = pApp->CmdChipConnection();
				DataInSending=FALSE;
				/*if(Result == FALSE){
					//pMainDlg->AddToInfOut(_T("Connect fail!"),1,1);
				}
				else{
					pMainDlg->AddToInfOut(_T("Connect success!"),1,1);
				}*/

				//pDlg->SendMessage(WM_SETCURSOR, 0, 0);
				continue;

			}



			else if(m_curCmd == CMD_UPDATE_APROM)
			{
				tranBuf = CodeFileBuffer;
				tranBufStartAddr = m_sMyFileInfo.uCodeFileStartAddr;
				tranBufSize = m_sMyFileInfo.uCodeFileSize;
				tranBufCheckSum = m_sMyFileInfo.uCodeFileCheckSum;
			}
			else if(m_curCmd == CMD_UPDATE_DATAFLASH)
			{
				tranBuf = DataFileBuffer;
				tranBufStartAddr = m_sMyFileInfo.uDataFileStartAddr;
				tranBufSize = m_sMyFileInfo.uDataFileSize;
				tranBufCheckSum = m_sMyFileInfo.uDataFileCheckSum;
			}
			else{
				//pMainDlg->AddToInfOut(_T("Unknown CMD!"),1,1);
				Result = FALSE;
				goto out;

			}

			
			Result = pApp->CmdSyncPackno();
			if(Result == FALSE)
			{
				//pDlg->AddToInfOut(_T("Send sync packno cmd fail"),1,1);
				//pMainDlg->m_writeProgress.SetPos(0);
				goto out;
			}
			//pMainDlg->m_writeProgress.SetPos(5);
			
			/** send updata aprom command**/
			memset(WriteReportBuffer, 0, MAX_PACKET+1);
			cmdData = m_curCmd;//CMD_UPDATE_APROM
			memcpy(WriteReportBuffer+1, &cmdData, 4);
			memcpy(WriteReportBuffer+5, &g_packno, 4);
			g_packno++;


			//if(!pMainDlg->bIsChipLocked)
			//    pDlg->AddToInfOut(_T("Start address:")+pMainDlg->itos(tranBufStartAddr,16) + _T("Size:")+pMainDlg->itos(tranBufSize,16),1,1);


			memcpy(WriteReportBuffer+9, &tranBufStartAddr, 4);
			memcpy(WriteReportBuffer+13, &tranBufSize, 4);

			readcn = tranBufSize;
			sendcn = MAX_PACKET - 16;
			if(sendcn > readcn)
				sendcn = readcn;
			memcpy(WriteReportBuffer+17, tranBuf, sendcn);

			
			//send CMD
			Result = pApp->WriteData();
			if(Result == FALSE)
				goto out;

			Result = pApp->ReadData();
			if(Result == FALSE)
				goto out;

			//nCurProgress = (sendcn*100)/tranBufSize;
			//if(nCurProgress>100)
			//	nCurProgress = 100;


			//if(nCurProgress < 20) //第一个CMD用时较长,占用5% - 20%
			//	nCurProgress = 20;

			//pMainDlg->m_writeProgress.SetPos(nCurProgress);
	

			while(sendcn < readcn) //传送剩余数据
			{
					WriteReportBuffer[0] = 0x00;
					cmdData = 0x00000000;//continue


					memcpy(WriteReportBuffer+1, &cmdData, 4);
					memcpy(WriteReportBuffer+5, &g_packno, 4);
					g_packno++;

					if((readcn - sendcn) < (MAX_PACKET-8)) //剩余不足MAX_PACKET-8全部发出去
					{
						memcpy(WriteReportBuffer+9, tranBuf+sendcn, readcn - sendcn);
						sendPacketSize = readcn - sendcn;
						sendcn = readcn;
					}
					else
					{
					    memcpy(WriteReportBuffer+9, tranBuf+sendcn, MAX_PACKET-8);
						sendPacketSize = MAX_PACKET-8;
						sendcn += MAX_PACKET-8;
					}
					Result = pApp->WriteData();
					if(Result == FALSE)
						goto out;
					Result = pApp->ReadData();
					if(Result == FALSE){
						if( (g_ErrorCode == ERR_CODE_LOST_PACKET)||(g_ErrorCode == ERR_CODE_CHECKSUM_ERROR) )
						{
							//pDlg->AddToInfOut(_T("Resend this packet"),1,1);
							cmdData = CMD_RESEND_PACKET; //丢包重发
							memcpy(WriteReportBuffer+1, &cmdData, 4);
					        memcpy(WriteReportBuffer+5, &g_packno, 4);
					        g_packno++;
							Result = pApp->WriteData();
					        if(Result == FALSE)
						       goto out;
					        Result = pApp->ReadData();
							if(Result == FALSE)
						       goto out;

							sendcn -= sendPacketSize;
							continue;


						}
						else
						  goto out;
					}
					

					//20% - 100%之间取值
					/*
					nCurProgress = (sendcn*100)/tranBufSize;
					nCurProgress = (nCurProgress*80)/100;
					nCurProgress += 20;
					
					if(nCurProgress > 100) //防止万一
				        nCurProgress = 100;
					if(pMainDlg->m_writeProgress.GetPos() != nCurProgress)
					    pMainDlg->m_writeProgress.SetPos(nCurProgress);
					*/
					//SleepEx(1,TRUE);
			}


//Check sum again
         if(m_IspVersion<0x23)			
			Result = pApp->CmdGetCheckSum(tranBufStartAddr, tranBufSize, &get_cksum); //FW 2.3以后取消此命令
		 else
			memcpy(&get_cksum, ReadReportBuffer+9, 2);

			if(Result == TRUE)
			{
				if(tranBufCheckSum == get_cksum)
				{
					Result = TRUE;
					//Str.Format(_T("Compare checksum value again:0x%X,it's right!"),get_cksum);
					//pDlg->AddToInfOut(Str,1,1);

					//if((pDlg->ISP_RESERVED_SIZE)&&(pMainDlg->m_curCmd == CMD_UPDATE_APROM)){ //Save checksum function enabled
					//  pDlg->AddToInfOut(_T("Set checksum..."),1,1);
                    //  Result = pMainDlg->CmdSetCheckSum(bUSB, tranBufCheckSum, tranBufSize);//Write checksum value
					//}

				}
				else{
					Result = FALSE;
					//Str.Format(_T("Get a wrong checksum value:%X"),get_cksum);
					//pDlg->AddToInfOut(Str,1,1);
				}
			}
			//else
			//	pDlg->AddToInfOut(_T("Get checksum error"),1,1);


out:	


			if(Result == TRUE){
//Run APROM
				if(m_sMyCmdList.nCmdNum == 1){ //last one
				    //::MessageBeep(MB_OK);
					if(m_curCmd != CMD_ERASE_ALL)
					  pApp->CloseDeviceUsb();
						//pMainDlg->OnBnClickedButtonClose();
				}

				//pMainDlg->AddToInfOut(_T("Send success"),1,1);
				//pMainDlg->uTxtFontColor = 2; //绿色字体
	            //pMainDlg->SetDlgItemText(IDC_TXT_RESULT,_T("PASS"));
				//pMainDlg->m_writeProgress.SetPos(100);
				

			}
			else{
				//::MessageBeep(MB_ICONERROR);
				//Str.Format(_T("Send fail:progress=%d%%"),nCurProgress);
				//pDlg->AddToInfOut(Str,1,1);
				//pMainDlg->uTxtFontColor = 1; //红色字体
	            //pMainDlg->SetDlgItemText(IDC_TXT_RESULT,_T("Fail"));
				m_sMyCmdList.nCmdNum = 1; //忽略剩余的CMD
			}			
			
//next:
//CMD
			DataInSending=FALSE;
	
			m_sMyCmdList.nCmdNum--;
			if(m_sMyCmdList.nCmdNum > 0){
				//pDlg->AddToInfOut(_T("\r\n \r\nNext CMD..."),1,1);
				Result = pApp->CmdToDo(m_sMyCmdList.aCmdList[m_sMyCmdList.nCmdNum-1]);
				if(Result == FALSE)
					DataInSending=FALSE;

			}
			//else{
				//
				//pDlg->SendMessage(WM_SETCURSOR, 0, 0);
				//(CButton *)(pDlg->GetDlgItem(IDC_CHECK_SAVE_CHECKSUM))->EnableWindow(TRUE);
			//}
			

		}
		else
			SleepEx(10,TRUE);
	}
	//pDlg->AddToInfOut(_T("Error:Transfer thread exit!"),1,1);
	return 0;


}

BOOL CMyHidLibApp::CmdToDo(DWORD cmd)
{

	CString Str;

	if( (MyDevFound==FALSE)&&(cmd!=CMD_GET_VERSION) )
	{
		//AddToInfOut(_T("Device not found"),1,1);
		g_ErrorCode = ERROR_CODE_DEV_NOT_FOUND;
		return FALSE;
	}
	
	if(hUsbHandle==INVALID_HANDLE_VALUE)
	{
		//AddToInfOut(_T("Invalid device!"),1,1);
		g_ErrorCode = ERROR_CODE_INVALID_HANDLE;
		return FALSE;
	}

	
	//如果数据仍在发送中，则返回失败
	if(DataInSending==TRUE)
	{
		//AddToInfOut(_T("Data in sending,try later!"),1,1);
		g_ErrorCode = ERROR_CODE_SENDING;
		return FALSE;
	}

	g_packno = 1;
	g_debug = 0;
	
	m_curCmd = cmd;
	DataInSending=TRUE;

	return TRUE;

}

INT CMyHidLibApp::getErrorCode()
{
	return g_ErrorCode;
}

INT CMyHidLibApp::setErrorCode(INT code)
{
	g_ErrorCode = code;
	return g_ErrorCode;
}

UINT CMyHidLibApp::getDataFileSize()
{
	return m_sMyFileInfo.uDataFileSize;
}

void writeLog(char* string)
{
	FILE *f=fopen("debug.txt","a");
	if(f) {
	  fprintf(f, "%s",string);
	  fclose(f);
	}
}

void writeLog(CString string)
{
	FILE *f=fopen("debug.txt","a");
	if(f) {
	  fprintf(f, "%s",string);
	  fclose(f);
	}
}

CString CMyHidLibApp::itos(INT value, INT radix)
{
	static CString Str;

	Str.Empty();
	if(radix==16)
	{
	    Str.Format(_T("0x%08x"),value);
	}
	else
	{
		Str.Format(_T("%d"),value);
	}
	Str.MakeUpper();
	return Str;
}

unsigned short CMyHidLibApp::Checksum (unsigned char *buf, int len)
{
    int i;
    unsigned short c;

    for (c=0, i=0; i < len; i++) {
        c += buf[i];
    }
    return (c);
}

BOOL CMyHidLibApp::ReadData()
{
	BOOL Result;
	unsigned long nRead;
	UINT LastError, len;
	CString Str;
	unsigned short lcksum;
	HANDLE hRead;
	UCHAR *pBuf;
//	DWORD waitRet;
	static int debugcn;
	UINT uWaitProgress;
	DWORD tick1,tick2,timeout;
	UINT curPacketNo=0;
	int i;
	COMSTAT ComStat;
	DWORD dwErrorFlags;

	g_ErrorCode = 0;

	len = MAX_PACKET+1;
	hRead = hUsbHandle;
	
	memset(ReadReportBuffer,0,sizeof(ReadReportBuffer));
	ResetEvent(ReadOverlapped.hEvent);
	ReadOverlapped.Offset = 0;
	ReadOverlapped.OffsetHigh = 0;


//Time out
	if( (m_curCmd == CMD_UPDATE_DATAFLASH) || (m_curCmd == CMD_UPDATE_APROM) || (m_curCmd == CMD_ERASE_ALL)) 
		timeout = 20000; //ms
	else
		timeout = 5000;

	//if(bDetectingSaved){
	//	timeout = 20;//clyu
	//}

//Progress

	tick1 = ::GetTickCount();
	
	while(1)
	{
		pBuf = ReadReportBuffer;

		Result = ReadFile(hRead,
					pBuf,
					len,
					&nRead,
					&ReadOverlapped);	


		if(Result == FALSE)
		{
			LastError=GetLastError();
			if(LastError==ERROR_IO_PENDING)
			{				
				ResetEvent(ReadOverlapped.hEvent);

				do{
				  Result = GetOverlappedResult(hRead, &ReadOverlapped, &nRead, FALSE);
				  if(Result)
				     break;

				  //Progress
		          /*if( (( m_curCmd == CMD_UPDATE_CONFIG)||( m_curCmd == CMD_ERASE_ALL)) && (g_packno == 4) ){//20% - 100%
			          m_writeProgress.SetPos(uWaitProgress/8);
			          uWaitProgress ++;					
			          if(uWaitProgress >= 800)
				          uWaitProgress = 800;

					  
				      SleepEx(10,TRUE);

		          }
		          else if( (( m_curCmd == CMD_UPDATE_APROM) || (m_curCmd == CMD_UPDATE_DATAFLASH)) && (g_packno==4) ){
					  if(m_writeProgress.GetPos() != uWaitProgress/30)
			              m_writeProgress.SetPos(uWaitProgress/100); //5% - 20%
			          uWaitProgress ++;					
			          if(uWaitProgress >= 2000)
			              uWaitProgress = 2000;

					  SleepEx(10,TRUE);

		          }*/


				  //Time out
				 tick2 = ::GetTickCount();			  
		         if( (tick2 - tick1)>timeout )
		         {

						
					 //if(!bDetectingSaved){
			         //  AddToInfOut(_T("Time out[Read PENDING]"),1,1);
			         //  MessageBox(_T("Time out!"),_T("Error"),MB_OK);
					 //}
					
					 g_ErrorCode = ERR_CODE_TIME_OUT;
			         return FALSE;

		         }

				}while(Result == FALSE);


			}
			else if(LastError != 0)
			{
				//pDlg->AddToInfOut(pDlg->itos(debugcn,10)+_T("Failed to read data,error code:")+pDlg->itos(LastError,10),1,1);
				return FALSE;
			}
		}	

		ResetEvent(ReadOverlapped.hEvent);
		Result = GetOverlappedResult(hRead, &ReadOverlapped, &nRead, FALSE);

		if(Result == TRUE)
		{
			if(nRead < len){
				//pDlg->AddToInfOut(_T("Wrong size:")+pDlg->itos(nRead,10) + _T(" ") + pDlg->itos(len,10),1,1);//ReadOverlapped.InternalHigh
				g_ErrorCode = ERR_CODE_LOST_PACKET;
				g_packno++;
			    return FALSE;

			}
		}
		else
		{
			LastError=GetLastError();
			//pDlg->AddToInfOut(_T("GetOverlappedResult error")+pDlg->itos(LastError,10),1,1);
			return FALSE;
		}

		
		//len = MAX_PACKET+1;//test
		memcpy(&lcksum, pBuf+1, 2);
		pBuf += 5;
		
		memcpy(&curPacketNo, pBuf, 4);		
		if(curPacketNo != g_packno)
		{

		//debug
		//AddToInfOut(_T("curPacketNo:")+itos(curPacketNo,10) + _T(" requestNo:") +itos(rcvpackno,10),0,1);


//Progress
		  /*if( (( m_curCmd == CMD_UPDATE_CONFIG)||( m_curCmd == CMD_ERASE_ALL)) && (g_packno == 4) ){//20% - 100%
			  m_writeProgress.SetPos(uWaitProgress/8);
			  uWaitProgress ++;					
			  if(uWaitProgress >= 800)
				  uWaitProgress = 800;

		  }
		  else if( (( m_curCmd == CMD_UPDATE_APROM) || (m_curCmd == CMD_UPDATE_DATAFLASH)) && (g_packno==4) ){
			  if(m_writeProgress.GetPos() != uWaitProgress/30)
			      m_writeProgress.SetPos(uWaitProgress/100); //5% - 20%
			  uWaitProgress ++;					
			  if(uWaitProgress >= 2000)
			      uWaitProgress = 2000;

		  }*/
			
		  tick2 = ::GetTickCount();			  
		  if( (tick2 - tick1)>timeout )
		  {
			  /*if(!bDetectingSaved){
			    AddToInfOut(_T("Time out[Read]"),1,1);
			    MessageBox(_T("Time out!"),_T("Error"),MB_OK);
			  }*/
			  g_ErrorCode = ERR_CODE_TIME_OUT;
			  return FALSE;

		  }
		  else
			   continue;
		}
		else
		{
			if(lcksum != gcksum)
			{

#ifdef _DEBUG
				char szBuf[8192];
				Str=_T("Checksum error:");
				for(i=len-1;i>=0;i--)
				{
					Str+=itos(ReadReportBuffer[i],16).Right(2)+_T(" ");
				}
				Str+=_T("gcksum=")+itos(gcksum,16).Right(4)+_T(" ");
				Str+=_T("\n");
				writeLog(Str);
#endif

				g_ErrorCode = ERR_CODE_CHECKSUM_ERROR;
				g_packno++;
				return FALSE;
			}

#ifdef _DEBUG
			if( ( m_curCmd == CMD_UPDATE_APROM) && (g_packno==4) )
				writeLog("Erase success,sending next packet...\n");
				//AddToInfOut(_T("Erase success,sending next packet..."),1,1);
#endif


			g_packno++;
			break;
		}
	}
	debugcn++;

	return TRUE;
}

BOOL CMyHidLibApp::WriteData()
{
	BOOL Result;
	unsigned long written;
	UINT LastError, len;
	CString Str;
	HANDLE hWrite;
	UCHAR	*pBuf;
	DWORD waitRet;
	COMSTAT ComStat;
	DWORD dwErrorFlags;

	ResetEvent(WriteOverlapped.hEvent);
	WriteOverlapped.Offset = 0;
	WriteOverlapped.OffsetHigh = 0;
	g_ErrorCode = 0;
	
	
	pBuf = WriteReportBuffer;
	len = MAX_PACKET+1;
	hWrite = hUsbHandle;
	
	gcksum = Checksum(WriteReportBuffer+1, MAX_PACKET);
	
	Result=WriteFile(hWrite,
					pBuf,
					len,
					&written,
					&WriteOverlapped);

	if(Result == 0)
	{
		LastError=GetLastError();
		if(LastError==ERROR_IO_PENDING)
		{
			waitRet = WaitForSingleObject(WriteOverlapped.hEvent, 5000 ); //5sec
			if(waitRet == WAIT_TIMEOUT)
			{
#ifdef _DEBUG
				writeLog("Timeout[Write PENDING]\n");
#endif
				return FALSE;
			}

			ResetEvent(WriteOverlapped.hEvent);	
			//clear the error
			SetLastError(0);
		}
		else if(LastError != 0)
		{
			//pDlg->AddToInfOut(pDlg->itos(g_debug,10)+_T("Failed to write data,error code:")+pDlg->itos(LastError,10),1,1);
			
			//ExceptionHandle(wndHandle, LastError);
			return FALSE;
		}
	}
	//FlushFileBuffers(hWrite);
	
	//pDlg->AddToInfOut(pDlg->itos(g_debug,10) + _T(" write success"),1,1);
	g_debug++;
	
	return TRUE;
		
}

BOOL CMyHidLibApp::GetFileInfo(LPCTSTR filename,BOOL bCodeFile)
{
	HANDLE hFileHandle = INVALID_HANDLE_VALUE;
	BOOL Result=FALSE;
	unsigned long totallen=0,readcn=0;
	unsigned short lcksum; //16 bit checksum
	CString postfix = filename;//根据后缀名区别hex和bin
	UCHAR * buf;

	CString tmpStr;
	WIN32_FIND_DATA   ffd   ;   
    HANDLE   hFind   =   FindFirstFile(filename,&ffd);
	FindClose(hFind);
	if(bCodeFile)
		m_sMyFileInfo.ftLastCodeFileWriteTime = ffd.ftLastWriteTime;
	else
		m_sMyFileInfo.ftLastDataFileWriteTime = ffd.ftLastWriteTime;
	//bActivateExitNow = FALSE;

	postfix = postfix.Right(postfix.GetLength()-postfix.ReverseFind('.')-1);
	if(!postfix.CompareNoCase(_T("hex")))
	{
		Result = HexToBin(filename,MAX_BIN_FILE_SIZE,bCodeFile,&m_sMyFileInfo);
		//tmpStr.Format(_T("Result:%d; size:%d; Start:%X"),Result,fileInfo->uCodeFileSize,fileInfo->uCodeFileStartAddr);
		//AfxMessageBox(tmpStr);
		if( (bCodeFile&&(m_sMyFileInfo.uCodeFileSize > MAX_BIN_FILE_SIZE)) ||
			((!bCodeFile)&&(m_sMyFileInfo.uDataFileSize > MAX_BIN_FILE_SIZE)) )
	    {
		    //MessageBox(_T("File size is too big"),_T("Error"),MB_OK);
			g_ErrorCode = ERROR_CODE_FILESIZE2BIG;
		    return FALSE;
	    }
		if(Result == FALSE)
		{
			g_ErrorCode = ERROR_CODE_HEX2BIN;
			return FALSE;
		}

		if(bCodeFile)
			m_sMyFileInfo.uCodeFileCheckSum = Checksum(CodeFileBuffer, m_sMyFileInfo.uCodeFileSize);
		else 
			m_sMyFileInfo.uDataFileCheckSum = Checksum(DataFileBuffer, m_sMyFileInfo.uDataFileSize);

	}
	else
	{
	  hFileHandle=CreateFile(filename, 
				GENERIC_READ,
				FILE_SHARE_READ, 
				NULL,
				OPEN_EXISTING,
				FILE_ATTRIBUTE_NORMAL,
				NULL);
	  if(hFileHandle==INVALID_HANDLE_VALUE)
	  {
		  g_ErrorCode = GetLastError();//ERROR_CODE_INVALID_HANDLE;
		  return FALSE;
	  }

	  totallen = GetFileSize(hFileHandle, NULL); 

	  if(bCodeFile){
		  m_sMyFileInfo.uCodeFileType = 0;//bin
		  m_sMyFileInfo.uCodeFileStartAddr = 0;
	      m_sMyFileInfo.uCodeFileSize = totallen;
		  buf = CodeFileBuffer;
	  }
	  else{
		  m_sMyFileInfo.uDataFileType = 0;
		  m_sMyFileInfo.uDataFileStartAddr = m_sMyChipType.uDataFlashStartAddr;
		  m_sMyFileInfo.uDataFileSize = totallen;
		  buf = DataFileBuffer;
	  }

	  if(totallen > MAX_BIN_FILE_SIZE)
	  {
		  //MessageBox(_T("File size is too big"),_T("Error"),MB_OK);
		  g_ErrorCode = ERROR_CODE_FILESIZE2BIG;
		  return FALSE;
	  }


	  Result = ReadFile(hFileHandle,
							buf,
							MAX_BIN_FILE_SIZE,
							&readcn,
							NULL);
				
	 if( (Result == TRUE) && (readcn !=totallen) )
	 {
		 g_ErrorCode = GetLastError();//ERROR_CODE_INVALID_HANDLE;
		 CloseHandle(hFileHandle);
		 return FALSE;
	 }
	 else if(Result == FALSE)
	 {
		 g_ErrorCode = GetLastError();//ERROR_CODE_INVALID_HANDLE;
		 CloseHandle(hFileHandle);
		 return FALSE;
	 }
	 lcksum = Checksum(buf, readcn);
	 if(bCodeFile)
		 m_sMyFileInfo.uCodeFileCheckSum = lcksum;
	 else
		 m_sMyFileInfo.uDataFileCheckSum = lcksum;

	}

	CloseHandle(hFileHandle);
       
    return TRUE;
}

BOOL CMyHidLibApp::HexToBin(LPCTSTR filename,UINT nMaxBufSize,BOOL bCodeFile,MY_FILE_INFO_TYPE *fileInfo)
{
	
	int nRecordType, nRecordLen;
	UINT nRecordAddr;
	bool bEndOfField;
	CString strBuffer;
	TCHAR *Buffer;
	TCHAR *pRecordData;
	BYTE cCalCheckSum, cRecordCheckSum;
	UCHAR cFillByte = 0x00;
	UCHAR *TargetBuf;

	UINT curMode = 0;
	UINT highOffset = 0;
	BOOL bInitAddr = TRUE;

	
	UINT startAddr = 0;
	UINT maxAddr = 0;
	UINT lastlen = 0;

	CStdioFile file;
	if( file.Open(filename,CFile::modeRead) == NULL )
		return FALSE;


	bEndOfField = false;
	UINT nReadSize = 0;

	while( bEndOfField == false)
	{
		if( file.ReadString(strBuffer) == NULL )
			break;

		Buffer = strBuffer.GetBuffer();

		if ( Buffer[0] != _T(':') )
		{
			// An field format error.
			goto out;
		}
		// Get record's data length.
		nRecordLen = HexStringToDec( Buffer + 1, 2 );
		// Get record's start address.
		nRecordAddr = HexStringToDec( Buffer + 3, 4 );
		// Get Record's type.
		nRecordType = HexStringToDec( Buffer + 7, 2 );
		// Get the first data's address within record.
		pRecordData = Buffer + 9;
		switch( nRecordType )
		{
		case 00:
			if(curMode == 0x2) 
			{
				nRecordAddr = (highOffset<<4) + nRecordAddr;
			}
			else if(curMode == 0x4)
			{
				nRecordAddr = (highOffset<<16) + nRecordAddr;
			}

			if(bInitAddr)
			{
				startAddr = nRecordAddr;
				maxAddr = nRecordAddr;
				lastlen = nRecordLen;
				bInitAddr = FALSE;

			}


			if( nRecordAddr<startAddr)
				startAddr = nRecordAddr;
			if( nRecordAddr>maxAddr )
			{
				maxAddr = nRecordAddr;
				lastlen = nRecordLen;
			}


			break;
		case 01:
			bEndOfField = true;

			break;
		case 02: 
			curMode = 0x02;
			highOffset = HexStringToDec( pRecordData, 4 );

			break;
		case 04: 
			curMode = 0x04;
			highOffset = HexStringToDec( pRecordData, 4 );

			break;
		default: 
			break;
		}

		strBuffer.ReleaseBuffer();

	}

	  if(bCodeFile){
		  fileInfo->uCodeFileType = 1;//hex
		  fileInfo->uCodeFileStartAddr = startAddr;
	      fileInfo->uCodeFileSize = maxAddr-startAddr+lastlen;
		  TargetBuf = CodeFileBuffer;
	  }
	  else{
		  fileInfo->uDataFileType = 1;
		  fileInfo->uDataFileStartAddr = startAddr;
		  fileInfo->uDataFileSize = maxAddr-startAddr+lastlen;
		  TargetBuf = DataFileBuffer;
	  }

	  if(!bEndOfField)
		  goto out;
	  
//goto out;


	bEndOfField = false;
	curMode = 0;
	highOffset = 0;
	::memset( TargetBuf, cFillByte, nMaxBufSize );
	file.SeekToBegin();
	while( bEndOfField == false)
	{
		if( file.ReadString(strBuffer) == NULL )
			break;

		Buffer = strBuffer.GetBuffer();

		if ( Buffer[0] != _T(':') )
		{
			// An field format error.
			goto out;
		}
		// Get record's data length.
		nRecordLen = HexStringToDec( Buffer + 1, 2 );
		// Get record's start address.
		nRecordAddr = HexStringToDec( Buffer + 3, 4 );
		// Get Record's type.
		nRecordType = HexStringToDec( Buffer + 7, 2 );
		// Get the first data's address within record.
		pRecordData = Buffer + 9;
		switch( nRecordType )
		{
		case 00:
			cCalCheckSum = (BYTE)nRecordLen + 
			((BYTE)(nRecordAddr>>8) + (BYTE)nRecordAddr) +
			(BYTE)nRecordType;

			if(curMode == 0x2) 
			{
				nRecordAddr = (highOffset<<4) + nRecordAddr;
			}
			else if(curMode == 0x4)
			{
				nRecordAddr = (highOffset<<16) + nRecordAddr;
			}

			break;
		case 01:
			bEndOfField = true;
			strBuffer.ReleaseBuffer();

			goto out;
		case 02: 
			curMode = 0x02;
			highOffset = HexStringToDec( pRecordData, 4 );
			strBuffer.ReleaseBuffer();

			continue;
		case 04: 
			curMode = 0x04;
			highOffset = HexStringToDec( pRecordData, 4 );
			strBuffer.ReleaseBuffer();

			continue;
		default: 

			strBuffer.ReleaseBuffer();
			continue;
		}

		

		if ( (nRecordAddr-startAddr) >= nMaxBufSize ) 
			break;
		else if( (nRecordAddr-startAddr+nRecordLen) > nMaxBufSize ) 
			break;

		BYTE cData;
		int nValidDataLen, i;


		nValidDataLen = nRecordLen;

		// Read data from record.
		for( i = 0; i < nValidDataLen; i++ )
		{
			cData = (BYTE)HexStringToDec( pRecordData, 2 );
			TargetBuf[nRecordAddr-startAddr] = cData;
			cCalCheckSum += cData;
			nRecordAddr ++;
			pRecordData += 2;
		}

		cCalCheckSum = -cCalCheckSum; // 2'complement
		// Get Check sum from record.
		cRecordCheckSum = (BYTE)HexStringToDec( pRecordData, 2 );
		if ( cRecordCheckSum != cCalCheckSum )
		{
			//MessageBox(_T("Check sum error!"),_T("Error"),MB_OK);
			break;
		}

		strBuffer.ReleaseBuffer();

	}



out:
	file.Close();

	return bEndOfField;


}

unsigned long CMyHidLibApp::HexStringToDec(TCHAR *buf, UINT len)
{
	TCHAR hexString[16]={0};
	memcpy(hexString,buf,len*sizeof(hexString[0]));

	//return wcstoul(hexString, NULL, 16);
	return strtol(hexString, NULL, 16);

}