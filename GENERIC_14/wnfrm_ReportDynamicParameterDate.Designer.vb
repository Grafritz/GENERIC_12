<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class wnfrm_ReportDynamicParameterDate
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ddl_RelatedColumnDebut = New System.Windows.Forms.ComboBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ddl_RelatedTable = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtParameterDebutName = New System.Windows.Forms.TextBox()
        Me.chkIsinMaster = New System.Windows.Forms.CheckBox()
        Me.txtParameterFinName = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(20, 159)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(128, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Related column :"
        '
        'ddl_RelatedColumnDebut
        '
        Me.ddl_RelatedColumnDebut.FormattingEnabled = True
        Me.ddl_RelatedColumnDebut.Location = New System.Drawing.Point(155, 158)
        Me.ddl_RelatedColumnDebut.Name = "ddl_RelatedColumnDebut"
        Me.ddl_RelatedColumnDebut.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedColumnDebut.TabIndex = 3
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(58, 194)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(90, 23)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Sauvegarder"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(199, 194)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(87, 23)
        Me.btnCancel.TabIndex = 7
        Me.btnCancel.Text = "Annuler"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(36, 66)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(112, 20)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Related table :"
        '
        'ddl_RelatedTable
        '
        Me.ddl_RelatedTable.FormattingEnabled = True
        Me.ddl_RelatedTable.Location = New System.Drawing.Point(155, 65)
        Me.ddl_RelatedTable.Name = "ddl_RelatedTable"
        Me.ddl_RelatedTable.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedTable.TabIndex = 9
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 93)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(136, 20)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Parameter debut :"
        '
        'txtParameterDebutName
        '
        Me.txtParameterDebutName.Location = New System.Drawing.Point(155, 95)
        Me.txtParameterDebutName.Name = "txtParameterDebutName"
        Me.txtParameterDebutName.Size = New System.Drawing.Size(183, 20)
        Me.txtParameterDebutName.TabIndex = 11
        '
        'chkIsinMaster
        '
        Me.chkIsinMaster.AutoSize = True
        Me.chkIsinMaster.Location = New System.Drawing.Point(155, 37)
        Me.chkIsinMaster.Name = "chkIsinMaster"
        Me.chkIsinMaster.Size = New System.Drawing.Size(74, 17)
        Me.chkIsinMaster.TabIndex = 14
        Me.chkIsinMaster.Text = "IsinMaster"
        Me.chkIsinMaster.UseVisualStyleBackColor = True
        '
        'txtParameterFinName
        '
        Me.txtParameterFinName.Location = New System.Drawing.Point(155, 127)
        Me.txtParameterFinName.Name = "txtParameterFinName"
        Me.txtParameterFinName.Size = New System.Drawing.Size(183, 20)
        Me.txtParameterFinName.TabIndex = 16
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(35, 125)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(112, 20)
        Me.Label3.TabIndex = 15
        Me.Label3.Text = "Parameter fin :"
        '
        'wnfrm_ReportDynamicParameterDate
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(353, 250)
        Me.Controls.Add(Me.txtParameterFinName)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.chkIsinMaster)
        Me.Controls.Add(Me.txtParameterDebutName)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ddl_RelatedTable)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.ddl_RelatedColumnDebut)
        Me.Controls.Add(Me.Label1)
        Me.Name = "wnfrm_ReportDynamicParameterDate"
        Me.Text = "wnfrm_ReportDynamicParameterDate"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedColumnDebut As System.Windows.Forms.ComboBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedTable As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtParameterDebutName As System.Windows.Forms.TextBox
    Friend WithEvents chkIsinMaster As System.Windows.Forms.CheckBox
    Friend WithEvents txtParameterFinName As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
End Class
