Public Class TouchBar

    Dim rect As Rectangle = New Rectangle(0, 0, 20, 60)

    Dim bluePen As New Pen(Color.Blue)
    Dim redPen As New Pen(Color.Red, 2)
    Dim grayPen As New Pen(Color.Gray, 2)
    Dim blackPen As New Pen(Color.Black, 2)
    Dim whitePen As New Pen(Color.White, 2)

    Dim siliverBrush As New SolidBrush(Color.Silver)
    Dim greenBrush As New SolidBrush(Color.ForestGreen)
    Dim orangeBrush As New SolidBrush(Color.Orange)
    Dim blackBrush As New SolidBrush(Color.Black)
    Dim whiteBrush As New SolidBrush(Color.White)
    Dim redBrush As New SolidBrush(Color.Red)
    Dim fon As New Font("新細明體", 10)

    Public Event triggerPortChanged(ByVal id As Integer, ByVal value As Integer)
    Public Event sensorPortChanged(ByVal id As Integer, ByVal value As Integer)
    Public Event mapPortChanged(ByVal id As Integer, ByVal value As Integer)

    Public Property myRectangle() As Rectangle
        Get
            Return rect
        End Get
        Set(ByVal Value As Rectangle)
            rect = Value
            Refresh()
        End Set
    End Property

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
        End Set
    End Property

    Dim m_sensor_port As Integer = -1
    Public Property SensorPort() As Integer
        Get
            Return m_sensor_port
        End Get
        Set(ByVal Value As Integer)
            m_sensor_port = Value
            RaiseEvent sensorPortChanged(m_object, Value)
        End Set
    End Property

    Dim m_map_port As Integer = -1
    Public Property MapPort() As Integer
        Get
            Return m_map_port
        End Get
        Set(ByVal Value As Integer)
            m_map_port = Value
            RaiseEvent mapPortChanged(m_object, Value)
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
            RaiseEvent triggerPortChanged(m_object, Value)
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

    Dim m_direction As Integer = TKtoolkit.Direction.left
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


    Private Sub TouchBar_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub TouchBar_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint


        Dim Path As New System.Drawing.Drawing2D.GraphicsPath

        'Dim i As Integer

        'For i = 0 To 5
        '    rect = New Rectangle((15 * i), 0, 10, 40)
        '    e.Graphics.FillRectangle(siliverBrush, rect)
        '    e.Graphics.DrawRectangle(blackPen, rect)

        'Next

        'Dim rect2 As New Rectangle(0, 0, 15 * 6 - 5, 40)
        'Path.AddRectangle(rect2)
        'Me.Region = New Region(Path)

        'rect = New Rectangle(0, 0, 20, 60)

        If onFire Then
            e.Graphics.FillRectangle(redBrush, rect)
            'e.Graphics.DrawRectangle(blackPen, rect)
            e.Graphics.DrawRectangle(whitePen, rect)
            e.Graphics.DrawString(m_object, fon, blackBrush, 2, 2)
        Else
            If Hover Then
                e.Graphics.FillRectangle(orangeBrush, rect)
            Else
                e.Graphics.FillRectangle(greenBrush, rect)
            End If

            'e.Graphics.DrawRectangle(blackPen, rect)
            e.Graphics.DrawRectangle(whitePen, rect)
            e.Graphics.DrawString(m_object, fon, blackBrush, 2, 2)
        End If
        

        Me.Region = New Region(rect)

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
