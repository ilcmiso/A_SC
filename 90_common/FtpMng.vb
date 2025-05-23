Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions

Public Class FtpMng

    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private ReadOnly FtpLocal As String = cmn.CurrentPath & "FTP"
    Private ReadOnly FtpUrl As String = FtpMngConfig.FtpUrl
    Private ReadOnly UserID As String = FtpMngConfig.FtpUser
    Private ReadOnly UserPW As String = FtpMngConfig.FtpPass

    Sub New()
        cmn.CreateDir(FtpLocal)
    End Sub

    ''' <summary>
    ''' FTP上の5桁数字のzipファイル一覧から最大のファイル名を返す
    ''' </summary>
    Public Function GetLatestFileName() As String
        Dim request = CType(WebRequest.Create(FtpUrl), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.ListDirectory
        request.Credentials = New NetworkCredential(UserID, UserPW)

        Dim fileList As New List(Of String)
        Using response = CType(request.GetResponse(), FtpWebResponse)
            Using reader As New StreamReader(response.GetResponseStream())
                While Not reader.EndOfStream
                    Dim file = reader.ReadLine()
                    ' 5桁の数字 + .zip に一致するものだけ抽出
                    If Regex.IsMatch(file, "^\d{5}\.zip$") Then
                        fileList.Add(file)
                    End If
                End While
            End Using
        End Using

        If fileList.Count = 0 Then Return ""

        ' 数字部分で比較して最大を返す
        Dim fileName As String = fileList.OrderByDescending(Function(f) Integer.Parse(f.Substring(0, 5))).First()
        Return fileName
    End Function

    ' FTP最新バージョンが、現在のバージョンより新しい場合にダウンロード実行
    Public Function GetNewFile() As String
        Dim latestFile As String = GetLatestFileName()
        Dim latestNum As String = Path.GetFileNameWithoutExtension(latestFile)
        log.cLog($"FTP - [Latest]{latestNum} : [APL]{SC.SCVer}")
        If latestFile = "" Then Return ""

        If CInt(latestNum) > CInt(SC.SCVer) Then
            Return GetFile(latestFile)
        End If
        Return ""
    End Function

    ''' <summary>
    ''' FTPサーバーからファイルをダウンロード（存在チェック付き）
    ''' </summary>
    Public Function GetFile(fileName As String) As String
        log.cLog($"FTP - GetFile:{fileName}")
        If fileName = "" Then Return ""
        Dim localFullPath As String = Path.Combine(FtpLocal, fileName)

        ' ローカルに既にファイルがあればDLしない
        If File.Exists(localFullPath) Then Return ""

        ' FTP上にファイルがあるか確認
        Dim existsRequest = CType(WebRequest.Create(FtpUrl & fileName), FtpWebRequest)
        existsRequest.Method = WebRequestMethods.Ftp.GetFileSize
        existsRequest.Credentials = New NetworkCredential(UserID, UserPW)

        Try
            Using existsResponse = CType(existsRequest.GetResponse(), FtpWebResponse)
                ' 存在する → DL実行
                log.cLog($"FTP - Download Start")
            End Using
        Catch ex As WebException
            ' ファイルが存在しなければ終了
            Dim resp = CType(ex.Response, FtpWebResponse)
            If resp.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then Return ""
            Throw
        End Try

        ' ダウンロード処理
        Dim request = CType(WebRequest.Create(FtpUrl & fileName), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.DownloadFile
        request.Credentials = New NetworkCredential(UserID, UserPW)

        Using response = CType(request.GetResponse(), FtpWebResponse)
            Using responseStream = response.GetResponseStream()
                Using outputStream As New FileStream(localFullPath, FileMode.Create)
                    responseStream.CopyTo(outputStream)
                End Using
            End Using
        End Using
        log.cLog($"FTP - Download Complite")
        Return Path.GetFileNameWithoutExtension(fileName)
    End Function

    ''' <summary>
    ''' ローカルのファイルをFTPサーバーへアップロード
    ''' </summary>
    Public Sub UploadFile(ftpUrl As String, username As String, password As String)
        Dim request = CType(WebRequest.Create(ftpUrl), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.UploadFile
        request.Credentials = New NetworkCredential(username, password)

        Dim fileContents() As Byte = File.ReadAllBytes(FtpLocal)
        request.ContentLength = fileContents.Length

        Using requestStream = request.GetRequestStream()
            requestStream.Write(fileContents, 0, fileContents.Length)
        End Using
    End Sub

    Public Sub OpenFTPDir()
        Process.Start(FtpLocal)
    End Sub
End Class