Imports Microsoft.Office.Interop.Excel

Public Class ExcelManager
    Private ReadOnly excelApp As Application
    Private ReadOnly workbook As Workbook
    Private ReadOnly filePath As String ' ファイルパスを保持するための変数

    Public Sub New(fileName As String)
        excelApp = New Application
        workbook = excelApp.Workbooks.Add()
        Me.filePath = fileName ' コンストラクターでファイル名を設定
    End Sub

    ' List型のデータをExcelに出力する
    Public Sub ExportToExcel(data As List(Of List(Of String)), sheetName As String)
        If data Is Nothing OrElse data.Count = 0 Then Return

        Dim worksheet As Worksheet
        Try
            worksheet = workbook.Sheets(sheetName)
        Catch ex As Exception
            ' 指定された名前のシートが見つからなかった場合、新しいシートを作成
            worksheet = workbook.Sheets.Add(After:=workbook.Sheets(workbook.Sheets.Count))
            worksheet.Name = sheetName
        End Try

        Dim rowCount As Integer = data.Count
        Dim colCount As Integer = If(data(0) IsNot Nothing, data(0).Count, 0)

        Dim dataArray(rowCount - 1, colCount - 1) As Object
        For i = 0 To rowCount - 1
            If data(i) IsNot Nothing Then
                For j = 0 To Math.Min(data(i).Count, colCount) - 1
                    dataArray(i, j) = data(i)(j)
                Next
            End If
        Next

        Dim c1 As Range = worksheet.Cells(1, 1)
        Dim c2 As Range = worksheet.Cells(rowCount, colCount)
        Dim range As Range = worksheet.Range(c1, c2)

        range.NumberFormat = "@"
        range.Value = dataArray
        range.EntireColumn.AutoFit()

    End Sub

    ' 表示中のDGVをExcelに出力する（非表示列・非表示行は出力しない）
    Public Sub ExportDGVToExcel(dgv As DataGridView)
        Dim data As New List(Of List(Of String))
        Dim visibleColumnIndexes As New List(Of Integer) ' 表示されているカラムのインデックスを保持するリスト

        ' 表示されているカラム名を取得し、そのインデックスを保存
        Dim columnNames As New List(Of String)
        For Each col As DataGridViewColumn In dgv.Columns
            If col.Visible Then ' カラムが表示されているかチェック
                columnNames.Add(col.HeaderText)
                visibleColumnIndexes.Add(col.Index) ' 表示されているカラムのインデックスを追加
            End If
        Next
        data.Add(columnNames) ' 表示されているカラム名をリストの最初の要素として追加

        ' 各行のデータを取得（表示されているカラムのみ、非表示の行は無視）
        For Each row As DataGridViewRow In dgv.Rows
            If Not row.IsNewRow AndAlso row.Visible Then ' 新しい行でなく、行が表示されていることを確認
                Dim rowList As New List(Of String)
                For Each index As Integer In visibleColumnIndexes ' 表示されているカラムのインデックスに従ってデータを追加
                    Dim cell As DataGridViewCell = row.Cells(index)
                    rowList.Add(If(cell.Value IsNot Nothing, cell.Value.ToString(), ""))
                Next
                data.Add(rowList)
            End If
        Next
        ExportToExcel(data, "Sheet1")
    End Sub

    ' 指定したシート名のシートを削除する
    Public Sub DeleteSheet(sheetName As String)
        Dim sheet As Worksheet = Nothing
        For Each sh As Worksheet In workbook.Sheets
            If sh.Name = sheetName Then
                sheet = sh
                Exit For
            End If
        Next

        If sheet IsNot Nothing Then sheet.Delete()
    End Sub

    Public Sub SaveAndClose()
        workbook.SaveAs(filePath)
        workbook.Close()
        excelApp.Quit()
    End Sub

    Public Sub OpenFile()
        Process.Start(filePath)
    End Sub
End Class
