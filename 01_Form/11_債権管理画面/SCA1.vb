Imports System.IO
Imports System.Text
Imports System.Threading

Public Class SCA1

#Region "定義"
    Public CID As String = ""                       ' 顧客番号(機構番号)
    Public addSW                                    ' 追加/編集ボタンの識別子(SCA1_S2で判別)
    Private ReadOnly cmn As New Common
    Private ReadOnly sccmn As New SCcommon
    Private ReadOnly log As New Log
    Private ReadOnly exc As New CExcel
    Public ReadOnly db As New Sqldb
    Private thSC As SCThread
    Private CallerFunc = ""


    ' 着信
    Private ReadOnly TEL_HEADLEN As Integer = 17                    ' 着信ファイルのヘッダ文字数     ※yyyy/MM/dd HH:mm-
    Public FPATH_TEL As String                                      ' 着信ログファイルのフルパス   C:\xxx\TELLOG\ILC_SCTEL.log   SCC1でも使用する
    ' 監視
    Private PoolingStart As Boolean = False                         ' 監視フラグ
    Private Const POLLING_ID_TEL As String = "TEL"                  ' 着信イベントの識別子
    Private Const POLLING_ID_SCD As String = "SCD更新"              ' DB(SCD)更新イベントの識別子
    Private Const POLLING_ID_TASK As String = "タスク更新"          ' タスク更新イベントの識別子
    Private Const POLLING_ID_CUDB As String = "顧客DB更新"          ' 顧客DB更新イベントの識別子
    Private Const POLLING_ID_ASC As String = "A_SC本体"             ' A_SC更新イベントの識別子
    Private Const POLLING_CYCLE As Integer = 500                    ' イベント監視周期(ms)
    ' フリーメモ変更前バッファ(変更されたか検知したい)
    Private BeforeFreeTxt As String = ""
    Private BeforeAddTel As String = ""
    ' タスク
    Private Const NODE_ALL = "node00"                               ' 全体ノードの名称
    Public Const ADDTEL_WORD = "電話番号を追加"                     ' 追加電話番号のテキストボックス文言
    ' 外付けフォーム
    Private SearchForm As SCA1_S3_Search = Nothing                  ' 検索オプションフォーム
    Public AddTelForm As SCA1_S4_TEL = Nothing                      ' 電話追加フォーム
    Public EditForm As SCE_S2 = Nothing                           ' 交渉記録フォーム
    ' イベントハンドラーロック 記録一覧チェックリストボックス
    Private LockEventHandler_CLB As Boolean = False
    'Private LockEventHandler_LCSum As Boolean = False               ' 延滞損害金の表示
    Private MyDBUpdate As Boolean = False                            ' 自分がDB更新したフラグ
    Private PIItemList As String()                                   ' 物件情報の大項目
    ' スレッド
    Private ReadOnly Thread_Entry As Thread = Nothing

#End Region

#Region " OPEN CLOSE "

    Private Sub FK4B_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim loadTime = Date.Now
        Dim xml As New XmlMng
        Me.Text += " - " & xml.GetCPath
        CB_AUTOUPD.Checked = xml.GetAutoUpd
        CB_NOTICETELL.Checked = xml.GetNoticeTell

        FPATH_TEL = db.CurrentPath_SV & Common.DIR_TEL & Common.FILE_TEL                     ' 着信ログまでのフルパス生成
        ' 顧客情報DB(FKSC.DB3,ASSIST.DB3)の最新確認 古ければサーバからダウンロード
        CheckUpdateCDB()

        AddTelInit()

        ' DGV表示設定
        db.DBFileDL(Sqldb.TID.SCD)
        db.DBFileDL(Sqldb.TID.SCTD)
        db.UpdateOrigDT()
        db.UpdateOrigDT_ASsist()
        ShowDGVList(DGV1)
        ShowDGVList(DGV2)
        ShowDGVList(DGV5)           ' 交渉記録
        'ShowDGVList(DGV8)           ' 着信履歴
        ShowDGVList(DGV9)

        ' タブ 記録一覧の初期設定
        CheckedListBoxInit()

        SetToolTips()               ' ツールチップの設定
        TaskListInit()              ' タスクリスト初期設定
        ' DGVちらつき防止
        cmn.SetDoubleBufferDGV(DGV1, DGV2, DGV3, DGV4, DGV5, DGV6, DGV7, DGV8, DGV9)
        DunInit()                   ' 督促管理の初期設定
        ' スレッド生成
        'ThreadInit()

        ' 物件情報初期設定
        PIItemList = {"基本物件情報", "任売", "競売①", "競売②", "再生・破産①", "再生・破産②", "再生・破産③", "差押①", "差押②", "差押③"}
        For nnn = 0 To PIItemList.Length - 1
            DGV_PIMENU.Rows.Add()
            DGV_PIMENU(0, nnn).Value = PIItemList(nnn)
        Next


        ' 解像度が小さいPCは、左端に寄せる (横幅1600px未満なら左寄せ)
        If Screen.PrimaryScreen.Bounds.Width < 1600 Then Me.Left = 0
        log.cLog("--- Load完了: " & (Date.Now - loadTime).ToString("ss\.fff"))
    End Sub

    Private Sub SCA1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' 着信ログ、DBファイル監視開始
        StartWatching()

        ' イベントハンドラ設定
        AddHandler DGV1.CellEnter, AddressOf DGV1_CellEnter
        AddHandler TV_A1.AfterSelect, AddressOf ChangeTaskFilter
        AddHandler CLB_Progress.SelectedIndexChanged, AddressOf ChangeTaskFilter
        AddHandler CLB_Group.SelectedIndexChanged, AddressOf ChangeTaskFilter
        AddHandler CB_Limit.SelectedIndexChanged, AddressOf ChangeTaskFilter

        SearchOptionInit()          ' 検索オプション

        SC.Visible = False
    End Sub

    Private Sub FLS_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopWatching()
        'thSC.Dispose()
        Dim xml As New XmlMng
        xml.SetAutoUpd(CB_AUTOUPD.Checked)
        xml.SetNoticeTell(CB_NOTICETELL.Checked)
    End Sub

#End Region

#Region " UIイベント"
    ' DGV選択時に融資情報を表示
    Private Sub DGV1_CellEnter(sender As Object, e As DataGridViewCellEventArgs) ' Handles DGV1.CellEnter
        Static Dim lastIdx As Integer = -1
        ' DGV範囲外クリックの場合はイベント不要
        If e.RowIndex < 0 Then Exit Sub
        If e.RowIndex = lastIdx Then Exit Sub
        lastIdx = e.RowIndex        ' 最後に選択した位置を保存
        DGV1_ClickShow()
    End Sub
    ' DGV選択時の表示
    Private Sub DGV1_ClickShow()
        ShowDGVList(DGV2)                               ' 交渉記録
        ShowDGVList(DGV7)                               ' 物件情報
        ShowDGVList(DGV9)                               ' 顧客詳細情報
        'ShowInfoDetail()                                ' 債務者情報の表示

        ShowAssignee()                                  ' 物件情報の受任者マークの表示設定
        'Dim a As New SCA1_S4_TEL
        AddTelForm.LoadDB()         ' 追加電話番号のDB読み込み(選択中の顧客)

        L_STS.Text = " ( " & DGV1.Rows.Count & " / " & db.OrgDataTablePlusAssist.Rows.Count & " ) 件 表示中  -  " &
                     DGV1.SelectedRows.Count & " 人を選択中"
    End Sub
    ' DGV2選択時に記録情報を表示
    Private Sub DGV2_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DGV2.CellEnter
        If e.RowIndex < 0 Then Exit Sub             ' DGV範囲外クリックの場合はイベント不要
        TB_Remarks.Text = DGV2.CurrentRow.Cells(8).Value ' 備考内容表示
    End Sub

    ' 追加ボタン 編集ボタン
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles BT_B1.Click, BT_B2.Click
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
        'MyDBUpdate = True
        db.ExeSQL(Sqldb.TID.SCD, "Delete From FKSCD Where FKD01 = '" & id & "'")
        ExUpdateButton()
    End Sub

    ' 更新ボタン DGV1
    Private Sub ExUpdateButton() Handles Button1.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.DBFileFDL(Sqldb.TID.SCD)                     ' ファイル強制ダウンロード
        db.UpdateOrigDT(Sqldb.TID.SCD)
        db.UpdateOrigDT(Sqldb.TID.SCR)

        ShowDGVList(DGV2)
        ShowDGVList(DGV4)
        ShowDGVList(DGV5)
        ShowDunLB()
        'L_UPDDB.Visible = False
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
            log.cLog("検索開始")
            e.Handled = True
            Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
            ShowDGVList(DGV1, TB_SearchInput.Text)
            DGV1_ClickShow()
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
        Dim r = MessageBox.Show("債権管理アプリのアップデートを行います。" & vbCrLf &
                                "「はい」を押すと、アプリを自動で再起動しますのでしばらくお待ち下さい。",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        SC.RestartApl()
    End Sub

    ' 記録一覧 検索でEnterキー
    Private Sub TB_SearchDGV5_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB_RecA1.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
            ShowDGVList(DGV5, TB_RecA1.Text)
        End If
    End Sub

    ' タブ移動時、検索欄選択時に検索オプションを非表示にする
    Private Sub TAB_A1_SelectedIndexChanged() Handles TAB_A1.SelectedIndexChanged, TB_SearchInput.Enter, Tab_4Dun.Enter, Tab_2Record.Enter
        SearchForm.Visible = False
        AddTelForm.Visible = False
    End Sub

    ' 物件情報の日付設定ボタン、DTP
    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DTP_PI1.ValueChanged, BT_PI3.Click
        If DGV7.CurrentCell.ColumnIndex <> 2 Then Exit Sub
        DGV7.CurrentCell.Value = DTP_PI1.Value.ToString("yyyy/MM/dd")
    End Sub

    ' 物件情報の日付空白設定
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles BT_PI2.Click
        If DGV7.CurrentCell.ColumnIndex <> 2 Then Exit Sub
        DGV7.CurrentCell.Value = ""
    End Sub

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles TAB_A1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                cmn.OpenCurrentDir()
            Case Keys.F2
                Button5.Visible = True
            Case Keys.F3
            Case Keys.F4
                ShowDGVList(DGV9)

        End Select
    End Sub

#End Region

#Region "Thread"
    Private Sub ThreadInit()
        thSC = New SCThread(Me)
        thSC.Init()
    End Sub

    ' アプリ更新通知
    Public Sub NoticeUpdateApp(result As Integer)
        log.cLog("EventCB: アプリ更新通知 result: " & result)
        L_UPDMsg.Visible = True
    End Sub

    ' DB更新通知 SCD
    Public Sub NoticeUpdateDB_SCD(result As Integer)
        log.cLog("EventCB: DB-SCD更新通知result: " & result)
        ShowDGVList(DGV2)
        ShowDGVList(DGV4)
        ShowDGVList(DGV5)
    End Sub

    ' DB更新通知 PI
    Public Sub NoticeUpdateDB_PI(result As Integer)
        log.cLog("EventCB: DB-PI更新通知result: " & result)
        ShowDGVList(DGV7)
    End Sub

#End Region

#Region "表示"
    ' 各DGVの表示
    Private Sub ShowDGVList(dgv As DataGridView)
        ShowDGVList(dgv, "")
    End Sub
    Private Sub ShowDGVList(dgv As DataGridView, FilterWord As String)
        ' コーラー関数を保持
        Dim st As New StackTrace()
        Dim caller1 As Reflection.MethodBase = st.GetFrame(1).GetMethod()
        Dim caller2 As Reflection.MethodBase = st.GetFrame(2).GetMethod()
        CallerFunc = caller1.Name & " <- " & caller2.Name

        Dim idx As Integer = 0
        Dim dt As DataTable = Nothing
        Dim bindID As Integer
        log.TimerST()

        If dgv.Rows.Count > 0 Then idx = dgv.CurrentRow.Index           ' 元の選択中行を覚えておく

        Select Case True
            Case dgv Is DGV1                                ' ## 顧客情報タブ リスト
                bindID = 0
                dt = db.OrgDataTablePlusAssist.Copy         ' DataTableをオリジナルからコピー
                ' AddREMtoDGV1(dt)                            ' REMをDGVに追加

                ' ダミー顧客を生成
                Dim newRow As DataRow = dt.NewRow
                With newRow
                    .Item(0) = Common.DUMMY_NO & "_000"
                    .Item(1) = Common.DUMMY_NO
                    .Item(9) = "ダミー"
                    .Item(10) = "ダミー"
                    .Item(54) = "999999999"
                End With
                dt.Rows.Add(newRow)
            Case dgv Is DGV2                                ' ## 顧客情報タブ 交渉記録
                bindID = 1
                ' DGV1の選択した顧客の交渉記録をフィルタ表示
                If DGV1.Rows.Count > 0 Then
                    Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCD).Select("FKD02 = '" & DGV1.CurrentRow.Cells(0).Value & "'")
                    If dr.Length > 0 Then dt = dr.CopyToDataTable
                End If
            Case dgv Is DGV3                                ' ## タスクタブ リスト
                bindID = 2
                dt = db.OrgDataTable(Sqldb.TID.SCTD).Copy   ' DataTableをオリジナルからコピー
                FilterTaskList(dt)                          ' 条件フィルタ
            Case dgv Is DGV4                                ' ## 更新履歴タブ リスト
                bindID = 3
                ' 日時が一致した督促状を取得
                dt = db.GetSelect(Sqldb.TID.SCD, "Select FKD01, FKD03, FKD08, FKD02, FKD09, FKD10, FKD04, FKD07 From FKSCD Where FKD08 <> '' And FKD08 = '" & LB_DunRead.SelectedItem & "'")
                'AddColumnsDun(dt)                           ' 督促管理に残高更新日を追加     処理に時間かかるから一旦削除
            Case dgv Is DGV5                                ' ## 対応一覧タブ リスト
                bindID = 4
                dt = db.OrgDataTable(Sqldb.TID.SCD).Copy    ' DataTableをオリジナルからコピー

                ' 記録一覧を従来のフィルタに戻す
                If CB_RecRe.Checked Then
                    FilterDGV5_old(dt)                          ' 検索条件チェックボックスのフィルタ(従来)
                Else
                    FilterDGV5(dt)                              ' 検索条件チェックボックスのフィルタ
                End If

            Case dgv Is DGV6                                ' ## 督促リスト
                bindID = 5
                dt = GetDunCosDataTable()
            Case dgv Is DGV7                                ' 物件情報
                ' ダミー顧客選択では物件情報を非表示
                If DGV1.Rows.Count = 0 Then
                    BT_PI4FIX.Enabled = False
                    DGV7.Enabled = False
                    Exit Sub
                End If
                If DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then
                    BT_PI4FIX.Enabled = False
                    DGV7.Enabled = False
                    Exit Sub
                End If
                DGV7.Enabled = True
                BT_PI4FIX.Enabled = True
                bindID = 6
                dt = db.GetSelect(Sqldb.TID.PIM, "Select C02, C03, C04, C05 From ITEM Where C01 = '" & DGV_PIMENU.CurrentRow.Index & "' Order By C02")

            Case dgv Is DGV8                                ' 着信履歴

                Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
                Dim showCnt As Integer = 10     ' 着信履歴の表示行数
                log.TimerST()
                dgv.Rows.Clear()

                ' 着信履歴ファイルテキストから読み込み
                Dim lines As IEnumerable(Of String) = File.ReadLines(FPATH_TEL)
                'dt = New DataTable
                'dt = cmn.DataGridViewClone(dt, DGV8)
                Dim tim As String
                Dim tel As String
                If lines.Count = 0 Then Exit Sub
                If showCnt > lines.Count Then showCnt = lines.Count
                For n = 0 To showCnt - 1
                    If lines(n).Length <= (TEL_HEADLEN) Then Continue For           ' ヘッダがないのでスキップ
                    tim = lines(n).Substring(0, TEL_HEADLEN - 1)                          ' 着信時刻取得
                    tel = lines(n).Substring(TEL_HEADLEN, lines(n).Length - TEL_HEADLEN)  ' 電話番号取得

                    dgv.Rows.Add()
                    dgv(0, n).Value = tim
                    dgv(1, n).Value = tel
                Next

                log.TimerED("dgv8-1")
                log.TimerST()
                ' 電話番号をもとに、債務者名と債務者番号を取得
                ' 　顧客情報の電話番号はハイフンありで、受話履歴はハイフンがないので直接の検索できない。
                ' 　受話履歴の末尾5桁をもとにSelectで検索   ※080-1234-5678なら4-5678で顧客情報の電話番号(主債務者TEL,TEL2、連帯債務者TEL,TEL2)から検索する
                Dim telNo As String
                dt = db.GetSelect(Sqldb.TID.SC, "Select FK02, FK10, FK14, FK15, FK34, FK35  From FKSC")

                Dim dr As DataRow()

                For nn = 0 To showCnt - 1
                    telNo = dgv(1, nn).Value
                    Dim telFilter As String = telNo.Substring(telNo.Length - 5).Insert(1, "-").Insert(0, "*")   ' 末尾5桁にハイフンをつけて取得 ※*5-6789
                    dr = dt.Select(String.Format("FK14 like '{0}' OR FK15 like '{0}' OR FK34 like '{0}' OR FK35 like '{0}'", telFilter))
                    log.cLog("len:" & dr.Length)

                    ' 末尾5桁だから複数一致する場合があるので、その中から番号検索して完全一致させる
                    For Each tellno As DataRow In dr
                        If cmn.RegReplace(String.Format("{0},{1},{2},{3}", tellno(2), tellno(3), tellno(4), tellno(5)), "-", "").IndexOf(telNo) >= 0 Then
                            dgv(2, nn).Value = tellno(0)
                            dgv(3, nn).Value = tellno(1)
                        End If
                    Next

                Next
                dgv.Sort(dgv.Columns(0), ComponentModel.ListSortDirection.Descending)

                log.TimerED("ShowDGVList End:" & dgv.Name & " - CallBack: " & CallerFunc)
                Exit Sub

            Case dgv Is DGV9    ' 顧客詳細情報表示
                ' DGV生成
                InitDGV9()

                ' DGV情報表示
                If DGV1.Rows.Count = 0 Then Exit Sub
                Dim cid As String = DGV1.CurrentRow.Cells(0).Value
                ShowAddTel("")  ' 追加電話番号の初期設定
                TB_FreeMemo.Text = ""

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
                    ' dgv(3, 3).Value = sccmn.GetGroupCredit(cInfo.Item(20))              ' 団信加入サイン
                    dgv(3, 5).Value = cInfo.Item(18)                                 ' 勤務先TEL1
                    ' 連帯債務者
                    dgv(1, 6).Value = cInfo.Item(30)                                 ' ヨミカナ
                    dgv(1, 7).Value = cInfo.Item(29)                                 ' 氏名
                    dgv(1, 8).Value = cInfo.Item(35)                                 ' 郵便番号
                    dgv(1, 9).Value = cInfo.Item(36)                                 ' 住所
                    dgv(1, 10).Value = cInfo.Item(37)                                ' 勤務先
                    dgv(3, 6).Value = cInfo.Item(33)                                 ' TEL1
                    dgv(3, 7).Value = cInfo.Item(31)                                 ' 生年月日
                    ' dgv(3, 8).Value = sccmn.GetGroupCredit(cInfo.Item(40))              ' 団信加入サイン
                    dgv(3, 10).Value = cInfo.Item(38)                                ' 勤務先TEL1
                    ' 証券番号(アシスト)
                    Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C02 = '{0}'", cid))
                    If dr.Length > 0 Then dgv(3, 0).Value = dr(0).Item(11)

                    ' DGV9の住所欄の幅が狭いのでテキストボックスにも表示させておく
                    TB_ADDRESS1.Text = dgv(1, 4).Value
                    TB_ADDRESS2.Text = dgv(1, 9).Value

                    ' F35 契約金額
                    Dim repmo As Integer = cmn.Int(cInfo.Item(49))
                    If cInfo.Item(48) IsNot DBNull.Value Then repmo += cmn.Int(cInfo.Item(48))
                    If cInfo.Item(2) = "1" Or cInfo.Item(2) = "3" Then
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
                    End If
                    If cInfo.Item(2) = "2" Or cInfo.Item(2) = "3" Then
                        dgv(6, 0).Value = "アシスト"
                        Dim dtime As DateTime
                        If DateTime.TryParse(cInfo.Item(58), dtime) Then              ' 前リソースが償還回数だった名残りで、日付の場合だけ金消契約日として表示する
                            dgv(6, 1).Value = cInfo.Item(58)                          ' 金消契約日
                        End If
                        dgv(6, 2).Value = cmn.Int(cInfo.Item(59)).ToString("#,0")     ' 貸付金額
                        dgv(6, 3).Value = cmn.Int(cInfo.Item(62)).ToString("#,0")     ' 更新日残高
                        'dgv(6, 4).Value = ""                                         ' 貸付金額(B)
                        'dgv(6, 5).Value = cmn.Int(cInfo.Item(62)).ToString("#,0")     ' 更新日残高(B)
                        'dgv(6, 6).Value = ""                                         ' 残高更新日
                        dgv(6, 7).Value = cmn.Int(cInfo.Item(61)).ToString("#,0")     ' 返済額
                        'dgv(6, 8).Value = ""                                         ' 返済額(B)
                        dgv(6, 9).Value = cmn.Int(cInfo.Item(60)).ToString("#,0")     ' 延滞回数
                        'dgv(6, 10).Value = ""                                         ' 延滞合計額
                    End If
                End If

                ' 追加要素の顧客情報取得
                Dim remDr As DataRow() = db.OrgDataTable(Sqldb.TID.SCR).Select(String.Format("FKR01 = '{0}'", cid))
                If remDr.Count = 1 Then
                    Dim cInfo As DataRow = remDr(0)
                    TB_FreeMemo.Text = cInfo.Item(2)
                    ShowAddTel(cInfo.Item(3))            ' 追加電話番号
                End If

                SearchColor(TB_SearchInput.Text)
                log.TimerED("ShowDGVList End:" & dgv.Name & " - CallBack: " & CallerFunc)
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
            Case dgv Is DGV1
                DGV1.Sort(DGV1.Columns(5), ComponentModel.ListSortDirection.Descending)
                L_STS.Text = " ( " & DGV1.Rows.Count & " / " & db.OrgDataTablePlusAssist.Rows.Count & " ) 件 表示中"
                AddTelForm.LoadDB()                             ' 追加電話番号のDB読み込み(選択中の顧客)
                EnableObjects(dgv.Rows.Count <> 0)              ' もしDGV1のメンバーが0なら編集できなくする
            Case dgv Is DGV2
                TB_Remarks.Text = ""         ' 備考欄初期化
                'LockEventHandler_LCSum = False
                dgv.Sort(dgv.Columns(1), ComponentModel.ListSortDirection.Descending)
                If DGV2.Rows.Count > 0 Then TB_Remarks.Text = DGV2.CurrentRow.Cells(8).Value
            Case dgv Is DGV3
                UpdateStatusBar_Task()
            Case dgv Is DGV4
                dgv.Sort(dgv.Columns(2), ComponentModel.ListSortDirection.Descending)
            Case dgv Is DGV5
                dgv.Sort(dgv.Columns(2), ComponentModel.ListSortDirection.Descending)
                DGV5_CellClick()
                L_STS_Rec.Text = " ( " & DGV5.Rows.Count & " / " & db.OrgDataTable(Sqldb.TID.SCD).Rows.Count & " ) 件 表示中"
            Case dgv Is DGV7
                ReadPIDB()
                PIItemColoring()
                ' ユーザーソート禁止
                For Each c As DataGridViewColumn In dgv.Columns
                    c.SortMode = DataGridViewColumnSortMode.NotSortable
                Next c
            Case Else
        End Select

        log.TimerED("ShowDGVList End:" & dgv.Name & " - CallBack: " & CallerFunc)
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
    ' 検索ワードフィルタ
    Private Sub FilterWordsDGV(ByRef dt As DataTable, FilterWord As String, dgv As DataGridView)
        If FilterWord = "" Then Exit Sub

        log.TimerST()

        ' 検索ワードが追加電話番号でヒットした場合、その顧客番号は次の検索対象チェックで無条件ヒットにする
        ' db.UpdateOrigDT(Sqldb.TID.SCR)
        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.SCR).Select("FKR04 like '%" & FilterWord & "%' Or FKR05 like '%" & FilterWord & "%' Or FKR06 like '%" & FilterWord & "%'")
        log.cLog("検索ワード追加電話番号 cnt:" & dr.Length)

        ' 検索対象チェックがついた情報を1行の文字列sbに設定して、検索ワードがヒットするか確認する
        ' 検索ワードがヒットしなかった場合は、dtの該当行を削除することで除外
        ' ex) sb 「12345678, 山田太郎, ﾔﾏﾀﾞﾀﾛｳ, ...」
        FilterWord = FilterWord.Replace(" ", "").Replace("　", "").Replace(",", "")   ' 検索ワードから半角全角のスペースを除去
        Dim word As String
        Dim sb As New StringBuilder
        For r = dt.Rows.Count - 1 To 0 Step -1

            ' 追加電話番号でヒットした顧客番号を無条件ヒット(Continue)にする
            Dim addTelHit = False
            For addNum = 0 To dr.Length - 1
                If dt.Rows(r).Item(1).ToString = dr(addNum)(0) Then addTelHit = True
            Next
            If addTelHit Then Continue For


            ' 検索したときの検索対象列 DGV毎
            Select Case True
                Case dgv Is DGV1
                    With sb
                        ' 検索オプションによって検索対象を追加する
                        If SearchForm.CB_ID.Checked Then
                            .Append(dt.Rows(r).Item(1).ToString).Append(",")                           ' 機構番号
                        End If
                        If SearchForm.CB_NAME.Checked Then
                            .Append(dt.Rows(r).Item(9).ToString).Append(",")                           ' 債務者 氏名
                            .Append(dt.Rows(r).Item(10).ToString).Append(",")                          ' 債務者 ﾖﾐｶﾅ
                            .Append(dt.Rows(r).Item(29).ToString).Append(",")                          ' 連帯債務者 氏名
                            .Append(dt.Rows(r).Item(30).ToString).Append(",")                          ' 連帯債務者 ﾖﾐｶﾅ
                        End If
                        If SearchForm.CB_TEL.Checked Then
                            .Append(dt.Rows(r).Item(13).ToString).Append(",")                          ' 債務者 TEL1
                            '.Append(dt.Rows(r).Item(14).ToString).Append(",")                          ' 債務者 TEL2
                            .Append(dt.Rows(r).Item(18).ToString).Append(",")                          ' 債務者勤務先 TEL1
                            '.Append(dt.Rows(r).Item(19).ToString).Append(",")                          ' 債務者勤務先 TEL2
                            .Append(dt.Rows(r).Item(33).ToString).Append(",")                          ' 連帯債務者 TEL1
                            '.Append(dt.Rows(r).Item(34).ToString).Append(",")                          ' 連帯債務者 TEL2
                            .Append(dt.Rows(r).Item(38).ToString).Append(",")                          ' 連帯債務者勤務先 TEL1
                            '.Append(dt.Rows(r).Item(39).ToString).Append(",")                          ' 連帯債務者勤務先 TEL2
                            '.Append(dt.Rows(r).Item(67).ToString).Append(",")                          ' 追加電話番号
                            '.Append(dt.Rows(r).Item(66).ToString).Append(",")                          ' 追加電話番号(複数)
                        End If
                        If SearchForm.CB_WORK.Checked Then
                            .Append(dt.Rows(r).Item(16).ToString.Replace("-", "ｰ")).Append(",")        ' 債務者 住所
                        End If
                        If SearchForm.CB_ADDR.Checked Then
                            .Append(dt.Rows(r).Item(37).ToString.Replace("-", "ｰ")).Append(",")        ' 連帯債務者勤務先
                            .Append(dt.Rows(r).Item(17).ToString.Replace("-", "ｰ")).Append(",")        ' 債務者勤務先 
                        End If
                        If SearchForm.CB_REPAY.Checked Then
                            .Append(cmn.Int(dt.Rows(r).Item(48).ToString) + cmn.Int(dt.Rows(r).Item(49).ToString)).Append(",")        ' 返済額
                        End If
                        If SearchForm.CB_BIRTH.Checked Then
                            .Append(dt.Rows(r).Item(11).ToString).Append(",")                          ' 債務者 生年月日
                            .Append(dt.Rows(r).Item(31).ToString).Append(",")                          ' 連帯債務者 生年月日
                        End If

                        .Replace(" ", "").Replace("　", "").Replace("-", "")
                    End With
                    word = sb.ToString
                    sb.Clear()
                Case dgv Is DGV2
                    Exit Sub
                Case dgv Is DGV3
                    word = dt.Rows(r).Item(0).ToString & "," &
                           dt.Rows(r).Item(5).ToString & "," &
                           dt.Rows(r).Item(6).ToString
                    word = word.Replace(" ", "").Replace("　", "").Replace("-", "")
                Case dgv Is DGV4
                    Exit Sub
                Case dgv Is DGV5
                    word = dt.Rows(r).Item(1).ToString & "," &                          ' 顧客番号
                           dt.Rows(r).Item(4).ToString & "," &                          ' 手法
                           dt.Rows(r).Item(8).ToString & "," &                          ' 債務者氏名
                           dt.Rows(r).Item(9).ToString & "," &                          ' 債務者カナ
                           dt.Rows(r).Item(10).ToString                                 ' 概要
                    word = word.Replace(" ", "").Replace("　", "").Replace("-", "")
                Case Else
                    Exit Sub
            End Select
            If (word.IndexOf(FilterWord) < 0) And
               (word.IndexOf(StrConv(FilterWord, VbStrConv.Katakana Or VbStrConv.Narrow)) < 0) Then
                dt.Rows(r).Delete()        ' 含まれていなければ削除(非表示)
            End If
        Next
        log.TimerED("検索終了")
    End Sub

    ' DataTableにFKSCREMを結合
    'Public Sub AddREMtoDGV1(ByRef dt As DataTable)

    '    ' ワード検索の対象に追加電話番号を含めるためにDataTableに追加電話番号などを結合していた。
    '    ' 結合処理にかなり時間がかかるためコメントアウト

    '    log.cLog("DataTable REM連結 Start REM総数: " & db.OrgDataTable(Sqldb.TID.SCR).Rows.Count)
    '    For Each row As DataRow In db.OrgDataTable(Sqldb.TID.SCR).Rows
    '        Dim dDay = row.Item(1)
    '        Dim addTel = row.Item(3)
    '        Dim addTel2 = row.Item(4)
    '        If dDay = "" And addTel = "" And addTel2 = "" Then Continue For           ' 全てブランクなら設定しない
    '        For Each dtrow As DataRow In dt.Rows
    '            If row.Item(0) = dtrow.Item(1) Then  ' 機構番号が一致するデータに設定
    '                dtrow.Item(66) = addTel2         ' DataTableのFK67 に追加電話番号(複数)
    '                dtrow.Item(67) = addTel          ' DataTableのFK68 に追加電話番号
    '                dtrow.Item(69) = dDay            ' DataTableのFK70 に督促日設定
    '                Exit For
    '            End If
    '        Next
    '    Next
    '    log.cLog("DataTable REM連結 End")
    'End Sub

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
#End Region

#Region "検索オプション"
    ' 検索オプションのフォームを生成
    Private Sub SearchOptionInit()
        SearchForm = New SCA1_S3_Search()
        With SearchForm
            .TopLevel = False
            Me.Controls.Add(SearchForm)
            .Show()
            .BringToFront()
            .Left = 62
            .Top = 43
            .Visible = False
        End With
    End Sub

    ' 検索オプションの表示/非表示
    Private Sub ShowSwitchSearchForm() Handles Button3.Click
        SearchForm.Visible = Not SearchForm.Visible
    End Sub

    ' ツールチップの設定
    Private Sub SetToolTips()
        tt1.SetToolTip(TB_SearchInput, "「債権番号」「債務者名」「連帯債務者名」「各電話番号」から検索できます。")   ' ツールチップ
        tt1.SetToolTip(BT_B1, "左の表(債務者一覧)から、追加したい債務者を複数選択して同時に追加できます。" & vbCrLf &
                              "同時に選択するには、Ctrlキーを押しながら左クリックします。")     ' ツールチップ
    End Sub
#End Region

#Region "子フォーム関連"
    Private f1 As SCA1_S1 = Nothing     ' 子フォーム1
    Private f2 As SCA1_S1 = Nothing     ' 子フォーム2
    Private f3 As SCA1_S1 = Nothing     ' 子フォーム3

    ' 疑似着信ボタン
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim dNow As DateTime = DateTime.Now  ' 現在の日時を取得
        Dim rnd As Integer = DateTime.Now.Millisecond Mod db.OrgDataTablePlusAssist.Rows.Count ' 債務者からランダムな値取得
        Dim sw As New StreamWriter(FPATH_TEL, True, Encoding.GetEncoding("shift_jis"))
        sw.WriteLine(dNow.ToString("yyyy/MM/dd HH:mm-") & db.OrgDataTablePlusAssist.Rows(rnd).Item(13).Replace("-", ""))
        sw.Close()
    End Sub

    ' 子フォーム終了イベント受信
    Private Sub F1_closed(sender As Object, e As FormClosedEventArgs)   ' f1が閉じた時
        f1 = Nothing
    End Sub
    Private Sub F2_closed(sender As Object, e As FormClosedEventArgs)   ' f2が閉じた時
        f2 = Nothing
    End Sub
    Private Sub F3_closed(sender As Object, e As FormClosedEventArgs)   ' f3が閉じた時
        f3 = Nothing
    End Sub

    ' 子フォーム作成
    Private Sub CreateChildForm()
        log.cLog("createChildForm")
        If Not CB_NOTICETELL.Checked Then Exit Sub              ' 受話ポップアップ表示のチェックボックスOFFなら非表示

        Static Dim LastTel As String = ""
        If Not File.Exists(FPATH_TEL) Then
            MsgBox("着信を検出しましたが、以下の受信データが見つかりませんでした。" & vbCrLf & FPATH_TEL)
            log.D(Log.ERR, "着信ファイルが見つからない。 " & FPATH_TEL)
            Exit Sub
        End If

        Dim lines As IEnumerable(Of String) = File.ReadLines(FPATH_TEL)
        If lines.Count = 0 Then Exit Sub
        If lines(lines.Count - 1).Length <= (TEL_HEADLEN) Then Exit Sub         ' ヘッダが書かれてなければ読み込めないので終了

        Dim tim = lines(lines.Count - 1).Substring(0, TEL_HEADLEN - 1)                                        ' 着信時刻取得
        Dim tel = lines(lines.Count - 1).Substring(TEL_HEADLEN, lines(lines.Count - 1).Length - TEL_HEADLEN)  ' 電話番号取得
        ' [時刻] と [電話番号] が前回と一緒の場合は、重複イベントとして破棄
        If LastTel = (tim & tel) Then Exit Sub
        LastTel = (tim & tel)   ' 最後に来たイベントを保持しておく

        ' ポップアップの位置調整(メイン画面の現在位置を基準に右側に表示する)
        Dim cleft = Me.Left - 15 + Me.Width
        Dim ctop = Me.Top

        If f1 Is Nothing Then
            Me.f1 = New SCA1_S1()
            AddHandler f1.FormClosed, AddressOf F1_closed           ' フォーム閉じたときのイベント関数登録
            f1.ShowInTaskbar = False                                ' タスクバー非表示
            f1.Show(Me)
            f1.Location = New Point(cleft, ctop)                    ' ポップアップの位置調整
            ShowChildForm(f1, tel, tim)
        ElseIf f2 Is Nothing Then
            Me.f2 = New SCA1_S1()
            AddHandler f2.FormClosed, AddressOf F2_closed
            f2.ShowInTaskbar = False
            f2.Show(Me)
            f2.Location = New Point(cleft, ctop + f2.Size.Height - 10)
            ShowChildForm(f2, tel, tim)
        ElseIf f3 Is Nothing Then
            Me.f3 = New SCA1_S1()
            AddHandler f3.FormClosed, AddressOf F3_closed
            f3.ShowInTaskbar = False
            f3.Show(Me)
            f3.Location = New Point(cleft, ctop + (f3.Size.Height - 10) * 2)
            ShowChildForm(f3, tel, tim)
        Else
            ' 3つ以上はフォーム作らない
        End If
    End Sub

    ' 子フォーム情報表示
    Private Sub ShowChildForm(f As SCA1_S1, num As String, tim As String)
        f.Text &= num
        For Each row As DataRow In db.OrgDataTablePlusAssist.Rows
            ' TEL検索 　前の番号と、後の番号を混ぜて検索しないようにカンマを間に入れる
            Dim searchWord = ("," & row.Item(13) & "," & row.Item(14) & "," & row.Item(18) & "," & row.Item(19) & "," &
                                    row.Item(33) & "," & row.Item(34) & "," & row.Item(38) & "," & row.Item(39) & ",").Replace(" ", "").Replace("-", "")

            If searchWord.IndexOf("," & num & ",") >= 0 Then        ' 完全一致させるためにカンマも含めて検索
                f.recvCID = row.Item(1)      ' 着信識別番号
                f.recvTelNo = num            ' 着信TEL番号

                f.TB_A1.Text = row.Item(9)   ' 債務者氏名
                f.TB_A2.Text = row.Item(10)   ' 債務者氏名(ヨミ)
                f.TB_A3.Text = row.Item(13)   ' 債務者TEL1
                f.TB_A4.Text = row.Item(14)   ' 債務者TEL2
                'f.TB_B1.Text = row.item(29)   ' 連帯債務者氏名
                'f.TB_B2.Text = row.item(30)   ' 連帯債務者氏名(ヨミ)
                f.TB_B3.Text = row.Item(33)  ' 連帯債務者TEL1
                f.TB_B4.Text = row.Item(34)  ' 連帯債務者TEL2
                f.TB_C1.Text = row.Item(54)  ' 延滞合計額
                'f.TB_C2.Text = tim           ' 着信時刻
                Exit For
            End If
        Next

        ' Media.SystemSounds.Asterisk.Play()   ' システム音声
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
            ' 監視するファイルがなければ空ファイルを作成
            If Not File.Exists(FPATH_TEL) Then
                Using fs As FileStream = File.Create(FPATH_TEL)
                End Using
            End If
            ' 監視機能①  Watcherを使った監視    高速だけどsamba非対応            対象：着信ログ
            WatcherFiles()
            ' 監視機能②  周期監視による監視     タイムラグあるけどsambaも対応    対象：着信ログ、DB更新、TaskDB更新
            PoolingFiles()
        Catch ex As Exception
            ' 監視ファイルが無い等の理由で監視できない
        End Try
    End Sub

    ' 監視機能①
    Private Sub WatcherFiles()
        Watcher = New FileSystemWatcher()
        Watcher.Path = db.CurrentPath_SV & Common.DIR_TEL                   ' 監視するパス
        Watcher.NotifyFilter = NotifyFilters.LastWrite Or                   ' 検知条件 最終書き込み時間
        Watcher.Filter = Common.FILE_TEL                                    ' フィルタで監視するファイルを.txtのみにする
        Watcher.IncludeSubdirectories = False                               ' サブディレクトリ以下も監視する
        AddHandler Watcher.Changed, AddressOf Changed                       ' 変更発生時のイベントを定義する　変更時
        Watcher.EnableRaisingEvents = True                                  ' 監視開始  必要がなくなったら監視終了処理(StopWatching)を呼ぶ
    End Sub

    ' 監視機能① イベント受信 (同じイベントが2回発生する仕様あり)
    Delegate Function dele() As Boolean
    Private Sub Changed(ByVal source As Object, ByVal e As FileSystemEventArgs)
        Select Case e.ChangeType
            Case WatcherChangeTypes.Changed
                log.cLog("--- changed 着信検知:")
                Invoke(New MethodInvoker(AddressOf CreateChildForm))
        End Select
    End Sub

    ' 監視機能① 監視停止
    Private Sub StopWatching()
        PoolingStart = False
        If (Not IsNothing(Watcher)) Then
            Watcher.EnableRaisingEvents = False
            Watcher.Dispose()
        End If
    End Sub

    ' 監視機能② ポーリング処理
    Private Sub PoolingFiles()
        ' 監視対象一覧             監視識別子      監視ファイルパス(*付きは複数ファイル)
        '{POLLING_ID_SCD, db.CurrentPath_SV & Common.DIR_LOG & Log.LOGNAME_DB & "_*"},     ' ログ A_SC_DB_*.
        Dim PList(,) As String = {{POLLING_ID_TEL, FPATH_TEL},                                                      ' ログ ILC_SCTEL.log
                                  {POLLING_ID_SCD, db.CurrentPath_SV & Common.DIR_LOG & Log.LOGNAME_DB & "_*"},     ' A_SC_DB.log
                                  {POLLING_ID_TASK, db.CurrentPath_SV & Common.DIR_LOG & Log.LOGNAME_TASK & "_*"},  ' ログ A_SC_Task_*
                                  {POLLING_ID_CUDB, db.CurrentPath_SV & Common.DIR_UPD & Sqldb.DB_FKSC},            ' DB   FKSC.DB3
                                  {POLLING_ID_ASC, db.CurrentPath_SV & Common.DIR_UPD & Common.EXE_NAME}}           ' EXE  A_SC.exe
        ' {POLLING_ID_SCD, db.CurrentPath_SV & Common.DIR_DB3 & Sqldb.DB_FKSCLOG},          ' DB FKSC_LOG.DB3
        Const PL_ID As Integer = 0
        Const PL_FILE As Integer = 1

        ' 監視対象一覧分のタイマー生成
        Dim LastTime(PList.GetLength(0) - 1) As String      ' 最終更新時刻(ファイル更新時に更新)

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
                                Dim fTime As String = File.GetLastWriteTime(f)             ' タイムスタンプ取得
                                'log.DBGLOG("#" & f & "    : " & fTime & ",   lt : " & LastTime(n))
                                If LastTime(n) < fTime Then
                                    ' ファイル更新を検出
                                    If EditForm IsNot Nothing Then
                                        If EditForm.Visible Then Continue For                   ' 交渉記録の編集中は更新しない
                                    End If

                                    log.cLog("cycle - FUPD検出:" & PList(n, PL_ID))
                                    updateFlag = True
                                    LastTime(n) = fTime
                                End If
                            Next

                            If firstCycle Then Continue For      ' 初回はファイル更新時刻の初期設定のために必ず更新が発生する、ただし更新はしない
                            'If MyDBUpdate Then Continue For      ' 自分自身のDB更新だったら更新しない
                            If Not updateFlag Then Continue For

                            log.cLog("cycle - 実行:" & PList(n, PL_ID))
                            Invoke(New delegate_PoolingCallBack(AddressOf PoolingCallBack), PList(n, PL_ID))
                            updateFlag = False
                            MyDBUpdate = False
                        Next
                        firstCycle = False
                        Threading.Thread.Sleep(POLLING_CYCLE)
                    End While
                End Sub
            )
    End Sub

    ' 監視機能② イベント受信 (Pooling)                     
    Delegate Sub delegate_PoolingCallBack(id As String)         ' UIコールバック用Delegate宣言
    Private Sub PoolingCallBack(id As String)
        log.cLog("Pooling 検出: " & id)
        Select Case id
            Case POLLING_ID_TEL     ' 着信ログ
                Invoke(New MethodInvoker(AddressOf CreateChildForm))       ' 子フォーム作成処理
            Case POLLING_ID_SCD     ' SCD DB更新
                Invoke(New MethodInvoker(AddressOf UpdateDB_SCD))          ' SCD更新
            Case POLLING_ID_TASK    ' Task更新
                Invoke(New MethodInvoker(AddressOf UpdateDB_Task))         ' タスク一覧更新
            Case POLLING_ID_CUDB    ' 顧客DB更新
                Invoke(New MethodInvoker(AddressOf DownloadCustomerDB))    ' 顧客DBのダウンロード更新
            Case POLLING_ID_ASC     ' A_SC本体の更新
                Invoke(New MethodInvoker(AddressOf NoticeUpdate_ASC))      ' A_SC更新
        End Select
    End Sub

    ' 他PCでDB更新を通知
    Private Sub UpdateDB_SCD()
        log.cLog("-- UpdateDB_SCD")
        ' L_UPD.Visible = True

        ' 自動更新チェックがONの場合のみ自動更新
        If CB_AUTOUPD.Checked Then
            ExUpdateButton()
        End If

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

    ' DGV1カーソル選択中の債務者の最新督促日を設定
    'Private Sub SetLatestDDay()
    '    log.cLog("最新督促日の設定")
    '    For Each r As DataGridViewRow In DGV1.SelectedRows
    '        Dim id As String = DGV1(0, r.Index).Value
    '        Dim dday As String = ""
    '        ' FKSCDから最新督促日を取得
    '        For Each row As DataRow In db.OrgDataTable(Sqldb.TID.SCD).Rows
    '            If row.Item(1) <> id Then Continue For
    '            If row.Item(7) > dday Then dday = row.Item(7)
    '        Next
    '        ' REMに既にある場合はUPDATE、なければINSERT
    '        If db.IsExistREM(id) Then
    '            db.AddSQL("Update FKSCREM Set FKR02 = '" & dday & "' Where FKR01 = '" & id & "'") ' 督促日更新
    '        Else
    '            db.AddSQL("Insert Into FKSCREM Values('" & id & "','" & dday & "','','','')")     ' 督促日新規登録
    '        End If
    '        DGV1(4, r.Index).Value = dday                                                       ' DGV1の督促通知日を更新
    '    Next
    '    db.ExeSQL(Sqldb.TID.SCR)
    'End Sub

#Region "タスクリスト"
    ' タスクリスト初期設定
    Private Sub TaskListInit()
        SetChedkedList(CLB_Progress)
        SetChedkedList(CLB_Group)
        TV_A1.ExpandAll()                   ' ノード展開
        TV_A1.SelectedNode = TV_A1.TopNode  ' ノードの先頭を選択
        DGV3.ContextMenuStrip = CMenu_DGV3
        CB_Limit.SelectedIndex = 0
    End Sub

    ' タスク表選択時、更新ボタン時にタスク表の最新化
    Private Sub UpdateDB_Task() Handles Tab_3ToDo.Enter, Button2.Click
        log.cLog(" -- UpdateDB_Task")
        db.UpdateOrigDT(Sqldb.TID.SCTD)         ' タスクDB取得
        ShowDGVList(DGV3)                       ' タスクリスト表示
    End Sub

    ' 検索ボタン
    Private Sub Button3_Click_1(sender As Object, e As EventArgs) Handles BT_TaskSearch.Click
        ShowDGVList(DGV3, TB_TaskSeach.Text)          ' タスクリスト表示
    End Sub
    ' 検索でEnterキー
    Private Sub TB_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB_TaskSeach.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            ShowDGVList(DGV3, TB_TaskSeach.Text)          ' タスクリスト表示

        End If
    End Sub
    ' 検索フォーカス
    Private Sub TB_TaskSeach_Enter(sender As Object, e As EventArgs) Handles TB_SearchInput.Click, TB_TaskSeach.Click
        sender.SelectAll()
    End Sub

    ' タスクのフィルタを変更した契機で表示更新
    Private Sub ChangeTaskFilter() ' Handles  TV_A1.AfterSelect, CLB_Progress.SelectedIndexChanged, CLB_Group.SelectedIndexChanged, CB_Limit.SelectedIndexChanged
        ' ShowTaskList()
        ShowDGVList(DGV3)
    End Sub
    Private Sub UpdateTitle() Handles TV_A1.AfterSelect
        L_TITLE.Text = TV_A1.SelectedNode.Text
    End Sub

    ' タスクリストの表示に条件フィルタをかける
    Private Sub FilterTaskList(ByRef dt As DataTable)
        For r = dt.Rows.Count - 1 To 0 Step -1
            ' ノードのフィルタ ノードが指定されているタスクは非表示にする
            Dim nodeID = NODE_ALL
            If TV_A1.SelectedNode IsNot Nothing Then nodeID = TV_A1.SelectedNode.Name
            If nodeID <> NODE_ALL Then
                If nodeID <> dt.Rows(r).Item(1).ToString Then
                    dt.Rows(r).Delete()
                    Continue For
                End If
            End If

            ' 期限
            Dim LimitDate As String = dt.Rows(r).Item(4).ToString
            Select Case CB_Limit.SelectedIndex
                Case 0 ' 全表示
                Case 1 ' 期限切れ
                    If LimitDate >= Date.Today.ToString("yyyy/MM/dd") Or LimitDate = "" Then
                        dt.Rows(r).Delete()
                        Continue For
                    End If
                Case 2 ' 今日まで
                    If LimitDate > Date.Today.ToString("yyyy/MM/dd") Or LimitDate = "" Then
                        dt.Rows(r).Delete()
                        Continue For
                    End If
                Case 3 ' 明日まで
                    If LimitDate > Date.Today.AddDays(1).ToString("yyyy/MM/dd") Or LimitDate = "" Then
                        dt.Rows(r).Delete()
                        Continue For
                    End If
            End Select

            ' 担当

            ' 進捗
            Dim n = 0
            While n < CLB_Progress.Items.Count
                If CLB_Progress.GetItemChecked(n) = False And                       ' チェックがOFFならその進捗をフィルタかける
                   dt.Rows(r).Item(2).ToString = CLB_Progress.Items(n).ToString Then
                    dt.Rows(r).Delete()
                    Continue For
                End If
                n += 1
            End While

            ' 分類
            n = 0
            While n < CLB_Group.Items.Count
                If CLB_Group.GetItemChecked(n) = False And                          ' チェックがOFFならその分類をフィルタかける
                   dt.Rows(r).Item(3).ToString = CLB_Group.Items(n).ToString Then
                    dt.Rows(r).Delete()
                    Continue For
                End If
                n += 1
            End While
        Next
    End Sub

    ' タスク 編集・追加ボタン
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles BT_A1_ADD.Click, BT_A1_EDIT.Click
        If DGV3.RowCount = 0 And sender.text = BT_A1_EDIT.Text Then Exit Sub   ' 編集はタスクがないときは不要

        Dim form = New SCA1_S3()
        If sender.text = BT_A1_EDIT.Text Then       ' 編集
            form.SetItems(DGV3.CurrentRow)          ' 選択中のタスク行をSCA1_S3に送る
        End If
        form.ShowInTaskbar = False
        form.ShowDialog()
        If form.DialogResult = DialogResult.OK Then
            Dim i As S3Items = form.GetItems()              ' 登録or編集で入力した情報取得
            Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

            ' 識別子を取得
            Dim nextNo As Integer = db.GetNextID_TD()

            ' 添付ファイルのアップロード  .\A_SC\File\[識別番号4桁]\ に格納
            Dim FileCheck = ""
            If i.p_file IsNot Nothing Then
                FileCheck = "○"
                For Each f In i.p_file
                    My.Computer.FileSystem.CopyFile(f, db.CurrentPath_SV & Common.DIR_FLE & nextNo.ToString("D4") & "\" & Path.GetFileName(f).ToString)
                Next
            End If

            ' SQL実行 タスクを追加or編集
            If sender.text = BT_A1_ADD.Text Then
                ' 追加      No 分類リスト(NodeID) 進捗 分類 期限 タスク 担当 添付 作成日
                db.ExeSQL(Sqldb.TID.SCTD, "INSERT INTO TODO VALUES('" &
                            nextNo.ToString("D4") & "','" &       ' C01 No
                            i.p_list & "','" &                          ' C02 分類リスト(nodeID)
                            "未完了','" &                               ' C03 進捗 初回は"未"固定
                            i.p_group & "','" &                         ' C04 分類
                            i.p_limit & "','" &                         ' C05 期限
                            i.p_content & "','" &                       ' C06 タスク
                            i.p_person & "','" &                        ' C07 担当
                            FileCheck & "','" &                         ' C08 添付
                            i.p_date &                                  ' C09 作成日
                            "','','','','','','')")
                log.cLog("タスク登録: " & nextNo.ToString("D4"))
            Else
                ' 編集
                db.ExeSQL(Sqldb.TID.SCTD, "UPDATE TODO SET " &
                            "C04 = '" & i.p_group & "'," &
                            "C05 = '" & i.p_limit & "'," &
                            "C06 = '" & i.p_content & "'," &
                            "C07 = '" & i.p_person & "' " &
                            "WHERE C01 = '" & DGV3.CurrentRow.Cells(0).Value & "'")
                log.cLog("タスク更新: " & DGV3.CurrentRow.Cells(0).Value)
            End If
        End If
        form.Dispose()
        db.UpdateOrigDT(Sqldb.TID.SCTD)         ' タスクDB取得
        ShowDGVList(DGV3)           ' タスクリスト表示
    End Sub

    ' タスク 削除ボタン
    Private Sub BT_A1_DEL_Click(sender As Object, e As EventArgs) Handles BT_A1_DEL.Click
        If DGV3.RowCount = 0 Then Exit Sub   ' 削除はタスクがないときは不要
        Dim taskID As String = DGV3.CurrentRow.Cells(0).Value
        Dim r As Integer
        r = MessageBox.Show("削除してよろしいですか？",
                            "ご確認ください",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        Dim ret = db.ExeSQL(Sqldb.TID.SCTD, "Delete From TODO Where C01 = '" & taskID & "'")
        If ret And GetTaskFilePath(taskID) <> "" Then                                   ' DB削除が成功して、添付フォルダが存在するならフォルダ削除
            ' 添付フォルダを削除するとき、削除の代わりにリネームする
            ' ただし、リネームしたフォルダが既に存在したら古いフォルダを先に削除する
            Dim DelFile = GetTaskFilePath(taskID) & Date.Now.ToString("DEL_MMddHHmm")
            If Directory.Exists(DelFile) Then Directory.Delete(DelFile, True)
            Directory.Move(GetTaskFilePath(taskID), GetTaskFilePath(taskID) & Date.Now.ToString("DEL_MMddHHmm"))   ' 削除の代わりに、フォルダ名変更
        End If
        db.UpdateOrigDT(Sqldb.TID.SCTD)         ' タスクDB取得
        ShowDGVList(DGV3)           ' タスクリスト表示
    End Sub

    ' タスクリスト クリック 添付ファイル表示
    Private Sub DGV3_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGV3.CellClick
        If sender.CurrentCell.OwningColumn.Name <> "DGVTask_添付" Then Exit Sub    ' 添付ファイルセルのみ実行
        If DGV3.CurrentRow.Cells("DGVTask_添付").Value = "" Then Exit Sub          ' タスクファイル無いなら終了

        log.cLog("メニュー表示 row:" & e.RowIndex & ", columns:" & e.ColumnIndex)
        Dim TaskID As Integer = DGV3.CurrentRow.Cells("DGVTask_No").Value                       ' タスクの識別子
        If Directory.Exists(GetTaskFilePath(TaskID)) Then Process.Start(GetTaskFilePath(TaskID))
    End Sub

    ' タスクリスト 添付ファイルパス取得 ファイルなければ "" 返却
    Private Function GetTaskFilePath(taskID As Integer) As String
        Dim ret = ""
        If Directory.Exists(db.CurrentPath_SV & Common.DIR_FLE & taskID.ToString("D4")) Then ret = db.CurrentPath_SV & Common.DIR_FLE & taskID.ToString("D4")
        Return ret
    End Function

    ' タスクリスト 内容表示
    Private Sub DGV3_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DGV3.CellEnter
        TB_TaskContext.Text = DGV3.CurrentRow.Cells("DGVTask_タスク").Value
    End Sub

    ' タスクリスト 条件チェックリストの初期設定
    Private Sub SetChedkedList(CheckList As CheckedListBox)
        For cl = 0 To CheckList.Items.Count - 1
            CheckList.SetItemChecked(cl, True)
        Next
    End Sub

    ' 進捗更新 ダブルクリック or 進捗ボタン
    Private Sub DGV3_DoubleClick(sender As Object, e As EventArgs) Handles DGV3.DoubleClick, BT_A1_PROG.Click
        Dim Proggress = "完了"
        If DGV3.CurrentRow.Cells(2).Value = "完了" Then Proggress = "未完了"
        db.ExeSQL(Sqldb.TID.SCTD, "UPDATE TODO SET C03 = '" & Proggress & "' WHERE C01 = '" & DGV3.CurrentRow.Cells(0).Value & "'")
        db.UpdateOrigDT(Sqldb.TID.SCTD)         ' タスクDB取得
        ShowDGVList(DGV3)                       ' タスクリスト表示
    End Sub

    ' タスクリストのステータスバー表示
    Private Sub UpdateStatusBar_Task()
        Dim dt As DataTable = db.OrgDataTable(Sqldb.TID.SCTD).Copy               ' 現在のDataTableをコピー
        Dim total As Integer = dt.Rows.Count
        Dim comp As Integer = 0
        Dim todayl As Integer = 0
        For r = dt.Rows.Count - 1 To 0 Step -1
            If dt.Rows(r).Item(2).ToString = "完了" Then comp += 1 : Continue For
            If dt.Rows(r).Item(4).ToString = Date.Today.ToString("yyyy/MM/dd") Then todayl += 1
        Next
        L_STS_Task.Text = "[本日期限の未完了タスク数]　" & todayl & "件　　[合計未完了タスク数]　" & (total - comp) & "件"
    End Sub

#End Region

#Region "追加電話番号"
    ' 追加電話番号のテキストボックス設定
    Private Sub TB_B11_Enter(sender As Object, e As EventArgs) Handles TB_B11.Enter
        ' テキストボックスが選択されたら定型文を消す
        sender.ForeColor = System.Drawing.Color.Black
        If sender.Text = ADDTEL_WORD Then
            sender.Text = ""
        End If
        BeforeAddTel = sender.Text  ' 編集前の電話番号を保存
    End Sub
    Private Sub TB_B11_Leave(sender As Object, e As EventArgs) Handles TB_B11.Leave
        If BeforeAddTel = sender.Text Then Exit Sub    ' 電話番号が変更されていないならそのまま終了

        ' テキストボックスから離れたときに空白なら定型文を表示
        If sender.Text = "" Then
            sender.ForeColor = System.Drawing.Color.DarkGray
            sender.Text = ADDTEL_WORD
            Exit Sub
        End If

        ' 電話番号を設定したらDB保存 FKR04
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim id As String = DGV1.CurrentRow.Cells(0).Value
        ' REMに既にある場合はUPDATE、なければINSERT
        If db.IsExistREM(id) Then
            db.ExeSQL(Sqldb.TID.SCR, "Update FKSCREM Set FKR04 = '" & sender.Text & "' Where FKR01 = '" & id & "'")
        Else
            db.ExeSQL(Sqldb.TID.SCR, "Insert Into FKSCREM Values('" & id & "','','','" & sender.Text & "','')")     ' FKSCREM更新
        End If
        'db.WriteHistory(id, DGV1.CurrentRow.Cells(1).Value, "追加電話番号", "編集", sender.Text)
        db.UpdateOrigDT(Sqldb.TID.SCR)
    End Sub

    ' 追加電話番号の表示
    Private Sub ShowAddTel(word As String)
        If word = "" Then
            TB_B11.ForeColor = System.Drawing.Color.DarkGray
            TB_B11.Text = ADDTEL_WORD
        Else
            TB_B11.ForeColor = System.Drawing.Color.Black
            TB_B11.Text = word
        End If
    End Sub

    ' 追加電話番号フォームを生成
    Private Sub AddTelInit()
        AddTelForm = New SCA1_S4_TEL()
        With AddTelForm
            .TopLevel = False
            Me.Controls.Add(AddTelForm)
            .Show()
            .BringToFront()
            .Left = 870
            .Top = 26
            .Visible = False
        End With
        'AddTelForm.LoadDB()         ' 追加電話番号のDB読み込み(選択中の顧客)
    End Sub

    ' 検索オプションの表示/非表示
    Private Sub ShowAddTelForm() Handles L_TELADD.MouseEnter
        If Not AddTelForm.Visible Then AddTelForm.Visible = True
    End Sub

    ' 顧客詳細情報DGV9の整形
    Private Sub InitDGV9()
        Dim dgv As DataGridView = DGV9
        If dgv.Rows.Count = 0 Then
            ' 新規DGV成形
            Dim ItemNames As String(,) = {
                    {"債権番号", "", "証券番号", "", "契約種別", ""},
                    {"ﾖﾐｶﾅ", "", "TEL", "", "金消契約日", ""},
                    {"債務者", "", "生年月日", "", "貸付金額", ""},
                    {"郵便番号", "", "団信加入", "", "残高", ""},
                    {"住所", "", "", "", "貸付金額(B)", ""},
                    {"勤務先", "", "勤務先TEL", "", "残高(B)", ""},
                    {"ﾖﾐｶﾅ", "", "TEL", "", "残高更新日", ""},
                    {"連債者", "", "生年月日", "", "返済額", ""},
                    {"郵便番号", "", "団信加入", "", "返済額(B)", ""},
                    {"住所", "", "", "", "延滞月数", ""},
                    {"勤務先", "", "勤務先TEL", "", "延滞合計額", ""}
                }
            For row = 0 To ItemNames.GetLength(0) - 1
                dgv.Rows.Add()
                For col = 0 To ItemNames.GetLength(1) - 1
                    dgv(col, row).Value = ItemNames(row, col)
                Next
            Next

            ' DGVデザイン
            dgv.Columns(0).DefaultCellStyle.BackColor = System.Drawing.Color.Gainsboro
            dgv.Columns(2).DefaultCellStyle.BackColor = System.Drawing.Color.Gainsboro
            dgv.Columns(4).DefaultCellStyle.BackColor = System.Drawing.Color.Gainsboro
            dgv.Rows(0).DividerHeight = 1
            dgv.Rows(5).DividerHeight = 1
            dgv.Columns(3).DividerWidth = 1
        Else
            ' 顧客情報のみクリア
            Dim clearCooumns() As Integer = {1, 3, 5, 6}
            For Each col In clearCooumns
                For n = 0 To dgv.Rows.Count - 1
                    dgv(col, n).Value = ""
                Next
            Next
            TB_ADDRESS1.Text = ""
            TB_ADDRESS2.Text = ""

        End If
    End Sub

#End Region

    ' 指定した加入者情報を表示する DGV1
    Public Sub ShowSelectUser(cid As String)
        ' フィルタかけられて、DGVに非表示になっていたら予め解除しておく
        Dim dt As DataTable = CType(DGV1.DataSource, DataTable)
        If dt.Select("[FK02] = '" & cid & "'").Length = 0 Then
            TB_SearchInput.Text = ""
            ShowDGVList(DGV1, "")
        End If

        ' 検索して表示
        For Each row As DataGridViewRow In DGV1.Rows
            If row.Cells(0).Value = cid Then DGV1.CurrentCell = DGV1(1, row.Index) : Exit For
        Next
        TAB_A1.SelectedTab = Tab_1SC
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
            db.AddSQL(sqlCmd)
        Next
        db.ExeSQL(Sqldb.TID.SCD)
        ShowDGVList(DGV5)
    End Sub

    ' 記録一覧フィルタ ShowDGVListにコールされる(古いバージョン)
    Private Sub FilterDGV5_old(ByRef dt As DataTable)
        ' チェック全解除の場合は0行で表示
        If CLB_RecB1.CheckedItems.Count = 0 Or CLB_RecB2.CheckedItems.Count = 0 Then dt.Rows.Clear() : Exit Sub

        ' #### -----「手法」チェックボックスのフィルタ  ※チェックのない項目を削り落とすフィルタ方法
        Dim filter As String = ""
        ' 「その他」OFFなら、まずCLBの項目が含まれたものだけにする。含まれないものは予め除いておく
        If Not CLB_RecB1.CheckedItems.Contains("その他") Then
            Dim tmpf As String = ""
            For n = 0 To CLB_RecB1.Items.Count - 1
                tmpf += "[FKD05] like '%" & CLB_RecB1.Items(n) & "%' Or "
            Next
            tmpf = RegularExpressions.Regex.Replace(tmpf, " Or $", "")  ' 末尾の or を削除
            dt = dt.Select(tmpf).CopyToDataTable
        End If
        ' 基本表示させるが、チェックボックスがOFFならその項目をフィルタする
        For n = 0 To CLB_RecB1.Items.Count - 1
            If Not CLB_RecB1.GetItemChecked(n) Then filter += "[FKD05] not like '%" & CLB_RecB1.Items(n) & "%' And "
        Next

        ' フィルタ実行(フィルタの結果0件ならフィルタかけずに0行表示)
        filter = RegularExpressions.Regex.Replace(filter, " And $", "")  ' 末尾の And を削除
        If dt.Select(filter).Length = 0 Then
            dt.Rows.Clear()                     ' selectの結果が0件だとエラーになるから、その場合はrows.clearで回避して何も表示しない
        Else
            dt = dt.Select(filter).CopyToDataTable
        End If
        ' 「架電」OFF「架電×」ONのとき、「架電×」もフィルタかけられてしまっているので、「架電×」だけを追加
        If Not CLB_RecB1.CheckedItems.Contains("架電") And CLB_RecB1.CheckedItems.Contains("架電×") Then
            If db.OrgDataTable(Sqldb.TID.SCD).Select("[FKD05] like '*架電×*'").Length > 0 Then
                Dim addDt As DataTable = db.OrgDataTable(Sqldb.TID.SCD).Select("[FKD05] like '*架電×*'").CopyToDataTable
                dt.Merge(addDt)
            End If
        End If


        ' #### -----「対応者」チェックボックスのフィルタ  ※チェックある項目のみ表示するフィルタ方法
        Dim filter2 As String = ""
        ' 対応者、対応者2共に含まれていなければ排除
        For n = 0 To CLB_RecB2.Items.Count - 1
            If CLB_RecB2.Items(n) = "" Then Continue For    ' チェックボックス「空白」は後でチェックするからパス
            If CLB_RecB2.GetItemChecked(n) Then filter2 += "([FKD06] = '" & CLB_RecB2.Items(n) & "' Or [FKD12] = '" & CLB_RecB2.Items(n) & "') Or "
        Next
        If CLB_RecB2.GetItemChecked(0) Then filter2 += "([FKD06] = '' And [FKD12] = '')"    ' チェックボックス「空白」がONなら対応者が両方空白を追加

        ' フィルタ実行 (フィルタの結果0件ならフィルタかけずに0行表示)
        'filter2 = filter & filter2
        filter2 = RegularExpressions.Regex.Replace(filter2, " Or $", "")  ' 末尾の Or を削除
        If dt.Select(filter2).Length = 0 Then
            dt.Rows.Clear()                     ' selectの結果が0件だとエラーになるから、その場合はrows.clearで回避して何も表示しない
        Else
            dt = dt.Select(filter2).CopyToDataTable
        End If
    End Sub

    ' 記録一覧フィルタ ShowDGVListにコールされる
    Private Sub FilterDGV5(ByRef dt As DataTable)
        Dim dr As DataRow()
        ' チェック全解除の場合は0行で表示
        If CLB_RecB1.CheckedItems.Count = 0 Or CLB_RecB2.CheckedItems.Count = 0 Then
            dt.Rows.Clear()
            Exit Sub
        End If

        ' 表示期間 範囲設定
        Dim dateRngCmd As String = String.Format("[FKD03] >= '{0}' And [FKD03] <= '{1}'", DTP_Rec1ST.Value.ToShortDateString, DTP_Rec1ED.Value.ToString("yyyy/MM/dd 23:59:59"))
        If CB_RecRNG.Checked Then dateRngCmd = ""     ' 全期間チェックボックスONなら、期間フィルタをかけない

        ' 手法チェックボックス 範囲設定
        Dim methodCmd As String = ""
        For n = 0 To CLB_RecB1.Items.Count - 1
            If CLB_RecB1.GetItemChecked(n) Then methodCmd += "[FKD05] like '%" & CLB_RecB1.Items(n) & "%' Or "
        Next
        methodCmd = cmn.RegReplace(methodCmd, " Or $", "")  ' 末尾の Or を削除

        ' 対応者チェックボックス 範囲設定
        Dim personCmd As String = ""
        For n = 0 To CLB_RecB2.Items.Count - 1
            If CLB_RecB2.Items(n) = "" Then Continue For    ' チェックボックス「空白」は後でチェックするからパス
            If CLB_RecB2.GetItemChecked(n) Then personCmd += "([FKD06] = '" & CLB_RecB2.Items(n) & "' Or [FKD12] = '" & CLB_RecB2.Items(n) & "') Or "   ' 担当者・対応者 共に含まれていなければ排除
        Next
        If CLB_RecB2.GetItemChecked(0) Then personCmd += "([FKD06] = '' And [FKD12] = '')"    ' チェックボックス「空白」がONなら、対応者が両方空白を追加
        personCmd = cmn.RegReplace(personCmd, " Or $", "")  ' 末尾の Or を削除

        ' 各[期間][手法][対応者]条件の結合
        Dim selectCmd = String.Format("({0}) And ({1}) And ({2})", dateRngCmd, methodCmd, personCmd)
        selectCmd = cmn.RegReplace(selectCmd, "\(\) And", "")  ' 上の条件で、いずれかのCmdが空白の場合にエラーになってしまうので防止
        selectCmd = cmn.RegReplace(selectCmd, "And \(\)", "")

        dr = dt.Select(selectCmd)
        If dr.Length = 0 Then
            dt.Rows.Clear()
            Exit Sub
        End If
        dt = dr.CopyToDataTable
        Exit Sub
    End Sub


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
        CLB_B1_ItemCheck()  ' 最後に1回だけは変更後のイベントを発生させる
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
        Dim xml As New XmlMng
        If xml.xmlData.UserName <> "" Then TB_DunA4.Text = xml.xmlData.UserName2        ' 最後に書き込んだユーザー名(PC固有)を表示

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
            db.AddSQL("Insert Into FKSCD Values('" & dNowM & n & "', '" &                                       ' ユニーク番号
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
            db.AddSQL("Insert Into FKSCD Values('" & dNowM & n & "R', '" &                                  ' ユニーク番号 +R
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
            db.AddSQL(sqlCmd)
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

    Private Sub DGV_PIMENU_SelectionChanged(sender As Object, e As EventArgs) Handles DGV_PIMENU.SelectionChanged
        ShowDGVList(DGV7)
    End Sub

    ' 確定ボタン（なくても良さそうなら削除）
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles BT_PI4FIX.Click
        DGV7.EndEdit()
        DGV7.CurrentCell = DGV7(1, 0)
        UpdatePIDB()
        PIItemColoring()    ' 物件データの項目名カラーリング
        ShowAssignee()      ' 受任マークの表示
        ShowDGVList(DGV7)
        MsgBox("編集を確定しました。")
    End Sub

    ' 入力データをDBに更新
    Private Sub UpdatePIDB()
        Dim dgv As DataGridView = DGV7
        Dim ccid As String = DGV1.CurrentRow.Cells(0).Value
        Dim clmName As String = "C" & (DGV_PIMENU.CurrentRow.Index + 2).ToString("D2")
        Dim regText As String = ""
        Dim cmd As String = ""
        For n = 0 To dgv.Rows.Count - 1
            regText += String.Format("{0}`", dgv(2, n).Value)
        Next
        log.cLog("[UpdatePIDB] regText:" & regText)

        ' 該当顧客1名の物件情報データを検索or取得して、新規登録or更新
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.PI, "Select * From " & db.DBTbl(Sqldb.TID.PI, Sqldb.DBID.TABLE) & " Where C01 = '" & ccid & "'")
        If dt.Rows.Count = 0 Then
            ' 存在しないので新規登録
            ' 新規登録時に、PIM(マスター)を参照して各物件情報の項目数分の区切り文字 ` を入れておく。区切りがないとExcel出力できない
            cmd = String.Format("Insert Into [TBL] Values('{0}',", ccid)
            Dim dtm As DataTable = db.GetSelect(Sqldb.TID.PIM, "Select C01 From ITEM  Order By C01, C02")           ' マスター項目を取得して各項目の数を取得する
            Dim cMax As Integer = dtm.Compute("MAX(C01)", Nothing)                                                  ' C01の最大値を取得
            For n = 0 To cMax
                Dim prep As New String(Common.PARTITION, dtm.Select("C01 = '" & n & "'").Length)                    ' 項目(C02)の数だけパーティション記号 ` をつける
                cmd += String.Format("'{0}',", prep)
            Next
            cmd = RegularExpressions.Regex.Replace(cmd, ",$", "")  ' 末尾の , を削除
            cmd += ");"        ' 未使用のC09,C10分
            log.cLog("[UpdatePIDB] 新規登録 cmd: " & cmd)
        End If
        ' (新規作成後に)更新
        cmd += String.Format("Update [TBL] Set {0} = '{1}' Where C01 = '{2}';", clmName, regText, ccid)
        log.cLog("[UpdatePIDB] 更新 cmd: " & cmd)
        db.ExeSQL(Sqldb.TID.PI, cmd)
    End Sub

    ' 入力値を読み込む
    Private Sub ReadPIDB()
        Dim ccid As String = DGV1.CurrentRow.Cells(0).Value
        ' 入力値をDBから読み出す
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.PI, "Select * From " & db.DBTbl(Sqldb.TID.PI, Sqldb.DBID.TABLE) & " Where C01 = '" & ccid & "'")
        If dt.Rows.Count = 1 Then
            Dim words As String() = dt.Rows(0).Item(DGV_PIMENU.CurrentRow.Index + 1).ToString.Split("`")
            ReDim Preserve words(UBound(words) - 1)                             ' words末尾の余白を-2削除
            For n = 0 To words.Length - 1
                ' DataTableにBindさせるには以下のコードを使う。 bind設定に C4 を追加することで設定完了
                ' ただしBindさせると入力値欄でnullを設定するとエラーが発生してしまうので、暫定でDGVに直書きしている。
                'dt.Rows(n).Item(3) = words(n)          
                DGV7.Rows(n).Cells(2).Value = words(n)      'DGV直書き (DGVソートが出来なくなる)
                ' log.cLog(String.Format("[ReadPIDB]  words({0}): {1}", n, words(n)))
            Next
        End If

        DGV7.Columns(2).DefaultCellStyle.BackColor = Color.White                            ' 予め背景色を白に設定
        ' 各項目ごとの初期セッティング
        Select Case DGV_PIMENU.CurrentRow.Index
            Case 0
                ' 基本物件情報の「先頭1,2,3番目の項目」はフラット35情報を表示（編集不可能）
                If db.OrgDataTablePlusAssist.Select("[FK02] = '" & ccid & "'").Length = 0 Then Exit Sub
                Dim tdt As DataTable = db.OrgDataTablePlusAssist.Select("[FK02] = '" & ccid & "'").CopyToDataTable
                If Not tdt.Rows.Count = 1 Then Exit Sub  ' データがない場合は読み込みしないで終了
                DGV7(2, 0).Value = tdt.Rows(0).Item(63).ToString
                DGV7(2, 1).Value = tdt.Rows(0).Item(64).ToString
                DGV7(2, 2).Value = tdt.Rows(0).Item(65).ToString

                ' 上3段のセルを編集不可・カラーリング
                DGV7(2, 0).ReadOnly = True
                DGV7(2, 1).ReadOnly = True
                DGV7(2, 2).ReadOnly = True
                DGV7(2, 0).Style.BackColor = Color.LightSalmon
                DGV7(2, 1).Style.BackColor = Color.LightSalmon
                DGV7(2, 2).Style.BackColor = Color.LightSalmon

            Case 4, 5, 6
                ' 再生・破産のカラーリング
                DGV7(1, 0).Style.BackColor = Color.DeepSkyBlue
                DGV7(1, 1).Style.BackColor = Color.DeepSkyBlue
                DGV7(1, 9).Style.BackColor = Color.Pink
                DGV7(1, 17).Style.BackColor = Color.Pink
                DGV7(1, 18).Style.BackColor = Color.Pink
        End Select

    End Sub

    ' Excel出力
    Private Sub Button9_Click_1(sender As Object, e As EventArgs) Handles BT_PI5OUT.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim f As New SCA1_S5_ExcOut
        f.ShowDialog()
        f.Dispose()
        Exit Sub
    End Sub

    ' Excel 読込ボタン
    Private Sub BT_PI6READ_Click(sender As Object, e As EventArgs) Handles BT_PI6READ.Click
        Dim fpath As String = cmn.DialogReadFile("A_SC物件情報.xlsx")
        If fpath <> "" Then
            ReadPIExcel(fpath)
        End If
    End Sub

    Private Sub DGV7_DragEnter(sender As Object, e As DragEventArgs) Handles DGV7.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            'ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
            e.Effect = DragDropEffects.Copy
        Else
            'ファイル以外は受け付けない
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub DGV7_DragDrop(sender As Object, e As DragEventArgs) Handles DGV7.DragDrop
        'ドロップされたすべてのファイル名を取得する
        Dim dFile As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())
        If Path.GetExtension(dFile(0)) <> ".xlsx" Then
            MsgBox("エクセルファイル(拡張子が.xlsx)ではないので読み込めません。")
            Exit Sub
        End If
        ReadPIExcel(dFile(0))
    End Sub

    ' 物件情報エクセルファイルの読み込み
    Private Sub ReadPIExcel(fname As String)
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.UpdateOrigDT(Sqldb.TID.PI)

        Dim rData As String(,) = exc.ReadExc(fname, "Sheet1")         ' エクセルデータ取得
        If rData Is Nothing Then Exit Sub
        Dim maxCol As Integer = rData.GetLength(0)                    ' 最大列数(列)
        Dim maxRow As Integer = rData.GetLength(1)                    ' 最大行数(行)
        Dim cosCnt As Integer = maxRow - 3                            ' 顧客数

        Dim r As Integer = MessageBox.Show(cosCnt & "件分の物件情報を読み込みます。", "ご確認ください", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        ' Excel1列目を右側に、大項目(基本物件情報など)を検索してリストボックスと名称が一致したら、各顧客分の読み込みを実行する
        ' 読み込む小項目の数は、次の大項目が見つかるまでの空白の個数。（もし項目数オーバーの場合はエラー通知）
        ' リストボックスに見つからない大項目だった場合は、ユーザーに項目名が変わったことを告げる

        ' 1行目の大項目名だけ抽出
        Dim itemList As New List(Of String)
        For idx = 0 To maxCol - 1 - PI_OFFSET_X
            If rData(idx + PI_OFFSET_X, 0) <> "" Then itemList.Add(rData(idx + PI_OFFSET_X, 0))
        Next

        Dim cmd As String = ""
        Dim insCnt As Integer = 0
        Dim updCnt As Integer = 0
        ' 顧客毎に読み込む
        For cosNum = 0 To cosCnt - 1
            Dim readExcelIdx As Integer = 0                                 ' Excel読み取り位置(Columun)
            Dim ccid As String = rData(0, cosNum + PI_OFFSET_Y)             ' 顧客番号取得
            Dim cmdArr(11) As String                                        ' SQLコマンドのデータ部  要素数 10 はPIテーブル数
            If ccid = "" Then Continue For                                  ' 念の為顧客番号が空白ならスキップ

            ' コマンドのフォーマット部を作成
            If db.OrgDataTable(Sqldb.TID.PI).Select("[C01] = '" & ccid & "'").Length = 0 Then
                ' 該当顧客番号が存在しないので新規登録
                cmd += "Insert Into TBL Values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}');"
                insCnt += 1
            Else
                ' 該当顧客番号が既に存在するので更新登録
                cmd += "Update TBL Set C02='{1}',C03='{2}',C04='{3}',C05='{4}',C06='{5}',C07='{6}',C08='{7}',C09='{8}',C10='{9}',C11='{10}' " &
                       "Where C01='{0}';"
                updCnt += 1
            End If

            ' 大項目毎に読み込む
            Dim itemIdxL As Integer
            For itemNum = 0 To itemList.Count - 1
                itemIdxL = Array.IndexOf(PIItemList, itemList(itemNum))
                If itemIdxL < 0 Then
                    ' 大項目名がリストボックスに見当たらない。(項目名が変わった可能性)
                    MsgBox("読み込んだエクセルの項目が変わっている可能性があります。" & vbCrLf &
                           "以下の項目が物件情報に見当たらないため、読み込みを中止します。" & vbCrLf & vbCrLf & itemList(itemNum))
                    Exit Sub
                End If

                Dim itemCntS As Integer = db.OrgDataTable(Sqldb.TID.PIM).Select("[C01] = '" & itemIdxL & "'").Length          ' 項目名マスタDBから、指定大項目に該当する小項目数を取得
                For idx = 0 To itemCntS - 1
                    cmdArr(itemIdxL) += rData(readExcelIdx + idx + PI_OFFSET_X, cosNum + PI_OFFSET_Y) & "`"
                Next idx
                readExcelIdx += itemCntS
            Next itemNum

            ' コマンドのデータ部を作成
            cmd = String.Format(cmd,
                            ccid,                          ' 01 顧客番号
                            cmdArr(0),                     ' 02 物件情報
                            cmdArr(1),                     ' 03 任売情報 
                            cmdArr(2),                     ' 04 競売情報1 
                            cmdArr(3),                     ' 05 競売情報2 
                            cmdArr(4),                     ' 06 個人再生・破産情報1
                            cmdArr(5),                     ' 07 個人再生・破産情報2
                            cmdArr(6),                     ' 08 個人再生・破産情報3
                            cmdArr(7),                     ' 09 差押1 
                            cmdArr(8),                     ' 10 差押2
                            cmdArr(9))                     ' 11 差押3
        Next cosNum
        cmd = cmn.RegReplace(cmd, "C[0-9][0-9]='`*'[,]", "")         ' Excelの値がない部分は上書き(空白)しない。「C09=''」 の文言を置換削除
        cmd = cmn.RegReplace(cmd, ",Where", " Where")               ' 上の置換で、「,Where」となる場合SQL構文エラーになるのでカンマを取り除く
        db.ExeSQL(Sqldb.TID.PI, cmd)
        Console.WriteLine("cmd:" & cmd)
        ShowDGVList(DGV7)
        MsgBox("読み込み完了")
    End Sub

    ' 物件情報の項目が存在する場合にカラーリング
    Private Sub PIItemColoring()
        For n = 0 To DGV_PIMENU.Rows.Count - 1
            DGV_PIMENU.Rows(n).DefaultCellStyle.BackColor = System.Drawing.Color.White
        Next
        If DGV1.Rows.Count = 0 Then Exit Sub
        If DGV1.CurrentRow.Cells(0).Value = "" Then Exit Sub
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.PI, "Select C02, C03, C04, C05, C06, C07, C08, C09, C10, C11 From TBL Where C01 = '" & DGV1.CurrentRow.Cells(0).Value & "'")
        If dt.Rows.Count = 1 Then
            For n = 0 To DGV_PIMENU.Rows.Count - 1
                If cmn.RegReplace(dt.Rows(0).Item(n), Common.PARTITION, "").Length > 0 Then DGV_PIMENU.Rows(n).DefaultCellStyle.BackColor = System.Drawing.Color.Orange
            Next
        End If
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
        log.cLog("ShowAssignee 受任マーク設定")

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
        Dim itemIdx1() As Integer = {6, 7, 8}       ' 大項目番号
        Dim NameIdx As Integer = 0                  ' 該当者名識別番号
        Dim OnIdx As Integer = 1                    ' 受任者ONにする条件の識別番号
        Dim OffIdx() As Integer = {9, 17, 18}       ' 受任者OFFにする条件の識別番号

        If cName.Length = 0 Then Return False       ' 顧客名がない場合は無条件で非受任者

        ' 大項目番号リストを全てチェック
        '   受任者は主債務者と連帯債務者の場合があるので、複数個の情報があっても全て有効。
        ' 　同じ該当者に、受任ON 受任OFFが混在してた場合は、受任ONが優先。
        For num = 0 To itemIdx1.Length - 1
            ' 大項目番号の情報取得
            Dim col As String = "C" & (itemIdx1(num)).ToString("00")
            Dim dt As DataTable = db.GetSelect(Sqldb.TID.PI, String.Format("Select {0} From TBL Where C01 = '{1}'", col, cid))
            If dt.Rows.Count = 0 Then Continue For

            ' 小項目ごとの情報取得
            Dim words As String() = dt.Rows(0).Item(0).ToString.Split("`")

            ' 該当者名が設定されてなければ受任ONにしない
            If words(NameIdx).Length = 0 Then Continue For
            ' 受任日が設定されてなければ受任ONにしない
            If words(OnIdx).Length = 0 Then Continue For

            ' 受任OFFの項目に設定されているものがあれば受任ONにしない
            Dim offNum = 0
            While (offNum <= OffIdx.Length - 1)
                If words(OffIdx(offNum)).Length > 0 Then Continue For
                offNum += 1
            End While

            ' 該当者名に主債務者名、もしくは連帯債務者のcNameが含まれていれば受任者ON
            ' カタカナを半角→全角、スペース(空白)を削除　して、該当者名に含まれているか比較
            If cmn.RegReplace(StrConv(words(NameIdx), VbStrConv.Wide), "　", "").IndexOf(cmn.RegReplace(cName, "　", "")) >= 0 Then
                ' 受任者を返却
                Return True
            End If
        Next

        Return False
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

    ' 受信履歴更新ボタン
    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        ShowDGVList(DGV8)
    End Sub

    ' 受任ボタン
    Private Sub L_JUNIN1_Click(sender As Object, e As EventArgs) Handles L_JUNIN1.Click, L_JUNIN2.Click
        If DGV1.Rows.Count = 0 Then Exit Sub
        If DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then Exit Sub
        Dim eBt As Label = CType(sender, Label)

        ShowDGVList(DGV7)
        HintSet("必要事項を入力して、編集確定ボタンを押してください。")

        ' 再生・破産1～3以外を選択してたら再生・破産1を選択中にする
        Select Case DGV_PIMENU.CurrentRow.Index
            Case 4, 5, 6
            Case Else : DGV_PIMENU(0, 4).Selected = True : ShowDGVList(DGV7)
        End Select

        Select Case eBt.ForeColor
            Case System.Drawing.Color.Gray       ' 受任ON

                If DGV7(2, 1).Value = "" Then DGV7(2, 1).Value = Today.ToString("yyyy/MM/dd")       ' 受任日が空欄なら今日の日付を設定
                DGV7(2, 2).Selected = True
                DGV7.BeginEdit(True)

                Select Case eBt.Name
                    Case "L_JUNIN1"             ' 主債務者
                        DGV7(2, 0).Value = DGV9(1, 2).Value
                    Case Else                   ' 連帯債務者
                        DGV7(2, 0).Value = DGV9(1, 7).Value
                End Select
                MsgBox(String.Format("受任に設定します。{0}必要事項(青い項目)を入力して確定してください。{0}赤い項目に入力がある場合は、入力情報を削除することで受任状態になります。", vbCrLf))

            Case System.Drawing.Color.Black        ' 受任OFF
                DGV7(2, 2).Selected = True

                MsgBox(String.Format("受任を解除します。{0}必要事項(赤い項目)のいずれかに入力して確定してください。", vbCrLf))
        End Select



    End Sub

    Private Sub HintSet(str As String)
        L_STS.Text = str
    End Sub

    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        Dim f As New SCE_S1
        f.Show()
    End Sub

#End Region



End Class
