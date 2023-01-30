Public Class SCE_S1

    Private ReadOnly db As New Sqldb
    Private ReadOnly log As New Log
    Private ReadOnly xml As New XmlMng
    Private ReadOnly cmn As New Common
    Private Const WEEK_CNT = 7              ' 一週間の日数
    Private Const TASK_ROW_CNT = 6          ' 1日のタスク表示件数
    Private Const WEEK_ROW_CNT = 6          ' カレンダーの表示週数
    Private Const ROW_HEIGHT = 15           ' カレンダーの縦幅サイズ

    Private StartDate As Date               ' カレンダーの基準日(日曜日)
    Private TaskCID As String               ' 選択中のタスク識別番号
    Private HolidayList As DataTable        ' 国民の祝日リスト


    Private Sub FLS_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        InitComboBoxTime()
        InitComboBoxTags()
        ReadHolidayDB()
        ReadTags()

        ClickThisMonth()
        'ShowDGV1()
    End Sub

    Private Sub ShowDGV1()
        db.DBFileFDL(Sqldb.TID.CAL)         ' FLS環境でダウンロードができてないので強制ダウンロード

        DGV1.Rows.Clear()
        L_DATE.Text = StartDate
        L_MONTH.Text = StartDate.AddDays(6).ToString("yyyy年 MMM") & "月"

        ' 行の作成
        For weekNum = 0 To WEEK_ROW_CNT - 1
            ' 見出し（日付）のフォーマット設定
            DGV1.Rows.Add()
            DGV1.Rows(DGV1.Rows.Count - 1).Height = ROW_HEIGHT
            DGV1.Rows(DGV1.Rows.Count - 1).DefaultCellStyle.Font = New Font(DGV1.DefaultCellStyle.Font, FontStyle.Bold)
            DGV1.Rows(DGV1.Rows.Count - 1).DefaultCellStyle.ForeColor = Color.DimGray
            DGV1.Rows(DGV1.Rows.Count - 1).DefaultCellStyle.BackColor = Color.LightGray
            DGV1.Rows(DGV1.Rows.Count - 1).Cells(5).Style.ForeColor = Color.SteelBlue               ' 土曜日のカラーリング
            DGV1.Rows(DGV1.Rows.Count - 1).Cells(6).Style.ForeColor = Color.Red                     ' 日曜・祝日のカラーリング
            For day = 0 To WEEK_CNT - 1
                ' 日付設定
                Dim dd As Date = StartDate.AddDays(day + weekNum * WEEK_CNT)
                If dd.ToString("dd") = "01" Then     ' 月の頭だけ○月と表記
                    DGV1.Rows(DGV1.Rows.Count - 1).Cells(day).Value = dd.ToString("[MMM月] ") & dd.ToString("d (ddd)        ")
                Else
                    DGV1.Rows(DGV1.Rows.Count - 1).Cells(day).Value = dd.ToString("d (ddd)")
                End If

                If dd = Today Then
                    DGV1.Rows(DGV1.Rows.Count - 1).Cells(day).Style.BackColor = Color.DeepSkyBlue
                End If
            Next

            ' タスク行のフォーマット設定
            For taskNum = 0 To TASK_ROW_CNT - 1
                DGV1.Rows.Add()
                DGV1.Rows(DGV1.Rows.Count - 1).Height = ROW_HEIGHT
                DGV1.Rows(DGV1.Rows.Count - 1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft     ' 左寄せ
            Next
        Next

        ' 国民の祝日の場合タスクの1番目に祝日名をタスク行先頭に記載
        For n = 0 To (WEEK_CNT * WEEK_ROW_CNT) - 1
            Dim hol As String = ""
            Dim row As Integer
            Dim col As Integer
            CheckHoriday(StartDate.AddDays(n), hol)
            If hol <> "" Then
                GetTaskPoint(StartDate.AddDays(n), row, col)     ' タスク書き込み座標を取得 row, col
                DGV1(col, row + 1).Value = hol
                DGV1(col, row + 1).Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                DGV1(col, row + 1).Style.Font = New Font(DGV1.Font, FontStyle.Bold)
                DGV1(col, row + 1).Style.ForeColor = Color.RoyalBlue
                DGV1(col, row + 1).Style.BackColor = Color.Pink
            End If
        Next

        ' 表示期間のタスクをDBから読み込む
        db.UpdateOrigDT(Sqldb.TID.CAL)
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.CAL,
                                           String.Format("Select * From TBL Where {0} >= '{1}' And {0} <= '{2}' Order By C02, C04 ",
                                                         "C02",
                                                         StartDate.ToShortDateString,
                                                         StartDate.AddDays((WEEK_CNT * WEEK_ROW_CNT) - 1).ToShortDateString
                                                         )
                                           )

        ' タグ用カラー取得
        Dim dtcol As DataTable = db.GetSelect(Sqldb.TID.TAGS, "Select C03 From TBL")

        ' 取得したタスクをカレンダーに設定する
        For taskNum = 0 To dt.Rows.Count - 1
            Dim row As Integer
            Dim col As Integer
            Dim taskDate As String = dt.Rows(taskNum)(1)

            GetTaskPoint(dt.Rows(taskNum)(1), row, col)     ' タスク書き込み座標を取得 row, col

            Dim offset As Integer
            For offset = 0 To TASK_ROW_CNT
                ' 表示領域の行数を超えた場合(35行目以降)はエラーとなるので表示できない
                If row > (TASK_ROW_CNT + 1) * WEEK_ROW_CNT Then
                    log.cLog("### row:" & row & ":" & dt.Rows(taskNum)(1) & ":" & dt.Rows(taskNum)(2))
                    Continue For
                End If
                If DGV1(col, row + offset).Value <> "" Then Continue For    ' 書き込み行に既にタスクがあれば１つ下の行に設定する

                ' ----------------------------------
                ' - タスクをカレンダーに設定

                ' 1. 時間 00:00
                DGV1(col, row + offset).Value = String.Format("{0} ", dt.Rows(taskNum)(3))

                ' 2. タグ 【AM休】
                If dt.Rows(taskNum)(4) <> "" And dt.Rows(taskNum)(4) <> "0" Then
                    If IsNumeric(dt.Rows(taskNum)(4)) Then
                        DGV1(col, row + offset).Value += String.Format("【{0}】", GetTagName(dt.Rows(taskNum)(4)))
                        ' タグがあればタグの色を変更
                        DGV1(col, row + offset).Style.BackColor = Color.FromArgb(dtcol.Rows(dt.Rows(taskNum)(4) - 1)(0))
                    Else
                        DGV1(col, row + offset).Value += String.Format("【{0}】", dt.Rows(taskNum)(4))
                    End If
                End If

                ' 3. タスク名
                DGV1(col, row + offset).Value += String.Format("{0}", dt.Rows(taskNum)(2))

                ' 4. 担当 (山田)
                If dt.Rows(taskNum)(5) <> "" Then
                    DGV1(col, row + offset).Value += String.Format("({0})", dt.Rows(taskNum)(5))
                End If

                Exit For
            Next
            If offset > (TASK_ROW_CNT) Then
                log.cLog("## OFFSET!! ##" & offset)
                Dim taskCount() As DataRow = dt.Select(String.Format("C02 = '{0}'", dt.Rows(taskNum)(1)))
                DGV1(col, row + offset - 1).Value = String.Format("* タスク合計{0}件", taskCount.Length)
            End If
        Next



    End Sub


    ' カレンダークリックイベント
    Private Sub DGV1_CurrentCellChanged(sender As Object, e As EventArgs) Handles DGV1.CurrentCellChanged
        If DGV1.CurrentCell Is Nothing Then Exit Sub
        log.cLog("row: " & DGV1.CurrentCell.RowIndex & ", col: " & DGV1.CurrentCell.ColumnIndex)
        TaskCID = ""
        L_DATE.Text = GetSelectDate()

        ' 何もなかった場合などデフォルト値を予め設定
        TB_A1.Text = ""
        CB_A2.Text = "00:00"
        CB_A3.Text = ""
        TB_A4.Text = xml.xmlData.UserName       ' 担当者
        TB_A6.Text = ""
        BT_A1.Text = "保　存"


        ' 日付部分をクリック
        If DGV1.CurrentCell.RowIndex Mod (TASK_ROW_CNT + 1) = 0 Then Exit Sub

        ' タスク部分をクリック
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.CAL,
                                            String.Format("Select * From TBL Where {0} = '{1}' Order By C04 ",
                                                            "C02",
                                                            GetSelectDate.ToShortDateString)
                                            )

        Dim taskId As Integer = GetSelectTaskID()       ' 選択したタスクが何行目のタスクなのか取得

        ' 選択日付のタスク１行目を取得。祝日名があるか判定
        Dim Task1Name As String = DGV1(DGV1.CurrentCell.ColumnIndex, DGV1.CurrentCell.RowIndex - (DGV1.CurrentCell.RowIndex Mod (TASK_ROW_CNT + 1)) + 1).Value
        If Task1Name Is Nothing Then Exit Sub
        If Not IsNumeric(Task1Name(0)) Then
            taskId -= 1         ' 祝日の1行分ずらす
        End If

        ' 選択箇所にタスクがないので空白表示
        If dt.Rows.Count < taskId + 1 Then Exit Sub

        ' タスクがあるのでそれを表示する
        Dim row As Integer
        Dim col As Integer
        GetTaskPoint(L_DATE.Text, row, col)
        If taskId < 0 Then Exit Sub

        TaskCID = dt.Rows(taskId)(0)
        TB_A1.Text = dt.Rows(taskId)(2)
        CB_A2.Text = dt.Rows(taskId)(3)
        If IsNumeric(dt.Rows(taskId)(4)) Then CB_A3.Text = GetTagName(dt.Rows(taskId)(4))
        TB_A4.Text = dt.Rows(taskId)(5)
        'L_A5.BackColor = dt.Rows(taskId)(6)
        TB_A6.Text = dt.Rows(taskId)(7)
        BT_A1.Text = "変　更"


        ' ********************************************************
        ' ********************************************************

        'log.cLog("カラー:" & [Enum].IsDefined(GetType(KnownColor), dt.Rows(taskId)(6)))
        'If [Enum].IsDefined(GetType(KnownColor), dt.Rows(taskId)(6)) Then
        '    Button2.BackColor = Color.FromName(dt.Rows(taskId)(6))
        'Else
        '    Button2.BackColor = dt.Rows(taskId)(6)
        'End If
    End Sub


    ' いつか使うかも　DGV マウススクロールイベント
    Private Sub Form1_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        If e.Delta > 0 Then
            Console.WriteLine("上方向にスクロールしました。")
            ClickMonthBack()
        Else
            Console.WriteLine("下方向にスクロールしました。")
            ClickMonthFront()
        End If
    End Sub

    ' 保存ボタン
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        Dim saveTaskid As String = TaskCID
        If TaskCID = "" Then
            ' 新規タスク作成の場合はタスク識別子を新規で設定
            saveTaskid = Now.ToString("yyyy/MM/dd-HH:mm-ss-") & Now.Millisecond
        End If
        db.ExeSQLInsUpd(Sqldb.TID.CAL,
                        saveTaskid,
                        L_DATE.Text,
                        TB_A1.Text,
                        CB_A2.Text,
                        CB_A3.SelectedIndex,        ' タグindex
                        TB_A4.Text,
                        "",
                        TB_A6.Text)
        db.ExeSQL(Sqldb.TID.CAL)
        MsgBox("予定を反映しました。")
        ShowDGV1()
    End Sub

    ' 削除ボタン
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' 選択中のタスクIDを取得   日付選択時は何もしない
        If TaskCID = "" Then
            MsgBox("予定を選択してから削除ボタンを押してください。")
            Exit Sub
        End If

        Dim r = MessageBox.Show(TB_A1.Text & vbCrLf & "の予定を削除して良いですか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        db.AddSQL(String.Format("Delete From TBL Where C01 = '{0}'", TaskCID))
        db.ExeSQL(Sqldb.TID.CAL)
        MsgBox("予定を削除しました。")
        ShowDGV1()
    End Sub

    ' ----------------------------------------
    ' ----- 便利系                       -----

    ' 指定日のタスク書き込み座標取得
    Private Sub GetTaskPoint(basicDate As Date, ByRef row As Integer, ByRef col As Integer)
        row = 1
        col = 0
        Dim diffDate As Integer = DateDiff("d", StartDate, basicDate)
        If diffDate < 0 Then Exit Sub
        col = diffDate Mod WEEK_CNT
        row = Math.Floor(diffDate / WEEK_CNT) * (TASK_ROW_CNT + 1)

        log.cLog("GetTaskPoint basicDate: " & basicDate & ", StartDate: " & StartDate & ", diffDate: " & diffDate & ", row: " & row & ", col: " & col)
    End Sub

    ' カレンダーの選択中の日付を取得する
    Private Function GetSelectDate() As Date
        Dim ret As Date
        Dim row As Integer = DGV1.CurrentCell.RowIndex
        Dim col As Integer = DGV1.CurrentCell.ColumnIndex
        Dim addDays As Integer = Math.Floor(row / (TASK_ROW_CNT + 1)) * WEEK_CNT + col    ' 今週頭からの経過日数

        ret = StartDate.AddDays(addDays)
        Return ret
    End Function

    ' カレンダーの選択中の日付番号(タスクID)を取得する
    ' 日付選択時は-1
    Private Function GetSelectTaskID() As Integer
        Dim row As Integer = DGV1.CurrentCell.RowIndex
        Dim taskId As Integer = (row Mod (TASK_ROW_CNT + 1)) - 1
        Return taskId
    End Function

    ' 時間コンボボックスに30分毎の時間を設定
    Private Sub InitComboBoxTime()
        CB_A2.Items.Clear()
        For ctime = 0 To 23
            CB_A2.Items.Add(String.Format("{0}:{1}", ctime.ToString("00"), "00"))
            CB_A2.Items.Add(String.Format("{0}:{1}", ctime.ToString("00"), "30"))
        Next
        CB_A2.SelectedIndex = 0
    End Sub

    ' タグコンボボックスに設定
    Private Sub InitComboBoxTags()
        CB_A3.Items.Clear()
        CB_A3.Items.Add("")
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.TAGS, "Select C02 From TBL")
        For Each d In dt.Rows
            CB_A3.Items.Add(d(0))
        Next
        CB_A3.SelectedIndex = 0
    End Sub

    Private Sub ColorDialogInit()
        'ColorDialogクラスのインスタンスを作成
        Dim cd As New ColorDialog()

        'はじめに選択されている色を設定
        cd.Color = DGV2.CurrentRow.Cells(0).Style.BackColor
        '色の作成部分を表示可能にする
        'デフォルトがTrueのため必要はない
        cd.AllowFullOpen = True
        '純色だけに制限しない
        'デフォルトがFalseのため必要はない
        cd.SolidColorOnly = True
        '[作成した色]に指定した色（RGB値）を表示する
        cd.CustomColors = New Integer() {}

        'ダイアログを表示する
        If cd.ShowDialog() = DialogResult.OK Then
            '選択された色の取得
            DGV2.CurrentRow.Cells(0).Style.BackColor = cd.Color
            'log.cLog(cd.Color.ToArgb)
            'log.cLog(Color.FromArgb(cd.Color.ToArgb).ToString)
        End If
    End Sub

    ' 国民の祝日のDB読み込み
    Private Sub ReadHolidayDB()
        HolidayList = db.GetSelect(Sqldb.TID.HOLI, "Select * From TBL")
    End Sub

    ' タグ一覧のDB読み込み
    Private Sub ReadTags()
        DGV2.Rows.Clear()
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.TAGS, "Select * From TBL")
        For Each d In dt.Rows
            DGV2.Rows.Add()
            DGV2.Rows(DGV2.Rows.Count - 1).Cells(0).Value = d(1)
            DGV2.Rows(DGV2.Rows.Count - 1).Cells(0).Style.BackColor = Color.FromArgb(d(2))
        Next
        DGV2.Rows.Add()
    End Sub

    ' 国民の祝日判定       holidayNameに、”祝日名”が返却されれば祝日　ブランクなら平日
    Private Sub CheckHoriday(day As Date, ByRef holidayName As String)
        holidayName = ""
        Dim idx As Integer = Array.IndexOf(HolidayList.AsEnumerable().Select(Function(row) row(0)).ToArray(), day.ToString("yyyy/M/d"))
        If idx >= 0 Then
            holidayName = HolidayList.Rows(idx)(1)
        End If
    End Sub

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles DGV1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
            Case Keys.F2
            Case Keys.F3
            Case Keys.F5
                ShowDGV1()
        End Select
    End Sub






    ' 複製ボタン
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim r = MessageBox.Show("予定を複製して良いですか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        db.ExeSQLInsUpd(Sqldb.TID.CAL,
                        Now.ToString("yyyy/MM/dd-HH:mm-ss-") & Now.Millisecond,
                        DTP_A7.Value.ToString("yyyy/MM/dd"),
                        TB_A1.Text,
                        CB_A2.Text,
                        CB_A3.SelectedIndex,        ' タグindex
                        TB_A4.Text,
                        "",
                        TB_A6.Text)
        db.ExeSQL(Sqldb.TID.CAL)
        MsgBox("予定を複製しました。")
        ShowDGV1()

    End Sub

    Private Sub ClickMonthBack() Handles Button3.Click
        StartDate = StartDate.AddDays(6).AddMonths(-1).ToString("yyyy/MM/01")
        Dim dayDiff As Integer = StartDate.DayOfWeek - DayOfWeek.Monday
        StartDate = StartDate.AddDays(-dayDiff)
        ShowDGV1()
    End Sub

    Private Sub ClickMonthFront() Handles Button4.Click
        StartDate = StartDate.AddDays(6).AddMonths(1).ToString("yyyy/MM/01")
        Dim dayDiff As Integer = StartDate.DayOfWeek - DayOfWeek.Monday
        StartDate = StartDate.AddDays(-dayDiff)
        ShowDGV1()
    End Sub

    Private Sub ClickThisMonth() Handles Button5.Click
        StartDate = Today.ToString("yyyy/MM/01")
        Dim dayDiff As Integer = StartDate.DayOfWeek - DayOfWeek.Monday
        StartDate = StartDate.AddDays(-dayDiff)
        ShowDGV1()

    End Sub

    Private Sub DGV2_DoubleClick(sender As Object, e As EventArgs) Handles DGV2.DoubleClick
        ColorDialogInit()
        TagColorSave()
    End Sub

    ' タグ名からタグ番号を取得　0返却は未発見
    Private Function GetTagsNum(TagName As String) As Integer
        Dim cmd As String = String.Format("Select C01 From TBL Where C02 = '{0}'", TagName)
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.TAGS, cmd)
        If dt.Rows.Count = 1 Then
            ' タグ名が一致した 番号を返却 1～
            Return dt.Rows(0)(0)
        End If
        ' タグ名が一致しなかった。0を返却
        Return 0
    End Function

    Private Function GetTagName(id As Integer) As String
        Dim cmd As String = String.Format("Select C02 From TBL Where C01 = '{0}'", id)
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.TAGS, cmd)
        If dt.Rows.Count = 1 Then
            ' タグ番号が一致したタグ名を返却 
            Return dt.Rows(0)(0)
        End If
        ' タグ番号が一致しなかった
        Return ""
    End Function

    ' タグ色保存
    Private Sub TagColorSave()
        For n = 0 To DGV2.Rows.Count - 1
            ' デフォルト値0だと色が青くなるので白(-1)に設定
            If DGV2(0, n).Style.BackColor.ToArgb = 0 Then DGV2(0, n).Style.BackColor = Color.FromArgb(-1)
            ' 編集されたセルの値と、色をDBに設定
            db.ExeSQLInsUpd(Sqldb.TID.TAGS, n + 1, DGV2(0, n).Value, DGV2(0, n).Style.BackColor.ToArgb.ToString)
            ' 空白にされているセルがあればDBから削除
            If DGV2(0, n).Value = "" Then
                db.AddSQL(String.Format("Delete From TBL Where C01 = '{0}'", n + 1))
            End If
        Next
        db.ExeSQL(Sqldb.TID.TAGS)
    End Sub

    Private Sub DGV2_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DGV2.CellValueChanged
        TagColorSave()
    End Sub

End Class