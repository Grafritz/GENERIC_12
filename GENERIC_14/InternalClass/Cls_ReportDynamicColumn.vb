Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamicColumn

#Region "Attribut"
    Private _id As Long
    Private _Id_ReportDynamicTable As Long
    Private _ReportDynamicTable As Cls_ReportDynamicTable
    Private _Id_Column As Long
    Private _Column As Cls_Column


    Private _isdirty As Boolean
    Private _LogData As String
#End Region

#Region "New"
    Public Sub New()
        '  BlankProperties()
    End Sub

    Public Sub New(ByVal _idOne As Long)
        ' Read(_idOne)
    End Sub

    Public Sub New(ByVal idreportdynamicTable As Long, ByVal idcolumn As Long)
        Read(idreportdynamicTable, idcolumn)
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

    Public Property Id_ReportDynamicTable() As Long
        Get
            Return _Id_ReportDynamicTable
        End Get
        Set(ByVal value As Long)
            _Id_ReportDynamicTable = value
        End Set
    End Property


    Public Property ReportDynamicTable As Cls_ReportDynamicTable

        Get
            If Not (_ReportDynamicTable Is Nothing) Then
                If (_ReportDynamicTable.Id = 0) Or (_ReportDynamicTable.Id <> _Id_ReportDynamicTable) Then
                    _ReportDynamicTable = New Cls_ReportDynamicTable(_Id_ReportDynamicTable)
                End If
            Else
                _ReportDynamicTable = New Cls_ReportDynamicTable(_Id_ReportDynamicTable)
            End If

            Return _ReportDynamicTable
        End Get
        Set(ByVal value As Cls_ReportDynamicTable)
            If value Is Nothing Then

                _Id_ReportDynamicTable = 0
            Else
                If _ReportDynamicTable.Id <> value.ID Then
                    _Id_ReportDynamicTable = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property ReportDynamicTable_Name As String
        Get
            Return ReportDynamicTable.Table_Name
        End Get
    End Property

    Public Property Id_Column As Long
        Get
            Return _Id_Column
        End Get
        Set(ByVal value As Long)
            _Id_Column = value
        End Set
    End Property

    Public Property Column As Cls_Column

        Get
            If Not (_Column Is Nothing) Then
                If (_Column.ID = 0) Or (_Column.ID <> _Id_Column) Then
                    _Column = New Cls_Column(_Id_Column)
                End If
            Else
                _Column = New Cls_Column(_Id_Column)
            End If

            Return _Column
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_Column = 0
            Else
                If _Column.ID <> value.ID Then
                    _Id_Column = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property Column_Name As String
        Get
            Return Column.Name
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
            Dim commandString As String = "Insert into tbl_ReportDynamicColumn (Id_ReportDynamicTable, Id_Column) values (" & _Id_ReportDynamicTable & ", " & _Id_Column & ") "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamicColumn from tbl_ReportDynamicColumn order by Id_ReportDynamicColumn desc"
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
        Dim commandString As String = "Update tbl_ReportDynamicColumn set  Id_ReportDynamicTable = " & _Id_ReportDynamicTable & " , Id_Column = " & _Id_Column & "  where Id_ReportDynamicTable =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function

    Public Function Read(ByVal idreportdynamicTable As Long, ByVal idcolumn As String) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_ReportDynamicColumn where Id_ReportDynamicTable = " & idreportdynamicTable & " and Id_Column = " & idcolumn & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Id_ReportDynamicTable = myReader.Item(1)
            _Id_Column = myReader.Item(2)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function

#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamicColumn)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicColumn)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicColumn "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicColumn
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamicTable = row.Item(1)
                .Id_Column = row.Item(2)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllbyId_ReportDynamicTable(ByVal idReportDynamicTable As Long) As List(Of Cls_ReportDynamicColumn)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicColumn)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicColumn where Id_ReportDynamicTable = " & idReportDynamicTable & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicColumn
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamicTable = row.Item(1)
                .Id_Column = row.Item(2)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllbyId_Report(ByVal idReport As Long) As List(Of Cls_ReportDynamicColumn)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicColumn)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select rc.* from tbl_ReportDynamicColumn rc inner join tbl_ReportDynamicTable rt on rc.Id_ReportDynamicTable = rt.Id_ReportDynamicTable  where rt.Id_ReportDynamic = " & idReport & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicColumn
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamicTable = row.Item(1)
                .Id_Column = row.Item(2)
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
