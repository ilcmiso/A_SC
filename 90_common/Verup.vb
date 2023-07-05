Public Class Verup
    ' Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log

    ' 指定ファイル(EXE_NAME)更新されていた場合に、
    ' サーバーフォルダ(S_PATH)内の全てのファイルを、ローカルフォルダ(L_PATH)内にコピーする
    Public Function Update(Ver As String) As Boolean
        Dim xml As New XmlMng
        Dim L_PATH = SC.CurrentAppPath
        Dim S_PATH = xml.GetCPath() & Common.DIR_UPD          ' 更新ファイル設置場所は固有設定から取得

        If xml.GetAplUpdOff Then
            log.D(Log.DBG, "アプリ更新停止中")
            Return False
        End If

        log.D(Log.DBG, "Update:ファイル: " & IO.File.Exists(S_PATH & Common.EXE_NAME) & ": " & S_PATH & Common.EXE_NAME)
        ' サーバーにバージョンアップファイルがない場合は更新不要
        If Not IO.File.Exists(S_PATH & Common.EXE_NAME) Then Return False

        Dim timel As Date = IO.File.GetLastWriteTime(L_PATH & Common.EXE_NAME)    ' ローカルファイルの更新時刻
        Dim times As Date = IO.File.GetLastWriteTime(S_PATH & Common.EXE_NAME)    ' サーバーファイルの更新時刻

        ' タイムスタンプが一致している場合は、既に最新なので更新不要
        log.D(Log.DBG, "Update:差分無し: " & (timel = times) & ": timel." & timel & " times." & times)
        If timel = times Then Return False

        ' ------ ここからファイルの更新処理 ------
        MsgBox("ファイル更新をします。" & vbCrLf &
               "OKボタンを押して、しばらくお待ち下さい。")
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Try
            log.D(Log.DBG, "Update:更新開始")
            IO.File.Delete(L_PATH & Common.EXE_NAME & ".old")                ' 古いexe(.old)は削除する
            IO.File.Move(L_PATH & Common.EXE_NAME, L_PATH & Common.EXE_NAME & ".old")  ' 起動中のA_SX.exeは上書きできないのでリネーム(削除扱い)する

            ' サーバーの更新ディレクトリ内を全てコピー
            My.Computer.FileSystem.CopyDirectory(S_PATH, L_PATH, True)

            ' たまにタイムスタンプに誤差が生まれるので、ローカルファイルの更新日付を上書き
            IO.File.SetLastWriteTime(L_PATH & Common.EXE_NAME, times)
        Catch ex As Exception
            IO.File.Move(L_PATH & Common.EXE_NAME & ".old", L_PATH & Common.EXE_NAME)  ' 失敗したらoldをもとに戻す
            MsgBox("A_SCのファイル更新が正常に行われませんでした。" & vbCrLf & ex.Message, MessageBoxIcon.Warning)
            Log.D(Log.ERR, "ファイル更新失敗" & timel & ", " & times & vbCrLf & ex.Message)
            Return False
        End Try
        log.D(Log.DBG, "Update:更新完了 旧Ver:" & Ver)
        MsgBox("A_SCが最新版に更新されました。")
        Return True
    End Function
End Class
