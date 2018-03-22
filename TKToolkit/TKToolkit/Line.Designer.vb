<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Line
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
        Me.components = New System.ComponentModel.Container
        Me.FlashingTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'FlashingTimer
        '
        '
        'Line
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Name = "Line"
        Me.Size = New System.Drawing.Size(103, 124)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents FlashingTimer As System.Windows.Forms.Timer

End Class
