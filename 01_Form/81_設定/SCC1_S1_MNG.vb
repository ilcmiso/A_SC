Imports System.Data.SQLite

Public Class SCC1_S1_MNG

    Private ReadOnly db As New Sqldb
    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private ReadOnly xml As New XmlMng

    Private Sub SCC1_S1_MNG_Load(sender As Object, e As EventArgs) Handles Me.Load
        ShowListbox1()
        InitCommandList()
        xml.GetCPath()
        CB_DebugLog.Checked = xml.GetDebugMode()
        CB_AplUpdateOff.Checked = xml.GetAplUpdOff()
    End Sub

    Private Sub SCC1_S1_MNG_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        xml.SetDebugMode(CB_DebugLog.Checked)
        xml.SetAplUpdOff(CB_AplUpdateOff.Checked)
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
            "追加電話番号の検索ハイフン無しFKR06初期設定",
            "db3ファイル新規作成 (Value値)",
            "PINFO DB移管"
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
                    db.AddSQL(Sqldb.TID.SCR, cmd)
                Next
                log.cLog(dt.Rows.Count)
                db.ExeSQL(Sqldb.TID.SCR)

            Case 2
                Dim fileName As String = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{TextBox1.Text}.db3"
                Dim columnCount As Integer = NumericUpDown1.Value

                Dim connectionString As String = $"Data Source={fileName}; Version=3;"
                Using connection As New SQLiteConnection(connectionString)
                    Try
                        connection.Open()

                        Dim command As New SQLiteCommand(connection)

                        ' テーブル作成コマンドの作成
                        Dim createTableCommand As String = "CREATE TABLE IF NOT EXISTS TBL ("

                        ' 主キーのカラムを追加（NULL非許容、ブランクの初期値）
                        createTableCommand &= "C01 TEXT PRIMARY KEY NOT NULL DEFAULT ''"

                        ' その他のカラムを追加（ブランクの初期値）
                        For i As Integer = 2 To columnCount
                            createTableCommand &= $", C{i.ToString("D2")} TEXT NOT NULL DEFAULT ''"
                        Next

                        createTableCommand &= ");"

                        command.CommandText = createTableCommand
                        command.ExecuteNonQuery()
                    Finally
                        ' 接続を明示的に閉じる
                        If connection.State = ConnectionState.Open Then
                            connection.Close()
                        End If
                    End Try
                End Using

            Case 3
                db.DataTransferFPINFO()

        End Select

    End Sub

End Class
