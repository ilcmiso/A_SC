Public Class ILCNetDrive

    Private ReadOnly ServerAddr As String = "https://ilcc.rakusaba.jp:2078"
    Private ReadOnly WebdavAddr As String = "\\ilcc.rakusaba.jp@SSL@2078\DavWWWRoot"

    Public Function Connect(id As String, pw As String) As Boolean
        Dim ret As String

        ' net use で既に登録されているかを確認
        ret = ExeCMDPrompt("net use")
        If ret.IndexOf(WebdavAddr) >= 0 Then
            cLog("既にネットワークドライブの登録済み")
            Return True
        End If

        cLog("ネットワークドライブを新規登録")
        ret = ExeCMDPrompt(String.Format("net use {0} {1} /user:{2}@ilcc.rakusaba.jp /persistent:yes", ServerAddr, pw, id))
        If ret.IndexOf("エラー") < 0 Then
            cLog("新規にネットワークドライブの登録が完了")
            Return True
        End If

        ' エラーになった場合、登録情報を一旦削除して再登録
        ExeCMDPrompt(String.Format("net use {0}", WebdavAddr))
        ExeCMDPrompt(String.Format("net use {0} /delete", WebdavAddr))
        ret = ExeCMDPrompt(String.Format("net use {0} {1} /user:{2}@ilcc.rakusaba.jp /persistent:yes", ServerAddr, pw, id))
        If ret.IndexOf("エラー") < 0 Then
            cLog("新規にネットワークドライブの登録が完了")
            Return True
        End If

        MsgBox("ネットワーク接続が正常に行えませんでした。")
        Return False
    End Function


    Private Function ExeCMDPrompt(cmd As String) As String
        Dim results As String
        'Processオブジェクトを作成
        Static Dim p As Process

        If p Is Nothing Then
            p = New Process
            'ComSpec(cmd.exe)のパスを取得して、FileNameプロパティに指定
            p.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec")
            '出力を読み取れるようにする
            p.StartInfo.UseShellExecute = False
            p.StartInfo.RedirectStandardOutput = True
            p.StartInfo.RedirectStandardInput = False
            'ウィンドウを表示しないようにする
            p.StartInfo.CreateNoWindow = True
        End If
        p.StartInfo.Arguments = "/c" & cmd
        p.Start()
        results = p.StandardOutput.ReadToEnd()
        Console.WriteLine(results)
        p.WaitForExit()
        p.Close()
        Return results
    End Function

    ' デバッグログ(Console)
    Private Sub cLog(msg As String)
        Dim tim As String = Date.Now.ToString("yyyy/MM/dd/HH:mm:ss.fff")
        Console.WriteLine(tim & " # " & msg)
    End Sub

End Class
