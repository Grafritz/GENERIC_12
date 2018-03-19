Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_InterfaceColumn



#Region "Attribut"

    Private _id As Long
    Private _Id_Interface As Long
    Private _Interface As Cls_Interface
    Private _Id_Column As Long
    Private _Column As Cls_Column
    Private _Position As Long
    Private _Id_Control As Long
    Private _Control As Cls_Control

#End Region

#Region "New"
    Public Sub New()

    End Sub

    Public Sub New(ByVal idColumn As Long, ByVal columnname As String)
        '    Read(idColumn, columnname)
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


    Public Property Id_Interface() As Long
        Get
            Return _Id_Interface
        End Get
        Set(ByVal value As Long)
            _Id_Interface = value
        End Set
    End Property

    Public Property InterfaceN As Cls_Interface

        Get
            If Not (_Interface Is Nothing) Then
                If (_Interface.ID = 0) Or (_Interface.ID <> _Id_Interface) Then
                    _Interface = New Cls_Interface(_Id_Interface)
                End If
            Else
                _Interface = New Cls_Interface(_Id_Interface)
            End If

            Return _Interface
        End Get
        Set(ByVal value As Cls_Interface)
            If Value Is Nothing Then

                _Id_Interface = 0
            Else
                If _Interface.ID <> Value.ID Then
                    _Id_Interface = Value.ID
                End If
            End If
        End Set
    End Property




    Public Property Id_Column() As Long
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

     Public Property Position() As Long
		Get
			Return _Position
		End Get
		Set
			_Position = value
		End Set
	End Property

    Public Property Id_Control() As Long
        Get
            Return _Id_Control
        End Get
        Set(ByVal value As Long)
            _Id_Control = value
        End Set
    End Property

    Public Property Control As Cls_Control

        Get
            If Not (_Control Is Nothing) Then
                If (_Control.ID = 0) Or (_Control.ID <> _Id_Control) Then
                    _Control = New Cls_Control(_Id_Control)
                End If
            Else
                _Control = New Cls_Control(_Id_Control)
            End If

            Return _Control
        End Get
        Set(ByVal value As Cls_Control)
            If value Is Nothing Then

                _Id_Control = 0
            Else
                If _Control.ID <> value.ID Then
                    _Id_Control = value.ID
                End If
            End If
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
            Dim commandString As String = "Insert into tbl_InterfaceColumn (Id_Interface, Id_Column, Position , Id_Control) values ( " & _Id_Interface & ", " & _Id_Column & ", " & Position & ", " & _Id_Control & " ); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_InterfaceColumn from tbl_InterfaceColumn order by Id_InterfaceColumn desc"
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
        Dim commandString As String = "Update tbl_InterfaceColumn set  Id_Interface = '" & _Id_Interface & "' , Id_Column = " & _Id_Column & " , Position = " & Position & ", Id_Control = " & _Id_Control & ""
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
        Dim commandString As String = "Select * from tbl_InterfaceColumn where Id_InterfaceColumn = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Id_Interface = myReader.Item(1)
            _Id_Column = myReader.Item(2)
            _Id_Control = myReader.Item(3)
            _Position = myReader.Item(4)

        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function


    

#End Region

#Region " Search "
    Public Shared Function SearchAllBy_Interface(ByVal idInterface As Long) As List(Of Cls_InterfaceColumn)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_InterfaceColumn)
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_Interface where Id_Interface = " & idInterface & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_InterfaceColumn
            With obj
                ._id = row.Item(0)
                ._Id_Interface = row.Item(1)
                ._Id_Column = row.Item(2)
            End With
            objs.Add(obj)
        Next
        ObjectConnection.Close()
        Return objs
    End Function

    Public Shared Function SearchAllBy_Column(ByVal idStructure As Long) As List(Of Cls_InterfaceColumn)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_InterfaceColumn)
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
            Dim obj As New Cls_InterfaceColumn
            With obj
                ._id = row.Item(0)
                ._Id_Interface = row.Item(1)
                ._Id_Column = row.Item(2)
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
