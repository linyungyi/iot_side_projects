<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TKInput
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TKInput))
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.SmoothProgressBar1 = New SmoothProgressBar.SmoothProgressBar
        Me.GTrackBar1 = New gTrackBar.gTrackBar
        Me.GTrackBar2 = New gTrackBar.gTrackBar
        Me.SuspendLayout()
        '
        'Timer1
        '
        Me.Timer1.Interval = 10
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.ForeColor = System.Drawing.Color.Blue
        Me.Label1.Location = New System.Drawing.Point(15, 113)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(11, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "0"
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(3, 155)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(40, 10)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "0"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'SmoothProgressBar1
        '
        Me.SmoothProgressBar1.BackColor = System.Drawing.SystemColors.Control
        Me.SmoothProgressBar1.Horizontal = False
        Me.SmoothProgressBar1.Location = New System.Drawing.Point(18, 28)
        Me.SmoothProgressBar1.Maximum = 256
        Me.SmoothProgressBar1.Name = "SmoothProgressBar1"
        Me.SmoothProgressBar1.Size = New System.Drawing.Size(16, 76)
        Me.SmoothProgressBar1.TabIndex = 1
        '
        'GTrackBar1
        '
        Me.GTrackBar1.AButColorA = System.Drawing.Color.CornflowerBlue
        Me.GTrackBar1.AButColorB = System.Drawing.Color.Lavender
        Me.GTrackBar1.AButColorBorder = System.Drawing.Color.SteelBlue
        Me.GTrackBar1.ArrowColorDown = System.Drawing.Color.GhostWhite
        Me.GTrackBar1.ArrowColorHover = System.Drawing.Color.DarkBlue
        Me.GTrackBar1.ArrowColorUp = System.Drawing.Color.LightSteelBlue
        Me.GTrackBar1.BackColor = System.Drawing.Color.Transparent
        Me.GTrackBar1.BorderColor = System.Drawing.Color.Black
        Me.GTrackBar1.BorderShow = False
        Me.GTrackBar1.BrushDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal
        Me.GTrackBar1.BrushStyle = gTrackBar.gTrackBar.eBrushStyle.Image
        Me.GTrackBar1.ChangeLarge = 1
        Me.GTrackBar1.ChangeSmall = 1
        Me.GTrackBar1.ColorDown = System.Drawing.Color.CornflowerBlue
        Me.GTrackBar1.ColorDownBorder = System.Drawing.Color.DarkSlateBlue
        Me.GTrackBar1.ColorDownHiLt = System.Drawing.Color.AliceBlue
        Me.GTrackBar1.ColorHover = System.Drawing.Color.RoyalBlue
        Me.GTrackBar1.ColorHoverBorder = System.Drawing.Color.Blue
        Me.GTrackBar1.ColorHoverHiLt = System.Drawing.Color.White
        Me.GTrackBar1.ColorUp = System.Drawing.Color.MediumBlue
        Me.GTrackBar1.ColorUpBorder = System.Drawing.Color.DarkBlue
        Me.GTrackBar1.ColorUpHiLt = System.Drawing.Color.AliceBlue
        Me.GTrackBar1.FloatValue = False
        Me.GTrackBar1.FloatValueFont = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.GTrackBar1.FloatValueFontColor = System.Drawing.Color.MediumBlue
        Me.GTrackBar1.Label = Nothing
        Me.GTrackBar1.LabelAlighnment = System.Drawing.StringAlignment.Near
        Me.GTrackBar1.LabelColor = System.Drawing.Color.MediumBlue
        Me.GTrackBar1.LabelFont = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold)
        Me.GTrackBar1.LabelPadding = New System.Windows.Forms.Padding(3)
        Me.GTrackBar1.LabelShow = False
        Me.GTrackBar1.Location = New System.Drawing.Point(-11, -5)
        Me.GTrackBar1.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.GTrackBar1.MaxValue = 255
        Me.GTrackBar1.MinValue = 0
        Me.GTrackBar1.Name = "GTrackBar1"
        Me.GTrackBar1.Orientation = System.Windows.Forms.Orientation.Vertical
        Me.GTrackBar1.ShowFocus = False
        Me.GTrackBar1.Size = New System.Drawing.Size(42, 120)
        Me.GTrackBar1.SliderCapEnd = System.Drawing.Drawing2D.LineCap.Round
        Me.GTrackBar1.SliderCapStart = System.Drawing.Drawing2D.LineCap.Round
        Me.GTrackBar1.SliderColorHigh = System.Drawing.Color.DarkGray
        Me.GTrackBar1.SliderColorLow = System.Drawing.Color.Green
        Me.GTrackBar1.SliderFocalPt = CType(resources.GetObject("GTrackBar1.SliderFocalPt"), System.Drawing.PointF)
        Me.GTrackBar1.SliderHighlightPt = CType(resources.GetObject("GTrackBar1.SliderHighlightPt"), System.Drawing.PointF)
        Me.GTrackBar1.SliderImage = CType(resources.GetObject("GTrackBar1.SliderImage"), System.Drawing.Bitmap)
        Me.GTrackBar1.SliderShape = gTrackBar.gTrackBar.eShape.Ellipse
        Me.GTrackBar1.SliderSize = New System.Drawing.Size(16, 16)
        Me.GTrackBar1.SliderWidthHigh = 1
        Me.GTrackBar1.SliderWidthLow = 3
        Me.GTrackBar1.TabIndex = 0
        Me.GTrackBar1.TickColor = System.Drawing.Color.DarkGray
        Me.GTrackBar1.TickInterval = 10
        Me.GTrackBar1.TickType = gTrackBar.gTrackBar.eTickType.None
        Me.GTrackBar1.TickWidth = 5
        Me.GTrackBar1.UpDownAutoWidth = True
        Me.GTrackBar1.UpDownShow = False
        Me.GTrackBar1.UpDownWidth = 10
        Me.GTrackBar1.Value = 0
        Me.GTrackBar1.ValueBox = gTrackBar.gTrackBar.eValueBox.Left
        Me.GTrackBar1.ValueBoxBackColor = System.Drawing.Color.Transparent
        Me.GTrackBar1.ValueBoxBorder = System.Drawing.Color.Transparent
        Me.GTrackBar1.ValueBoxFont = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GTrackBar1.ValueBoxFontColor = System.Drawing.Color.Green
        Me.GTrackBar1.ValueBoxShape = gTrackBar.gTrackBar.eShape.Rectangle
        Me.GTrackBar1.ValueBoxSize = New System.Drawing.Size(40, 20)
        '
        'GTrackBar2
        '
        Me.GTrackBar2.AButColorA = System.Drawing.Color.CornflowerBlue
        Me.GTrackBar2.AButColorB = System.Drawing.Color.Lavender
        Me.GTrackBar2.AButColorBorder = System.Drawing.Color.SteelBlue
        Me.GTrackBar2.ArrowColorDown = System.Drawing.Color.GhostWhite
        Me.GTrackBar2.ArrowColorHover = System.Drawing.Color.DarkBlue
        Me.GTrackBar2.ArrowColorUp = System.Drawing.Color.LightSteelBlue
        Me.GTrackBar2.BackColor = System.Drawing.Color.Transparent
        Me.GTrackBar2.BorderColor = System.Drawing.Color.Black
        Me.GTrackBar2.BorderShow = False
        Me.GTrackBar2.BrushDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal
        Me.GTrackBar2.BrushStyle = gTrackBar.gTrackBar.eBrushStyle.Image
        Me.GTrackBar2.ChangeLarge = 1
        Me.GTrackBar2.ChangeSmall = 1
        Me.GTrackBar2.ColorDown = System.Drawing.Color.CornflowerBlue
        Me.GTrackBar2.ColorDownBorder = System.Drawing.Color.DarkSlateBlue
        Me.GTrackBar2.ColorDownHiLt = System.Drawing.Color.AliceBlue
        Me.GTrackBar2.ColorHover = System.Drawing.Color.RoyalBlue
        Me.GTrackBar2.ColorHoverBorder = System.Drawing.Color.Blue
        Me.GTrackBar2.ColorHoverHiLt = System.Drawing.Color.White
        Me.GTrackBar2.ColorUp = System.Drawing.Color.MediumBlue
        Me.GTrackBar2.ColorUpBorder = System.Drawing.Color.DarkBlue
        Me.GTrackBar2.ColorUpHiLt = System.Drawing.Color.AliceBlue
        Me.GTrackBar2.FloatValue = False
        Me.GTrackBar2.FloatValueFont = New System.Drawing.Font("Arial", 8.0!, System.Drawing.FontStyle.Bold)
        Me.GTrackBar2.FloatValueFontColor = System.Drawing.Color.MediumBlue
        Me.GTrackBar2.Label = Nothing
        Me.GTrackBar2.LabelAlighnment = System.Drawing.StringAlignment.Near
        Me.GTrackBar2.LabelColor = System.Drawing.Color.MediumBlue
        Me.GTrackBar2.LabelFont = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold)
        Me.GTrackBar2.LabelPadding = New System.Windows.Forms.Padding(3)
        Me.GTrackBar2.LabelShow = False
        Me.GTrackBar2.Location = New System.Drawing.Point(25, -5)
        Me.GTrackBar2.Margin = New System.Windows.Forms.Padding(2, 3, 2, 3)
        Me.GTrackBar2.MaxValue = 255
        Me.GTrackBar2.MinValue = 0
        Me.GTrackBar2.Name = "GTrackBar2"
        Me.GTrackBar2.Orientation = System.Windows.Forms.Orientation.Vertical
        Me.GTrackBar2.ShowFocus = False
        Me.GTrackBar2.Size = New System.Drawing.Size(32, 120)
        Me.GTrackBar2.SliderCapEnd = System.Drawing.Drawing2D.LineCap.Round
        Me.GTrackBar2.SliderCapStart = System.Drawing.Drawing2D.LineCap.Round
        Me.GTrackBar2.SliderColorHigh = System.Drawing.Color.Red
        Me.GTrackBar2.SliderColorLow = System.Drawing.Color.DarkGray
        Me.GTrackBar2.SliderFocalPt = CType(resources.GetObject("GTrackBar2.SliderFocalPt"), System.Drawing.PointF)
        Me.GTrackBar2.SliderHighlightPt = CType(resources.GetObject("GTrackBar2.SliderHighlightPt"), System.Drawing.PointF)
        Me.GTrackBar2.SliderImage = CType(resources.GetObject("GTrackBar2.SliderImage"), System.Drawing.Bitmap)
        Me.GTrackBar2.SliderShape = gTrackBar.gTrackBar.eShape.Ellipse
        Me.GTrackBar2.SliderSize = New System.Drawing.Size(16, 16)
        Me.GTrackBar2.SliderWidthHigh = 3
        Me.GTrackBar2.SliderWidthLow = 1
        Me.GTrackBar2.TabIndex = 0
        Me.GTrackBar2.TickColor = System.Drawing.Color.DarkGray
        Me.GTrackBar2.TickInterval = 10
        Me.GTrackBar2.TickType = gTrackBar.gTrackBar.eTickType.None
        Me.GTrackBar2.TickWidth = 5
        Me.GTrackBar2.UpDownAutoWidth = True
        Me.GTrackBar2.UpDownShow = False
        Me.GTrackBar2.UpDownWidth = 10
        Me.GTrackBar2.Value = 0
        Me.GTrackBar2.ValueBox = gTrackBar.gTrackBar.eValueBox.Left
        Me.GTrackBar2.ValueBoxBackColor = System.Drawing.Color.Transparent
        Me.GTrackBar2.ValueBoxBorder = System.Drawing.Color.Transparent
        Me.GTrackBar2.ValueBoxFont = New System.Drawing.Font("Arial", 8.25!)
        Me.GTrackBar2.ValueBoxFontColor = System.Drawing.Color.Red
        Me.GTrackBar2.ValueBoxShape = gTrackBar.gTrackBar.eShape.Rectangle
        Me.GTrackBar2.ValueBoxSize = New System.Drawing.Size(40, 20)
        '
        'TKInput
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.SmoothProgressBar1)
        Me.Controls.Add(Me.GTrackBar1)
        Me.Controls.Add(Me.GTrackBar2)
        Me.Name = "TKInput"
        Me.Size = New System.Drawing.Size(55, 186)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents GTrackBar1 As gTrackBar.gTrackBar
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents SmoothProgressBar1 As SmoothProgressBar.SmoothProgressBar
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents GTrackBar2 As gTrackBar.gTrackBar

End Class
