Imports System.Runtime.InteropServices
Module I2CBridge
    '
    ' Types
    '
    Public Structure U2C_WORD
        Public bLo As Byte
        Public bHi As Byte
    End Structure

    Public Structure U2C_LONG
        Dim b3 As Byte
        Dim b2 As Byte
        Dim b1 As Byte
        Dim b0 As Byte
    End Structure

    Public Structure U2C_VERSION_INFO
        Public wMajor As U2C_WORD
        Public wMinor As U2C_WORD
    End Structure

    Public Enum U2C_RESULT
        U2C_SUCCESS = 0
        U2C_BAD_PARAMETER
        U2C_HARDWARE_NOT_FOUND
        U2C_SLAVE_DEVICE_NOT_FOUND
        U2C_TRANSACTION_FAILED
        U2C_SLAVE_OPENNING_FOR_WRITE_FAILED
        U2C_SLAVE_OPENNING_FOR_READ_FAILED
        U2C_SENDING_MEMORY_ADDRESS_FAILED
        U2C_SENDING_DATA_FAILED
        U2C_NOT_IMPLEMENTED
        U2C_NO_ACK
        U2C_UNKNOWN_ERROR
    End Enum

    Public Structure U2C_TRANSACTION
        Public nSlaveDeviceAddress As Byte
        Public nMemoryAddressLength As Byte 'can be from 0 up to 4 bytes
        Public nMemoryAddress As U2C_LONG
        Public nBufferLength As U2C_WORD 'can be from 1 up to 256
        <VBFixedArray(256), MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)> Public Buffer() As Byte
        Public Sub Initialize()
            ReDim Buffer(256)
        End Sub
    End Structure

    Public Structure U2C_SLAVE_ADDR_LIST
        Public nDeviceNumber As Byte
        <VBFixedArray(255), MarshalAs(UnmanagedType.ByValArray, SizeConst:=255)> Public list() As Byte
        Public Sub Initialize()
            ReDim list(255)
        End Sub
    End Structure


    'Public Structure SPI_DATA
    '<VBFixedArray(255), MarshalAs(UnmanagedType.ByValArray, SizeConst:=255)> Public buffer() As Byte
    'Public Sub Initialize()
    '    ReDim buffer(255)
    'End Sub
    'End Structure

    Public Enum U2C_LINE_STATE
        LS_RELEASED = 0
        LS_DROPPED_BY_I2C_BRIDGE
        LS_DROPPED_BY_SLAVE
    End Enum
    '
    ' Constants
    '
    Public Const INVALID_HANDLE_VALUE As Integer = -1

    Public Const U2C_I2C_FREQ_FAST As Byte = 0
    Public Const U2C_I2C_FREQ_STD As Byte = 1
    Public Const U2C_I2C_FREQ_83KHZ As Byte = 2
    Public Const U2C_I2C_FREQ_71KHZ As Byte = 3
    Public Const U2C_I2C_FREQ_62KHZ As Byte = 4
    Public Const U2C_I2C_FREQ_50KHZ As Byte = 6
    Public Const U2C_I2C_FREQ_25KHZ As Byte = 16
    Public Const U2C_I2C_FREQ_10KHZ As Byte = 46
    Public Const U2C_I2C_FREQ_5KHZ As Byte = 96
    Public Const U2C_I2C_FREQ_2KHZ As Byte = 242


    Public Const U2C_SPI_FREQ_200KHZ As Byte = 0
    Public Const U2C_SPI_FREQ_100KHZ As Byte = 1
    Public Const U2C_SPI_FREQ_83KHZ As Byte = 2
    Public Const U2C_SPI_FREQ_71KHZ As Byte = 3
    Public Const U2C_SPI_FREQ_62KHZ As Byte = 4
    Public Const U2C_SPI_FREQ_50KHZ As Byte = 6
    Public Const U2C_SPI_FREQ_25KHZ As Byte = 16
    Public Const U2C_SPI_FREQ_10KHZ As Byte = 46
    Public Const U2C_SPI_FREQ_5KHZ As Byte = 8
    Public Const U2C_SPI_FREQ_2KHZ As Byte = 242


    '
    ' DLL functions
    '
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetDeviceCount() As Byte
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetSerialNum(ByVal hDevice As Integer, ByRef pSerialNum As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_IsHandleValid(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_OpenDevice(ByVal nDevice As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_OpenDeviceBySerialNum(ByVal nSerialNum As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_CloseDevice(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetFirmwareVersion(ByVal hDevice As Integer, ByRef FwVersion As U2C_VERSION_INFO) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetDriverVersion(ByVal hDevice As Integer, ByRef FwVersion As U2C_VERSION_INFO) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetDllVersion() As U2C_VERSION_INFO
        '
    End Function
    ' I2C high level and configuration routines
    <DllImport("I2cBrdg.dll")> Public Function U2C_SetI2cFreq(ByVal hDevice As Integer, ByVal Frequency As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetI2cFreq(ByVal hDevice As Integer, ByRef pFrequency As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_Read(ByVal hDevice As Integer, ByRef pTransaction As U2C_TRANSACTION) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SetClockSynch(ByVal hDevice As Integer, ByVal Enable As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetClockSynch(ByVal hDevice As Integer, ByRef pEnable As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SetClockSynchTimeout(ByVal hDevice As Integer, ByVal Timeout As Short) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetClockSynchTimeout(ByVal hDevice As Integer, ByRef pTimeout As Short) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_Write(ByVal hDevice As Integer, ByRef pTransaction As U2C_TRANSACTION) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_ScanDevices(ByVal hDevice As Integer, ByRef pList As U2C_SLAVE_ADDR_LIST) As Integer
        '
    End Function

    ' I2C low level routines
    <DllImport("I2cBrdg.dll")> Public Function U2C_Start(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_RepeatedStart(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_Stop(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_PutByte(ByVal hDevice As Integer, ByVal Data As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetByte(ByVal hDevice As Integer, ByRef pData As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_PutByteWithAck(ByVal hDevice As Integer, ByVal Data As Byte) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetByteWithAck(ByVal hDevice As Integer, ByRef pData As Byte, ByVal bAck As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_PutAck(ByVal hDevice As Integer, ByVal bAck As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetAck(ByVal hDevice As Integer) As Integer
        '
    End Function
    '  I2c wire level routines
    <DllImport("I2cBrdg.dll")> Public Function U2C_ReadScl(ByVal hDevice As Integer, ByRef pState As U2C_LINE_STATE) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_ReadSda(ByVal hDevice As Integer, ByRef pState As U2C_LINE_STATE) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_ReleaseScl(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_ReleaseSda(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_DropScl(ByVal hDevice As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_DropSda(ByVal hDevice As Integer) As Integer
        '
    End Function
    'GPIO routines
    <DllImport("I2cBrdg.dll")> Public Function U2C_SetIoDirection(ByVal hDevice As Integer, ByVal Value As Integer, ByVal Mask As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetIoDirection(ByVal hDevice As Integer, ByRef pValue As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_IoWrite(ByVal hDevice As Integer, ByVal Value As Integer, ByVal Mask As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_IoRead(ByVal hDevice As Integer, ByRef pValue As Integer) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SetSingleIoDirection(ByVal hDevice As Integer, ByVal IoNumber As Integer, ByVal bOutput As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_GetSingleIoDirection(ByVal hDevice As Integer, ByVal IoNumber As Integer, ByRef pbOutput As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SingleIoWrite(ByVal hDevice As Integer, ByVal IoNumber As Integer, ByVal bOutput As Boolean) As Integer
        '
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SingleIoRead(ByVal hDevice As Integer, ByVal IoNumber As Integer, ByRef pbOutput As Boolean) As Integer
        '
    End Function

    ' SPI routines
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiSetConfig(ByVal hDevice As Integer, ByVal CPOL As Byte, ByVal CPHA As Byte) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiGetConfig(ByVal hDevice As Integer, ByRef pCPOL As Byte, ByRef pCPHA As Byte) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiSetConfigEx(ByVal hDevice As Integer, ByVal Config As U2C_LONG) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiGetConfigEx(ByVal hDevice As Integer, ByRef pConfig As U2C_LONG) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiSetFreq(ByVal hDevice As Integer, ByVal Frequency As Byte) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiGetFreq(ByVal hDevice As Integer, ByRef pFrequency As Byte) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiReadWrite(ByVal hDevice As Integer, ByRef pOutBuffer As SPI_DATA, ByRef pInBuffer As SPI_DATA, ByVal Lenght As Short) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiWrite(ByVal hDevice As Integer, ByRef pOutBuffer As SPI_DATA, ByVal Lenght As Short) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiRead(ByVal hDevice As Integer, ByRef pInBuffer As SPI_DATA, ByVal Lenght As Short) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiConfigSS(ByVal hDevice As Integer, ByVal IoNumber As Integer, ByVal ActiveHigh As Boolean) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiReadWriteSS(ByVal hDevice As Integer, ByRef pOutBuffer As SPI_DATA, ByRef pInBuffer As SPI_DATA, ByVal Lenght As Short, ByVal IoNumber As Integer, ByVal ActiveHigh As Boolean) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiWriteSS(ByVal hDevice As Integer, ByRef pOutBuffer As SPI_DATA, ByVal Lenght As Short, ByVal IoNumber As Integer, ByVal ActiveHigh As Boolean) As Integer
    End Function
    <DllImport("I2cBrdg.dll")> Public Function U2C_SpiReadSS(ByVal hDevice As Integer, ByRef pInBuffer As SPI_DATA, ByVal Lenght As Short, ByVal IoNumber As Integer, ByVal ActiveHigh As Boolean) As Integer
    End Function

End Module
