Imports System.IO
Imports DocumentFormat.OpenXml.Spreadsheet

Public Class SCGA_REG

    Private ReadOnly cmn As New Common
    Private ReadOnly xml As New XmlMng
    Private ReadOnly log As New Log
    Private ownForm As SCA1
    Private Const BTADD As String = "BT_MRAdd"
    Private Const BTEDIT As String = "BT_MREdit"

#Region " OPEN CLOSE "
    Private Sub FLS_Shown(sender As Object, e As EventArgs) Handles MyBase.Load
        ownForm = DirectCast(Me.Owner, SCA1)            ' 親フォームを参照できるようにキャスト
        cmn.SetDoubleBufferDGV(DGV_REG1)
        Label33.Text = $"{ownForm.CB_MRLIST.SelectedItem} の 登録"

        FillDataGridView()
        ' 編集ボタン契機だとデータ読み込みを含める
        If ownForm.ActiveControl.Name = BTEDIT Then
            LoadDataGridView()
        End If
    End Sub

    Private Sub LoadDataGridView()
        For n = 0 To ownForm.DGV_MR1.CurrentRow.Cells.Count - 1
            DGV_REG1(1, n).Value = ownForm.DGV_MR1.CurrentRow.Cells(n).Value
            'DirectCast(DGV_REG1(1, n).Tag, DateTimePicker).Value = targetDate
        Next
    End Sub

    Private Sub FillDataGridView()
        ' DGVの既存の行をクリア
        DGV_REG1.Rows.Clear()

        ' 定義データを取得
        Dim dr As DataRow() = ownForm.db.OrgDataTable(Sqldb.TID.MRM).Select($"C01 = '{ownForm.CB_MRLIST.SelectedIndex}'")

        ' DGVに新しい行を追加
        For Each row As DataRow In dr
            Dim order As String = row("C02").ToString()  ' 表示順
            Dim width As String = row("C04").ToString()  ' 横幅
            Dim format As String = row("C05").ToString() ' 表示形式

            DGV_REG1.Rows.Add(row(2))
            If width = 0 Then
                DGV_REG1.Rows(DGV_REG1.Rows.Count - 1).Visible = False
            End If

            Select Case format
                Case "Cal"
                    ReplaceCell2DTP(DGV_REG1, order, 1)
                Case "CB"
                    ReplaceCell2Checkbox(DGV_REG1, order, 1)
                Case Else
                    If format.Contains(",") Then
                        Dim items As String() = format.Split(",")
                        ReplaceCell2ComboBox(DGV_REG1, order, 1, items)
                    End If
            End Select
        Next
    End Sub
#End Region

#Region "ボタン"
    ' 登録ボタン
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        '' 値を設定する例
        'Dim targetDate As DateTime = DateTime.Parse("2022/2/22")
        'DGV_REG1(1, 3).Value = targetDate
        'DirectCast(DGV_REG1(1, 3).Tag, DateTimePicker).Value = targetDate

        ' SQLコマンド生成
        Dim commandText As New List(Of String)
        For n = 0 To DGV_REG1.Rows.Count - 1
            commandText.Add(DGV_REG1(1, n).Value)
        Next

        If ownForm.ActiveControl.Name = BTADD Then
            commandText(0) = cmn.GetCurrentDateTime()               ' 登録日時
        End If
        commandText(1) = ownForm.CB_MRLIST.SelectedIndex            ' カテゴリ

        ownForm.db.ExeSQLInsUpd(Sqldb.TID.MR, commandText)
        ownForm.db.ExeSQL(Sqldb.TID.MR)
        MsgBox("登録しました。")
        ownForm.db.UpdateOrigDT(Sqldb.TID.MR)
        ownForm.ShowDGVMR()
        Me.Close()
    End Sub

    ' キャンセルボタン
    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.Close()
    End Sub

    Private Sub ReplaceCell2DTP(dgv As DataGridView, rowIndex As Integer, colIndex As Integer)
        Dim rect As Rectangle = dgv.GetCellDisplayRectangle(colIndex, rowIndex, False)
        Dim dtPicker As New DateTimePicker()

        ' DateTimePickerの位置とサイズを設定
        'dtPicker.Location = rect.Location
        'dtPicker.Size = rect.Size

        dtPicker.Location = New Point(rect.Location.X + 132, rect.Location.Y) ' X座標を50ピクセル右にずらす
        dtPicker.Size = New Size(18, rect.Height) ' 幅を18ピクセルに設定

        ' 日付のみ表示するようにフォーマット設定
        dtPicker.Format = DateTimePickerFormat.Short

        ' 初期値を設定
        Dim initialDate As DateTime = DateTime.Now
        dtPicker.Value = initialDate
        dgv(colIndex, rowIndex).Value = initialDate.ToString("yyyy/MM/dd")  ' ここでDataGridViewのセルに初期値を設定
        dgv(colIndex, rowIndex).Tag = dtPicker  ' TagプロパティにDateTimePickerを設定

        Dim editTimer As New Timer()
        editTimer.Interval = 100
        ' 値変更時にDataGridViewのセルに反映
        ' 空欄からのdtPicker変更時、DGVに反映されない場合があるため、CommitEditと次のセル選択処理をしている
        AddHandler dtPicker.CloseUp, Sub(sender, e)
                                         dgv.CommitEdit(DataGridViewDataErrorContexts.Commit)
                                         dgv(colIndex, rowIndex).Value = dtPicker.Value.ToString("yyyy/MM/dd")
                                         ' 次の行のセルを選択し、編集モードにする
                                         If rowIndex + 1 < dgv.Rows.Count Then
                                             dgv.CurrentCell = dgv(colIndex, rowIndex + 1)
                                             dgv.BeginEdit(True)
                                         End If
                                         editTimer.Stop()
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
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList

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

    Private Sub ReplaceCell2Checkbox(dgv As DataGridView, rowIndex As Integer, colIndex As Integer)
        ' DataGridViewのセルの位置とサイズを取得
        Dim rect As Rectangle = dgv.GetCellDisplayRectangle(colIndex, rowIndex, False)

        ' CheckBoxコントロールを生成
        Dim checkBox As New CheckBox()
        checkBox.Size = New Size(rect.Width, rect.Height)
        checkBox.Location = New Point(rect.X, rect.Y)

        ' 値変更時にDataGridViewのセルに反映
        AddHandler checkBox.CheckedChanged, Sub(sender, e)
                                                dgv(colIndex, rowIndex).Value = checkBox.Checked
                                            End Sub

        ' CheckBoxをDataGridViewに追加
        dgv.Controls.Add(checkBox)
    End Sub

    ' ショートカット F1
    Private Sub SCA1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles DGV_REG1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                For n = 0 To DGV_REG1.Rows.Count - 1
                    log.cLog(DGV_REG1(1, n).Value)
                Next
            Case Keys.F2
                LoadDataGridView()
            Case Keys.F3
        End Select
    End Sub

#End Region

End Class