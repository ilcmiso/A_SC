Imports ClosedXML

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



End Class
