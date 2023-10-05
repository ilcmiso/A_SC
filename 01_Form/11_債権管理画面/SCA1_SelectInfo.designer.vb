<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCA1_SelectInfo
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
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.PBXX = New System.Windows.Forms.Button()
        Me.BT_RecE1 = New System.Windows.Forms.Button()
        Me.DGV_SI = New System.Windows.Forms.DataGridView()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.DataGridViewTextBoxColumn17 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column1 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        CType(Me.DGV_SI, System.ComponentModel.ISupportInitialize).BeginInit()
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
        'BT_RecE1
        '
        Me.BT_RecE1.Location = New System.Drawing.Point(261, 609)
        Me.BT_RecE1.Name = "BT_RecE1"
        Me.BT_RecE1.Size = New System.Drawing.Size(101, 23)
        Me.BT_RecE1.TabIndex = 1757
        Me.BT_RecE1.Text = "全選択/解除"
        Me.BT_RecE1.UseVisualStyleBackColor = True
        '
        'DGV_SI
        '
        Me.DGV_SI.AllowDrop = True
        Me.DGV_SI.AllowUserToAddRows = False
        Me.DGV_SI.AllowUserToDeleteRows = False
        Me.DGV_SI.AllowUserToResizeColumns = False
        Me.DGV_SI.AllowUserToResizeRows = False
        Me.DGV_SI.BackgroundColor = System.Drawing.Color.White
        Me.DGV_SI.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.LightGray
        DataGridViewCellStyle3.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DGV_SI.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle3
        Me.DGV_SI.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.DGV_SI.ColumnHeadersVisible = False
        Me.DGV_SI.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn17, Me.Column1})
        Me.DGV_SI.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        Me.DGV_SI.Location = New System.Drawing.Point(-1, 0)
        Me.DGV_SI.MultiSelect = False
        Me.DGV_SI.Name = "DGV_SI"
        Me.DGV_SI.RowHeadersVisible = False
        Me.DGV_SI.RowHeadersWidth = 51
        Me.DGV_SI.RowTemplate.Height = 21
        Me.DGV_SI.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.DGV_SI.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.DGV_SI.Size = New System.Drawing.Size(256, 698)
        Me.DGV_SI.TabIndex = 1769
        Me.DGV_SI.TabStop = False
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(261, 658)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(101, 40)
        Me.Button1.TabIndex = 1770
        Me.Button1.Text = "Excelに出力"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'DataGridViewTextBoxColumn17
        '
        DataGridViewCellStyle4.BackColor = System.Drawing.Color.White
        Me.DataGridViewTextBoxColumn17.DefaultCellStyle = DataGridViewCellStyle4
        Me.DataGridViewTextBoxColumn17.HeaderText = "項目"
        Me.DataGridViewTextBoxColumn17.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn17.Name = "DataGridViewTextBoxColumn17"
        Me.DataGridViewTextBoxColumn17.ReadOnly = True
        Me.DataGridViewTextBoxColumn17.Width = 200
        '
        'Column1
        '
        Me.Column1.HeaderText = "CheckBOX"
        Me.Column1.Name = "Column1"
        Me.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.Column1.Width = 30
        '
        'SCA1_SelectInfo
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(370, 700)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.DGV_SI)
        Me.Controls.Add(Me.BT_RecE1)
        Me.Controls.Add(Me.PBXX)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "SCA1_SelectInfo"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "対応記録の登録"
        CType(Me.DGV_SI, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents BT_RecE1 As Button
    Friend WithEvents DGV_SI As DataGridView
    Friend WithEvents Button1 As Button
    Friend WithEvents DataGridViewTextBoxColumn17 As DataGridViewTextBoxColumn
    Friend WithEvents Column1 As DataGridViewCheckBoxColumn
End Class
