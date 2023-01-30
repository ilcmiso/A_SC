Public Class SCA1_S3_Search
    Private Sub SCA1_S3_Search_Leave(sender As Object, e As EventArgs) Handles MyBase.Leave
        Me.Visible = False
    End Sub

    Private Sub BT_RecE1_Click(sender As Object, e As EventArgs) Handles BT_RecE1.Click
        Static Dim onoff As Boolean = False
        Dim blist = {CB_ADDR, CB_BIRTH, CB_ID, CB_NAME, CB_REPAY, CB_TEL, CB_WORK}
        For Each b In blist
            b.Checked = onoff
        Next
        onoff = Not onoff
    End Sub
End Class