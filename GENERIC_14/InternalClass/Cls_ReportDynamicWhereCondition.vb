Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamicWhereCondition

#Region "Attribut"
    Private _id As Long

    Private _Id_ReportDynamic As Long
    Private _ReportDynamic As Cls_ReportDynamic
    Private _Id_RelatedTable1 As Long
    Private _RelatedTable1 As Cls_ReportDynamicTable
    Private _Id_RelatedColumn1 As Long
    Private _RelatedColumn1 As Cls_Column
    Private _Id_RelatedTable2 As Long
    Private _RelatedTable2 As Cls_ReportDynamicTable
    Private _Id_RelatedColumn2 As Long
    Private _RelatedColumn2 As Cls_Column

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
                If _ReportDynamic.Id <> value.Id Then
                    _Id_ReportDynamic = value.Id
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property ReportDynamic_Name As String
        Get
            Return ReportDynamic.Name
        End Get
    End Property

    Public Property Id_RelatedTable1() As Long
        Get
            Return _Id_RelatedTable1
        End Get
        Set(ByVal value As Long)
            _Id_RelatedTable1 = value
        End Set
    End Property

    Public Property RelatedTable1 As Cls_ReportDynamicTable

        Get
            If Not (_RelatedTable1 Is Nothing) Then
                If (_RelatedTable1.Id = 0) Or (_RelatedTable1.Id <> _Id_RelatedTable1) Then
                    _RelatedTable1 = New Cls_ReportDynamicTable(_Id_RelatedTable1)
                End If
            Else
                _RelatedTable1 = New Cls_ReportDynamicTable(_Id_RelatedTable1)
            End If

            Return _RelatedTable1
        End Get
        Set(ByVal value As Cls_ReportDynamicTable)
            If value Is Nothing Then

                _Id_RelatedTable1 = 0
            Else
                If _RelatedTable1.Id <> value.Id Then
                    _Id_RelatedTable1 = value.Id
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedTable1_CompleteName As String
        Get
            Return RelatedTable1.CompleteName
        End Get
    End Property


    Public Property Id_RelatedColumn1() As Long
        Get
            Return _Id_RelatedColumn1
        End Get
        Set(ByVal value As Long)
            _Id_RelatedColumn1 = value
        End Set
    End Property

    Public Property RelatedColumn1 As Cls_Column

        Get
            If Not (_RelatedColumn1 Is Nothing) Then
                If (_RelatedColumn1.ID = 0) Or (_RelatedColumn1.ID <> _Id_RelatedColumn1) Then
                    _RelatedColumn1 = New Cls_Column(_Id_RelatedColumn1)
                End If
            Else
                _RelatedColumn1 = New Cls_Column(_Id_RelatedColumn1)
            End If

            Return _RelatedColumn1
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_RelatedColumn1 = 0
            Else
                If _RelatedColumn1.ID <> value.ID Then
                    _Id_RelatedColumn1 = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedColumn1_Name As String
        Get
            Return RelatedColumn1.Name
        End Get
    End Property

    Public Property Id_RelatedTable2() As Long
        Get
            Return _Id_RelatedTable2
        End Get
        Set(ByVal value As Long)
            _Id_RelatedTable2 = value
        End Set
    End Property

    Public Property RelatedTable2 As Cls_ReportDynamicTable

        Get
            If Not (_RelatedTable2 Is Nothing) Then
                If (_RelatedTable2.Id = 0) Or (_RelatedTable2.Id <> _Id_RelatedTable2) Then
                    _RelatedTable2 = New Cls_ReportDynamicTable(_Id_RelatedTable2)
                End If
            Else
                _RelatedTable2 = New Cls_ReportDynamicTable(_Id_RelatedTable2)
            End If

            Return _RelatedTable2
        End Get
        Set(ByVal value As Cls_ReportDynamicTable)
            If value Is Nothing Then

                _Id_RelatedTable2 = 0
            Else
                If _RelatedTable2.Id <> value.Id Then
                    _Id_RelatedTable2 = value.Id
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedTable2_CompleteName As String
        Get
            Return RelatedTable2.CompleteName
        End Get
    End Property


    Public Property Id_RelatedColumn2() As Long
        Get
            Return _Id_RelatedColumn2
        End Get
        Set(ByVal value As Long)
            _Id_RelatedColumn2 = value
        End Set
    End Property

    Public Property RelatedColumn2 As Cls_Column

        Get
            If Not (_RelatedColumn2 Is Nothing) Then
                If (_RelatedColumn2.ID = 0) Or (_RelatedColumn2.ID <> _Id_RelatedColumn2) Then
                    _RelatedColumn2 = New Cls_Column(_Id_RelatedColumn2)
                End If
            Else
                _RelatedColumn2 = New Cls_Column(_Id_RelatedColumn2)
            End If

            Return _RelatedColumn2
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_RelatedColumn2 = 0
            Else
                If _RelatedColumn2.ID <> value.ID Then
                    _Id_RelatedColumn2 = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedColumn2_Name As String
        Get
            Return RelatedColumn2.Name
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
            Dim commandString As String = "Insert into tbl_ReportDynamicWhereCondition ( Id_ReportDynamic, Id_RelatedTable1, Id_RelatedColumn1, Id_RelatedTable2, Id_RelatedColumn2) values (" & _Id_ReportDynamic & "," & _Id_RelatedTable1 & "," & _Id_RelatedColumn1 & "," & _Id_RelatedTable2 & "," & _Id_RelatedColumn2 & "); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamicWhereCondition from tbl_ReportDynamicWhereCondition order by Id_ReportDynamicWhereCondition desc"
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
        Dim commandString As String = "Update tbl_ReportDynamicWhereCondition set   Id_ReportDynamic = " & _Id_ReportDynamic & " , Id_RelatedTable1  = " & Id_RelatedTable1 & ", Id_RelatedColumn1 = " & _Id_RelatedColumn1 & ",  Id_RelatedTable2  = " & Id_RelatedTable2 & ", Id_RelatedColumn2 = " & _Id_RelatedColumn2 & ",  where Id_ReportDynamicWhereCondition =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function


#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamicWhereCondition)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicWhereCondition)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicWhereCondition"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicWhereCondition
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamic = CLng(row.Item(1))
                .Id_RelatedTable1 = CLng(row.Item(2))
                .Id_RelatedColumn1 = CLng(row.Item(3))
                .Id_RelatedTable2 = CLng(row.Item(4))
                .Id_RelatedColumn2 = CLng(row.Item(5))
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllById_Report(ByVal idreportdynamic As Long) As List(Of Cls_ReportDynamicWhereCondition)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicWhereCondition)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicWhereCondition where Id_ReportDynamic = " & idreportdynamic & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicWhereCondition
            With obj
                .Id = row.Item(0)
                .Id_ReportDynamic = CLng(row.Item(1))
                .Id_RelatedTable1 = CLng(row.Item(2))
                .Id_RelatedColumn1 = CLng(row.Item(3))
                .Id_RelatedTable2 = CLng(row.Item(4))
                .Id_RelatedColumn2 = CLng(row.Item(5))
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
