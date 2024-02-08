<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCC1_S1_MNG
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SCC1_S1_MNG))
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.ListBox2 = New System.Windows.Forms.ListBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.CB_DebugLog = New System.Windows.Forms.CheckBox()
        Me.CB_AplUpdateOff = New System.Windows.Forms.CheckBox()
        Me.NumericUpDown1 = New System.Windows.Forms.NumericUpDown()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ListBox1
        '
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 12
        Me.ListBox1.Location = New System.Drawing.Point(12, 12)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(178, 124)
        Me.ListBox1.TabIndex = 0
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(196, 12)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(94, 28)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "DBカラム追加"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(196, 46)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(94, 28)
        Me.Button2.TabIndex = 3
        Me.Button2.Text = "TimeStamp"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'ListBox2
        '
        Me.ListBox2.FormattingEnabled = True
        Me.ListBox2.ItemHeight = 12
        Me.ListBox2.Location = New System.Drawing.Point(359, 12)
        Me.ListBox2.Name = "ListBox2"
        Me.ListBox2.Size = New System.Drawing.Size(178, 124)
        Me.ListBox2.TabIndex = 4
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(443, 142)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(94, 28)
        Me.Button3.TabIndex = 5
        Me.Button3.Text = "コマンド実行"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'CB_DebugLog
        '
        Me.CB_DebugLog.AutoSize = True
        Me.CB_DebugLog.Location = New System.Drawing.Point(197, 81)
        Me.CB_DebugLog.Name = "CB_DebugLog"
        Me.CB_DebugLog.Size = New System.Drawing.Size(78, 16)
        Me.CB_DebugLog.TabIndex = 33
        Me.CB_DebugLog.Text = "デバッグログ"
        Me.CB_DebugLog.UseVisualStyleBackColor = True
        '
        'CB_AplUpdateOff
        '
        Me.CB_AplUpdateOff.AutoSize = True
        Me.CB_AplUpdateOff.Location = New System.Drawing.Point(197, 103)
        Me.CB_AplUpdateOff.Name = "CB_AplUpdateOff"
        Me.CB_AplUpdateOff.Size = New System.Drawing.Size(107, 16)
        Me.CB_AplUpdateOff.TabIndex = 34
        Me.CB_AplUpdateOff.Text = "アプリ更新の停止"
        Me.CB_AplUpdateOff.UseVisualStyleBackColor = True
        '
        'NumericUpDown1
        '
        Me.NumericUpDown1.Location = New System.Drawing.Point(359, 200)
        Me.NumericUpDown1.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        Me.NumericUpDown1.Name = "NumericUpDown1"
        Me.NumericUpDown1.Size = New System.Drawing.Size(60, 19)
        Me.NumericUpDown1.TabIndex = 35
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(359, 175)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(100, 19)
        Me.TextBox1.TabIndex = 36
        '
        'SCC1_S1_MNG
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(549, 328)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.NumericUpDown1)
        Me.Controls.Add(Me.CB_AplUpdateOff)
        Me.Controls.Add(Me.CB_DebugLog)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.ListBox2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ListBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "SCC1_S1_MNG"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "管理"
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents ListBox2 As ListBox
    Friend WithEvents Button3 As Button
    Friend WithEvents CB_DebugLog As CheckBox
    Friend WithEvents CB_AplUpdateOff As CheckBox
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents TextBox1 As TextBox
End Class
