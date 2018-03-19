Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamicTable

#Region "Attribut"
    Private _id As Long
    Private _Id_ReportDynamic As Long
    Private _ReportDynamic As Cls_ReportDynamic
    Private _Id_Table As Long
    Private _Table As Cls_Table
    Private _Alias As String

    Private _isdirty As Boolean
    Private _LogData As String
#End Region

#Region "New"
    Public Sub New()
        '  BlankProperties()
    End Sub

    Public Sub New(ByVal _idOne As Long)
        Read(_idOne)
    End Sub

    Public Sub New(ByVal idparent As Long, ByVal idliaison As Long, ByVal idenfant As Long)
        ' Read(idparent, idliaison, idenfant)
    End Sub

#End Region

#Region "Properties"
    Public Property Id() As Long
        Get
            Return _id
        End Get
        Set(ByVal value As Long)
            _id = value
        End Set
    End Property

    Public Property Id_ReportDynamic() As Long
        Get
            Return _Id_ReportDynamic
        End Get
        Set(ByVal value As Long)
            _Id_ReportDynamic = value
        End Set
    End Property


    Public Property ReportDynamic As Cls_ReportDynamic

        Get
            If Not (_ReportDynamic Is Nothing) Then
                If (_ReportDynamic.Id = 0) Or (_ReportDynamic.Id <> _Id_ReportDynamic) Then
                    _ReportDynamic = New Cls_ReportDynamic(_Id_ReportDynamic)
                End If
            Else
                _ReportDynamic = New Cls_ReportDynamic(_Id_ReportDynamic)
            End If

            Return _ReportDynamic
        End Get
        Set(ByVal value As Cls_ReportDynamic)
            If value Is Nothing Then

                _Id_ReportDynamic = 0
            Else
                If _ReportDynamic.Id <> value.ID Then
                    _Id_ReportDynamic = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property ReportDynamic_Name As String
        Get
            Return ReportDynamic.Name
        End Get
    End Property

    Public Property Id_Table() As Long
        Get
            Return _Id_Table
        End Get
        Set(ByVal value As Long)
            _Id_Table = value
        End Set
    End Property

    Public Property Table As Cls_Table

        Get
            If Not (_Table Is Nothing) Then
                If (_Table.ID = 0) Or (_Table.ID <> _Id_Table) Then
                    _Table = New Cls_Table(_Id_Table)
                End If
            Else
                _Table = New Cls_Table(_Id_Table)
            End If

            Return _Table
        End Get
        Set(ByVal value As Cls_Table)
            If value Is Nothing Then

                _Id_Table = 0
            Else
                If _Table.ID <> value.ID Then
                    _Id_Table = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property Table_Name As String
        Get
            Return Table.Name
        End Get
    End Property

    Public Property AliasString As String
        Get
            Return _Alias
        End Get
        Set(ByVal value As String)
            _Alias = value
        End Set
    End Property

    Public ReadOnly Property CompleteName As String
        Get
            Return Table_Name + "  " + AliasString
        End Get
    End Property


    Public Property Isdirty() As Boolean
        Get
            Return _isdirty
        End Get
        Set(ByVal value As Boolean)
            _isdirty = value
        End Set
    End Property

    Public Property LogData() As String
        Get
            Return _LogData
        End Get
        Set(ByVal value As String)
            _LogData = value
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
            Dim commandString As String = "Insert into tbl_ReportDynamicTable (Id_ReportDynamic, Id_Table , Alias) values (" & _Id_ReportDynamic & ", " & _Id_Table & ", '" & _Alias & "'); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamicTable from tbl_ReportDynamicTable order by Id_ReportDynamicTable desc"
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

    Public Function Update() As Integer
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Update tbl_ReportDynamicTable set  Id_ReportDynamic = " & _Id_ReportDynamic & " , Id_Table = " & _Id_Table & " , Alias = '" & _Alias & "'  where Id_ReportDynamic =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function

    Public Function Read(ByVal _idpass As Long) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_ReportDynamicTable where Id_ReportDynamicTable = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Id_ReportDynamic = myReader.Item(1)
            _Id_Table = myReader.Item(2)
            _Alias = myReader.Item(3)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function

#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamicTable)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicTable)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicTable "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicTable
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamic = row.Item(1)
                .Id_Table = row.Item(2)
                .AliasString = row.Item(3)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllbyId_ReportDynamic(ByVal idreportdynamic As Long) As List(Of Cls_ReportDynamicTable)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicTable)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicTable where Id_ReportDynamic = " & idreportdynamic & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicTable
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamic = row.Item(1)
                .Id_Table = row.Item(2)
                .AliasString = row.Item(3)
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
