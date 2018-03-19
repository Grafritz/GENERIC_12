Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamic

#Region "Attribut"
    Private _id As Long
    Private _Name As String
    Private _Id_MasterTable As Long
    Private _MasterTable As Cls_Table
    Private _Id_TypeReport As Long
    Private _TypeReport As Cls_TypeReport
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

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property

    Public Property Id_MasterTable() As Long
        Get
            Return _Id_MasterTable
        End Get
        Set(ByVal value As Long)
            _Id_MasterTable = value
        End Set
    End Property


    Public Property MasterTable As Cls_Table

        Get
            If Not (_MasterTable Is Nothing) Then
                If (_MasterTable.ID = 0) Or (_MasterTable.ID <> _Id_MasterTable) Then
                    _MasterTable = New Cls_Table(_Id_MasterTable)
                End If
            Else
                _MasterTable = New Cls_Table(_Id_MasterTable)
            End If

            Return _MasterTable
        End Get
        Set(ByVal value As Cls_Table)
            If value Is Nothing Then

                _Id_MasterTable = 0
            Else
                If _MasterTable.ID <> value.ID Then
                    _Id_MasterTable = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property MasterTable_Name As String
        Get
            Return MasterTable.Name
        End Get
    End Property

    Public Property Id_TypeReport() As Long
        Get
            Return _Id_TypeReport
        End Get
        Set(ByVal value As Long)
            _Id_TypeReport = value
        End Set
    End Property


    Public Property TypeReport As Cls_TypeReport

        Get
            If Not (_TypeReport Is Nothing) Then
                If (_TypeReport.ID = 0) Or (_TypeReport.ID <> _Id_TypeReport) Then
                    _TypeReport = New Cls_TypeReport(_Id_TypeReport)
                End If
            Else
                _TypeReport = New Cls_TypeReport(_Id_TypeReport)
            End If

            Return _TypeReport
        End Get
        Set(ByVal value As Cls_TypeReport)
            If value Is Nothing Then

                _Id_TypeReport = 0
            Else
                If _TypeReport.ID <> value.ID Then
                    _Id_TypeReport = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property TypeReport_Name As String
        Get
            Return TypeReport.Name
        End Get
    End Property

    Public ReadOnly Property ListofReportDynamicTable As List(Of Cls_ReportDynamicTable)
        Get
            Return Cls_ReportDynamicTable.SearchAllbyId_ReportDynamic(_id)
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
            Dim commandString As String = "Insert into tbl_ReportDynamic (Name, Id_MasterTable, Id_TypeReport) values ('" & _Name & "' ,'" & _Id_MasterTable & "', '" & _Id_TypeReport & "'); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamic from tbl_ReportDynamic order by Id_ReportDynamic desc"
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
        Dim commandString As String = "Update tbl_ReportDynamic set  Name = '" & _Name & "' , Id_MasterTable = " & _Id_MasterTable & "  , Id_TypeReport = " & _Id_TypeReport & " where Id_ReportDynamic =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function

    Public Function Read(ByVal _idone As Long) As Boolean
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Select * from tbl_ReportDynamic where Id_ReportDynamic = " & _idone & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Name = myReader.Item(1)
            _Id_MasterTable = myReader.Item(2)
            Id_TypeReport = myReader.Item(3)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function
#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamic)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamic)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamic "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamic
            With obj
                .Id = CLng(row.Item(0))
                .Name = row.Item(1).ToString
                .Id_MasterTable = CLng(row.Item(2))
                .Id_TypeReport = CLng(row.Item(3))
                
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
