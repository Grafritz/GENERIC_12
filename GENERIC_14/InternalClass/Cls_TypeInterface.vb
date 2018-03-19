Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml


Public Class Cls_TypeInterface


#Region "Attribut"
    Private _id As Long
    Private _Name As String
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
    Public Function Insert(ByVal usr As String) As Integer

    End Function

    Public Function Update(ByVal usr As String) As Integer
        '        _LogData = ""
        '        _LogData = GetObjectString()
        '        Return SqlHelper.ExecuteScalar(SqlHelperParameterCache.BuildConfigDB(), "SP_UpdateMedicament", _id, _CodeInternational, _Description, _Prix, _Id_Monnaie, usr)
        '    End Function



        '    Private Sub BlankProperties()

        '        _id = 0D:\Dropbox\Dropbox\DeveloppementProject\GenericEvoluted_Version2\GENERIC_12\Databases\SqlServerHelper.vb
        '        _CodeInternational = ""
        '        _Description = ""
        '        _Prix = 0
        '        _Id_Monnaie = 0
        '        _Monnaie = Nothing
        '        _isdirty = False

        '    End Sub

        '    Public Function Read(ByVal _idpass As Long) As Boolean Implements IGeneral.Read
        '        Try

        '            If _idpass <> 0 Then
        '                Dim ds As DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), "SP_SelectMedicament_ByID", _idpass)

        '                If ds.Tables(0).Rows.Count < 1 Then
        '                    BlankProperties()
        '                    Return False
        '                End If

        '                SetProperties(ds.tables(0).rows(0))
        '            Else
        '                BlankProperties()
        '            End If
        '            Return True

        '        Catch ex As Exception

        '            Throw ex

        '        End Try

    End Function

    Private Sub SetProperties(ByVal dr As DataRow)

        '_id = TypeSafeConversion.NullSafeLong(dr("Id_Medicament"))
        '_CodeInternational = TypeSafeConversion.NullSafeString(dr("CodeInternational"))
        '_Description = TypeSafeConversion.NullSafeString(dr("Description"))
        '_Prix = TypeSafeConversion.NullSafeDecimal(dr("Prix"))
        '_Id_Monnaie = TypeSafeConversion.NullSafeLong(dr("Id_Monnaie"))
        '_Monnaie = Nothing
    End Sub

    Public Function ReadSqlServerName(ByVal sqlservernername As String) As Long
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_Type where SqlServerName = '" & sqlservernername & "'"
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

    Public Function ReadPostGresName(ByVal postgresname As String) As Long
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_Type where PostGresName = '" & postgresname & "'"
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

    Public Function Read(ByVal idone As String) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_Type where Id_Type = " & idone & ""
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


    'Public Sub Delete() Implements IGeneral.Delete
    '    Try
    '        SqlHelper.ExecuteNonQuery(SqlHelperParameterCache.BuildConfigDB(), "SP_DeleteMedicament", _id)

    '    Catch ex As SqlClient.SqlException
    '        Throw New System.Exception(ex.ErrorCode)
    '    End Try
    'End Sub

    'Public Function Refresh() As Boolean Implements IGeneral.Refresh
    '    If _id = 0 Then
    '        Return False
    '    Else
    '        Read(_id)
    '        Return True
    '    End If
    'End Function

    'Public Function Save(ByVal usr As String) As Integer Implements IGeneral.Save
    '    If _isdirty Then
    '        Validation()

    '        If _id = 0 Then
    '            Return Insert(usr)
    '        Else
    '            If _id > 0 Then
    '                Return Update(usr)
    '            Else
    '                _id = 0
    '                Return False
    '            End If
    '        End If
    '    End If

    '    _isdirty = False
    'End Function
#End Region

#Region " Search "

    Public Shared Function GetId_TypeBySqlServerName(ByVal name As String) As Long

    End Function

    Public Shared Function GetVbNameById(ByVal idtype As Long) As String

    End Function
#End Region

#Region " Other Methods "

#End Region
End Class
