Public Class SCA1_SendNGList

    Private ReadOnly log As New Log
    Private ReadOnly cmn As New Common
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' FKSCREMからデータを取得
        SCA1.db.UpdateOrigDT(Sqldb.TID.SCR)
        Dim sendNGList As DataTable = SCA1.db.GetSelect(Sqldb.TID.SCR, $"SELECT FKR01, FKR04 FROM FKSCREM WHERE FKR04 IN ('1', '2', '3')")

        ' FKR01のリストを作成
        Dim cidList As New List(Of String)
        For Each row As DataRow In sendNGList.Rows
            Dim cid As String = row("FKR01").ToString()
            cidList.Add(cid)
        Next

        ' FKSCから対応するデータを一度に取得
        Dim cids As String = String.Join(",", cidList.Select(Function(s) $"'{s}'"))
        Dim cosNames As DataTable = SCA1.db.GetSelect(Sqldb.TID.SC, $"SELECT FK02, FK10 FROM FKSC WHERE FK02 IN ({cids})")

        ' データを辞書にマッピング
        Dim cosNameDict As New Dictionary(Of String, String)
        For Each row As DataRow In cosNames.Rows
            cosNameDict(row("FK02").ToString()) = row("FK10").ToString()
        Next

        ' 種類のマッピング
        Dim typeMapping As New Dictionary(Of String, String) From {
        {"1", "主"},
        {"2", "連"},
        {"3", "両名"}
    }

        ' DataGridViewにデータを追加
        For Each row As DataRow In sendNGList.Rows
            Dim cid As String = row("FKR01").ToString()
            Dim fkr04 As String = row("FKR04").ToString()
            If cosNameDict.ContainsKey(cid) Then
                Dim name As String = cosNameDict(cid)
                Dim displayType As String = If(typeMapping.ContainsKey(fkr04), typeMapping(fkr04), fkr04)
                DGV1.Rows.Add(cid, name, displayType)
            End If
        Next
        L_STS.Text = $"該当者 {DGV1.Rows.Count} 件"
    End Sub

    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        If DGV1.Rows.Count = 0 Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        SCA1.ShowSelectUser(DGV1.CurrentRow.Cells(0).Value)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        cmn.ExcelOutputDGV($"督促状送付NGリスト.xlsx", DGV1)
    End Sub
End Class
