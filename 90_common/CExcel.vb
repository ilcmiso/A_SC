Imports ClosedXML
Imports Microsoft.Office.Interop.Excel
Imports System.Windows.Forms

Public Class CExcel

    Public Const FILTER_EXCEL As String = "Excel files (*.xlsx)|*.xlsx"

    Public Function ReadExc(ExcFilePath As String, SheetName As String) As String(,)
        Dim ret(,) As String = Nothing
        'Dim xlBook As New Excel.XLWorkbook("C:\Users\hiro\Desktop\A_SC物件情報.xlsx")
        Try
            Dim xlBook As New Excel.XLWorkbook(ExcFilePath)
            Dim xlSheet As Excel.IXLWorksheet = xlBook.Worksheet("Sheet1")
            Dim endRow As Integer = xlSheet.RowsUsed().Count
            Dim endCol As Integer = xlSheet.ColumnsUsed().Count

            ReDim ret(endCol, endRow)
            For row = 0 To endRow - 1
                For col = 0 To endCol - 1
                    ret(col, row) = xlSheet.Cell(row + 1, col + 1).Value.ToString
                Next
            Next
        Catch ex As Exception
            MsgBox("エクセル読み込みエラーが発生しました。" & vbCrLf &
                   "もしエクセルファイルが開いている場合は閉じてから実行してください。")
        End Try
        Return ret
    End Function

    ' ファイル保存場所ダイアログ関数
    Public Function GetSaveFileName(filter As String, defaultFileName As String) As String
        Dim saveFileDialog As New SaveFileDialog()
        saveFileDialog.Filter = filter
        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        saveFileDialog.FileName = defaultFileName
        If saveFileDialog.ShowDialog() = DialogResult.OK Then
            Return saveFileDialog.FileName
        Else
            Return String.Empty
        End If
    End Function

    ' Excel出力関数
    Public Sub ExportToExcel(data As List(Of List(Of String)), fileName As String)
        If data Is Nothing OrElse data.Count = 0 Then Return

        Dim excelApp As New Microsoft.Office.Interop.Excel.Application
        Dim workbook As Workbook = excelApp.Workbooks.Add()
        Dim worksheet As Worksheet = workbook.Sheets(1)

        Dim rowCount As Integer = data.Count
        Dim colCount As Integer = 0
        If data(0) IsNot Nothing Then
            colCount = data(0).Count
        End If

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

        workbook.SaveAs(fileName)
        workbook.Close()
        excelApp.Quit()
        Process.Start(fileName)
    End Sub

End Class
