<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCA1_S5_ExcOut
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
        Me.PBXX = New System.Windows.Forms.Button()
        Me.LB_SELITEM = New System.Windows.Forms.ListBox()
        Me.Label45 = New System.Windows.Forms.Label()
        Me.Button9 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
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
        'LB_SELITEM
        '
        Me.LB_SELITEM.BackColor = System.Drawing.Color.White
        Me.LB_SELITEM.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.LB_SELITEM.FormattingEnabled = True
        Me.LB_SELITEM.ItemHeight = 12
        Me.LB_SELITEM.Items.AddRange(New Object() {"基本物件情報", "任売", "競売①", "競売②", "再生・破産①", "再生・破産②", "再生・破産③", "差押①", "差押②", "差押③"})
        Me.LB_SELITEM.Location = New System.Drawing.Point(0, 24)
        Me.LB_SELITEM.Name = "LB_SELITEM"
        Me.LB_SELITEM.Size = New System.Drawing.Size(109, 124)
        Me.LB_SELITEM.TabIndex = 1607
        '
        'Label45
        '
        Me.Label45.BackColor = System.Drawing.Color.SteelBlue
        Me.Label45.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label45.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label45.ForeColor = System.Drawing.Color.White
        Me.Label45.Location = New System.Drawing.Point(0, 0)
        Me.Label45.Name = "Label45"
        Me.Label45.Size = New System.Drawing.Size(275, 24)
        Me.Label45.TabIndex = 1764
        Me.Label45.Text = "出力する項目を選択（複数選択可能）"
        Me.Label45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Button9
        '
        Me.Button9.BackColor = System.Drawing.Color.MediumAquamarine
        Me.Button9.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.Button9.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button9.ForeColor = System.Drawing.Color.White
        Me.Button9.Location = New System.Drawing.Point(107, 24)
        Me.Button9.Name = "Button9"
        Me.Button9.Size = New System.Drawing.Size(168, 122)
        Me.Button9.TabIndex = 1765
        Me.Button9.Text = "Excel出力 確定"
        Me.Button9.UseVisualStyleBackColor = False
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.Silver
        Me.Button1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.Button1.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button1.ForeColor = System.Drawing.Color.White
        Me.Button1.Location = New System.Drawing.Point(107, 145)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(168, 51)
        Me.Button1.TabIndex = 1766
        Me.Button1.Text = "キャンセル"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(0, 154)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(93, 16)
        Me.CheckBox1.TabIndex = 1767
        Me.CheckBox1.Text = "全項目を出力"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'SCA1_S5_ExcOut
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(275, 196)
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Button9)
        Me.Controls.Add(Me.Label45)
        Me.Controls.Add(Me.LB_SELITEM)
        Me.Controls.Add(Me.PBXX)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "SCA1_S5_ExcOut"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "対応記録の登録"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents LB_SELITEM As ListBox
    Friend WithEvents Label45 As Label
    Friend WithEvents Button9 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents CheckBox1 As CheckBox
End Class
