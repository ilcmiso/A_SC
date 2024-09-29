Public Class VBR_Dun

    Private ReadOnly cmn As New Common
    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If SCA1.DGV6.Rows.Count > 100 Then
            Dim r = MessageBox.Show("印刷データが " & SCA1.DGV6.Rows.Count & " 件あるため時間がかかります。よろしいですか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
            If r = vbNo Then Exit Sub
        End If

        ViewerControl1.Clear()
        CellReport1.FileName = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT2
        cmn.CreateDir(CellReport1.FileName)
        Select Case SCA1.CB_DunA6.SelectedIndex
            Case 0 : CellReport1.FileName += "SC02_督促状.xlsx"
            Case 1 : CellReport1.FileName += "SC03_延滞督促状.xlsx"
            Case 2 : CellReport1.FileName += "SC04_延滞督促状_引落日無し.xlsx"
            Case 3 : CellReport1.FileName += "SC05_翌月不能通知.xlsx"
            Case 4 : CellReport1.FileName += "SC06_SPﾚﾀｰ.xlsx"
            Case 5 : CellReport1.FileName += "SC07_アシスト督促状1.xlsx"
            Case 6 : CellReport1.FileName += "SC08_アシスト督促状2.xlsx"
            Case 7 : CellReport1.FileName += "SC09_アシストSPレター.xlsx"
        End Select

        Dim sendNGListMain As New HashSet(Of String)(SCA1.db.GetSelect(Sqldb.TID.SCR, "SELECT FKR01 FROM FKSCREM WHERE FKR04 = '1' OR FKR04 = '3'").
                                AsEnumerable().
                                Select(Function(row) row.Field(Of String)("FKR01")))

        Dim sendNGListSub As New HashSet(Of String)(SCA1.db.GetSelect(Sqldb.TID.SCR, "SELECT FKR01 FROM FKSCREM WHERE FKR04 = '2' OR FKR04 = '3'").
                                AsEnumerable().
                                Select(Function(row) row.Field(Of String)("FKR01")))

        CellReport1.ScaleMode = AdvanceSoftware.VBReport8.ScaleMode.Pixel
        CellReport1.ApplyFormula = True
        CellReport1.Report.Start()
        CellReport1.Report.File()

        For n = 0 To SCA1.DGV6.Rows.Count - 1
            ' 主債務者　連帯債務者
            For deb = 0 To 1
                Dim cid As String = SCA1.DGV6.Rows(n).Cells(0).Value
                Dim contract As String = "ﾌﾗｯﾄ35"
                Dim am As Integer                                    ' 督促金額 ＝ 返済額1＋返済額2＋（B返済額）　※B返済額は、B返済月の場合のみ

                ' 督促状送付NGリストの顧客番号と一致した場合は印刷しない
                If deb = 0 And sendNGListMain.Contains(cid) Then Continue For
                If deb = 1 And sendNGListSub.Contains(cid) Then Continue For

                am = SCA1.DGV6.Rows(n).Cells(5).Value
                If SCA1.DGV6.Rows(n).Cells(6).Value <> "" Then am += SCA1.DGV6.Rows(n).Cells(6).Value           ' 返済額1 + 2
                If ChkBonus(SCA1.DGV6.Rows(n).Cells(10).Value) Then am += SCA1.DGV6.Rows(n).Cells(9).Value      ' + B返済額

                Dim dt As DataTable = SCA1.db.OrgDataTablePlusAssist.Select("[FK02] = '" & cid & "'").CopyToDataTable
                If deb = 1 And dt.Rows(0).Item(29) = "" Then Continue For                   ' 連帯債務者がいない場合は連帯者処理スキップ
                If SCA1.CB_DunA7.SelectedIndex = 0 Then                                             ' 同住所の印刷部数が「纏める」なら
                    If deb = 1 And dt.Rows(0).Item(16) = dt.Rows(0).Item(36) Then Continue For      ' 主債務者と同じ住所なら連帯債務者はスキップ
                End If

                ' 主債務者名、連帯債務者名の特殊文字の置換
                Dim chkWords_b() As String = {"", "", "", ""}
                Dim chkWords_a() As String = {"髙", "𠮷", "釼", "祐"}
                For cn = 0 To chkWords_a.Length - 1
                    If dt.Rows(0).Item(9).ToString.IndexOf(chkWords_b(cn)) >= 0 Then dt.Rows(0).Item(9) = dt.Rows(0).Item(9).ToString.Replace(chkWords_b(cn), chkWords_a(cn))
                    If dt.Rows(0).Item(29).ToString.IndexOf(chkWords_b(cn)) >= 0 Then dt.Rows(0).Item(29) = dt.Rows(0).Item(29).ToString.Replace(chkWords_b(cn), chkWords_a(cn))
                Next

                CellReport1.Page.Start("Sheet1", "1")

                If SCA1.CB_DunA7.SelectedIndex = 0 And
                   dt.Rows(0).Item(16) = dt.Rows(0).Item(36) Then
                    ' 主債務者＋連帯債務者    同じ住所は両名印刷
                    CellReport1.Cell("A1").Value = "〒" & dt.Rows(0).Item(15)                           ' 郵便番号
                    CellReport1.Cell("A2").Value = dt.Rows(0).Item(16)                                  ' 住所1
                    CellReport1.Cell("A3").Value = ""                                                   ' 住所2

                    If sendNGListMain.Contains(cid) Then
                        CellReport1.Cell("B1").Value = dt.Rows(0).Item(9) & "　様"                          ' 債務者名
                    End If
                    If sendNGListSub.Contains(cid) Then
                        If dt.Rows(0).Item(29) <> "" Then
                            CellReport1.Cell("B2").Value = dt.Rows(0).Item(29) & "　様"                     ' 連帯債務者名
                        End If
                    End If
                ElseIf deb = 0 Then
                    ' 主債務者 単体
                    CellReport1.Cell("A1").Value = "〒" & dt.Rows(0).Item(15)                           ' 郵便番号
                        CellReport1.Cell("A2").Value = dt.Rows(0).Item(16)                                  ' 住所1
                        CellReport1.Cell("A3").Value = ""                                                   ' 住所2

                        CellReport1.Cell("B1").Value = dt.Rows(0).Item(9) & "　様"                          ' 債務者名
                    ElseIf deb = 1 Then
                        ' 連帯債務者 単体
                        CellReport1.Cell("A1").Value = "〒" & dt.Rows(0).Item(35)                           ' 郵便番号
                    CellReport1.Cell("A2").Value = dt.Rows(0).Item(36)                                  ' 住所1
                    CellReport1.Cell("A3").Value = ""                                                   ' 住所2

                    CellReport1.Cell("B1").Value = dt.Rows(0).Item(29) & "　様"                         ' 連帯債務者名
                End If

                CellReport1.Cell("B3").Value = SCA1.DTP_DunA1.Value.ToString("yyyy年M月d日")                         ' 督促通知日
                CellReport1.Cell("C1").Value = contract                                                              ' 契約名(ﾌﾗｯﾄ35)
                CellReport1.Cell("C2").Value = SCA1.NUD_DunA1.Value                                                  ' 未納月
                CellReport1.Cell("C3").Value = SCA1.DTP_DunA2.Value.ToString("yyyy年M月d日(ddd)")                    ' 期限日
                CellReport1.Cell("C4").Value = StrConv(String.Format("\{0:#,0}円を", am), vbWide)                    ' 金額
                CellReport1.Cell("C5").Value = SCA1.DTP_DunA3.Value.ToString("yyyy年M月d日(ddd)")                    ' 口座引落期限日
                CellReport1.Cell("C6").Value = SCA1.DTP_DunA1.Value.AddMonths(1).ToString("MMM")                     ' 未納月の翌月
                CellReport1.Cell("C7").Value = SCA1.DTP_DunA3.Value.ToString("yyyy年M月")                            ' 振替再開月
                CellReport1.Cell("C8").Value = SCA1.DTP_DunA4.Value.ToString("yyyy年M月d日(ddd)")                    ' 補助欄

                CellReport1.Page.End()
            Next
        Next
        CellReport1.Report.End()
        ViewerControl1.Document = CellReport1.Document
    End Sub

    ' 当月がボーナス月か判定 IN: 4桁文字列(ex0208)   RET:True=B月 
    Private Function ChkBonus(month As String) As Boolean
        If month.Length <> 4 Then Return False
        If cmn.RegReplace(month, "[0-9]", "").Length > 0 Then Return False ' 数字以外が含まれていたらB月ではないと判定する
        If month.Substring(0, 2) <> SCA1.NUD_DunA1.Value.ToString("00") And month.Substring(2, 2) <> SCA1.NUD_DunA1.Value.ToString("00") Then Return False
        Return True
    End Function
End Class