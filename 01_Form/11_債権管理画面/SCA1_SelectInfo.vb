Public Class SCA1_SelectInfo

    Dim InfoItems() As String = {"機構番号", "ローン種別(1=F35,2=A)", "金消契約日", "当月回収元金", "次回償還金", "次々回償還金", "増額返済月",
                                 "証券番号", "債務者 氏名", "債務者 ﾖﾐｶﾅ", "債務者 生年月日", "債務者 性別", "債務者 TEL1", "債務者 郵便番号",
                                 "債務者 住所 ", "債務者 勤務先 ", "債務者 勤務先TEL1 ", "債務者 団信加入サイン", "連帯債務者 氏名 ",
                                 "連帯債務者 ﾖﾐｶﾅ ", "連帯債務者 生年月日 ", "連帯債務者 性別 ", "連帯債務者 TEL1 ", "連帯債務者 TEL2 ",
                                 "連帯債務者 郵便番号 ", "連帯債務者 住所 ", "連帯債務者 勤務先 ", "連帯債務者 勤務先TEL1 ", "連帯債務者 勤務先TEL2 ",
                                 "連帯債務者 団信加入サイン", "更新日残高(ボーナス)", "返済額2(加算用)", "返済額", "残高更新日", "延滞月数",
                                 "返済額(ボーナス)", "更新日残高", "延滞合計額", "貸付金額", "貸付金額(ボーナス)", "金消契約日(アシスト)", "受任者"}
    Dim InfoIdx() As Integer = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 21, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 59, 0}
    Dim ownForm As SCA1
    Dim log As New Log

    Private Sub SCA1_SelectInfo_Load(sender As Object, e As EventArgs) Handles Me.Load
        ownForm = DirectCast(Me.Owner, SCA1)            ' 親フォームを参照できるようにキャスト
        UpdateDataGridView(DGV_SI)
    End Sub

    ' DGV生成
    Public Sub UpdateDataGridView(dgv As DataGridView)
        ' 行をインプット配列の数だけ追加
        dgv.Rows.Clear()
        For Each str As String In InfoItems
            dgv.Rows.Add(str, False)
        Next
    End Sub

    ' 全選択
    Private Sub BT_RecE1_Click(sender As Object, e As EventArgs) Handles BT_RecE1.Click
        Static Dim toggleState = True

        ' DGVの2列目のチェックボックスを全てONまたはOFFにする
        For Each row As DataGridViewRow In DGV_SI.Rows
            row.Cells(1).Value = toggleState
        Next
        toggleState = Not toggleState
    End Sub

    ' Excel出力
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim outList As New List(Of List(Of String))
        Dim cid As String

        log.cLog("-- Excel出力開始 --")
        log.TimerST()

        ' 項目名を出力
        Dim rowItems As New List(Of String)
        For idx = 0 To InfoItems.Length - 1
            ' 出力情報のチェックボックスがOFFの項目は出力しない
            If DGV_SI(1, idx).Value = False Then Continue For

            rowItems.Add(InfoItems(idx))
        Next
        outList.Add(rowItems)

        ' 顧客番号リストから１つづつ番号を取得
        For cIdx = 0 To ownForm.TB_DunIN.Lines.Length - 1
            cid = ownForm.TB_DunIN.Lines(cIdx)
            log.cLog("-- cid : " & cid)

            ' 顧客番号をもとに、顧客DBからInfoItemsのデータを取得してdataRowに格納
            'Dim dataRow As String() = Nothing
            Dim cInfo As DataRow
            Dim cInfoDataRow As DataRow()
            Dim rowValue As New List(Of String)

            For n = 0 To InfoIdx.Length - 1
                ' 出力情報のチェックボックスがOFFの項目は出力しない
                If DGV_SI(1, n).Value = False Then Continue For

                ' 該当顧客のFKSC情報を取得して設定
                Dim dInfoStr As String = ""
                cInfoDataRow = ownForm.db.OrgDataTablePlusAssist.Select(String.Format("FK02 = '{0}' Or FK09 = '{0}'", cid))
                If cInfoDataRow.Length > 0 Then
                    cInfo = cInfoDataRow(0)

                    ' InfoItemsが"受任者"のみ、FKSC情報ではなく物件情報(PINFO)から取得して設定する
                    If InfoItems(n) = "受任者" Then
                        ' cidが証券番号の可能性もあるので、cidではなく顧客情報であるcInfo(1)を引数にする
                        If SCA1.GetAssignee(cInfo(1), cInfo(9)) Then dInfoStr = "主"      ' 主債務者
                        If SCA1.GetAssignee(cInfo(1), cInfo(29)) Then dInfoStr += "連"    ' 連帯債務者
                        rowValue.Add(dInfoStr)
                        Continue For
                    End If

                    ' DBの情報がDBNullだった場合は値を設定せず、空欄のままにしておく
                    If cInfo(InfoIdx(n) - 1) IsNot DBNull.Value Then
                        dInfoStr = cInfo(InfoIdx(n) - 1)
                    End If
                    rowValue.Add(dInfoStr)
                End If
            Next
            outList.Add(rowValue)
        Next

        If outList.Count > 0 Then
            log.cLog("-- outList.Count : " & outList.Count)
            log.TimerED("Excel出力")
            Dim cExc As New CExcel
            cExc.ExportToExcel(outList, "督促情報一覧.xlsx")
        End If
        log.cLog("-- Excel出力終了 --")
        Me.Close()
    End Sub

End Class