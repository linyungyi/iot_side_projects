Option Strict On
Option Explicit On

Imports System.Threading

Public Class Form1

    Private Sub btnInitDev_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInitDev.Click
        If InitDevice() Then
            If OpenDeviceUsb() Then
                'MsgBox("FOUND")
                CmdToDo(&HA6)

                Dim a, b As Integer
                a = 0
                b = 0

                Do
                    Thread.Sleep(1000)
                Loop While isDataInSending()

                getHexConfig(a, b)

                MsgBox(CStr(a))

            Else
                MsgBox("not found")
            End If
            'MsgBox("init")
        Else
            MsgBox("not init")
        End If
    End Sub

    Private Sub btnOpenDev_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenDev.Click

        If OpenDeviceUsb() Then
            MsgBox("FOUND")
        Else
            MsgBox("not found")
        End If

    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        'CloseDeviceUsb()
    End Sub

    Private Sub btnDataFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDataFile.Click

        OpenFileDialog1.Filter = "BIN File (*.bin)|*.bin|All Files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.RestoreDirectory = True
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            'reset buffer
            If GetFileInfo(OpenFileDialog1.FileName, False) Then
                MsgBox("YES")
            Else
                MsgBox(CStr(getErrorCode()))

            End If
            'MsgBox(OpenFileDialog1.FileName)

        End If
    End Sub

    Private Sub btnDownloadDatafile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownloadDatafile.Click
        OpenFileDialog1.Filter = "BIN File (*.bin)|*.bin|All Files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.RestoreDirectory = True
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            'reset buffer
            If GetFileInfo(OpenFileDialog1.FileName, False) Then

                'MsgBox(CStr(getDataFileSize()))
                If getDataFileSize() > 0 Then
                    setErrorCode(0)

                    CmdToDo(&HC3)
                    Do
                        Me.Enabled = False
                        Thread.Sleep(1000)
                    Loop While isDataInSending()

                    Me.Enabled = True

                    MsgBox("finished " + CStr(getErrorCode()))

                End If
            Else
                MsgBox(CStr(getErrorCode()))

            End If
            'MsgBox(OpenFileDialog1.FileName)

        End If
    End Sub
End Class
