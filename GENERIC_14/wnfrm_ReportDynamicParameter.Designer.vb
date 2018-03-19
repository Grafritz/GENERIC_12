<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class wnfrm_ReportDynamicParameter
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
        Me.ddl_RelatedColumn = New System.Windows.Forms.ComboBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ddl_RelatedTable = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtParameterName = New System.Windows.Forms.TextBox()
        Me.chkIsinMaster = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(22, 126)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(128, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Related column :"
        '
        'ddl_RelatedColumn
        '
        Me.ddl_RelatedColumn.FormattingEnabled = True
        Me.ddl_RelatedColumn.Location = New System.Drawing.Point(155, 125)
        Me.ddl_RelatedColumn.Name = "ddl_RelatedColumn"
        Me.ddl_RelatedColumn.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedColumn.TabIndex = 3
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(113, 185)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(90, 23)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Sauvegarder"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(231, 185)
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
        Me.Label4.Location = New System.Drawing.Point(36, 95)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(112, 20)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Related table :"
        '
        'ddl_RelatedTable
        '
        Me.ddl_RelatedTable.FormattingEnabled = True
        Me.ddl_RelatedTable.Location = New System.Drawing.Point(155, 94)
        Me.ddl_RelatedTable.Name = "ddl_RelatedTable"
        Me.ddl_RelatedTable.Size = New System.Drawing.Size(183, 21)
        Me.ddl_RelatedTable.TabIndex = 9
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(11, 66)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(137, 20)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Parameter Name :"
        '
        'txtParameterName
        '
        Me.txtParameterName.Location = New System.Drawing.Point(155, 66)
        Me.txtParameterName.Name = "txtParameterName"
        Me.txtParameterName.Size = New System.Drawing.Size(196, 20)
        Me.txtParameterName.TabIndex = 11
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
        'wnfrm_ReportDynamicParameter
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(383, 229)
        Me.Controls.Add(Me.chkIsinMaster)
        Me.Controls.Add(Me.txtParameterName)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ddl_RelatedTable)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.ddl_RelatedColumn)
        Me.Controls.Add(Me.Label1)
        Me.Name = "wnfrm_ReportDynamicParameter"
        Me.Text = "wnfrm_ReportDynamicParameter"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedColumn As System.Windows.Forms.ComboBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ddl_RelatedTable As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtParameterName As System.Windows.Forms.TextBox
    Friend WithEvents chkIsinMaster As System.Windows.Forms.CheckBox
End Class
