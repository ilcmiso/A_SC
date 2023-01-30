<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCD1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SCD1))
        Me.bgWorker = New System.ComponentModel.BackgroundWorker()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.TB_A1 = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.NUD_C1 = New System.Windows.Forms.NumericUpDown()
        Me.DTP_B1 = New System.Windows.Forms.DateTimePicker()
        Me.DTP_B2 = New System.Windows.Forms.DateTimePicker()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.TB_H1 = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.BT_A1 = New System.Windows.Forms.Button()
        Me.TB_D1 = New System.Windows.Forms.TextBox()
        Me.TB_D2 = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.NUD_D1 = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.LB_J1 = New System.Windows.Forms.ListBox()
        Me.Label8 = New System.Windows.Forms.Label()
        CType(Me.NUD_C1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NUD_D1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'bgWorker
        '
        Me.bgWorker.WorkerReportsProgress = True
        Me.bgWorker.WorkerSupportsCancellation = True
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label1.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(98, 25)
        Me.Label1.TabIndex = 24
        Me.Label1.Text = "顧客指名"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label2.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 43)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(98, 25)
        Me.Label2.TabIndex = 25
        Me.Label2.Text = "日　付"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.Khaki
        Me.Label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label3.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label3.Location = New System.Drawing.Point(12, 157)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(98, 25)
        Me.Label3.TabIndex = 26
        Me.Label3.Text = "時　間"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label4
        '
        Me.Label4.BackColor = System.Drawing.Color.Khaki
        Me.Label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label4.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label4.Location = New System.Drawing.Point(12, 192)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(98, 25)
        Me.Label4.TabIndex = 27
        Me.Label4.Text = "連絡先"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label7.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label7.Location = New System.Drawing.Point(12, 228)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(98, 25)
        Me.Label7.TabIndex = 30
        Me.Label7.Text = "担当者"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label9
        '
        Me.Label9.BackColor = System.Drawing.Color.Khaki
        Me.Label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label9.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label9.Location = New System.Drawing.Point(12, 263)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(98, 25)
        Me.Label9.TabIndex = 32
        Me.Label9.Text = "交渉経緯等"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TB_A1
        '
        Me.TB_A1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_A1.Location = New System.Drawing.Point(119, 9)
        Me.TB_A1.Name = "TB_A1"
        Me.TB_A1.Size = New System.Drawing.Size(243, 24)
        Me.TB_A1.TabIndex = 33
        '
        'Label10
        '
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label10.Location = New System.Drawing.Point(374, 10)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(32, 25)
        Me.Label10.TabIndex = 34
        Me.Label10.Text = "様"
        '
        'Label11
        '
        Me.Label11.BackColor = System.Drawing.Color.Transparent
        Me.Label11.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label11.Location = New System.Drawing.Point(231, 46)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(32, 25)
        Me.Label11.TabIndex = 36
        Me.Label11.Text = "～"
        '
        'Label12
        '
        Me.Label12.BackColor = System.Drawing.Color.Transparent
        Me.Label12.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label12.Location = New System.Drawing.Point(148, 81)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(61, 25)
        Me.Label12.TabIndex = 38
        Me.Label12.Text = "そのうち"
        '
        'Label13
        '
        Me.Label13.BackColor = System.Drawing.Color.Transparent
        Me.Label13.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label13.Location = New System.Drawing.Point(252, 81)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(142, 25)
        Me.Label13.TabIndex = 39
        Me.Label13.Text = "日分の記録を作成"
        '
        'NUD_C1
        '
        Me.NUD_C1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.NUD_C1.Location = New System.Drawing.Point(208, 79)
        Me.NUD_C1.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.NUD_C1.Name = "NUD_C1"
        Me.NUD_C1.ReadOnly = True
        Me.NUD_C1.Size = New System.Drawing.Size(43, 24)
        Me.NUD_C1.TabIndex = 40
        Me.NUD_C1.Value = New Decimal(New Integer() {20, 0, 0, 0})
        '
        'DTP_B1
        '
        Me.DTP_B1.CalendarFont = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.DTP_B1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.DTP_B1.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.DTP_B1.Location = New System.Drawing.Point(119, 43)
        Me.DTP_B1.Name = "DTP_B1"
        Me.DTP_B1.Size = New System.Drawing.Size(110, 24)
        Me.DTP_B1.TabIndex = 44
        '
        'DTP_B2
        '
        Me.DTP_B2.CalendarFont = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.DTP_B2.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.DTP_B2.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.DTP_B2.Location = New System.Drawing.Point(252, 44)
        Me.DTP_B2.Name = "DTP_B2"
        Me.DTP_B2.Size = New System.Drawing.Size(110, 24)
        Me.DTP_B2.TabIndex = 45
        '
        'Label14
        '
        Me.Label14.BackColor = System.Drawing.Color.Transparent
        Me.Label14.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label14.Location = New System.Drawing.Point(231, 158)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(32, 25)
        Me.Label14.TabIndex = 46
        Me.Label14.Text = "～"
        '
        'Label15
        '
        Me.Label15.BackColor = System.Drawing.Color.Transparent
        Me.Label15.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label15.Location = New System.Drawing.Point(121, 192)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(285, 25)
        Me.Label15.TabIndex = 49
        Me.Label15.Text = "「携　帯」「自　宅」「会　社」からランダム"
        '
        'TB_H1
        '
        Me.TB_H1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_H1.Location = New System.Drawing.Point(119, 228)
        Me.TB_H1.Name = "TB_H1"
        Me.TB_H1.Size = New System.Drawing.Size(110, 24)
        Me.TB_H1.TabIndex = 50
        Me.TB_H1.Text = "伊賀"
        '
        'Label16
        '
        Me.Label16.BackColor = System.Drawing.Color.Transparent
        Me.Label16.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label16.Location = New System.Drawing.Point(121, 263)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(305, 25)
        Me.Label16.TabIndex = 51
        Me.Label16.Text = "「電源OFF」「居留守」「出ず」からランダム"
        '
        'BT_A1
        '
        Me.BT_A1.BackColor = System.Drawing.Color.SteelBlue
        Me.BT_A1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A1.Font = New System.Drawing.Font("MS UI Gothic", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A1.ForeColor = System.Drawing.Color.White
        Me.BT_A1.Location = New System.Drawing.Point(284, 341)
        Me.BT_A1.Name = "BT_A1"
        Me.BT_A1.Size = New System.Drawing.Size(112, 31)
        Me.BT_A1.TabIndex = 52
        Me.BT_A1.Text = "作　　成"
        Me.BT_A1.UseVisualStyleBackColor = False
        '
        'TB_D1
        '
        Me.TB_D1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_D1.Location = New System.Drawing.Point(169, 157)
        Me.TB_D1.Name = "TB_D1"
        Me.TB_D1.ReadOnly = True
        Me.TB_D1.Size = New System.Drawing.Size(60, 24)
        Me.TB_D1.TabIndex = 53
        Me.TB_D1.Text = "08:30"
        Me.TB_D1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'TB_D2
        '
        Me.TB_D2.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_D2.Location = New System.Drawing.Point(302, 157)
        Me.TB_D2.Name = "TB_D2"
        Me.TB_D2.ReadOnly = True
        Me.TB_D2.Size = New System.Drawing.Size(60, 24)
        Me.TB_D2.TabIndex = 54
        Me.TB_D2.Text = "20:40"
        Me.TB_D2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label5
        '
        Me.Label5.BackColor = System.Drawing.Color.Khaki
        Me.Label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label5.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label5.Location = New System.Drawing.Point(12, 120)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(98, 25)
        Me.Label5.TabIndex = 55
        Me.Label5.Text = "督促状送付"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'NUD_D1
        '
        Me.NUD_D1.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.NUD_D1.Increment = New Decimal(New Integer() {0, 0, 0, 0})
        Me.NUD_D1.Location = New System.Drawing.Point(119, 120)
        Me.NUD_D1.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
        Me.NUD_D1.Name = "NUD_D1"
        Me.NUD_D1.ReadOnly = True
        Me.NUD_D1.Size = New System.Drawing.Size(43, 24)
        Me.NUD_D1.TabIndex = 57
        Me.NUD_D1.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label6.Location = New System.Drawing.Point(165, 120)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(64, 25)
        Me.Label6.TabIndex = 56
        Me.Label6.Text = "回送付"
        '
        'LB_J1
        '
        Me.LB_J1.FormattingEnabled = True
        Me.LB_J1.ItemHeight = 12
        Me.LB_J1.Items.AddRange(New Object() {"1. 自宅、携帯あり顧客", "2. 自宅のみ顧客", "3. 携帯のみ顧客"})
        Me.LB_J1.Location = New System.Drawing.Point(119, 296)
        Me.LB_J1.Name = "LB_J1"
        Me.LB_J1.Size = New System.Drawing.Size(132, 40)
        Me.LB_J1.TabIndex = 58
        '
        'Label8
        '
        Me.Label8.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label8.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label8.Location = New System.Drawing.Point(12, 296)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(98, 25)
        Me.Label8.TabIndex = 59
        Me.Label8.Text = "パターン"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'SCD1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(408, 376)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.LB_J1)
        Me.Controls.Add(Me.NUD_D1)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.TB_D2)
        Me.Controls.Add(Me.TB_D1)
        Me.Controls.Add(Me.BT_A1)
        Me.Controls.Add(Me.Label16)
        Me.Controls.Add(Me.TB_H1)
        Me.Controls.Add(Me.Label15)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.DTP_B2)
        Me.Controls.Add(Me.DTP_B1)
        Me.Controls.Add(Me.NUD_C1)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.TB_A1)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "SCD1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "交渉記録表の作成"
        CType(Me.NUD_C1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NUD_D1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents bgWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents TB_A1 As TextBox
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents NUD_C1 As NumericUpDown
    Friend WithEvents DTP_B1 As DateTimePicker
    Friend WithEvents DTP_B2 As DateTimePicker
    Friend WithEvents Label14 As Label
    Friend WithEvents Label15 As Label
    Friend WithEvents TB_H1 As TextBox
    Friend WithEvents Label16 As Label
    Friend WithEvents BT_A1 As Button
    Friend WithEvents TB_D1 As TextBox
    Friend WithEvents TB_D2 As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents NUD_D1 As NumericUpDown
    Friend WithEvents Label6 As Label
    Friend WithEvents LB_J1 As ListBox
    Friend WithEvents Label8 As Label
End Class
