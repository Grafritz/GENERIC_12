<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class wnfrm_GroupTable
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
        Me.lbl_Liaison = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ddl_Parent = New System.Windows.Forms.ComboBox()
        Me.ddl_Liaison = New System.Windows.Forms.ComboBox()
        Me.ddl_Enfant = New System.Windows.Forms.ComboBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ddl_Typegroupe = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(41, 66)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(68, 20)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Parent : "
        '
        'lbl_Liaison
        '
        Me.lbl_Liaison.AutoSize = True
        Me.lbl_Liaison.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_Liaison.Location = New System.Drawing.Point(41, 106)
        Me.lbl_Liaison.Name = "lbl_Liaison"
        Me.lbl_Liaison.Size = New System.Drawing.Size(67, 20)
        Me.lbl_Liaison.TabIndex = 1
        Me.lbl_Liaison.Text = "Liaison :"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(41, 143)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 20)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Enfant :"
        '
        'ddl_Parent
        '
        Me.ddl_Parent.FormattingEnabled = True
        Me.ddl_Parent.Location = New System.Drawing.Point(131, 66)
        Me.ddl_Parent.Name = "ddl_Parent"
        Me.ddl_Parent.Size = New System.Drawing.Size(183, 21)
        Me.ddl_Parent.TabIndex = 3
        '
        'ddl_Liaison
        '
        Me.ddl_Liaison.FormattingEnabled = True
        Me.ddl_Liaison.Location = New System.Drawing.Point(131, 98)
        Me.ddl_Liaison.Name = "ddl_Liaison"
        Me.ddl_Liaison.Size = New System.Drawing.Size(183, 21)
        Me.ddl_Liaison.TabIndex = 4
        '
        'ddl_Enfant
        '
        Me.ddl_Enfant.FormattingEnabled = True
        Me.ddl_Enfant.Location = New System.Drawing.Point(131, 135)
        Me.ddl_Enfant.Name = "ddl_Enfant"
        Me.ddl_Enfant.Size = New System.Drawing.Size(183, 21)
        Me.ddl_Enfant.TabIndex = 5
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(131, 176)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(90, 23)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Sauvegarder"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(227, 176)
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
        Me.Label4.Location = New System.Drawing.Point(1, 31)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(105, 20)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Type groupe :"
        '
        'ddl_Typegroupe
        '
        Me.ddl_Typegroupe.FormattingEnabled = True
        Me.ddl_Typegroupe.Location = New System.Drawing.Point(131, 33)
        Me.ddl_Typegroupe.Name = "ddl_Typegroupe"
        Me.ddl_Typegroupe.Size = New System.Drawing.Size(183, 21)
        Me.ddl_Typegroupe.TabIndex = 9
        '
        'wnfrm_GroupTable
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(383, 260)
        Me.Controls.Add(Me.ddl_Typegroupe)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.ddl_Enfant)
        Me.Controls.Add(Me.ddl_Liaison)
        Me.Controls.Add(Me.ddl_Parent)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lbl_Liaison)
        Me.Controls.Add(Me.Label1)
        Me.Name = "wnfrm_GroupTable"
        Me.Text = "wnfrm_GroupTable"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lbl_Liaison As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ddl_Parent As System.Windows.Forms.ComboBox
    Friend WithEvents ddl_Liaison As System.Windows.Forms.ComboBox
    Friend WithEvents ddl_Enfant As System.Windows.Forms.ComboBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ddl_Typegroupe As System.Windows.Forms.ComboBox
End Class
