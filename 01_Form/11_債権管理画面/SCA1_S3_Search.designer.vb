<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SCA1_S3_Search
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
        Me.CB_ID = New System.Windows.Forms.CheckBox()
        Me.CB_NAME = New System.Windows.Forms.CheckBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.CB_BIRTH = New System.Windows.Forms.CheckBox()
        Me.CB_ADDR = New System.Windows.Forms.CheckBox()
        Me.CB_REPAY = New System.Windows.Forms.CheckBox()
        Me.CB_WORK = New System.Windows.Forms.CheckBox()
        Me.CB_TEL = New System.Windows.Forms.CheckBox()
        Me.BT_RecE1 = New System.Windows.Forms.Button()
        Me.Panel1.SuspendLayout()
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
        'CB_ID
        '
        Me.CB_ID.AutoSize = True
        Me.CB_ID.BackColor = System.Drawing.Color.Azure
        Me.CB_ID.Checked = True
        Me.CB_ID.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_ID.Location = New System.Drawing.Point(12, 9)
        Me.CB_ID.Name = "CB_ID"
        Me.CB_ID.Size = New System.Drawing.Size(72, 16)
        Me.CB_ID.TabIndex = 5
        Me.CB_ID.Text = "債権番号"
        Me.CB_ID.UseVisualStyleBackColor = False
        '
        'CB_NAME
        '
        Me.CB_NAME.AutoSize = True
        Me.CB_NAME.Checked = True
        Me.CB_NAME.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_NAME.Location = New System.Drawing.Point(11, 30)
        Me.CB_NAME.Name = "CB_NAME"
        Me.CB_NAME.Size = New System.Drawing.Size(48, 16)
        Me.CB_NAME.TabIndex = 20
        Me.CB_NAME.Text = "氏名"
        Me.CB_NAME.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.Azure
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.BT_RecE1)
        Me.Panel1.Controls.Add(Me.CB_BIRTH)
        Me.Panel1.Controls.Add(Me.CB_ADDR)
        Me.Panel1.Controls.Add(Me.CB_REPAY)
        Me.Panel1.Controls.Add(Me.CB_WORK)
        Me.Panel1.Controls.Add(Me.CB_NAME)
        Me.Panel1.Controls.Add(Me.CB_TEL)
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(260, 103)
        Me.Panel1.TabIndex = 21
        '
        'CB_BIRTH
        '
        Me.CB_BIRTH.AutoSize = True
        Me.CB_BIRTH.Checked = True
        Me.CB_BIRTH.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_BIRTH.Location = New System.Drawing.Point(11, 74)
        Me.CB_BIRTH.Name = "CB_BIRTH"
        Me.CB_BIRTH.Size = New System.Drawing.Size(72, 16)
        Me.CB_BIRTH.TabIndex = 26
        Me.CB_BIRTH.Text = "生年月日"
        Me.CB_BIRTH.UseVisualStyleBackColor = True
        '
        'CB_ADDR
        '
        Me.CB_ADDR.AutoSize = True
        Me.CB_ADDR.Checked = True
        Me.CB_ADDR.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_ADDR.Location = New System.Drawing.Point(146, 30)
        Me.CB_ADDR.Name = "CB_ADDR"
        Me.CB_ADDR.Size = New System.Drawing.Size(48, 16)
        Me.CB_ADDR.TabIndex = 25
        Me.CB_ADDR.Text = "住所"
        Me.CB_ADDR.UseVisualStyleBackColor = True
        '
        'CB_REPAY
        '
        Me.CB_REPAY.AutoSize = True
        Me.CB_REPAY.Checked = True
        Me.CB_REPAY.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_REPAY.Location = New System.Drawing.Point(11, 52)
        Me.CB_REPAY.Name = "CB_REPAY"
        Me.CB_REPAY.Size = New System.Drawing.Size(60, 16)
        Me.CB_REPAY.TabIndex = 24
        Me.CB_REPAY.Text = "返済額"
        Me.CB_REPAY.UseVisualStyleBackColor = True
        '
        'CB_WORK
        '
        Me.CB_WORK.AutoSize = True
        Me.CB_WORK.Checked = True
        Me.CB_WORK.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_WORK.Location = New System.Drawing.Point(146, 52)
        Me.CB_WORK.Name = "CB_WORK"
        Me.CB_WORK.Size = New System.Drawing.Size(60, 16)
        Me.CB_WORK.TabIndex = 23
        Me.CB_WORK.Text = "勤務先"
        Me.CB_WORK.UseVisualStyleBackColor = True
        '
        'CB_TEL
        '
        Me.CB_TEL.AutoSize = True
        Me.CB_TEL.Checked = True
        Me.CB_TEL.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CB_TEL.Location = New System.Drawing.Point(146, 8)
        Me.CB_TEL.Name = "CB_TEL"
        Me.CB_TEL.Size = New System.Drawing.Size(72, 16)
        Me.CB_TEL.TabIndex = 22
        Me.CB_TEL.Text = "電話番号"
        Me.CB_TEL.UseVisualStyleBackColor = True
        '
        'BT_RecE1
        '
        Me.BT_RecE1.Location = New System.Drawing.Point(142, 74)
        Me.BT_RecE1.Name = "BT_RecE1"
        Me.BT_RecE1.Size = New System.Drawing.Size(113, 23)
        Me.BT_RecE1.TabIndex = 1756
        Me.BT_RecE1.Text = "全選択/解除"
        Me.BT_RecE1.UseVisualStyleBackColor = True
        '
        'SCA1_S3_Search
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(260, 103)
        Me.Controls.Add(Me.CB_ID)
        Me.Controls.Add(Me.PBXX)
        Me.Controls.Add(Me.Panel1)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "SCA1_S3_Search"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "対応記録の登録"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PBXX As Button
    Friend WithEvents CB_ID As CheckBox
    Friend WithEvents CB_NAME As CheckBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents CB_WORK As CheckBox
    Friend WithEvents CB_TEL As CheckBox
    Friend WithEvents CB_REPAY As CheckBox
    Friend WithEvents CB_ADDR As CheckBox
    Friend WithEvents CB_BIRTH As CheckBox
    Friend WithEvents BT_RecE1 As Button
End Class
