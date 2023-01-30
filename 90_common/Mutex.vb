Imports System.IO
Imports System.Text

Public Class Mutex
    Private ReadOnly log As New Log
    Private ReadOnly CurrentPath As String
    Const retryCount = 10        ' ロック時のリトライ回数
    Const retryDelay = 500       ' ロック時のリトライ間隔
    Public Const MTX_LOCK_W = "W"
    Public Const MTX_LOCK_R = "R"


    Sub New()
        Dim xml As New XmlMng
        CurrentPath = xml.GetCPath() & Common.DIR_MTX
    End Sub

    ' 排他 抑止
    Public Function Lock(rwType As String, id As String)
        For n = 1 To retryCount
            If CanIAttach(rwType, id) Then
                ' アクセス可
                log.D(Log.MTX, String.Format("  [{0}:{1}] --->", id, rwType))
                Attach(rwType, id)
                Exit For
            Else
                ' アクセス不可
                log.D(Log.MTX, String.Format("  [{0}:{1}] wait {2}", id, rwType, n))
                Threading.Thread.Sleep(retryDelay)
                If n = retryCount Then
                    log.D(Log.MTX, String.Format("  [{0}] ---> RetryOut ", id))  ' リトライアウト(デッドロック回避のために無理やりアタッチする)
                    Attach(rwType, id)
                End If
            End If
        Next
        Return True
    End Function

    ' 排他 解除
    Public Function UnLock(id As String)
        Try
            log.D(Log.MTX, String.Format("[{0}]   <---", id))
            Detach(id)
        Catch ex As Exception
            log.D(Log.MTX, String.Format("[{0}]   NG: ", id, ex.Message))
        End Try
        Return True
    End Function

    Private Sub Attach(rwType As String, id As String)
        Dim file As String = CurrentPath & id & "_" & rwType & "_" & My.Computer.Name
        Using sw As StreamWriter = New StreamWriter(File, False, Encoding.GetEncoding("Shift_JIS"))
            sw.Write("")
        End Using
    End Sub

    Private Sub Detach(id As String)
        ' RW関係なく、同じID+ユーザ名のファイルを削除  id_*_UserName
        Dim fileList As String() = Directory.GetFileSystemEntries(CurrentPath, id & "_*_" & My.Computer.Name)
        For Each f As String In fileList
            File.Delete(f)
        Next
    End Sub

    ' アクセス可能か確認
    Private Function CanIAttach(rwType As String, id As String)
        Dim ret = True
        ' ファイル検索 [識別番号]_*      1_R_MASHIRO などが検出される
        Dim fileList As String() = Directory.GetFileSystemEntries(CurrentPath, id & "_*")
        For Each f As String In fileList
            If Path.GetFileName(f).Chars(0) <> id Then Continue For     ' 同じIDのファイルを全て抽出
            If (rwType & Path.GetFileName(f).Chars(2)).IndexOf(MTX_LOCK_W) >= 0 Then
                ret = False
                log.cLog("CanIAttach[" & id & "] NG:" & Path.GetFileName(f))
                Exit For
            End If
        Next
        'log.DBGLOG("CanIAttach[" & id & "] ret:" & ret)
        Return ret
    End Function

End Class
