Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_TypeGroupe


#Region "Attribut"
    Private _id As Long
    Private _TypeGroupe As String

#End Region

#Region "New"
    Public Sub New()
        '  BlankProperties()
    End Sub

    Public Sub New(ByVal _idOne As Long)
        Read(_idOne)
    End Sub
#End Region

#Region "Properties"

    Public ReadOnly Property ID As Long
        Get
            Return _id
        End Get
    End Property

    Public Property TypeGroupe() As String
        Get
            Return _TypeGroupe
        End Get
        Set(ByVal value As String)
            _TypeGroupe = value
        End Set
    End Property




#End Region

#Region " Db Access "
    Public Function Insert() As Integer
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Insert into tbl_TypeGroupe (TypeGroupe) values ('" & TypeGroupe & "'); "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        commandString = "SELECT TOP(1) Id_Groupe from tbl_TypeGroupe order by Id_TypeGroupe desc"
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function

    Public Function Update(ByVal usr As String) As Integer
        '        _LogData = ""
        '        _LogData = GetObjectString()
        '        Return SqlHelper.ExecuteScalar(SqlHelperParameterCache.BuildConfigDB(), "SP_UpdateMedicament", _id, _CodeInternational, _Description, _Prix, _Id_Monnaie, usr)
        '    End Function

        '    Private Sub SetProperties(ByVal dr As DataRow)

        '        _id = TypeSafeConversion.NullSafeLong(dr("Id_Medicament"))
        '        _CodeInternational = TypeSafeConversion.NullSafeString(dr("CodeInternational"))
        '        _Description = TypeSafeConversion.NullSafeString(dr("Description"))
        '        _Prix = TypeSafeConversion.NullSafeDecimal(dr("Prix"))
        '        _Id_Monnaie = TypeSafeConversion.NullSafeLong(dr("Id_Monnaie"))
        '        _Monnaie = Nothing
        '    End Sub

        '    Private Sub BlankProperties()

        '        _id = 0
        '        _CodeInternational = ""
        '        _Description = ""
        '        _Prix = 0
        '        _Id_Monnaie = 0
        '        _Monnaie = Nothing
        '        _isdirty = False

    End Function

    Public Function Read(ByVal _idpass As Long) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_TypeGroupe where Id_TypeGroupe = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _TypeGroupe = myReader.Item(1)
        End While

        myReader.Close()
        ObjectConnection.Close()
        Return _id

    End Function

#End Region

#Region " Search "
    Public Shared Function GetLastGroupe() As Cls_TypeGroupe
        Dim obj As New Cls_TypeGroupe
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString = "SELECT TOP(1) Id_TypeGroupe , TypeGroupe  from tbl_TypeGroupe order by Id_TypeGroupe desc"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            With obj
                ._id = myReader.Item(0)
                ._TypeGroupe = myReader.Item(1)
            End With
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return obj
    End Function

    Public Shared Function SearchAll() As List(Of Cls_TypeGroupe)

        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_TypeGroupe)
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_TypeGroupe  "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_TypeGroupe
            With obj
                ._id = row.Item(0)
                ._TypeGroupe = row.Item(1)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs

    End Function
#End Region

#Region " Other Methods "

#End Region
End Class
