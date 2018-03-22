<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TKBar
    Inherits System.Windows.Forms.UserControl

    'UserControl 覆寫 Dispose 以清除元件清單。
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
        Me.Label1 = New System.Windows.Forms.Label
        Me.SmoothProgressBar1 = New SmoothProgressBar.SmoothProgressBar
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(3, 88)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(11, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "0"
        '
        'SmoothProgressBar1
        '
        Me.SmoothProgressBar1.BackColor = System.Drawing.SystemColors.Control
        Me.SmoothProgressBar1.Horizontal = False
        Me.SmoothProgressBar1.Location = New System.Drawing.Point(0, 0)
        Me.SmoothProgressBar1.Name = "SmoothProgressBar1"
        Me.SmoothProgressBar1.Size = New System.Drawing.Size(16, 76)
        Me.SmoothProgressBar1.TabIndex = 1
        '
        'TKBar
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.SmoothProgressBar1)
        Me.Name = "TKBar"
        Me.Size = New System.Drawing.Size(16, 109)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents SmoothProgressBar1 As SmoothProgressBar.SmoothProgressBar
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
