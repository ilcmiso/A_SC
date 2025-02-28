<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCC1
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SCC1))
        Me.SP = New System.IO.Ports.SerialPort(Me.components)
        Me.BT_A1 = New System.Windows.Forms.Button()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.nIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.CB_DBSW = New System.Windows.Forms.CheckBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TB_SQLAddr = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'SP
        '
        '
        'BT_A1
        '
        Me.BT_A1.Font = New System.Drawing.Font("メイリオ", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A1.ForeColor = System.Drawing.Color.Blue
        Me.BT_A1.Location = New System.Drawing.Point(244, 107)
        Me.BT_A1.Name = "BT_A1"
        Me.BT_A1.Size = New System.Drawing.Size(178, 70)
        Me.BT_A1.TabIndex = 1
        Me.BT_A1.Text = "接続開始"
        Me.BT_A1.UseVisualStyleBackColor = True
        '
        'ListBox1
        '
        Me.ListBox1.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 12
        Me.ListBox1.Location = New System.Drawing.Point(14, 107)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(204, 40)
        Me.ListBox1.TabIndex = 11
        '
        'nIcon
        '
        Me.nIcon.BalloonTipText = "タスクトレイで稼働中です。"
        Me.nIcon.BalloonTipTitle = "A_SC 電話接続"
        Me.nIcon.Icon = CType(resources.GetObject("nIcon.Icon"), System.Drawing.Icon)
        Me.nIcon.Text = "電話接続"
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button1.Location = New System.Drawing.Point(13, 150)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(205, 25)
        Me.Button1.TabIndex = 13
        Me.Button1.Text = "更新"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label2.Location = New System.Drawing.Point(16, 8)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(406, 69)
        Me.Label2.TabIndex = 15
        Me.Label2.Text = " [ 電話接続手順 ]" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "①　ナンバーディスプレイ(アロハND6)をパソコンとUSB接続" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "②　下部のシリアルポートのリストから該当ポートを選択" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "③ 「接続開始" &
    "」ボタンを押す"
        '
        'TextBox1
        '
        Me.TextBox1.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TextBox1.Location = New System.Drawing.Point(32, 259)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(344, 19)
        Me.TextBox1.TabIndex = 16
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Blue
        Me.Label1.Location = New System.Drawing.Point(15, 87)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(125, 18)
        Me.Label1.TabIndex = 17
        Me.Label1.Text = "[ COMポートリスト ]"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Blue
        Me.Label3.Location = New System.Drawing.Point(15, 240)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(146, 18)
        Me.Label3.TabIndex = 18
        Me.Label3.Text = "[ データ格納場所の設定 ]"
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("MS UI Gothic", 9.0!)
        Me.Button2.Location = New System.Drawing.Point(375, 258)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(47, 21)
        Me.Button2.TabIndex = 19
        Me.Button2.Text = "変更"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'TextBox2
        '
        Me.TextBox2.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TextBox2.Location = New System.Drawing.Point(32, 285)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(344, 19)
        Me.TextBox2.TabIndex = 20
        '
        'Button3
        '
        Me.Button3.Font = New System.Drawing.Font("MS UI Gothic", 9.0!)
        Me.Button3.Location = New System.Drawing.Point(375, 284)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(47, 21)
        Me.Button3.TabIndex = 21
        Me.Button3.Text = "変更"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Checked = True
        Me.RadioButton1.Location = New System.Drawing.Point(15, 262)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(14, 13)
        Me.RadioButton1.TabIndex = 22
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Location = New System.Drawing.Point(15, 287)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(14, 13)
        Me.RadioButton2.TabIndex = 23
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'CB_DBSW
        '
        Me.CB_DBSW.AutoSize = True
        Me.CB_DBSW.Location = New System.Drawing.Point(15, 210)
        Me.CB_DBSW.Name = "CB_DBSW"
        Me.CB_DBSW.Size = New System.Drawing.Size(64, 16)
        Me.CB_DBSW.TabIndex = 24
        Me.CB_DBSW.Text = "DB切替"
        Me.CB_DBSW.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("メイリオ", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Blue
        Me.Label4.Location = New System.Drawing.Point(16, 189)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(146, 18)
        Me.Label4.TabIndex = 25
        Me.Label4.Text = "[ データベース切り替え ]"
        '
        'TB_SQLAddr
        '
        Me.TB_SQLAddr.Font = New System.Drawing.Font("MS UI Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_SQLAddr.Location = New System.Drawing.Point(85, 207)
        Me.TB_SQLAddr.Name = "TB_SQLAddr"
        Me.TB_SQLAddr.Size = New System.Drawing.Size(291, 19)
        Me.TB_SQLAddr.TabIndex = 26
        '
        'SCC1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(434, 312)
        Me.Controls.Add(Me.TB_SQLAddr)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.CB_DBSW)
        Me.Controls.Add(Me.RadioButton2)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.TextBox2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.BT_A1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ListBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "SCC1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "設定画面"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents SP As IO.Ports.SerialPort
    Friend WithEvents BT_A1 As Button
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents nIcon As NotifyIcon
    Friend WithEvents Button1 As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Button3 As Button
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents CB_DBSW As CheckBox
    Friend WithEvents Label4 As Label
    Friend WithEvents TB_SQLAddr As TextBox
End Class
