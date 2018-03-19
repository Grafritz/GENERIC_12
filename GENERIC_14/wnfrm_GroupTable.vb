Imports System.Collections

Public Class wnfrm_GroupTable




    Private Sub wnfrm_GroupTable_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load


        FillComboParent()
        FillComboEnfant()
        FillComboLiaison()
        FillComboTypeGroupe()
    End Sub

    Private Sub FillComboTypeGroupe()

        Dim objs As List(Of Cls_TypeGroupe) = Cls_TypeGroupe.SearchAll

        With ddl_Typegroupe
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "TypeGroupe"

        End With

    End Sub

    Private Sub FillComboParent()

        Dim objs As List(Of Cls_Table) = Cls_Table.SearchAllBy_Database(Cls_Database.GetLastDatabase.ID)

        With ddl_Parent
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With

    End Sub

    Private Sub FillComboEnfant()

        Dim objs As List(Of Cls_Table) = Cls_Table.SearchAllBy_Database(Cls_Database.GetLastDatabase.ID)

        With ddl_Enfant
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With
    End Sub

    Private Sub FillComboLiaison()

        Dim objs As List(Of Cls_Table) = Cls_Table.SearchAllBy_Database(Cls_Database.GetLastDatabase.ID)

        With ddl_Liaison
            .DataSource = objs
            .ValueMember = "ID"
            .DisplayMember = "Name"

        End With
    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim objs As List(Of Cls_GroupTable)
        objs = Cls_GroupTable.SearchAll

        Dim obj As New Cls_GroupTable
        With obj
            .Name = "Groupe" & (objs.Count + 1).ToString
            .Id_ChildTable = ddl_Enfant.SelectedValue
            .Id_Parenttable = ddl_Parent.SelectedValue
            If ddl_Liaison.Visible = True Then
                .Id_LiaisonTable = ddl_Liaison.SelectedValue
            Else
                .Id_LiaisonTable = 0
            End If

            .Insert()
        End With

        Me.Close()

    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

  
    Private Sub ddl_Typegroupe_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddl_Typegroupe.SelectedIndexChanged
        If ddl_Typegroupe.SelectedIndex = 0 Then
            lbl_Liaison.Visible = False
            ddl_Liaison.Visible = False
        Else
            lbl_Liaison.Visible = True
            ddl_Liaison.Visible = True

        End If
    End Sub
End Class