<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VBR_Dun
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
        Me.CellReport1 = New AdvanceSoftware.VBReport8.CellReport(Me.components)
        Me.ViewerControl1 = New AdvanceSoftware.VBReport8.ViewerControl()
        Me.SuspendLayout()
        '
        'CellReport1
        '
        Me.CellReport1.TemporaryPath = Nothing
        '
        'ViewerControl1
        '
        Me.ViewerControl1.Enabled = False
        Me.ViewerControl1.Location = New System.Drawing.Point(0, 0)
        Me.ViewerControl1.MinimumSize = New System.Drawing.Size(64, 64)
        Me.ViewerControl1.Name = "ViewerControl1"
        Me.ViewerControl1.ShowReportFrame = AdvanceSoftware.VBReport8.ReportFrame.All
        Me.ViewerControl1.Size = New System.Drawing.Size(1004, 641)
        Me.ViewerControl1.TabIndex = 15
        '
        'VBR_DGV2
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.AppWorkspace
        Me.ClientSize = New System.Drawing.Size(1004, 641)
        Me.Controls.Add(Me.ViewerControl1)
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "VBR_DGV2"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "印刷プレビュー"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents CellReport1 As AdvanceSoftware.VBReport8.CellReport
    Friend WithEvents ViewerControl1 As AdvanceSoftware.VBReport8.ViewerControl
End Class
