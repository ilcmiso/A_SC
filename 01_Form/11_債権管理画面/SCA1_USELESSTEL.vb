Public Class SCA1_USELESSTEL
    Private ReadOnly log As New Log
    Private ReadOnly db As New Sqldb
    Private fHeight As Integer

    Private BeforeVal As String

    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        fHeight = Me.Height             ' 自フォームの高さを保持する
    End Sub

    ' 画面表示イベント
    Private Sub SCA1_S4_TEL_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        If Not Me.Visible Then Exit Sub      ' 表示されたときのみ実行する
        BeforeVal = GetDGVVal()              ' 変更前のDGVデータを保持(変更したのかを判別する)
    End Sub

    ' フォーム外に移動(データ保存)
    Private Sub SCA1_S4_Search_Leave(sender As Object, e As EventArgs) Handles DGV1.MouseLeave
        DGV1.EndEdit()                      ' DGV入力中の状態を確定させる
        Dim val As String = GetDGVVal()
        If Not BeforeVal = val Then
            ' 編集されていたらDB更新する
            BeforeVal = val
            SaveDB(val)
            SCA1.db.UpdateOrigDT(Sqldb.TID.UNUMS)
        End If
        Me.Visible = False
    End Sub

    ' 行追加
    Private Sub ME_() Handles DGV1.RowsAdded
        ' 行追加に合わせてDGVのサイズ変更
        Dim h As Integer = fHeight + (DGV1.Rows.Count - 1) * 21
        Me.Height = h
        DGV1.Height = h
    End Sub

    ' DB Save
    Private Sub SaveDB(val As String)
        log.cLog("SaveDB")
        ' 先に一旦削除
        Dim deleteQuery As String = "DELETE FROM TBL"
        db.ExeSQL(Sqldb.TID.UNUMS, deleteQuery)

        ' DGVの各行を処理
        For Each row As DataGridViewRow In DGV1.Rows
            If Not row.IsNewRow Then ' 新しい行（未入力）はスキップ
                Dim tel As String = row.Cells(0).Value
                Dim dest As String = row.Cells(1).Value

                ' データをDBに保存
                Dim insertQuery As String = "INSERT INTO TBL (C01, C02) VALUES ('" & tel & "', '" & dest & "')"
                db.ExeSQL(Sqldb.TID.UNUMS, insertQuery)
            End If
        Next
    End Sub

    ' DBから読み取ってDGVに設定する
    Public Sub LoadDB()
        log.cLog("LoadDB")
        ' 既存の行をクリア
        SCA1.UselessForm.DGV1.Rows.Clear()

        ' DBからデータを読み込む
        Dim dt As DataTable = db.ReadOrgDtSelect(Sqldb.TID.UNUMS)  ' テーブル名を適切に設定してください

        ' データがない場合は読み込みしないで終了
        If dt.Rows.Count = 0 Then Exit Sub

        ' DGVにデータを設定
        For n = 0 To dt.Rows.Count - 1
            SCA1.UselessForm.DGV1.Rows.Add()
            SCA1.UselessForm.DGV1.Rows(n).Cells(0).Value = dt.Rows(n)("C01")  ' C01は電話番号
            SCA1.UselessForm.DGV1.Rows(n).Cells(1).Value = dt.Rows(n)("C02")  ' C02は宛先
        Next
        log.cLog("LoadDB completed")
    End Sub

    ' DGVに入力されている値を取得
    Private Function GetDGVVal() As String
        Dim val As String = ""
        For Each row As DataGridViewRow In DGV1.Rows
            If row.Cells(0).Value = "" And row.Cells(1).Value = "" Then Continue For  ' 空白行は保存しない
            val += row.Cells(0).Value & Sqldb.DELIMITER & row.Cells(1).Value & Sqldb.DELIMITER
        Next
        If val.EndsWith(Sqldb.DELIMITER) Then val = val.Remove(val.Length - 1, 1)     ' 余分な区切り文字を削除
        log.cLog("GetDGVVal:" & val)
        Return val
    End Function

    ' 全削除ボタン
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim result As DialogResult = MessageBox.Show("不通リストを全てクリアしますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            ' テーブルをクリア
            log.cLog("Deleted all records from UNUMS")
            db.ExeSQL(Sqldb.TID.UNUMS, "DELETE FROM TBL")
            db.ReadOrgDtSelect(Sqldb.TID.UNUMS)
        End If
        Me.Visible = False
    End Sub
End Class