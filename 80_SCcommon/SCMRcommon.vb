Public Class SCMRcommon

    Private ReadOnly cmn As New Common
    ' 申請物管理
    Private HolidaysDate As Dictionary(Of Date, String)             ' 祝日データ
    Const START_DAY As Integer = 5
    Const END_DAY As Integer = 13

    ' 祝日データ読み込み
    Public Sub GetHolidayDate()
        HolidaysDate = LoadHolidaysFromCSV($"{cmn.CurrentPath}{Common.DIR_DB3}syukujitsu.csv")      ' 祝日データの読み込み
    End Sub

    ' 指定したカラム名のデータ取得
    Private Function FindColumnIndex(dgv As DataGridView, columnName As String) As Integer
        For i As Integer = 0 To dgv.Columns.Count - 1
            If dgv.Columns(i).Name = columnName Then
                Return i
            End If
        Next
        Return -1
    End Function

    ' 完済日を見つけ、5日～13日（※14日以降が土日祝日なら範囲とする）だった場合に赤色にする
    Public Sub PaymentDateColor()
        Dim columnIndex As Integer = FindColumnIndex(SCA1.DGV_MR1, "完済日")
        If columnIndex = -1 Then Return

        For Each row As DataGridViewRow In SCA1.DGV_MR1.Rows
            If Not IsDBNull(row.Cells(columnIndex).Value) AndAlso Not String.IsNullOrEmpty(row.Cells(columnIndex).Value.ToString()) Then
                Dim dateValue As Date
                If Date.TryParse(row.Cells(columnIndex).Value.ToString(), dateValue) Then
                    ' 範囲内の日付であるかチェックし、条件に合致する場合は赤色にする
                    If AdjustedEndDay(dateValue) Then
                        row.Cells(columnIndex).Style.ForeColor = System.Drawing.Color.Red
                    End If
                End If
            End If
        Next
    End Sub

    ' 指定した日付が5日～13日か判定
    Private Function AdjustedEndDay(dateValue As Date) As Boolean
        Dim isHoliday As Boolean
        ' 指定日が、5日以前なら無条件で非対象
        If dateValue.Day < START_DAY Then Return False
        ' 指定日が、5日～13日なら無条件で合致
        If dateValue.Day >= START_DAY And dateValue.Day <= END_DAY Then Return True

        ' 14日以降の判定
        For day = END_DAY + 1 To dateValue.Day
            isHoliday = dateValue.DayOfWeek = DayOfWeek.Saturday OrElse dateValue.DayOfWeek = DayOfWeek.Sunday OrElse HolidaysDate.ContainsKey(dateValue)
            If Not isHoliday Then Return False
        Next
        ' 14日以降で土日祝日続きだったため、合致判定
        Return True
    End Function

    ' syukujitsu.csvから祝日データ取得
    Function LoadHolidaysFromCSV(csvPath As String) As Dictionary(Of Date, String)
        Dim holidays As New Dictionary(Of Date, String)

        Using reader As New Microsoft.VisualBasic.FileIO.TextFieldParser(csvPath)
            reader.TextFieldType = FileIO.FieldType.Delimited
            reader.SetDelimiters(",")
            While Not reader.EndOfData
                Dim fields = reader.ReadFields()
                If fields(0) <> "国民の祝日・休日月日" Then ' ヘッダー行をスキップ
                    Dim holidayDate As Date
                    If Date.TryParse(fields(0), holidayDate) Then
                        holidays(holidayDate) = fields(1)
                    End If
                End If
            End While
        End Using

        Return holidays
    End Function

    ' 申請物管理DGVのカラム作成
    Public Sub InitDGVInfo(dgv As DataGridView, tid As Integer, index As Integer)
        ' SQLiteからデータを取得
        Dim dr As DataRow() = SCA1.db.OrgDataTable(tid).Select($"C01 = {index}")

        ' DGVを初期化
        dgv.Columns.Clear()

        ' DataRowをソート（C02値により昇順）
        Array.Sort(dr, Function(x, y) x("C02").CompareTo(y("C02")))

        ' DGVにデータをセット
        For Each row As DataRow In dr
            Dim columnName As String = row("C03").ToString()
            Dim columnWidthStr As String = row("C04").ToString()
            Dim columnWidth As Integer

            If String.IsNullOrEmpty(columnWidthStr) Then
                columnWidth = 60 ' デフォルト値
            Else
                columnWidth = Integer.Parse(columnWidthStr)
            End If

            Dim newColumn As New DataGridViewTextBoxColumn()
            newColumn.Name = columnName
            newColumn.HeaderText = columnName
            newColumn.Width = columnWidth
            newColumn.Visible = columnWidth <> 0

            dgv.Columns.Add(newColumn)
        Next
    End Sub

    ' 申請物管理DGVの行データ作成
    Public Sub LoadDGVInfo(dgv As DataGridView, tid As Integer, category As String)
        ' SQLiteからデータを取得
        Dim dr As DataRow() = SCA1.db.OrgDataTable(Sqldb.TID.MR).Select($"C02 = '{category}'")

        ' DGVを初期化
        dgv.Rows.Clear()

        ' DGVのカラム数に応じてデータを表示
        For Each row As DataRow In dr
            Dim newRow As New DataGridViewRow()
            For i As Integer = 0 To dgv.ColumnCount - 1
                Dim cell As New DataGridViewTextBoxCell()
                cell.Value = row($"C{i + 1:D2}")
                newRow.Cells.Add(cell)
            Next
            dgv.Rows.Add(newRow)
        Next
    End Sub

    ' カラム名がキャンセル日の行が、空欄でなかったら行グレーアウト
    Public Sub HighlightCancelledRows(dgv As DataGridView)
        ' "キャンセル日"カラムのインデックスを探す
        Dim columnIndex As Integer = FindColumnIndex(dgv, "キャンセル日")
        If columnIndex = -1 Then Return ' カラムが見つからなければ処理を終了

        ' DataGridViewの各行をループして条件に一致する行のセルの背景色を変更
        For Each row As DataGridViewRow In dgv.Rows
            If Not row.IsNewRow AndAlso Not IsDBNull(row.Cells(columnIndex).Value) AndAlso Not String.IsNullOrEmpty(Trim(row.Cells(columnIndex).Value.ToString())) Then
                ' キャンセル日が空欄でない場合、その行の全セルの背景色を薄いグレーに設定
                For Each cell As DataGridViewCell In row.Cells
                    cell.Style.BackColor = Color.LightGray
                Next
            End If
        Next
    End Sub

    ' カラム名に完済日が含まれているか
    Public Function IsColumnsPaymentDate(dgv As DataGridView) As Boolean
        Dim columnIndex As Integer = FindColumnIndex(dgv, "完済日")
        If columnIndex = -1 Then Return False
        Return True
    End Function

End Class
