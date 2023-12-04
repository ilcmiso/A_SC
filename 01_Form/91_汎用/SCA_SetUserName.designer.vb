<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCA_SetUserName
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
        Me.TB_UserName = New System.Windows.Forms.TextBox()
        Me.BT_A1 = New System.Windows.Forms.Button()
        Me.BT_A2 = New System.Windows.Forms.Button()
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
        'TB_UserName
        '
        Me.TB_UserName.BackColor = System.Drawing.Color.White
        Me.TB_UserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TB_UserName.Font = New System.Drawing.Font("メイリオ", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.TB_UserName.ForeColor = System.Drawing.Color.Black
        Me.TB_UserName.ImeMode = System.Windows.Forms.ImeMode.Hiragana
        Me.TB_UserName.Location = New System.Drawing.Point(13, 12)
        Me.TB_UserName.Name = "TB_UserName"
        Me.TB_UserName.Size = New System.Drawing.Size(203, 30)
        Me.TB_UserName.TabIndex = 20
        Me.TB_UserName.WordWrap = False
        '
        'BT_A1
        '
        Me.BT_A1.BackColor = System.Drawing.Color.SteelBlue
        Me.BT_A1.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A1.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A1.ForeColor = System.Drawing.Color.White
        Me.BT_A1.Location = New System.Drawing.Point(43, 46)
        Me.BT_A1.Name = "BT_A1"
        Me.BT_A1.Size = New System.Drawing.Size(86, 31)
        Me.BT_A1.TabIndex = 1813
        Me.BT_A1.Text = "登　　録"
        Me.BT_A1.UseVisualStyleBackColor = False
        '
        'BT_A2
        '
        Me.BT_A2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.BT_A2.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.BT_A2.Font = New System.Drawing.Font("MS UI Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.BT_A2.ForeColor = System.Drawing.Color.White
        Me.BT_A2.Location = New System.Drawing.Point(130, 46)
        Me.BT_A2.Name = "BT_A2"
        Me.BT_A2.Size = New System.Drawing.Size(86, 31)
        Me.BT_A2.TabIndex = 1814
        Me.BT_A2.Text = "キャンセル"
        Me.BT_A2.UseVisualStyleBackColor = False
        '
        'SCA_SetUserName
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(228, 84)
        Me.Controls.Add(Me.BT_A1)
        Me.Controls.Add(Me.BT_A2)
        Me.Controls.Add(Me.TB_UserName)
        Me.Controls.Add(Me.PBXX)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.Name = "SCA_SetUserName"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ユーザー名の設定"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents TB_UserName As TextBox
    Friend WithEvents BT_A1 As Button
    Friend WithEvents BT_A2 As Button
End Class
