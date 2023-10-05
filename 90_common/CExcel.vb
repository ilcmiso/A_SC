Imports ClosedXML
Imports Microsoft.Office.Interop.Excel
Imports System.Windows.Forms

Public Class CExcel

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

    ' Excel出力
    Public Sub ExportToExcel(data As List(Of List(Of String)), fileName As String)
        Dim excelApp As New Microsoft.Office.Interop.Excel.Application
        Dim workbook As Workbook = excelApp.Workbooks.Add()
        Dim worksheet As Worksheet = workbook.Sheets(1)

        For i = 0 To data.Count - 1
            For j = 0 To data(i).Count - 1
                Dim cell = worksheet.Cells(i + 1, j + 1)
                cell.NumberFormat = "@"  ' "@"はテキスト形式を意味します
                cell.Value = data(i)(j)
            Next
        Next

        Dim saveFileDialog As New SaveFileDialog()
        saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx"
        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        saveFileDialog.FileName = fileName

        If saveFileDialog.ShowDialog() = DialogResult.OK Then
            workbook.SaveAs(saveFileDialog.FileName)
            workbook.Close()
            excelApp.Quit()
            Process.Start(saveFileDialog.FileName)
        Else
            workbook.Close(False)
            excelApp.Quit()
        End If
    End Sub


End Class
