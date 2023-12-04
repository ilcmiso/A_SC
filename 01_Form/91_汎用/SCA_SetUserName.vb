Public Class SCA_SetUserName

    Private ReadOnly log As New Log
    Private ReadOnly xml As New XmlMng

    Private Sub SCA_SetUserName_Load(sender As Object, e As EventArgs) Handles Me.Load
        TB_UserName.Text = xml.GetUserName
        TB_UserName.Select()
    End Sub

    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        If TB_UserName.Text.Length = 0 Then
            MsgBox($"空欄は設定できません。{vbCrLf}ご自身の名前を設定してください。")
            Exit Sub
        End If
        xml.SetUserName(TB_UserName.Text)
        MsgBox("ユーザー名を設定しました。")
        Me.Close()
    End Sub

    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.Close()
    End Sub
End Class
