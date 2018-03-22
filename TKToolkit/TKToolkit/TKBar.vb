Public Class TKBar

    Dim m_index As Integer = 0 '
    Public Property Index() As Integer
        Get
            Return m_index
        End Get
        Set(ByVal Value As Integer)
            m_index = Value
        End Set
    End Property

    Dim m_object As Integer = -1 '
    Public Property ObjectId() As Integer
        Get
            Return m_object
        End Get
        Set(ByVal Value As Integer)
            m_object = Value
            Me.Label1.Text = m_object
        End Set
    End Property


End Class
