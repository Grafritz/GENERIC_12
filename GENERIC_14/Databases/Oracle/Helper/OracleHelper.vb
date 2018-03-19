Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.OleDb
Imports MySql.Data.MySqlClient
Imports System.Data.OracleClient

Imports System.IO
Imports System.Text
Imports System.Xml
Public Class OracleHelper

    Public Shared servername As String
    Public Shared password As String
    Public Shared database As String
    Public Shared user_login As String

#Region "Oracle Fonctions"
    Public Shared Function CreateStore(ByVal Name As String) As String

        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim ds_constraint As DataSet = LoadTableConstraint(Name)

        Dim cap As Integer

        cap = ds.Tables(0).Rows.Count


        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim valueStore As String = ""
        Dim SpecialChar As New List(Of String) From {"VARCHAR2", "nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary", "decimal", "numeric"}
        Dim LevelOneSpecialChar As New List(Of String) From {"VARCHAR2","nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary"}
        Dim LevelTwoSpecialChar As New List(Of String) From {"decimal", "numeric"}

        For Each dt As DataRow In ds_constraint.Tables(0).Rows
            If dt(2).ToString = "P" Then
                Id_table = dt(3).ToString()
            End If

        Next

        For Each dt As DataRow In ds.Tables(0).Rows
            If dt(1).ToString <> Id_table Then
                If count < cap - 4 Then

                    If (paramStore = "") Then
                        paramStore = "_" & dt(1) & " IN " & IIf(SpecialChar.Contains(dt(2).ToString.Trim), IIf(LevelOneSpecialChar.Contains(dt(2).ToString.Trim), dt(2) & "(" & dt(5) & ")", dt(2) & "(" & dt(4).ToString.Trim() & "," & dt(5).ToString.Trim() & ")"), dt(2))
                        champStore = """" & dt(1) & """"
                        valueStore = "@" & dt(1)
                    Else
                        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "_" & dt(1) & " " & IIf(SpecialChar.Contains(dt(2).ToString.Trim), IIf(LevelOneSpecialChar.Contains(dt(2).ToString.Trim), dt(2) & "(" & dt(5) & ")", dt(2) & "(" & dt(5).ToString.Trim() & "," & dt(5).ToString.Trim() & ")"), dt(2))
                        champStore &= Chr(13) & Chr(9) & Chr(9) & "," & """" & dt(1) & """"
                        valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "_" & dt(1)
                    End If
                    count += 1
                Else
                    Exit For
                End If
            End If

        Next
        ' paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@user" & " nvarchar(50)"
        '  champStore &= Chr(13) & Chr(9) & Chr(9) & "," & "createdby"
        champStore &= Chr(13) & Chr(9) & Chr(9) & "," & "datecreated"
        ' valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@user"
        valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "sysdate"
        Dim command As String = Chr(9) & "INSERT INTO " & Name & Chr(13) _
                                & Chr(9) & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & champStore & Chr(13) _
                                & Chr(9) & ")" & Chr(13) _
                                & Chr(9) & "VALUES" & Chr(13) _
                                & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & valueStore & Chr(13) _
                                & Chr(9) & ");"
        '' "USE MCI_db" & Chr(13) & "" & Chr(13) _&
        ''/****** Object:  StoredProcedure [dbo].[SP_AddProfession]    Script Date: 08/12/2012 15:42:54 ******/
        ''SET ANSI_NULLS ON
        ''GO
        ''SET QUOTED_IDENTIFIER ON
        ''GO
        '"USE MCI_db" & Chr(13) & "GO" & Chr(13) _
        '                            & "/****** Object: StoredProcedure [dbo].[SP_Add" & objectname & "]    " & "Script Date: " & Now.Date & " " & Now.Date.TimeOfDay.ToString & " ******/" & Chr(13) _
        '                            & "SET ANSI_NULLS ON" & Chr(13) _
        '                            & "GO" & Chr(13) _
        '                            & "SET QUOTED_IDENTIFIER ON" & Chr(13) _
        '                            & "GO" & Chr(13) & Chr(13) _
        '                            & 

        Dim objectname As String = Name.Substring(4, Name.Length - 4)

        Dim store As String = "CREATE IR REPLACE PROCEDURE SP_" & objectname & " " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & paramStore & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "IS" & Chr(13) _
                            & Chr(9) & "BEGIN  " & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "COMMIT;" & Chr(13) _
                            & Chr(9) & "END;" & Chr(13) & Chr(13) _
                            & Chr(9) & "EXCEPTION" & Chr(13) & Chr(13) _
                            & Chr(9) & "WHEN OTHERS THEN" & Chr(13) & Chr(13) _
                            & Chr(9) & "SELECT @ID" & Chr(13) & Chr(13) _
                            & Chr(9) & "end;" & Chr(13) _
                            & "end SP_" & objectname & ";" & Chr(13) _
                           
        Return store
    End Function

    Public Shared Function UpdateStore_Oracle(ByVal Name As String) As String

        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim cap As Integer

        cap = ds.Tables(1).Rows.Count


        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim QuerySet As String = ""
        Dim SpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary", "decimal", "numeric"}
        Dim LevelOneSpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary"}
        Dim LevelTwoSpecialChar As New List(Of String) From {"decimal", "numeric"}


        For Each dt As DataRow In ds.Tables(2).Rows
            Id_table = dt(0).ToString()
        Next


        For Each dt As DataRow In ds.Tables(1).Rows
            If count < cap - 4 Then
                If QuerySet = "" Then
                    If dt(0) <> Id_table Then
                        QuerySet = "[" & dt(0) & "]" & " " & "= " & "@" & dt(0)
                    End If
                Else
                    QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & "[" & dt(0) & "]" & " " & "= " & "@" & dt(0)
                End If
                If (paramStore = "") Then
                    paramStore = "@" & dt(0) & " " & IIf(SpecialChar.Contains(dt(1).ToString.Trim), IIf(LevelOneSpecialChar.Contains(dt(1).ToString.Trim), dt(1) & "(" & dt(3) & ")", dt(1) & "(" & dt(4).ToString.Trim() & "," & dt(5).ToString.Trim() & ")"), dt(1))

                Else
                    paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@" & dt(0) & " " & IIf(SpecialChar.Contains(dt(1).ToString.Trim), IIf(LevelOneSpecialChar.Contains(dt(1).ToString.Trim), dt(1) & "(" & dt(3) & ")", dt(1) & "(" & dt(4).ToString.Trim() & "," & dt(5).ToString.Trim() & ")"), dt(1))

                End If
                count += 1
            Else
                Exit For
            End If
        Next



        paramStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@user" & " nvarchar(50)"
        QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & "Modifby " & "=" & "@user"
        QuerySet &= Chr(13) & Chr(9) & Chr(9) & "," & "DateModif" & "=" & "GETDATE()" & Chr(13)
        'valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "@user"
        'valueStore &= Chr(13) & Chr(9) & Chr(9) & "," & "GETDATE()"
        Dim command As String = Chr(9) & "UPDATE " & Name & Chr(13) _
                                & Chr(9) & "SET" & Chr(13) & Chr(13) _
                                & Chr(9) & Chr(9) & QuerySet & Chr(13)

        '' "USE MCI_db" & Chr(13) & "GO" & Chr(13) _&
        ''"/***Store Update For table  " & Name & "****/" & Chr(13) _ &
        '' "CREATE PROCEDURE [dbo].[SP_Update" & StrConv(Name, VbStrConv.ProperCase) & "] " & Chr(13) _
        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        Dim store As String =
                            "CREATE PROCEDURE [dbo].[SP_Update" & objectname & "] " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & paramStore & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "AS" & Chr(13) & Chr(13) _
                            & Chr(9) & "DECLARE @Error int" & Chr(13) & Chr(13) _
                            & Chr(9) & "BEGIN TRANSACTION " & Chr(13) & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "Where " & Id_table & " = " & "@" & Id_table & Chr(13) & Chr(13) _
                            & Chr(9) & "SET @Error = @@ERROR" & Chr(13) _
                            & Chr(9) & "IF @Error != 0 GOTO ERROR_HANDLER" & Chr(13) & Chr(13) _
                            & Chr(9) & "COMMIT TRANSACTION" & Chr(13) & Chr(13) _
                            & Chr(9) & "RETURN(0)" & Chr(13) & Chr(13) _
                            & "ERROR_HANDLER:" & Chr(13) _
                            & Chr(9) & "IF @@TRANCOUNT != 0 ROLLBACK TRANSACTION" & Chr(13) _
                            & Chr(9) & "RETURN @Error" & Chr(13)
        Return store
    End Function

    Public Shared Function DeleteStore_Oracle(ByVal Name As String) As String
        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim cap As Integer

        cap = ds.Tables(1).Rows.Count

        Dim count As Integer = 0
        Dim paramStore As String = ""
        Dim champStore As String = ""
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""

        For Each dt As DataRow In ds.Tables(2).Rows
            Id_table = dt(0).ToString()
        Next
        For Each dt As DataRow In ds.Tables(1).Rows
            If dt(0).ToString = Id_table Then
                Id_table_type = dt(1).ToString
            End If
        Next

        Dim command As String = Chr(9) & "DELETE FROM " & Name
        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        Dim store As String =
                            "CREATE PROCEDURE [dbo].[SP_Delete" & objectname & "] " & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & "@ID " & Id_table_type & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "AS" & Chr(13) & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "WHERE " & Id_table & " = " & "@ID" & Chr(13) & Chr(13) _
                            & "" & Chr(13) & Chr(13)

        Return store
    End Function

    Public Shared Function SelectStore_Oracle(ByVal Name As String) As String

        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim cap As Integer

        cap = ds.Tables(1).Rows.Count


        Dim count As Integer = 0
        Dim Id_table As String = ""
        Dim Id_table_type As String = ""
        Dim QuerySet As String = ""

        For Each dt As DataRow In ds.Tables(2).Rows
            Id_table = dt(0).ToString()
        Next
        For Each dt As DataRow In ds.Tables(1).Rows
            If dt(0).ToString = Id_table Then
                Id_table_type = dt(1).ToString
            End If
        Next

        Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM " & Name

        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        Dim store As String =
                            "CREATE PROCEDURE [dbo].[SP_Select" & objectname & "_ByID] " & Chr(13) & Chr(13) _
                            & Chr(9) & "(" & Chr(13) _
                            & Chr(9) & Chr(9) & "@ID " & Id_table_type & Chr(13) _
                            & Chr(9) & ")" & Chr(13) _
                            & "AS" & Chr(13) & Chr(13) _
                            & command & Chr(13) _
                            & Chr(9) & "WHERE " & Id_table & " = " & "@ID" & Chr(13) & Chr(13) _
                            & "" & Chr(13) & Chr(13)
        Return store
    End Function

    Public Shared Function SelectByIndexStore_Oracle(ByVal Name As String) As String
        Dim ds As DataSet = LoadTableStructure_Oracle(Name)

        Dim cap As Integer

        cap = ds.Tables(1).Rows.Count

        Dim count As Integer = 0
        Dim Id_table As String = ""
        Dim ListofIndex As New List(Of String)
        Dim ListofIndexType As New List(Of String)
        Dim index_li_type As New Hashtable
        Dim countIndex As Integer = 0
        Dim QuerySet As String = ""
        Dim storeglobal As String = ""
        Dim indexPosition As Integer = 0


        Dim SpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary", "decimal", "numeric"}
        Dim LevelOneSpecialChar As New List(Of String) From {"nvarchar", "varchar", "char", "nchar", "binary", "datetime2", "datetimeoffset", "time", "varbinary"}
        Dim LevelTwoSpecialChar As New List(Of String) From {"decimal", "numeric"}

        For Each dt As DataRow In ds.Tables(2).Rows
            Id_table = dt(0).ToString()
        Next

        For Each dt As DataRow In ds.Tables(5).Rows
            If dt(2).ToString <> Id_table Then
                ListofIndex.Insert(countIndex, dt(2).ToString)
                countIndex = countIndex + 1
            End If
        Next

        For Each dt As DataRow In ds.Tables(1).Rows
            For Each index In ListofIndex
                If dt(0).ToString = index Then

                    If SpecialChar.Contains(dt(1).ToString) Then
                        If LevelOneSpecialChar.Contains(dt(1).ToString) Then
                            ListofIndexType.Add(dt(1) & "(" & dt(3) & ")")
                            index_li_type.Add(ListofIndex.IndexOf(index), ListofIndexType.IndexOf(dt(1) & "(" & dt(3) & ")"))
                        Else
                            ListofIndexType.Add(dt(1) & "(" & dt(4).ToString.Trim() & "," & dt(5).ToString.Trim() & ")")
                            index_li_type.Add(ListofIndex.IndexOf(index), ListofIndexType.IndexOf(dt(1) & "(" & dt(4).ToString.Trim() & "," & dt(5).ToString.Trim() & ")"))
                        End If

                    Else
                        ListofIndexType.Add(dt(1))
                        index_li_type.Add(ListofIndex.IndexOf(index), ListofIndexType.IndexOf(dt(1)))
                    End If

                End If
            Next

        Next


        For Each index As String In ListofIndex
            Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM " & Name

            Dim objectname As String = Name.Substring(4, Name.Length - 4)
            Dim store As String =
                                "CREATE PROCEDURE [dbo].[SP_Select" & objectname & "_" & ListofIndex.Item(indexPosition) & "] " & Chr(13) _
                                & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & "@" & ListofIndex.Item(indexPosition) & " " & ListofIndexType.Item(index_li_type(indexPosition)) & Chr(13) _
                                & Chr(9) & ")" & Chr(13) & Chr(13) _
                                & "AS" & Chr(13) & Chr(13) _
                                & command & Chr(13) _
                                & Chr(9) & "WHERE " & ListofIndex.Item(indexPosition) & " = " & "@" & ListofIndex.Item(indexPosition) & Chr(13) & Chr(13) _
                            & "" & Chr(13) & Chr(13)

            indexPosition = indexPosition + 1
            storeglobal = storeglobal & store
        Next


        Return storeglobal
    End Function

    Public Shared Function ListAllStore_Oracle(ByVal Name As String) As String
        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim cap As Integer

        cap = ds.Tables(1).Rows.Count


        Dim count As Integer = 0

        Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                & Chr(9) & "FROM " & Name

        Dim objectname As String = Name.Substring(4, Name.Length - 4)
        Dim store As String =
                            "CREATE PROCEDURE [dbo].[SP_ListAll_" & objectname & "] " & Chr(13) & Chr(13) _
                            & "AS" & Chr(13) & Chr(13) _
                            & command & Chr(13)
        Return store
    End Function

    Public Shared Function ListAllByForeignKey_Oracle(ByVal Name As String) As String
        Dim ds As DataSet = LoadTableStructure_Oracle(Name)
        Dim cap As Integer = ds.Tables(1).Rows.Count
        Dim count As Integer = 0
        Dim Id_table As String = ""
        Dim ListofForeignKey As New List(Of String)
        Dim ListofForeignKeyType As New List(Of String)
        Dim countForeignKey As Integer = 0
        Dim QuerySet As String = ""
        Dim storeglobal As String = ""
        Dim foreignkeyPosition As Integer = 0
        Dim key_li_type As New Hashtable


        For Each dt As DataRow In ds.Tables(2).Rows
            Id_table = dt(0).ToString()
        Next

        For Each dt As DataRow In ds.Tables(6).Rows
            If dt(0).ToString = "FOREIGN KEY" Then
                ListofForeignKey.Add(dt(6).ToString)
                countForeignKey = countForeignKey + 1
            End If
        Next

        For Each dt As DataRow In ds.Tables(1).Rows
            For Each key In ListofForeignKey
                If dt(0).ToString = key Then
                    If dt(1).ToString = "nvarchar" Then
                        ListofForeignKeyType.Add(dt(1) & "(" & dt(3) & ")")
                        key_li_type.Add(ListofForeignKey.IndexOf(key), ListofForeignKeyType.IndexOf(dt(1) & "(" & dt(3) & ")"))
                    Else
                        ListofForeignKeyType.Add(dt(1))
                        key_li_type.Add(ListofForeignKey.IndexOf(key), ListofForeignKeyType.IndexOf(dt(1)))
                    End If

                End If
            Next
        Next


        For Each key As String In ListofForeignKey
            Dim command As String = Chr(9) & "SELECT *" & Chr(13) _
                                    & Chr(9) & "FROM " & Name

            Dim objectname As String = Name.Substring(4, Name.Length - 4)
            Dim patternname As String = ListofForeignKey.Item(foreignkeyPosition).Substring(3, ListofForeignKey.Item(foreignkeyPosition).Length - 3)

            Dim store As String =
                                "CREATE PROCEDURE [dbo].[SP_ListAll_" & objectname & "_" & patternname & "] " & Chr(13) _
                                & Chr(9) & "(" & Chr(13) _
                                & Chr(9) & Chr(9) & "@" & ListofForeignKey.Item(foreignkeyPosition) & " " & ListofForeignKeyType(key_li_type(foreignkeyPosition)) & Chr(13) _
                                & Chr(9) & ")" & Chr(13) & Chr(13) _
                                & "AS" & Chr(13) & Chr(13) _
                                & command & Chr(13) _
                                & Chr(9) & "WHERE " & ListofForeignKey.Item(foreignkeyPosition) & " = " & "@" & ListofForeignKey.Item(foreignkeyPosition) & Chr(13) & Chr(13) _
                                & "" & Chr(13) & Chr(13)
            foreignkeyPosition = foreignkeyPosition + 1
            storeglobal = storeglobal & store
        Next


        Return storeglobal
    End Function

    Public Shared Function LoadTableStructure_Oracle(ByVal table As String) As DataSet
        Dim ConString As String = _
      "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)" _
      & "(PORT=" & database & "))" & _
      "(CONNECT_DATA=(SERVICE_NAME=" & servername & ")));" & _
      "User Id=" & user_login & ";Password=" & password & ";"
        Try
            Dim Con As New OracleConnection(ConString)
            Con.Open()
            Dim cmd As New OracleCommand

            cmd.CommandText = "Select * from user_tab_columns where table_name = '" & table & "'"
            cmd.CommandType = CommandType.Text
            cmd.Connection = Con
            Dim p As New OracleParameter
            p.Value = table
            Dim ds As New DataSet
            Dim da As OracleDataAdapter
            da = New OracleDataAdapter(cmd)
            da.Fill(ds)
            cmd.Parameters.Clear()
            Con.Close()
            Return ds
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Load Table Structure MySql", MessageBoxButtons.OK)
            '  Error_Log("LoadTableStructure_Oracle", ex.Message)
        End Try
    End Function

    Public Shared Function LoadTableConstraint(ByVal table As String, Optional ByVal Constraint As String = "") As DataSet
        Dim ConString As String = _
      "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)" _
      & "(PORT=" & database & "))" & _
      "(CONNECT_DATA=(SERVICE_NAME=" & servername & ")));" & _
      "User Id=" & user_login & ";Password=" & password & ";"
        Try
            Dim Con As New OracleConnection(ConString)
            Con.Open()
            Dim cmd As New OracleCommand

            cmd.CommandText = "select distinct cols.owner ,cols.table_name, cons.constraint_type ,cols.column_name from  all_cons_columns cols, all_constraints  cons where cons.constraint_name = cols.constraint_name  and cons.table_name = '" & table.ToUpper & "'"


            'cmd.CommandText = "SELECT cols.owner,cols.table_name, cols.column_name FROM  all_constraints cons, all_cons_columns cols" & _
            '"WHERE cons.constraint_name = cols.constraint_name;" & _
            '"AND cons.owner = cols.owner AND" & _
            '"cols.table_name = '" & table & "' AND" & _
            '"cons.owner = '" & user_login.ToUpper & "'" & _
            '"ORDER BY cols.table_name, cols.position"
            cmd.CommandType = CommandType.Text
            cmd.Connection = Con
            Dim p As New OracleParameter
            p.Value = table
            Dim ds As New DataSet
            Dim da As OracleDataAdapter
            da = New OracleDataAdapter(cmd)
            da.Fill(ds)
            cmd.Parameters.Clear()
            Con.Close()
            Return ds
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "LoadTableConstraint", MessageBoxButtons.OK)
            '  Error_Log("LoadTableStructure_Oracle", ex.Message)
        End Try
    End Function

    Public Shared Function LoadUserTablesSchema_Oracle( _
             ByVal strServer As String, _
             ByVal strUser As String, _
             ByVal strPwd As String, _
             ByVal strDatabase As String, ByRef treeview As TreeView) As ArrayList

        Dim slTables As ArrayList = New ArrayList()

        
        ' Dim cnString2 As String = "Data Source=" & strServer & ";User Id=" & strUser & ";Password=" & strPwd & ";Integrated Security=no;"

        Dim cnString As String = _
      "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)" _
      & "(PORT=" & strDatabase & "))" & _
      "(CONNECT_DATA=(SERVICE_NAME=" & strServer & ")));" & _
      "User Id=" & strUser & ";Password=" & strPwd & ";"

        Dim strQUERY As String = "Select * FROM user_tables"
        Dim table As DataTable = Nothing
        Dim con As OracleConnection = New OracleConnection(cnString)
        Dim cmd As New OracleCommand

        Dim ds As New DataSet
        Dim ds2 As New DataSet
        Dim da As OracleDataAdapter
        Dim dr2 As DataRow
        Try
            con.Open()
            cmd.Connection = con
            cmd.CommandText = strQUERY

            da = New OracleDataAdapter(strQUERY, con)
            da.Fill(ds2)
            table = ds2.Tables(0)
            For Each dr2 In table.Rows
                treeview.Nodes.Add(dr2("TABLE_NAME"))
            Next

            con.Close()
        Catch x As OleDbException
            'lblMsg.Text = x.Message
            slTables = Nothing
        End Try

        Return slTables

    End Function
#End Region

End Class
