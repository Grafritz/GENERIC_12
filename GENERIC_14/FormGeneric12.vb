Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.OleDb
Imports Npgsql
Imports MySql.Data.MySqlClient
Imports Microsoft.SqlServer.Management.Smo
Imports Microsoft.SqlServer.Management.Common
Imports Microsoft.SqlServer.Management.Smo.Server
Imports System.Data.OracleClient
Imports System.IO
Imports System.Text
Imports System.Xml
Imports System.Data.SqlServerCe
Imports System.ComponentModel
Imports System.Data.Sql
Imports System.Data.Common

Public Class FormGeneric12

    Private ispossible As Boolean = False
#Region "Attributes"
    Dim list As New List(Of DataSet)
    Private Delegate Sub UpdateFormDelegate()
    Private UpdateFormDelegate1 As UpdateFormDelegate
    Private updateComboServer As UpdateFormDelegate
    Private _systeme As Cls_Systeme = Cls_Systeme.getInstance
    Private WorkIsDone As Long
    Private IntelligentMode As Boolean = False
    Private test As BackgroundWorker
    Private TypeDatabaseChecked As Long
#End Region

#Region "Form Loading"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        '   InitializeServerCombobox()
        BackgroundWorker1.WorkerReportsProgress = True
        Btn_GenererScript.Enabled = False
        TabControl_Form.DeselectTab(TabBaseDeDonnees)
        TabControl_Form.SelectTab(TabParametre)
        'FillComboPrefixStoredProcedure()
        '_systeme.CleanData()
        'SetupReportGeneration()
        'SetupCurrentPrefix()
        ispossible = True

        rcmb_DatabaseName.Items.Add("CREATE ")
        rcmb_DatabaseName.Items.Add("UPDATE ")
        CB_ActionStoreProcedure.SelectedIndex = 0

        LoadInstanceSQLServer()

    End Sub

    Public Sub LoadInstanceSQLServer()
        'declare variables
        Dim dt As Data.DataTable = New DataTable()
        Dim dr As Data.DataRow = Nothing

        Try
            ''get sql server instances in to DataTable object
            Dim servers As Sql.SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance

            ' Check if datatable is empty
            If dt.Rows.Count = 0 Then
                ' Get a datatable with info about SQL Server 2000 and 2005 instances
                dt = servers.GetDataSources()

                ' List that will be combobox’s datasource   
                Dim listServers As List(Of String) = New List(Of String)
                ' For each element in the datatable add a new element in the list

                For Each rowServer As DataRow In dt.Rows
                    ' SQL Server instance could have instace name or only server name,
                    ' check this for show the name
                    If String.IsNullOrEmpty(rowServer("InstanceName").ToString()) Then
                        If rowServer("ServerName").ToString().Equals("JFDUVERS-PC") Then
                            listServers.Add(rowServer("ServerName").ToString() & "\MSSQLSERVER_08R2")
                        Else
                            listServers.Add(rowServer("ServerName").ToString())
                        End If
                    Else
                        listServers.Add(rowServer("ServerName") & "\" & rowServer("InstanceName"))
                    End If
                Next
                'Set servers list to combobox’s datasource
                Me.cmb_server.DataSource = listServers
            End If
        Catch ex As System.Data.SqlClient.SqlException
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error!")

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error!")

        Finally
            'clean up ;)
            dr = Nothing
            dt = Nothing
        End Try
    End Sub

    Private Sub InitializeServerCombobox()
        If _systeme.getLastConnectionString Is Nothing Then
            cmb_server.Items.Clear()
            Dim dtable As DataTable = SmoApplication.EnumAvailableSqlServers(False)
            Dim ServerName As String
            For Each dr As DataRow In dtable.Rows
                ServerName = dr("Server").ToString()
                If Not Convert.IsDBNull(dr("Instance")) And dr("Instance").ToString().Length > 0 Then
                    ServerName += "\" & dr("Instance").ToString()
                End If
                If cmb_server.Items.IndexOf(ServerName) < 0 Then
                    cmb_server.Items.Add(ServerName)
                End If
                If cmb_server.Items.Count > 0 Then
                    cmb_server.SelectedIndex = 0
                End If
            Next
        End If
        'Dim instance As SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance
        'Dim dtable2 As DataTable = instance.GetDataSources
        'For Each dr As DataRow In dtable2.Rows
        '    For Each col As DataColumn In dtable2.Columns
        '        MessageBox.Show(col.ColumnName & " " & dr(col))
        '    Next
        'Next
        Dim tbl As DataTable = DbProviderFactories.GetFactoryClasses()
        Dim strInvariantProviderName As String = "System.Data.OracleClient"
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(strInvariantProviderName)

        Dim dsenum As DbDataSourceEnumerator = factory.CreateDataSourceEnumerator

        Dim servers As DataTable = dsenum.GetDataSources()
        For Each serverlist As DataRow In servers.Rows
            MessageBox.Show(serverlist(0) & " " & serverlist(1) & " " & serverlist(2))
        Next
    End Sub

#End Region

#Region "Event on Generate Script"
    Private Sub Btn_GenererScriptClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_GenererScript.Click
        Dim s As String = ""
        Dim supdate As String = ""
        Dim sdelete As String = ""
        Dim slistall As String = ""
        Dim sselect As String = ""
        Dim sselectindex As String = ""
        Dim sselectanycolumn As String = ""
        Dim slistallforeign As String = ""
        Dim slistallPagination As String = ""

        Dim ssupdateparentaddchild As String = ""
        Dim ssupdateparentremovechild As String = ""
        Dim slistallchildinparent As String = ""
        Dim slistallchildnotintparent As String = ""

        'SetupCurrentPrefix()

        If rbtn_Oracle.Checked Then
            GenerateScriptForOracle(s, supdate, sdelete, slistall, slistallforeign, sselectindex, sselect)

        ElseIf rbtn_SqlServer.Checked Then
            'If txt_FkPrefix.Text = "" Then
            'MessageBox.Show("Le prefixe des cles etrangeres n'est pas renseigne")
            'Else
            'SqlServerHelper.ForeinKeyPrefix = txt_FkPrefix.Text.Trim
            GenerateScriptForSqlServer(s, supdate, sdelete, slistall, slistallforeign, sselectindex, sselect, ssupdateparentaddchild, ssupdateparentremovechild, slistallchildinparent, slistallchildnotintparent, sselectanycolumn)
            'End If

        ElseIf rbtn_MySql.Checked Then
            GenerateScriptForMySql(s, supdate, sdelete, slistall, slistallforeign, sselectindex, sselect, slistallPagination)
        ElseIf rbtn_PostGres.Checked Then
            If txt_FkPrefix.Text = "" Then
                MessageBox.Show("Le prefixe des cles etrangeres n'est pas renseigne")
            Else
                PostgresSqlManager.ForeinKeyPrefix = txt_FkPrefix.Text.Trim
                GenerateScriptForPostGres(s, supdate, sdelete, slistall, slistallforeign, sselectindex, sselect)
            End If

        Else
            MessageBox.Show("Aucune base de données sélectionnée")
        End If

    End Sub

    Private Sub Btn_GenererParIndex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_GenererParIndex.Click

        If rbtn_Oracle.Checked Then
        ElseIf rbtn_SqlServer.Checked Then
            GenerateInDexScriptForSqlServer()
        ElseIf rbtn_MySql.Checked Then
            GenerateInDexScriptForMySQL()
        ElseIf rbtn_PostGres.Checked Then
            GenerateInDexScriptForPostGres()
        End If


    End Sub
#End Region

#Region "Generate Script Sub"

    Private Sub GenerateInDexScriptForSqlServer()
        Dim i As Integer = 0

        For Each tr As TreeNode In TreeView2.Nodes
            If tr.IsSelected Then
                i = i + 1
                Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
                Dim pathlistallfield As String = txt_PathGenerate_Script & "ListAll" & tr.Text.Replace("tbl_", "") & "ByField.sql"
                Dim s As String = ""
                Dim strField As String = ""
                For Each node As TreeNode In tr.Nodes
                    If node.Checked Then
                        If strField = "" Then
                            strField = node.Text
                        Else
                            strField = strField & "," & node.Text
                        End If

                    End If
                Next
                s &= SqlServer.IMode.ScriptGenerator.ListAllStoreByField(tr.Text, strField)
                s &= Chr(13)

                SqlServer.IMode.VbClassGenerator.CreateFileWithSearchAllByField(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, strField)

                If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
                    If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                        Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
                    End If
                Else
                    If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                        Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
                    End If
                End If

                Dim fs As FileStream = File.Create(pathlistallfield, 1024)
                fs.Close()

                Dim objWriter As New System.IO.StreamWriter(pathlistallfield, True)
                objWriter.WriteLine(s)
                objWriter.Close()
            End If




        Next
        If i = 0 Then
            MessageBox.Show("Aucun élément sélectionné")
        Else
            MessageBox.Show("Effectué")
        End If


    End Sub

    Private Sub GenerateInDexScriptForPostGres()
        Dim i As Integer = 0
        For Each tr As TreeNode In TreeView2.Nodes
            If tr.IsSelected Then
                i = i + 1
                Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
                Dim pathlistallfield As String = txt_PathGenerate_Script & "ListAll" & tr.Text.Replace("tbl_", "") & "ByField.sql"
                Dim s As String = ""
                Dim strField As String = ""
                For Each node As TreeNode In tr.Nodes
                    If node.Checked Then
                        If strField = "" Then
                            strField = node.Text
                        Else
                            strField = strField & "," & node.Text
                        End If

                    End If
                Next
                s &= PostgresSqlManager.ListAllStoreByField(tr.Text, strField)
                s &= Chr(13)


                If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
                    If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\") Then
                        Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
                    End If
                Else
                    If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\") Then
                        Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
                    End If
                End If



                Dim fs As FileStream = File.Create(pathlistallfield, 1024)
                fs.Close()

                Dim objWriter As New System.IO.StreamWriter(pathlistallfield, True)
                objWriter.WriteLine(s)
                objWriter.Close()
            End If



        Next

        If i = 0 Then
            MessageBox.Show("Aucun élément sélectionné")
        Else
            MessageBox.Show("Effectué")
        End If

    End Sub

    Private Sub GenerateInDexScriptForMySQL()
        Dim i As Integer = 0
        For Each tr As TreeNode In TreeView2.Nodes

            If tr.IsSelected Then
                i = i + 1
                Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
                Dim pathlistallfield As String = txt_PathGenerate_Script & "ListAll" & tr.Text.Replace("tbl_", "") & "ByField.sql"
                Dim s As String = ""
                Dim strField As String = ""
                For Each node As TreeNode In tr.Nodes
                    If node.Checked Then
                        If strField = "" Then
                            strField = node.Text
                        Else
                            strField = strField & "," & node.Text
                        End If

                    End If
                Next
                s &= MySqlManager.ListAllStoreByField(tr.Text, strField)
                s &= Chr(13)

                If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
                    If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\") Then
                        Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\")
                    End If
                Else
                    If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\") Then
                        Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\")
                    End If
                End If

                Dim fs As FileStream = File.Create(pathlistallfield, 1024)
                fs.Close()

                Dim objWriter As New System.IO.StreamWriter(pathlistallfield, True)
                objWriter.WriteLine(s)
                objWriter.Close()

            End If


        Next

        If i = 0 Then
            MessageBox.Show("Aucun élément sélectionné")
        Else
            MessageBox.Show("Effectué")
        End If

    End Sub

    Private Sub GenerateScriptForSqlServer(ByVal s As String, ByVal supdate As String, ByVal sdelete As String, ByVal slistall As String _
                                           , ByVal slistallforeign As String, ByVal sselectindex As String, ByVal sselect As String _
                                           , ByVal ssupdateparentaddchild As String, ByVal ssupdateparentremovechild As String _
                                           , ByVal slistallchildinparent As String, ByVal slistallchildnotintparent As String _
                                           , ByVal sListAllByAnyField As String)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
        Dim path As String = txt_PathGenerate_Script & "InsertScript.sql"
        Dim pathupdate As String = txt_PathGenerate_Script & "UpdateScript.sql"
        Dim pathdelete As String = txt_PathGenerate_Script & "DeleteScript.sql"
        Dim pathlistall As String = txt_PathGenerate_Script & "ListAllScript.sql"
        Dim pathselect As String = txt_PathGenerate_Script & "SelectScript.sql"
        Dim pathselectindex As String = txt_PathGenerate_Script & "SelectIndexScript.sql"
        Dim pathselectanycolumn As String = txt_PathGenerate_Script & "ListAllAnyColumnScript.sql"
        Dim pathlistallforeign As String = txt_PathGenerate_Script & "ListAllForeignScript.sql"
        Dim pathupdateparentaddchild As String = txt_PathGenerate_Script & "UpdateParentAddChildScript.sql"
        Dim pathupdateparentremovechild As String = txt_PathGenerate_Script & "UpdateParentRemoveChildScript.sql"
        Dim pathlistallparentinchild As String = txt_PathGenerate_Script & "ListAllParentInChildScript.sql"
        Dim pathlistallparentnotinchild As String = txt_PathGenerate_Script & "ListAllParentNotInChildScript.sql"
        Dim ds As DataSet
        For Each tr As TreeNode In TreeView1.Nodes
            If tr.Checked Then
                'If IntelligentMode And rbtnIMode_Yes.Checked Then
                '    Dim _table As Cls_Table
                '    Dim node As New TreeNode
                '    node.Text = tr.Text

                '    s &= SqlServer.IMode.ScriptGenerator.CreateStore(tr.Text)
                '    s &= Chr(13)
                '    supdate &= SqlServer.IMode.ScriptGenerator.UpdateStore(tr.Text)
                '    supdate &= Chr(13)
                '    sdelete &= SqlServer.IMode.ScriptGenerator.DeleteStore(tr.Text)
                '    sdelete &= Chr(13)
                '    slistall &= SqlServer.IMode.ScriptGenerator.ListAllStore(tr.Text)
                '    slistall &= Chr(13)
                '    sselect &= SqlServer.IMode.ScriptGenerator.SelectStore(tr.Text)
                '    sselect &= Chr(13)
                '    sselectindex &= SqlServer.IMode.ScriptGenerator.SelectByIndexStore(tr.Text)
                '    sselectindex &= Chr(13)
                '    sListAllByAnyField &= SqlServer.IMode.ScriptGenerator.ListAllByAnyField(tr.Text)
                '    sListAllByAnyField &= Chr(13)
                '    slistallforeign &= SqlServer.IMode.ScriptGenerator.ListAllByForeignKey(tr.Text)
                '    slistallforeign &= Chr(13)



                '    SqlServer.IMode.VbClassGenerator.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '    SqlServer.IMode.AspFormGenerator.CreateInterfaceCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName)
                '    SqlServer.IMode.AspFormGenerator.CreateInterfaceCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '    SqlServer.IMode.AspFormGenerator.CreateListingCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName)
                '    SqlServer.IMode.AspFormGenerator.CreateListingCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '    '   SqlServerHelper.CreateTableInXMLFormat(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '    SqlServer.IMode.AndroidClassGenerator.CreateAndroidModel(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServer.IMode.AndroidClassGenerator.CreateAndroidHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServer.IMode.AndroidClassGenerator.CreateAndroiDBHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServer.IMode.AndroidFormGenerator.CreateAndroidBinderListview(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServer.IMode.AndroidFormGenerator.CreateAndroidFormActivity(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServer.IMode.AndroidFormGenerator.CreateAndroidFormLayout(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)


                '    ds = SqlServerHelper.LoadTableStructure(tr.Text)
                '    '_table = New Cls_Table(
                '    '    For Each _column In  
                '    For Each dt As DataRow In ds.Tables(1).Rows
                '        node.Nodes.Add(dt(0))
                '    Next
                '    TreeView2.Nodes.Add(node)
                'Else
                Dim _table As Cls_Table
                Dim node As New TreeNode
                node.Text = tr.Text
                '  SqlServerHelper.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                ' SqlServerHelper.CreateInterfaceCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '  s &= SqlServerHelper.CreateStore(tr.Text)

                SqlServerHelper.CurrentPrefixStored = CB_ActionStoreProcedure.Text

                s &= SqlServer.Fast.ScriptGenerator.CreateStore(tr.Text)
                s &= Chr(13)
                supdate &= SqlServer.Fast.ScriptGenerator.UpdateStore(tr.Text)
                supdate &= Chr(13)
                sdelete &= SqlServer.Fast.ScriptGenerator.DeleteStore(tr.Text)
                sdelete &= Chr(13)
                slistall &= SqlServer.Fast.ScriptGenerator.ListAllStore(tr.Text)
                slistall &= Chr(13)
                sselect &= SqlServer.Fast.ScriptGenerator.SelectStore(tr.Text)
                sselect &= Chr(13)
                sselectindex &= SqlServer.Fast.ScriptGenerator.SelectByIndexStore(tr.Text)
                sselectindex &= Chr(13)
                slistallforeign &= SqlServer.Fast.ScriptGenerator.ListAllByForeignKey(tr.Text)
                slistallforeign &= Chr(13)

                SqlServer.Fast.VbClassGenerator.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)

                REM AdminLTE-Master
                If RB_Template_AdminLTE_Master.Checked Then
                    'Interface ADD EDIT
                    SqlServer.Fast.AspFormGenerator.CreateInterfaceCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                    SqlServer.Fast.AspFormGenerator.CreateInterfaceCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                    'Interface LISTING
                    SqlServer.Fast.AspFormGenerator.CreateListingCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                    SqlServer.Fast.AspFormGenerator.CreateListingCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                ElseIf RB_Template_CleanZone.Checked Then ' CLeanZone
                    'Interface ADD EDIT
                    If RB_Formulaire_Tableau.Checked Then
                        SqlServer.Fast.AspFormGenerator.CreateInterface_Tableau_Formulaire_Design(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                        SqlServer.Fast.AspFormGenerator.CreateInterface_Tableau_Formulaire_CodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                    ElseIf RB_Formulaire_FlowLayout.Checked Then
                        SqlServer.Fast.AspFormGenerator.CreateInterface_Tableau_Formulaire_Design(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                        SqlServer.Fast.AspFormGenerator.CreateInterfaceCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                    End If

                    'Interface LISTING
                    SqlServer.Fast.AspFormGenerator.CreateInterface_CleanZone_Listing_Design(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                    SqlServer.Fast.AspFormGenerator.CreateInterface_CleanZone_Listing_CodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                ElseIf RB_Template_Inspinia.Checked Then

                End If

                ' SqlServerHelper.CreateAndroidModel(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '  SqlServerHelper.CreateAndroidHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                '    SqlServerHelper.CreateAndroiDBHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                ds = SqlServerHelper.LoadTableStructure(tr.Text)

                TreeView2.Nodes.Clear()
                For Each dt As DataRow In ds.Tables(1).Rows
                    node.Nodes.Add(dt(0))
                Next
                TreeView2.Nodes.Add(node)
            End If

            'End If
        Next




        REM on verifie si le repertoir existe bien
        If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
            If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
            End If
        Else
            If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
            End If
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim fs_update As FileStream = File.Create(pathupdate, 1024)
        fs_update.Close()

        Dim fs_delete As FileStream = File.Create(pathdelete, 1024)
        fs_delete.Close()

        Dim fs_listAll As FileStream = File.Create(pathlistall, 1024)
        fs_listAll.Close()

        Dim fs_select As FileStream = File.Create(pathselect, 1024)
        fs_select.Close()

        Dim fs_selectindex As FileStream = File.Create(pathselectindex, 1024)
        fs_selectindex.Close()

        Dim fs_listAllforeign As FileStream = File.Create(pathlistallforeign, 1024)
        fs_listAllforeign.Close()

        Dim fs_listAllanycolumn As FileStream = File.Create(pathselectanycolumn, 1024)
        fs_listAllanycolumn.Close()


        Dim fs_updateparentaddchild As FileStream = File.Create(pathupdateparentaddchild, 1024)
        fs_updateparentaddchild.Close()


        Dim fs_updateparentremovechild As FileStream = File.Create(pathupdateparentremovechild, 1024)
        fs_updateparentremovechild.Close()

        Dim fs_listallparentinchild As FileStream = File.Create(pathlistallparentinchild, 1024)
        fs_listallparentinchild.Close()

        Dim fs_listallparentnotinchild As FileStream = File.Create(pathlistallparentnotinchild, 1024)
        fs_listallparentnotinchild.Close()

        If IntelligentMode And rbtnIMode_Yes.Checked Then


            ssupdateparentaddchild &= SqlServer.IMode.ScriptGenerator.UpdateStoreAddChild()
            ssupdateparentaddchild &= Chr(13)
            ssupdateparentremovechild &= SqlServer.IMode.ScriptGenerator.UpdateStoreRemoveChild()
            ssupdateparentremovechild &= Chr(13)
            slistallchildinparent &= SqlServer.IMode.ScriptGenerator.ListAllChildinParent()
            slistallchildinparent &= Chr(13)
            slistallchildnotintparent &= SqlServer.IMode.ScriptGenerator.ListAllChildNotinParent()
            slistallchildnotintparent &= Chr(13)

            Dim objWriterUpdateparentaddchild As New System.IO.StreamWriter(pathupdateparentaddchild, True, System.Text.Encoding.UTF8)
            Dim objWriterUpdateParentRemovechild As New System.IO.StreamWriter(pathupdateparentremovechild, True, System.Text.Encoding.UTF8)
            Dim objWriterlistAllparentinchild As New System.IO.StreamWriter(pathlistallparentinchild, True, System.Text.Encoding.UTF8)
            Dim objWriterlistAllparentnotinChild As New System.IO.StreamWriter(pathlistallparentnotinchild, True, System.Text.Encoding.UTF8)

            With objWriterUpdateparentaddchild

                .WriteLine(ssupdateparentaddchild)
                .Close()
            End With

            With objWriterUpdateParentRemovechild

                .WriteLine(ssupdateparentremovechild)
                .Close()
            End With

            With objWriterlistAllparentinchild

                .WriteLine(slistallchildinparent)
                .Close()
            End With

            With objWriterlistAllparentnotinChild

                .WriteLine(slistallchildnotintparent)
                .Close()
            End With

            SqlServer.IMode.VbClassGenerator.CreateFileForParent(txt_PathGenerate_ScriptFile, ListBox_NameSpace)
            SqlServer.IMode.VbClassGenerator.CreateFileForChild(txt_PathGenerate_ScriptFile, ListBox_NameSpace)
            For Each group In Cls_GroupTable.SearchAll
                SqlServer.IMode.AspFormGenerator.CreateParentChildInterfaceAsp("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
                SqlServer.IMode.AspFormGenerator.CreateParentChildInterfaceCodeBehind("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
                SqlServer.IMode.AspFormGenerator.CreateSendRemoveChildInParentAsp("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group)
                SqlServer.IMode.AspFormGenerator.CreateSendRemoveChildInParentBehind("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
            Next

        End If

        Dim objWriter As New System.IO.StreamWriter(path, True, System.Text.Encoding.UTF8)

        Dim objWriterupdate As New System.IO.StreamWriter(pathupdate, True, System.Text.Encoding.UTF8)
        Dim objWriterdelete As New System.IO.StreamWriter(pathdelete, True, System.Text.Encoding.UTF8)
        Dim objWriterlistAll As New System.IO.StreamWriter(pathlistall, True, System.Text.Encoding.UTF8)
        Dim objWriterSelect As New System.IO.StreamWriter(pathselect, True, System.Text.Encoding.UTF8)
        Dim objWriterSelectIndex As New System.IO.StreamWriter(pathselectindex, True, System.Text.Encoding.UTF8)
        Dim objWriterListallForeign As New System.IO.StreamWriter(pathlistallforeign, True, System.Text.Encoding.UTF8)
        Dim objWriterListallAnycolumn As New System.IO.StreamWriter(pathselectanycolumn, True, System.Text.Encoding.UTF8)


        objWriter.WriteLine(s)
        objWriter.Close()

        objWriterupdate.WriteLine()
        objWriterupdate.WriteLine(supdate)
        objWriterupdate.Close()

        objWriterdelete.WriteLine()
        objWriterdelete.WriteLine(sdelete)
        objWriterdelete.Close()

        objWriterlistAll.WriteLine()
        objWriterlistAll.WriteLine(slistall)
        objWriterlistAll.Close()

        objWriterSelect.WriteLine()
        objWriterSelect.WriteLine(sselect)
        objWriterSelect.Close()

        objWriterSelectIndex.WriteLine()
        objWriterSelectIndex.WriteLine(sselectindex)
        objWriterSelectIndex.Close()

        With objWriterListallForeign
            .WriteLine(slistallforeign)
            .Close()
        End With

        With objWriterListallAnycolumn
            .WriteLine(sListAllByAnyField)
            .Close()
        End With
    End Sub

    Private Sub GenerateScriptForMySql(ByVal s As String, ByVal supdate As String, ByVal sdelete As String, ByVal slistall As String, ByVal slistallforeign As String, ByVal sselectindex As String, ByVal sselect As String, ByVal slistallpagination As String)
        '  Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\")
        Dim path As String = txt_PathGenerate_Script & "InsertScript.sql"
        Dim pathupdate As String = txt_PathGenerate_Script & "UpdateScript.sql"
        Dim pathdelete As String = txt_PathGenerate_Script & "DeleteScript.sql"
        Dim pathlistall As String = txt_PathGenerate_Script & "ListAllScript.sql"
        Dim pathselect As String = txt_PathGenerate_Script & "SelectScript.sql"
        Dim pathselectindex As String = txt_PathGenerate_Script & "SelectIndexScript.sql"
        Dim pathlistallforeign As String = txt_PathGenerate_Script & "ListAllForeignScript.sql"
        Dim pathlistallpagination As String = txt_PathGenerate_Script & "ListAllPaginationScript.sql"

        Dim ds As DataSet

        For Each tr As TreeNode In TreeView1.Nodes
            If tr.Checked Then
                Dim node As New TreeNode
                node.Text = tr.Text
                MySqlManager.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                MySqlManager.CreatePHPClass(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                MySqlManager.CreateInterfaceCodePHP(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                MySqlManager.CreateInterfaceCodeHTmlPHP(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                MySqlManager.CreateGridPhpForListing(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, txt_DatabaseName.Text)

                '   MySqlManager.CreateJavaClassDomaine(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                '  MySqlManager.CreateJavaClassDAL(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                ' MySqlManager.CreateJavaClassSession(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_DatabaseName.Text)
                s &= MySqlHelper.CreateStoreMySql(tr.Text)
                s &= Chr(13)

                supdate &= MySqlHelper.UpdateStoreMySql(tr.Text)
                supdate &= Chr(13)

                sdelete &= MySqlHelper.DeleteStoreMySql(tr.Text)
                sdelete &= Chr(13)

                slistall &= MySqlHelper.ListAllStoreMySql(tr.Text)
                slistall &= Chr(13)

                sselect &= MySqlHelper.SelectStoreMySql(tr.Text)
                sselect &= Chr(13)

                slistallpagination &= MySqlManager.ListAllStorePaginationMySql(tr.Text)
                slistallpagination &= Chr(13)

                'sselectindex &= SelectByIndexStoreMySql(tr.Text)
                'sselectindex &= Chr(13)

                'slistallforeign &= ListAllByForeignKeyMySql(tr.Text)
                'slistallforeign &= Chr(13)

                '                Dim cm As New SqlConnection(ConnectionString)
                '                cm.Open()
                '                Dim cmd As New SqlCommand
                '                cmd.Connection = cm
                '                cmd.CommandType = CommandType.Text
                '                cmd.CommandText = s

                'cmd.Prepare()
                ' cmd.ExecuteNonQuery()

                ds = MySqlHelper.LoadTableStructure_MySql(tr.Text)



                For Each dt As DataRow In ds.Tables(0).Rows

                    node.Nodes.Add(dt(0))

                Next
                TreeView2.Nodes.Add(node)
            End If
        Next

        REM on verifie si le repertoir existe bien
        If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
            If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\") Then
                Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\")
            End If
        Else
            If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\") Then
                Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\MySQLScript\")
            End If
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim fs_update As FileStream = File.Create(pathupdate, 1024)
        fs_update.Close()

        Dim fs_delete As FileStream = File.Create(pathdelete, 1024)
        fs_delete.Close()

        Dim fs_listAll As FileStream = File.Create(pathlistall, 1024)
        fs_listAll.Close()

        Dim fs_select As FileStream = File.Create(pathselect, 1024)
        fs_select.Close()

        Dim fs_selectindex As FileStream = File.Create(pathselectindex, 1024)
        fs_selectindex.Close()

        Dim fs_listAllforeign As FileStream = File.Create(pathlistallforeign, 1024)
        fs_listAllforeign.Close()

        Dim fs_listAllPagination As FileStream = File.Create(pathlistallpagination, 1024)
        fs_listAllPagination.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim objWriterupdate As New System.IO.StreamWriter(pathupdate, True)
        Dim objWriterdelete As New System.IO.StreamWriter(pathdelete, True)
        Dim objWriterlistAll As New System.IO.StreamWriter(pathlistall, True)
        Dim objWriterSelect As New System.IO.StreamWriter(pathselect, True)
        Dim objWriterSelectIndex As New System.IO.StreamWriter(pathselectindex, True)
        Dim objWriterListallForeign As New System.IO.StreamWriter(pathlistallforeign, True)
        Dim objWriterListallPagination As New System.IO.StreamWriter(pathlistallpagination, True)

        objWriter.WriteLine(s)
        objWriter.Close()


        objWriterupdate.WriteLine()
        objWriterupdate.WriteLine(supdate)
        objWriterupdate.Close()

        objWriterdelete.WriteLine()
        objWriterdelete.WriteLine(sdelete)
        objWriterdelete.Close()

        objWriterlistAll.WriteLine()
        objWriterlistAll.WriteLine(slistall)
        objWriterlistAll.Close()

        objWriterSelect.WriteLine()
        objWriterSelect.WriteLine(sselect)
        objWriterSelect.Close()

        objWriterSelectIndex.WriteLine()
        objWriterSelectIndex.WriteLine(sselectindex)
        objWriterSelectIndex.Close()

        With objWriterListallForeign
            .WriteLine(slistallforeign)
            .Close()
        End With

        With objWriterListallPagination
            .WriteLine(slistallpagination)
            .Close()
        End With

    End Sub

    Private Sub GenerateScriptForOracle(ByVal s As String, ByVal supdate As String, ByVal sdelete As String, ByVal slistall As String, ByVal slistallforeign As String, ByVal sselectindex As String, ByVal sselect As String)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\", Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Dim path As String = txt_PathGenerate_Script & "InsertScript.sql"
        Dim pathupdate As String = txt_PathGenerate_Script & "UpdateScript.sql"
        Dim pathdelete As String = txt_PathGenerate_Script & "DeleteScript.sql"
        Dim pathlistall As String = txt_PathGenerate_Script & "ListAllScript.sql"
        Dim pathselect As String = txt_PathGenerate_Script & "SelectScript.sql"
        Dim pathselectindex As String = txt_PathGenerate_Script & "SelectIndexScript.sql"
        Dim pathlistallforeign As String = txt_PathGenerate_Script & "ListAllForeignScript.sql"

        Dim ds As DataSet

        For Each tr As TreeNode In TreeView1.Nodes
            If tr.Checked Then
                Dim node As New TreeNode
                node.Text = tr.Text
                'OracleHelper.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)

                '  s &= OracleHelper.CreateStore(tr.Text)
                's &= Chr(13)

                'supdate &= OracleHelper.UpdateStore(tr.Text)
                'supdate &= Chr(13)

                'sdelete &= OracleHelper.DeleteStore(tr.Text)
                'sdelete &= Chr(13)

                'slistall &= OracleHelper.ListAllStore(tr.Text)
                'slistall &= Chr(13)

                'sselect &= OracleHelper.SelectStore(tr.Text)
                'sselect &= Chr(13)

                'sselectindex &= OracleHelper.SelectByIndexStore(tr.Text)
                'sselectindex &= Chr(13)

                'slistallforeign &= OracleHelper.ListAllByForeignKey(tr.Text)
                'slistallforeign &= Chr(13)

                ds = OracleHelper.LoadTableStructure_Oracle(tr.Text)
                For Each dt As DataRow In ds.Tables(0).Rows
                    node.Nodes.Add(dt(1))
                Next
                TreeView2.Nodes.Add(node)
            End If
        Next

        REM on verifie si le repertoir existe bien
        If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
            If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\") Then
                Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\")
            End If
        Else
            If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\") Then
                Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\")
            End If
        End If

        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim fs_update As FileStream = File.Create(pathupdate, 1024)
        fs_update.Close()

        Dim fs_delete As FileStream = File.Create(pathdelete, 1024)
        fs_delete.Close()

        Dim fs_listAll As FileStream = File.Create(pathlistall, 1024)
        fs_listAll.Close()

        Dim fs_select As FileStream = File.Create(pathselect, 1024)
        fs_select.Close()

        Dim fs_selectindex As FileStream = File.Create(pathselectindex, 1024)
        fs_selectindex.Close()

        Dim fs_listAllforeign As FileStream = File.Create(pathlistallforeign, 1024)
        fs_listAllforeign.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim objWriterupdate As New System.IO.StreamWriter(pathupdate, True)
        Dim objWriterdelete As New System.IO.StreamWriter(pathdelete, True)
        Dim objWriterlistAll As New System.IO.StreamWriter(pathlistall, True)
        Dim objWriterSelect As New System.IO.StreamWriter(pathselect, True)
        Dim objWriterSelectIndex As New System.IO.StreamWriter(pathselectindex, True)
        Dim objWriterListallForeign As New System.IO.StreamWriter(pathlistallforeign, True)


        objWriter.WriteLine(s)
        objWriter.Close()


        objWriterupdate.WriteLine()
        objWriterupdate.WriteLine(supdate)
        objWriterupdate.Close()

        objWriterdelete.WriteLine()
        objWriterdelete.WriteLine(sdelete)
        objWriterdelete.Close()

        objWriterlistAll.WriteLine()
        objWriterlistAll.WriteLine(slistall)
        objWriterlistAll.Close()

        objWriterSelect.WriteLine()
        objWriterSelect.WriteLine(sselect)
        objWriterSelect.Close()

        objWriterSelectIndex.WriteLine()
        objWriterSelectIndex.WriteLine(sselectindex)
        objWriterSelectIndex.Close()

        With objWriterListallForeign

            .WriteLine(slistallforeign)
            .Close()
        End With
    End Sub

    Private Sub GenerateScriptForPostGres(ByVal s As String, ByVal supdate As String, ByVal sdelete As String, ByVal slistall As String, ByVal slistallforeign As String, ByVal sselectindex As String, ByVal sselect As String)
        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
        Dim path As String = txt_PathGenerate_Script & "InsertScript.sql"
        Dim pathupdate As String = txt_PathGenerate_Script & "UpdateScript.sql"
        Dim pathdelete As String = txt_PathGenerate_Script & "DeleteScript.sql"
        Dim pathlistall As String = txt_PathGenerate_Script & "ListAllScript.sql"
        Dim pathselect As String = txt_PathGenerate_Script & "SelectScript.sql"
        Dim pathselectindex As String = txt_PathGenerate_Script & "SelectIndexScript.sql"
        Dim pathlistallforeign As String = txt_PathGenerate_Script & "ListAllForeignScript.sql"

        Dim ds As DataSet

        For Each tr As TreeNode In TreeView1.Nodes
            If tr.Checked Then
                Dim node As New TreeNode
                node.Text = tr.Text

                s &= PostgresSqlManager.CreateStore(tr.Text)
                s &= Chr(13)

                supdate &= PostgresSqlManager.UpdateStore(tr.Text)
                supdate &= Chr(13)

                sdelete &= PostgresSqlManager.DeleteStore(tr.Text)
                sdelete &= Chr(13)

                slistall &= PostgresSqlManager.ListAllStore(tr.Text)
                slistall &= Chr(13)

                sselect &= PostgresSqlManager.SelectStore(tr.Text)
                sselect &= Chr(13)

                slistallforeign &= PostgresSqlManager.ListAllByForeignKey(tr.Text)
                slistallforeign &= Chr(13)



                PostgresSqlManager.CreateFile(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                PostgresSqlManager.CreateInterfaceCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName)
                PostgresSqlManager.CreateInterfaceCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                PostgresSqlManager.CreateListingCodeBehind(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName)
                PostgresSqlManager.CreateListingCodeAsp(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                '   SqlServerHelper.CreateTableInXMLFormat(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace)
                PostgresSqlManager.CreateAndroidModel(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                PostgresSqlManager.CreateAndroidHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                PostgresSqlManager.CreateAndroiDBHelper(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                PostgresSqlManager.CreateAndroidBinderListview(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                PostgresSqlManager.CreateAndroidFormActivity(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)
                PostgresSqlManager.CreateAndroidFormLayout(tr.Text, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_projectName)

                ds = PostgresSqlManager.LoadTableStructure(tr.Text)
                For Each dt As DataRow In ds.Tables(0).Rows
                    node.Nodes.Add(dt(0))
                Next
                TreeView2.Nodes.Add(node)
            End If
        Next

        REM on verifie si le repertoir existe bien

        If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
            If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\") Then
                Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
            End If
        Else
            If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\") Then
                Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\PostGresSQLScript\")
            End If
        End If


        Dim fs As FileStream = File.Create(path, 1024)
        fs.Close()

        Dim fs_update As FileStream = File.Create(pathupdate, 1024)
        fs_update.Close()

        Dim fs_delete As FileStream = File.Create(pathdelete, 1024)
        fs_delete.Close()

        Dim fs_listAll As FileStream = File.Create(pathlistall, 1024)
        fs_listAll.Close()

        Dim fs_select As FileStream = File.Create(pathselect, 1024)
        fs_select.Close()

        Dim fs_selectindex As FileStream = File.Create(pathselectindex, 1024)
        fs_selectindex.Close()

        Dim fs_listAllforeign As FileStream = File.Create(pathlistallforeign, 1024)
        fs_listAllforeign.Close()

        Dim objWriter As New System.IO.StreamWriter(path, True)
        Dim objWriterupdate As New System.IO.StreamWriter(pathupdate, True)
        Dim objWriterdelete As New System.IO.StreamWriter(pathdelete, True)
        Dim objWriterlistAll As New System.IO.StreamWriter(pathlistall, True)
        Dim objWriterSelect As New System.IO.StreamWriter(pathselect, True)
        Dim objWriterSelectIndex As New System.IO.StreamWriter(pathselectindex, True)
        Dim objWriterListallForeign As New System.IO.StreamWriter(pathlistallforeign, True)


        objWriter.WriteLine(s)
        objWriter.Close()


        objWriterupdate.WriteLine()
        objWriterupdate.WriteLine(supdate)
        objWriterupdate.Close()

        objWriterdelete.WriteLine()
        objWriterdelete.WriteLine(sdelete)
        objWriterdelete.Close()

        objWriterlistAll.WriteLine()
        objWriterlistAll.WriteLine(slistall)
        objWriterlistAll.Close()

        objWriterSelect.WriteLine()
        objWriterSelect.WriteLine(sselect)
        objWriterSelect.Close()

        objWriterSelectIndex.WriteLine()
        objWriterSelectIndex.WriteLine(sselectindex)
        objWriterSelectIndex.Close()

        With objWriterListallForeign

            .WriteLine(slistallforeign)
            .Close()
        End With
    End Sub

#End Region

#Region "Folder Browser"
    Public Sub Folder_Browser_Dialog(ByVal _FolderBrowserDialog As FolderBrowserDialog, ByVal _Textbox As TextBox)
        Try
            Dim dlgResult As DialogResult = _FolderBrowserDialog.ShowDialog()

            If dlgResult = Windows.Forms.DialogResult.OK Then
                _Textbox.Text = _FolderBrowserDialog.SelectedPath
            End If
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Folder Browser Dialog", MessageBoxButtons.OK)
            Error_Log("Folder_Browser_Dialog", ex.Message)
        End Try
    End Sub

    Private Sub Btn_FolderBrowserDialog_Script_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_FolderBrowserDialog_Script.Click
        Folder_Browser_Dialog(FolderBrowserDialog1, txt_PathGenerate_ScriptFile)
    End Sub

#End Region

#Region "Event on Connection"
    Private Sub cmb_server_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmb_server.TextChanged
        txt_ServerName.Text = cmb_server.Text
        If cmb_server.SelectedIndex > 0 Then
            'Dim _connection As New Cls_Connection(CLng(cmb_server.SelectedValue))
            'Txt_Login.Text = _connection.Login
            'Txt_Password.Text = _connection.Password
        End If

    End Sub

    Private Sub Btn_ConnexionServerName_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_ConnexionServerName.Click
        txt_DatabaseName.Text = rcmb_DatabaseName.Text
        Try
            Application.DoEvents()
            If rbtn_Oracle.Checked Then
                ValidateRequiredField_ForOracle()
            Else
                ValidateRequiredField_ForOthers()
            End If
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Connexion ServerName", MessageBoxButtons.OK)
            Error_Log("Btn_ConnexionServerName_Click", ex.Message)
        End Try
    End Sub

    Private Sub txt_ServerName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txt_ServerName.TextChanged, txt_DatabaseName.TextChanged, Txt_Login.TextChanged
        EnableBtnConnexion()
    End Sub

    Private Sub rbtn_Oracle_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtn_Oracle.CheckedChanged

        If rbtn_Oracle.Checked Then

        Else
            lbl_Servername.Text = "Server Name"
            lbl_Database_port.Text = "Database"
            txt_DatabaseName.Text = ""
        End If
    End Sub

    Private Sub rbtn_Oracle_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtn_Oracle.Click
        lbl_Servername.Text = "Instance/SID"
        lbl_Database_port.Text = "Port"
        txt_DatabaseName.Text = "1521"
        txt_ServerName.Text = "orcl"

        rcmb_DatabaseName.Text = txt_DatabaseName.Text
        cmb_server.Text = txt_ServerName.Text
        Txt_Login.Text = ""
        Txt_Password.Text = ""
    End Sub

    Private Sub rbtn_SqlServer_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtn_SqlServer.Click
        TypeDatabaseChecked = TypeDatabase.SQLSERVER
        lbl_Servername.Text = "Server Name"
        lbl_Database_port.Text = "Database"
        rcmb_DatabaseName.Text = ""
        Dim objs As New List(Of Cls_Connection)
        Dim connectionname As New List(Of String)
        If Not _systeme.getLastConnectionString Is Nothing Then
            For Each _connection2 As Cls_Connection In _systeme.SearchAllConnectionByType(TypeDatabase.SQLSERVER)
                If Not connectionname.Contains(_connection2.ServerName.Trim) Then
                    objs.Add(_connection2)
                    connectionname.Add(_connection2.ServerName.Trim)
                End If
            Next
            cmb_server.DataSource = objs
            cmb_server.DisplayMember = "ServerName"
            cmb_server.ValueMember = "ID"
            Dim _connection As New Cls_Connection(CLng(cmb_server.SelectedValue))
            Txt_Login.Text = _connection.Login
            Txt_Password.Text = _connection.Password
        Else
            cmb_server.Items.Clear()
            Dim dtable As DataTable = SmoApplication.EnumAvailableSqlServers(False)
            Dim ServerName As String
            For Each dr As DataRow In dtable.Rows
                ServerName = dr("Server").ToString()
                If Not Convert.IsDBNull(dr("Instance")) And dr("Instance").ToString().Length > 0 Then
                    ServerName += "\" & dr("Instance").ToString()
                End If
                If cmb_server.Items.IndexOf(ServerName) < 0 Then
                    cmb_server.Items.Add(ServerName)
                End If
                If cmb_server.Items.Count > 0 Then
                    cmb_server.SelectedIndex = 0
                End If
            Next
        End If
        ' bckWorkerForServers.RunWorkerAsync()
    End Sub

    Private Sub rbtn_MySql_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtn_MySql.Click
        TypeDatabaseChecked = TypeDatabase.MYSQL
        lbl_Servername.Text = "Server Name"
        lbl_Database_port.Text = "Database"
        rcmb_DatabaseName.Text = ""
        cmb_server.DataSource = Nothing
        Txt_Login.Text = ""
        Txt_Password.Text = ""
        rcmb_DatabaseName.Items.Clear()

        Dim objs As New List(Of Cls_Connection)
        Dim connectionname As New List(Of String)
        If Not _systeme.getLastConnectionString Is Nothing Then
            For Each _connection2 As Cls_Connection In _systeme.SearchAllConnectionByType(TypeDatabase.MYSQL)
                If Not connectionname.Contains(_connection2.ServerName.Trim) Then
                    objs.Add(_connection2)
                    connectionname.Add(_connection2.ServerName.Trim)
                End If
            Next
            cmb_server.DataSource = objs
            cmb_server.DisplayMember = "ServerName"
            cmb_server.ValueMember = "ID"
            Dim _connection As New Cls_Connection(CLng(cmb_server.SelectedValue))
            Txt_Login.Text = _connection.Login
            Txt_Password.Text = _connection.Password

        End If
    End Sub

    Private Sub rbtn_PostGres_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbtn_PostGres.Click
        TypeDatabaseChecked = TypeDatabase.POSTGRESSQL
        lbl_Servername.Text = "Server Name"
        lbl_Database_port.Text = "Database"
        rcmb_DatabaseName.Text = ""
        cmb_server.DataSource = Nothing
        Txt_Login.Text = ""
        Txt_Password.Text = ""
        rcmb_DatabaseName.Items.Clear()

        Dim objs As New List(Of Cls_Connection)
        Dim connectionname As New List(Of String)
        If Not _systeme.getLastConnectionString Is Nothing Then
            For Each _connection2 As Cls_Connection In _systeme.SearchAllConnectionByType(TypeDatabase.POSTGRESSQL)
                If Not connectionname.Contains(_connection2.ServerName.Trim) Then
                    objs.Add(_connection2)
                    connectionname.Add(_connection2.ServerName.Trim)
                End If
            Next
            cmb_server.DataSource = objs
            cmb_server.DisplayMember = "ServerName"
            cmb_server.ValueMember = "ID"
            Dim _connection As New Cls_Connection(CLng(cmb_server.SelectedValue))
            Txt_Login.Text = _connection.Login
            Txt_Password.Text = _connection.Password

        End If
    End Sub

    Private Sub rcmb_DatabaseName_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles rcmb_DatabaseName.DropDown
        If Not (rbtn_Oracle.Checked Or rbtn_MySql.Checked Or rbtn_PostGres.Checked) Then
            Try
                rcmb_DatabaseName.Items.Clear()
                Dim servconn As New ServerConnection(txt_ServerName.Text)
                servconn.LoginSecure = False
                servconn.Login = Txt_Login.Text
                servconn.Password = Txt_Password.Text
                Dim server As New Server(servconn)
                For Each db As Database In server.Databases
                    If Not rcmb_DatabaseName.Items.Contains(db.Name) Then
                        rcmb_DatabaseName.Items.Add(db.Name)
                    End If
                Next
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
        If rbtn_MySql.Checked Then
            Try
                Dim myConStr As String = "Data Source=" & cmb_server.Text & ";User Id=" & Txt_Login.Text & ";Pwd=" & Txt_Password.Text & ";"
                Dim myConnection As New MySqlConnection(myConStr)
                myConnection.Open()
                Dim cmd As New MySqlCommand
                cmd.CommandText = "SHOW DATABASES"
                cmd.CommandType = CommandType.Text
                cmd.Connection = myConnection
                Dim ds As New DataSet
                Dim da As MySqlDataAdapter
                da = New MySqlDataAdapter(cmd)
                da.Fill(ds)
                cmd.Parameters.Clear()
                myConnection.Close()
                For Each row As DataRow In ds.Tables(0).Rows
                    If Not rcmb_DatabaseName.Items.Contains(row(0)) Then
                        rcmb_DatabaseName.Items.Add(row(0))
                    End If

                Next
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
        If rbtn_PostGres.Checked Then
            Try
                Dim servername As String = txt_ServerName.Text
                Dim port As String = "5432"
                Dim user_login As String = Txt_Login.Text
                Dim password As String = Txt_Password.Text


                Dim ConString As String = String.Format("Server={0};Port={1};User Id={2};Password={3};", servername, port, user_login, password)
                Dim myConStr As String = "Data Source=" & cmb_server.Text & ";User Id=" & Txt_Login.Text & ";Pwd=" & Txt_Password.Text & ";"
                Dim myConnection As New NpgsqlConnection(ConString)
                myConnection.Open()
                Dim cmd As New NpgsqlCommand
                cmd.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = false;"
                cmd.CommandType = CommandType.Text
                cmd.Connection = myConnection
                Dim ds As New DataSet
                Dim da As NpgsqlDataAdapter
                da = New NpgsqlDataAdapter(cmd)
                da.Fill(ds)
                cmd.Parameters.Clear()
                myConnection.Close()
                For Each row As DataRow In ds.Tables(0).Rows
                    If Not rcmb_DatabaseName.Items.Contains(row(0)) Then
                        rcmb_DatabaseName.Items.Add(row(0))
                    End If

                Next
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Private Sub rcmb_DatabaseName_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rcmb_DatabaseName.TextChanged
        If rcmb_DatabaseName.Text <> "" Then
            txt_DatabaseName.Text = rcmb_DatabaseName.Text
            EnableBtnConnexion()
        End If
    End Sub

#End Region

#Region "Sub for Connection"
    Private Sub ValidateRequiredField_ForOthers()

        If txt_DatabaseName.Text.Trim <> "" And txt_ServerName.Text.Trim <> "" And Txt_Login.Text.Trim <> "" And Txt_Password.Text.Trim <> "" Then
            GiveConnectionString_ToHelper()
            If rbtn_MySql.Checked Then
                MySqlManager.LoadUserTablesSchema(TreeView1)
                MySqlManager.InitializeDb()
                BackgroundWorker1.RunWorkerAsync()

            ElseIf rbtn_SqlServer.Checked Then
                Dim DatabaseName As String = txt_DatabaseName.Text
                txt_LibraryName.Text = DatabaseName.Replace("db", "") + "Library"
                SqlServerHelper.LoadUserTablesSchema(txt_ServerName.Text.Trim, Txt_Login.Text.Trim, Txt_Password.Text, txt_DatabaseName.Text, TreeView1)
                'If rbtnIMode_Yes.Checked Then
                'If SqlServerHelper.InitializeDb(txt_ServerName.Text.Trim, Txt_Login.Text.Trim, Txt_Password.Text, txt_DatabaseName.Text) > 0 Then

                '    BackgroundWorkerTrue1.RunWorkerAsync()
                '    BackgroundWorkerTrue2.RunWorkerAsync()
                '    BackgroundWorkerTrue3.RunWorkerAsync()
                '    BackgroundWorkerTrue4.RunWorkerAsync()
                '    BackgroundWorkerTrue5.RunWorkerAsync()
                '    'BackgroundWorkerTrue6.RunWorkerAsync()
                '    'BackgroundWorkerTrue7.RunWorkerAsync()
                '    'BackgroundWorkerTrue8.RunWorkerAsync()
                '    'BackgroundWorkerTrue9.RunWorkerAsync()
                '    'BackgroundWorkerTrue10.RunWorkerAsync()

                'End If
                '' BackgroundWorker1.RunWorkerAsync()


                ''End If
                'SetupImode()

            ElseIf rbtn_PostGres.Checked Then
                txt_LibraryName.Text = txt_DatabaseName.Text + "Library"
                PostgresSqlManager.LoadUserTablesSchema(TreeView1)
                PostgresSqlManager.InitializeDb()
                BackgroundWorker1.RunWorkerAsync()

            ElseIf rbtn_Oracle.Checked Then

            Else
                MessageBox.Show("Pas de base de donnees selectionnee")
            End If

            Btn_GenererScript.Enabled = True
            REM select Table
            TabControl_Form.DeselectTab(TabParametre)
            TabControl_Form.SelectTab(TabBaseDeDonnees)
        Else
            MessageBox.Show("Les Paramètres de Connexion à la Base de Données sont obligatoires")
        End If
    End Sub

    Private Sub ValidateRequiredField_ForOracle()
        If txt_ServerName.Text.Trim <> "" And Txt_Login.Text.Trim <> "" And Txt_Password.Text.Trim <> "" Then

            GiveConnectionString_ToHelper()

            If rbtn_MySql.Checked Then
                MySqlHelper.LoadUserTablesSchema_MySql(txt_ServerName.Text.Trim, Txt_Login.Text.Trim, Txt_Password.Text, txt_DatabaseName.Text, TreeView1)
            ElseIf rbtn_SqlServer.Checked Then    ' Si c'est une connexion MySql Server
                SqlServerHelper.LoadUserTablesSchema(txt_ServerName.Text.Trim, Txt_Login.Text.Trim, Txt_Password.Text, txt_DatabaseName.Text, TreeView1)
            ElseIf rbtn_Oracle.Checked Then
                OracleHelper.LoadUserTablesSchema_Oracle(txt_ServerName.Text.Trim, Txt_Login.Text.Trim, Txt_Password.Text, txt_DatabaseName.Text, TreeView1)

            Else
                MessageBox.Show("Pas de base de donnees selectionnee")
            End If
            Btn_GenererScript.Enabled = True
            REM select Table
            TabControl_Form.DeselectTab(TabParametre)
            TabControl_Form.SelectTab(TabBaseDeDonnees)
        Else
            MessageBox.Show("Les Paramètres de Connexion à la Base de Données sont obligatoires")
        End If
    End Sub

    Private Sub GiveConnectionString_ToHelper()
        MySqlHelper.database = txt_DatabaseName.Text.Trim
        MySqlHelper.servername = txt_ServerName.Text.Trim
        MySqlHelper.password = Txt_Password.Text.Trim
        MySqlHelper.user_login = Txt_Login.Text.Trim
        SqlServerHelper.database = txt_DatabaseName.Text.Trim
        SqlServerHelper.servername = txt_ServerName.Text.Trim
        SqlServerHelper.password = Txt_Password.Text.Trim
        SqlServerHelper.user_login = Txt_Login.Text.Trim

        MySqlManager.servername = txt_ServerName.Text.Trim
        MySqlManager.user_login = Txt_Login.Text.Trim
        MySqlManager.password = Txt_Password.Text.Trim
        MySqlManager.database = txt_DatabaseName.Text.Trim

        PostgresSqlManager.servername = txt_ServerName.Text.Trim
        PostgresSqlManager.user_login = Txt_Login.Text.Trim
        PostgresSqlManager.password = Txt_Password.Text.Trim
        PostgresSqlManager.database = txt_DatabaseName.Text.Trim

        OracleHelper.database = txt_DatabaseName.Text.Trim
        OracleHelper.servername = txt_ServerName.Text.Trim
        OracleHelper.password = Txt_Password.Text.Trim
        OracleHelper.user_login = Txt_Login.Text.Trim
    End Sub

    Public Sub EnableBtnConnexion()
        If rbtn_Oracle.Checked Then
            Btn_ConnexionServerName.Enabled = IIf(txt_ServerName.Text.Trim.Equals("") OrElse Txt_Login.Text.Trim.Equals(""), False, True)
        Else
            Btn_ConnexionServerName.Enabled = IIf(txt_ServerName.Text.Trim.Equals("") OrElse txt_DatabaseName.Text.Trim.Equals("") OrElse Txt_Login.Text.Trim.Equals(""), False, True)
        End If

    End Sub
#End Region

#Region "NameSpace "

    Private Sub Btn_AddNameSpace_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_AddNameSpace.Click
        Try
            Application.DoEvents()
            If txt_NameSpace.Text.Trim <> "" Then
                Dim i As Integer = 0
                For y As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                    If ListBox_NameSpace.Items(y) = txt_NameSpace.Text.Trim Then
                        Throw (New System.Exception("Espace de nom [ " & txt_NameSpace.Text & " ] existe déjà."))
                    End If
                Next
                ListBox_NameSpace.Items.Insert(i, txt_NameSpace.Text.Trim)
                txt_NameSpace.Text = ""
                i += i
            Else
                MessageBox.Show("Entrer l'espace de nom (NameSpace)", ".:: Erreur Add NameSpace Log ::.", MessageBoxButtons.OK)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, ".:: Erreur Add NameSpace Log ::.", MessageBoxButtons.OK)
            Error_Log("Folder_Browser_Dialog", ex.Message)
        End Try
    End Sub

    Private Sub Btn_Remove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Btn_Remove.Click
        Try
            Application.DoEvents()
            For i As Integer = 0 To ListBox_NameSpace.Items.Count - 1
                If ListBox_NameSpace.SelectedIndex = i Then
                    ListBox_NameSpace.Items.RemoveAt(i)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message, ".:: Erreur Add NameSpace Log ::.", MessageBoxButtons.OK)
            Error_Log("Folder_Browser_Dialog", ex.Message)
        End Try
    End Sub

    Private Sub ListBox_NameSpace_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox_NameSpace.SelectedIndexChanged
        Btn_Remove.Enabled = IIf(ListBox_NameSpace.SelectedIndex >= 0, True, False)
    End Sub

    Private Sub txt_NameSpace_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txt_NameSpace.TextChanged
        Btn_AddNameSpace.Enabled = IIf(Not txt_NameSpace.Text.Trim.Equals(""), True, False)
    End Sub

#End Region

    Private Sub afterChecked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TreeView2.AfterCheck
        For Each tn1 As TreeNode In e.Node.Nodes
            tn1.Checked = e.Node.Checked
        Next
    End Sub

#Region "ERREUR"
    Public Shared Sub Error_Log(ByVal requestedMethod As String, ByVal errorMessage As String)
        Try
            Dim sw As StreamWriter
            Dim _path As String = Application.StartupPath & "\Log\"
            If Not Directory.Exists(_path) Then
                Directory.CreateDirectory(_path)
            End If
            sw = New StreamWriter(_path + Date.Now.ToString("dd-MMM-yyy") + ".txt", True)
            'sw = New StreamWriter(System.Configuration.ConfigurationManager.AppSettings("logDirectory") + Date.Now.ToString("dd-MMM-yyy") + ".txt", True)

            sw.WriteLine(Date.Now.ToString("hh:mm --> ") + " - " + requestedMethod + "; Error : " + errorMessage)
            sw.Flush()
            sw.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, ".:: Erreur Save Log ::.", MessageBoxButtons.OK)
        End Try
    End Sub

    Public Shared Sub Error_Log(ByVal errorMessage As Exception)
        Try
            Dim sw As StreamWriter
            Dim _path As String = Application.StartupPath & "\Log\"
            If Not Directory.Exists(_path) Then
                Directory.CreateDirectory(_path)
            End If
            sw = New StreamWriter(_path + Date.Now.ToString("dd-MMM-yyy") + ".txt", True)
            'sw = New StreamWriter(System.Configuration.ConfigurationManager.AppSettings("logDirectory") + Date.Now.ToString("dd-MMM-yyy") + ".txt", True)

            sw.WriteLine(Date.Now.ToString("hh:mm --> ") + ": Error : " + errorMessage.ToString)
            sw.Flush()
            sw.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, ".:: Erreur Save Log ::.", MessageBoxButtons.OK)
        End Try
    End Sub
#End Region

#Region "Taches Asynchrones"

    Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)

        End If

    End Sub


    'Private Sub BackgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
    '    progressBar_IMode.Value = e.ProgressPercentage
    'End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        UpdateFormDelegate1 = New UpdateFormDelegate(AddressOf EnableBtnGenerate)
        Me.Invoke(UpdateFormDelegate1)
    End Sub

    Private Sub BackgroundWorker1second_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1second.DoWork
        SqlServerHelper.InitializeLocalColumn2(_systeme.currentDatabase.ID, 3)
    End Sub

    Private Sub BackgroundWorker1second_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1second.RunWorkerCompleted
        UpdateFormDelegate1 = New UpdateFormDelegate(AddressOf EnableBtnGenerate)
        Me.Invoke(UpdateFormDelegate1)
    End Sub

    Private Sub BackgroundWorker2_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        SqlServerHelper.InitializeLocalColumn2(_systeme.currentDatabase.ID, 2)
    End Sub

    Private Sub BackgroundWorker2_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker2.RunWorkerCompleted
        UpdateFormDelegate1 = New UpdateFormDelegate(AddressOf EnableBtnGenerate)
        Me.Invoke(UpdateFormDelegate1)
    End Sub

    Private Sub EnableBtnGenerate()
        ' WorkIsDone = WorkIsDone + 1
        ' If WorkIsDone = 3 Then
        '  Btn_GenererScript.Enabled = True
        IntelligentMode = True
        ' End If
    End Sub

    Private Sub bckWorkerForServers_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bckWorkerForServers.DoWork

    End Sub



#End Region

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

#Region "IMode Event"
    Private Sub rbtnIMode_No_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnIMode_No.CheckedChanged
        SetupImode()
    End Sub

    Private Sub SetupImode()
        If rbtnIMode_Yes.Checked Then
            lbl_IMode.Visible = True
            progressBar_IMode.Visible = True
            btn_Refresh.Visible = True
        Else
            progressBar_IMode.Visible = False
            lbl_IMode.Visible = False
            btn_Refresh.Visible = False
        End If
    End Sub

#End Region

#Region "Groupe "

    Private Sub btn_Refresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_Refresh.Click
        TreeView1.Nodes.Clear()
        txt_DatabaseName.Text = rcmb_DatabaseName.Text
        Try
            Application.DoEvents()
            If rbtn_Oracle.Checked Then
                ValidateRequiredField_ForOracle()
            Else
                ValidateRequiredField_ForOthers()
            End If
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Connexion ServerName", MessageBoxButtons.OK)
            Error_Log("Btn_ConnexionServerName_Click", ex.Message)
        End Try
    End Sub


    Private Sub bnt_AddGroupTable_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bnt_AddGroupTable.Click
        Dim frm As New wnfrm_GroupTable
        frm.ShowDialog()
    End Sub

    Private Sub BindGrid()
        Dim objs As List(Of Cls_GroupTable)
        objs = Cls_GroupTable.SearchAll


        dtgGroupe.DataSource = objs
        'For Each Column As DataGridViewc In dtgGroupe.Columns

        '    Select Case Column.ColumnName
        '        Case "Isdirty"

        '        Case "LogData"
        '        Case "ChildTable"
        '        Case "Id_ChildTable"
        '        Case "ParentTable"
        '        Case "Id_ParentTable"
        '        Case "Id_LiaisonTable"
        '        Case "LiaisonTable"
        '        Case "Id"

        '    End Select
        'Next
        dtgGroupe.Columns.Item(0).Visible = False
        dtgGroupe.Columns.Item(2).Visible = False
        dtgGroupe.Columns.Item(3).Visible = False
        dtgGroupe.Columns.Item(5).Visible = False
        dtgGroupe.Columns.Item(6).Visible = False
        dtgGroupe.Columns.Item(8).Visible = False
        dtgGroupe.Columns.Item(9).Visible = False
        dtgGroupe.Columns.Item(10).Visible = True
        dtgGroupe.Columns.Item(11).Visible = False
        dtgGroupe.Columns.Item(12).Visible = False


    End Sub

    Private Sub BindTreeViewGroupe()
        treeViewGroupe.Nodes.Clear()
        Dim objs As List(Of Cls_GroupTable)
        objs = Cls_GroupTable.SearchAll

        For Each groupe As Cls_GroupTable In objs
            treeViewGroupe.Nodes.Add(groupe.Name)
        Next
        For Each node As TreeNode In treeViewGroupe.Nodes
            For Each groupe As Cls_GroupTable In objs
                If node.Text = groupe.Name Then
                    node.Nodes.Add(groupe.ParentTable_Name)
                    If groupe.Id_LiaisonTable <> 0 Then
                        node.Nodes.Add(groupe.LiasonTable_Name)
                    End If
                    node.Nodes.Add(groupe.ChildTable_Name)
                End If
            Next
            For Each innernode As TreeNode In node.Nodes
                Dim table As New Cls_Table(Cls_Database.GetLastDatabase.ID, innernode.Text)
                For Each Column As Cls_Column In table.ListofColumn
                    innernode.Nodes.Add(Column.Name)
                Next

            Next
        Next



    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        BindGrid()
        BindTreeViewGroupe()
    End Sub

    Private Sub btnOpenOutput_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOpenOutput.Click
        'OpenExplorer("c:\test")
        If txt_PathGenerate_ScriptFile.Text = "" Then
            Process.Start(Application.StartupPath & "\SCRIPT\GENERIC_12\")
        Else
            Process.Start(txt_PathGenerate_ScriptFile.Text & "\SCRIPT\GENERIC_12")
        End If
    End Sub

    Private Sub btnGenerateScriptGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGenerateScriptGroup.Click
        GenerateGroupScriptSimple()
        GenerateGroupScriptSpecific()
    End Sub

    Private Sub GenerateGroupScriptSimple()
        Dim ssupdateparentaddchild As String = ""
        Dim ssupdateparentremovechild As String = ""
        Dim slistallchildinparent As String = ""
        Dim slistallchildnotintparent As String = ""


        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
        Dim pathupdateparentaddchild As String = txt_PathGenerate_Script & "UpdateParentAddChildScript.sql"
        Dim pathupdateparentremovechild As String = txt_PathGenerate_Script & "UpdateParentRemoveChildScript.sql"
        Dim pathlistallparentinchild As String = txt_PathGenerate_Script & "ListAllParentInChildScript.sql"
        Dim pathlistallparentnotinchild As String = txt_PathGenerate_Script & "ListAllParentNotInChildScript.sql"

        If IntelligentMode And rbtnIMode_Yes.Checked Then


            ssupdateparentaddchild &= SqlServer.IMode.ScriptGenerator.UpdateStoreAddChild()
            ssupdateparentaddchild &= Chr(13)
            ssupdateparentremovechild &= SqlServer.IMode.ScriptGenerator.UpdateStoreRemoveChild()
            ssupdateparentremovechild &= Chr(13)
            slistallchildinparent &= SqlServer.IMode.ScriptGenerator.ListAllChildinParent()
            slistallchildinparent &= Chr(13)
            slistallchildnotintparent &= SqlServer.IMode.ScriptGenerator.ListAllChildNotinParent()
            slistallchildnotintparent &= Chr(13)

            Dim objWriterUpdateparentaddchild As New System.IO.StreamWriter(pathupdateparentaddchild, True, System.Text.Encoding.UTF8)
            Dim objWriterUpdateParentRemovechild As New System.IO.StreamWriter(pathupdateparentremovechild, True, System.Text.Encoding.UTF8)
            Dim objWriterlistAllparentinchild As New System.IO.StreamWriter(pathlistallparentinchild, True, System.Text.Encoding.UTF8)
            Dim objWriterlistAllparentnotinChild As New System.IO.StreamWriter(pathlistallparentnotinchild, True, System.Text.Encoding.UTF8)

            With objWriterUpdateparentaddchild

                .WriteLine(ssupdateparentaddchild)
                .Close()
            End With

            With objWriterUpdateParentRemovechild

                .WriteLine(ssupdateparentremovechild)
                .Close()
            End With

            With objWriterlistAllparentinchild

                .WriteLine(slistallchildinparent)
                .Close()
            End With

            With objWriterlistAllparentnotinChild

                .WriteLine(slistallchildnotintparent)
                .Close()
            End With

            SqlServer.IMode.VbClassGenerator.CreateFileForParent(txt_PathGenerate_ScriptFile, ListBox_NameSpace)
            SqlServer.IMode.VbClassGenerator.CreateFileForChild(txt_PathGenerate_ScriptFile, ListBox_NameSpace)
            For Each group In Cls_GroupTable.SearchAll
                SqlServer.IMode.AspFormGenerator.CreateParentChildInterfaceAsp("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
                SqlServer.IMode.AspFormGenerator.CreateParentChildInterfaceCodeBehind("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
                SqlServer.IMode.AspFormGenerator.CreateSendRemoveChildInParentAsp("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group)
                SqlServer.IMode.AspFormGenerator.CreateSendRemoveChildInParentBehind("", txt_PathGenerate_ScriptFile, ListBox_NameSpace, group, txt_LibraryName.Text)
                SqlServer.IMode.AspFormGenerator.CreateParentListingCodeAsp(group.ParentTable_Name, txt_PathGenerate_ScriptFile, ListBox_NameSpace, group)
                SqlServer.IMode.AspFormGenerator.CreateParentListingCodeBehind(group.ParentTable_Name, txt_PathGenerate_ScriptFile, ListBox_NameSpace, txt_LibraryName, group)
            Next

        End If
    End Sub

    Private Sub GenerateGroupScriptSpecific()

    End Sub

#End Region

#Region "Report Generation"

    Private Sub SetupReportGeneration(Optional ByVal includePrincipal As Boolean = True)
        If includePrincipal Then
            FillComboTablePrincipale()
        End If
        FillComboTypeReport()
        FillComboOtherReportTable()
        BindgridReportGenerationTable()
        BindgridReportGenerationParameter()
        BindgridReportGenerationParameterDate()
        BindgridReportGenerationWhereCondition()
    End Sub

    Private Sub BindgridReportGenerationTable()
        Dim objs As List(Of Cls_ReportDynamicTable)
        objs = Cls_ReportDynamicTable.SearchAll


        dtgOtherReportTable.DataSource = objs

        dtgOtherReportTable.Columns.Item(0).Visible = False
        dtgOtherReportTable.Columns.Item(1).Visible = False
        dtgOtherReportTable.Columns.Item(2).Visible = False
        dtgOtherReportTable.Columns.Item(3).Visible = False
        dtgOtherReportTable.Columns.Item(4).Visible = False
        dtgOtherReportTable.Columns.Item(5).Visible = False
        dtgOtherReportTable.Columns.Item(8).Visible = False
        dtgOtherReportTable.Columns.Item(9).Visible = False
        dtgOtherReportTable.Columns.Item(10).Visible = False
        'dtgGroupe.Columns.Item(8).Visible = False
        'dtgGroupe.Columns.Item(9).Visible = False
        'dtgGroupe.Columns.Item(10).Visible = True
        'dtgGroupe.Columns.Item(11).Visible = False
        'dtgGroupe.Columns.Item(12).Visible = False
    End Sub

    Private Sub BindgridReportGenerationWhereCondition()
        Dim objs As List(Of Cls_ReportDynamicWhereCondition)
        objs = Cls_ReportDynamicWhereCondition.SearchAll


        rdgWhereCondition.DataSource = objs

        rdgWhereCondition.Columns.Item(0).Visible = False
        rdgWhereCondition.Columns.Item(1).Visible = False
        rdgWhereCondition.Columns.Item(2).Visible = False
        rdgWhereCondition.Columns.Item(3).Visible = False
        rdgWhereCondition.Columns.Item(4).Visible = False
        rdgWhereCondition.Columns.Item(5).Visible = False
        rdgWhereCondition.Columns.Item(7).Visible = False
        rdgWhereCondition.Columns.Item(8).Visible = False
        rdgWhereCondition.Columns.Item(10).Visible = False
        rdgWhereCondition.Columns.Item(11).Visible = False
        rdgWhereCondition.Columns.Item(13).Visible = False
        rdgWhereCondition.Columns.Item(14).Visible = False
        rdgWhereCondition.Columns.Item(16).Visible = False
        rdgWhereCondition.Columns.Item(17).Visible = False
        'dtgGroupe.Columns.Item(8).Visible = False
        'dtgGroupe.Columns.Item(9).Visible = False
        'dtgGroupe.Columns.Item(10).Visible = True
        'dtgGroupe.Columns.Item(11).Visible = False
        'dtgGroupe.Columns.Item(12).Visible = False
    End Sub

    Private Sub BindgridReportGenerationParameter()
        Dim objs As List(Of Cls_ReportDynamicParameter)
        objs = Cls_ReportDynamicParameter.SearchAll


        dtgReportParameters.DataSource = objs
        'For Each Column As DataGridViewc In dtgGroupe.Columns

        '    Select Case Column.ColumnName
        '        Case "Isdirty"

        '        Case "LogData"
        '        Case "ChildTable"
        '        Case "Id_ChildTable"
        '        Case "ParentTable"
        '        Case "Id_ParentTable"
        '        Case "Id_LiaisonTable"
        '        Case "LiaisonTable"
        '        Case "Id"

        '    End Select
        'Next
        dtgReportParameters.Columns.Item(0).Visible = False
        ' dtgReportParameters.Columns.Item(1).Visible = False
        dtgReportParameters.Columns.Item(2).Visible = False
        dtgReportParameters.Columns.Item(3).Visible = False
        dtgReportParameters.Columns.Item(4).Visible = False
        dtgReportParameters.Columns.Item(5).Visible = False
        dtgReportParameters.Columns.Item(6).Visible = False
        'dtgReportParameters.Columns.Item(7).Visible = False
        dtgReportParameters.Columns.Item(8).Visible = False
        dtgReportParameters.Columns.Item(9).Visible = False
        '  dtgReportParameters.Columns.Item(10).Visible = True
        dtgReportParameters.Columns.Item(11).Visible = False
        dtgReportParameters.Columns.Item(12).Visible = False
        dtgReportParameters.Columns.Item(13).Visible = False
        ' dtgReportParameters.Columns.Item(14).Visible = False
    End Sub

    Private Sub BindgridReportGenerationParameterDate()
        Dim objs As List(Of Cls_ReportDynamicParameterDate)
        objs = Cls_ReportDynamicParameterDate.SearchAll


        dtgDateParameter.DataSource = objs
        'For Each Column As DataGridViewc In dtgGroupe.Columns

        '    Select Case Column.ColumnName
        '        Case "Isdirty"

        '        Case "LogData"
        '        Case "ChildTable"
        '        Case "Id_ChildTable"
        '        Case "ParentTable"
        '        Case "Id_ParentTable"
        '        Case "Id_LiaisonTable"
        '        Case "LiaisonTable"
        '        Case "Id"

        '    End Select
        'Next
        dtgDateParameter.Columns.Item(0).Visible = False
        ' dtgReportParameters.Columns.Item(1).Visible = False
        dtgDateParameter.Columns.Item(2).Visible = False
        dtgDateParameter.Columns.Item(3).Visible = False
        dtgDateParameter.Columns.Item(4).Visible = False
        dtgDateParameter.Columns.Item(5).Visible = False
        dtgDateParameter.Columns.Item(6).Visible = False
        'dtgReportParameters.Columns.Item(7).Visible = False
        dtgDateParameter.Columns.Item(8).Visible = False
        dtgDateParameter.Columns.Item(9).Visible = False
        '  dtgReportParameters.Columns.Item(10).Visible = True
        dtgDateParameter.Columns.Item(11).Visible = False
        dtgDateParameter.Columns.Item(12).Visible = False
        dtgDateParameter.Columns.Item(13).Visible = False
        ' dtgReportParameters.Columns.Item(14).Visible = False
    End Sub


    Private Sub FillComboTablePrincipale()

        Dim objs As List(Of Cls_Table) = Cls_Table.SearchAllBy_Database(Cls_Database.GetLastDatabase.ID)

        With ddl_Parent
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With
    End Sub

    Private Sub FillComboOtherReportTable()

        Dim objs As List(Of Cls_Table) = Cls_Table.SearchAllBy_DatabaseExeptTable(Cls_Database.GetLastDatabase.ID, ddl_Parent.SelectedValue)

        With ddl_OtherReportTable
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With
    End Sub

    Private Sub BindTreeviewReport()
        treeView_ReportColumn.Nodes.Clear()
        Dim objs As List(Of Cls_ReportDynamicTable)
        objs = Cls_ReportDynamicTable.SearchAllbyId_ReportDynamic(GlobalVariables.Id_ReportDynamic)

        Dim i As Integer = 0
        For Each reporttab As Cls_ReportDynamicTable In objs
            treeView_ReportColumn.Nodes.Add(reporttab.CompleteName)
            treeView_ReportColumn.Nodes(i).Tag = reporttab.Id
            i = i + 1
        Next


        For Each innernode As TreeNode In treeView_ReportColumn.Nodes
            Dim reportdynamictable As New Cls_ReportDynamicTable(innernode.Tag)
            Dim table As New Cls_Table(Cls_Database.GetLastDatabase.ID, reportdynamictable.Table.Name)
            i = 0
            For Each Column As Cls_Column In table.ListofColumn
                innernode.Nodes.Add(Column.Name)
                innernode.Nodes(i).Tag = Column.ID
                i = i + 1
            Next

        Next


        'For Each node As TreeNode In treeViewGroupe.Nodes
        '    For Each rpt As Cls_ReportDynamicTable In objs
        '        If node.Text = rpt.Table_Name Then
        '            node.Nodes.Add(rpt.Table)
        '            If groupe.Id_LiaisonTable <> 0 Then
        '                node.Nodes.Add(groupe.LiasonTable_Name)
        '            End If
        '            node.Nodes.Add(groupe.ChildTable_Name)
        '        End If
        '    Next

        'Next
    End Sub

    Private Sub FillComboTypeReport()

        Dim objs As List(Of Cls_TypeReport) = Cls_TypeReport.SearchAll

        With ddl_TypeReport
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With
    End Sub

    Private Sub btnValidatePrincipalTable_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnValidatePrincipalTable.Click
        Try

            If txt_ReportProcName.Text = "" Then
                Throw (New System.Exception("Le nom de la procédure n'est pas renseignée"))
            End If

            If txt_AliasMaster.Text = "" Then
                Throw (New System.Exception("L'alias de la table n'est pas renseignée"))
            End If

            Dim obj As New Cls_ReportDynamic
            With obj
                .Id_TypeReport = CLng(ddl_TypeReport.SelectedValue)
                .Id_MasterTable = CLng(ddl_Parent.SelectedValue)
                .Name = txt_ReportProcName.Text
                .Insert()
            End With

            GlobalVariables.Id_ReportDynamic = obj.Id
            GlobalVariables.Id_MasterTableReportDynamic = CLng(ddl_Parent.SelectedValue)

            pnlDetailReportGeneration.Visible = True
            Dim rptmas As New Cls_ReportDynamicTable
            With rptmas
                .Id_ReportDynamic = GlobalVariables.Id_ReportDynamic
                .Id_Table = CLng(ddl_Parent.SelectedValue)
                .AliasString = txt_AliasMaster.Text
                .Insert()
            End With
            BindTreeviewReport()
            If ddl_TypeReport.SelectedValue = 2 Then
                pnlParametreDate.Visible = True
            Else
                pnlParametreDate.Visible = False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub btnAddNewTable_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddNewTable.Click

        Try
            If txtAliasReportTable.Text = "" Then
                Throw (New System.Exception("Alias obligatoire"))
            End If
            Dim obj As New Cls_ReportDynamicTable
            With obj
                .Id_Table = CLng(ddl_OtherReportTable.SelectedValue)
                .Id_ReportDynamic = GlobalVariables.Id_ReportDynamic
                .AliasString = txtAliasReportTable.Text
                .Insert()
            End With
            SetupReportGeneration(False)
            BindTreeviewReport()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try


    End Sub

    Private Sub btnAddReportParameter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddReportParameter.Click
        Dim frm As New wnfrm_ReportDynamicParameter
        frm.ShowDialog()
        SetupReportGeneration()
    End Sub

    Private Sub btnSaveReport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            For Each innernode As TreeNode In treeView_ReportColumn.Nodes
                For Each inneinnernnode As TreeNode In innernode.Nodes
                    Dim obj As New Cls_ReportDynamicColumn
                    With obj
                        .Id_Column = inneinnernnode.Tag
                        .Id_ReportDynamicTable = innernode.Tag
                        .Insert()
                    End With
                Next
            Next


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub tabReportGeneration_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles tabReportGeneration.Click
        SetupReportGeneration()
    End Sub

    Private Sub TabControl_Form_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TabControl_Form.SelectedIndexChanged
        If TabControl_Form.SelectedIndex = 4 Then
            SetupReportGeneration()
        End If
    End Sub

    Private Sub ddl_Parent_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_Parent.SelectedIndexChanged
        SetupReportGeneration(False)
    End Sub

    Private Sub btnGenerateReportScript_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGenerateReportScript.Click
        'Dim columnsToShow As List(Of Cls_ReportDynamicColumn) = Cls_ReportDynamicColumn.SearchAllbyId_Report(GlobalVariables.Id_ReportDynamic)
        'For Each column As Cls_ReportDynamicColumn In columnsToShow

        'Next

        Try
            For Each innernode As TreeNode In treeView_ReportColumn.Nodes
                For Each inneinnernnode As TreeNode In innernode.Nodes
                    If inneinnernnode.Checked Then
                        Dim obj As New Cls_ReportDynamicColumn(innernode.Tag, inneinnernnode.Tag)
                        With obj
                            .Id_Column = inneinnernnode.Tag
                            .Id_ReportDynamicTable = innernode.Tag
                            If .Id = 0 Then
                                .Insert()
                            End If
                        End With
                    End If
                Next
            Next


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        Dim txt_PathGenerate_Script As String = IIf(txt_PathGenerate_ScriptFile.Text.Trim <> "", txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\", Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
        Dim pathgeneratereport As String = txt_PathGenerate_Script & txt_ReportProcName.Text & ".sql"
        Dim s As String = ""


        s &= SqlServer.IMode.ScriptGenerator.GenerateReport(GlobalVariables.Id_ReportDynamic)
        s &= Chr(13)


        If txt_PathGenerate_ScriptFile.Text.Trim <> "" Then
            If Not Directory.Exists(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                Directory.CreateDirectory(txt_PathGenerate_ScriptFile.Text.Trim & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
            End If
        Else
            If Not Directory.Exists(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\") Then
                Directory.CreateDirectory(Application.StartupPath & "\SCRIPT\GENERIC_12\" & txt_DatabaseName.Text.Trim & "\SQLServerScript\")
            End If
        End If

        Dim fs As FileStream = File.Create(pathgeneratereport, 1024)
        fs.Close()

        Dim objWriter As New System.IO.StreamWriter(pathgeneratereport, True)
        objWriter.WriteLine(s)
        objWriter.Close()


    End Sub

    Private Sub ddl_OtherReportTable_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddl_OtherReportTable.SelectedIndexChanged
        txtAliasReportTable.Text = ""
    End Sub

    Private Sub btnAddCondition_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddCondition.Click
        Dim frm As New wnfrm_ReportDynamicWhereCondition
        frm.ShowDialog()
        SetupReportGeneration()
    End Sub

    Private Sub btnDateParameterGroupe_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDateParameterGroupe.Click
        Dim frm As New wnfrm_ReportDynamicParameterDate
        frm.ShowDialog()
        SetupReportGeneration()
    End Sub

#End Region


    Private Sub FillComboPrefixStoredProcedure()

        Dim objs As List(Of Cls_PrefixStoredProcedure) = Cls_PrefixStoredProcedure.SearchAll

        With ddl_PrefixStoredProcedure
            .ValueMember = "ID"
            .DisplayMember = "PrefixStoredProcedure"
            .DataSource = objs
        End With

    End Sub




    Private Sub ddl_PrefixStoredProcedure_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_PrefixStoredProcedure.Click
        'If ispossible Then
        '    SetupCurrentPrefix()
        'End If
    End Sub

    Private Sub SetupCurrentPrefix()

        Dim obj As New Cls_PrefixStoredProcedure(ddl_PrefixStoredProcedure.SelectedValue)
        SqlServerHelper.CurrentPrefixStored = obj.PrefixStoredProcedure
    End Sub

    Private Sub BackgroundWorkerTrue1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue1.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue1, 1)
            '  SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue1, 1)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue2_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue2.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue2, 2)
            '  SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue2, 2)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue3_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue3.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue3, 3)
            ' SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue3, 3)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue4_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue4.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue4, 4)
            '  SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue4, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorker1)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue5_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue5.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            ' SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue4, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue5)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue5)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue6_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue6.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            '  SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue6, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue6)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue6)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue7_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue7.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            '  SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue7, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue7)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue7)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue8_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue8.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            '  SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue8, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue8)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue8)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue9_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue9.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            '  SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue9, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue9)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue9)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue10_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorkerTrue10.DoWork
        If TypeDatabaseChecked = TypeDatabase.SQLSERVER Then
            '  SqlServerHelper.InitializeLocalColumnAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue5, 5)
            SqlServerHelper.InitializeLocalColumnSuperAnyThread(_systeme.currentDatabase.ID, BackgroundWorkerTrue10, 4)
        ElseIf TypeDatabaseChecked = TypeDatabase.MYSQL Then
            MySqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue5)
        ElseIf TypeDatabaseChecked = TypeDatabase.POSTGRESSQL Then
            PostgresSqlManager.InitializeLocalColumn(_systeme.currentDatabase.ID, BackgroundWorkerTrue5)

        End If
    End Sub

    Private Sub BackgroundWorkerTrue1_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue1.ProgressChanged
        progressTrue1.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundWorkerTrue2_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue2.ProgressChanged
        progressTrue2.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundWorkerTrue3_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue3.ProgressChanged
        progressTrue3.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundWorkerTrue4_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue4.ProgressChanged
        progressTrue4.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundWorkerTrue5_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue5.ProgressChanged
        progressTrue5.Value = e.ProgressPercentage
    End Sub

    'Private Sub BackgroundWorkerTrue6_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue6.ProgressChanged
    '    progressTrue6.Value = e.ProgressPercentage
    'End Sub

    'Private Sub BackgroundWorkerTrue7_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue7.ProgressChanged
    '    progressTrue7.Value = e.ProgressPercentage
    'End Sub

    'Private Sub BackgroundWorkerTrue8_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue8.ProgressChanged
    '    progressTrue8.Value = e.ProgressPercentage
    'End Sub

    'Private Sub BackgroundWorkerTrue9_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue9.ProgressChanged
    '    progressTrue9.Value = e.ProgressPercentage
    'End Sub

    'Private Sub BackgroundWorkerTrue10_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorkerTrue10.ProgressChanged
    '    progressTrue10.Value = e.ProgressPercentage
    'End Sub

    Private Sub RB_Template_AdminLTE_Master_CheckedChanged(sender As Object, e As EventArgs) Handles RB_Template_AdminLTE_Master.CheckedChanged, RB_Template_Inspinia.CheckedChanged, RB_Template_CleanZone.CheckedChanged
        Try
            If RB_Template_AdminLTE_Master.Checked Then
                Me.PictureBox_Template.Image = Global.GENERIC_14.My.Resources.Resources.AdminLTE_Master_fw

            ElseIf RB_Template_Inspinia.Checked Then
                Me.PictureBox_Template.Image = Global.GENERIC_14.My.Resources.Resources.Inspinia_Template_fw

            ElseIf RB_Template_CleanZone.Checked Then
                Me.PictureBox_Template.Image = Global.GENERIC_14.My.Resources.Resources.CleanZone_Template_fw

            End If
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Erreur", MessageBoxButtons.OK)
            Error_Log(ex)
        End Try
    End Sub

    Private Sub RB_Formulaire_Tableau_CheckedChanged(sender As Object, e As EventArgs) Handles RB_Formulaire_Tableau.CheckedChanged, RB_Formulaire_FlowLayout.CheckedChanged
        Try
            If RB_Formulaire_Tableau.Checked Then
                Me.PictureBox_Formulaire.Image = Global.GENERIC_14.My.Resources.Resources.CleanZone_Form_fw

            ElseIf RB_Formulaire_FlowLayout.Checked Then
                Me.PictureBox_Formulaire.Image = Global.GENERIC_14.My.Resources.Resources.formFlowLayout_fw

            End If
        Catch ex As Exception
            MessageBox.Show("ERREUR:" & ex.Message, "Erreur", MessageBoxButtons.OK)
            Error_Log(ex)
        End Try
    End Sub
End Class


