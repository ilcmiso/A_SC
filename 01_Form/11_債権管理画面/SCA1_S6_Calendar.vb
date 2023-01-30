Public Class SCA1_S6_Calendar
    Private OwnerForm As SCA1
    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        OwnerForm = DirectCast(Me.Owner, SCA1)            ' 親フォームを参照できるようにキャスト
    End Sub

    Private Sub ME_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'Me.DateTimePicker1.Select()
        'SendKeys.SendWait("%{DOWN}") ' ［↓］キーの送信
    End Sub

    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker1.ValueChanged, Button2.Click
        OwnerForm.DGV7.CurrentCell.Value = DateTimePicker1.Value.ToString("yyyy/MM/dd")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OwnerForm.DGV7.CurrentCell.Value = ""
    End Sub
End Class