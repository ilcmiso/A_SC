Public Class SCA1_SelectInfo

    Dim InfoItems() As String = {"機構番号", "ローン種別(1=F35,2=A)", "金消契約日", "当月回収元金", "次回償還金", "次々回償還金", "増額返済月",
                                 "証券番号", "債務者 氏名", "債務者 ﾖﾐｶﾅ", "債務者 生年月日", "債務者 性別", "債務者 TEL1", "債務者 郵便番号",
                                 "債務者 住所 ", "債務者 勤務先 ", "債務者 勤務先TEL1 ", "債務者 団信加入サイン", "連帯債務者 氏名 ",
                                 "連帯債務者 ﾖﾐｶﾅ ", "連帯債務者 生年月日 ", "連帯債務者 性別 ", "連帯債務者 TEL1 ", "連帯債務者 TEL2 ",
                                 "連帯債務者 郵便番号 ", "連帯債務者 住所 ", "連帯債務者 勤務先 ", "連帯債務者 勤務先TEL1 ", "連帯債務者 勤務先TEL2 ",
                                 "連帯債務者 団信加入サイン", "更新日残高(ボーナス)", "返済額2(加算用)", "返済額", "残高更新日", "延滞月数",
                                 "返済額(ボーナス)", "更新日残高", "延滞合計額", "貸付金額", "貸付金額(ボーナス)", "金消契約日(アシスト)", "受任者", "延滞請求額"}
    Dim InfoIdx() As Integer = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 21, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 59, 0, 0}
    Dim ownForm As SCA1
    Dim log As New Log
    Dim cmn As New Common

    Private Sub SCA1_SelectInfo_Load(sender As Object, e As EventArgs) Handles Me.Load
        ownForm = DirectCast(Me.Owner, SCA1)            ' 親フォームを参照できるようにキャスト

        ' オートコール日割り計算、約定日13日から当日までの日数を算出
        SetAutoCallDays()

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
        Dim cExc As New CExcel

        Dim fileName As String = cExc.GetSaveFileName(CExcel.FILTER_EXCEL, "督促情報一覧.xlsx")
        If fileName = String.Empty Then Exit Sub

        log.cLog("-- Excel出力開始 --")
        log.TimerST()
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

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
            'log.cLog("-- cid : " & cid)

            ' 顧客番号をもとに、顧客DBからInfoItemsのデータを取得してdataRowに格納
            Dim cInfo As DataRow = Nothing
            Dim cInfoDataRow As DataRow()
            Dim rowValue As New List(Of String)

            cInfoDataRow = ownForm.db.OrgDataTablePlusAssist.Select(String.Format("FK02 = '{0}' Or FK09 = '{0}'", cid))
            If cInfoDataRow.Length > 0 Then
                cInfo = cInfoDataRow(0)
            End If

            For n = 0 To InfoIdx.Length - 1
                ' 出力情報のチェックボックスがOFFの項目は出力しない
                If DGV_SI(1, n).Value = False Then Continue For
                If cInfo Is Nothing Then Continue For

                ' 該当顧客のFKSC情報を取得して設定
                Dim dInfoStr As String = ""

                ' InfoItemsが"受任者"のみ、FKSC情報ではなく物件情報(PINFO)から取得して設定する

                Select Case (InfoItems(n))
                    Case "受任者"
                        ' cidが証券番号の可能性もあるので、cidではなく顧客情報であるcInfo(1)を引数にする
                        If SCA1.GetAssignee(cInfo(1), cInfo(9)) Then dInfoStr = "主"      ' 主債務者
                        If SCA1.GetAssignee(cInfo(1), cInfo(29)) Then dInfoStr += "連"    ' 連帯債務者
                        rowValue.Add(dInfoStr)
                        Continue For

                    Case "延滞請求額"
                        ' オートコールによる延滞損害金の合計額
                        Dim dr As DataRow()
                        dr = ownForm.db.OrgDataTable(Sqldb.TID.AC).Select($"C01 = {cInfo(1)}")
                        If dr.Length > 0 Then
                            Dim lateVal1 As Integer = cmn.Int(dr(0)(1)) ' 延滞元利金（累計）      約定日現在の累計（当月分含む）
                            Dim lateVal2 As Integer = cmn.Int(dr(0)(2)) ' 延滞損害金（累計）      約定日現在の累計（当月分含まず）
                            Dim lateValU As Integer = cmn.Int(dr(0)(3)) ' 延滞損害金単価（今後）  前回までの単価累計と今回の単価合計値（下4桁は円未満）
                            Dim period As Integer = NUD_AC_DAYS.Value
                            Dim lateValDay As Integer = (Math.Floor((lateValU * period) / 10000)) ' 延滞損害金の単価 x 日数 （小数値を切り捨て）
                            Dim totalVal As Integer = lateVal1 + lateVal2 + lateValDay
                            rowValue.Add(totalVal)
                        End If
                        Continue For

                    Case "返済額(ボーナス)"
                        If Not IsBonusMonth(cInfo(7), ownForm.NUD_DunA1.Value.ToString) Then
                            ' ボーナス対象月でない場合、ボーナス返済額を0として表示
                            rowValue.Add("0")
                            Continue For
                        End If
                End Select


                ' DBの情報がDBNullだった場合は値を設定せず、空欄のままにしておく
                If cInfo(InfoIdx(n) - 1) IsNot DBNull.Value Then
                    dInfoStr = cInfo(InfoIdx(n) - 1)
                End If
                rowValue.Add(dInfoStr)
            Next
            outList.Add(rowValue)
        Next
        log.TimerED("Excel出力")

        log.cLog("-- outList.Count : " & outList.Count)
        If outList.Count > 0 Then
            If Not String.IsNullOrEmpty(fileName) Then
                cExc.ExportToExcel(outList, fileName)
            End If
        End If
        log.cLog("-- Excel出力終了 --")
        Me.Close()
    End Sub

    Private Sub SetAutoCallDays()
        Const CONTRACT_DATE As Integer = 13     ' 約定日
        Dim today As DateTime = DateTime.Today
        Dim startDay As DateTime
        If today.Day >= CONTRACT_DATE Then
            ' 当日が約定日以降の場合、同じ月の13日を起点とする
            startDay = New DateTime(today.Year, today.Month, CONTRACT_DATE)
        Else
            ' 当日が約定日より前の場合、先月の13日を起点とする
            Dim lastMonth As DateTime = today.AddMonths(-1)
            startDay = New DateTime(lastMonth.Year, lastMonth.Month, CONTRACT_DATE)
        End If

        Dim daysBetween As TimeSpan = today - startDay
        NUD_AC_DAYS.Value = daysBetween.Days
    End Sub

    ' ボーナス対象月の判定
    Function IsBonusMonth(bonus_Month As String, today_Month As String) As Boolean
        If bonus_Month.Length = 0 Then Return False

        Dim formattedTodayMonth As String = today_Month.PadLeft(2, "0"c)        ' today_Monthを比較するため2桁に変換
        Dim firstBonusMonth As String = bonus_Month.Substring(0, 2)
        Dim secondBonusMonth As String = bonus_Month.Substring(2, 2)

        Return formattedTodayMonth = firstBonusMonth Or formattedTodayMonth = secondBonusMonth
    End Function


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class