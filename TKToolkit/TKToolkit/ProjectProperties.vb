Imports System
Imports System.ComponentModel


Public Class ProjectProperties

    Private m_IC_model As String
    Private m_Scan_type As String
    Private m_Work_Freq As String
    Private m_LVD_level As String
    Private m_LVR_level As String
    Private m_Display_level As String

    Private m_Salary As Integer

    <Description("show port information"), _
Category("Project Information"), _
Browsable(True), _
[ReadOnly](False), _
BindableAttribute(False), _
DefaultValueAttribute("False"), _
TypeConverter(GetType(ShowPortChoice)), _
DesignOnly(False)> _
Public Property Show_Port() As String
        Get
            Return m_Display_level
        End Get
        Set(ByVal value As String)
            m_Display_level = value
        End Set
    End Property

    <Description("select LVR level 2.0V/2.3V/2.6V/2.9Vl"), _
Category("Project Information"), _
Browsable(True), _
[ReadOnly](False), _
BindableAttribute(False), _
DefaultValueAttribute("2.0V"), _
TypeConverter(GetType(LVRChoice)), _
DesignOnly(False)> _
Public Property LVR_level() As String
        Get
            Return m_LVR_level
        End Get
        Set(ByVal value As String)
            m_LVR_level = value
        End Set
    End Property

    <Description("select LVD level 2.1V/2.4V/2.7V/3.0V"), _
Category("Project Information"), _
Browsable(True), _
[ReadOnly](False), _
BindableAttribute(False), _
DefaultValueAttribute("2.1V"), _
TypeConverter(GetType(LVDChoice)), _
DesignOnly(False)> _
Public Property LVD_level() As String
        Get
            Return m_LVD_level
        End Get
        Set(ByVal value As String)
            m_LVD_level = value
        End Set
    End Property

    <Description("select working frequence"), _
Category("Project Information"), _
Browsable(True), _
[ReadOnly](False), _
BindableAttribute(False), _
DefaultValueAttribute("12MHz"), _
TypeConverter(GetType(frequenceChoice)), _
DesignOnly(False)> _
Public Property Work_Freq() As String
        Get
            Return m_Work_Freq
        End Get
        Set(ByVal value As String)
            m_Work_Freq = value
        End Set
    End Property

    <Description("select scan type: Mutual or Self"), _
Category("Project Information"), _
Browsable(True), _
[ReadOnly](False), _
BindableAttribute(False), _
DefaultValueAttribute("Self"), _
TypeConverter(GetType(scanChoice)), _
DesignOnly(False)> _
Public Property Scan_type() As String
        Get
            Return m_Scan_type
        End Get
        Set(ByVal value As String)
            m_Scan_type = value
        End Set
    End Property

    <Description("select IC model for different pacgage"), _
Category("Project Information"), _
    Browsable(True), _
   [ReadOnly](False), _
   BindableAttribute(False), _
   DefaultValueAttribute("ZET8234WMA"), _
   TypeConverter(GetType(icChoice)), _
   DesignOnly(False)> _
Public Property IC_model() As String
        Get
            Return m_IC_model
        End Get
        Set(ByVal value As String)
            m_IC_model = value
        End Set
    End Property

End Class

Public Class icChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"ZET8234WMA", "ZET8234WLA", "ZET8234VGA"})
            End If
            Return theValues
        End Get
    End Property
End Class

Public Class scanChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"Mutual", "Self"})
            End If
            Return theValues
        End Get
    End Property
End Class

Public Class frequenceChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"12MHz", "8MHz", "4MHz"})
            End If
            Return theValues
        End Get
    End Property
End Class

Public Class LVDChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"2.1V", "2.4V", "2.7V", "3.0V"})
            End If
            Return theValues
        End Get
    End Property
End Class

Public Class LVRChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"2.0V", "2.3V", "2.6V", "2.9V"})
            End If
            Return theValues
        End Get
    End Property
End Class

Public Class ShowPortChoice
    Inherits StringConverter

    Dim theValues As TypeConverter.StandardValuesCollection

    Public Overrides Function GetStandardValuesSupported( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive( _
        ByVal context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues( _
        ByVal context As ITypeDescriptorContext) _
        As TypeConverter.StandardValuesCollection
        Return Values
    End Function

    Private ReadOnly Property Values() As TypeConverter.StandardValuesCollection
        Get
            If theValues Is Nothing Then
                theValues = New TypeConverter.StandardValuesCollection( _
                    New String() {"True", "False"})
            End If
            Return theValues
        End Get
    End Property
End Class
