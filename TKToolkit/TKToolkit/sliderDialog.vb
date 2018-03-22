Imports System.Windows.Forms

Public Class sliderDialog

    Private total As Integer = 12

    Public Sub New(ByVal val As Integer)
        If val < total Then
            total = val
        End If

        Me.InitializeComponent()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub sliderDialog_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ComboBox1.Items.Clear()
        Dim i As Integer
        For i = 0 To total - 1
            'If i < 2 Then
            'Me.ComboBox1.Text = i + 1
            'End If
            Me.ComboBox1.Items.Add(i + 1)
        Next

        If total >= 2 Then
            Me.ComboBox1.SelectedItem = 2
        Else
            Me.ComboBox1.SelectedItem = 1
        End If

    End Sub
End Class
