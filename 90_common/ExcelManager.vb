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
        Try
            workbook.SaveAs(filePath)
        Catch ex As System.Runtime.InteropServices.COMException
            MsgBox("Excelファイルが既に開かれています。閉じてからもう一度お試しください。")
            Exit Sub
        End Try
        If workbook IsNot Nothing Then workbook.Close()
        If excelApp IsNot Nothing Then excelApp.Quit()
        Runtime.InteropServices.Marshal.ReleaseComObject(excelApp)
        Runtime.InteropServices.Marshal.ReleaseComObject(workbook)
    End Sub

    ' シートをアクティブにする
    Public Sub ActivateSheet(sheetName As String)
        Dim worksheet As Worksheet
        Try
            worksheet = workbook.Sheets(sheetName)
            worksheet.Activate()
        Catch ex As Exception
            MsgBox($"シート「{sheetName}」が見つかりません。", MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Sub OpenFile()
        Process.Start(filePath)
    End Sub


    ' 複数のExcelファイルを1つのBookにシートをマージ  シート名を返却
    Public Function MergeExcelFiles(ByVal FilePaths As List(Of String), OutPath As String) As List(Of String)
        Dim excelApp As New Application
        Dim WbTarget As Workbook = excelApp.Workbooks.Add()
        Dim WbSource As Workbook
        Dim WsSource As Worksheet
        Dim WsTarget As Worksheet
        Dim FilePath As String
        Dim SheetName As String
        Dim NewSheetName As String
        Dim SheetCounter As Integer
        Dim SheetList As New List(Of String)

        ' Excelの表示をオフにする
        excelApp.Visible = False

        ' 先頭のデフォルトシートの名前を "XXX" に変更
        WbTarget.Sheets(1).Name = "XXXXX"

        ' 引数で渡されたExcelファイルのパスをループ
        For Each FilePath In FilePaths
            ' Excelファイルを開く
            WbSource = excelApp.Workbooks.Open(FilePath)

            ' 各シートをターゲットのブックにコピー
            For Each WsSource In WbSource.Sheets
                ' 元のシート名を取得
                SheetName = WsSource.Name
                NewSheetName = SheetName
                SheetCounter = 1

                ' シート名が重複している場合、連番を付ける
                Do While SheetExists(WbTarget, NewSheetName)
                    NewSheetName = SheetName & "_" & SheetCounter
                    SheetCounter += 1
                Loop

                ' ターゲットに新しいシートを追加
                WsTarget = WbTarget.Sheets.Add(After:=WbTarget.Sheets(WbTarget.Sheets.Count))

                ' 新しいシート名を設定
                WsTarget.Name = NewSheetName
                SheetList.Add(WsTarget.Name)

                ' シートの内容をコピー
                WsSource.Cells.Copy(WsTarget.Cells)
            Next

            ' ソースのブックを閉じる
            WbSource.Close(False)
        Next

        ' マージが完了したら、先頭の "XXX" シートを削除
        WbTarget.Sheets("XXXXX").Delete()

        ' ターゲットのブックを保存
        WbTarget.SaveAs(OutPath)
        WbTarget.Close()

        ' Excelアプリケーションを終了
        excelApp.Quit()

        ' リソースの解放
        ReleaseObject(WbTarget)
        ReleaseObject(WbSource)
        ReleaseObject(excelApp)
        Return SheetList

    End Function

    ' 指定したシート名がターゲットブックに存在するかチェックする関数
    Private Function SheetExists(ByVal Wb As Workbook, ByVal SheetName As String) As Boolean
        On Error Resume Next
        Dim Ws As Worksheet = Wb.Sheets(SheetName)
        If Ws Is Nothing Then
            SheetExists = False
        Else
            SheetExists = True
        End If
        On Error GoTo 0
    End Function

    ' オブジェクト解放用の関数
    Private Sub ReleaseObject(ByVal obj As Object)
        Try
            If obj IsNot Nothing Then
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
                obj = Nothing
            End If
        Catch ex As Exception
            obj = Nothing
        Finally
            GC.Collect()
        End Try
    End Sub

End Class
