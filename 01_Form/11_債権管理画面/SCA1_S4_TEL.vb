Public Class SCA1_S4_TEL
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
        BeforeVal = GetDGVVal()         ' 変更前のDGVデータを保持(変更したのかを判別する)
    End Sub

    ' フォーム外に移動(データ保存)
    Private Sub SCA1_S4_Search_Leave(sender As Object, e As EventArgs) Handles DGV1.MouseLeave
        DGV1.EndEdit()                      ' DGV入力中の状態を確定させる
        Dim val As String = GetDGVVal()
        If Not BeforeVal = val Then
            ' 編集されていたらDB更新する
            BeforeVal = val
            SaveDB(val)
            SCA1.db.UpdateOrigDT(Sqldb.TID.SCR)
            SCA1.BuildDGV1SearchCache()
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
        Dim id As String = SCA1.CurrentCID      ' 債権番号

        ' 追加電話番号の更新
        If db.IsExistREM(id) Then
            ' 既存に存在するので05と06だけ更新 ※06はハイフン無しの文字列
            db.ExeSQL(Sqldb.TID.SCR, $"UPDATE FKSCREM SET FKR05 = '{val}', FKR06 = '{val.Replace("-", "")}' WHERE FKR01 = '{id}'")
        Else
            ' 新規は05、06だけ更新して他は空白にしておく
            db.ExeSQL(Sqldb.TID.SCR, "Insert Into FKSCREM Values('" & id & "','','','','" & val & "','" & val.Replace("-", "") & "')")
        End If
    End Sub

    ' DBから読み取ってDGVに設定する
    Public Sub LoadDB()
        log.cLog("AddTell LoadDB")
        log.TimerST()
        If SCA1.DGV1.Rows.Count = 0 Then Exit Sub
        SCA1.AddTelForm.DGV1.Rows.Clear()
        Dim id As String = SCA1.DGV1.CurrentRow.Cells(0).Value      ' 債権番号
        Dim dt As DataTable = db.ReadOrgDtSelect(Sqldb.TID.SCR, "Select FKR05 From FKSCREM Where FKR01 = '" & id & "'")
        If Not dt.Rows.Count = 1 Then Exit Sub  ' データがない場合は読み込みしないで終了

        ' 「#名称#番号」の繰り返し形式データを読み出してDGVに設定
        Dim arr() As String = dt.Rows(0).Item(0).ToString.Split(Sqldb.DELIMITER)
        If arr.Length < 2 Then Exit Sub ' データが保存されてない場合は終了
        log.cLog("LoadDB SQL:" & dt.Rows(0).Item(0).ToString)
        For c = 0 To arr.Length - 1 Step 2      ' 2カラム分処理するから余分な回スキップ
            SCA1.AddTelForm.DGV1.Rows.Add()
            SCA1.AddTelForm.DGV1.Rows(c / 2).Cells(0).Value = arr(c)
            SCA1.AddTelForm.DGV1.Rows(c / 2).Cells(1).Value = arr(c + 1)
        Next

        log.TimerED("LoadDB")
    End Sub

    ' DGV内容取得
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

End Class