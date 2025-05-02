Imports System.IO
Imports System.IO.Compression
Imports System.Data.SqlClient
Imports System.Text

Public Class SCA_InputData

    Private ReadOnly cmn As New Common

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' フォームの設定
        Me.AllowDrop = True
        AddHandler Me.DragEnter, AddressOf MainForm_DragEnter
        AddHandler Me.DragDrop, AddressOf MainForm_DragDrop
    End Sub

    Private Sub MainForm_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub MainForm_DragDrop(sender As Object, e As DragEventArgs)
        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length > 0 Then
            Dim filePath As String = files(0)
            Dim extension As String = Path.GetExtension(filePath).ToLower()

            Select Case extension
                Case ".zip"
                    HandleZipFile(filePath)
                Case ".txt"
                    HandleTextFile(filePath)
                Case ".xlsm"
                    HandleXlsmFile(filePath)
                Case Else
                    MessageBox.Show("サポートされていないファイル形式です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
        End If
        Me.Close()
    End Sub

    Private Sub HandleZipFile(zipPath As String)
        Dim extractPath As String = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(zipPath))

        Try
            If File.Exists(zipPath) Then
                ExtractZipFile(zipPath, extractPath)
                ' フォルダ上書き保存処理
                Dim destinationPath As String = cmn.CurrentPath
                CopyDirectory(extractPath, destinationPath)
                MessageBox.Show("ファイルの更新が完了しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ' 一時ディレクトリの削除
                Directory.Delete(extractPath, True)
            Else
                MessageBox.Show("指定されたZIPファイルが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ExtractZipFile(zipPath As String, extractPath As String)
        Using archive As ZipArchive = ZipFile.OpenRead(zipPath)
            For Each entry As ZipArchiveEntry In archive.Entries
                Dim destinationPath = Path.Combine(extractPath, entry.FullName)
                Dim destinationDir = Path.GetDirectoryName(destinationPath)
                If Not Directory.Exists(destinationDir) Then
                    Directory.CreateDirectory(destinationDir)
                End If
                If entry.Name <> "" Then
                    entry.ExtractToFile(destinationPath, True)
                End If
            Next
        End Using
    End Sub

    ' Excel交渉記録の登録
    Private Sub HandleXlsmFile(xlsmPath As String)
        Dim dataList As List(Of String) = ExcelDataReaderUtil.ExcelToList(xlsmPath)
        Dim cnt As Integer = 0
        Dim NGList As New List(Of String)

        dataList.RemoveRange(0, 5)  ' Excelの読み込みたくない先頭5行を削除
        For Each line As String In dataList
            Dim inData As List(Of String) = line.Split(","c).ToList()
            Dim cid As String = inData(1)
            Dim cName As String() = SCA1.db.GetCosName(cid)

            ' 顧客番号ば存在しない場合はNGリスト追加
            If cName Is Nothing Then
                NGList.Add(cid)
                Continue For
            End If

            ' ExcelのLF改行だとVBでは改行を認識できないため、vbCrLfに変換
            ' inData(6)は「内容」欄のみ変換
            If inData(6).Contains(vbLf) Then inData(6) = inData(6).Replace(vbLf, vbCrLf)

            ' 「日時」と「督促日」の形式を整える
            Try
                Dim dt As DateTime
                If Not String.IsNullOrWhiteSpace(inData(2)) Then
                    If DateTime.TryParse(inData(2), dt) Then inData(2) = dt.ToString("yyyy/MM/dd HH:mm")
                End If
                If Not String.IsNullOrWhiteSpace(inData(7)) Then
                    If DateTime.TryParse(inData(7), dt) Then inData(7) = dt.ToString("yyyy/MM/dd")
                End If
            Catch
                ' 日付の変換が失敗
                NGList.Add(cid)
                Continue For
            End Try

            ' FKD01 にユニークIDを付加
            inData(0) = $"{Now:yyyyMMddHHmmss}_{Now.Millisecond:D3}_{cnt:D3}"

            ' FKD09,10 に氏名とカナ
            inData(8) = If(cName IsNot Nothing, cName(0), "")
            inData(9) = If(cName IsNot Nothing, cName(1), "")

            ' DBに追加登録
            SCA1.db.ExeSQLInsert(Sqldb.TID.SCD, inData.ToArray)
            cnt += 1
        Next

        Dim r = MessageBox.Show($"{dataList.Count - NGList.Count} 件のデータを読み込みます。{vbCrLf}{vbCrLf}読み込みできない顧客[ {NGList.Count} ]件{vbCrLf}{String.Join(vbCrLf, NGList)}",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbNo Then Exit Sub

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        SCA1.db.ExeSQL(Sqldb.TID.SCD)
        MessageBox.Show("ファイルの更新が完了しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub HandleTextFile(txtPath As String)
        Dim db As New Sqldb
        Dim lines() As String = File.ReadAllLines(txtPath, cmn.GetSmartEncoding(txtPath))

        For Each line As String In lines
            Dim parts() As String = line.Split(New Char() {":"c}, 2)
            If parts.Length = 2 Then
                Dim tableNo As Integer = Integer.Parse(parts(0).Trim())
                Dim sql As String = parts(1).Trim().Replace("<BR>", vbCrLf)     ' テキストに<BR>があれば改行(CRLF)するために置換する
                ExecuteSQLCommand(tableNo, sql)
                db.AddSQL(tableNo, sql)
            End If
        Next
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        db.ExeSQL()
        MessageBox.Show("データベースの更新が完了しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ExecuteSQLCommand(TableNo As Integer, sql As String)
    End Sub

    Private Sub CopyDirectory(sourceDir As String, destDir As String)
        Dim dir As New DirectoryInfo(sourceDir)
        If Not dir.Exists Then
            Throw New DirectoryNotFoundException($"ソースディレクトリが見つかりません: {dir.FullName}")
        End If

        If Not Directory.Exists(destDir) Then
            Directory.CreateDirectory(destDir)
        End If

        For Each file In dir.GetFiles()
            Dim targetFilePath = Path.Combine(destDir, file.Name)
            file.CopyTo(targetFilePath, True)
        Next

        For Each subDir In dir.GetDirectories()
            Dim newDestDir = Path.Combine(destDir, subDir.Name)
            CopyDirectory(subDir.FullName, newDestDir)
        Next
    End Sub

End Class
