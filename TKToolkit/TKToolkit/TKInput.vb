Public Class TKInput



    Dim bluePen As New Pen(Color.Blue)
    Dim redPen As New Pen(Color.Red, 2)
    Dim grayPen As New Pen(Color.Gray, 2)
    Dim blackPen As New Pen(Color.Black, 2)
    Dim siliverBrush As New SolidBrush(Color.Silver)
    Dim blackBrush As New SolidBrush(Color.Black)
    Dim grayBrush As New SolidBrush(Color.Gray)
    Dim greenBrush As New SolidBrush(Color.Green)
    Dim redBrush As New SolidBrush(Color.Red)
    Dim fon As New Font("新細明體", 10)
    Dim barX As Integer = 25
    Dim barY As Integer = 25
    Dim barWidth As Integer = 20
    Dim barHeight As Integer = 40

    Dim rect As New Rectangle(barX, barY, barWidth, barHeight)

    Public Event Fire(ByVal id As Integer, ByVal fire As Boolean)
    Public Event TrackBarChange(ByVal id As Integer, ByVal val As Integer, ByVal trace As Integer)
    Public Event SmoothProgressBarChange(ByVal id As Integer, ByVal val As Integer)

    Dim GTrackBarValue1 As Integer
    Dim GTrackBarValue2 As Integer

    Dim m_debug As Boolean = False '
    Public Property startDebug() As Boolean
        Get
            Return m_debug
        End Get
        Set(ByVal Value As Boolean)
            m_debug = Value
            If Not m_debug Then
                Me.SmoothProgressBar1.Value = 0
            End If


            Me.Refresh()
        End Set
    End Property

    Dim m_enable As Boolean = False '
    Public Property work() As Boolean
        Get
            Return m_enable
        End Get
        Set(ByVal Value As Boolean)
            m_enable = Value
            If m_enable Then
                Me.GTrackBar1.Enabled = True
                Me.GTrackBar2.Enabled = True
                Me.GTrackBar1.ColorUp = Color.Green
                Me.GTrackBar2.ColorUp = Color.Red
                Me.Label1.Enabled = True
                Me.Label2.Enabled = True
                'Me.GTrackBar1.Refresh()
            Else
                Me.GTrackBar1.Enabled = False
                Me.GTrackBar2.Enabled = False
                Me.GTrackBar1.ColorUp = Color.Gray
                Me.GTrackBar2.ColorUp = Color.Gray
                Me.GTrackBar1.Value = 0
                Me.GTrackBar2.Value = 255
                Me.Label1.Enabled = False
                Me.Label2.Enabled = False

                'Me.GTrackBar1.Refresh()
            End If
            Me.Refresh()

        End Set
    End Property

    Dim m_value As Integer = 0 '
    Public Property Value() As Integer
        Get
            Return m_value
        End Get
        Set(ByVal Value As Integer)
            m_value = Value
            Me.Label1.Text = m_value
        End Set
    End Property

    Dim m_base As Integer = 0 '
    Public Property BaseValue() As Integer
        Get
            Return m_base
        End Get
        Set(ByVal Value As Integer)
            m_base = Value
            Me.Label2.Text = m_base
        End Set
    End Property

    Dim m_index As Integer = 0 '
    Public Property Index() As Integer
        Get
            Return m_index
        End Get
        Set(ByVal Value As Integer)
            m_index = Value
            'Me.Label1.Text = m_index
        End Set
    End Property

    Dim m_object As Integer = -1 '
    Public Property ObjectId() As Integer
        Get
            Return m_object
        End Get
        Set(ByVal Value As Integer)
            m_object = Value
            'Me.Label1.Text = m_index
        End Set
    End Property

    Dim m_grouptag As Guid
    Public Property GroupTag() As Guid
        Get
            Return m_grouptag
        End Get
        Set(ByVal Value As Guid)
            m_grouptag = Value
        End Set
    End Property

    Dim m_monitor As Boolean = False '
    Public Property monitorSmoothProgressBarChange() As Boolean
        Get
            Return m_monitor
        End Get
        Set(ByVal Value As Boolean)
            m_monitor = Value
        End Set
    End Property

    Private Sub TKInput_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SmoothProgressBar1.Value = 0
        Me.m_value = 0
        Me.Timer1.Interval = 1
        'Me.Timer1.Enabled = True
        Me.GTrackBar1.Value = 0
        Me.GTrackBar2.Value = 255

    End Sub
    Private Sub TouchKey_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        Dim rect1 As New Rectangle(15, 130, 18, 18)

        If m_enable Then
            e.Graphics.DrawEllipse(blackPen, rect1)
            If m_debug Then
                If Me.SmoothProgressBar1.Value >= Me.GTrackBar1.Value And Me.SmoothProgressBar1.Value <= Me.GTrackBar2.Value Then
                    e.Graphics.FillEllipse(Brushes.Red, rect1)
                    If m_index < 10 Then
                        e.Graphics.DrawString(m_index, fon, blackBrush, 20, 132)
                    Else
                        e.Graphics.DrawString(m_index, fon, blackBrush, 17, 132)
                    End If

                    'e.Graphics.DrawString("on", fon, blackBrush, 18, 132)
                    'RaiseEvent Fire(m_index, True)
                    'Debug.Print("y")
                Else
                    e.Graphics.FillEllipse(Brushes.Orange, rect1)
                    If m_index < 10 Then
                        e.Graphics.DrawString(m_index, fon, blackBrush, 20, 132)
                    Else
                        e.Graphics.DrawString(m_index, fon, blackBrush, 17, 132)
                    End If

                    'e.Graphics.DrawString("off", fon, blackBrush, 15, 132)
                    'RaiseEvent Fire(m_index, False)
                    'Debug.Print("n")
                End If
            Else
                e.Graphics.DrawEllipse(Pens.Black, rect1)
                e.Graphics.FillEllipse(Brushes.LightGray, rect1)
                'e.Graphics.DrawString("off", fon, blackBrush, 15, 132)
                If m_index < 10 Then
                    e.Graphics.DrawString(m_index, fon, blackBrush, 20, 132)
                Else
                    e.Graphics.DrawString(m_index, fon, blackBrush, 17, 132)
                End If

            End If
            
        Else
            e.Graphics.DrawEllipse(grayPen, rect1)
            e.Graphics.FillEllipse(Brushes.LightGray, rect1)
            'e.Graphics.DrawString("off", fon, grayBrush, 15, 132)
            If m_index < 10 Then
                e.Graphics.DrawString(m_index, fon, grayBrush, 20, 132)
            Else
                e.Graphics.DrawString(m_index, fon, grayBrush, 17, 132)
            End If

        End If
        'Me.Invalidate()

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If m_debug And m_enable Then
            Me.Label1.Text = Me.SmoothProgressBar1.Value
            If (Me.SmoothProgressBar1.Value < m_value) Then
                Me.SmoothProgressBar1.StepValue = 10
                Me.SmoothProgressBar1.Stepit()
            ElseIf (Me.SmoothProgressBar1.Value > m_value) Then
                Me.SmoothProgressBar1.StepValue = -10
                Me.SmoothProgressBar1.Stepit()
            End If

            If Me.SmoothProgressBar1.Value <> m_value Then
                'Refresh()
                Invalidate()

                'If Me.SmoothProgressBar1.Value >= Me.GTrackBar1.Value And Me.SmoothProgressBar1.Value <= Me.GTrackBar2.Value Then
                '    RaiseEvent Fire(m_index, True)
                '    'Debug.Print("y")
                'Else
                '    RaiseEvent Fire(m_index, False)
                '    'Debug.Print("n")
                'End If
                If Me.SmoothProgressBar1.Value >= Me.GTrackBar2.Value Then
                    RaiseEvent Fire(m_index, True)
                    'Debug.Print("y")
                ElseIf Me.SmoothProgressBar1.Value < Me.GTrackBar1.Value Then
                    RaiseEvent Fire(m_index, False)
                    'Debug.Print("n")
                End If
            ElseIf Me.SmoothProgressBar1.Value = m_value Then
                Me.SmoothProgressBar1.Value = 0
            End If
            'Debug.Print(m_index)
            If m_monitor Then
                RaiseEvent SmoothProgressBarChange(m_index, Me.SmoothProgressBar1.Value)
            End If

        End If

        If m_enable Then
            Me.GTrackBar2.LowPos = Me.GTrackBar1.SliderPos
            'If Me.GTrackBar2.LowPos > Me.GTrackBar1.SliderPos Then
            '    Debug.Print("pos2 : " & Me.GTrackBar2.SliderPos & " pos1 :" & Me.GTrackBar1.SliderPos & " " & Me.GTrackBar2.LowPos)
            'Me.GTrackBar2.LowPos = Me.GTrackBar1.SliderPos
            '    'Me.GTrackBar2.Invalidate()
            'End If

            If Me.GTrackBar1.Value >= Me.GTrackBar2.Value Then
                Me.GTrackBar2.Value = Me.GTrackBar1.Value
            End If

            If Me.GTrackBar1.Value <> Me.GTrackBarValue1 Then
                Me.GTrackBarValue1 = Me.GTrackBar1.Value
                RaiseEvent TrackBarChange(m_index, Me.GTrackBarValue1, 1)
            End If
            If Me.GTrackBar2.Value <> Me.GTrackBarValue2 Then
                Me.GTrackBarValue2 = Me.GTrackBar2.Value
                RaiseEvent TrackBarChange(m_index, Me.GTrackBarValue2, 2)
            End If


            'If Me.GTrackBar2.SliderPos > Me.GTrackBar1.SliderPos Then
            'Me.GTrackBar2.Value = Me.GTrackBar1.Value
            'End If
        End If

    End Sub


End Class
