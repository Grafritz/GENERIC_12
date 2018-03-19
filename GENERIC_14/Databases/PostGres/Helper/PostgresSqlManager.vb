Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.OleDb
Imports Npgsql
Imports System.IO
Imports System.Text
Imports System.Xml
Imports System.ComponentModel

Public Class PostgresSqlManager

#Region "Attributes"
    Public Shared servername As String
    Public Shared password As String
    Public Shared database As String
    Public Shared user_login As String
    Public Shared port As String
    Public Shared ForeinKeyPrefix As String
    Public Shared Schema As DataTable
    Public Shared List_of_Nervous_Types_My_Sql As New List(Of String)
#End Region

#Region "Loading Tables Fonctions"

    Public Shared Function LoadTableStructure(ByVal table As String) As DataSet
        port = "5432"
        Dim ConString As String = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", servername, port, user_login, password, database)
        Try
            Dim Con As New NpgsqlConnection(ConString)
            Con.Open()
            Dim cmd As New NpgsqlCommand
         
            cmd.CommandText = " select SC.Column_name , SC.data_type, " &
                                    "SC.CHARACTER_MAXIMUM_LENGTH," &
                                    "SC.numeric_precision," &
                                    "SC.numeric_scale," &
                                    "SC.iS_NULLABLE," &
                                    "TC.CONSTRAINT_TYPE," &
                                    "TC.CONSTRAINT_NAME ," &
                                    "CC.TABLE_NAME," &
                                    "CC.Column_name" &
                                " from information_schema.columns SC" &
                                " LEFT OUTER JOIN " &
                                "INFORMATION_SCHEMA.KEY_COLUMN_USAGE  KC on SC.Column_name = KC.COLUMN_NAME and SC.TABLE_NAME = KC.TABLE_NAME " &
                                "LEFT OUTER JOIN  " &
                                "INFORMATION_SCHEMA.TABLE_CONSTRAINTS  TC on KC.CONSTRAINT_NAME = TC.CONSTRAINT_NAME" &
                                " LEFT OUTER JOIN  INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CC" &
                                " on CC.CONSTRAINT_NAME = TC.CONSTRAINT_NAME " &
                                " where SC.table_catalog =  '" & database & "'" &
                                " and SC.table_name = '" & table & "'" &
                                " and SC.table_schema = 'public' "


            cmd.CommandType = CommandType.Text
            cmd.Connection = Con
            Dim ds As New DataSet
            Dim da As NpgsqlDataAdapter
            da = New NpgsqlDataAdapter(cmd)
            da.Fill(ds)
            cmd.Parameters.Clear()
            Con.Close()
            Return ds
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Load Table Structure MySql", MessageBoxButtons.OK)
            '  Error_Log("LoadTableStructure", ex.Message)
        End Try
    End Function

    Public Shared Function LoadUserTablesSchema(ByRef treeview1 As TreeView) As ArrayList

        Dim slTables As ArrayList = New ArrayList()

        'Dim cnString As String = "Persist Security Info=True;" & _
        '            "server=" & servername & ";" & _
        '            "User Id=" & user_login & ";" & _
        '            "database=" & database & ";" & _
        '            "password=" & password & ""
        port = "5432"
        Dim cnString As String = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", servername, port, user_login, password, database)
        Dim strQUERY As String = "select table_name from information_schema.tables where table_catalog = '" & database & "' and table_schema='public'"

        Dim table As DataTable = Nothing
        Dim con As NpgsqlConnection = New NpgsqlConnection(cnString)
        Dim cmd As New NpgsqlCommand
        Dim ds As New DataSet
        Dim ds2 As New DataSet
        Dim da As NpgsqlDataAdapter
        Dim dr2 As DataRow
        Try
            con.Open()
            cmd.Connection = con
            cmd.CommandText = strQUERY
            da = New NpgsqlDataAdapter(strQUERY, con)
            da.Fill(ds2)
            Schema = ds2.Tables(0)
            For Each dr2 In Schema.Rows
                treeview1.Nodes.Add(dr2("table_name"))
            Next
            Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
            _systeme.CreateConnectionLog(servername, user_login, password, TypeDatabase.POSTGRESSQL)
            con.Close()
        Catch x As OleDbException
            slTables = Nothing
        End Try
        Return slTables

    End Function

    Public Shared Function InitializeDb() As Long
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Try
            _systeme.CreateLocalDatabase(database, TypeDatabase.POSTGRESSQL)
        Catch x As OleDbException
        End Try
        Return _systeme.currentDatabase.ID
    End Function

    Public Shared Function InitializeLocalColumn(ByVal iddatabase As Long, ByRef background As BackgroundWorker)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim duree As TimeSpan
        Dim momentdebute As DateTime = Now
        _systeme.CreateLocalTablePart(Schema, iddatabase, background)
        Dim momenttermine As DateTime = Now
        duree = momenttermine - momentdebute
        MessageBox.Show("INTELLIGENT MODE READY:" & duree.ToString)
    End Function

#End Region

#Region "Conversion Fonctions"
    

    Public Shared Function ConvertMySQLDBToJavaType(ByVal Type As String, Optional ByVal canSubstring As Boolean = False) As String
        Dim AndroidTypeHash As New Hashtable
        AndroidTypeHash.Add("bigint", "long")
        AndroidTypeHash.Add("binary", "boolean")
        AndroidTypeHash.Add("bit", "byte")
        AndroidTypeHash.Add("char", "char")
        AndroidTypeHash.Add("date", "Date")
        AndroidTypeHash.Add("datetime", "Date")
        AndroidTypeHash.Add("datetime2", "Date")
        AndroidTypeHash.Add("DATETIMEOFFSET", "Date")
        AndroidTypeHash.Add("decimal", "double")
        AndroidTypeHash.Add("float", "float")
        AndroidTypeHash.Add("int", "int")
        AndroidTypeHash.Add("image", "byte[]")
        AndroidTypeHash.Add("money", "Currency")
        AndroidTypeHash.Add("nchar", "String") '' /* or tableau of char*/
        AndroidTypeHash.Add("nvarchar", "String")
        AndroidTypeHash.Add("numeric", "double")
        AndroidTypeHash.Add("rowversion", "")
        AndroidTypeHash.Add("smallint", "short")
        AndroidTypeHash.Add("smallmoney", "Currency")
        AndroidTypeHash.Add("time", "Time")
        AndroidTypeHash.Add("varbinary", "")
        AndroidTypeHash.Add("varchar", "String")

        If canSubstring Then
            Return AndroidTypeHash(Type.Substring(Type.Length, 4))
        Else
            Return AndroidTypeHash(Type)
        End If

    End Function

#End Region

#Region "Stored Procedure Fonctions "

    Public Shared Function CreateStore(ByVal Name As String) As String
        Dim ds As DataSet = LoadTableStructure(Name)
        Dim cap As Integer
        cap = ds.Tables(0).Rows.Count
        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim valueStore As String = ""
        Dim SpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary", "decimal", "numeric"}
        Dim LevelOneSpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary"}
        Dim LevelTwoSpecialChar As New List(Of String) From {"decimal", "numeric"}

        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)

        Id_table = _table.ListofColumn(0).Name

        Dim count2 As Long = 0
        For Each column As Cls_Column In _table.ListofColumn
            If column.Name <> Id_table Then
                If count < _table.ListofColumn.Count - 4 Then
                    If (paramStore = "") Then
                        paramStore = "" & column.Name.ToString.ToLower & " " & column.Type.PostgresName.ToString
                        champStore = """" & column.Name & """"
                        valueStore = " " & column.Name.ToString.ToLower
                    Else
                        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "" & column.Name.ToString.ToLower & " " & column.Type.PostgresName.ToString
                        champStore &= Chr(13) & Chr(9) & Chr(9) & "," & """" & column.Name & """"
                        valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & " " & column.Name.ToString.ToLower
                    End If
                    count += 1
                Else
                    Exit For
                End If
            End If
        Next

        champStore &= Chr(13) & Chr(9) & Chr(9) & "," & """DateCreated"""

        valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "current_timestamp"
        Dim command As String = Chr(9) & "INSERT INTO """ & Name & """" & Chr(13) _
                                & Chr(9) & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & champStore & Chr(13) _
                                & Chr(9) & ")" & Chr(13) _
                                & Chr(9) & "VALUES" & Chr(13) _
                                & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & valueStore & Chr(13) _
                                & Chr(9) & ");"
        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        objectname = objectname.Substring(0, 1).ToUpper() & objectname.Substring(1, objectname.Length - 1)
        Dim store As String = "CREATE OR REPLACE FUNCTION SP_Insert" & objectname & " " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & paramStore & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "RETURNS SETOF record AS" & Chr(13) _
                            & "$BODY$" & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "select max(""" & Id_table & """) AS ID from """ & _table.Name & """ ;" & Chr(13) & Chr(13) _
                            & "$BODY$" & Chr(13) & Chr(13) _
                            & "LANGUAGE SQL;" & Chr(13)

        Return store
    End Function

    Public Shared Function UpdateStore(ByVal Name As String) As String

        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)

        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim QuerySet As String = ""
        Id_table = _table.ListofColumn.Item(0).Name
        For Each _column As Cls_Column In _table.ListofColumn
            If count < _table.ListofColumn.Count - 4 Then
                If QuerySet = "" Then
                    If _column.Name <> Id_table Then
                        QuerySet = """" & _column.Name & """" & " " & "= " & "" & _column.Name.ToLower
                    End If
                Else
                    QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & """" & _column.Name & """" & " " & "= " & "" & _column.Name.ToLower
                End If
                If (paramStore = "") Then
                    paramStore = "" & _column.Name.ToLower & " " & _column.Type.PostgresName
                Else
                    paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "" & _column.Name.ToLower & " " & _column.Type.PostgresName

                End If
                count += 1
            Else
                Exit For
            End If
        Next

        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "_user" & " character varying"
        QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & """ModifBy"" " & "=" & "_user"
        QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & """DateModif""" & "=" & "current_timestamp" & Chr(13)
        'valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@user"
        'valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "GETDATE()"
        Dim command As String = Chr(9) & "UPDATE """ & Name & """" & Chr(13) _
                                & Chr(9) & "SET" & Chr(13) & Chr(13) _
                                & Chr(9) & Chr(9) & QuerySet & Chr(13)

        '' "USE MCI_db" & Chr(13) & "GO" & Chr(13) _&
        ''"/***Store Update For table  " & Name & "****/" & Chr(13) _ &
        '' "CREATE PROCEDURE [dbo].[SP_Update" & StrConv(Name, VbStrConv.ProperCase) & "] " & Chr(13) _
        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        Dim store As String =
                            "CREATE OR REPLACE FUNCTION SP_Update" & objectname & " " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & paramStore & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "RETURNS void AS" & Chr(13) _
                            & "$BODY$" & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "Where """ & Id_table & """ = " & "" & Id_table.ToLower & Chr(13) & Chr(13) _
                            & "$BODY$" & Chr(13) & Chr(13) _
                            & "LANGUAGE SQL;" & Chr(13)
        Return store
    End Function

    Public Shared Function DeleteStore(ByVal Name As String) As String


       Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)

        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""
        Id_table = _table.ListofColumn.Item(0).Name
        Id_table_type = _table.ListofColumn.Item(0).Type.PostgresName


        Dim command As String = Chr(9) & "DELETE FROM """ & Name & """"
        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        objectname = objectname.Substring(0, 1).ToUpper() & objectname.Substring(1, objectname.Length - 1)
        Dim store As String = "CREATE OR REPLACE FUNCTION SP_Delete" & objectname & " " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & " _ID " & Id_table_type & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                             & "RETURNS void AS" & Chr(13) _
                            & "$BODY$" & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "WHERE """ & Id_table & """ = " & "_ID ;" & Chr(13) & Chr(13) _
                            & "$BODY$" & Chr(13) & Chr(13) _
                            & "LANGUAGE SQL;" & Chr(13)



        Return store
    End Function

    Public Shared Function SelectStore(ByVal Name As String) As String
           Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)

        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""
        Id_table = _table.ListofColumn.Item(0).Name
        Id_table_type = _table.ListofColumn.Item(0).Type.PostgresName

        Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM """ & Name & """"

        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        objectname = objectname.Substring(0, 1).ToUpper() & objectname.Substring(1, objectname.Length - 1)
        Dim store As String = "CREATE OR REPLACE FUNCTION SP_Select" & objectname & "_ByID " & Chr(13) & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & " _ID " & Id_table_type & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                             & "RETURNS SETOF """ & Name & """ AS" & Chr(13) _
                            & "$BODY$" & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "WHERE """ & Id_table & """ = " & "_ID ;" & Chr(13) & Chr(13) _
                            & "$BODY$" & Chr(13) & Chr(13) _
                            & "LANGUAGE SQL;" & Chr(13)
        Return store
    End Function

    Public Shared Function SelectByIndexStore(ByVal Name As String) As String
         Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)
        Dim countIndex As Integer = 0
        Dim QuerySet As String = ""
        Dim storeglobal As String = ""

        Dim paramStore As String = ""
        Dim SelectClause As String = ""
        Dim libelleStored As String = ""

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            If _index.ListofColumn.Count = 1 Then
                Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                              & Chr(9) & "FROM " & Name
                Dim objectname As String = Name.Substring(4, Name.Length - 4)
                Dim store As String =
                                    "CREATE PROCEDURE [dbo].[SP_Select" & objectname & "_" & _index.ListofColumn.Item(0).Name & "] " & Chr(13) _
                                    & Chr(9) & "(" & Chr(13) _
                                    & Chr(9) & Chr(9) & "@" & _index.ListofColumn.Item(0).Name & " " & _index.ListofColumn.Item(0).TrueSqlServerType & Chr(13) _
                                    & Chr(9) & ")" & Chr(13) & Chr(13) _
                                    & "AS" & Chr(13) & Chr(13) _
                                    & command & Chr(13) _
                                    & Chr(9) & "WHERE " & _index.ListofColumn.Item(0).Name & " = " & "@" & _index.ListofColumn.Item(0).Name & Chr(13) & Chr(13) _
                                & "" & Chr(13) & Chr(13)
                storeglobal = storeglobal & store

            Else
                For Each _column In _index.ListofColumn
                    If (paramStore = "") Then
                        paramStore = "@" & _column.Name & " " & _column.TrueSqlServerType
                        SelectClause = "[" & _column.Name & "] = " & "@" & _column.Name
                        libelleStored = "_" & _column.Name
                    Else
                        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@" & _column.Name & " " & _column.TrueSqlServerType
                        SelectClause &= Chr(13) & Chr(9) & "and " & "[" & _column.Name & "] = " & "@" & _column.Name
                        libelleStored &= "_" & _column.Name
                    End If

                Next



                Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                            & Chr(9) & "FROM " & Name & Chr(13) _
                            & Chr(9) & "Where " & SelectClause
                Dim objectname As String = Name.Substring(4, Name.Length - 4)
                Dim store As String =
                                    "CREATE PROCEDURE [dbo].[SP_Select" & objectname & libelleStored & "] " & Chr(13) & Chr(13) _
                                     & Chr(9) & "(" & Chr(13) _
                                    & Chr(9) & Chr(9) & paramStore & Chr(13) _
                                    & Chr(9) & ")" & Chr(13) _
                                    & "AS" & Chr(13) _
                                    & command & Chr(13)
                storeglobal = storeglobal & store


            End If
        Next
        Return storeglobal
    End Function

    Public Shared Function ListAllStore(ByVal Name As String) As String
        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)

        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""
        Id_table = _table.ListofColumn.Item(0).Name
        Id_table_type = _table.ListofColumn.Item(0).Id_Type

        Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM """ & Name & """ ;"

        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        objectname = objectname.Substring(0, 1).ToUpper() & objectname.Substring(1, objectname.Length - 1)
        Dim store As String = "CREATE OR REPLACE FUNCTION SP_ListAll_" & objectname & " ()" & Chr(13) & Chr(13) _
                            & "RETURNS SETOF """ & Name & """ AS" & Chr(13) _
                            & "$BODY$" & Chr(13) _
                            & command & Chr(13) & Chr(13) _
                            & "$BODY$" & Chr(13) & Chr(13) _
                            & "LANGUAGE SQL;" & Chr(13)

        Return store
    End Function

    Public Shared Function ListAllByForeignKey(ByVal Name As String) As String
        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, Name)
        Dim storeglobal As String = ""
        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                  & Chr(9) & "FROM """ & Name & """"
            Dim objectname As String = Name.Substring(4, Name.Length - 4)
            Dim patternname As String = _foreignkey.Column.Name
            Dim store As String =
                                "CREATE OR REPLACE FUNCTION SP_ListAll_" & objectname & "_" & patternname & " " & Chr(13) _
                                & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & "_" & _foreignkey.Column.Name.ToLower & " " & _foreignkey.Column.Type.PostgresName & Chr(13) _
                                & Chr(9) & ")" & Chr(13) & Chr(13) _
                                & "RETURNS SETOF """ & Name & """ AS" & Chr(13) _
                                & "$BODY$" & Chr(13) _
                                & command & Chr(13) _
                                & Chr(9) & "WHERE """ & _foreignkey.Column.Name & """ = " & "_" & _foreignkey.Column.Name.ToLower & Chr(13) & Chr(13) _
                                & "$BODY$" & Chr(13) & Chr(13) _
                                & "LANGUAGE SQL;" & Chr(13) _
                                & "" & Chr(13) & Chr(13)
            storeglobal = storeglobal & store
        Next
        Return storeglobal
    End Function

    Public Shared Function ListAllStoreByField(ByVal name As String, ByVal Fiedlds As String) As String
        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, name)
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""
        Dim count As Integer = 0
        Dim fields_String As String() = Fiedlds.Split(",")
        Dim paramStore As String = ""
        Dim SelectClause As String = ""
        Dim libelleStored As String = ""

        For i As Integer = 0 To fields_String.Count - 1
            For Each _column In _table.ListofColumn
                If _column.Name = fields_String(i) Then
                    If (paramStore = "") Then
                        paramStore = "_" & _column.Name.ToLower & " " & _column.Type.PostgresName
                        SelectClause = """" & _column.Name & """ = " & "_" & _column.Name.ToLower
                        libelleStored = "_" & _column.Name
                    Else
                        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "_" & _column.Name.ToLower & " " & _column.Type.PostgresName
                        SelectClause &= Chr(13) & Chr(9) & "and " & """" & _column.Name & """ = " & "_" & _column.Name.ToLower
                        libelleStored &= "_" & _column.Name
                    End If
                    Exit For
                End If
            Next
        Next

        Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM """ & name & """" & Chr(13) _
                                & Chr(9) & "Where " & SelectClause
        Dim objectname As String = name.Substring(4, name.Length - 4)
        Dim store As String =
                            "CREATE OR REPLACE FUNCTION SP_ListAll_" & objectname & libelleStored & " " & Chr(13) & Chr(13) _
                             & Chr(9) & "(" & Chr(13) _
                             & Chr(9) & Chr(9) & paramStore & Chr(13) _
                             & Chr(9) & ")" & Chr(13) _
                             & "RETURNS SETOF """ & name & """ AS" & Chr(13) _
                             & "$BODY$" & Chr(13) _
                             & command & Chr(13) _
                             & "$BODY$" & Chr(13) & Chr(13) _
                             & "LANGUAGE SQL;" & Chr(13) _
                             & "" & Chr(13) & Chr(13)
        Return store
    End Function

#End Region


#Region "VB.Net Class Fonctions"
    Public Shared Sub CreateFile(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\VbNetClass\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\VbNetClass\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".vb"

        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomClasse & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")
        header &= ""
        Dim content As String = "Public Class " & nomClasse & Chr(13) _
                                 & "Implements IGeneral"

        _end = "End Class" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()



        Dim objWriter As New System.IO.StreamWriter(path, True)
        '  objWriter.WriteLine(header)
        If ListBox_NameSpace.Items.Count > 0 Then
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                objWriter.WriteLine(ListBox_NameSpace.Items(i))
            Next
        End If
        objWriter.WriteLine()
        objWriter.WriteLine(content)
        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}

        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")
        objWriter.WriteLine("#Region ""Attribut""")
        objWriter.WriteLine("Private _id As Long")
        objWriter.WriteLine()
        Try
            For i As Int32 = 1 To cols.Count - 1
                If Not nottoputforlist.Contains(cols(i)) Then
                    If initialtypes(i).ToString() = "image" Then
                        insertstring &= ", " & "IIf(" & cols(i) & " Is Nothing, DBNull.Value, " & cols(i) & ")"
                    Else
                        insertstring &= ", " & cols(i)
                        updatestring &= ", " & cols(i)
                    End If
                End If
                objWriter.WriteLine("Private " & cols(i) & " As " & types(i))
                If ListofForeignKey.Contains(cols(i)) Then

                    Dim valueColumn As New Cls_Column(_table.ID, cols(i).ToString.Substring(1, cols(i).Length - 1))
                    Dim foreingn As New Cls_ForeignKey(_table.ID, valueColumn.ID)

                    Dim ClassName As String = "Cls_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1))
                    ClassName = foreingn.RefTable.Name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")

                    objWriter.WriteLine("Private _" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " As " & ClassName & "")
                End If
            Next
        Catch ex As Exception

        End Try
        objWriter.WriteLine()
        objWriter.WriteLine("#End Region")
        objWriter.WriteLine()

        objWriter.WriteLine("#Region ""New""")
        objWriter.WriteLine("Public Sub New()")
        objWriter.WriteLine("BlankProperties()")
        objWriter.WriteLine("End Sub")
        objWriter.WriteLine()
        objWriter.WriteLine("Public Sub New(ByVal _idOne As Long)")
        objWriter.WriteLine("Read(_idOne)")
        objWriter.WriteLine("End Sub" & Chr(13))


        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            If _index.ListofColumn.Count > 1 Then
            Else
                objWriter.WriteLine("Public Sub New(ByVal " & _index.ListofColumn.Item(0).Name & " As " & _index.ListofColumn.Item(0).Type.VbName & ")")
                objWriter.WriteLine("Read_" & _index.ListofColumn.Item(0).Name & "(" & _index.ListofColumn.Item(0).Name & ")")
                objWriter.WriteLine("End Sub " & Chr(13))
            End If
        Next


        objWriter.WriteLine("#End Region")

        objWriter.WriteLine()

        objWriter.WriteLine("#Region ""Properties""")
        objWriter.WriteLine("<AttributLogData(True,1)> _")
        objWriter.WriteLine("Public ReadOnly Property ID() As Long Implements IGeneral.ID")
        objWriter.WriteLine("Get")
        objWriter.WriteLine("Return _id")
        objWriter.WriteLine("End Get")
        objWriter.WriteLine("End Property")
        objWriter.WriteLine()

        For i As Int32 = 1 To cols.Count - 3 ''On ne cree pas de property pour la derniere column
            Dim propName As String = ""
            Dim s As String() = cols(i).Split("_")
            For j As Integer = 1 To s.Length - 1
                propName &= StrConv(s(j), VbStrConv.ProperCase)
            Next
            propName = cols(i).Substring(1, cols(i).Length - 1)
            'propName = StrConv(cols(i).Split("_")(1), VbStrConv.ProperCase) & StrConv(cols(i).Split("_")(2), VbStrConv.ProperCase)
            Dim log As String = "<AttributLogData(True, " & i + 1 & ")> _"
            Dim attrib As String = "Public Property  " & propName & " As " & types(i)

            If cols(i) <> "_isdirty" Or cols(i) <> "_LogData" Then
                objWriter.WriteLine(log)
                objWriter.WriteLine(attrib)
                objWriter.WriteLine("Get" & Chr(13) _
                                    & " Return " & cols(i) & Chr(13) _
                                    & "End Get")

                If Lcasevalue.Contains(types(i)) Then
                    objWriter.WriteLine("Set(ByVal Value As " & types(i) & ")" & Chr(13) _
                                  & " If LCase(Trim(" & cols(i) & ")) <> LCase(Trim(Value)) Then" & Chr(13) _
                                  & "_isdirty = True " & Chr(13) _
                                  & cols(i) & " = Trim(Value)" & Chr(13) _
                                  & "End If" & Chr(13) _
                                  & "End Set" & Chr(13) _
                                  & "End Property")
                Else
                    objWriter.WriteLine("Set(ByVal Value As " & types(i) & ")" & Chr(13) _
                                  & " If " & cols(i) & " <> Value Then" & Chr(13) _
                                  & "_isdirty = True " & Chr(13) _
                                  & cols(i) & " = Value" & Chr(13) _
                                  & "End If" & Chr(13) _
                                  & "End Set" & Chr(13) _
                                  & "End Property")
                End If

                objWriter.WriteLine()

                If ListofForeignKey.Contains(cols(i)) Then
                    Dim attributUsed As String = cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1))
                    Dim valueColumn As New Cls_Column(_table.ID, cols(i).ToString.Substring(1, cols(i).Length - 1))
                    Dim foreingn As New Cls_ForeignKey(_table.ID, valueColumn.ID)

                    Dim ClassName As String = "Cls_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1))
                    ClassName = foreingn.RefTable.Name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")

                    Dim simplename As String = cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1))
                    objWriter.WriteLine("Public Property " & attributUsed & " As " & ClassName & Chr(13))
                    objWriter.WriteLine("Get")
                    objWriter.WriteLine("If Not (_" & attributUsed & " Is Nothing) Then" & Chr(13) _
                                        & "If (_" & attributUsed & ".ID = 0) Or (_" & attributUsed & ".ID <>  " & cols(i) & ") Then" & Chr(13) _
                                        & "_" & attributUsed & "= New " & ClassName & "(" & cols(i) & ")" & Chr(13) _
                                        & "End If" & Chr(13) _
                                        & "Else" & Chr(13) _
                                        & "_" & attributUsed & "= New " & ClassName & "(" & cols(i) & ")" & Chr(13) _
                                        & "End If" & Chr(13) & Chr(13) _
                                        & "Return _" & attributUsed & Chr(13) _
                                        & "End Get" & Chr(13) _
                                        & "Set(ByVal value As " & ClassName & ")" & Chr(13) _
                                        & "If Value Is Nothing Then" & Chr(13) _
                                        & "_isdirty = True" & Chr(13) _
                                        & cols(i) & " = 0" & Chr(13) _
                                        & "Else" & Chr(13) _
                                        & "If _" & attributUsed & ".ID <> Value.ID Then" & Chr(13) _
                                        & "_isdirty = True" & Chr(13) _
                                        & cols(i) & " = Value.ID" & Chr(13) _
                                        & "End If" & Chr(13) _
                                        & "End If" & Chr(13) _
                                        & "End Set" & Chr(13) _
                                        & "End Property" & Chr(13) _
                                        )
                    objWriter.WriteLine()
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, cols(i).Substring(1, cols(i).Length - 1))
                    Dim textForcombo As String = ""

                    Dim compteur As Integer = 0

                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            objWriter.WriteLine("Public ReadOnly Property " & simplename & "_" & column_fore.Name & "  As String ")
                            objWriter.WriteLine("Get")
                            objWriter.WriteLine("Return " & simplename & "." & column_fore.Name & "")
                            objWriter.WriteLine("End Get")
                            objWriter.WriteLine("End Property")
                            objWriter.WriteLine()
                        End If
                        If compteur = foreignkey.RefTable.ListofColumn.Count - 5 Then
                            Exit For
                        End If
                        compteur = compteur + 1
                    Next
                End If
            End If
            If initialtypes(i).ToString() = "image" Then
                objWriter.WriteLine("Public Property " & cols(i).Substring(1, cols(i).Length - 1) & "String() As String")
                objWriter.WriteLine("Get")
                objWriter.WriteLine("If " & cols(i) & " IsNot Nothing Then")
                objWriter.WriteLine("Return Encode(" & cols(i) & " )")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("Return """"")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End Get")
                objWriter.WriteLine("Set(ByVal Value As String)")
                objWriter.WriteLine(cols(i) & " = Decode(Value)")
                objWriter.WriteLine("_isdirty = True")
                objWriter.WriteLine("End Set")
                objWriter.WriteLine("End Property")
            End If

        Next

        With objWriter
            .WriteLine("ReadOnly Property IsDataDirty() As Boolean")
            .WriteLine("Get")
            .WriteLine("Return _isdirty")
            .WriteLine("End Get")
            .WriteLine("End Property")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Public ReadOnly Property LogData() As String")
            .WriteLine("Get")
            .WriteLine("Return _LogData")
            .WriteLine("End Get")
            .WriteLine("End Property")
        End With


        objWriter.WriteLine("#End Region")

        objWriter.WriteLine()
        objWriter.WriteLine("#Region "" Db Access """)
        objWriter.WriteLine("Public Function Insert(ByVal usr As String) As Integer Implements IGeneral.Insert")
        objWriter.WriteLine("_LogData = """"")


        objWriter.WriteLine("_id = Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelperParameterCache.BuildConfigDB(), ""SP_Insert" & nomClasse.Substring(4, nomClasse.Length - 4) & """" & insertstring & ", usr))")


        objWriter.WriteLine("Return _id")
        objWriter.WriteLine("End Function")

        objWriter.WriteLine()
        objWriter.WriteLine("Public Function Update(ByVal usr As String) As Integer Implements IGeneral.Update")
        objWriter.WriteLine("_LogData = """"")
        objWriter.WriteLine("_LogData = GetObjectString()")
        objWriter.WriteLine("Return SqlHelper.ExecuteScalar(SqlHelperParameterCache.BuildConfigDB(), ""SP_Update" & nomClasse.Substring(4, nomClasse.Length - 4) & """, _id" & updatestring & ", usr)")
        objWriter.WriteLine("End Function" & Chr(13))

        objWriter.WriteLine("Private Sub SetProperties(ByVal dr As DataRow)" & Chr(13))
        objWriter.WriteLine("_id = TypeSafeConversion.NullSafeLong(dr(""" & cols(0).Substring(1, cols(0).Length - 1) & """))")

        For i As Int32 = 1 To cols.Count - 2

            If cols(i) <> "_isdirty" Then
                If types(i) = "DateTime" Then
                    objWriter.WriteLine(cols(i) & " = " & "TypeSafeConversion.NullSafeDate(dr(""" & cols(i).Substring(1, cols(i).Length - 1) & """))")
                ElseIf initialtypes(i) = "image" Then
                    objWriter.WriteLine("If" & "dr(""" & cols(i).Substring(1, cols(i).Length - 1) & """)" & "IsNot DBNull.Value Then")
                    objWriter.WriteLine(cols(i) & " = " & "dr(""" & cols(i).Substring(1, cols(i).Length - 1) & """)")
                    objWriter.WriteLine("Else")
                    objWriter.WriteLine("" & cols(i) & " =  " & "Nothing")
                    objWriter.WriteLine("End If")
                Else
                    objWriter.WriteLine(cols(i) & " = " & "TypeSafeConversion.NullSafe" & types(i) & "(dr(""" & cols(i).Substring(1, cols(i).Length - 1) & """))")
                End If
            End If

            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " = Nothing")

            End If

        Next

        objWriter.WriteLine()
        objWriter.WriteLine("End Sub" & Chr(13))

        objWriter.WriteLine("Private Sub BlankProperties()" & Chr(13))
        objWriter.WriteLine("_id = 0")
        For i As Int32 = 1 To cols.Count - 2

            If types(i) <> "Boolean" Then
                If types(i) = "DateTime" Or types(i) = "Date" Then
                    objWriter.WriteLine(cols(i) & " = " & "Now")
                Else
                    If Lcasevalue.Contains(types(i)) Then
                        objWriter.WriteLine(cols(i) & " = " & """""")
                    ElseIf initialtypes(i) = "image" Then
                        objWriter.WriteLine(cols(i) & " = Nothing")
                    Else
                        objWriter.WriteLine(cols(i) & " = " & "0")
                    End If

                End If
            Else
                objWriter.WriteLine(cols(i) & " = " & "False")
            End If

            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " = Nothing")
            End If


        Next

        objWriter.WriteLine()
        objWriter.WriteLine("End Sub" & Chr(13))

        objWriter.WriteLine("Public Function Read(ByVal _idpass As Long) As Boolean Implements IGeneral.Read")
        objWriter.WriteLine("Try " & Chr(13))

        objWriter.WriteLine("If _idpass <> 0 Then " & Chr(13) _
                        & "Dim ds As DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(),""SP_Select" & nomClasse.Substring(4, nomClasse.Length - 4) & "_ByID"", _idpass)" & Chr(13) & Chr(13) _
                        & "If ds.Tables(0).Rows.Count < 1 Then" & Chr(13) _
                        & "BlankProperties()" & Chr(13) _
                        & "Return False" & Chr(13) _
                        & "End If" & Chr(13) & Chr(13) _
                        & "SetProperties(ds.tables(0).rows(0))" & Chr(13) _
                        & "Else" & Chr(13) _
                        & "BlankProperties()" & Chr(13) _
                        & "End If" & Chr(13) _
                        & "Return True" & Chr(13) _
                        )

        objWriter.WriteLine("Catch ex As Exception" & Chr(13))
        objWriter.WriteLine("Throw ex" & Chr(13))
        objWriter.WriteLine("End Try" & Chr(13))
        objWriter.WriteLine("End Function" & Chr(13))

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            If _index.ListofColumn.Count = 1 Then
                objWriter.WriteLine("Public Function Read_" & _index.ListofColumn.Item(0).Name & "(ByVal " & _index.ListofColumn.Item(0).Name & " As " & _index.ListofColumn.Item(0).Type.VbName & ") As Boolean")
                objWriter.WriteLine("Try " & Chr(13))
                objWriter.WriteLine("If " & _index.ListofColumn.Item(0).Name & " <> """" Then ")
                objWriter.WriteLine("Dim ds as Dataset = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_Select" & nomClasse.Substring(4, nomClasse.Length - 4) & """, " & _index.ListofColumn.Item(0).Name & ")" & Chr(13))
                objWriter.WriteLine("If ds.tables(0).Rows.Count < 1 Then")
                objWriter.WriteLine("BlankProperties()")
                objWriter.WriteLine("Return False")
                objWriter.WriteLine("End If" & Chr(13))
                objWriter.WriteLine("SetProperties(ds.Tables(0).Rows(0))")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("BlankProperties()")
                objWriter.WriteLine("End If" & Chr(13))
                objWriter.WriteLine("Return True")
                objWriter.WriteLine("Catch ex As Exception" & Chr(13))
                objWriter.WriteLine("Throw ex" & Chr(13))
                objWriter.WriteLine("End Try" & Chr(13))
                objWriter.WriteLine("End Function" & Chr(13) & Chr(13))
            Else
                Dim _strOfIndexToUse As String = String.Empty
                Dim _strOfValueToUse As String = String.Empty
                Dim _strParameterToUse As String = String.Empty
                Dim ind As Integer = 0
                For Each _column As Cls_Column In _index.ListofColumn
                    If _strOfIndexToUse.Length = 0 Then
                        _strOfIndexToUse = _column.Name
                        _strOfValueToUse = "ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse = "_value" & ind
                    Else
                        _strOfIndexToUse += "_" & _column.Name
                        _strOfValueToUse += ", ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse += ", _value" & ind
                    End If
                    ind = ind + 1
                Next
                objWriter.WriteLine("Public Function Read_" & _strOfIndexToUse & "(" & _strOfValueToUse & ") As Boolean")
                objWriter.WriteLine("Try " & Chr(13))
                '  objWriter.WriteLine("If " & ListofIndex(i) & " <> """" Then ")
                objWriter.WriteLine("Dim ds As Data.DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_Select" & nomClasse.Substring(4, nomClasse.Length - 4) & "_" & _strOfIndexToUse & """, " & _strParameterToUse & ")")
                objWriter.WriteLine("If ds.tables(0).Rows.Count < 1 Then")
                objWriter.WriteLine("BlankProperties()")
                objWriter.WriteLine("Return False")
                objWriter.WriteLine("End If" & Chr(13))

                objWriter.WriteLine("SetProperties(ds.Tables(0).Rows(0))")
                'objWriter.WriteLine("Else")
                'objWriter.WriteLine("BlankProperties()")
                '  objWriter.WriteLine("End If" & Chr(13))

                objWriter.WriteLine("Return True")

                objWriter.WriteLine("Catch ex As Exception" & Chr(13))
                objWriter.WriteLine("Throw ex" & Chr(13))
                objWriter.WriteLine("End Try" & Chr(13))
                objWriter.WriteLine("End Function" & Chr(13) & Chr(13))
            End If
        Next

        objWriter.WriteLine("Public Sub Delete() Implements IGeneral.Delete" & Chr(13) _
                            & "Try" & Chr(13) _
                            & "SqlHelper.ExecuteNonQuery(SqlHelperParameterCache.BuildConfigDB(), ""SP_Delete" & nomClasse.Substring(4, nomClasse.Length - 4) & """, _id)" & Chr(13) & Chr(13) _
                            & "Catch ex As SqlClient.SqlException" & Chr(13) _
                            & "Throw New System.Exception(ex.ErrorCode)" & Chr(13) _
                            & "End Try" & Chr(13) _
                            & "End Sub" & Chr(13)
                            )

        objWriter.WriteLine("Public Function Refresh() As Boolean Implements IGeneral.Refresh" & Chr(13) _
                             & "If _id = 0 Then" & Chr(13) _
                             & "Return False" & Chr(13) _
                             & "Else" & Chr(13) _
                             & "Read(_id)" & Chr(13) _
                             & "Return True" & Chr(13) _
                             & "End If" & Chr(13) _
                             & "End Function" & Chr(13) _
                             )

        objWriter.WriteLine("Public Function Save(ByVal usr As String) As Integer Implements IGeneral.Save" & Chr(13) _
                             & "If _isdirty Then" & Chr(13) _
                             & "Validation()" & Chr(13) & Chr(13) _
                             & "If _id = 0 Then" & Chr(13) _
                             & "Return Insert(usr)" & Chr(13) _
                             & "Else" & Chr(13) _
                             & "If _id > 0 Then" & Chr(13) _
                             & "Return Update(usr)" & Chr(13) _
                             & "Else" & Chr(13) _
                             & "_id = 0" & Chr(13) _
                             & "Return False" & Chr(13) _
                             & "End If" & Chr(13) _
                             & "End If" & Chr(13) _
                             & "End If" & Chr(13) & Chr(13) _
                             & "_isdirty = False" & Chr(13) _
                             & "End Function"
                             )


        objWriter.WriteLine("#End Region")

        objWriter.WriteLine()
        objWriter.WriteLine("#Region "" Search """)

        objWriter.WriteLine("Public Function Search() As System.Collections.ICollection Implements IGeneral.Search" & Chr(13) _
                            & "Return SearchAll()" & Chr(13) _
                            & "End Function" & Chr(13)
                            )


        objWriter.WriteLine("Public Shared Function SearchAll() As List(Of " & nomClasse & ")" & Chr(13) _
                            & "Try " & Chr(13) _
                            & "Dim objs As New List(Of " & nomClasse & ")" & Chr(13) _
                            & "Dim r As Data.DataRow" & Chr(13) _
                            & "Dim ds As Data.DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_ListAll_" & nomClasse.Substring(4, nomClasse.Length - 4) & """)" & Chr(13) _
                            & "For Each r In ds.Tables(0).Rows" & Chr(13) _
                            & "Dim obj As New " & nomClasse & Chr(13) & Chr(13) _
                            & "obj.SetProperties(r)" & Chr(13) & Chr(13) _
                            & "objs.Add(obj)" & Chr(13) _
                            & "Next r" & Chr(13) _
                            & "Return objs"
                            )
        objWriter.WriteLine("Catch ex As Exception" & Chr(13))
        objWriter.WriteLine("Throw ex")
        objWriter.WriteLine("End Try")
        objWriter.WriteLine("End Function" & Chr(13))


        objWriter.WriteLine()

        For i As Int32 = 1 To cols.Count - 2
            If ListofForeignKey.Contains(cols(i)) Then
                Dim searchtext = cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1))

                objWriter.WriteLine("Public Shared Function SearchAllBy" & searchtext & "(Byval " & cols(i).ToString.ToLower & " As " & types(i) & ") As List(Of " & nomClasse & ")" & Chr(13) _
                                    & "Try " & Chr(13) _
                                    & "Dim objs As New List(Of " & nomClasse & ")" & Chr(13) _
                                    & "Dim r As Data.DataRow" & Chr(13) _
                                    & "Dim ds As Data.DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_ListAll_" & nomClasse.Substring(4, nomClasse.Length - 4) & cols(i) & """," & cols(i).ToString.ToLower & ")" & Chr(13) _
                                    & "For Each r In ds.Tables(0).Rows" & Chr(13) _
                                    & "Dim obj As New " & nomClasse & Chr(13) & Chr(13) _
                                    & "obj.SetProperties(r)" & Chr(13) & Chr(13) _
                                    & "objs.Add(obj)" & Chr(13) _
                                    & "Next r" & Chr(13) _
                                    & "Return objs"
                                    )
                objWriter.WriteLine("Catch ex As Exception" & Chr(13))
                objWriter.WriteLine("Throw ex")
                objWriter.WriteLine("End Try")
                objWriter.WriteLine("End Function" & Chr(13))


            End If
        Next
        objWriter.WriteLine()
        objWriter.WriteLine("#End Region")
        objWriter.WriteLine()
        objWriter.WriteLine("#Region "" Other Methods """)


        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            If _index.ListofColumn.Count = 1 Then
                objWriter.WriteLine("Private Function FoundAlreadyExist" & "_" & _index.ListofColumn.Item(0).Name & "(ByVal _value As " & _index.ListofColumn.Item(0).Type.VbName & ") As Boolean ")
                objWriter.WriteLine("Dim ds As Data.DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_Select" & nomClasse.Substring(4, nomClasse.Length - 4) & "_" & _index.ListofColumn.Item(0).Name & """, _value)")
                objWriter.WriteLine(" If ds.Tables(0).Rows.Count < 1 Then")
                objWriter.WriteLine(" Return False")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("If _id = 0 Then")
                objWriter.WriteLine("Return True")
                objWriter.WriteLine("Else")
                objWriter.WriteLine(" If ds.Tables(0).Rows(0).Item(""" & Id_table & """) <> _id Then")
                objWriter.WriteLine("Return True")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("Return False")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End Function" & Chr(13) & Chr(13))
            Else
                Dim _strOfIndexToUse As String = String.Empty
                Dim _strOfValueToUse As String = String.Empty
                Dim _strParameterToUse As String = String.Empty
                Dim ind As Integer = 0
                For Each _column In _index.ListofColumn
                    If _strOfIndexToUse.Length = 0 Then
                        _strOfIndexToUse = _column.Name
                        _strOfValueToUse = "ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse = "_value" & ind
                    Else
                        _strOfIndexToUse += "_" & _column.Name
                        _strOfValueToUse += ", ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse += ", _value" & ind
                    End If
                    ind = ind + 1
                Next
                objWriter.WriteLine("Private Function FoundAlreadyExist" & "_" & _strOfIndexToUse & "(" & _strOfValueToUse & ") As Boolean ")
                objWriter.WriteLine("Try" & Chr(13))
                objWriter.WriteLine("Dim ds As Data.DataSet = SqlHelper.ExecuteDataset(SqlHelperParameterCache.BuildConfigDB(), ""SP_Select" & nomClasse.Substring(4, nomClasse.Length - 4) & "_" & _strOfIndexToUse & """, " & _strParameterToUse & ")")

                objWriter.WriteLine(" If ds.Tables(0).Rows.Count < 1 Then")
                objWriter.WriteLine(" Return False")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("If _id = 0 Then")
                objWriter.WriteLine("Return True")
                objWriter.WriteLine("Else")
                objWriter.WriteLine(" If ds.Tables(0).Rows(0).Item(""" & Id_table & """) <> _id Then")
                objWriter.WriteLine("Return True")
                objWriter.WriteLine("Else")
                objWriter.WriteLine("Return False")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End If")
                objWriter.WriteLine("End If" & Chr(13))
                objWriter.WriteLine("Catch ex As Exception" & Chr(13))
                objWriter.WriteLine("Throw ex" & Chr(13))
                objWriter.WriteLine("End Try" & Chr(13))
                objWriter.WriteLine("End Function" & Chr(13) & Chr(13))
            End If
        Next


        objWriter.WriteLine("Private Sub Validation() " & Chr(13))
        Dim stringlistforvalidation As New List(Of String) From {"String"}
        Dim decimalintegerforvalidation As New List(Of String) From {"Integer", "Long", "Decimal"}



        For i As Int32 = 1 To cols.Count - 2
            If stringlistforvalidation.Contains(types(i)) Then
                objWriter.WriteLine("If " & cols(i) & " = """" Then " & Chr(13) _
                                    & "Throw (New System.Exception("" " & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & " obligatoire""))" & Chr(13) _
                                    & "End If"
                                    )
                objWriter.WriteLine()
                objWriter.WriteLine("If Len(" & cols(i) & ") > " & length(i) & " Then" & Chr(13) _
                                    & "Throw (New System.Exception("" " & "Trop de caractères insérés pour" & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & "  (la longueur doit être inférieure a " & length(i) & " caractères.  )""))" & Chr(13) _
                                    & "End If"
                                    )
                objWriter.WriteLine()
            End If

            If decimalintegerforvalidation.Contains(types(i)) Then
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("If " & cols(i) & " = 0 Then " & Chr(13) _
                                  & "Throw (New System.Exception("" " & cols(i).Substring(4, cols(i).Length - 4).ToString.ToLower & " obligatoire""))" & Chr(13) _
                                  & "End If"
                                  )
                    objWriter.WriteLine()
                Else
                    objWriter.WriteLine("If " & cols(i) & " = 0 Then " & Chr(13) _
                                   & "Throw (New System.Exception("" " & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & " obligatoire.""))" & Chr(13) _
                                   & "End If"
                                   )
                    objWriter.WriteLine()
                End If
            End If


        Next

        objWriter.WriteLine()

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            If _index.ListofColumn.Count = 1 Then
                objWriter.WriteLine("If  FoundAlreadyExist" & "_" & _index.ListofColumn.Item(0).Name & "(" & "_" & _index.ListofColumn.Item(0).Name & ") Then" & Chr(13) _
                                  & "Throw (New System.Exception(""Ce " & _index.ListofColumn.Item(0).Name & " est déjà enregistré.""))" & Chr(13) _
                                  & "End If"
                                 )
                objWriter.WriteLine()
            Else
                Dim _strOfIndexToUse As String = String.Empty
                Dim _strOfValueToUse As String = String.Empty
                Dim _strParameterToUse As String = String.Empty
                Dim _parametervalueToUse As String = String.Empty
                Dim ind As Integer = 0
                For Each _column In _index.ListofColumn
                    If _strOfIndexToUse.Length = 0 Then
                        _strOfIndexToUse = _column.Name
                        _parametervalueToUse = _column.Name
                        _strOfValueToUse = "ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse = "_value" & ind
                    Else
                        _strOfIndexToUse += "_" & _column.Name
                        _parametervalueToUse += ", " & _column.Name
                        _strOfValueToUse += ", ByVal _value" & ind & " As " & _column.Type.VbName
                        _strParameterToUse += ", value" & ind
                    End If
                Next

                objWriter.WriteLine("If FoundAlreadyExist_" & _strOfIndexToUse & "(" & _parametervalueToUse & ") Then" & Chr(13) _
                                   & "Throw (New System.Exception(""Cette combinaison " & _strOfIndexToUse & " est déjà enregistrée.""))" & Chr(13) _
                                   & "End If"
                                   )
                objWriter.WriteLine()
            End If
        Next

        objWriter.WriteLine("End Sub" & Chr(13))

        objWriter.WriteLine("Public Function Encode(ByVal str As Byte()) As String")
        objWriter.WriteLine("Return Convert.ToBase64String(str)")
        objWriter.WriteLine("End Function")
        objWriter.WriteLine()

        objWriter.WriteLine("Public Function Decode(ByVal str As String) As Byte()")
        objWriter.WriteLine("Dim decbuff As Byte() = Convert.FromBase64String(str)")
        objWriter.WriteLine("Return decbuff")
        objWriter.WriteLine("End Function")
        objWriter.WriteLine()

        objWriter.WriteLine(" Public Function GetObjectString() As String Implements IGeneral.GetObjectString" & Chr(13) _
                         & "Dim _old As New " & nomClasse & "(Me.ID)" & Chr(13) & Chr(13) _
                         & "Return LogStringBuilder.BuildLogStringChangesOnly(_old,Me)" & Chr(13) _
                         & "End Function" & Chr(13) _
                         )
        objWriter.WriteLine("#End Region")
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub
#End Region

#Region "Asp.Net Vb.net Interface Fonctions"

    Public Shared Sub CreateInterfaceCodeBehind(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_LibraryName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomWebform & ".aspx.vb"

        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")
        header &= ""
        Dim content As String = "Partial Class " & nomWebform & Chr(13) _
                                 & " Inherits BasePage"

        _end = "End Class" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()



        Dim objWriter As New System.IO.StreamWriter(path, True)
        'objWriter.WriteLine(header)
        If ListBox_NameSpace.Items.Count > 0 Then
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                objWriter.WriteLine(ListBox_NameSpace.Items(i))
            Next
        End If
        Dim libraryname As String = "Imports " & txt_LibraryName.Text
        objWriter.WriteLine(libraryname)
        'objWriter.WriteLine("Imports System.Data")
        'objWriter.WriteLine("Imports SolutionsHT")
        'objWriter.WriteLine("Imports SolutionsHT.DataAccessLayer")
        objWriter.WriteLine()

        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next


        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")


        objWriter.WriteLine("Dim _out As Boolean = False")
        objWriter.WriteLine("Dim _tmpEditState As Boolean = False")

        objWriter.WriteLine()

        objWriter.WriteLine(" Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load")
        objWriter.WriteLine("If txt_Code" & nomSimple & "_Hid.Text <> ""0"" Then " & Chr(13) _
                            & Chr(9) & Chr(9) & "_tmpEditState = True" & Chr(13) _
                            & "Else" & Chr(13) _
                            & Chr(9) & Chr(9) & "_tmpEditState = False" & Chr(13) _
                            & "End If")

        objWriter.WriteLine("   If _message = """" Then" & Chr(13) _
                          & "  lbl_Error.Text = """" " & Chr(13) _
                          & Chr(13) _
                          & "If Not IsPostBack Then")

        objWriter.WriteLine(" txt_Code" & nomSimple & "_Hid.Text = CStr(SolutionsHT.TypeSafeConversion.NullSafeLong(Request.QueryString(""id""), 0)) ")
        objWriter.WriteLine("If txt_Code" & nomSimple & "_Hid.Text <> ""0"" Then " & Chr(13) _
                          & Chr(9) & Chr(9) & "_tmpEditState = True" & Chr(13) _
                          & "Else" & Chr(13) _
                          & Chr(9) & Chr(9) & "_tmpEditState = False" & Chr(13) _
                          & "End If")
        objWriter.WriteLine("  btnCancel.Attributes.Add(""onclick"", ""javascript:void(closeWindow());"")" & Chr(13) _
                            & "Setup()" & Chr(13) _
                            & "End If")

        objWriter.WriteLine(" Else " & Chr(13) _
                            & "MessageToShow(_message)" & Chr(13) _
                            & "pn_Infos" & nomSimple & ".Enabled = False" & Chr(13) _
                            & "End If")
        objWriter.WriteLine("End Sub")

        objWriter.WriteLine()

        objWriter.WriteLine(" Private Sub EnableEditInfos" & nomSimple & "(ByVal modify As Boolean)")

        objWriter.WriteLine("If modify Then " & Chr(13) _
                            & "btnSave.Text = ""Sauvegarder""" & Chr(13) _
                            & "Else" & Chr(13) _
                            & "btnSave.Text = ""Editer""" & Chr(13) _
                            & "End If" & Chr(13) _
                            & "btnSave.CausesValidation = modify")

        For i As Int32 = 1 To cols.Count - 3
            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("rcmb_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " .Enabled =   modify ")
            ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                objWriter.WriteLine("rdp" & cols(i) & ".Enabled =   modify ")
            ElseIf types(i) = "Boolean" And cols(i) <> "isdirty" Then
                'objWriter.WriteLine(" radio_yes" & cols(i) & ".Enabled =   modify ")
                'objWriter.WriteLine("radio_no" & cols(i) & ".Enabled =   modify ")
                objWriter.WriteLine("chk" & cols(i) & ".Enabled =   modify ")
            Else
                objWriter.WriteLine("rtxt" & cols(i) & ".Enabled =   modify ")
            End If
        Next

        objWriter.WriteLine("End Sub")

        objWriter.WriteLine()

        objWriter.WriteLine("Private Sub Setup()")


        For Each foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            Dim attributUsed As String = foreignkey.Column.Name.Substring(ForeinKeyPrefix.Length, foreignkey.Column.Name.Length - (ForeinKeyPrefix.Length))
            Dim ClassName As String = "Cls_" & foreignkey.Column.Name.Substring(ForeinKeyPrefix.Length, foreignkey.Column.Name.Length - (ForeinKeyPrefix.Length))
            objWriter.WriteLine("FillCombo" & attributUsed & "()")
        Next

        objWriter.WriteLine("  If _tmpEditState Then" & Chr(13) _
                            & "Dim obj as New " & nomClasse & "( CLng(txt_Code" & nomSimple & "_Hid.Text))" & Chr(13) _
                            & "With obj")

        For i As Int32 = 1 To cols.Count - 3
            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("rcmb_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " .SelectedIndex =  rcmb_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & ".FindItemIndexByValue(." & cols(i).Substring(1, cols(i).Length - 1) & ")")
            ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                objWriter.WriteLine("rdp" & cols(i) & ".SelectedDate = ." & cols(i).Substring(1, cols(i).Length - 1))
            ElseIf types(i) = "Boolean" Then
                'objWriter.WriteLine("  If ." & cols(i).Substring(1, cols(i).Length - 1) & " = True Then")
                'objWriter.WriteLine(" radio_yes" & cols(i) & ".Checked = True ")
                'objWriter.WriteLine("Else")
                'objWriter.WriteLine("radio_no" & cols(i) & ".Checked = True")
                'objWriter.WriteLine("End If")
                objWriter.WriteLine("chk" & cols(i) & ".Checked = ." & cols(i).Substring(1, cols(i).Length - 1))
            Else
                objWriter.WriteLine("rtxt" & cols(i) & ".Text = ." & cols(i).Substring(1, cols(i).Length - 1))
            End If
        Next
        objWriter.WriteLine("End With")
        objWriter.WriteLine("Else")
        For i As Int32 = 1 To cols.Count - 3
            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("rcmb_" & cols(i).Substring(ForeinKeyPrefix.Length + 1, cols(i).Length - (ForeinKeyPrefix.Length + 1)) & " .SelectedIndex = 0")
            ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                objWriter.WriteLine("rdp" & cols(i) & ".SelectedDate = Now")
            ElseIf (types(i) = "Boolean") Then
                '  objWriter.WriteLine("radio_no" & cols(i) & ".Checked = True")
                'objWriter.WriteLine("radio_no" & cols(i) & ".Checked = False")
                objWriter.WriteLine("chk" & cols(i) & ".Checked = False")
            Else
                objWriter.WriteLine("rtxt" & cols(i) & ".Text = """"")
            End If
        Next
        objWriter.WriteLine("End If")
        objWriter.WriteLine("End Sub")
        objWriter.WriteLine()
        objWriter.WriteLine("Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click" & Chr(13) _
                            & "Select Case btnSave.Text" & Chr(13) _
                            & " Case ""Editer""" & Chr(13) _
                            & "EnableEditInfos" & nomSimple & "(True)" & Chr(13) _
                            & "Case ""Sauvegarder""" & Chr(13) _
        & "Dim _id As Long = CLng(txt_Code" & nomSimple & "_Hid.Text)" & Chr(13) _
        & "Dim _obj As New " & nomClasse & "(_id)" & Chr(13) _
        & "Try" & Chr(13) _
        & "With _obj")

        For i As Int32 = 1 To cols.Count - 3
            Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
            If ListofForeignKey.Contains(cols(i)) Then
                objWriter.WriteLine("." & columnToUse & "  =   rcmb_" & columnToUse.Substring(ForeinKeyPrefix.Length, columnToUse.Length - (ForeinKeyPrefix.Length)) & " .SelectedValue ")
            ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                objWriter.WriteLine("." & columnToUse & " = rdp_" & columnToUse & " .SelectedDate ")
            ElseIf types(i) = "Boolean" Then
                ' objWriter.WriteLine("." & columnToUse & " =  radio_yes_" & columnToUse & " .Checked")
                objWriter.WriteLine("." & columnToUse & " =  chk_" & columnToUse & " .Checked")
            Else
                objWriter.WriteLine("." & columnToUse & " = rtxt_" & columnToUse & ".Text ")
            End If
        Next

        objWriter.WriteLine("End With")

        objWriter.WriteLine("_obj.Save(""Admin"")")

        objWriter.WriteLine("_message = ""Sauvegarde Effectuée""")
        objWriter.WriteLine("MessageToShow(_message, ""S"")")
        objWriter.WriteLine("RadAjaxManager1.ResponseScripts.Add(""CloseAndRefreshListe" & nomSimple & "();"")")
        objWriter.WriteLine(" Catch ex As Exception")
        objWriter.WriteLine(" _message = ex.Message & Chr(13) & ""La sauvegarde a échouée!""")
        objWriter.WriteLine("MessageToShow(_message)")
        objWriter.WriteLine("End Try")
        objWriter.WriteLine("  End Select" & Chr(13) & _
                            "End Sub")
        objWriter.WriteLine()


        objWriter.WriteLine("Private Sub MessageToShow(ByVal _message As String, Optional ByVal E_or_S As String = ""E"")")
        objWriter.WriteLine("   Panel_Msg.Visible = True")
        objWriter.WriteLine(" GlobalFunctions.Message_Image(Image_Msg, E_or_S)")
        objWriter.WriteLine(" Label_Msg.Text = _message")
        objWriter.WriteLine(" If E_or_S = ""S"" Then")
        objWriter.WriteLine(" Label_Msg.ForeColor = Drawing.Color.Green")
        objWriter.WriteLine(" Else")
        objWriter.WriteLine(" Label_Msg.ForeColor = Drawing.Color.Red")
        objWriter.WriteLine(" End If")
        objWriter.WriteLine(" RadAjaxManager1.ResponseScripts.Add(""alert('"" & _message.Replace(""'"", ""\'"") & ""');"")")
        objWriter.WriteLine(" End Sub")
        objWriter.WriteLine()
        For Each foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            Dim textForcombo As String = ""
            Dim attributUsed As String = foreignkey.Column.Name.Substring(ForeinKeyPrefix.Length, foreignkey.Column.Name.Length - (ForeinKeyPrefix.Length))

            Dim ClassName As String = "Cls_" & foreignkey.Column.Name.Substring(ForeinKeyPrefix.Length, foreignkey.Column.Name.Length - (ForeinKeyPrefix.Length))
            ClassName = foreignkey.RefTable.Name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
            Dim nomforeign As String = foreignkey.Column.Name.Substring(ForeinKeyPrefix.Length, foreignkey.Column.Name.Length - (ForeinKeyPrefix.Length))
            objWriter.WriteLine("Private Sub FillCombo" & attributUsed & "()")
            objWriter.WriteLine("Try")
            objWriter.WriteLine("Dim objs1 As List(of " & ClassName & ") = " & ClassName & ".SearchAll ")

            For Each column As Cls_Column In foreignkey.RefTable.ListofColumn
                If column.Type.VbName = "String" Then
                    textForcombo = column.Name
                    Exit For
                End If
            Next

            objWriter.WriteLine("With rcmb_" & nomforeign)
            objWriter.WriteLine(".Datasource = objs1" & Chr(13) _
                                & ".DataValueField = ""ID""" & Chr(13) _
                                & ".DataTextField = """ & textForcombo & """" & Chr(13) _
                                & ".DataBind()" & Chr(13) _
                                & ".SelectedIndex = 0" & Chr(13) _
                                & "End With")
            objWriter.WriteLine("Catch ex As Exception" & Chr(13) _
                                & "_message = ex.Message" & Chr(13) _
                                & "MessageToShow(_message)")

            objWriter.WriteLine("End Try")
            objWriter.WriteLine("End Sub")
            objWriter.WriteLine()
        Next

        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateInterfaceCodeAsp(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".aspx"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)

        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine()

        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer
        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name
        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")

        objWriter.WriteLine("<%@ Page Title="""" Language=""VB"" MasterPageFile=""~/Pages/MasterPage/MasterPagePopup.master""" & _
       "AutoEventWireup=""false"" CodeFile=""wbfrm_" & nomSimple & ".aspx.vb"" Inherits=""wbfrm_" & nomSimple & """ %>")

        objWriter.WriteLine("<asp:Content ID=""Content3"" ContentPlaceHolderID=""SheetContentPlaceHolder"" runat=""Server"">")

        objWriter.WriteLine("<telerik:RadCodeBlock ID=""RadCodeBlock1"" runat=""server"">")
        objWriter.WriteLine("<script type=""text/javascript"">")
        objWriter.WriteLine("function closeWindow() {")
        objWriter.WriteLine(" GetRadWindow().BrowserWindow.refreshMe();")
        objWriter.WriteLine("  GetRadWindow().close();")
        objWriter.WriteLine("}")



        objWriter.WriteLine("function CloseAndRefreshListe" & nomSimple & "() {")
        objWriter.WriteLine(" GetRadWindow().BrowserWindow.refreshMe();")
        objWriter.WriteLine("  GetRadWindow().close();")
        objWriter.WriteLine(" }")

        objWriter.WriteLine(" function GetRadWindow() {")
        objWriter.WriteLine("  var oWindow = null;")
        objWriter.WriteLine("  if (window.radWindow) oWindow = window.radWindow; //Will work in Moz in all cases, including clasic dialog")
        objWriter.WriteLine("  else if (window.frameElement.radWindow) oWindow = window.frameElement.radWindow; //IE (and Moz as well)")

        objWriter.WriteLine("   return oWindow;")
        objWriter.WriteLine("  }")


        objWriter.WriteLine("</script>")
        objWriter.WriteLine("</telerik:RadCodeBlock>")

        objWriter.WriteLine(" <telerik:RadScriptManager ID=""RadScriptManager1""  runat=""server"">")
        objWriter.WriteLine("<Scripts>")
        objWriter.WriteLine("</Scripts>")
        objWriter.WriteLine("</telerik:RadScriptManager>")

        objWriter.WriteLine(" <telerik:RadAjaxManager ID=""RadAjaxManager1"" runat=""server"">")
        objWriter.WriteLine("<AjaxSettings>")
        objWriter.WriteLine("<telerik:AjaxSetting AjaxControlID=""btnSave"">")
        objWriter.WriteLine("<UpdatedControls>")
        objWriter.WriteLine(" <telerik:AjaxUpdatedControl ControlID=""Panel_Msg"" LoadingPanelID=""RadAjaxLoadingPanel1"" />")
        objWriter.WriteLine(" <telerik:AjaxUpdatedControl ControlID=""pn_Infos" & nomSimple & """ LoadingPanelID=""RadAjaxLoadingPanel1"" />")

        objWriter.WriteLine("</UpdatedControls>")
        objWriter.WriteLine("</telerik:AjaxSetting>")
        objWriter.WriteLine("</AjaxSettings>")
        objWriter.WriteLine("</telerik:RadAjaxManager>")

        objWriter.WriteLine("<telerik:RadSkinManager ID=""RadSkinManager1"" runat=""server"" Skin=""Windows7"">")
        objWriter.WriteLine("</telerik:RadSkinManager>")

        objWriter.WriteLine("<telerik:RadAjaxLoadingPanel ID=""RadAjaxLoadingPanel1"" runat=""server"" />")
        objWriter.WriteLine("<center>")
        objWriter.WriteLine(" <asp:Panel runat=""server"" ID=""pnCompletePage"">")
        objWriter.WriteLine(" <table id=""global"" width=""800px"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F7F7F7;  border: 3px solid #66A6CC"">")
        objWriter.WriteLine(" <tr valign=""top"">")
        objWriter.WriteLine(" <td valign=""top"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <table cellspacing=""0"" cellpadding=""0"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <tr valign=""top"" style=""background-color: #FFFFFF; height: 100px; width: 100%; border-bottom-color: #66A6CC;border-bottom-style : solid"">")
        objWriter.WriteLine(" <td align=""left"" style=""width: 10%"">")
        objWriter.WriteLine(" <asp:Image ID=""imgHeader"" runat=""server"" ImageUrl=""~/images/img_default.png"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine(" <td align=""left"" valign=""top"" style=""width: 90%"">")
        objWriter.WriteLine(" <h1 Class=""popupFormTitleLabel"" >")
        objWriter.WriteLine(" <asp:Label ID=""lbl_title"" CssClass=""popupFormTitleLabel""   Text=""Gestion des " & nomSimple & """   runat=""server""></asp:Label>")
        objWriter.WriteLine("</h1>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")

        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td style=""width: 100%;"" colspan=""2"" valign=""top"">")
        objWriter.WriteLine("<hr class=""hrHeaderAfterPhoto"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")

        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td style=""width: 100%;""  align=""center"" colspan=""2"" valign=""top"">")
        objWriter.WriteLine("<asp:Panel runat=""server"" ID=""Panel_Msg"" Visible=""false"">")
        objWriter.WriteLine("<div class=""Classe_MsgErreur"">")
        objWriter.WriteLine(" <asp:Image ID=""Image_Msg"" runat=""server"" Style=""vertical-align: middle;"" />")
        objWriter.WriteLine(" <asp:Label ID=""Label_Msg"" runat=""server"" Text=""""></asp:Label>")
        objWriter.WriteLine(" </div>")
        objWriter.WriteLine("</asp:Panel>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")

        objWriter.WriteLine(" <tr valign=""top"">")
        objWriter.WriteLine(" <td valign=""top"" align=""center"" style=""width: 100%;"" colspan=""2"">")
        objWriter.WriteLine(" <asp:Label ID=""lbl_Error"" runat=""server"" CssClass=""LabelBold"" Font-Bold=""True"" Font-Size=""Small"" ForeColor=""Red"" Font-Italic=""True""></asp:Label> ")


        objWriter.WriteLine(" </td>")
        objWriter.WriteLine("  </tr>")
        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td valign=""top"" style=""width: 100%;"" colspan=""2"">")
        objWriter.WriteLine(" <asp:Panel ID=""pn_Infos" & nomSimple & """ runat=""server"">")



        objWriter.WriteLine("  <asp:Panel ID=""pn_entete"" runat=""server"" Width=""100%"">")
        objWriter.WriteLine("<table border=""0"" class=""form_Enregistrement"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")

        Dim countColumn As Integer = 0
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then

                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<asp:Label ID=""lbl_" & columnName & """ CssClass=""popupFormInputLabel"" Text=""" & columnName & " : "" runat=""server""></asp:Label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadComboBox ID=""rcmb_" & columnName & """  Filter=""Contains""  AutoPostBack=""true""  Width=""50%"" runat=""server""> " & Chr(13) & " </telerik:RadComboBox> ")
                    objWriter.WriteLine(" <asp:RequiredFieldValidator CssClass=""error_color"" ID=""RequiredFieldValidator_" & columnName & """ runat=""server"" ControlToValidate=""rcmb_" & columnName & """  ")
                    objWriter.WriteLine(" InitialValue=""--Choisir un " & columnName & "--"" ErrorMessage=""Le " & columnName & " est obligatoire !"" ValidationGroup=""" & nomSimple & "Group"">*</asp:RequiredFieldValidator> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType <> "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<asp:Label ID=""lbl_" & column.Name & """ CssClass=""popupFormInputLabel"" Text=""" & column.Name & " : "" runat=""server""></asp:Label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadTextBox ID=""rtxt_" & column.Name & """ Width=""50%"" runat=""server""> " & Chr(13) & " </telerik:RadTextBox> ")
                    objWriter.WriteLine(" <asp:RequiredFieldValidator CssClass=""error_color"" ID=""RequiredFieldValidator_" & column.Name & """ runat=""server"" ControlToValidate=""rtxt_" & column.Name & """  ")
                    objWriter.WriteLine("  ErrorMessage=""Le " & column.Name & " est obligatoire !"" ValidationGroup=""" & nomSimple & "Group"">*</asp:RequiredFieldValidator> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf column.TrueSqlServerType = "date" And column.TrueSqlServerType = "datetime" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<asp:Label ID=""lbl_" & column.Name & """ CssClass=""popupFormInputLabel"" Text=""" & column.Name & " : "" runat=""server""></asp:Label> ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadDatePicker ID=""rdp_" & column.Name & """ Width=""50%"" runat=""server""" & _
                                     "DateInput-DateFormat=""dd/MM/yyyy"" MinDate=""1900-01-01""  ToolTip=""Cliquer sur le bouton pour choisir une date"" > " & Chr(13) & " </telerik:RadDatePicker>  ")
                    objWriter.WriteLine(" <asp:RequiredFieldValidator CssClass=""error_color"" ID=""RequiredFieldValidator_" & column.Name & """ runat=""server"" ControlToValidate=""rdp_" & column.Name & """  ")
                    objWriter.WriteLine("  ErrorMessage=""La " & column.Name & " est obligatoire !"" ValidationGroup=""" & nomSimple & "Group"">*</asp:RequiredFieldValidator> ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("</tr>")

                ElseIf column.TrueSqlServerType = "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<asp:Label ID=""lbl_" & column.Name & """ CssClass=""popupFormInputLabel"" Text=""" & column.Name & " : "" runat=""server""></asp:Label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<asp:CheckBox ID=""chk_" & column.Name & """ Width=""50%"" runat=""server"">" & _
                                      " </asp:CheckBox>  ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("</tr>")
                End If
            End If
            countColumn = countColumn + 1

        Next

        objWriter.WriteLine("<tr>")
        objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
        objWriter.WriteLine("<telerik:RadButton ID=""btnSave"" runat=""server"" Skin=""Windows7""  Text=""Sauvegarder"" ValidationGroup=""" & nomSimple & "Group""> ")
        objWriter.WriteLine("<Icon PrimaryIconCssClass=""rbSave"" PrimaryIconLeft=""4"" PrimaryIconTop=""4""></Icon>")
        objWriter.WriteLine("</telerik:RadButton>")
        objWriter.WriteLine("&nbsp;")
        objWriter.WriteLine("<telerik:RadButton ID=""btnCancel"" runat=""server"" CausesValidation=""False"" Skin=""Windows7""  Text = ""Abandonner"" >")
        objWriter.WriteLine("<Icon PrimaryIconCssClass=""rbCancel"" PrimaryIconLeft=""4"" PrimaryIconTop=""4""></Icon>")
        objWriter.WriteLine("</telerik:RadButton>")

        objWriter.WriteLine("</td>")

        objWriter.WriteLine("</tr>")
        objWriter.WriteLine(" </table>")
        objWriter.WriteLine("</asp:Panel>")
        objWriter.WriteLine(" </asp:Panel>")
        objWriter.WriteLine("  </td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")
        objWriter.WriteLine("</asp:Panel>")

        objWriter.WriteLine("<br />")
        objWriter.WriteLine("</center>")
        objWriter.WriteLine("<asp:TextBox ID=""txt_Code" & nomSimple & "_Hid"" runat=""server"" Text=""0"" Visible=""False"" Width=""1px""></asp:TextBox>")
        objWriter.WriteLine("<asp:ValidationSummary ID=""ValidationSummary1"" runat=""server"" ShowMessageBox=""true""" & _
        " ShowSummary=""false"" DisplayMode=""list"" ValidationGroup=""" & nomSimple & "Group"" />")
        objWriter.WriteLine("</asp:Content>")
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateListingCodeBehind(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_libraryname As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomWebform & "Listing.aspx.vb"

        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")
        header &= ""
        Dim content As String = "Partial Class " & nomWebform & "Listing" & Chr(13) _
                                 & " Inherits BasePage"

        _end = "End Class" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()



        Dim objWriter As New System.IO.StreamWriter(path, True)
        'objWriter.WriteLine(header)
        If ListBox_NameSpace.Items.Count > 0 Then
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                objWriter.WriteLine(ListBox_NameSpace.Items(i))
            Next
        End If
        Dim libraryname As String = "Imports " & txt_libraryname.Text
        objWriter.WriteLine("Imports Telerik.Web.UI")
        objWriter.WriteLine(libraryname)
        'objWriter.WriteLine("Imports System.Data")
        'objWriter.WriteLine("Imports SolutionsHT")
        'objWriter.WriteLine("Imports SolutionsHT.DataAccessLayer")
        objWriter.WriteLine()

        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next


        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")


        objWriter.WriteLine("Dim _out As Boolean = False")
        objWriter.WriteLine("Dim _tmpEditState As Boolean = False")

        objWriter.WriteLine()

        With objWriter
            .WriteLine(" Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load")


            .WriteLine("   If _message = """" Then" & Chr(13) _
                                & " ' lbl_Error.Text = """" " & Chr(13) _
                                & Chr(13) _
                                & "If Not IsPostBack Then")

            .WriteLine("  rbtnAdd" & nomSimple & ".Attributes.Add(""onclick"", ""javascript:void(ShowAddUpdateForm('../Parametres/wbfrm_" & nomSimple & ".aspx', 550, 285));"")" & Chr(13) _
                                & "BindGrid()" & Chr(13) _
                                & "End If")

            .WriteLine(" Else " & Chr(13) _
                                & "MessageToShow(_message)" & Chr(13) _
                                & "pn_List.Visible = False" & Chr(13) _
                                & "End If")
            .WriteLine("End Sub")

            .WriteLine()
        End With

        objWriter.WriteLine("Private Sub MessageToShow(ByVal _message As String, Optional ByVal E_or_S As String = ""E"")")
        objWriter.WriteLine("   Panel_Msg.Visible = True")
        objWriter.WriteLine(" GlobalFunctions.Message_Image(Image_Msg, E_or_S)")
        objWriter.WriteLine(" Label_Msg.Text = _message")
        objWriter.WriteLine(" If E_or_S = ""S"" Then")
        objWriter.WriteLine(" Label_Msg.ForeColor = Drawing.Color.Green")
        objWriter.WriteLine(" Else")
        objWriter.WriteLine(" Label_Msg.ForeColor = Drawing.Color.Red")
        objWriter.WriteLine(" End If")
        objWriter.WriteLine(" RadAjaxManager1.ResponseScripts.Add(""alert('"" & _message.Replace(""'"", ""\'"") & ""');"")")
        objWriter.WriteLine(" End Sub")
        objWriter.WriteLine()


        With objWriter
            .WriteLine("Private Function BindGrid(Optional ByVal _refresh As Boolean = True ) As Long")
            .WriteLine("Dim objs As List(of Cls_" & nomSimple & ")")
            .WriteLine("Dim _ret As Long = 0")
            .WriteLine("Try")
            .WriteLine("objs = Cls_" & nomSimple & ".SearchAll")
            .WriteLine("rdg" & nomSimple & ".DataSource = objs")
            .WriteLine("If _refresh Then")
            .WriteLine("rdg" & nomSimple & ".DataBind()")
            .WriteLine("End If")
            .WriteLine(" _ret = objs.Count")
            .WriteLine("Catch ex As Exception")
            .WriteLine("_ret = 0")
            .WriteLine("End Try")
            .WriteLine("Return _ret")
            objWriter.WriteLine(" End Function")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Protected Sub rdg" & nomSimple & "_ItemDataBound(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridItemEventArgs) Handles rdg" & nomSimple & ".ItemDataBound")
            .WriteLine("Dim gridDataItem = TryCast(e.Item, GridDataItem)")
            .WriteLine(" If e.Item.ItemType = GridItemType.Item Or e.Item.ItemType = GridItemType.AlternatingItem Then")
            .WriteLine(" 'Dim _lnk As HyperLink = DirectCast(gridDataItem.FindControl(""hlk""), HyperLink)")
            .WriteLine("  'Dim _lbl_ID As Label = DirectCast(gridDataItem.FindControl(""lbl_ID""), Label)")
            .WriteLine("'_lnk.Attributes.Clear()")
            .WriteLine("'_lnk.Attributes.Add(""onclick"", ""javascript:void(ShowAddUpdateForm('../Parametres/wbfrm_" & nomSimple & "?id="" & CLng(_lbl_ID.Text) & ""', 550, 285));"")")
            .WriteLine("End If")

            .WriteLine("If (gridDataItem IsNot Nothing) Then")
            .WriteLine("Dim item As GridDataItem = gridDataItem")
            .WriteLine("Dim imagedelete As ImageButton = CType(item(""delete"").Controls(0), ImageButton)")
            .WriteLine("Dim imageediter As ImageButton = CType(item(""editer"").Controls(0), ImageButton)")
            .WriteLine("imagedelete.ToolTip = ""Effacer ce " & nomSimple.ToLower & """")
            .WriteLine("imageediter.ToolTip = ""Editer ce " & nomSimple.ToLower & """")
            .WriteLine("imagedelete.CommandArgument = CType(DataBinder.Eval(e.Item.DataItem, ""ID""), String)")
            .WriteLine("imageediter.Attributes.Add(""onclick"", ""javascript:void(ShowAddUpdateForm('../Parametres/wbfrm_" & nomSimple & ".aspx?id="" & CType(DataBinder.Eval(e.Item.DataItem, ""ID""), Long) & ""',600,400));"")")
            .WriteLine("End If")

            objWriter.WriteLine(" End Sub")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Protected Sub rdg" & nomSimple & "_NeedDataSource(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles rdg" & nomSimple & ".NeedDataSource")
            .WriteLine("If IsPostBack Then")
            .WriteLine("BindGrid(False)")
            .WriteLine("End If")
            .WriteLine("End Sub")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Protected Sub rbtnClearFilters_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtnClearFilters.Click")
            .WriteLine("For Each column As GridColumn In rdg" & nomSimple & ".MasterTableView.Columns")
            .WriteLine("column.CurrentFilterFunction = GridKnownFunction.NoFilter")
            .WriteLine("column.CurrentFilterValue = String.Empty")
            .WriteLine("Next")
            .WriteLine("rdg" & nomSimple & ".MasterTableView.FilterExpression = String.Empty")
            .WriteLine(" rdg" & nomSimple & ".MasterTableView.Rebind()")
            .WriteLine("End Sub")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Protected Sub RadAjaxManager1_AjaxRequest(ByVal sender As Object, ByVal e As Telerik.Web.UI.AjaxRequestEventArgs) Handles RadAjaxManager1.AjaxRequest")
            .WriteLine(" Try")
            .WriteLine("Select Case e.Argument")
            .WriteLine("Case ""Reload""")
            .WriteLine("BindGrid(True)")
            .WriteLine(" End Select")
            .WriteLine("Catch ex As Exception")
            .WriteLine("_message = ex.Message + "": RadAjaxManager1_AjaxRequest""")
            .WriteLine("MessageToShow(_message)")
            .WriteLine("End Try")
            .WriteLine("End Sub")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("Protected Sub rdg" & nomSimple & "_ItemCommand(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridCommandEventArgs)")
            .WriteLine("If e.CommandName = Telerik.Web.UI.RadGrid.ExportToExcelCommandName Then")
            .WriteLine("rdg" & nomSimple & ".ExportSettings.ExportOnlyData = True")

            .WriteLine("rdg" & nomSimple & ".GridLines = GridLines.Both")
            .WriteLine("rdg" & nomSimple & ".ExportSettings.IgnorePaging = True")
            .WriteLine("rdg" & nomSimple & ".ExportSettings.OpenInNewWindow = False")
            .WriteLine("rdg" & nomSimple & ".ExportSettings.FileName = ""Liste des " & nomSimple & """")
            .WriteLine("rdg" & nomSimple & ".MasterTableView.Columns(0).Visible = False")
            .WriteLine("rdg" & nomSimple & ".MasterTableView.ExportToExcel()")
            .WriteLine("End If")
            .WriteLine("")
            .WriteLine("Select Case e.CommandName")
            .WriteLine("Case ""delete""")
            .WriteLine("Dim obj As New Cls_" & nomSimple & "(TypeSafeConversion.NullSafeLong(e.CommandArgument))")
            .WriteLine("obj.Delete()")
            .WriteLine("rdg" & nomSimple & ".Rebind()")
            .WriteLine("End Select")
            .WriteLine()

            .WriteLine("End Sub")
            .WriteLine()
        End With

        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateListingCodeAsp(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\ASPWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & "Listing.aspx"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)

        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine()

        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer
        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name
        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")

        objWriter.WriteLine("<%@ Page Title=""" & nomSimple & """ Language=""VB"" MasterPageFile=""~/Pages/MasterPage/MasterPageCommon.master""" & _
       "AutoEventWireup=""false"" CodeFile=""wbfrm_" & nomSimple & "Listing.aspx.vb"" Inherits=""wbfrm_" & nomSimple & "Listing"" %>")

        objWriter.WriteLine("<asp:Content ID=""Content3"" ContentPlaceHolderID=""SheetContentPlaceHolder"" runat=""Server"">")


        objWriter.WriteLine("<telerik:RadCodeBlock ID=""RadCodeBlock1"" runat=""server"">")
        objWriter.WriteLine("<script type=""text/javascript"">")
        objWriter.WriteLine()
        objWriter.WriteLine("function onRequestStart(sender, args) {")
        objWriter.WriteLine(" if (args.get_eventTarget().indexOf(""ExportToExcelButton"") >= 0) {")
        objWriter.WriteLine("args.set_enableAjax(false);")
        objWriter.WriteLine(" }")
        objWriter.WriteLine(" }")
        objWriter.WriteLine()

        With objWriter
            .WriteLine(" function ShowAddUpdateForm(strPage, tmpW, tmpH) {")
            .WriteLine("var oWindow = window.radopen(strPage, ""AddUpdateDialog"");")
            .WriteLine("oWindow.set_autoSize(true);")
            .WriteLine("document.getElementById(""txtWindowPage"").value = strPage;")
            .WriteLine(" if (oWindow) {")
            .WriteLine("if (!oWindow.isClosed()) {")
            .WriteLine("oWindow.center();")
            .WriteLine("var bounds = oWindow.getWindowBounds();")
            .WriteLine("oWindow.moveTo(bounds.x + 'px', ""50px"");")
            .WriteLine("}")
            .WriteLine("}")
            .WriteLine("  return false;")
            .WriteLine("}")
            .WriteLine()
        End With




        objWriter.WriteLine(" function ShowAddUpdateFormMaximize(strPage, tmpW, tmpH) {")
        objWriter.WriteLine("var oWindow = window.radopen(strPage, ""AddUpdateDialog"");")
        objWriter.WriteLine("oWindow.SetSize(tmpW, tmpH);")
        objWriter.WriteLine("document.getElementById(""txtWindowPage"").value = strPage;")
        objWriter.WriteLine(" if (oWindow) {")
        objWriter.WriteLine("if (!oWindow.isClosed()) {")
        objWriter.WriteLine("oWindow.center();")
        objWriter.WriteLine("var bounds = oWindow.getWindowBounds();")
        objWriter.WriteLine("oWindow.moveTo(bounds.x + 'px', ""50px"");")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("oWindow.maximize();")
        objWriter.WriteLine("  return false;")
        objWriter.WriteLine("}")
        objWriter.WriteLine()



        With objWriter
            .WriteLine("function RadWindowClosing() {")
            .WriteLine(" $find(""<%= RadAjaxManager1.ClientID %>"").ajaxRequest(""Reload"");")
            .WriteLine("}")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("function RadWindowClientResizeEnd() {")
            .WriteLine("var manager = GetRadWindowManager();")
            .WriteLine("var window1 = manager.getActiveWindow();")
            .WriteLine(" window1.center();")
            .WriteLine("var bounds = window1.getWindowBounds();")
            .WriteLine("window1.moveTo(bounds.x + 'px', ""50px"");")
            .WriteLine("}")
            .WriteLine()
        End With

        With objWriter
            .WriteLine("function refreshMe() {")
            .WriteLine("$find(""<%= RadAjaxManager1.ClientID %>"").ajaxRequest(""Reload"");")
            .WriteLine("}")
            .WriteLine()
        End With


        objWriter.WriteLine("</script>")
        objWriter.WriteLine("</telerik:RadCodeBlock>")

        objWriter.WriteLine(" <telerik:RadScriptManager ID=""RadScriptManager1""  runat=""server"">")
        objWriter.WriteLine("<Scripts>")
        objWriter.WriteLine("</Scripts>")
        objWriter.WriteLine("</telerik:RadScriptManager>")

        objWriter.WriteLine(" <telerik:RadAjaxManager ID=""RadAjaxManager1"" runat=""server"">")
        objWriter.WriteLine("<AjaxSettings>")

        With objWriter
            .WriteLine("<telerik:AjaxSetting AjaxControlID=""RadAjaxManager1"">")
            .WriteLine("<UpdatedControls>")
            .WriteLine(" <telerik:AjaxUpdatedControl ControlID=""pn_List"" LoadingPanelID=""RadAjaxLoadingPanel1"" />")
            .WriteLine("</UpdatedControls>")
            .WriteLine("</telerik:AjaxSetting>")
        End With

        With objWriter
            .WriteLine("<telerik:AjaxSetting AjaxControlID=""rdg" & nomSimple & """>")
            .WriteLine("<UpdatedControls>")
            .WriteLine(" <telerik:AjaxUpdatedControl ControlID=""Panel_Msg"" LoadingPanelID=""RadAjaxLoadingPanel1"" />")
            .WriteLine(" <telerik:AjaxUpdatedControl ControlID=""rdg" & nomSimple & """ LoadingPanelID=""RadAjaxLoadingPanel1"" />")
            .WriteLine("</UpdatedControls>")
            .WriteLine("</telerik:AjaxSetting>")
        End With

        With objWriter
            .WriteLine("<telerik:AjaxSetting AjaxControlID=""rbtnClearFilters"">")
            .WriteLine("<UpdatedControls>")
            .WriteLine(" <telerik:AjaxUpdatedControl ControlID=""rdg" & nomSimple & """ LoadingPanelID=""RadAjaxLoadingPanel1"" />")
            .WriteLine("</UpdatedControls>")
            .WriteLine("</telerik:AjaxSetting>")
        End With

        objWriter.WriteLine("</AjaxSettings>")
        objWriter.WriteLine("</telerik:RadAjaxManager>")

        objWriter.WriteLine("<telerik:RadSkinManager ID=""RadSkinManager1"" runat=""server"" Skin=""Windows7"">")
        objWriter.WriteLine("</telerik:RadSkinManager>")

        objWriter.WriteLine("<telerik:RadAjaxLoadingPanel ID=""RadAjaxLoadingPanel1"" runat=""server"" />")

        objWriter.WriteLine("<center>")


        With objWriter
            .WriteLine("<table style=""width: 100%; height: 100%;"">")
            .WriteLine("<tr valign=""top"">")
            .WriteLine("<td valign=""top"" style=""width: 100%; height: 100%;"">")
            .WriteLine("<table cellspacing=""0"" cellpadding=""0"" style=""width: 100%; height: 100%;"">")
            .WriteLine("<tr valign=""top"">")
            .WriteLine("<td align=""left"">")
            .WriteLine("<h1>")
            .WriteLine("<asp:Label ID=""lbHeaderTitle"" runat=""server"" CssClass=""LabelBold"" Font-Bold=""True""")
            .WriteLine("Font-Size=""Large"" ForeColor=""Black"">")
            .WriteLine("</asp:Label>")
            .WriteLine("</h1>")
            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("<tr valign=""top"">")
            .WriteLine("<td style=""width: 100%;""  align=""center"" colspan=""2"" valign=""top"">")
            .WriteLine("<asp:Panel runat=""server"" ID=""Panel_Msg"" Visible=""false"">")
            .WriteLine("<div class=""Classe_MsgErreur"">")
            .WriteLine(" <asp:Image ID=""Image_Msg"" runat=""server"" Style=""vertical-align: middle;"" />")
            .WriteLine(" <asp:Label ID=""Label_Msg"" runat=""server"" Text=""""></asp:Label>")
            .WriteLine(" </div>")
            .WriteLine("</asp:Panel>")
            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("<tr valign=""top"">")
            .WriteLine("<td style=""width: 100%;""  align=""center"" colspan=""2"" valign=""top"">")
            .WriteLine("<asp:Panel ID=""pn_List"" runat=""server"" Width=""100%"">")

            .WriteLine("<table style=""width: 100%;"">")
            .WriteLine("<tr valign=""top"">")
            .WriteLine("<td valign=""top"" style=""width: 100%;"">")
            .WriteLine("<table style=""width: 100%;"">")

            .WriteLine(" <tr valign=""top"">")
            .WriteLine("   <td align=""right"" valign=""top"" style=""padding-right: 10px;"">")
            .WriteLine("<div style=""display: block; width: 100%;"">")
            .WriteLine("<div style=""display: block; float: right; padding-right: 10px;"">")
            .WriteLine("<telerik:RadButton ID=""rbtnClearFilters""  Visible=""false"" runat=""server"" Text=""Vider filtres"">")
            .WriteLine("</telerik:RadButton>")
            .WriteLine(" <telerik:RadButton ID=""rbtnRepot"" Visible=""false"" runat=""server"" Text=""Imprimer Rapport"">")
            .WriteLine("</telerik:RadButton>")
            .WriteLine("</div>")
            .WriteLine(" <div style=""display: block; float: left; padding: 0px 5px"">")
            .WriteLine("<telerik:RadButton ID=""rbtnAdd" & nomSimple & """  runat=""server"" AutoPostBack=""False"" Text=""Enregistrer un " & nomSimple.ToLower & """>")
            .WriteLine("</telerik:RadButton>")
            .WriteLine(" </div>")
            .WriteLine("<br clear=""all"" />")
            .WriteLine("</div>")
            .WriteLine("</td>")
            .WriteLine(" </tr>")
            .WriteLine(" <tr valign=""top"">")
            .WriteLine("<td align=""left"" valign=""top"">")


            .WriteLine(" <telerik:RadGrid ID=""rdg" & nomSimple & """ AllowPaging=""True"" AllowSorting=""True"" PageSize=""20""")
            .WriteLine(" runat=""server"" AutoGenerateColumns=""False"" GridLines=""None"" AllowFilteringByColumn=""true"" ")

            .WriteLine(" EnableViewState=""true"" AllowMultiRowSelection=""false"" OnItemCommand=""rdg" & nomSimple & "_ItemCommand"" GroupingSettings-CaseSensitive=""false"">")
            .WriteLine(" <ExportSettings HideStructureColumns=""true"" />")
            .WriteLine("  <MasterTableView CommandItemDisplay=""Top"" GridLines=""None"" DataKeyNames=""ID"" NoDetailRecordsText=""Pas d'enregistrement""")
            .WriteLine("NoMasterRecordsText=""Pas d'enregistrement"">")
            .WriteLine(" <CommandItemSettings ShowAddNewRecordButton=""false"" ShowRefreshButton=""false"" ShowExportToExcelButton=""false"" ")
            .WriteLine("  ExportToExcelText=""Exporter en excel"" />")
            .WriteLine(" <Columns>")

            .WriteLine("<telerik:GridBoundColumn DataField=""ID"" UniqueName=""ID"" Display=""false"">")
            .WriteLine("<ItemStyle Width=""1px"" />")
            .WriteLine("</telerik:GridBoundColumn>")

            Dim countColumn As Integer = 0
            Dim pourcentagevalue As Decimal = 100 / (_table.ListofColumn.Count - 4)
            Dim pourcentage As String = pourcentagevalue.ToString + "%"
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then

                    If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                        Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                        Dim textForcombo As String = ""
                        Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                        Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                        Dim countcolumnref As Long
                        For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                            If column_fore.Type.VbName = "String" Then
                                textForcombo = column_fore.Name
                                Exit For
                            End If

                        Next
                        .WriteLine("<telerik:GridBoundColumn DataField=""" & reftablename & "_" & textForcombo & """ UniqueName=""" & reftablename & "_" & textForcombo & """ HeaderText=""" & reftablename & "_" & textForcombo & """")
                        .WriteLine(" FilterControlAltText=""Filter " & reftablename & "_" & textForcombo & " column"" FilterControlWidth=""95%"" ShowFilterIcon=""false""")
                        .WriteLine(" AutoPostBackOnFilter=""true"" CurrentFilterFunction=""Contains"">")
                        .WriteLine("<ItemStyle Width=""" & pourcentage & """ />")

                        '.WriteLine("                   <ItemTemplate>")
                        '.WriteLine("<asp:HyperLink ID=""hlk"" runat=""server"" Font-Underline=""true"" ToolTip=""Cliquer pour visualiser"" Text='<%# Bind(""" & nomSimple & "_" & textForcombo & """) %>' NavigateUrl=""#""></asp:HyperLink>")
                        '.WriteLine("<asp:Label ID=""lbl_ID"" runat=""server"" Visible=""false"" Text='<%# Bind(""ID"") %>'></asp:Label>")
                        '.WriteLine("</ItemTemplate>")
                        .WriteLine("</telerik:GridBoundColumn>")
                    ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "Date" And column.Name <> _table.ListofColumn(0).Name Then
                        .WriteLine("<telerik:GridBoundColumn DataField=""" & column.Name & """ UniqueName=""" & column.Name & """ HeaderText=""" & column.Name & """")
                        .WriteLine(" FilterControlAltText=""Filter " & column.Name & " column"" FilterControlWidth=""95%"" ShowFilterIcon=""false""")
                        .WriteLine(" AutoPostBackOnFilter=""true"" CurrentFilterFunction=""Contains"">")
                        .WriteLine("<ItemStyle Width=""" & pourcentage & """ />")
                        .WriteLine("</telerik:GridBoundColumn>")
                    ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "Date" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then
                        .WriteLine("<telerik:GridBoundColumn DataField=""" & column.Name & """ UniqueName=""" & column.Name & """ HeaderText=""" & column.Name & """")
                        .WriteLine(" FilterControlAltText=""Filter " & column.Name & " column"" FilterControlWidth=""95%"" DataFormatString=""{0:##,###,##0.00}"" ShowFilterIcon=""false""")
                        .WriteLine(" AutoPostBackOnFilter=""true"" CurrentFilterFunction=""Contains"">")
                        .WriteLine("<ItemStyle Width=""" & pourcentage & """ />")
                        .WriteLine("</telerik:GridBoundColumn>")
                    ElseIf column.TrueSqlServerType = "Date" And column.Name <> _table.ListofColumn(0).Name Then

                    End If
                End If
                countColumn = countColumn + 1

            Next

            .WriteLine(" <telerik:GridButtonColumn ButtonType=""ImageButton"" CommandArgument=""ID"" CommandName=""editer""")
            .WriteLine("        DataTextField=""ID"" HeaderStyle-HorizontalAlign=""Right"" ImageUrl=""~/images/_edit.png""")
            .WriteLine("        ItemStyle-HorizontalAlign=""Right"" UniqueName=""editer"">")
            .WriteLine("        <HeaderStyle HorizontalAlign=""Right"" Width=""1%"" />")
            .WriteLine("        <ItemStyle HorizontalAlign=""Right"" Width=""1%"" />")
            .WriteLine("    </telerik:GridButtonColumn>")

            .WriteLine("<telerik:GridButtonColumn ButtonType=""ImageButton"" CommandName=""delete"" DataTextField=""ID""")
            .WriteLine("HeaderStyle-HorizontalAlign=""Right"" ImageUrl=""~/images/delete.png"" ItemStyle-HorizontalAlign=""Right""")
            .WriteLine("UniqueName=""delete"" ConfirmDialogType=""RadWindow"" ConfirmText=""Voulez-vous vraiment supprimer ce " & nomSimple & "?""")
            .WriteLine("ConfirmTitle=""Attention!"">")
            .WriteLine("<HeaderStyle HorizontalAlign=""Right"" Width=""1%"" />")
            .WriteLine("<ItemStyle HorizontalAlign=""Right"" Width=""1%"" />")
            .WriteLine("</telerik:GridButtonColumn>")

            .WriteLine("  </Columns>")
            .WriteLine("<RowIndicatorColumn FilterControlAltText=""Filter RowIndicator column"">")
            .WriteLine("</RowIndicatorColumn>")
            .WriteLine("<ExpandCollapseColumn FilterControlAltText=""Filter ExpandColumn column"">")
            .WriteLine("</ExpandCollapseColumn>")
            .WriteLine("</MasterTableView>")

            .WriteLine("<ClientSettings>")
            .WriteLine("<ClientEvents ></ClientEvents>")
            .WriteLine("<Selecting AllowRowSelect=""true"" />")
            .WriteLine("</ClientSettings>")
            .WriteLine("<HeaderContextMenu />")
            .WriteLine("<FilterMenu EnableImageSprites=""False"">")
            .WriteLine("</FilterMenu>")
            .WriteLine("</telerik:RadGrid>")
            .WriteLine("<telerik:RadWindowManager ID=""RadWindowManager1"" runat=""server"" VisibleStatusbar=""false""")
            .WriteLine("EnableViewState=""false"">")
            .WriteLine("<Windows>")
            .WriteLine("<telerik:RadWindow ID=""AddUpdateDialog"" runat=""server"" Title="""" Left=""75px"" ReloadOnShow=""true""")
            .WriteLine(" ShowContentDuringLoad=""false"" Modal=""true"" OnClientClose=""RadWindowClosing"" Behaviors=""Minimize, Move, Resize, Maximize, Close""")
            .WriteLine("EnableShadow=""true"" OnClientResizeEnd=""RadWindowClientResizeEnd"" />")
            .WriteLine("</Windows>")
            .WriteLine("</telerik:RadWindowManager>")


            .WriteLine("<telerik:RadContextMenu ID=""ContextMenu"" runat=""server"" OnClientItemClicked=""""")
            .WriteLine("EnableRoundedCorners=""true"" EnableShadows=""true"">")
            .WriteLine("<Items>")
            .WriteLine("<telerik:RadMenuItem Text=""Editer"" />")
            .WriteLine("</Items>")
            .WriteLine("</telerik:RadContextMenu>")


            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("</table>")
            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("</table>")


            .WriteLine("</asp:Panel>")
            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("</table>")
            .WriteLine("</td>")
            .WriteLine("</tr>")
            .WriteLine("</table>")
        End With



        objWriter.WriteLine("<br />")
        objWriter.WriteLine("<input id=""txtWindowPage"" type=""hidden"" />")
        objWriter.WriteLine("</center>")
        objWriter.WriteLine("</asp:Content>")

        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateTableInXMLFormat(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".xml"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomClasse & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")
        header &= ""
        Dim content As String = "Public Class " & nomClasse & Chr(13) _
                                 & "Implements IGeneral"

        _end = "End Class" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        objWriter.WriteLine(header)
        If ListBox_NameSpace.Items.Count > 0 Then
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                objWriter.WriteLine(ListBox_NameSpace.Items(i))
            Next
        End If
        objWriter.WriteLine()
        objWriter.WriteLine(content)
        objWriter.WriteLine()
        Dim _table As New Cls_Table()
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        Dim settings As XmlWriterSettings = New XmlWriterSettings()
        settings.Indent = True
        Using writer As XmlWriter = XmlWriter.Create(txt_PathGenerate_Script & nomSimple.Substring(1, nomSimple.Length - 1) & ".xml", settings)
            writer.WriteStartDocument()
            writer.WriteStartElement(nomSimple)
            Dim count2 As Integer = 0
            For Each _column As Cls_Column In _table.ListofColumn
                If count2 < cap - 4 Then
                    writer.WriteStartElement("Colonne")
                    writer.WriteElementString("name", _column.Name)
                    writer.WriteElementString("type", _column.Type.SqlServerName)
                    writer.WriteElementString("longueur", _column.Length)
                    writer.WriteEndElement()
                    count2 += 1
                Else
                    Exit For
                End If
            Next
            writer.WriteEndElement()
            'writer.WriteEndDocument()
        End Using

    End Sub

#End Region

#Region "Conversion Fonctions"
    Public Shared Function DefaultValue(ByVal type As String) As Object
        Select Case type
            Case "String"
                Return ""
            Case "Long"
                Return 0
            Case "Integer"
                Return 0
            Case "Int32"
                Return 0
            Case "Int64"
                Return 0
            Case "Boolean"
                Return False
            Case Else
                Return "Nothing"
        End Select
    End Function

    Public Shared Function ConvertDBToJavaType(ByVal Type As String) As String
        Dim AndroidTypeHash As New Hashtable
        AndroidTypeHash.Add("bigint", "long")
        AndroidTypeHash.Add("binary", "boolean")
        AndroidTypeHash.Add("bit", "byte")
        AndroidTypeHash.Add("char", "char")
        AndroidTypeHash.Add("date", "Date")
        AndroidTypeHash.Add("datetime", "Date")
        AndroidTypeHash.Add("datetime2", "Date")
        AndroidTypeHash.Add("DATETIMEOFFSET", "Date")
        AndroidTypeHash.Add("decimal", "double")
        AndroidTypeHash.Add("float", "float")
        AndroidTypeHash.Add("int", "int")
        AndroidTypeHash.Add("image", "byte[]")
        AndroidTypeHash.Add("money", "Currency")
        AndroidTypeHash.Add("nchar", "String") '' /* or tableau of char*/
        AndroidTypeHash.Add("nvarchar", "String")
        AndroidTypeHash.Add("numeric", "double")
        AndroidTypeHash.Add("rowversion", "")
        AndroidTypeHash.Add("smallint", "short")
        AndroidTypeHash.Add("smallmoney", "Currency")
        AndroidTypeHash.Add("time", "Time")
        AndroidTypeHash.Add("varbinary", "")
        AndroidTypeHash.Add("varchar", "String")


        Return AndroidTypeHash(Type)
    End Function

    Public Shared Function ConvertDBToJavaParsingType(ByVal Type As String) As String
        If Type.Contains("decimal") Or Type.Contains("numeric") Then
            Type = "decimal"
        End If
        Dim AndroidTypeHash As New Hashtable
        AndroidTypeHash.Add("bigint", "Long")
        AndroidTypeHash.Add("binary", "Boolean")
        AndroidTypeHash.Add("bit", "Boolean")
        AndroidTypeHash.Add("char", "char")
        AndroidTypeHash.Add("date", "Date")
        AndroidTypeHash.Add("datetime", "Date")
        AndroidTypeHash.Add("datetime2", "Date")
        AndroidTypeHash.Add("DATETIMEOFFSET", "Date")
        AndroidTypeHash.Add("decimal", "Double")
        AndroidTypeHash.Add("float", "FLOAT")
        AndroidTypeHash.Add("int", "Integer")
        AndroidTypeHash.Add("image", "byte[]")
        AndroidTypeHash.Add("money", "Currency")
        AndroidTypeHash.Add("nchar", "String") '' /* or tableau of char*/
        AndroidTypeHash.Add("nvarchar", "String")
        AndroidTypeHash.Add("numeric", "Double")
        AndroidTypeHash.Add("rowversion", "")
        AndroidTypeHash.Add("smallint", "Integer")
        AndroidTypeHash.Add("smallmoney", "Currency")
        AndroidTypeHash.Add("time", "Time")
        AndroidTypeHash.Add("varbinary", "")
        AndroidTypeHash.Add("varchar", "String")


        Return AndroidTypeHash(Type)
    End Function

    Public Shared Function ConvertJavaToSetPropertiesAndroidType(ByVal Type As String) As String
        Dim AndroidTypeHash As New Hashtable
        AndroidTypeHash.Add("long", "Long")
        AndroidTypeHash.Add("boolean", "boolean")
        AndroidTypeHash.Add("byte", "byte")
        AndroidTypeHash.Add("date", "Date")
        AndroidTypeHash.Add("Date", "Date")
        AndroidTypeHash.Add("float", "Float")
        AndroidTypeHash.Add("int", "Int")
        AndroidTypeHash.Add("byte[]", "byte[]")
        AndroidTypeHash.Add("Currency", "Currency")
        AndroidTypeHash.Add("String", "String")
        AndroidTypeHash.Add("double", "Double")
        AndroidTypeHash.Add("rowversion", "")
        AndroidTypeHash.Add("Time", "Time")
        AndroidTypeHash.Add("varbinary", "")
        AndroidTypeHash.Add("DateTime", "String")



        Return AndroidTypeHash(Type)
    End Function

#End Region

#Region "Java Class Fonctions"
    Public Shared Sub CreateJavaClassDomaine(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl_", "")
        Dim nomUpperClasse As String = nomClasse.Substring(0, 1).ToUpper() & nomClasse.Substring(1, nomClasse.Length - 1)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim path As String = txt_PathGenerate_Script & nomUpperClasse & ".java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""

        Dim header As String = "'''Generate By Edou Application *******" & Chr(13) _
                               & "''' Class " + nomUpperClasse & Chr(13) & Chr(13)
        header = ""
        Dim content As String = "public class " & nomUpperClasse & " {" & Chr(13)

        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine("package ht.edu.fds.domaine;")
        objWriter.WriteLine()
        objWriter.WriteLine("import java.sql.Timestamp;")
        objWriter.WriteLine("import java.util.*;")
        objWriter.WriteLine("import javax.validation.constraints.*;")
        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim ds As DataSet = MySqlHelper.LoadTableStructure_MySql(name)
        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer = ds.Tables(0).Rows.Count

        For Each dt As DataRow In ds.Tables(0).Rows
            If dt(3).ToString = "PRI" Then
                Id_table = dt(0).ToString()
            End If

        Next

        For Each dt As DataRow In ds.Tables(0).Rows
            Dim arrstring As String() = Nothing
            If count < cap - 4 Then
                cols.Add("" & dt(0))
                initialtypes.Add(dt(1))
                If dt(1).ToString.Contains("(") Then
                    arrstring = dt(1).ToString.Split("(")
                    types.Add(ConvertDBToJavaType(arrstring(0).ToString))
                    length.Add(arrstring(1).ToString.Replace(")", ""))
                Else
                    types.Add(ConvertDBToJavaType((dt(1))))
                    length.Add("0")
                End If

                count += 1
            Else
                Exit For
            End If
        Next
        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Attributes"">")
        'objWriter.WriteLine("Private _id As Long")
        objWriter.WriteLine()
        objWriter.WriteLine(" private long id ; ")
        Try
            For i As Int32 = 1 To cols.Count - 1

                'If Not nottoputforlist.Contains(cols(i)) Then
                '    insertstring &= ", " & cols(i)
                '    updatestring &= ", " & cols(i)
                'End If
                Dim attrib As String = ""  'not used for now to be updated.

                objWriter.WriteLine("private " & types(i) & " " & cols(i) & ";")
                If initialtypes(i) = "image" Then

                    objWriter.WriteLine("private String " & cols(i) & "String;" & "")
                End If
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("private " & cols(i).Substring(3, cols(i).Length - 3) & " " & cols(i).Substring(3, cols(i).Length - 3).ToLower & ";")
                End If
            Next
        Catch ex As Exception

        End Try
        objWriter.WriteLine()
        objWriter.WriteLine("//</editor-fold>")
        objWriter.WriteLine()
        objWriter.WriteLine()

        ''''''''''''''''''''''''''''''''''''''''çonstructeur'''''''''''''''''''''''''''''''''''''
        Dim listof_number As New List(Of String) From {"long"}
        Dim listof_string As New List(Of String) From {"String"}
        Dim listof_nulltypes As New List(Of String) From {"Date"}

        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Constructeurs"">")
        objWriter.WriteLine()
        objWriter.WriteLine("public " & nomUpperClasse & "() {")
        objWriter.WriteLine("this.id = 0;")
        Dim attribut_string As String = ""
        Dim constructeur_string As String = ""
        Dim attribut_string2 As String = "long id,"
        For i As Int32 = 1 To cols.Count - 1
            If attribut_string = "" Then
                attribut_string &= types(i) & " " & cols(i)
                constructeur_string &= cols(i)
            Else
                attribut_string &= "," & types(i) & " " & cols(i)
                constructeur_string &= "," & cols(i)
            End If
            If listof_number.Contains(types(i)) Then
                objWriter.WriteLine("this." & cols(i) & " = " & "0;")
            ElseIf listof_string.Contains(types(i)) Then
                objWriter.WriteLine("this." & cols(i) & " = " & """"";")
            ElseIf listof_nulltypes.Contains(types(i)) Then
                objWriter.WriteLine("this." & cols(i) & " = " & "null;")
            End If
        Next
        objWriter.WriteLine("}")


        attribut_string &= ")"
        attribut_string2 &= attribut_string
        objWriter.WriteLine()


        objWriter.WriteLine("public " & nomUpperClasse & "(long id)" & " {")
        objWriter.WriteLine(" this();")
        objWriter.WriteLine("this.id = id;")
        objWriter.WriteLine("}")

        objWriter.WriteLine()


        objWriter.WriteLine("public " & nomUpperClasse & "(" & attribut_string & " {")
        objWriter.WriteLine("  this.id = 0;")
        For i As Int32 = 1 To cols.Count - 1
            objWriter.WriteLine("this." & cols(i) & " = " & cols(i) & ";")
        Next

        objWriter.WriteLine("}")
        objWriter.WriteLine()

        objWriter.WriteLine("public " & nomUpperClasse & "(" & attribut_string2 & " {")
        objWriter.WriteLine("this(" & constructeur_string & ") ;")
        objWriter.WriteLine("  this.id = id;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("//</editor-fold>")


        objWriter.WriteLine()

        '''''''''''''''''''''''''''''''''''''''''properties''''''''''''''''''''''''''''''''''''

        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Properties"">")

        objWriter.WriteLine("public long getId() {" & Chr(13) & _
                             " return id;" & Chr(13) & _
                             "}")

        For i As Int32 = 1 To cols.Count - 1 ''On ne cree pas de property pour la derniere column
            Dim propName As String = ""
            Dim s As String() = cols(i).Split("_")
            For j As Integer = 1 To s.Length - 1
                propName &= StrConv(s(j), VbStrConv.ProperCase)
            Next
            Dim validation As String = ""

            If types(i) = "String" Then
                validation = "@Size( min  = 1 , max =" & length(i) & " ,  message = ""Le nombre de caractere de" & cols(i) & " est invalide"")"
            ElseIf types(i) = "int" Or types(i) = "long" Then
                validation = "@Min(value = 1, message = ""Le " & cols(i) & " est obligatoire "") "

            End If

            If cols(i) = "Email" Or cols(i).ToString.Contains("Email") Or cols(i) = "AdresseElectronique" Then
                validation = "@NotBlank(message = ""L'adresse électronique est obligatoire"")" & Chr(13) _
                     & "@Email(message = ""Le format de l'adresse électronique n'est pas valide"")"
            End If

            Dim attrib As String = "public " & types(i) & " get" & cols(i) & "() {"
            Dim setter As String = "public void " & "set" & cols(i) & "(" & types(i) & " " & cols(i).ToLower() & ") {"

            If cols(i) <> "_isdirty" Or cols(i) <> "_LogData" Then
                '   objWriter.WriteLine(log)

                objWriter.WriteLine()
                objWriter.WriteLine(validation)
                objWriter.WriteLine(attrib)
                objWriter.WriteLine("return " & cols(i) & ";")
                objWriter.WriteLine("}")
                objWriter.WriteLine(setter)
                objWriter.WriteLine("this." & cols(i) & " = " & cols(i).ToLower & ";")
                objWriter.WriteLine("}")

                If initialtypes(i) = "image" Then
                    objWriter.WriteLine()
                    objWriter.WriteLine("public boolean hasImage() {")
                    objWriter.WriteLine("return (this." & cols(i) & "String != null && this." & cols(i) & "String.length() > 0 || this." & cols(i) & "!= null && this." & cols(i) & ".length > 0);")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("public String get" & cols(i) & "String() {")
                    objWriter.WriteLine("return " & cols(i).ToLower & "String;")
                    objWriter.WriteLine("}")

                    objWriter.WriteLine("public void set" & cols(i) & "String(String " & cols(i).ToLower & "String) {")
                    objWriter.WriteLine("this." & cols(i) & "String = " & cols(i).ToLower & "String;")
                    objWriter.WriteLine("}")


                End If
            End If

        Next

        objWriter.WriteLine("//</editor-fold>")

        objWriter.WriteLine()

        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()

    End Sub

    Public Shared Sub CreateJavaClassDAL(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl_", "")
        Dim nomUpperClasse As String = nomClasse.Substring(0, 1).ToUpper() & nomClasse.Substring(1, nomClasse.Length - 1)

        Dim point_interogation As String = ""
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim path As String = txt_PathGenerate_Script & nomUpperClasse & "DAL.java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim compteur As Integer
        Dim header As String = "'''Generate By Edou Application *******" & Chr(13) _
                               & "''' Class " + nomUpperClasse & Chr(13) & Chr(13)
        header = ""
        Dim content As String = "public class " & nomUpperClasse & "DAL {" & Chr(13)

        _end = "}" & Chr(13)
        nomClasse = nomUpperClasse
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine("package ht.edu.fds.dal;")
        objWriter.WriteLine()
        objWriter.WriteLine("import java.util.*;")
        objWriter.WriteLine("import com.sun.rowset.CachedRowSetImpl;" & Chr(13) & _
                            "import ht.edu.fds.domaine.*;" & Chr(13) & _
                            "import ht.edu.fds.servtech.*;" & Chr(13) & _
                            "import java.sql.*;" & Chr(13) & _
                            "import javax.sql.rowset.CachedRowSet;")

        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim ds As DataSet = MySqlHelper.LoadTableStructure_MySql(name)
        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer = ds.Tables(0).Rows.Count

        For Each dt As DataRow In ds.Tables(0).Rows
            If dt(3).ToString = "PRI" Then
                Id_table = dt(0).ToString()
            End If

        Next
        For Each dt As DataRow In ds.Tables(0).Rows
            If count < cap - 4 Then
                cols.Add("" & dt(0))
                initialtypes.Add(dt(1))
                If dt(1).ToString.Contains("(") Then
                    Dim arrstring As String() = dt(1).ToString.Split("(")
                    types.Add(ConvertDBToJavaType(arrstring(0).ToString))
                Else
                    types.Add(ConvertDBToJavaType((dt(1))))

                End If
                length.Add(dt(3))
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Attributes"">")
        'objWriter.WriteLine("Private _id As Long")
        objWriter.WriteLine("private static String driverstring = ConnectionDAL.getCurrentdriverstring();")
        objWriter.WriteLine("private static String connectionstring = ConnectionDAL.getCurrentconnectionstring();")
        objWriter.WriteLine("private static CallableStatement cs ;")
        objWriter.WriteLine("private static Connection con ;")
        objWriter.WriteLine("private static ResultSet reponse;")
        objWriter.WriteLine("private static CachedRowSet crs; ")
        objWriter.WriteLine("private static String currentexception = """" ;")

        objWriter.WriteLine("//</editor-fold>")
        objWriter.WriteLine()

        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Save"">")

        objWriter.WriteLine("  public static boolean Save(" & nomUpperClasse & " obj) throws Exception ")
        objWriter.WriteLine(" {")
        objWriter.WriteLine()
        objWriter.WriteLine("long ID = obj.getId() ;")
        objWriter.WriteLine("Boolean valret = false ;")
        objWriter.WriteLine(" try " & Chr(13) & _
                            " { " & Chr(13) & _
                            "Class.forName(driverstring); " & Chr(13) & _
                            "con = DriverManager.getConnection(connectionstring);" & Chr(13) & _
                            "if(ID == 0)" & Chr(13) & _
                            "{ ")
        compteur = 0
        For i As Int32 = 1 To cols.Count
            If point_interogation = "" Then
                point_interogation = "?"
            Else
                point_interogation &= ",?"
            End If

        Next
        Dim fin_point_interogation As String = ")  }"");"

        objWriter.WriteLine(" cs =   con.prepareCall(""{call SP_Insert" & nomClasse & "(" & point_interogation & fin_point_interogation)


        For i As Int32 = 1 To cols.Count - 1
            Dim attrib As String = ""  'not used for now to be updated.
            If types(i) = "Date" Then
                objWriter.WriteLine(" cs.set" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i & ",TypeSafeConversion.NullSafeDate_ToSql(obj.get" & cols(i) & "()));")
            Else
                objWriter.WriteLine(" cs.set" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i & ",obj.get" & cols(i) & "());")
            End If

        Next

        objWriter.WriteLine("   cs.setString(" & cols.Count & ",""Admin"");")

        objWriter.WriteLine("}")
        objWriter.WriteLine("else")
        objWriter.WriteLine("{")
        point_interogation &= ",?"

        objWriter.WriteLine(" cs =   con.prepareCall(""{call SP_Update" & nomClasse & "(" & point_interogation & fin_point_interogation)
        objWriter.WriteLine(" cs.setLong" & "(1,obj.getId());")
        For i As Int32 = 1 To cols.Count - 1
            Dim attrib As String = ""  'not used for now to be updated.
            If types(i) = "Date" Then
                objWriter.WriteLine(" cs.set" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i & ",TypeSafeConversion.NullSafeDate_ToSql(obj.get" & cols(i) & "()));")
            Else
                objWriter.WriteLine(" cs.set" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i + 1 & ",obj.get" & cols(i) & "());")
            End If

        Next
        objWriter.WriteLine("   cs.setString(" & cols.Count + 1 & ",""Admin"");")
        objWriter.WriteLine("}")




        objWriter.WriteLine("   ResultSet rs = cs.executeQuery(); " & Chr(13) & _
                             "while (rs.next()) {" & Chr(13) & _
                              " ID  = rs.getLong(""ID"");   " & Chr(13) & _
                              "  }" & Chr(13) & _
                              "if (ID > 0) {" & Chr(13) & _
                              " valret = true;" & Chr(13) & _
                              "    }" & Chr(13) & _
                              "  }" & Chr(13) & _
                              "  catch (ClassNotFoundException nfex)" & Chr(13) & _
                              "{" & Chr(13) & _
                              "  throw new ClassNotFoundException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",save) ! ""+ nfex.getMessage());  " & Chr(13) & _
                              "}" & Chr(13) & _
                              "catch (Exception ex)" & Chr(13) & _
                              "{" & Chr(13) & _
                             "  throw new Exception("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",save) ! ""+ ex.getMessage());  " & Chr(13) & _
                              "}" & Chr(13) & _
                              "finally" & Chr(13) & _
                              "{" & Chr(13) & _
                              "try" & Chr(13) & _
                              "{" & Chr(13) & _
                              " con.close();" & Chr(13) & _
                              "}" & Chr(13) & _
                              "catch (SQLException sqlex)" & Chr(13) & _
                              "{" & Chr(13) & _
                              "  throw new SQLException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",save) ! ""+ sqlex.getMessage());  " & Chr(13) & _
                              "}" & Chr(13) & _
                              "}" & Chr(13) & _
                              "return valret;   " & Chr(13) & _
                              "}" & Chr(13))


        objWriter.WriteLine("//</editor-fold>")
        objWriter.WriteLine()
        objWriter.WriteLine()

        '''''''''''''''''''''''''''''''''''''''''delete''''''''''''''''''''''''''''''''''''

        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Delete"">")

        objWriter.WriteLine("public static boolean Delete(" & nomUpperClasse & " obj) throws Exception" & Chr(13) & _
                                "{" & Chr(13) & _
                                    "boolean isDeleted = false;" & Chr(13) & _
                                    "try" & Chr(13) & _
                                    "{" & Chr(13) & _
                                       " Class.forName(driverstring);" & Chr(13) & _
                                        "con = DriverManager.getConnection(connectionstring);" & Chr(13) & _
                                        "cs =   con.prepareCall(""{call SP_Delete" & nomClasse & "(?) }"");" & Chr(13) & _
                                        "cs.setLong(1, obj.getId());" & Chr(13) & _
                                        "int result =  cs.executeUpdate();" & Chr(13) & _
                                        "if (result > 0) {isDeleted = true; }" & Chr(13) & _
                                    "}" & Chr(13) & _
                                    "catch (ClassNotFoundException nfex)" & Chr(13) & _
                                    "{" & Chr(13) & _
                                    "  throw new ClassNotFoundException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Delete) ! ""+ nfex.getMessage());  " & Chr(13) & _
                                    "}" & Chr(13) & _
                                    "catch (Exception ex)" & Chr(13) & _
                                    "{" & Chr(13) & _
                                         "  throw new Exception("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Delete) ! ""+ ex.getMessage());  " & Chr(13) & _
                                    "}" & Chr(13) & _
                                    "finally" & Chr(13) & _
                                    "{" & Chr(13) & _
                                        "try" & Chr(13) & _
                                        "{" & Chr(13) & _
                                            "con.close();" & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "catch (SQLException sqlex)" & Chr(13) & _
                                        "{" & Chr(13) & _
                                          "  throw new SQLException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Delete) ! ""+ sqlex.getMessage());  " & Chr(13) & _
                                        "}" & Chr(13) & _
                                    "}     " & Chr(13) & _
                                     "return isDeleted;" & Chr(13) & _
                                "}")
        objWriter.WriteLine("//</editor-fold>")

        '''''''''''''''''''''''''''''''''''''''''Read''''''''''''''''''''''''''''''''''''
        objWriter.WriteLine()
        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""Read"">")

        objWriter.WriteLine("public static void Read(" & nomUpperClasse & " obj) throws Exception " & Chr(13) & _
                               " {" & Chr(13) & _
                                    "try" & Chr(13) & _
                                    "{" & Chr(13) & _
                                        "Class.forName(driverstring);" & Chr(13) & _
                                        "con = DriverManager.getConnection(connectionstring);" & Chr(13) & _
                                        "cs =   con.prepareCall(""{call SP_Select" & nomClasse & "_ByID(?) }"");" & Chr(13) & _
                                        "cs.setLong(1, obj.getId());" & Chr(13) & _
                                        "reponse = cs.executeQuery();" & Chr(13) & _
                                        "reponse.last();" & Chr(13) & _
                                        "reponse.beforeFirst();" & Chr(13) & _
                                        "while(reponse.next())" & Chr(13) & _
                                        "{")
        For i As Int32 = 1 To cols.Count - 1
            Dim attrib As String = ""  'not used for now to be updated.
            objWriter.WriteLine(" obj.set" & cols(i) & "(reponse.get" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i + 1 & " ));")
        Next
        objWriter.WriteLine("             } " & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "catch (ClassNotFoundException nfex)" & Chr(13) & _
                                        "{" & Chr(13) & _
                                         "currentexception = nfex.getMessage();" & Chr(13) & _
                                          "  throw new ClassNotFoundException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Read) ! ""+ nfex.getMessage());  " & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "catch (Exception ex)" & Chr(13) & _
                                        "{" & Chr(13) & _
                                         "currentexception = ex.getMessage();" & Chr(13) & _
                                            "  throw new Exception("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Read) ! ""+ ex.getMessage());  " & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "finally" & Chr(13) & _
                                        "{" & Chr(13) & _
                                            "try" & Chr(13) & _
                                            "{" & Chr(13) & _
                                                "con.close();" & Chr(13) & _
                                            "}" & Chr(13) & _
                                            "catch (SQLException sqlex)" & Chr(13) & _
                                            "{" & Chr(13) & _
                                             "currentexception = sqlex.getMessage();" & Chr(13) & _
                                                "  throw new SQLException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",Read) ! ""+ sqlex.getMessage());  " & Chr(13) & _
                                            "}" & Chr(13) & _
                                        "} " & Chr(13) & _
                                    "}")
        objWriter.WriteLine("//</editor-fold>")
        objWriter.WriteLine()
        '''''''''''''''''''''''''''''''''''''''''ListAll''''''''''''''''''''''''''''''''''''
        objWriter.WriteLine("//<editor-fold defaultstate=""collapsed"" desc=""ListAll"">")

        objWriter.WriteLine("public static CachedRowSet ListAll() throws Exception" & Chr(13) & _
                                    "{" & Chr(13) & _
            Chr(9) & Chr(9) & Chr(9) & "try" & Chr(13) & _
                                        "{" & Chr(13) & _
                                            "crs = new CachedRowSetImpl();" & Chr(13) & _
                                            "Class.forName(driverstring);" & Chr(13) & _
                                            "con = DriverManager.getConnection(connectionstring);" & Chr(13) & _
                                            "cs =   con.prepareCall(""{call SP_ListAll_" & nomClasse & "() }"");" & Chr(13) & _
                                            "reponse = cs.executeQuery();" & Chr(13) & _
                                            "crs.populate(reponse);" & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "catch (ClassNotFoundException nfex)" & Chr(13) & _
                                        "{" & Chr(13) & _
                                        "currentexception = nfex.getMessage();" & Chr(13) & _
                                           "  throw new ClassNotFoundException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",ListAll) ! ""+ nfex.getMessage());  " & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "catch (Exception ex)" & Chr(13) & _
                                        "{" & Chr(13) & _
                                        "currentexception = ex.getMessage();" & Chr(13) & _
                                            "  throw new Exception("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",ListAll) ! ""+ ex.getMessage());  " & Chr(13) & _
                                        "}" & Chr(13) & _
                                        "finally" & Chr(13) & _
                                        "{" & Chr(13) & _
                                            "try" & Chr(13) & _
                                            "{" & Chr(13) & _
                                                "con.close();" & Chr(13) & _
                                            "}" & Chr(13) & _
                                            "catch (SQLException sqlex)" & Chr(13) & _
                                            "{" & Chr(13) & _
                                               "currentexception = sqlex.getMessage();" & Chr(13) & _
                                              "  throw new SQLException("" Erreur systeme ! Contacter l'administrateur (" & nomUpperClasse & ",ListAll) ! ""+ sqlex.getMessage());  " & Chr(13) & _
                                            "}" & Chr(13) & _
                                            "if (!"""".equals(currentexception.toString()) ){" & Chr(13) & _
             "     throw new Exception("" Erreur systeme ! Contacter l'administrateur (Unite,ListAll) ! "" + currentexception.toString());" & Chr(13) & _
            "}" & Chr(13) &
                                            "return crs;   " & Chr(13) & _
                                        "}        " & Chr(13) & _
                                    "}")

        objWriter.WriteLine("//</editor-fold>")

        objWriter.WriteLine()

        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()

    End Sub

    Public Shared Sub CreateJavaClassSession(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox)
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl_", "")
        Dim nomUpperClasse As String = nomClasse.Substring(0, 1).ToUpper() & nomClasse.Substring(1, nomClasse.Length - 1)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim path As String = txt_PathGenerate_Script & "Session" & nomUpperClasse & ".java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""

        Dim header As String = "'''Generate By Edou Application *******" & Chr(13) _
                               & "''' Class " + nomUpperClasse & Chr(13) & Chr(13)
        header = ""
        Dim content As String = "public class " & "Session" & nomUpperClasse & " {" & Chr(13)

        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine("package ht.edu.fds.application;")
        objWriter.WriteLine()


        objWriter.WriteLine("import ht.edu.fds.dal.*;" & Chr(13) & _
                            "import ht.edu.fds.domaine.*;" & Chr(13) & _
                            "import ht.edu.fds.servtech.*;" & Chr(13) & _
                            "import java.lang.Object;" & Chr(13) & _
                            "import java.sql.ResultSet;" & Chr(13) & _
                            "import java.sql.SQLException;" & Chr(13) & _
                            "import java.sql.Timestamp;" & Chr(13) & _
                            "import java.util.ArrayList;" & Chr(13) & _
                            "import java.util.Date;" & Chr(13) & _
                            "import java.util.logging.Level;" & Chr(13) & _
                            "import java.util.logging.Logger;")

        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim ds As DataSet = MySqlHelper.LoadTableStructure_MySql(name)
        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer = ds.Tables(0).Rows.Count

        For Each dt As DataRow In ds.Tables(0).Rows
            If dt(3).ToString = "PRI" Then
                Id_table = dt(0).ToString()
            End If

        Next
        For Each dt As DataRow In ds.Tables(0).Rows
            If count < cap - 4 Then
                cols.Add("" & dt(0))
                initialtypes.Add(dt(1))
                If dt(1).ToString.Contains("(") Then
                    Dim arrstring As String() = dt(1).ToString.Split("(")
                    types.Add(ConvertDBToJavaType(arrstring(0).ToString))
                Else
                    types.Add(ConvertDBToJavaType((dt(1))))

                End If
                length.Add(dt(3))
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine("private " & nomUpperClasse & " obj;")
        objWriter.WriteLine("private ResultSet list" & nomUpperClasse & ";")
        objWriter.WriteLine()
        objWriter.WriteLine("ResultSet getResultSet" & nomUpperClasse & "()")
        objWriter.WriteLine("{")
        objWriter.WriteLine("return this.list" & nomUpperClasse & ";")
        objWriter.WriteLine("}")
        objWriter.WriteLine()
        objWriter.WriteLine("public " & nomUpperClasse & " getObjetCourant()")
        objWriter.WriteLine("{")
        objWriter.WriteLine("return this.obj;")
        objWriter.WriteLine("}")
        objWriter.WriteLine()
        objWriter.WriteLine("public long getIDCurrentObject(" & nomUpperClasse & " obj)")
        objWriter.WriteLine("{")
        objWriter.WriteLine("this.obj = obj;")
        objWriter.WriteLine("return this.obj.getId();")
        objWriter.WriteLine("}")

        objWriter.WriteLine()
        Dim attribut_string As String = ""
        Dim constructeur_string As String = "Long.parseLong(ID)"
        Dim attribut_string2 As String = "String ID"
        For i As Int32 = 1 To cols.Count - 1
            If attribut_string2 = "" Then
                attribut_string2 &= "String " & cols(i)
                constructeur_string &= cols(i)
            Else
                attribut_string2 &= ", String " & cols(i)
                If types(i).ToString = "long" Then
                    constructeur_string &= ",Long.parseLong(" & cols(i) & ")"
                ElseIf types(i).ToString = "Double" Then
                    constructeur_string &= ",Double.parseDouble(" & cols(i) & ")"
                ElseIf types(i).ToString = "Date" Then
                    constructeur_string &= ",TypeSafeConversion.NullSafeDate(" & cols(i) & ")"
                ElseIf types(i).ToString = "float" Then
                    constructeur_string &= ",Float.parseFloat(" & cols(i) & ")"
                ElseIf types(i).ToString = "Timestamp" Then
                    constructeur_string &= ",Timestamp.valueOf(" & cols(i) & ")"
                ElseIf types(i).ToString = "boolean" Then
                    constructeur_string &= ",Boolean.parseBoolean(" & cols(i) & ")"
                Else
                    constructeur_string &= "," & cols(i)
                End If
            End If

        Next

        objWriter.WriteLine("public boolean save" & nomUpperClasse & "(" & attribut_string2 & " ) throws Exception")
        objWriter.WriteLine("{")
        objWriter.WriteLine("boolean isSaved = false;")
        objWriter.WriteLine("obj = new " & nomUpperClasse & "(" & constructeur_string & ");")
        objWriter.WriteLine("isSaved = " & nomUpperClasse & "DAL.Save(obj);")
        objWriter.WriteLine("return isSaved;")
        objWriter.WriteLine("}")

        objWriter.WriteLine()

        objWriter.WriteLine("public Object[][] listAll" & nomUpperClasse & "() throws Exception {")
        objWriter.WriteLine("int taille= 0 , i = 0 ;")
        objWriter.WriteLine(" Object[][] data = null;")
        objWriter.WriteLine("list" & nomUpperClasse & " = " & nomUpperClasse & "DAL.ListAll();")
        objWriter.WriteLine("list" & nomUpperClasse & ".last();")
        objWriter.WriteLine("taille = list" & nomUpperClasse & ".getRow();")
        objWriter.WriteLine("list" & nomUpperClasse & ".beforeFirst();")
        objWriter.WriteLine("data = new Object[taille][" & cols.Count & "];")
        objWriter.WriteLine("while(list" & nomUpperClasse & ".next())")
        objWriter.WriteLine("{")
        objWriter.WriteLine("long ID = list" & nomUpperClasse & ".getLong(1);")
        For i As Int32 = 1 To cols.Count - 1
            objWriter.WriteLine(types(i) & " " & cols(i) & " = list" & nomUpperClasse & ".get" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i + 1 & ");")
        Next
        objWriter.WriteLine("data[i][0] = ID;")
        For i As Int32 = 1 To cols.Count - 1
            objWriter.WriteLine("data[i][" & i & "] = " & cols(i) & ";")
        Next
        objWriter.WriteLine("i++;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("return data;")
        objWriter.WriteLine("}")


        objWriter.WriteLine("public ArrayList<" & nomUpperClasse & "> listAll" & nomUpperClasse & "_ForObject() throws Exception")
        objWriter.WriteLine("{")
        objWriter.WriteLine("ArrayList<" & nomUpperClasse & "> liste = new ArrayList<" & nomUpperClasse & ">" & "();")
        objWriter.WriteLine("list" & nomUpperClasse & " = " & nomUpperClasse & "DAL.ListAll();")

        objWriter.WriteLine(" while ( list" & nomUpperClasse & ".next())")
        objWriter.WriteLine("{")

        objWriter.WriteLine("long id = list" & nomUpperClasse & ".getLong(1);")

        Dim stringobj As String = "id"
        For i As Int32 = 1 To cols.Count - 1
            If stringobj = "" Then
                stringobj &= cols(i)
            Else
                stringobj &= "," & cols(i)
            End If
            objWriter.WriteLine(types(i) & " " & cols(i) & " = list" & nomUpperClasse & ".get" & types(i).Substring(0, 1).ToUpper() & types(i).Substring(1, types(i).Length - 1) & "(" & i + 1 & ");")
        Next

        objWriter.WriteLine("obj = new " & nomUpperClasse & "(" & stringobj & ");")
        objWriter.WriteLine("liste.add(obj);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("return liste; " & Chr(13) & _
                        "}")
        objWriter.WriteLine()
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub
#End Region

#Region "Android Class Fonctions"
    Public Shared Sub CreateAndroidModel(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\model\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\modele\")
        Dim path As String = txt_PathGenerate_Script & nomSimple & ".java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        objWriter.WriteLine("package ht.solutions.android." & txt_projectName.Text & ".modele;")
        objWriter.WriteLine("import com.google.gson.annotations.Expose;")
        objWriter.WriteLine("import com.google.gson.annotations.SerializedName;")
        objWriter.WriteLine()
        objWriter.WriteLine("import java.util.*;")
        objWriter.WriteLine(content)
        objWriter.WriteLine()

        'cols.Add("LocalId")
        'cols.Add("isSync")
        'types.Add("long")
        'types.Add("boolean")
        'initialtypes.Add("Bigint")
        'initialtypes.Add("bit")
        objWriter.WriteLine("//region Attribut")

        objWriter.WriteLine()
        Try
            For i As Int32 = 0 To cols.Count - 1
                If i = 0 Then
                    objWriter.WriteLine("@SerializedName(""" & cols(i) & """)")
                    objWriter.WriteLine("@Expose")
                    objWriter.WriteLine("private long id;")
                    objWriter.WriteLine()
                ElseIf (i = cols.Count - 1) Or (i = cols.Count - 2) Then
                    objWriter.WriteLine("private " & types(i) & " " & cols(i) & ";")
                    objWriter.WriteLine()
                Else
                    objWriter.WriteLine("@SerializedName(""" & cols(i) & """)")
                    objWriter.WriteLine("@Expose")
                    objWriter.WriteLine("private " & types(i) & " " & cols(i) & ";")
                    If initialtypes(i) = "image" Then
                        objWriter.WriteLine("@SerializedName(""" & cols(i) & "String" & """)")
                        objWriter.WriteLine("@Expose")
                        objWriter.WriteLine("private String " & cols(i) & "String;" & "")
                    End If
                    If ListofForeignKey.Contains(cols(i)) Then
                        objWriter.WriteLine("private " & cols(i).Substring(3, cols(i).Length - 3) & " " & cols(i).Substring(3, cols(i).Length - 3).ToLower & ";")
                    End If
                    objWriter.WriteLine()
                End If

            Next
            objWriter.WriteLine("private long localId;")
            objWriter.WriteLine("private boolean isSync;")

            objWriter.WriteLine("private String MacTabletteCreated;")
            objWriter.WriteLine("private String MacTabletteModif;")
            objWriter.WriteLine("private String DateCreated;")
            objWriter.WriteLine("private String DateModif;")
        Catch ex As Exception

        End Try
        objWriter.WriteLine()
        objWriter.WriteLine("//endregion")
        objWriter.WriteLine()

        '''''''''''''''''''''''''''''''''''''''''properties''''''''''''''''''''''''''''''''''''

        objWriter.WriteLine("//region Properties")

        objWriter.WriteLine("public long getId() {")
        objWriter.WriteLine("return id;")
        objWriter.WriteLine("}")
        objWriter.WriteLine()
        objWriter.WriteLine("  public void setId(long id) {")
        objWriter.WriteLine("this.id = id;")
        objWriter.WriteLine("}")

        For i As Int32 = 1 To cols.Count - 2 ''On ne cree pas de property pour la derniere column
            Dim propName As String = ""
            Dim s As String() = cols(i).Split("_")
            For j As Integer = 1 To s.Length - 1
                propName &= StrConv(s(j), VbStrConv.ProperCase)
            Next

            Dim attrib As String = "public " & types(i) & " get" & cols(i) & "() {"
            Dim setter As String = "public void " & "set" & cols(i) & "(" & types(i) & " " & cols(i).ToLower() & ") {"

            If cols(i) <> "_isdirty" Or cols(i) <> "_LogData" Then
                '   objWriter.WriteLine(log)

                objWriter.WriteLine()
                objWriter.WriteLine(attrib)
                objWriter.WriteLine("return " & cols(i) & ";")
                objWriter.WriteLine("}")
                objWriter.WriteLine(setter)
                objWriter.WriteLine("this." & cols(i) & " = " & cols(i).ToLower & ";")
                objWriter.WriteLine("}")

                If initialtypes(i) = "image" Then
                    objWriter.WriteLine()
                    objWriter.WriteLine("public boolean hasImage() {")
                    objWriter.WriteLine("return (this." & cols(i) & "String != null && this." & cols(i) & "String.length() > 0 || this." & cols(i) & "!= null && this." & cols(i) & ".length > 0);")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("public String get" & cols(i) & "String() {")
                    objWriter.WriteLine("return " & cols(i).ToLower & "String;")
                    objWriter.WriteLine("}")

                    objWriter.WriteLine("public void set" & cols(i) & "String(String " & cols(i).ToLower & "String) {")
                    objWriter.WriteLine("this." & cols(i) & "String = " & cols(i).ToLower & "String;")
                    objWriter.WriteLine("}")


                End If

                If ListofForeignKey.Contains(cols(i)) Then

                    Dim ClassName As String = cols(i).Substring(3, cols(i).Length - 3)
                    Dim attributUsed As String = ClassName.ToLower()
                    '  objWriter.WriteLine("public cols(i).Substring(3, cols(i).Length - 3) & Chr(13))
                    objWriter.WriteLine("public " & ClassName & " get" & ClassName & "() {")
                    objWriter.WriteLine("if (" & attributUsed & "==null)")
                    objWriter.WriteLine(attributUsed & " = " & ClassName & "Helper.searchById(" & cols(i) & ");")
                    objWriter.WriteLine("return " & attributUsed & ";")
                    objWriter.WriteLine("}")

                    objWriter.WriteLine("public void set" & ClassName & "(" & ClassName & " " & attributUsed & ") {")
                    objWriter.WriteLine("this." & attributUsed & " = " & attributUsed & ";")
                    objWriter.WriteLine("}")

                End If
            End If

        Next


        objWriter.WriteLine("public long getLocalId() {")
        objWriter.WriteLine("return localId;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public void setLocalId(long localId) {")
        objWriter.WriteLine("this.localId = localId;")
        objWriter.WriteLine("}")


        objWriter.WriteLine("public boolean isSync() {")
        objWriter.WriteLine("return isSync;")
        objWriter.WriteLine("}")


        objWriter.WriteLine("public void setSync(boolean sync) {")
        objWriter.WriteLine("isSync = sync;")
        objWriter.WriteLine("}")


        objWriter.WriteLine(" public String getMacTabletteCreated() {")
        objWriter.WriteLine("return MacTabletteCreated;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public void setMacTabletteCreated(String macTabletteCreated) {")
        objWriter.WriteLine("MacTabletteCreated = macTabletteCreated;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public String getMacTabletteModif() {")
        objWriter.WriteLine("return MacTabletteModif;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public void setMacTabletteModif(String macTabletteModif) {")
        objWriter.WriteLine("MacTabletteModif = macTabletteModif;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public String getDateCreated() {")
        objWriter.WriteLine("return DateCreated;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public void setDateCreated(String dateCreated) {")
        objWriter.WriteLine("DateCreated = dateCreated;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public String getDateModif() {")
        objWriter.WriteLine("return DateModif;")
        objWriter.WriteLine("}")

        objWriter.WriteLine(" public void setDateModif(String dateModif) {")
        objWriter.WriteLine("DateModif = dateModif;")
        objWriter.WriteLine("}")



        objWriter.WriteLine("//endregion")

        objWriter.WriteLine()

        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()

    End Sub

    Public Shared Sub CreateAndroidHelper(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\helper\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\helper\")
        Dim path As String = txt_PathGenerate_Script & nomSimple & "Helper.java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & "Helper {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        objWriter.WriteLine("package ht.solutions.android." & txt_projectName.Text.Trim & ".helper;")

        objWriter.WriteLine("import android.content.ContentValues;")
        objWriter.WriteLine("import android.database.Cursor;")
        objWriter.WriteLine("import android.database.sqlite.SQLiteDatabase;")
        objWriter.WriteLine("import ht.agrotracking.mobile.DbHelper.Currentdb;")
        objWriter.WriteLine("import ht.agrotracking.mobile.DbHelper.DBConstants;")
        objWriter.WriteLine("import ht.agrotracking.mobile.model." & nomSimple & ";")

        objWriter.WriteLine("import java.util.*;")
        objWriter.WriteLine(content)
        objWriter.WriteLine()


        'cols.Add("LocalId")
        'cols.Add("isSync")
        'types.Add("long")
        'types.Add("boolean")
        'initialtypes.Add("Bigint")
        'initialtypes.Add("bit")


        With objWriter
            .WriteLine("  public static int save(" & nomSimple & " " & "obj" & ") throws Exception { ")

            .WriteLine("  Currentdb currentdb = new Currentdb();")
            .WriteLine("  currentdb.open();")
            .WriteLine("  SQLiteDatabase database = currentdb.getDb();")
            .WriteLine("  int result = 0;")
            .WriteLine("  try {")
            .WriteLine("       ContentValues newTaskValue = new ContentValues();")
            .WriteLine("       newTaskValue.put(DBConstants." & nomSimple.ToUpper & "_" & "ID" & ", " & "obj" & ".getId());")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    With objWriter
                        If initialtypes(i) <> "date" Then
                            .WriteLine("       newTaskValue.put(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & ", " & "obj" & ".get" & cols(i) & "());")
                        ElseIf initialtypes(i) = "bit" Then
                            .WriteLine("       newTaskValue.put(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & ", " & "obj" & ".is" & cols(i) & "());")
                        Else
                            .WriteLine("       newTaskValue.put(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & ", " & "obj" & ".get" & cols(i) & "();")
                        End If
                    End With
                Next
            Catch

            End Try

            .WriteLine("       newTaskValue.put(DBConstants.IS_SYNCHRONIZED, obj.IsSync());")
            .WriteLine("      newTaskValue.put(DBConstants.MAC_TABLETTE_CREATED, obj.getMacTabletteCreated());")
            .WriteLine("      newTaskValue.put(DBConstants.MAC_TABLETTE_MODIFIED, obj.getMacTabletteModif());")
            .WriteLine("       newTaskValue.put(DBConstants.DATE_CREATED, obj.getDateCreated());")
            .WriteLine("       newTaskValue.put(DBConstants.DATE_MODIF, obj.getDateModif());")


            .WriteLine("      if (searchById(" & "obj" & ".getId())==null)")
            .WriteLine("     {")
            .WriteLine("             result = (database.insert(DBConstants." & nomSimple.ToUpper & "_TABLE" & ", null, newTaskValue) > 0 ? 1: 0);")
            .WriteLine("     }  else{")
            .WriteLine("           database.update(DBConstants." & nomSimple.ToUpper & "_TABLE" & ", newTaskValue, DBConstants." & nomSimple.ToUpper & "_ID + ""=?"", new String[]{String.valueOf(" & "obj" & ".getId())});")
            .WriteLine("     }")
            .WriteLine("      return result;")
            .WriteLine("   } finally {")
            .WriteLine("        currentdb.close();")
            .WriteLine("   }")
            '  //return result;
            .WriteLine(" }")
            .WriteLine()
        End With

        With objWriter
            .WriteLine(" private static " & nomSimple & " setProperties(Cursor c) {")
            .WriteLine(" " & nomSimple & " " & "obj" & ";")
            .WriteLine("  " & "obj" & " = new " & nomSimple & "();")
            .WriteLine("  " & "obj" & ".setId(c.get" & "Int" & "(c.getColumnIndex(DBConstants." & nomSimple.ToUpper & "_" & "ID" & ")));")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    With objWriter
                        If initialtypes(i) <> "date" Then
                            .WriteLine("  " & "obj" & ".set" & cols(i) & "(c.get" & ConvertJavaToSetPropertiesAndroidType(types(i)) & "(c.getColumnIndex(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & ")));")
                        ElseIf initialtypes(i) = "bit" Then
                            .WriteLine("  " & "obj" & ".set" & cols(i) & "(Boolean.parseBoolean(c.get" & "Int" & "(c.getColumnIndex(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & ")) + """" ));")
                        Else
                            .WriteLine("  " & "obj" & ".set" & cols(i) & "(c.get" & ConvertJavaToSetPropertiesAndroidType(types(i)) & "(c.getColumnIndex(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & "))  );")
                        End If
                    End With
                Next
            Catch
            End Try
            .WriteLine(" obj.setLocalId(c.getLong(c.getColumnIndex(DBConstants.KEY_ID)));")
            .WriteLine("obj.setSync(Boolean.parseBoolean(c.getInt(c.getColumnIndex(DBConstants.IS_SYNCHRONIZED))+""));")
            .WriteLine("obj.setMacTabletteCreated(c.getString(c.getColumnIndex(DBConstants.MAC_TABLETTE_CREATED)));")
            .WriteLine("obj.setMacTabletteModif(c.getString(c.getColumnIndex(DBConstants.MAC_TABLETTE_MODIFIED)));")
            .WriteLine("obj.setDateCreated(c.getString(c.getColumnIndex(DBConstants.DATE_CREATED)));")
            .WriteLine("obj.setDateModif(c.getString(c.getColumnIndex(DBConstants.DATE_MODIF)));")
            .WriteLine("  return " & "obj" & ";")
            .WriteLine("}")
            .WriteLine()
        End With



        objWriter.WriteLine("public static ArrayList<" & nomSimple & "> SearchAll( ) {")
        objWriter.WriteLine("  ArrayList<" & nomSimple & "> " & "obj" & "s= new ArrayList<" & nomSimple & ">();")
        objWriter.WriteLine("  Currentdb currentdb = new Currentdb();")
        objWriter.WriteLine("  currentdb.open();")
        objWriter.WriteLine("" & nomSimple & " obj = null;")
        objWriter.WriteLine("  SQLiteDatabase database = currentdb.getDb();")
        objWriter.WriteLine("  Cursor c = database.query(DBConstants." & nomSimple.ToUpper & "_TABLE" & ", null, null,null, null, null, null);")
        objWriter.WriteLine("  while (c.moveToNext()) {")
        objWriter.WriteLine("      " & "obj" & " = setProperties(c);")
        objWriter.WriteLine("      " & "obj" & "s.add(" & "obj" & ");")
        objWriter.WriteLine("  }")
        objWriter.WriteLine("   c.close() ;")
        objWriter.WriteLine("   currentdb.close();")
        objWriter.WriteLine("   return " & "obj" & "s;")
        objWriter.WriteLine("}")
        objWriter.WriteLine()

        objWriter.WriteLine("public static " & nomSimple & " searchById(long id ) {")
        objWriter.WriteLine("  Currentdb currentdb = new Currentdb();")
        objWriter.WriteLine("  currentdb.open();")
        objWriter.WriteLine("  SQLiteDatabase database = currentdb.getDb();")
        objWriter.WriteLine("  String filter = DBConstants." & nomSimple.ToUpper & "_ID" & " + "" =?"";")
        objWriter.WriteLine("  Cursor c = database.query(DBConstants." & nomSimple.ToUpper & "_TABLE" & ", null, filter, new String[] {String.valueOf(id)}, null, null, null);")
        objWriter.WriteLine("  " & nomSimple & " " & "obj" & "=null;")
        objWriter.WriteLine("  if (c.moveToNext()){")
        objWriter.WriteLine("       " & "obj" & " = setProperties(c);")
        objWriter.WriteLine("   }")
        objWriter.WriteLine("  c.close() ;")
        objWriter.WriteLine("  currentdb.close();")
        objWriter.WriteLine("  return " & "obj" & ";")
        objWriter.WriteLine("}")
        objWriter.WriteLine()
        objWriter.WriteLine("//endregion")

        objWriter.WriteLine()

        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroiDBHelper(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\")
        Dim path As String = txt_PathGenerate_Script & nomSimple & "DBConstantsHelperScript.java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        'objWriter.WriteLine("package ht.solutions.android." & txt_projectName.Text & ".modele;")
        'objWriter.WriteLine("import com.google.gson.annotations.Expose;")
        'objWriter.WriteLine("import com.google.gson.annotations.SerializedName;")
        'objWriter.WriteLine()
        'objWriter.WriteLine("import java.util.*;")
        '   objWriter.WriteLine(content)
        objWriter.WriteLine()


        'cols.Add("localId")
        'cols.Add("isSync")
        'types.Add("long")
        'types.Add("boolean")
        'initialtypes.Add("Byte")
        'initialtypes.Add("nvarchar")

        With objWriter
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_TABLE = ""tbl_" & nomSimple.ToLower & """;")
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID = """ & nomSimple.ToLower & "_id"";")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("  public static final String " & nomSimple.ToUpper & "_" & cols(i).ToUpper & " = """ & nomSimple.ToLower & "_" & cols(i).ToLower & """;")
                Next
            Catch
            End Try
        End With

        With objWriter
            .WriteLine()
            .WriteLine("  public static final String CREATE_TABLE_" & nomSimple.ToUpper & " = ""create table "" +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_TABLE + "" ("" +")
            .WriteLine("DBConstants.KEY_ID + "" integer primary key autoincrement, "" +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_ID + "" integer not null, "" +")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    If types(i) = "String" Or types(i) = "Date" Or types(i) = "DateTime" Then
                        .WriteLine("DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & " + "" " & "text" & " not null, "" +")
                    Else
                        .WriteLine("DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & " + "" " & types(i) & " not null, "" +")
                    End If
                Next
                .WriteLine(" DBConstants.IS_SYNCHRONIZED + "" boolean not null default 0, "" +")
                .WriteLine(" DBConstants.MAC_TABLETTE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.MAC_TABLETTE_MODIFIED + "" text, "" +")
                .WriteLine(" DBConstants.DATE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.DATE_MODIF + "" text, "" +")
                .WriteLine(" DBConstants.DATE_NAME + "" long);"";")
            Catch
            End Try
        End With
        objWriter.WriteLine()
        objWriter.WriteLine(" db.execSQL(DBScript.CREATE_TABLE_" & nomSimple.ToUpper & ");")
        objWriter.WriteLine(" Log.v(""MyDBhelper onCreate"", ""Created Table "" + DBConstants." & nomSimple.ToUpper & "_TABLE);")

        objWriter.WriteLine()
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidBinderListview(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\adaptor\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\adaptor\")
        Dim path As String = txt_PathGenerate_Script & "Binder" & nomSimple & ".java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine()





        objWriter.WriteLine("  package ht.mesi.mobile.adaptor;")

        objWriter.WriteLine("import android.app.Activity;")
        objWriter.WriteLine("import android.content.Context;")
        objWriter.WriteLine("import android.view.LayoutInflater;")
        objWriter.WriteLine("import android.view.View;")
        objWriter.WriteLine("import android.view.ViewGroup;")
        objWriter.WriteLine("import android.widget.BaseAdapter;")
        objWriter.WriteLine("import android.widget.ImageView;")
        objWriter.WriteLine("import android.widget.TextView;")
        objWriter.WriteLine("import ht.mesi.mobile.activity.R;")

        objWriter.WriteLine("import java.util.HashMap;")
        objWriter.WriteLine("import java.util.List;")

        objWriter.WriteLine("/**")
        objWriter.WriteLine("* Created with IntelliJ IDEA.")
        objWriter.WriteLine("* User: JSMINFINI")
        objWriter.WriteLine("* Date: 1/27/14")
        objWriter.WriteLine("* Time: 3:54 PM")
        objWriter.WriteLine("* To change this template use File | Settings | File Templates.")
        objWriter.WriteLine("*/")
        objWriter.WriteLine("public class Binder" & nomSimple & " extends BaseAdapter {")


        objWriter.WriteLine("LayoutInflater inflater;")
        objWriter.WriteLine("ImageView thumb_image;")
        objWriter.WriteLine("List<HashMap<String,String>> " & nomSimple.ToLower & "DataCollection;")
        objWriter.WriteLine("ViewHolder holder;")
        objWriter.WriteLine("Activity _activity ;")

        objWriter.WriteLine("public Binder" & nomSimple & "() {")
        objWriter.WriteLine("// TODO Auto-generated constructor stub")
        objWriter.WriteLine("}")


        objWriter.WriteLine("public Binder" & nomSimple & "(Activity act, List<HashMap<String,String>> map) {")

        objWriter.WriteLine("this." & nomSimple.ToLower & "DataCollection = map;")
        objWriter.WriteLine("this._activity = act;")

        objWriter.WriteLine("inflater = (LayoutInflater) act")
        objWriter.WriteLine(".getSystemService(Context.LAYOUT_INFLATER_SERVICE);")
        objWriter.WriteLine("}")


        objWriter.WriteLine("public int getCount() {")
        objWriter.WriteLine("// TODO Auto-generated method stub")
        objWriter.WriteLine("//		return idlist.size();")
        objWriter.WriteLine("return " & nomSimple.ToLower & "DataCollection.size();")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public Object getItem(int arg0) {")
        objWriter.WriteLine("// TODO Auto-generated method stub")
        objWriter.WriteLine("return null;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public long getItemId(int position) {")
        objWriter.WriteLine("// TODO Auto-generated method stub")
        objWriter.WriteLine("return 0;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public View getView(int position, View convertView, ViewGroup parent) {")

        objWriter.WriteLine("View vi=convertView;")
        objWriter.WriteLine("if(convertView==null){")

        objWriter.WriteLine("vi = inflater.inflate(R.layout.list_row_" & nomSimple.ToLower & ", null);")
        objWriter.WriteLine("holder = new ViewHolder();")


        With objWriter
            Dim val As String = ""
            val = " holder.tv" & nomSimple & "Id" & nomSimple & " = (TextView)vi.findViewById(R.id.tv" & nomSimple & "Id" & nomSimple & "); "

            .WriteLine(val)
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("holder.tv" & nomSimple & "" & cols(i) & " = (TextView)vi.findViewById(R.id.tv" & nomSimple & "" & cols(i) & "); ")
                Next
            Catch
            End Try
        End With

        With objWriter
            .WriteLine("holder.tv" & nomSimple & "Id" & nomSimple & ".setText(" & nomSimple & "DataCollection.get(position).get(DBConstants." & nomSimple.ToUpper & "_ID_" & nomSimple.ToUpper & "));")

            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("holder.tv" & nomSimple & "" & cols(i) & ".setText(" & nomSimple & "DataCollection.get(position).get(DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & "));")
                Next
            Catch
            End Try
        End With


        objWriter.WriteLine("vi.setTag(holder);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("else{")

        objWriter.WriteLine("holder = (ViewHolder)vi.getTag();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("if (position % 2 == 1) {")
        objWriter.WriteLine("vi.setBackgroundColor(_activity.getResources().getColor(R.color.grid_alt_row_color));")
        objWriter.WriteLine("} else {")
        objWriter.WriteLine("vi.setBackgroundColor(_activity.getResources().getColor(R.color.grid_row_color));")
        objWriter.WriteLine("}")



        objWriter.WriteLine("return vi;")
        objWriter.WriteLine("    }")


        objWriter.WriteLine("static class ViewHolder{")


        With objWriter
            .WriteLine("        TextView tv" & nomSimple & "Id;")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("        TextView tv" & nomSimple & "" & cols(i) & ";")
                Next
            Catch
            End Try
        End With

        objWriter.WriteLine("}")
        objWriter.WriteLine("}")

        objWriter.WriteLine()
        objWriter.WriteLine()

        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidListViewActivity(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\")
        Dim path As String = txt_PathGenerate_Script & nomSimple & "DBConstantsHelperScript.java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        'objWriter.WriteLine("package ht.solutions.android." & txt_projectName.Text & ".modele;")
        'objWriter.WriteLine("import com.google.gson.annotations.Expose;")
        'objWriter.WriteLine("import com.google.gson.annotations.SerializedName;")
        'objWriter.WriteLine()
        'objWriter.WriteLine("import java.util.*;")
        '   objWriter.WriteLine(content)
        objWriter.WriteLine()


        cols.Add("localId")
        cols.Add("isSync")
        types.Add("long")
        types.Add("boolean")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")



        objWriter.WriteLine("package ht.mesi.mobile.activity;")

        objWriter.WriteLine("import android.app.*;")
        objWriter.WriteLine("import android.content.Context;")
        objWriter.WriteLine("import android.content.DialogInterface;")
        objWriter.WriteLine("import android.content.Intent;")
        objWriter.WriteLine("import android.content.SharedPreferences;")
        objWriter.WriteLine("import android.os.AsyncTask;")
        objWriter.WriteLine("import android.os.Bundle;")
        objWriter.WriteLine("import android.preference.PreferenceManager;")
        objWriter.WriteLine("import android.util.Log;")
        objWriter.WriteLine("import android.view.*;")
        objWriter.WriteLine("import android.widget.*;")
        objWriter.WriteLine("import ht.mesi.mobile.DbHelper.CurrentDB;")
        objWriter.WriteLine("import ht.mesi.mobile.DbHelper.DBConstants;")
        objWriter.WriteLine("import ht.mesi.mobile.adaptor.BinderQuestionnaire;")
        objWriter.WriteLine("import ht.mesi.mobile.global.Utils;")
        objWriter.WriteLine("import ht.mesi.mobile.helper.*;")
        objWriter.WriteLine("import ht.mesi.mobile.model.*;")
        objWriter.WriteLine("import ht.mesi.mobile.service.SynchroData;")

        objWriter.WriteLine("import java.util.ArrayList;")
        objWriter.WriteLine("import java.util.HashMap;")
        objWriter.WriteLine("import java.util.List;")

        objWriter.WriteLine("/**")
        objWriter.WriteLine("* Created by DIMITRY DABADY on 2/19/14.")
        objWriter.WriteLine("*/")
        objWriter.WriteLine("public class ListQuestionnaireActivity extends Activity implements ActionBar.OnNavigationListener {")
        objWriter.WriteLine("Context context;")
        objWriter.WriteLine("ListView listview_questionnaire;")
        objWriter.WriteLine("List<HashMap<String, String>> questionnaireDataCollection;")
        objWriter.WriteLine("ArrayList<Institution> listInstitution;")
        objWriter.WriteLine("ArrayList<Mois> listMois;")
        objWriter.WriteLine("ArrayList<ht.mesi.mobile.model.Annee> listAnnee;")
        objWriter.WriteLine("Spinner spinnerInstitution;")
        objWriter.WriteLine("Spinner spinnerMois;")
        objWriter.WriteLine("Spinner spinnerAnnee;")
        objWriter.WriteLine("Spinner spinnerStatut;")
        objWriter.WriteLine("long id_Institution = 0;")
        objWriter.WriteLine("int id_Mois = 0;")
        objWriter.WriteLine("long Annee = 0;")
        objWriter.WriteLine("String statut = GlobalVariables.KEY_QUESTIONNAIRE_EN_COURS;")
        objWriter.WriteLine("String _message;")
        objWriter.WriteLine("private int SYNCHO = 1480;")
        objWriter.WriteLine("private int NOTIFICATION = 1478;")

        objWriter.WriteLine("    LayoutInflater inflater;")
        objWriter.WriteLine("private ActionBar actionBar;")
        objWriter.WriteLine("private ProgressDialog progress;")
        objWriter.WriteLine("private SessionManager session;")
        objWriter.WriteLine("private UserModel currentUser;")
        objWriter.WriteLine("private Menu _menu = null;")

        objWriter.WriteLine("    long idformulaire = 0;")
        objWriter.WriteLine("Button btn_prev_question;")
        objWriter.WriteLine("Button btn_next_question;")

        objWriter.WriteLine("int lastMenuActionClicked;")
        objWriter.WriteLine("TextView label_connectedUser;")

        objWriter.WriteLine("    @Override")
        objWriter.WriteLine("public void onCreate(Bundle savedInstanceState) {")
        objWriter.WriteLine("super.onCreate(savedInstanceState);")
        objWriter.WriteLine("getWindow().requestFeature(Window.FEATURE_ACTION_BAR);")
        objWriter.WriteLine("setContentView(R.layout.list_questionnaire_layout);")
        objWriter.WriteLine("context = this;")
        objWriter.WriteLine("progress = new ProgressDialog(this);")
        objWriter.WriteLine("Intent i = getIntent();")
        objWriter.WriteLine("idformulaire = Long.parseLong(i.getStringExtra(GlobalVariables.KEY_INTENT_ID_FORMULAIRE));")

        objWriter.WriteLine("ActionBarCall();")
        objWriter.WriteLine("//Code a tester")
        objWriter.WriteLine("inflater = (LayoutInflater) this.getSystemService(Context.LAYOUT_INFLATER_SERVICE);")
        objWriter.WriteLine("View vi = inflater.inflate(R.layout.list_questionnaire_layout, null);")
        objWriter.WriteLine("//code a tester")
        objWriter.WriteLine("session = new SessionManager(this);")

        objWriter.WriteLine("/**")
        objWriter.WriteLine("* Call this function whenever you want to check user login")
        objWriter.WriteLine("* This will redirect user to LoginActivity is he is not")
        objWriter.WriteLine("* logged in")
        objWriter.WriteLine("* */")
        objWriter.WriteLine("currentUser = session.getUserDetails();")
        objWriter.WriteLine("Utils.PopulateMoisData();")
        objWriter.WriteLine("Utils.PopulateAnneeData();")
        objWriter.WriteLine("")
        objWriter.WriteLine("        try {")
        objWriter.WriteLine("questionnaireDataCollection = new ArrayList<HashMap<String, String>>();")
        objWriter.WriteLine("")
        objWriter.WriteLine("            listInstitution = InstitutionHelper.SearchAllTypeDroit(Integer.parseInt(String.valueOf(currentUser.getTypeDroit())), currentUser.getIdDroit());")
        objWriter.WriteLine("listMois = MoisHelper.SearchAll();")
        objWriter.WriteLine("listAnnee = AnneeHelper.SearchAll();")
        objWriter.WriteLine("")
        objWriter.WriteLine("            listview_questionnaire = (ListView) findViewById(R.id.listview_questionnaire);")

        objWriter.WriteLine("spinnerInstitution = (Spinner) findViewById(R.id.spinner_Institution);")
        objWriter.WriteLine("spinnerMois = (Spinner) findViewById(R.id.spinner_Mois);")
        objWriter.WriteLine("spinnerAnnee = (Spinner) findViewById(R.id.spinner_Annee);")
        objWriter.WriteLine("spinnerStatut = (Spinner) findViewById(R.id.spinner_Statut);")
        objWriter.WriteLine("")
        objWriter.WriteLine("label_connectedUser = (TextView) findViewById(R.id.label_connectedUser);")
        objWriter.WriteLine("label_connectedUser.setText(""Utilisateur connecté : "" + currentUser.toString());")
        objWriter.WriteLine("if (listInstitution != null) {")
        objWriter.WriteLine("Institution inst = new Institution();")
        objWriter.WriteLine("inst.setNomInstitution(""Filtrer par institution"");")
        objWriter.WriteLine("listInstitution.add(0, inst);")
        objWriter.WriteLine("final ArrayAdapter<Institution> adapterInstitution = new ArrayAdapter<Institution>(ListQuestionnaireActivity.this,")
        objWriter.WriteLine("android.R.layout.simple_spinner_item, listInstitution);")
        objWriter.WriteLine("adapterInstitution.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
        objWriter.WriteLine("spinnerInstitution.setAdapter(adapterInstitution); // Set the custom adapter to the spinner")
        objWriter.WriteLine("id_Institution = adapterInstitution.getItem(0).getId_Institution();")
        objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
        objWriter.WriteLine("spinnerInstitution.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
        objWriter.WriteLine("int position, long id) {")
        objWriter.WriteLine("id_Institution = adapterInstitution.getItem(position).getId_Institution();")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("                    @Override")
        objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("if (listMois != null) {")
        objWriter.WriteLine("Mois m = new Mois();")
        objWriter.WriteLine("m.setMois(""Filtrer par mois"");")
        objWriter.WriteLine("listMois.add(0, m);")
        objWriter.WriteLine("final ArrayAdapter<Mois> adapterMois = new ArrayAdapter<Mois>(ListQuestionnaireActivity.this,")
        objWriter.WriteLine("android.R.layout.simple_spinner_item,")
        objWriter.WriteLine("listMois);")
        objWriter.WriteLine("adapterMois.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
        objWriter.WriteLine("spinnerMois.setAdapter(adapterMois); // Set the custom adapter to the spinner")
        objWriter.WriteLine("id_Mois = adapterMois.getItem(0).getId_Mois();")
        objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
        objWriter.WriteLine("spinnerMois.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
        objWriter.WriteLine("int position, long id) {")
        objWriter.WriteLine("id_Mois = adapterMois.getItem(position).getId_Mois();")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("//                        new LoginTask().execute(1);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("                    @Override")
        objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("            if (listAnnee != null) {")
        objWriter.WriteLine("Annee a = new Annee();")
        objWriter.WriteLine("a.setAnnee(""Filtrer par année"");")
        objWriter.WriteLine("listAnnee.add(0, a);")
        objWriter.WriteLine("final ArrayAdapter<Annee> adapterAnnee = new ArrayAdapter<Annee>(ListQuestionnaireActivity.this, android.R.layout.simple_spinner_item, listAnnee);")
        objWriter.WriteLine("adapterAnnee.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
        objWriter.WriteLine("spinnerAnnee.setAdapter(adapterAnnee); // Set the custom adapter to the spinner")
        objWriter.WriteLine("Annee = Long.parseLong(""0"");")
        objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
        objWriter.WriteLine("spinnerAnnee.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
        objWriter.WriteLine("int position, long id) {")
        objWriter.WriteLine("if (adapterAnnee.getItem(position).getAnnee() == ""Filtrer par année"") {")
        objWriter.WriteLine("Annee = Long.parseLong(""0"");")
        objWriter.WriteLine("} else {")
        objWriter.WriteLine("Annee = Long.parseLong(adapterAnnee.getItem(position).getAnnee());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("                        BindListView();")
        objWriter.WriteLine("}")

        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("            String[] questionnaireStatut = getResources().getStringArray(R.array.questionnaire_statut);")
        objWriter.WriteLine("final ArrayAdapter<String> adapterStatut = new ArrayAdapter<String>(ListQuestionnaireActivity.this, android.R.layout.simple_spinner_item, questionnaireStatut);")
        objWriter.WriteLine("adapterStatut.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
        objWriter.WriteLine("spinnerStatut.setAdapter(adapterStatut);")
        objWriter.WriteLine("spinnerStatut.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
        objWriter.WriteLine("int position, long id) {")
        objWriter.WriteLine("statut = adapterStatut.getItem(position);")
        objWriter.WriteLine("ShowMenuActions(Questionnaire.StringToStatut(statut));")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("                @Override")
        objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("")
        objWriter.WriteLine("listview_questionnaire.setOnItemClickListener(new AdapterView.OnItemClickListener() {")
        objWriter.WriteLine("    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {")
        objWriter.WriteLine("Intent i = new Intent();")
        objWriter.WriteLine("i.setClass(context, ListQuestionActivity.class);")
        objWriter.WriteLine("long tmp_idInstitution = Long.parseLong(questionnaireDataCollection.get(position).get(DBConstants.QUESTIONNAIRE_ID_INSTITUTION));")
        objWriter.WriteLine("int tmp_Mois = Integer.parseInt(questionnaireDataCollection.get(position).get(DBConstants.QUESTIONNAIRE_MOIS));")
        objWriter.WriteLine("long tmp_Annee = Long.parseLong(questionnaireDataCollection.get(position).get(DBConstants.QUESTIONNAIRE_ANNEE));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ID_FORMULAIRE, String.valueOf(idformulaire));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ID_INSTITUTION, String.valueOf(tmp_idInstitution));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ID_MOIS, String.valueOf(tmp_Mois));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ANNEE, String.valueOf(tmp_Annee));")
        objWriter.WriteLine("startActivity(i);")
        objWriter.WriteLine("")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("} catch (Exception ex) {")
        objWriter.WriteLine("Log.e(""Error onCreate ListQuestionnaireActivity"", """" + ex.getMessage().toString());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onResume() {")
        objWriter.WriteLine("super.onResume();")
        objWriter.WriteLine("//        statut = GlobalVariables.KEY_QUESTIONNAIRE_EN_COURS;")
        objWriter.WriteLine("//        ShowMenuActions(Questionnaire.StringToStatut(statut));")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void ActionBarCall() {")
        objWriter.WriteLine("actionBar = getActionBar();")
        objWriter.WriteLine("// Hide the action bar title")
        objWriter.WriteLine("CharSequence activity_title_listequestionnaireactivity = getText(R.string.activity_title_listequestionnaireactivity);")
        objWriter.WriteLine("Formulaire form = FormulaireHelper.searchByID(idformulaire);")
        objWriter.WriteLine("actionBar.setTitle((activity_title_listequestionnaireactivity + "" "" + form.getDescription()).toString().toUpperCase());")
        objWriter.WriteLine("actionBar.setDisplayShowTitleEnabled(true);")
        objWriter.WriteLine("// Enabling Spinner dropdown navigation")
        objWriter.WriteLine("actionBar.setNavigationMode(ActionBar.NAVIGATION_MODE_STANDARD);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void BindListView() {")
        objWriter.WriteLine("progress.setMessage(this.getString(R.string.loading_data));")
        objWriter.WriteLine("progress.setProgressStyle(ProgressDialog.STYLE_SPINNER);")
        objWriter.WriteLine("progress.setIndeterminate(true);")
        objWriter.WriteLine("progress.show();")
        objWriter.WriteLine("progress.setProgress(0);")
        objWriter.WriteLine("")
        objWriter.WriteLine("ArrayList<Questionnaire> objs = QuestionnaireHelper.searchAllBy_Form_Inst_Mois_Annee_Statut_User(idformulaire, id_Institution, id_Mois, Annee, statut, currentUser);")
        objWriter.WriteLine("")
        objWriter.WriteLine("HashMap<String, String> map = null;")
        objWriter.WriteLine("questionnaireDataCollection.clear();")
        objWriter.WriteLine("for (Questionnaire obj : objs) {")
        objWriter.WriteLine("map = new HashMap<String, String>();")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_ID_QUESTIONNAIRE, String.valueOf(obj.getIdQuestionnaire()));")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_ID_FORMULAIRE, String.valueOf(obj.getIdFormulaire()));")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_ID_INSTITUTION, String.valueOf(obj.getIdInstitution()));")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_MOIS, String.valueOf(obj.getMois()));")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_ANNEE, String.valueOf(obj.getAnnee()));")
        objWriter.WriteLine("map.put(DBConstants.QUESTIONNAIRE_STATUT, String.valueOf(obj.getStatut()));")
        objWriter.WriteLine("")
        objWriter.WriteLine("questionnaireDataCollection.add(map);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("BinderQuestionnaire bindingData = new BinderQuestionnaire(this, questionnaireDataCollection, id_Institution, id_Mois, Annee);")
        objWriter.WriteLine("")
        objWriter.WriteLine("listview_questionnaire.setAdapter(null);")
        objWriter.WriteLine("listview_questionnaire.setAdapter(bindingData);")
        objWriter.WriteLine("progress.setProgress(100);")
        objWriter.WriteLine("progress.dismiss();")
        objWriter.WriteLine("")
        objWriter.WriteLine("if (objs == null || objs.size() == 0) {")
        objWriter.WriteLine("_message = ""La recherhe ne ramène aucun rapport !"";")
        objWriter.WriteLine("MessageToShow();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public boolean onNavigationItemSelected(int itemPosition, long itemId) {")
        objWriter.WriteLine("return false;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public boolean onCreateOptionsMenu(Menu menu) {")
        objWriter.WriteLine("MenuInflater inflater = getMenuInflater();")
        objWriter.WriteLine("inflater.inflate(R.menu.menu_add_valid_return_sync, menu);")
        objWriter.WriteLine("_menu = menu;")
        objWriter.WriteLine("")
        objWriter.WriteLine("ShowMenuActions(Questionnaire.StringToStatut(statut));")
        objWriter.WriteLine("")
        objWriter.WriteLine("return super.onCreateOptionsMenu(menu);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void ShowMenuActions(Questionnaire.STATUS_QUESTIONNAIRE stat) {")
        objWriter.WriteLine("switch (stat) {")
        objWriter.WriteLine("case EN_COURS: case RETOURNE:")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, true, R.id.action_validate);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, false, R.id.action_return);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, false, R.id.action_synchronise);")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case TO_BE_SYNCHRONIZED:")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, false, R.id.action_validate);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, true, R.id.action_return);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, true, R.id.action_synchronise);")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case SYNCHRONIZED:")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, false, R.id.action_validate);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, true, R.id.action_return);")
        objWriter.WriteLine("ApplicationHelper.ShowOrHideMenuOption(_menu, false, R.id.action_synchronise);")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public boolean onOptionsItemSelected(MenuItem item) {")
        objWriter.WriteLine("// Take appropriate action for each action item click")
        objWriter.WriteLine("")
        objWriter.WriteLine("switch (item.getItemId()) {")
        objWriter.WriteLine("case android.R.id.home:")
        objWriter.WriteLine("ApplicationHelper.SafelyNavigateUpTo(this);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("")
        objWriter.WriteLine("case R.id.action_add:")
        objWriter.WriteLine("Intent i = new Intent();")
        objWriter.WriteLine("i.setClass(context, AddEditQuestionnaireActivity.class);")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ID_INSTITUTION, String.valueOf(id_Institution));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ID_FORMULAIRE, String.valueOf(idformulaire));")
        objWriter.WriteLine("                i.putExtra(GlobalVariables.KEY_INTENT_ID_MOIS, String.valueOf(id_Mois));")
        objWriter.WriteLine("i.putExtra(GlobalVariables.KEY_INTENT_ANNEE, String.valueOf(Annee));")
        objWriter.WriteLine("startActivity(i);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("")
        objWriter.WriteLine("case R.id.action_validate:")
        objWriter.WriteLine("DoConfirmAction(item.getItemId(), R.string.lb_questionnaire_validate_msg, R.string.lb_questionnaire_validate_title);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("")
        objWriter.WriteLine("case R.id.action_return:")
        objWriter.WriteLine("DoConfirmAction(item.getItemId(), R.string.lb_questionnaire_return_msg, R.string.lb_questionnaire_return_title);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("")
        objWriter.WriteLine("case R.id.action_synchronise:")
        objWriter.WriteLine("DoConfirmAction(item.getItemId(), R.string.lb_questionnaire_synchronize_msg, R.string.lb_questionnaire_synchronize_title);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("")
        objWriter.WriteLine("case R.id.action_disconnect:")
        objWriter.WriteLine("session = new SessionManager(this);")
        objWriter.WriteLine("session.logoutUser();")
        objWriter.WriteLine("finish();")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("default:")
        objWriter.WriteLine("return super.onOptionsItemSelected(item);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void DoConfirmAction(int menuActionClicked, int confirmMessageRes, int confirmTitleRes){")
        objWriter.WriteLine("lastMenuActionClicked = menuActionClicked;")
        objWriter.WriteLine("// 1. Instantiate an AlertDialog.Builder with its constructor")
        objWriter.WriteLine("AlertDialog.Builder builder = new AlertDialog.Builder(context);")
        objWriter.WriteLine("")
        objWriter.WriteLine("// 2. Chain together various setter methods to set the dialog characteristics")
        objWriter.WriteLine("builder.setMessage(confirmMessageRes).setTitle(confirmTitleRes);")
        objWriter.WriteLine("")
        objWriter.WriteLine("// 3. Add the buttons")
        objWriter.WriteLine("builder.setPositiveButton(R.string.lb_Yes, new DialogInterface.OnClickListener() {")
        objWriter.WriteLine("public void onClick(DialogInterface dialog, int id) {")
        objWriter.WriteLine("QuestionnaireActionTask _doQuestTask = new QuestionnaireActionTask(context);")
        objWriter.WriteLine("_doQuestTask.execute(lastMenuActionClicked);")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                // User clicked OK button")
        objWriter.WriteLine("//                progress.setProgressStyle(ProgressDialog.STYLE_SPINNER);")
        objWriter.WriteLine("//                progress.setIndeterminate(true);")
        objWriter.WriteLine("//                progress.setCancelable(false);")
        objWriter.WriteLine("//                switch (lastMenuActionClicked){")
        objWriter.WriteLine("//                    case R.id.action_validate :")
        objWriter.WriteLine("//                        progress.setMessage(getString(R.string.lb_questionnaire_validate_title));")
        objWriter.WriteLine("//                        break;")
        objWriter.WriteLine("//                    case R.id.action_return :")
        objWriter.WriteLine("//                        progress.setMessage(getString(R.string.lb_questionnaire_return_title));")
        objWriter.WriteLine("//                        break;")
        objWriter.WriteLine("//                    case R.id.action_synchronise :")
        objWriter.WriteLine("//                        progress.setMessage(getString(R.string.lb_questionnaire_synchronize_title));")
        objWriter.WriteLine("//                        break;")
        objWriter.WriteLine("//                }")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                progress.show();")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                final Thread t = new Thread(){")
        objWriter.WriteLine("//                    @Override")
        objWriter.WriteLine("//                    public void run(){")
        objWriter.WriteLine("//                        CurrentDB.context = context;")
        objWriter.WriteLine("//                        NotificationManager mNM;")
        objWriter.WriteLine("//                        SharedPreferences mgr;")
        objWriter.WriteLine("//                        mNM = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);")
        objWriter.WriteLine("//                        mgr = PreferenceManager.getDefaultSharedPreferences(context);")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                        switch (lastMenuActionClicked){")
        objWriter.WriteLine("//                            case R.id.action_validate :")
        objWriter.WriteLine("//                                if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.TO_BE_SYNCHRONIZED)){")
        objWriter.WriteLine("//                                    SynchroData.showNotificationSuccess(context, mNM, mgr, R.string.lb_questionnaire_validate_title, getString(R.string.lb_questionnaire_validate_result_msg));")
        objWriter.WriteLine("//                                    BindListView();")
        objWriter.WriteLine("//                                }")
        objWriter.WriteLine("//                                break;")
        objWriter.WriteLine("//                            case R.id.action_return :")
        objWriter.WriteLine("//                                if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.RETOURNE)){")
        objWriter.WriteLine("//                                    SynchroData.showNotificationSuccess(context, mNM, mgr, R.string.lb_questionnaire_return_title, getString(R.string.lb_questionnaire_return_result_msg));")
        objWriter.WriteLine("//                                    BindListView();")
        objWriter.WriteLine("//                                }")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                                break;")
        objWriter.WriteLine("//                            case R.id.action_synchronise :")
        objWriter.WriteLine("//                                if (ApplicationHelper.isNetworkAvailable(context)) {")
        objWriter.WriteLine("//                                    SynchroData.showNotificationSynchro(context, mNM);")
        objWriter.WriteLine("//                                    if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.SYNCHRONIZED)){")
        objWriter.WriteLine("//                                        SynchroData.showNotificationSuccess(context, mNM, mgr, R.string.lb_questionnaire_synchronize_title, getString(R.string.lb_questionnaire_synchronize_result_msg));")
        objWriter.WriteLine("//                                        BindListView();")
        objWriter.WriteLine("//                                    }")
        objWriter.WriteLine("//                                }")
        objWriter.WriteLine("//                                else{")
        objWriter.WriteLine("//                                   SynchroData.showNotificationError(context, mNM, mgr, R.string.no_network, getString(R.string.no_network_message));")
        objWriter.WriteLine("//                                }")
        objWriter.WriteLine("//")
        objWriter.WriteLine("//                                break;")
        objWriter.WriteLine("//                        }")
        objWriter.WriteLine("//                        progress.dismiss();")
        objWriter.WriteLine("//                        mNM.cancel(SYNCHO);")
        objWriter.WriteLine("//                    }")
        objWriter.WriteLine("//                };")
        objWriter.WriteLine("//                t.start();")
        objWriter.WriteLine("")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("")
        objWriter.WriteLine("builder.setNegativeButton(R.string.lb_Non, new DialogInterface.OnClickListener() {")
        objWriter.WriteLine("public void onClick(DialogInterface dialog, int id) {")
        objWriter.WriteLine("    // User cancelled the dialog")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("// 4. Set other dialog properties")
        objWriter.WriteLine("builder.setIcon(android.R.drawable.ic_dialog_alert);")
        objWriter.WriteLine("")
        objWriter.WriteLine("// 5. Get the AlertDialog from create()")
        objWriter.WriteLine("AlertDialog dialog = builder.create();")
        objWriter.WriteLine("dialog.show();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private boolean DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE newStatus) {")
        objWriter.WriteLine("boolean isStatusChanged = false;")
        objWriter.WriteLine("try{")
        objWriter.WriteLine("Adapter adapter = listview_questionnaire.getAdapter();")
        objWriter.WriteLine("")
        objWriter.WriteLine("ArrayList<Long> listQuestToChangeStatus = new ArrayList<Long>();")
        objWriter.WriteLine("boolean atLeastOneChecked = false;")
        objWriter.WriteLine("for (int i = 0; i < listview_questionnaire.getChildCount(); i++) {")
        objWriter.WriteLine("RelativeLayout listViewRow = (RelativeLayout) listview_questionnaire.getChildAt(i);")
        objWriter.WriteLine("CheckBox cbxSelect = (CheckBox) listViewRow.findViewById(R.id.cbxSelect);")
        objWriter.WriteLine("if (cbxSelect.isChecked()) {")
        objWriter.WriteLine("atLeastOneChecked = true;")
        objWriter.WriteLine("TextView tvStatut = (TextView) listViewRow.findViewById(R.id.tvStatut);")
        objWriter.WriteLine("switch (newStatus) {")
        objWriter.WriteLine("case RETOURNE:")
        objWriter.WriteLine("if (tvStatut.getText().toString().equals(GlobalVariables.KEY_QUESTIONNAIRE_TO_BE_SYNCHRONIZED) ||")
        objWriter.WriteLine("tvStatut.getText().toString().equals(GlobalVariables.KEY_QUESTIONNAIRE_SYNCHRONIZED)) {")
        objWriter.WriteLine("TextView tvIdQuestionnaire = (TextView) listViewRow.findViewById(R.id.tvIdQuestionnaire);")
        objWriter.WriteLine("listQuestToChangeStatus.add(Long.parseLong(tvIdQuestionnaire.getText().toString()));")
        objWriter.WriteLine("}")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case TO_BE_SYNCHRONIZED:")
        objWriter.WriteLine("if (tvStatut.getText().toString().equals(GlobalVariables.KEY_QUESTIONNAIRE_EN_COURS) ||")
        objWriter.WriteLine("tvStatut.getText().toString().equals(GlobalVariables.KEY_QUESTIONNAIRE_RETOURNE)) {")
        objWriter.WriteLine("TextView tvIdQuestionnaire = (TextView) listViewRow.findViewById(R.id.tvIdQuestionnaire);")
        objWriter.WriteLine("listQuestToChangeStatus.add(Long.parseLong(tvIdQuestionnaire.getText().toString()));")
        objWriter.WriteLine("}")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case SYNCHRONIZED:")
        objWriter.WriteLine("if(tvStatut.getText().toString().equals(GlobalVariables.KEY_QUESTIONNAIRE_TO_BE_SYNCHRONIZED)){")
        objWriter.WriteLine("TextView tvIdQuestionnaire = (TextView) listViewRow.findViewById(R.id.tvIdQuestionnaire);")
        objWriter.WriteLine("listQuestToChangeStatus.add(Long.parseLong(tvIdQuestionnaire.getText().toString()));")
        objWriter.WriteLine("}")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("if (atLeastOneChecked == false) {")
        objWriter.WriteLine("_message = ""Il faut cocher au moins un rapport !"";")
        objWriter.WriteLine("MessageToShow();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("else {")
        objWriter.WriteLine("if (listQuestToChangeStatus.size() > 0) {")
        objWriter.WriteLine("for (int i = 0; i < listQuestToChangeStatus.size(); i++) {")
        objWriter.WriteLine("try {")
        objWriter.WriteLine("if (newStatus.equals(Questionnaire.STATUS_QUESTIONNAIRE.SYNCHRONIZED)) {")
        objWriter.WriteLine("SynchroData.SynchronizeQuestionnaire(context, listQuestToChangeStatus.get(i));")
        objWriter.WriteLine("}")
        objWriter.WriteLine("QuestionnaireHelper.ChangeStatus(listQuestToChangeStatus.get(i), newStatus);")
        objWriter.WriteLine("isStatusChanged = true;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("catch(Exception e){")
        objWriter.WriteLine("if (e != null)")
        objWriter.WriteLine("Log.e(""Error ListQuestionnaireActivity.DoChangeStatusQuestionnaires"", """" + e.getMessage().toString());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("catch(Exception e){")
        objWriter.WriteLine("    if(e != null)")
        objWriter.WriteLine("Log.e(""Error ListQuestionnaireActivity.DoChangeStatusQuestionnaires"", """" + e.getMessage().toString());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("return isStatusChanged;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void MessageToShow() {")
        objWriter.WriteLine("Toast.makeText(getApplicationContext(), _message, Toast.LENGTH_LONG).show();")
        objWriter.WriteLine("//        AlertDialog.Builder alertbox = new AlertDialog.Builder(ListDetailQuestionCol1Activity.this);")
        objWriter.WriteLine("//        alertbox.setMessage(_message);")
        objWriter.WriteLine("//        alertbox.setNeutralButton(""Ok"", new DialogInterface.OnClickListener() {")
        objWriter.WriteLine("//            public void onClick(DialogInterface arg0, int arg1) {")
        objWriter.WriteLine("//                Toast.makeText(getApplicationContext(), _message  , Toast.LENGTH_LONG).show();")
        objWriter.WriteLine("//            }")
        objWriter.WriteLine("//        });")
        objWriter.WriteLine("//        alertbox.show();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("public class QuestionnaireActionTask extends AsyncTask<Integer, Void, Integer> {")
        objWriter.WriteLine("")
        objWriter.WriteLine("private Context _context;")
        objWriter.WriteLine("private String _msg;")
        objWriter.WriteLine("private SessionManager session;")
        objWriter.WriteLine("")
        objWriter.WriteLine("// Constructor")
        objWriter.WriteLine("public QuestionnaireActionTask(Context context){")
        objWriter.WriteLine("this._context = context;")
        objWriter.WriteLine("session = new SessionManager(context);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("// -- gets called just before thread begins")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("protected void onPreExecute() {")
        objWriter.WriteLine("super.onPreExecute();")
        objWriter.WriteLine("progress.setProgressStyle(ProgressDialog.STYLE_SPINNER);")
        objWriter.WriteLine("progress.setIndeterminate(true);")
        objWriter.WriteLine("progress.setCancelable(false);")
        objWriter.WriteLine("switch (lastMenuActionClicked){")
        objWriter.WriteLine("case R.id.action_validate :")
        objWriter.WriteLine("progress.setMessage(getString(R.string.lb_questionnaire_validate_title));")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case R.id.action_return :")
        objWriter.WriteLine("progress.setMessage(getString(R.string.lb_questionnaire_return_title));")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case R.id.action_synchronise :")
        objWriter.WriteLine("progress.setMessage(getString(R.string.lb_questionnaire_synchronize_title));")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("progress.show();")
        objWriter.WriteLine("progress.setProgress(20);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("// Do the long-running work in here")
        objWriter.WriteLine("protected Integer doInBackground(Integer... params) {")
        objWriter.WriteLine("int _result = 0;")
        objWriter.WriteLine("try {")
        objWriter.WriteLine("int clickedMenu = params[0];")
        objWriter.WriteLine("CurrentDB.context = context;")
        objWriter.WriteLine("switch (clickedMenu){")
        objWriter.WriteLine("case R.id.action_validate :")
        objWriter.WriteLine("if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.TO_BE_SYNCHRONIZED)){")
        objWriter.WriteLine("_result = 1;")
        objWriter.WriteLine("_msg = getString(R.string.lb_questionnaire_validate_result_msg);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("case R.id.action_return :")
        objWriter.WriteLine("    if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.RETOURNE)){")
        objWriter.WriteLine("_result = 1;")
        objWriter.WriteLine("                            _msg = getString(R.string.lb_questionnaire_return_result_msg);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("   break;")
        objWriter.WriteLine("case R.id.action_synchronise :")
        objWriter.WriteLine("if (ApplicationHelper.isNetworkAvailable(context)) {")
        objWriter.WriteLine("if(DoChangeStatusQuestionnaires(Questionnaire.STATUS_QUESTIONNAIRE.SYNCHRONIZED)){")
        objWriter.WriteLine("_result = 1;")
        objWriter.WriteLine("_msg = getString(R.string.lb_questionnaire_synchronize_result_msg);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("else{")
        objWriter.WriteLine("_result = -1;")
        objWriter.WriteLine("_msg = getString(R.string.no_network_message);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("}")
        objWriter.WriteLine("catch (Exception e) {")
        objWriter.WriteLine("if (e != null) {")
        objWriter.WriteLine("e.printStackTrace();")
        objWriter.WriteLine("_result = -1;")
        objWriter.WriteLine("_msg = ""Erreur : "" + e.getMessage();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("return _result;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("        protected void onPostExecute(Integer result) {")
        objWriter.WriteLine("switch (result) {")
        objWriter.WriteLine("case 1:")
        objWriter.WriteLine("BindListView();")
        objWriter.WriteLine("break;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("Toast.makeText(_context, _msg, Toast.LENGTH_LONG).show();")
        objWriter.WriteLine("progress.dismiss();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        With objWriter
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_TABLE = ""tbl_" & nomSimple.ToLower & """;")
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID = """ & nomSimple.ToLower & "_id"";")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID_" & cols(i).ToUpper & " = """ & nomSimple.ToLower & "_id_" & cols(i).ToLower & """;")
                Next
            Catch
            End Try
        End With

        With objWriter
            .WriteLine("  public static final String CREATE_TABLE_" & nomSimple.ToUpper & " = ""create table +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_TABLE + "" ("" +")
            .WriteLine("DBConstants.KEY_ID + "" integer primary key autoincrement, "" +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_ID + "" integer not null, "" +")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & "  "" " & types(i) & " not null, "" +")
                Next
                .WriteLine(" DBConstants.MAC_TABLETTE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.MAC_TABLETTE_MODIFIED + "" text, "" +")
                .WriteLine(" DBConstants.DATE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.DATE_MODIF + "" text, "" +")
                .WriteLine(" DBConstants.DATE_NAME + "" long);"";")
            Catch
            End Try
        End With

        objWriter.WriteLine(" db.execSQL(DBScript.CREATE_TABLE_" & nomSimple.ToUpper & ");")
        objWriter.WriteLine(" Log.v(""MyDBhelper onCreate"", ""Created Table "" + DBConstants." & nomSimple.ToUpper & "_TABLE);")

        objWriter.WriteLine()
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidListViewLayout(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\")
        Dim path As String = txt_PathGenerate_Script & "list_" & nomSimple.ToLower & "_layout.xml"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        'objWriter.WriteLine("package ht.solutions.android." & txt_projectName.Text & ".modele;")
        'objWriter.WriteLine("import com.google.gson.annotations.Expose;")
        'objWriter.WriteLine("import com.google.gson.annotations.SerializedName;")
        'objWriter.WriteLine()
        'objWriter.WriteLine("import java.util.*;")
        '   objWriter.WriteLine(content)
        objWriter.WriteLine()


        cols.Add("localId")
        cols.Add("isSync")
        types.Add("long")
        types.Add("boolean")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")


        objWriter.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
        objWriter.WriteLine("<LinearLayout xmlns:android=""http://schemas.android.com/apk/res/android""")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""fill_parent""")
        objWriter.WriteLine("android: paddingLeft = ""5dp""")
        objWriter.WriteLine("android: paddingTop = ""5dp""")
        objWriter.WriteLine("android: paddingRight = ""5dp""")
        objWriter.WriteLine("android: paddingBottom = ""5dp""")
        objWriter.WriteLine("style=""@style/layout_vertical"">")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""match_parent""")
        objWriter.WriteLine("android: Orientation = ""vertical""")
        objWriter.WriteLine("android: background = ""@drawable/border""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: Orientation = ""horizontal""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<Spinner")
        objWriter.WriteLine("")
        objWriter.WriteLine("android: id = ""@+id/spinner_Institution""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.40""")
        objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
        objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
        objWriter.WriteLine("/>")
        objWriter.WriteLine("<Spinner")
        objWriter.WriteLine("android: id = ""@+id/spinner_Mois""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.20""")
        objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
        objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
        objWriter.WriteLine("/>")
        objWriter.WriteLine("<Spinner")
        objWriter.WriteLine("android: id = ""@+id/spinner_Annee""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.20""")
        objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
        objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
        objWriter.WriteLine("/>")
        objWriter.WriteLine("<Spinner")
        objWriter.WriteLine("android: id = ""@+id/spinner_Statut""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.20""")
        objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
        objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
        objWriter.WriteLine("/>")
        objWriter.WriteLine("")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: Orientation = ""horizontal""")
        objWriter.WriteLine("style = ""@style/grid_header_style""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<CheckBox")
        objWriter.WriteLine("android: layout_width = ""40dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: Padding = ""10dp""")
        objWriter.WriteLine("android:visibility=""invisible""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.25""")
        objWriter.WriteLine("android: text = ""Formulaire""")
        objWriter.WriteLine("style=""@style/grid_header_text_style""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.40""")
        objWriter.WriteLine("android: text = ""Institution""")
        objWriter.WriteLine("style=""@style/grid_header_text_style""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.20""")
        objWriter.WriteLine("android: text = ""Periode""")
        objWriter.WriteLine("style=""@style/grid_header_text_style""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.15""")
        objWriter.WriteLine("android: text = ""Statut""")
        objWriter.WriteLine("style=""@style/grid_header_text_style""/>")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""match_parent""")
        objWriter.WriteLine("android: layout_height = ""match_parent""")
        objWriter.WriteLine("android: Orientation = ""vertical""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<ListView")
        objWriter.WriteLine("android: id = ""@+id/listview_questionnaire""")
        objWriter.WriteLine("android: divider = ""#000""")
        objWriter.WriteLine("android: dividerHeight = ""1dp""")
        objWriter.WriteLine("android: cacheColorHint = ""#00000000""")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""0dp""")
        objWriter.WriteLine("android: layout_weight = ""1""")
        objWriter.WriteLine("android: fadingEdge = ""none""")
        objWriter.WriteLine("android:listSelector=""@drawable/list_selector"">")
        objWriter.WriteLine("</ListView>")
        objWriter.WriteLine("")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""30dp""")
        objWriter.WriteLine("android: Orientation = ""horizontal""")
        objWriter.WriteLine("android: paddingTop = ""5dp""")
        objWriter.WriteLine("android: background = ""@drawable/border""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/label_connectedUser""")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: textSize = ""10dp""")
        objWriter.WriteLine("android: text = ""Institution :""")
        objWriter.WriteLine("style = ""@style/form_label""")
        objWriter.WriteLine("android: gravity = ""left""")
        objWriter.WriteLine("android: layout_gravity = ""left""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("</TextView>")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("")
        With objWriter
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_TABLE = ""tbl_" & nomSimple.ToLower & """;")
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID = """ & nomSimple.ToLower & "_id"";")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID_" & cols(i).ToUpper & " = """ & nomSimple.ToLower & "_id_" & cols(i).ToLower & """;")
                Next
            Catch
            End Try
        End With

        With objWriter
            .WriteLine("  public static final String CREATE_TABLE_" & nomSimple.ToUpper & " = ""create table +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_TABLE + "" ("" +")
            .WriteLine("DBConstants.KEY_ID + "" integer primary key autoincrement, "" +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_ID + "" integer not null, "" +")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & "  "" " & types(i) & " not null, "" +")
                Next
                .WriteLine(" DBConstants.MAC_TABLETTE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.MAC_TABLETTE_MODIFIED + "" text, "" +")
                .WriteLine(" DBConstants.DATE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.DATE_MODIF + "" text, "" +")
                .WriteLine(" DBConstants.DATE_NAME + "" long);"";")
            Catch
            End Try
        End With

        objWriter.WriteLine(" db.execSQL(DBScript.CREATE_TABLE_" & nomSimple.ToUpper & ");")
        objWriter.WriteLine(" Log.v(""MyDBhelper onCreate"", ""Created Table "" + DBConstants." & nomSimple.ToUpper & "_TABLE);")

        objWriter.WriteLine()
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidListRowLayout(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\DBHelper\")
        Dim path As String = txt_PathGenerate_Script & nomSimple & "DBConstantsHelperScript.xml"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine()



        objWriter.WriteLine(" <?xml version=""1.0"" encoding=""utf-8""?>")


        objWriter.WriteLine("<RelativeLayout xmlns:android=""http://schemas.android.com/apk/res/android""")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: background = ""@drawable/list_selector""")
        objWriter.WriteLine("android: Orientation = ""horizontal""")
        objWriter.WriteLine("android:padding=""2dip"" >")
        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: id = ""@+id/linearlayout_forRow""")
        objWriter.WriteLine("android: Orientation = ""horizontal""")
        objWriter.WriteLine("android: typeface = ""sans""")
        objWriter.WriteLine(">")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvIdQuestionnaire""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android:layout_height=""0dp""/>")
        objWriter.WriteLine("<CheckBox")
        objWriter.WriteLine("android: layout_width = ""40dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: id = ""@+id/cbxSelect""")
        objWriter.WriteLine("android: focusable = ""false""")
        objWriter.WriteLine("android: focusableInTouchMode = ""false""")
        objWriter.WriteLine("/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvLibelleFormulaire""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.25""")
        objWriter.WriteLine("android: text = ""libelle formulaire""")
        objWriter.WriteLine("android:textColor=""#000000""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvIdFormulaire""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android:layout_height=""0dp""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvLibelleInstitution""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.40""")
        objWriter.WriteLine("android: text = ""libelle institution""")
        objWriter.WriteLine("android:textColor=""#000000""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvIdInstitution""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android:layout_height=""0dp""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvPeriode""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.20""")
        objWriter.WriteLine("android: text = ""Periode""")
        objWriter.WriteLine("android:textColor=""#000000""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvMois""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android:layout_height=""0dp""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvAnnee""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android:layout_height=""0dp""/>")
        objWriter.WriteLine("<TextView")
        objWriter.WriteLine("android: id = ""@+id/tvStatut""")
        objWriter.WriteLine("android: layout_width = ""0dp""")
        objWriter.WriteLine("android: layout_height = ""wrap_content""")
        objWriter.WriteLine("android: layout_weight = ""0.15""")
        objWriter.WriteLine("android: text = ""Statut""")
        objWriter.WriteLine("android:textColor=""#000000""/>")
        objWriter.WriteLine("</LinearLayout>")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("</RelativeLayout>")
        objWriter.WriteLine("")
        With objWriter
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_TABLE = ""tbl_" & nomSimple.ToLower & """;")
            .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID = """ & nomSimple.ToLower & "_id"";")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("  public static final String " & nomSimple.ToUpper & "_ID_" & cols(i).ToUpper & " = """ & nomSimple.ToLower & "_id_" & cols(i).ToLower & """;")
                Next
            Catch
            End Try
        End With

        With objWriter
            .WriteLine("  public static final String CREATE_TABLE_" & nomSimple.ToUpper & " = ""create table +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_TABLE + "" ("" +")
            .WriteLine("DBConstants.KEY_ID + "" integer primary key autoincrement, "" +")
            .WriteLine("DBConstants." & nomSimple.ToUpper & "_ID + "" integer not null, "" +")
            Try
                For i As Int32 = 1 To cols.Count - 1
                    .WriteLine("DBConstants." & nomSimple.ToUpper & "_" & cols(i).ToUpper & "  "" " & types(i) & " not null, "" +")
                Next
                .WriteLine(" DBConstants.MAC_TABLETTE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.MAC_TABLETTE_MODIFIED + "" text, "" +")
                .WriteLine(" DBConstants.DATE_CREATED + "" text not null, "" +")
                .WriteLine(" DBConstants.DATE_MODIF + "" text, "" +")
                .WriteLine(" DBConstants.DATE_NAME + "" long);"";")
            Catch
            End Try
        End With

        objWriter.WriteLine(" db.execSQL(DBScript.CREATE_TABLE_" & nomSimple.ToUpper & ");")
        objWriter.WriteLine(" Log.v(""MyDBhelper onCreate"", ""Created Table "" + DBConstants." & nomSimple.ToUpper & "_TABLE);")

        objWriter.WriteLine()
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidFormLayout(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\layout\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\layout\")
        Dim path As String = txt_PathGenerate_Script & "add_edit_" & nomSimple.ToLower & "_layout.xml"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")


        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine()

        objWriter.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")

        objWriter.WriteLine("<LinearLayout xmlns:android=""http://schemas.android.com/apk/res/android""")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""fill_parent""")
        objWriter.WriteLine("android: paddingLeft = ""5dp""")
        objWriter.WriteLine("android: paddingTop = ""5dp""")
        objWriter.WriteLine("android: paddingRight = ""5dp""")
        objWriter.WriteLine("android: paddingBottom = ""5dp""")
        objWriter.WriteLine("style=""@style/layout_vertical"">")

        objWriter.WriteLine("<LinearLayout")
        objWriter.WriteLine("android: layout_width = ""fill_parent""")
        objWriter.WriteLine("android: layout_height = ""fill_parent""")
        objWriter.WriteLine("android: Orientation = ""vertical""")
        objWriter.WriteLine("android: background = ""@drawable/border""")
        objWriter.WriteLine(">")


        Dim countColumn As Integer = 0
        Dim pourcentagevalue As Decimal = 100 / (_table.ListofColumn.Count - 4)
        Dim pourcentage As String = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If

                    Next

                    objWriter.WriteLine("  <LinearLayout")
                    objWriter.WriteLine("android: layout_width = ""match_parent""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: Orientation = ""horizontal""")
                    objWriter.WriteLine("android: paddingTop = ""15dp""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("<TextView")
                    objWriter.WriteLine("android: id = ""@+id/label_" & reftablename & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.4""")
                    objWriter.WriteLine("android: layout_marginLeft = ""5dp""")
                    objWriter.WriteLine("android: textSize = ""20dp""")
                    objWriter.WriteLine("android: text = """ & reftablename & ":""")
                    objWriter.WriteLine("style = ""@style/form_label""")
                    objWriter.WriteLine("android: gravity = ""left""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</TextView>")

                    objWriter.WriteLine("<Spinner")

                    objWriter.WriteLine("android: id = ""@+id/spinner_" & reftablename & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.6""")
                    objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("/>")


                    objWriter.WriteLine("  </LinearLayout>")



                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("  <LinearLayout")
                    objWriter.WriteLine("android: layout_width = ""match_parent""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: Orientation = ""horizontal""")
                    objWriter.WriteLine("android: paddingTop = ""15dp""")
                    objWriter.WriteLine(">")

                    objWriter.WriteLine("<TextView")
                    objWriter.WriteLine("android: id = ""@+id/label_" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.4""")
                    objWriter.WriteLine("android: layout_marginLeft = ""5dp""")
                    objWriter.WriteLine("android: textSize = ""20dp""")
                    objWriter.WriteLine("android: text = """ & column.Name & ":""")
                    objWriter.WriteLine("style = ""@style/form_label""")
                    objWriter.WriteLine("android: gravity = ""left""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</TextView>")



                    objWriter.WriteLine("<EditText")
                    objWriter.WriteLine("android: id = ""@+id/editText_" & column.Name & """")
                    objWriter.WriteLine("android: layout_weight = ""0.6""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</EditText>")

                    objWriter.WriteLine("  </LinearLayout>")

                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then

                    objWriter.WriteLine("  <LinearLayout")
                    objWriter.WriteLine("android: layout_width = ""match_parent""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: Orientation = ""horizontal""")
                    objWriter.WriteLine("android: paddingTop = ""15dp""")
                    objWriter.WriteLine(">")

                    objWriter.WriteLine("<TextView")
                    objWriter.WriteLine("android: id = ""@+id/label_" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.4""")
                    objWriter.WriteLine("android: layout_marginLeft = ""5dp""")
                    objWriter.WriteLine("android: textSize = ""20dp""")
                    objWriter.WriteLine("android: text = """ & column.Name & ":""")
                    objWriter.WriteLine("style = ""@style/form_label""")
                    objWriter.WriteLine("android: gravity = ""left""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</TextView>")


                    objWriter.WriteLine("<EditText")
                    objWriter.WriteLine("android: id = ""@+id/editText_" & column.Name & """")
                    objWriter.WriteLine("android: layout_weight = ""0.6""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android:inputType=""numberDecimal""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</EditText>")

                    objWriter.WriteLine("  </LinearLayout>")

                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then

                    objWriter.WriteLine("  <LinearLayout")
                    objWriter.WriteLine("android: layout_width = ""match_parent""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: Orientation = ""horizontal""")
                    objWriter.WriteLine("android: paddingTop = ""15dp""")
                    objWriter.WriteLine(">")

                    objWriter.WriteLine("<TextView")
                    objWriter.WriteLine("android: id = ""@+id/label_" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.4""")
                    objWriter.WriteLine("android: layout_marginLeft = ""5dp""")
                    objWriter.WriteLine("android: textSize = ""20dp""")
                    objWriter.WriteLine("android: text = """ & column.Name & ":""")
                    objWriter.WriteLine("style = ""@style/form_label""")
                    objWriter.WriteLine("android: gravity = ""left""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine(">")
                    objWriter.WriteLine("</TextView>")

                    objWriter.WriteLine(" <Spinner")
                    objWriter.WriteLine("android: id = ""@+id/spinner_Jour" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.20""")
                    objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("/>")
                    objWriter.WriteLine("<Spinner")
                    objWriter.WriteLine("android: id = ""@+id/spinner_Mois" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.20""")
                    objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("/>")
                    objWriter.WriteLine("<Spinner")
                    objWriter.WriteLine("android: id = ""@+id/spinner_Annee" & column.Name & """")
                    objWriter.WriteLine("android: layout_width = ""0dp""")
                    objWriter.WriteLine("android: layout_height = ""wrap_content""")
                    objWriter.WriteLine("android: layout_weight = ""0.20""")
                    objWriter.WriteLine("android: prompt = ""@string/label_prompt""")
                    objWriter.WriteLine("android: layout_gravity = ""center_vertical""")
                    objWriter.WriteLine("/>")


                    objWriter.WriteLine("  </LinearLayout>")

                End If
            End If
            countColumn = countColumn + 1

        Next


        objWriter.WriteLine(" </LinearLayout>")
        objWriter.WriteLine("</LinearLayout>")

        objWriter.Close()
    End Sub

    Public Shared Sub CreateAndroidFormActivity(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_projectName As TextBox)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\activity\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\Android\activity\")
        Dim path As String = txt_PathGenerate_Script & "AddEdit" & nomSimple & "Activity.java"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")

        Dim content As String = "public class " & nomSimple & " {" & Chr(13)
        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add(_foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add(_column.Name)
                types.Add(_column.Type.JavaName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next

        objWriter.WriteLine()

        objWriter.WriteLine("package ht.agrotracking.mobile.activity;")

        objWriter.WriteLine("import android.app.ActionBar;")
        objWriter.WriteLine("import android.app.Activity;")
        objWriter.WriteLine("import android.app.AlertDialog;")
        objWriter.WriteLine("import android.app.ProgressDialog;")
        objWriter.WriteLine("import android.content.Context;")
        objWriter.WriteLine("import android.content.DialogInterface;")
        objWriter.WriteLine("import android.content.Intent;")
        objWriter.WriteLine("import android.net.wifi.WifiInfo;")
        objWriter.WriteLine("import android.net.wifi.WifiManager;")
        objWriter.WriteLine("import android.os.Bundle;")
        objWriter.WriteLine("import android.util.Log;")
        objWriter.WriteLine("import android.view.*;")
        objWriter.WriteLine("import android.widget.*;")
        objWriter.WriteLine("import ht.agrotracking.mobile.global.Utils;")
        objWriter.WriteLine("import ht.agrotracking.mobile.helper.*;")
        objWriter.WriteLine("import ht.agrotracking.mobile.model.*;")

        objWriter.WriteLine("import java.text.SimpleDateFormat;")
        objWriter.WriteLine("import java.util.ArrayList;")
        objWriter.WriteLine("import java.util.Date;")





        objWriter.WriteLine("public class AddEdit" & nomSimple & "Activity extends Activity {")
        objWriter.WriteLine("Context context;")
        objWriter.WriteLine("String _message;")
        objWriter.WriteLine("  ArrayList<" & "Jour> list" & "Jour" & ";")
        objWriter.WriteLine("  ArrayList<" & "Mois> list" & "Mois" & ";")
        objWriter.WriteLine("  ArrayList<" & "Annee> list" & "Annee" & ";")


        Dim countColumn As Integer = 0
        Dim pourcentagevalue As Decimal = 100 / (_table.ListofColumn.Count - 4)
        Dim pourcentage As String = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If

                    Next

                    objWriter.WriteLine("  ArrayList<" & reftablename & "> list" & reftablename & ";")
                    objWriter.WriteLine("  Spinner spinner_" & reftablename & ";")
                    objWriter.WriteLine(" lont id_" & reftablename & " = 0; ")


                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name Then

                    objWriter.WriteLine("EditText editText_" & column.Name & "")


                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then

                    objWriter.WriteLine("EditText editText_" & column.Name & "")

                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then


                    objWriter.WriteLine("  Spinner spinner_" & "Jour" & column.Name & ";")
                    objWriter.WriteLine(" lont id_" & "Jour" & column.Name & " = 0; ")



                    objWriter.WriteLine("  Spinner spinner_" & "Mois" & column.Name & ";")
                    objWriter.WriteLine(" lont id_" & "Mois" & column.Name & " = 0; ")



                    objWriter.WriteLine("  Spinner spinner_" & "Annee" & column.Name & ";")
                    objWriter.WriteLine(" lont " & "Annee" & column.Name & " = 0; ")


                End If
            End If
            countColumn = countColumn + 1
        Next




        objWriter.WriteLine("private ProgressDialog progress;")
        objWriter.WriteLine("//    private SessionManager session;")
        objWriter.WriteLine("//    private UserModel currentUser;")
        objWriter.WriteLine("LayoutInflater inflater;")
        objWriter.WriteLine("private ActionBar actionBar;")

        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public void onCreate(Bundle savedInstanceState) {")
        objWriter.WriteLine("super.onCreate(savedInstanceState);")
        objWriter.WriteLine("getWindow().requestFeature(Window.FEATURE_ACTION_BAR);")
        objWriter.WriteLine("setContentView(R.layout.add_edit_commande_layout);")
        objWriter.WriteLine("context = this;")
        objWriter.WriteLine("progress = new ProgressDialog(this);")
        objWriter.WriteLine("Intent i = getIntent();")


        objWriter.WriteLine(" inflater = (LayoutInflater) this.getSystemService(Context.LAYOUT_INFLATER_SERVICE);")

        objWriter.WriteLine("//        session = new SessionManager(this);")

        objWriter.WriteLine("//        currentUser =   session.getUserDetails();")
        objWriter.WriteLine("//        Utils.PopulateMoisData();")
        objWriter.WriteLine("//        Utils.PopulateAnneeData();")


        objWriter.WriteLine("try {")
        objWriter.WriteLine("Utils.PopulateAnneeData();")
        objWriter.WriteLine("Utils.PopulateMoisData();")

        objWriter.WriteLine("listMois = MoisHelper.SearchAll();")
        objWriter.WriteLine("listAnnee = AnneeHelper.SearchAll();")

        countColumn = 0
        pourcentagevalue = 100 / (_table.ListofColumn.Count - 4)
        pourcentage = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If
                    Next
                    objWriter.WriteLine("list" & reftablename & " = " & reftablename & "Helper.SearchAll();")
                    objWriter.WriteLine("  spinner_" & reftablename & " = (Spinner)findViewById(R.id.spinner_" & reftablename & ");")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine(" editText_" & column.Name & " = (EditText) findViewById(R.id.editText_" & column.Name & ");")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then
                    objWriter.WriteLine(" editText_" & column.Name & " = (EditText) findViewById(R.id.editText_" & column.Name & ");")
                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("  spinner_" & column.Name & " = (Spinner)findViewById(R.id.spinner_" & column.Name & ");")
                End If
            End If
            countColumn = countColumn + 1
        Next


        countColumn = 0
        pourcentagevalue = 100 / (_table.ListofColumn.Count - 4)
        pourcentage = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If
                    Next



                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name Then

                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then

                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("            spinner_" & column.Name & ".setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")

                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
                    objWriter.WriteLine("      int position, long id) {")
                    objWriter.WriteLine("id_" & column.Name & " = position ;")

                    objWriter.WriteLine("}")

                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("});")

                    objWriter.WriteLine("            spinner_" & column.Name & ".setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
                    objWriter.WriteLine("      int position, long id) {")
                    objWriter.WriteLine("id_" & column.Name & " = position ;")

                    objWriter.WriteLine("}")

                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("});")


                    objWriter.WriteLine("ArrayAdapter<CharSequence> adapter = ArrayAdapter.createFromResource(")
                    objWriter.WriteLine("this, R.array.jour_array, android.R.layout.simple_spinner_item);")
                    objWriter.WriteLine("adapter.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
                    objWriter.WriteLine("spinner_" & column.Name & ".setAdapter(adapter);")



                    objWriter.WriteLine("if (listMois != null) {")
                    objWriter.WriteLine("Mois m = new Mois();")
                    objWriter.WriteLine("m.setMois(""Mois"");")
                    objWriter.WriteLine("listMois.add(0, m);")
                    objWriter.WriteLine("final ArrayAdapter<Mois> adapterMois = new ArrayAdapter<Mois>(AddEditCommandeActivity.this,")
                    objWriter.WriteLine("android.R.layout.simple_spinner_item,")
                    objWriter.WriteLine("listMois);")
                    objWriter.WriteLine("adapterMois.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
                    objWriter.WriteLine("spinner_Mois" & column.Name & ".setAdapter(adapterMois); // Set the custom adapter to the spinner")
                    objWriter.WriteLine("id_Mois" & column.Name & " = adapterMois.getItem(0).getId_Mois();")
                    objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
                    objWriter.WriteLine("spinner_Mois" & column.Name & ".setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
                    objWriter.WriteLine("int position, long id) {")
                    objWriter.WriteLine("id_Mois" & column.Name & " = adapterMois.getItem(position).getId_Mois();")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("}")

                    objWriter.WriteLine("                    @Override")
                    objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("});")

                    objWriter.WriteLine("                spinner_Mois" & column.Name & ".setAdapter(adapterMois); // Set the custom adapter to the spinner")
                    objWriter.WriteLine("id_Mois" & column.Name & " = adapterMois.getItem(0).getId_Mois();")
                    objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
                    objWriter.WriteLine("spinner_Mois" & column.Name & ".setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
                    objWriter.WriteLine("int position, long id) {")
                    objWriter.WriteLine("id_Mois" & column.Name & " = adapterMois.getItem(position).getId_Mois();")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("                    @Override")
                    objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("});")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("if (listAnnee != null) {")
                    objWriter.WriteLine("Annee a = new Annee();")
                    objWriter.WriteLine("a.setAnnee(""Annee"");")
                    objWriter.WriteLine("listAnnee.add(0, a);")
                    objWriter.WriteLine("final ArrayAdapter<Annee> adapterAnnee = new ArrayAdapter<Annee>(AddEditCommandeActivity.this, android.R.layout.simple_spinner_item, listAnnee);")
                    objWriter.WriteLine("adapterAnnee.setDropDownViewResource(android.R.layout.select_dialog_singlechoice);")
                    objWriter.WriteLine("spinner_Annee" & column.Name & ".setAdapter(adapterAnnee); // Set the custom adapter to the spinner")
                    objWriter.WriteLine("Annee" & column.Name & " = Long.parseLong(""0"");")
                    objWriter.WriteLine("// You can create an anonymous listener to handle the event when is selected an spinner item")
                    objWriter.WriteLine("spinner_Annee" & column.Name & ".setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {")
                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onItemSelected(AdapterView<?> adapterView, View view,")
                    objWriter.WriteLine("int position, long id) {")
                    objWriter.WriteLine("if (adapterAnnee.getItem(position).getAnnee() == ""Annee"") {")
                    objWriter.WriteLine("Annee" & column.Name & " = Long.parseLong(""0"");")
                    objWriter.WriteLine("} else {")
                    objWriter.WriteLine("Annee" & column.Name & " = Long.parseLong(adapterAnnee.getItem(position).getAnnee());")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("@Override")
                    objWriter.WriteLine("public void onNothingSelected(AdapterView<?> adapter) {")
                    objWriter.WriteLine("}")
                    objWriter.WriteLine("});")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("")
                    objWriter.WriteLine("}")



                End If
            End If
            countColumn = countColumn + 1
        Next


        objWriter.WriteLine(" ActionBarCall();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("catch (Exception e){")
        objWriter.WriteLine("if(e != null){")
        objWriter.WriteLine("Log.e(""AddEditQuestionnaireActivity.onCreate"", """" + e.getMessage().toString());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")


        objWriter.WriteLine("private void ActionBarCall(){")
        objWriter.WriteLine("actionBar = getActionBar();")
        objWriter.WriteLine("// Hide the action bar title")
        objWriter.WriteLine("// CharSequence activity_title_addeditquestionnaireactivity = getText(R.string.activity_title_addeditquestionnaireactivity);")
        objWriter.WriteLine("actionBar.setTitle(""Enregistrer un " & nomSimple & """);")
        objWriter.WriteLine("actionBar.setDisplayShowTitleEnabled(true);")
        objWriter.WriteLine("// Enabling Spinner dropdown navigation")
        objWriter.WriteLine("actionBar.setNavigationMode(ActionBar.NAVIGATION_MODE_STANDARD);")
        objWriter.WriteLine("}")

        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public boolean onCreateOptionsMenu(Menu menu) {")
        objWriter.WriteLine("MenuInflater inflater = getMenuInflater();")
        objWriter.WriteLine("inflater.inflate(R.menu.menu_save_and_cancel, menu);")

        objWriter.WriteLine("return super.onCreateOptionsMenu(menu);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("@Override")
        objWriter.WriteLine("public boolean onOptionsItemSelected(MenuItem item) {")
        objWriter.WriteLine("// Take appropriate action for each action item click")
        objWriter.WriteLine("switch (item.getItemId()) {")
        objWriter.WriteLine("case android.R.id.home:")
        objWriter.WriteLine("case R.id.action_cancel:")
        objWriter.WriteLine("ApplicationHelper.SafelyNavigateUpTo(this);")
        objWriter.WriteLine("return true;")
        objWriter.WriteLine("case R.id.action_save:")
        objWriter.WriteLine("if(Save" & nomSimple & "()){")
        objWriter.WriteLine("_message = """ & nomSimple & " enregistrée avec succès !"";")
        objWriter.WriteLine("MessageToShow();")
        objWriter.WriteLine("ApplicationHelper.SafelyNavigateUpTo(this);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("return true;")

        objWriter.WriteLine("            case R.id.action_addClient:")
        objWriter.WriteLine("Intent i = new Intent ();")
        objWriter.WriteLine("i.setClass(AddEdit" & nomSimple & "Activity.this,AddEditClientActivity.class);")
        objWriter.WriteLine("startActivity(i);")
        objWriter.WriteLine("return true;")

        objWriter.WriteLine("            case R.id.action_disconnect:")
        objWriter.WriteLine("//                session = new SessionManager(this);")
        objWriter.WriteLine("//                session.logoutUser();")
        objWriter.WriteLine("finish();")
        objWriter.WriteLine("return true;")


        objWriter.WriteLine("            default:")
        objWriter.WriteLine("return super.onOptionsItemSelected(item);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")


        objWriter.WriteLine("    private boolean Save" & nomSimple & "(){")
        objWriter.WriteLine("boolean isSaved = false;")
        objWriter.WriteLine("try{")
        objWriter.WriteLine("_message = """";")



        countColumn = 0
        pourcentagevalue = 100 / (_table.ListofColumn.Count - 4)
        pourcentage = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If
                    Next

                    objWriter.WriteLine("           if(id_" & columnName & " == 0){")
                    objWriter.WriteLine("               _message = ""Il faut renseigner le " & reftablename & " !"";")
                    objWriter.WriteLine("            }")


                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name Then


                    objWriter.WriteLine("if (editText_" & column.Name & ".getText().toString().trim().equalsIgnoreCase("""") ){")
                    objWriter.WriteLine("_message = ""Il faut renseigner la " & column.Name & " !"";")
                    objWriter.WriteLine("}")

                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType = "Decimal" Then
                    objWriter.WriteLine("if (editText_" & column.Name & ".getText().toString().trim().equalsIgnoreCase("""") ){")
                    objWriter.WriteLine("_message = ""Il faut renseigner la " & column.Name & " !"";")
                    objWriter.WriteLine("}")
                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then


                    objWriter.WriteLine("            if (id_Jour" & column.Name & " == 0 || id_Mois" & column.Name & " ==0 || Annee" & column.Name & " ==0 ){")
                    objWriter.WriteLine("_message = ""il faut renseigner la date de livraison"";")
                    objWriter.WriteLine("}")
                End If
            End If
            countColumn = countColumn + 1
        Next





        objWriter.WriteLine("")
        objWriter.WriteLine("if (_message.equalsIgnoreCase("""")){")
        objWriter.WriteLine("WifiManager wifiManager = (WifiManager) getSystemService(Context.WIFI_SERVICE);")
        objWriter.WriteLine("WifiInfo wInfo = wifiManager.getConnectionInfo();")
        objWriter.WriteLine("String macAddress = wInfo.getMacAddress();")
        objWriter.WriteLine("")
        objWriter.WriteLine("SimpleDateFormat sdf = new SimpleDateFormat(""yyyy-MM-dd"");")
        objWriter.WriteLine("String currentDateandTime = sdf.format(new Date());")

        objWriter.WriteLine("" & nomSimple & " obj  = new " & nomSimple & "();")

        countColumn = 0
        pourcentagevalue = 100 / (_table.ListofColumn.Count - 4)
        pourcentage = pourcentagevalue.ToString + "%"
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then
                    Dim foreignkey As New Cls_ForeignKey(_table.ID, column.ID)
                    Dim textForcombo As String = ""
                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    Dim reftablename As String = foreignkey.RefTable.Name.Substring(4, foreignkey.RefTable.Name.Length - 4)
                    Dim countcolumnref As Long
                    For Each column_fore As Cls_Column In foreignkey.RefTable.ListofColumn
                        If column_fore.Type.VbName = "String" Then
                            textForcombo = column_fore.Name
                            Exit For
                        End If
                    Next

                    objWriter.WriteLine("obj.set" & column.Name & "(" & column.Name & ");")

                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name And column.Type.JavaName = "String" Then

                    objWriter.WriteLine("obj.set" & column.Name & "(editText_" & column.Name & ".getText().toString());")

                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("obj.set" & column.Name & "(" & ConvertDBToJavaParsingType(column.TrueSqlServerType) & ".parse" & ConvertDBToJavaParsingType(column.TrueSqlServerType) & "(editText_" & column.Name & ".getText().toString()));")
                ElseIf column.TrueSqlServerType = "date" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("obj.set" & column.Name & "(Annee" & column.Name & " + ""-"" + id_Mois" & column.Name & " + ""-"" + id_Jour" & column.Name & ");")


                End If
            End If
            countColumn = countColumn + 1
        Next

        objWriter.WriteLine("if(obj.getId() ==0){")
        objWriter.WriteLine("obj.setMacTabletteCreated(macAddress);")
        objWriter.WriteLine("obj.setDateCreated(currentDateandTime);")
        objWriter.WriteLine("obj.setMacTabletteModif("");")
        objWriter.WriteLine("obj.setDateModif("");")
        objWriter.WriteLine("}else{")
        objWriter.WriteLine("obj.setMacTabletteCreated(macAddress);")
        objWriter.WriteLine("obj.setDateModif(currentDateandTime);")
        objWriter.WriteLine("}")


        objWriter.WriteLine("" & nomSimple & "Helper.save(obj);")
        objWriter.WriteLine("")


        objWriter.WriteLine("isSaved = true;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("else{")
        objWriter.WriteLine("MessageToShow();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("catch (Exception e){")
        objWriter.WriteLine("if(e != null){")
        objWriter.WriteLine("_message = e.getMessage();")
        objWriter.WriteLine("MessageToShow();")
        objWriter.WriteLine("Log.e(""AddEdit" & nomSimple & "Activity.Save" & nomSimple & """, """" + e.getMessage().toString());")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("return isSaved;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("private void MessageToShow (){")
        objWriter.WriteLine("Toast.makeText(getApplicationContext(), _message, Toast.LENGTH_LONG).show();")
        objWriter.WriteLine("AlertDialog.Builder alertbox = new AlertDialog.Builder(AddEditCommandeActivity.this);")
        objWriter.WriteLine("alertbox.setMessage(_message);")
        objWriter.WriteLine("alertbox.setNeutralButton(""Ok"", new DialogInterface.OnClickListener() {")
        objWriter.WriteLine("public void onClick(DialogInterface arg0, int arg1) {")
        objWriter.WriteLine("Toast.makeText(getApplicationContext(), _message  , Toast.LENGTH_LONG).show();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("});")
        objWriter.WriteLine("        alertbox.show();")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.WriteLine("")
        objWriter.Close()

    End Sub


#End Region

#Region "Php Fonctions"

    Public Shared Sub CreatePHPClass(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByVal databasename As String)
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl_", "")
        Dim nomUpperClasse As String = nomClasse.Substring(0, 1).ToUpper() & nomClasse.Substring(1, nomClasse.Length - 1)
        ''   Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & databasename & "\PHPClass\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & databasename & "\PHPClass\")
        Dim path As String = txt_PathGenerate_Script & "Cls_" & nomUpperClasse & ".class.php"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""


        Dim header As String = "'''Generate By Edou Application *******" & Chr(13) _
                               & "''' Class " + nomUpperClasse & Chr(13) & Chr(13)
        header = ""
        Dim content As String = "class Cls_" & nomUpperClasse & " {" & Chr(13)

        _end = "}" & Chr(13)
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If
        ' Create the file.
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        ''    objWriter.WriteLine("<?php require_once('../Query/DAL/MySQL_Helper.php'); ?>")
        objWriter.WriteLine("<?php")
        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim ds As DataSet = LoadTableStructure(name)
        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer = ds.Tables(0).Rows.Count

        For Each dt As DataRow In ds.Tables(0).Rows
            If dt(0).ToString = "PRI" Then
                Id_table = dt(0).ToString()
            End If

        Next

        For Each dt As DataRow In ds.Tables(0).Rows
            If count < cap - 4 Then
                cols.Add("" & dt(0))
                initialtypes.Add(dt(1))
                If dt(1).ToString.Contains("(") Then
                    Dim arrstring As String() = dt(1).ToString.Split("(")
                    types.Add(arrstring(0).ToString)
                Else
                    types.Add(ConvertDBToJavaType((dt(1))))
                End If
                '   length.Add(dt(2))
                count += 1
            Else
                Exit For
            End If
        Next
        objWriter.WriteLine(" #Region "" Attribut """)
        'objWriter.WriteLine("Private _id As Long")
        objWriter.WriteLine()
        objWriter.WriteLine(" private  $_id ; ")
        Try
            For i As Int32 = 1 To cols.Count - 1

                'If Not nottoputforlist.Contains(cols(i)) Then
                '    insertstring &= ", " & cols(i)
                '    updatestring &= ", " & cols(i)
                'End If
                Dim attrib As String = ""  'not used for now to be updated.

                objWriter.WriteLine("private $_" & cols(i) & ";")
                If initialtypes(i) = "image" Then

                    objWriter.WriteLine("private String " & cols(i) & "String;" & "")
                End If
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("private " & cols(i).Substring(3, cols(i).Length - 3) & " " & cols(i).Substring(3, cols(i).Length - 3).ToLower & ";")
                End If
            Next
        Catch ex As Exception

        End Try
        objWriter.WriteLine(" private  $isdirty = false; ")
        objWriter.WriteLine()
        objWriter.WriteLine("#EndRegion "" Attribut """)
        objWriter.WriteLine()

        objWriter.WriteLine("#Region ""Constructeur""")

        objWriter.WriteLine("public function Cls_" & nomUpperClasse & "($id=0)")
        objWriter.WriteLine("{")
        objWriter.WriteLine("if($id == 0){")
        objWriter.WriteLine("	$this->BlankProperties();")
        objWriter.WriteLine("}else{")
        objWriter.WriteLine("	$this->Read($id);")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}//")

        objWriter.WriteLine("#EndRegion ""Constructeur""")

        objWriter.WriteLine("#Region ""Proprietes""")


        objWriter.WriteLine("public function get_" & "ID" & "(){")
        objWriter.WriteLine("return $this->_" & "id" & ";")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public function set_" & "ID" & "($value) {")
        objWriter.WriteLine("	if ($this->_" & "id" & " <> $value ){")
        objWriter.WriteLine("$this->_" & "id" & " = $value;")
        objWriter.WriteLine("}")
        objWriter.WriteLine("}")

        Try
            For i As Int32 = 1 To cols.Count - 1

                'If Not nottoputforlist.Contains(cols(i)) Then
                '    insertstring &= ", " & cols(i)
                '    updatestring &= ", " & cols(i)
                'End If
                Dim attrib As String = ""  'not used for now to be updated.

                objWriter.WriteLine("public function get_" & cols(i) & "(){")
                objWriter.WriteLine("return $this->_" & cols(i) & ";")
                objWriter.WriteLine("}")

                objWriter.WriteLine("public function set_" & cols(i) & "($value) {")
                objWriter.WriteLine("	if ($this->_" & cols(i) & " <> $value ){")
                objWriter.WriteLine("$this->_" & cols(i) & " = $value;")
                objWriter.WriteLine("}")
                objWriter.WriteLine("}")
                If initialtypes(i) = "image" Then

                    objWriter.WriteLine("private String " & cols(i) & "String;" & "")
                End If
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("private " & cols(i).Substring(3, cols(i).Length - 3) & " " & cols(i).Substring(3, cols(i).Length - 3).ToLower & ";")
                End If
            Next
        Catch ex As Exception

        End Try
        objWriter.WriteLine()
        objWriter.WriteLine("#EndRegion ""Proprietes""")
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''acces base  de donnee ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        objWriter.WriteLine()
        objWriter.WriteLine("#Region ""Access BASE DE DONNEE""")


        objWriter.WriteLine("private function Insert($User)" & Chr(13) & _
                             "{" & Chr(13) & _
                             "try{ " & Chr(13) & _
                             "$rows = SQLHelper::ExecuteProcedure (""SP_Insert_" & nomUpperClasse & """)")
        Try
            For i As Int32 = 1 To cols.Count - 1
                objWriter.WriteLine(", $this->_" & cols(i))
                If initialtypes(i) = "image" Then
                    objWriter.WriteLine("private String " & cols(i) & "String;" & "")
                End If
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("private " & cols(i).Substring(3, cols(i).Length - 3) & " " & cols(i).Substring(3, cols(i).Length - 3).ToLower & ";")
                End If
            Next
        Catch ex As Exception
        End Try

        objWriter.WriteLine(", $User );")

        objWriter.WriteLine("$count = count($rows);")
        objWriter.WriteLine("if ($count == 0) {")
        objWriter.WriteLine("return 0;")
        objWriter.WriteLine("} else {")

        objWriter.WriteLine("  	return $this->_id = (int) $result[0][""ID""]; " & Chr(13) & _
                            " }" & Chr(13) & _
                            "}catch(Exception $e){" & Chr(13) & _
                            "throw New Exception ($e->getMessage());" & Chr(13) & _
                            "}" & Chr(13) & _
                            "}//")




        objWriter.WriteLine("private function Update($User)" & Chr(13) & _
                             "{" & Chr(13) & _
                             "try{ " & Chr(13) & _
                             "$result = SQLHelper::ExecuteProcedure (""SP_Update_" & nomUpperClasse & """)")

        objWriter.WriteLine(", $this->_id")

        Try
            For i As Int32 = 1 To cols.Count - 1
                objWriter.WriteLine(", $this->_" & cols(i))
            Next
        Catch ex As Exception
        End Try

        objWriter.WriteLine("return $result;" & Chr(13) & _
                            " }catch(Exception $e){" & Chr(13) & _
                              " throw New Exception ( $e->getMessage());" & Chr(13) & _
                            "}" & Chr(13) & _
                            "}")

        objWriter.WriteLine()



        objWriter.WriteLine("public function Read($id)" & Chr(13) & _
                                 "{" & Chr(13) & _
                                 "	if($id <> 0 ){" & Chr(13) & _
                                 "		$rows = SQLHelper::ExecuteProcedure(""SP_Select_" & nomUpperClasse & "_ByID"", $id);" & Chr(13) & _
                                 "		if(count($rows) < 0 )" & Chr(13) & _
                                 "		{ " & Chr(13) & _
                                 "			$this->BlankProperties();" & Chr(13) & _
                                 "		}else{" & Chr(13) & _
                                 "			$this->SetProperties($rows[0]);" & Chr(13) & _
                                 "		}" & Chr(13) & _
                                 "	}else{" & Chr(13) & _
                                 "		$this->BlankProperties();" & Chr(13) & _
                                 "	}" & Chr(13) & _
                                 "}")
        objWriter.WriteLine()
        objWriter.WriteLine(" public function SearchAll()" & Chr(13) & _
                                 "{" & Chr(13) & _
                                  "$rows = SQLHelper::ExecuteProcedure(""SP_ListAll_" & nomUpperClasse & """ );" & Chr(13) & _
                                  "objs = array();" & Chr(13) & _
                                  "foreach ($rows as $row) {" & Chr(13) & _
                                  "$obj = new Cls_ " & nomUpperClasse & ";" & Chr(13) & _
                                   "$obj->SetProperties($result);	" & Chr(13) & _
                                   "$objs[] = $obj;" & Chr(13) & _
                                  "}" & Chr(13) & _
                                  "return $objs;" & Chr(13) & _
                                 "}"
                            )




        objWriter.WriteLine("public static function SearchAll_ForPagination($elementToSkyp, $rows, $columnToSort, $sortingValue) {")
        objWriter.WriteLine("$rows = SQLHelper::ExecuteProcedure(""SP_ListAll_Specialisation_ForPagination"", $elementToSkyp, $rows, $columnToSort, $sortingValue);")
        objWriter.WriteLine("$objs = array();")
        objWriter.WriteLine("foreach ($rows as $row) {")
        objWriter.WriteLine("   $obj =  new Cls_ " & nomUpperClasse & ";")
        objWriter.WriteLine("   $obj->SetProperties($row);")
        objWriter.WriteLine("   $objs[] = $obj;")
        objWriter.WriteLine(" }")
        objWriter.WriteLine("  return $objs;")
        objWriter.WriteLine("}")

        objWriter.WriteLine("public function Delete()" & Chr(13) & _
                                         "{" & Chr(13) & _
                                          "$result = SQLHelper::ExecuteProcedure(""SP_Delete_" & nomUpperClasse & """, $this->_id);" & Chr(13) & _
                                          "return $result;" & Chr(13) & _
                                         "}"
                                    )

        objWriter.WriteLine()

        objWriter.WriteLine("	public function Save($User) " &
                                  "{" & Chr(13) & _
                                  "if($this->isdirty)" & Chr(13) & _
                                  "{" & Chr(13) & _
                                  "	Cls_" & nomUpperClasse & "::Validation();" & Chr(13) & _
                                  "	if($this->_id == 0)" & Chr(13) & _
                                  "	{" & Chr(13) & _
                                  "		$this->Insert($User);" & Chr(13) & _
                                  "	}else{ " & Chr(13) & _
                                  "		if($this->_id > 0 )" & Chr(13) & _
                                  "		{" & Chr(13) & _
                                  "			$this->Update($User);" & Chr(13) & _
                                  "		}else{ " & Chr(13) & _
                                  "			$this->_id = 0; " & Chr(13) & _
                                  "			//return false;	" & Chr(13) & _
                                  "		}" & Chr(13) & _
                                  "	}" & Chr(13) & _
                                  "}" & Chr(13) & _
                                  "$this->isdirty = false;" & Chr(13) & _
                                 "}")

        objWriter.WriteLine()
        objWriter.WriteLine("public function BlankProperties() " & Chr(13) & _
                            "{" & Chr(13) & _
                            "$this->_id = 0;"
                            )
        Try
            For i As Int32 = 1 To cols.Count - 1
                objWriter.WriteLine(", $this->_" & cols(i) & "= '';")
            Next
        Catch ex As Exception
        End Try
        objWriter.WriteLine("$this->isdirty = false;")
        objWriter.WriteLine("}")

        objWriter.WriteLine()

        objWriter.WriteLine("	public function SetProperties($rs)" & Chr(13) & _
                            "{" & Chr(13) & _
                            "$this->_id = $rs['ID'];"
                            )
        Try
            For i As Int32 = 1 To cols.Count - 1
                objWriter.WriteLine("$this->_" & cols(i) & "= &rs['" & cols(i) & "'];")
            Next
        Catch ex As Exception
        End Try

        objWriter.WriteLine("}")
        objWriter.WriteLine()


        objWriter.WriteLine("#EndRegion ""Access BASE DE DONNEE""")
        objWriter.WriteLine()

        objWriter.WriteLine("#Region ""Other """)
        objWriter.WriteLine(" private function Validation")

        Dim stringlistforvalidation As New List(Of String) From {"String"}
        Dim decimalintegerforvalidation As New List(Of String) From {"Integer", "Long", "Decimal"}



        'For i As Int32 = 1 To cols.Count - 2
        '    If stringlistforvalidation.Contains(types(i)) Then
        '        objWriter.WriteLine("If (" & cols(i) & " = "")"" { " & Chr(13) _
        '                            & "Throw (New System.Exception("" " & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & " obligatoire""))" & Chr(13) _
        '                            & "}"
        '                            )
        '        objWriter.WriteLine()
        '        objWriter.WriteLine("If Len(" & cols(i) & ") > " & length(i) & " {" & Chr(13) _
        '                            & "Throw (New System.Exception("" " & "Trop de caractères insérés pour" & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & "  (la longueur doit être inférieure a " & length(i) & " caractères.  )""))" & Chr(13) _
        '                            & "}"
        '                            )
        '        objWriter.WriteLine()
        '    End If

        '    If decimalintegerforvalidation.Contains(types(i)) Then
        '        If ListofForeignKey.Contains(cols(i)) Then
        '            objWriter.WriteLine("If (" & cols(i) & " == 0) { " & Chr(13) _
        '                          & "Throw (New System.Exception("" " & cols(i).Substring(4, cols(i).Length - 4).ToString.ToLower & " obligatoire""))" & Chr(13) _
        '                          & "}"
        '                          )
        '            objWriter.WriteLine()
        '        Else
        '            objWriter.WriteLine("If ( " & cols(i) & " == 0 ) { " & Chr(13) _
        '                           & "Throw (New System.Exception("" " & cols(i).Substring(1, cols(i).Length - 1).ToString.ToLower & " obligatoire.""))" & Chr(13) _
        '                           & "}"
        '                           )
        '            objWriter.WriteLine()
        '        End If
        '    End If


        'Next
        objWriter.WriteLine("")

        objWriter.WriteLine("#EndRegion ""Other """)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''end''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        objWriter.WriteLine()
        objWriter.WriteLine(_end)
        objWriter.WriteLine("?>")
        objWriter.Close()

    End Sub

#End Region


#Region " Php Interface Fonctions"

    Public Shared Sub CreateInterfaceCodePHP1(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByVal databasename As String)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "frm").Replace("Tbl", "frm").Replace("TBL", "frm")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".view.php"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)

        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine()

        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer
        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name
        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")


        With objWriter
            .WriteLine("<?php require_once('../Model/Specialisation.class.php'); ?>")
            .WriteLine("<?php require_once('../DAL/MySQL_Helper.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ValidationFunction.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ApplicationHelper.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/QueryConnexion.php'); ?>")
        End With

        With objWriter
            .WriteLine("<?php")
            .WriteLine("//if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//    ApplicationHelper::Refresh_MainPage();")
            .WriteLine("//}")
            .WriteLine("$PageName = 'frm_Specialisation.view.php';")
            .WriteLine("$Btn_Save = 'btnSave';")
            .WriteLine("$Btn_Annuler = 'btnCancel';")
            .WriteLine("//        if( !isset($_SESSION[$GLOBAL_SESSION]) )")
            .WriteLine("//	{")
            .WriteLine("//		Cls_Parameters::Refresh_MainPage();")
            .WriteLine("//	}else{")
            .WriteLine("//		$Is_Acces_Page = 'display:block;';")
            .WriteLine("//		if(!Select_Privilege($PageName,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql))")
            .WriteLine("//		{// No acces a la page")
            .WriteLine("//			Cls_Parameters::Dialog_PopUP($NO_ACCES_PAGE);")
            .WriteLine("//			$_message = Cls_Parameters::Show_Message($NO_ACCES_PAGE);")
            .WriteLine("//			$Is_Acces_Page = 'display:none;';")
            .WriteLine("//		}else{")
            .WriteLine("//			$Btn_Save = (Select_Privilege($Btn_AddEdit,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql)) ? '' : 'display:none;' ;")
            .WriteLine("//		}//")
            .WriteLine("//	}//   ")
            .WriteLine("?>")

        End With

        With objWriter
            .WriteLine("<?php")
            .WriteLine("$ID = (isset($_GET[""ID""])) ? $_GET[""ID""] : '0';")


            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                ElseIf types(i) = "Boolean" Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                Else
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                End If
            Next


            .WriteLine("if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//	$_SESSION[$GLOBAL_SESSION_PAGENAME] = ""../Administration/Entreprise.php"";")
            .WriteLine("//	Cls_Parameters::Refresh_MainPage();")
            .WriteLine("}")
            .WriteLine("$IsPostBack = false;")
            .WriteLine("if ($_POST[""btnCancel""] == ""Abandonner"") {")
            .WriteLine("ApplicationHelper::Refresh_MainPage(false);")
            .WriteLine("}//")
            .WriteLine("if (isset($_POST['btnSave'])) {")

            Dim saveString As String = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next
            .WriteLine("btnSaveClick(" & saveString & ");")


            .WriteLine("}")
            .WriteLine("if (!$IsPostBack) {")
            .WriteLine("if (isset($_GET['ID']) && isset($_GET['Action']) && is_numeric($_GET['ID'])) {")
            .WriteLine("global $ID;")
            .WriteLine("try {")
            .WriteLine("switch ($_GET['Action']) {")
            .WriteLine("case 'Edit': {")
            .WriteLine("$obj = new" & nomSimple & "($ID);")

            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                Else
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                End If
            Next



            .WriteLine("                    break;")
            .WriteLine("}")
            .WriteLine("default: ")
            .WriteLine("}")
            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();	")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("	    $_message = ApplicationHelper::Show_Message($_message,'S','../../');")
            .WriteLine("}")
            .WriteLine("}")
            .WriteLine("}")


            saveString = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next

            .WriteLine("function btnSaveClick(" & saveString & ") { ")


            .WriteLine("$IsPostBack = true;")
            .WriteLine("try {")
            .WriteLine("Validation();")


            saveString = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next

            .WriteLine("Save(" & saveString & ");")




            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("$_message = ApplicationHelper::Show_Message($_message, 'S');")
            .WriteLine("}")
            .WriteLine("}")

            .WriteLine("function Validation() {")

            .WriteLine("global " & saveString & ";")


            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                Else
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                End If
            Next


            .WriteLine("}")



            .WriteLine("function Save() {")

            .WriteLine("global " & saveString & ";")
            .WriteLine("$obj = new " & nomSimple & "($ID);")



            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                Else
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                End If
            Next

            .WriteLine("$obj->Save(""Admin"");")
            .WriteLine("ApplicationHelper::Dialog_PopUP(""Sauvegarde efféctuée"");")
            .WriteLine("ApplicationHelper::Refresh_MainPage();")
            .WriteLine("}")


            .WriteLine("?>")


        End With


        objWriter.WriteLine("<!DOCTYPE html PUBLIC ""-//W3C// DTD XHTML 1.0 Transitional//EN"" ""http://www.w3org/TR/XHTML1/DTD/xhtml1-transitional.dtd ""  ")
        objWriter.WriteLine("<html xmlns=""http://www.w3.org/1999/xhtml"">  ")
        objWriter.WriteLine("<head>")

        objWriter.WriteLine("<meta http-equiv= ""Content-Type"" content=""text/html; charset=utf-8"" />")

        objWriter.WriteLine("<link rel=""shortcut icon"" href=""icon.ico"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")

        objWriter.WriteLine("</head>")

        objWriter.WriteLine("<body>")
        objWriter.WriteLine("<center>")
        objWriter.WriteLine(" <table id=""global"" width=""800px"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F7F7F7;  border: 3px solid #66A6CC"">")
        objWriter.WriteLine(" <tr valign=""top"">")
        objWriter.WriteLine(" <td valign=""top"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <table cellspacing=""0"" cellpadding=""0"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <tr valign=""top"" style=""background-color: #FFFFFF; height: 100px; width: 100%; border-bottom-color: #66A6CC;border-bottom-style : solid"">")
        objWriter.WriteLine(" <td align=""left"" style=""width: 10%"">")
        objWriter.WriteLine(" <asp:Image ID=""imgHeader"" runat=""server"" ImageUrl=""~/images/img_default.png"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine(" <td align=""left"" valign=""top"" style=""width: 90%"">")
        objWriter.WriteLine(" <h1 Class=""popupFormTitleLabel"" >")
        objWriter.WriteLine(" <label ID=""lbl_title"" class=""popupFormTitleLabel""   text=""Gestion des " & nomSimple & """   runat=""server""></label>")
        objWriter.WriteLine("</h1>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")

        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td style=""width: 100%;"" colspan=""2"" valign=""top"">")
        objWriter.WriteLine("<hr class=""hrHeaderAfterPhoto"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")


        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td valign=""top"" style=""width: 100%;"" colspan=""2"">")

        objWriter.WriteLine("<form name=""frm_" & nomSimple & " method=""post"" action="" frm_" & nomSimple & ".php"" >")



        objWriter.WriteLine(" <table border=""0"" class=""form_Enregistrement"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")
        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine(" <td style=""width: 100%;"">")
        objWriter.WriteLine("<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")



        Dim countColumn As Integer = 0
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then

                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & columnName & """ class=""popupFormInputLabel"" "">" & columnName & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadComboBox ID=""rcmb_" & columnName & """  Filter=""Contains""  AutoPostBack=""true""  Width=""50%"" runat=""server""> " & Chr(13) & " </telerik:RadComboBox> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType <> "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""text"" id=""txt_" & column.Name & """ name=""txt_" & column.Name & """  value=""<?php echo $txt_" & column.Name & "; ?>""  Width=""50%"" > " & Chr(13) & " </input> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf column.TrueSqlServerType = "date" And column.TrueSqlServerType = "datetime" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadDatePicker ID=""rdp_" & column.Name & """ Width=""50%"" runat=""server""" & _
                                     "DateInput-DateFormat=""dd/MM/yyyy"" MinDate=""1900-01-01""  ToolTip=""Cliquer sur le bouton pour choisir une date"" > " & Chr(13) & " </telerik:RadDatePicker>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")

                ElseIf column.TrueSqlServerType = "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""checkbox"" name=""chk_" & column.Name & """ width=""50%"" >" & _
                                      " </input>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                End If
            End If
            countColumn = countColumn + 1

        Next

        objWriter.WriteLine("<tr>")
        objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
        objWriter.WriteLine("<input type=""submit"" name=""btnSave"" value=""Sauvegarder"" > ")
        objWriter.WriteLine("</input>")
        objWriter.WriteLine("&nbsp;")
        objWriter.WriteLine("<input type=""submit"" name=""btnCancel"" value= ""Abandonner"" >")
        objWriter.WriteLine("</input>")

        objWriter.WriteLine("</td>")

        objWriter.WriteLine("</tr>")
        objWriter.WriteLine(" </table>")
        objWriter.WriteLine(" </td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")

        objWriter.WriteLine("</form>")
        objWriter.WriteLine("  </td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")

        objWriter.WriteLine("<br />")
        objWriter.WriteLine("</center>")
        objWriter.WriteLine("<input type=""text"" id=""txt_CodeHid" & nomSimple & """ name=""txt_" & nomSimple & """  value=""0""  width=""0px"" > " & Chr(13) & " </input> ")

        objWriter.WriteLine("</body>")

        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateInterfaceCodeHTmlPHP(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByVal databasename As String)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "frm").Replace("Tbl", "frm").Replace("TBL", "frm")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".view.php"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)

        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine()

        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer
        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name
        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")


        With objWriter
            .WriteLine("<?php require_once('../Model/Specialisation.class.php'); ?>")
            .WriteLine("<?php require_once('../DAL/MySQL_Helper.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ValidationFunction.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ApplicationHelper.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/QueryConnexion.php'); ?>")
        End With

        With objWriter
            .WriteLine("<?php")
            .WriteLine("//if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//    ApplicationHelper::Refresh_MainPage();")
            .WriteLine("//}")
            .WriteLine("$PageName = 'frm_Specialisation.view.php';")
            .WriteLine("$Btn_Save = 'btnSave';")
            .WriteLine("$Btn_Annuler = 'btnCancel';")
            .WriteLine("//        if( !isset($_SESSION[$GLOBAL_SESSION]) )")
            .WriteLine("//	{")
            .WriteLine("//		Cls_Parameters::Refresh_MainPage();")
            .WriteLine("//	}else{")
            .WriteLine("//		$Is_Acces_Page = 'display:block;';")
            .WriteLine("//		if(!Select_Privilege($PageName,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql))")
            .WriteLine("//		{// No acces a la page")
            .WriteLine("//			Cls_Parameters::Dialog_PopUP($NO_ACCES_PAGE);")
            .WriteLine("//			$_message = Cls_Parameters::Show_Message($NO_ACCES_PAGE);")
            .WriteLine("//			$Is_Acces_Page = 'display:none;';")
            .WriteLine("//		}else{")
            .WriteLine("//			$Btn_Save = (Select_Privilege($Btn_AddEdit,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql)) ? '' : 'display:none;' ;")
            .WriteLine("//		}//")
            .WriteLine("//	}//   ")
            .WriteLine("?>")

        End With

        With objWriter
            .WriteLine("<?php")
            .WriteLine("$ID = (isset($_GET[""ID""])) ? $_GET[""ID""] : '0';")


            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                ElseIf types(i) = "Boolean" Then
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                Else
                    objWriter.WriteLine("$txt_" & columnToUse & " = ValidationFunction::test_input($_POST['txt_" & columnToUse & "']);")
                End If
            Next


            .WriteLine("if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//	$_SESSION[$GLOBAL_SESSION_PAGENAME] = ""../Administration/Entreprise.php"";")
            .WriteLine("//	Cls_Parameters::Refresh_MainPage();")
            .WriteLine("}")
            .WriteLine("$IsPostBack = false;")
            .WriteLine("if ($_POST[""btnCancel""] == ""Abandonner"") {")
            .WriteLine("ApplicationHelper::Refresh_MainPage(false);")
            .WriteLine("}//")
            .WriteLine("if (isset($_POST['btnSave'])) {")

            Dim saveString As String = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next
            .WriteLine("btnSaveClick(" & saveString & ");")


            .WriteLine("}")
            .WriteLine("if (!$IsPostBack) {")
            .WriteLine("if (isset($_GET['ID']) && isset($_GET['Action']) && is_numeric($_GET['ID'])) {")
            .WriteLine("global $ID;")
            .WriteLine("try {")
            .WriteLine("switch ($_GET['Action']) {")
            .WriteLine("case 'Edit': {")
            .WriteLine("$obj = new" & nomSimple & "($ID);")

            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                Else
                    .WriteLine("$txt" & columnToUse & " = $obj->get_" & columnToUse & "();")
                End If
            Next



            .WriteLine("                    break;")
            .WriteLine("}")
            .WriteLine("default: ")
            .WriteLine("}")
            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();	")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("	    $_message = ApplicationHelper::Show_Message($_message,'S','../../');")
            .WriteLine("}")
            .WriteLine("}")
            .WriteLine("}")


            saveString = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next

            .WriteLine("function btnSaveClick(" & saveString & ") { ")


            .WriteLine("$IsPostBack = true;")
            .WriteLine("try {")
            .WriteLine("Validation();")


            saveString = "$ID"
            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    saveString = saveString & ", txt_" & columnToUse
                ElseIf types(i) = "Boolean" Then
                    saveString = saveString & ", txt_" & columnToUse
                Else
                    saveString = saveString & ", txt_" & columnToUse
                End If
            Next

            .WriteLine("Save(" & saveString & ");")




            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("$_message = ApplicationHelper::Show_Message($_message, 'S');")
            .WriteLine("}")
            .WriteLine("}")

            .WriteLine("function Validation() {")

            .WriteLine("global " & saveString & ";")


            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                Else
                    .WriteLine("if ($txt_" & columnToUse & " == '') {")
                    .WriteLine("throw (new Exception(""le " & columnToUse & " est obligatoire!""));")
                    .WriteLine("}")
                End If
            Next


            .WriteLine("}")



            .WriteLine("function Save() {")

            .WriteLine("global " & saveString & ";")
            .WriteLine("$obj = new " & nomSimple & "($ID);")



            For i As Int32 = 1 To cols.Count - 3
                Dim columnToUse As String = cols(i).Substring(1, cols(i).Length - 1)
                If ListofForeignKey.Contains(cols(i)) Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                ElseIf types(i) = "Date" Or types(i) = "DateTime" Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                ElseIf types(i) = "Boolean" Then
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                Else
                    .WriteLine("$obj->set_" & columnToUse & "($txt_" & columnToUse & ");")
                End If
            Next

            .WriteLine("$obj->Save(""Admin"");")
            .WriteLine("ApplicationHelper::Dialog_PopUP(""Sauvegarde efféctuée"");")
            .WriteLine("ApplicationHelper::Refresh_MainPage();")
            .WriteLine("}")


            .WriteLine("?>")


        End With


        objWriter.WriteLine("<!DOCTYPE html PUBLIC ""-//W3C// DTD XHTML 1.0 Transitional//EN"" ""http://www.w3org/TR/XHTML1/DTD/xhtml1-transitional.dtd ""  ")
        objWriter.WriteLine("<html xmlns=""http://www.w3.org/1999/xhtml"">  ")
        objWriter.WriteLine("<head>")

        objWriter.WriteLine("<meta http-equiv= ""Content-Type"" content=""text/html; charset=utf-8"" />")

        objWriter.WriteLine("<link rel=""shortcut icon"" href=""icon.ico"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")

        objWriter.WriteLine("</head>")

        objWriter.WriteLine("<body>")
        objWriter.WriteLine("<center>")
        objWriter.WriteLine(" <table id=""global"" width=""800px"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F7F7F7;  border: 3px solid #66A6CC"">")
        objWriter.WriteLine(" <tr valign=""top"">")
        objWriter.WriteLine(" <td valign=""top"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <table cellspacing=""0"" cellpadding=""0"" style=""width: 100%; height: 100%;"">")
        objWriter.WriteLine(" <tr valign=""top"" style=""background-color: #FFFFFF; height: 100px; width: 100%; border-bottom-color: #66A6CC;border-bottom-style : solid"">")
        objWriter.WriteLine(" <td align=""left"" style=""width: 10%"">")
        objWriter.WriteLine(" <asp:Image ID=""imgHeader"" runat=""server"" ImageUrl=""~/images/img_default.png"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine(" <td align=""left"" valign=""top"" style=""width: 90%"">")
        objWriter.WriteLine(" <h1 Class=""popupFormTitleLabel"" >")
        objWriter.WriteLine(" <label ID=""lbl_title"" class=""popupFormTitleLabel""   text=""Gestion des " & nomSimple & """   runat=""server""></label>")
        objWriter.WriteLine("</h1>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")

        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td style=""width: 100%;"" colspan=""2"" valign=""top"">")
        objWriter.WriteLine("<hr class=""hrHeaderAfterPhoto"" />")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")


        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine("<td valign=""top"" style=""width: 100%;"" colspan=""2"">")

        objWriter.WriteLine("<form name=""frm_" & nomSimple & " method=""post"" action="" frm_" & nomSimple & ".php"" >")



        objWriter.WriteLine(" <table border=""0"" class=""form_Enregistrement"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")
        objWriter.WriteLine("<tr valign=""top"">")
        objWriter.WriteLine(" <td style=""width: 100%;"">")
        objWriter.WriteLine("<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")



        Dim countColumn As Integer = 0
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then

                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & columnName & """ class=""popupFormInputLabel"" "">" & columnName & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadComboBox ID=""rcmb_" & columnName & """  Filter=""Contains""  AutoPostBack=""true""  Width=""50%"" runat=""server""> " & Chr(13) & " </telerik:RadComboBox> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType <> "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""text"" id=""txt_" & column.Name & """ name=""txt_" & column.Name & """  value=""<?php echo $txt_" & column.Name & "; ?>""  Width=""50%"" > " & Chr(13) & " </input> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf column.TrueSqlServerType = "date" And column.TrueSqlServerType = "datetime" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadDatePicker ID=""rdp_" & column.Name & """ Width=""50%"" runat=""server""" & _
                                     "DateInput-DateFormat=""dd/MM/yyyy"" MinDate=""1900-01-01""  ToolTip=""Cliquer sur le bouton pour choisir une date"" > " & Chr(13) & " </telerik:RadDatePicker>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")

                ElseIf column.TrueSqlServerType = "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""checkbox"" name=""chk_" & column.Name & """ width=""50%"" >" & _
                                      " </input>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                End If
            End If
            countColumn = countColumn + 1

        Next

        objWriter.WriteLine("<tr>")
        objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
        objWriter.WriteLine("<input type=""submit"" name=""btnSave"" value=""Sauvegarder"" > ")
        objWriter.WriteLine("</input>")
        objWriter.WriteLine("&nbsp;")
        objWriter.WriteLine("<input type=""submit"" name=""btnCancel"" value= ""Abandonner"" >")
        objWriter.WriteLine("</input>")

        objWriter.WriteLine("</td>")

        objWriter.WriteLine("</tr>")
        objWriter.WriteLine(" </table>")
        objWriter.WriteLine(" </td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")

        objWriter.WriteLine("</form>")
        objWriter.WriteLine("  </td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("</tr>")
        objWriter.WriteLine("</table>")

        objWriter.WriteLine("<br />")
        objWriter.WriteLine("</center>")
        objWriter.WriteLine("<input type=""text"" id=""txt_CodeHid" & nomSimple & """ name=""txt_" & nomSimple & """  value=""0""  width=""0px"" > " & Chr(13) & " </input> ")

        objWriter.WriteLine("</body>")

        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateGridPhpForListing(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByRef txt_libraryname As TextBox, ByVal databasename As String)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomWebform As String = name.Replace("tbl", "wbfrm").Replace("Tbl", "wbfrm").Replace("TBL", "wbfrm")
        Dim nomClasse As String = name.Replace("tbl", "Cls").Replace("Tbl", "Cls").Replace("TBL", "Cls")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomWebform & "Grid.php"

        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)
        Dim header As String = "REM Generate By [GENERIC 12] Application *******" & Chr(13) _
                               & "REM  Class " + nomWebform & Chr(13) & Chr(13) _
                               & "REM Date:" & Date.Now.ToString("dd-MMM-yyyy H\h:i:s\m")
        header &= ""
        ' Dim content As String = "Partial Class " & nomWebform & "Listing" & Chr(13) _
        '                         & " Inherits BasePage"

        Dim content As String = ""
        _end = "End Class" & Chr(13)
        _end = ""
        ' Delete the file if it exists.
        If File.Exists(path) Then
            File.Delete(path)
        End If

        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If
        ' Create the file.
        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()



        Dim objWriter As New System.IO.StreamWriter(path, True)
        'objWriter.WriteLine(header)
        If ListBox_NameSpace.Items.Count > 0 Then
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                objWriter.WriteLine(ListBox_NameSpace.Items(i))
            Next
        End If
        Dim libraryname As String = "Imports " & txt_libraryname.Text
        '  objWriter.WriteLine("Imports Telerik.Web.UI")
        ' objWriter.WriteLine(libraryname)
        'objWriter.WriteLine("Imports System.Data")
        'objWriter.WriteLine("Imports SolutionsHT")
        'objWriter.WriteLine("Imports SolutionsHT.DataAccessLayer")
        objWriter.WriteLine()

        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        objWriter.WriteLine(content)
        objWriter.WriteLine()


        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0

        Dim cap As Integer

        cap = _table.ListofColumn.Count


        Id_table = _table.ListofColumn.Item(0).Name

        For Each _index As Cls_UniqueIndex In _table.ListofIndex
            ListofIndex.Insert(countindex, _index.ListofColumn.Item(0).Name)
            countindex = countindex + 1
        Next

        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}


        For Each _foreignkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreignkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next


        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next


        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")

        With objWriter
            .WriteLine(" <?php")
            .WriteLine("  require_once() '../Model/" & nomSimple & ".class.php';")
            .WriteLine(" require_once() '../Persistance/MySQL_Helper.class.php';")
            .WriteLine("  require_once() '../GridHelper/grid.php';")
            .WriteLine("  require_once() '../ServTech/PaginationHelper.php';")
            .WriteLine("require_once('../ServTech/ApplicationHelper.php');")
            .WriteLine("?>")
        End With

        Dim countColumn As Integer = 0

        With objWriter
            .WriteLine("<div id=""gridContainer"" class=""divEncadrementGridPrime"">")
            .WriteLine("<div  id=""dialog"" title=""Nouvelle " & nomSimple & """ ></div>")

            .WriteLine("   <script type=""text/javascript"">")
            .WriteLine(" var i = 0;")
            .WriteLine(" //        $(function() {")
            .WriteLine(" //            $(""#dialog"").dialog({")
            .WriteLine(" //                autoOpen: false,")
            .WriteLine(" //                height: 230,")
            .WriteLine(" //                width: 500,")
            .WriteLine(" //                modal: true});")
            .WriteLine(" //            $(""#btnAdd" & nomSimple & """).click(function() {")
            .WriteLine(" //                $(""#dialog"").load('frm_" & nomSimple & ".view.php').dialog(""open"");")
            .WriteLine(" //            });")
            .WriteLine(" //        });")

            .WriteLine("         function modalWin() {")
            .WriteLine(" if (window.showModalDialog) {")
            .WriteLine(" window.showModalDialog(""frm_" & nomSimple & ".view.php"", ""name"",")
            .WriteLine(" ""dialogWidth:800px;dialogHeight:250px"");")
            .WriteLine(" } else {")
            .WriteLine(" window.open('frm_" & nomSimple & ".view.php', 'name',")
            .WriteLine(" 'height=800,width=250,toolbar=no,directories=no,status=no,\n\")
            .WriteLine(" menubar = no, scrollbars = no, resizable = no, modal = yes');")
            .WriteLine(" }")
            .WriteLine(" }")
            .WriteLine(" function editRow(id) {")
            .WriteLine(" $(""#dialog"").load('frm_" & nomSimple & "" & nomSimple & "" & nomSimple & ".view.php?ID=' + id).dialog(""open"");")
            .WriteLine(" }")
            .WriteLine(" function changePage(value, currentPage) {")


            Dim varcols As String = "var columnId"
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    varcols = varcols + ", column" + column.Name
                End If
                countColumn = countColumn + 1
            Next

            varcols = varcols + ";"

            .WriteLine(varcols)

            .WriteLine(" var elementPerPage, elementToSkyp, firstPaginator, prevPaginator, nextPaginator, lastPaginator, totalPage;")
            .WriteLine(" totalPage = document.getElementById(""totalPage"").value;")
            .WriteLine(" columnId = document.getElementById(""columnId"").value;")


            .WriteLine(" columnDescription = document.getElementById(""columnDescription"").value;")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" column" & column.Name & " = document.getElementById(""column" & column.Name & """).value;")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine(" switch (value) {")
            .WriteLine(" case 1:")
            .WriteLine(" document.getElementById(""firstPaginator"").value = 1;")
            .WriteLine(" firstPaginator = 1;")
            .WriteLine(" prevPaginator = 0;")
            .WriteLine(" nextPaginator = 0;")
            .WriteLine(" lastPaginator = 0;")
            .WriteLine(" break;")
            .WriteLine(" case 2:")
            .WriteLine(" document.getElementById(""prevPaginator"").value = 1;")
            .WriteLine(" firstPaginator = 0;")
            .WriteLine(" prevPaginator = 1;")
            .WriteLine(" nextPaginator = 0;")
            .WriteLine(" lastPaginator = 0;")
            .WriteLine(" break;")
            .WriteLine(" case 3:")
            .WriteLine(" document.getElementById(""nextPaginator"").value = 1;")
            .WriteLine(" firstPaginator = 0;")
            .WriteLine(" prevPaginator = 0;")
            .WriteLine(" nextPaginator = 1;")
            .WriteLine(" lastPaginator = 0;")
            .WriteLine(" break;")
            .WriteLine(" case 4:")
            .WriteLine(" document.getElementById(""lastPaginator"").value = 1;")
            .WriteLine(" firstPaginator = 0;")
            .WriteLine(" prevPaginator = 0;")
            .WriteLine(" nextPaginator = 0;")
            .WriteLine(" lastPaginator = 1;")
            .WriteLine(" break;")
            .WriteLine(" default:")
            .WriteLine(" firstPaginator = 0;")
            .WriteLine(" prevPaginator = 0;")
            .WriteLine(" nextPaginator = 0;")
            .WriteLine(" lastPaginator = 0;")
            .WriteLine(" break;")
            .WriteLine(" }")

            .WriteLine("             elementPerPage = document.getElementById(""ddl_Paginator"").value;")
            .WriteLine(" elementToSkyp = elementPerPage * (currentPage - 1);")
            .WriteLine(" getTagContent('frm_SpecialisationGrid.php?' +")
            .WriteLine(" 'elementPerPage=' + elementPerPage +")
            .WriteLine(" '&elementToSkyp=' + elementToSkyp +")
            .WriteLine(" '&currentPage=' + currentPage +")
            .WriteLine(" '&firstPaginator=' + firstPaginator +")
            .WriteLine(" '&prevPaginator=' + prevPaginator +")
            .WriteLine(" '&nextPaginator=' + nextPaginator +")
            .WriteLine(" '&lastPaginator=' + lastPaginator +")
            .WriteLine(" '&columnId=' + columnId +")
            .WriteLine(" '&columnCode=' + columnCode +")
            .WriteLine(" '&columnDescription=' + columnDescription +")
            .WriteLine(" '', 'gridContainer');")
            .WriteLine(" }")
            .WriteLine(" function orderColumn(value, currentPage) {")
            .WriteLine(varcols)
            .WriteLine(" var elementPerPage, elementToSkyp, firstPaginator, prevPaginator, nextPaginator, lastPaginator, totalPage;")
            .WriteLine(" totalPage = document.getElementById(""totalPage"").value;")



            .WriteLine(" columnId = document.getElementById(""columnId"");")



            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" column" & column.Name & " = document.getElementById(""column" & column.Name & """).value;")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine("             firstPaginator = document.getElementById(""firstPaginator"").value;")
            .WriteLine(" prevPaginator = document.getElementById(""prevPaginator"").value;")
            .WriteLine(" nextPaginator = document.getElementById(""nextPaginator"").value;")
            .WriteLine(" lastPaginator = document.getElementById(""lastPaginator"").value;")
            .WriteLine(" switch (value) {")
            .WriteLine(" case 1:")
            .WriteLine(" if (columnId.value == ""NONE"") {")
            .WriteLine(" columnId.value = ""ASC"";")



            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" column" & column.Name & ".value = ""NONE"";")
                End If
                countColumn = countColumn + 1
            Next



            .WriteLine(" } else if (columnId.value == ""ASC"") {")
            .WriteLine(" columnId.value = ""DESC"";")
            .WriteLine(" columnCode.value = ""NONE"";")
            .WriteLine(" columnDescription.value = ""NONE"";")
            .WriteLine(" }")
            .WriteLine(" else if (columnId.value == ""DESC"") {")

            .WriteLine(" columnId.value = ""ASC"";")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" column" & column.Name & ".value = ""NONE"";")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine(" }")
            .WriteLine(" break;")


            countColumn = 2
            Dim compteurCol As Integer = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" case " & compteurCol & ":")
                    .WriteLine(" if (column" & column.Name & ".value == ""NONE"") {")
                    .WriteLine(" column" & column.Name & ".value = ""ASC"";")
                    .WriteLine(" columnId.value = ""NONE"";")
                    For Each columninner As Cls_Column In _table.ListofColumn
                        If countColumn < _table.ListofColumn.Count - 4 Then
                            If columninner.Name <> column.Name Then
                                .WriteLine(" column" & columninner.Name & "= ""NONE"";")
                            End If
                        End If
                    Next
                    compteurCol = compteurCol + 1
                    .WriteLine(" } else if (column" & column.Name & ".value == ""ASC"") {")
                    .WriteLine(" column" & column.Name & ".value = ""DESC"";")
                    .WriteLine(" columnId.value = ""NONE"";")
                    compteurCol = 0
                    For Each columninner As Cls_Column In _table.ListofColumn
                        If countColumn < _table.ListofColumn.Count - 4 Then
                            If columninner.Name <> column.Name Then
                                .WriteLine(" column" & columninner.Name & "= ""NONE"";")
                            End If
                        End If
                    Next

                    .WriteLine(" }")

                    .WriteLine(" else if (column" & column.Name & ".value == ""DESC"") {")
                    .WriteLine(" column" & column.Name & ".value = ""ASC"";")
                    .WriteLine(" columnId.value = ""NONE"";")
                    compteurCol = 0
                    For Each columninner As Cls_Column In _table.ListofColumn
                        If countColumn < _table.ListofColumn.Count - 4 Then
                            If columninner.Name <> column.Name Then
                                .WriteLine(" column" & columninner.Name & "= ""NONE"";")
                            End If
                        End If
                    Next
                    .WriteLine(" }")
                End If
                countColumn = countColumn + 1
                .WriteLine(" break;")
                .WriteLine(" }")
            Next


            .WriteLine(" elementPerPage = document.getElementById(""ddl_Paginator"").value;")
            .WriteLine(" elementToSkyp = elementPerPage * (currentPage - 1);")

            .WriteLine(" getTagContent('frm_SpecialisationGrid.php?' +")
            .WriteLine(" 'elementPerPage=' + elementPerPage +")
            .WriteLine(" '&elementToSkyp=' + elementToSkyp +")
            .WriteLine(" '&currentPage=' + currentPage +")
            .WriteLine(" '&firstPaginator=' + firstPaginator +")
            .WriteLine(" '&prevPaginator=' + prevPaginator +")
            .WriteLine(" '&nextPaginator=' + nextPaginator +")
            .WriteLine(" '&lastPaginator=' + lastPaginator +")
            .WriteLine(" '&columnId=' + columnId.value +")


            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" '&column" & column.Name & "=' + column" & column.Name & ".value +")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine(" '', 'gridContainer');")
            .WriteLine(" //                  document.forms[""form1""].submit();")
            .WriteLine(" }")

            .WriteLine(" </script>")


            .WriteLine("<div class=""divHeaderEncadrementGridPrime"">")
            .WriteLine(" Liste des " & nomSimple & "")
            .WriteLine("</div>")
            .WriteLine(" <div class=""divBodyEncadrementGridPrime"">")
            .WriteLine("<table style=""border:1px solid #A8A8A8; width: 100%;"" cellspacing=""0"">")
        End With


        countColumn = 0
        With objWriter
            .WriteLine(" <?php")
            .WriteLine(" $ElementPerPage = 5;")
            .WriteLine("  $elementToSkyp = 0;")
            .WriteLine(" $currentPage = 1;")
            .WriteLine(" $paginatorFromTo = "";")
            .WriteLine(" $columnId = ""NONE"";")
            .WriteLine(" $theadColumnId = ""gridPrimeth"";")
            .WriteLine(" $spanId = ""gridPrimethSpanSortNONE"";")


            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("  $column" & column.Name & " = ""NONE"";")
                    .WriteLine("  $theadColumn" & column.Name & "  = ""gridPrimeth"";")
                    .WriteLine("  $span" & column.Name & "  = ""gridPrimethSpanSortNONE"";")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine(" $columnToSort = "";")
            .WriteLine(" $sortingValue = ""NONE"";")
            .WriteLine(" $firstPaginator = 0;")
            .WriteLine(" $prevPaginator = 0;")
            .WriteLine("  $nextPaginator = 0;")
            .WriteLine(" $lastPaginator = 0;")
            .WriteLine(" $totalPage = 0;")

            .WriteLine("if (!empty($_GET)) {")
            .WriteLine("   $ElementPerPage = filter_input(INPUT_GET, 'elementPerPage', FILTER_VALIDATE_INT);")
            .WriteLine("   $elementToSkyp = filter_input(INPUT_GET, 'elementToSkyp', FILTER_VALIDATE_INT);")
            .WriteLine("   $currentPage = filter_input(INPUT_GET, 'currentPage', FILTER_VALIDATE_INT);")
            .WriteLine("  $firstPaginator = filter_input(INPUT_GET, 'firstPaginator', FILTER_VALIDATE_INT);")
            .WriteLine("   $prevPaginator = filter_input(INPUT_GET, 'prevPaginator', FILTER_VALIDATE_INT);")
            .WriteLine("  $nextPaginator = filter_input(INPUT_GET, 'nextPaginator', FILTER_VALIDATE_INT);")
            .WriteLine(" $lastPaginator = filter_input(INPUT_GET, 'lastPaginator', FILTER_VALIDATE_INT);")
            .WriteLine("  $columnId = filter_input(INPUT_GET, 'columnId', FILTER_SANITIZE_STRING);")


            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("  $column" & column.Name & " = filter_input(INPUT_GET, 'column" & column.Name & "', FILTER_SANITIZE_STRING);")
                End If
                countColumn = countColumn + 1
            Next
            .WriteLine(" }")
        End With
        With objWriter


            .WriteLine("ParametizeGrid();")

        End With

        With objWriter
            .WriteLine("function ParametizeGrid() {")
            .WriteLine(" global $ElementPerPage, $currentPage, $elementToSkyp, $paginatorFromTo, $columnToSort, $sortingValue;")
            .WriteLine(" global $firstPaginator, $prevPaginator, $nextPaginator, $lastPaginator, $totalPage;")

            countColumn = 0
            Dim columnStr As String = " global $columnId "
            Dim spanStr As String = " global $spanId "
            Dim theadColumnStr As String = " global $theadColumnId "

            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    columnStr = columnStr & ", $column" & column.Name
                    spanStr = spanStr & ", $span" & column.Name
                    theadColumnStr = theadColumnStr & ", $theadColumn" & column.Name

                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("" & columnStr & ";")
            .WriteLine("" & spanStr & ";")
            .WriteLine("" & theadColumnStr & ";")

            .WriteLine(" $paginatorFromTo = ($elementToSkyp + 1) . "" of "" . $ElementPerPage;")
            .WriteLine("  $objs_count = Specialisation::SearchAll();")
            .WriteLine("$helper = new PaginationHelper(count($objs_count), $ElementPerPage);")
            .WriteLine(" $totalPage = $helper->numberOfPage;")

            .WriteLine(" if ($columnId != ""NONE"") {")
            .WriteLine("    $columnToSort = """ & _table.PrimaryKey.Name & """;")
            .WriteLine("    $spanId = ($columnId == ""ASC"") ? ""gridPrimethSpanSortASC"" : ""gridPrimethSpanSortDESC"";")
            .WriteLine("    $theadColumnId = ($columnId == ""ASC"") ? ""gridPrimethSortASC"" : ""gridPrimethSortDESC"";")
            .WriteLine("   $sortingValue = $columnId;")
            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" } else if ($column" & column.Name & " != ""NONE"") {")
                    .WriteLine("     $columnToSort = """ & column.Name & """;")
                    .WriteLine("     $span" & column.Name & " = ($column" & column.Name & " == ""ASC"") ? ""gridPrimethSpanSortASC"" : ""gridPrimethSpanSortDESC"";")
                    .WriteLine("     $theadColumn" & column.Name & " = ($column" & column.Name & " == ""ASC"") ? ""gridPrimethSortASC"" : ""gridPrimethSortDESC"";")
                    .WriteLine("    $sortingValue = $column" & column.Name & ";")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine(" }")

            .WriteLine("  if ($firstPaginator == 1) {")
            .WriteLine("     $currentPage = 1;")
            .WriteLine("     $elementToSkyp = 0;")
            .WriteLine("  } else if ($prevPaginator == 1) {")
            .WriteLine("     if ($currentPage != 1) {")
            .WriteLine("          $currentPage = $currentPage - 1;")
            .WriteLine("     }")
            .WriteLine("  } else if ($nextPaginator == 1) {")
            .WriteLine("      if ($currentPage != $helper->numberOfPage) {")
            .WriteLine("         $currentPage = $currentPage + 1;")
            .WriteLine("      }")
            .WriteLine("  } else if ($lastPaginator == 1) {")
            .WriteLine("      $currentPage = $helper->numberOfPage;")
            .WriteLine(" } else {")
            .WriteLine("      $currentPage = 1;")
            .WriteLine("   }")

            .WriteLine("   $elementToSkyp = $ElementPerPage * ($currentPage - 1);")
            .WriteLine("   $paginatorFromTo = ""("" . ($currentPage) . "" of "" . $helper->numberOfPage . ("")"");")
            .WriteLine("     $i = 0;")
            .WriteLine("    $couleur = '';")
            .WriteLine("}")

            .WriteLine(" $objs = " & nomSimple & "::SearchAll_ForPagination($elementToSkyp, $ElementPerPage, $columnToSort, $sortingValue);")
            .WriteLine(" ?>")

        End With

        With objWriter
            .WriteLine("")

            .WriteLine(" <thead>")
            .WriteLine(" <tr class=""gridPrime"">")
            .WriteLine(" <th class=""<?php echo ""$theadColumnId"" ?>"" onclick=""orderColumn(1,<?php echo ""$currentPage"" ?>);""")
            .WriteLine("    align=""left"" scope='col'>Id")
            .WriteLine("  <span class=""<?php echo $spanId ?>""></span>")
            .WriteLine(" <input type=""hidden"" id=""columnId"" name=""columnId""")
            .WriteLine("      value=""<?php echo ""$columnId"" ?>""/>")
            .WriteLine(" </th>")

            countColumn = 0

            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" <th class=""<?php echo ""$theadColumn" & column.Name & """ ?>"" onclick=""orderColumn(" & countColumn + 1 & ",<?php echo ""$currentPage"" ?>);""")
                    .WriteLine("    align=""left"" scope='col'>" & column.Name & "")
                    .WriteLine("  <span class=""<?php echo $span" & column.Name & " ?>""></span>")
                    .WriteLine(" <input type=""hidden"" id=""column" & column.Name & """ name=""column" & column.Name & """")
                    .WriteLine("      value=""<?php echo ""$column" & column.Name & """ ?>""/>")
                    .WriteLine(" </th>")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine(" <th class=""gridPrimeth"" scope='col' style=""width:60px; white-space:nowrap;"">Actions</th>")
            .WriteLine(" </tr>")
            .WriteLine(" </thead>")

        End With

        With objWriter
            .WriteLine("<?php")

            .WriteLine("foreach ($objs as $obj) {")
            .WriteLine("  $id = $obj->get_ID();")

            countColumn = 0

            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine(" $" & column.Name & "$obj->get_" & column.Name & " ();"" ")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("   if ($i % 2 == 0) {")
            .WriteLine("     $couleur = '#EBEFF3';")
            .WriteLine("  } else {")
            .WriteLine("    $couleur = '#FCFCFC';")
            .WriteLine("}")
            .WriteLine(" $i++;")
            .WriteLine(" ?>")


            .WriteLine(" <tr style=""height: 30px"" bgcolor='<?php echo $couleur; ?>'")
            .WriteLine(" onclick = ""this.style.cursor = 'hand';""")
            .WriteLine("                    this.style.backgroundColor = '#DFEDFD'""")
            .WriteLine(" onmouseout = ""this.style.backgroundColor = '<?php echo $couleur; ?>'""")
            .WriteLine("        >")

            .WriteLine("<td style=""padding: 4px 10px;"">>")
            .WriteLine("       <label type=""label"" style=""font-size: 15px; font-family: Cambria;""><?php echo ""$id""; ?></label>>")
            .WriteLine(" </td>>")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("<td style=""padding: 4px 10px;"">")
                    .WriteLine("<td style=""padding: 4px 10px;"">")
                    .WriteLine("   <label type=""label"" style=""font-size: 15px; font-family: Cambria;""><?php echo ""$" & column.Name & """; ?></label>")
                    .WriteLine("</td>")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine("<td style=""padding: 4px 10px;"" nowrap=""nowrap"" align=""center"">&nbsp;&nbsp;")
            .WriteLine(" <td style=""padding: 4px 10px;"" nowrap=""nowrap"" align=""center"">&nbsp;&nbsp;")
            .WriteLine("     <a href=""javascript:Open_Window('frm_" & nomSimple & ".view.php?ID=<?php echo $id; ?>&Action=Edit','frm_" & nomSimple & ".view.php','800','500');""")
            .WriteLine("      title=""Modifier"">")
            .WriteLine("     <img alt=""Edit"" src=""../webroot/img/img_edit_row.png"" style=""width: 16px; cursor: pointer;""")
            .WriteLine("           border=""0""/></a>&nbsp;&nbsp;")
            .WriteLine("  <a href=""javascript:Open_Window('Equipes_Add.php?ID=<?php echo $id; ?>&Action=Delete','AddEditUtilisateur','800','400');""")
            .WriteLine("     style=""<?php echo $Btn_Delete; ?>"">")
            .WriteLine("     <img src=""../webroot/img/img_delete_row.png"" alt=""Delete"" width=""20"" border=""0""")
            .WriteLine("          style=""width: 16px; cursor: pointer;"" title=""Supprimer""")
            .WriteLine("         onclick="" return confirm('Êtes-vous sûr de vouloir supprimer ?');""/></a>")
            .WriteLine(" </td>")
            .WriteLine("  </tr>")
            .WriteLine(" <?php } // END While                 ?>")
            .WriteLine(" </table>")
            .WriteLine("  </div>")

        End With



        With objWriter
            .WriteLine(" <div class=""divFooterEncadrementGridPrime"">")
            .WriteLine("<span class=""spanCurrentPaginatorPage"">")
            .WriteLine("  <label id=""lbl_CurrentPaginatorPage"">")
            .WriteLine("      <?php echo ""$paginatorFromTo"" ?>")
            .WriteLine("  </label>")
            .WriteLine("  <input id=""currentPage"" type=""hidden"" name=""currentPage"" value=""<?php echo $currentPage ?>""/>")
            .WriteLine(" </span>")

            .WriteLine(" <span")
            .WriteLine("    onclick=""changePage(1,<?php echo ""$currentPage"" ?>);""")
            .WriteLine("    class=""spanPaginatorPage"">")
            .WriteLine("     <span class=""spanFirstPaginatorPageInner"">")
            .WriteLine("        <img style=""margin-top: 3px"" src=""../webroot/img/imgGrid_first.png""/>")
            .WriteLine("       <input id=""firstPaginator"" type=""hidden"" name=""firstPaginator"" value=""0""/>")
            .WriteLine("   </span>")
            .WriteLine("  </span>")
            .WriteLine("  <span")
            .WriteLine("     onclick=""changePage(2,<?php echo ""$currentPage"" ?>);""")
            .WriteLine("     class=""spanPaginatorPage"">")
            .WriteLine("     <span class=""spanPrevPaginatorPageInner"">")
            .WriteLine("         <img style=""margin-top: 3px"" src=""../webroot/img/imgGrid_prev.png""/>")
            .WriteLine("      <input id=""prevPaginator"" type=""hidden"" name=""prevPaginator"" value=""0""/>")
            .WriteLine("    </span>")
            .WriteLine(" </span>")
            .WriteLine(" <span")
            .WriteLine("    onclick=""changePage(3,<?php echo ""$currentPage"" ?>);""")
            .WriteLine("    class=""spanPaginatorPage"">")
            .WriteLine("    <span class=""spanNextPaginatorPageInner"">")
            .WriteLine("        <img style=""margin-top: 3px"" src=""../webroot/img/imgGrid_next.png""/>")
            .WriteLine("        <input id=""nextPaginator"" type=""hidden"" name=""nextPaginator"" value=""0""/>")
            .WriteLine("    </span>")
            .WriteLine(" </span>")
            .WriteLine(" <span")
            .WriteLine("    onclick=""changePage(4,<?php echo ""$currentPage"" ?>);""")
            .WriteLine("   class=""spanPaginatorPage"">")
            .WriteLine("   <span class=""spanLastPaginatorPageInner"">")
            .WriteLine("       <img style=""margin-top: 3px"" src=""../webroot/img/imgGrid_last.png"">")
            .WriteLine("        </img>")
            .WriteLine("        <input id=""lastPaginator"" type=""hidden"" name=""lastPaginator"" value=""0""/>")
            .WriteLine("     </span>")
            .WriteLine(" </span>")
            .WriteLine(" <label type=""hidden"" id=""ddl_PaginatorValue"">Page size:</label>")
            .WriteLine("  <select onchange=""changePage(0,<?php echo ""$currentPage"" ?>);"" id=""ddl_Paginator"" name=""ddl_Paginator""")
            .WriteLine("         class=""selectPaginatorPage"">")
            .WriteLine("            <?php")
            .WriteLine("           $array = array(")
            .WriteLine("                5 => ""5"",")
            .WriteLine("              10 => ""10"",")
            .WriteLine("               25 => ""25"",")
            .WriteLine("             50 => ""50"",")
            .WriteLine("         100 => ""100""")
            .WriteLine("       );")
            .WriteLine("    foreach ($array as $elem) {")
            .WriteLine("          ?>")
            .WriteLine("    <option")
            .WriteLine(" value = ""<?php""")
            .WriteLine("      ;")
            .WriteLine("        echo $elem")
            .WriteLine("            ?>""")
            .WriteLine("        <?php")
            .WriteLine("       if ($ElementPerPage == $elem) {")
            .WriteLine("echo() 'selected=""selected""';")
            .WriteLine("       }")
            .WriteLine("        ?>>")
            .WriteLine("        <?php echo $elem ?>")
            .WriteLine("   </option>")
            .WriteLine(" <?php } ?>")
            .WriteLine(" <input id=""totalPage"" name=""totalPage"" type=""hidden"" value=""<?php echo ""$totalPage"" ?>""/>")
            .WriteLine("</select>")
            .WriteLine("</div>")
            .WriteLine("</div>")

        End With


        objWriter.WriteLine()
        objWriter.Close()
    End Sub

    Public Shared Sub CreateInterfaceCodePHP(ByVal name As String, ByRef txt_PathGenerate_ScriptFile As TextBox, ByRef ListBox_NameSpace As ListBox, ByVal databasename As String)
        Dim _systeme As Cls_Systeme = Cls_Systeme.getInstance
        Dim Id_table As String = ""
        Dim _end As String
        Dim ListofForeignKey As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim db As String = ""
        Dim Lcasevalue As New List(Of String) From {"String"}
        Dim nomClasse As String = name.Replace("tbl", "frm").Replace("Tbl", "frm").Replace("TBL", "frm")
        Dim nomSimple As String = name.Substring(4, name.Length - 4)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & _systeme.currentDatabase.Name & "\PHPHTMLWebForm\")
        Dim path As String = txt_PathGenerate_Script & nomClasse & ".view.php"
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countindex As Long = 0
        Dim insertstring As String = ""
        Dim updatestring As String = ""
        Dim listoffound_virguleIndex As New List(Of String)

        If File.Exists(path) Then
            File.Delete(path)
        End If
        REM on verifie si le repertoir existe bien       
        If Not Directory.Exists(txt_PathGenerate_Script) Then
            Directory.CreateDirectory(txt_PathGenerate_Script)
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()
        Dim objWriter As New System.IO.StreamWriter(path, True)

        objWriter.WriteLine()

        objWriter.WriteLine()
        Dim _table As New Cls_Table()

        _table.Read(_systeme.currentDatabase.ID, name)

        Dim cols As New List(Of String)
        Dim types As New List(Of String)
        Dim initialtypes As New List(Of String)
        Dim length As New List(Of String)
        Dim count As Integer = 0
        Dim cap As Integer
        cap = _table.ListofColumn.Count
        Id_table = _table.ListofColumn.Item(0).Name
        Dim nottoputforlist As New List(Of String) From {Id_table, "_isdirty", "_LogData"}
        For Each _foreingkey As Cls_ForeignKey In _table.ListofForeinKey
            ListofForeignKey.Add("_" & _foreingkey.Column.Name)
            countForeignKey = countForeignKey + 1
        Next

        For Each _column As Cls_Column In _table.ListofColumn
            If count < cap - 4 Then
                cols.Add("_" & _column.Name)
                types.Add(_column.Type.VbName)
                initialtypes.Add(_column.Type.SqlServerName)
                length.Add(_column.Length)
                count += 1
            Else
                Exit For
            End If
        Next
        cols.Add("_isdirty")
        cols.Add("_LogData")
        types.Add("Boolean")
        types.Add("String")
        initialtypes.Add("Byte")
        initialtypes.Add("nvarchar")

        Dim countColumn As Integer = 0
        With objWriter
            .WriteLine("<?php require_once('../Model/" & nomSimple & ".class.php'); ?>")
            .WriteLine("<?php require_once('../DAL/MySQL_Helper.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ValidationFunction.class.php'); ?>")
            .WriteLine("<?php require_once('../ServTech/ApplicationHelper.php'); ?>")
        End With

        With objWriter
            .WriteLine("<?php")
            .WriteLine("//if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//    ApplicationHelper::Refresh_MainPage();")
            .WriteLine("//}")
            .WriteLine("$PageName = 'frm_Specialisation.view.php';")
            .WriteLine("$Btn_Save = 'btnSave';")
            .WriteLine("$Btn_Annuler = 'btnCancel';")
            .WriteLine("//        if( !isset($_SESSION[$GLOBAL_SESSION]) )")
            .WriteLine("//	{")
            .WriteLine("//		Cls_Parameters::Refresh_MainPage();")
            .WriteLine("//	}else{")
            .WriteLine("//		$Is_Acces_Page = 'display:block;';")
            .WriteLine("//		if(!Select_Privilege($PageName,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql))")
            .WriteLine("//		{// No acces a la page")
            .WriteLine("//			Cls_Parameters::Dialog_PopUP($NO_ACCES_PAGE);")
            .WriteLine("//			$_message = Cls_Parameters::Show_Message($NO_ACCES_PAGE);")
            .WriteLine("//			$Is_Acces_Page = 'display:none;';")
            .WriteLine("//		}else{")
            .WriteLine("//			$Btn_Save = (Select_Privilege($Btn_AddEdit,$_SESSION[$GLOBAL_SESSION_GROUPE],$ConString_MySql)) ? '' : 'display:none;' ;")
            .WriteLine("//		}//")
            .WriteLine("//	}//   ")


            .WriteLine("      $ID = (isset($_GET[""ID""])) ? $_GET[""ID""] : '0';")
            .WriteLine("$tmpEditState = false;")
            .WriteLine("if (isset($_GET['ID']) && is_numeric($_GET['ID'])) {")
            .WriteLine("$tmpEditState = true;")
            .WriteLine("}")

            .WriteLine("if (!isset($_SESSION[$GLOBAL_SESSION])) {")
            .WriteLine("//	$_SESSION[$GLOBAL_SESSION_PAGENAME] = ""../Administration/Entreprise.php"";")
            .WriteLine("//	Cls_Parameters::Refresh_MainPage();")
            .WriteLine("}")


            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("$txt_" & column.Name & " = """";")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("$btnSaveInput = 0 ;")
            .WriteLine("$txt_CodeHID = 0;")

            .WriteLine("if ($_SERVER['REQUEST_METHOD'] == 'POST') {")


            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("$txt_" & column.Name & " = ValidationFunction::test_input(filter_input(INPUT_POST, 'txt_" & column.Name & "', FILTER_SANITIZE_STRING));")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine("if ($_POST[""btnCancel""]) {")
            .WriteLine("ApplicationHelper::Refresh_MainPage(false);")
            .WriteLine("}")
            .WriteLine("if (isset($_POST['btnSave'])) {")
            .WriteLine("frm_" & nomSimple & "::btnSaveClick();")
            .WriteLine("}")
            .WriteLine("} else {")
            .WriteLine("frm_" & nomSimple & "::Setup();")
            .WriteLine("}")
            .WriteLine()

            .WriteLine("class frm_" & nomSimple & " {")
            .WriteLine()
            .WriteLine("static function Setup() {")
            .WriteLine("global $tmpEditState;")

            Dim globalPropert As String = "global $ID"
            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    globalPropert = globalPropert + " , "
                End If
                countColumn = countColumn + 1
            Next
            .WriteLine(globalPropert & ";")

            .WriteLine("if ($tmpEditState) {")
            .WriteLine("try {")
            .WriteLine("$obj = new " & nomSimple & "($ID);")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then

                    .WriteLine("$txt_" & column.Name & " = $obj->get_" & column.Name & "();")
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("$_message = ApplicationHelper::Show_Message($_message, 'S', '../../');")
            .WriteLine("}")
            .WriteLine("} else {")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("$txt_" & column.Name & " = """";")
                End If
                countColumn = countColumn + 1
            Next


            .WriteLine("}")
            .WriteLine("}")
            .WriteLine()

            .WriteLine("static function btnSaveClick() {")
            .WriteLine("try {")
            .WriteLine("frm_" & nomSimple & ":: Validation();")
            .WriteLine("frm_" & nomSimple & "::Save();")
            .WriteLine("} catch (Exception $ex) {")
            .WriteLine("$_message = $ex->getMessage();")
            .WriteLine("ApplicationHelper::Dialog_PopUP($_message);")
            .WriteLine("$_message = ApplicationHelper::Show_Message($_message, 'S');")
            .WriteLine("}")
            .WriteLine("}")
            .WriteLine()

            .WriteLine("static function Validation() {")
            .WriteLine(globalPropert & ";")

            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("if ($txt_" & column.Name & " == NULL) {")
                    .WriteLine("throw (new Exception(""le " & column.Name & " est obligatoire!""));")
                    .WriteLine("}")
                    .WriteLine()
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("}")


            .WriteLine("static function Save() {")
            .WriteLine(globalPropert & ";")
            .WriteLine("$obj = new " & nomSimple & "($ID);")


            countColumn = 0
            For Each column As Cls_Column In _table.ListofColumn
                Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
                Dim primary As String = _table.ListofColumn(0).Name
                If countColumn < _table.ListofColumn.Count - 4 Then
                    .WriteLine("$obj->set_" & column.Name & "($txt_" & column.Name & ");")
                    .WriteLine()
                End If
                countColumn = countColumn + 1
            Next

            .WriteLine("$obj->Save(""Blanc"");")
            .WriteLine("ApplicationHelper::Dialog_PopUP(""Sauvegarde Effectuée"");")
            .WriteLine("ApplicationHelper::Refresh_MainPage();")
            .WriteLine("}")
            .WriteLine("}")

            .WriteLine("?>")

        End With




        objWriter.WriteLine("<!DOCTYPE html PUBLIC ""-//W3C// DTD XHTML 1.0 Transitional//EN"" ""http://www.w3org/TR/XHTML1/DTD/xhtml1-transitional.dtd ""  ")
        objWriter.WriteLine("<html xmlns=""http://www.w3.org/1999/xhtml"">  ")
        objWriter.WriteLine("<head>")

        objWriter.WriteLine("<meta http-equiv= ""Content-Type"" content=""text/html; charset=utf-8"" />")

        objWriter.WriteLine("<link rel=""shortcut icon"" href=""icon.ico"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")
        objWriter.WriteLine("<link rel=""stylesheet"" type=""text/css"" href=""style/style.css"" />")

        objWriter.WriteLine("</head>")

        objWriter.WriteLine("<body>")
        objWriter.WriteLine("<center>")

        objWriter.WriteLine("  <div id=""divFormContainer"" class=""divFormContainer"">")
        objWriter.WriteLine("  <form id=""formPopup"" class=""form_Enregistrement"" style=""width: 100%"" name=""frm" & nomSimple & """ method=""post"" action="""" >")
        objWriter.WriteLine("     <div class=""divHeaderEncadrementGridPrime"">")
        objWriter.WriteLine("Nouvelle " & nomSimple & " ")
        objWriter.WriteLine("      </div> ")
        objWriter.WriteLine("      <div class=""divFormContent"" >")

        objWriter.WriteLine("<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")



        countColumn = 0
        For Each column As Cls_Column In _table.ListofColumn
            Dim listforeignkey As List(Of Cls_Column) = _table.ListofColumnForeignKey
            Dim primary As String = _table.ListofColumn(0).Name
            If countColumn < _table.ListofColumn.Count - 4 Then

                If _table.LisOfForeingKeyContainsThisColumn(column) And column.Type.SqlServerName <> "Date" And column.Name <> primary Then

                    Dim columnName As String = column.Name.Substring(ForeinKeyPrefix.Length, column.Name.Length - (ForeinKeyPrefix.Length))
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & columnName & """ class=""popupFormInputLabel"" "">" & columnName & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadComboBox ID=""rcmb_" & columnName & """  Filter=""Contains""  AutoPostBack=""true""  Width=""50%"" runat=""server""> " & Chr(13) & " </telerik:RadComboBox> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf Not _table.LisOfForeingKeyContainsThisColumn(column) And column.TrueSqlServerType <> "date" And column.TrueSqlServerType <> "datetime" And column.Name <> _table.ListofColumn(0).Name And column.TrueSqlServerType <> "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""text"" id=""txt_" & column.Name & """ name=""txt_" & column.Name & """  value=""<?php echo $txt_" & column.Name & "; ?>""  Width=""50%"" > " & Chr(13) & " </input> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                ElseIf column.TrueSqlServerType = "date" And column.TrueSqlServerType = "datetime" And column.Name <> _table.ListofColumn(0).Name Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")

                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<telerik:RadDatePicker ID=""rdp_" & column.Name & """ Width=""50%"" runat=""server""" & _
                                     "DateInput-DateFormat=""dd/MM/yyyy"" MinDate=""1900-01-01""  ToolTip=""Cliquer sur le bouton pour choisir une date"" > " & Chr(13) & " </telerik:RadDatePicker>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")

                ElseIf column.TrueSqlServerType = "bit" Then
                    objWriter.WriteLine("<tr>")
                    objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
                    objWriter.WriteLine("<label for=""" & column.Name & """ class=""popupFormInputLabel"" "">" & column.Name & ": </label> ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
                    objWriter.WriteLine("<input type=""checkbox"" name=""chk_" & column.Name & """ width=""50%"" >" & _
                                      " </input>  ")
                    objWriter.WriteLine("</td>")
                    objWriter.WriteLine("</tr>")
                End If
            End If
            countColumn = countColumn + 1

        Next

        objWriter.WriteLine("<tr>")
        objWriter.WriteLine("<td style=""width: 30%;"" align=""right"">")
        objWriter.WriteLine("</td>")
        objWriter.WriteLine("<td style=""width: 70%;"" align=""left"">")
        objWriter.WriteLine("<input type=""submit"" name=""btnSave"" value=""Sauvegarder"" > ")
        objWriter.WriteLine("</input>")
        objWriter.WriteLine("&nbsp;")
        objWriter.WriteLine("<input type=""submit"" name=""btnCancel"" value= ""Abandonner"" >")
        objWriter.WriteLine("</input>")

        objWriter.WriteLine("</td>")

        objWriter.WriteLine("</tr>")
        objWriter.WriteLine(" </table>")
        objWriter.WriteLine(" </div>")
        objWriter.WriteLine("   <div class=""divFormButtonSaveCancel"">")
        objWriter.WriteLine("      <button name=""btnSave"" type=""submit"" onclick=""""  value=""Sauvegarder"" class=""buttonPrime"">")
        objWriter.WriteLine("       <span  class=""buttonPrimeSpan"">")
        objWriter.WriteLine(" Sauvegarder()")
        objWriter.WriteLine("           </span>")
        objWriter.WriteLine("           <input id=""btnSaveInput""  value=""<?php echo ""$btnSaveInput"" ?>"" type=""hidden""></input>")
        objWriter.WriteLine("       </button>")
        objWriter.WriteLine("        <button  type=""submit"" value=""Abandonner"" name=""btnCancel"" class=""buttonPrime"">")
        objWriter.WriteLine("            <span class=""buttonPrimeSpan"">")
        objWriter.WriteLine("  Abandonner()")
        objWriter.WriteLine("           <input id=""txt_CodeHID""  value=""<?php echo ""$txt_CodeHID"" ?>"" type=""hidden""  ></input>")
        objWriter.WriteLine("          </span>")
        objWriter.WriteLine("      </button>")
        objWriter.WriteLine("   </div>")
        objWriter.WriteLine(" </form>")
        objWriter.WriteLine(" </div>")
        objWriter.WriteLine("</center>")


        objWriter.WriteLine("</body>")

        objWriter.WriteLine()
        objWriter.Close()
    End Sub


#End Region


End Class
