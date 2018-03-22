Imports System.ComponentModel
Public Class SmoothProgressBar
    Inherits System.Windows.Forms.UserControl
#Region " Private variables "

    Private min As Integer = 0                ' Minimum value for progress range
    Private max As Integer = 100              ' Maximum value for progress range
    Private val As Integer = 0                ' Current progress
    Private barColor As Color = Color.Blue    ' Color of progress meter
    Private isHorz As Boolean = True          ' Progresses horizontally / vertically
    Private isRevs As Boolean = False         ' Reverses direction of fill progression
    Private mBorderstyle As BorderStyle = BorderStyle.Fixed3D
    Private mStep As Integer = 1              ' Increment value for Step() method

#End Region

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim brush As SolidBrush = New SolidBrush(barColor)
        Dim percent As Decimal = (val - min) / (max - min)
        Dim rect As Rectangle = Me.ClientRectangle



        ' Calculate area for drawing the progress.
        With Me.ClientRectangle
            If isHorz Then
                rect.Width = rect.Width * percent
                If isRevs Then rect.X = .Width - rect.Width
            Else
                rect.Height = rect.Height * percent
                If Not isRevs Then rect.Y = .Height - rect.Height

            End If
        End With

        ' Draw the progress meter.
        g.FillRectangle(brush, rect)

        ' Draw a border around the control.
        If Me.BorderStyle <> BorderStyle.None Then DrawBorder(g)





        ' Clean up.
        brush.Dispose()
        g.Dispose()
    End Sub

    Protected Overrides Sub OnResize(ByVal e As EventArgs)
        ' Invalidate the control to get a repaint.
        Me.Invalidate()
    End Sub

    Public Sub Stepit()
        Me.Value += mStep
    End Sub

    Public Sub Reverseit()
        Me.Value -= mStep
    End Sub

    Private Sub DrawBorder(ByVal g As Graphics)
        Dim PenWidth As Integer = Pens.White.Width
        Dim PenDG As Pen = New Pen(SystemColors.ControlDark, 1.0F)
        Dim PenWh As Pen = New Pen(SystemColors.ControlLightLight, 1.0F)

        If Me.BorderStyle = BorderStyle.FixedSingle Then
            PenDG = Pens.Black
            PenWh = Pens.Black
        End If

        With Me.ClientRectangle
            g.DrawLine(PenDG, New Point(.Left, .Top), New Point(.Width - PenWidth, .Top))
            g.DrawLine(PenDG, New Point(.Left, .Top), New Point(.Left, .Height - PenWidth))
            g.DrawLine(PenWh, New Point(.Left, .Height - PenWidth), _
               New Point(.Width - PenWidth, .Height - PenWidth))
            g.DrawLine(PenWh, New Point(.Width - PenWidth, .Top), _
               New Point(.Width - PenWidth, .Height - PenWidth))
        End With
    End Sub

#Region " Public Properties "

    <Category("Behavior"), _
    DefaultValue(True), _
    Description("Progresses horizontally when True, vertically when False.")> _
    Public Property Horizontal() As Boolean
        Get
            Return isHorz
        End Get
        Set(ByVal Value As Boolean)
            If Value <> isHorz Then
                Dim tmp As Integer = Me.Width
                Me.Width = Me.Height
                Me.Height = tmp
            End If
            isHorz = Value
        End Set
    End Property

    <Category("Behavior"), _
    DefaultValue(False), _
    Description("Reverses direction of fill progression when True.")> _
    Public Property Reverse() As Boolean
        Get
            Return isRevs
        End Get
        Set(ByVal Value As Boolean)
            isRevs = Value
            Me.Invalidate()
        End Set
    End Property

    <Category("Behavior"), _
    DefaultValue(0), _
    Description("The lower bound of the range this ProgressBar is working with.")> _
    Public Property Minimum() As Integer
        Get
            Return min
        End Get

        Set(ByVal Value As Integer)
            ' Prevent a negative value.
            If Value < 0 Then Value = 0
            min = Value
            ' Make sure that the minimum value is never set >= the maximum value.
            If min >= max Then max = min + 1

            ' Make sure that the value is still in range.
            If val < min Then val = min

            ' Invalidate the control to get a repaint.
            Me.Invalidate()
        End Set
    End Property

    <Category("Behavior"), _
    DefaultValue(100), _
    Description("The upper bound of the range this ProgressBar is working with.")> _
    Public Property Maximum() As Integer
        Get
            Return max
        End Get

        Set(ByVal Value As Integer)
            ' Make sure that the maximum value is never set <= the minimum value.
            If Value <= min Then
                max = min + 1
            Else
                max = Value
            End If

            ' Make sure that the value is still in range.
            If val > max Then val = max

            ' Invalidate the control to get a repaint.
            Me.Invalidate()
        End Set
    End Property

    <Category("Behavior"), _
    DefaultValue(0), _
    Description("The current value for the ProgressBar, in the range specified by the minimum and maximum properties.")> _
    Public Property Value() As Integer
        Get
            Return val
        End Get

        Set(ByVal Value As Integer)
            Dim oval As Integer = val

            ' Make sure that the value does not stray outside the valid range.
            If Value < min Then
                val = min
            ElseIf Value > max Then
                val = max
            Else
                val = Value
            End If

            Dim dm, dv, mm, mx As Integer
            dm = max - min
            dv = Math.Abs(val - oval)
            mx = Math.Max(val, oval)
            mm = Math.Min(val, oval)
            Dim r As Rectangle

            With Me.ClientRectangle
                If isHorz Then
                    If isRevs Then
                        r = New Rectangle(.Width - mx * .Width / dm, 0, dv * .Width / dm, .Height)
                        'r.X = .Width - r.X
                    Else
                        r = New Rectangle(mm * .Width / dm, 0, dv * .Width / dm, .Height)
                    End If
                Else
                    If isRevs Then
                        r = New Rectangle(0, mm * .Height / dm, .Width, dv * .Height / dm)
                    Else
                        r = New Rectangle(0, .Height - mx * .Height / dm, .Width, dv * .Height / dm)
                    End If
                End If
            End With

            'Invalidate only the changed area.
            'Dim g As Graphics = Me.CreateGraphics
            'Dim brush As SolidBrush = New SolidBrush(Color.Red)
            'g.FillRectangle(brush, r)
            'For z As Integer = 0 To 1000000 : Next
            'g.Dispose()
            'brush.Dispose()
            Me.Invalidate(r)
        End Set
    End Property

    <Category("Behavior"), _
    DefaultValue(1), _
    Description("The amount to increment the current value of the control by when the Stepit() method is called.")> _
    Public Property StepValue() As Integer
        Get
            Return mStep
        End Get
        Set(ByVal Value As Integer)
            mStep = Value
        End Set
    End Property


    <Category("Appearance"), _
    DefaultValue(GetType(Color), "Blue"), _
    Description("The foreground color used to display the current progress value of the ProgressBar.")> _
    Public Property ProgressBarColor() As Color
        Get
            Return barColor
        End Get

        Set(ByVal Value As Color)
            barColor = Value

            ' Invalidate the control to get a repaint.
            Me.Invalidate()
        End Set
    End Property

    <Category("Appearance"), _
    DefaultValue(GetType(BorderStyle), "Fixed3D"), _
    Description("Indicates whether or not the ProgressBar should have a border.")> _
    Public Overloads Property BorderStyle() As BorderStyle
        Get
            Return mBorderstyle
        End Get
        Set(ByVal Value As BorderStyle)
            mBorderstyle = Value
            Me.Invalidate()
        End Set
    End Property

    '<Browsable(False)> _
    'Public Shadows ReadOnly Property ForeColor() As Color
    '    Get
    '        Return Me.ForeColor
    '    End Get
    'End Property

    '<Browsable(False)> _
    'Public Shadows ReadOnly Property Font() As Font
    '    Get
    '        Return Me.Font
    '    End Get
    'End Property

#End Region

End Class
