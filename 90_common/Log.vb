Imports System.IO
Imports System.Text

Public Class Log
    ' タグ
    Private Const TagD As String = "D"
    Private Const TagE As String = "E"
    ' ログ・ファイル名
    Private Const EXTENSION_LOG As String = ".log"      ' 拡張子
    Public Const LOGNAME_DB As String = "A_SC_DB"
    Private Const LOGNAME_ERR As String = "A_SC_Error"
    Private Const LOGNAME_DBG As String = "A_SC_DBG"
    Public Const LOGNAME_TASK As String = "A_SC_Task"
    Public Const LOGNAME_MTX As String = "A_SC_Mutex"
    Public Const LOGNAME_TES As String = "TestLog"

    ' ログ種別テーブル (ログファイル名, ログ識別子[D,E])
    Private ReadOnly LogTable(,) = {
        {LOGNAME_DB, TagD},
        {LOGNAME_DBG, TagD},
        {LOGNAME_ERR, TagE},
        {LOGNAME_TASK, TagD},
        {LOGNAME_MTX, TagD}
    }
    ' ログ種別テーブル 識別子
    Public Const DB As Integer = 0
    Public Const DBG As Integer = 1
    Public Const ERR As Integer = 2
    Public Const TASK As Integer = 3
    Public Const MTX As Integer = 4

    Private ReadOnly CurrentPath As String
    Private ReadOnly xml As New XmlMng

    Sub New()
        CurrentPath = xml.GetCPath()
    End Sub

    ' ログ 出力:DB用
    Public Sub D(id As Integer, msg As String)
        PLog(LogTable(id, 0), msg, LogTable(id, 1))
    End Sub
    Public Sub D(id As Integer, msgList As List(Of String))
        Dim msg As String = ""
        For Each m In msgList
            msg += m & vbCrLf
        Next
        msg = msg.TrimEnd(Chr(10)).TrimEnd(Chr(13))     ' 最後の改行(chr(10) CR chr(13) LF)を削除
        PLog(LogTable(id, 0), msg, LogTable(id, 1))
    End Sub
    Private Sub PLog(file As String, msg As String, Tag As String)
        Try
            If Not Directory.Exists(CurrentPath & Common.DIR_LOG) Then Directory.CreateDirectory(CurrentPath & Common.DIR_LOG)        ' ディレクトリ作成
        Catch ex As Exception
            MsgBox("パスが見つかりません。" & vbCrLf & CurrentPath & Common.DIR_LOG)
            Exit Sub
        End Try

        Dim filePath As String = CurrentPath & Common.DIR_LOG & file & "_" & My.Computer.Name & EXTENSION_LOG       ' ファイルパス  \\xxx\Log\[File_Name]_[User_Name].log
        Try
            Using sw As StreamWriter = New StreamWriter(filePath, True, Encoding.GetEncoding("shift_jis"))
                Dim sf As New StackFrame(2)
                sw.WriteLine(DateTime.Now.ToString("[" & Tag & "] yyyy/MM/dd HH:mm:ss:fff # ") & My.Computer.Name.PadRight(15) & " ## " & SC.SCVer & " [" & sf.GetMethod.Name & "] " & msg)
                If Tag = TagE Then sw.WriteLine("# TRACE #" & vbCrLf & Environment.StackTrace)                                  ' ERR時はバックトレース出力
            End Using
        Catch ex As Exception
        End Try
    End Sub

    ' デバッグログ(Console)
    Public Sub cLog(msg As String)
        Dim tim As String = Date.Now.ToString("yyyy/MM/dd/HH:mm:ss.fff")
        If SC.DEBUG_MODE Then
            Using sr As StreamWriter = New StreamWriter(SC.CurrentAppPath & "DebugLog.log", True, Encoding.Default)
                Try
                    sr.WriteLine($"{tim} # {msg}")
                Catch ex As Exception
                End Try
            End Using
        Else
            Console.WriteLine(tim & " # " & msg)
        End If

    End Sub

    Public Sub TestLog(msg As String)
        PLog(LOGNAME_TES, msg, 9)
    End Sub

    ' 時間測定
    Private STtime As Date
    Public Sub TimerST()
        STtime = Date.Now
    End Sub
    Public Sub TimerED(str As String)
        Dim EDtime As Date = Date.Now
        cLog(str & " # Timer # " & (EDtime - STtime).ToString("ss\.fff"))
    End Sub

End Class
