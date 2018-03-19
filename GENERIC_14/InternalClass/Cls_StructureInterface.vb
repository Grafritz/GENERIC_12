Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_StructureInterface

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
    Public Function Insert() As Long

        Dim cantpass As Boolean = True

        While cantpass
            Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
            Dim ObjectConnection As SqlCeConnection
            Dim ObjectCommand As SqlCeCommand
            Dim commandString As String = "" '"Insert into tbl_Column (Name, Id_Table, Id_Type, Length, Precision, Scale, Id_IndexParent ) values ('" & _Name & "' ,'" & _Id_Table & "','" & _Id_Type & "','" & _Length & "','" & _Precision & "', '" & _Scale & "', '" & _Id_IndexParent & "' ); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_Column from tbl_Column order by Id_Column desc"
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
                While myReader.Read()
                    _id = CLng(myReader.Item(0))
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
        Dim commandString As String = "Update tbl_Column set  Name = '"
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
        Dim commandString As String = "Select * from tbl_Column where Id_Column = " & _idpass & ""
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


    Public Function Read(ByVal idtable As Long, ByVal columnname As String) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_Column where Id_Table = " & idtable & " and Name = '" & columnname & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)

            _Name = myReader.Item(2)
           
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function

    '    Public Function Read_CodeInternational(ByVal CodeInternational As String) As Boolean
    '        Try

    '            If CodeInternational <> "" Then
    '                Dim ds As Dataset = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), "Sp_SelectMedicament_ByID", CodeInternational)

    '                If ds.tables(0).Rows.Count < 1 Then
    '                    BlankProperties()
    '                    Return False
    '                End If

    '                SetProperties(ds.Tables(0).Rows(0))
    '            Else
    '                BlankProperties()
    '            End If

    '            Return True
    '        Catch ex As Exception

    '            Throw ex

    '        End Try

    '    End Function


    '    Public Function Read_Description(ByVal Description As String) As Boolean
    '        Try

    '            If Description <> "" Then
    '                Dim ds As Dataset = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), "Sp_SelectMedicament_ByID", Description)

    '                If ds.tables(0).Rows.Count < 1 Then
    '                    BlankProperties()
    '                    Return False
    '                End If

    '                SetProperties(ds.Tables(0).Rows(0))
    '            Else
    '                BlankProperties()
    '            End If

    '            Return True
    '        Catch ex As Exception

    '            Throw ex

    '        End Try

    '    End Function


    '    Public Sub Delete() Implements IGeneral.Delete
    '        Try
    '            SqlHelper.ExecuteNonQuery(SqlHelperParameterCache.BuildConfigDB(), "SP_DeleteMedicament", _id)

    '        Catch ex As SqlClient.SqlException
    '            Throw New System.Exception(ex.ErrorCode)
    '        End Try
    '    End Sub

    '    Public Function Refresh() As Boolean Implements IGeneral.Refresh
    '        If _id = 0 Then
    '            Return False
    '        Else
    '            Read(_id)
    '            Return True
    '        End If
    '    End Function

    '    Public Function Save(ByVal usr As String) As Integer Implements IGeneral.Save
    '        If _isdirty Then
    '            Validation()

    '            If _id = 0 Then
    '                Return Insert(usr)
    '            Else
    '                If _id > 0 Then
    '                    Return Update(usr)
    '                Else
    '                    _id = 0
    '                    Return False
    '                End If
    '            End If
    '        End If

    '        _isdirty = False
    '    End Function
#End Region

#Region " Search "
  
#End Region

#Region " Other Methods "

#End Region


End Class
