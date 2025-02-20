<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SC
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SC))
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.TB_History = New System.Windows.Forms.TextBox()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.BT_APPUPDATE = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.GreenYellow
        Me.Button1.Font = New System.Drawing.Font("メイリオ", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.ForeColor = System.Drawing.Color.Blue
        Me.Button1.Location = New System.Drawing.Point(7, 12)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(233, 126)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "債権管理"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Button2.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button2.Location = New System.Drawing.Point(246, 56)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(176, 38)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "F35データ読込"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Button3
        '
        Me.Button3.BackColor = System.Drawing.SystemColors.Control
        Me.Button3.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button3.Location = New System.Drawing.Point(246, 100)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(176, 38)
        Me.Button3.TabIndex = 2
        Me.Button3.Text = "設　定"
        Me.Button3.UseVisualStyleBackColor = False
        '
        'TB_History
        '
        Me.TB_History.Font = New System.Drawing.Font("メイリオ", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_History.Location = New System.Drawing.Point(7, 144)
        Me.TB_History.Multiline = True
        Me.TB_History.Name = "TB_History"
        Me.TB_History.ReadOnly = True
        Me.TB_History.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TB_History.Size = New System.Drawing.Size(424, 91)
        Me.TB_History.TabIndex = 3
        '
        'Button4
        '
        Me.Button4.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Button4.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.Button4.Location = New System.Drawing.Point(246, 12)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(176, 38)
        Me.Button4.TabIndex = 4
        Me.Button4.Text = "顧客交渉経過記録"
        Me.Button4.UseVisualStyleBackColor = False
        '
        'BT_APPUPDATE
        '
        Me.BT_APPUPDATE.BackColor = System.Drawing.Color.Fuchsia
        Me.BT_APPUPDATE.Font = New System.Drawing.Font("メイリオ", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_APPUPDATE.Location = New System.Drawing.Point(7, 240)
        Me.BT_APPUPDATE.Name = "BT_APPUPDATE"
        Me.BT_APPUPDATE.Size = New System.Drawing.Size(92, 26)
        Me.BT_APPUPDATE.TabIndex = 6
        Me.BT_APPUPDATE.Text = "アプリ更新"
        Me.BT_APPUPDATE.UseVisualStyleBackColor = False
        Me.BT_APPUPDATE.Visible = False
        '
        'SC
        '
        Me.ClientSize = New System.Drawing.Size(434, 267)
        Me.Controls.Add(Me.BT_APPUPDATE)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.TB_History)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "SC"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "A_SC"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents TB_History As TextBox
    Friend WithEvents Button4 As Button
    Friend WithEvents BT_APPUPDATE As Button
End Class
