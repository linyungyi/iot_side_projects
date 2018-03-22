

Module HidMdl
    Public Declare Function OpenDeviceUsb Lib "ZetHidLib.dll" () As Boolean
    Public Declare Function InitDevice Lib "ZetHidLib.dll" () As Boolean
    Public Declare Function ReadData Lib "ZetHidLib.dll" () As Boolean
    Public Declare Function WriteData Lib "ZetHidLib.dll" () As Boolean
    Public Declare Function CloseDeviceUsb Lib "ZetHidLib.dll" ()
    Public Declare Function CmdToDo Lib "ZetHidLib.dll" (ByVal cmd As Integer) As Boolean
    Public Declare Function getHexConfig Lib "ZetHidLib.dll" (ByRef conf0 As Integer, ByRef conf1 As Integer) As Boolean
    Public Declare Function isDataInSending Lib "ZetHidLib.dll" () As Boolean
    Public Declare Function getErrorCode Lib "ZetHidLib.dll" () As Integer
    Public Declare Function setErrorCode Lib "ZetHidLib.dll" (ByVal code As Integer) As Integer
    Public Declare Function GetFileInfo Lib "ZetHidLib.dll" (ByVal filename As String, ByVal bCodeFile As Boolean) As Boolean
    Public Declare Function getDataFileSize Lib "ZetHidLib.dll" () As Integer

End Module
