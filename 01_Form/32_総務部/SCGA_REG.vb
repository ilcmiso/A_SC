Public Class SCGA_REG

    Private ReadOnly cmn As New Common
    Private ReadOnly sccmn As New SCcommon
    Private ReadOnly xml As New XmlMng
    Private ReadOnly log As New Log
    Private ownForm As SCA1
    Private MRType As Integer
    Private Const BTADD As String = "BT_MRAdd"
    Private Const BTEDIT As String = "BT_MREdit"

    Private Const MR_CALENDAR As String = "Cal"
    Private Const MR_NUMERIC As String = "NUM"
    Private Const MR_BLANK As String = "Blank"
    Private Const MR_FORMAT As String = "Format"
    Private Const MR_READONLY As String = "ReadOnly"
    Private Const NUMBER_LENGTH As Integer = 3

    Private AddRepayFACount As Integer         ' 団信弁済のフラットorアシストを追加しますか？のメッセージボックスを1度しか表示しないためのカウント

#Region " OPEN CLOSE "
    Private Sub FLS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ownForm = DirectCast(Me.Owner, SCA1)
        MRType = ownForm.CB_MRLIST.SelectedIndex
        ShowDGV()
        AddRepayFACount = 0
    End Sub

    Private Sub ShowDGV()
        cmn.SetDoubleBufferDGV(DGV_REG1)
        L_REGTITLE.Text = $"{ownForm.CB_MRLIST.SelectedItem} の 登録"
        ' UserListのユーザー名を配列に設定
        Dim dataTable As DataTable = ownForm.db.OrgDataTable(Sqldb.TID.USER)
        Dim userList As String() = dataTable.AsEnumerable().Where(Function(row) row.Field(Of String)("C04") = "1") _
                                                           .Select(Function(row) row.Field(Of String)("C03")) _
                                                           .ToArray()

        FillDataGridView()
        cmn.SetCellFontDGV(DGV_REG1, "項目", "実施年月", isBold:=True)
        cmn.SetCellFontDGV(DGV_REG1, "項目", "アシスト同時完済", isBold:=True)
        cmn.SetCellFontDGV(DGV_REG1, "項目", "完済日", isBold:=True)
        cmn.SetCellFontDGV(DGV_REG1, "項目", "登録変更予定月", isBold:=True)

        Select Case ownForm.ActiveControl.Name
            Case BTADD  ' 追加ボタン契機

                ' 各項目に初期入力値を設定
                SetValueDGV("番号", GetNextMaxValue())
                SetComboBoxItemsDGV("担当者", userList)
                SetValueDGV("担当者", xml.GetUserName)
                If ownForm.DGV1.CurrentRow IsNot Nothing Then
                    If sccmn.IsDGVCurrentValid(ownForm.DGV1) Then
                        SetValueDGV("顧客番号", ownForm.DGV1.CurrentRow.Cells(0).Value)
                        SetValueDGV("主債務者名", ownForm.DGV1.CurrentRow.Cells(1).Value)
                        SetValueDGV("債務者", ownForm.DGV1.CurrentRow.Cells(1).Value)
                        SetValueDGV("実行日", ownForm.DGV9(5, 1).Value)                     ' 金消日を設定
                        SetValueDGV("証券番号", ownForm.DGV9(3, 0).Value)
                    End If
                End If

                Select Case MRType
                    Case SCcommon.MRITEMID.ACCOUNT_CHANGE  ' 口座変更
                        SetValueDGV("新口座開始月", "")
                    Case SCcommon.MRITEMID.MAIL_SEND       ' 郵便発送
                        If sccmn.IsDGVCurrentValid(ownForm.DGV1) Then
                            SetValueDGV("発送先", ownForm.DGV1.CurrentRow.Cells(1).Value)
                        End If
                        ' UserListにあるユーザー名をコンボボックスのItemsに設定
                        SetComboBoxItemsDGV("再鑑者", userList)
                    Case SCcommon.MRITEMID.MAIL_RECV       ' 郵便受領
                        ' UserListにあるユーザー名をコンボボックスのItemsに設定
                        SetComboBoxItemsDGV("受領者", userList)
                        SetValueDGV("受領者", xml.GetUserName)
                End Select

            Case BTEDIT ' 編集ボタン契機
                ' DGVデータ読み込み
                SetComboBoxItemsDGV("担当者", userList)
                SetComboBoxItemsDGV("再鑑者", userList)
                SetValueDGV("担当者", xml.GetUserName)
                LoadDataGridView()

                If DGV_REG1.Rows(5).Cells(0).Value = "顧客番号" And DGV_REG1.Rows(5).Cells(1).ReadOnly = True Then
                    L_EDITCOS.Visible = True
                End If

        End Select
    End Sub

    Private Sub LoadDataGridView()
        For n = 0 To ownForm.DGV_MR1.CurrentRow.Cells.Count - 1
            DGV_REG1(1, n).Value = ownForm.DGV_MR1.CurrentRow.Cells(n).Value

            ' DGV_REG1の対応するセルに配置されたNumericUpDownを探す
            Dim rect As Rectangle = DGV_REG1.GetCellDisplayRectangle(1, n, False)
            For Each control In DGV_REG1.Controls
                If TypeOf control Is NumericUpDown AndAlso
               control.Bounds.IntersectsWith(rect) Then

                    Dim numericUpDown As NumericUpDown = CType(control, NumericUpDown)
                    ' セルの値をDecimalに変換し、NumericUpDownのValueに設定
                    Dim cellValue As Decimal
                    If Decimal.TryParse(DGV_REG1(1, n).Value.ToString(), cellValue) Then
                        numericUpDown.Value = cellValue
                    End If

                ElseIf TypeOf control Is ComboBox AndAlso
                   control.Bounds.IntersectsWith(rect) Then

                    Dim comboBox As ComboBox = CType(control, ComboBox)
                    ' セルの値をStringに変換し、ComboBoxのSelectedItemに設定
                    Dim cellValue As String = DGV_REG1(1, n).Value.ToString()
                    If comboBox.Items.Contains(cellValue) Then
                        comboBox.SelectedItem = cellValue
                    End If

                End If
            Next
        Next

        ' 備考の内容を備考テキストボックスに設定
        Dim idx As Integer = cmn.FindColumnIndex(ownForm.DGV_MR1, "備考")
        If idx >= 0 Then
            TB_Remarks.Text = ownForm.DGV_MR1.Rows(ownForm.DGV_MR1.CurrentRow.Index).Cells(idx).Value
        End If
    End Sub


    ' DB(MRItem)を元にDGVを作成
    Private Sub FillDataGridView()
        ' DGVの既存の行をクリア
        DGV_REG1.Rows.Clear()

        ' 定義データを取得
        Dim dt As DataTable = ownForm.db.GetSelect(Sqldb.TID.MRM, $"SELECT * FROM {ownForm.db.GetTable(Sqldb.TID.MRM)} WHERE C01 = '{MRType}'")

        ' DGVに新しい行を追加
        For Each row As DataRow In dt.Rows
            Dim order As String = row("C02").ToString()  ' 表示順
            Dim itemName As String = row("C03").ToString()  ' 項目名
            Dim width As String = row("C04").ToString()  ' 横幅
            Dim format As String = row("C05").ToString() ' 表示形式

            DGV_REG1.Rows.Add(row(2))
            If width = 0 Then
                DGV_REG1.Rows(DGV_REG1.Rows.Count - 1).Visible = False
            End If

            Dim formatParts As String() = format.Split(":"c)

            Select Case formatParts(0)
                Case MR_CALENDAR
                    Select Case formatParts(1)
                        Case "", MR_BLANK
                            ReplaceCell2DTP(DGV_REG1, order, 1, formatParts(1))
                        Case MR_FORMAT
                            ReplaceCell2DTP(DGV_REG1, order, 1, formatParts(2))
                    End Select

                Case MR_NUMERIC
                    ReplaceCell2NumericUpDown(DGV_REG1, order, 1)

                Case MR_READONLY
                    ' データ追加時、ダミーを選択している場合は、債務者と顧客番号を手入力可能にする。
                    ' データ変更時でも、債務者と顧客番号が空欄であれば、手入力可能とする。
                    ' 一度登録すると変更ができないが、入力誤りや誤ったデータ削除が心配とのことなので仕様。
                    ' 202410 三浦様ご要望
                    If (ownForm.ActiveControl.Name = BTADD) AndAlso
                       (Not sccmn.IsDGVCurrentValid(ownForm.DGV1)) AndAlso
                       (itemName = "債務者" Or itemName = "顧客番号") Then
                        DGV_REG1.Rows(order).Cells(1).Style.BackColor = Color.FromArgb(255, 255, 192)
                    Else
                        DGV_REG1.Rows(order).Cells(1).ReadOnly = True
                    End If


                Case Else
                    If format.Contains(",") Then
                        Dim items As String() = format.Split(",")
                        ReplaceCell2ComboBox(DGV_REG1, order, 1, items)
                    End If
            End Select
            ' 備考は別途テキストボックスとして表示するため、行は非表示にする
            If row(2) = "備考" Then
                L_Remarks.Visible = True
                TB_Remarks.Visible = True
                DGV_REG1.Rows(order).Visible = False
            End If
        Next
        ' DGV_REG1.Columns(1).DefaultCellStyle.WrapMode = DataGridViewTriState.True

    End Sub
#End Region

#Region "ボタン"
    ' 登録ボタン
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        ' SQLコマンド生成
        Dim commandText As New List(Of String)
        For n = 0 To DGV_REG1.Rows.Count - 1
            If DGV_REG1(0, n).Value = "備考" Then
                commandText.Add(TB_Remarks.Text)
            Else
                commandText.Add(DGV_REG1(1, n).Value)
            End If
        Next

        If ownForm.ActiveControl.Name = BTADD Then
            commandText(0) = cmn.GetCurrentDateTime()               ' 登録日時
        End If
        commandText(1) = MRType            ' カテゴリ

        ownForm.db.ExeSQLInsUpd(Sqldb.TID.MR, commandText)
        ownForm.db.ExeSQL(Sqldb.TID.MR)

        ' 追加後、他の追加登録が必要か確認
        If ownForm.ActiveControl.Name = BTADD AndAlso MRType < SCcommon.MRITEMID.MAIL_SEND Then
            Dim r As Integer
            ' 団信弁済を追加するとき、対となる項目(フラットとアシスト)が必要か確認するダイアログ表示
            If AddRepayFACount = 0 AndAlso (MRType = SCcommon.MRITEMID.REPAY_A OrElse MRType = SCcommon.MRITEMID.REPAY_F) Then
                Dim msg As String = If((MRType = SCcommon.MRITEMID.REPAY_F), "アシスト", "フラット")
                r = MessageBox.Show($"登録しました。{vbCrLf}併せて「団信弁済({msg})」を追加しますか？",
                                    "ご確認ください",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question)
                If r = vbYes Then
                    AddRepayFACount = 1
                    AddRepayFA()
                    Exit Sub
                End If
            End If

            ' 発送届けが基本セットになるため、発送届けが必要か確認するダイアログ表示
            r = MessageBox.Show($"登録しました。{vbCrLf}併せて「郵便発送簿」を追加しますか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
            If r = vbYes Then
                ' 追加の郵便発送簿を作成
                AddPostSend()
                Exit Sub
            End If
        Else
            MsgBox("登録しました。")
        End If
        Me.Close()
    End Sub

    ' キャンセルボタン
    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.Close()
    End Sub

    ' 債務者編集ボタン
    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles L_EDITCOS.DoubleClick
        Dim r As DialogResult = MessageBox.Show($"債務者情報を編集しますか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub
        DGV_REG1.Rows(5).Cells(1).ReadOnly = False
        DGV_REG1.Rows(6).Cells(1).ReadOnly = False
        DGV_REG1.Rows(5).Cells(1).Style.BackColor = Color.FromArgb(255, 255, 192)
        DGV_REG1.Rows(6).Cells(1).Style.BackColor = Color.FromArgb(255, 255, 192)
    End Sub

#End Region

    ' 番号の最大値+1取得
    Private Function GetNextMaxValue() As String
        Return GetNextMaxValue(Today.ToString("yyMM"))
    End Function
    Private Function GetNextMaxValue(sDate_yymm As String) As String
        Dim val As String = $"{sDate_yymm}{MRType}{1.ToString("D" & NUMBER_LENGTH)}"
        Dim filter As String = ""
        Dim dt As DataTable
        ' 口座変更(4)の場合のみ、5000番からのインクリメントを大嶋様が希望
        If MRType = SCcommon.MRITEMID.ACCOUNT_CHANGE Then
            val = 5000
        Else
            filter = sDate_yymm
        End If

        ' 年月が一致する番号を取得。新規番号であればそのまま001として返却
        dt = ownForm.db.GetSelect(Sqldb.TID.MR, $"SELECT * FROM {ownForm.db.GetTable(Sqldb.TID.MR)} WHERE C02 = '{MRType}' AND C03 LIKE '{filter}%'")
        If dt.Rows.Count = 0 Then Return val

        ' 取得したリストの最大値を取得
        Dim maxVal As Integer? = dt.AsEnumerable().
                             Select(Function(row)
                                        Dim value As Integer
                                        If Integer.TryParse(row.Field(Of String)("C03"), value) Then
                                            Return value
                                        Else
                                            Return 0
                                        End If
                                    End Function).
                             DefaultIfEmpty(0).
                             Max()

        ' 最大値に1を加える
        Dim nextMaxVal As Integer = maxVal.GetValueOrDefault() + 1
        If nextMaxVal.ToString.Length > NUMBER_LENGTH Then
            val = nextMaxVal
        Else
            val = $"{sDate_yymm}{MRType}{nextMaxVal.ToString("D" & NUMBER_LENGTH)}"
        End If
        log.cLog($"IN:{sDate_yymm} maxval:{maxVal} OUT:{val}")
        Return val
    End Function

    ' 指定文字のセルに値を設定
    Sub SetValueDGV(searchWord As String, valueToSet As String)
        Dim dgv = DGV_REG1

        For Each row As DataGridViewRow In dgv.Rows
            If row.Cells(0).Value.ToString() = searchWord Then
                ' DGVセルに値を設定
                row.Cells(1).Value = valueToSet

                ' セルの上にComboBoxがある場合、ComboBoxにも値を設定しなければDGV上に値が見えないため設定する
                For Each control As Control In dgv.Controls
                    If TypeOf control Is ComboBox Then
                        Dim comboBox As ComboBox = DirectCast(control, ComboBox)
                        ' ComboBoxの位置をチェック
                        Dim cellRect As Rectangle = dgv.GetCellDisplayRectangle(1, row.Index, False)
                        If comboBox.Bounds.IntersectsWith(cellRect) Then
                            ' 適切なComboBoxに値を設定
                            If comboBox.Items.Contains(valueToSet) Then
                                comboBox.SelectedItem = valueToSet
                            Else
                                comboBox.Text = valueToSet
                            End If
                            Exit For
                        End If
                    End If
                Next
                Exit For ' 最初に見つかった一致行でループを終了
            End If
        Next
    End Sub

    ' ComboBoxのアイテムリストを設定
    Sub SetComboBoxItemsDGV(searchWord As String, items As String())
        Dim dgv = DGV_REG1

        For Each row As DataGridViewRow In dgv.Rows
            If row.Cells(0).Value.ToString() = searchWord Then
                ' セルの上にあるComboBoxを探す
                For Each control As Control In dgv.Controls
                    If TypeOf control Is ComboBox Then
                        Dim comboBox As ComboBox = DirectCast(control, ComboBox)
                        ' ComboBoxの位置をチェック
                        Dim cellRect As Rectangle = dgv.GetCellDisplayRectangle(1, row.Index, False)
                        If comboBox.Bounds.IntersectsWith(cellRect) Then
                            ' ComboBoxのアイテムリストを設定
                            comboBox.Items.Clear()
                            comboBox.Items.AddRange(items)

                            ' 必要に応じて、最初のアイテムを選択
                            If comboBox.Items.Count > 0 Then
                                comboBox.SelectedIndex = 0
                            End If
                            Exit For
                        End If
                    End If
                Next
                Exit For ' 最初に見つかった一致行でループを終了
            End If
        Next
    End Sub

    Private Sub ReplaceCell2DTP(dgv As DataGridView, rowIndex As Integer, colIndex As Integer, type As String)
        Dim rect As Rectangle = dgv.GetCellDisplayRectangle(colIndex, rowIndex, False)
        Dim dtPicker As New DateTimePicker()
        Dim formatType As String = "yyyy/MM/dd"

        dtPicker.Location = New Point(rect.Location.X + 132, rect.Location.Y) ' X座標を50ピクセル右にずらす
        dtPicker.Size = New Size(18, rect.Height) ' 幅を18ピクセルに設定

        ' 初期値を設定
        Dim initialDate As DateTime = DateTime.Now

        ' typeにFormatが設定されていればそのFormatを反映する
        If type <> MR_BLANK And type <> "" Then formatType = type

        dgv(colIndex, rowIndex).Value = initialDate.ToString(formatType)
        dgv(colIndex, rowIndex).Tag = dtPicker  ' TagプロパティにDateTimePickerを設定

        If type = MR_BLANK Then dgv(colIndex, rowIndex).Value = ""

        ' 値変更時にDataGridViewのセルに反映
        AddHandler dtPicker.CloseUp, Sub(sender, e)
                                         dgv.EndEdit() ' 追加: これで現在の編集が終了します。
                                         dgv.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                         dgv(colIndex, rowIndex).Value = dtPicker.Value.ToString(formatType)
                                         ' 現在のセルの1つ左のセルを選択
                                         Dim previousCell As Integer = Math.Max(0, colIndex - 1)
                                         dgv.CurrentCell = dgv.Rows(rowIndex).Cells(previousCell)

                                         ' 選択状態を元のセルに戻す
                                         dgv.CurrentCell = dgv.Rows(rowIndex).Cells(colIndex)
                                         ' 一部繰越用の実施年月を設定したとき、実施年月を基準に番号を設定する
                                         If dgv.Rows(rowIndex).Cells(colIndex - 1).Value = "実施年月" And ownForm.ActiveControl.Name = BTADD Then
                                             SetValueDGV("番号", GetNextMaxValue(dtPicker.Value.ToString("yyMM")))
                                         End If
                                         If dgv.Rows(rowIndex).Cells(colIndex - 1).Value = "完済日" And ownForm.ActiveControl.Name = BTADD Then
                                             SetValueDGV("番号", GetNextMaxValue(dtPicker.Value.ToString("yyMM")))
                                         End If

                                     End Sub
        AddHandler dtPicker.Leave, Sub(sender, e)
                                       DGV_REG1.Focus()
                                   End Sub

        ' DataGridViewに追加
        dgv.Controls.Add(dtPicker)
    End Sub

    Private Sub ReplaceCell2ComboBox(dgv As DataGridView, rowIndex As Integer, colIndex As Integer, items As String())
        Dim rect As Rectangle = dgv.GetCellDisplayRectangle(colIndex, rowIndex, False)
        Dim comboBox As New ComboBox()

        ' コンボボックスの位置とサイズを設定
        comboBox.Location = rect.Location
        comboBox.Size = rect.Size
        ' items(C05)に "*" があればDropDown(任意入力可)にする
        If items.Any(Function(w) w = "*") Then
            items = items.Where(Function(w) w <> "*").ToArray   ' "*" を選択肢から削除
            comboBox.DropDownStyle = ComboBoxStyle.DropDown
        Else
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList
        End If

        ' コンボボックスにアイテムを追加
        comboBox.Items.AddRange(items)

        If comboBox.Items.Count > 0 Then
            comboBox.SelectedIndex = 0
            dgv(colIndex, rowIndex).Value = comboBox.SelectedItem
        End If

        ' 値変更時にDataGridViewのセルに反映
        AddHandler comboBox.SelectedIndexChanged, Sub(sender, e)
                                                      dgv(colIndex, rowIndex).Value = comboBox.SelectedItem
                                                  End Sub
        ' テキストが変更されたときもDGVのセルに反映
        AddHandler comboBox.TextChanged, Sub(sender, e)
                                             dgv(colIndex, rowIndex).Value = comboBox.Text
                                         End Sub
        ' DataGridViewに追加
        dgv.Controls.Add(comboBox)
    End Sub

    Private Sub ReplaceCell2NumericUpDown(dgv As DataGridView, rowIndex As Integer, colIndex As Integer)
        ' DataGridViewのセルの位置とサイズを取得
        Dim rect As Rectangle = dgv.GetCellDisplayRectangle(colIndex, rowIndex, False)

        ' NumericUpDownコントロールを生成
        Dim numericUpDown As New NumericUpDown()
        numericUpDown.Size = New Size(rect.Width, rect.Height)
        numericUpDown.Location = New Point(rect.X, rect.Y)
        numericUpDown.TextAlign = LeftRightAlignment.Right
        numericUpDown.UpDownAlign = LeftRightAlignment.Left
        numericUpDown.Font = New Font(DGV_REG1.Font.FontFamily, 10, FontStyle.Regular)
        numericUpDown.Minimum = 0
        numericUpDown.Maximum = 100000000

        ' 値変更時にDataGridViewのセルに反映
        AddHandler numericUpDown.ValueChanged, Sub(sender, e)
                                                   dgv(colIndex, rowIndex).Value = numericUpDown.Value
                                               End Sub

        ' NumericUpDownをDataGridViewに追加
        dgv.Controls.Add(numericUpDown)
        ' NumericUpDownをDataGridViewのTagに格納する（あるいは他の方法で保持）
        dgv.Tag = numericUpDown
    End Sub

    Private Sub DGV_REG1_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles DGV_REG1.CellEnter
        For Each control As Control In DGV_REG1.Controls
            If TypeOf control Is ComboBox OrElse TypeOf control Is DateTimePicker OrElse TypeOf control Is NumericUpDown Then
                Dim cellRect As Rectangle = DGV_REG1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, False)
                If control.Bounds.IntersectsWith(cellRect) Then
                    ' コントロールにフォーカスを移動
                    control.Focus()
                    Exit Sub
                End If
            End If
        Next
    End Sub

    ' DGV上のオブジェクトを削除
    Private Sub RemoveControlsFromDGV(dgv As DataGridView)
        ' コントロールリストを作成して削除対象を一時的に保持します。
        ' 直接削除するとコレクションを変更しながらイテレートすることになり、エラーの原因になるためです。
        Dim controlsToRemove As New List(Of Control)()

        ' DataGridView内のすべてのコントロールを検査
        For Each ctrl As Control In dgv.Controls
            ' コントロールがDateTimePicker、ComboBox、またはNumericUpDownの場合、リストに追加
            If TypeOf ctrl Is DateTimePicker OrElse TypeOf ctrl Is ComboBox OrElse TypeOf ctrl Is NumericUpDown Then
                controlsToRemove.Add(ctrl)
            End If
        Next

        ' 削除対象のコントロールをDataGridViewから削除
        For Each ctrlToRemove In controlsToRemove
            dgv.Controls.Remove(ctrlToRemove)
            ctrlToRemove.Dispose() ' コントロールのリソースを解放
        Next
    End Sub

    ' 追加の郵便発送簿実行
    Private Sub AddPostSend()
        Dim beforeType As Integer = MRType
        MRType = SCcommon.MRITEMID.MAIL_SEND
        ownForm.CB_MRLIST.SelectedIndex = MRType

        ' 「内容」の入力文字列を取得
        Dim dr As DataRow() = ownForm.db.OrgDataTable(Sqldb.TID.MRM).Select($"C01 = '{MRType}' And C03 = '内容'")
        Dim contentWords As String() = dr(0)(4).ToString.Split(","c)

        ' 郵便発送簿の入力内容へ流用するため、現在の登録画面の内容を保持しておく
        Dim flatType As String = "フラット"
        For Each row As DataGridViewRow In DGV_REG1.Rows
            If row.Cells(0).Value IsNot Nothing AndAlso row.Cells(0).Value.ToString() = "ローン種類" Then
                flatType = row.Cells(1).Value
            End If
        Next

        ' 追加画面を再構成するためにオブジェクト削除して表示
        RemoveControlsFromDGV(DGV_REG1)
        ShowDGV()

        ' 内容
        Select Case beforeType
            Case SCcommon.MRITEMID.REPAY, SCcommon.MRITEMID.REPAY_F, SCcommon.MRITEMID.REPAY_A
                ' 団信弁済
                DGV_REG1(1, 6).Value = contentWords(9)
                SetValueDGV("内容", contentWords(9))
            Case SCcommon.MRITEMID.PARTIAL_REPAY_F, SCcommon.MRITEMID.PARTIAL_REPAY_A
                ' 一部繰り上げ返済
                DGV_REG1(1, 6).Value = contentWords(1)
                SetValueDGV("内容", contentWords(1))
            Case SCcommon.MRITEMID.FULL_REPAY
                ' 完済管理
                DGV_REG1(1, 6).Value = contentWords(0)
                SetValueDGV("内容", contentWords(0))
            Case SCcommon.MRITEMID.CONTACT_CHANGE
                ' 契約条件変更
                DGV_REG1(1, 6).Value = contentWords(10)
                SetValueDGV("内容", contentWords(10))
            Case SCcommon.MRITEMID.ACCOUNT_CHANGE
                ' 口座変更
                DGV_REG1(1, 6).Value = contentWords(5)
                SetValueDGV("内容", contentWords(5))
        End Select

        'ローン種類
        DGV_REG1(1, 7).Value = flatType
        SetValueDGV("ローン種類", flatType)
    End Sub

    ' 追加の団信弁済(ForA)
    Private Sub AddRepayFA()
        If MRType = SCcommon.MRITEMID.REPAY_A Then
            MRType = SCcommon.MRITEMID.REPAY_F
        Else
            MRType = SCcommon.MRITEMID.REPAY_A
        End If
        ownForm.CB_MRLIST.SelectedIndex = MRType

        ' 追加画面を再構成するためにオブジェクト削除して表示
        RemoveControlsFromDGV(DGV_REG1)
        ShowDGV()
    End Sub

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles DGV_REG1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
            Case Keys.F2
            Case Keys.F3
        End Select
    End Sub

End Class