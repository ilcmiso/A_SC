Public Class SCC1_S1_MNG

    Private ReadOnly db As New Sqldb
    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private ReadOnly xml As New XmlMng

    Private Sub SCC1_S1_MNG_Load(sender As Object, e As EventArgs) Handles Me.Load
        ShowListbox1()
        InitCommandList()
        xml.GetCPath()
        CB_DebugMode.Checked = xml.GetDebugMode()
    End Sub

    Private Sub FLS_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        xml.SetDebugMode(CB_DebugMode.Checked)
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


    Private Sub ShowListbox1()
        ListBox1.Items.Clear()
        For Each tid In [Enum].GetValues(GetType(Sqldb.TID))
            Dim dt As DataTable = db.GetSelect(tid, "PRAGMA table_info('" & db.DBTbl(tid, Sqldb.DBID.TABLE) & "')")
            ListBox1.Items.Add(tid.ToString & vbTab & " 定義[" & db.DBTbl(tid, Sqldb.DBID.CNUM) & "]" & vbTab & "DB[" & dt.Rows.Count & "]")
        Next
    End Sub

    ' コマンドリスト生成
    Private Sub InitCommandList()
        Dim commandNames As String() = {
            "SC.exeのタイムスタンプ更新",
            "追加電話番号の検索ハイフン無しFKR06初期設定"
            }
        ListBox2.Items.Clear()
        For Each cl In commandNames
            ListBox2.Items.Add(cl)
        Next
    End Sub

    ' コマンド実行
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

                Dim r As Integer
        r = MessageBox.Show(ListBox2.SelectedItem.ToString & " 実行しますか？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

        Select Case ListBox2.SelectedIndex
            Case 0
                ' SC.exeのタイムスタンプ更新
                Dim svFilePath As String = cmn.CurrentPath & Common.DIR_UPD & Common.EXE_NAME
                If IO.File.Exists(svFilePath) Then IO.File.SetLastWriteTime(svFilePath, DateTime.Now)

            Case 1
                ' 追加電話番号の検索ハイフン無しFKR06初期設定
                Dim dt As DataTable = db.GetSelect(Sqldb.TID.SCR, "Select FKR01, FKR05 From FKSCREM Where FKR05 <> ''")
                Dim cmd As String
                log.cLog(dt.Rows.Count)
                For n = 0 To dt.Rows.Count - 1
                    cmd = String.Format("Update FKSCREM Set FKR06 = '{0}' Where FKR01 = '{1}'", dt.Rows(n)(1).ToString.Replace("-", ""), dt.Rows(n)(0))
                    db.AddSQL(cmd)
                Next
                log.cLog(dt.Rows.Count)
                db.ExeSQL(Sqldb.TID.SCR)
        End Select

    End Sub

End Class
