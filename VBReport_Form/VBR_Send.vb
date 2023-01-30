Imports System.IO.Path
Public Class VBR_Send

    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        log.TimerST()

        'ViewerControl1.Clear()
        Dim ownerForm As SCE_S2 = DirectCast(Me.Owner, SCE_S2)            ' 親フォームを参照できるようにキャスト
        Dim fName As String = ownerForm.LB_SendFMTList.SelectedItem
        Dim outfPath As String = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCOUT1

        ' 出力先ディレクトリ作成
        cmn.CreateDir(outfPath)

        If ownerForm.TB_A1.Text.Length < 10 Then
            MsgBox("交渉記録の日時欄がおかしいです。" & vbCrLf & "10文字以上にしてやり直してください。")
            Me.Close()
            Exit Sub
        End If
        CellReport1.FileName = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT1 & fName
        CellReport1.ScaleMode = AdvanceSoftware.VBReport8.ScaleMode.Pixel
        'CellReport1.ApplyFormula = True

        ' 帳票手動指定ボタンを押された場合そのファイルパスに変更する
        If ownerForm.PrinfFileName IsNot Nothing Then
            fName = GetFileName(ownerForm.PrinfFileName)
            CellReport1.FileName = ownerForm.PrinfFileName
        End If


        If ownerForm.DGV_S2.Rows.Count > 1 Then
            ' 顧客複数選択
            For n = 0 To ownerForm.DGV_S2.Rows.Count - 1
                CellReport1.Report.Start()
                CellReport1.Report.File()
                CellReport1.Page.Start("Sheet1", "1")
                WriteExcel(ownerForm.DGV_S2.Rows(n).Cells(0).Value)
                CellReport1.Page.End()
                CellReport1.Report.End()
                CellReport1.Report.SaveAs(String.Format("{0}{1}_{2}_{3}",
                                                        outfPath,
                                                        DateTime.Parse(ownerForm.TB_A1.Text).ToString("yyyyMMdd"),
                                                        cmn.RegReplace(ownerForm.DGV_S2.Rows(n).Cells(1).Value, "　| ", ""), fName),
                                          AdvanceSoftware.VBReport8.ExcelVersion.ver2016)
            Next
        Else
            ' 顧客単体指定
            CellReport1.Report.Start()
            CellReport1.Report.File()
            CellReport1.Page.Start("Sheet1", "1")
            WriteExcel(SCA1.DGV1.CurrentRow.Cells(0).Value)
            CellReport1.Page.End()
            CellReport1.Report.End()
            CellReport1.Report.SaveAs(String.Format("{0}{1}_{2}_{3}",
                                                    outfPath,
                                                    DateTime.Parse(ownerForm.TB_A1.Text).ToString("yyyyMMdd"),
                                                    cmn.RegReplace(SCA1.DGV1.CurrentRow.Cells(1).Value, "　| ", ""), fName),
                                      AdvanceSoftware.VBReport8.ExcelVersion.ver2016)
        End If
        Process.Start(outfPath)
        log.timerED("Exce出力")
        Me.Close()
    End Sub

    ' 顧客データをExcelに出力
    Private Sub WriteExcel(cid As String)
        Dim ownerForm As SCE_S2 = DirectCast(Me.Owner, SCE_S2)            ' 親フォームを参照できるようにキャスト
        Dim idxList As Integer() = {    ' 顧客情報を出力インデックス番号リスト　 FKSCデータ構造　FKS.FKSC_R.FK01 ～ FK70
             1, 3, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 48, 49, 50, 51, 52, 53, 54, 55, 56, 59, 60, 61, 62
        }
        Dim itemList As String() = {
            ownerForm.DTP_Multi1.Text,                                       ' A37 汎用日付1
            ownerForm.DTP_Multi2.Text,                                       ' A38 汎用日付2
            ownerForm.DTP_A1.Text,                                           ' A39 督促通知日
            ownerForm.TB_A4.Text,                                            ' A40 担当者名
            ownerForm.TB_A7.Text,                                            ' A41 対応者名
            "",                                                              ' A42 
            cmn.JpTimeString(ownerForm.DTP_Multi1.Value, "ggy年MMM月d日"),   ' A43 汎用日付1の月
            cmn.JpTimeString(ownerForm.DTP_Multi2.Value, "ggy年MMM月d日"),   ' A44 汎用日付1の月
            ownerForm.DTP_Multi1.Value.ToString("MMM"),                      ' A45 汎用日付1の月
            ownerForm.DTP_Multi2.Value.ToString("MMM"),                      ' A46 汎用日付2の月
            ownerForm.TB_A1.Text.Substring(0, 10),                           ' A47 交渉記録日
            ownerForm.TB_A9.Text,                                            ' A48 送付先郵便番号 
            ownerForm.TB_A10.Text,                                           ' A49 送付先住所
            ownerForm.TB_A11.Text                                            ' A50 送付先名前
        }

        ' 顧客情報取得 見つからなかったら終了
        If cid <> Common.DUMMY_NO Then
            Dim dr As DataRow() = SCA1.db.OrgDataTablePlusAssist.Select("[FK02] = '" & cid & "'")
            If dr.Length = 0 Then Exit Sub
            Dim dt As DataTable = dr.CopyToDataTable

            ' 顧客情報をエクセルA1セルから下方向に順番に書き込み
            For n = 0 To idxList.Length - 1
                CellReport1.Pos(0, n).Value = dt.Rows(0).Item(idxList(n))   ' 顧客情報
            Next
        End If

        ' その他書き込み
        For n = 0 To itemList.Length - 1
            CellReport1.Pos(0, idxList.Length + n).Value = itemList(n)
        Next
    End Sub

End Class