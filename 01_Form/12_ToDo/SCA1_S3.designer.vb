<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCA1_S3
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SCA1_S3))
        Me.PBXX = New System.Windows.Forms.Button()
        Me.BT_A1 = New System.Windows.Forms.Button()
        Me.Label65 = New System.Windows.Forms.Label()
        Me.BT_A2 = New System.Windows.Forms.Button()
        Me.DTP_A4 = New System.Windows.Forms.DateTimePicker()
        Me.TB_A5 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Pic1 = New System.Windows.Forms.PictureBox()
        Me.L_FILE = New System.Windows.Forms.Label()
        Me.ColorDialog1 = New System.Windows.Forms.ColorDialog()
        Me.L_STS = New System.Windows.Forms.Label()
        Me.CB_A3 = New System.Windows.Forms.ComboBox()
        Me.CB_A2 = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TB_A1 = New System.Windows.Forms.TextBox()
        Me.CB_Limit = New System.Windows.Forms.CheckBox()
        CType(Me.Pic1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PBXX
        '
        Me.PBXX.BackColor = System.Drawing.Color.Silver
        Me.PBXX.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.PBXX.ForeColor = System.Drawing.Color.Black
        Me.PBXX.Location = New System.Drawing.Point(-106, -106)
        Me.PBXX.Name = "PBXX"
        Me.PBXX.Size = New System.Drawing.Size(75, 23)
        Me.PBXX.TabIndex = 19
        Me.PBXX.Text = "PBXX"
        Me.PBXX.UseVisualStyleBackColor = False
        '
        'BT_A1
        '
        Me.BT_A1.BackColor = System.Drawing.Color.SteelBlue
        Me.BT_A1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A1.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A1.ForeColor = System.Drawing.Color.White
        Me.BT_A1.Location = New System.Drawing.Point(341, 182)
        Me.BT_A1.Name = "BT_A1"
        Me.BT_A1.Size = New System.Drawing.Size(86, 31)
        Me.BT_A1.TabIndex = 4
        Me.BT_A1.Text = "確　　定"
        Me.BT_A1.UseVisualStyleBackColor = False
        '
        'Label65
        '
        Me.Label65.BackColor = System.Drawing.Color.Gainsboro
        Me.Label65.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label65.Font = New System.Drawing.Font("MS UI Gothic", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label65.ForeColor = System.Drawing.Color.Black
        Me.Label65.Location = New System.Drawing.Point(196, 44)
        Me.Label65.Name = "Label65"
        Me.Label65.Size = New System.Drawing.Size(77, 21)
        Me.Label65.TabIndex = 671
        Me.Label65.Text = "期　限"
        Me.Label65.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BT_A2
        '
        Me.BT_A2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.BT_A2.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A2.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A2.ForeColor = System.Drawing.Color.White
        Me.BT_A2.Location = New System.Drawing.Point(430, 182)
        Me.BT_A2.Name = "BT_A2"
        Me.BT_A2.Size = New System.Drawing.Size(86, 31)
        Me.BT_A2.TabIndex = 5
        Me.BT_A2.Text = "キャンセル"
        Me.BT_A2.UseVisualStyleBackColor = False
        '
        'DTP_A4
        '
        Me.DTP_A4.Enabled = False
        Me.DTP_A4.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.DTP_A4.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.DTP_A4.Location = New System.Drawing.Point(272, 45)
        Me.DTP_A4.MaxDate = New Date(2200, 12, 31, 0, 0, 0, 0)
        Me.DTP_A4.Name = "DTP_A4"
        Me.DTP_A4.Size = New System.Drawing.Size(115, 20)
        Me.DTP_A4.TabIndex = 0
        '
        'TB_A5
        '
        Me.TB_A5.BackColor = System.Drawing.Color.White
        Me.TB_A5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TB_A5.Font = New System.Drawing.Font("MS UI Gothic", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_A5.ForeColor = System.Drawing.Color.Black
        Me.TB_A5.ImeMode = System.Windows.Forms.ImeMode.Hiragana
        Me.TB_A5.Location = New System.Drawing.Point(82, 69)
        Me.TB_A5.Multiline = True
        Me.TB_A5.Name = "TB_A5"
        Me.TB_A5.Size = New System.Drawing.Size(434, 107)
        Me.TB_A5.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.Gainsboro
        Me.Label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label1.Font = New System.Drawing.Font("MS UI Gothic", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(6, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 20)
        Me.Label1.TabIndex = 1601
        Me.Label1.Text = "タスクリスト"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.Gainsboro
        Me.Label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label2.Font = New System.Drawing.Font("MS UI Gothic", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(6, 69)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(77, 107)
        Me.Label2.TabIndex = 1602
        Me.Label2.Text = "内容"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.Gainsboro
        Me.Label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label3.Font = New System.Drawing.Font("MS UI Gothic", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(196, 25)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(77, 20)
        Me.Label3.TabIndex = 1603
        Me.Label3.Text = "担当者"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Pic1
        '
        Me.Pic1.Image = CType(resources.GetObject("Pic1.Image"), System.Drawing.Image)
        Me.Pic1.Location = New System.Drawing.Point(405, 17)
        Me.Pic1.Name = "Pic1"
        Me.Pic1.Size = New System.Drawing.Size(112, 54)
        Me.Pic1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.Pic1.TabIndex = 1604
        Me.Pic1.TabStop = False
        '
        'L_FILE
        '
        Me.L_FILE.AutoSize = True
        Me.L_FILE.Location = New System.Drawing.Point(431, 5)
        Me.L_FILE.Name = "L_FILE"
        Me.L_FILE.Size = New System.Drawing.Size(63, 12)
        Me.L_FILE.TabIndex = 1605
        Me.L_FILE.Text = "添付ファイル"
        '
        'L_STS
        '
        Me.L_STS.AutoSize = True
        Me.L_STS.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.L_STS.ForeColor = System.Drawing.Color.Blue
        Me.L_STS.Location = New System.Drawing.Point(7, 200)
        Me.L_STS.Name = "L_STS"
        Me.L_STS.Size = New System.Drawing.Size(79, 13)
        Me.L_STS.TabIndex = 1606
        Me.L_STS.Text = "　ヒント　　　　"
        '
        'CB_A3
        '
        Me.CB_A3.BackColor = System.Drawing.Color.White
        Me.CB_A3.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.CB_A3.FormattingEnabled = True
        Me.CB_A3.Items.AddRange(New Object() {"田中", "佐々木", "山野"})
        Me.CB_A3.Location = New System.Drawing.Point(272, 25)
        Me.CB_A3.Name = "CB_A3"
        Me.CB_A3.Size = New System.Drawing.Size(115, 21)
        Me.CB_A3.TabIndex = 1608
        '
        'CB_A2
        '
        Me.CB_A2.BackColor = System.Drawing.Color.White
        Me.CB_A2.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.CB_A2.FormattingEnabled = True
        Me.CB_A2.Items.AddRange(New Object() {"重要", "低優先", "夜勤対応"})
        Me.CB_A2.Location = New System.Drawing.Point(82, 44)
        Me.CB_A2.Name = "CB_A2"
        Me.CB_A2.Size = New System.Drawing.Size(115, 21)
        Me.CB_A2.TabIndex = 1610
        '
        'Label4
        '
        Me.Label4.BackColor = System.Drawing.Color.Gainsboro
        Me.Label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label4.Font = New System.Drawing.Font("MS UI Gothic", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(6, 44)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(77, 21)
        Me.Label4.TabIndex = 1609
        Me.Label4.Text = "分　類"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TB_A1
        '
        Me.TB_A1.Enabled = False
        Me.TB_A1.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_A1.Location = New System.Drawing.Point(82, 25)
        Me.TB_A1.Name = "TB_A1"
        Me.TB_A1.ReadOnly = True
        Me.TB_A1.Size = New System.Drawing.Size(115, 20)
        Me.TB_A1.TabIndex = 1611
        '
        'CB_Limit
        '
        Me.CB_Limit.AutoSize = True
        Me.CB_Limit.Location = New System.Drawing.Point(391, 49)
        Me.CB_Limit.Name = "CB_Limit"
        Me.CB_Limit.Size = New System.Drawing.Size(15, 14)
        Me.CB_Limit.TabIndex = 1612
        Me.CB_Limit.UseVisualStyleBackColor = True
        '
        'SCA1_S3
        '
        Me.AllowDrop = True
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.WhiteSmoke
        Me.ClientSize = New System.Drawing.Size(522, 218)
        Me.Controls.Add(Me.CB_Limit)
        Me.Controls.Add(Me.TB_A5)
        Me.Controls.Add(Me.TB_A1)
        Me.Controls.Add(Me.CB_A2)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.CB_A3)
        Me.Controls.Add(Me.L_STS)
        Me.Controls.Add(Me.L_FILE)
        Me.Controls.Add(Me.Pic1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.DTP_A4)
        Me.Controls.Add(Me.BT_A2)
        Me.Controls.Add(Me.BT_A1)
        Me.Controls.Add(Me.Label65)
        Me.Controls.Add(Me.PBXX)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "SCA1_S3"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "タスク登録"
        CType(Me.Pic1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents BT_A1 As Button
    Friend WithEvents Label65 As Label
    Friend WithEvents BT_A2 As Button
    Friend WithEvents DTP_A4 As DateTimePicker
    Friend WithEvents TB_A5 As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Pic1 As PictureBox
    Friend WithEvents L_FILE As Label
    Friend WithEvents ColorDialog1 As ColorDialog
    Friend WithEvents L_STS As Label
    Friend WithEvents CB_A3 As ComboBox
    Friend WithEvents CB_A2 As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents TB_A1 As TextBox
    Friend WithEvents CB_Limit As CheckBox
End Class
