Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_Control

#Region "Attribut"

    Private _id As Long
    Private _Name As String
    Private _Id_TypeControl As Long
    Private _TypeControl As Cls_TypeControl

#End Region

#Region "New"
    Public Sub New()

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


    Public Property Id_TypeControl() As Long
        Get
            Return _Id_TypeControl
        End Get
        Set(ByVal value As Long)
            _Id_TypeControl = value
        End Set
    End Property

    Public Property TypeControl As Cls_TypeControl

        Get
            If Not (_TypeControl Is Nothing) Then
                If (_TypeControl.ID = 0) Or (_TypeControl.ID <> _Id_TypeControl) Then
                    _TypeControl = New Cls_TypeControl(_Id_TypeControl)
                End If
            Else
                _TypeControl = New Cls_TypeControl(_Id_TypeControl)
            End If

            Return _TypeControl
        End Get
        Set(ByVal value As Cls_TypeControl)
            If Value Is Nothing Then

                _Id_TypeControl = 0
            Else
                If _TypeControl.ID <> Value.ID Then
                    _Id_TypeControl = Value.ID
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
            Dim commandString As String = "Insert into tbl_Control (Name, Id_TypeControl) values ('" & _Name & "' ," & _Id_TypeControl & " ); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_Control from tbl_Control order by Id_Control desc"
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
        Dim commandString As String = "Update tbl_Control set  Name = '" & _Name & "' , Id_TypeControl = " & _Id_TypeControl & "  where Id_Control =  " & _id & ""
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
        Dim commandString As String = "Select * from tbl_Control where Id_Control = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Name = myReader.Item(1)
            _Id_TypeControl = myReader.Item(2)
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return _id
    End Function


#End Region

#Region " Search "
    Public Shared Function SearchAllBy_TypeControl(ByVal idtype As Long) As List(Of Cls_Control)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim objs As New List(Of Cls_Control)
        Dim ObjectConnection As SqlCeConnection
        Dim ObjectCommand As SqlCeCommand
        Dim ObjectAdapter As SqlCeDataAdapter
        Dim ds As New DataSet
        Dim commandString As String = "Select * from tbl_Control where Id_TypeControl = " & idtype & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        ObjectAdapter = New SqlCeDataAdapter(ObjectCommand)
        ObjectAdapter.Fill(ds)
        For Each row As DataRow In ds.Tables(0).Rows
            Dim obj As New Cls_Control
            With obj
                ._id = row.Item(0)
                ._Name = row.Item(2)
                ._Id_TypeControl = row.Item(3)
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
