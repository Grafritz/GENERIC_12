Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_DependantTable

#Region "Attribut"
    Private _id As Long
    Private _Name As String
    Private _Id_Parenttable As Long
    Private _ParentTable As Cls_Table
    Private _Id_DependantTable As Long
    Private _DependantTable As Cls_Table
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

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property

    Public Property Id_Parenttable() As Long
        Get
            Return _Id_Parenttable
        End Get
        Set(ByVal value As Long)
            _Id_Parenttable = value
        End Set
    End Property


    Public Property ParentTable As Cls_Table

        Get
            If Not (_ParentTable Is Nothing) Then
                If (_ParentTable.ID = 0) Or (_ParentTable.ID <> _Id_Parenttable) Then
                    _ParentTable = New Cls_Table(_Id_Parenttable)
                End If
            Else
                _ParentTable = New Cls_Table(_Id_Parenttable)
            End If

            Return _ParentTable
        End Get
        Set(ByVal value As Cls_Table)
            If value Is Nothing Then

                _Id_Parenttable = 0
            Else
                If _ParentTable.ID <> value.ID Then
                    _Id_Parenttable = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property ParentTable_Name
        Get
            Return ParentTable.Name
        End Get
    End Property



    Public Property Id_DependantTable() As Long
        Get
            Return _Id_DependantTable
        End Get
        Set(ByVal value As Long)
            _Id_DependantTable = value
        End Set
    End Property

    Public Property DependantTable As Cls_Table

        Get
            If Not (_DependantTable Is Nothing) Then
                If (_DependantTable.ID = 0) Or (_DependantTable.ID <> _Id_DependantTable) Then
                    _DependantTable = New Cls_Table(_Id_DependantTable)
                End If
            Else
                _DependantTable = New Cls_Table(_Id_DependantTable)
            End If

            Return _DependantTable
        End Get
        Set(ByVal value As Cls_Table)
            If value Is Nothing Then

                _Id_DependantTable = 0
            Else
                If _DependantTable.ID <> value.ID Then
                    _Id_DependantTable = value.ID
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property DependantTable_Name
        Get
            Return DependantTable.Name
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
    Public Function Insert() As Integer

        Dim cantpass As Boolean = True

        While cantpass
            Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
            Dim ObjectConnection As SqlCeConnection
            Dim ObjectCommand As SqlCeCommand
            Dim commandString As String = "Insert into tbl_DependantTable (Name, Id_ParentTable, Id_DependantTable) values ('" & _Name & "' ,'" & _Id_Parenttable & _Id_DependantTable & "'); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_GroupTable from tbl_GroupTable order by Id_GroupTable desc"
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

    Public Function Update() As Integer
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim commandString As String = "Update tbl_GroupTable set  Name = '" & _Name & "' ,  Id_ParentTable = '" & Id_DependantTable & "' where Id_GroupTable =  '" & _id & "'"
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectCommand.ExecuteNonQuery()
        ObjectConnection.Close()
        Return _id
    End Function


#End Region

#Region " Search "
    Public Shared Function SearchAll() As List(Of Cls_GroupTable)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_GroupTable)

        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_GroupTable "
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_GroupTable
            With obj
                .Id = row.Item(0)
                .Name = row.Item(1)
                .Id_Parenttable = row.Item(2)
                .Id_LiaisonTable = row.Item(3)
                .Id_ChildTable = row.Item(4)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function
#End Region

End Class
