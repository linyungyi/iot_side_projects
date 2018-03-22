<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnInitDev = New System.Windows.Forms.Button
        Me.btnOpenDev = New System.Windows.Forms.Button
        Me.btnDataFile = New System.Windows.Forms.Button
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog
        Me.btnDownloadDatafile = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'btnInitDev
        '
        Me.btnInitDev.Location = New System.Drawing.Point(12, 12)
        Me.btnInitDev.Name = "btnInitDev"
        Me.btnInitDev.Size = New System.Drawing.Size(260, 31)
        Me.btnInitDev.TabIndex = 0
        Me.btnInitDev.Text = "InitDev"
        Me.btnInitDev.UseVisualStyleBackColor = True
        '
        'btnOpenDev
        '
        Me.btnOpenDev.Location = New System.Drawing.Point(12, 49)
        Me.btnOpenDev.Name = "btnOpenDev"
        Me.btnOpenDev.Size = New System.Drawing.Size(260, 31)
        Me.btnOpenDev.TabIndex = 1
        Me.btnOpenDev.Text = "OpenDev"
        Me.btnOpenDev.UseVisualStyleBackColor = True
        '
        'btnDataFile
        '
        Me.btnDataFile.Location = New System.Drawing.Point(12, 86)
        Me.btnDataFile.Name = "btnDataFile"
        Me.btnDataFile.Size = New System.Drawing.Size(260, 31)
        Me.btnDataFile.TabIndex = 2
        Me.btnDataFile.Text = "DataFile"
        Me.btnDataFile.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'btnDownloadDatafile
        '
        Me.btnDownloadDatafile.Location = New System.Drawing.Point(12, 124)
        Me.btnDownloadDatafile.Name = "btnDownloadDatafile"
        Me.btnDownloadDatafile.Size = New System.Drawing.Size(260, 31)
        Me.btnDownloadDatafile.TabIndex = 3
        Me.btnDownloadDatafile.Text = "DownloadDataFile"
        Me.btnDownloadDatafile.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 262)
        Me.Controls.Add(Me.btnDownloadDatafile)
        Me.Controls.Add(Me.btnDataFile)
        Me.Controls.Add(Me.btnOpenDev)
        Me.Controls.Add(Me.btnInitDev)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnInitDev As System.Windows.Forms.Button
    Friend WithEvents btnOpenDev As System.Windows.Forms.Button
    Friend WithEvents btnDataFile As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents btnDownloadDatafile As System.Windows.Forms.Button

End Class
