<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCB1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SCB1))
        Me.bgWorker = New System.ComponentModel.BackgroundWorker()
        Me.PB_TXT = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.PB_CSV = New System.Windows.Forms.PictureBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.L_UptimeTXT = New System.Windows.Forms.Label()
        Me.L_UptimeCSV = New System.Windows.Forms.Label()
        Me.DGV1 = New System.Windows.Forms.DataGridView()
        Me.CB_A1 = New System.Windows.Forms.CheckBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.PB_AC = New System.Windows.Forms.PictureBox()
        CType(Me.PB_TXT, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PB_CSV, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DGV1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PB_AC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'bgWorker
        '
        Me.bgWorker.WorkerReportsProgress = True
        Me.bgWorker.WorkerSupportsCancellation = True
        '
        'PB_TXT
        '
        Me.PB_TXT.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PB_TXT.Image = CType(resources.GetObject("PB_TXT.Image"), System.Drawing.Image)
        Me.PB_TXT.Location = New System.Drawing.Point(325, 9)
        Me.PB_TXT.Name = "PB_TXT"
        Me.PB_TXT.Size = New System.Drawing.Size(129, 140)
        Me.PB_TXT.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PB_TXT.TabIndex = 22
        Me.PB_TXT.TabStop = False
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Khaki
        Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label1.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(160, 25)
        Me.Label1.TabIndex = 23
        Me.Label1.Text = "フラット35加入者"
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.Khaki
        Me.Label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label2.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 190)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(160, 25)
        Me.Label2.TabIndex = 24
        Me.Label2.Text = "アシスト加入者"
        '
        'PB_CSV
        '
        Me.PB_CSV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PB_CSV.Image = CType(resources.GetObject("PB_CSV.Image"), System.Drawing.Image)
        Me.PB_CSV.Location = New System.Drawing.Point(325, 190)
        Me.PB_CSV.Name = "PB_CSV"
        Me.PB_CSV.Size = New System.Drawing.Size(129, 140)
        Me.PB_CSV.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PB_CSV.TabIndex = 25
        Me.PB_CSV.TabStop = False
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.LightYellow
        Me.Label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label3.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label3.Location = New System.Drawing.Point(12, 34)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(298, 115)
        Me.Label3.TabIndex = 26
        Me.Label3.Text = "「kokyaku.txt」ファイルを、右側のTXTアイコンにドラッグ＆ドロップしてください。"
        '
        'Label4
        '
        Me.Label4.BackColor = System.Drawing.Color.LightYellow
        Me.Label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label4.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label4.Location = New System.Drawing.Point(12, 215)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(298, 130)
        Me.Label4.TabIndex = 27
        Me.Label4.Text = "SQL Makerから、以下の4つのファイルを「CSVフォーマット」で取り出して、右側のアイコンにドラッグ＆ドロップしてください。" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "　・機構借入申込情報還元デー" &
    "タ" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "　・督促状出力" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "　・債務者" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "　・連保人"
        '
        'L_UptimeTXT
        '
        Me.L_UptimeTXT.BackColor = System.Drawing.Color.Transparent
        Me.L_UptimeTXT.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.L_UptimeTXT.ForeColor = System.Drawing.Color.Blue
        Me.L_UptimeTXT.Location = New System.Drawing.Point(12, 149)
        Me.L_UptimeTXT.Name = "L_UptimeTXT"
        Me.L_UptimeTXT.Size = New System.Drawing.Size(439, 20)
        Me.L_UptimeTXT.TabIndex = 29
        Me.L_UptimeTXT.Text = "[最終更新日時] 2020/01/01 "
        '
        'L_UptimeCSV
        '
        Me.L_UptimeCSV.BackColor = System.Drawing.Color.Transparent
        Me.L_UptimeCSV.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.L_UptimeCSV.ForeColor = System.Drawing.Color.Blue
        Me.L_UptimeCSV.Location = New System.Drawing.Point(12, 345)
        Me.L_UptimeCSV.Name = "L_UptimeCSV"
        Me.L_UptimeCSV.Size = New System.Drawing.Size(442, 20)
        Me.L_UptimeCSV.TabIndex = 30
        Me.L_UptimeCSV.Text = "[最終更新日時] 2020/01/01"
        '
        'DGV1
        '
        Me.DGV1.AllowUserToAddRows = False
        Me.DGV1.AllowUserToDeleteRows = False
        Me.DGV1.AllowUserToResizeColumns = False
        Me.DGV1.AllowUserToResizeRows = False
        Me.DGV1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DGV1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGV1.Location = New System.Drawing.Point(0, 0)
        Me.DGV1.Name = "DGV1"
        Me.DGV1.RowHeadersVisible = False
        Me.DGV1.RowTemplate.Height = 21
        Me.DGV1.Size = New System.Drawing.Size(15, 10)
        Me.DGV1.TabIndex = 31
        Me.DGV1.TabStop = False
        Me.DGV1.Visible = False
        '
        'CB_A1
        '
        Me.CB_A1.AutoSize = True
        Me.CB_A1.Location = New System.Drawing.Point(262, 9)
        Me.CB_A1.Name = "CB_A1"
        Me.CB_A1.Size = New System.Drawing.Size(48, 16)
        Me.CB_A1.TabIndex = 32
        Me.CB_A1.Text = "解析"
        Me.CB_A1.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Button1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.Button1.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button1.ForeColor = System.Drawing.Color.Black
        Me.Button1.Location = New System.Drawing.Point(325, 336)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(129, 24)
        Me.Button1.TabIndex = 1569
        Me.Button1.TabStop = False
        Me.Button1.Text = "アシスト4ファイル完了"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Label5
        '
        Me.Label5.BackColor = System.Drawing.Color.LightYellow
        Me.Label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label5.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label5.Location = New System.Drawing.Point(460, 34)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(298, 115)
        Me.Label5.TabIndex = 1572
        Me.Label5.Text = "オートコールデータのファイルを、右側のTXTアイコンにドラッグ＆ドロップしてください。"
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.Color.Khaki
        Me.Label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label6.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label6.Location = New System.Drawing.Point(460, 9)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(160, 25)
        Me.Label6.TabIndex = 1571
        Me.Label6.Text = "オートコール"
        '
        'PB_AC
        '
        Me.PB_AC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PB_AC.Image = CType(resources.GetObject("PB_AC.Image"), System.Drawing.Image)
        Me.PB_AC.Location = New System.Drawing.Point(773, 9)
        Me.PB_AC.Name = "PB_AC"
        Me.PB_AC.Size = New System.Drawing.Size(129, 140)
        Me.PB_AC.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PB_AC.TabIndex = 1570
        Me.PB_AC.TabStop = False
        '
        'SCB1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(914, 365)
        Me.Controls.Add(Me.DGV1)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.PB_AC)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.CB_A1)
        Me.Controls.Add(Me.L_UptimeCSV)
        Me.Controls.Add(Me.L_UptimeTXT)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.PB_CSV)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.PB_TXT)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "SCB1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "F35データ読込"
        CType(Me.PB_TXT, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PB_CSV, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DGV1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PB_AC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents bgWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents PB_TXT As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents PB_CSV As PictureBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents L_UptimeTXT As Label
    Friend WithEvents L_UptimeCSV As Label
    Friend WithEvents DGV1 As DataGridView
    Friend WithEvents CB_A1 As CheckBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents PB_AC As PictureBox
End Class
