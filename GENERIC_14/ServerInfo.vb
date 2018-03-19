
#Region "Namespace references"
Imports System
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Data
Imports System.Data.OleDb
#End Region

Namespace Util
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Class: SqlServerInfo                                                  '
    ' Provides information about a MS SQL server instance.                  '
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
    'Copyright &#169; 2005, James M. Curran.                                '
    'First published on CodeProject.com, Nov 2005                           '
    'May be used freely.                                                    '
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
    Public Class SqlServerInfo
#Region "Fields"
        Private oServerName As String
        Private oInstanceName As String
        Private oIsClustered As Boolean
        Private oVersion As String
        Private otcpPort As Integer
        Private oNp As String
        Private oRpc As String
        Private oIP As IPAddress
        Private oCatalogs As StringCollection
        Private oUserId As String
        Private oPassword As String
        Private oIntegratedSecurity As Boolean = True
        Private oTimeOut As Integer = 2
#End Region

#Region "Constructors"
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Initializes a new instance of the SqlServerInfo class.                 '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Private Sub New()

        End Sub

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Initializes a new instance of the SqlServerInfo class.                 '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public Sub New(ByVal ip As IPAddress, ByVal info As Byte())

            Me.New(ip, System.Text.ASCIIEncoding.ASCII.GetString(info, 3, BitConverter.ToInt16(info, 1)))

        End Sub

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Initializes a new instance of the SqlServerInfo class.                 '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public Sub New(ByVal ip As IPAddress, ByVal info As String)

            oIP = ip

            Dim nvs As String() = info.Split(";"c)

            For i As Integer = 0 To nvs.Length - 1 Step 2

                Select Case nvs(i).ToLower()
                    Case "servername"
                        Me.oServerName = nvs(i + 1)
                        Exit Select
                    Case "instancename"

                        Me.oInstanceName = nvs(i + 1)
                        Exit Select
                    Case "isclustered"

                        Me.oIsClustered = (nvs(i + 1).ToLower() = "yes")
                        'bool.Parse(nvs[i+1]); 
                        Exit Select
                    Case "version"

                        Me.oVersion = nvs(i + 1)
                        Exit Select
                    Case "tcp"

                        Me.otcpPort = Integer.Parse(nvs(i + 1))
                        Exit Select
                    Case "np"

                        Me.oNp = nvs(i + 1)
                        Exit Select
                    Case "rpc"

                        Me.oRpc = nvs(i + 1)
                        Exit Select

                End Select

            Next

        End Sub

#End Region

#Region "Public Properties"

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the IP address.                                                   '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public ReadOnly Property Address() As IPAddress
            Get
                Return oIP
            End Get
        End Property
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Gets the name of the server.                                           '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public ReadOnly Property ServerName() As String
            Get
                Return oServerName
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the name of the instance.                                         '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public ReadOnly Property InstanceName() As String
            Get
                Return oInstanceName
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets a value indicating whether this instance is clustered.            '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public ReadOnly Property IsClustered() As Boolean
            Get
                Return oIsClustered
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the version.                                                      '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public ReadOnly Property Version() As String
            Get
                Return oVersion
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the TCP port.                                                     '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public ReadOnly Property TcpPort() As Integer
            Get
                Return otcpPort
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the named pipe.                                                   '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public ReadOnly Property NamedPipe() As String
            Get
                Return oNp
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets the catalogs.                                                     '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public ReadOnly Property Catalogs() As StringCollection
            Get
                If oCatalogs Is Nothing Then
                    oCatalogs = GetCatalogs()
                End If
                Return oCatalogs
            End Get
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets or sets the user id.                                              '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        Public Property UserId() As String
            Get
                Return oUserId
            End Get
            Set(ByVal value As String)
                oUserId = value
                oIntegratedSecurity = False
            End Set
        End Property

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Gets or sets the password.                                             '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Property Password() As String
            Get
                Return oPassword
            End Get
            Set(ByVal value As String)
                oPassword = value
                oIntegratedSecurity = False
            End Set
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets or sets a value indicating whether [integrated security].         ' 
        'If true then Integrated Security enabled.                              '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Property IntegratedSecurity() As Boolean
            Get
                Return oIntegratedSecurity
            End Get
            Set(ByVal value As Boolean)
                oIntegratedSecurity = value
            End Set
        End Property

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Gets or sets the time out.                                             '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Property TimeOut() As Integer
            Get
                Return oTimeOut
            End Get
            Set(ByVal value As Integer)
                oTimeOut = value
            End Set
        End Property

#End Region

#Region "Public Methods"

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Tests the connection.                                                  '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Function TestConnection() As Boolean
            Dim conn As OleDbConnection = Me.GetConnection()
            Dim success As Boolean = False
            Try
                conn.Open()
                conn.Close()
                success = True
            Catch
            End Try
            Return success
        End Function

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Returns a string representing the fully qualified instance name        '
        '(<Server Name>\<Instance Name>                                         '     
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Overloads Overrides Function ToString() As String
            If Me.InstanceName Is Nothing OrElse Me.InstanceName = "MSSQLSERVER" Then
                Return Me.ServerName
            Else
                Return Me.ServerName + "\" + Me.InstanceName
            End If
        End Function
#End Region

#Region "Private Methods"

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Private Function GetCatalogs() As StringCollection

            Dim catalogs As New StringCollection()

            Try
                Dim myConnection As OleDbConnection = Me.GetConnection()

                myConnection.Open()

                Dim schemaTable As DataTable = myConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, Nothing)

                myConnection.Close()

                For Each dr As DataRow In schemaTable.Rows

                    catalogs.Add(TryCast(dr(0), String))

                Next

            Catch ex As Exception

                ' System.Windows.Forms.MessageBox.Show(ex.Message); 

            End Try

            Return catalogs

        End Function

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Private Function GetConnection() As OleDbConnection

            Dim myConnString As String = IIf(Me.IntegratedSecurity, _
                                             [String].Format("Provider=SQLOLEDB;DataSource={0};Integrated Security=SSPI;Connect Timeout={1}", _
                                             Me, Me.TimeOut), [String].Format("Provider=SQLOLEDB;DataSource={0};User Id={1};Password={2};Connect Timeout={3}", _
                                             Me, Me.UserId, Me.Password, Me.TimeOut))

            Return New OleDbConnection(myConnString)

        End Function

#End Region

#Region "Public Static Method - Seek"

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''' 
        'Seeks SQL servers on this network. Returnss an array of SqlServerInfo  '
        'objects describing Sql Servers on this network.                        '
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Shared Function Seek() As SqlServerInfo()

            Dim socket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1)
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000)

            ' For .Net v 2.0 it's a bit simpler 
            ' socket.EnableBroadcast = true; // for .Net v2.0 
            ' socket.ReceiveTimeout = 3000; // for .Net v2.0 

            Dim servers As New ArrayList()

            Try

                Dim msg As Byte() = New Byte() {2}
                Dim ep As New IPEndPoint(IPAddress.Broadcast, 1434)

                socket.SendTo(msg, ep)

                Dim cnt As Integer = 0
                Dim bytBuffer As Byte() = New Byte(1023) {}

                Do

                    cnt = socket.Receive(bytBuffer)
                    servers.Add(New SqlServerInfo(Nothing, bytBuffer))
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 300)

                Loop While cnt <> 0

            Catch socex As SocketException

                Const WSAETIMEDOUT As Integer = 10060

                ' Connection timed out. 
                Const WSAEHOSTUNREACH As Integer = 10065

                ' No route to host. 
                ' Re-throw if it's not a timeout. 
                ' DO nothing...... 
                If socex.ErrorCode = WSAETIMEDOUT OrElse socex.ErrorCode = WSAEHOSTUNREACH Then

                Else

                    ' Console.WriteLine("{0} {1}", socex.ErrorCode, socex.Message); 
                    Throw

                End If

            Finally
                socket.Close()
            End Try

            ' Copy from the untyped but expandable ArrayList, to a 
            ' type-safe but fixed array of SqlServerInfos. 

            Dim aServers As SqlServerInfo() = New SqlServerInfo(servers.Count - 1) {}

            servers.CopyTo(aServers)

            Return aServers

        End Function

#End Region

    End Class
End Namespace