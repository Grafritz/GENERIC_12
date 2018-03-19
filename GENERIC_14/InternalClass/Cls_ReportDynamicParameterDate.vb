Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_ReportDynamicParameterDate

#Region "Attribut"
    Private _id As Long
    Private _ParameterNameDebut As String
    Private _Id_ReportDynamic As Long
    Private _ReportDynamic As Cls_ReportDynamic
    Private _Id_RelatedTable As Long
    Private _RelatedTable As Cls_ReportDynamicTable
    Private _Id_RelatedColumnDebut As Long
    Private _RelatedColumnDebut As Cls_Column
    Private _ParameterNameFin As String
    Private _Id_RelatedColumnFin As Long
    Private _RelatedColumnFin As Cls_Column
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

    Public Property ParameterNameDebut() As String
        Get
            Return _ParameterNameDebut
        End Get
        Set(ByVal value As String)
            _ParameterNameDebut = value
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

    Public Property Id_RelatedColumnDebut() As Long
        Get
            Return _Id_RelatedColumnDebut
        End Get
        Set(ByVal value As Long)
            _Id_RelatedColumnDebut = value
        End Set
    End Property

    Public Property RelatedColumnDebut As Cls_Column

        Get
            If Not (_RelatedColumnDebut Is Nothing) Then
                If (_RelatedColumnDebut.ID = 0) Or (_RelatedColumnDebut.ID <> _Id_RelatedColumnDebut) Then
                    _RelatedColumnDebut = New Cls_Column(_Id_RelatedColumnDebut)
                End If
            Else
                _RelatedColumnDebut = New Cls_Column(_Id_RelatedColumnDebut)
            End If

            Return _RelatedColumnDebut
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_RelatedColumnDebut = 0
            Else
                If _RelatedColumnDebut.ID <> value.ID Then
                    _Id_RelatedColumnDebut = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedColumnDebut_Name As String
        Get
            Return RelatedColumnDebut.Name
        End Get
    End Property

    Public Property ParameterNameFin() As String
        Get
            Return _ParameterNameFin
        End Get
        Set(ByVal value As String)
            _ParameterNameFin = value
        End Set
    End Property


    Public Property Id_RelatedColumnFin() As Long
        Get
            Return _Id_RelatedColumnFin
        End Get
        Set(ByVal value As Long)
            _Id_RelatedColumnFin = value
        End Set
    End Property

    Public Property RelatedColumnFin As Cls_Column

        Get
            If Not (_RelatedColumnFin Is Nothing) Then
                If (_RelatedColumnFin.ID = 0) Or (_RelatedColumnFin.ID <> _Id_RelatedColumnFin) Then
                    _RelatedColumnFin = New Cls_Column(_Id_RelatedColumnFin)
                End If
            Else
                _RelatedColumnFin = New Cls_Column(_Id_RelatedColumnFin)
            End If

            Return _RelatedColumnFin
        End Get
        Set(ByVal value As Cls_Column)
            If value Is Nothing Then

                _Id_RelatedColumnFin = 0
            Else
                If _RelatedColumnFin.ID <> value.ID Then
                    _Id_RelatedColumnFin = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property RelatedColumnFin_Name As String
        Get
            Return RelatedColumnFin.Name
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
            Dim commandString As String = "Insert into tbl_ReportDynamicParameterDate (ParameterNameDebut, Id_ReportDynamic, Id_RelatedTable, Id_RelatedColumnDebut, ParameterNameFin, Id_RelatedColumnFin, IsInMaster ) values ('" & _ParameterNameDebut & "' ," & _Id_ReportDynamic & "," & _Id_RelatedTable & "," & _Id_RelatedColumnDebut & ", '" & _ParameterNameFin & "'," & _Id_RelatedColumnFin & ", " & IIf(_IsInMaster, 1, 0) & "); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_ReportDynamicParameterDate from tbl_ReportDynamicParameterDate order by Id_ReportDynamicParameterDate desc"
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
        Dim commandString As String = "Update tbl_ReportDynamicParameterDate set  ParameterNameDebut = '" & _ParameterNameDebut & "' , Id_ReportDynamic = " & _Id_ReportDynamic & " , Id_RelatedTable  = " & Id_RelatedTable & ", Id_RelatedColumnDebut = " & Id_RelatedColumnDebut & ", ParameterNameFin = '" & _ParameterNameFin & "' , IsInMaster = " & IsInMaster & "  where Id_ReportDynamicParameterDate =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function


#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_ReportDynamicParameterDate)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicParameterDate)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicParameterDate"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicParameterDate
            With obj
                .Id = row.Item(0)
                .ParameterNameDebut = row.Item(1).ToString
                .Id_ReportDynamic = CLng(row.Item(2))
                .Id_RelatedTable = CLng(row.Item(3))
                .Id_RelatedColumnDebut = CLng(row.Item(4))
                .ParameterNameFin = row.Item(5).ToString
                .Id_RelatedColumnFin = row.Item(6).ToString
                .IsInMaster = row.Item(7)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllById_Report(ByVal idreportdynamic As Long) As List(Of Cls_ReportDynamicParameterDate)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_ReportDynamicParameterDate)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_ReportDynamicParameterDate where Id_ReportDynamic = " & idreportdynamic & " "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_ReportDynamicParameterDate
            With obj
                .Id = row.Item(0)
                .ParameterNameDebut = row.Item(1).ToString
                .Id_ReportDynamic = CLng(row.Item(2))
                .Id_RelatedTable = CLng(row.Item(3))
                .Id_RelatedColumnDebut = CLng(row.Item(4))
                .ParameterNameFin = row.Item(5).ToString
                .Id_RelatedColumnFin = row.Item(6).ToString
                .IsInMaster = row.Item(7)

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
