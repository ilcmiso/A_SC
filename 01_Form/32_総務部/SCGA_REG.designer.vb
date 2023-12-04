<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCGA_REG
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.PBXX = New System.Windows.Forms.Button()
        Me.L_REGTITLE = New System.Windows.Forms.Label()
        Me.DGV_REG1 = New System.Windows.Forms.DataGridView()
        Me.項目 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.BT_A1 = New System.Windows.Forms.Button()
        Me.BT_A2 = New System.Windows.Forms.Button()
        CType(Me.DGV_REG1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PBXX
        '
        Me.PBXX.BackColor = System.Drawing.Color.Silver
        Me.PBXX.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.PBXX.ForeColor = System.Drawing.Color.Black
        Me.PBXX.Location = New System.Drawing.Point(-100, -100)
        Me.PBXX.Name = "PBXX"
        Me.PBXX.Size = New System.Drawing.Size(75, 23)
        Me.PBXX.TabIndex = 19
        Me.PBXX.Text = "PBXX"
        Me.PBXX.UseVisualStyleBackColor = False
        '
        'L_REGTITLE
        '
        Me.L_REGTITLE.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.L_REGTITLE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.L_REGTITLE.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.L_REGTITLE.ForeColor = System.Drawing.Color.White
        Me.L_REGTITLE.Location = New System.Drawing.Point(1, -1)
        Me.L_REGTITLE.Name = "L_REGTITLE"
        Me.L_REGTITLE.Size = New System.Drawing.Size(298, 24)
        Me.L_REGTITLE.TabIndex = 1814
        Me.L_REGTITLE.Text = "団信弁済の登録内容"
        Me.L_REGTITLE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'DGV_REG1
        '
        Me.DGV_REG1.AllowUserToAddRows = False
        Me.DGV_REG1.AllowUserToDeleteRows = False
        Me.DGV_REG1.AllowUserToResizeColumns = False
        Me.DGV_REG1.AllowUserToResizeRows = False
        Me.DGV_REG1.BackgroundColor = System.Drawing.Color.White
        Me.DGV_REG1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGray
        DataGridViewCellStyle1.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DGV_REG1.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.DGV_REG1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.DGV_REG1.ColumnHeadersVisible = False
        Me.DGV_REG1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.項目, Me.Column1})
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle3.Font = New System.Drawing.Font("MS UI Gothic", 9.0!)
        DataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DGV_REG1.DefaultCellStyle = DataGridViewCellStyle3
        Me.DGV_REG1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        Me.DGV_REG1.Location = New System.Drawing.Point(1, 22)
        Me.DGV_REG1.MultiSelect = False
        Me.DGV_REG1.Name = "DGV_REG1"
        Me.DGV_REG1.RowHeadersVisible = False
        Me.DGV_REG1.RowTemplate.Height = 21
        Me.DGV_REG1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.DGV_REG1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.DGV_REG1.Size = New System.Drawing.Size(298, 434)
        Me.DGV_REG1.TabIndex = 1813
        Me.DGV_REG1.TabStop = False
        '
        '項目
        '
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.項目.DefaultCellStyle = DataGridViewCellStyle2
        Me.項目.HeaderText = "Column2"
        Me.項目.Name = "項目"
        Me.項目.ReadOnly = True
        Me.項目.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.項目.Width = 145
        '
        'Column1
        '
        Me.Column1.HeaderText = "Column1"
        Me.Column1.Name = "Column1"
        Me.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Column1.Width = 150
        '
        'BT_A1
        '
        Me.BT_A1.BackColor = System.Drawing.Color.SteelBlue
        Me.BT_A1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A1.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A1.ForeColor = System.Drawing.Color.White
        Me.BT_A1.Location = New System.Drawing.Point(126, 462)
        Me.BT_A1.Name = "BT_A1"
        Me.BT_A1.Size = New System.Drawing.Size(86, 31)
        Me.BT_A1.TabIndex = 1811
        Me.BT_A1.Text = "登　　録"
        Me.BT_A1.UseVisualStyleBackColor = False
        '
        'BT_A2
        '
        Me.BT_A2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.BT_A2.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A2.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A2.ForeColor = System.Drawing.Color.White
        Me.BT_A2.Location = New System.Drawing.Point(213, 462)
        Me.BT_A2.Name = "BT_A2"
        Me.BT_A2.Size = New System.Drawing.Size(86, 31)
        Me.BT_A2.TabIndex = 1812
        Me.BT_A2.Text = "キャンセル"
        Me.BT_A2.UseVisualStyleBackColor = False
        '
        'SCGA_REG
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLight
        Me.ClientSize = New System.Drawing.Size(302, 495)
        Me.Controls.Add(Me.L_REGTITLE)
        Me.Controls.Add(Me.DGV_REG1)
        Me.Controls.Add(Me.BT_A1)
        Me.Controls.Add(Me.BT_A2)
        Me.Controls.Add(Me.PBXX)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "SCGA_REG"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "申請物の登録"
        CType(Me.DGV_REG1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents L_REGTITLE As Label
    Friend WithEvents DGV_REG1 As DataGridView
    Friend WithEvents BT_A1 As Button
    Friend WithEvents BT_A2 As Button
    Friend WithEvents 項目 As DataGridViewTextBoxColumn
    Friend WithEvents Column1 As DataGridViewTextBoxColumn
End Class
