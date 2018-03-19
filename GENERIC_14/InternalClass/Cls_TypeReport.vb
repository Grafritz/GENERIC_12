Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_TypeReport

#Region "Attribut"

    Private _id As Long
    Private _Name As String

#End Region


#Region "New"
    Public Sub New()

    End Sub

    Public Sub New(ByVal _idOne As Long)
        Read(_idOne)
    End Sub
#End Region


#Region "Properties"

    Public ReadOnly Property ID() As Long
        Get
            Return _id
        End Get
    End Property

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property

#End Region


#Region " Db Access "
    Public Function Insert() As Integer

        Dim cantpass As Boolean = True

        While cantpass
            Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
            Dim ObjectConnection As SqlCeConnection
            Dim ObjectCommand As SqlCeCommand
            Dim commandString As String = "Insert into tbl_TypeReport (Name) values ('" & _Name & "'); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_TypeReport from tbl_TypeReport order by Id_TypeReport desc"
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
                While myReader.Read()
                    _id = myReader.Item(0)
                End While
                myReader.Close()
                ObjectConnection.Close()
                cantpass = False
            Catch ex As Exception

            End Try


        End While


        Return _id
    End Function

    Public Shared Function GetId_ColumnByTableAndColumnName(ByVal idtable As Long, ByVal columnname As String) As Long
        Dim cantpass As Boolean = True
        Dim returnId As Long
        While cantpass

            Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
            Dim ObjectConnection As SqlCeConnection
            Dim ObjectCommand As SqlCeCommand
            Dim commandString As String = "Select Id_Column from tbl_Column where Id_Table = " & idtable & " and Name = '" & columnname & "'"
            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
                While myReader.Read()
                    returnId = myReader.Item(0)

                End While
                myReader.Close()
                ObjectConnection.Close()
                cantpass = False
            Catch ex As Exception

            Finally

            End Try

        End While

        Return returnId
    End Function

    Public Function Update() As Integer
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Update tbl_TypeReport set  Name = '" & _Name & "'  where Id_TypeReport = " & _id & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function

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

    '    End Sub

    Public Function Read(ByVal _idpass As Long) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_TypeReport where Id_TypeReport = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Name = myReader.Item(1)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function

#End Region

#Region " Search "

    Public Shared Function SearchAll() As List(Of Cls_TypeReport)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_TypeReport)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from  tbl_TypeReport "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_TypeReport
            With obj
                ._id = CLng(row.Item(0))
                .Name = row.Item(1).ToString
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
