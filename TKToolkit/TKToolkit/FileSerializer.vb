Imports System.IO
Imports System.Collections
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Formatters.Soap
Imports System.Runtime.Serialization


Module FileSerializer

    Sub Serialize(ByVal strPath As String, ByVal myFile As SortedList)

        ' Create a filestream to allow saving the file after it has 
        ' been serialized in this method
        Dim fs As New FileStream(strPath, FileMode.OpenOrCreate)

        ' Create a new instance of the binary formatter
        Dim formatter As New BinaryFormatter
        'Dim formatter As New SoapFormatter

        Try

            ' save the serialized data to the file path specified
            formatter.Serialize(fs, myFile)
            fs.Close()

        Catch ex As SerializationException

            MessageBox.Show(ex.Message & ": " & ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try

    End Sub



    Public Function Deserialize(ByVal strPath As String) As SortedList

        ' Create filestream allowing the user to open an existing file
        Dim fs As New FileStream(strPath, FileMode.Open)

        ' Create a new instance of the Personal Data class
        Dim myFile As SortedList
        myfile = New SortedList

        Try

            ' Create a binary formatter
            Dim formatter As New BinaryFormatter
            'Dim formatter As New SoapFormatter

            ' Deserialize the data stored in the specified file and 
            ' use that data to populate the new instance of the personal data class.
            myfile = formatter.Deserialize(fs)
            fs.Close()

            ' Return the deserialized data back to the calling application
            Return myFile

        Catch ex As SerializationException

            MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return myFile

        End Try

    End Function

End Module
