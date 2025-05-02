Imports System.IO
Imports ExcelDataReader

Public Class ExcelDataReaderUtil
    ''' <summary>
    ''' 指定されたExcelファイル(.xlsm/.xlsx)のA1から有効範囲までの値を読み込み、List(Of String)で返す
    ''' </summary>
    ''' <param name="excelPath">読み込むExcelファイルのパス</param>
    ''' <returns>セル値を1行ずつ格納したList(Of String)</returns>
    Public Shared Function ExcelToList(excelPath As String) As List(Of String)
        Dim result As New List(Of String)

        Using stream As FileStream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader As IExcelDataReader = ExcelReaderFactory.CreateReader(stream)
                Dim ds = reader.AsDataSet()
                Dim table = ds.Tables(0) ' 最初のシート

                For Each row As DataRow In table.Rows
                    Dim rowValues As New List(Of String)
                    For Each col As Object In row.ItemArray
                        rowValues.Add(If(col IsNot Nothing, col.ToString(), ""))
                    Next
                    result.Add(String.Join(",", rowValues))
                Next
            End Using
        End Using

        Return result
    End Function
End Class
