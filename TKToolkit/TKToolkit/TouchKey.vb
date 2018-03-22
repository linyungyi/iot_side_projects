Public Class TouchKey

    Dim rect As New Rectangle(0, 0, 40, 40)

    Dim bluePen As New Pen(Color.Blue)
    Dim redPen As New Pen(Color.Red, 2)
    Dim grayPen As New Pen(Color.Gray, 2)
    Dim blackPen As New Pen(Color.Black, 2)
    Dim whitePen As New Pen(Color.White, 2)
    Dim siliverBrush As New SolidBrush(Color.Silver)
    Dim blackBrush As New SolidBrush(Color.Black)
    Dim whiteBrush As New SolidBrush(Color.White)
    Dim greenBrush As New SolidBrush(Color.Green)
    Dim orangeBrush As New SolidBrush(Color.Orange)
    Dim redBrush As New SolidBrush(Color.Red)
    Dim fon As New Font("新細明體", 10)

    Dim onFire As Integer = False

    Public Event triggerPortChanged(ByVal id As Integer, ByVal value As Integer)
    Public Event sensorPortChanged(ByVal id As Integer, ByVal value As Integer)
    Public Event mapPortChanged(ByVal id As Integer, ByVal value As Integer)


    Public Sub New()

        ' 此為 Windows Form 設計工具所需的呼叫。
        InitializeComponent()

        ' 在 InitializeComponent() 呼叫之後加入任何初始設定。
        Dim newGuid As Guid = Guid.NewGuid
        Me.mUniqueID = newGuid

    End Sub

    Public Sub New(ByVal theId As Guid, _
                ByVal theIndex As Integer, _
                ByVal theObjectId As Integer, _
                ByVal theLocation As Point)

        InitializeComponent()

        ' when restoring from data after deserialization, set the UID to
        ' the stored value and the recovered properties for the control
        ' including its location are stored into a new instance of the 
        ' user control
        mUniqueID = theId
        m_index = theIndex
        m_object = theObjectId

        'Me.Location = theLocation

    End Sub

#Region "Properties"

    Public Property myRectangle() As Rectangle
        Get
            Return rect
        End Get
        Set(ByVal Value As Rectangle)
            rect = Value
            Refresh()
        End Set
    End Property


    Private mUniqueID As Guid
    Public ReadOnly Property ID() As Guid
        Get
            Return mUniqueID
        End Get
    End Property

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

    Dim m_index As Integer = 1 'number of index
    Public Property Index() As Integer
        Get
            Return m_index
        End Get
        Set(ByVal Value As Integer)
            m_index = Value
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

#End Region



    Private Sub TouchKey_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub TouchKey_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        If onFire Then
            e.Graphics.FillRectangle(redBrush, rect)
            e.Graphics.DrawRectangle(whitePen, rect)

            'e.Graphics.DrawString("key" & m_index, fon, blackBrush, 5, 5)
            e.Graphics.DrawString(m_object, fon, blackBrush, 2, 2)

            Me.Region = New Region(rect)
        Else
            If Hover Then
                e.Graphics.FillRectangle(orangeBrush, rect)
            Else
                e.Graphics.FillRectangle(greenBrush, rect)
            End If
            'e.Graphics.FillRectangle(greenBrush, rect)

            e.Graphics.DrawRectangle(whitePen, rect)

            'e.Graphics.DrawString("key" & m_index, fon, blackBrush, 5, 5)
            e.Graphics.DrawString(m_object, fon, blackBrush, 2, 2)

            Me.Region = New Region(rect)
        End If


    End Sub


    'Shadows Event MouseMove(ByVal sender As Object)
    'Shadows Event MouseLeave(ByVal sender As Object)

    'mouse leave event
    'Private Sub TouchKey_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.MouseLeave
    ''RaiseEvent MouseLeave(Me)
    '    Debug.Print("MouseLeave")
    'End Sub

    'mouse move event
    'Private Sub TouchKey_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseMove
    ''RaiseEvent MouseMove(Me)
    '    Debug.Print("MouseMove")
    'End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'If onFire Then
        '    onFire = False
        'Else
        '    onFire = True
        'End If
        'Me.Refresh()
        Invalidate()

    End Sub

    'ReadOnly Property IsWorking() As Boolean 'returns whether the Key is working or not
    '    Get
    '        Return Timer1.Enabled
    '    End Get
    'End Property

    'Public Overrides Sub Refresh() 'this function places the line in its proper place
    '    If onFire Then
    '        e.Graphics.FillRectangle(siliverBrush, rect)
    '        e.Graphics.DrawRectangle(blackPen, rect)
    '        Me.Region = New Region(rect)
    '    Else

    '    End If
    'End Sub

End Class
