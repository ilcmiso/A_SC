Imports DocumentFormat.OpenXml.Spreadsheet

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

    ' 完済日を見つけ、5日～13日（※14日以降が土日祝日なら範囲とする）だった場合に赤色にする
    Public Sub PaymentDateColor(dgv As DataGridView)
        Dim columnIndex As Integer = cmn.FindColumnIndex(dgv, "完済日")
        If columnIndex = -1 Then Return

        For Each row As DataGridViewRow In dgv.Rows
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

    ' 申請物管理DataViewの行データ作成
    Public Function LoadDataViewInfo(ByRef dv As DataView, category As String) As DataView

        ' SQLiteからカラムデータを取得
        Dim dr As DataRow() = SCA1.db.OrgDataTable(Sqldb.TID.MRM).Select($"C01 = {category}", "C02 ASC")
        Dim dtNew As New DataTable("TBL")

        ' カラム情報を設定
        For Each row As DataRow In dr
            Dim columnName As String = row("C03").ToString()
            Dim columnWidthStr As String = row("C04").ToString()
            Dim columnWidth As Integer

            If String.IsNullOrEmpty(columnWidthStr) Then
                columnWidth = 60 ' デフォルト値
            Else
                columnWidth = Integer.Parse(columnWidthStr)
            End If

            ' カラム幅など、UI側の情報はExtendedPropertiesに保持しておく
            Dim dc As New DataColumn(columnName, GetType(String))
            dc.ExtendedProperties("Width") = columnWidth
            dtNew.Columns.Add(dc)
        Next

        ' SQLiteから申請物データを取得
        Dim dtSource As DataTable = SCA1.db.GetSelect(Sqldb.TID.MR, $"SELECT * FROM {SCA1.db.GetTable(Sqldb.TID.MR)} WHERE C02 = '{category}'")
        ' 行データを設定
        For row = 0 To dtSource.Rows.Count - 1
            Dim newRow As DataRow = dtNew.NewRow()
            For i As Integer = 0 To dtNew.Columns.Count - 1
                newRow(i) = dtSource.Rows(row)($"C{i + 1:D2}")
            Next
            dtNew.Rows.Add(newRow)
        Next

        dv = New DataView(dtNew)
        Return dv
    End Function


    ' DataGridViewの指定カラムに指定文字列が含まれる場合、その行の全セルの背景色を変更する汎用メソッド
    Public Sub HighlightRows(dgv As DataGridView, columnName As String, hitWords As String, color As System.Drawing.Color)
        ' カラムのインデックスを探す
        Dim columnIndex As Integer = cmn.FindColumnIndex(dgv, columnName)
        If columnIndex = -1 Then Return ' カラムが見つからなければ処理を終了

        ' DataGridViewの各行をループして条件に一致する行のセルの背景色を変更
        For Each row As DataGridViewRow In dgv.Rows
            If Not row.IsNewRow AndAlso Not IsDBNull(row.Cells(columnIndex).Value) Then
                Dim cellValue As String = row.Cells(columnIndex).Value.ToString()
                If (String.IsNullOrEmpty(hitWords) AndAlso Not String.IsNullOrEmpty(cellValue)) OrElse (Not String.IsNullOrEmpty(hitWords) AndAlso cellValue.Contains(hitWords)) Then
                    ' HitWordsが空欄の場合は指定カラムに何かが含まれていれば、その行の全セルの背景色を設定
                    ' HitWordsが指定されている場合はその文字列が含まれている場合に背景色を設定
                    For Each cell As DataGridViewCell In row.Cells
                        cell.Style.BackColor = color
                    Next
                End If
            End If
        Next
    End Sub

    ' カラム名に完済日が含まれているか
    Public Function IsColumnsPaymentDate(dgv As DataGridView) As Boolean
        Dim columnIndex As Integer = cmn.FindColumnIndex(dgv, "完済日")
        If columnIndex = -1 Then Return False
        Return True
    End Function

End Class
