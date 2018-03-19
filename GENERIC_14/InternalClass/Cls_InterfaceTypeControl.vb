Imports System.Data.SqlServerCe
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Xml

Public Class Cls_InterfaceTypeControl
	

#Region "Attribut"

    Private _id As Long

    Private _Id_Interface As Long
    Private _Interface As Cls_Interface
    Private _Id_TypeControl As Long
    Private _TypeControl As Cls_TypeControl

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
            Dim commandString As String = "Insert into tbl_InterfaceTypeControl (Id_Interface, Id_TypeControl) values (" & _Id_Interface & " ," & _Id_TypeControl & " ); "

            Try
                ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
                ObjectConnection.Open()
                ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
                ObjectCommand.ExecuteNonQuery()
                commandString = "SELECT TOP(1) Id_InterfaceTypeControl from tbl_InterfaceTypeControl order by Id_Column desc"
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
        Dim commandString As String = "Update tbl_InterfaceTypeControl set  Id_Interface = " & _Id_Interface & ", Id_TypeControl = " & _Id_TypeControl & ""
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
        Dim commandString As String = "Select * from tbl_Column where Id_Column = " & _idpass & ""
        ObjectConnection = New SqlCeConnection(_systeme.getConnectionString)
        ObjectConnection.Open()
        ObjectCommand = New SqlCeCommand(commandString, ObjectConnection)
        Dim myReader As SqlCeDataReader = ObjectCommand.ExecuteReader()
        While myReader.Read()
            _id = myReader.Item(0)
            _Id_Interface = myReader.Item(1)
            _Id_TypeControl = myReader.Item(2)
            
        End While
        myReader.Close()
        ObjectConnection.Close()
        Return True
    End Function


    
#End Region

#Region " Search "
    

    

    

#End Region

#Region " Other Methods "

#End Region

	
End Class
