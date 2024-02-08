Public Class SCGA_REG

    Private ReadOnly cmn As New Common
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
    Private Const NUMBER_LENGTH As Integer = 5

#Region " OPEN CLOSE "
    Private Sub FLS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ownForm = DirectCast(Me.Owner, SCA1)
        MRType = ownForm.CB_MRLIST.SelectedIndex
        cmn.SetDoubleBufferDGV(DGV_REG1)
        L_REGTITLE.Text = $"{ownForm.CB_MRLIST.SelectedItem} の 登録"
        ' UserListのユーザー名を配列に設定
        Dim dataTable As DataTable = ownForm.db.OrgDataTable(Sqldb.TID.USER)
        Dim userList As String() = dataTable.AsEnumerable().Select(Function(row) row.Field(Of String)("C03")).ToArray()

        FillDataGridView()

        Select Case ownForm.ActiveControl.Name
            Case BTADD  ' 追加ボタン契機

                ' 各項目に初期入力値を設定
                SetValueDGV("番号", GetNextMaxValue(MRType))
                SetComboBoxItemsDGV("担当者", userList)
                SetValueDGV("担当者", xml.GetUserName)
                If ownForm.DGV1.CurrentRow IsNot Nothing Then
                    If ownForm.DGV1.CurrentRow.Cells(0).Value <> Common.DUMMY_NO Then
                        SetValueDGV("顧客番号", ownForm.DGV1.CurrentRow.Cells(0).Value)
                        SetValueDGV("主債務者名", ownForm.DGV1.CurrentRow.Cells(1).Value)
                    End If
                End If

                Select Case MRType
                    Case 0
                        SetValueDGV("相続人代表者", ownForm.DGV9(1, 7).Value)
                        If ownForm.DGV9(1, 7).Value <> "" Then
                            SetValueDGV("続柄", "配偶者")
                        End If
                    Case 4
                        If ownForm.DGV1.CurrentRow.Cells(0).Value <> Common.DUMMY_NO Then
                            SetValueDGV("フリガナ", ownForm.DGV1.CurrentRow.Cells(2).Value)
                        End If
                    Case 5
                        ' UserListにあるユーザー名をコンボボックスのItemsに設定
                        SetComboBoxItemsDGV("再鑑者", userList)
                    Case 6
                        ' UserListにあるユーザー名をコンボボックスのItemsに設定
                        SetComboBoxItemsDGV("受領者", userList)
                End Select

            Case BTEDIT ' 編集ボタン契機
                ' DGVデータ読み込み
                SetComboBoxItemsDGV("担当者", userList)
                SetValueDGV("担当者", xml.GetUserName)
                LoadDataGridView()
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
    End Sub


    ' DB(MRItem)を元にDGVを作成
    Private Sub FillDataGridView()
        ' DGVの既存の行をクリア
        DGV_REG1.Rows.Clear()

        ' 定義データを取得
        Dim dr As DataRow() = ownForm.db.OrgDataTable(Sqldb.TID.MRM).Select($"C01 = '{MRType}'")

        ' DGVに新しい行を追加
        For Each row As DataRow In dr
            Dim order As String = row("C02").ToString()  ' 表示順
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

                Case Else
                    If format.Contains(",") Then
                        Dim items As String() = format.Split(",")
                        ReplaceCell2ComboBox(DGV_REG1, order, 1, items)
                    End If
            End Select
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
            commandText.Add(DGV_REG1(1, n).Value)
        Next

        If ownForm.ActiveControl.Name = BTADD Then
            commandText(0) = cmn.GetCurrentDateTime()               ' 登録日時
        End If
        commandText(1) = MRType            ' カテゴリ

        ownForm.db.ExeSQLInsUpd(Sqldb.TID.MR, commandText)
        ownForm.db.ExeSQL(Sqldb.TID.MR)
        MsgBox("登録しました。")
        Me.Close()
    End Sub

    ' キャンセルボタン
    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.Close()
    End Sub
#End Region

    ' 番号の最大値+1取得
    Public Function GetNextMaxValue(CateNo As String) As String
        ' DataTableを取得
        Dim dt As DataTable = ownForm.db.OrgDataTable(Sqldb.TID.MR)

        ' CateNoと等しい行をフィルタリングし、3列目の最大値を取得
        Dim maxVal As Integer? = dt.AsEnumerable().
                             Where(Function(row) row.Field(Of String)("C02") = CateNo).
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
        If nextMaxVal.ToString.Length <= NUMBER_LENGTH Then
            Return CateNo & nextMaxVal.ToString("D" & NUMBER_LENGTH)
        Else
            Return nextMaxVal.ToString("D" & NUMBER_LENGTH)
        End If
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

        Dim editTimer As New Timer()
        editTimer.Interval = 100
        ' 値変更時にDataGridViewのセルに反映
        ' 空欄からのdtPicker変更時、DGVに反映されない場合があるため、CommitEditと次のセル選択処理をしている
        AddHandler dtPicker.CloseUp, Sub(sender, e)
                                         dgv.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                         dgv(colIndex, rowIndex).Value = dtPicker.Value.ToString(formatType)
                                         ' 次の行のセルを選択し、編集モードにする
                                         If rowIndex + 1 < dgv.Rows.Count Then
                                             dgv.CurrentCell = dgv(colIndex, rowIndex + 1)
                                             dgv.BeginEdit(True)
                                         End If
                                         editTimer.Stop()
                                     End Sub
        AddHandler dtPicker.Leave, Sub(sender, e)
                                       DGV_REG1.Focus()
                                       log.cLog("Cocus")
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

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles DGV_REG1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
            Case Keys.F2
            Case Keys.F3
        End Select
    End Sub

End Class