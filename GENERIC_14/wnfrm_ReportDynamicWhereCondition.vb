Imports System.Collections

Public Class wnfrm_ReportDynamicWhereCondition


    Private ispossible As Boolean = False

    Private Sub wnfrm_GroupTable_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Setup()
    End Sub

    Private Sub Setup()
        FillComboRelatedTable1()
        FillComboRelatedTable2()
        FillComboRelatedColonne1()
        FillComboRelatedColonne2()
        ispossible = True
    End Sub

    Private Sub FillComboRelatedTable1()

        Dim objs As List(Of Cls_ReportDynamicTable) = Cls_ReportDynamicTable.SearchAllbyId_ReportDynamic(GlobalVariables.Id_ReportDynamic)

        With ddl_RelatedTable1
            .ValueMember = "ID"
            .DisplayMember = "CompleteName"
            .DataSource = objs
        End With

    End Sub

    Private Sub FillComboRelatedColonne1()
        'ddl_RelatedColumn.Items.Clea
        Dim obj As New Cls_ReportDynamicTable(CLng(ddl_RelatedTable1.SelectedValue))
        Dim objs As List(Of Cls_Column) = Cls_Column.SearchAllBy_Table(CLng(obj.Id_Table))

        With rcmbColonne1
            .ValueMember = "ID"
            .DisplayMember = "Name"
            .DataSource = objs
        End With

    End Sub

    Private Sub FillComboRelatedTable2()

        Dim objs As List(Of Cls_ReportDynamicTable) = Cls_ReportDynamicTable.SearchAllbyId_ReportDynamic(GlobalVariables.Id_ReportDynamic)

        With ddl_RelatedTable2
            .ValueMember = "ID"
            .DisplayMember = "CompleteName"
            .DataSource = objs
        End With

    End Sub

    Private Sub FillComboRelatedColonne2()
        'ddl_RelatedColumn.Items.Clea
        Dim obj As New Cls_ReportDynamicTable(CLng(ddl_RelatedTable2.SelectedValue))
        Dim objs As List(Of Cls_Column) = Cls_Column.SearchAllBy_Table(CLng(obj.Id_Table))

        With rcmbColonne2
            .ValueMember = "ID"
            .DisplayMember = "Name"
            .DataSource = objs
        End With

    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try

            Dim obj As New Cls_ReportDynamicWhereCondition
            With obj
                .Id_ReportDynamic = GlobalVariables.Id_ReportDynamic
                .Id_RelatedTable1 = CLng(ddl_RelatedTable1.SelectedValue)
                .Id_RelatedColumn1 = CLng(rcmbColonne1.SelectedValue)
                .Id_RelatedTable2 = CLng(ddl_RelatedTable2.SelectedValue)
                .Id_RelatedColumn2 = CLng(rcmbColonne2.SelectedValue)
                .Insert()
            End With
            Me.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub ddl_RelatedTable1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_RelatedTable1.SelectedIndexChanged
        If ispossible Then
            FillComboRelatedColonne1()
        End If
    End Sub

    
    Private Sub ddl_RelatedTable2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_RelatedTable2.SelectedIndexChanged
        If ispossible Then
            FillComboRelatedColonne2()
        End If
    End Sub
End Class