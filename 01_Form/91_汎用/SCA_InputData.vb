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
                Case Else
                    MessageBox.Show("サポートされていないファイル形式です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
        End If
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

    Private Sub HandleTextFile(txtPath As String)
        Dim lines() As String = File.ReadAllLines(txtPath, Encoding.GetEncoding("Shift-JIS")) ' Shift-JISエンコーディングを指定

        For Each line As String In lines
            Dim parts() As String = line.Split(New Char() {":"c}, 2)
            If parts.Length = 2 Then
                Dim tableNo As Integer = Integer.Parse(parts(0).Trim())
                Dim sql As String = parts(1).Trim()
                ExecuteSQLCommand(tableNo, sql)
            End If
        Next
    End Sub

    Private Sub ExecuteSQLCommand(TableNo As Integer, sql As String)
        Dim db As New Sqldb
        db.ExeSQL(TableNo, sql)
        MessageBox.Show("データベースの更新が完了しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
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
