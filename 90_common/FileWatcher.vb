Imports System.IO
Imports System.Threading

Public Class FileWatcher
    Private _timer As Timer
    Private _filePath As String
    Private _callback As Action
    Private _lastWriteTime As DateTime

    ' ファイル監視の開始
    Public Sub StartWatching(filePath As String, callback As Action)
        _filePath = filePath
        _callback = callback
        _lastWriteTime = GetLatestWriteTime()

        ' タイマーの設定 (500ミリ秒間隔)
        _timer = New Timer(AddressOf CheckFileChange, Nothing, 0, 500)
    End Sub

    Private Function GetLatestWriteTime() As DateTime
        Dim lastWriteTime As DateTime = New DateTime(0)
        Dim directoryPath As String = Path.GetDirectoryName(_filePath)
        Dim searchPattern As String = Path.GetFileName(_filePath)

        For Each file As String In Directory.GetFiles(directoryPath, searchPattern)
            Dim writeTime As DateTime = System.IO.File.GetLastWriteTime(file)
            If writeTime > lastWriteTime Then
                lastWriteTime = writeTime
            End If
        Next
        Return lastWriteTime
    End Function

    ' ファイル変更のチェック
    Private Sub CheckFileChange(state As Object)
        Dim latestWriteTime = GetLatestWriteTime()
        If latestWriteTime > _lastWriteTime Then
            _lastWriteTime = latestWriteTime
            _callback?.Invoke()
        End If
    End Sub

    ' ファイル監視の終了
    Public Sub StopWatching()
        If _timer IsNot Nothing Then
            _timer.Change(Timeout.Infinite, Timeout.Infinite)
            _timer.Dispose()
        End If
    End Sub
End Class

Public Class FileSetting
    Public Property FilePath As String
    Public Property Callback As Action

    Public Sub New(filePath As String, callback As Action)
        Me.FilePath = filePath
        Me.Callback = callback
    End Sub
End Class