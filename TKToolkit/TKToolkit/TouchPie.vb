Public Class TouchPie
    Dim bluePen As New Pen(Color.Blue)
    Dim redPen As New Pen(Color.Red, 2)
    Dim grayPen As New Pen(Color.Gray, 2)
    Dim blackPen As New Pen(Color.Black, 2)
    Dim whitePen As New Pen(Color.White, 2)
    Dim silverPen As New Pen(Color.Silver, 2)

    Dim whiteBrush As New SolidBrush(Color.White)
    Dim blackBrush As New SolidBrush(Color.Black)
    Dim redBrush As New SolidBrush(Color.Red)
    Dim grayBrush As New SolidBrush(Color.Gray)
    Dim silverBrush As New SolidBrush(Color.Silver)
    Dim greenBrush As New SolidBrush(Color.Green)
    Dim orangeBrush As New SolidBrush(Color.Orange)
    Dim transparentBrush As New SolidBrush(Color.Transparent)
    Dim fon As New Font("新細明體", 8)

    Dim onFire As Integer = False
    Public Property Fire() As Boolean
        Get
            Return onFire
        End Get
        Set(ByVal Value As Boolean)
            onFire = Value
        End Set
    End Property

    Dim m_hover As Boolean = False
    Public Property Hover() As Boolean
        Get
            Return m_hover
        End Get
        Set(ByVal Value As Boolean)
            m_hover = Value
        End Set
    End Property

    Dim m_startAngle As Single = 0.0F
    Public Property StartAngle() As Single
        Get
            Return m_startAngle
        End Get
        Set(ByVal Value As Single)
            m_startAngle = Value
        End Set
    End Property

    Dim m_sweepAngle As Single = 0.0F
    Public Property SweepAngle() As Single
        Get
            Return m_sweepAngle
        End Get
        Set(ByVal Value As Single)
            m_sweepAngle = Value
        End Set
    End Property

    Dim m_pin As Integer = -1 'number of index
    Public Property pin() As Integer
        Get
            Return m_pin
        End Get
        Set(ByVal Value As Integer)
            m_pin = Value
        End Set
    End Property

    Dim m_object As Integer = -1 'number of index
    Public Property ObjectID() As Integer
        Get
            Return m_object
        End Get
        Set(ByVal Value As Integer)
            m_object = Value
            Refresh()
        End Set
    End Property

    Dim m_sensor_port As Integer = -1
    Public Property SensorPort() As Integer
        Get
            Return m_sensor_port
        End Get
        Set(ByVal Value As Integer)
            m_sensor_port = Value
        End Set
    End Property

    Dim m_map_port As Integer = -1
    Public Property MapPort() As Integer
        Get
            Return m_map_port
        End Get
        Set(ByVal Value As Integer)
            m_map_port = Value
        End Set
    End Property

    Dim m_control_type As Integer = 0
    Public Property ControlType() As Integer
        Get
            Return m_control_type
        End Get
        Set(ByVal Value As Integer)
            m_control_type = Value
        End Set
    End Property

    Dim m_sensitivity As Integer = 7
    Public Property Sensitivity() As Integer
        Get
            Return m_sensitivity
        End Get
        Set(ByVal Value As Integer)
            m_sensitivity = Value
        End Set
    End Property

    Dim m_sensitivity_ana As Integer = 7
    Public Property SensitivityAna() As Integer
        Get
            Return m_sensitivity_ana
        End Get
        Set(ByVal Value As Integer)
            m_sensitivity_ana = Value
        End Set
    End Property

    Dim m_sensitivity_dig As Integer = 7
    Public Property SensitivityDig() As Integer
        Get
            Return m_sensitivity_dig
        End Get
        Set(ByVal Value As Integer)
            m_sensitivity_dig = Value
        End Set
    End Property

    Dim m_noise_filter As Integer = 7
    Public Property NoiseFilter() As Integer
        Get
            Return m_noise_filter
        End Get
        Set(ByVal Value As Integer)
            m_noise_filter = Value
        End Set
    End Property

    Dim m_deglitch_count As Integer = 7
    Public Property DeglitchCount() As Integer
        Get
            Return m_deglitch_count
        End Get
        Set(ByVal Value As Integer)
            m_deglitch_count = Value
        End Set
    End Property

    Dim m_trigger_port As Integer = -1
    Public Property TiggerPort() As Integer
        Get
            Return m_trigger_port
        End Get
        Set(ByVal Value As Integer)
            m_trigger_port = Value
        End Set
    End Property

    Dim m_map_port_init As Integer = 0
    Public Property MapPortInit() As Integer
        Get
            Return m_map_port_init
        End Get
        Set(ByVal Value As Integer)
            m_map_port_init = Value
        End Set
    End Property

    Dim m_direction As Integer = TKtoolkit.Direction.clockwise
    Public Property Direction() As Integer
        Get
            Return m_direction
        End Get
        Set(ByVal Value As Integer)
            m_direction = Value
        End Set
    End Property

    Dim m_pwm As Integer = -1
    Public Property MapPWM() As Integer
        Get
            Return m_pwm
        End Get
        Set(ByVal Value As Integer)
            m_pwm = Value
        End Set
    End Property

    Dim m_value_type As Integer = TKtoolkit.ValueType.absolute
    Public Property ValueType() As Integer
        Get
            Return m_value_type
        End Get
        Set(ByVal Value As Integer)
            m_value_type = Value
        End Set
    End Property

    Dim m_step_type As Integer = 0
    Public Property StepValue() As Integer
        Get
            Return m_step_type
        End Get
        Set(ByVal Value As Integer)
            m_step_type = Value
        End Set
    End Property

    Dim m_start_type As Integer = 0
    Public Property StartValue() As Integer
        Get
            Return m_start_type
        End Get
        Set(ByVal Value As Integer)
            m_start_type = Value
        End Set
    End Property

    Dim m_stop_type As Integer = 0
    Public Property StopValue() As Integer
        Get
            Return m_stop_type
        End Get
        Set(ByVal Value As Integer)
            m_stop_type = Value
        End Set
    End Property

    Dim m_interpolation As Integer = 1
    Public Property Interpolation() As Integer
        Get
            Return m_interpolation
        End Get
        Set(ByVal Value As Integer)
            m_interpolation = Value
        End Set
    End Property

    Dim m_speed As Integer = 1
    Public Property SpeedVector() As Integer
        Get
            Return m_speed
        End Get
        Set(ByVal Value As Integer)
            m_speed = Value
        End Set
    End Property

    Dim m_noise_filter_outer As Integer = 0
    Public Property NoiseFilterOuter() As Integer
        Get
            Return m_noise_filter_outer
        End Get
        Set(ByVal Value As Integer)
            m_noise_filter_outer = Value
        End Set
    End Property

    Dim m_threshold_high As Integer = 255
    Public Property ThresholdHigh() As Integer
        Get
            Return m_threshold_high
        End Get
        Set(ByVal Value As Integer)
            m_threshold_high = Value
        End Set
    End Property

    Dim m_threshold_low As Integer = 0
    Public Property ThresholdLow() As Integer
        Get
            Return m_threshold_low
        End Get
        Set(ByVal Value As Integer)
            m_threshold_low = Value
        End Set
    End Property

    Private Sub TouchBar_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        'Dim n, x, y, w, h As Integer
        'x = 20
        'y = 20
        'w = 40
        'h = 40
        'Dim rect1 As New Rectangle(x, y, w, h)
        'Dim rect2 As New Rectangle(x + 1, y + 1, w - 1, h - 1)
        'Dim slbBrush As New SolidBrush(Color.Silver)
        'e.Graphics.TranslateTransform(100, 100)
        'For n = 1 To 9
        '    e.Graphics.RotateTransform(Math.PI * 13)
        '    e.Graphics.DrawRectangle(Pens.Red, rect1)
        '    e.Graphics.FillRectangle(slbBrush, rect2)
        'Next

        'Dim startAngle As Single = 0.0F
        'Dim sweepAngle As Single = 30.0F
        'Dim rect1 As New Rectangle(0, 0, 150, 150)
        'Dim rect2 As New Rectangle(0, 0, 150, 150)

        'Dim number As Integer = 28
        'Dim i As Integer
        'sweepAngle = 360 / number

        'For i = 0 To number - 1
        '    e.Graphics.FillPie(silverBrush, rect1, sweepAngle * i, sweepAngle * (i + 1))
        'Next

        'For i = 0 To number - 1
        '    e.Graphics.DrawPie(blackPen, rect1, sweepAngle * i, sweepAngle * (i + 1))
        'Next

        'Dim Path As New System.Drawing.Drawing2D.GraphicsPath

        'Path.AddEllipse(rect1)
        'Me.Region = New Region(Path)



        ''''''''''''''''''''''''''''''''''''''
        'Dim path1, path2 As New System.Drawing.Drawing2D.GraphicsPath
        'Dim p As New Pen(Color.Red, 5) '畫筆  
        'Dim b As Brush = Brushes.Orange '筆刷  
        'path1.AddEllipse(0, 0, 150, 150)
        'path2.AddEllipse(25, 25, 100, 100)
        'e.Graphics.DrawPath(p, path1) '空心  
        'e.Graphics.FillPath(b, path2) '填滿  
        ''''''''''''''''''''''''''''''''''''''

        Dim Path As New System.Drawing.Drawing2D.GraphicsPath

        Dim rect1 As New Rectangle(0, 0, 150, 150)
        Dim rect2 As New Rectangle(50, 50, 50, 50)

        ''e.Graphics.FillPie(silverBrush, rect1, sweepAngle, sweepAngle)
        ''e.Graphics.FillPie(transparentBrush, rect2, sweepAngle, sweepAngle)
        ''e.Graphics.DrawPie(blackPen, rect1, sweepAngle, sweepAngle)
        ''e.Graphics.DrawPie(blackPen, rect2, sweepAngle, sweepAngle)

        Dim rgn As Region

        'Debug.Print(m_startAngle & ",," & m_sweepAngle)
        Path.AddPie(rect1, m_startAngle, m_sweepAngle)
        Path.AddPie(rect2, m_startAngle, m_sweepAngle)

        rgn = New Region(Path)

        If onFire Then
            e.Graphics.FillRegion(redBrush, rgn)
        Else
            If Hover Then
                e.Graphics.FillRegion(orangeBrush, rgn)
            Else
                e.Graphics.FillRegion(greenBrush, rgn)
            End If

        End If



        '中心點 x,y
        '圓周上的任一點座標為(x1, y1)
        'Sin(度數)=y1/R		R:半徑
        '所以(y1 = y + R * Sin(度數))
        '同理(x1 = x + R * Cos(度數))
        '徑度數 = 度數 * PI / 180


        'e.Graphics.DrawPie(blackPen, rect1, StartAngle, SweepAngle)
        'e.Graphics.DrawPie(blackPen, rect2, StartAngle, SweepAngle)
        e.Graphics.DrawPath(whitePen, Path)
        e.Graphics.FillEllipse(Brushes.Black, rect2)


        'e.Graphics.DrawPie(blackPen, rect2, StartAngle, SweepAngle)

        Dim strX As Integer = 70 + 61 * Math.Cos((m_startAngle + m_sweepAngle / 2) * Math.PI / 180)
        Dim strY As Integer = 70 + 61 * Math.Sin((m_startAngle + m_sweepAngle / 2) * Math.PI / 180)
        'Debug.Print(strX & "---" & strY)
        e.Graphics.DrawString(m_object, fon, blackBrush, strX, strY)


        Me.Region = rgn

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        'If onFire Then
        '    onFire = False
        'Else
        '    onFire = True
        'End If
        Me.Refresh()
        'Me.Invalidate()

    End Sub

    ReadOnly Property IsWorking() As Boolean 'returns whether the Key is working or not
        Get
            Return Timer1.Enabled
        End Get
    End Property

End Class
