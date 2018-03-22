Imports System.Runtime.InteropServices
Imports System.IO

Module Module1
    Public Declare Function OpenDevice Lib "u2c_12.dll" (ByVal ii As Integer) As Integer
    Public Declare Function pcRead Lib "u2c_12.dll" (ByRef n As Integer, ByRef x1 As Integer, ByRef y1 As Integer, ByRef x2 As Integer, ByRef y2 As Integer, ByRef x3 As Integer, ByRef y3 As Integer, ByRef x4 As Integer, ByRef y4 As Integer) As Integer
    Public Declare Function adRead Lib "u2c_12.dll" (ByVal nx As Integer, ByVal ny As Integer, ByVal x() As Integer, ByVal y() As Integer, ByRef fingers As Integer) As Boolean
    Public Declare Function CloseDevice Lib "u2c_12.dll" () As Integer
    Public Declare Function DownloadCode Lib "u2c_12.dll" (ByVal buff() As Byte, ByVal n As Integer) As Integer
    Public Declare Function DownloadStart Lib "u2c_12.dll" () As Integer
    Public Declare Function DownloadEnd Lib "u2c_12.dll" () As Integer
    Public Declare Function passI2CSPI Lib "u2c_12.dll" (ByVal ii As Integer) As Integer
    Public Declare Function passI2CFreq Lib "u2c_12.dll" (ByVal f As Integer) As Integer
    Public Declare Function OpenDeviceISP Lib "u2c_12.dll" () As Integer
    Public Declare Function CloseDeviceISP Lib "u2c_12.dll" (ByVal i As Integer) As Integer
    Public Declare Function passOptionBit Lib "u2c_12.dll" (ByVal n As Integer) As Integer
    Public Declare Function zUSBIO_StreamI2C Lib "u2c_12.dll" (ByVal iIndex As Integer, ByVal iWriteLength As Integer, ByVal iWriteBuffer() As Byte, ByVal iReadLength As Integer, ByRef res As Integer) As String
    Public Declare Function zUSBIO_StreamSPI Lib "u2c_12.dll" (ByVal iIndex As Integer, ByVal iChipSelect As Integer, ByVal iWriteLength As Integer, ByVal iWriteBuffer() As Byte, ByRef res As Integer) As Integer
    Public Declare Function passADOption Lib "u2c_12.dll" (ByVal n As Integer) As Integer
    Public Declare Function initDivider Lib "u2c_12.dll" () As Integer

    Public Structure SPI_DATA
        <VBFixedArray(1023), MarshalAs(UnmanagedType.ByValArray, SizeConst:=1023)> Public buffer() As Byte
        Public Sub Initialize()
            ReDim buffer(1023)
        End Sub
    End Structure
    <DllImport("USBIOX.DLL")> Public Function USBIO_StreamSPI4(ByVal iIndex As Integer, ByVal iChipSelect As Integer, ByVal iLength As Integer, ByRef ioBuffer As SPI_DATA) As Boolean
    End Function

    <DllImport("USBIOX.DLL")> Public Function USBIO_StreamI2C(ByVal iIndex As Integer, ByVal iWriteLength As Integer, ByRef iWriteBuffer As SPI_DATA, ByVal iReadLength As Integer, ByRef oReadBuffer As SPI_DATA) As Boolean
    End Function

    ' Below function is used to READ from INI file(All One Line) 
    Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Int32, ByVal lpFileName As String) As Int32
    ' Below function is used to WRITE to INI file(All One Line)
    Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Long
    Declare Function GetPrivateProfileInt Lib "kernel32" Alias "GetPrivateProfileIntA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As Integer, ByVal lpFileName As String) As Integer

    Sub ResetIC(ByVal hDevice As Integer)
        Dim transact As U2C_TRANSACTION

        'reset RESET pin
        U2C_SingleIoWrite(hDevice, 2, False)

        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H20S
            .Buffer(1) = &HC5S
            .Buffer(2) = &H9DS
            .nBufferLength.bLo = 3
        End With
        U2C_Write(hDevice, transact)

        'set RESET pin
        U2C_SingleIoWrite(hDevice, 2, True)

        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H29S
            .nBufferLength.bLo = 1
        End With
        U2C_Write(hDevice, transact)
    End Sub

    Function getSign(ByVal n As Integer) As Boolean
        If n > 0 Then
            Return True
        Else : Return False
        End If
    End Function
    Sub initByteArray(ByRef a As Byte(), ByVal n As Integer)
        For i As Integer = 0 To a.Length - 1
            a(i) = n
        Next
    End Sub
    Function findMappingIdx(ByVal x() As Integer, ByVal idx As Integer) As Integer
        Dim pos() As Integer
        pos = findIndex(x, x(idx))
        If pos.Length = 1 Then
            Return pos(0)
        Else
            If pos(0) = idx Then
                Return pos(1)
            Else
                Return pos(0)
            End If
        End If
    End Function
    Function findIndex(ByVal x() As Integer, ByVal t As Integer) As Integer()
        Dim count As Integer = 0
        Dim result() As Integer
        For i As Integer = 0 To x.Length - 1
            If x(i) = t Then
                ReDim Preserve result(count)
                result(count) = i
                count += 1
            End If
        Next
        Return result
    End Function
    Function findIntInArray(ByVal x() As Integer, ByVal t As Integer) As Integer
        Dim count As Integer = 0
        Dim result As Integer = -1
        For i As Integer = 0 To x.Length - 1
            If x(i) = t Then
                result = i
                'count += 1
                Exit For
            End If
        Next
        Return result
    End Function
    Sub eliminateTHD(ByRef x As Integer(), ByVal thd As Integer)
        For i As Integer = 0 To x.Length - 1
            If x(i) < thd Then x(i) = 0
        Next
    End Sub
    Sub removeZeroEnclosed(ByRef x As Integer())
        For i As Integer = 1 To x.Length - 2
            If x(i - 1) = 0 And x(i + 1) = 0 Then x(i) = 0
        Next
    End Sub
    Function findAvgUTHD(ByVal x As Integer(), ByVal thd As Integer) As Integer
        Dim s As Integer = 0, count As Integer = 0
        For i As Integer = 0 To x.Length - 1
            If (x(i) < thd) Then
                s += x(i)
                count += 1
            End If
        Next
        Return s / count
    End Function
    Function findAvg(ByVal x As Integer(), ByVal n As Integer) As Double
        Dim s As Integer = 0
        For i As Integer = 0 To n - 1
            s += x(i)
        Next
        Return s / n
    End Function
    Function findMax(ByVal x As Integer()) As Integer
        Dim M As Integer = 0
        For i As Integer = 0 To x.Length - 1
            If x(i) > M Then
                M = x(i)
            End If
        Next
        Return M
    End Function
    Function findMaxIdx(ByVal x As Integer()) As Integer
        Dim M As Integer = 0
        Dim idx As Integer = -3
        For i As Integer = 0 To x.Length - 1
            If x(i) > M Then
                M = x(i)
                idx = i
            End If
        Next
        Return idx
    End Function
    Function findMin(ByVal x As Integer()) As Integer
        Dim m As Integer = 65535
        For i As Integer = 0 To x.Length - 1
            If x(i) < m Then
                m = x(i)
            End If
        Next
        Return m
    End Function
    Function find2nd(ByVal x As Integer()) As Integer
        'ignore Max
        Dim M As Integer = 0, S As Integer = 0
        For i As Integer = 0 To x.Length - 1
            If x(i) > M Then
                S = M
                M = x(i)
            ElseIf x(i) > S And x(i) <> M Then
                S = x(i)
            End If
        Next
        Return S
    End Function
    Function findNth(ByVal x As Integer(), ByVal n As Integer) As Integer
        'support to 4th
        Dim v(4) As Integer
        For i As Integer = 0 To x.Length - 1
            If x(i) > v(1) Then
                v(4) = v(3)
                v(3) = v(2)
                v(2) = v(1)
                v(1) = x(i)
            ElseIf x(i) > v(2) And x(i) <> v(1) Then
                v(4) = v(3)
                v(3) = v(2)
                v(2) = x(i)
            ElseIf x(i) > v(3) And x(i) < v(2) And x(i) <> v(2) Then
                v(4) = v(3)
                v(3) = x(i)
            ElseIf x(i) > v(4) And x(i) < v(3) And x(i) <> v(3) Then
                v(4) = x(i)
            End If
        Next
        Return v(n)
    End Function
    Sub swap(ByRef x As Integer, ByRef y As Integer)
        Dim temp As Integer
        temp = x
        x = y
        y = temp
    End Sub
    Function getFileLines(ByVal str As String) As Integer
        Dim sr As StreamReader
        Dim line As String
        Dim n As Integer = 0

        sr = New StreamReader(str)
        If Not (sr Is Nothing) Then
            Do
                line = sr.ReadLine()
                n = n + 1
            Loop Until line Is Nothing
        Else
            sr.Close()
            Return 0
        End If
        sr.Close()
        Return n + 1
    End Function
    Public Function byte2String(ByVal b() As Byte, ByVal len As Integer) As String
        Dim i As Integer
        Dim s, sTemp As String
        For i = 0 To len - 1
            sTemp = Hex(b(i))
            If sTemp.Length = 1 Then
                s = s + "0" + sTemp + " "
            Else
                s = s + Hex(b(i)) + " "
            End If
        Next
        Return s
    End Function
    Public Function byte2String(ByVal b() As Byte, ByVal idx As Integer, ByVal len As Integer) As String
        Dim i As Integer
        Dim s, sTemp As String
        For i = idx To idx + len - 1
            sTemp = Hex(b(i))
            If sTemp.Length = 1 Then
                s = s + "0" + sTemp + " "
            Else
                s = s + Hex(b(i)) + " "
            End If
        Next
        Return s
    End Function
    Public Function byte2StringNoSpace(ByVal b() As Byte, ByVal idx As Integer, ByVal len As Integer) As String
        Dim i As Integer
        Dim s, sTemp As String
        For i = idx To idx + len - 1
            sTemp = Hex(b(i))
            If sTemp.Length = 1 Then
                s = s + "0" + sTemp
            Else
                s = s + Hex(b(i))
            End If
        Next
        Return s
    End Function
    Public Sub removeEmptyArray(ByRef tempArray() As String)
        Dim LastNonEmpty As Integer = -1

        For i As Integer = 0 To tempArray.Length - 1
            If tempArray(i) <> "" Then
                LastNonEmpty += 1
                tempArray(LastNonEmpty) = tempArray(i)
            End If
        Next
        ReDim Preserve tempArray(LastNonEmpty)
    End Sub
    Public Function getExtension(ByVal str As String)
        Dim tempStr, tempArray() As String
        tempStr = StrReverse(str)
        tempArray = Split(tempStr, ".")
        Return StrReverse(tempArray(0))
    End Function
    Public Function removeExtension(ByVal str As String)
        Dim tempStr, tempArray() As String
        tempStr = StrReverse(str)
        tempArray = Split(tempStr, ".")
        Return StrReverse(tempArray(1))
    End Function
    Function Chr2String(ByVal c() As Char, ByVal len As Integer) As String
        Dim provider As IFormatProvider
        For i As Integer = 0 To len - 1
            Chr2String = Chr2String + Hex(CType(c(i), IConvertible).ToByte(provider)) + " "
        Next
    End Function
    Function HexToDec(ByVal HexValue As String) As Integer
        If HexValue.StartsWith("0x") = True Then
            HexValue = Mid(HexValue, 3, HexValue.Length - 1)
        Else
            HexValue = Mid(HexValue, 1, 2)
        End If
        HexToDec = Val("&H" & HexValue)
    End Function
    Function DecToBin(ByVal N As Integer) As String
        Dim count As Integer = 0
        Do
            DecToBin = N Mod 2 & DecToBin : N = N \ 2
            count += 1
        Loop Until N = 0
        For i As Integer = count + 1 To 8
            DecToBin = "0" & DecToBin
            If i = 4 Then
                DecToBin = " " & DecToBin
            End If
        Next
        DecToBin = DecToBin & " B"
    End Function
    Function BinToDec(ByVal Binary As String) As Integer
        Dim A, i As Integer

        For i = Len(Binary) To 1 Step -1
            A = (2 ^ (Len(Binary) - i)) * Val(Mid(Binary, i, 1))
            BinToDec = BinToDec + A
        Next i
    End Function
    Public Function calSigma(ByVal n As Integer, ByVal value() As Integer) As Double
        Dim sum As Long = 0
        Dim sum2 As Long = 0
        Dim avg, var As Double

        For i As Integer = 0 To n - 1
            sum += value(i)
        Next
        avg = sum / n
        For i As Integer = 0 To n - 1
            sum2 += Math.Pow((value(i) - avg), 2)
        Next
        var = sum2 / n
        calSigma = Math.Sqrt(var)
    End Function

    'Utility Class for Reading INI Files
    Public Class IniFile
        ' API functions
        Private Declare Ansi Function GetPrivateProfileString _
          Lib "kernel32.dll" Alias "GetPrivateProfileStringA" _
          (ByVal lpApplicationName As String, _
          ByVal lpKeyName As String, ByVal lpDefault As String, _
          ByVal lpReturnedString As System.Text.StringBuilder, _
          ByVal nSize As Integer, ByVal lpFileName As String) _
          As Integer
        Private Declare Ansi Function WritePrivateProfileString _
          Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
          (ByVal lpApplicationName As String, _
          ByVal lpKeyName As String, ByVal lpString As String, _
          ByVal lpFileName As String) As Integer
        Private Declare Ansi Function GetPrivateProfileInt _
          Lib "kernel32.dll" Alias "GetPrivateProfileIntA" _
          (ByVal lpApplicationName As String, _
          ByVal lpKeyName As String, ByVal nDefault As Integer, _
          ByVal lpFileName As String) As Integer
        Private Declare Ansi Function FlushPrivateProfileString _
          Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
          (ByVal lpApplicationName As Integer, _
          ByVal lpKeyName As Integer, ByVal lpString As Integer, _
          ByVal lpFileName As String) As Integer
        Dim strFilename As String

        ' Constructor, accepting a filename
        Public Sub New(ByVal Filename As String)
            strFilename = Filename
        End Sub

        ' Read-only filename property
        ReadOnly Property FileName() As String
            Get
                Return strFilename
            End Get
        End Property

        Public Function GetString(ByVal Section As String, _
          ByVal Key As String, ByVal [Default] As String) As String
            ' Returns a string from your INI file
            Dim intCharCount As Integer
            Dim objResult As New System.Text.StringBuilder(256)
            intCharCount = GetPrivateProfileString(Section, Key, _
               [Default], objResult, objResult.Capacity, strFilename)
            If intCharCount > 0 Then GetString = _
               Left(objResult.ToString, intCharCount)
        End Function

        Public Function GetInteger(ByVal Section As String, _
          ByVal Key As String, ByVal [Default] As Integer) As Integer
            ' Returns an integer from your INI file
            Return GetPrivateProfileInt(Section, Key, _
               [Default], strFilename)
        End Function

        Public Function GetBoolean(ByVal Section As String, _
          ByVal Key As String, ByVal [Default] As Boolean) As Boolean
            ' Returns a boolean from your INI file
            Return (GetPrivateProfileInt(Section, Key, _
               CInt([Default]), strFilename) = 1)
        End Function

        Public Sub WriteString(ByVal Section As String, _
          ByVal Key As String, ByVal Value As String)
            ' Writes a string to your INI file
            WritePrivateProfileString(Section, Key, Value, strFilename)
            Flush()
        End Sub

        Public Sub WriteInteger(ByVal Section As String, _
          ByVal Key As String, ByVal Value As Integer)
            ' Writes an integer to your INI file
            WriteString(Section, Key, CStr(Value))
            Flush()
        End Sub

        Public Sub WriteBoolean(ByVal Section As String, _
          ByVal Key As String, ByVal Value As Boolean)
            ' Writes a boolean to your INI file
            WriteString(Section, Key, CStr(CInt(Value)))
            Flush()
        End Sub

        Private Sub Flush()
            ' Stores all the cached changes to your INI file
            FlushPrivateProfileString(0, 0, 0, strFilename)
        End Sub
    End Class
End Module
