Imports System.Xml
Imports System.ComponentModel
Imports System.Globalization
Imports System.Reflection
Imports system.io
Imports System.Runtime.InteropServices
Imports System.Threading
Imports ztlib.zt_intf


Public Class TKtoolkit

#Region "Declarations"
    Dim hDevice As Integer
    '======================= Generate the last K =========================
    Dim bLastK(128 * 8 - 1) As Byte
    Dim startingAddr As UShort = &H3C00S
    Dim u2c_result As U2C_RESULT
    Dim t As Thread

    Private Saved As Boolean
    Private Dragging As Boolean
    Private mousex As Integer
    Private mousey As Integer
    Private topy As Integer
    Private leftx As Integer
    Private bottomy As Integer
    Private pinNumber As Integer = 0
    Private keyNumber As Integer = 0
    Private sliderNumber As Integer = 0
    Private rotatorNumber As Integer = 0
    Private keyIndex As Integer = 0
    Private sliderIndex As Integer = 0
    Private rotatorIndex As Integer = 0
    Private ObjectIndex As Integer = 0

    'Private mKeyCollection As New SortedList
    'Private mGroupCollection As New SortedList


    Private controlList As ArrayList
    Private fileName As String

    Private Const maxGroupNumber As Integer = 3
    Private Const maxPinNumber As Integer = 28
    Private Const reservedPinNumber As Integer = 3
    Private Const MyName As String = "Zeitec Touch Key Toolkit"

    Dim TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    'Dim ApplyButton As System.Windows.Forms.Button
    Dim TK2GROUP(maxPinNumber - 1) As Guid


    Public Enum Language
        English
        TraditionalChinese
        SimplifiedChinese
    End Enum

    Public Enum KeyType
        key
        bar
        pie
    End Enum

    Public Enum GroupType
        key
        slider
        rotor
    End Enum

    Public Enum ControlType
        follow
        toggle
        oneshot
    End Enum

    Public Enum Direction
        none
        left
        right
        clockwise
        counterclockwise
    End Enum

    Public Enum GroupValue
        none
        absolute
        relative
    End Enum

    Public Enum PortStatus
        unavailable
        available
        used
    End Enum

    Public Enum ValueType
        absolute
        relative
    End Enum

    <Serializable()> _
    Private Structure inputData
        Public smonitorSmoothProgressBarChange As Boolean
        Public groupTag As Guid
        Public objectId As Integer
        Public index As Integer
        Public baseValue As Integer
        Public value As Integer
        Public work As Boolean
        Public startDebug As Boolean

        Public traceValue1 As Integer
        Public traceValue2 As Integer
        Public barValue As Integer
        Public timer As Boolean
    End Structure

    <Serializable()> _
    Private Structure keyData
        Public theGroupId As Guid
        Public theId As Guid
        Public theDisplayName As String
        Public theIndex As Integer
        Public theType As KeyType
        Public theobjectId As Integer
        Public theLocation As Point
        Public theSize As Size

        Public SensorPort As Integer
        Public MappingPort As Integer
        Public ControlType As ControlType
        Public Sensitivity As Integer
        Public SensitivityAna As Integer
        Public SensitivityDig As Integer
        Public NoiseFilter As Integer
        Public DeglitchCount As Integer
        Public Tiggerport As Integer
        Public MapPortInit As Integer
        Public ThresholdHigh As Integer
        Public ThresholdLow As Integer
    End Structure

    <Serializable()> _
    Private Structure GroupData
        Public theId As Guid
        Public theText As String
        Public theName As String
        Public theIndex As Integer
        Public theType As KeyType
        Public theobjectId As Integer
        Public theLocation As Point
        Public theSize As Size
        Public theCount As Integer

        Public ActiveDir As Direction
        Public MapPWM As Integer
        Public ValueType As GroupValue
        Public StepValue As Integer
        Public StartValue As Integer
        Public StopValue As Integer
        Public Interpolation As Integer
        Public Sensitivity As Integer
        Public SpeedVector As Integer
        Public TriggerPort As Integer
        Public NoiseFilter As Integer

    End Structure

    <Serializable()> _
    Private Structure ProjectData
        Public IC_model As String
        Public Scan_type As String
        Public LVD_level As String
        Public LVR_level As String
        Public Work_Freq As String
        Public Show_port As String

        Public keyIndex As Integer
        Public sliderIndex As Integer
        Public rotorIndex As Integer
        Public keyNumber As Integer
        Public sliderNumber As Integer
        Public rotorNumber As Integer

    End Structure

    Private Structure RomHeader
        Public ic_model As Byte
        Public scan_type As Byte
        Public LVD_level As Byte
        <VBFixedArray(13), MarshalAs(UnmanagedType.ByValArray, SizeConst:=13)> Public Spare() As Byte
        Public Sub Initialize()
            ReDim Spare(12)
        End Sub
    End Structure

    Private Structure RomKey
        Public key_type As Byte
        Public drive_io As Byte
        Public sensing_io As Byte
        Public map_io As Byte
        Public ctrl_type As Byte
        Public sensitivity_ana As Byte
        Public sensitivity_dgt As Byte
        Public noise_level As Byte
        Public deglitch_count As Byte
        'Public map_io_init As Byte
        Public threshold_high As Byte
        Public threshold_low As Byte
    End Structure

    Private Structure RomGroup
        Public key_type As Byte
        Public key_count As Byte
        Public direction As Byte
        <VBFixedArray(12), MarshalAs(UnmanagedType.ByValArray, SizeConst:=12)> Public io_no() As Byte
        Public Sub Initialize_iono()
            ReDim io_no(11)
        End Sub
        Public map_pwm As Byte
        Public slider_type As Byte
        <VBFixedArray(2), MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)> Public start_value() As Byte
        Public Sub Initialize_startvalue()
            ReDim start_value(1)
        End Sub
        <VBFixedArray(2), MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)> Public stop_value() As Byte
        Public Sub Initialize_stopvalue()
            ReDim stop_value(1)
        End Sub
        Public speed_vector As Byte
        Public interpol As Byte
    End Structure


    'Dim PIN(maxPinNumber - 1) As pinData
    Dim availableObject(maxPinNumber - 1) As Boolean
    Dim availableKeyGroup(maxPinNumber - 1) As Boolean
    Dim availableSliderGroup(maxPinNumber - 1) As Boolean
    Dim availableRotorGroup(maxPinNumber - 1) As Boolean
    Dim myTKInputArray(maxPinNumber - 1) As TKInput
    Dim pp As ProjectProperties = New ProjectProperties
    Dim pjd As ProjectData = New ProjectData

    'port table
    Public maxPort As Integer = maxPinNumber + reservedPinNumber - 1
    Public IC() As String = New String() {"ZET8234WMA", "ZET8234WLA", "ZET8234VGA"}
    Public SCAN() As String = New String() {"Self", "Mutual"}
    Public FREQUENCE() As String = New String() {"12MHz", "8MHz", "4MHz"}
    Public LVD() As String = New String() {"2.1V", "2.4V", "2.7V", "3.0V"}
    Public LVR() As String = New String() {"2.0V", "2.3V", "2.6V", "2.9V"}


    Public P(IC.Length, maxPort) As Integer
    Public TPGT(IC.Length, maxPort) As Integer
    'group table
    Private groupTable(maxGroupNumber - 1) As GroupData

    Dim smallKeylocation As New Point(10, 30)
    Dim bigKeyLocation As New Point(16, 20)
    Dim smallKeyRect As New Rectangle(0, 0, 50, 50)
    Dim bigKeyRect As New Rectangle(0, 0, 70, 70)


    Dim smallBarRect As New Rectangle(0, 0, 20, 30)
    Dim bigBarRect As New Rectangle(0, 0, 20, 60)

    Dim RM As Resources.ResourceManager
    Dim CI As CultureInfo


#End Region
    'end of Declarations


#Region "Drag and Drop - Mouse Handlers"
    Public Sub MyMouseHover(ByVal sender As Object, ByVal e As System.EventArgs)
        'Debug.Print("hover")
        If TypeOf sender Is TouchKey Then
            CType(sender, TouchKey).Hover = True
        ElseIf TypeOf sender Is TouchBar Then
            CType(sender, TouchBar).Hover = True
        ElseIf TypeOf sender Is TouchPie Then
            CType(sender, TouchPie).Hover = True
        End If

        sender.invalidate()

    End Sub

    Public Sub MyMouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        'Debug.Print("leave")
        If TypeOf sender Is TouchKey Then
            CType(sender, TouchKey).Hover = False
        ElseIf TypeOf sender Is TouchBar Then
            CType(sender, TouchBar).Hover = False
        ElseIf TypeOf sender Is TouchPie Then
            CType(sender, TouchPie).Hover = False
        End If
        sender.invalidate()
    End Sub

    Public Sub MyMouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        'Debug.Print("hover")
        If TypeOf sender Is TouchKey Then
            CType(sender, TouchKey).Hover = True
        ElseIf TypeOf sender Is TouchBar Then
            CType(sender, TouchBar).Hover = True
        ElseIf TypeOf sender Is TouchPie Then
            CType(sender, TouchPie).Hover = True
        End If

        sender.invalidate()

    End Sub

    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
    '    ToolStripButton7_Click(sender, e)
    'End Sub

    ' The handler for the MouseClick event
    Public Sub MyMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        ' Find out if it is in Drag and Drop mode
        REM  If ToolStrip1.Visible = True Then

        'Prosedure to move an image from the workspace. Using Mousebutton right
        If e.Button = Windows.Forms.MouseButtons.Right Then

            'Dim Response As MsgBoxResult = MsgBox("Do you want to remove this object", MsgBoxStyle.YesNo, "Id = " & sender.tag)
            'If Response = MsgBoxResult.Yes Then   ' User chose Yes.
            ''Remove from workspace
            'Me.Controls.Remove(sender)

            'End If

        End If

        'Prosedure to select the image
        If e.Button = Windows.Forms.MouseButtons.Left Then

            Me.Cursor = Cursors.Hand
            Dragging = True
            mousex = -e.X
            mousey = -e.Y
            Dim clipleft As Integer = Me.Panel1.PointToClient(MousePosition).X - sender.Location.X
            Dim cliptop As Integer = Me.Panel1.PointToClient(MousePosition).Y - sender.Location.Y
            Dim clipwidth As Integer = Me.Panel1.ClientSize.Width - (sender.Width - clipleft)
            Dim clipheight As Integer = Me.Panel1.ClientSize.Height - (sender.Height - cliptop)
            Windows.Forms.Cursor.Clip = Me.Panel1.RectangleToScreen(New Rectangle(clipleft, cliptop, clipwidth, clipheight))
            sender.Invalidate()

        End If
        REM  End If
    End Sub

    ' The handler for the MouseMove event
    Public Sub MyMouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        ' Find out if it is in Drag and Drop mode
        REM If ToolStrip1.Visible = True Then
        If Dragging Then
            'move control to new position
            Dim MPosition As New Point()
            MPosition = Me.Panel1.PointToClient(MousePosition)
            MPosition.Offset(mousex, mousey)
            'ensure control cannot leave container

            'If leftx > MPosition.X Or topy > MPosition.Y Or MPosition.Y + 70 > bottomy Then
            'Exit Sub
            'End If
            sender.Location = MPosition
        End If
        REM End If


    End Sub

    ' The handler for the MouseUp event
    Private Sub MyMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        ' Find out if it is in Drag and Drop mode
        REM If ToolStrip1.Visible = True Then

        If Dragging Then
            'After dragging update the database with the new position X and Y

            'End the dragging
            Dragging = False
            Windows.Forms.Cursor.Clip = Nothing
            sender.Invalidate()
        End If

        Me.Cursor = Cursors.Arrow
        REM End If
    End Sub
#End Region
    'end of Mouse Handlers

#Region "form - controls"

    Private Sub TKtoolkit_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ztCloseDevice(hDevice)
    End Sub

    Private Sub TKtoolkit_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If IsNothing(t) = False Then
            If (t.IsAlive = True) Then
                If hDevice <> &H0 And hDevice <> &HFFFFFFFF Then
                    ztCloseDevice(hDevice)
                End If

                t.Abort()
            End If
        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        hDevice = ztOpenDevice()

        RM = New Resources.ResourceManager("TKtoolkit.Resource", Assembly.GetExecutingAssembly)


        CI = New CultureInfo("zh-TW")
        'CI = New CultureInfo("en-US")
        'CI = New CultureInfo("zh-CN")
        Threading.Thread.CurrentThread.CurrentUICulture = CI

        Me.Label1.Text = RM.GetString("StillHave") & maxPinNumber & RM.GetString("KeyAvailable")
        ProjctApply.Text = RM.GetString("Apply")
        ApplyButton.Text = RM.GetString("Apply")

        ' Me.Label1.Text = "尚用" & maxPinNumber & "個key可使用"

        'Me.BackColor = Color.White
        Me.Text = MyName
        Me.StartPosition = FormStartPosition.CenterScreen
        'Me.Width = 800
        'Me.Height = 600
        Me.ToolStripButton5.Enabled = False
        Me.ToolStripButton1.Enabled = True


        'setting layout
        Me.MenuStrip1.Width = Me.Width
        Me.MenuStrip1.Refresh()
        Me.ToolStrip1.Width = Me.Width
        Me.ToolStrip1.Refresh()
        Me.StatusStrip1.Width = Me.Width
        Me.StatusStrip1.Refresh()

        'topy = Me.MenuStrip1.Height + Me.ToolStrip1.Height
        'leftx = Me.SplitContainer1.SplitterDistance
        ' bottomy = Me.Height - Me.StatusStrip1.Height
        'Me.SplitContainer1.Width = Me.Width
        Me.SplitContainer1.SplitterDistance = 16 * Me.Height / 30
        Me.SplitContainer1.Refresh()
        'Debug.Print("a:" & Me.SplitContainer1.Panel1.Height & " b:" & Me.SplitContainer1.SplitterDistance & "c " & Me.SplitContainer1.Panel2.Height)


        'TKInput layout
        Dim i As Integer
        For i = 0 To maxPinNumber - 1
            availableObject(i) = False
            availableKeyGroup(i) = False
            availableSliderGroup(i) = False
            availableRotorGroup(i) = False

            Dim MyInput As New TKInput
            'Me.SplitContainer1.Panel1.Controls.Add(MyInput)
            Me.FlowLayoutPanel1.Controls.Add(MyInput)
            'MyInput.Location = New Point(200, 200)
            MyInput.BringToFront()
            MyInput.Index = (maxPinNumber - 1) - i ' if change here, the order of key data structure in rom will be reversed.0,1,2... > ...2,1,0
            MyInput.work = False
            MyInput.Timer1.Enabled = False
            myTKInputArray(MyInput.Index) = MyInput
            'Me.SplitContainer1.Panel1.Refresh()
            AddHandler MyInput.Fire, AddressOf onFire
            AddHandler MyInput.TrackBarChange, AddressOf onTrackBarChange
            AddHandler MyInput.SmoothProgressBarChange, AddressOf onSmoothProgressBarChange
        Next


        Dim title As New Label
        'Me.ApplyButton = New System.Windows.Forms.Button
        'Me.ApplyButton.Text = "Apply"
        'Me.ApplyButton.Size = New Size(50, 25)

        'Me.FlowLayoutPanel1.Controls.Add(ApplyButton)

        'TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
        'Me.TableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None
        'Me.TableLayoutPanel1.ColumnCount = 1
        'Me.TableLayoutPanel1.RowCount = 2
        'Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        ''Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        'Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        'Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))

        'Me.TableLayoutPanel1.Controls.Add(title, 0, 0)
        'Me.TableLayoutPanel1.Controls.Add(ApplyButton, 0, 1)

        'Me.FlowLayoutPanel1.Controls.Add(Me.TableLayoutPanel1)


        ' Me.FlowLayoutPanel1.Controls.Add(title)
        title.Text = "輸入源"
        title.Size = New Size(20, 50)
        title.BringToFront()
        title.Visible = True


        Me.FlowLayoutPanel1.Refresh()
        Me.ToolStripButton10.Enabled = False

        Me.ApplyButton.Enabled = False
        Me.ProjctApply.Enabled = False

        pjd.IC_model = IC(0)
        pjd.Scan_type = SCAN(0)
        pjd.Work_Freq = FREQUENCE(0)
        pjd.LVD_level = LVD(0)
        pjd.LVR_level = LVR(0)
        pjd.Show_port = "False"


        pp.IC_model = pjd.IC_model
        pp.Scan_type = pjd.Scan_type
        pp.Work_Freq = pjd.Work_Freq
        pp.LVD_level = pjd.LVD_level
        pp.LVR_level = pjd.LVR_level
        pp.Show_Port = pjd.Show_port
        Me.PropertyGrid1.SelectedObject = pp

        initialICTable()

        'Dim l As New Line
        'Me.Panel1.Controls.Add(l)
        'l.Location = New Point(100, 100)
        'l.BringToFront()
        'l.StartPoint = New Point(10, 0)
        'l.EndPoint = New Point(10, 20)
        'l.LineColor = Color.White


    End Sub

    Private Sub TKtoolkit_Resize(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Resize
        On Error Resume Next
        Me.MenuStrip1.Width = Me.Width
        Me.ToolStripContainer1.Width = Me.Width
        Me.ToolStripContainer1.Height = Me.Height - 32
        Me.ToolStrip1.Width = Me.Width
        Me.ToolStrip1.Refresh()
        Me.MenuStrip1.Refresh()
        Me.ToolStripContainer1.Refresh()

        REM Debug.Print(Me.Width)

        Me.SplitContainer1.Panel2.Height = 11 * (Me.Height) / 25
        Me.SplitContainer1.Panel2.Width = Me.Width
        Debug.Print("a " & Me.SplitContainer1.Panel2.Width & " f " & Me.Width)
        Me.SplitContainer1.SplitterDistance = 16 * Me.Height / 30
        Me.SplitContainer1.Refresh()

        Me.FlowLayoutPanel1.Height = Me.SplitContainer1.Panel2.Height - 58
        Me.FlowLayoutPanel1.Width = Me.Width
        Debug.Print(Me.FlowLayoutPanel1.Width)
        Me.FlowLayoutPanel1.Refresh()


    End Sub

    Private Sub SplitContainer1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles SplitContainer1.Resize
        Debug.Print("resize")
        Me.FlowLayoutPanel1.Height = Me.SplitContainer1.Panel2.Height - 58
    End Sub

    Private Sub SplitContainer1_SplitterMoved(ByVal sender As Object, ByVal e As System.Windows.Forms.SplitterEventArgs) Handles SplitContainer1.SplitterMoved
        Me.FlowLayoutPanel1.Height = Me.SplitContainer1.Panel2.Height - 58
    End Sub


    Private Sub 檔案ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 檔案ToolStripMenuItem.Click

    End Sub

    Private Sub 新增ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 新增ToolStripMenuItem.Click

    End Sub

    'Private Sub 專案ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 專案ToolStripMenuItem.Click

    'End Sub

    Private Sub 專案ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 專案ToolStripMenuItem1.Click

    End Sub

    'Private Sub 執行ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 執行ToolStripMenuItem.Click
    '    Debug.Print("debug")

    'End Sub


    Private Sub ToolStripButton10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton10.Click


        If hDevice = &H0 Or hDevice = &HFFFFFFFF Then
            hDevice = ztOpenDevice()
        End If

        Me.ToolStripButton10.Enabled = False

        'Dim fs As New FileStream("Test.hex", FileMode.OpenOrCreate)
        'Dim w As New BinaryWriter(fs)
        'Dim s As String = ":00000001FF"
        ''This would be: Hello World! in hex
        'w.Write(s)
        'w.Close()
        'fs.Close()
        'Dim b As Byte = &H5E
        'Console.WriteLine((b And &HF).ToString("X"))    ' displays "E" (low nibble)
        'Console.WriteLine((b >> 4).ToString("X"))       ' displays "5" (high nibble)
        'Dim b As Byte = &HA3
        'Dim c As Byte
        'c = CByte(0)
        'MsgBox(c.ToString("x2"))
        'MsgBox(Marshal.SizeOf(header) & " " & Marshal.SizeOf(header.ic_model) & " " & header.Spare.Length)
        generateHex()


    End Sub

    Private Sub generateHex()
        Dim RomDataArrarySize = maxPinNumber + maxGroupNumber - 1
        Dim RomDataArray(RomDataArrarySize) As Object
        Dim n, m As Integer
        For n = 0 To RomDataArrarySize
            If n < maxPinNumber Then
                Dim key As RomKey = New RomKey
                key.key_type = &HFF
                key.drive_io = &HFF
                key.sensing_io = &HFF
                key.map_io = &HFF
                key.ctrl_type = &HFF
                key.sensitivity_ana = &HFF
                key.sensitivity_dgt = &HFF
                key.noise_level = &HFF
                key.deglitch_count = &HFF
                key.threshold_high = &HFF
                key.threshold_low = &HFF
                RomDataArray(n) = key
            Else
                Dim group As RomGroup = New RomGroup
                group.Initialize_iono()
                group.Initialize_startvalue()
                group.Initialize_stopvalue()
                group.key_type = &HFF
                group.key_count = &H0
                group.direction = &H1
                For m = 0 To group.io_no.Length - 1
                    group.io_no(m) = &HFF
                Next
                group.map_pwm = &HFF
                group.slider_type = &H1
                For m = 0 To group.start_value.Length - 1
                    group.start_value(m) = &H0
                Next
                For m = 0 To group.stop_value.Length - 1
                    group.stop_value(m) = &H0
                Next
                group.speed_vector = &HFF
                group.interpol = &HFF
                RomDataArray(n) = group
            End If
        Next

        getRomData(RomDataArray)

        Dim fstream As FileStream
        'If f.Exists("test.hex") Then
        'fstream = New FileStream("test.hex", FileMode.Truncate, FileAccess.ReadWrite)
        'Else
        fstream = New FileStream("sample.hex", FileMode.Create, FileAccess.ReadWrite)
        'End If
        Dim sWriter As New StreamWriter(fstream)
        sWriter.BaseStream.Seek(0, SeekOrigin.Begin)
        'sWriter.WriteLine(":" & c.ToString("X").PadLeft(2, "0"))
        'sWriter.WriteLine(":061F400071600205712230")
        'sWriter.WriteLine(":00000001FF")

        '3C00-3E00 = &HFF
        ' header
        Dim startAddressReserved As Integer = &H3C00
        Dim recordlengthFixed As Integer = &H10

        Dim startAddress As Integer = &H3E00
        Dim recordLength As Integer = &H0
        'Dim Address() As Byte = {&H3D, &HFF}
        Dim recordType As Byte = &H0
        Dim checkSum As Integer = &H0
        Dim header As RomHeader = New RomHeader
        header.Initialize()

        'MsgBox(startAddress)
        'MsgBox(CByte(startAddress >> 8).ToString("x2"))
        'MsgBox(CByte(startAddress And &HFF).ToString("x2"))

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''reserved
        Dim j As Integer
        While (startAddress > startAddressReserved)
            If startAddress - startAddressReserved < &H10 Then
                Dim restLength As Integer = startAddress - startAddressReserved
                recordType = &H0
                checkSum = recordlengthFixed + CInt(CByte(startAddressReserved >> 8)) + CInt(CByte(startAddressReserved And &HFF)) + CInt(recordType)

                sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                                    CByte(startAddressReserved >> 8).ToString("x2") & _
                                    CByte(startAddressReserved And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                For j = 0 To restLength - 1
                    sWriter.Write("FF")
                    checkSum = checkSum + &HFF
                Next
                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                startAddressReserved = startAddressReserved + restLength
            Else
                recordType = &H0
                checkSum = recordlengthFixed + CInt(CByte(startAddressReserved >> 8)) + CInt(CByte(startAddressReserved And &HFF)) + CInt(recordType)

                sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                                    CByte(startAddressReserved >> 8).ToString("x2") & _
                                    CByte(startAddressReserved And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                For j = 0 To recordlengthFixed - 1
                    sWriter.Write("FF")
                    checkSum = checkSum + &HFF
                Next
                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                startAddressReserved = startAddressReserved + recordlengthFixed
            End If
        End While

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''end of reserved


        Select Case pjd.IC_model
            Case IC(0)
                header.ic_model = &H1
            Case IC(1)
                header.ic_model = &H2
            Case IC(2)
                header.ic_model = &H3
        End Select

        Select Case pjd.Scan_type
            Case SCAN(0)
                header.scan_type = &H1
            Case SCAN(1)
                header.scan_type = &H2
        End Select

        Select Case pjd.LVD_level
            Case LVD(0)
                header.LVD_level = &H1
            Case LVD(1)
                header.LVD_level = &H2
            Case LVD(2)
                header.LVD_level = &H3
            Case LVD(3)
                header.LVD_level = &H4
        End Select

        Dim i As Integer
        For i = 0 To header.Spare.Length - 1
            header.Spare(i) = &H0
        Next

        recordLength = Marshal.SizeOf(header)
        recordType = &H0
        checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
        checkSum = checkSum + CInt(header.ic_model) + CInt(header.scan_type) + CInt(header.LVD_level)

        sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                            CByte(startAddress >> 8).ToString("x2") & _
                            CByte(startAddress And &HFF).ToString("X2") & _
                            recordType.ToString("X2"))
        sWriter.Write(header.ic_model.ToString("X2") & _
                      header.scan_type.ToString("X2") & _
                      header.LVD_level.ToString("X2"))
        For i = 0 To header.Spare.Length - 1
            sWriter.Write(header.Spare(i).ToString("X2"))
            checkSum = checkSum + CInt(header.Spare(i))
        Next
        If checkSum < 256 Then
            checkSum = 256 - checkSum
        Else
            checkSum = checkSum And &HFF
            checkSum = 256 - checkSum
        End If
        sWriter.Write(CByte(checkSum).ToString("X2"))
        sWriter.WriteLine()

        'body
        For n = 0 To RomDataArrarySize
            If TypeOf RomDataArray(n) Is RomKey Then
                Dim key As RomKey = CType(RomDataArray(n), RomKey)

                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                recordLength = Marshal.SizeOf(key)         '
                recordType = &H0
                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                checkSum = checkSum + CInt(key.key_type) + CInt(key.drive_io) + CInt(key.sensing_io) + CInt(key.map_io) + _
                                      CInt(key.ctrl_type) + CInt(key.sensitivity_ana) + CInt(key.sensitivity_dgt) + _
                                      CInt(key.noise_level) + CInt(key.deglitch_count) + _
                                      CInt(key.threshold_high) + CInt(key.threshold_low)



                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                sWriter.Write(key.key_type.ToString("X2") & _
                                key.drive_io.ToString("X2") & _
                                key.sensing_io.ToString("X2") & _
                                key.map_io.ToString("X2") & _
                                key.ctrl_type.ToString("X2") & _
                                key.sensitivity_ana.ToString("X2") & _
                                key.sensitivity_dgt.ToString("X2") & _
                                key.noise_level.ToString("X2") & _
                                key.deglitch_count.ToString("X2") & _
                                key.threshold_high.ToString("X2") & _
                                key.threshold_low.ToString("X2"))

                'If recordlengthFixed > recordLength Then
                '    For i = 0 To recordlengthFixed - recordLength - 1
                '        sWriter.Write("FF")
                '        checkSum = checkSum + &HFF
                '    Next
                'End If


                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

            ElseIf TypeOf RomDataArray(n) Is RomGroup Then
                Dim group As RomGroup = CType(RomDataArray(n), RomGroup)

                'PART 1
                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                recordLength = Marshal.SizeOf(group)         '
                recordLength = 16
                recordType = &H0

                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                checkSum = checkSum + CInt(group.key_type) + CInt(group.key_count) + CInt(group.direction)


                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                sWriter.Write(group.key_type.ToString("X2") & _
                                group.key_count.ToString("X2") & _
                                group.direction.ToString("X2"))

                For m = 0 To group.io_no.Length - 1
                    sWriter.Write(group.io_no(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.io_no(m))
                Next

                'sWriter.Write(group.map_pwm.ToString("X2") & group.slider_type.ToString("X2"))
                'checkSum = checkSum + CInt(group.map_pwm) + CInt(group.slider_type)
                sWriter.Write(group.map_pwm.ToString("X2"))
                checkSum = checkSum + CInt(group.map_pwm)

                'For m = 0 To group.start_value.Length - 1
                '    sWriter.Write(group.start_value(m).ToString("X2"))
                '    checkSum = checkSum + CInt(group.start_value(m))
                'Next

                'For m = 0 To group.stop_value.Length - 1
                '    sWriter.Write(group.stop_value(m).ToString("X2"))
                '    checkSum = checkSum + CInt(group.stop_value(m))
                'Next

                'sWriter.Write(group.speed_vector.ToString("X2") & group.interpol.ToString("X2"))
                'checkSum = checkSum + CInt(group.speed_vector) + CInt(group.interpol)

                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If

                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                'PART 2
                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'recordLength = Marshal.SizeOf(group)         '
                recordLength = Marshal.SizeOf(group) - 16
                recordType = &H0

                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)


                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))

                sWriter.Write(group.slider_type.ToString("X2"))
                checkSum = checkSum + CInt(group.slider_type)

                For m = 0 To group.start_value.Length - 1
                    sWriter.Write(group.start_value(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.start_value(m))
                Next

                For m = 0 To group.stop_value.Length - 1
                    sWriter.Write(group.stop_value(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.stop_value(m))
                Next

                sWriter.Write(group.speed_vector.ToString("X2") & group.interpol.ToString("X2"))
                checkSum = checkSum + CInt(group.speed_vector) + CInt(group.interpol)

                'If recordlengthFixed > recordLength Then
                '    For i = 0 To recordlengthFixed - recordLength - 1
                '        sWriter.Write("FF")
                '        checkSum = checkSum + &HFF
                '    Next
                'End If

                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If

                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()



            End If
        Next

        'MsgBox(CByte(startAddress >> 8).ToString("x2"))
        'MsgBox(CByte(startAddress And &HFF).ToString("x2"))

        'keys

        'sliders/rotors




        'end of rom
        sWriter.WriteLine(":00000001FF")


        sWriter.Close()
    End Sub

    Private Sub generateHex16()
        Dim RomDataArrarySize = maxPinNumber + maxGroupNumber - 1
        Dim RomDataArray(RomDataArrarySize) As Object
        Dim n, m As Integer
        For n = 0 To RomDataArrarySize
            If n < maxPinNumber Then
                Dim key As RomKey = New RomKey
                key.key_type = &HFF
                key.drive_io = &HFF
                key.sensing_io = &HFF
                key.map_io = &HFF
                key.ctrl_type = &HFF
                key.sensitivity_ana = &HFF
                key.sensitivity_dgt = &HFF
                key.noise_level = &HFF
                key.deglitch_count = &HFF
                key.threshold_high = &HFF
                key.threshold_low = &HFF
                RomDataArray(n) = key
            Else
                Dim group As RomGroup = New RomGroup
                group.Initialize_iono()
                group.Initialize_startvalue()
                group.Initialize_stopvalue()
                group.key_type = &HFF
                group.key_count = &H0
                group.direction = &H1
                For m = 0 To group.io_no.Length - 1
                    group.io_no(m) = &HFF
                Next
                group.map_pwm = &HFF
                group.slider_type = &H1
                For m = 0 To group.start_value.Length - 1
                    group.start_value(m) = &H0
                Next
                For m = 0 To group.stop_value.Length - 1
                    group.stop_value(m) = &H0
                Next
                group.speed_vector = &HFF
                group.interpol = &HFF
                RomDataArray(n) = group
            End If
        Next

        getRomData(RomDataArray)

        Dim fstream As FileStream
        'If f.Exists("test.hex") Then
        'fstream = New FileStream("test.hex", FileMode.Truncate, FileAccess.ReadWrite)
        'Else
        fstream = New FileStream("sample.hex", FileMode.Create, FileAccess.ReadWrite)
        'End If
        Dim sWriter As New StreamWriter(fstream)
        sWriter.BaseStream.Seek(0, SeekOrigin.Begin)
        'sWriter.WriteLine(":" & c.ToString("X").PadLeft(2, "0"))
        'sWriter.WriteLine(":061F400071600205712230")
        'sWriter.WriteLine(":00000001FF")

        '3C00-3E00 = &HFF
        ' header
        Dim startAddressReserved As Integer = &H3C00
        Dim recordlengthFixed As Integer = &H10

        Dim startAddress As Integer = &H3E00
        Dim recordLength As Integer = &H0
        'Dim Address() As Byte = {&H3D, &HFF}
        Dim recordType As Byte = &H0
        Dim checkSum As Integer = &H0
        Dim header As RomHeader = New RomHeader
        header.Initialize()

        'MsgBox(startAddress)
        'MsgBox(CByte(startAddress >> 8).ToString("x2"))
        'MsgBox(CByte(startAddress And &HFF).ToString("x2"))

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''reserved
        Dim j As Integer
        While (startAddress > startAddressReserved)
            If startAddress - startAddressReserved < &H10 Then
                Dim restLength As Integer = startAddress - startAddressReserved
                recordType = &H0
                checkSum = recordlengthFixed + CInt(CByte(startAddressReserved >> 8)) + CInt(CByte(startAddressReserved And &HFF)) + CInt(recordType)

                sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                                    CByte(startAddressReserved >> 8).ToString("x2") & _
                                    CByte(startAddressReserved And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                For j = 0 To restLength - 1
                    sWriter.Write("FF")
                    checkSum = checkSum + &HFF
                Next
                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                startAddressReserved = startAddressReserved + restLength
            Else
                recordType = &H0
                checkSum = recordlengthFixed + CInt(CByte(startAddressReserved >> 8)) + CInt(CByte(startAddressReserved And &HFF)) + CInt(recordType)

                sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                                    CByte(startAddressReserved >> 8).ToString("x2") & _
                                    CByte(startAddressReserved And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                For j = 0 To recordlengthFixed - 1
                    sWriter.Write("FF")
                    checkSum = checkSum + &HFF
                Next
                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                startAddressReserved = startAddressReserved + recordlengthFixed
            End If
        End While

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''end of reserved


        Select Case pjd.IC_model
            Case IC(0)
                header.ic_model = &H1
            Case IC(1)
                header.ic_model = &H2
            Case IC(2)
                header.ic_model = &H3
        End Select

        Select Case pjd.Scan_type
            Case SCAN(0)
                header.scan_type = &H1
            Case SCAN(1)
                header.scan_type = &H2
        End Select

        Select Case pjd.LVD_level
            Case LVD(0)
                header.LVD_level = &H1
            Case LVD(1)
                header.LVD_level = &H2
            Case LVD(2)
                header.LVD_level = &H3
            Case LVD(3)
                header.LVD_level = &H4
        End Select

        Dim i As Integer
        For i = 0 To header.Spare.Length - 1
            header.Spare(i) = &H0
        Next

        recordLength = Marshal.SizeOf(header)
        recordType = &H0
        checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
        checkSum = checkSum + CInt(header.ic_model) + CInt(header.scan_type) + CInt(header.LVD_level)

        sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                            CByte(startAddress >> 8).ToString("x2") & _
                            CByte(startAddress And &HFF).ToString("X2") & _
                            recordType.ToString("X2"))
        sWriter.Write(header.ic_model.ToString("X2") & _
                      header.scan_type.ToString("X2") & _
                      header.LVD_level.ToString("X2"))
        For i = 0 To header.Spare.Length - 1
            sWriter.Write(header.Spare(i).ToString("X2"))
            checkSum = checkSum + CInt(header.Spare(i))
        Next
        If checkSum < 256 Then
            checkSum = 256 - checkSum
        Else
            checkSum = checkSum And &HFF
            checkSum = 256 - checkSum
        End If
        sWriter.Write(CByte(checkSum).ToString("X2"))
        sWriter.WriteLine()

        'body
        For n = 0 To RomDataArrarySize
            If TypeOf RomDataArray(n) Is RomKey Then
                Dim key As RomKey = CType(RomDataArray(n), RomKey)

                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                recordLength = Marshal.SizeOf(key)         '
                recordType = &H0
                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                checkSum = checkSum + CInt(key.key_type) + CInt(key.drive_io) + CInt(key.sensing_io) + CInt(key.map_io) + _
                                      CInt(key.ctrl_type) + CInt(key.sensitivity_ana) + CInt(key.sensitivity_dgt) + _
                                      CInt(key.noise_level) + CInt(key.deglitch_count) + _
                                      CInt(key.threshold_high) + CInt(key.threshold_low)



                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                sWriter.Write(key.key_type.ToString("X2") & _
                                key.drive_io.ToString("X2") & _
                                key.sensing_io.ToString("X2") & _
                                key.map_io.ToString("X2") & _
                                key.ctrl_type.ToString("X2") & _
                                key.sensitivity_ana.ToString("X2") & _
                                key.sensitivity_dgt.ToString("X2") & _
                                key.noise_level.ToString("X2") & _
                                key.deglitch_count.ToString("X2") & _
                                key.threshold_high.ToString("X2") & _
                                key.threshold_low.ToString("X2"))

                'If recordlengthFixed > recordLength Then
                '    For i = 0 To recordlengthFixed - recordLength - 1
                '        sWriter.Write("FF")
                '        checkSum = checkSum + &HFF
                '    Next
                'End If


                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If
                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

            ElseIf TypeOf RomDataArray(n) Is RomGroup Then
                Dim group As RomGroup = CType(RomDataArray(n), RomGroup)

                'PART 1
                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                recordLength = Marshal.SizeOf(group)         '
                recordLength = 16
                recordType = &H0

                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                checkSum = checkSum + CInt(group.key_type) + CInt(group.key_count) + CInt(group.direction)


                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))
                sWriter.Write(group.key_type.ToString("X2") & _
                                group.key_count.ToString("X2") & _
                                group.direction.ToString("X2"))

                For m = 0 To group.io_no.Length - 1
                    sWriter.Write(group.io_no(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.io_no(m))
                Next

                'sWriter.Write(group.map_pwm.ToString("X2") & group.slider_type.ToString("X2"))
                'checkSum = checkSum + CInt(group.map_pwm) + CInt(group.slider_type)
                sWriter.Write(group.map_pwm.ToString("X2"))
                checkSum = checkSum + CInt(group.map_pwm)

                'For m = 0 To group.start_value.Length - 1
                '    sWriter.Write(group.start_value(m).ToString("X2"))
                '    checkSum = checkSum + CInt(group.start_value(m))
                'Next

                'For m = 0 To group.stop_value.Length - 1
                '    sWriter.Write(group.stop_value(m).ToString("X2"))
                '    checkSum = checkSum + CInt(group.stop_value(m))
                'Next

                'sWriter.Write(group.speed_vector.ToString("X2") & group.interpol.ToString("X2"))
                'checkSum = checkSum + CInt(group.speed_vector) + CInt(group.interpol)

                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If

                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()

                'PART 2
                startAddress = startAddress + recordLength ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'startAddress = startAddress + recordlengthFixed ' new startAddress = previous startAddress + previous recordLength so can't change order this line and next line
                'recordLength = Marshal.SizeOf(group)         '
                recordLength = Marshal.SizeOf(group) - 16
                recordType = &H0

                checkSum = recordLength + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)
                'checkSum = recordlengthFixed + CInt(CByte(startAddress >> 8)) + CInt(CByte(startAddress And &HFF)) + CInt(recordType)


                'sWriter.Write(":" & CByte(recordlengthFixed And &HFF).ToString("X2") & _
                sWriter.Write(":" & CByte(recordLength And &HFF).ToString("X2") & _
                                    CByte(startAddress >> 8).ToString("x2") & _
                                    CByte(startAddress And &HFF).ToString("X2") & _
                                    recordType.ToString("X2"))

                sWriter.Write(group.slider_type.ToString("X2"))
                checkSum = checkSum + CInt(group.slider_type)

                For m = 0 To group.start_value.Length - 1
                    sWriter.Write(group.start_value(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.start_value(m))
                Next

                For m = 0 To group.stop_value.Length - 1
                    sWriter.Write(group.stop_value(m).ToString("X2"))
                    checkSum = checkSum + CInt(group.stop_value(m))
                Next

                sWriter.Write(group.speed_vector.ToString("X2") & group.interpol.ToString("X2"))
                checkSum = checkSum + CInt(group.speed_vector) + CInt(group.interpol)

                'If recordlengthFixed > recordLength Then
                '    For i = 0 To recordlengthFixed - recordLength - 1
                '        sWriter.Write("FF")
                '        checkSum = checkSum + &HFF
                '    Next
                'End If

                If checkSum < 256 Then
                    checkSum = 256 - checkSum
                Else
                    checkSum = checkSum And &HFF
                    checkSum = 256 - checkSum
                End If

                sWriter.Write(CByte(checkSum).ToString("X2"))
                sWriter.WriteLine()



            End If
        Next

        'MsgBox(CByte(startAddress >> 8).ToString("x2"))
        'MsgBox(CByte(startAddress And &HFF).ToString("x2"))

        'keys

        'sliders/rotors




        'end of rom
        sWriter.WriteLine(":00000001FF")


        sWriter.Close()
    End Sub

    Private Function getRomData(ByRef dataArray() As Object) As Integer
        'Dim key As RomKey = CType(dataArray(0), RomKey)
        'key.key_type = &H0
        'dataArray(0) = key

        Dim groupIndex As Integer
        Dim groupNumber As Integer = 0

        Dim ctl As Control
        For Each ctl In Me.Panel1.Controls
            'Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl

                Exit For

            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl

                Exit For

            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl

                Exit For

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm

                            Dim key As RomKey = CType(dataArray(tk.ObjectID), RomKey)
                            key.key_type = &H1

                            Dim c As Integer
                            Select Case tk.TiggerPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tk.TiggerPort Mod 8
                                    key.drive_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tk.TiggerPort Mod 8
                                    key.drive_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tk.TiggerPort Mod 8
                                    key.drive_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tk.TiggerPort Mod 8
                                    key.drive_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tk.SensorPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tk.SensorPort Mod 8
                                    key.sensing_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tk.SensorPort Mod 8
                                    key.sensing_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tk.SensorPort Mod 8
                                    key.sensing_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tk.SensorPort Mod 8
                                    key.sensing_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tk.MapPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tk.MapPort Mod 8
                                    key.map_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tk.MapPort Mod 8
                                    key.map_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tk.MapPort Mod 8
                                    key.map_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tk.MapPort Mod 8
                                    key.map_io = (&H3 << 4) Or CByte(c)
                            End Select
                            key.map_io = (CByte(tk.MapPortInit) << 7) Or key.map_io

                            Select Case tk.ControlType
                                Case ControlType.follow
                                    key.ctrl_type = &H1
                                Case ControlType.toggle
                                    key.ctrl_type = &H2
                                Case ControlType.oneshot
                                    key.ctrl_type = &H3
                            End Select

                            key.sensitivity_ana = CByte(tk.SensitivityAna)
                            key.sensitivity_dgt = CByte(tk.SensitivityDig)
                            key.noise_level = CByte(tk.NoiseFilter)
                            key.deglitch_count = CByte(tk.DeglitchCount)
                            key.threshold_high = CByte(tk.ThresholdHigh)
                            key.threshold_low = CByte(tk.ThresholdLow)

                            dataArray(tk.ObjectID) = key
                        End If
                    Next
                ElseIf ctl.Name = "slidergroup" Then
                    groupIndex = maxPinNumber + groupNumber
                    groupNumber = groupNumber + 1
                    Dim group As RomGroup = CType(dataArray(groupIndex), RomGroup)

                    group.key_type = &H2

                    Dim KeyList As List(Of TouchBar) = New List(Of TouchBar)
                    Dim keyCount As Integer = 0
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm

                            KeyList.Add(tb)

                            keyCount = keyCount + 1

                            Dim key As RomKey = CType(dataArray(tb.ObjectID), RomKey)
                            key.key_type = &H2

                            Dim c As Integer
                            Select Case tb.TiggerPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tb.TiggerPort Mod 8
                                    key.drive_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tb.TiggerPort Mod 8
                                    key.drive_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tb.TiggerPort Mod 8
                                    key.drive_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tb.TiggerPort Mod 8
                                    key.drive_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tb.SensorPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tb.SensorPort Mod 8
                                    key.sensing_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tb.SensorPort Mod 8
                                    key.sensing_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tb.SensorPort Mod 8
                                    key.sensing_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tb.SensorPort Mod 8
                                    key.sensing_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tb.MapPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tb.MapPort Mod 8
                                    key.map_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tb.MapPort Mod 8
                                    key.map_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tb.MapPort Mod 8
                                    key.map_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tb.MapPort Mod 8
                                    key.map_io = (&H3 << 4) Or CByte(c)
                            End Select
                            key.map_io = (CByte(tb.MapPortInit) << 7) Or key.map_io

                            Select Case tb.ControlType
                                Case ControlType.follow
                                    key.ctrl_type = &H1
                                Case ControlType.toggle
                                    key.ctrl_type = &H2
                                Case ControlType.oneshot
                                    key.ctrl_type = &H3
                            End Select

                            key.sensitivity_ana = CByte(tb.SensitivityAna)
                            key.sensitivity_dgt = CByte(tb.SensitivityDig)
                            key.noise_level = CByte(tb.NoiseFilter)
                            key.deglitch_count = CByte(tb.DeglitchCount)
                            key.threshold_high = CByte(tb.ThresholdHigh)
                            key.threshold_low = CByte(tb.ThresholdLow)

                            dataArray(tb.ObjectID) = key

                            Select Case tb.Direction
                                Case Direction.left
                                    group.direction = &H1
                                Case Direction.right
                                    group.direction = &H2
                                Case Direction.clockwise
                                    group.direction = &H3
                                Case Direction.counterclockwise
                                    group.direction = &H4
                            End Select

                            If tb.MapPWM >= 0 Then
                                group.map_pwm = CByte(tb.MapPWM)
                            Else
                                group.map_pwm = &HFF
                            End If


                            Select Case tb.ValueType
                                Case ValueType.absolute
                                    group.slider_type = &H1
                                Case ValueType.relative
                                    group.slider_type = &H2
                            End Select

                            group.start_value(0) = CByte(tb.StartValue >> 8)
                            group.start_value(1) = CByte(tb.StartValue And &HFF)
                            group.stop_value(0) = CByte(tb.StopValue >> 8)
                            group.stop_value(1) = CByte(tb.StopValue And &HFF)
                            group.speed_vector = CByte(tb.SpeedVector)
                            group.interpol = CByte(tb.Interpolation)

                        End If

                    Next

                    Select Case group.direction
                        Case &H1, &H3 'left,clockwise
                            Dim colsorter As CollectionSorter(Of TouchBar) = New CollectionSorter(Of TouchBar)("ObjectID asc")
                            KeyList.Sort(colsorter)
                        Case &H2, &H4 'right,counterclockwise
                            Dim colsorter As CollectionSorter(Of TouchBar) = New CollectionSorter(Of TouchBar)("ObjectID desc")
                            KeyList.Sort(colsorter)
                    End Select

                    Dim klc As Integer
                    For klc = 0 To KeyList.Count - 1
                        'group.io_no(klc) = KeyList(klc).SensorPort
                        Dim c As Integer
                        Select Case KeyList(klc).SensorPort
                            Case 0, 1, 2, 3, 4, 5, 6, 7
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H0) Or CByte(c)
                            Case 8, 9, 10, 11, 12, 13, 14, 15
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H1 << 4) Or CByte(c)
                            Case 16, 17, 18, 19, 20, 21, 22, 23
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H2 << 4) Or CByte(c)
                            Case 24, 25, 26, 27, 28, 29, 30
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H3 << 4) Or CByte(c)
                        End Select
                    Next

                    group.key_count = CByte(keyCount)

                    dataArray(groupIndex) = group

                ElseIf ctl.Name = "rotatorgroup" Then
                    groupIndex = maxPinNumber + groupNumber
                    groupNumber = groupNumber + 1
                    Dim group As RomGroup = CType(dataArray(groupIndex), RomGroup)

                    group.key_type = &H3

                    Dim KeyList As List(Of TouchPie) = New List(Of TouchPie)
                    Dim keyCount As Integer = 0
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm

                            KeyList.Add(tp)

                            keyCount = keyCount + 1

                            Dim key As RomKey = CType(dataArray(tp.ObjectID), RomKey)
                            key.key_type = &H3

                            Dim c As Integer
                            Select Case tp.TiggerPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tp.TiggerPort Mod 8
                                    key.drive_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tp.TiggerPort Mod 8
                                    key.drive_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tp.TiggerPort Mod 8
                                    key.drive_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tp.TiggerPort Mod 8
                                    key.drive_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tp.SensorPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tp.SensorPort Mod 8
                                    key.sensing_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tp.SensorPort Mod 8
                                    key.sensing_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tp.SensorPort Mod 8
                                    key.sensing_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tp.SensorPort Mod 8
                                    key.sensing_io = (&H3 << 4) Or CByte(c)
                            End Select

                            Select Case tp.MapPort
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = tp.MapPort Mod 8
                                    key.map_io = (&H0) Or CByte(c)
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = tp.MapPort Mod 8
                                    key.map_io = (&H1 << 4) Or CByte(c)
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = tp.MapPort Mod 8
                                    key.map_io = (&H2 << 4) Or CByte(c)
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = tp.MapPort Mod 8
                                    key.map_io = (&H3 << 4) Or CByte(c)
                            End Select
                            key.map_io = (CByte(tp.MapPortInit) << 7) Or key.map_io

                            Select Case tp.ControlType
                                Case ControlType.follow
                                    key.ctrl_type = &H1
                                Case ControlType.toggle
                                    key.ctrl_type = &H2
                                Case ControlType.oneshot
                                    key.ctrl_type = &H3
                            End Select

                            key.sensitivity_ana = CByte(tp.SensitivityAna)
                            key.sensitivity_dgt = CByte(tp.SensitivityDig)
                            key.noise_level = CByte(tp.NoiseFilter)
                            key.deglitch_count = CByte(tp.DeglitchCount)
                            key.threshold_high = CByte(tp.ThresholdHigh)
                            key.threshold_low = CByte(tp.ThresholdLow)

                            dataArray(tp.ObjectID) = key

                            Select Case tp.Direction
                                Case Direction.left
                                    group.direction = &H1
                                Case Direction.right
                                    group.direction = &H2
                                Case Direction.clockwise
                                    group.direction = &H3
                                Case Direction.counterclockwise
                                    group.direction = &H4
                            End Select

                            If tp.MapPWM >= 0 Then
                                group.map_pwm = CByte(tp.MapPWM)
                            Else
                                group.map_pwm = &HFF
                            End If


                            Select Case tp.ValueType
                                Case ValueType.absolute
                                    group.slider_type = &H1
                                Case ValueType.relative
                                    group.slider_type = &H2
                            End Select

                            group.start_value(0) = CByte(tp.StartValue >> 8)
                            group.start_value(1) = CByte(tp.StartValue And &HFF)
                            group.stop_value(0) = CByte(tp.StopValue >> 8)
                            group.stop_value(1) = CByte(tp.StopValue And &HFF)
                            group.speed_vector = CByte(tp.SpeedVector)
                            group.interpol = CByte(tp.Interpolation)

                        End If

                    Next

                    Select Case group.direction
                        Case &H1, &H3 'left,clockwise
                            Dim colsorter As CollectionSorter(Of TouchPie) = New CollectionSorter(Of TouchPie)("ObjectID asc")
                            KeyList.Sort(colsorter)
                        Case &H2, &H4 'right,counterclockwise
                            Dim colsorter As CollectionSorter(Of TouchPie) = New CollectionSorter(Of TouchPie)("ObjectID desc")
                            KeyList.Sort(colsorter)
                    End Select

                    Dim klc As Integer
                    For klc = 0 To KeyList.Count - 1
                        'group.io_no(klc) = KeyList(klc).SensorPort
                        Dim c As Integer
                        Select Case KeyList(klc).SensorPort
                            Case 0, 1, 2, 3, 4, 5, 6, 7
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H0) Or CByte(c)
                            Case 8, 9, 10, 11, 12, 13, 14, 15
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H1 << 4) Or CByte(c)
                            Case 16, 17, 18, 19, 20, 21, 22, 23
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H2 << 4) Or CByte(c)
                            Case 24, 25, 26, 27, 28, 29, 30
                                c = KeyList(klc).SensorPort Mod 8
                                group.io_no(klc) = (&H3 << 4) Or CByte(c)
                        End Select
                    Next

                    group.key_count = CByte(keyCount)

                    dataArray(groupIndex) = group

                End If
            End If

        Next ctl

    End Function

    Function Base10toX(ByVal num As Long, ByVal lngBase As Long) As String
        Dim i As Long, numDigits As Long
        Dim b36 As String = "", digit As Long

        If lngBase < 2 Or lngBase > 36 Then Err.Raise(vbObjectError + 1024, "Number Base Conversion", "Bases must be between 2 and 36")

        ' Calc the number of digits
        numDigits = 1
        Do Until Int(num / (lngBase ^ numDigits)) = 0
            numDigits = numDigits + 1
        Loop

        For i = 1 To numDigits
            digit = num Mod lngBase
            num = Int(num / lngBase)

            If digit >= 0 And digit <= 9 Then
                b36 = digit & b36
            ElseIf digit >= 10 And digit <= 35 Then
                b36 = Chr((digit - 10) + 65) & b36
            End If
        Next i

        Base10toX = b36
    End Function

    Function BaseXto10(ByVal num As String, ByVal lngBase As Long) As Long
        Dim i As Integer, ch As String
        Dim value As Long

        num = StrReverse(UCase(num))
        For i = 1 To Len(num)
            ch = Mid(num, i, 1)
            If ch >= "A" And ch <= "Z" Then
                value = value + (Asc(ch) - 65 + 10) * (lngBase ^ (i - 1))
            ElseIf ch <= "0" And ch <= "9" Then
                value = value + Val(ch) * (lngBase ^ (i - 1))
            End If
        Next i

        BaseXto10 = value
    End Function

    Private Sub ApplyButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ApplyButton.Click
        Me.ApplyButton.Enabled = False
    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click, 執行ToolStripMenuItem.Click

        If hDevice = &H0 Then
            MsgBox(RM.GetString("You have to download code first"))
            Return
        End If
        If hDevice = &HFFFFFFFF Then
            MsgBox(RM.GetString("Device not found"))
            Return
        End If

        Me.ToolStripButton7.Enabled = False
        Me.ToolStripButton8.Enabled = False
        Me.ToolStripButton9.Enabled = False
        Me.Button1.Enabled = False
        Me.Button2.Enabled = False
        Me.Button3.Enabled = False
        Me.ToolStripButton5.Enabled = True
        Me.ToolStripButton1.Enabled = False


        Me.ToolStripButton10.Enabled = False


        Dim ctl As Control
        For Each ctl In Me.Panel1.Controls
            'Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl
                tk.Timer1.Enabled = True
                tk.ContextMenuStrip.Enabled = False
            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl
                tb.Timer1.Enabled = True
                tb.ContextMenuStrip.Enabled = False
            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl
                tp.Timer1.Enabled = True
                tp.ContextMenuStrip.Enabled = False
            ElseIf TypeOf ctl Is GroupBox Then
                ctl.ContextMenuStrip.Enabled = False
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm
                            tk.Timer1.Enabled = True
                            'tk.ContextMenuStrip.Enabled = False
                        End If

                    Next
                ElseIf ctl.Name = "slidergroup" Then
                    ctl.ContextMenuStrip.Enabled = False
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm
                            tb.Timer1.Enabled = True
                            tb.ContextMenuStrip.Enabled = False
                        End If

                    Next
                ElseIf ctl.Name = "rotatorgroup" Then
                    ctl.ContextMenuStrip.Enabled = False
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm
                            tp.Timer1.Enabled = True
                            tp.ContextMenuStrip.Enabled = False
                        End If

                    Next
                End If
            End If

        Next ctl

        For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
            'Debug.Print(MyInput.Name)
            If TypeOf MyInput Is TKInput Then
                CType(MyInput, TKInput).Value = 256
                CType(MyInput, TKInput).Timer1.Enabled = True
                CType(MyInput, TKInput).startDebug = True
                If CType(MyInput, TKInput).work Then
                    CType(MyInput, TKInput).BaseValue = 1000
                End If
                'cant change during running
                CType(MyInput, TKInput).GTrackBar1.Enabled = False
                CType(MyInput, TKInput).GTrackBar2.Enabled = False


            End If

        Next

        'cant change during running
        Me.PropertyGrid1.Enabled = False

        t = New Thread(AddressOf keepReading)
        t.Start()

    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        Me.ToolStripButton7.Enabled = True
        Me.ToolStripButton8.Enabled = True
        Me.ToolStripButton9.Enabled = True
        Me.Button1.Enabled = True
        Me.Button2.Enabled = True
        Me.Button3.Enabled = True
        Me.ToolStripButton5.Enabled = False
        Me.ToolStripButton1.Enabled = True
        Dim ctl As Control
        For Each ctl In Me.Panel1.Controls
            'Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl

                tk.Fire = False
                tk.Timer1.Enabled = False
                tk.Refresh()

                tk.ContextMenuStrip.Enabled = True

            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl

                tb.Fire = False
                tb.Timer1.Enabled = False
                tb.Refresh()

                tb.ContextMenuStrip.Enabled = True
            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl

                tp.Fire = False
                tp.Timer1.Enabled = False
                tp.Refresh()

                tp.ContextMenuStrip.Enabled = True
            ElseIf TypeOf ctl Is GroupBox Then
                ctl.ContextMenuStrip.Enabled = True
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm
                            tk.Fire = False
                            tk.Timer1.Enabled = False
                            tk.Refresh()
                        End If
                        'tk.ContextMenuStrip.Enabled = True
                    Next
                ElseIf ctl.Name = "slidergroup" Then
                    ctl.ContextMenuStrip.Enabled = True
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm

                            tb.Fire = False
                            tb.Timer1.Enabled = False
                            tb.Refresh()

                            tb.ContextMenuStrip.Enabled = True
                        End If

                    Next
                ElseIf ctl.Name = "rotatorgroup" Then
                    ctl.ContextMenuStrip.Enabled = True
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm

                            tp.Fire = False
                            tp.Timer1.Enabled = False
                            tp.Refresh()

                            tp.ContextMenuStrip.Enabled = True
                        End If
                    Next
                End If
            End If

        Next ctl

        For Each Myiput As Control In Me.FlowLayoutPanel1.Controls
            If TypeOf Myiput Is TKInput Then
                'CType(Myiput, TKInput).Timer1.Enabled = False
                CType(Myiput, TKInput).startDebug = False
                CType(Myiput, TKInput).BaseValue = 0
                CType(Myiput, TKInput).Value = 0
                'resume changalbe after stop
                CType(Myiput, TKInput).GTrackBar1.Enabled = True
                CType(Myiput, TKInput).GTrackBar2.Enabled = True

            End If

        Next

        For Each MyGroup2 As Control In Me.FlowLayoutPanel2.Controls
            If TypeOf MyGroup2 Is GroupBox Then

                For Each itm As Control In MyGroup2.Controls
                    If TypeOf itm Is TKBar Then
                        Dim tkb As TKBar = itm
                        tkb.SmoothProgressBar1.Value = 0
                    End If
                Next


            End If

        Next

        For Each MyGroup3 As Control In Me.FlowLayoutPanel3.Controls
            If TypeOf MyGroup3 Is GroupBox Then

                For Each itm As Control In MyGroup3.Controls
                    If TypeOf itm Is TKBar Then
                        Dim tkb As TKBar = itm
                        tkb.SmoothProgressBar1.Value = 0
                    End If
                Next


            End If

        Next

        'resume changable after stop
        Me.PropertyGrid1.Enabled = True

        t.Abort()
    End Sub

#Region "create keys"
    ' create key group
    Private Sub ToolStripButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton7.Click, Button1.Click

        Dim left As Integer = getAvailableObjects()
        Dim objid As Integer
        If left = 0 Then
            Exit Sub
        End If

        objid = getObjectId(False)


        Dim GroupBox1 As New GroupBox()
        AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
        AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
        AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp
        'Me.Controls.Add(GroupBox1)
        Me.Panel1.Controls.Add(GroupBox1)
        GroupBox1.ContextMenuStrip = Me.ContextMenuStripKey
        ''''''''''''''''''''''''''''''
        'Dim cms As New ContextMenuStrip
        'GroupBox1.ContextMenuStrip = cms
        'Dim myMenuItem0 As New ToolStripMenuItem
        'myMenuItem0.Text = "Remove"
        'myMenuItem0.Name = "DeleteToolStripMenuItem"
        'AddHandler myMenuItem0.Click, AddressOf Me.DeleteToolStripMenuItem_Click
        'cms.Items.Add(myMenuItem0)

        'Dim myMenuItem1 As New ToolStripMenuItem
        'myMenuItem1.Text = "Bring to Front"
        'myMenuItem1.Name = "Bring2FrontToolStripMenuItem"
        'AddHandler myMenuItem1.Click, AddressOf Me.SenToolStripMenuItem_Click
        'cms.Items.Add(myMenuItem1)

        'Dim myMenuItem2 As New ToolStripMenuItem
        'myMenuItem2.Text = "Send to Back"
        'myMenuItem2.Name = "end2BackToolStripMenuItem"
        'AddHandler myMenuItem2.Click, AddressOf Me.SendToBackToolStripMenuItem_Click
        'cms.Items.Add(myMenuItem2)

        'Dim mySeparator As New ToolStripSeparator
        'cms.Items.Add(mySeparator)

        ''''''''''''''''''''''''''''''

        Dim r As New Random(System.DateTime.Now.Millisecond)
        Dim x As New Integer
        Dim y As New Integer
        x = r.Next(100, 200)
        y = r.Next(100, 200)

        GroupBox1.Name = "keygroup"
        GroupBox1.Location = New Point(x, y)
        'GroupBox1.Size = New Size(60, 65)
        GroupBox1.Size = New Size(100, 100)
        GroupBox1.Text = "key " & Me.keyIndex
        'setting the caption to the groupbox
        GroupBox1.BringToFront()
        GroupBox1.BackColor = Color.Transparent
        Dim newGuid As Guid = Guid.NewGuid
        GroupBox1.Tag = newGuid
        GroupBox1.ForeColor = Color.White

        ''''''''''''''''''''''
        'Dim gd As New GroupData
        'gd.theIndex = Me.keyIndex
        'gd.theobjectId = -1
        'gd.theLocation = GroupBox1.Location
        'gd.theText = GroupBox1.Text
        'gd.theSize = GroupBox1.Size
        'gd.theName = GroupBox1.Name
        'gd.theId = GroupBox1.Tag

        ''''''''''''''''''''''

        Dim MyTouchKey As New TouchKey
        'AddHandler MyTouchKey.MouseDown, AddressOf MyMouseClick
        'AddHandler MyTouchKey.MouseMove, AddressOf MyMouseMove
        'AddHandler MyTouchKey.MouseUp, AddressOf MyMouseUp
        AddHandler MyTouchKey.MouseHover, AddressOf MyMouseHover
        AddHandler MyTouchKey.MouseLeave, AddressOf MyMouseLeave
        AddHandler MyTouchKey.MouseEnter, AddressOf MyMouseEnter
        AddHandler MyTouchKey.triggerPortChanged, AddressOf triggerPortChanged
        AddHandler MyTouchKey.sensorPortChanged, AddressOf sensorPortChanged
        AddHandler MyTouchKey.mapPortChanged, AddressOf mapPortChanged


        'Me.Controls.Add(MyTouchKey)
        'Me.Panel1.Controls.Add(MyTouchKey)
        GroupBox1.Controls.Add(MyTouchKey)


        Dim triggerLine As New Line
        GroupBox1.Controls.Add(triggerLine)
        triggerLine.Location = New Point(60, 45)
        triggerLine.StartPoint = New Point(10, 10)
        triggerLine.EndPoint = New Point(0, 10)
        triggerLine.LineColor = Color.Red
        triggerLine.Name = "triggerline"
        triggerLine.Tag = objid
        triggerLine.BringToFront()

        Dim sensorLine As New Line
        GroupBox1.Controls.Add(sensorLine)
        sensorLine.Location = New Point(30, 85)
        sensorLine.StartPoint = New Point(10, 10)
        sensorLine.EndPoint = New Point(10, 0)
        sensorLine.LineColor = Color.White
        sensorLine.Name = "sensorline"
        sensorLine.Tag = objid
        sensorLine.BringToFront()

        Dim mapLine As New Line
        GroupBox1.Controls.Add(mapLine)
        mapLine.Location = New Point(30, 15)
        mapLine.StartPoint = New Point(10, 10)
        mapLine.EndPoint = New Point(10, 0)
        mapLine.LineColor = Color.White
        mapLine.Name = "mapline"
        mapLine.Tag = objid
        mapLine.BringToFront()

        Dim triggerLabel As New Label
        GroupBox1.Controls.Add(triggerLabel)
        triggerLabel.Location = New Point(70, 48)
        triggerLabel.AutoSize = True
        triggerLabel.ForeColor = Color.Red
        triggerLabel.Text = "N"
        triggerLabel.Name = "triggerlabel"
        triggerLabel.Tag = objid

        Dim sensorLabel As New Label
        GroupBox1.Controls.Add(sensorLabel)
        sensorLabel.Location = New Point(50, 85)
        sensorLabel.AutoSize = True
        sensorLabel.ForeColor = Color.White
        sensorLabel.Text = "N"
        sensorLabel.Name = "sensorlabel"
        sensorLabel.Tag = objid

        Dim mapLabel As New Label
        GroupBox1.Controls.Add(mapLabel)
        mapLabel.Location = New Point(50, 15)
        mapLabel.AutoSize = True
        mapLabel.ForeColor = Color.White
        mapLabel.Text = "N"
        mapLabel.Name = "maplabel"
        mapLabel.Tag = objid

        If pp.Show_Port = "True" Then
            'MyTouchKey.Location = New Point(40, 45)
            'MyTouchKey.myRectangle = New Rectangle(0, 0, 20, 20)
            MyTouchKey.Location = smallKeylocation
            MyTouchKey.myRectangle = smallKeyRect

            triggerLine.Visible = True
            sensorLine.Visible = True
            mapLine.Visible = True
            triggerLabel.Visible = True
            sensorLabel.Visible = True
            mapLabel.Visible = True

        Else
            'MyTouchKey.Location = New Point(16, 20)
            'MyTouchKey.myRectangle = New Rectangle(0, 0, 70, 70)
            MyTouchKey.Location = bigKeyLocation
            MyTouchKey.myRectangle = bigKeyRect


            triggerLine.Visible = False
            sensorLine.Visible = False
            mapLine.Visible = False
            triggerLabel.Visible = False
            sensorLabel.Visible = False
            mapLabel.Visible = False

        End If

        'MyTouchKey.Location = New Point(20, 20)

        MyTouchKey.Index = keyIndex
        'MyTouchKey.ObjectID = Me.ObjectIndex
        MyTouchKey.ObjectID = objid
        MyTouchKey.BringToFront()
        'Debug.Print(objid)
        TK2GROUP(objid) = GroupBox1.Tag

        ''''''''''''''''''''''
        'Dim kd As New keyData
        'kd.theGroupId = gd.theId
        'kd.theIndex = MyTouchKey.Index
        'kd.theobjectId = MyTouchKey.ObjectID
        'kd.theLocation = MyTouchKey.Location

        'kd.SensorPort = MyTouchKey.SensorPort
        'kd.MappingPort = MyTouchKey.MapPort
        'kd.ControlType = MyTouchKey.ControlType
        'kd.Sensitivity = MyTouchKey.Sensitivity
        'kd.SensitivityAna = MyTouchKey.SensitivityAna
        'kd.SensitivityDig = MyTouchKey.SensitivityDig
        'kd.NoiseFilter = MyTouchKey.NoiseFilter
        'kd.DeglitchCount = MyTouchKey.DeglitchCount
        'kd.Tiggerport = MyTouchKey.TiggerPort
        'kd.MapPortInit = MyTouchKey.MapPortInit


        'gd.theType = KeyType.key
        'gd.ActiveDir = direction.none
        'gd.TriggerPort = kd.Tiggerport
        'gd.NoiseFilter = kd.NoiseFilter


        'Me.mKeyCollection.Add(kd.theobjectId, kd)
        'Me.mGroupCollection.Add(gd.theId, gd)
        ''''''''''''''''''''''

        'MyTouchKey.BackgroundImageLayout = ImageLayout.Stretch
        'MyTouchKey.Size = New System.Drawing.Size(103, 77)
        'MyTouchKey.BorderStyle = BorderStyle.FixedSingle
        'MyTouchKey.BackgroundImageLayout = ImageLayout.Zoom
        'MyTouchKey.ContextMenuStrip = Me.ContextMenuStrip1

        ''''''Me.mObjectCollection.Add(MyTouchKey.ID, MyTouchKey)

        ''''''Debug.Print(GroupBox1.Tag.ToString())


        'Dim r As New Random(System.DateTime.Now.Millisecond)
        'Dim x As New Integer
        'Dim y As New Integer
        'x = r.Next(100, 200)
        'y = r.Next(100, 200)


        'Dim MyTouchKey As New TouchKey
        'AddHandler MyTouchKey.MouseDown, AddressOf MyMouseClick
        'AddHandler MyTouchKey.MouseMove, AddressOf MyMouseMove
        'AddHandler MyTouchKey.MouseUp, AddressOf MyMouseUp
        'AddHandler MyTouchKey.MouseHover, AddressOf MyMouseHover
        'AddHandler MyTouchKey.MouseLeave, AddressOf MyMouseLeave


        ''Me.Controls.Add(MyTouchKey)
        'Me.Panel1.Controls.Add(MyTouchKey)
        'MyTouchKey.Location = New Point(x, y)
        'MyTouchKey.Index = keyIndex
        'MyTouchKey.ObjectID = Me.ObjectIndex
        'MyTouchKey.BringToFront()
        ''MyTouchKey.BackgroundImageLayout = ImageLayout.Stretch
        ''MyTouchKey.Size = New System.Drawing.Size(103, 77)
        ''MyTouchKey.BorderStyle = BorderStyle.FixedSingle
        ''MyTouchKey.BackgroundImageLayout = ImageLayout.Zoom
        'MyTouchKey.ContextMenuStrip = Me.ContextMenuStrip1

        Me.increaseKey()
        If getAvailableObjects() <= 0 Then
            disableCreateKey()
        End If

        Me.ToolStripButton10.Enabled = True


        sender.Invalidate()
    End Sub


    'create slider group
    Private Sub ToolStripButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton8.Click, Button2.Click


        Dim left As Integer = getAvailableObjects()
        Dim objid As Integer
        If left = 0 Then
            Exit Sub
        End If



        Dim dialog As New sliderDialog(left)

        If (dialog.ShowDialog() = DialogResult.OK) Then
            Dim number As Integer = dialog.ComboBox1.Text

            If number > left Then
                number = left
            End If
            '''''''''''''''''''group in the panel1
            Dim GroupBox1 As New GroupBox()
            AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
            AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
            AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp
            'Me.Controls.Add(GroupBox1)
            Me.Panel1.Controls.Add(GroupBox1)

            ''''''''''''''''''''''''''''''''''group in the flowlayoutpanel2
            Dim GroupBox2 As New GroupBox()
            Me.FlowLayoutPanel2.Controls.Add(GroupBox2)
            '''''''''''''''''''''''''''''''''''group in the panel1

            Dim r As New Random(System.DateTime.Now.Millisecond)
            Dim x As New Integer
            Dim y As New Integer
            x = r.Next(200, 400)
            y = r.Next(100, 200)

            GroupBox1.Name = "slidergroup"
            GroupBox1.Location = New Point(x, y)
            'GroupBox1.Size = New Size(40 + ((5 + 20) * number) + 16, 95)
            GroupBox1.Size = New Size(40 + ((5 + 20) * number) + 26, 100)
            GroupBox1.Text = "slider " & Me.sliderIndex
            'setting the caption to the groupbox
            GroupBox1.BringToFront()
            GroupBox1.BackColor = Color.Transparent
            Dim newGuid As Guid = Guid.NewGuid
            GroupBox1.Tag = newGuid
            GroupBox1.ForeColor = Color.White

            '''''''''''''''''''''''''''''''''''group data
            'Dim gd As New GroupData
            'gd.theobjectId = -1
            'gd.theLocation = GroupBox1.Location
            'gd.theText = GroupBox1.Text
            'gd.theSize = GroupBox1.Size
            'gd.theName = GroupBox1.Name
            'gd.theId = GroupBox1.Tag


            ''''''''''''''''''''''''''''''''''''group in the flowlayoutpanel2
            GroupBox2.Name = "slidergroup"
            GroupBox2.Size = New Size(10 + ((5 + 20) * number) + 16, 118)
            GroupBox2.Text = "slider " & Me.sliderIndex
            GroupBox2.BringToFront()
            GroupBox2.BackColor = Color.Transparent
            GroupBox2.Tag = newGuid
            ''''''''''''''''''''''''''''''''''''group direction

            Dim arrow As PictureBox
            arrow = New PictureBox
            arrow.BackgroundImageLayout = ImageLayout.Stretch
            arrow.Size = New Size(32, 32)
            'arrow.Image = My.Resources.Resource1._1287798761_arrow_right_blue_round
            'arrow.Location = New Point(35 + ((5 + 20) * number), 40)
            Dim direction As Direction


            Dim triggerLine As New Line
            GroupBox1.Controls.Add(triggerLine)
            triggerLine.Location = New Point(31 + ((5 + 20) * number), 45)
            triggerLine.StartPoint = New Point(10, 10)
            triggerLine.EndPoint = New Point(0, 10)
            triggerLine.LineColor = Color.Red
            triggerLine.Name = "triggerline"
            triggerLine.BringToFront()


            Dim triggerLabel As New Label
            GroupBox1.Controls.Add(triggerLabel)
            triggerLabel.Location = New Point(31 + ((5 + 20) * number) + 10, 48)
            triggerLabel.AutoSize = True
            triggerLabel.ForeColor = Color.Red
            triggerLabel.Text = "N"
            triggerLabel.Name = "triggerlabel"


            ''''''''''''''''''''''''''''''''''''bar 
            Dim i As Integer
            For i = 0 To number - 1

                objid = getObjectId(True)

                If objid = -1 Then
                    Exit For
                End If

                Dim MyTouchBar As New TouchBar
                'AddHandler MyTouchBar.MouseDown, AddressOf MyMouseClick
                'AddHandler MyTouchBar.MouseMove, AddressOf MyMouseMove
                'AddHandler MyTouchBar.MouseUp, AddressOf MyMouseUp
                AddHandler MyTouchBar.MouseHover, AddressOf MyMouseHover
                AddHandler MyTouchBar.MouseLeave, AddressOf MyMouseLeave
                AddHandler MyTouchBar.MouseEnter, AddressOf MyMouseEnter
                AddHandler MyTouchBar.triggerPortChanged, AddressOf triggerPortChanged
                AddHandler MyTouchBar.sensorPortChanged, AddressOf sensorPortChanged
                AddHandler MyTouchBar.mapPortChanged, AddressOf mapPortChanged


                Dim sensorLine As New Line
                GroupBox1.Controls.Add(sensorLine)
                sensorLine.Location = New Point(32 + (5 + 20) * i, 70)
                sensorLine.StartPoint = New Point(10, 10)
                sensorLine.EndPoint = New Point(10, 0)
                sensorLine.LineColor = Color.White
                sensorLine.Name = "sensorline"
                sensorLine.Tag = objid
                sensorLine.BringToFront()

                Dim mapLine As New Line
                GroupBox1.Controls.Add(mapLine)
                mapLine.Location = New Point(32 + (5 + 20) * i, 26)
                mapLine.StartPoint = New Point(10, 10)
                mapLine.EndPoint = New Point(10, 0)
                mapLine.LineColor = Color.White
                mapLine.Name = "mapline"
                mapLine.Tag = objid
                mapLine.BringToFront()


                Dim sensorLabel As New Label
                GroupBox1.Controls.Add(sensorLabel)
                sensorLabel.Location = New Point(32 + (5 + 20) * i, 83)
                sensorLabel.AutoSize = True
                sensorLabel.ForeColor = Color.White
                sensorLabel.Text = "N"
                sensorLabel.Name = "sensorlabel"
                sensorLabel.Tag = objid

                Dim mapLabel As New Label
                GroupBox1.Controls.Add(mapLabel)
                mapLabel.Location = New Point(32 + (5 + 20) * i, 15)
                mapLabel.AutoSize = True
                mapLabel.ForeColor = Color.White
                mapLabel.Text = "N"
                mapLabel.Name = "maplabel"
                mapLabel.Tag = objid

                triggerLine.Tag = objid
                triggerLabel.Tag = objid



                If pp.Show_Port = "True" Then

                    MyTouchBar.Location = New Point(32 + (5 + 20) * i, 40)
                    MyTouchBar.myRectangle = smallBarRect

                    triggerLine.Visible = True
                    sensorLine.Visible = True
                    mapLine.Visible = True
                    triggerLabel.Visible = True
                    sensorLabel.Visible = True
                    mapLabel.Visible = True

                Else

                    MyTouchBar.Location = New Point(32 + (5 + 20) * i, 25)
                    MyTouchBar.myRectangle = bigBarRect

                    triggerLine.Visible = False
                    sensorLine.Visible = False
                    mapLine.Visible = False
                    triggerLabel.Visible = False
                    sensorLabel.Visible = False
                    mapLabel.Visible = False

                End If

                'MyTouchBar.BringToFront()
                MyTouchBar.ObjectID = objid
                GroupBox1.Controls.Add(MyTouchBar)
                MyTouchBar.BringToFront()
                MyTouchBar.ContextMenuStrip = Me.ContextMenuStripSliderKey
                increaseObject()

                TK2GROUP(objid) = GroupBox1.Tag
                direction = MyTouchBar.Direction

                ''''''''''''''''''''''''''''''''''''bar in the flowlayoutpanel2
                Dim MyTKBar As New TKBar
                MyTKBar.Location = New Point(12 + (5 + 20) * i, 15)
                MyTKBar.ObjectId = objid
                GroupBox2.Controls.Add(MyTKBar)


                ''''''''''''''''''''''''''''''''''''

            Next

            ''''''''''''''''''''''''''''''''''''group direction
            GroupBox1.Controls.Add(arrow)
            GroupBox1.ContextMenuStrip = Me.ContextMenuStripSlider
            If direction = TKtoolkit.Direction.left Then
                arrow.Image = My.Resources.Resource1._1287798671_previous
            Else
                arrow.Image = My.Resources.Resource1._1287798727_next
            End If

            arrow.Location = New Point(0, 36)


            ''''''''''''''''''''''''''''''''''''value in the flowlayoutpanel2
            Dim av As New Label
            GroupBox2.Controls.Add(av)
            av.Name = "value"
            av.Text = "0"
            '.Size = New Size(20, 50)
            av.BringToFront()
            av.Visible = True
            av.Location = New Point(10 + ((5 + 20) * number), 50)

            ''''''''''''''''''''''''''''''''''''


            Me.increaseSlider()

            If getAvailableObjects() <= 0 Then
                disableCreateKey()
            End If

            Me.ToolStripButton10.Enabled = True

            sender.Invalidate()

            Me.isGroupAvailable()

        End If



    End Sub

    'create rotor group
    Private Sub ToolStripButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton9.Click, Button3.Click


        Dim left As Integer = getAvailableObjects()
        Dim objid As Integer
        If left = 0 Then
            Exit Sub
        End If

        Dim dialog As New rotatorDialog(left)

        If (dialog.ShowDialog() = DialogResult.OK) Then
            Dim number As Integer = dialog.ComboBox1.Text

            If number > left Then
                number = left
            End If

            ''''''''''''''''''''''''''''''''group in the panel1
            Dim GroupBox1 As New GroupBox()
            AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
            AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
            AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp
            'Me.Controls.Add(GroupBox1)
            Me.Panel1.Controls.Add(GroupBox1)

            ''''''''''''''''''''''''''''''''''group in the flowlayoutpanel
            Dim GroupBox2 As New GroupBox()
            Me.FlowLayoutPanel3.Controls.Add(GroupBox2)
            ''''''''''''''''''''''''''''''''''group in the panel1
            Dim r As New Random(System.DateTime.Now.Millisecond)
            Dim x As New Integer
            Dim y As New Integer
            x = r.Next(300, 600)
            y = r.Next(100, 200)

            GroupBox1.Name = "rotatorgroup"
            GroupBox1.Location = New Point(x, y)
            GroupBox1.Size = New Size(170, 170)
            GroupBox1.Text = "rotator " & Me.rotatorIndex
            'setting the caption to the groupbox
            GroupBox1.BringToFront()
            GroupBox1.BackColor = Color.Transparent
            Dim newGuid As Guid = Guid.NewGuid
            GroupBox1.Tag = newGuid
            GroupBox1.ContextMenuStrip = Me.ContextMenuStripRotor
            GroupBox1.ForeColor = Color.White


            ''''''''''''''''''''''''''''''''''''group in the flowlayoutpanel
            GroupBox2.Name = "rotatorgroup"
            GroupBox2.Size = New Size(10 + ((5 + 20) * number) + 16, 118)
            GroupBox2.Text = "rotator " & Me.rotatorIndex
            GroupBox2.BringToFront()
            GroupBox2.BackColor = Color.Transparent
            GroupBox2.Tag = newGuid

            ''''''''''''''''''''''''''''''''''''group direction

            Dim arrow As PictureBox
            arrow = New PictureBox
            arrow.BackgroundImageLayout = ImageLayout.Stretch
            arrow.Size = New Size(32, 32)
            'arrow.Image = My.Resources.Resource1._1287798761_arrow_right_blue_round
            'arrow.Location = New Point(35 + ((5 + 20) * number), 40)
            Dim direction As Direction

            ''''''''''''''''''''''''''''''''''''pie

            Dim startAngle As Single = 0.0F
            Dim sweepAngle As Single = 0.0F

            sweepAngle = 360 / number

            Dim i As Integer
            For i = 0 To number - 1

                objid = getObjectId(True)

                If objid = -1 Then
                    Exit For
                End If

                Dim MyTouchPie As New TouchPie
                'AddHandler MyTouchPie.MouseDown, AddressOf MyMouseClick
                'AddHandler MyTouchPie.MouseMove, AddressOf MyMouseMove
                'AddHandler MyTouchPie.MouseUp, AddressOf MyMouseUp
                AddHandler MyTouchPie.MouseHover, AddressOf MyMouseHover
                AddHandler MyTouchPie.MouseLeave, AddressOf MyMouseLeave
                AddHandler MyTouchPie.MouseEnter, AddressOf MyMouseEnter
                'Me.Controls.Add(MyTouchPie)

                MyTouchPie.Location = New Point(12, 12)
                MyTouchPie.StartAngle = i * sweepAngle
                MyTouchPie.SweepAngle = sweepAngle

                MyTouchPie.BringToFront()

                MyTouchPie.ObjectID = objid
                'MyTouchKey.BackgroundImageLayout = ImageLayout.Stretch
                'MyTouchKey.Size = New System.Drawing.Size(103, 77)
                'MyTouchPie.BorderStyle = BorderStyle.FixedSingle
                'MyTouchKey.BackgroundImageLayout = ImageLayout.Zoom
                'Debug.Print(MyTouchPie.StartAngle & "," & MyTouchPie.SweepAngle)
                GroupBox1.Controls.Add(MyTouchPie)
                MyTouchPie.ContextMenuStrip = Me.ContextMenuStripRotorKey
                increaseObject()

                TK2GROUP(objid) = GroupBox1.Tag
                direction = MyTouchPie.Direction

                ''''''''''''''''''''''''''''''''''''bar in the flowlayoutpanel
                Dim MyTKBar As New TKBar
                MyTKBar.Location = New Point(12 + (5 + 20) * i, 15)
                MyTKBar.ObjectId = objid
                GroupBox2.Controls.Add(MyTKBar)


                ''''''''''''''''''''''''''''''''''''

            Next

            ''''''''''''''''''''''''''''''''''''group direction
            GroupBox1.Controls.Add(arrow)
            If direction = TKtoolkit.Direction.counterclockwise Then
                arrow.Image = My.Resources.Resource1._1289492364_arrow_counterclockwise
            Else
                arrow.Image = My.Resources.Resource1._1289492322_arrow_clockwise
            End If

            arrow.Location = New Point(70, 70)
            arrow.BringToFront()

            ''''''''''''''''''''''''''''''''''''value in the flowlayoutpanel2
            Dim av As New Label
            GroupBox2.Controls.Add(av)
            av.Name = "value"
            av.Text = "0"
            '.Size = New Size(20, 50)
            av.BringToFront()
            av.Visible = True
            av.Location = New Point(10 + ((5 + 20) * number), 50)

            ''''''''''''''''''''''''''''''''''''

            Me.increaseRotator()

            If getAvailableObjects() <= 0 Then
                disableCreateKey()
            End If

            Me.ToolStripButton10.Enabled = True

            sender.Invalidate()
            Me.isGroupAvailable()

        End If

    End Sub

#End Region





    Private Sub ContextMenuStrip1_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStripKey.Opening, ContextMenuStripSlider.Opening
        'Dim SenderLine As LineControl.Line = sender.SourceControl

        'Me.ToolStripTextBox1.Text = SenderLine.LineWidth
        'Me.ToolStripMenuItem2.Checked = SenderLine.IsFlashing

        If TypeOf sender.SourceControl Is TouchKey Then
            Dim tk As TouchKey = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is TouchBar Then
            Dim tb As TouchBar = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is TouchPie Then
            Dim tp As TouchPie = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is GroupBox Then
            If sender.SourceControl.name = "keygroup" Then
                'Debug.Print(sender.Owner.SourceControl.tag.ToString())
                Dim itm As Control
                For Each itm In sender.SourceControl.controls
                    If TypeOf itm Is TouchKey Then
                        Dim tk As TouchKey = itm
                        'MsgBox(tk.ObjectID)
                        Me.reloadKeyToolStripMenuItem(tk)
                        Exit For
                    End If
                Next


            ElseIf sender.SourceControl.name = "slidergroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchBar Then
                        Dim tb As TouchBar = itm
                        Me.reloadSliderToolStripMenuItem(tb)
                        Exit For
                    End If
                Next

            ElseIf sender.SourceControl.name = "rotatorgroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchPie Then
                        Dim tp As TouchPie = itm

                    End If

                Next

            End If
        End If


    End Sub

    Private Sub DeleteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeleteToolStripMenuItem.Click, EditToolStripMenuItem.Click, RemoveToolStripMenuItem.Click
        Dim Response As MsgBoxResult = MsgBox("Do you want to remove this object", MsgBoxStyle.YesNo, "remove " & sender.tag)
        If Response = MsgBoxResult.Yes Then   ' User chose Yes.
            ''Remove from workspace
            'Me.Controls.Remove(sender)

            'Dim tk As TouchKey = sender.Owner.SourceControl

            Me.Panel1.Controls.Remove(sender.Owner.SourceControl)
            ''''''mObjectCollection.Remove(sender.Owner.SourceControl.tag)


            If TypeOf sender.Owner.SourceControl Is TouchKey Then
                Dim tk As TouchKey = sender.Owner.SourceControl
                Me.availableObject(tk.ObjectID) = False
                Me.removeObject(tk.ObjectID)
                Me.decreaseKey()
            ElseIf TypeOf sender.Owner.SourceControl Is TouchBar Then
                Dim tb As TouchBar = sender.Owner.SourceControl
                Me.availableObject(tb.ObjectID) = False
                Me.removeObject(tb.ObjectID)
                Me.decreaseSlider()
            ElseIf TypeOf sender.Owner.SourceControl Is TouchPie Then
                Dim tp As TouchPie = sender.Owner.SourceControl
                Me.availableObject(tp.ObjectID) = False
                Me.removeObject(tp.ObjectID)
                Me.decreaseRotator()
            ElseIf TypeOf sender.Owner.SourceControl Is GroupBox Then
                If sender.Owner.SourceControl.name = "keygroup" Then
                    'Debug.Print(sender.Owner.SourceControl.tag.ToString())
                    Dim itm As Control
                    For Each itm In sender.Owner.SourceControl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm
                            Me.availableObject(tk.ObjectID) = False
                            Me.removeObject(tk.ObjectID)

                            Dim a As Integer
                            For a = 0 To IC.Length - 1
                                If IC(a) = pjd.IC_model Then
                                    If tk.SensorPort <> -1 Then
                                        P(a, tk.SensorPort) = PortStatus.available
                                    End If
                                    If tk.MapPort <> -1 Then
                                        P(a, tk.MapPort) = PortStatus.available
                                    End If
                                    If tk.TiggerPort <> -1 Then
                                        P(a, tk.TiggerPort) = PortStatus.available
                                    End If

                                End If
                            Next

                        End If
                    Next

                    Me.decreaseKey()

                ElseIf sender.Owner.SourceControl.name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In sender.Owner.SourceControl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm
                            Me.availableObject(tb.ObjectID) = False
                            Me.removeObject(tb.ObjectID)

                            Dim a As Integer
                            For a = 0 To IC.Length - 1
                                If IC(a) = pjd.IC_model Then
                                    If tb.SensorPort <> -1 Then
                                        P(a, tb.SensorPort) = PortStatus.available
                                    End If
                                    If tb.MapPort <> -1 Then
                                        P(a, tb.MapPort) = PortStatus.available
                                    End If
                                    If tb.TiggerPort <> -1 Then
                                        P(a, tb.TiggerPort) = PortStatus.available
                                    End If
                                End If
                            Next
                        End If

                    Next

                    removeSliderTag(CType(sender.Owner.SourceControl, GroupBox).Tag)

                    Me.decreaseSlider()

                    Me.isGroupAvailable()
                ElseIf sender.Owner.SourceControl.name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In sender.Owner.SourceControl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm
                            Me.availableObject(tp.ObjectID) = False
                            Me.removeObject(tp.ObjectID)

                            Dim a As Integer
                            For a = 0 To IC.Length - 1
                                If IC(a) = pjd.IC_model Then
                                    If tp.SensorPort <> -1 Then
                                        P(a, tp.SensorPort) = PortStatus.available
                                    End If
                                    If tp.MapPort <> -1 Then
                                        P(a, tp.MapPort) = PortStatus.available
                                    End If
                                    If tp.TiggerPort <> -1 Then
                                        P(a, tp.TiggerPort) = PortStatus.available
                                    End If
                                End If
                            Next
                        End If

                    Next

                    removeRotorTag(CType(sender.Owner.SourceControl, GroupBox).Tag)

                    Me.decreaseRotator()

                    Me.isGroupAvailable()
                End If
            End If

            If getAvailableObjects() > 0 Then
                enableCreateKey()
            End If

            Me.ToolStripButton10.Enabled = True

        End If
    End Sub

    Private Sub removeSliderTag(ByVal val As Guid)
        For Each MyGroup As Control In Me.FlowLayoutPanel2.Controls
            If TypeOf MyGroup Is GroupBox Then
                If MyGroup.Tag = val Then
                    Me.FlowLayoutPanel2.Controls.Remove(MyGroup)
                    Exit For
                End If
            End If

        Next
    End Sub

    Private Sub removeRotorTag(ByVal val As Guid)
        For Each MyGroup As Control In Me.FlowLayoutPanel3.Controls
            If TypeOf MyGroup Is GroupBox Then
                If MyGroup.Tag = val Then
                    Me.FlowLayoutPanel3.Controls.Remove(MyGroup)
                    Exit For
                End If
            End If

        Next
    End Sub


    'Private Sub EditToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditToolStripMenuItem.Click
    '    Dim ctl As Control
    '    Dim msg As String = ""
    '    ctl = sender.Owner.SourceControl
    '    If TypeOf ctl Is TouchKey Then
    '        Dim tk As TouchKey = ctl
    '        msg = tk.ObjectID
    '    ElseIf TypeOf ctl Is TouchBar Then
    '        Dim tb As TouchBar = ctl
    '        msg = tb.ObjectID
    '    ElseIf TypeOf ctl Is TouchPie Then
    '        Dim tp As TouchPie = ctl
    '        msg = tp.ObjectID
    '    ElseIf TypeOf ctl Is GroupBox Then
    '        If ctl.Name = "keygroup" Then
    '            Dim itm As Control
    '            For Each itm In ctl.Controls
    '                Dim tk As TouchKey = ctl
    '                msg = tk.ObjectID
    '            Next
    '        ElseIf ctl.Name = "slidergroup" Then
    '            Dim itm As Control
    '            For Each itm In ctl.Controls
    '                Dim tb As TouchBar = itm
    '                msg = tb.ObjectID
    '            Next
    '        ElseIf ctl.Name = "rotatorgroup" Then
    '            Dim itm As Control
    '            For Each itm In ctl.Controls
    '                Dim tp As TouchPie = itm
    '                msg = tp.ObjectID
    '            Next
    '        End If
    '    End If

    '    MsgBox(msg)

    'End Sub

    Private Sub SenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SenToolStripMenuItem.Click, BringToFrontToolStripMenuItem.Click, BringToFrontToolStripMenuItem1.Click
        sender.Owner.SourceControl().BringToFront()

    End Sub

    Private Sub SendToBackToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SendToBackToolStripMenuItem.Click, SendToBackToolStripMenuItem1.Click, SendToBackToolStripMenuItem2.Click
        sender.Owner.SourceControl().sendtoback()
    End Sub


    Private Sub EditToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim ctl As Control
        Dim msg As String = ""
        ctl = sender.Owner.SourceControl
        If TypeOf ctl Is TouchKey Then
            Dim tk As TouchKey = ctl
            msg = tk.ObjectID
        ElseIf TypeOf ctl Is TouchBar Then
            Dim tb As TouchBar = ctl
            msg = tb.ObjectID
        ElseIf TypeOf ctl Is TouchPie Then
            Dim tp As TouchPie = ctl
            msg = tp.ObjectID
        ElseIf TypeOf ctl Is GroupBox Then
            If ctl.Name = "keygroup" Then
                'Dim itm As Control
                'For Each itm In ctl.Controls
                '    Dim tb As TouchBar = itm
                '    msg = tb.ObjectID
                'Next
                msg = "edit key group"
            ElseIf ctl.Name = "slidergroup" Then
                'Dim itm As Control
                'For Each itm In ctl.Controls
                '    Dim tb As TouchBar = itm
                '    msg = tb.ObjectID
                'Next
                msg = "edit slider group"
            ElseIf ctl.Name = "rotatorgroup" Then
                'Dim itm As Control
                'For Each itm In ctl.Controls
                '    Dim tp As TouchPie = itm
                '    msg = tp.ObjectID
                'Next
                msg = "edit rotator group"
            End If
        End If

        MsgBox(msg)
    End Sub

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click, 儲存ToolStripMenuItem.Click
        Dim sFileDialog As New SaveFileDialog
        'sFileDialog.Filter = "專案檔 (*.zproj)|*.zproj" ' 只能寫入zproj檔
        sFileDialog.Filter = RM.GetString("Project File") & " (*.zproj)|*.zproj" ' 只能寫入zproj檔
        sFileDialog.FilterIndex = 1
        sFileDialog.RestoreDirectory = True

        If fileName Is Nothing Then
            If sFileDialog.ShowDialog() = DialogResult.OK Then
                fileName = sFileDialog.FileName
                Debug.Print(fileName)
                SaveProject(fileName)

            End If
        End If

    End Sub

    Private Sub ToolStripButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton3.Click
        Dim fdlg As OpenFileDialog = New OpenFileDialog()
        'fdlg.Title = "開啟專案"
        fdlg.Title = RM.GetString("Open Project")
        fdlg.InitialDirectory = "c:\"
        'fdlg.Filter = "專案檔 (*.zproj)|*.zproj"
        fdlg.Filter = RM.GetString("Project File") & " (*.zproj)|*.zproj" '
        fdlg.FilterIndex = 1
        fdlg.RestoreDirectory = True
        If fdlg.ShowDialog() = DialogResult.OK Then
            'textBox1.Text = fdlg.FileName
            Debug.Print(fdlg.FileName)
            'loadProject(fdlg.FileName)
            OpenProject(fdlg.FileName)
            Me.Refresh()

        End If

    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click, 專案ToolStripMenuItem.Click
        If Me.ObjectIndex <> 0 Then
            'Dim Response As MsgBoxResult = MsgBox("儲存並離開?", MsgBoxStyle.YesNo, MyName)
            Dim Response As MsgBoxResult = MsgBox(RM.GetString("Save and exit?"), MsgBoxStyle.YesNoCancel, MyName)
            If Response = MsgBoxResult.Yes Then   ' User chose Yes.
                Dim sFileDialog As New SaveFileDialog
                'sFileDialog.Filter = "專案檔 (*.zproj)|*.zproj" ' 只能寫入zproj檔
                sFileDialog.Filter = RM.GetString("Project File") & " (*.zproj)|*.zproj"  ' 只能寫入zproj檔
                sFileDialog.FilterIndex = 1
                sFileDialog.RestoreDirectory = True

                If fileName Is Nothing Then
                    If sFileDialog.ShowDialog() = DialogResult.OK Then
                        fileName = sFileDialog.FileName
                        Debug.Print(fileName)
                        SaveProject(fileName)
                        Clear()
                    End If
                Else
                    SaveProject(fileName)
                    Clear()
                End If

            ElseIf Response = MsgBoxResult.No Then
                Clear()
            End If
        End If



    End Sub

    Private Sub 另存ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 另存ToolStripMenuItem.Click
        Dim sFileDialog As New SaveFileDialog
        'sFileDialog.Filter = "專案檔 (*.zproj)|*.zproj" ' 只能寫入zproj檔
        sFileDialog.Filter = RM.GetString("Project File") & " (*.zproj)|*.zproj"  ' 只能寫入zproj檔
        sFileDialog.FilterIndex = 1
        sFileDialog.RestoreDirectory = True


        If sFileDialog.ShowDialog() = DialogResult.OK Then
            fileName = sFileDialog.FileName
            Debug.Print(fileName)

            SaveProject(fileName)
        End If

    End Sub

    Private Sub ProjctApply_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ProjctApply.Click

        Dim changed As Boolean = False

        If pp.IC_model <> pjd.IC_model Then
            changed = True
        End If

        If pp.Scan_type <> pjd.Scan_type Then
            changed = True
        End If

        If changed Then
            If Me.Panel1.Controls.Count <> 0 Then
                'If MsgBox("IC_model或Scan_type的變更會清除編輯區內的所有物件，您必須重新編輯，您確定要套用？", MsgBoxStyle.DefaultButton2 Or _
                If MsgBox(RM.GetString("IC_model or Scan_type changed"), MsgBoxStyle.DefaultButton2 Or _
                        MsgBoxStyle.Exclamation Or MsgBoxStyle.OkCancel, RM.GetString("warning")) = MsgBoxResult.Ok Then
                    Me.Clear()
                    pjd.IC_model = pp.IC_model
                    pjd.Scan_type = pp.Scan_type
                    Me.initialICTable()

                Else
                    pp.IC_model = pjd.IC_model
                    pp.Scan_type = pjd.Scan_type
                End If
            End If

        End If

        pjd.IC_model = pp.IC_model
        pjd.Scan_type = pp.Scan_type
        pjd.LVD_level = pp.LVD_level
        pjd.LVR_level = pp.LVR_level
        pjd.Work_Freq = pp.Work_Freq
        pjd.Show_port = pp.Show_Port

        Me.ProjctApply.Enabled = False

        refreshDisplay()

    End Sub

    Private Sub PropertyGrid1_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PropertyGrid1.PropertyValueChanged

        ' Whenever the user updates a property in the property editor, the object will update
        ' automatically but the treeview will not; this call will update the treeview in response
        ' to dynamic edits to the property grid control.
        'UpdateTreeview()
        Dim changed As Boolean = False

        If pp.IC_model <> pjd.IC_model Then
            changed = True
        End If

        If pp.LVD_level <> pjd.LVD_level Then
            changed = True
        End If

        If pp.LVR_level <> pjd.LVR_level Then
            changed = True
        End If

        If pp.Scan_type <> pjd.Scan_type Then
            changed = True
        End If

        If pp.Work_Freq <> pjd.Work_Freq Then
            changed = True
        End If

        If pp.Show_Port <> pjd.Show_port Then
            changed = True
        End If

        If changed Then
            Me.ProjctApply.Enabled = True
        End If

    End Sub

    Private Sub Xu6d9ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Xu6d9ToolStripMenuItem.Click
        Me.Close()
    End Sub

#End Region
    'end of form

#Region "Methods"
    Private Sub increaseObject()
        Me.ObjectIndex = Me.ObjectIndex + 1
    End Sub

    Private Sub increaseKey()
        increaseObject()
        Me.keyIndex = Me.keyIndex + 1
        Me.keyNumber = Me.keyNumber + 1
        Me.keynumberLabel.Text = Me.keyNumber

        'isKeyAvailable()
    End Sub

    Private Sub decreaseKey()
        Me.keyNumber = Me.keyNumber - 1
        Me.keynumberLabel.Text = Me.keyNumber

        'isKeyAvailable()
    End Sub
    Private Sub increaseSlider()
        Me.sliderIndex = Me.sliderIndex + 1
        Me.sliderNumber = Me.sliderNumber + 1
        Me.slidernumberLabel.Text = Me.sliderNumber

        'isKeyAvailable()
    End Sub

    Private Sub decreaseSlider()
        Me.sliderNumber = Me.sliderNumber - 1
        Me.slidernumberLabel.Text = Me.sliderNumber

        'isKeyAvailable()
    End Sub
    Private Sub increaseRotator()
        Me.rotatorIndex = Me.rotatorIndex + 1
        Me.rotatorNumber = Me.rotatorNumber + 1
        Me.rotatornumberLabel.Text = Me.rotatorNumber

        'isKeyAvailable()
    End Sub

    Private Sub decreaseRotator()
        Me.rotatorNumber = Me.rotatorNumber - 1
        Me.rotatornumberLabel.Text = Me.rotatorNumber

        'isKeyAvailable()
    End Sub

    Private Sub onSmoothProgressBarChange(ByVal id As Integer, ByVal val As Integer)

        'Dim i As Integer

        For Each MyGroup2 As Control In Me.FlowLayoutPanel2.Controls
            If TypeOf MyGroup2 Is GroupBox Then
                If MyGroup2.Tag = TK2GROUP(id) Then
                    For Each itm As Control In MyGroup2.Controls
                        If TypeOf itm Is TKBar Then
                            Dim tkb As TKBar = itm
                            If tkb.ObjectId = id Then
                                tkb.SmoothProgressBar1.Value = val
                                Exit For
                            End If
                        End If
                    Next
                    Exit Sub
                End If
            End If

        Next

        For Each MyGroup3 As Control In Me.FlowLayoutPanel3.Controls
            If TypeOf MyGroup3 Is GroupBox Then
                If MyGroup3.Tag = TK2GROUP(id) Then
                    For Each itm As Control In MyGroup3.Controls
                        If TypeOf itm Is TKBar Then
                            Dim tkb As TKBar = itm
                            If tkb.ObjectId = id Then
                                tkb.SmoothProgressBar1.Value = val
                                Exit For
                            End If
                        End If
                    Next
                    Exit Sub
                End If
            End If

        Next


    End Sub

    Private Sub onTrackBarChange(ByVal id As Integer, ByVal val As Integer, ByVal trace As Integer)
        Me.ApplyButton.Enabled = True
        Dim i As Integer
        'change value in the same group
        For i = 0 To maxPinNumber - 1
            If i <> id And TK2GROUP(i) = TK2GROUP(id) Then
                For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
                    'Debug.Print(MyInput.Name)
                    If TypeOf MyInput Is TKInput Then
                        If CType(MyInput, TKInput).work Then
                            If CType(MyInput, TKInput).Index = i Then
                                If trace = 1 Then
                                    CType(MyInput, TKInput).GTrackBar1.Value = val
                                ElseIf trace = 2 Then
                                    CType(MyInput, TKInput).GTrackBar2.Value = val
                                End If
                                'Exit For
                            End If
                        End If
                    End If
                Next
            End If
        Next


        For Each ctl As Control In Me.Panel1.Controls
            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl
                If tk.ObjectID = id Then
                    If trace = 1 Then
                        tk.ThresholdLow = val
                    ElseIf trace = 2 Then
                        tk.ThresholdHigh = val
                    End If
                    Exit For
                End If
            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl
                If tb.ObjectID = id Then
                    If trace = 1 Then
                        tb.ThresholdLow = val
                    ElseIf trace = 2 Then
                        tb.ThresholdHigh = val
                    End If
                    Exit For
                End If
            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl
                If tp.ObjectID = id Then
                    If trace = 1 Then
                        tp.ThresholdLow = val
                    ElseIf trace = 2 Then
                        tp.ThresholdHigh = val
                    End If
                    Exit For
                End If
            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm
                            If tk.ObjectID = id Then
                                If trace = 1 Then
                                    tk.ThresholdLow = val
                                ElseIf trace = 2 Then
                                    tk.ThresholdHigh = val
                                End If
                                Exit For
                            End If
                        End If
                    Next
                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm
                            If tb.ObjectID = id Then
                                If trace = 1 Then
                                    tb.ThresholdLow = val
                                ElseIf trace = 2 Then
                                    tb.ThresholdHigh = val
                                End If
                                Exit For
                            End If
                        End If
                    Next
                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm
                            If tp.ObjectID = id Then
                                If trace = 1 Then
                                    tp.ThresholdLow = val
                                ElseIf trace = 2 Then
                                    tp.ThresholdHigh = val
                                End If
                                Exit For
                            End If
                        End If

                    Next
                End If
            End If
        Next ctl


    End Sub

    Private Sub onFire(ByVal id As Integer, ByVal fire As Boolean)
        Dim ctl As Control
        For Each ctl In Me.Panel1.Controls
            'Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl
                tk.Fire = fire
                Exit For

            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl
                tb.Fire = fire
                Exit For

            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl
                tp.Fire = fire
                Exit For

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm
                            If tk.ObjectID = id Then
                                tk.Fire = fire
                                Exit For
                            End If
                        End If
                    Next
                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm

                            If tb.ObjectID = id Then
                                tb.Fire = fire
                                Exit For
                            End If

                        End If

                    Next
                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm
                            If tp.ObjectID = id Then
                                tp.Fire = fire
                                Exit For
                            End If
                        End If

                    Next
                End If
            End If

        Next ctl

    End Sub

    Private Function getObjectId(ByVal monitor As Boolean) As Integer
        Dim ret As Integer = -1
        'Dim i As Integer
        'For i = 0 To maxPinNumber - 1
        '    If Not availableObject(i) Then
        '        ret = i
        '        availableObject(i) = True
        '        Exit For
        '    End If
        'Next

        For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
            'Debug.Print(MyInput.Name)
            If TypeOf MyInput Is TKInput Then
                If Not CType(MyInput, TKInput).work Then
                    CType(MyInput, TKInput).work = True
                    CType(MyInput, TKInput).Timer1.Enabled = True
                    CType(MyInput, TKInput).monitorSmoothProgressBarChange = monitor

                    ret = CType(MyInput, TKInput).Index
                    Exit For
                End If

            End If

        Next

        Return ret
    End Function

    Private Function getAvailableObjects() As Integer

        Dim left As Integer = 0
        'For i = 0 To maxPinNumber - 1
        '    If Not availableObject(i) Then
        '        left += 1
        '    End If
        'Next

        For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
            'Debug.Print(MyInput.Name)
            If TypeOf MyInput Is TKInput Then
                If Not CType(MyInput, TKInput).work Then
                    left += 1
                End If

            End If

        Next

        'Me.Label1.Text = "尚用" & left & "個key可使用"
        Me.Label1.Text = RM.GetString("StillHave") & left & RM.GetString("KeyAvailable")

        Return left
    End Function

    Private Sub removeObject(ByVal id As Integer)
        For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
            'Debug.Print(MyInput.Name)
            If TypeOf MyInput Is TKInput Then
                If CType(MyInput, TKInput).Index = id Then
                    CType(MyInput, TKInput).work = False
                    CType(MyInput, TKInput).Timer1.Enabled = False

                    Debug.Print(id)
                End If

            End If

        Next


    End Sub

    Private Sub disableCreateKey()
        Me.ToolStripButton7.Enabled = False
        Me.ToolStripButton8.Enabled = False
        Me.ToolStripButton9.Enabled = False
        Me.Button1.Enabled = False
        Me.Button2.Enabled = False
        Me.Button3.Enabled = False
        Left = 0
    End Sub

    Private Sub enableCreateKey()
        Me.ToolStripButton7.Enabled = True
        Me.ToolStripButton8.Enabled = True
        Me.ToolStripButton9.Enabled = True
        Me.Button1.Enabled = True
        Me.Button2.Enabled = True
        Me.Button3.Enabled = True
    End Sub

    Private Sub isKeyAvailable()
        Dim left As Integer = 0 ' = maxPinNumber - Me.ObjectIndex
        Dim total As Integer = 0


        For Each ctl As Control In Me.Panel1.Controls
            'Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                total += 1
            ElseIf TypeOf ctl Is TouchBar Then
                total += 1
            ElseIf TypeOf ctl Is TouchPie Then
                total += 1
            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            total += 1
                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            total += 1
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            total += 1
                        End If
                    Next

                End If
            End If

        Next ctl

        left = maxPinNumber - total

        If left <= 0 Then
            Me.ToolStripButton7.Enabled = False
            Me.ToolStripButton8.Enabled = False
            Me.ToolStripButton9.Enabled = False
            Me.Button1.Enabled = False
            Me.Button2.Enabled = False
            Me.Button3.Enabled = False
            left = 0
        Else
            Me.ToolStripButton7.Enabled = True
            Me.ToolStripButton8.Enabled = True
            Me.ToolStripButton9.Enabled = True
            Me.Button1.Enabled = True
            Me.Button2.Enabled = True
            Me.Button3.Enabled = True
            'Me.ToolStripButton5.Enabled = False ' stop
            'Me.ToolStripButton1.Enabled = True 'run
        End If

        'Me.Label1.Text = "尚用" & left & "個key可使用"
        Me.Label1.Text = RM.GetString("StillHave") & left & RM.GetString("KeyAvailable")

    End Sub

    Private Function isGroupAvailable() As Integer
        Dim left As Integer = 0 ' = maxPinNumber - Me.ObjectIndex
        Dim total As Integer = 0


        For Each ctl As Control In Me.Panel1.Controls
            'Debug.Print(ctl.Name)
            If TypeOf ctl Is GroupBox Then
                If ctl.Name = "slidergroup" Then
                    total += 1
                ElseIf ctl.Name = "rotatorgroup" Then
                    total += 1
                End If
            End If

        Next ctl

        left = maxGroupNumber - total

        If left <= 0 Then

            Me.ToolStripButton8.Enabled = False
            Me.ToolStripButton9.Enabled = False

            Me.Button2.Enabled = False
            Me.Button3.Enabled = False
            left = 0
        Else

            Me.ToolStripButton8.Enabled = True
            Me.ToolStripButton9.Enabled = True

            Me.Button2.Enabled = True
            Me.Button3.Enabled = True
            'Me.ToolStripButton5.Enabled = False ' stop
            'Me.ToolStripButton1.Enabled = True 'run
        End If

        'Me.Label1.Text = "尚用" & left & "個key可使用"
        Return left
    End Function


    Private Sub OpenProject(ByVal sFilePath As String)

        ' Verify that the requested file is in place and
        ' exit the subroutine if the file does not exist.
        If System.IO.File.Exists(sFilePath) = False Then
            Exit Sub
        End If

        ' Since we have a valid source file, clear all is called
        ' to empty the applications sorted lists and to clear the
        ' panel of controls
        Me.Clear()

        Dim mObjectFileBundle As SortedList = New SortedList
        mObjectFileBundle = FileSerializer.Deserialize(sFilePath)

        Dim keyDataList As New SortedList
        Dim groupDataList As New SortedList
        Dim inputDataList As New SortedList

        ' Recover the object collections to populate the local
        ' sortedlists used to hold the objects.
        Dim de As DictionaryEntry
        For Each de In mObjectFileBundle
            If de.Key.ToString() = "keydatalist" Then
                keyDataList = de.Value
            ElseIf de.Key.ToString() = "groupdatalist" Then
                'mObjectNotes.Clear()
                groupDataList = de.Value
            ElseIf de.Key.ToString() = "projectproperties" Then
                pjd = de.Value

                pp.IC_model = pjd.IC_model
                pp.Scan_type = pjd.Scan_type
                pp.Work_Freq = pjd.Work_Freq
                pp.LVD_level = pjd.LVD_level
                pp.LVR_level = pjd.LVR_level
                pp.Show_Port = pjd.Show_port
                Me.PropertyGrid1.SelectedObject = pp

                Me.keyIndex = pjd.keyIndex
                Me.sliderIndex = pjd.sliderIndex
                Me.rotatorIndex = pjd.rotorIndex
                Me.keyNumber = pjd.keyNumber
                Me.sliderNumber = pjd.sliderNumber
                Me.rotatorNumber = pjd.rotorNumber
                Me.keynumberLabel.Text = Me.keyNumber
                Me.slidernumberLabel.Text = Me.sliderNumber
                Me.rotatornumberLabel.Text = Me.rotatorNumber

            ElseIf de.Key.ToString = "inputdatalist" Then
                inputDataList = de.Value
            End If

        Next

        'Select Case de2.Value.GetType.ToString()
        ' Add the objects back into the panel by recovering the stored
        ' object data and creating a new set of controls with the properties
        ' set to the object data gathered from the original object during
        ' file save and serialization

        Dim de2 As DictionaryEntry
        Dim de3 As DictionaryEntry

        For Each de2 In groupDataList
            'Debug.Print(de2.Value.GetType.ToString())
            Dim gd As New GroupData
            gd = de2.Value

            Select Case gd.theName
                Case "keygroup"
                    Dim GroupBox1 As New GroupBox()
                    AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
                    AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
                    AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp

                    Me.Panel1.Controls.Add(GroupBox1)
                    GroupBox1.ContextMenuStrip = Me.ContextMenuStripKey

                    GroupBox1.Name = gd.theName
                    GroupBox1.Location = gd.theLocation
                    GroupBox1.Size = gd.theSize
                    GroupBox1.Text = gd.theText
                    GroupBox1.BringToFront()
                    GroupBox1.BackColor = Color.Transparent
                    GroupBox1.Tag = gd.theId
                    GroupBox1.ForeColor = Color.White

                    Dim MyTouchKey As New TouchKey
                    AddHandler MyTouchKey.MouseHover, AddressOf MyMouseHover
                    AddHandler MyTouchKey.MouseLeave, AddressOf MyMouseLeave
                    AddHandler MyTouchKey.MouseEnter, AddressOf MyMouseEnter
                    AddHandler MyTouchKey.triggerPortChanged, AddressOf triggerPortChanged
                    AddHandler MyTouchKey.sensorPortChanged, AddressOf sensorPortChanged
                    AddHandler MyTouchKey.mapPortChanged, AddressOf mapPortChanged

                    GroupBox1.Controls.Add(MyTouchKey)

                    For Each de3 In keyDataList
                        Dim kd As New keyData
                        kd = de3.Value
                        If kd.theGroupId = gd.theId Then

                            Dim triggerLine As New Line
                            GroupBox1.Controls.Add(triggerLine)
                            triggerLine.Location = New Point(60, 45)
                            triggerLine.StartPoint = New Point(10, 10)
                            triggerLine.EndPoint = New Point(0, 10)
                            triggerLine.LineColor = Color.Red
                            triggerLine.Name = "triggerline"
                            triggerLine.Tag = kd.Tiggerport
                            triggerLine.BringToFront()

                            Dim sensorLine As New Line
                            GroupBox1.Controls.Add(sensorLine)
                            sensorLine.Location = New Point(30, 85)
                            sensorLine.StartPoint = New Point(10, 10)
                            sensorLine.EndPoint = New Point(10, 0)
                            sensorLine.LineColor = Color.White
                            sensorLine.Name = "sensorline"
                            sensorLine.Tag = kd.SensorPort
                            sensorLine.BringToFront()

                            Dim mapLine As New Line
                            GroupBox1.Controls.Add(mapLine)
                            mapLine.Location = New Point(30, 15)
                            mapLine.StartPoint = New Point(10, 10)
                            mapLine.EndPoint = New Point(10, 0)
                            mapLine.LineColor = Color.White
                            mapLine.Name = "mapline"
                            mapLine.Tag = kd.MappingPort
                            mapLine.BringToFront()

                            Dim triggerLabel As New Label
                            GroupBox1.Controls.Add(triggerLabel)
                            triggerLabel.Location = New Point(70, 48)
                            triggerLabel.AutoSize = True
                            triggerLabel.ForeColor = Color.Red
                            triggerLabel.Name = "triggerlabel"
                            triggerLabel.Tag = kd.theobjectId

                            Dim sensorLabel As New Label
                            GroupBox1.Controls.Add(sensorLabel)
                            sensorLabel.Location = New Point(50, 85)
                            sensorLabel.AutoSize = True
                            sensorLabel.ForeColor = Color.White
                            sensorLabel.Name = "sensorlabel"
                            sensorLabel.Tag = kd.theobjectId

                            Dim mapLabel As New Label
                            GroupBox1.Controls.Add(mapLabel)
                            mapLabel.Location = New Point(50, 15)
                            mapLabel.AutoSize = True
                            mapLabel.ForeColor = Color.White
                            mapLabel.Name = "maplabel"
                            mapLabel.Tag = kd.theobjectId

                            If pp.Show_Port = "True" Then
                                MyTouchKey.Location = smallKeylocation
                                MyTouchKey.myRectangle = smallKeyRect

                                triggerLine.Visible = True
                                sensorLine.Visible = True
                                mapLine.Visible = True
                                triggerLabel.Visible = True
                                sensorLabel.Visible = True
                                mapLabel.Visible = True

                            Else
                                MyTouchKey.Location = bigKeyLocation
                                MyTouchKey.myRectangle = bigKeyRect

                                triggerLine.Visible = False
                                sensorLine.Visible = False
                                mapLine.Visible = False
                                triggerLabel.Visible = False
                                sensorLabel.Visible = False
                                mapLabel.Visible = False

                            End If

                            MyTouchKey.Index = keyIndex
                            MyTouchKey.ObjectID = kd.theobjectId
                            MyTouchKey.BringToFront()

                            MyTouchKey.TiggerPort = kd.Tiggerport
                            MyTouchKey.SensorPort = kd.SensorPort
                            MyTouchKey.MapPort = kd.MappingPort
                            MyTouchKey.ControlType = kd.ControlType
                            MyTouchKey.Sensitivity = kd.Sensitivity
                            MyTouchKey.SensitivityAna = kd.SensitivityAna
                            MyTouchKey.SensitivityDig = kd.SensitivityDig
                            MyTouchKey.NoiseFilter = kd.NoiseFilter
                            MyTouchKey.DeglitchCount = kd.DeglitchCount
                            MyTouchKey.MapPortInit = kd.MapPortInit

                            TK2GROUP(kd.theobjectId) = GroupBox1.Tag

                            keyDataList.Remove(de3.Key)
                            Exit For
                        End If
                    Next
                Case "slidergroup"
                    Dim GroupBox1 As New GroupBox()
                    AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
                    AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
                    AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp
                    Me.Panel1.Controls.Add(GroupBox1)

                    GroupBox1.Name = gd.theName
                    GroupBox1.Location = gd.theLocation
                    GroupBox1.Size = gd.theSize
                    GroupBox1.Text = gd.theText
                    GroupBox1.BringToFront()
                    GroupBox1.BackColor = Color.Transparent
                    GroupBox1.Tag = gd.theId
                    GroupBox1.ForeColor = Color.White

                    Dim GroupBox2 As New GroupBox()
                    Me.FlowLayoutPanel2.Controls.Add(GroupBox2)

                    Dim arrow As PictureBox
                    arrow = New PictureBox
                    arrow.BackgroundImageLayout = ImageLayout.Stretch
                    arrow.Size = New Size(32, 32)
                    GroupBox1.Controls.Add(arrow)
                    GroupBox1.ContextMenuStrip = Me.ContextMenuStripSlider
                    If gd.ActiveDir = TKtoolkit.Direction.left Then
                        arrow.Image = My.Resources.Resource1._1287798671_previous
                    Else
                        arrow.Image = My.Resources.Resource1._1287798727_next
                    End If

                    arrow.Location = New Point(0, 36)

                    Dim i As Integer = 0

                    For Each de3 In keyDataList
                        Dim kd As New keyData
                        kd = de3.Value
                        If kd.theGroupId = gd.theId Then

                            Dim MyTouchBar As New TouchBar
                            AddHandler MyTouchBar.MouseHover, AddressOf MyMouseHover
                            AddHandler MyTouchBar.MouseLeave, AddressOf MyMouseLeave
                            AddHandler MyTouchBar.MouseEnter, AddressOf MyMouseEnter
                            AddHandler MyTouchBar.triggerPortChanged, AddressOf triggerPortChanged
                            AddHandler MyTouchBar.sensorPortChanged, AddressOf sensorPortChanged
                            AddHandler MyTouchBar.mapPortChanged, AddressOf mapPortChanged

                            Dim sensorLine As New Line
                            GroupBox1.Controls.Add(sensorLine)
                            sensorLine.Location = New Point(32 + (5 + 20) * i, 70)
                            sensorLine.StartPoint = New Point(10, 10)
                            sensorLine.EndPoint = New Point(10, 0)
                            sensorLine.LineColor = Color.White
                            sensorLine.Name = "sensorline"
                            sensorLine.Tag = kd.SensorPort
                            sensorLine.BringToFront()

                            Dim mapLine As New Line
                            GroupBox1.Controls.Add(mapLine)
                            mapLine.Location = New Point(32 + (5 + 20) * i, 26)
                            mapLine.StartPoint = New Point(10, 10)
                            mapLine.EndPoint = New Point(10, 0)
                            mapLine.LineColor = Color.White
                            mapLine.Name = "mapline"
                            mapLine.Tag = kd.MappingPort
                            mapLine.BringToFront()

                            Dim sensorLabel As New Label
                            GroupBox1.Controls.Add(sensorLabel)
                            sensorLabel.Location = New Point(32 + (5 + 20) * i, 83)
                            sensorLabel.AutoSize = True
                            sensorLabel.ForeColor = Color.White
                            sensorLabel.Name = "sensorlabel"
                            sensorLabel.Tag = kd.theobjectId

                            Dim mapLabel As New Label
                            GroupBox1.Controls.Add(mapLabel)
                            mapLabel.Location = New Point(32 + (5 + 20) * i, 15)
                            mapLabel.AutoSize = True
                            mapLabel.ForeColor = Color.White
                            mapLabel.Name = "maplabel"
                            mapLabel.Tag = kd.theobjectId

                            If pp.Show_Port = "True" Then
                                MyTouchBar.Location = New Point(32 + (5 + 20) * i, 40)
                                MyTouchBar.myRectangle = smallBarRect

                                sensorLine.Visible = True
                                mapLine.Visible = True
                                sensorLabel.Visible = True
                                mapLabel.Visible = True

                            Else
                                MyTouchBar.Location = New Point(32 + (5 + 20) * i, 25)
                                MyTouchBar.myRectangle = bigBarRect

                                sensorLine.Visible = False
                                mapLine.Visible = False
                                sensorLabel.Visible = False
                                mapLabel.Visible = False

                            End If

                            GroupBox1.Controls.Add(MyTouchBar)
                            'MyTouchBar.Index = keyIndex
                            MyTouchBar.ObjectID = kd.theobjectId
                            MyTouchBar.BringToFront()
                            MyTouchBar.ContextMenuStrip = Me.ContextMenuStripSliderKey


                            MyTouchBar.SensorPort = kd.SensorPort
                            MyTouchBar.MapPort = kd.MappingPort
                            MyTouchBar.ControlType = kd.ControlType
                            MyTouchBar.NoiseFilter = kd.NoiseFilter
                            MyTouchBar.NoiseFilterOuter = gd.NoiseFilter
                            MyTouchBar.DeglitchCount = kd.DeglitchCount
                            MyTouchBar.MapPortInit = kd.MapPortInit

                            MyTouchBar.TiggerPort = gd.TriggerPort
                            MyTouchBar.Sensitivity = gd.Sensitivity
                            MyTouchBar.Direction = gd.ActiveDir
                            MyTouchBar.MapPWM = gd.MapPWM
                            MyTouchBar.StepValue = gd.StepValue
                            MyTouchBar.StartValue = gd.StartValue
                            MyTouchBar.StopValue = gd.StopValue
                            MyTouchBar.Interpolation = gd.Interpolation
                            MyTouchBar.Sensitivity = gd.Sensitivity
                            MyTouchBar.SpeedVector = gd.SpeedVector
                            MyTouchBar.NoiseFilterOuter = gd.NoiseFilter
                            MyTouchBar.TiggerPort = gd.TriggerPort

                            TK2GROUP(kd.theobjectId) = GroupBox1.Tag

                            Dim MyTKBar As New TKBar
                            MyTKBar.Location = New Point(12 + (5 + 20) * i, 15)
                            MyTKBar.ObjectId = kd.theobjectId
                            GroupBox2.Controls.Add(MyTKBar)

                            'keyDataList.Remove(de3.Key)
                            'Exit For
                            i = i + 1
                        End If
                    Next

                    Dim triggerLine As New Line
                    GroupBox1.Controls.Add(triggerLine)
                    triggerLine.Location = New Point(31 + ((5 + 20) * i), 45)
                    triggerLine.StartPoint = New Point(10, 10)
                    triggerLine.EndPoint = New Point(0, 10)
                    triggerLine.LineColor = Color.Red
                    triggerLine.Name = "triggerline"
                    triggerLine.Tag = gd.TriggerPort
                    triggerLine.BringToFront()

                    Dim triggerLabel As New Label
                    GroupBox1.Controls.Add(triggerLabel)
                    triggerLabel.Location = New Point(31 + ((5 + 20) * i) + 10, 48)
                    triggerLabel.AutoSize = True
                    triggerLabel.ForeColor = Color.Red
                    triggerLabel.Name = "triggerlabel"
                    triggerLabel.Tag = gd.theobjectId
                    triggerPortChanged(gd.theobjectId, gd.TriggerPort)

                    If pp.Show_Port = "True" Then
                        triggerLine.Visible = True
                        triggerLabel.Visible = True

                    Else
                        triggerLine.Visible = False
                        triggerLabel.Visible = False
                    End If

                    GroupBox2.Name = gd.theName
                    GroupBox2.Size = New Size(10 + ((5 + 20) * i) + 16, 118)
                    GroupBox2.Text = gd.theText
                    GroupBox2.BringToFront()
                    GroupBox2.BackColor = Color.Transparent
                    GroupBox2.Tag = gd.theId

                    Dim av As New Label
                    GroupBox2.Controls.Add(av)
                    av.Name = "value"
                    av.Text = "0"
                    av.BringToFront()
                    av.Visible = True
                    av.Location = New Point(10 + ((5 + 20) * i), 50)

                Case "rotatorgroup"
                    Dim GroupBox1 As New GroupBox()
                    AddHandler GroupBox1.MouseDown, AddressOf MyMouseClick
                    AddHandler GroupBox1.MouseMove, AddressOf MyMouseMove
                    AddHandler GroupBox1.MouseUp, AddressOf MyMouseUp
                    Me.Panel1.Controls.Add(GroupBox1)

                    GroupBox1.Name = gd.theName
                    GroupBox1.Location = gd.theLocation
                    GroupBox1.Size = gd.theSize
                    GroupBox1.Text = gd.theText
                    GroupBox1.BringToFront()
                    GroupBox1.BackColor = Color.Transparent
                    GroupBox1.Tag = gd.theId
                    GroupBox1.ForeColor = Color.White

                    Dim GroupBox2 As New GroupBox()
                    Me.FlowLayoutPanel3.Controls.Add(GroupBox2)

                    Dim arrow As PictureBox
                    arrow = New PictureBox
                    arrow.BackgroundImageLayout = ImageLayout.Stretch
                    arrow.Size = New Size(32, 32)
                    GroupBox1.Controls.Add(arrow)
                    GroupBox1.ContextMenuStrip = Me.ContextMenuStripSlider
                    If gd.ActiveDir = TKtoolkit.Direction.counterclockwise Then
                        arrow.Image = My.Resources.Resource1._1289492364_arrow_counterclockwise
                    Else
                        arrow.Image = My.Resources.Resource1._1289492322_arrow_clockwise
                    End If

                    arrow.Location = New Point(70, 70)
                    arrow.BringToFront()

                    Dim startAngle As Single = 0.0F
                    Dim sweepAngle As Single = 0.0F

                    sweepAngle = 360 / gd.theCount

                    Dim i As Integer = 0
                    For Each de3 In keyDataList
                        Dim kd As New keyData
                        kd = de3.Value
                        If kd.theGroupId = gd.theId Then

                            Dim MyTouchPie As New TouchPie
                            AddHandler MyTouchPie.MouseHover, AddressOf MyMouseHover
                            AddHandler MyTouchPie.MouseLeave, AddressOf MyMouseLeave
                            AddHandler MyTouchPie.MouseEnter, AddressOf MyMouseEnter
                            'AddHandler MyTouchPie.triggerPortChanged, AddressOf triggerPortChanged
                            'AddHandler MyTouchPie.sensorPortChanged, AddressOf sensorPortChanged
                            'AddHandler MyTouchPie.mapPortChanged, AddressOf mapPortChanged

                            'Dim sensorLine As New Line
                            'GroupBox1.Controls.Add(sensorLine)
                            'sensorLine.Location = New Point(32 + (5 + 20) * i, 70)
                            'sensorLine.StartPoint = New Point(10, 10)
                            'sensorLine.EndPoint = New Point(10, 0)
                            'sensorLine.LineColor = Color.White
                            'sensorLine.Name = "sensorline"
                            'sensorLine.Tag = kd.SensorPort
                            'sensorLine.BringToFront()

                            'Dim mapLine As New Line
                            'GroupBox1.Controls.Add(mapLine)
                            'mapLine.Location = New Point(32 + (5 + 20) * i, 26)
                            'mapLine.StartPoint = New Point(10, 10)
                            'mapLine.EndPoint = New Point(10, 0)
                            'mapLine.LineColor = Color.White
                            'mapLine.Name = "mapline"
                            'mapLine.Tag = kd.MappingPort
                            'mapLine.BringToFront()

                            'Dim sensorLabel As New Label
                            'GroupBox1.Controls.Add(sensorLabel)
                            'sensorLabel.Location = New Point(32 + (5 + 20) * i, 83)
                            'sensorLabel.AutoSize = True
                            'sensorLabel.ForeColor = Color.White
                            'sensorLabel.Name = "sensorlabel"
                            'sensorLabel.Tag = kd.theobjectId

                            'Dim mapLabel As New Label
                            'GroupBox1.Controls.Add(mapLabel)
                            'mapLabel.Location = New Point(32 + (5 + 20) * i, 15)
                            'mapLabel.AutoSize = True
                            'mapLabel.ForeColor = Color.White
                            'mapLabel.Name = "maplabel"
                            'mapLabel.Tag = kd.theobjectId


                            'If pp.Show_Port = "True" Then
                            '    MyTouchBar.Location = New Point(32 + (5 + 20) * i, 40)
                            '    MyTouchBar.myRectangle = smallBarRect

                            '    sensorLine.Visible = True
                            '    mapLine.Visible = True
                            '    sensorLabel.Visible = True
                            '    mapLabel.Visible = True

                            'Else
                            '    MyTouchBar.Location = New Point(32 + (5 + 20) * i, 25)
                            '    MyTouchBar.myRectangle = bigBarRect

                            '    sensorLine.Visible = False
                            '    mapLine.Visible = False
                            '    sensorLabel.Visible = False
                            '    mapLabel.Visible = False

                            'End If

                            MyTouchPie.Location = kd.theLocation
                            MyTouchPie.StartAngle = i * sweepAngle
                            MyTouchPie.SweepAngle = sweepAngle

                            MyTouchPie.BringToFront()

                            MyTouchPie.ObjectID = kd.theobjectId
                            GroupBox1.Controls.Add(MyTouchPie)
                            MyTouchPie.ContextMenuStrip = Me.ContextMenuStripRotorKey

                            GroupBox1.Controls.Add(MyTouchPie)
                            'MyTouchBar.Index = keyIndex
                            MyTouchPie.ObjectID = kd.theobjectId

                            MyTouchPie.SensorPort = kd.SensorPort
                            MyTouchPie.MapPort = kd.MappingPort
                            MyTouchPie.ControlType = kd.ControlType
                            MyTouchPie.NoiseFilter = kd.NoiseFilter
                            MyTouchPie.NoiseFilterOuter = gd.NoiseFilter
                            MyTouchPie.DeglitchCount = kd.DeglitchCount
                            MyTouchPie.MapPortInit = kd.MapPortInit

                            MyTouchPie.TiggerPort = gd.TriggerPort
                            MyTouchPie.Sensitivity = gd.Sensitivity
                            MyTouchPie.Direction = gd.ActiveDir
                            MyTouchPie.MapPWM = gd.MapPWM
                            MyTouchPie.StepValue = gd.StepValue
                            MyTouchPie.StartValue = gd.StartValue
                            MyTouchPie.StopValue = gd.StopValue
                            MyTouchPie.Interpolation = gd.Interpolation
                            MyTouchPie.Sensitivity = gd.Sensitivity
                            MyTouchPie.SpeedVector = gd.SpeedVector
                            MyTouchPie.NoiseFilterOuter = gd.NoiseFilter
                            MyTouchPie.TiggerPort = gd.TriggerPort

                            TK2GROUP(kd.theobjectId) = GroupBox1.Tag

                            Dim MyTKBar As New TKBar
                            MyTKBar.Location = New Point(12 + (5 + 20) * i, 15)
                            MyTKBar.ObjectId = kd.theobjectId
                            GroupBox2.Controls.Add(MyTKBar)

                            'keyDataList.Remove(de3.Key)
                            'Exit For
                            i = i + 1
                        End If
                    Next

                    'Dim triggerLine As New Line
                    'GroupBox1.Controls.Add(triggerLine)
                    'triggerLine.Location = New Point(31 + ((5 + 20) * i), 45)
                    'triggerLine.StartPoint = New Point(10, 10)
                    'triggerLine.EndPoint = New Point(0, 10)
                    'triggerLine.LineColor = Color.Red
                    'triggerLine.Name = "triggerline"
                    'triggerLine.Tag = gd.TriggerPort
                    'triggerLine.BringToFront()

                    'Dim triggerLabel As New Label
                    'GroupBox1.Controls.Add(triggerLabel)
                    'triggerLabel.Location = New Point(31 + ((5 + 20) * i) + 10, 48)
                    'triggerLabel.AutoSize = True
                    'triggerLabel.ForeColor = Color.Red
                    'triggerLabel.Name = "triggerlabel"
                    'triggerLabel.Tag = gd.theobjectId

                    'If pp.Show_Port = "True" Then
                    '    triggerLine.Visible = True
                    '    triggerLabel.Visible = True

                    'Else
                    '    triggerLine.Visible = False
                    '    triggerLabel.Visible = False
                    'End If

                    GroupBox2.Name = gd.theName
                    GroupBox2.Size = New Size(10 + ((5 + 20) * i) + 16, 118)
                    GroupBox2.Text = gd.theText
                    GroupBox2.BringToFront()
                    GroupBox2.BackColor = Color.Transparent
                    GroupBox2.Tag = gd.theId

                    Dim av As New Label
                    GroupBox2.Controls.Add(av)
                    av.Name = "value"
                    av.Text = "0"
                    av.BringToFront()
                    av.Visible = True
                    av.Location = New Point(10 + ((5 + 20) * i), 50)

            End Select

        Next

        Dim de4 As DictionaryEntry
        For Each de4 In inputDataList
            Dim ind As New inputData
            ind = de4.Value

            For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
                'Debug.Print(MyInput.Name)
                If TypeOf MyInput Is TKInput Then
                    If CType(MyInput, TKInput).Index = ind.index Then
                        CType(MyInput, TKInput).BaseValue = ind.baseValue
                        CType(MyInput, TKInput).GroupTag = ind.groupTag
                        CType(MyInput, TKInput).SmoothProgressBar1.Value = ind.barValue
                        CType(MyInput, TKInput).monitorSmoothProgressBarChange = ind.smonitorSmoothProgressBarChange
                        CType(MyInput, TKInput).ObjectId = ind.objectId
                        CType(MyInput, TKInput).startDebug = ind.startDebug
                        CType(MyInput, TKInput).GTrackBar1.Value = ind.traceValue1
                        CType(MyInput, TKInput).GTrackBar2.Value = ind.traceValue2
                        CType(MyInput, TKInput).Timer1.Enabled = ind.timer
                        CType(MyInput, TKInput).work = ind.work
                    End If
                End If

            Next

        Next

        isKeyAvailable()
        isGroupAvailable()
        reloadICTable()
    End Sub

    'Private Sub loadProject(ByVal file As String)
    '    Try

    '        Dim myXmlDocument As XmlDocument = New XmlDocument()
    '        myXmlDocument.Load(file)

    '        Me.Clear()

    '        Dim node As XmlNode
    '        node = myXmlDocument.DocumentElement
    '        Dim node2 As XmlNode 'Used for internal loop.
    '        'Dim nodePriceText As XmlNode
    '        For Each node In node.ChildNodes
    '            Debug.Print("node:" & node.Name)
    '            'Find the price child node.
    '            If node.Name = "GLOBAL" Then
    '                For Each node2 In node.ChildNodes
    '                    Debug.Print("node2:" & node2.Name & " innerText" & node2.InnerText)
    '                    If node2.Name = "ObjectIndex" Then
    '                        'Dim price As Decimal
    '                        'price = System.Decimal.Parse(node2.InnerText)
    '                        ' Double the price.
    '                        'newprice = CType(price * 2, Decimal).ToString("#.00")
    '                        'Debug.Print(node2.InnerText)
    '                        ObjectIndex = node2.InnerText
    '                    ElseIf node2.Name = "keyIndex" Then
    '                        keyIndex = node2.InnerText
    '                    ElseIf node2.Name = "keyNumber" Then
    '                        keyNumber = node2.InnerText
    '                        Me.keynumberLabel.Text = Me.keyNumber
    '                    ElseIf node2.Name = "sliderIndex" Then
    '                        sliderIndex = node2.InnerText
    '                    ElseIf node2.Name = "sliderNumber" Then
    '                        sliderNumber = node2.InnerText
    '                        Me.slidernumberLabel.Text = Me.sliderNumber
    '                    ElseIf node2.Name = "rotatorIndex" Then
    '                        rotatorIndex = node2.InnerText
    '                    ElseIf node2.Name = "rotatorNumber" Then
    '                        rotatorNumber = node2.InnerText
    '                        Me.rotatornumberLabel.Text = Me.rotatorNumber
    '                    End If
    '                Next
    '            ElseIf node.Name = "Object" Then
    '                For Each node2 In node.ChildNodes
    '                    Debug.Print("node2:" & node2.Name & " innerText" & node2.InnerText)

    '                Next
    '            Else
    '                For Each node2 In node.ChildNodes
    '                    Debug.Print(">>>>node2:" & node2.Name & " innerText" & node2.InnerText)

    '                Next

    '            End If

    '        Next

    '    Catch ex As Exception
    '        Debug.Assert(ex.Message)
    '        MsgBox(file & " open failed")
    '    End Try

    'End Sub

    Private Sub SaveProject(ByVal sFilePath As String)
        Dim KeyDataList As New SortedList
        Dim GroupDataList As New SortedList
        Dim ProjectCollection As New SortedList
        ' Dim ProjectPropertiesList As New ProjectData\
        Dim inputDataList As New SortedList

        For Each MyInput As Control In Me.FlowLayoutPanel1.Controls
            'Debug.Print(MyInput.Name)
            If TypeOf MyInput Is TKInput Then
                Dim ind As New inputData
                ind.baseValue = CType(MyInput, TKInput).BaseValue
                ind.groupTag = CType(MyInput, TKInput).GroupTag
                ind.barValue = CType(MyInput, TKInput).SmoothProgressBar1.Value
                ind.index = CType(MyInput, TKInput).Index
                ind.smonitorSmoothProgressBarChange = CType(MyInput, TKInput).monitorSmoothProgressBarChange
                ind.objectId = CType(MyInput, TKInput).ObjectId
                ind.startDebug = CType(MyInput, TKInput).startDebug
                ind.traceValue1 = CType(MyInput, TKInput).GTrackBar1.Value
                ind.traceValue2 = CType(MyInput, TKInput).GTrackBar2.Value
                ind.timer = CType(MyInput, TKInput).Timer1.Enabled
                ind.work = CType(MyInput, TKInput).work

                inputDataList.Add(ind.index, ind)

            End If

        Next

        For Each ctl As Control In Me.Panel1.Controls
            Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl
                Debug.Print(tk.Location.X & " " & tk.Location.Y & " " & tk.Index & " " & tk.ObjectID & " ")
            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl
                Debug.Print(tb.Location.X & " " & tb.Location.Y & " " & tb.ObjectID)
            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl
                Debug.Print(tp.Location.X & " " & tp.Location.Y & " " & tp.ObjectID)
            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)
                    Dim gd As New GroupData
                    Dim kd As New keyData

                    gd.theName = CType(ctl, GroupBox).Name
                    gd.theLocation = CType(ctl, GroupBox).Location
                    gd.theSize = CType(ctl, GroupBox).Size
                    gd.theId = CType(ctl, GroupBox).Tag
                    gd.theText = CType(ctl, GroupBox).Text

                    Dim itm As Control
                    Dim count As Integer = 0
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            Dim tk As TouchKey = itm


                            kd.theIndex = tk.Index
                            kd.theobjectId = tk.ObjectID
                            kd.theLocation = tk.Location
                            kd.theSize = tk.Size
                            kd.theType = KeyType.key
                            kd.theGroupId = CType(ctl, GroupBox).Tag

                            kd.SensorPort = tk.SensorPort
                            kd.MappingPort = tk.MapPort
                            kd.ControlType = tk.ControlType
                            kd.Sensitivity = tk.Sensitivity
                            kd.SensitivityAna = tk.SensitivityAna
                            kd.SensitivityDig = tk.SensitivityDig
                            kd.NoiseFilter = tk.NoiseFilter
                            kd.DeglitchCount = tk.DeglitchCount
                            kd.Tiggerport = tk.TiggerPort
                            kd.MapPortInit = tk.MapPortInit
                            kd.ThresholdHigh = tk.ThresholdHigh
                            kd.ThresholdLow = tk.ThresholdLow

                            count = count + 1
                        End If

                    Next

                    gd.theCount = count
                    KeyDataList.Add(kd.theobjectId, kd)
                    GroupDataList.Add(gd.theId, gd)

                ElseIf ctl.Name = "slidergroup" Then
                    'Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)
                    Dim gd As New GroupData
                    Dim kd As New keyData

                    gd.theName = CType(ctl, GroupBox).Name
                    gd.theLocation = CType(ctl, GroupBox).Location
                    gd.theSize = CType(ctl, GroupBox).Size
                    gd.theId = CType(ctl, GroupBox).Tag
                    gd.theText = CType(ctl, GroupBox).Text

                    Dim itm As Control
                    Dim count As Integer = 0
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm

                            'inner
                            'kd.theIndex = tk.Index
                            kd.theobjectId = tb.ObjectID
                            kd.theLocation = tb.Location
                            kd.theSize = tb.Size
                            kd.theType = KeyType.bar
                            kd.theGroupId = CType(ctl, GroupBox).Tag

                            kd.SensorPort = tb.SensorPort
                            kd.MappingPort = tb.MapPort
                            kd.ControlType = tb.ControlType
                            kd.NoiseFilter = tb.NoiseFilter
                            kd.DeglitchCount = tb.DeglitchCount
                            kd.MapPortInit = tb.MapPortInit
                            kd.ThresholdHigh = tb.ThresholdHigh
                            kd.ThresholdLow = tb.ThresholdLow

                            'outer
                            gd.theobjectId = tb.ObjectID
                            gd.ActiveDir = tb.Direction
                            gd.MapPWM = tb.MapPWM
                            gd.StepValue = tb.StepValue
                            gd.StartValue = tb.StartValue
                            gd.StopValue = tb.StopValue
                            gd.Interpolation = tb.Interpolation
                            gd.Sensitivity = tb.Sensitivity
                            gd.SpeedVector = tb.SpeedVector
                            gd.NoiseFilter = tb.NoiseFilterOuter
                            gd.TriggerPort = tb.TiggerPort

                            KeyDataList.Add(kd.theobjectId, kd)
                            count = count + 1
                        End If

                    Next

                    gd.theCount = count
                    GroupDataList.Add(gd.theId, gd)

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)
                    'Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)
                    Dim gd As New GroupData
                    Dim kd As New keyData

                    gd.theName = CType(ctl, GroupBox).Name
                    gd.theLocation = CType(ctl, GroupBox).Location
                    gd.theSize = CType(ctl, GroupBox).Size
                    gd.theId = CType(ctl, GroupBox).Tag
                    gd.theText = CType(ctl, GroupBox).Text

                    Dim itm As Control
                    Dim count As Integer = 0
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            Dim tp As TouchPie = itm

                            'inner
                            'kd.theIndex = tk.Index
                            kd.theobjectId = tp.ObjectID
                            kd.theLocation = tp.Location
                            kd.theSize = tp.Size
                            kd.theType = KeyType.pie
                            kd.theGroupId = CType(ctl, GroupBox).Tag

                            kd.SensorPort = tp.SensorPort
                            kd.MappingPort = tp.MapPort
                            kd.ControlType = tp.ControlType
                            kd.NoiseFilter = tp.NoiseFilter
                            kd.DeglitchCount = tp.DeglitchCount
                            kd.MapPortInit = tp.MapPortInit
                            kd.ThresholdHigh = tp.ThresholdHigh
                            kd.ThresholdLow = tp.ThresholdLow

                            'outer
                            gd.theobjectId = tp.ObjectID
                            gd.ActiveDir = tp.Direction
                            gd.MapPWM = tp.MapPWM
                            gd.StepValue = tp.StepValue
                            gd.StartValue = tp.StartValue
                            gd.StopValue = tp.StopValue
                            gd.Interpolation = tp.Interpolation
                            gd.Sensitivity = tp.Sensitivity
                            gd.SpeedVector = tp.SpeedVector
                            gd.NoiseFilter = tp.NoiseFilterOuter
                            gd.TriggerPort = tp.TiggerPort

                            KeyDataList.Add(kd.theobjectId, kd)
                            count = count + 1
                        End If
                    Next
                    gd.theCount = count
                    GroupDataList.Add(gd.theId, gd)
                End If
            End If

        Next ctl

        pjd.keyIndex = Me.keyIndex
        pjd.sliderIndex = Me.sliderIndex
        pjd.rotorIndex = Me.rotatorIndex
        pjd.keyNumber = Me.keyNumber
        pjd.sliderNumber = Me.sliderNumber
        pjd.rotorNumber = Me.rotatorNumber

        ProjectCollection.Add("keydatalist", KeyDataList)
        ProjectCollection.Add("groupdatalist", GroupDataList)
        ProjectCollection.Add("projectproperties", pjd)
        ProjectCollection.Add("inputdatalist", inputDataList)

        FileSerializer.Serialize(sFilePath, ProjectCollection)
    End Sub

    Private Sub SaveXML(ByVal sFilePath As String)

        Dim doc As New XmlDocument()
        Dim dec As XmlDeclaration
        Dim docRoot As XmlElement
        Dim attr As XmlAttribute
        Dim node As XmlElement
        'Dim nodDetail As XmlElement
        dec = doc.CreateXmlDeclaration("1.0", "utf-8", "")
        doc.AppendChild(dec)
        docRoot = doc.CreateElement("Objects")
        doc.AppendChild(docRoot)

        Dim nodOrder As XmlElement = doc.CreateElement("GLOBAL")
        docRoot.AppendChild(nodOrder)

        node = doc.CreateElement("ObjectIndex") '新增
        node.InnerText = ObjectIndex
        nodOrder.AppendChild(node)

        node = doc.CreateElement("keyIndex")
        node.InnerText = keyIndex
        nodOrder.AppendChild(node)

        node = doc.CreateElement("keyNumber")
        node.InnerText = keyNumber
        nodOrder.AppendChild(node)

        node = doc.CreateElement("sliderIndex")
        node.InnerText = sliderIndex
        nodOrder.AppendChild(node)

        node = doc.CreateElement("sliderNumber")
        node.InnerText = sliderNumber
        nodOrder.AppendChild(node)

        node = doc.CreateElement("rotatorIndex")
        node.InnerText = sliderNumber
        nodOrder.AppendChild(node)

        node = doc.CreateElement("rotatorNumber")
        node.InnerText = sliderNumber
        nodOrder.AppendChild(node)


        For Each ctl As Control In Me.Controls
            Debug.Print(ctl.Name)

            If TypeOf ctl Is TouchKey Then
                Dim tk As TouchKey = ctl
                Debug.Print(tk.Location.X & " " & tk.Location.Y & " " & tk.Index & " " & tk.ObjectID & " ")
                nodOrder = doc.CreateElement("Object")
                docRoot.AppendChild(nodOrder)
                attr = doc.CreateAttribute("Type") '設定屬性
                attr.Value = "key"
                nodOrder.Attributes.Append(attr)
                attr = doc.CreateAttribute("X") '設定屬性
                attr.Value = tk.Location.X
                nodOrder.Attributes.Append(attr)
                attr = doc.CreateAttribute("Y") '設定屬性
                attr.Value = tk.Location.Y
                nodOrder.Attributes.Append(attr)
                attr = doc.CreateAttribute("Index") '設定屬性
                attr.Value = tk.Index
                nodOrder.Attributes.Append(attr)
                attr = doc.CreateAttribute("number") '設定屬性
                attr.Value = "1"
                nodOrder.Attributes.Append(attr)

                node = doc.CreateElement("Item") '新增
                node.InnerText = tk.ObjectID
                nodOrder.AppendChild(node)


            ElseIf TypeOf ctl Is TouchBar Then
                Dim tb As TouchBar = ctl
                Debug.Print(tb.Location.X & " " & tb.Location.Y & " " & tb.ObjectID)
            ElseIf TypeOf ctl Is TouchPie Then
                Dim tp As TouchPie = ctl
                Debug.Print(tp.Location.X & " " & tp.Location.Y & " " & tp.ObjectID)
            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "slidergroup" Then
                    Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)
                    nodOrder = doc.CreateElement("Object")
                    docRoot.AppendChild(nodOrder)
                    attr = doc.CreateAttribute("Type") '設定屬性
                    attr.Value = "slidergroup"
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("X") '設定屬性
                    attr.Value = ctl.Location.X
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("Y") '設定屬性
                    attr.Value = ctl.Location.Y
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("Index") '設定屬性
                    attr.Value = ctl.Text
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("number") '設定屬性
                    attr.Value = ctl.Controls.Count
                    nodOrder.Attributes.Append(attr)

                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            Dim tb As TouchBar = itm
                            node = doc.CreateElement("Item") '新增
                            node.InnerText = tb.ObjectID
                            nodOrder.AppendChild(node)
                        End If

                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Debug.Print(ctl.Location.X & " " & ctl.Location.Y & " " & ctl.Text)

                    nodOrder = doc.CreateElement("Object")
                    docRoot.AppendChild(nodOrder)
                    attr = doc.CreateAttribute("Type") '設定屬性
                    attr.Value = "slidergroup"
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("X") '設定屬性
                    attr.Value = ctl.Location.X
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("Y") '設定屬性
                    attr.Value = ctl.Location.Y
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("Index") '設定屬性
                    attr.Value = ctl.Text
                    nodOrder.Attributes.Append(attr)
                    attr = doc.CreateAttribute("number") '設定屬性
                    attr.Value = ctl.Controls.Count
                    nodOrder.Attributes.Append(attr)

                    Dim itm As Control
                    For Each itm In ctl.Controls
                        Dim tp As TouchPie = itm
                        node = doc.CreateElement("Item") '新增
                        node.InnerText = tp.ObjectID
                        nodOrder.AppendChild(node)
                    Next

                End If
            End If

        Next ctl


        'nodOrder = doc.CreateElement("Object")
        'docRoot.AppendChild(nodOrder)
        ''-------設定第一個Order的內容----------------------------
        'node = doc.CreateElement("OrderNo") '新增OrderNo
        'node.InnerText = "1"
        'nodOrder.AppendChild(node)

        'node = doc.CreateElement("OrderDate")
        'node.InnerText = "2008/8/22"
        'nodOrder.AppendChild(node)

        'node = doc.CreateElement("Customer") '新增Customer
        'node.InnerText = "025506"
        'nodOrder.AppendChild(node)
        'attr = doc.CreateAttribute("Name") '設定Customer的Name屬性
        'attr.Value = "陳威文"
        'node.Attributes.Append(attr)
        'attr = doc.CreateAttribute("tel") '設定Customer的Tel屬性
        'attr.Value = "2773"
        'node.Attributes.Append(attr)
        ''新增Detail Item1
        'nodDetail = doc.CreateElement("Detail")
        'nodOrder.AppendChild(nodDetail)
        'node = doc.CreateElement("Item")
        'node.InnerText = "Item1"
        'nodDetail.AppendChild(node)
        'attr = doc.CreateAttribute("price") '設定Item的Price屬性
        'attr.Value = 580
        'node.Attributes.Append(attr)
        'attr = doc.CreateAttribute("qty") '設定Item的Qty屬性
        'attr.Value = 5
        'node.Attributes.Append(attr)
        ''新增Detail Item2
        'node = doc.CreateElement("Item")
        'node.InnerText = "Item2"
        'nodDetail.AppendChild(node)
        'attr = doc.CreateAttribute("price") '設定Item的Price屬性
        'attr.Value = 500
        'node.Attributes.Append(attr)
        'attr = doc.CreateAttribute("qty") '設定Item的Qty屬性
        'attr.Value = 3
        'node.Attributes.Append(attr)

        'nodOrder = doc.CreateElement("Order")
        'docRoot.AppendChild(nodOrder)

        ''-------設定第二個Order的內容----------------------------
        'node = doc.CreateElement("OrderNo")

        'nodOrder.AppendChild(node)
        'node.InnerText = 2

        'node = doc.CreateElement("OrderDate")
        'node.InnerText = "2008/8/23"
        'nodOrder.AppendChild(node)

        'node = doc.CreateElement("Customer")
        'node.InnerText = "022519"
        'nodOrder.AppendChild(node)
        'attr = doc.CreateAttribute("Name")
        'attr.Value = "陳天亮"
        'node.Attributes.Append(attr)
        'attr = doc.CreateAttribute("tel")
        'attr.Value = "6440"
        'node.Attributes.Append(attr)
        ''新增Detail Item1
        'nodDetail = doc.CreateElement("Detail")
        'nodOrder.AppendChild(nodDetail)
        'node = doc.CreateElement("Item")
        'node.InnerText = "Item3"
        'nodDetail.AppendChild(node)
        'attr = doc.CreateAttribute("price") '設定Item的Price屬性
        'attr.Value = 590
        'node.Attributes.Append(attr)
        'attr = doc.CreateAttribute("qty") '設定Item的Qty屬性
        'attr.Value = 2
        'node.Attributes.Append(attr)
        'Debug.Print(doc.InnerXml)


        doc.Save(sFilePath)


        Me.Saved = True
    End Sub

    Private Sub Clear()
        Dim ctls(100) As Control
        Dim number As Integer = 0
        'Dim ctl As Control
        'For Each ctl In Me.Controls
        '    'Debug.Print(ctl.Name)

        '    If TypeOf ctl Is TouchKey Or TypeOf ctl Is TouchBar Or TypeOf ctl Is TouchPie Then
        '        ctls(number) = ctl
        '        number += 1
        '    ElseIf TypeOf ctl Is GroupBox Then
        '        If ctl.Name = "slidergroup" Or ctl.Name = "rotatorgroup" Then
        '            ctls(number) = ctl
        '            number += 1
        '        End If
        '    End If


        'Next ctl
        ''Debug.Print(number)
        'Dim i As Integer
        'For i = 0 To number - 1

        '    If ctls(i) IsNot Nothing Then
        '        Me.Controls.Remove(ctls(i))
        '    End If

        'Next
        'Me.Refresh()

        Me.Panel1.Controls.Clear()

        For Each Myiput As Control In Me.FlowLayoutPanel1.Controls
            If TypeOf Myiput Is TKInput Then
                CType(Myiput, TKInput).work = False
                CType(Myiput, TKInput).monitorSmoothProgressBarChange = False
                CType(Myiput, TKInput).Timer1.Enabled = False
                CType(Myiput, TKInput).GTrackBar1.Value = 0
                CType(Myiput, TKInput).GTrackBar2.Value = 255
                CType(Myiput, TKInput).SmoothProgressBar1.Value = 0
                'CType(Myiput, TKInput).Invalidate()
                'CType(Myiput, TKInput).Refresh()
            End If

        Next

        Me.FlowLayoutPanel2.Controls.Clear()
        Me.FlowLayoutPanel3.Controls.Clear()


        Me.ObjectIndex = 0
        Me.keyIndex = 0
        Me.keyNumber = 0
        Me.keynumberLabel.Text = 0
        Me.sliderNumber = 0
        Me.sliderIndex = 0
        Me.slidernumberLabel.Text = 0
        Me.rotatorNumber = 0
        Me.rotatorIndex = 0
        Me.rotatornumberLabel.Text = 0
        'Me.Label1.Text = "尚用" & maxPinNumber & "個key可使用"
        Me.Label1.Text = RM.GetString("StillHave") & maxPinNumber & RM.GetString("KeyAvailable")

        Me.isGroupAvailable()
        Me.isKeyAvailable()

    End Sub

    Private Sub initialICTable()
        Dim a, b As Integer
        For a = 0 To IC.Length
            For b = 0 To maxPort
                P(a, b) = -1
                TPGT(a, b) = PortStatus.unavailable
            Next
        Next

        For a = 0 To IC.Length
            Select Case a
                Case 0 'ZET8234WMA
                    For b = 0 To maxPort
                        P(a, b) = PortStatus.available
                    Next

                    TPGT(a, 26) = PortStatus.available
                    TPGT(a, 27) = PortStatus.available
                    TPGT(a, 28) = PortStatus.available
                    TPGT(a, 29) = PortStatus.available
                    TPGT(a, 30) = PortStatus.available

                Case 1 'ZET8234WLA
                    For b = 0 To maxPort
                        P(a, b) = PortStatus.available
                    Next
                    P(a, 28) = PortStatus.unavailable
                    P(a, 29) = PortStatus.unavailable
                    P(a, 30) = PortStatus.unavailable

                    TPGT(a, 26) = PortStatus.available
                    TPGT(a, 27) = PortStatus.available
                    TPGT(a, 28) = PortStatus.available
                    TPGT(a, 29) = PortStatus.available
                    TPGT(a, 30) = PortStatus.available

                Case 2 'ZET8234VGA
                    For b = 0 To maxPort
                        P(a, b) = PortStatus.available
                    Next
                    P(a, 24) = PortStatus.unavailable
                    P(a, 25) = PortStatus.unavailable
                    P(a, 26) = PortStatus.unavailable
                    P(a, 27) = PortStatus.unavailable
                    P(a, 28) = PortStatus.unavailable
                    P(a, 29) = PortStatus.unavailable
                    P(a, 30) = PortStatus.unavailable

                    TPGT(a, 26) = PortStatus.available
                    TPGT(a, 27) = PortStatus.available
                    TPGT(a, 28) = PortStatus.available
                    TPGT(a, 29) = PortStatus.available
                    TPGT(a, 30) = PortStatus.available

            End Select
        Next
    End Sub

    Private Sub reloadICTable()
        Dim a As Integer
        For a = 0 To IC.Length - 1
            If IC(a) = pjd.IC_model Then
                For Each ctl As Control In Me.Panel1.Controls

                    If TypeOf ctl Is TouchKey Then

                    ElseIf TypeOf ctl Is TouchBar Then

                    ElseIf TypeOf ctl Is TouchPie Then

                    ElseIf TypeOf ctl Is GroupBox Then
                        If ctl.Name = "keygroup" Then
                            Dim itm As Control
                            For Each itm In ctl.Controls
                                If TypeOf itm Is TouchKey Then
                                    If CType(itm, TouchKey).SensorPort <> -1 Then
                                        P(a, CType(itm, TouchKey).SensorPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchKey).MapPort <> -1 Then
                                        P(a, CType(itm, TouchKey).MapPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchKey).TiggerPort <> -1 Then
                                        P(a, CType(itm, TouchKey).TiggerPort) = PortStatus.used
                                        TPGT(a, CType(itm, TouchKey).TiggerPort) = PortStatus.used
                                    End If

                                End If
                            Next

                        ElseIf ctl.Name = "slidergroup" Then
                            Dim itm As Control
                            For Each itm In ctl.Controls
                                If TypeOf itm Is TouchBar Then
                                    If CType(itm, TouchBar).SensorPort <> -1 Then
                                        P(a, CType(itm, TouchBar).SensorPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchBar).MapPort <> -1 Then
                                        P(a, CType(itm, TouchBar).MapPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchBar).TiggerPort <> -1 Then
                                        P(a, CType(itm, TouchBar).TiggerPort) = PortStatus.used
                                        TPGT(a, CType(itm, TouchBar).TiggerPort) = PortStatus.used
                                    End If

                                End If
                            Next

                        ElseIf ctl.Name = "rotatorgroup" Then
                            Dim itm As Control
                            For Each itm In ctl.Controls
                                If TypeOf itm Is TouchPie Then
                                    If CType(itm, TouchPie).SensorPort <> -1 Then
                                        P(a, CType(itm, TouchPie).SensorPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchPie).MapPort <> -1 Then
                                        P(a, CType(itm, TouchPie).MapPort) = PortStatus.used
                                    End If
                                    If CType(itm, TouchPie).TiggerPort <> -1 Then
                                        P(a, CType(itm, TouchPie).TiggerPort) = PortStatus.used
                                        TPGT(a, CType(itm, TouchPie).TiggerPort) = PortStatus.used
                                    End If

                                End If
                            Next

                        End If
                    End If

                Next ctl
            End If
        Next

    End Sub


    Private Sub reloadKeyToolStripMenuItem(ByVal tk As TouchKey)

        Try

            'Dim tk As New TouchKey

            'For Each ctl As Control In Me.Panel1.Controls

            '    If TypeOf ctl Is TouchKey Then

            '    ElseIf TypeOf ctl Is TouchBar Then

            '    ElseIf TypeOf ctl Is TouchPie Then

            '    ElseIf TypeOf ctl Is GroupBox Then
            '        If ctl.Name = "keygroup" Then
            '            Dim itm As Control
            '            For Each itm In ctl.Controls
            '                If TypeOf itm Is TouchKey Then
            '                    If CType(itm, TouchKey).ObjectID = oid Then
            '                        tk = itm
            '                        Exit For
            '                    End If

            '                End If
            '            Next

            '        ElseIf ctl.Name = "slidergroup" Then
            '            Dim itm As Control
            '            For Each itm In ctl.Controls
            '                If TypeOf itm Is TouchBar Then

            '                End If
            '            Next

            '        ElseIf ctl.Name = "rotatorgroup" Then
            '            Dim itm As Control
            '            For Each itm In ctl.Controls
            '                If TypeOf itm Is TouchPie Then

            '                End If
            '            Next

            '        End If
            '    End If

            'Next ctl

            If tk Is Nothing Then
                Exit Sub
            End If


            Dim a, b, c As Integer
            'Key sensor port
            Me.KSP0ToolStripMenuItem.DropDownItems.Clear()
            Me.KSP1ToolStripMenuItem.DropDownItems.Clear()
            Me.KSP2ToolStripMenuItem.DropDownItems.Clear()
            Me.KSP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KSPmyPrivateMenuItemHandler
                                    Me.KSP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KSP0ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KSP0ToolStripMenuItem.DropDown.Tag = tk.SensorPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KSPmyPrivateMenuItemHandler
                                    Me.KSP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KSP1ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KSP1ToolStripMenuItem.DropDown.Tag = tk.SensorPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KSPmyPrivateMenuItemHandler
                                    Me.KSP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KSP2ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KSP2ToolStripMenuItem.DropDown.Tag = tk.SensorPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KSPmyPrivateMenuItemHandler
                                    Me.KSP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KSP3ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KSP3ToolStripMenuItem.DropDown.Tag = tk.SensorPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next

            'key mapping port
            Me.MappingPortToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.MappingPortToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value

            If tk.MapPort <> -1 Then
                Me.KMPDisableToolStripMenuItem.Checked = False
            Else
                Me.KMPDisableToolStripMenuItem.Checked = True
            End If
            Me.KMP0ToolStripMenuItem.DropDownItems.Clear()
            Me.KMP1ToolStripMenuItem.DropDownItems.Clear()
            Me.KMP2ToolStripMenuItem.DropDownItems.Clear()
            Me.KMP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KMPmyPrivateMenuItemHandler
                                    Me.KMP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KMP0ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KMP0ToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KMPmyPrivateMenuItemHandler
                                    Me.KMP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KMP1ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KMP1ToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KMPmyPrivateMenuItemHandler
                                    Me.KMP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KMP2ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KMP2ToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tk.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.KMPmyPrivateMenuItemHandler
                                    Me.KMP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.KMP3ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                    Me.KMP3ToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next

            'key tigger port
            Me.KeyTiggerPortToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyTiggerPortToolStripMenuItem.DropDown.Tag = tk.TiggerPort 'used for log previous value

            If tk.TiggerPort <> -1 Then
                Me.KTPDisableToolStripMenuItem.Checked = False
            Else
                Me.KTPDisableToolStripMenuItem.Checked = True
            End If
            Me.KTP0ToolStripMenuItem.DropDownItems.Clear()
            Me.KTP1ToolStripMenuItem.DropDownItems.Clear()
            Me.KTP2ToolStripMenuItem.DropDownItems.Clear()
            Me.KTP3ToolStripMenuItem.DropDownItems.Clear()
            If pjd.Scan_type = "Mutual" Then
                Me.KeyTiggerPortToolStripMenuItem.Enabled = True
                For a = 0 To IC.Length - 1
                    If IC(a) = pjd.IC_model Then
                        For b = 0 To maxPort
                            If P(a, b) <> -1 Then
                                Select Case b
                                    Case 0, 1, 2, 3, 4, 5, 6, 7
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P0[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tk.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tk.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.KTPmyPrivateMenuItemHandler
                                        Me.KTP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.KTP0ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                        Me.KTP0ToolStripMenuItem.DropDown.Tag = tk.TiggerPort 'used for log previous value
                                    Case 8, 9, 10, 11, 12, 13, 14, 15
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P1[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tk.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tk.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.KTPmyPrivateMenuItemHandler
                                        Me.KTP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.KTP1ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                        Me.KTP1ToolStripMenuItem.DropDown.Tag = tk.TiggerPort 'used for log previous value
                                    Case 16, 17, 18, 19, 20, 21, 22, 23
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P2[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tk.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tk.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.KTPmyPrivateMenuItemHandler
                                        Me.KTP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.KTP2ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                        Me.KTP2ToolStripMenuItem.DropDown.Tag = tk.TiggerPort 'used for log previous value
                                    Case 24, 25, 26, 27, 28, 29, 30
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P3[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tk.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tk.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.KTPmyPrivateMenuItemHandler
                                        Me.KTP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.KTP3ToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                                        Me.KTP3ToolStripMenuItem.DropDown.Tag = tk.TiggerPort 'used for log previous value
                                End Select
                            End If
                        Next

                    End If
                Next
            Else
                Me.KeyTiggerPortToolStripMenuItem.Enabled = False
            End If

            'key control type
            If tk.MapPort <> -1 Then
                Me.KeyControlTypeToolStripMenuItem.Enabled = True

                Me.KeyControlTypeToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
                Me.KeyControlTypeToolStripMenuItem.DropDown.Tag = tk.MapPort 'used for log previous value

                Me.KeyFollowToolStripMenuItem.Tag = ControlType.follow
                Me.KeyToggleToolStripMenuItem.Tag = ControlType.toggle
                Me.KeyOneShotToolStripMenuItem.Tag = ControlType.oneshot

                Me.KeyFollowToolStripMenuItem.Checked = False
                Me.KeyOneShotToolStripMenuItem.Checked = False
                Me.KeyToggleToolStripMenuItem.Checked = False
                Select Case tk.ControlType
                    Case ControlType.follow
                        Me.KeyFollowToolStripMenuItem.Checked = True
                    Case ControlType.oneshot
                        Me.KeyOneShotToolStripMenuItem.Checked = True
                    Case ControlType.toggle
                        Me.KeyToggleToolStripMenuItem.Checked = True
                End Select

            Else
                Me.KeyControlTypeToolStripMenuItem.Enabled = False
            End If

            'key sensitivity
            Me.KeySensitivityToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            'Me.KeySensitivityToolStripMenuItem.DropDown.Tag = tk.Sensitivity 'used for log previous value
            Me.KeySensitivityToolStripTextBox.Text = tk.Sensitivity

            'key sensitivity ana
            Me.KeyAnaToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyAnaToolStripMenuItem.DropDown.Tag = tk.SensitivityAna 'used for log previous value
            Me.KeyAnaToolStripTextBox.Text = tk.SensitivityAna

            'key sensitivity dig
            Me.KeyDigToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyDigToolStripMenuItem.DropDown.Tag = tk.SensitivityDig 'used for log previous value
            Me.KeyDigToolStripTextBox.Text = tk.SensitivityDig

            'key noise filter
            Me.KeyNoiseToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyNoiseToolStripMenuItem.DropDown.Tag = tk.NoiseFilter 'used for log previous value
            Me.KeyNoiseToolStripTextBox.Text = tk.NoiseFilter

            'key Deglitch Count
            Me.KeyDeglitchCountToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyDeglitchCountToolStripMenuItem.DropDown.Tag = tk.DeglitchCount 'used for log previous value
            Me.KeyDCountToolStripTextBox.Text = tk.DeglitchCount

            'key map port initial
            Me.KeyMPintitToolStripMenuItem.DropDown.Text = tk.ObjectID ' used for log object
            Me.KeyMPintitToolStripMenuItem.DropDown.Tag = tk.MapPortInit 'used for log previous value
            Me.KeyMPinitToolStripTextBox.Text = tk.MapPortInit

        Catch ex As Exception
            Debug.Assert(ex.Message)

        End Try

    End Sub

    Private Sub KSPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(CType(sender.owner, ToolStripDropDown).Text)

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).SensorPort = i

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub KMPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)
        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).MapPort = i
                                Me.KMPDisableToolStripMenuItem.Checked = False

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub


    Private Sub KMPDisableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KMPDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).MapPort = -1
                                Me.KMPDisableToolStripMenuItem.Checked = True

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub KTPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)


        'sender.Owner.SourceControl()
        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).TiggerPort = i
                                Me.KTPDisableToolStripMenuItem.Checked = False

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used

                                        If TPGT(a, i) <> PortStatus.unavailable Then
                                            TPGT(a, i) = PortStatus.used
                                        End If

                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available

                                            If TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) <> PortStatus.unavailable Then
                                                TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                            End If

                                        End If
                                    End If
                                Next

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub KTPDisableToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KTPDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).TiggerPort = -1
                                Me.KTPDisableToolStripMenuItem.Checked = True

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available

                                            If TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) <> PortStatus.unavailable Then
                                                TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                            End If
                                        End If
                                    End If
                                Next

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub FollowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KeyFollowToolStripMenuItem.Click, KeyToggleToolStripMenuItem.Click, KeyOneShotToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)

        'For Each item As ToolStripMenuItem In Me.ControlTypeToolStripMenuItem.DropDownItems
        '    If item.Tag <> i Then
        '        item.Checked = False
        '    Else
        '        item.Checked = True
        '    End If
        'Next

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchKey).ControlType = i
                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub



#End Region
    'end of methods

#Region "ToolStripMenu"
    Private Sub KeySensitivityToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeySensitivityToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).Sensitivity
                                Else
                                    CType(itm, TouchKey).Sensitivity = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub KeySensitivityToolStripTextBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeySensitivityToolStripTextBox.TextChanged, KeyAnaToolStripTextBox.TextChanged, KeyNoiseToolStripTextBox.TextChanged, KeyDigToolStripTextBox.TextChanged, KeyDCountToolStripTextBox.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub MPinitToolStripTextBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyMPinitToolStripTextBox.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub KeySensitivityAnaToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyAnaToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).SensitivityAna
                                Else
                                    CType(itm, TouchKey).SensitivityAna = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub


    Private Sub KeyDigToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyDigToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).SensitivityDig
                                Else
                                    CType(itm, TouchKey).SensitivityDig = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub NoiseToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyNoiseToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).NoiseFilter
                                Else
                                    CType(itm, TouchKey).NoiseFilter = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub DCountToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyDCountToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).DeglitchCount
                                Else
                                    CType(itm, TouchKey).DeglitchCount = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub MPinitToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyMPinitToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If CType(itm, TouchKey).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If CInt(sender.text.ToString) > 1 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchKey).MapPortInit
                                Else
                                    CType(itm, TouchKey).MapPortInit = CInt(sender.text.ToString)
                                End If

                            End If

                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub reloadSliderToolStripMenuItem(ByVal tb As TouchBar)

        Try
            If tb Is Nothing Then
                Exit Sub
            End If


            Dim a, b, c As Integer

            'key tigger port
            Me.SliderTiggerPortToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderTiggerPortToolStripMenuItem.DropDown.Tag = tb.TiggerPort 'used for log previous value

            If tb.TiggerPort <> -1 Then
                Me.SliderTriggerDisableToolStripMenuItem.Checked = False
            Else
                Me.SliderTriggerDisableToolStripMenuItem.Checked = True
            End If
            Me.STP0ToolStripMenuItem.DropDownItems.Clear()
            Me.STP1ToolStripMenuItem.DropDownItems.Clear()
            Me.STP2ToolStripMenuItem.DropDownItems.Clear()
            Me.STP3ToolStripMenuItem.DropDownItems.Clear()
            If pjd.Scan_type = "Mutual" Then
                Me.SliderTiggerPortToolStripMenuItem.Enabled = True
                For a = 0 To IC.Length - 1
                    If IC(a) = pjd.IC_model Then
                        For b = 0 To maxPort
                            If P(a, b) <> -1 Then
                                Select Case b
                                    Case 0, 1, 2, 3, 4, 5, 6, 7
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P0[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tb.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tb.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.STPmyPrivateMenuItemHandler
                                        Me.STP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.STP0ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                        Me.STP0ToolStripMenuItem.DropDown.Tag = tb.TiggerPort 'used for log previous value
                                    Case 8, 9, 10, 11, 12, 13, 14, 15
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P1[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tb.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tb.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.STPmyPrivateMenuItemHandler
                                        Me.STP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.STP1ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                        Me.STP1ToolStripMenuItem.DropDown.Tag = tb.TiggerPort 'used for log previous value
                                    Case 16, 17, 18, 19, 20, 21, 22, 23
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P2[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tb.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tb.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.STPmyPrivateMenuItemHandler
                                        Me.STP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.STP2ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                        Me.STP2ToolStripMenuItem.DropDown.Tag = tb.TiggerPort 'used for log previous value
                                    Case 24, 25, 26, 27, 28, 29, 30
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P3[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tb.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tb.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.STPmyPrivateMenuItemHandler
                                        Me.STP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.STP3ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                        Me.STP3ToolStripMenuItem.DropDown.Tag = tb.TiggerPort 'used for log previous value
                                End Select
                            End If
                        Next

                    End If
                Next
            Else
                Me.SliderTiggerPortToolStripMenuItem.Enabled = False
            End If

            'direction
            Me.SliderDirectionToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderDirectionToolStripMenuItem.DropDown.Tag = tb.Direction 'used for log previous value

            If tb.Direction = Direction.left Then
                Me.SliderLeftToolStripMenuItem.Checked = True
                Me.SliderRightToolStripMenuItem.Checked = False
            Else
                Me.SliderLeftToolStripMenuItem.Checked = False
                Me.SliderRightToolStripMenuItem.Checked = True
            End If

            'mapping pwmx
            Me.SliderPWMXToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderPWMXToolStripMenuItem.DropDown.Tag = tb.MapPWM 'used for log previous value

            Me.SliderPWMXDisableToolStripMenuItem1.Checked = False
            Me.SliderPWM0ToolStripMenuItem.Checked = False
            Me.SliderPWM1ToolStripMenuItem.Checked = False
            Me.SliderPWM2ToolStripMenuItem.Checked = False
            Me.SliderPWM3ToolStripMenuItem.Checked = False
            Me.SliderPWM4ToolStripMenuItem.Checked = False
            Me.SliderPWM5ToolStripMenuItem.Checked = False
            Me.SliderPWM6ToolStripMenuItem.Checked = False
            Me.SliderPWM7ToolStripMenuItem.Checked = False

            Select Case tb.MapPWM
                Case -1
                    Me.SliderPWMXDisableToolStripMenuItem1.Checked = True
                Case 0
                    Me.SliderPWM0ToolStripMenuItem.Checked = True
                Case 1
                    Me.SliderPWM1ToolStripMenuItem.Checked = True
                Case 2
                    Me.SliderPWM2ToolStripMenuItem.Checked = True
                Case 3
                    Me.SliderPWM3ToolStripMenuItem.Checked = True
                Case 4
                    Me.SliderPWM4ToolStripMenuItem.Checked = True
                Case 5
                    Me.SliderPWM5ToolStripMenuItem.Checked = True
                Case 6
                    Me.SliderPWM6ToolStripMenuItem.Checked = True
                Case 7
                    Me.SliderPWM7ToolStripMenuItem.Checked = True

            End Select

            'slider type
            Me.SliderTypeToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderTypeToolStripMenuItem.DropDown.Tag = tb.ValueType 'used for log previous value
            If tb.ValueType = ValueType.absolute Then
                Me.SliderTypeToolStripComboBox.Text = "Absolute"
            Else
                Me.SliderTypeToolStripComboBox.Text = "Relative"
            End If


            'step value
            Me.SliderStepValueToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderStepValueToolStripMenuItem.DropDown.Tag = tb.StepValue 'used for log previous value

            If tb.ValueType = ValueType.relative Then
                Me.SliderStepValueToolStripMenuItem.Enabled = True
            Else
                Me.SliderStepValueToolStripMenuItem.Enabled = False
            End If

            'start value
            Me.SliderStartValueToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderStartValueToolStripMenuItem.DropDown.Tag = tb.StartValue 'used for log previous value
            Me.SliderStartToolStripTextBox.Text = tb.StartValue

            'stop value
            Me.SliderStopValueToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderStopValueToolStripMenuItem.DropDown.Tag = tb.StopValue 'used for log previous value
            Me.SliderStopToolStripTextBox.Text = tb.StopValue

            'interpolation
            Me.SliderInterpolationToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderInterpolationToolStripMenuItem.DropDown.Tag = tb.Interpolation 'used for log previous value
            Me.SliderInterToolStripComboBox.Text = tb.Interpolation

            If tb.ValueType = ValueType.absolute Then
                Me.SliderInterpolationToolStripMenuItem.Enabled = True
            Else
                Me.SliderInterpolationToolStripMenuItem.Enabled = False
            End If

            'sensitivity
            Me.SliderSensitivityToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderSensitivityToolStripMenuItem.DropDown.Tag = tb.Sensitivity 'used for log previous value
            Me.SliderSensitivityToolStripTextBox.Text = tb.Sensitivity


            'speed vector
            Me.SliderSpeedToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderSpeedToolStripMenuItem.DropDown.Tag = tb.SpeedVector 'used for log previous value
            Me.SliderSpeedToolStripTextBox1.Text = tb.SpeedVector

            'noise
            Me.SliderNoiseToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderNoiseToolStripMenuItem.DropDown.Tag = tb.NoiseFilterOuter 'used for log previous value
            Me.SliderNoiseToolStripTextBox1.Text = tb.NoiseFilterOuter




        Catch ex As Exception
            Debug.Assert(ex.Message)

        End Try

    End Sub

    Private Sub STPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(CType(sender.owner, ToolStripDropDown).Text)

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).TiggerPort = i
                                Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used

                                        If TPGT(a, i) <> PortStatus.unavailable Then
                                            TPGT(a, i) = PortStatus.used
                                        End If

                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available

                                            If TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) <> PortStatus.unavailable Then
                                                TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                            End If

                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub SliderLeftToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SliderLeftToolStripMenuItem.Click, SliderRightToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                'CType(itm, TouchKey).ControlType = i
                                CType(itm, TouchBar).Direction = i
                                syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                            End If

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderPWM0ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SliderPWMXDisableToolStripMenuItem1.Click, SliderPWM0ToolStripMenuItem.Click, SliderPWM1ToolStripMenuItem.Click, SliderPWM2ToolStripMenuItem.Click, SliderPWM3ToolStripMenuItem.Click, SliderPWM4ToolStripMenuItem.Click, SliderPWM5ToolStripMenuItem.Click, SliderPWM6ToolStripMenuItem.Click, SliderPWM7ToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                'CType(itm, TouchKey).ControlType = i
                                CType(itm, TouchBar).MapPWM = i
                                syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                            End If

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderTriggerDisableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SliderTriggerDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).TiggerPort = -1
                                Me.SliderTriggerDisableToolStripMenuItem.Checked = True
                                Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then

                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderTypeToolStripComboBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderTypeToolStripComboBox.LostFocus
        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If sender.text.ToString = "Absolute" Then
                                    Me.SliderInterpolationToolStripMenuItem.Enabled = True
                                    Me.SliderStepValueToolStripMenuItem.Enabled = False
                                    If CType(sender.owner, ToolStripDropDown).Tag <> ValueType.absolute Then
                                        CType(itm, TouchBar).ValueType = ValueType.absolute
                                        Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                    End If
                                Else
                                    Me.SliderInterpolationToolStripMenuItem.Enabled = False
                                    Me.SliderStepValueToolStripMenuItem.Enabled = True
                                    If CType(sender.owner, ToolStripDropDown).Tag <> ValueType.relative Then
                                        CType(itm, TouchBar).ValueType = ValueType.relative
                                        Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                    End If

                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub SliderStepToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderStepToolStripTextBox.LostFocus

        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 255 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).StepValue
                                Else
                                    CType(itm, TouchBar).StepValue = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderStepToolStripTextBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderStepToolStripTextBox.TextChanged, RotorStepToolStripTextBox.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~255間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~255" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))

            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 255 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~255間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~255" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub SliderStartToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderStartToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If
        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 65535 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).StartValue
                                Else
                                    CType(itm, TouchBar).StartValue = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderStartToolStripTextBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderStartToolStripTextBox.TextChanged, SliderStopToolStripTextBox.TextChanged, RotorStopToolStripTextBox.TextChanged, RotorStartToolStripTextBox.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~65535間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~65535" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 65535 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~65535間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~65535" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub SliderStopToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderStopToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 65535 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).StopValue
                                Else
                                    CType(itm, TouchBar).StopValue = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub SliderInterToolStripComboBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderInterToolStripComboBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True


        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) <> CType(sender.owner, ToolStripDropDown).Tag Then
                                    CType(itm, TouchBar).Interpolation = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderSensitivityToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderSensitivityToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True


        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).Sensitivity
                                Else
                                    CType(itm, TouchBar).Sensitivity = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderSensitivityToolStripTextBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderSensitivityToolStripTextBox.TextChanged, SliderSpeedToolStripTextBox1.TextChanged, SliderNoiseToolStripTextBox1.TextChanged, RotorNoiseToolStripTextBox.TextChanged, RotorSensitivityToolStripTextBox.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~15間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~15" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub SliderSpeedToolStripTextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderSpeedToolStripTextBox1.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True


        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).SpeedVector
                                Else
                                    CType(itm, TouchBar).SpeedVector = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderNoiseToolStripTextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderNoiseToolStripTextBox1.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).NoiseFilterOuter
                                Else
                                    CType(itm, TouchBar).NoiseFilterOuter = CInt(sender.text.ToString)
                                    Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub ContextMenuStripSliderKey_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStripSliderKey.Opening
        'Dim SenderLine As LineControl.Line = sender.SourceControl

        'Me.ToolStripTextBox1.Text = SenderLine.LineWidth
        'Me.ToolStripMenuItem2.Checked = SenderLine.IsFlashing

        If TypeOf sender.SourceControl Is TouchKey Then
            Dim tk As TouchKey = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is TouchBar Then
            Dim tb As TouchBar = sender.SourceControl

            Me.reloadSliderKeyToolStripMenuItem(tb)

        ElseIf TypeOf sender.SourceControl Is TouchPie Then
            Dim tp As TouchPie = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is GroupBox Then
            If sender.SourceControl.name = "keygroup" Then
                'Debug.Print(sender.Owner.SourceControl.tag.ToString())
                Dim itm As Control
                For Each itm In sender.SourceControl.controls
                    If TypeOf itm Is TouchKey Then
                        Dim tk As TouchKey = itm
                        'MsgBox(tk.ObjectID)
                        Me.reloadKeyToolStripMenuItem(tk)
                        Exit For
                    End If
                Next


            ElseIf sender.SourceControl.name = "slidergroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchBar Then
                        Dim tb As TouchBar = itm
                        Me.reloadSliderToolStripMenuItem(tb)
                        Exit For
                    End If
                Next

            ElseIf sender.SourceControl.name = "rotatorgroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchPie Then
                        Dim tp As TouchPie = itm

                    End If

                Next

            End If
        End If
    End Sub
    Private Sub reloadSliderKeyToolStripMenuItem(ByVal tb As TouchBar)
        Try

            If tb Is Nothing Then
                Exit Sub
            End If

            Dim a, b, c As Integer
            'Key sensor port
            Me.SliderKeySP0ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeySP1ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeySP2ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeySP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKSPmyPrivateMenuItemHandler
                                    Me.SliderKeySP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeySP0ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeySP0ToolStripMenuItem.DropDown.Tag = tb.SensorPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKSPmyPrivateMenuItemHandler
                                    Me.SliderKeySP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeySP1ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeySP1ToolStripMenuItem.DropDown.Tag = tb.SensorPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKSPmyPrivateMenuItemHandler
                                    Me.SliderKeySP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeySP2ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeySP2ToolStripMenuItem.DropDown.Tag = tb.SensorPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKSPmyPrivateMenuItemHandler
                                    Me.SliderKeySP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeySP3ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeySP3ToolStripMenuItem.DropDown.Tag = tb.SensorPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next

            'key mapping port
            Me.SliderKeyMappingPortToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderKeyMappingPortToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value

            If tb.MapPort <> -1 Then
                Me.SliderKeyMPDisableToolStripMenuItem.Checked = False
            Else
                Me.SliderKeyMPDisableToolStripMenuItem.Checked = True
            End If
            Me.SliderKeyMP0ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeyMP1ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeyMP2ToolStripMenuItem.DropDownItems.Clear()
            Me.SliderKeyMP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKMPmyPrivateMenuItemHandler
                                    Me.SliderKeyMP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeyMP0ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeyMP0ToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKMPmyPrivateMenuItemHandler
                                    Me.SliderKeyMP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeyMP1ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeyMP1ToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKMPmyPrivateMenuItemHandler
                                    Me.SliderKeyMP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeyMP2ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeyMP2ToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tb.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKMPmyPrivateMenuItemHandler
                                    Me.SliderKeyMP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.SliderKeyMP3ToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                                    Me.SliderKeyMP3ToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next


            'key control type
            If tb.MapPort <> -1 Then
                Me.SliderKeyControlTypeToolStripMenuItem.Enabled = True

                Me.SliderKeyControlTypeToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                Me.SliderKeyControlTypeToolStripMenuItem.DropDown.Tag = tb.MapPort 'used for log previous value

                Me.SliderKeyFollowToolStripMenuItem.Tag = ControlType.follow
                Me.SliderKeyToggleToolStripMenuItem.Tag = ControlType.toggle
                Me.SliderKeyOneShotToolStripMenuItem.Tag = ControlType.oneshot

                Me.SliderKeyFollowToolStripMenuItem.Checked = False
                Me.SliderKeyOneShotToolStripMenuItem.Checked = False
                Me.SliderKeyToggleToolStripMenuItem.Checked = False
                Select Case tb.ControlType
                    Case ControlType.follow
                        Me.SliderKeyFollowToolStripMenuItem.Checked = True
                    Case ControlType.oneshot
                        Me.SliderKeyOneShotToolStripMenuItem.Checked = True
                    Case ControlType.toggle
                        Me.SliderKeyToggleToolStripMenuItem.Checked = True
                End Select

            Else
                Me.SliderKeyControlTypeToolStripMenuItem.Enabled = False
            End If

            'key noise filter
            Me.SliderKeyNoiseToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderKeyNoiseToolStripMenuItem.DropDown.Tag = tb.NoiseFilter 'used for log previous value
            Me.SliderKeyNoiseToolStripTextBox1.Text = tb.NoiseFilter

            'key Deglitch Count
            Me.SliderKeyDeglitchVolumeToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
            Me.SliderKeyDeglitchVolumeToolStripMenuItem.DropDown.Tag = tb.DeglitchCount 'used for log previous value
            Me.SliderKeyDCToolStripTextBox1.Text = tb.DeglitchCount

            'key map port initial
            If tb.MapPort <> -1 Then
                Me.SliderKeyMPinitToolStripMenuItem.Enabled = True

                Me.SliderKeyMPinitToolStripMenuItem.DropDown.Text = tb.ObjectID ' used for log object
                Me.SliderKeyMPinitToolStripMenuItem.DropDown.Tag = tb.SensitivityAna 'used for log previous value
                Me.SliderKeyMPinitToolStripTextBox1.Text = tb.MapPortInit


            Else
                Me.SliderKeyMPinitToolStripMenuItem.Enabled = False
            End If


        Catch ex As Exception
            Debug.Assert(ex.Message)

        End Try
    End Sub

    Private Sub SKSPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)

        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(CType(sender.owner, ToolStripDropDown).Text)

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).SensorPort = i

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SKMPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)
        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).MapPort = i
                                Me.SliderKeyMPDisableToolStripMenuItem.Checked = False

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderKeyMPDisableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SliderKeyMPDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).MapPort = -1
                                Me.SliderKeyMPDisableToolStripMenuItem.Checked = True

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub SliderKeyFollowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SliderKeyFollowToolStripMenuItem.Click, SliderKeyToggleToolStripMenuItem.Click, SliderKeyOneShotToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)

        'For Each item As ToolStripMenuItem In Me.ControlTypeToolStripMenuItem.DropDownItems
        '    If item.Tag <> i Then
        '        item.Checked = False
        '    Else
        '        item.Checked = True
        '    End If
        'Next

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchBar).ControlType = i
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderKeyNoiseToolStripTextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderKeyNoiseToolStripTextBox1.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 7 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).NoiseFilter
                                Else
                                    CType(itm, TouchBar).NoiseFilter = CInt(sender.text.ToString)
                                    'Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub SliderKeyNoiseToolStripTextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderKeyNoiseToolStripTextBox1.TextChanged, SliderKeyDCToolStripTextBox1.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~7間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~7" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 7 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~7間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~7" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub SliderKeyDCToolStripTextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderKeyDCToolStripTextBox1.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).DeglitchCount
                                Else
                                    CType(itm, TouchBar).DeglitchCount = CInt(sender.text.ToString)
                                    'Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub SliderKeyMPinitToolStripTextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderKeyMPinitToolStripTextBox1.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If CType(itm, TouchBar).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchBar).MapPortInit
                                Else
                                    CType(itm, TouchBar).MapPortInit = CInt(sender.text.ToString)
                                    'Me.syncSliderGroupInfo(CType(ctl, GroupBox), CType(itm, TouchBar))
                                End If

                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub SliderKeyMPinitToolStripTextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles SliderKeyMPinitToolStripTextBox1.TextChanged
        If Not IsNumeric(sender.text.ToString) Then
            If sender.text <> "" Then
                'MsgBox("請輸入介於0~1間數值", MsgBoxStyle.DefaultButton2 Or _
                MsgBox(RM.GetString("Only available in") & "0~1" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                         MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
            End If
            Exit Sub
        End If

        If CInt(sender.text.ToString) > 1 Or CInt(sender.text.ToString) < 0 Then
            'MsgBox("請輸入介於0~1間數值", MsgBoxStyle.DefaultButton2 Or _
            MsgBox(RM.GetString("Only available in") & "0~1" & RM.GetString("values"), MsgBoxStyle.DefaultButton2 Or _
                     MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, RM.GetString("warning"))
        End If
    End Sub

    Private Sub ContextMenuStripRotor_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStripRotor.Opening
        'Dim SenderLine As LineControl.Line = sender.SourceControl

        'Me.ToolStripTextBox1.Text = SenderLine.LineWidth
        'Me.ToolStripMenuItem2.Checked = SenderLine.IsFlashing

        If TypeOf sender.SourceControl Is TouchKey Then
            Dim tk As TouchKey = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is TouchBar Then
            Dim tb As TouchBar = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is TouchPie Then
            Dim tp As TouchPie = sender.SourceControl

        ElseIf TypeOf sender.SourceControl Is GroupBox Then
            If sender.SourceControl.name = "keygroup" Then
                'Debug.Print(sender.Owner.SourceControl.tag.ToString())
                Dim itm As Control
                For Each itm In sender.SourceControl.controls
                    If TypeOf itm Is TouchKey Then
                        Dim tk As TouchKey = itm
                        'MsgBox(tk.ObjectID)
                        Me.reloadKeyToolStripMenuItem(tk)
                        Exit For
                    End If
                Next


            ElseIf sender.SourceControl.name = "slidergroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchBar Then
                        Dim tb As TouchBar = itm
                        Me.reloadSliderToolStripMenuItem(tb)
                        Exit For
                    End If
                Next

            ElseIf sender.SourceControl.name = "rotatorgroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchPie Then
                        Dim tp As TouchPie = itm
                        Me.reloadRotorToolStripMenuItem(tp)
                        Exit For
                    End If

                Next

            End If
        End If
    End Sub

    Private Sub reloadRotorToolStripMenuItem(ByVal tp As TouchPie)

        Try
            If tp Is Nothing Then
                Exit Sub
            End If

            Dim a, b, c As Integer

            'key tigger port
            Me.RotorTriggerPortToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorTriggerPortToolStripMenuItem.DropDown.Tag = tp.TiggerPort 'used for log previous value

            If tp.TiggerPort <> -1 Then
                Me.RotorTPDisableToolStripMenuItem.Checked = False
            Else
                Me.RotorTPDisableToolStripMenuItem.Checked = True
            End If
            Me.RotorTP0ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorTP1ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorTP2ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorTP3ToolStripMenuItem.DropDownItems.Clear()
            If pjd.Scan_type = "Mutual" Then
                Me.RotorTriggerPortToolStripMenuItem.Enabled = True
                For a = 0 To IC.Length - 1
                    If IC(a) = pjd.IC_model Then
                        For b = 0 To maxPort
                            If P(a, b) <> -1 Then
                                Select Case b
                                    Case 0, 1, 2, 3, 4, 5, 6, 7
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P0[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tp.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tp.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.RTPmyPrivateMenuItemHandler
                                        Me.RotorTP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.RotorTP0ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                        Me.RotorTP0ToolStripMenuItem.DropDown.Tag = tp.TiggerPort 'used for log previous value
                                    Case 8, 9, 10, 11, 12, 13, 14, 15
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P1[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tp.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tp.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.RTPmyPrivateMenuItemHandler
                                        Me.RotorTP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.RotorTP1ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                        Me.RotorTP1ToolStripMenuItem.DropDown.Tag = tp.TiggerPort 'used for log previous value
                                    Case 16, 17, 18, 19, 20, 21, 22, 23
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P2[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tp.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tp.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.RTPmyPrivateMenuItemHandler
                                        Me.RotorTP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.RotorTP2ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                        Me.RotorTP2ToolStripMenuItem.DropDown.Tag = tp.TiggerPort 'used for log previous value
                                    Case 24, 25, 26, 27, 28, 29, 30
                                        c = b Mod 8
                                        Dim myMenuItem As New ToolStripMenuItem
                                        myMenuItem.Text = "P3[" & c & "]"
                                        myMenuItem.Tag = b
                                        If P(a, b) = PortStatus.available Then
                                            myMenuItem.Enabled = True
                                        ElseIf P(a, b) = PortStatus.used Then
                                            If tp.TiggerPort = b Then
                                                myMenuItem.Enabled = True
                                                myMenuItem.Checked = True
                                            Else
                                                myMenuItem.Enabled = False
                                                myMenuItem.Checked = True
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        If TPGT(a, b) <> PortStatus.unavailable Then
                                            If TPGT(a, b) = PortStatus.used Then
                                                If tp.TiggerPort <> b Then
                                                    myMenuItem.Enabled = True
                                                    myMenuItem.Checked = False
                                                End If
                                            End If
                                        Else
                                            myMenuItem.Enabled = False
                                        End If

                                        AddHandler myMenuItem.Click, AddressOf Me.RTPmyPrivateMenuItemHandler
                                        Me.RotorTP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                        Me.RotorTP3ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                        Me.RotorTP3ToolStripMenuItem.DropDown.Tag = tp.TiggerPort 'used for log previous value
                                End Select
                            End If
                        Next

                    End If
                Next
            Else
                Me.RotorTriggerPortToolStripMenuItem.Enabled = False
            End If

            'direction
            Me.RotorDirectionToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorDirectionToolStripMenuItem.DropDown.Tag = tp.Direction 'used for log previous value

            If tp.Direction = Direction.counterclockwise Then
                Me.RotorContraclockwiseToolStripMenuItem.Checked = True
                Me.RotorClockwiseToolStripMenuItem.Checked = False
            Else
                Me.RotorContraclockwiseToolStripMenuItem.Checked = False
                Me.RotorClockwiseToolStripMenuItem.Checked = True
            End If

            'mapping pwmx
            Me.RotorPWMToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorPWMToolStripMenuItem.DropDown.Tag = tp.MapPWM 'used for log previous value

            Me.RotorPWMDisableToolStripMenuItem.Checked = False
            Me.RotorPWM0ToolStripMenuItem.Checked = False
            Me.RotorPWM1ToolStripMenuItem.Checked = False
            Me.RotorPWM2ToolStripMenuItem.Checked = False
            Me.RotorPWM3ToolStripMenuItem.Checked = False
            Me.RotorPWM4ToolStripMenuItem.Checked = False
            Me.RotorPWM5ToolStripMenuItem.Checked = False
            Me.RotorPWM6ToolStripMenuItem.Checked = False
            Me.RotorPWM7ToolStripMenuItem.Checked = False

            Select Case tp.MapPWM
                Case -1
                    Me.RotorPWMDisableToolStripMenuItem.Checked = True
                Case 0
                    Me.RotorPWM0ToolStripMenuItem.Checked = True
                Case 1
                    Me.RotorPWM1ToolStripMenuItem.Checked = True
                Case 2
                    Me.RotorPWM2ToolStripMenuItem.Checked = True
                Case 3
                    Me.RotorPWM3ToolStripMenuItem.Checked = True
                Case 4
                    Me.RotorPWM4ToolStripMenuItem.Checked = True
                Case 5
                    Me.RotorPWM5ToolStripMenuItem.Checked = True
                Case 6
                    Me.RotorPWM6ToolStripMenuItem.Checked = True
                Case 7
                    Me.RotorPWM7ToolStripMenuItem.Checked = True

            End Select

            'rotor type
            Me.RotorTypeToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorTypeToolStripMenuItem.DropDown.Tag = tp.ValueType 'used for log previous value
            If tp.ValueType = ValueType.absolute Then
                Me.RotorTypeToolStripComboBox.Text = "Absolute"
            Else
                Me.RotorTypeToolStripComboBox.Text = "Relative"
            End If

            'step value
            Me.RotorStepValueToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorStepValueToolStripMenuItem.DropDown.Tag = tp.StepValue 'used for log previous value

            If tp.ValueType = ValueType.relative Then
                Me.RotorStepValueToolStripMenuItem.Enabled = True
            Else
                Me.RotorStepValueToolStripMenuItem.Enabled = False
            End If

            'start value
            Me.RotorStartValueToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorStartValueToolStripMenuItem.DropDown.Tag = tp.StartValue 'used for log previous value
            Me.RotorStartToolStripTextBox.Text = tp.StartValue

            'stop value
            Me.RotorStopValueToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorStopValueToolStripMenuItem.DropDown.Tag = tp.StopValue 'used for log previous value
            Me.RotorStopToolStripTextBox.Text = tp.StopValue

            'interpolation
            Me.RotorInterpolationToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorInterpolationToolStripMenuItem.DropDown.Tag = tp.Interpolation 'used for log previous value
            Me.RotorInterpolationToolStripTextBox.Text = tp.Interpolation

            If tp.ValueType = ValueType.absolute Then
                Me.RotorInterpolationToolStripMenuItem.Enabled = True
            Else
                Me.RotorInterpolationToolStripMenuItem.Enabled = False
            End If

            'sensitivity
            Me.RotorSensitivityToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorSensitivityToolStripMenuItem.DropDown.Tag = tp.Sensitivity 'used for log previous value
            Me.RotorSensitivityToolStripTextBox.Text = tp.Sensitivity


            ''spee vector
            'Me.RotorSpeedVectorToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            'Me.RotorSpeedVectorToolStripMenuItem.DropDown.Tag = tp.SpeedVector 'used for log previous value
            'Me.RotorSpeedVectorToolStripMenuItem.Text = tp.SpeedVector

            'noise
            Me.RotorNoiseFilterLevelToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorNoiseFilterLevelToolStripMenuItem.DropDown.Tag = tp.NoiseFilterOuter 'used for log previous value
            Me.RotorNoiseToolStripTextBox.Text = tp.NoiseFilterOuter



        Catch ex As Exception
            Debug.Assert(ex.Message)

        End Try

    End Sub

    Private Sub RTPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(CType(sender.owner, ToolStripDropDown).Text)

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).TiggerPort = i
                                Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used

                                        If TPGT(a, i) <> PortStatus.unavailable Then
                                            TPGT(a, i) = PortStatus.used
                                        End If

                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available

                                            If TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) <> PortStatus.unavailable Then
                                                TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                            End If

                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl

    End Sub

    Private Sub RotorClockwiseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotorClockwiseToolStripMenuItem.Click, RotorContraclockwiseToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls

                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If TypeOf itm Is TouchPie Then
                                If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                    'CType(itm, TouchKey).ControlType = i
                                    CType(itm, TouchPie).Direction = i
                                    syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub RotorTPDisableToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotorTPDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).TiggerPort = -1
                                Me.RotorTPDisableToolStripMenuItem.Checked = True
                                Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available

                                            If TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) <> PortStatus.unavailable Then
                                                TPGT(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                            End If
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorPWMDisableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotorPWMDisableToolStripMenuItem.Click, RotorPWM0ToolStripMenuItem.Click, RotorPWM1ToolStripMenuItem.Click, RotorPWM2ToolStripMenuItem.Click, RotorPWM3ToolStripMenuItem.Click, RotorPWM4ToolStripMenuItem.Click, RotorPWM5ToolStripMenuItem.Click, RotorPWM6ToolStripMenuItem.Click, RotorPWM7ToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                'CType(itm, TouchKey).ControlType = i
                                CType(itm, TouchPie).MapPWM = i
                                syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub RotorTypeToolStripComboBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorTypeToolStripComboBox.LostFocus
        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then

                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                If sender.text.ToString = "Absolute" Then
                                    Me.RotorInterpolationToolStripMenuItem.Enabled = True
                                    Me.RotorStepValueToolStripMenuItem.Enabled = False
                                    If CType(sender.owner, ToolStripDropDown).Tag <> ValueType.absolute Then
                                        CType(itm, TouchPie).ValueType = ValueType.absolute
                                        Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                    End If
                                Else
                                    Me.RotorInterpolationToolStripMenuItem.Enabled = False
                                    Me.RotorStepValueToolStripMenuItem.Enabled = True
                                    If CType(sender.owner, ToolStripDropDown).Tag <> ValueType.relative Then
                                        CType(itm, TouchPie).ValueType = ValueType.relative
                                        Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                    End If

                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorStopToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorStopToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 65535 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).StopValue
                                Else
                                    CType(itm, TouchPie).StopValue = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorStepToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorStepToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 255 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).StepValue
                                Else
                                    CType(itm, TouchPie).StepValue = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorStartToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorStartToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 65535 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).StartValue
                                Else
                                    CType(itm, TouchPie).StartValue = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorInterpolationToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorInterpolationToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) <> CType(sender.owner, ToolStripDropDown).Tag Then
                                    CType(itm, TouchPie).Interpolation = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub RotorSensitivityToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorSensitivityToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).Sensitivity
                                Else
                                    CType(itm, TouchPie).Sensitivity = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorNoiseToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorNoiseToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).NoiseFilterOuter
                                Else
                                    CType(itm, TouchPie).NoiseFilterOuter = CInt(sender.text.ToString)
                                    Me.syncRotorGroupInfo(CType(ctl, GroupBox), CType(itm, TouchPie))
                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub ContextMenuStripRotorKey_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStripRotorKey.Opening
        'Dim SenderLine As LineControl.Line = sender.SourceControl

        'Me.ToolStripTextBox1.Text = SenderLine.LineWidth
        'Me.ToolStripMenuItem2.Checked = SenderLine.IsFlashing

        If TypeOf sender.SourceControl Is TouchKey Then
            Dim tk As TouchKey = sender.SourceControl

            Me.reloadKeyToolStripMenuItem(tk)

        ElseIf TypeOf sender.SourceControl Is TouchBar Then
            Dim tb As TouchBar = sender.SourceControl

            Me.reloadSliderKeyToolStripMenuItem(tb)

        ElseIf TypeOf sender.SourceControl Is TouchPie Then
            Dim tp As TouchPie = sender.SourceControl

            Me.reloadRotorKeyToolStripMenuItem(tp)

        ElseIf TypeOf sender.SourceControl Is GroupBox Then
            If sender.SourceControl.name = "keygroup" Then
                'Debug.Print(sender.Owner.SourceControl.tag.ToString())
                Dim itm As Control
                For Each itm In sender.SourceControl.controls
                    If TypeOf itm Is TouchKey Then
                        Dim tk As TouchKey = itm
                        'MsgBox(tk.ObjectID)
                        Me.reloadKeyToolStripMenuItem(tk)
                        Exit For
                    End If
                Next


            ElseIf sender.SourceControl.name = "slidergroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchBar Then
                        Dim tb As TouchBar = itm
                        Me.reloadSliderToolStripMenuItem(tb)
                        Exit For
                    End If
                Next

            ElseIf sender.SourceControl.name = "rotatorgroup" Then
                Dim itm As Control
                For Each itm In sender.SourceControl.Controls
                    If TypeOf itm Is TouchPie Then
                        Dim tp As TouchPie = itm
                        Me.reloadRotorToolStripMenuItem(tp)
                        Exit For

                    End If

                Next

            End If
        End If
    End Sub

    Private Sub reloadRotorKeyToolStripMenuItem(ByVal tp As TouchPie)
        Try

            If tp Is Nothing Then
                Exit Sub
            End If

            Dim a, b, c As Integer
            'Key sensor port
            Me.RotorKeySP0ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeySP1ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeySP2ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeySP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKSPmyPrivateMenuItemHandler
                                    Me.RotorKeySP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeySP0ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeySP0ToolStripMenuItem.DropDown.Tag = tp.SensorPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKSPmyPrivateMenuItemHandler
                                    Me.RotorKeySP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeySP2ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeySP3ToolStripMenuItem.DropDown.Tag = tp.SensorPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKSPmyPrivateMenuItemHandler
                                    Me.RotorKeySP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeySP2ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeySP2ToolStripMenuItem.DropDown.Tag = tp.SensorPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.SensorPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.SKSPmyPrivateMenuItemHandler
                                    Me.RotorKeySP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeySP3ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeySP3ToolStripMenuItem.DropDown.Tag = tp.SensorPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next

            'key mapping port
            Me.RotorKeyMappingPortToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorKeyMappingPortToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value

            If tp.MapPort <> -1 Then
                Me.RotorKeyMPDisableToolStripMenuItem.Checked = False
            Else
                Me.RotorKeyMPDisableToolStripMenuItem.Checked = True
            End If
            Me.RotorKeyMP0ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeyMP1ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeyMP2ToolStripMenuItem.DropDownItems.Clear()
            Me.RotorKeyMP3ToolStripMenuItem.DropDownItems.Clear()
            For a = 0 To IC.Length - 1
                If IC(a) = pjd.IC_model Then
                    For b = 0 To maxPort
                        If P(a, b) <> -1 Then
                            Select Case b
                                Case 0, 1, 2, 3, 4, 5, 6, 7
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P0[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKMPmyPrivateMenuItemHandler
                                    Me.RotorKeyMP0ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeyMP0ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeyMP0ToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value
                                Case 8, 9, 10, 11, 12, 13, 14, 15
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P1[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKMPmyPrivateMenuItemHandler
                                    Me.RotorKeyMP1ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeyMP1ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeyMP1ToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value
                                Case 16, 17, 18, 19, 20, 21, 22, 23
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P2[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKMPmyPrivateMenuItemHandler
                                    Me.RotorKeyMP2ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeyMP2ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeyMP2ToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value
                                Case 24, 25, 26, 27, 28, 29, 30
                                    c = b Mod 8
                                    Dim myMenuItem As New ToolStripMenuItem
                                    myMenuItem.Text = "P3[" & c & "]"
                                    myMenuItem.Tag = b
                                    If P(a, b) = PortStatus.available Then
                                        myMenuItem.Enabled = True
                                    ElseIf P(a, b) = PortStatus.used Then
                                        If tp.MapPort = b Then
                                            myMenuItem.Enabled = True
                                            myMenuItem.Checked = True
                                        Else
                                            myMenuItem.Enabled = False
                                            myMenuItem.Checked = True
                                        End If
                                    Else
                                        myMenuItem.Enabled = False
                                    End If
                                    AddHandler myMenuItem.Click, AddressOf Me.RKMPmyPrivateMenuItemHandler
                                    Me.RotorKeyMP3ToolStripMenuItem.DropDownItems.Add(myMenuItem)
                                    Me.RotorKeyMP3ToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                                    Me.RotorKeyMP3ToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value
                            End Select
                        End If
                    Next

                End If
            Next


            'key control type
            If tp.MapPort <> -1 Then
                Me.RotorKeyControlTypeToolStripMenuItem.Enabled = True

                Me.RotorKeyControlTypeToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                Me.RotorKeyControlTypeToolStripMenuItem.DropDown.Tag = tp.MapPort 'used for log previous value

                Me.RotorKeyFollowToolStripMenuItem.Tag = ControlType.follow
                Me.RotorKeyToggleToolStripMenuItem.Tag = ControlType.toggle
                Me.RotorKeyOneShotToolStripMenuItem.Tag = ControlType.oneshot

                Me.RotorKeyFollowToolStripMenuItem.Checked = False
                Me.RotorKeyOneShotToolStripMenuItem.Checked = False
                Me.RotorKeyToggleToolStripMenuItem.Checked = False
                Select Case tp.ControlType
                    Case ControlType.follow
                        Me.RotorKeyFollowToolStripMenuItem.Checked = True
                    Case ControlType.oneshot
                        Me.RotorKeyOneShotToolStripMenuItem.Checked = True
                    Case ControlType.toggle
                        Me.RotorKeyToggleToolStripMenuItem.Checked = True
                End Select

            Else
                Me.RotorKeyControlTypeToolStripMenuItem.Enabled = False
            End If

            'key noise filter
            Me.RotorKeyNoiseToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorKeyNoiseToolStripMenuItem.DropDown.Tag = tp.NoiseFilter 'used for log previous value
            Me.RotorKeyNoiseToolStripTextBox.Text = tp.NoiseFilter

            'key Deglitch Count
            Me.RotorKeyDCToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
            Me.RotorKeyDCToolStripMenuItem.DropDown.Tag = tp.DeglitchCount 'used for log previous value
            Me.RotorKeyDCToolStripTextBox.Text = tp.DeglitchCount

            'key map port initial
            If tp.MapPort <> -1 Then
                Me.RotorKeyMPinitToolStripMenuItem.Enabled = True

                Me.RotorKeyMPinitToolStripMenuItem.DropDown.Text = tp.ObjectID ' used for log object
                Me.RotorKeyMPinitToolStripMenuItem.DropDown.Tag = tp.SensitivityAna 'used for log previous value
                Me.RotorKeyMPinitToolStripTextBox.Text = tp.MapPortInit


            Else
                Me.RotorKeyMPinitToolStripMenuItem.Enabled = False
            End If


        Catch ex As Exception
            Debug.Assert(ex.Message)

        End Try
    End Sub

    Private Sub RKSPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(CType(sender.owner, ToolStripDropDown).Text)

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).SensorPort = i

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RKMPmyPrivateMenuItemHandler(ByVal sender As Object, ByVal e As EventArgs)
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)
        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).MapPort = i
                                Me.RotorKeyMPDisableToolStripMenuItem.Checked = False

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        P(a, i) = PortStatus.used
                                        If CType(sender.owner, ToolStripDropDown).Tag >= 0 And CType(sender.owner, ToolStripDropDown).Tag <> i Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorKeyFollowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotorKeyFollowToolStripMenuItem.Click, RotorKeyToggleToolStripMenuItem.Click, RotorKeyOneShotToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        ' Display the item number as the last item seen.
        'MsgBox(i)

        'For Each item As ToolStripMenuItem In Me.ControlTypeToolStripMenuItem.DropDownItems
        '    If item.Tag <> i Then
        '        item.Checked = False
        '    Else
        '        item.Checked = True
        '    End If
        'Next

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then


                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).ControlType = i
                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub


    Private Sub RotorKeyNoiseToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorKeyNoiseToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 7 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).NoiseFilter
                                Else
                                    CType(itm, TouchPie).NoiseFilter = CInt(sender.text.ToString)

                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorKeyDCToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorKeyDCToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).DeglitchCount
                                Else
                                    CType(itm, TouchPie).DeglitchCount = CInt(sender.text.ToString)

                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorKeyMPinitToolStripTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles RotorKeyMPinitToolStripTextBox.LostFocus
        If Not IsNumeric(sender.text.ToString) Then
            Exit Sub
        End If

        Me.ToolStripButton10.Enabled = True

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then

                                If CInt(sender.text.ToString) > 15 Or CInt(sender.text.ToString) < 0 Then
                                    sender.text = CType(itm, TouchPie).MapPortInit
                                Else
                                    CType(itm, TouchPie).MapPortInit = CInt(sender.text.ToString)

                                End If

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub RotorKeyMPDisableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RotorKeyMPDisableToolStripMenuItem.Click
        Me.ToolStripButton10.Enabled = True

        Dim a As Integer
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then

            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchKey Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "slidergroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchBar Then

                    '    End If
                    'Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchPie Then
                            If CType(itm, TouchPie).ObjectID = CType(sender.owner, ToolStripDropDown).Text Then
                                CType(itm, TouchPie).MapPort = -1
                                Me.RotorKeyMPDisableToolStripMenuItem.Checked = True

                                For a = 0 To IC.Length - 1
                                    If IC(a) = pjd.IC_model Then
                                        If CType(sender.owner, ToolStripDropDown).Tag <> -1 Then
                                            P(a, CType(sender.owner, ToolStripDropDown).Tag) = PortStatus.available
                                        End If
                                    End If
                                Next

                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

#End Region

 

    Public Sub syncSliderGroupInfo(ByVal group As GroupBox, ByVal tb As TouchBar)
        Me.ToolStripButton10.Enabled = True

        For Each itm As Control In group.Controls
            If TypeOf itm Is TouchBar Then
                If CType(itm, TouchBar).ObjectID <> tb.ObjectID Then
                    CType(itm, TouchBar).Direction = tb.Direction
                    CType(itm, TouchBar).MapPWM = tb.MapPWM
                    CType(itm, TouchBar).ValueType = tb.ValueType
                    CType(itm, TouchBar).StepValue = tb.StepValue
                    CType(itm, TouchBar).StartValue = tb.StartValue
                    CType(itm, TouchBar).StopValue = tb.StopValue
                    CType(itm, TouchBar).Interpolation = tb.Interpolation
                    CType(itm, TouchBar).Sensitivity = tb.Sensitivity
                    CType(itm, TouchBar).SpeedVector = tb.SpeedVector
                    CType(itm, TouchBar).TiggerPort = tb.TiggerPort
                    CType(itm, TouchBar).NoiseFilterOuter = tb.NoiseFilterOuter
                End If
            ElseIf TypeOf itm Is PictureBox Then
                If tb.Direction = TKtoolkit.Direction.left Then
                    CType(itm, PictureBox).Image = My.Resources.Resource1._1287798671_previous
                Else
                    CType(itm, PictureBox).Image = My.Resources.Resource1._1287798727_next
                End If
            End If
        Next
    End Sub




    Public Sub syncRotorGroupInfo(ByVal group As GroupBox, ByVal tp As TouchPie)
        Me.ToolStripButton10.Enabled = True

        For Each itm As Control In group.Controls
            If TypeOf itm Is TouchPie Then
                If CType(itm, TouchPie).ObjectID <> tp.ObjectID Then
                    CType(itm, TouchPie).Direction = tp.Direction
                    CType(itm, TouchPie).MapPWM = tp.MapPWM
                    CType(itm, TouchPie).ValueType = tp.ValueType
                    CType(itm, TouchPie).StepValue = tp.StepValue
                    CType(itm, TouchPie).StartValue = tp.StartValue
                    CType(itm, TouchPie).StopValue = tp.StopValue
                    CType(itm, TouchPie).Interpolation = tp.Interpolation
                    CType(itm, TouchPie).Sensitivity = tp.Sensitivity
                    CType(itm, TouchPie).SpeedVector = tp.SpeedVector
                    CType(itm, TouchPie).TiggerPort = tp.TiggerPort
                    CType(itm, TouchPie).NoiseFilterOuter = tp.NoiseFilterOuter
                End If
            ElseIf TypeOf itm Is PictureBox Then
                If tp.Direction = TKtoolkit.Direction.counterclockwise Then
                    CType(itm, PictureBox).Image = My.Resources.Resource1._1289492364_arrow_counterclockwise
                Else
                    CType(itm, PictureBox).Image = My.Resources.Resource1._1289492322_arrow_clockwise
                End If
            End If
        Next
    End Sub



    Private Sub refreshDisplay()
        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchKey Then
                            If pp.Show_Port = "True" Then
                                CType(itm, TouchKey).myRectangle = Me.smallKeyRect
                                CType(itm, TouchKey).Location = Me.smallKeylocation
                            Else
                                CType(itm, TouchKey).myRectangle = Me.bigKeyRect
                                CType(itm, TouchKey).Location = Me.bigKeyLocation
                            End If
                            'CType(itm, TouchKey).Refresh()
                        ElseIf TypeOf itm Is Label Then
                            If pp.Show_Port = "True" Then
                                CType(itm, Label).Visible = True
                            Else
                                CType(itm, Label).Visible = False
                            End If
                        ElseIf TypeOf itm Is Line Then
                            If pp.Show_Port = "True" Then
                                CType(itm, Line).Visible = True
                            Else
                                CType(itm, Line).Visible = False
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is TouchBar Then
                            If pp.Show_Port = "True" Then
                                CType(itm, TouchBar).myRectangle = Me.smallBarRect
                                'CType(itm, TouchKey).Location = Me.smallKeylocation
                                CType(itm, TouchBar).Location = New Point(CType(itm, TouchBar).Location.X, 40)
                            Else
                                CType(itm, TouchBar).myRectangle = Me.bigBarRect
                                'CType(itm, TouchKey).Location = Me.bigKeyLocation
                                CType(itm, TouchBar).Location = New Point(CType(itm, TouchBar).Location.X, 25)
                            End If
                        ElseIf TypeOf itm Is Label Then
                            If pp.Show_Port = "True" Then
                                CType(itm, Label).Visible = True
                            Else
                                CType(itm, Label).Visible = False
                            End If
                        ElseIf TypeOf itm Is Line Then
                            If pp.Show_Port = "True" Then
                                CType(itm, Line).Visible = True
                            Else
                                CType(itm, Line).Visible = False
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    'Dim itm As Control
                    'For Each itm In ctl.Controls
                    '    If TypeOf itm Is TouchPie Then

                    '    End If
                    'Next

                End If
            End If

        Next ctl
    End Sub

    Sub triggerPortChanged(ByVal id As Integer, ByVal value As Integer)
        Dim text As String = "N"
        Dim c As Integer
        Select Case value
            Case 0, 1, 2, 3, 4, 5, 6, 7
                c = value Mod 8
                text = "P0." & c
            Case 8, 9, 10, 11, 12, 13, 14, 15
                c = value Mod 8
                text = "P1." & c
            Case 16, 17, 18, 19, 20, 21, 22, 23
                c = value Mod 8
                text = "P2." & c
            Case 24, 25, 26, 27, 28, 29, 30
                c = value Mod 8
                text = "P3." & c
        End Select

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "triggerlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()
                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "triggerlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "triggerlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Sub mapPortChanged(ByVal id As Integer, ByVal value As Integer)
        Dim text As String = "N"
        Dim c As Integer
        Select Case value
            Case 0, 1, 2, 3, 4, 5, 6, 7
                c = value Mod 8
                text = "P0." & c
            Case 8, 9, 10, 11, 12, 13, 14, 15
                c = value Mod 8
                text = "P1." & c
            Case 16, 17, 18, 19, 20, 21, 22, 23
                c = value Mod 8
                text = "P2." & c
            Case 24, 25, 26, 27, 28, 29, 30
                c = value Mod 8
                text = "P3." & c
        End Select

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "maplabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()
                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "maplabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "maplabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Sub sensorPortChanged(ByVal id As Integer, ByVal value As Integer)
        Dim text As String = "N"
        Dim c As Integer
        Select Case value
            Case 0, 1, 2, 3, 4, 5, 6, 7
                c = value Mod 8
                text = "P0." & c
            Case 8, 9, 10, 11, 12, 13, 14, 15
                c = value Mod 8
                text = "P1." & c
            Case 16, 17, 18, 19, 20, 21, 22, 23
                c = value Mod 8
                text = "P2." & c
            Case 24, 25, 26, 27, 28, 29, 30
                c = value Mod 8
                text = "P3." & c
        End Select

        For Each ctl As Control In Me.Panel1.Controls

            If TypeOf ctl Is TouchKey Then

            ElseIf TypeOf ctl Is TouchBar Then


            ElseIf TypeOf ctl Is TouchPie Then

            ElseIf TypeOf ctl Is GroupBox Then
                If ctl.Name = "keygroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "sensorlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    'CType(itm, Label).Refresh()
                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "slidergroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "sensorlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                ElseIf ctl.Name = "rotatorgroup" Then
                    Dim itm As Control
                    For Each itm In ctl.Controls
                        If TypeOf itm Is Label Then
                            If CType(itm, Label).Name = "sensorlabel" Then
                                If CType(itm, Label).Tag = id Then
                                    If value <> -1 Then
                                        CType(itm, Label).Text = text
                                    Else
                                        CType(itm, Label).Text = "N"
                                    End If
                                    CType(itm, Label).Refresh()

                                End If
                            End If
                        End If
                    Next

                End If
            End If

        Next ctl
    End Sub

    Private Sub 英文ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 英文ToolStripMenuItem.Click, 繁體中文ToolStripMenuItem.Click, 簡體中文ToolStripMenuItem.Click
        Dim i As Integer
        Dim myItem As ToolStripMenuItem

        ' Extract the tag value from the item received.
        myItem = CType(sender, ToolStripMenuItem)
        i = CInt(myItem.Tag)

        Select Case i
            Case Language.English
                CI = New CultureInfo("en-US")

                英文ToolStripMenuItem.Checked = True
                繁體中文ToolStripMenuItem.Checked = False
                簡體中文ToolStripMenuItem.Checked = False

            Case Language.TraditionalChinese
                CI = New CultureInfo("zh-TW")

                英文ToolStripMenuItem.Checked = False
                繁體中文ToolStripMenuItem.Checked = True
                簡體中文ToolStripMenuItem.Checked = False

            Case Language.SimplifiedChinese
                CI = New CultureInfo("zh-CN")

                英文ToolStripMenuItem.Checked = False
                繁體中文ToolStripMenuItem.Checked = False
                簡體中文ToolStripMenuItem.Checked = True

            Case Else
                Exit Sub
        End Select

        Threading.Thread.CurrentThread.CurrentUICulture = CI

        Me.Label1.Text = RM.GetString("StillHave") & maxPinNumber & RM.GetString("KeyAvailable")
        檔案ToolStripMenuItem.Text = RM.GetString("Files") & "(&F)"
        DebugToolStripMenuItem.Text = RM.GetString("Debug") & "(&D)"
        檢視ToolStripMenuItem.Text = RM.GetString("View") & "(&V)"
        新增ToolStripMenuItem.Text = RM.GetString("NewFile") & "(&N)"
        專案ToolStripMenuItem.Text = RM.GetString("Project") & "(&P)"
        開啟ToolStripMenuItem.Text = RM.GetString("Open") & "(&O)"
        專案ToolStripMenuItem1.Text = RM.GetString("Project") & "(&P)"
        儲存ToolStripMenuItem.Text = RM.GetString("Save") & "(&S)"
        另存ToolStripMenuItem.Text = RM.GetString("Save As") & "(&S)"
        Xu6d9ToolStripMenuItem.Text = RM.GetString("Exit") & "(&X)"
        執行ToolStripMenuItem.Text = RM.GetString("Excute")
        語言ToolStripMenuItem.Text = RM.GetString("Language") & "(&L)"
        英文ToolStripMenuItem.Text = RM.GetString("English")
        繁體中文ToolStripMenuItem.Text = RM.GetString("Traditional Chinese")
        簡體中文ToolStripMenuItem.Text = RM.GetString("Simplified Chinese")
        ToolStripButton2.ToolTipText = RM.GetString("New Project")
        ToolStripButton3.ToolTipText = RM.GetString("Open Project")
        ToolStripButton4.ToolTipText = RM.GetString("Save Project")
        ToolStripButton6.ToolTipText = RM.GetString("Setting")
        ToolStripButton1.ToolTipText = RM.GetString("Excute")
        ToolStripButton5.ToolTipText = RM.GetString("Stop")
        ToolStripButton10.ToolTipText = RM.GetString("Download")
        TabControl1.TabPages(0).Text = RM.GetString("Library")
        TabControl1.TabPages(1).Text = RM.GetString("Attributes")
        ProjctApply.Text = RM.GetString("Apply")
        ApplyButton.Text = RM.GetString("Apply")
        ToolStripSplitButton1.ToolTipText = RM.GetString("Debug Termial")



    End Sub


    Private Sub ToolStripSplitButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripSplitButton1.Click
        If hDevice = &H0 Or hDevice = &HFFFFFFFF Then
            hDevice = ztOpenDevice()
        End If
        If hDevice <> &H0 And hDevice <> &HFFFFFFFF Then
            'ztTLPReadFromIC()
            'ztTLPGenerateLastPage(hDevice)
            ztImport()
            ztTLPWriteToIC()

            ztTLPReadFromIC()
            ztTLPGenerateLastPage(hDevice)
        End If

    End Sub

    Private Sub keepReading()

        Do While (1)
            'Thread.Sleep(5000)
            'Debug.Print("thread")

            Dim INTStatus As Boolean

            U2C_SingleIoRead(hDevice, 0, INTStatus)
            'the INT pin is low,so start to use I2C command to get normal packet data
            If INTStatus = False Then
                'send a command to slave to retrieve data
                Debug.Print("low ")

            End If
        Loop
    End Sub

#Region "U2C"
    Sub ztTLPWriteByte(ByVal addr As Integer, ByVal value As Byte)
        bLastK(addr) = value
    End Sub
    Sub fSendPassword()
        'reset RESET pin
        U2C_SingleIoWrite(hDevice, 2, False)

        Dim transact As U2C_TRANSACTION
        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H20S
            .Buffer(1) = &HC5S
            .Buffer(2) = &H9DS
            .nBufferLength.bLo = 3
        End With
        u2c_result = U2C_Write(hDevice, transact)
    End Sub
    Sub fResetMCU()
        'set RESET pin
        U2C_SingleIoWrite(hDevice, 2, True)

        Dim transact As U2C_TRANSACTION
        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H29S
            .nBufferLength.bLo = 1
        End With
        u2c_result = U2C_Write(hDevice, transact)
    End Sub
    Sub fPageRead(ByVal n As Integer)
        Dim transact As U2C_TRANSACTION
        transact.Initialize()
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H25S
            .Buffer(1) = n
            .nBufferLength.bLo = 2
        End With
        u2c_result = U2C_Write(hDevice, transact)

        transact.nBufferLength.bLo = 128
        u2c_result = U2C_Read(hDevice, transact)
        Array.Copy(transact.Buffer, 0, bLastK, (n - 120) * 128, 128)
    End Sub
    Sub fPageWrite(ByVal n As Integer)
        Dim transact As U2C_TRANSACTION

        transact.Initialize()
        initByteArray(transact.Buffer, &HFFS)
        With transact
            .nSlaveDeviceAddress = &H76S
            .nMemoryAddressLength = 0
            .Buffer(0) = &H22S
            .Buffer(1) = n
            .nBufferLength.bLo = 130
        End With
        For i As Integer = 0 To 128 - 1
            transact.Buffer(i + 2) = bLastK(((n - 120) * 128) + i)
        Next
        u2c_result = U2C_Write(hDevice, transact)
    End Sub
    Sub ztTLPWriteToIC()
        fSendPassword()
        For i As Integer = 128 - 8 To 128 - 1
            fPageWrite(i)
        Next
        fResetMCU()
    End Sub

    Sub ztTLPReadFromIC()
        fSendPassword()
        For i As Integer = 128 - 8 To 128 - 1
            fPageRead(i)
        Next
        fResetMCU()
    End Sub

    Function ztTLPGenerateLastPage(ByVal hDevice As Integer) As Integer
        Dim sw As StreamWriter
        Dim str As String
        Dim SaveFileDialog1 As New SaveFileDialog

        SaveFileDialog1.Filter = "HEX File (*.hex)|*.hex|All Files (*.*)|*.*"
        SaveFileDialog1.FilterIndex = 1
        SaveFileDialog1.RestoreDirectory = True
        If SaveFileDialog1.ShowDialog() = DialogResult.OK Then
            'llHEXFile.Text = SaveFileDialog1.FileName
            sw = New StreamWriter(SaveFileDialog1.FileName)
            sw.AutoFlush = True
            For i As Integer = 0 To ((128 * 8) / 16) - 1
                str = ":10" + Hex(startingAddr + i * 16).ToString + "00"
                str += Trim(byte2StringNoSpace(bLastK, i * 16, 16))
                str += "EC"
                sw.WriteLine(str)
            Next
            sw.WriteLine(":00000001FF")
            sw.Close()
        End If

    End Function

    Function ztImport() As Integer
        Dim length, idx As Integer
        Dim sr As StreamReader
        Dim line, temp As String
        Dim curAddr As Integer
        Dim buffer(1024 - 1), b As Byte
        Dim OpenFileDialog1 As New OpenFileDialog

        OpenFileDialog1.Filter = "HEX File (*.hex)|*.hex|All Files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.RestoreDirectory = True
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            'reset buffer
            initByteArray(buffer, &HFFS)

            If UCase(getExtension(OpenFileDialog1.FileName)) = "HEX" Then
                sr = New StreamReader(OpenFileDialog1.FileName)
                If Not (sr Is Nothing) Then
                    Do
                        line = sr.ReadLine()
                        If line = Nothing Then
                            Exit Do
                        End If
                        temp = Mid(line, 2, 2)
                        length = Convert.ToInt32(temp, 16)
                        'parse current address
                        curAddr = Convert.ToInt32(Mid(line, 4, 4), 16)
                        idx = 10
                        For i As Integer = 0 To length - 1
                            b = CByte(Convert.ToInt32(Mid(line, idx + (i * 2), 2), 16))
                            ztTLPWriteByte(curAddr - startingAddr + i, b)
                        Next
                    Loop Until line Is Nothing
                    sr.Close()

                End If
            End If
        End If

    End Function

#End Region

End Class

