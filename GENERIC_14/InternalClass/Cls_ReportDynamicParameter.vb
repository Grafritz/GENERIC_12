Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamicParameter

#Region "Attribut"
    Private _id As Long
    Private _ParameterName As String
    Private _Id_ReportDynamic As Long
    Private _ReportDynamic As Cls_ReportDynamic
    Private _Id_RelatedTable As Long
    Private _RelatedTable As Cls_ReportDynamicTable
    Private _Id_RelatedColumn As Long
    Private _RelatedColumn As Cls_Column
    Private _IsInMaster As Boolean
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

    Public Property ParameterName() As String
        Get
            Return _ParameterName
        End Get
        Set(ByVal value As String)
            _ParameterName = value
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

    Public Property Id_RelatedTable() As Long
        Get
            Return _Id_RelatedTable
        End Get
        Set(ByVal value As Long)
            _Id_RelatedTable = value
        End Set
    End Property

    Public Property RelatedTable As Cls_ReportDynamicTable

        Get
            If Not (_RelatedTable Is Nothing) Then
                If (_RelatedTable.ID = 0) Or (_RelatedTable.ID <> _Id_RelatedTable) Then
                    _RelatedTable = New Cls_ReportDynamicTable(_Id_RelatedTable)
                End If
            Else
                _RelatedTable = New Cls_ReportDynamicTable(_Id_RelatedTable)
            End If

            Return _RelatedTable
        End Get
        Set(ByVal value As Cls_ReportDynamicTable)
            If value Is Nothing Then

                _Id_RelatedTable = 0
            Else
                If _RelatedTable.ID <> value.ID Then
                    _Id_RelatedTable = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedTable_CompleteName As String
        Get
            Return RelatedTable.CompleteName
        End Get
    End Property

    Public Property Id_RelatedColumn() As Long
        Get
            Return _Id_RelatedColumn
        End Get
        Set(ByVal value As Long)
            _Id_RelatedColumn = value
        End Set
    End Property

    Public Property RelatedColumn As Cls_Column

        Get
            If Not (_RelatedColumn Is Nothing) Then
                If (_RelatedColumn.ID = 0) Or (_RelatedColumn.ID <> _Id_RelatedColumn) Then
                    _RelatedColumn = New Cls_Column(_Id_RelatedColumn)
                End If
            Else
                _RelatedColumn = New Cls_Column(_Id_RelatedColumn)
            End If

            Return _RelatedColumn
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_RelatedColumn = 0
            Else
                If _RelatedColumn.ID <> value.ID Then
                    _Id_RelatedColumn = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedColumn_Name As String
        Get
            Return RelatedColumn.Name
        End Get
    End Property

    Public Property IsInMaster() As Boolean
        Get
            Return _IsInMaster
        End Get
        Set(ByVal value As Boolean)
            _IsInMaster = value
        End Set
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
            Dim commandString As String = "Insert into tbl_ReportDynamicParameter (ParameterName, Id_ReportDynamic, Id_RelatedTable, Id_RelatedColumn, IsInMaster ) values ('" & _ParameterName & "' ," & _Id_ReportDynamic & "," & _Id_RelatedTable & "," & _Id_RelatedColumn & "," & IIf(_IsInMaster, 1, 0) & "); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamicParameter from tbl_ReportDynamicParameter order by Id_ReportDynamicParameter desc"
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
        Dim commandString As String = "Update tbl_ReportDynamicParameter set  ParameterName = '" & _ParameterName & "' , Id_ReportDynamic = " & _Id_ReportDynamic & " , Id_RelatedTable  = " & Id_RelatedTable & ", Id_RelatedColumn = " & Id_RelatedColumn & ", IsInMaster = " & IsInMaster & "  where Id_ReportDynamicParameter =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function


#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamicParameter)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicParameter)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicParameter"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicParameter
            With obj
                .Id = row.Item(0)
                .ParameterName = row.Item(1).ToString
                .Id_ReportDynamic = CLng(row.Item(2))
                .Id_RelatedTable = CLng(row.Item(3))
                .Id_RelatedColumn = CLng(row.Item(4))
                .IsInMaster = row.Item(5)

            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllById_Report(ByVal idreportdynamic As Long) As List(Of Cls_ReportDynamicParameter)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicParameter)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicParameter where Id_ReportDynamic = " & idreportdynamic & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicParameter
            With obj
                .Id = row.Item(0)
                .ParameterName = row.Item(1).ToString
                .Id_ReportDynamic = CLng(row.Item(2))
                .Id_RelatedTable = CLng(row.Item(3))
                .Id_RelatedColumn = CLng(row.Item(4))
                .IsInMaster = row.Item(5)

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
