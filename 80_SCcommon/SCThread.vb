Imports System.IO
Imports System.Threading

Public Class SCThread

    Private ReadOnly ownForm As SCA1
    Private ReadOnly cmn As Common
    Private ReadOnly log As Log
    Private ReadOnly db As Sqldb
    Private ReadOnly exc As CExcel
    Private Const CYCLE As Integer = 500

    Private Thread_NoticeEvent As Thread = Nothing
    Private ThreadActive As Boolean = True

    Private ReadOnly ReqSQL_TID As New List(Of Integer)
    Private ReadOnly ReqSQL_CMD As New List(Of String)

    Delegate Sub delegate_ThreadCallBack(result As Integer)         ' UIコールバック用Delegate宣言

    Public Sub New(form As SCA1)
        ownForm = form
        cmn = New Common
        log = New Log
        db = form.db                    ' ownForm(SCA1)のdbを使用
        exc = New CExcel
    End Sub

    ' スレッド生成・開始
    Public Sub Init()
        ' イベント通知スレッド生成
        Thread_NoticeEvent = New Thread(New ParameterizedThreadStart(AddressOf Thread_Main))
        Thread_NoticeEvent.Start()
    End Sub

    ' スレッド破棄
    Public Sub Dispose()
        ThreadActive = False
    End Sub

    ' ---------------------------------------------------------------------------------------------
    ' イベント通知メインスレッド
    Private Sub Thread_Main()
        Dim result As Integer
        log.cLog("$ Thread_Main Start")
        While (ThreadActive)
            Try
                If EvUpdateDB_SCD() Then ownForm.Invoke(New delegate_ThreadCallBack(AddressOf ownForm.NoticeUpdateDB_SCD), result)
                If EvUpdateDB_PI() Then ownForm.Invoke(New delegate_ThreadCallBack(AddressOf ownForm.NoticeUpdateDB_PI), result)
                If EvUpdateApp() Then ownForm.Invoke(New delegate_ThreadCallBack(AddressOf ownForm.NoticeUpdateApp), result)
                'If EvUpdateGP() Then ownForm.Invoke(New delegate_ThreadCallBack(AddressOf ownForm.NoticeUpdateGP), result)
                ExeSQLThread()

                'If ReqSQL.Count > 0 Then ExeSqlThread()
            Catch ex As Exception
            End Try
            Thread.Sleep(CYCLE)
        End While
        log.cLog("$ Thread_Main End")
    End Sub

    ' ---------------------------------------------------------------------------------------------
    ' 各種 監視イベンド

    ' アプリ更新イベント
    Private Function EvUpdateApp() As Boolean
        If Not File.Exists(cmn.CurrentPath & Common.DIR_UPD & Common.EXE_NAME) Then Return False
        Dim timel As Date = File.GetLastWriteTime(SC.CurrentAppPath & Common.EXE_NAME)                  ' ローカルファイルの更新時刻
        Dim times As Date = File.GetLastWriteTime(cmn.CurrentPath & Common.DIR_UPD & Common.EXE_NAME)   ' サーバーファイルの更新時刻

        If timel <> times Then Return True      ' アップデート検知
        Return False
    End Function

    ' DB更新イベント SCD
    Private Function EvUpdateDB_SCD() As Boolean
        Dim dbFile As String = Sqldb.DB_FKSCLOG

        If Not File.Exists(cmn.CurrentPath & Common.DIR_DB3 & dbFile) Then Return False
        Dim timel As Date = File.GetLastWriteTime(SC.CurrentAppPath & dbFile)     ' ローカルファイルの更新時刻
        Dim times As Date = File.GetLastWriteTime(cmn.CurrentPath & Common.DIR_DB3 & dbFile)       ' サーバーファイルの更新時刻

        If timel <> times Then
            log.cLog("$ DB更新: " & dbFile)
            db.UpdateOrigDT(Sqldb.TID.SCD)
            db.UpdateOrigDT(Sqldb.TID.SCR)
            Return True      ' アップデート検知
        End If
        Return False
    End Function

    ' DB更新イベント PI
    Private Function EvUpdateDB_PI() As Boolean
        Dim dbFile As String = Sqldb.DB_FKSCPI

        If Not File.Exists(cmn.CurrentPath & Common.DIR_DB3 & dbFile) Then Return False
        Dim timel As Date = File.GetLastWriteTime(SC.CurrentAppPath & dbFile)     ' ローカルファイルの更新時刻
        Dim times As Date = File.GetLastWriteTime(cmn.CurrentPath & Common.DIR_DB3 & dbFile)       ' サーバーファイルの更新時刻

        If timel <> times Then
            log.cLog("$ DB更新: " & dbFile)
            db.UpdateOrigDT(Sqldb.TID.PI)
            Return True      ' アップデート検知
        End If
        Return False
    End Function

    ' 還元データ(GP)更新イベント
    Private Function EvUpdateGP() As Boolean
        'Dim filePath As String = Common.DIR_DB3 & Sqldb.DB_CONFIG_GP
        'If Not File.Exists(cmn.CurrentPath & filePath) Then Return False
        'Dim timel As Date = File.GetLastWriteTime(PF.CurrentAppPath & filePath)   ' ローカルファイルの更新時刻
        'Dim times As Date = File.GetLastWriteTime(cmn.CurrentPath & filePath)   ' サーバーファイルの更新時刻

        'If timel <> times Then
        '    log.CLog("$ GP更新")
        '    Return True      ' アップデート検知
        'End If
        Return False
    End Function

    ' SQL実行イベント
    Private Function ExeSQLThread() As Boolean
        While (ReqSQL_TID.Count > 0)
            ' リクエスト内容を実行
            db.ExeSQL(ReqSQL_TID(0), ReqSQL_CMD(0))

            ' 実行したリクエストを削除
            ReqSQL_TID.RemoveAt(0)
            ReqSQL_CMD.RemoveAt(0)
        End While
        Return True
    End Function

    ' ---------------------------------------------------------------------------------------------
    ' 提供IF

    ' Thread SQLコマンドリクエスト
    Public Sub ReqSQLThread(TableID As Integer, cmd As String)
        ReqSQL_TID.Add(TableID)
        ReqSQL_CMD.Add(cmd)
    End Sub

    ' ---------------------------------------------------------------------------------------------
    ' 関連IF


End Class
