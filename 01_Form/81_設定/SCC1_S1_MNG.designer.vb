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
        Me.CB_DebugMode = New System.Windows.Forms.CheckBox()
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
        'CB_DebugMode
        '
        Me.CB_DebugMode.AutoSize = True
        Me.CB_DebugMode.Location = New System.Drawing.Point(197, 81)
        Me.CB_DebugMode.Name = "CB_DebugMode"
        Me.CB_DebugMode.Size = New System.Drawing.Size(88, 16)
        Me.CB_DebugMode.TabIndex = 33
        Me.CB_DebugMode.Text = "デバッグモード"
        Me.CB_DebugMode.UseVisualStyleBackColor = True
        '
        'SCC1_S1_MNG
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(549, 328)
        Me.Controls.Add(Me.CB_DebugMode)
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
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents ListBox2 As ListBox
    Friend WithEvents Button3 As Button
    Friend WithEvents CB_DebugMode As CheckBox
End Class
