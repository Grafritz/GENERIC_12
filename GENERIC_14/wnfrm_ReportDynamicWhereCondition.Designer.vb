<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class wnfrm_ReportDynamicWhereCondition
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
        Me.ddl_RelatedTable2 = New System.Windows.Forms.ComboBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ddl_RelatedTable1 = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.rcmbColonne1 = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.rcmbColonne2 = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(311, 53)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(61, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Table 2"
        '
        'ddl_RelatedTable2
        '
        Me.ddl_RelatedTable2.FormattingEnabled = True
        Me.ddl_RelatedTable2.Location = New System.Drawing.Point(406, 53)
        Me.ddl_RelatedTable2.Name = "ddl_RelatedTable2"
        Me.ddl_RelatedTable2.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedTable2.TabIndex = 3
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(195, 119)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(90, 23)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Sauvegarder"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(327, 119)
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
        Me.Label4.Location = New System.Drawing.Point(22, 54)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(61, 20)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Table 1"
        '
        'ddl_RelatedTable1
        '
        Me.ddl_RelatedTable1.FormattingEnabled = True
        Me.ddl_RelatedTable1.Location = New System.Drawing.Point(89, 53)
        Me.ddl_RelatedTable1.Name = "ddl_RelatedTable1"
        Me.ddl_RelatedTable1.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedTable1.TabIndex = 9
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(5, 91)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(81, 20)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Colonne 1"
        '
        'rcmbColonne1
        '
        Me.rcmbColonne1.FormattingEnabled = True
        Me.rcmbColonne1.Location = New System.Drawing.Point(92, 91)
        Me.rcmbColonne1.Name = "rcmbColonne1"
        Me.rcmbColonne1.Size = New System.Drawing.Size(183, 21)
        Me.rcmbColonne1.TabIndex = 11
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(291, 91)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(81, 20)
        Me.Label3.TabIndex = 12
        Me.Label3.Text = "Colonne 2"
        '
        'rcmbColonne2
        '
        Me.rcmbColonne2.FormattingEnabled = True
        Me.rcmbColonne2.Location = New System.Drawing.Point(406, 90)
        Me.rcmbColonne2.Name = "rcmbColonne2"
        Me.rcmbColonne2.Size = New System.Drawing.Size(183, 21)
        Me.rcmbColonne2.TabIndex = 13
        '
        'wnfrm_ReportDynamicWhereCondition
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(612, 154)
        Me.Controls.Add(Me.rcmbColonne2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.rcmbColonne1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ddl_RelatedTable1)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.ddl_RelatedTable2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "wnfrm_ReportDynamicWhereCondition"
        Me.Text = "wnfrm_ReportDynamicWhereCondition"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedTable2 As System.Windows.Forms.ComboBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedTable1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents rcmbColonne1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents rcmbColonne2 As System.Windows.Forms.ComboBox
End Class
