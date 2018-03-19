Imports System.Collections

Public Class wnfrm_ReportDynamicParameter


    Private ispossible As Boolean = False

    Private Sub wnfrm_GroupTable_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Setup()
    End Sub

    Private Sub Setup()
        FillComboRelatedTable()
        FillComboRelatedColumn()
        txtParameterName.Text = ""
        chkIsinMaster.Checked = False
        ispossible = True
    End Sub

    Private Sub FillComboRelatedTable()

        Dim objs As List(Of Cls_ReportDynamicTable) = Cls_ReportDynamicTable.SearchAllbyId_ReportDynamic(GlobalVariables.Id_ReportDynamic)

        With ddl_RelatedTable
            .ValueMember = "ID"
            .DisplayMember = "CompleteName"
            .DataSource = objs
        End With

    End Sub

    Private Sub FillComboRelatedColumn()
        'ddl_RelatedColumn.Items.Clea
        Dim obj As New Cls_ReportDynamicTable(CLng(ddl_RelatedTable.SelectedValue))
        Dim objs As List(Of Cls_Column) = Cls_Column.SearchAllBy_Table(CLng(obj.Id_Table))

        With ddl_RelatedColumn
            .ValueMember = "ID"
            .DisplayMember = "Name"
            .DataSource = objs
        End With

    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            If txtParameterName.Text = "" Then
                Throw (New System.Exception("Le libéllé du paramètre n'est pas renseigné"))
            End If
            Dim obj As New Cls_ReportDynamicParameter
            With obj
                .ParameterName = txtParameterName.Text
                .Id_RelatedTable = CLng(ddl_RelatedTable.SelectedValue)
                .Id_RelatedColumn = CLng(ddl_RelatedColumn.SelectedValue)
                .IsInMaster = chkIsinMaster.Checked
                .Id_ReportDynamic = GlobalVariables.Id_ReportDynamic
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

    Private Sub ddl_RelatedTable_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_RelatedTable.SelectedIndexChanged
        If ispossible Then
            FillComboRelatedColumn()
        End If
    End Sub
End Class