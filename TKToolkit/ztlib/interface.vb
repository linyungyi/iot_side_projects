Imports System.IO

Public Class zt_intf
    Shared i2s As iI2CSPI = iI2CSPI.iI2C

    Enum iI2CSPI
        iSPI
        iI2C
    End Enum

    Shared Function initialize() As Integer
    End Function

    Shared Function ztOpenDevice() As Integer
        Return U2C_OpenDevice(0)
    End Function

    Shared Function ztCloseDevice(ByVal hDevice As Integer) As Integer
        Return U2C_CloseDevice(hDevice) '
    End Function

    Shared Function ztEnableI2C() As Integer
        i2s = iI2CSPI.iI2C
    End Function

    Shared Function ztEnableSPI() As Integer
        i2s = iI2CSPI.iSPI
    End Function

    Shared Function ztRead(ByVal hDevice As Integer, ByRef buffer() As Byte, ByVal length As Short) As Boolean
        Dim result As U2C_RESULT
        If i2s = iI2CSPI.iI2C Then
            'I2C
            Dim transact As U2C_TRANSACTION
            transact.Initialize()
            With transact
                .nSlaveDeviceAddress = &H76S
                .nMemoryAddressLength = 0
            End With
            transact.nBufferLength.bLo = length
            result = U2C_Read(hDevice, transact)
            If result = I2CBridge.U2C_RESULT.U2C_SUCCESS Then
                'copy to buffer()
                buffer = transact.Buffer.Clone
                Return True
            Else
                Return False
            End If
        ElseIf i2s = iI2CSPI.iSPI Then
            'SPI
            Dim data As SPI_DATA
            data.Initialize()
            U2C_SpiSetFreq(hDevice, U2C_SPI_FREQ_200KHZ)
            result = U2C_SpiRead(hDevice, data, length)
            If result = I2CBridge.U2C_RESULT.U2C_SUCCESS Then
                'copy to buffer()
                buffer = data.buffer.Clone
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Shared Function ztWrite(ByVal hDevice As Integer, ByVal buffer() As Byte, ByVal length As Short) As Boolean
        Dim result As U2C_RESULT
        If i2s = iI2CSPI.iI2C Then
            'I2C
            Dim transact As U2C_TRANSACTION
            transact.Initialize()
            With transact
                .nSlaveDeviceAddress = &H76S
                .nMemoryAddressLength = 0
                .nBufferLength.bLo = length
            End With
            'copy from buffer() to transact's buffer
            transact.Buffer = buffer.Clone
            result = U2C_Write(hDevice, transact)

            If result = I2CBridge.U2C_RESULT.U2C_SUCCESS Then
                Return True
            Else
                Return False
            End If
        ElseIf i2s = iI2CSPI.iSPI Then
            'SPI
            Dim data As SPI_DATA
            data.Initialize()
            'copy from buffer() to SPI_DATA's buffer
            data.buffer = buffer.Clone
            result = U2C_SpiWrite(hDevice, data, length)

            If result = I2CBridge.U2C_RESULT.U2C_SUCCESS Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Shared Function ztDownload(ByVal hDevice As Integer, ByVal strFileName As String) As Boolean
        'This command is used to write program page by page, i.e. writer can only write one page at a time. 
        'One page consist of 128 bytes, therefore 16k bytes ROM data should be written 128 times
        'Dim oBuf As SPI_DATA
        Dim length, i, idx, nPage, idxBuffer As Integer
        Dim sr As StreamReader
        Dim line, temp As String
        Dim transact As U2C_TRANSACTION
        Dim pLength, pAddr, curAddr, dif As Integer
        Dim buffer(24 * 1024) As Byte
        Dim result As U2C_RESULT

        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H22S
            .nBufferLength.bLo = 3
        End With

        idx = 2
        idxBuffer = 0
        nPage = 0
        'reset buffer
        initByteArray(transact.Buffer, &HFFS)
        initByteArray(buffer, &HFFS)

        If UCase(getExtension(strFileName)) = "BIN" Then
            buffer = My.Computer.FileSystem.ReadAllBytes(strFileName)
            length = buffer.Length
            While length > 0
                If length >= 128 Then
                    'one page a time
                    For i = 0 To 128 - 1
                        transact.Buffer(idx) = buffer(idxBuffer)
                        idx += 1
                        idxBuffer += 1
                    Next
                    'write one page (128 bytes)
                    transact.Buffer(0) = &H22S
                    transact.Buffer(1) = nPage
                    transact.nBufferLength.bHi = 0
                    transact.nBufferLength.bLo = 128 + 2
                    length -= 128
                Else
                    'handle remaining bytes
                    For i = 0 To length - 1
                        transact.Buffer(idx) = buffer(idxBuffer)
                        idx += 1
                        idxBuffer += 1
                    Next
                    'write remaining bytes
                    transact.Buffer(0) = &H22S
                    transact.Buffer(1) = nPage
                    transact.nBufferLength.bHi = 0
                    transact.nBufferLength.bLo = length + 2
                End If
                result = U2C_Write(hDevice, transact)
                nPage += 1
            End While
        ElseIf UCase(getExtension(strFileName)) = "HEX" Then
            sr = New StreamReader(strFileName)
            If Not (sr Is Nothing) Then
                'send PASSWORD
                'fSendPassword()
                'send CODE OPTION
                'fSendCodeOption()
                'download code
                Do
                    transact.Buffer(0) = &H22S
                    line = sr.ReadLine()
                    If line = Nothing Then
                        Exit Do
                    End If
                    temp = Mid(line, 2, 2)
                    length = Convert.ToInt32(temp, 16)
                    'parse current address
                    curAddr = Convert.ToInt32(Mid(line, 4, 4), 16)
                    'add dummy bytes here if necessary
                    If curAddr > pAddr + pLength Then
                        dif = (curAddr - (pAddr + pLength))
                        idx += dif
                        idxBuffer += dif
                        While idx >= 127 + 2
                            'write one page (128 bytes)
                            transact.Buffer(0) = &H22S
                            transact.Buffer(1) = nPage
                            transact.nBufferLength.bHi = 0
                            transact.nBufferLength.bLo = 128 + 2
                            'copy to buffer for checking correctiveness

                            result = U2C_Write(hDevice, transact)
                            nPage += 1
                            idx -= 128
                            'reset buffer
                            initByteArray(transact.Buffer, &HFFS)
                        End While
                    End If
                    pLength = length
                    pAddr = curAddr
                    For i = 0 To length * 2 - 2 Step 2
                        temp = Mid(line, i + 10, 2)
                        'fill-in the Buffer
                        transact.Buffer(idx) = Convert.ToInt32(temp, 16)
                        buffer(idxBuffer) = transact.Buffer(idx)
                        If idx >= 127 + 2 Then
                            'write one page (128 bytes)
                            transact.Buffer(0) = &H22S
                            transact.Buffer(1) = nPage
                            transact.nBufferLength.bHi = 0
                            transact.nBufferLength.bLo = 128 + 2
                            'copy to buffer for checking correctiveness

                            result = U2C_Write(hDevice, transact)
                            nPage += 1
                            idx -= 128
                            idxBuffer = 128 * (nPage + 1)
                            'reset buffer
                            initByteArray(transact.Buffer, &HFFS)
                        Else
                            'idx = idx + 1
                            idxBuffer += 1
                        End If
                        idx += 1
                    Next
                Loop Until line Is Nothing
                'write remaining bytes, fill out to one page 128bytes
                transact.Buffer(0) = &H22S
                transact.Buffer(1) = nPage
                transact.nBufferLength.bHi = 0
                transact.nBufferLength.bLo = 128 + 2
                'copy to buffer for checking correctiveness

                result = U2C_Write(hDevice, transact)
                sr.Close()

            End If
        End If
    End Function
End Class
