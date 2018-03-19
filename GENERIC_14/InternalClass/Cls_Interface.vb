Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_Interface


#Region "Attribut"

    Private _id As Long
    Private _Name As String
    Private _FileName As String
    Private _MasterPage As String
    Private _NbreCadrans As Long
    Private _Id_TypeInterface As Long ' send one send all, master detail
    Private _TypeInterface As Cls_TypeInterface
    Private _Id_StructureInterface As Long
    Private _StructureInterface As Cls_StructureInterface

#End Region

#Region "New"
    Public Sub New()

    End Sub
    Public Sub New(ByVal idtable As Long, ByVal columnname As String)
        Read(idtable, columnname)
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

    Public Property Id_TypeInterface() As Long
        Get
            Return _Id_TypeInterface
        End Get
        Set(ByVal value As Long)
            _Id_TypeInterface = value
        End Set
    End Property

    Public Property TypeInterface As Cls_TypeInterface

        Get
            If Not (_TypeInterface Is Nothing) Then
                If (_TypeInterface.ID = 0) Or (_TypeInterface.ID <> _Id_TypeInterface) Then
                    _TypeInterface = New Cls_TypeInterface(_Id_TypeInterface)
                End If
            Else
                _TypeInterface = New Cls_TypeInterface(_Id_TypeInterface)
            End If

            Return _TypeInterface
        End Get
        Set(ByVal value As Cls_TypeInterface)
            If Value Is Nothing Then

                _Id_TypeInterface = 0
            Else
                If _TypeInterface.ID <> Value.ID Then
                    _Id_TypeInterface = Value.ID
                End If
            End If
        End Set
    End Property


  

    Public Property Id_StructureInterface() As Long
        Get
            Return _Id_StructureInterface
        End Get
        Set(ByVal value As Long)
            _Id_StructureInterface = value
        End Set
    End Property

    Public Property StructureInterface As Cls_StructureInterface

        Get
            If Not (_StructureInterface Is Nothing) Then
                If (_StructureInterface.ID = 0) Or (_StructureInterface.ID <> _Id_StructureInterface) Then
                    _StructureInterface = New Cls_StructureInterface(_Id_StructureInterface)
                End If
            Else
                _StructureInterface = New Cls_StructureInterface(_Id_StructureInterface)
            End If

            Return _StructureInterface
        End Get
        Set(ByVal value As Cls_StructureInterface)
            If value Is Nothing Then

                _Id_StructureInterface = 0
            Else
                If _StructureInterface.ID <> value.ID Then
                    _Id_StructureInterface = value.ID
                End If
            End If
        End Set
    End Property

    Public Property NbreCadrans() As Long
        Get
            Return _NbreCadrans
        End Get
        Set(ByVal value As Long)
            _NbreCadrans = value
        End Set
    End Property

    Public Property FileName() As String
        Get
            Return _FileName
        End Get
        Set(ByVal value As String)
            _FileName = value
        End Set
    End Property

    Public Property MasterPage() As String
        Get
            Return _MasterPage
        End Get
        Set(ByVal value As String)
            _MasterPage = value
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
            Dim commandString As String = "Insert into tbl_Interface (Name, FileName, MasterPage, NbreCadrans, Id_TypeInterface, Id_StructureInterface) values ('" & _Name & "' ,'" & _FileName & "','" & _MasterPage & "', " & _NbreCadrans & ", " & _Id_TypeInterface & ", " & _Id_StructureInterface & " ); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_Interface from tbl_Interface order by Id_Interface desc"
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
        Dim commandString As String = "" ' "Update tbl_Column set  Name = '" & _Name & "' , Id_Table = " & _Id_Table & " , Id_Type = '" & _Id_Type & "', Length = '" & _Length & "'  , Precision  = '" & _Precision & "'  , Scale  = '" & _Scale & "' , Id_IndexParent = '" & _Id_IndexParent & "'  where Id_Column =  '" & _id & "'"
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
            _FileName = myReader.Item(2)
            _MasterPage = myReader.Item(3)
            _NbreCadrans = myReader.Item(4)
            _Id_TypeInterface = myReader.Item(5)
            _Id_StructureInterface = myReader.Item(6)
            
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
            _Name = myReader.Item(1)
            _FileName = myReader.Item(2)
            _MasterPage = myReader.Item(3)
            _NbreCadrans = myReader.Item(4)
            _Id_TypeInterface = myReader.Item(5)
            _Id_StructureInterface = myReader.Item(6)
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
    Public Shared Function SearchAllBy_TypeInterface(ByVal idtypeinterface As Long) As List(Of Cls_Interface)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_Interface)
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_Interface where Id_TypeInterface = " & idtypeinterface & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_Interface
            With obj
                ._id = row.Item(0)
                ._Name = row.Item(1)
                ._FileName = row.Item(2)
                ._MasterPage = row.Item(3)
                ._NbreCadrans = row.Item(4)
                ._Id_TypeInterface = row.Item(5)
                ._Id_StructureInterface = row.Item(6)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllBy_StructureInterface(ByVal idStructure As Long) As List(Of Cls_Interface)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_Interface)
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_Column where Id_IndexParent = " & idStructure & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_Interface
            With obj
                ._id = row.Item(0)
                ._Name = row.Item(1)
                ._FileName = row.Item(2)
                ._MasterPage = row.Item(3)
                ._NbreCadrans = row.Item(4)
                ._Id_TypeInterface = row.Item(5)
                ._Id_StructureInterface = row.Item(6)
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
