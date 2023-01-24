Public Class SCC1_S1_MNG

    Private ReadOnly db As New Sqldb
    Private ReadOnly cmn As New Common

    Private Sub SCC1_S1_MNG_Load(sender As Object, e As EventArgs) Handles Me.Load
        ShowListbox1()
        DGV1.Rows.Add()
        DGV1.Rows.Add()
    End Sub

    Private Sub ShowListbox1()
        ListBox1.Items.Clear()
        For Each tid In [Enum].GetValues(GetType(Sqldb.TID))
            Dim dt As DataTable = db.GetSelect(tid, "PRAGMA table_info('" & db.DBTbl(tid, Sqldb.DBID.TABLE) & "')")
            ListBox1.Items.Add(tid.ToString & vbTab & " 定義[" & db.DBTbl(tid, Sqldb.DBID.CNUM) & "]" & vbTab & "DB[" & dt.Rows.Count & "]")
        Next
    End Sub



    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim r As Integer
        r = MessageBox.Show("カラム追加？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        db.AddColumns(ListBox1.SelectedIndex)
        ShowListbox1()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim r As Integer
        r = MessageBox.Show("サーバーのタイムスタンプ更新？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)

        IO.File.SetLastWriteTime(cmn.CurrentPath & Common.DIR_UPD & Common.EXE_NAME, DateTime.Now)
    End Sub
End Class
