Public Class Line

    Dim m_usecursorasendobject As Boolean = False
    Public Property UseCursorAsEndObject() As Boolean 'gets or sets whether the end of the line should follow the mouse cursor
        Get
            Return m_usecursorasendobject
        End Get
        Set(ByVal Value As Boolean)
            If Value = True Then
                m_endobject = New Control
            End If

            m_usecursorasendobject = Value

            If m_endobject Is Nothing OrElse m_startobject Is Nothing Then
                Exit Property
            End If

            Refresh()
        End Set
    End Property

    Dim m_startobject As Control = Nothing 'the starting point of the line

    Public Property StartObject() As Control 'gets or sets the object that the line starts at
        Get
            Return m_startobject
        End Get
        Set(ByVal Value As Control)
            m_startobject = Value

            If m_endobject Is Nothing OrElse m_startobject Is Nothing Then
                Exit Property
            End If

            Refresh()
        End Set
    End Property

    Dim m_endobject As Control = Nothing 'the ending point of the line
    Public Property EndObject() As Control 'gets or sets the object that the line points to
        Get
            Return m_endobject
        End Get
        Set(ByVal Value As Control)
            m_endobject = Value

            UseCursorAsEndObject = False

            If m_endobject Is Nothing OrElse m_startobject Is Nothing Then
                Exit Property
            End If

            Refresh()
        End Set
    End Property

    Dim m_linecolor As Color = Color.Black 'the color of the line
    Public Property LineColor() As Color 'gets or sets the line color
        Get
            Return m_linecolor
        End Get
        Set(ByVal Value As Color)
            m_linecolor = Value
            Refresh()
        End Set
    End Property

    Dim m_ArrowBegin As System.Drawing.Drawing2D.AdjustableArrowCap 'the cap style of the start point
    Public Property ArrowBegin() As System.Drawing.Drawing2D.AdjustableArrowCap 'gets or sets the arrow cap of the start of the line
        Get
            Return m_ArrowBegin
        End Get
        Set(ByVal Value As System.Drawing.Drawing2D.AdjustableArrowCap)
            m_ArrowBegin = Value
            Refresh()
        End Set
    End Property

    Dim m_ArrowEnd As New System.Drawing.Drawing2D.AdjustableArrowCap(4, 5, True) 'the cap style of the end point
    Public Property ArrowEnd() As System.Drawing.Drawing2D.AdjustableArrowCap 'gets or sets the arrow cap of the end of the line
        Get
            Return m_ArrowEnd
        End Get
        Set(ByVal Value As System.Drawing.Drawing2D.AdjustableArrowCap)
            m_ArrowEnd = Value
            Refresh()
        End Set
    End Property

    Dim m_dashstyle As System.Drawing.Drawing2D.DashStyle 'the dash style of the line
    Public Property DashStyle() As System.Drawing.Drawing2D.DashStyle 'sets or gets the dash style of the line
        Get
            Return m_dashstyle
        End Get
        Set(ByVal Value As System.Drawing.Drawing2D.DashStyle)
            m_dashstyle = Value
            Refresh()
        End Set
    End Property

    Dim m_startpoint As Point 'the coordinates of the start point
    <System.ComponentModel.Browsable(False)> _
    Public Property StartPoint() As Point 'gets the point (x, y) of the start of the line
        Get
            Return m_startpoint
        End Get
        Set(ByVal Value As Point)
            m_startpoint = Value
            Refresh()
        End Set
    End Property

    Dim m_endpoint As Point 'the coordinates of the end point
    <System.ComponentModel.Browsable(False)> _
    Public Property EndPoint() As Point 'gets the point (x and y) of the end of the line
        Get
            Return m_endpoint
        End Get
        Set(ByVal Value As Point)
            m_endpoint = Value
            Refresh()
        End Set
    End Property

    Sub DrawLine(ByVal startP As Point, ByVal endP As Point)
        'store the starting and ending points of the line
        m_startpoint = startP
        m_endpoint = endP

        'shap the form according to the shape of the line
        Dim Path As New System.Drawing.Drawing2D.GraphicsPath
        Dim Pen As New Pen(Color.Black)

        If Not m_ArrowBegin Is Nothing Then 'if the user set the cap style of the start of the line
            Pen.CustomEndCap = m_ArrowBegin
        End If

        If Not m_ArrowEnd Is Nothing Then 'if the user set the cap style of the start of the line
            Pen.CustomEndCap = m_ArrowEnd
        End If

        Pen.DashStyle = m_dashstyle

        'draw the line
        Path.AddLine(startP, endP)
        Pen.Width = m_width
        Path.Widen(Pen)

        Me.BackColor = m_linecolor
        Me.Region = New Region(Path)
    End Sub

    Dim m_width As Integer = 1 'the width of the line
    Public Property LineWidth() As Integer
        Get
            Return m_width
        End Get
        Set(ByVal Value As Integer)
            m_width = Value
            Refresh()
        End Set
    End Property

    Public Overrides Sub Refresh() 'this function places the line in its proper place

        DrawLine(m_startpoint, m_endpoint)
        'Dim FormCheck As Integer = 30 'the least space between the two forms
        'Dim Offset As Integer = 4 'the space between the line and the starting and ending forms

        'If m_endobject Is Nothing OrElse m_startobject Is Nothing Then 'if no starting point or ending point for the line
        '    Exit Sub
        'End If

        'If Me.Parent Is Nothing Then 'if no parent for the form
        '    Exit Sub
        'End If

        'If UseCursorAsEndObject = True Then
        '    Dim Position As Point = MousePosition
        '    Position = Me.Parent.PointToClient(Position)

        '    EndObject.Top = Position.Y
        '    EndObject.Left = Position.X
        '    EndObject.Height = 20
        '    EndObject.Width = 10
        'End If

        'Me.BackColor = m_linecolor 'set the color of the line

        'If m_startobject.Top >= m_endobject.Top + m_endobject.Height Then 'Top Case
        '    If m_endobject.Left + m_endobject.Width <= m_startobject.Left Then 'Top Left
        '        If m_startobject.Top - (m_endobject.Top + m_endobject.Height) < FormCheck Then 'if height of form is less than FormCheck
        '            SetForm(m_startobject.Left - (m_endobject.Left + m_endobject.Width), (m_startobject.Top + m_startobject.Height / 2) - (m_endobject.Top + m_endobject.Height), m_endobject.Left + m_endobject.Width, m_endobject.Top + m_endobject.Height)
        '        ElseIf m_startobject.Left - (m_endobject.Left + m_endobject.Width) < FormCheck Then 'if width of form is less than FormCheck
        '            SetForm((m_startobject.Left + m_startobject.Width / 2) - (m_endobject.Left + m_endobject.Width), m_startobject.Top - (m_endobject.Top + m_endobject.Height), m_endobject.Left + m_endobject.Width, m_endobject.Top + m_endobject.Height)
        '        Else
        '            SetForm(m_startobject.Left - (m_endobject.Left + m_endobject.Width), m_startobject.Top - (m_endobject.Top + m_endobject.Height), m_endobject.Left + m_endobject.Width, m_endobject.Top + m_endobject.Height)
        '        End If

        '        DrawLine(New Point(Me.Width - Offset, Me.Height - Offset), New Point(Offset, Offset))
        '    ElseIf m_startobject.Left + m_startobject.Width <= m_endobject.Left Then 'Top Right
        '        If m_startobject.Top - (m_endobject.Top + m_endobject.Height) < FormCheck Then 'if height of form is less than FormCheck
        '            SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width), (m_startobject.Top + m_startobject.Height / 2) - (m_endobject.Top + m_endobject.Height), m_startobject.Left + m_startobject.Width, m_endobject.Top + m_endobject.Height)
        '        ElseIf m_endobject.Left - (m_startobject.Left + m_startobject.Width) < FormCheck Then 'if width of form is less than FormCheck
        '            SetForm((m_endobject.Left + m_endobject.Width / 2) - (m_startobject.Left + m_startobject.Width), m_startobject.Top - (m_endobject.Top + m_endobject.Height), m_startobject.Left + m_startobject.Width, m_endobject.Top + m_endobject.Height)
        '        Else
        '            SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width), m_startobject.Top - (m_endobject.Top + m_endobject.Height), m_startobject.Left + m_startobject.Width, m_endobject.Top + m_endobject.Height)
        '        End If

        '        DrawLine(New Point(Offset, Me.Height - Offset), New Point(Me.Width - Offset, Offset))
        '    Else 'Top Middle
        '        SetForm(Math.Max((m_startobject.Left + m_startobject.Width) - (m_endobject.Left), (m_endobject.Left + m_endobject.Width) - m_startobject.Left), m_startobject.Top - (m_endobject.Top + m_endobject.Height), Math.Min(m_endobject.Left, m_startobject.Left), m_endobject.Top + m_endobject.Height)
        '        Dim FormStartPoints As New Point(m_startobject.Left + (m_startobject.Width / 2), m_startobject.Top)
        '        Dim FormEndPoints As New Point(m_endobject.Left + (m_endobject.Width / 2), m_endobject.Top + m_endobject.Height)

        '        FormStartPoints = Me.Parent.PointToScreen(FormStartPoints) 'convert the points to screen coordinates
        '        FormStartPoints = Me.PointToClient(FormStartPoints) 'convert the points to client coordinates

        '        FormEndPoints = Me.Parent.PointToScreen(FormEndPoints) 'convert the points to screen coordinates
        '        FormEndPoints = Me.PointToClient(FormEndPoints) 'convert the points to client coordinates

        '        DrawLine(FormStartPoints, FormEndPoints)
        '    End If

        'ElseIf m_startobject.Top + m_startobject.Height <= m_endobject.Top Then 'Bottom Case
        '    If m_endobject.Left + m_endobject.Width <= m_startobject.Left Then 'Bottom Left
        '        If m_endobject.Top - (m_startobject.Top + m_startobject.Height) < FormCheck Then 'if height of form is less than FormCheck
        '            SetForm(m_startobject.Left - (m_endobject.Left + m_endobject.Width), (m_endobject.Top + m_endobject.Height / 2) - (m_startobject.Top + m_startobject.Height), m_endobject.Left + m_endobject.Width, m_startobject.Top + m_startobject.Height)
        '        ElseIf m_startobject.Left - (m_endobject.Left + m_endobject.Width) < FormCheck Then 'if width of form is less than FormCheck
        '            SetForm((m_startobject.Left + m_startobject.Width / 2) - (m_endobject.Left + m_endobject.Width), m_endobject.Top - (m_startobject.Top + m_startobject.Height), m_endobject.Left + m_endobject.Width, m_startobject.Top + m_startobject.Height)
        '        Else
        '            SetForm(m_startobject.Left - (m_endobject.Left + m_endobject.Width), m_endobject.Top - (m_startobject.Top + m_startobject.Height), m_endobject.Left + m_endobject.Width, m_startobject.Top + m_startobject.Height)
        '        End If

        '        DrawLine(New Point(Me.Width - Offset, Offset), New Point(Offset, Me.Height - Offset))
        '    ElseIf m_startobject.Left + m_startobject.Width <= m_endobject.Left Then 'Bottom Right
        '        If m_endobject.Top - (m_startobject.Top + m_startobject.Height) < FormCheck Then 'if height of form is less than FormCheck
        '            SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width), m_endobject.Top - (m_startobject.Top + m_startobject.Height / 2), m_startobject.Left + m_startobject.Width, m_startobject.Top + m_startobject.Height)
        '        ElseIf m_endobject.Left - (m_startobject.Left + m_startobject.Width) < FormCheck Then 'if width of form is less than FormCheck
        '            SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width / 2), m_endobject.Top - (m_startobject.Top + m_startobject.Height), m_startobject.Left + m_startobject.Width / 2, m_startobject.Top + m_startobject.Height)
        '        Else
        '            SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width), m_endobject.Top - (m_startobject.Top + m_startobject.Height), m_startobject.Left + m_startobject.Width, m_startobject.Top + m_startobject.Height)
        '        End If

        '        DrawLine(New Point(Offset, Offset), New Point(Me.Width - Offset, Me.Height - Offset))
        '    Else 'Bottom Middle
        '        SetForm(Math.Max((m_endobject.Left + m_endobject.Width) - (m_startobject.Left), (m_startobject.Left + m_startobject.Width) - m_endobject.Left), m_endobject.Top - (m_startobject.Top + m_startobject.Height), Math.Min(m_startobject.Left, m_endobject.Left), m_startobject.Top + m_startobject.Height)

        '        Dim FormStartPoints As New Point(m_startobject.Left + (m_startobject.Width / 2), m_startobject.Top + m_startobject.Height)
        '        Dim FormEndPoints As New Point(m_endobject.Left + (m_endobject.Width / 2), m_endobject.Top)

        '        FormStartPoints = Me.Parent.PointToScreen(FormStartPoints) 'convert the points to screen coordinates
        '        FormStartPoints = Me.PointToClient(FormStartPoints) 'convert the points to client coordinates

        '        FormEndPoints = Me.Parent.PointToScreen(FormEndPoints) 'convert the points to screen coordinates
        '        FormEndPoints = Me.PointToClient(FormEndPoints) 'convert the points to client coordinates

        '        DrawLine(FormStartPoints, FormEndPoints)
        '    End If

        'ElseIf m_startobject.Left >= m_endobject.Left + m_endobject.Width Then 'Left Case
        '    SetForm(m_startobject.Left - (m_endobject.Left + m_endobject.Width), Math.Max((m_endobject.Top + m_endobject.Height) - (m_startobject.Top), (m_startobject.Top + m_startobject.Height) - m_endobject.Top), m_endobject.Left + m_endobject.Width, Math.Min(m_startobject.Top, m_endobject.Top))

        '    Dim FormStartPoints As New Point(m_startobject.Left, m_startobject.Top + m_startobject.Height / 2)
        '    Dim FormEndPoints As New Point(m_endobject.Left + (m_endobject.Width), m_endobject.Top + m_endobject.Height / 2)

        '    FormStartPoints = Me.Parent.PointToScreen(FormStartPoints) 'convert the points to screen coordinates
        '    FormStartPoints = Me.PointToClient(FormStartPoints) 'convert the points to client coordinates

        '    FormEndPoints = Me.Parent.PointToScreen(FormEndPoints) 'convert the points to screen coordinates
        '    FormEndPoints = Me.PointToClient(FormEndPoints) 'convert the points to client coordinates

        '    DrawLine(FormStartPoints, FormEndPoints)
        'ElseIf m_startobject.Left + m_startobject.Width <= m_endobject.Left Then 'Right Case

        '    SetForm(m_endobject.Left - (m_startobject.Left + m_startobject.Width), Math.Max((m_endobject.Top + m_endobject.Height) - (m_startobject.Top), (m_startobject.Top + m_startobject.Height) - m_endobject.Top), m_startobject.Left + m_startobject.Width, Math.Min(m_startobject.Top, m_endobject.Top))

        '    Dim FormStartPoints As New Point(m_startobject.Left + (m_startobject.Width), m_startobject.Top + m_startobject.Height / 2)
        '    Dim FormEndPoints As New Point(m_endobject.Left, m_endobject.Top + m_endobject.Height / 2)

        '    FormStartPoints = Me.Parent.PointToScreen(FormStartPoints) 'convert the points to screen coordinates
        '    FormStartPoints = Me.PointToClient(FormStartPoints) 'convert the points to client coordinates

        '    FormEndPoints = Me.Parent.PointToScreen(FormEndPoints) 'convert the points to screen coordinates
        '    FormEndPoints = Me.PointToClient(FormEndPoints) 'convert the points to client coordinates

        '    DrawLine(FormStartPoints, FormEndPoints)
        'Else
        '    SetForm(0, 0, 0, 0)
        'End If
    End Sub

    'sets the control width and height and position in its proper place
    Sub SetForm(ByVal width As Integer, ByVal height As Integer, ByVal left As Integer, ByVal top As Integer)
        Me.Left = left
        Me.Top = top

        Me.Height = height
        Me.Width = width
    End Sub

    Shadows Event MouseMove(ByVal sender As Object)
    Shadows Event MouseLeave(ByVal sender As Object)

    'mouse leave event
    Private Sub Line_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.MouseLeave
        RaiseEvent MouseLeave(Me)
    End Sub

    'mouse move event
    Private Sub Line_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseMove
        RaiseEvent MouseMove(Me)
    End Sub

    Public Sub StartFlashLine(ByVal FColor As Color) 'start the timer to flash the line
        FlashColor = FColor
        OriginalColor = Me.LineColor
        Me.LineColor = FColor
        Refresh()
        FlashingTimer.Enabled = True
    End Sub

    Public Sub EndFlashLine() 'returns the line to its original color and stop the flashing
        Me.LineColor = OriginalColor
        Refresh()

        FlashingTimer.Enabled = False
        Me.LineColor = OriginalColor
    End Sub

    Dim OriginalColor As Color 'original color of the line
    Dim FlashColor As Color 'the flashing color of the line

    'the timer to alternate between the original color and the flashing color
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FlashingTimer.Tick
        If Me.LineColor.ToString = OriginalColor.ToString Then
            Me.LineColor = FlashColor
        Else
            Me.LineColor = OriginalColor
        End If
    End Sub

    ReadOnly Property IsFlashing() As Boolean 'returns whether the line is flashing or not
        Get
            Return FlashingTimer.Enabled
        End Get
    End Property

    ReadOnly Property FlashingColor() As Color 'returns the current flashing color
        Get
            Return FlashColor
        End Get
    End Property
End Class
