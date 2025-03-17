Imports System.IO
Imports System.Text
Imports System.Threading
Imports A_SC.SCcommon

Public Class SCA1

#Region "定義"
    Public addSW                                    ' 追加/編集ボタンの識別子(SCA1_S2で判別)
    Private FPPageNo As Integer                     ' 物件情報のページ数
    Private ReadOnly cmn As New Common
    Private ReadOnly sccmn As New SCcommon
    Private ReadOnly log As New Log
    Public ReadOnly db As New Sqldb
    Private oview As SCGA_OVIEW
    Public xml As New XmlMng
    Public MaxCosCount As Integer = 0               ' 顧客最大数(DB読み込み時)

    ' 監視
    Private PoolingStart As Boolean = False                         ' 監視フラグ
    Private Const POLLING_ID_SCD As String = "SCD更新"              ' DB(SCD)更新イベントの識別子
    Private Const POLLING_ID_FP As String = "物件情報更新"
    Private Const POLLING_ID_MR As String = "申請物管理更新"
    Private Const POLLING_ID_CUDB As String = "顧客DB更新"          ' 顧客DB更新イベントの識別子
    Private Const POLLING_ID_ASC As String = "A_SC本体"             ' A_SC更新イベントの識別子
    Private Const POLLING_CYCLE As Integer = 500                    ' イベント監視周期(ms)
    ' 監視 new
    Private fwatchers As List(Of FileWatcher)
    ' フリーメモ変更前バッファ(変更されたか検知したい)
    Private BeforeFreeTxt As String = ""
    ' 外付けフォーム
    Public AddTelForm As SCA1_S4_TEL = Nothing                      ' 電話追加フォーム
    Public EditForm As SCE_S2 = Nothing                           ' 交渉記録フォーム
    ' イベントハンドラーロック 記録一覧チェックリストボックス
    Private LockEventHandler_CLB As Boolean = False
    Private LockEventHandler_FP As Boolean = False
    ' スレッド
    Private ReadOnly Thread_Entry As Thread = Nothing
    ' デリゲート
    Delegate Sub delegate_PoolingCallBack(id As String)         ' UIコールバック用Delegate宣言
    ' ChangeTrackingUpdater のインスタンス（フィールドとして保持）
    Private changeTracker As ChangeTrackingUpdater
    ' 選択中顧客番号
    Public Shared CurrentCID As String


#End Region

#Region " OPEN CLOSE "

    Private Sub FK4B_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim loadTime = Date.Now
        Me.Text += " - " & xml.GetCPath
        CB_AUTOUPD.Checked = xml.GetAutoUpd

        ' 顧客情報DB(FKSC.DB3,ASSIST.DB3)の最新確認 古ければサーバからダウンロード
        CheckUpdateCDB()

        ' 追加電話番号の吹き出しフォーム初期設定
        InitForms()

        cmn.StartPBar([Enum].GetValues(GetType(Sqldb.TID)).Length + 6)
        cmn.UpdPBar("顧客情報のファイルダウンロード中")
        ' DGV表示設定
        db.DBFileDL(Sqldb.TID.SCD)
        db.UpdateOrigDT()
        db.UpdateOrigDT_ASsist()

        ' DGV1の初期設定
        InitDGV1()

        ShowDGVList(DGV1)
        ShowDGVList(DGV2)
        ShowDGVList(DGV9)
        FPINFOInit()                ' 物件情報管理の初期設定
        cmn.UpdPBar("最終設定中")

        CheckedListBoxInit()        ' タブ 記録一覧の初期設定
        SetToolTips()               ' ツールチップの設定
        MRInit()                    ' 申請物の初期設定
        DunInit()                   ' 督促管理の初期設定
        ' DGVちらつき防止
        cmn.SetDoubleBufferDGV(DGV1, DGV2, DGV4, DGV5, DGV6, DGV_FPLIST, DGV9, DGV_MR1, DGV_FPMNG)
        ' ファイル監視開始 ※ファイル監視によるDB更新を行うと排他競合がおこりアプリが強制終了してしまう問題があるので一旦行わない
        'fwatchers = New List(Of FileWatcher)
        'FWatchingStart()

        ' SQL Server 変更監視を開始
        If xml.GetDBSwitch Then
            ' DataGridView と変換関数のマッピングを作成
            ' ChangeTrackingUpdater を作成して監視開始
            Dim dgvMappings As New Dictionary(Of DataGridView, Func(Of DataTable, DataTable))()
            dgvMappings.Add(DGV2, AddressOf TransformForDGV2)
            dgvMappings.Add(DGV5, AddressOf TransformForDGV5)

            changeTracker = New ChangeTrackingUpdater(dgvMappings)
            changeTracker.StartMonitoring()
        End If
        CurrentCID = DGV1.CurrentRow.Cells(0).Value
        log.cLog($"CurrentCID:{CurrentCID}")

        cmn.EndPBar()
        log.cLog("--- Load完了: " & (Date.Now - loadTime).ToString("ss\.fff"))
    End Sub

    Private Sub SCA1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        ' 着信ログ、DBファイル監視開始
        StartWatching()

        ' イベントハンドラ設定
        AddHandler DGV1.CellEnter, AddressOf DGV1_CellEnter
        SC.Visible = False
    End Sub

    ' 監視 new 
    Private Sub FWatchingStart()
        Dim fileSettings As List(Of FileSetting) = New List(Of FileSetting) From {
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_DB3}{Sqldb.DB_FKSCLOG}", AddressOf CBWatcherUpdSCD),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_UPD}{Sqldb.DB_FKSC}", AddressOf CBWatcherUpdFKSC),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_UPD}{Sqldb.DB_FKSCPI}", AddressOf CBWatcherUpdFKSC),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_UPD}{Sqldb.DB_FKSCFPI}", AddressOf CBWatcherUpdFKSC),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_UPD}{Common.EXE_NAME}", AddressOf CBWatcherUpdEXE),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_DB3}{Sqldb.DB_MNGREQ}", AddressOf CBWatcherUpdMR),
            New FileSetting($"{cmn.CurrentPath}{Common.DIR_DB3}{Sqldb.DB_MRITEM}", AddressOf CBWatcherUpdMRM)
        }

        For Each setting In fileSettings
            Dim fwatch As New FileWatcher
            fwatch.StartWatching(setting.FilePath, setting.Callback)
            log.cLog($" file watching: {setting.FilePath}")
            fwatchers.Add(fwatch)
        Next
    End Sub

    Private Sub FWatchingEnd()
        For Each w In fwatchers
            w.StopWatching()
        Next
    End Sub

    ' コールバックIF
    Sub CBWatcherUpdSCD()
        If Me.InvokeRequired Then
            ' 非メインスレッド処理
            log.cLog("[CB][Watcher] Upd SCD")
            db.DBFileFDL(Sqldb.TID.SCD)                     ' ファイル強制ダウンロード
            db.UpdateOrigDT(Sqldb.TID.SCD)
            db.UpdateOrigDT(Sqldb.TID.SCR)
            Me.Invoke(New MethodInvoker(AddressOf CBWatcherUpdSCD))
        Else
            ' メインスレッド処理
            If CB_AUTOUPD.Checked Then
                cmn.StartPBar(3)
                cmn.UpdPBar("交渉記録の更新がありました")
                ShowDGVList(DGV2)
                ShowDGVList(DGV5)
                ShowDunLB()
                cmn.EndPBar()
            End If
        End If
    End Sub

    Sub CBWatcherUpdFKSC()
        If Me.InvokeRequired Then
            ' 非メインスレッド処理
            log.cLog("[CB][Watcher] Upd FKSC")
            Me.Invoke(New MethodInvoker(AddressOf CBWatcherUpdFKSC))
        Else
            ' メインスレッド処理
            L_UPDMsg.Visible = True
        End If
    End Sub

    Sub CBWatcherUpdFPI()
        log.cLog("[CB][Watcher] Upd FPI")
        db.UpdateOrigDT(Sqldb.TID.FPCOS)
    End Sub

    Sub CBWatcherUpdEXE()
        If Me.InvokeRequired Then
            ' 非メインスレッド処理
            log.cLog("[CB][Watcher] Upd EXE")
            Me.Invoke(New MethodInvoker(AddressOf CBWatcherUpdEXE))
        Else
            ' メインスレッド処理
            L_UPDMsg.Visible = True
        End If
    End Sub

    Sub CBWatcherUpdMR()
        If Me.InvokeRequired Then
            ' 非メインスレッド処理
            log.cLog("[CB][Watcher] Upd MR")
            db.UpdateOrigDT(Sqldb.TID.MR)
            Me.Invoke(New MethodInvoker(AddressOf CBWatcherUpdMR))
        Else
            ' メインスレッド処理
            oview.ShowOVIEW()
            ShowDGVMR()
        End If
    End Sub

    Private Const Msg As String = "[CB][Watcher] Upd MRM"

    Sub CBWatcherUpdMRM()
        log.cLog(Msg)
        db.UpdateOrigDT(Sqldb.TID.MRM)
    End Sub


    Private Sub FLS_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        'FWatchingEnd()
        'thSC.Dispose()
        PoolingStart = False
        xml.SetAutoUpd(CB_AUTOUPD.Checked)

        If changeTracker IsNot Nothing Then
            changeTracker.StopMonitoring()
            changeTracker = Nothing
        End If
    End Sub

#End Region

#Region "SQL Server Event"

    ' FKSCD データをそのまま適用する場合
    Function TransformForDGV2(ByVal dt As DataTable) As DataTable
        Return dt ' 変換なし
    End Function

    ' FKSCD データをDGV5用に変換
    Private Function TransformForDGV5(ByVal dt As DataTable) As DataTable
        ' DGV5 用にカスタムな変換処理を実装する例
        Dim resultTable As New DataTable()
        ' 例: 必要な列のみを抽出する、または列名を変換するなど
        For Each col As DataColumn In dt.Columns
            resultTable.Columns.Add(col.ColumnName, col.DataType)
        Next
        For Each row As DataRow In dt.Rows
            resultTable.ImportRow(row)
        Next
        Return resultTable
    End Function
#End Region

#Region " UIイベント"
    ' DGV選択時に融資情報を表示
    Private Sub DGV1_CellEnter(sender As Object, e As DataGridViewCellEventArgs) ' Handles DGV1.CellEnter
        Static Dim lastIdx As Integer = -1
        ' DGV範囲外クリックの場合はイベント不要
        If e.RowIndex < 0 Then Exit Sub
        If e.RowIndex = lastIdx Then Exit Sub
        lastIdx = e.RowIndex        ' 最後に選択した位置を保存
        CurrentCID = DGV1.CurrentRow.Cells(0).Value
        log.cLog($"CurrentCID:{CurrentCID}")
        DGV1_ClickShow()
    End Sub
    ' DGV選択時の表示
    Private Sub DGV1_ClickShow()
        ShowDGVList(DGV2)                               ' 交渉記録
        ShowDGV_FPLIST()                                ' 物件情報
        ShowDGVList(DGV9)                               ' 顧客詳細情報

        ShowAssignee()                                  ' 物件情報の受任者マークの表示設定
        ShowSendNGLabel()
    End Sub
    ' DGV2選択時に記録情報を表示
    Private Sub DGV2_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DGV2.CellEnter
        If e.RowIndex < 0 Then Exit Sub             ' DGV範囲外クリックの場合はイベント不要
        TB_Remarks.Text = DGV2.CurrentRow.Cells(8).Value ' 備考内容表示
    End Sub

    ' 追加ボタン 編集ボタン
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles BT_B2.Click, BT_B1.Click
        If DGV1.RowCount = 0 Then Exit Sub

        ' 編集ボタンが押下された場合
        If sender.Name = "BT_B2" Then
            If DGV2.RowCount = 0 Then Exit Sub
            addSW = False
        Else
            ' 追加ボタンが押下された場合
            addSW = True        ' True = 追加 SCA1_S2で判断
        End If
        ' DontUpdate = True
        ' 追加フォーム表示
        If EditForm Is Nothing Then EditForm = New SCE_S2()        ' 初回のインスタンス生成
        If EditForm.IsDisposed Then EditForm = New SCE_S2()        ' 2回目以降のフォーム破棄後の再生成(上処理がないとNothing参照でエラー)
        EditForm.ShowInTaskbar = False
        If Not EditForm.Visible Then EditForm.Show(Me)              ' 多重フォーム防止
    End Sub

    ' 削除ボタン
    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles BT_B3.Click
        If DGV2.Rows.Count = 0 Then Exit Sub
        Dim id As String = DGV2.CurrentRow.Cells(0).Value
        If DGV2.RowCount = 0 Then Exit Sub
        If EditForm IsNot Nothing Then
            If EditForm.Visible Then
                ' 編集中も削除して平気だと思うけど念の為抑止しておく
                MsgBox("交渉記録の編集中は、削除ができません。")
                Exit Sub
            End If
        End If
        Dim r As Integer
        r = MessageBox.Show("削除してよろしいですか？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        ' DGV2の指定行を削除
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.SCD, "Delete From FKSCD Where FKD01 = '" & id & "'")
        ExUpdateButton2()
    End Sub

    ' 更新ボタン DGV1
    Private Sub ExUpdateButton() Handles Button1.Click
        cmn.StartPBar(7)
        cmn.UpdPBar("顧客情報ダウンロード中")
        db.DBFileFDL(Sqldb.TID.SCD)                     ' ファイル強制ダウンロード
        'db.UpdateOrigDT(Sqldb.TID.SCD)
        db.UpdateOrigDT(Sqldb.TID.SCR)

        ShowDGVList(DGV2)
        'ShowDGVList(DGV4)      ' ShowDunLBを選択した契機で更新されるためここではしない
        ShowDGVList(DGV5)
        ShowDunLB()
        cmn.EndPBar()
    End Sub

    Public Sub ExUpdateButton2()
        If CB_AUTOUPD.Checked Then
            db.DBFileFDL(Sqldb.TID.SCD)                     ' ファイル強制ダウンロード
            ShowDGVList(DGV2)
        End If
    End Sub

    ' 印刷ボタン
    Private Sub BT_B4_Click(sender As Object, e As EventArgs) Handles BT_B4.Click
        If DGV2.Rows.Count = 0 Then Exit Sub
        Dim f As New VBR_DGV2
        f.ShowDialog()
        f.Dispose()
    End Sub

    ' 検索でEnterキー
    Private Sub TB_A1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB_SearchInput.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            SearchKeyEnterEvent()
        End If
    End Sub

    ' フリーメモをLeave時にDB保存
    Private Sub TB_F1_Enter(sender As Object, e As EventArgs) Handles TB_FreeMemo.Enter
        BeforeFreeTxt = TB_FreeMemo.Text
    End Sub
    Private Sub TB_F1_Leave(sender As Object, e As EventArgs) Handles TB_FreeMemo.Leave
        If BeforeFreeTxt = TB_FreeMemo.Text Then Exit Sub     ' 何も変更してないなら保存しない 
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim id As String = DGV1.CurrentRow.Cells(0).Value

        ' REMに既にある場合はUPDATE、なければINSERT
        If db.IsExistREM(id) Then
            db.ExeSQL(Sqldb.TID.SCR, "Update FKSCREM Set FKR03 = '" & TB_FreeMemo.Text & "' Where FKR01 = '" & id & "'")
            ' 更新履歴の記録
        Else
            db.ExeSQL(Sqldb.TID.SCR, "Insert Into FKSCREM Values('" & id & "','" & DGV1.CurrentRow.Cells(4).Value & "','" & TB_FreeMemo.Text & "','','','')")     ' FKSCREM更新
            ' 更新履歴の記録
        End If
        'db.WriteHistory(id, DGV1.CurrentRow.Cells(1).Value, "フリーメモ", "編集", TB_F1.Text)
        db.UpdateOrigDT(Sqldb.TID.SCR)
        ShowDGVList(DGV2)
    End Sub

    ' 手動でアップデート実行
    Private Sub L_UPDMsg_Click(sender As Object, e As EventArgs) Handles L_UPDMsg.Click
        MsgBox("債権管理アプリのアップデートがあります。" & vbCrLf &
               "右上のＸボタンで画面を閉じると、アプリ更新ボタンがありますので、" & vbCrLf &
               "必要に応じてアプリを更新してください。")
    End Sub

    ' 記録一覧 検索でEnterキー
    Private Sub TB_SearchDGV5_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB_RecA1.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            'Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
            cmn.StartPBar(2)
            ShowDGVList(DGV5, TB_RecA1.Text)
            cmn.EndPBar()
        End If
    End Sub

    ' タブ移動時、検索欄選択時に検索オプションを非表示にする
    Private Sub TAB_A1_SelectedIndexChanged() Handles TAB_A1.SelectedIndexChanged, TB_SearchInput.Enter, Tab_4Dun.Enter, Tab_2Record.Enter
        AddTelForm.Visible = False
    End Sub

    ' 物件情報の日付設定ボタン、DTP
    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DTP_PI1.ValueChanged, BT_PI3.Click
        If DGV_FPLIST.CurrentCell.ColumnIndex <> 2 Then Exit Sub
        DGV_FPLIST.CurrentCell.Value = DTP_PI1.Value.ToString("yyyy/MM/dd")
    End Sub

    ' 物件情報の日付空白設定
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles BT_PI2.Click
        If DGV_FPLIST.CurrentCell.ColumnIndex <> 2 Then Exit Sub
        DGV_FPLIST.CurrentCell.Value = ""
    End Sub

    ' 検索条件の変更
    Private Sub CB_SEARCHOPT_SelectedIndexChanged(sender As Object, e As EventArgs)
        If TB_SearchInput.Text.Length > 0 Then
            SearchKeyEnterEvent()
        End If
    End Sub

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles TAB_A1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                cmn.OpenCurrentDir()
            Case Keys.F2
            Case Keys.F3
            Case Keys.F4
        End Select
    End Sub

#End Region

#Region "Thread"
    ' アプリ更新通知
    Public Sub NoticeUpdateApp(result As Integer)
        log.cLog("EventCB: アプリ更新通知 result: " & result)
        L_UPDMsg.Visible = True
    End Sub

    ' DB更新通知 SCD
    Public Sub NoticeUpdateDB_SCD(result As Integer)
        log.cLog("EventCB: DB-SCD更新通知result: " & result)
        ShowDGVList(DGV2)
        'ShowDGVList(DGV4)
        ShowDGVList(DGV5)
    End Sub

    ' DB更新通知 PI
    Public Sub NoticeUpdateDB_PI(result As Integer)
        log.cLog("EventCB: DB-PI更新通知result: " & result)
        ShowDGV_FPLIST(GetFPPage())
    End Sub

#End Region

#Region "表示"
    ' 各DGVの表示
    Private Sub ShowDGVList(dgv As DataGridView)
        ShowDGVList(dgv, "")
    End Sub
    Private Sub ShowDGVList(dgv As DataGridView, FilterWord As String)
        cmn.UpdPBar("顧客情報の表示中")
        Dim idx As Integer = 0
        Dim dt As DataTable = Nothing
        Dim bindID As Integer
        If dgv.Visible = False Then Exit Sub
        log.TimerST()

        If dgv.Rows.Count > 0 Then idx = dgv.CurrentRow.Index           ' 元の選択中行を覚えておく        

        Select Case True
            Case dgv Is DGV1                                ' ## 顧客情報タブ リスト
                MaxCosCount = db.OrgDataTablePlusAssist.Rows.Count
                FilterWordsDGV1(FilterWord)
                dgv.Sort(dgv.Columns(5), ComponentModel.ListSortDirection.Descending)
                L_STS.Text = " ( " & DGV1.Rows.Count & " / " & MaxCosCount & " ) 件 表示中"
                EnableObjects(dgv.Rows.Count <> 0)              ' もしDGV1のメンバーが0なら編集できなくする
                Exit Sub

            Case dgv Is DGV2                                ' ## 顧客情報タブ 交渉記録
                bindID = 1
                ' DGV1の選択した顧客の交渉記録をフィルタ表示
                If DGV1.Rows.Count > 0 Then
                    Dim dr As DataRow() = Nothing
                    dt = db.GetSelect(Sqldb.TID.SCD, $"SELECT * From {db.GetTable(Sqldb.TID.SCD)} WHERE FKD02 = '{DGV1.CurrentRow.Cells(0).Value}'")
                End If
            'Case dgv Is DGV3                                ' ## タスクタブ リスト
            '    bindID = 2
            '    dt = db.OrgDataTable(Sqldb.TID.SCTD).Copy   ' DataTableをオリジナルからコピー
            Case dgv Is DGV4                                ' ## 更新履歴タブ リスト
                bindID = 3
                ' 日時が一致した督促状を取得
                dt = db.GetSelect(Sqldb.TID.SCD, "Select FKD01, FKD03, FKD08, FKD02, FKD09, FKD10, FKD04, FKD07 From FKSCD Where FKD08 <> '' And FKD08 = '" & LB_DunRead.SelectedItem & "'")
                'AddColumnsDun(dt)                           ' 督促管理に残高更新日を追加     処理に時間かかるから一旦削除
            Case dgv Is DGV5                                ' ## 対応一覧タブ リスト
                bindID = 4
                dt = FilterDGV5() ' フィルタリング条件を設定

            Case dgv Is DGV6                                ' ## 督促リスト
                bindID = 5
                dt = GetDunCosDataTable()
            'Case dgv Is DGV7                                
            'Case dgv Is DGV8           

            Case dgv Is DGV9    ' 顧客詳細情報表示
                ' DGV生成
                InitDGV9()

                ' DGV情報表示
                If DGV1.Rows.Count = 0 Then Exit Sub
                If DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then Exit Sub
                Dim cid As String = DGV1.CurrentRow.Cells(0).Value
                TB_FreeMemo.Text = ""

                ' 契約内容ラベル初期化
                L_TYPE_F.Visible = False
                L_TYPE_A.Visible = False
                L_TYPE_H.Visible = False

                ' 選択中の顧客番号を顧客DBから取得
                Dim cDr As DataRow() = db.OrgDataTablePlusAssist.Select(String.Format("FK02 = '{0}'", cid))
                If cDr.Count >= 1 Then
                    Dim cInfo As DataRow = cDr(0)

                    cid = cInfo.Item(1)                                         ' 顧客番号(機構番号)保存
                    dgv(1, 0).Value = cid                                       ' 顧客番号
                    ' 債務者
                    dgv(1, 1).Value = cInfo.Item(10)                                 ' ヨミカナ
                    dgv(1, 2).Value = cInfo.Item(9)                                  ' 氏名
                    dgv(1, 3).Value = cInfo.Item(15)                                 ' 郵便番号
                    dgv(1, 4).Value = cInfo.Item(16)                                 ' 住所
                    dgv(1, 5).Value = cInfo.Item(17)                                 ' 勤務先
                    dgv(3, 1).Value = cInfo.Item(13)                                 ' TEL1
                    dgv(3, 2).Value = cInfo.Item(11)                                 ' 生年月日
                    dgv(3, 3).Value = sccmn.GetGroupCredit(cmn.SetValueDefault(cInfo.Item(20))) ' 団信加入サイン
                    dgv(3, 5).Value = cInfo.Item(18)                                 ' 勤務先TEL1
                    ' 連帯債務者
                    dgv(1, 6).Value = cInfo.Item(30)                                 ' ヨミカナ
                    dgv(1, 7).Value = cInfo.Item(29)                                 ' 氏名
                    dgv(1, 8).Value = cInfo.Item(35)                                 ' 郵便番号
                    dgv(1, 9).Value = cInfo.Item(36)                                 ' 住所
                    dgv(1, 10).Value = cInfo.Item(37)                                ' 勤務先
                    dgv(3, 6).Value = cInfo.Item(33)                                 ' TEL1
                    dgv(3, 7).Value = cInfo.Item(31)                                 ' 生年月日
                    dgv(3, 8).Value = sccmn.GetGroupCredit(cmn.SetValueDefault(cInfo.Item(40))) ' 団信加入サイン

                    dgv(3, 10).Value = cInfo.Item(38)                                ' 勤務先TEL1
                    ' 証券番号(アシスト)
                    Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C02 = '{0}'", cid))
                    If dr.Length > 0 AndAlso dr(0).Item(11).ToString.Length > 0 Then
                        dgv(3, 0).Value = dr(0).Item(11)
                        ' 証券番号の先頭が6なら保証型とする
                        If dr(0)(11)(0) = "6" Then L_TYPE_H.Visible = True
                    End If

                    ' 総務部 物件情報欄
                    dgv(8, 1).Value = cmn.SetValueDefault(cInfo.Item(63), "")       ' 住居サイン
                    dgv(8, 2).Value = cmn.SetValueDefault(cInfo.Item(64), "")       ' 物件郵便番号
                    dgv(8, 3).Value = cmn.SetValueDefault(cInfo.Item(65), "")       ' 物件住所
                    dgv(8, 4).Value = cmn.SetValueDefault(cInfo.Item(43), "")       ' 金融機関
                    dgv(8, 5).Value = cmn.SetValueDefault(cInfo.Item(44), "")       ' 支店番号
                    dgv(8, 6).Value = cmn.SetValueDefault(cInfo.Item(41), "")       ' 口座番号
                    dgv(8, 7).Value = cmn.SetValueDefault(cInfo.Item(42), "")       ' 口座名義

                    ' メモ欄
                    dgv(8, 11).Value = cmn.SetValueDefault(cInfo.Item(46), "")       ' メモ欄

                    ' 物件住所 主・連の住所と全て同じなら(住所と同じ)、異なれば赤字で物件住所
                    Dim isSameAddress As Boolean = cmn.DiffStr(cInfo.Item(16), dgv(8, 3).Value)
                    Dim isSameAdditionalInfo As Boolean = cInfo.Item(36).Length > 0 AndAlso cmn.DiffStr(cInfo.Item(36), dgv(8, 3).Value)
                    If isSameAddress AndAlso (cInfo.Item(36).Length = 0 OrElse isSameAdditionalInfo) Then
                        dgv(8, 3).Value = "(住所と同じ)"
                        dgv(8, 3).Style.ForeColor = Color.DarkGray
                    Else
                        dgv(8, 3).Style.ForeColor = Color.Red
                    End If

                    ' DGV9の住所欄の幅が狭いのでテキストボックスにも表示させておく
                    TB_ADDRESS1.Text = cmn.SetValueDefault(cInfo.Item(16), "")
                    TB_ADDRESS2.Text = cmn.SetValueDefault(cInfo.Item(36), "")
                    TB_ADDRESS3.Text = cmn.SetValueDefault(cInfo.Item(65), "")

                    ' F35 契約金額
                    Dim repmo As Integer = cmn.Int(cInfo.Item(49))
                    If cInfo.Item(48) IsNot DBNull.Value Then repmo += cmn.Int(cInfo.Item(48))
                    If cInfo.Item(2) = "1" Or cInfo.Item(2) = "3" Then
                        L_TYPE_F.Visible = True
                        dgv(5, 0).Value = "フラット35"
                        dgv(5, 1).Value = cInfo.Item(3)                               ' 金消契約日
                        dgv(5, 2).Value = cmn.Int(cInfo.Item(55)).ToString("#,0")     ' 貸付金額
                        dgv(5, 3).Value = cmn.Int(cInfo.Item(53)).ToString("#,0")     ' 更新日残高
                        dgv(5, 4).Value = cmn.Int(cInfo.Item(56)).ToString("#,0")     ' 貸付金額(B)
                        dgv(5, 5).Value = cmn.Int(cInfo.Item(47)).ToString("#,0")     ' 更新日残高(B)
                        dgv(5, 6).Value = cInfo.Item(50)                              ' 残高更新日
                        dgv(5, 7).Value = repmo.ToString("#,0")                       ' 返済額
                        dgv(5, 8).Value = cmn.Int(cInfo.Item(52)).ToString("#,0")     ' 返済額(B)
                        dgv(5, 9).Value = cmn.Int(cInfo.Item(51)).ToString("#,0")     ' 延滞回数
                        dgv(5, 10).Value = cmn.Int(cInfo.Item(54)).ToString("#,0")    ' 延滞合計額
                        dgv(5, 11).Value = cInfo.Item(57)                             ' 完済日
                    End If
                    If cInfo.Item(2) = "2" Or cInfo.Item(2) = "3" Then
                        L_TYPE_A.Visible = True
                        dgv(6, 0).Value = "アシスト"
                        Dim dtime As DateTime
                        If DateTime.TryParse(cInfo.Item(58), dtime) Then              ' 前リソースが償還回数だった名残りで、日付の場合だけ金消契約日として表示する
                            dgv(6, 1).Value = cInfo.Item(58)                          ' 金消契約日
                        End If
                        dgv(6, 2).Value = cmn.Int(cInfo.Item(59)).ToString("#,0")     ' 貸付金額
                        dgv(6, 3).Value = cmn.Int(cInfo.Item(62)).ToString("#,0")     ' 更新日残高
                        dgv(6, 4).Value = cmn.Int(cInfo.Item(21)).ToString("#,0")     ' 貸付金額(B)
                        dgv(6, 5).Value = cmn.Int(cInfo.Item(22)).ToString("#,0")     ' 更新日残高(B)
                        'dgv(6, 6).Value = ""                                         ' 残高更新日
                        dgv(6, 7).Value = cmn.Int(cInfo.Item(61)).ToString("#,0")     ' 返済額
                        dgv(6, 8).Value = cmn.Int(cInfo.Item(23)).ToString("#,0")     ' 返済額(B)
                        dgv(6, 9).Value = cmn.Int(cInfo.Item(60)).ToString("#,0")     ' 延滞回数
                        'dgv(6, 10).Value = ""                                         ' 延滞合計額
                        dgv(6, 11).Value = cInfo.Item(24)                             ' 完済日
                    End If
                End If

                ' 追加要素の顧客情報取得
                Dim remDr As DataRow() = db.OrgDataTable(Sqldb.TID.SCR).Select(String.Format("FKR01 = '{0}'", cid))
                If remDr.Count = 1 Then
                    Dim cInfo As DataRow = remDr(0)
                    TB_FreeMemo.Text = cInfo.Item(2)
                End If

                SearchColor(TB_SearchInput.Text)
                log.TimerED("ShowDGVList End:" & dgv.Name)
                Exit Sub

            Case Else
                Exit Sub
        End Select
        BindDGVList(dgv, dt, bindID)                         ' Bind(DataTableとDGV紐付け)
        FilterWordsDGV(dt, FilterWord, dgv)                  ' 検索ワードフィルタ

        dgv.AutoGenerateColumns = False                      ' DataTable設定時に自動で列追加されないようにする
        dgv.DataSource = dt

        ' DGVの選択行を元に戻す
        Select Case dgv.Rows.Count
            Case 0                                            ' 表示結果、列0ならそのまま一番上
            Case <= idx : dgv.CurrentCell = dgv(2, 0)         ' 表示結果、列が減って表示外なら一番上
            Case Else : dgv.CurrentCell = dgv(2, idx)         ' 表示可能なら元の位置に戻す               ' 可視化されてる列じゃないとエラーになる
        End Select

        ' DGV毎の後処理
        Select Case True
            Case dgv Is DGV2
                TB_Remarks.Text = ""         ' 備考欄初期化
                'LockEventHandler_LCSum = False
                If dgv.Columns(1).DataPropertyName IsNot Nothing AndAlso dgv.Columns(1).DataPropertyName <> "" Then
                    dgv.Sort(dgv.Columns(1), ComponentModel.ListSortDirection.Descending)
                End If
                If DGV2.Rows.Count > 0 Then TB_Remarks.Text = DGV2.CurrentRow.Cells(8).Value
            'Case dgv Is DGV3
            Case dgv Is DGV4
                dgv.Sort(dgv.Columns(2), ComponentModel.ListSortDirection.Descending)
            Case dgv Is DGV5
                DGV5_CellClick()
                L_STS_Rec.Text = " ( " & DGV5.Rows.Count & " / " & MaxCosCount & " ) 件 表示中"
                'Case dgv Is DGV7

            Case Else
        End Select

        log.TimerED("ShowDGVList End:" & dgv.Name)
    End Sub

    ' Bind列の設定
    Private Sub BindDGVList(dgv As DataGridView, ByRef dt As DataTable, bindID As Integer)
        If dt Is Nothing Then Exit Sub
        ' DGVにBindするカラムリスト
        Dim BindList(,) As String = {
            {"FK02", "FK10", "FK11", "FK51", "FK12", "FK55", "", "", "", "", "", "", ""},                                            ' DGV1   債権情報一覧
            {"FKD01", "FKD03", "FKD04", "FKD11", "FKD05", "FKD06", "FKD12", "FKD08", "FKD07", "FKD13", "FKD09", "", ""},             ' DGV2   交渉記録
            {"C01", "C02", "C03", "C04", "C05", "C06", "C07", "C08", "C09", "", "", "", ""},                                         ' DGV3   タスク一覧
            {"FKD01", "FKD03", "FKD08", "FKD02", "FKD09", "FKD10", "FKD04", "FKD07", "", "", "", "", ""},                            ' DGV4   更新履歴
            {"FKD01", "FKD02", "FKD03", "FKD09", "FKD10", "FKD04", "FKD11", "FKD05", "FKD13", "FKD06", "FKD12", "FKD08", "FKD07"},   ' DGV5   記録一覧
            {"FK02", "FK03", "FK10", "FK11", "FK51", "FK50", "FK49", "FK17", "FK37", "FK53", "FK08", "", ""},                        ' DGV6   督促一覧
            {"", "C03", "", "", "", "", "", "", "", "", "", "", ""}                                                                  ' DGV7   物件情報
        }

        ' Bind列の設定  DGVとDataTableを紐付ける　BindListの設定するカラムだけをBind設定
        For clmSetNum = 0 To BindList.GetLength(1) - 1
            For dtNum = 0 To dt.Columns.Count - 1
                If BindList(bindID, clmSetNum) = "" Then Exit For                                   ' BindTblのメンバーが "" の場合はバインド列を設定しない Skip
                If BindList(bindID, clmSetNum) <> dt.Columns(dtNum).ToString Then Continue For
                dgv.Columns(clmSetNum).DataPropertyName = dt.Columns(dtNum).ToString                ' Bind列の設定
                Exit For
            Next
        Next
    End Sub

    ' 検索欄でEnterキー押下イベント
    Private Sub SearchKeyEnterEvent()
        Dim STtime = Date.Now
        log.cLog("検索開始")
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        cmn.StartPBar(4)
        ShowDGVList(DGV1, TB_SearchInput.Text)
        DGV1_ClickShow()
        cmn.EndPBar()
        log.cLog($"検索完了 : {(Date.Now - STtime).ToString("ss\.fff")}")
    End Sub
    ' 検索ワードフィルタ
    Private Sub FilterWordsDGV(ByRef dt As DataTable, FilterWord As String, dgv As DataGridView)
        If FilterWord = "" Then Exit Sub
        If dt Is Nothing Then Exit Sub
        log.TimerST()

        ' 検索ワードが追加電話番号でヒットした場合、その顧客番号は次の検索対象チェックで無条件ヒットにする
        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCR).Select($"FKR05 like '%{FilterWord}%' Or FKR06 like '%{FilterWord}%'")

        ' 検索対象チェックがついた情報を1行の文字列sbに設定して、検索ワードがヒットするか確認する
        ' 検索ワードがヒットしなかった場合は、dtの該当行を削除することで除外
        ' ex) sb 「12345678, 山田太郎, ﾔﾏﾀﾞﾀﾛｳ, ...」
        FilterWord = FilterWord.Replace(" ", "").Replace("　", "").Replace(",", "")   ' 検索ワードから半角全角のスペースを除去

        ' 事前にフィルタワードを正規化（ひらがな→カタカナ変換も一度だけ）
        Dim normalizedFilterWord As String = NormalizeText(FilterWord)
        Dim normalizedFilterWordKana As String = NormalizeText(StrConv(FilterWord, VbStrConv.Katakana Or VbStrConv.Narrow))

        '  DataTableからの逐次削除の改善（フィルタ済みの新しいDataTable作成）
        Dim keepRows As New List(Of DataRow)()

        ' dr 配列から顧客番号をHashSetに格納する
        Dim addTelSet As New HashSet(Of String)()
        For Each addRow As DataRow In dr
            addTelSet.Add(addRow(0).ToString())
        Next

        For r = dt.Rows.Count - 1 To 0 Step -1
            ' 追加電話番号でヒットしてる場合はスキップ
            If addTelSet.Contains(dt.Rows(r).Item(1).ToString()) Then Continue For

            Dim concatenatedString As String = ""
            Select Case True
                Case dgv Is DGV5
                    concatenatedString = NormalizeText(
                        dt.Rows(r).Item(1).ToString() & "," &
                        dt.Rows(r).Item(4).ToString() & "," &
                        dt.Rows(r).Item(8).ToString() & "," &
                        dt.Rows(r).Item(9).ToString() & "," &
                        dt.Rows(r).Item(10).ToString())
                Case Else
                    Continue For
            End Select

            ' ヒットしてたらkeepRowsに追加
            If concatenatedString.IndexOf(normalizedFilterWord) >= 0 Or concatenatedString.IndexOf(normalizedFilterWordKana) >= 0 Then
                keepRows.Add(dt.Rows(r))
            End If
        Next
        ' 新しいDataTableにフィルタ済みの行だけを取り込む
        Dim newDt As DataTable = dt.Clone()
        For Each row As DataRow In keepRows
            newDt.ImportRow(row)
        Next
        dt = newDt

        log.TimerED("検索終了")
    End Sub

    ' DGV1初期設定
    Private Sub InitDGV1()
        BuildDGV1SearchCache()
        DGV1.AutoGenerateColumns = False
        Dim dgv1propList As String() = {"FK02", "FK10", "FK11", "FK51", "FK12", "FK55"}
        For p = 0 To dgv1propList.Length - 1
            DGV1.Columns(p).DataPropertyName = dgv1propList(p)
        Next
    End Sub

    ' キャッシュテーブルからフィルタしてDGV1に表示する処理
    Private Sub FilterWordsDGV1(ByVal FilterWord As String)
        If db.gDGV1SearchCache Is Nothing Then Exit Sub
        If FilterWord <> "" Then
            ' DataView を用いて検索キャッシュテーブルからフィルタする
            Dim dv As New DataView(db.gDGV1SearchCache)
            FilterWord = FilterWord.Replace(" ", "").Replace("　", "").Replace(",", "")     ' FilterWord の正規化（スペース、カンマ除去）
            FilterWord = cmn.ConvertSmallKana(FilterWord)                                   ' あいまい検索用に小文字かなを大文字かなに変換。 ex)りょうた→りようた

            ' フィルタ設定 フィルタ文字と半角カナフィルタ文字を同時検索
            dv.RowFilter = $"g_SearchCache LIKE '%{NormalizeText(FilterWord)}%' OR g_SearchCache LIKE '%{NormalizeText(StrConv(FilterWord, VbStrConv.Katakana Or VbStrConv.Narrow))}%'"
            ' gDGV1SearchCacheのフィルタ結果から、FKデータを取得すためにOrgDataTablePlusAssistとマージしてDGV1に設定
            Dim margeDt As DataTable = JoinOrgAndCache(db.OrgDataTablePlusAssist, dv.ToTable())
            DGV1.DataSource = margeDt
        Else
            ' 検索ワードがなければフィルタせず全表示
            DGV1.DataSource = db.OrgDataTablePlusAssist
        End If
    End Sub

    ' DGV1用DataTableの結合
    Private Function JoinOrgAndCache(ByVal orgDt As DataTable, ByVal cacheDt As DataTable) As DataTable
        ' 結合キーは、orgDtの "FK02" とcacheDtの "CustNo" とする
        Dim query = From orgRow In orgDt.AsEnumerable()
                    Join cacheRow In cacheDt.AsEnumerable()
                  On orgRow.Field(Of String)("FK02") Equals cacheRow.Field(Of String)("CustNo")
                    Select orgRow

        ' 結合結果を新しいDataTableにインポートする
        Dim joinedDt As DataTable = orgDt.Clone()
        For Each row In query
            joinedDt.ImportRow(row)
        Next
        Return joinedDt
    End Function

    ' キャッシュテーブルを作成して更新する処理
    ' OrgDataTablePlusAssistの主要な要素、更にKFSCREM SCR05,SCR06を連結した文字列のテーブル
    ' 文字列からはハイフンとスペースを取り除いて、あいまい検索に対応させている
    Public Sub BuildDGV1SearchCache()
        ' db.g_SearchCacheが未初期化なら新規作成、既にあればクリア
        If db.gDGV1SearchCache Is Nothing Then
            db.gDGV1SearchCache = New DataTable()
            db.gDGV1SearchCache.Columns.Add("CustNo", GetType(String))
            db.gDGV1SearchCache.Columns.Add("g_SearchCache", GetType(String))
        Else
            db.gDGV1SearchCache.Clear()
        End If

        ' SCRテーブルから顧客番号をキーとしたDictionaryを作成
        Dim scrTable As DataTable = db.OrgDataTable(Sqldb.TID.SCR)
        Dim scrDict As New Dictionary(Of String, DataRow)()
        For Each scrRow As DataRow In scrTable.Rows
            Dim custKey As String = scrRow(0).ToString()
            If Not scrDict.ContainsKey(custKey) Then
                scrDict.Add(custKey, scrRow)
            End If
        Next

        ' dtの各行について、キャッシュ文字列を作成し、db.g_SearchCache に追加
        db.gDGV1SearchCache.BeginLoadData()
        For Each row As DataRow In db.OrgDataTablePlusAssist.Rows
            Dim custNo As String = row(1).ToString()
            Dim sb As New System.Text.StringBuilder(256)
            sb.Append(custNo).Append(",")
            sb.Append(row(9).ToString()).Append(",")                       ' 主 氏名
            sb.Append(row(10).ToString()).Append(",")                      ' 主 ﾖﾐｶﾅ
            sb.Append(row(29).ToString()).Append(",")                      ' 連 氏名
            sb.Append(row(30).ToString()).Append(",")                      ' 連 ﾖﾐｶﾅ
            sb.Append(row(13).ToString()).Append(",")                      ' 主 TEL1
            sb.Append(row(18).ToString()).Append(",")                      ' 主 勤務先TEL1
            sb.Append(row(33).ToString()).Append(",")                      ' 連 TEL1
            sb.Append(row(38).ToString()).Append(",")                      ' 連 勤務先TEL1
            sb.Append(row(16).ToString().Replace("-", "ｰ")).Append(",")    ' 主 住所
            sb.Append(row(37).ToString().Replace("-", "ｰ")).Append(",")    ' 連 勤務先
            sb.Append(row(17).ToString().Replace("-", "ｰ")).Append(",")    ' 主 勤務先
            sb.Append((cmn.Int(row(48).ToString()) + cmn.Int(row(49).ToString())).ToString()).Append(",")  ' 返済額 + 返済額2
            sb.Append(row(11).ToString()).Append(",")                      ' 主 生年月日
            sb.Append(row(31).ToString()).Append(",")                      ' 連 生年月日
            sb.Append(row(8).ToString())                                   ' 証券番号

            ' SCRテーブルのFKR05, FKR06を追加
            If scrDict.ContainsKey(custNo) Then
                sb.Append(",").Append(scrDict(custNo)("FKR05").ToString()).Append(",").Append(scrDict(custNo)("FKR06").ToString())
            End If

            Dim cacheString As String = NormalizeText(sb.ToString())
            Dim newRow As DataRow = db.gDGV1SearchCache.NewRow()
            newRow("CustNo") = custNo
            newRow("g_SearchCache") = cacheString
            db.gDGV1SearchCache.Rows.Add(newRow)
        Next
        db.gDGV1SearchCache.EndLoadData()
    End Sub

    ' ヘルパー関数：入力文字列から半角・全角スペースとハイフンを除去
    Private Function NormalizeText(ByVal input As String) As String
        Return input.Replace(" ", "").Replace("　", "").Replace("-", "")
    End Function

    ' 顧客情報詳細のテキストボックス位置調整
    Private Sub DataGridView1_Scroll(sender As Object, e As ScrollEventArgs) Handles DGV9.Scroll
        TextBoxPositionSetting(TB_ADDRESS1, 1, 4)
        TextBoxPositionSetting(TB_ADDRESS2, 1, 9)
        TextBoxPositionSetting(TB_ADDRESS3, 1, 12)
    End Sub

    ' テキストボックスの表示/非表示を判定し、位置を調整する
    Private Sub TextBoxPositionSetting(textBox As TextBox, columnIndex As Integer, rowIndex As Integer)
        Dim Label1Rocatangle As Rectangle = DGV9.GetCellDisplayRectangle(0, 1, False)
        Dim Label2Rocatangle As Rectangle = DGV9.GetCellDisplayRectangle(0, 6, False)
        Dim cellRectangle As Rectangle = DGV9.GetCellDisplayRectangle(columnIndex, rowIndex, False)
        textBox.Location = New Point(cellRectangle.X + DGV9.Location.X - 1, cellRectangle.Y + DGV9.Location.Y - 1)
        L_JUNIN1.Location = New Point(Label1Rocatangle.X + DGV9.Location.X + 37, Label1Rocatangle.Y + DGV9.Location.Y + 1)
        L_JUNIN2.Location = New Point(Label2Rocatangle.X + DGV9.Location.X + 37, Label2Rocatangle.Y + DGV9.Location.Y + 1)

        ' テキストボックスがDGVの表示範囲内にあるか判断して表示/非表示を切り替える
        textBox.Visible = Not (cellRectangle.Y = 0 Or cellRectangle.Y > 250)
        L_JUNIN1.Visible = Not (Label1Rocatangle.Y = 0 Or Label1Rocatangle.Y > 250)
        L_JUNIN2.Visible = Not (Label2Rocatangle.Y = 0 Or Label2Rocatangle.Y > 250)
        log.cLog($"{textBox.Name}:{cellRectangle.Y}:{textBox.Visible}")
    End Sub

    ' 検索結果一致のカラーリング
    Public Sub SearchColor(search As String)
        Dim search_txt() = {DGV9(3, 1), DGV9(3, 5), DGV9(3, 6), DGV9(3, 10)}   ' カラーリングするテキストリスト
        For Each t In search_txt
            t.Style.BackColor = System.Drawing.Color.White
            If search.Length <> 0 And   ' 検索欄が空白ならカラーリングしない
               t.Value.Replace("-", "").IndexOf(search.Replace("-", "")) >= 0 Then
                t.Style.BackColor = System.Drawing.Color.LightSalmon
            End If
        Next
    End Sub

    ' 債権者情報のテキストボックスなどUI操作の有効/無効
    Private Sub EnableObjects(sw As Boolean)
        Dim DisableObjList() As Object = {DGV2, TB_Remarks, TB_FreeMemo}
        For Each obj As Object In DisableObjList
            obj.enabled = sw
        Next
    End Sub

    ' 物件住所をクリックして比較表示
    Private Sub DGV9_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGV9.CellClick
        ' メモ欄
        If e.RowIndex = 11 AndAlso e.ColumnIndex = 8 Then
            If DGV9.Rows(11).Cells(8).Value.ToString.Length > 0 Then
                MsgBox(DGV9.Rows(11).Cells(8).Value)
            End If
        End If

        ' 物件住所
        If e.RowIndex = 3 AndAlso e.ColumnIndex = 8 Then
            Dim addressMain As String = DGV9.Rows(4).Cells(1).Value
            Dim addressSub As String = DGV9.Rows(9).Cells(1).Value
            Dim address2 As String = DGV9.Rows(3).Cells(8).Value
            If address2 = "(住所と同じ)" Then Exit Sub

            Dim outMsg As String = $"[主：住所]{vbCrLf}{addressMain}{vbCrLf}"
            If addressSub.Length > 0 Then
                outMsg += $"[連：住所]{vbCrLf}{addressSub}{vbCrLf}"
            End If
            outMsg += $"[物件住所]{vbCrLf}{address2}"
            MsgBox(outMsg)
        End If
    End Sub
#End Region

#Region "検索オプション"
    ' ツールチップの設定
    Private Sub SetToolTips()
        tt1.SetToolTip(TB_SearchInput, "「債権番号」「債務者名」「連帯債務者名」「各電話番号」から検索できます。")   ' ツールチップ
        tt1.SetToolTip(BT_B1, "左の表(債務者一覧)から、追加したい債務者を複数選択して同時に追加できます。" & vbCrLf &
                              "同時に選択するには、Ctrlキーを押しながら左クリックします。")     ' ツールチップ
    End Sub
#End Region

#Region "ファイル監視関連"
    ' ファイル監視準備 開始
    '   以下の2つの方法でファイルを監視する。
    '   1: FileSystemWacher = 監視APIだけどsamba非対応だから環境によっては検出できない
    '   2: 非同期タスク(200ms周期)でタイムスタンプを監視して検出 (sambaだったとき用) ちょっと遅い
    '  どっちも検出するパターンはあるが、同じイベントは2度通知されないようにしてある
    Private Watcher As FileSystemWatcher
    Private Sub StartWatching()
        Try
            ' 監視機能②  周期監視による監視     タイムラグあるけどsambaも対応    対象：着信ログ、DB更新、TaskDB更新
            PoolingFiles()
        Catch ex As Exception
            ' 監視ファイルが無い等の理由で監視できない
            MsgBox("エラーにより自動更新が出来ていない可能性があります。")
        End Try
    End Sub

    ' 監視機能② ポーリング処理
    Private Sub PoolingFiles()
        ' 監視対象一覧             監視識別子      監視ファイルパス(*付きは複数ファイル)
        Dim PList(,) As String = {{POLLING_ID_CUDB, db.CurrentPath_SV & Common.DIR_UPD & Sqldb.DB_FKSC},           ' DB   FKSC.DB3
                                  {POLLING_ID_ASC, db.CurrentPath_SV & Common.DIR_UPD & Common.EXE_NAME},          ' EXE  A_SC.exe
                                  {POLLING_ID_FP, db.CurrentPath_SV & Common.DIR_DB3 & Sqldb.DB_FKSCFPI},          ' FPINFO.db3
                                  {POLLING_ID_MR, db.CurrentPath_SV & Common.DIR_DB3 & Sqldb.DB_MNGREQ},           ' MRReq.db3
                                  {POLLING_ID_SCD, db.CurrentPath_SV & Common.DIR_DB3 & Sqldb.DB_FKSCLOG}}         ' FKSC.LOG.db3
        Const PL_ID As Integer = 0
        Const PL_FILE As Integer = 1

        ' 監視対象一覧分のタイマー生成
        Dim LastTime(PList.GetLength(0) - 1) As DateTime      ' 最終更新時刻(ファイル更新時に更新)

        ' 監視タスク起動 非同期
        Dim task2 As Task = Task.Run(
                Sub()
                    ' 周期監視
                    Dim firstCycle As Boolean = True        ' 一周目は更新実行せずタイムスタンプだけ更新
                    PoolingStart = True
                    While PoolingStart
                        For n = 0 To PList.GetLength(0) - 1
                            Dim updateFlag As Boolean = False               ' 更新フラグ
                            Dim filePath As String = PList(n, PL_FILE)      ' 監視対象ファイルフルパス
                            ' 監視対象パスに存在するファイルを全て取得
                            Dim fileList As String() = Nothing
                            Try
                                ' たまにネットワークアドレスにアクセスできないエラーが出る？からそのときはスルーするためのtry
                                fileList = Directory.GetFileSystemEntries(Path.GetDirectoryName(filePath), Path.GetFileName(filePath))
                            Catch ex As Exception
                                Continue For
                            End Try
                            For Each f As String In fileList
                                Dim fTime As DateTime = File.GetLastWriteTime(f)             ' タイムスタンプ取得
                                'log.cLog($"[cycle] {f} : {fTime:HH:mm:ss} == lastTime {LastTime(n):HH:mm:ss}")
                                If LastTime(n) < fTime Then
                                    ' ファイル更新を検出
                                    If EditForm IsNot Nothing Then
                                        If EditForm.Visible Then Continue For                   ' 交渉記録の編集中は更新しない
                                    End If
                                    updateFlag = True
                                    LastTime(n) = fTime
                                End If
                            Next

                            If firstCycle Then Continue For      ' 初回はファイル更新時刻の初期設定のために必ず更新が発生する、ただし更新はしない
                            If Not updateFlag Then Continue For

                            log.cLog("cycle - 実行:" & PList(n, PL_ID))
                            If Me.IsHandleCreated Then
                                Me.Invoke(New delegate_PoolingCallBack(AddressOf PoolingCallBack), PList(n, PL_ID))
                            End If
                            updateFlag = False
                        Next
                        firstCycle = False
                        Thread.Sleep(POLLING_CYCLE)
                    End While
                End Sub
            )
    End Sub

    ' 監視機能② イベント受信 (Pooling)                     
    Private Sub PoolingCallBack(id As String)
        log.cLog("Pooling 検出: " & id)
        Select Case id
            Case POLLING_ID_SCD     ' SCD DB更新
                Invoke(New MethodInvoker(AddressOf UpdateDB_SCD))          ' SCD更新
            Case POLLING_ID_FP     ' FP DB更新
                Invoke(New MethodInvoker(AddressOf UpdateDB_FP))           ' FP更新
            Case POLLING_ID_MR     ' MR DB更新
                Invoke(New MethodInvoker(AddressOf UpdateDB_MR))           ' MR更新
            Case POLLING_ID_CUDB    ' 顧客DB更新
                Invoke(New MethodInvoker(AddressOf DownloadCustomerDB))    ' 顧客DBのダウンロード更新
            Case POLLING_ID_ASC     ' A_SC本体の更新
                Invoke(New MethodInvoker(AddressOf NoticeUpdate_ASC))      ' A_SC更新
        End Select
    End Sub

    ' 他PCでDB更新を通知
    Private Sub UpdateDB_SCD()
        log.cLog("-- UpdateDB_SCD")
        ExUpdateButton2()
    End Sub

    ' FPDB更新を通知
    Private Sub UpdateDB_FP()
        log.cLog("-- UpdateDB_FP")
        'ShowDGV_FPLIST()    ' 入力中の文字が消えてしまうため自動更新をしない
        ShowDGV_FPMNG()
    End Sub

    ' FPDB更新を通知
    Private Sub UpdateDB_MR()
        log.cLog("-- UpdateDB_MR")
        ShowDGVMR()
    End Sub

    ' 顧客DBの更新の検出
    Private Sub DownloadCustomerDB()
        log.cLog(" -- DownloadCustomerDB")
        L_UPDMsg.Visible = True
    End Sub

    ' A_SC本体の更新の検出
    Private Sub NoticeUpdate_ASC()
        log.cLog(" -- NoticeUpdate_ASC")
        L_UPDMsg.Visible = True
    End Sub
#End Region

#Region "追加電話番号"
    Const FORM_LEFT As Integer = 1120
    Const FORM_TOP As Integer = 40

    ' フォーム初期化共通処理
    Private Sub InitForm(form As Form)
        With form
            .TopLevel = False
            Me.Controls.Add(form)
            .Show()
            .BringToFront()
            .Left = FORM_LEFT
            .Top = FORM_TOP
            .Visible = False
        End With
    End Sub

    ' 初期化処理
    Private Sub InitForms()
        ' 追加電話番号フォームを生成
        AddTelForm = New SCA1_S4_TEL()
        InitForm(AddTelForm)
    End Sub

    ' 追加電話番号フォームの表示
    Private Sub ShowAddTelForm() Handles L_TELADD.MouseEnter
        AddTelForm.LoadDB()
        AddTelForm.Visible = True
    End Sub

    ' 顧客詳細情報DGV9の整形
    Private Sub InitDGV9()
        Dim dgv As DataGridView = DGV9
        If dgv.Rows.Count = 0 Then
            ' 新規DGV成形
            Dim ItemNames As String(,) = {
                    {"債権番号", "", "証券番号", "", "契約種別", "", "", "", ""},
                    {"ﾖﾐｶﾅ", "", "TEL", "", "金消契約日", "", "", "住居サイン", ""},
                    {"債務者", "", "生年月日", "", "貸付金額", "", "", "物件〒", ""},
                    {"郵便番号", "", "旧団信加入", "", "残高", "", "", "物件住所", ""},
                    {"住所", "", "", "", "貸付金額(B)", "", "", "金融機関", ""},
                    {"勤務先", "", "勤務先TEL", "", "残高(B)", "", "", "支店番号", ""},
                    {"ﾖﾐｶﾅ", "", "TEL", "", "残高更新日", "", "", "口座番号", ""},
                    {"連債者", "", "生年月日", "", "返済額", "", "", "口座名義", ""},
                    {"郵便番号", "", "旧団信加入", "", "返済額(B)", "", "", "", ""},
                    {"住所", "", "", "", "延滞月数", "", "", "", ""},
                    {"勤務先", "", "勤務先TEL", "", "延滞合計額", "", "", "", ""},
                    {"", "", "", "", "完済日", "", "", "メモ欄", ""}
                }
            '{"住居サイン", "", "物件〒", "", "完済日", ""},
            '{"物件住所", "", "", "", "", ""},
            '{"金融機関", "", "支店番号", "", "", ""},
            '{"口座番号", "", "口座名義", "", "", ""}

            For row = 0 To ItemNames.GetLength(0) - 1
                dgv.Rows.Add()
                For col = 0 To ItemNames.GetLength(1) - 1
                    dgv(col, row).Value = ItemNames(row, col)
                Next
            Next

            ' DGVデザイン 太線,背景色
            dgv.Columns(0).DefaultCellStyle.BackColor = Color.Gainsboro
            dgv.Columns(2).DefaultCellStyle.BackColor = Color.Gainsboro
            dgv.Columns(4).DefaultCellStyle.BackColor = Color.Gainsboro
            dgv.Columns(7).DefaultCellStyle.BackColor = Color.Gainsboro
            dgv.Rows(0).DividerHeight = 1
            dgv.Rows(5).DividerHeight = 1
            'dgv.Rows(10).DividerHeight = 1
            dgv.Columns(3).DividerWidth = 1
            dgv.Columns(6).DividerWidth = 1
            ' 完済日の表示文字を赤字で強調する  202410 三浦様要望
            dgv(5, 11).Style = New DataGridViewCellStyle() With {.ForeColor = Color.Red}
            dgv(6, 11).Style = New DataGridViewCellStyle() With {.ForeColor = Color.Red}
        Else
            ' 顧客情報のみクリア
            Dim clearCooumns() As Integer = {1, 3, 5, 6, 8}
            For Each col In clearCooumns
                For n = 0 To dgv.Rows.Count - 1
                    dgv(col, n).Value = ""
                Next
            Next
            TB_ADDRESS1.Text = ""
            TB_ADDRESS2.Text = ""
            TB_ADDRESS3.Text = ""

        End If
    End Sub

#End Region

    ' 指定した加入者情報を表示する DGV1
    Public Sub ShowSelectUser(cid As String)
        ShowSelectUser(cid, 0)
    End Sub
    ' itemIndexは物件情報の大項目番号
    Public Sub ShowSelectUser(cid As String, itemIndex As Integer)
        TAB_A1.SelectedTab = Tab_1SC

        ' フィルタかけられて、DGVに非表示になっていたら予め解除しておく
        Dim dt As DataTable = CType(DGV1.DataSource, DataTable)
        If dt.Select("[FK02] = '" & cid & "'").Length = 0 Then
            TB_SearchInput.Text = ""
            ShowDGVList(DGV1, "")
        End If

        DGV_PIMENU(0, itemIndex).Selected = True

        ' 検索して表示
        For Each row As DataGridViewRow In DGV1.Rows
            If row.Cells(0).Value = cid Then
                DGV1(1, row.Index).Selected = True
                Exit For
            End If
        Next
    End Sub

    ' 指定した記録情報を表示する DGV2
    Public Sub ShowSelectRecord(cid As String)
        For Each row As DataGridViewRow In DGV2.Rows
            If row.Cells(0).Value = cid Then
                DGV2.CurrentCell = DGV2(1, row.Index)
                Exit For
            End If
        Next
        TAB_A1.SelectedTab = Tab_1SC
    End Sub

#Region "記録一覧"
    Private Sub CheckedListBoxInit()
        DTP_Rec1ST.Value = Today.AddDays(-7)

        ' 「対応者」を重複削除して項目一覧として取得
        CLB_RecB2.Items.Clear()
        Dim dt1 As DataTable = db.GetSelect(Sqldb.TID.SCD, "SELECT DISTINCT FKD06 FROM FKSCD")  ' 対応者 の重複削除データ取得
        Dim dt2 As DataTable = db.GetSelect(Sqldb.TID.SCD, "SELECT DISTINCT FKD12 FROM FKSCD")  ' 対応者2の重複削除データ取得
        For Each d In dt1.Rows
            CLB_RecB2.Items.Add(d(0))
        Next
        For Each d In dt2.Rows
            If CLB_RecB2.FindStringExact(d(0)) = -1 Then CLB_RecB2.Items.Add(d(0))  ' 項目に同じものが含まれていなければ追加
        Next

        ' チェックボックスを全てONに
        CLB_RecE1_AllSet()
        CLB_RecE2_AllSet()
        CLB_B1_ItemCheck()  ' 最後に1回だけは変更後のイベントを発生させる

        ' チェックボックスを全てONにするときにイベント多発して時間かかるからチェックボックスをONにした後でハンドラ設定
        'AddHandler CLB_RecB1.ItemCheck, AddressOf CLB_B1_ItemCheck
        'AddHandler CLB_RecB2.ItemCheck, AddressOf CLB_B1_ItemCheck
    End Sub

    ' DGV選択
    Private Sub DGV5_CellClick() Handles DGV5.CellEnter
        If DGV5.Rows.Count = 0 Then Exit Sub
        TB_RecD1.Text = DGV5.CurrentRow.Cells(12).Value                       ' 詳細内容を表示させる
    End Sub

    ' 記録一覧 クリックで債権者情報に飛ぶ
    Private Sub DGV5_DoubleClick() Handles BT_RecC1.Click
        If DGV5.Rows.Count = 0 Then Exit Sub
        If DGV5.SelectedRows.Count > 1 Then
            MsgBox("債権者を複数選択しているため情報を表示できません。" & vbCrLf &
                   "1人だけ選択してからボタンを押してください。")
            Exit Sub
        End If
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ShowSelectUser(DGV5.CurrentRow.Cells(1).Value)
        ShowSelectRecord(DGV5.CurrentRow.Cells(0).Value)
    End Sub

    ' 記録一覧の削除ボタン
    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        If DGV5.SelectedRows.Count = 0 Then Exit Sub
        Dim r As Integer
        r = MessageBox.Show(DGV5.SelectedRows.Count & " 件の記録を削除してよろしいですか？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim sqlCmd As String
        For n = 0 To DGV5.SelectedRows.Count - 1
            sqlCmd = "Delete From FKSCD Where FKD01 = '" & DGV5.SelectedRows(n).Cells(0).Value & "'"
            db.AddSQL(Sqldb.TID.SCD, sqlCmd)
        Next
        db.ExeSQL(Sqldb.TID.SCD)
        ShowDGVList(DGV5)
    End Sub

    ' 記録一覧フィルタ ShowDGVListにコールされる
    Private Function FilterDGV5() As DataTable
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

        ' チェック全解除の場合は0行で表示
        If CLB_RecB1.CheckedItems.Count = 0 Or CLB_RecB2.CheckedItems.Count = 0 Then
            Return Nothing
        End If

        ' 表示期間 範囲設定
        Dim dateRngCmd As String = $"[FKD03] >= '{DTP_Rec1ST.Value.ToShortDateString}' And [FKD03] <= '{DTP_Rec1ED.Value:yyyy/MM/dd 23:59:59}'"
        If CB_RecRNG.Checked Then dateRngCmd = ""     ' 全期間チェックボックスONなら、期間フィルタをかけない

        ' 手法チェックボックス 範囲設定
        Dim methodCmd As String = ""
        If CLB_RecB1.CheckedItems.Count < CLB_RecB1.Items.Count Then
            For n = 0 To CLB_RecB1.Items.Count - 1
                If CLB_RecB1.GetItemChecked(n) Then methodCmd += $"[FKD05] = '{CLB_RecB1.Items(n)}' Or "
            Next
            methodCmd = cmn.RegReplace(methodCmd, " Or $", "")  ' 末尾の Or を削除
        End If

        ' 対応者チェックボックス 範囲設定
        Dim personCmd As String = ""
        If CLB_RecB2.CheckedItems.Count < CLB_RecB2.Items.Count Then
            For n = 0 To CLB_RecB2.Items.Count - 1
                If CLB_RecB2.Items(n) = "" Then Continue For    ' チェックボックス「空白」は後でチェックするからパス
                If CLB_RecB2.GetItemChecked(n) Then personCmd += $"([FKD06] = '{CLB_RecB2.Items(n)}' Or [FKD12] = '{CLB_RecB2.Items(n)}') Or "   ' 担当者・対応者 共に含まれていなければ排除
            Next
            If CLB_RecB2.GetItemChecked(0) Then personCmd += "([FKD06] = '' And [FKD12] = '')"    ' チェックボックス「空白」がONなら、対応者が両方空白を追加
            personCmd = cmn.RegReplace(personCmd, " Or $", "")  ' 末尾の Or を削除
        End If

        ' 各[期間][手法][対応者]条件の結合
        Dim selectCmd As String = ""
        If dateRngCmd.Length > 0 Then selectCmd += $"({dateRngCmd})"
        If methodCmd.Length > 0 Then selectCmd += $" And ({methodCmd})"
        If personCmd.Length > 0 Then selectCmd += $" And ({personCmd})"
        selectCmd = cmn.RegReplace(selectCmd, "^ And ", "")  ' 先頭の And を削除

        Dim dt As DataTable
        If selectCmd.Length > 0 Then
            dt = db.GetSelect(Sqldb.TID.SCD, $"SELECT * FROM {db.GetTable(Sqldb.TID.SCD)} WHERE {selectCmd} ORDER BY FKD03 DESC")
        Else
            dt = db.GetSelect(Sqldb.TID.SCD, $"SELECT * FROM {db.GetTable(Sqldb.TID.SCD)} ORDER BY FKD03 DESC")
        End If
        Return dt
    End Function


    ' 各チェックボックスクリックイベント
    Private Sub CLB_B1_ItemCheck() Handles CLB_RecB1.ItemCheck, CLB_RecB2.ItemCheck
        ' ItemCheckはチェックボックスの変更前にコールされてしまう
        ' チェックボックスの変更後の状態を取得したいから、ItemCheckのイベント終了後に動作させるようにInvokeで遅延させる
        If LockEventHandler_CLB Then Exit Sub
        BeginInvoke(New MethodInvoker(AddressOf ShowDGV5Event))
    End Sub

    ' 概要チェックボックスクリック時のInbokeコール用
    Private Sub ShowDGV5Event()
        ShowDGVList(DGV5, TB_RecA1.Text)
    End Sub

    ' チェックボックスリストの全設定/解除ボタン
    Private Sub CLB_RecE1_AllSet() Handles BT_RecE1.Click
        Static Dim onoff As Boolean = True
        CheckedListBox_AllSet(CLB_RecB1, onoff)
        onoff = Not onoff
    End Sub
    Private Sub CLB_RecE2_AllSet() Handles BT_RecE2.Click
        Static Dim onoff As Boolean = True
        CheckedListBox_AllSet(CLB_RecB2, onoff)
        onoff = Not onoff
    End Sub

    ' チェックボックスリストを全てON/OFFに
    Private Sub CheckedListBox_AllSet(CLB As CheckedListBox, sw As Boolean)
        LockEventHandler_CLB = True                 ' チェックリストボックス操作中にイベント多発しないように防ぐ
        For idx = 0 To CLB.Items.Count - 1
            CLB.SetItemChecked(idx, sw)
        Next
        LockEventHandler_CLB = False
    End Sub

    ' 交渉記録 Excelファイルに出力ボタン
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

        Dim fileName As String = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT3 & Common.FILE_CSVREC
        Dim outFileName As String = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) & "\交渉記録.xlsx"
        ' オーバーレイオープン
        CreatorExpress1.OpenBook(outFileName, fileName)

        ' 値の設定
        Dim itemList As String() = {"顧客番号", "証券番号", "日時", "債務者氏名", "債務者カナ", "相手", "概要", "手法(手段)", "場所", "対応者", "対応者2", "督促状通知", "備考"}
        For itemIdx = 0 To itemList.Length - 1
            CreatorExpress1.Pos(itemIdx, 0).Value = itemList(itemIdx)
        Next

        ' 値を出力
        ' pox(0,n) pox(1,n) .. と順番に出力しないと出力されないっぽい
        For n = 0 To DGV5.Rows.Count - 1
            CreatorExpress1.Pos(0, n + 1).Value = DGV5.Rows(n).Cells(1).Value

            ' アシスト番号
            Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C02 = '{0}'", DGV5.Rows(n).Cells(1).Value))
            If dr.Length > 0 Then
                CreatorExpress1.Pos(1, n + 1).Value = dr(0).Item(11)
            End If

            CreatorExpress1.Pos(2, n + 1).Value = DGV5.Rows(n).Cells(2).Value
            CreatorExpress1.Pos(3, n + 1).Value = DGV5.Rows(n).Cells(3).Value
            CreatorExpress1.Pos(4, n + 1).Value = DGV5.Rows(n).Cells(4).Value
            CreatorExpress1.Pos(5, n + 1).Value = DGV5.Rows(n).Cells(5).Value
            CreatorExpress1.Pos(6, n + 1).Value = DGV5.Rows(n).Cells(6).Value
            CreatorExpress1.Pos(7, n + 1).Value = DGV5.Rows(n).Cells(7).Value
            CreatorExpress1.Pos(8, n + 1).Value = DGV5.Rows(n).Cells(8).Value
            CreatorExpress1.Pos(9, n + 1).Value = DGV5.Rows(n).Cells(9).Value
            CreatorExpress1.Pos(10, n + 1).Value = DGV5.Rows(n).Cells(10).Value
            CreatorExpress1.Pos(11, n + 1).Value = DGV5.Rows(n).Cells(11).Value
            CreatorExpress1.Pos(12, n + 1).Value = DGV5.Rows(n).Cells(12).Value.ToString.Replace(vbCrLf, " ")
        Next
        CreatorExpress1.CloseBook(True)
        MsgBox("ファイル作成が完了しました。")

    End Sub
#End Region

#Region "他"
    ' 顧客情報DBの更新確認、ダウンロード
    Private Sub CheckUpdateCDB()
        Dim DBList As String() = {Sqldb.DB_FKSC, Sqldb.DB_FKSCASSIST}       ' 確認するDBリスト
        For Each dbfile In DBList
            If File.GetLastWriteTime(db.CurrentPath_LO & dbfile) < File.GetLastWriteTime(db.CurrentPath_SV & Common.DIR_UPD & dbfile) Then
                ' サーバーが新しい場合、DBを上書きダウンロード
                log.D(Log.DBG, "DBDownload:" & dbfile)
                log.cLog("DBDownload:" & dbfile)
                File.Copy(db.CurrentPath_SV & Common.DIR_UPD & dbfile, db.CurrentPath_LO & dbfile, True)
            End If
        Next
    End Sub

#End Region

#Region "督促状管理"
    ' 督促管理の初期設定
    Private Sub DunInit()
        ShowDunLB()        ' 督促管理のリストボックス初期設定
        CB_DunA6.SelectedIndex = 0
        CB_DunA7.SelectedIndex = 0
        NUD_DunA1.Value = Today.AddMonths(-1).ToString("MMM")
        If xml.GetUserName <> "" Then TB_DunA4.Text = xml.GetUserName2        ' 最後に書き込んだユーザー名(PC固有)を表示

    End Sub

    ' 1件削除ボタン
    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Dim r = MessageBox.Show("選択中の督促状履歴を削除しますか？" & vbCrLf,
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        DelDun(DGV4.CurrentRow.Cells(0).Value)
    End Sub
    ' 表示全削除ボタン
    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Dim r = MessageBox.Show("表示されている督促履歴 " & DGV4.Rows.Count & "件 を全て削除しますか？" & vbCrLf,
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        DelDun()
    End Sub

    ' 督促状帳票フォルダを開くボタン
    Private Sub Button9_Click_2(sender As Object, e As EventArgs) Handles Button9.Click
        cmn.CreateDir(cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT2)
        Process.Start(cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT2)
    End Sub

    ' 督促顧客リスト(テキストボックス)のDataTable取得
    ' テキストボックスに入力された顧客番号を一行づつselectして1つのDataTableにマージしていく
    Private Function GetDunCosDataTable() As DataTable
        Dim retDt As DataTable = db.OrgDataTablePlusAssist.Clone
        Dim rs As New StringReader(TB_DunIN.Text)
        Dim tmpDt As DataTable          ' マージに使うテンポラリDataTable
        Dim cNo As String

        ' 顧客番号リストのテキストボックスから、顧客番号のみ取得してリスト
        While rs.Peek() > -1
            Dim f35Dr As DataRow()
            Dim assDr As DataRow()
            cNo = rs.ReadLine()
            If cNo = "" Then Continue While                                                                ' 空白なら顧客番号とみなさずスキップ
            If cmn.RegReplace(cNo, "[0-9]", "").Length > 0 Then Continue While                             ' 数字以外が含まれていたら顧客番号とみなさずスキップ

            f35Dr = db.OrgDataTablePlusAssist.Select("[FK02] = '" & cNo & "'")
            If f35Dr.Length = 0 Then
                assDr = db.OrgDataTable(Sqldb.TID.SCAS).Select("[C12] = '" & cNo & "'")
                If assDr.Count = 0 Then Continue While    ' 該当顧客番号がどちらのDBに存在しないならスキップ

                ' 顧客番号欄に入力された番号が、アシスト番号だった場合は、顧客番号に変換して検索する
                f35Dr = db.OrgDataTablePlusAssist.Select("[FK02] = '" & assDr(0).Item(1) & "'")
            End If

            ' 顧客番号から顧客データをDBからSelectで取得
            If f35Dr.Length > 0 Then
                tmpDt = f35Dr.CopyToDataTable             ' 顧客番号に該当するレコード取得してマージ
                retDt.Merge(tmpDt)
            End If
        End While
        Return retDt
    End Function

    ' 発送NGリスト確認ボタン
    Private Sub Button20_Click_1(sender As Object, e As EventArgs) Handles Button20.Click
        ' すでにフォームが開かれているか確認
        For Each form In Application.OpenForms
            If TypeOf form Is SCA1_SendNGList Then
                form.Activate()
                Return
            End If
        Next
        Dim f As New SCA1_SendNGList
        f.ShowInTaskbar = False
        f.TopMost = True  ' 手前に表示
        f.Show()
    End Sub


    ' 督促状対象者追加ボタン
    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ShowDGVList(DGV6)
    End Sub

    ' 印刷ボタン
    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        If DGV6.Rows.Count = 0 Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim f As New VBR_Dun
        f.ShowDialog()
        f.Dispose()
    End Sub

    ' 送付履歴記録ボタン
    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        If DGV6.Rows.Count = 0 Then Exit Sub
        Dim r = MessageBox.Show("督促通知日[ " & DTP_DunA1.Value.ToString("yyyy/MM/dd") & " ]で、督促状の送付履歴を記録しますか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim dNowM As String = DateTime.Now.ToString("yyyyMMddHHmmss_") & DateTime.Now.Millisecond & "_"  ' ユニーク番号を設定するために現在時刻のミリ秒までの値を取得
        Dim readTime As String = Now.ToString("yyyy/MM/dd HH:mm")
        For n = 0 To DGV6.Rows.Count - 1
            Dim Name1 As String = DGV6.Rows(n).Cells(2).Value
            Dim Name2 As String = DGV6.Rows(n).Cells(3).Value
            ' 読み込みデータをDB新規登録
            db.AddSQL(Sqldb.TID.SCD, "Insert Into FKSCD Values('" & dNowM & n & "', '" &                        ' ユニーク番号
                                                     DGV6.Rows(n).Cells(0).Value & "', '" &                     ' 顧客番号
                                                     DTP_DunA1.Value.ToString("yyyy/MM/dd 00:00") & "', '" &    ' 日時 (督促日で記録したいと要望があったので督促日の00:00)
                                                     "契約者（本人）" & "', '" &                                ' 相手
                                                     "ご通知送付" & "', '" &                                    ' 手法（手段）
                                                     TB_DunA4.Text & "', '" &                                   ' 担当者
                                                     TB_DunA5.Text & "', '" &                                   ' 内容
                                                     DTP_DunA1.Value.ToString("yyyy/MM/dd") & "', '" &          ' 督促状通知日
                                                     Name1 & "', '" &                                           ' 債権者名(一覧表示用)
                                                     Name2 & "', '" &                                           ' 債権者名読み(一覧表示用)
                                                     CB_DunA6.Text & "', '" &                                   ' 概要
                                                     "" & "', '" &                                              ' 対応者
                                                     CB_A7.Text & "','" &                                       ' 場所
                                                     "" & "','" &                                               ' 送付先郵便番号
                                                     "" & "','" &                                               ' 送付先住所
                                                     "" & "','" &                                               ' 送付先名前
                                                     "" & "')")                                                 ' 郵便発送種別


            ' 連帯債務者が居ない場合は、主債務者の登録だけして終了
            If DGV6.Rows(n).Cells(8).Value = "" Then Continue For
            ' 「纏める」且つ、同住所の場合は、記録1件で終了
            If CB_DunA7.SelectedIndex = 0 And DGV6.Rows(n).Cells(7).Value = DGV6.Rows(n).Cells(8).Value Then Continue For

            ' 別住所の連帯債務者単独のDB新規登録
            db.AddSQL(Sqldb.TID.SCD, "Insert Into FKSCD Values('" & dNowM & n & "R', '" &                       ' ユニーク番号 +R
                                                     DGV6.Rows(n).Cells(0).Value & "', '" &                     ' 顧客番号
                                                     DTP_DunA1.Value.ToString("yyyy/MM/dd 00:00") & "', '" &    ' 日時 (督促日で記録したいと要望があったので督促日の00:00)
                                                     "連帯債務者" & "', '" &                                    ' 相手
                                                     "ご通知送付" & "', '" &                                    ' 手法（手段）
                                                     TB_DunA4.Text & "', '" &                                   ' 担当者
                                                     TB_DunA5.Text & "', '" &                                   ' 内容
                                                     DTP_DunA1.Value.ToString("yyyy/MM/dd") & "', '" &          ' 督促状通知日
                                                     Name1 & "', '" &                                           ' 債権者名(一覧表示用)
                                                     Name2 & "', '" &                                           ' 債権者名読み(一覧表示用)
                                                     CB_DunA6.Text & "', '" &                                   ' 概要
                                                     "" & "', '" &                                              ' 対応者
                                                     CB_A7.Text & "','" &                                       ' 場所
                                                     "" & "','" &                                               ' 送付先郵便番号
                                                     "" & "','" &                                               ' 送付先住所
                                                     "" & "','" &                                               ' 送付先名前
                                                     "" & "')")                                                 ' 郵便発送種別
        Next
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.SCD)
        'ShowDunLB()
    End Sub

    ' 顧客情報の出力ボタン
    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        Dim f As New SCA1_SelectInfo
        f.ShowDialog(Me)
        f.Dispose()
    End Sub


    ' 督促日時選択イベント
    Private Sub LB_DunRead_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LB_DunRead.SelectedIndexChanged
        ShowDGVList(DGV4)
        L_STS_Dun2.Text = "督促日 " & LB_DunRead.SelectedItem & " の督促件数は " & DGV4.Rows.Count & " 件"
    End Sub

    ' 印刷フォーマット変更イベント
    Private Sub CB_DunA6_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CB_DunA6.SelectedIndexChanged
        If CB_DunA6.SelectedIndex = 3 Then
            L_PrintType.Text = "振替再開日(月)"
        Else
            L_PrintType.Text = "口座引落期限日"
        End If
    End Sub

    ' 督促状削除(1件)
    Private Sub DelDun(id As String)
        Dim sqlCmd As String = "Delete From FKSCD Where FKD01 = '" & id & "'"
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.SCD, sqlCmd)
        'ShowDunLB()
    End Sub
    ' 督促状削除(全件)
    Private Sub DelDun()
        Dim sqlCmd As String
        For n = 0 To DGV4.Rows.Count - 1
            sqlCmd = "Delete From FKSCD Where FKD01 = '" & DGV4(0, n).Value & "'"
            db.AddSQL(Sqldb.TID.SCD, sqlCmd)
        Next
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.SCD)
    End Sub

    ' 督促日リストの表示
    Private Sub ShowDunLB()
        LB_DunRead.Items.Clear()
        'db.UpdateOrigDT(Sqldb.TID.SCD)
        'db.DBFileDL(Sqldb.TID.SCD)                           ' ローカルを最新にする
        Dim dt1 As DataTable = db.GetSelect(Sqldb.TID.SCD, "Select Distinct FKD08 From FKSCD Where FKD08 <> ''")  ' 督促日 の重複削除データ取得
        For Each d In dt1.Rows
            LB_DunRead.Items.Add(d(0))
        Next
        ArrayList.Adapter(LB_DunRead.Items).Reverse()       ' 逆順(降順)
        If LB_DunRead.Items.Count > 0 Then LB_DunRead.SelectedIndex = 0                        ' 先頭(最新)選択
    End Sub

    ' 督促状 選択中の記録内容を表示
    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        If DGV4.Rows.Count = 0 Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ShowSelectUser(DGV4.CurrentRow.Cells(3).Value)
        ShowSelectRecord(DGV4.CurrentRow.Cells(0).Value)
    End Sub

    Private Sub TB_DunIN_Enter(sender As Object, e As EventArgs) Handles TB_DunIN.Enter
        If TB_DunIN.Text = "＜送付対象の顧客番号を入力＞" Then TB_DunIN.Text = ""
    End Sub

    Private Sub DGV6_RowsAdded(sender As Object, e As DataGridViewRowsAddedEventArgs) Handles DGV6.RowsAdded
        L_STS_Dun.Text = "送付対象件数 " & DGV6.Rows.Count & " 件"
    End Sub

#End Region

#Region "物件情報"
    Private Const PI_OFFSET_X As Integer = 2
    Private Const PI_OFFSET_Y As Integer = 2

    ' 物件情報初期設定
    Private Sub FPINFOInit()
        For n = 0 To sccmn.FPITEMLIST.Length - 1
            DGV_PIMENU.Rows.Add()
            DGV_PIMENU(0, n).Value = sccmn.FPITEMLIST(n)
        Next

        Dim firstDayOfMonth As DateTime = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
        DTP_FPST.Value = firstDayOfMonth
        ShowDGV_FPMNG()
        ' 各コンボボックス設定
        LockEventHandler_FP = True
        ' cmn.SetComboBoxUniqueDGVItems(DGV_FPMNG, "C05", CB_FPLIST, "(全表示)")     ' 内容
        CB_FPLIST.Items.Add("(全表示)")
        For n = 1 To sccmn.FPITEMLIST.Length - 1
            CB_FPLIST.Items.Add(sccmn.FPITEMLIST(n))
        Next
        CB_FPLIST.SelectedIndex = 0
        cmn.SetComboBoxUniqueDGVItems(DGV_FPMNG, "C06", CB_FPPerson, "(全表示)")   ' 担当者
        LockEventHandler_FP = False
        AddHandler CB_FPStatus.ItemCheck, AddressOf CB_FPStatus_ItemCheck
    End Sub

    ' イベントハンドラ
    Private Sub RegEventHandlerFP() Handles DTP_FPST.CloseUp, DTP_FPED.CloseUp, CB_FPRangeAll.CheckedChanged, CB_FPPerson.SelectedIndexChanged, CB_FPLIST.SelectedIndexChanged
        If Not LockEventHandler_FP Then ShowDGV_FPMNG()
    End Sub

    Private Sub CB_FPStatus_ItemCheck()
        ' チェックボックスの変更後の状態を取得したいから、ItemCheckのイベント終了後に動作させるようにInvokeで遅延させる
        BeginInvoke(New MethodInvoker(AddressOf ShowDGV_FPMNG))
    End Sub

    ' 物件情報の入力欄を表示
    Private Sub ShowDGV_FPLIST()
        ShowDGV_FPLIST(0)
    End Sub
    Private Sub ShowDGV_FPLIST(pageIndex As Integer)
        ' 債権管理部モード以外は表示されない
        If xml.GetDiv <> Common.DIV.SC Then Exit Sub

        log.TimerST()
        ' ダミー顧客選択では物件情報を非表示
        If DGV1.Rows.Count = 0 OrElse (DGV1.CurrentRow IsNot Nothing AndAlso DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO) Then
            BT_FPSave.Enabled = False
            DGV_FPLIST.Enabled = False
            Exit Sub
        End If
        Dim ccid As String = DGV1.CurrentRow.Cells(0).Value
        Dim piIndex As Integer = DGV_PIMENU.CurrentRow.Index
        Dim keyId As Integer = db.GetFPCOSKeyId(ccid)

        DGV_FPLIST.Enabled = True
        BT_FPSave.Enabled = True

        ' 小項目名をDBから取得して設定
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.FPITEM, $"SELECT {cmn.GetColumnsStr(2, 5)} FROM {db.GetTable(Sqldb.TID.FPITEM)} WHERE C01 = '{DGV_PIMENU.CurrentRow.Index:00}' Order By C02")
        BindDGVList(DGV_FPLIST, dt, 6)
        DGV_FPLIST.AutoGenerateColumns = False
        DGV_FPLIST.DataSource = dt

        Dim dt2 As DataTable
        If piIndex = 0 Then
            ' 基本情報の表示
            dt2 = db.GetSelect(Sqldb.TID.FPCOS, $"SELECT {cmn.GetColumnsStr(4, 15)} FROM {db.DBTbl(Sqldb.TID.FPCOS, Sqldb.DBID.TABLE)} WHERE C02 = '{ccid}'")
            SetFPPage(0, 0)   ' ページボタン非表示
        Else
            ' 基本情報以外の表示
            dt2 = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT {cmn.GetColumnsStr(5, 35)} FROM {db.DBTbl(Sqldb.TID.FPDATA, Sqldb.DBID.TABLE)} WHERE C03 = '{keyId}' AND C04 = '{piIndex - 1}' ORDER BY C01")
            SetFPPage(pageIndex, dt2.Rows.Count)
        End If

        ' 値をDGVに設定
        ' データが無い、もしくはNew PageのときはDBが未保存なので値設定を行わない
        If dt2.Rows.Count > 0 AndAlso dt2.Rows.Count > pageIndex Then
            For n = 0 To DGV_FPLIST.Rows.Count - 1
                DGV_FPLIST.Rows(n).Cells(2).Value = dt2.Rows(GetFPPage())(n)
            Next
        End If
        ' セルの色設定
        ColorSetFPLIST()

        ' ユーザーソート禁止
        For Each c As DataGridViewColumn In DGV_FPLIST.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
        Next c
        log.TimerED("ShowDGV_FPLIST")
    End Sub

    Private Sub DGV_PIMENU_SelectionChanged(sender As Object, e As EventArgs) Handles DGV_PIMENU.SelectionChanged
        ShowDGV_FPLIST()
    End Sub

    ' 保存ボタン
    Private Sub BT_FPSave_Click(sender As Object, e As EventArgs) Handles BT_FPSave.Click
        cmn.StartPBar(2)
        DGV_FPLIST.EndEdit()
        DGV_FPLIST.CurrentCell = DGV_FPLIST(1, 0)
        cmn.UpdPBar("保存中")
        UpdateFPLIST()
        ShowAssignee()      ' 受任マークの表示
        ShowDGV_FPLIST(GetFPPage())
        ShowDGV_FPMNG()
        cmn.EndPBar()
    End Sub

    ' ジャンプボタン
    Private Sub BT_FPMNG_JUMP_Click(sender As Object, e As EventArgs) Handles BT_FPMNG_JUMP.Click
        ' 該当する物件情報を表示
        ShowSelectUser(DGV_FPMNG.CurrentRow.Cells(2).Value, Array.IndexOf(sccmn.FPITEMLIST, DGV_FPMNG.CurrentRow.Cells(4).Value))
    End Sub
    Private Sub BT_FPMNG_JUMP_Click(sender As Object, e As DataGridViewCellEventArgs) Handles DGV_FPMNG.CellDoubleClick, BT_FPMNG_JUMP.Click
        If DGV_FPMNG.Rows.Count = 0 Then Exit Sub
        If e.RowIndex < 0 Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ' 該当する物件情報を表示
        ShowSelectUser(DGV_FPMNG.CurrentRow.Cells(2).Value, Array.IndexOf(sccmn.FPITEMLIST, DGV_FPMNG.CurrentRow.Cells(4).Value))
    End Sub

    ' Excel出力
    Private Sub BT_FPMNG_OutExcel_Click(sender As Object, e As EventArgs) Handles BT_FPMNG_OutExcel.Click
        cmn.ExcelOutputDGV($"融資物件一覧.xlsx", DGV_FPMNG)
    End Sub

    ' Excel全出力
    Private Sub BT_FPMNG_AllOutExcel_Click(sender As Object, e As EventArgs) Handles BT_FPMNG_AllOutExcel.Click
        Dim path As String = cmn.DialogSaveFile("融資物件一覧_全出力.xlsx")
        If path = String.Empty Then Exit Sub
        Dim excelManager As New ExcelManager(path)

        ' TBLテーブルとDATAテーブルから全てのレコードを読み込む
        Dim dtCos As DataTable = db.GetSelect(Sqldb.TID.FPCOS, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPCOS)}")
        Dim dtData As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPDATA)}")
        Dim dtItem As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPITEM)} ORDER BY C01, C02")
        ' 出力するデータのリストを作成
        For sheetNum = 0 To sccmn.FPITEMLIST.Length - 1
            Dim exportSheet As New List(Of List(Of String))

            ' 見出し行を追加
            Dim drItem As DataRow() = dtItem.Select($"C01 = '{sheetNum:00}'")
            Dim topRow As New List(Of String)
            topRow.Add("顧客番号")
            topRow.Add("顧客名")
            For item = 0 To drItem.Length - 1
                topRow.Add(drItem(item)(2))
            Next
            exportSheet.Add(topRow)

            Select Case sheetNum
                Case 0
                    For Each row As DataRow In dtCos.Rows
                        Dim basicRow As New List(Of String)
                        For idx = 1 To row.ItemArray.Length - 1
                            basicRow.Add(row.Item(idx).ToString())
                        Next
                        ' 全てが空欄であれば追加しない
                        Dim isAllEmpty As Boolean = basicRow.All(Function(item) String.IsNullOrWhiteSpace(item))
                        If Not isAllEmpty Then
                            exportSheet.Add(basicRow)
                        End If
                    Next

                Case Else
                    ' 内容(C04)毎にデータ取得して出力
                    Dim contentData As DataRow() = dtData.Select($"C04 = '{sheetNum - 1}'")
                    For row = 0 To contentData.Length - 1
                        ' COSから基本情報を設定
                        Dim exportRow As New List(Of String)
                        Dim rowCos As DataRow = dtCos.Select($"C01 = {contentData(row)(2)}").FirstOrDefault
                        If rowCos IsNot Nothing Then
                            exportRow.Add(rowCos(1))    ' 顧客番号
                            exportRow.Add(rowCos(2))    ' 顧客名
                        End If

                        ' DATAからデータを設定
                        For n = 4 To contentData(row).ItemArray.Length - 1
                            exportRow.Add(contentData(row)(n))
                        Next
                        exportSheet.Add(exportRow)
                    Next
            End Select
            ' Excelにデータを出力
            excelManager.ExportToExcel(exportSheet, sccmn.FPITEMLIST(sheetNum))
        Next
        excelManager.DeleteSheet("Sheet1")
        excelManager.ActivateSheet(sccmn.FPITEMLIST(0))
        excelManager.SaveAndClose()
        excelManager.OpenFile()
    End Sub

    '' Excel全出力
    'Private Sub BT_FPMNG_AllOutExcel_Click(sender As Object, e As EventArgs) Handles BT_FPMNG_AllOutExcel.Click
    '    Dim path As String = cmn.DialogSaveFile("融資物件一覧_全出力.xlsx")
    '    If path = String.Empty Then Exit Sub
    '    Dim excelManager As New ExcelManager(path)

    '    ' TBLテーブルとDATAテーブルから全てのレコードを読み込む
    '    Dim dtCos As DataTable = db.GetSelect(Sqldb.TID.FPCOS, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPCOS)}")
    '    Dim dtData As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPDATA)}")

    '    ' 出力するデータのリストを作成
    '    Dim exportData As New List(Of List(Of String))
    '    Dim colRow As New List(Of String)
    '    ' 見出し行を追加
    '    Dim colNameList As String() = {"", "顧客番号", "顧客名", "居住有無", "融資物件〒", "融資物件住所", "延滞原因", "フラット期失日", "任売同意期限", "競売申立期限", "アシスト期失日", "買戻日", "一部代位日", "フラット管理事務停止日", "アシスト管理事務停止日", "内容", "担当者", "最終対応日", "次回対応日", "ステータス", "概要", "汎用1", "汎用2", "汎用3", "汎用4", "汎用5", "汎用6", "汎用7", "汎用8", "汎用9", "汎用10", "汎用11", "汎用12", "汎用13", "汎用14", "汎用15", "汎用16", "汎用17", "汎用18", "汎用19", "汎用20", "汎用21", "汎用22", "汎用23", "汎用24", "汎用25", "汎用26"}
    '    exportData.Add(colNameList.ToList())

    '    ' DATAのデータを行ごとに処理し、それに基づいてTBLのデータをExcelに出力
    '    For Each rowData As DataRow In dtData.Rows
    '        Dim exportRow As New List(Of String)

    '        ' TBLテーブルから対応するレコードを検索
    '        Dim keyNumber As String = rowData("C03").ToString()
    '        Dim rowTbl As DataRow = dtCos.Select($"C01 = '{keyNumber}'").FirstOrDefault()

    '        If rowTbl IsNot Nothing Then
    '            ' TBLのC01～C15をリストに追加
    '            For i As Integer = 0 To 14
    '                exportRow.Add(rowTbl($"C{i + 1:00}").ToString())
    '            Next
    '        End If

    '        ' DATAテーブルのC04～C35をリストに追加（C04はSCcommon.FPITEMLISTから取得）
    '        For i As Integer = 3 To 34 ' C04～C35
    '            If i = 3 Then ' C04の場合はSCcommon.FPITEMLIST()から文字列を取得
    '                Dim index As Integer = Convert.ToInt32(rowData($"C{i + 1:00}"))
    '                If index >= 0 AndAlso index < sccmn.FPITEMLIST.Length Then
    '                    exportRow.Add(sccmn.FPITEMLIST(index + 1))
    '                Else
    '                    exportRow.Add("") ' 該当するインデックスがない場合は空文字を追加
    '                End If
    '            Else
    '                exportRow.Add(rowData($"C{i + 1:00}").ToString())
    '            End If
    '        Next
    '        ' Excelに出力するためのリストにこの行のデータを追加
    '        exportData.Add(exportRow)
    '    Next

    '    ' Excelにデータを出力し、ファイルを開く
    '    excelManager.ExportToExcel(exportData, "Sheet1")
    '    excelManager.SaveAndClose()
    '    excelManager.OpenFile()
    'End Sub

    ' ページボタン
    Private Sub BT_FP_PAGE_Click(sender As Object, e As EventArgs) Handles BT_FP_PAGE.Click
        If DGV1.Rows.Count = 0 Then Exit Sub
        Dim piIndex As Integer = DGV_PIMENU.CurrentRow.Index
        If piIndex = 0 Then Exit Sub
        Dim ccid As String = DGV1.CurrentRow.Cells(0).Value
        Dim keyId As Integer = db.GetFPCOSKeyId(ccid)

        ' 基本情報以外の表示
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT C01 FROM {db.GetTable(Sqldb.TID.FPDATA)} WHERE C03 = '{keyId}' AND C04 = '{piIndex - 1}' ORDER BY C01")
        Dim maxPageCount As Integer = dt.Rows.Count

        Select Case GetFPPage()
            Case (maxPageCount - 1)
                ' 最大値を超える新規ページの作成
                SetFPPage(GetFPPage() + 1, maxPageCount)
                ' 新規登録用の白紙画面を表示
                For n = 0 To DGV_FPLIST.Rows.Count - 1
                    DGV_FPLIST(2, n).Value = ""
                Next

            Case maxPageCount
                ' 新規登録画面から1ページ目に戻る
                ShowDGV_FPLIST(0)

            Case Else
                ' 通常のカウントアップ
                ShowDGV_FPLIST(GetFPPage() + 1)
        End Select
    End Sub

    ' 入力データをDBに更新
    Private Sub UpdateFPLIST()
        Dim dgv As DataGridView = DGV_FPLIST
        Dim cid As String = DGV1.CurrentRow.Cells(0).Value
        Dim keyId As Long
        Dim piIndex As String = DGV_PIMENU.CurrentRow.Index
        Dim dt As DataTable
        Dim cmd As String

        ' 基本情報があるか確認
        keyId = db.GetFPCOSKeyId(cid)
        If keyId < 0 Then
            ' 基本情報(FPCOS)の新規登録
            Dim cName As String = ""
            Dim dr As DataRow() = db.OrgDataTablePlusAssist.Select($"FK02 = {cid}")
            If dr.Length > 0 Then
                cName = dr(0)(9)  ' 顧客名を取得
            End If
            cmd = $"INSERT INTO {db.GetTable(Sqldb.TID.FPCOS)} (C02, C03) VALUES ('{cid}', '{cName}');"
            log.cLog($"FPCOS : {cmd}")
            keyId = db.ExeSQL(Sqldb.TID.FPCOS, cmd)
        End If
        log.cLog($"keyId : {keyId}")

        ' 選択中の大項目
        Select Case piIndex
            Case 0
                ' 基本情報が編集されたので、基本情報(FPCOS)の更新
                Dim regData As String = $"C02 = '{cid}',"
                For n = 0 To dgv.Rows.Count - 1
                    regData += $"C{(n + 4):00} = '{dgv(2, n).Value}',"
                Next
                regData = cmn.DelLastChar(regData)    ' 末尾のカンマを削除
                cmd = $"UPDATE {db.GetTable(Sqldb.TID.FPCOS)} SET {regData} WHERE C02 = '{cid}';"
                log.cLog($"FPCOS : {cmd}")
                db.ExeSQL(Sqldb.TID.FPCOS, cmd)

            Case Else
                ' データ部があるか確認
                dt = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT C01 FROM {db.GetTable(Sqldb.TID.FPDATA)} WHERE C03 = '{keyId}' AND C04 = '{piIndex - 1}' ORDER BY C01")
                ' データがない、もしくは、新規ページ作成の場合はInsert
                If dt.Rows.Count = 0 Or (GetFPPage() + 1) > dt.Rows.Count Then
                    ' データ部(FPDATA)の新規登録 C02～C04 + 項目数
                    Dim itemCnt As Integer = dgv.Rows.Count
                    Dim regData As String = $"'{Now:yyyy/MM/dd HH:mm}','{keyId}','{piIndex - 1}',"
                    For n = 0 To dgv.Rows.Count - 1
                        regData += $"'{dgv(2, n).Value}',"
                    Next
                    regData = cmn.DelLastChar(regData)    ' 末尾のカンマを削除
                    cmd = $"INSERT INTO {db.GetTable(Sqldb.TID.FPDATA)} ({cmn.GetColumnsStr(2, itemCnt + 4)}) VALUES ({regData});"
                Else

                    ' データ部(FPDATA)の更新
                    Dim regData As String = ""
                    Dim isEmpty As Boolean = True
                    Dim recordId As Integer = dt.Rows(GetFPPage())(0)
                    For n = 0 To dgv.Rows.Count - 1
                        If Not String.IsNullOrWhiteSpace(Convert.ToString(dgv(2, n).Value)) Then isEmpty = False    ' データ無しか確認
                        regData += $"C{(n + 5):00} = '{dgv(2, n).Value}',"
                    Next
                    regData = cmn.DelLastChar(regData)    ' 末尾のカンマを削除
                    If isEmpty Then
                        ' 全てデータ無しの場合はレコード削除
                        ' 複数レコードある場合は、先頭からページ数分の該当レコードが削除対象となる
                        cmd = $"DELETE FROM {db.GetTable(Sqldb.TID.FPDATA)} WHERE C01 = '{recordId}';"
                    Else
                        cmd = $"UPDATE {db.GetTable(Sqldb.TID.FPDATA)} SET {regData} WHERE C01 = '{recordId}';"
                    End If
                End If
                log.cLog($"FPDATA : {cmd}")
                db.ExeSQL(Sqldb.TID.FPDATA, cmd)
        End Select
    End Sub

    ' 物件情報のページ操作
    Private Sub SetFPPage(actIndex As Integer, maxCount As Integer)
        FPPageNo = actIndex
        log.cLog($"Page:{FPPageNo}")
        BT_FP_PAGE.Visible = (maxCount > 0)
        If (actIndex + 1) > maxCount Then
            BT_FP_PAGE.Text = $"New Page"
        Else
            BT_FP_PAGE.Text = $"Page {actIndex + 1} / {maxCount}"
        End If
    End Sub
    Private Function GetFPPage() As Integer
        Return FPPageNo
    End Function

    Private Sub ColorSetFPLIST()
        Dim ccid As String = DGV1.CurrentRow.Cells(0).Value
        Dim keyId As Integer = db.GetFPCOSKeyId(ccid)
        Dim piIndex As Integer = DGV_PIMENU.CurrentRow.Index

        ' 予め背景色を白に設定
        DGV_FPLIST.Columns(2).DefaultCellStyle.BackColor = System.Drawing.Color.White
        ' 各項目ごとの初期セッティング
        Select Case piIndex
            Case 0
                ' 基本物件情報の「先頭1,2,3番目の項目」はフラット35情報を表示（編集不可能）
                If db.OrgDataTablePlusAssist.Select("[FK02] = '" & ccid & "'").Length = 0 Then Exit Sub
                Dim tdt As DataTable = db.OrgDataTablePlusAssist.Select("[FK02] = '" & ccid & "'").CopyToDataTable
                DGV_FPLIST(2, 0).Value = tdt.Rows(0).Item(63).ToString
                DGV_FPLIST(2, 1).Value = tdt.Rows(0).Item(64).ToString
                DGV_FPLIST(2, 2).Value = tdt.Rows(0).Item(65).ToString

                ' 上3段のセルを編集不可・カラーリング
                Dim targetCells As DataGridViewCell() = {DGV_FPLIST(2, 0), DGV_FPLIST(2, 1), DGV_FPLIST(2, 2)}
                For Each cell In targetCells
                    cell.ReadOnly = True
                    cell.Style.BackColor = System.Drawing.Color.LightSalmon
                Next
            Case 3 ' 破産
                DGV_FPLIST(1, 5).Style.BackColor = System.Drawing.Color.DeepSkyBlue
                DGV_FPLIST(1, 6).Style.BackColor = System.Drawing.Color.DeepSkyBlue
                DGV_FPLIST(1, 7).Style.BackColor = System.Drawing.Color.Pink
                DGV_FPLIST(1, 25).Style.BackColor = System.Drawing.Color.Pink
            Case 4 ' 再生
                DGV_FPLIST(1, 5).Style.BackColor = System.Drawing.Color.DeepSkyBlue
                DGV_FPLIST(1, 6).Style.BackColor = System.Drawing.Color.DeepSkyBlue
                DGV_FPLIST(1, 7).Style.BackColor = System.Drawing.Color.Pink
                DGV_FPLIST(1, 21).Style.BackColor = System.Drawing.Color.Pink
        End Select

        ' PIMENUのカラーリング
        For piRow = 0 To DGV_PIMENU.Rows.Count - 1
            DGV_PIMENU.Rows(piRow).DefaultCellStyle.BackColor = System.Drawing.Color.White      ' 初期値の白色に設定

            ' 値が設定されている項目のみ色付け
            If db.GetSelect(Sqldb.TID.FPDATA, $"Select C04 From {db.DBTbl(Sqldb.TID.FPDATA, Sqldb.DBID.TABLE)} Where C03 = '{keyId}' And C04 = '{piRow - 1}'").Rows.Count > 0 Then
                DGV_PIMENU.Rows(piRow).DefaultCellStyle.BackColor = System.Drawing.Color.Orange
            End If
        Next
    End Sub

    ' 記録一覧 表示期間変更イベント
    Private Sub DTP_Rec1ST_CloseUp(sender As Object, e As EventArgs) Handles DTP_Rec1ST.CloseUp, DTP_Rec1ED.CloseUp
        ShowDGVList(DGV5)
    End Sub

    ' 記録一覧 表示期間の全期間チェックボックス選択イベント
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CB_RecRNG.CheckedChanged
        DTP_Rec1ST.Enabled = Not CB_RecRNG.Checked
        DTP_Rec1ED.Enabled = Not CB_RecRNG.Checked
        ShowDGVList(DGV5)
    End Sub

    ' 物件情報の受任者マークの表示設定
    ' 受任者条件 : 再生・破産の受任日が設定されている
    Private Sub ShowAssignee()
        If DGV1.Rows.Count = 0 Then Exit Sub

        Dim cid As String = DGV1.CurrentRow.Cells(0).Value
        ' 受任条件に一致していたら受任ONに設定
        SetAssignee(0, GetAssignee(cid, DGV9(1, 2).Value))      ' 主債務者
        SetAssignee(1, GetAssignee(cid, DGV9(1, 7).Value))      ' 連帯債務者
    End Sub

    ' 受任者判定
    ' [ret]   True : 受任者    False : 非受任者
    ' [cid]   顧客番号
    ' [cName] 主債務者名 or 連帯債務者名
    Public Function GetAssignee(cid As String, cName As String) As Boolean
        If cName.Length = 0 Then Return False       ' 顧客名がない場合は無条件で非受任者

        Dim keyId As Integer = db.GetFPCOSKeyId(cid)
        Dim flagON As Boolean = False
        ' 再生(3)の登録情報を取得
        Dim dtReg As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT C10, C11, C12, C26 FROM {db.GetTable(Sqldb.TID.FPDATA)} WHERE C03 = '{keyId}' AND C04 = '3' AND C10 LIKE '{cName}' ORDER BY C01 DESC")
        If dtReg.Rows.Count > 0 Then
            ' 再生の受任日(C11)があり、再生の辞任日(C12)が無く、
            ' 再生の認可確定日(C26)が無い場合のみ、受任を設定する。
            If dtReg(0)(1).Length > 0 AndAlso dtReg(0)(2).Length = 0 AndAlso dtReg(0)(3).Length = 0 Then
                flagON = True
            End If
        End If

        ' 破産(2)の登録情報を取得
        Dim dtDel As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT C10, C11, C12, C30 FROM {db.GetTable(Sqldb.TID.FPDATA)} WHERE C03 = '{keyId}' AND C04 = '2' AND C10 LIKE '{cName}' ORDER BY C01 DESC")
        If dtDel.Rows.Count > 0 Then
            ' 破産の受任日(C11)があり、破産の辞任日(C12)が無く、
            ' 破産の免責確定日(C30)が無い場合のみ、受任を設定する。
            If dtDel(0)(1).Length > 0 AndAlso dtDel(0)(2).Length = 0 AndAlso dtDel(0)(3).Length = 0 Then
                flagON = True
            End If
        End If
        ' 受任 を返却
        Return flagON
    End Function

    ' 受任者マークのON/OFF
    '   [target] 0: 主債務者 1: 連帯債務者
    '   [sw    ] True: 受任 False: 受任ではない
    Private Sub SetAssignee(target As Integer, sw As Boolean)
        Dim labelList As Label() = {L_JUNIN1, L_JUNIN2}
        If sw Then
            labelList(target).BackColor = System.Drawing.Color.Yellow
            labelList(target).ForeColor = System.Drawing.Color.Black
        Else
            labelList(target).BackColor = System.Drawing.Color.Gainsboro
            labelList(target).ForeColor = System.Drawing.Color.Gray
        End If
    End Sub

    ' 受任ボタン
    Private Sub L_JUNIN1_Click(sender As Object, e As EventArgs) Handles L_JUNIN2.Click, L_JUNIN1.Click
        If DGV1.Rows.Count = 0 Then Exit Sub
        If DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then Exit Sub
        Dim eBt As Label = CType(sender, Label)

        If DGV_PIMENU.CurrentRow.Index <> 4 Then
            DGV_PIMENU(0, 4).Selected = True
            ShowDGV_FPLIST(0)
        Else
        End If

        Select Case eBt.ForeColor
            Case System.Drawing.Color.Gray       ' 受任ONにする
                If DGV_FPLIST(2, 6).Value = "" Then
                    DGV_FPLIST(2, 6).Value = Today.ToString("yyyy/MM/dd")       ' 受任日が空欄なら今日の日付を設定
                End If

                ' 該当者名の設定
                Select Case eBt.Name
                    Case "L_JUNIN1"             ' 主債務者
                        DGV_FPLIST(2, 5).Value = DGV9(1, 2).Value
                    Case Else                   ' 連帯債務者
                        DGV_FPLIST(2, 5).Value = DGV9(1, 7).Value
                End Select
                MsgBox(String.Format("受任に設定します。{0}必要事項(青い項目)を入力して確定してください。{0}赤い項目に入力がある場合は、入力情報を削除することで受任状態になります。", vbCrLf))

            Case System.Drawing.Color.Black        ' 受任OFFにする
                MsgBox(String.Format("受任を解除します。{0}必要事項(赤い項目)のいずれかに入力して確定してください。", vbCrLf))
        End Select
    End Sub

    Private Sub ShowDGV_FPMNG()
        log.TimerST()
        LockEventHandler_FP = True
        Dim dtFPCOS As DataTable = db.GetSelect(Sqldb.TID.FPCOS, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPCOS)}")
        Dim dtFPDATA As DataTable = db.GetSelect(Sqldb.TID.FPDATA, $"SELECT * FROM {db.GetTable(Sqldb.TID.FPDATA)}")
        Dim dgv As DataGridView = DGV_FPMNG

        ' DGVの行をクリア
        dgv.Rows.Clear()
        ' dtFPDATAの行数分だけDGVの行数を追加
        For i As Integer = 0 To dtFPDATA.Rows.Count - 1
            ' 新しい行をDGVに追加
            Dim rowIndex As Integer = dgv.Rows.Add()
            Dim currentRow As DataGridViewRow = dgv.Rows(rowIndex)

            ' dtFPDATAのデータをDGVに設定
            With dtFPDATA.Rows(i)
                currentRow.Cells(0).Value = If(.Table.Columns.Contains("C01"), .Item("C01"), DBNull.Value)                                  ' No
                currentRow.Cells(1).Value = If(.Table.Columns.Contains("C02"), .Item("C02"), DBNull.Value)                                  ' 登録日時
                currentRow.Cells(4).Value = If(.Table.Columns.Contains("C04"), sccmn.FPITEMLIST(cmn.Int(.Item("C04")) + 1), DBNull.Value)   ' 内容
                currentRow.Cells(5).Value = If(.Table.Columns.Contains("C05"), .Item("C05"), DBNull.Value)                                  ' 担当者
                currentRow.Cells(6).Value = If(.Table.Columns.Contains("C06"), .Item("C06"), DBNull.Value)                                  ' ステータス
                currentRow.Cells(7).Value = If(.Table.Columns.Contains("C07"), .Item("C07"), DBNull.Value)                                  ' 概要
                currentRow.Cells(8).Value = If(.Table.Columns.Contains("C08"), .Item("C08"), DBNull.Value)                                  ' 次回対応日
                currentRow.Cells(9).Value = If(.Table.Columns.Contains("C09"), .Item("C09"), DBNull.Value)                                  ' 最終対応日

                ' FPDATAのC03をインデックスとしてFPCOSのデータを検索
                Dim matchingRow = dtFPCOS.Select("C01 = '" & .Item("C03").ToString() & "'").FirstOrDefault()
                If matchingRow IsNot Nothing Then
                    ' 一致するdtFPCOSのレコードがあれば、DGVにC02とC03の値を設定
                    currentRow.Cells(2).Value = matchingRow("C02")                  ' 債権番号
                    currentRow.Cells(3).Value = matchingRow("C03")                  ' 債権者指名
                End If
            End With
        Next
        FilterDGV_FPMNG(DGV_FPMNG, TB_FPMNG_Search.Text)
        dgv.Sort(dgv.Columns(7), ComponentModel.ListSortDirection.Descending)

        If dgv.Rows.Count > 0 Then
            dgv.ClearSelection() ' 既存の選択をクリア
            For Each row As DataGridViewRow In dgv.Rows
                If row.Visible Then
                    dgv.CurrentCell = row.Cells(0) ' 可視の行の最初のセルを現在のセルに設定
                    Exit For
                End If
            Next
        End If
        LockEventHandler_FP = False
        log.TimerED("ShowDGV_FPMNG")
    End Sub

    Private Sub TB_FPMNG_Search_TextChanged(sender As Object, e As KeyPressEventArgs) Handles TB_FPMNG_Search.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            ShowDGV_FPMNG()
        End If
    End Sub

    ' FPMNG 管理表フィルタ
    Private Sub FilterDGV_FPMNG(dgv As DataGridView, words As String)
        dgv.CurrentCell = Nothing ' 現在のセル選択をクリア
        Dim startDate As Date = DTP_FPST.Value.Date
        ' 終了日の時間を23:59に設定
        Dim endDate As Date = DTP_FPED.Value.Date.AddHours(23).AddMinutes(59)
        Dim filterByDate As Boolean = Not CB_FPRangeAll.Checked
        Dim searchHit As Integer = 0

        For Each row As DataGridViewRow In dgv.Rows
            row.Visible = False ' 一旦すべての行を非表示にする
            Dim keywordMatch As Boolean = False
            For Each cell As DataGridViewCell In row.Cells
                If cell.Value IsNot Nothing AndAlso cell.Value.ToString().ToLower().Contains(words.ToLower()) Then
                    keywordMatch = True ' キーワードが含まれている行を表示
                    Exit For ' 一つのセルでキーワードが見つかれば、その行は表示する
                End If
            Next

            ' C05列に対するフィルタリング
            Dim c05Match As Boolean = (CB_FPLIST.SelectedIndex <= 0 OrElse row.Cells("C05").Value.ToString().Equals(CB_FPLIST.SelectedItem.ToString()))

            ' C02列（日付）に対するフィルタリング
            Dim c02Match As Boolean = True
            If filterByDate Then
                Dim rowDate As Date = Date.ParseExact(row.Cells("C02").Value.ToString(), "yyyy/MM/dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)
                c02Match = (rowDate >= startDate AndAlso rowDate <= endDate)
            End If

            ' C06列に対するフィルタリング
            Dim c06Match As Boolean = (CB_FPPerson.SelectedIndex <= 0 OrElse row.Cells("C06").Value.ToString().Equals(CB_FPPerson.SelectedItem.ToString()))

            ' C07列に対するフィルタリング
            Dim c07Match As Boolean = True ' チェックが全てOFFのときは、すべて表示
            If CB_FPStatus.CheckedItems.Count > 0 Then
                c07Match = False ' チェックが1つでもONの場合は、初期値をFalseに設定
                For Each item As String In CB_FPStatus.CheckedItems
                    If row.Cells("C07").Value.ToString().Equals(item) Then
                        c07Match = True
                        Exit For
                    End If
                Next
            End If

            ' 全ての条件が真の場合のみ行を表示
            If keywordMatch AndAlso c05Match AndAlso c02Match AndAlso c06Match AndAlso c07Match Then
                row.Visible = True
                searchHit += 1
            End If
        Next
        L_FPSearchHit.Text = $"{searchHit} 件 表示中"
    End Sub

#End Region

#Region "申請物管理(MR)"
    Private mrcmn As New SCMRcommon
    Private Sub MRInit()
        mrcmn.GetHolidayDate()
        oview = New SCGA_OVIEW
        CB_MRLIST.Items.AddRange(sccmn.MRITEMLIST)
        CB_MRLIST.SelectedIndex = 0
        DTP_MRED.Value = Today.AddYears(1).Date         ' 1年後を指定
        ShowDGVMR()
        TB_MRPaymentDate.Text = Today.Date.ToString("yyyy/MM")


        DivMode(xml.GetDiv)
    End Sub

    ' 部署毎フォーム表示切り替え
    Private Sub DivMode(divNo As Common.DIV)
        cmn.DummyPBar()
        Dim labelStr As String() = {"物件情報", "直近の申請書一覧"}
        Label33.Text = labelStr(divNo)  ' ラベル文字の切り替え
        xml.SetDiv(divNo)               ' 部署xml記録
        db.ExeSQL(Sqldb.TID.USER, $"Update TBL Set C04 = '{CType(divNo, Integer)}' Where C01 = '{My.Computer.Name}'")

        ' 専用タブの表示
        cmn.SetTabVisible(TAB_A1, divNo = Common.DIV.SC, "Tab_3Mng")     ' 管理表
        cmn.SetTabVisible(TAB_A1, divNo = Common.DIV.SC, "Tab_4Dun")     ' 督促状管理
        cmn.SetTabVisible(TAB_A1, divNo = Common.DIV.GA, "Tab_6GA")      ' 申請物管理

        Select Case divNo
            Case Common.DIV.SC
                債権管理部ToolStripMenuItem.Checked = True
                総務課ToolStripMenuItem.Checked = False
            Case Common.DIV.GA
                債権管理部ToolStripMenuItem.Checked = False
                総務課ToolStripMenuItem.Checked = True
                ' FormがまだPanelに追加されていない場合、ここで追加
                If PAN_A.Controls.OfType(Of SCGA_OVIEW).Count() = 0 Then
                    oview.TopLevel = False
                    oview.FormBorderStyle = FormBorderStyle.None
                    oview.Dock = DockStyle.Fill
                    PAN_A.Controls.Add(oview)
                    oview.Show()
                End If
        End Select

        ' Panel内のすべてのコントロールをループ処理
        For Each ctrl As System.Windows.Forms.Control In PAN_A.Controls
            ' Form2のインスタンスを特定
            Dim f2 As SCGA_OVIEW = TryCast(ctrl, SCGA_OVIEW)

            Select Case divNo
                Case Common.DIV.SC  ' 債権管理部
                    If f2 IsNot Nothing Then
                        f2.Visible = False
                        f2.Enabled = False
                    Else
                        ctrl.Visible = True
                        ctrl.Enabled = True
                    End If

                Case Common.DIV.GA  ' 総務課
                    If f2 IsNot Nothing Then
                        f2.Visible = True
                        f2.Enabled = True
                    Else
                        ctrl.Visible = False
                        ctrl.Enabled = False
                    End If
            End Select
        Next
    End Sub

    Public Sub ShowDGVMR() Handles CB_MRLIST.SelectedIndexChanged
        log.TimerST()
        db.UpdateOrigDT(Sqldb.TID.MRM)
        mrcmn.InitDGVInfo(DGV_MR1, Sqldb.TID.MRM, CB_MRLIST.SelectedIndex)      ' 申請物管理DGVのカラム作成
        mrcmn.LoadDGVInfo(DGV_MR1, Sqldb.TID.MR, CB_MRLIST.SelectedIndex)       ' 申請物管理DGVの行データ作成
        ' 完済日フィルタを完済日のカラムがあるときだけ有効
        TB_MRPaymentDate.Enabled = mrcmn.IsColumnsPaymentDate(DGV_MR1)
        ' 申請物の移動先が、送信簿か受領簿の場合は全表示チェックをOFFにしておく。FLS三浦様要望
        If CB_MRLIST.SelectedIndex = SCcommon.MRITEMID.MAIL_RECV Or CB_MRLIST.SelectedIndex = SCcommon.MRITEMID.MAIL_SEND Then
            CB_MRRangeAll.Checked = False
        End If

        FilterMRSearch(TB_MRSearch.Text)
        mrcmn.PaymentDateColor()                ' 完済日が5～13日の間は色付け
        mrcmn.HighlightRows(DGV_MR1, "キャンセル日", "", System.Drawing.Color.DarkGray)
        mrcmn.HighlightRows(DGV_MR1, "ステータス", "完了", System.Drawing.Color.GreenYellow)
        mrcmn.HighlightRows(DGV_MR1, "抹消発送日", "", System.Drawing.Color.GreenYellow)
        mrcmn.HighlightRows(DGV_MR1, "ステータス", "取下げ", System.Drawing.Color.Salmon)
        Select Case CB_MRLIST.SelectedIndex
            Case SCcommon.MRITEMID.REPAY
                ' 支払不可の文字を赤色に変更
                cmn.SetCellFontDGV(DGV_MR1, "F審査結果", "支払不可", fontColor:=System.Drawing.Color.Red)
                cmn.SetCellFontDGV(DGV_MR1, "A審査結果", "支払不可", fontColor:=System.Drawing.Color.Red)
        End Select

        cmn.SetComboBoxUniqueDGVItems(DGV_MR1, "担当者", CB_Person, "(全表示)")   ' 担当コンボボックス設定
        L_STS_MR.Text = " ( " & DGV_MR1.Rows.Count & " ) 件 表示中"
        log.TimerED("ShowDGVMR")
    End Sub

    ' 追加・編集ボタン
    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles BT_MREdit.Click, BT_MRAdd.Click
        ' 何も選択せず編集ボタンを押していたら起動させない
        If sender.Text Is BT_MREdit.Text And DGV_MR1.CurrentRow Is Nothing Then
            Exit Sub
        End If

        Dim fm As Form = SCGA_REG
        fm.ShowDialog(Me)
        fm.Dispose()
    End Sub

    ' 削除ボタン
    Private Sub BT_MRDel_Click(sender As Object, e As EventArgs) Handles BT_MRDel.Click
        If DGV_MR1.CurrentRow Is Nothing Then Exit Sub
        Dim r As Integer
        r = MessageBox.Show("削除してよろしいですか？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        ' DGV2の指定行を削除
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.MR, $"Delete From TBL Where C01 = '{DGV_MR1.CurrentRow.Cells(0).Value}'")
        ShowDGVMR()
    End Sub

    ' 顧客情報の表示ボタン
    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        Dim columnIndex As Integer = cmn.FindColumnIndex(DGV_MR1, "顧客番号")
        If columnIndex = -1 Then Exit Sub ' カラムが見つからなければ何もしない

        ' 選択されている行を取得
        Dim selectedRow As DataGridViewRow = DGV_MR1.CurrentRow
        If selectedRow IsNot Nothing AndAlso Not selectedRow.IsNewRow Then
            ' 選択されている行の「顧客番号」を取得
            Dim cellValue As String = selectedRow.Cells(columnIndex).Value?.ToString()
            If Not String.IsNullOrEmpty(Trim(cellValue)) Then
                ShowSelectUser(cellValue)
            End If
        End If
    End Sub

    Private Sub FilterMRSearch(word As String)
        FilterMRSearch(word, "")
    End Sub
    ' DGV_MRフィルタ
    Private Sub FilterMRSearch(word As String, SelectRegNo As String)
        Dim searchText As String = word.ToLower()
        Dim selectedPerson As String = If(CB_Person.SelectedItem IsNot Nothing, CB_Person.SelectedItem.ToString(), String.Empty)
        Dim startDate As Date = DTP_MRST.Value.Date
        Dim endDate As Date = DTP_MRED.Value.Date
        Dim searchHit As Integer = 0

        ' DataGridViewの各行を走査
        For Each row As DataGridViewRow In DGV_MR1.Rows
            row.Visible = False

            ' 日付の範囲チェック
            If Not CB_MRRangeAll.Checked Then
                Dim dateValue As Date = DateTime.Parse(row.Cells(3).Value.ToString())
                If dateValue < startDate OrElse dateValue > endDate Then Continue For
            End If

            ' "担当者"カラムのフィルタリングを行うかどうかのチェック
            If selectedPerson <> "(全表示)" Then
                Dim person As String = row.Cells("担当者").Value.ToString()
                If Not selectedPerson.Equals(String.Empty) AndAlso Not person.Equals(selectedPerson) Then Continue For
                If selectedPerson.Equals(String.Empty) AndAlso Not person.Equals(String.Empty) Then Continue For
            End If

            ' 文字列の検索
            Dim containsSearchText As Boolean = False
            For Each cell As DataGridViewCell In row.Cells
                If cell.Value IsNot Nothing AndAlso cell.Value.ToString().ToLower().Contains(searchText) Then
                    containsSearchText = True
                    Exit For
                End If
            Next
            ' 検索文字列が含まれていなければ次の行へ
            If Not containsSearchText Then Continue For

            ' 完済日文字列の検索
            Dim columnsIdx As Integer = cmn.GetColumnIndexByName(DGV_MR1, "完済日")
            If columnsIdx >= 0 Then
                If Not row.Cells(columnsIdx).Value.ToString().ToLower().Contains(TB_MRPaymentDate.Text) Then
                    Continue For
                End If
            End If

            ' 条件に一致する行を表示
            row.Visible = True
            searchHit += 1

            ' 行を選択中にする
            If DGV_MR1.CurrentCell Is Nothing And row.Cells(2).Visible Then DGV_MR1.CurrentCell = row.Cells(2)

            ' SelectRegNoに一致する行を選択
            If SelectRegNo.Length > 0 Then
                If row.Cells(0).Value IsNot Nothing AndAlso row.Cells(0).Value.ToString().Equals(SelectRegNo) Then
                    If row.Cells(2).Visible Then DGV_MR1.CurrentCell = row.Cells(2)
                    DGV_MR1.FirstDisplayedScrollingRowIndex = row.Index
                End If
            End If
        Next
        L_MRSearchHit.Text = $"{searchHit} 件 表示中"
    End Sub


    ' OVIEWで選択中のデータに参照移動
    Public Sub ViewSelectedMR(TargetType As String, TargetNo As String)
        ' OVIEWで選択中の申請書種別と申請書登録番号から表示する行を算出
        Dim index As Integer = Array.IndexOf(sccmn.MRITEMLIST, TargetType)
        If index < 0 Then Exit Sub
        cmn.DummyPBar()

        ' 検索でフィルタがかからない状態にしておく
        CB_MRLIST.SelectedIndex = index                     ' 申請書の種類変更
        CB_MRRangeAll.Checked = True                        ' 全期間のチェックON
        CB_Person.Text = "(全表示)"                         ' 担当者を全表示
        TAB_A1.SelectedTab = TAB_A1.TabPages("Tab_6GA")     ' タブ移動

        ' 一致したデータの表示
        FilterMRSearch("", TargetNo)
    End Sub

    ' Excel全出力ボタン
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        MRExcelOutputAll()
    End Sub
    ' 表示出力ボタン
    Private Sub Button2_Click_2(sender As Object, e As EventArgs) Handles Button2.Click
        cmn.ExcelOutputDGV($"申請物管理_{CB_MRLIST.Text}.xlsx", DGV_MR1)
    End Sub

    Private Sub MRExcelOutputAll()
        Dim filePath As String = cmn.DialogSaveFile("申請物管理一覧.xlsx")
        If filePath = String.Empty Then Exit Sub
        Dim excelManager As New ExcelManager(filePath)

        Dim dt As DataTable = db.OrgDataTable(Sqldb.TID.MRM) ' 仮定のDataTable取得

        ' C01の値ごとにフィルタリングし、各シートに出力
        For i As Integer = 0 To 6
            Dim outSheetName As String = i.ToString()
            ' C01の値に基づいてDataTableからデータをフィルタリング
            Dim filteredData As DataTable = dt.Clone() ' スキーマのコピー
            Dim rows() As DataRow = dt.Select("C01 = '" & i & "'", "C01, C02 ASC")
            For Each row As DataRow In rows
                filteredData.ImportRow(row)
            Next

            ' 出力データを横向きに準備（C01とC02は除外し、C03のみを出力）
            Dim outData As New List(Of List(Of String)) From {New List(Of String)}
            For Each dRow As DataRow In filteredData.Rows
                ' 最初のリストにC03の値を追加して、横にデータを展開する
                outData(0).Add(dRow("C03").ToString())
            Next

            ' Excelにシート毎に出力（A1セルから右にデータを展開）
            excelManager.ExportToExcel(outData, outSheetName)
        Next
        ' Excelファイルを保存して閉じる
        excelManager.DeleteSheet("Sheet1")
        excelManager.SaveAndClose()
        excelManager.OpenFile()
    End Sub


    ' 検索欄
    Private Sub TB_MRSearch_TextChanged(sender As Object, e As EventArgs) Handles TB_MRSearch.TextChanged, TB_MRPaymentDate.TextChanged
        FilterMRSearch(TB_MRSearch.Text)
    End Sub
    ' 開始・終了期間
    Private Sub DTP_MRST_CloseUp(sender As Object, e As EventArgs) Handles DTP_MRST.CloseUp, DTP_MRED.CloseUp
        FilterMRSearch(TB_MRSearch.Text)
    End Sub
    ' 全期間チェックボックス
    Private Sub CB_MRRangeAll_CheckedChanged(sender As Object, e As EventArgs) Handles CB_MRRangeAll.CheckedChanged
        FilterMRSearch(TB_MRSearch.Text)
    End Sub
    ' 担当者コンボボックス
    Private Sub CB_Person_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CB_Person.SelectedIndexChanged
        FilterMRSearch(TB_MRSearch.Text)
    End Sub
#End Region

#Region "発送NG"

    ' 発送NGボタン
    Private Sub SendNG_Click(sender As Object, e As EventArgs) Handles L_SENDNG1.Click, L_SENDNG2.Click
        If DGV1.Rows.Count = 0 Then Exit Sub
        If DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then Exit Sub
        Dim eBt As Label = CType(sender, Label)
        Dim cid As String = DGV1.CurrentRow.Cells(0).Value
        Dim state As Integer = GetSendNGState(cid)

        Select Case eBt.Text
            Case "主"
                If L_SENDNG1.BackColor = System.Drawing.Color.Silver Then
                    ' OFF -> ON
                    L_SENDNG1.BackColor = System.Drawing.Color.Red
                    SendNGUpdate(cid, (state Or 1))
                Else
                    ' ON -> OFF
                    L_SENDNG1.BackColor = System.Drawing.Color.Silver
                    SendNGUpdate(cid, (state And Not 1))
                End If
            Case "連"
                If L_SENDNG2.BackColor = System.Drawing.Color.Silver Then
                    ' OFF -> ON
                    L_SENDNG2.BackColor = System.Drawing.Color.Red
                    SendNGUpdate(cid, (state Or 2))
                Else
                    ' ON -> OFF
                    L_SENDNG2.BackColor = System.Drawing.Color.Silver
                    SendNGUpdate(cid, (state And Not 2))
                End If
        End Select
        MsgBox(String.Format("[{0}] の発送NG設定を変更しました。", eBt.Text))
    End Sub

    ' 発送NGの状態更新( 1:主 2:連 3:両方 )
    Private Sub SendNGUpdate(cid As String, val As Integer)
        If db.IsExistREM(cid) Then
            db.ExeSQL(Sqldb.TID.SCR, $"UPDATE FKSCREM SET FKR04 = '{val}' Where FKR01 = '{cid}'")
        Else
            db.ExeSQL(Sqldb.TID.SCR, $"INSERT INTO FKSCREM VALUES('{cid}', '', '', '{val}', '', '')")
        End If
        db.ExeSQL(Sqldb.TID.SCR)
    End Sub

    ' DBから現在の発送NG状態を取得
    Private Function GetSendNGState(cid As String) As Integer
        ' FKSCREMからFKR04を取得する処理
        Dim ret As Integer = 0
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.SCR, $"SELECT FKR04 FROM FKSCREM WHERE FKR01 = '{cid}'")
        If dt.Rows.Count > 0 Then
            ' 過去に使われていたFKR04に残骸が残ってたら、未設定0とする。IntでExceptionでると処理遅延になるから防止してreturn
            If dt(0)(0).ToString.Length > 1 Then Return 0
            ret = cmn.Int(dt(0)(0))
        End If
        Return ret
    End Function

    ' 発送NGのラベルカラー設定
    Private Sub ShowSendNGLabel()
        If DGV1.Rows.Count = 0 Then Exit Sub
        Dim cid As String = DGV1.CurrentRow.Cells(0).Value
        Dim state As Integer = GetSendNGState(cid)
        L_SENDNG1.BackColor = System.Drawing.Color.Silver
        L_SENDNG2.BackColor = System.Drawing.Color.Silver
        Select Case state
            Case 1
                L_SENDNG1.BackColor = System.Drawing.Color.Red
            Case 2
                L_SENDNG2.BackColor = System.Drawing.Color.Red
            Case 3
                L_SENDNG1.BackColor = System.Drawing.Color.Red
                L_SENDNG2.BackColor = System.Drawing.Color.Red
        End Select
    End Sub

#End Region

#Region "MenuItemEvent"
    Private Sub 機能ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 機能ToolStripMenuItem.Click
        Dim fm As Form = SCB1
        fm.ShowInTaskbar = False
        fm.ShowDialog()
        fm.Dispose()
    End Sub

    Private Sub 債権管理部ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 債権管理部ToolStripMenuItem.Click
        DivMode(Common.DIV.SC)
    End Sub

    Private Sub 総務課ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 総務課ToolStripMenuItem.Click
        DivMode(Common.DIV.GA)
    End Sub
    ' ユーザー名設定
    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        Dim fm As Form = SCA_SetUserName
        fm.ShowInTaskbar = False
        fm.ShowDialog()
        fm.Dispose()
    End Sub

    Private Sub データ投入ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles データ投入ToolStripMenuItem.Click
        Dim fm As Form = SCA_InputData
        fm.ShowInTaskbar = False
        fm.ShowDialog()
        fm.Dispose()
    End Sub

#End Region

    ''' <summary>
    ''' SQL ServerとSQLiteそれぞれのFKSCDテーブル全件取得の処理時間を計測し、結果を出力します。
    ''' </summary>
    Public Sub CompareDatabasePerformance()
        Try
            Cursor.Current = Cursors.WaitCursor  ' マウスカーソルを砂時計に
            Dim sqlsv As New Sqlsv

            ' SQL Server側の処理計測
            Dim swSQLServer As Stopwatch = Stopwatch.StartNew()
            ' Sqlsv.vbに定義したGetFKSCD()を呼び出し
            Dim dtSQLServer As DataTable = sqlsv.SqlServerSelectFKSCLog("SELECT * FROM FKSCD")
            swSQLServer.Stop()
            Dim sqlServerTime As Long = swSQLServer.ElapsedMilliseconds

            ' SQLite側の処理計測
            Dim swSQLite As Stopwatch = Stopwatch.StartNew()
            ' Sqldb.vbに定義したGetFKSCD()を呼び出し（※こちらは既存のSQLite用実装）
            Dim dtSQLite As DataTable = db.GetSelect(Sqldb.TID.SCD, "SELECT * FROM FKSCD")
            swSQLite.Stop()
            Dim sqliteTime As Long = swSQLite.ElapsedMilliseconds

            ' 結果をコンソールに出力（必要に応じてMessageBox等での表示も可能）
            MessageBox.Show("SQL Server: " & sqlServerTime.ToString() & " ms" & vbCrLf &
                             "SQLite: " & sqliteTime.ToString() & " ms", "DB Performance Comparison")

        Catch ex As Exception
            Console.WriteLine("エラーが発生しました: " & ex.Message)
        End Try
    End Sub
End Class
