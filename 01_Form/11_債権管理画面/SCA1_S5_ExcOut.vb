Public Class SCA1_S5_ExcOut

    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LB_SELITEM.SetSelected(0, True)
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim f As New VBR_PI
        f.ShowDialog(Me)
        f.Dispose()
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            LB_SELITEM.SelectionMode = SelectionMode.MultiSimple
            For n = 0 To LB_SELITEM.Items.Count - 1
                LB_SELITEM.SetSelected(n, True)
            Next
            LB_SELITEM.Enabled = False
        Else
            LB_SELITEM.Enabled = True
            LB_SELITEM.SetSelected(0, True)
            LB_SELITEM.SelectionMode = SelectionMode.One
        End If
    End Sub
End Class