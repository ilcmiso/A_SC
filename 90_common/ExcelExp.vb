Public Class ExcelExp

    ' インスタンス生成
    'Private WithEvents CreatorExpress1 As New AdvanceSoftware.ExcelCreator.Xlsx.CreatorExpress
    Private WithEvents CreatorExpress1 As New AdvanceSoftware.ExcelCreator.Creator
    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private ReadOnly db As Sqldb

    Public Sub New()
        db = SCA1.db
    End Sub



    ' ----- 共通
    Public Function Eopen(filePath As String, outFilePath As String) As Boolean
        ' オーバーレイオープン
        If Not IO.File.Exists(filePath) Then
            MsgBox("帳票のフォーマットファイルが見つかりません。" & vbCrLf & filePath)
            Return False
        End If
        CreatorExpress1.OpenBook(outFilePath, filePath)
        Return True
    End Function

    Public Function Eclose() As Boolean
        CreatorExpress1.InitFormulaAnswer = True        ' 計算式の初期化
        CreatorExpress1.CloseBook(True)
        Return True
    End Function


    ' 交渉記録一覧出力(単体)
    Public Sub OutRec(oPath As String, cid As String)
        Const CLIST_H As Integer = 4
        Const RECORD_H As Integer = 4
        Dim fPath As String = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT3 & Common.FILE_EXCREC
        If oPath = "" Then Exit Sub
        'Dim db As Sqldb = SCA1.db

        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCD).Select(String.Format("FKD02 = '{0}'", cid))
        If dr.Length = 0 Then
            MsgBox("交渉記録が見つかりません。")
            Exit Sub
        End If

        Dim ret As Boolean
        ret = Eopen(fPath, oPath)
        If Not ret Then Exit Sub

        Dim sDr As DataRow() = db.OrgDataTable(Sqldb.TID.SC).Select(String.Format("FK02 = '{0}'", cid))     ' 連帯債務者名取得
        Dim asDr As DataRow() = db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C02 = '{0}'", cid))   ' 証券番号取得

        Dim cNum As Integer = 0
        Dim sid As String = ""
        Dim cName As String = cmn.FixSPName(dr(0)(8))
        Dim sName As String = ""
        If sDr.Length > 0 Then sName = cmn.FixSPName(sDr(0)(29))
        If asDr.Length > 0 Then sid = asDr(0)(11)

        ' 一覧シート
        CreatorExpress1.SheetNo = 0
        CreatorExpress1.Pos(4, cNum + CLIST_H).Value = cNum + 1                 ' No
        CreatorExpress1.Pos(5, cNum + CLIST_H).Value = cid                      ' 顧客番号
        CreatorExpress1.Pos(6, cNum + CLIST_H).Value = sid                      ' 証券番号
        CreatorExpress1.Pos(7, cNum + CLIST_H).Value = cName                    ' 主債務者名
        CreatorExpress1.Pos(8, cNum + CLIST_H).Value = sName                    ' 連帯債務者名
        CreatorExpress1.Pos(9, cNum + CLIST_H).Value = String.Format("=COUNTA('{0}'!A5:A1000)", "S" & cNum + 1)
        CreatorExpress1.Pos(10, cNum + CLIST_H).Value = String.Format("=HYPERLINK(""#{0}!A1"",""リンク {0}"")", "S" & cNum + 1)

        ' 顧客固有シート
        CreatorExpress1.SheetNo = cNum + 1
        CreatorExpress1.Pos(1, 0).Value = cid                 ' 顧客番号
        CreatorExpress1.Pos(1, 1).Value = cName               ' 主債務者名
        CreatorExpress1.Pos(6, 0).Value = sid                 ' 証券番号
        CreatorExpress1.Pos(6, 1).Value = sName               ' 連帯債務者名

        For rNum = 0 To dr.Length - 1
            CreatorExpress1.Pos(0, rNum + RECORD_H).Value = dr(rNum)(2)
            CreatorExpress1.Pos(1, rNum + RECORD_H).Value = dr(rNum)(3)
            CreatorExpress1.Pos(2, rNum + RECORD_H).Value = dr(rNum)(10)
            CreatorExpress1.Pos(3, rNum + RECORD_H).Value = dr(rNum)(4)
            CreatorExpress1.Pos(4, rNum + RECORD_H).Value = dr(rNum)(12)
            CreatorExpress1.Pos(5, rNum + RECORD_H).Value = dr(rNum)(5)
            CreatorExpress1.Pos(6, rNum + RECORD_H).Value = dr(rNum)(6)
        Next

        Eclose()
        Process.Start(oPath)
    End Sub

    ' 交渉記録一覧出力(期間)
    Public Sub OutRec(oPath As String)
        Dim SheetMAX As Integer = 10000
        Const CLIST_H As Integer = 4
        Const RECORD_H As Integer = 4
        Dim fPath As String = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT3 & Common.FILE_EXCREC
        If oPath = "" Then Exit Sub
        'Dim db As Sqldb = SCA1.db

        Dim s2 As SCE_S2 = SCA1.EditForm

        ' 交渉記録の日付が日付形式ではないものは、日付を変更しておく。
        ' 異常な日付が存在していると、DataTable.Selectの日付比較でエラーが発生してしまうため。
        Dim fixDB As Boolean = False      ' dbの日付を一時的に更新したフラグ。あとでdbを元に戻す
        For x = 0 To db.OrgDataTable(Sqldb.TID.SCD).Rows.Count - 1
            If Not DateTime.TryParse(db.OrgDataTable(Sqldb.TID.SCD).Rows(x)(2), Nothing) Then
                db.OrgDataTable(Sqldb.TID.SCD).Rows(x).Item(2) = "2999/12/31"
                fixDB = True
            End If
        Next

        Dim stDate As String = s2.DTP_T4_A.Value.ToString("yyyy/MM/dd 00:00")
        Dim edDate As String = s2.DTP_T4_B.Value.ToString("yyyy/MM/dd 23:59")
        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCD).Select(String.Format("FKD03 <> '' And FKD03 >= #{0}# And FKD03 <= #{1}#", stDate, edDate), "FKD02, FKD03")
        If dr.Length = 0 Then
            MsgBox("指定期間の交渉記録が見つかりません。")
            Exit Sub
        End If

        Dim ret As Boolean
        ret = Eopen(fPath, oPath)
        If Not ret Then Exit Sub

        CreatorExpress1.Pos(1, 4).Value = stDate
        CreatorExpress1.Pos(1, 5).Value = edDate

        Dim cid As String
        Dim sid As String
        Dim cName As String
        Dim sName As String
        Dim lastCid As String = ""      ' 前回の顧客番号(比較用)

        Dim cNum As Integer = 0     ' 顧客一覧に表示する顧客の数
        Dim rNum As Integer = 0     ' 固有シートの記録の数
        For idx = 0 To dr.Length - 1

            ' 値の設定
            cid = dr(idx)(1)
            cName = cmn.FixSPName(dr(idx)(8))
            sName = ""
            sid = ""
            Dim sDr As DataRow() = db.OrgDataTable(Sqldb.TID.SC).Select(String.Format("FK02 = '{0}'", cid))     ' 連帯債務者名取得
            Dim asDr As DataRow() = db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C02 = '{0}'", cid))   ' 証券番号取得

            ' 「アシストのみ」チェックがONならアシスト加入者に存在する人だけを抽出する
            If s2.CB_T4_ASS.Checked Then
                If asDr.Length = 0 Then Continue For
                If asDr(0)(11)(0) <> "5" Then Continue For   ' 債権番号が5はじまりじゃないものは除く  新保証型(6はじまり)等
            End If

            If sDr.Length > 0 Then sName = cmn.FixSPName(sDr(0)(29))
            If asDr.Length > 0 Then sid = asDr(0)(11)

            If cid = lastCid Then
                ' 連続で同じ顧客番号なので、一覧に追加せず、同じ固有シートに追記する

                ' 顧客固有シート
                'CreatorExpress1.SheetNo = cNum + 1
                CreatorExpress1.Pos(1, 0).Value = cid                 ' 顧客番号
                CreatorExpress1.Pos(1, 1).Value = cName               ' 主債務者名
                CreatorExpress1.Pos(6, 0).Value = sid                 ' 証券番号
                CreatorExpress1.Pos(6, 1).Value = sName               ' 連帯債務者名

                CreatorExpress1.Pos(0, rNum + RECORD_H).Value = dr(idx)(2)
                CreatorExpress1.Pos(1, rNum + RECORD_H).Value = dr(idx)(3)
                CreatorExpress1.Pos(2, rNum + RECORD_H).Value = dr(idx)(10)
                CreatorExpress1.Pos(3, rNum + RECORD_H).Value = dr(idx)(4)
                CreatorExpress1.Pos(4, rNum + RECORD_H).Value = dr(idx)(12)
                CreatorExpress1.Pos(5, rNum + RECORD_H).Value = dr(idx)(5)
                CreatorExpress1.Pos(6, rNum + RECORD_H).Value = dr(idx)(6)
                rNum += 1
            Else
                ' シート数が1万を超えたら新しいExcelファイルを作成
                If cNum >= SheetMAX Then
                    Eclose()   ' 現在のExcelファイルを閉じる
                    oPath = $"{IO.Path.GetDirectoryName(oPath)}\{IO.Path.GetFileNameWithoutExtension(oPath)}_.xlsx"
                    ret = Eopen(fPath, oPath)   ' 新しいExcelファイルを開く
                    CreatorExpress1.DeleteSheet(cNum + 1, 1)
                    cNum = 0
                    If Not ret Then Exit Sub
                End If

                ' 顧客番号の１件目の交渉記録なので、一覧シートをコピーして、固有シートを追加
                CreatorExpress1.CopySheet(cNum + 1, cNum + 1, "S" & cNum + 1)    ' シート追加
                rNum = 0

                ' 一覧シート
                CreatorExpress1.SheetNo = 0
                CreatorExpress1.Pos(4, cNum + CLIST_H).Value = cNum + 1                 ' No
                CreatorExpress1.Pos(5, cNum + CLIST_H).Value = cid                      ' 顧客番号
                CreatorExpress1.Pos(6, cNum + CLIST_H).Value = sid                      ' 証券番号
                CreatorExpress1.Pos(7, cNum + CLIST_H).Value = cName                    ' 主債務者名
                CreatorExpress1.Pos(8, cNum + CLIST_H).Value = sName                    ' 連帯債務者名
                CreatorExpress1.Pos(9, cNum + CLIST_H).Value = String.Format("=COUNTA('{0}'!A5:A1000)", "S" & cNum + 1)
                CreatorExpress1.Pos(10, cNum + CLIST_H).Value = String.Format("=HYPERLINK(""#{0}!A1"",""リンク {0}"")", "S" & cNum + 1)

                cNum += 1

                ' 顧客固有シート
                CreatorExpress1.SheetNo = cNum
                CreatorExpress1.Pos(1, 0).Value = cid                 ' 顧客番号
                CreatorExpress1.Pos(1, 1).Value = cName               ' 主債務者名
                CreatorExpress1.Pos(6, 0).Value = sid                 ' 証券番号
                CreatorExpress1.Pos(6, 1).Value = sName               ' 連帯債務者名

                CreatorExpress1.Pos(0, rNum + RECORD_H).Value = dr(idx)(2)
                CreatorExpress1.Pos(1, rNum + RECORD_H).Value = dr(idx)(3)
                CreatorExpress1.Pos(2, rNum + RECORD_H).Value = dr(idx)(10)
                CreatorExpress1.Pos(3, rNum + RECORD_H).Value = dr(idx)(4)
                CreatorExpress1.Pos(4, rNum + RECORD_H).Value = dr(idx)(12)
                CreatorExpress1.Pos(5, rNum + RECORD_H).Value = dr(idx)(5)
                CreatorExpress1.Pos(6, rNum + RECORD_H).Value = dr(idx)(6)
                rNum += 1
            End If

            lastCid = cid
        Next
        ' コピー用のフォーマットシート(S0シート)を削除
        CreatorExpress1.DeleteSheet(cNum + 1, 1)

        Eclose()
        Process.Start(oPath)
        If fixDB Then db.UpdateOrigDT(Sqldb.TID.SCD)            ' 日付を修正したDBを元に戻す
    End Sub
End Class
