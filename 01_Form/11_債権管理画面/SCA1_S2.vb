﻿Imports System.IO

Public Class SCE_S2

    Private ReadOnly cmn As New Common
    Private ReadOnly xml As New XmlMng
    Private DGV2cid As String
    Private CRow As DataGridViewRow
    Private addSW As Boolean
    Public PrinfFileName As String     ' 帳票手動選択印刷のファイル名　VBR_Sendで参照
    Public EXCPATH_FMT As String
    Public EXCPATH_OUT As String


#Region " OPEN CLOSE "
    Private Sub FLS_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        T4Init()
        ' 部署別 帳票パス設定
        If xml.GetDiv = Common.DIV.SC Then
            EXCPATH_FMT = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT1
            EXCPATH_OUT = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCOUT1
        Else
            EXCPATH_FMT = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMTGA
            EXCPATH_OUT = cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCOUTGA
        End If
        ShowSendFmtList()
        RadioButton1_CheckedChanged()
        ' 初期設定
        addSW = SCA1.addSW                                          ' 追加or編集ボタンのフラグ True:追加  False:編集
        CRow = SCA1.DGV1.CurrentRow                                 ' 追加or編集ボタンを押した時点での顧客情報を保持(あとで顧客選択を変更しても、最初の顧客情報は保持する必要がある)
        If SCA1.DGV2.Rows.Count > 0 Then
            DGV2cid = SCA1.DGV2.CurrentRow.Cells(0).Value                   ' 選択債務者のCID(DGV2)
        End If
        TB_A1.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
        TB_A2.SelectedIndex = 0
        TB_A3.SelectedIndex = 0
        TB_A8.SelectedIndex = 0
        If xml.GetUserName <> "" Then
            TB_A4.Text = xml.GetUserName        ' 最後に書き込んだユーザー名(PC固有)を表示
            TB_A7.Text = xml.GetUserName2
        Else
            TB_A4.Text = My.Computer.Name
        End If

        ' ダミーの交渉記録追加の場合は、ダミー顧客名のテキストボックスを非表示から表示に
        ' 顧客選択タブも切替不可に
        If CRow.Cells(0).Value = Common.DUMMY_NO Then
            Label6.Visible = True
            TB_DNAME.Visible = True
            '    TabControl1.TabPages.Remove(TabControl1.TabPages(3))        ' 交渉記録印刷の非表示
            '    TabControl1.TabPages.Remove(TabControl1.TabPages(2))        ' 送付物選択の非表示
            '    TabControl1.TabPages.Remove(TabControl1.TabPages(1))        ' 顧客複数選択の非表示
        End If
        L_STSSend.Text = SCA1.DGV9(1, 2).Value & " の顧客情報を印刷します。"

        ' データ読み込み
        If addSW Then       ' 追加ボタン
            L_STS.Text = SCA1.DGV9(1, 2).Value & " の記録を追加中。"
        Else                ' 編集ボタン 
            L_STS.Text = SCA1.DGV9(1, 2).Value & " の記録を編集中。"
            If TabControl1.TabPages.Count = 2 Then TabControl1.TabPages.Remove(TabControl1.TabPages(1))        ' 顧客複数選択の非表示

            ' 登録済みデータ読み込み
            Dim dt As DataTable = SCA1.db.GetSelect(Sqldb.TID.SCD, $"SELECT * From {SCA1.db.GetTable(Sqldb.TID.SCD)} WHERE FKD01 = '{SCA1.DGV2.CurrentRow.Cells(0).Value}'")
            If dt.Rows.Count = 1 Then
                TB_A1.Text = dt.Rows(0).Item(2)             ' 日時
                TB_A2.Text = dt.Rows(0).Item(3)             ' 相手
                TB_A3.Text = dt.Rows(0).Item(4)             ' 手法
                TB_A4.Text = dt.Rows(0).Item(5)             ' 担当者
                TB_A5.Text = dt.Rows(0).Item(6)             ' 内容
                TB_A6.Text = dt.Rows(0).Item(10)            ' 概要
                TB_A7.Text = dt.Rows(0).Item(11)            ' 対応者
                TB_A8.Text = dt.Rows(0).Item(12)            ' 場所
                TB_A9.Text = dt.Rows(0).Item(13)            ' 送付先郵便番号
                TB_A10.Text = dt.Rows(0).Item(14)           ' 送付先住所
                TB_A11.Text = dt.Rows(0).Item(15)           ' 送付先名前
                TB_DNAME.Text = dt.Rows(0).Item(8)          ' 表示名(ダミー顧客)
                DTP_A1.Text = dt.Rows(0).Item(7)            ' 督促日
                CB_A9.Text = dt.Rows(0).Item(16)            ' 発送種別
            End If

            ' 督促状チェックと日付を読み込み
            Dim dday As String = SCA1.DGV2(7, SCA1.DGV2.CurrentRow.Index).Value ' 督促日
            If dday <> "" Then
                CheckBox1.Checked = True
                DTP_A1.Text = dday
            End If
        End If

        ' 総務課用の概要選択肢を設定
        If xml.GetDiv = Common.DIV.GA Then
            TB_A6.Items.Clear()
            Dim gaItems As String() = {"完済", "一部繰上返済", "各種変更手続き", "書類発行依頼", "年末残高証明書", "団信", "団信弁済", "口座変更", "条件変更", "入金関係", "郵便物返戻", "相続", "債務引受", "その他"}
            TB_A6.Items.AddRange(gaItems)
        End If

        TB_A2.Select()
    End Sub
#End Region
    Public Function GetInstance() As SCE_S2
        Return Me
    End Function

#Region "イベント"
    Private Sub CheckBox1_MouseEnter(sender As Object, e As EventArgs) Handles CheckBox1.MouseEnter
        L_STS.Text = "チェックを入れると、督促状を通知した日付が記録されます。"
    End Sub

    Private Sub CheckBox2_MouseEnter(sender As Object, e As EventArgs)
        L_STS.Text = "チェックを入れると、顧客管理簿に紐付けしない交渉記録となります。"
    End Sub

    ' 督促通知チェックボックス変更イベント
    Private Sub CheckBox1_CheckStateChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckStateChanged
        ' チェックボックスONのときだけ日付を有効に
        DTP_A1.Enabled = (DirectCast(sender, CheckBox).CheckState = CheckState.Checked)
    End Sub

#End Region

#Region "ボタン"
    ' 確定ボタン(追加 or 編集)
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim dNowM As String = Now.ToString("yyyyMMddHHmmss_") & Now.Millisecond & "_"  ' ユニーク番号を設定するために現在時刻のミリ秒までの値を取得
        Dim DDay As String = ""                         ' 督促通知日　追加用
        Dim sqlret
        If CheckBox1.Checked Then DDay = DTP_A1.Text    ' 督促通知チェックがONのときだけ督促日を設定

        If addSW Then
            ' 追加ボタン   選択中の債務者全員の記録(FKSCD)作成
            Dim showName1 As String = CRow.Cells(1).Value          ' 債務者氏名(記録一覧表示表)
            Dim showName2 As String = CRow.Cells(2).Value          ' 債務者カナ(記録一覧検索表)
            If TB_DNAME.Text <> "" Then showName1 = TB_DNAME.Text ' もしダミー顧客の名前があればそっちを優先

            ' 複数顧客選択時
            If DGV_S2.Rows.Count > 0 Then
                For n = 0 To DGV_S2.Rows.Count - 1
                    AddDB(dNowM & CRow.Index & "_" & n, DGV_S2(0, n).Value, DDay, DGV_S2(1, n).Value, DGV_S2(2, n).Value)
                Next
            Else
                AddDB(dNowM & CRow.Index, CRow.Cells(0).Value, DDay, showName1, showName2)
            End If
            sqlret = SCA1.db.ExeSQL(Sqldb.TID.SCD)
        Else
            ' 編集ボタン
            Dim SqlCmd As String = "Update FKSCD Set " &
                                          "FKD03 = '" & TB_A1.Text & "', " &             ' 時間
                                          "FKD04 = '" & TB_A2.Text & "', " &             ' 相手
                                          "FKD05 = '" & TB_A3.Text & "', " &             ' 手法（手段）
                                          "FKD06 = '" & TB_A4.Text & "', " &             ' 対応者
                                          "FKD07 = '" & TB_A5.Text & "', " &             ' 備考内容
                                          "FKD08 = '" & DDay & "', " &                   ' 督促状通知日  (督促状設定した時だけ更新)
                                          "FKD11 = '" & TB_A6.Text & "', " &             ' 概要
                                          "FKD12 = '" & TB_A7.Text & "', " &             ' 対応者２
                                          "FKD13 = '" & TB_A8.Text & "', " &             ' 場所
                                          "FKD14 = '" & TB_A9.Text & "', " &             ' 送付先郵便番号
                                          "FKD15 = '" & TB_A10.Text & "', " &            ' 送付先住所
                                          "FKD16 = '" & TB_A11.Text & "', " &            ' 送付先名前
                                          "FKD17 = '" & CB_A9.Text & "' "                ' 発送種別

            If TB_DNAME.Text <> "" Then
                SqlCmd += ",FKD09 = '" & TB_DNAME.Text & "' "                            ' 顧客名(ダミー顧客)
            End If
            SqlCmd += "Where FKD01 = '" & DGV2cid & "'"
            sqlret = SCA1.db.ExeSQL(Sqldb.TID.SCD, SqlCmd)
        End If

        ' ユーザー名をPCに記録
        'xml.GetUserName = TB_A4.Text
        xml.SetUserName2(TB_A7.Text)
        xml.SetXml()
        If sqlret Then  ' SQL結果が正常ならフォーム閉じて正常応答
            'If CheckBox1.Checked Then
            '    Me.DialogResult = DialogResult.Yes      ' 督促状ありならYes返却
            'Else
            '    Me.DialogResult = DialogResult.OK
            'End If
            SCA1.ExUpdateButton2()
            Me.Close()
        End If
    End Sub

    ' キャンセルボタン
    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.Close()
    End Sub

    ' 顧客選択のラジオボタン選択
    Private Sub RadioButton1_CheckedChanged() Handles RadioButton1.CheckedChanged
        TextBox1.Enabled = RadioButton2.Checked
        DGV_S2.Enabled = RadioButton2.Checked
        Dim userName As String = SCA1.DGV9(1, 2).Value
        If userName = "" Then userName = "ダミー"
        If RadioButton1.Checked Then
            L_STS2.Text = userName & " の交渉記録を作成する"
        Else
            L_STS2.Text = "合計 " & DGV_S2.Rows.Count & " 人の顧客の交渉記録を作成する"
        End If
    End Sub

    ' 帳票フォルダを開くボタン
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Process.Start(EXCPATH_FMT)
    End Sub

    ' 帳票再読み込みボタン
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ShowSendFmtList()
    End Sub

    ' 作成済フォルダを開くボタン
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Process.Start(EXCPATH_OUT)
    End Sub

    ' 帳票手動選択ボタン
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Dim fpath As String = cmn.DialogReadFile("", EXCPATH_FMT)
        If fpath <> "" Then
            PrinfFileName = fpath
            Button4.PerformClick()
        End If
    End Sub

#End Region

    Private Sub AddDB(uId As String, cId As String, dDay As String, showName1 As String, showName2 As String)
        Dim SqlCmd As String = "Insert Into FKSCD Values('" &
                                        uId & "','" &                       ' 記録DB ユニーク番号
                                        cId & "','" &                       ' 債務者識別番号
                                        TB_A1.Text & "','" &                ' 記録日時
                                        TB_A2.Text & "','" &                ' 内容
                                        TB_A3.Text & "','" &                ' 相手
                                        TB_A4.Text & "','" &                ' 対応者
                                        TB_A5.Text & "','" &                ' 備考
                                        dDay & "','" &                      ' 督促通知日
                                        showName1 & "','" &                 ' 債務者氏名(記録一覧の表示用)
                                        showName2 & "','" &                 ' 債務者カナ(記録一覧の検索用)
                                        TB_A6.Text & "','" &                ' 概要
                                        TB_A7.Text & "','" &                ' 対応者２
                                        TB_A8.Text & "','" &                ' 場所
                                        TB_A9.Text & "','" &                ' 送付先郵便番号
                                        TB_A10.Text & "','" &               ' 送付先住所
                                        TB_A11.Text & "','" &               ' 送付先名前
                                        CB_A9.Text & "')"                   ' 発送種別
        SCA1.db.AddSQL(Sqldb.TID.SCD, SqlCmd)
    End Sub

    ' 顧客選択

    ' 督促顧客リスト(テキストボックス)のDataTable取得
    ' テキストボックスに入力された顧客番号を一行づつselectして1つのDataTableにマージしていく
    Private Function GetDunCosDataTable() As DataTable
        Dim cmn As New Common

        Dim retDt As DataTable = SCA1.db.OrgDataTablePlusAssist.Clone
        Dim rs As New StringReader(TextBox1.Text)
        Dim tmpDt As DataTable          ' マージに使うテンポラリDataTable
        Dim cNo As String

        ' 顧客番号リストのテキストボックスから、顧客番号のみ取得してリスト
        While rs.Peek() > -1
            Dim f35Dr As DataRow()
            Dim assDr As DataRow()
            cNo = rs.ReadLine()
            If cNo = "" Then Continue While                                                                ' 空白なら顧客番号とみなさずスキップ
            If cmn.RegReplace(cNo, "[0-9]", "").Length > 0 Then Continue While                             ' 数字以外が含まれていたら顧客番号とみなさずスキップ
            f35Dr = SCA1.db.OrgDataTablePlusAssist.Select("[FK02] = '" & cNo & "'")
            If f35Dr.Count = 0 Then
                ' 該当顧客番号がない。　アシストの番号からも検索
                assDr = SCA1.db.OrgDataTable(Sqldb.TID.SCAS).Select("[C12] = '" & cNo & "'")
                If assDr.Count = 0 Then Continue While    ' 該当顧客番号がどちらのDBに存在しないならスキップ

                ' 顧客番号欄に入力された番号が、アシスト番号だった場合は、顧客番号に変換して検索する
                f35Dr = SCA1.db.OrgDataTablePlusAssist.Select("[FK02] = '" & assDr(0).Item(1) & "'")
            End If

            ' 顧客番号から顧客データをDBからSelectで取得
            If f35Dr.Length > 0 Then
                tmpDt = f35Dr.CopyToDataTable             ' 顧客番号に該当するレコード取得してマージ
                retDt.Merge(tmpDt)
            End If
        End While
        Return retDt
    End Function

    ' Bind列の設定
    Private Sub BindDGVList(dgv As DataGridView, ByRef dt As DataTable, bindID As Integer)
        ' DGVにBindするカラムリスト
        Dim BindList(,) As String = {
            {"FK02", "FK10", "FK11", ""}                         ' DGV_S2
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

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim dt As DataTable
        dt = GetDunCosDataTable()
        BindDGVList(DGV_S2, dt, 0)
        DGV_S2.AutoGenerateColumns = False                      ' DataTable設定時に自動で列追加されないようにする
        DGV_S2.DataSource = dt
        DGV_S2.Sort(DGV_S2.Columns(1), ComponentModel.ListSortDirection.Descending)
        RadioButton1_CheckedChanged()
        L_STSSend.Text = "合計" & DGV_S2.Rows.Count & "件の顧客情報を印刷します。"
    End Sub

    ' 送付物のフォーマットリスト表示
    Private Sub ShowSendFmtList()
        If xml.GetDiv = Common.DIV.GA Then
            LB_SendFMTList.Visible = False
            Button9.Visible = False
            ShowSendFmtListGA()
        Else
            TV_ListFMT.Visible = False
            ShowSendFmtListSC()
        End If
    End Sub




    ' 複数選択されたノードを保持するリスト
    Private selectedNodes As New List(Of TreeNode)
    Public selectedNodesPath As New List(Of String)

    ' TreeViewのNodeMouseClickイベントで複数選択を実現
    Private Sub TV_ListFMT_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles TV_ListFMT.NodeMouseClick
        ' Ctrlキーが押されているか確認
        If (Control.ModifierKeys And Keys.Control) = Keys.Control Then
            ' ノードが既に選択されているかどうかをチェック
            If selectedNodes.Contains(e.Node) Then
                ' 選択済みの場合はリストから削除し、色を元に戻す
                selectedNodes.Remove(e.Node)
                selectedNodesPath.Remove(e.Node.FullPath)
                e.Node.BackColor = TV_ListFMT.BackColor
            Else
                ' 選択されていない場合はリストに追加し、色を変更
                selectedNodes.Add(e.Node)
                If IsExcelFile(e.Node.FullPath) Then
                    selectedNodesPath.Add(e.Node.FullPath)
                End If
                e.Node.BackColor = Color.LightBlue
            End If
        Else
            ' Ctrlキーが押されていない場合は、単一選択モードとして動作
            ClearSelection()
            selectedNodes.Add(e.Node)
            If IsExcelFile(e.Node.FullPath) Then
                selectedNodesPath.Add(e.Node.FullPath)
            End If
            e.Node.BackColor = Color.LightBlue
        End If
    End Sub

    ' 選択状態をクリアするメソッド
    Private Sub ClearSelection()
        For Each node As TreeNode In selectedNodes
            node.BackColor = TV_ListFMT.BackColor
        Next
        selectedNodes.Clear()
        selectedNodesPath.Clear()
    End Sub

    Public Function IsExcelFile(fileName As String) As Boolean
        Return fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) OrElse
           fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
    End Function


    ' 送付物のフォーマットリスト表示
    Private Sub ShowSendFmtListSC()
        LB_SendFMTList.Items.Clear()
        ' 対象ファイルを検索する
        Dim fileList As String() = Directory.GetFileSystemEntries(EXCPATH_FMT, "*.xls?")
        ' 抽出したファイル名をリストに設定
        For Each filePath As String In fileList
            If Path.GetFileName(filePath).StartsWith("~") Then Continue For              ' チルダ「~」で始まるファイル(隠しファイル)は非表示にする
            LB_SendFMTList.Items.Add(Path.GetFileName(filePath))
        Next
        If LB_SendFMTList.Items.Count > 0 Then LB_SendFMTList.SelectedIndex = 0
    End Sub


    ' 送付物のフォーマットリストをツリービューに表示
    Private Sub ShowSendFmtListGA()
        TV_ListFMT.Nodes.Clear() ' ツリービューの内容をクリア

        ' サブフォルダを含む全てのフォルダを取得
        Dim directories As String() = Directory.GetDirectories(EXCPATH_FMT)

        ' ルートフォルダに含まれるファイルを表示
        AddFilesToTreeView(EXCPATH_FMT, TV_ListFMT.Nodes)

        ' 各サブフォルダに対して処理を行う
        For Each folderPath As String In directories
            ' フォルダ名をツリービューのノードとして追加
            Dim folderNode As TreeNode = TV_ListFMT.Nodes.Add($"{Path.GetFileName(folderPath)}")

            ' フォルダ内のExcelファイルを取得し、ノードの下に追加
            AddFilesToTreeView(folderPath, folderNode.Nodes)
        Next

        ' 最初のノードを展開
        If TV_ListFMT.Nodes.Count > 0 Then
            TV_ListFMT.Nodes(0).Expand()
        End If
    End Sub

    ' 指定されたフォルダ内のExcelファイルをツリービューに追加するサブプロシージャ
    Private Sub AddFilesToTreeView(folderPath As String, nodes As TreeNodeCollection)
        Dim fileList As String() = Directory.GetFiles(folderPath, "*.xls?")

        For Each filePath As String In fileList
            If Path.GetFileName(filePath).StartsWith("~") Then Continue For ' チルダ「~」で始まるファイル(隠しファイル)は非表示にする
            nodes.Add(Path.GetFileName(filePath))
        Next
    End Sub

    ' 送付物の印刷
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If xml.GetDiv = Common.DIV.SC Then
            If LB_SendFMTList.SelectedItems.Count = 0 Then Exit Sub
        End If
        For Each node As TreeNode In selectedNodes
            Console.WriteLine(node.Text)
        Next
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim f As New VBR_Send
        f.ShowDialog(Me)
        f.Dispose()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim log As New Log
        Dim oPath As String = cmn.DialogSaveFile("交渉記録一覧.xlsx")
        log.TimerST()
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim ExcExp As New ExcelExp
        If RB_T4_A.Checked Then ExcExp.OutRec(oPath, SCA1.DGV1.CurrentRow.Cells(0).Value)
        If RB_T4_B.Checked Then ExcExp.OutRec(oPath)
        log.TimerED("ExcExp")
    End Sub

    ' 帳票フォルダを開くボタン
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Process.Start(cmn.CurrentPath & Common.DIR_EXC & Common.DIR_EXCFMT3)
    End Sub

    Private Sub TB_A3_TextChanged(sender As Object, e As EventArgs) Handles TB_A3.TextChanged
        If TB_A3.Text = "郵便発送" Then
            Label18.Visible = True
            CB_A9.Visible = True
            CB_A9.SelectedIndex = 0
        Else
            Label18.Visible = False
            CB_A9.Visible = False
            CB_A9.Text = ""
        End If
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        TB_A1.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
    End Sub


    ' ----- タブ4 交渉記録印刷
    Private Sub T4Init()
        DTP_T4_A.Text = Today.ToString("yyyy/MM/01")
        DTP_T4_B.Text = Today.ToString("yyyy/MM/dd")
    End Sub

    ' 顧客単体 or 複数 ラジオボタン切り替え
    Private Sub RB_T4_A_CheckedChanged(sender As Object, e As EventArgs) Handles RB_T4_A.CheckedChanged
        DTP_T4_A.Enabled = RB_T4_B.Checked
        DTP_T4_B.Enabled = RB_T4_B.Checked
        DTP_T4_A.Enabled = RB_T4_B.Checked
        DTP_T4_B.Enabled = RB_T4_B.Checked
        If RB_T4_B.Checked Then
            ShowDGV1()
            CB_T4_ASS.Enabled = True
        Else
            DGV1.Rows.Clear()
            CB_T4_ASS.Enabled = False
        End If
    End Sub

    Private Sub ShowDGV1()
        DGV1.Rows.Clear()
        ' 交渉記録の日付が日付形式ではないものをピックアップしてアナウンスする
        SCA1.db.UpdateOrigDT(Sqldb.TID.SCD)
        Dim dt As DataTable = SCA1.db.OrgDataTable(Sqldb.TID.SCD).Copy
        'Dim dt As DataTable = SCA1.db.GetSelect(Sqldb.TID.SCD, $"SELECT * FROM {SCA1.db.GetTable(Sqldb.TID.SCD)}")

        For x = 0 To dt.Rows.Count - 1
            If Not DateTime.TryParse(dt.Rows(x)(2), Nothing) Then
                DGV1.Rows.Add()
                DGV1(0, DGV1.Rows.Count - 1).Value = dt.Rows(x)(0)
                DGV1(1, DGV1.Rows.Count - 1).Value = dt.Rows(x)(1)
                DGV1(2, DGV1.Rows.Count - 1).Value = dt.Rows(x)(8)
                DGV1(3, DGV1.Rows.Count - 1).Value = dt.Rows(x)(2)
                DGV1(4, DGV1.Rows.Count - 1).Value = dt.Rows(x)(5)
            End If
        Next
    End Sub

    Private Sub DGV1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGV1.CellDoubleClick
        If DGV1.Rows.Count = 0 Then Exit Sub
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        SCA1.ShowSelectUser(DGV1.CurrentRow.Cells(1).Value)
        SCA1.ShowSelectRecord(DGV1.CurrentRow.Cells(0).Value)
        Me.Close()
    End Sub

End Class