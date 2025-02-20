Imports System.IO
Imports System.Windows.Forms
Imports System.Diagnostics
Imports System.Web.UI.WebControls

Public Class AppUpdate
    Private Shared ReadOnly log As New Log()

    ''' <summary>
    ''' ローカルとサーバーの更新時刻を比較し、更新が必要かどうかを判定する。
    ''' </summary>
    Public Shared Function IsUpdateAvailable() As Boolean
        Dim xml As New XmlMng
        Dim localPath As String = SC.CurrentAppPath
        Dim serverPath As String = xml.GetCPath() & Common.DIR_UPD

        Dim serverExe As String = serverPath & Common.EXE_NAME
        If Not IO.File.Exists(serverExe) Then
            ' サーバーに更新ファイルが無い場合は更新不要
            Return False
        End If

        Dim localExe As String = localPath & Common.EXE_NAME
        Dim localTime As Date = IO.File.GetLastWriteTime(localExe)
        Dim serverTime As Date = IO.File.GetLastWriteTime(serverExe)

        log.D(Log.DBG, "Update: 差分チェック　local=" & localTime & " server=" & serverTime)

        ' タイムスタンプが一致していれば更新不要
        Return localTime <> serverTime
    End Function

    ''' <summary>
    ''' サーバー上の更新ファイルのタイムスタンプをバージョン情報として文字列で取得する。
    ''' </summary>
    Public Shared Function GetLatestVersionInfo() As String
        Dim xml As New XmlMng
        Dim serverPath As String = xml.GetCPath() & Common.DIR_UPD
        Dim serverExe As String = serverPath & Common.EXE_NAME
        If IO.File.Exists(serverExe) Then
            Dim serverTime As Date = IO.File.GetLastWriteTime(serverExe)
            ' バージョン情報として"yyyyMMddHHmmss"形式の文字列を返す
            Return serverTime.ToString("yyyy/MM/dd HH:mm:ss")
        End If
        Return "不明"
    End Function

    ''' <summary>
    ''' 更新処理を実行する。更新前にユーザーに「更新しますか？」の確認を行い、
    ''' 「はい」が選択された場合、ローカルEXEをリネーム後、サーバーの全更新ファイルをコピーし、
    ''' ローカルファイルのタイムスタンプを合わせた上でアプリを再起動する。
    ''' </summary>
    Public Shared Function PerformUpdate() As Boolean
        Dim xml As New XmlMng
        Dim localPath As String = SC.CurrentAppPath
        Dim serverPath As String = xml.GetCPath() & Common.DIR_UPD
        Dim localExe As String = localPath & Common.EXE_NAME
        Dim serverExe As String = serverPath & Common.EXE_NAME

        If Not IO.File.Exists(serverExe) Then
            MessageBox.Show("サーバー上に更新ファイルが見つかりません。", "更新エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If

        Dim localTime As Date = IO.File.GetLastWriteTime(localExe)
        Dim serverTime As Date = IO.File.GetLastWriteTime(serverExe)

        ' 差分がなければ更新不要
        If localTime = serverTime Then
            MessageBox.Show("既に最新版です。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If

        ' ユーザーに更新確認のメッセージボックスを表示
        Dim latestVersionInfo As String = GetLatestVersionInfo()
        Dim confirmMsg As String = "最新バージョンのアプリがあります。" & vbCrLf &
                                   "最新バージョン: " & latestVersionInfo & vbCrLf &
                                   "アプリを更新しますか？"
        Dim result As DialogResult = MessageBox.Show(confirmMsg, "アップデート確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result <> DialogResult.Yes Then
            Return False
        End If

        Cursor.Current = Cursors.WaitCursor
        Try
            log.D(Log.DBG, "AppUpdate: 更新開始")
            Dim oldExe As String = localExe & ".old"
            ' 既存の古いexeがあれば削除
            If IO.File.Exists(oldExe) Then IO.File.Delete(oldExe)
            ' 現在のEXEをリネーム（更新対象から除外）
            IO.File.Move(localExe, oldExe)

            ' サーバー上の更新ディレクトリ内全ファイルをローカルにコピー
            My.Computer.FileSystem.CopyDirectory(serverPath, localPath, True)

            ' たまにタイムスタンプに誤差が生じるため、ローカルファイルの更新日時を合わせる
            IO.File.SetLastWriteTime(localExe, serverTime)
        Catch ex As Exception
            ' 更新失敗時は旧ファイルを復元する
            Dim oldExe As String = localExe & ".old"
            If IO.File.Exists(oldExe) Then IO.File.Move(oldExe, localExe)
            MessageBox.Show("A_SCのファイル更新が正常に行われませんでした。" & vbCrLf & ex.Message,
                            "更新エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            log.D(Log.ERR, "ファイル更新失敗: localTime=" & localTime & " serverTime=" & serverTime & vbCrLf & ex.Message)
            Return False
        End Try

        log.D(Log.DBG, "AppUpdate: 更新完了")
        Return True
    End Function

    Public Shared Sub AppRestart()
        MessageBox.Show($"アプリが最新に更新されました。{vbCrLf}アプリを再起動します。", "更新完了", MessageBoxButtons.OK, MessageBoxIcon.Information)

        ' 更新後、アプリケーションを再起動する
        Try
            Dim startInfo As New ProcessStartInfo
            startInfo.FileName = SC.CurrentAppPath & Common.EXE_NAME
            Process.Start(startInfo)
        Catch ex As Exception
            MessageBox.Show("更新後のアプリ再起動に失敗しました。" & vbCrLf & ex.Message, "再起動エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Application.Exit()
    End Sub
End Class
