Imports System.IO
Imports System.Threading

Public Class Pooling
    Private LastTime As Date            ' 最終更新時刻
    Private UpdTime As Date             ' 更新時刻
    Private pooling As Boolean          ' 監視中(True)
    Private pId As String               ' 監視機能の識別子
    Private ReadOnly FilePath As String ' 監視対象ファイル
    Private ReadOnly Cycle As Integer   ' 監視周期(ms)

    Delegate Sub DelegateFunc()    ' コールバック用デリゲート関数宣言

    Sub New(id As String, Path As String, Cycle_ms As Integer)
        FilePath = Path
        LastTime = File.GetLastWriteTime(FilePath)   ' タイムスタンプ取得して最終更新時刻を保持
        Cycle = Cycle_ms
        pId = id
    End Sub

    ' ポーリング開始
    Friend Sub Pstart()
        ' 非同期監視
        'Await Task.Run(Sub()
        '                   pooling = True
        '                   While pooling
        '                       UpdTime = File.GetLastWriteTime(FilePath)
        '                       If UpdTime <> LastTime Then
        '                           ' ファイル更新を検出
        '                           Detection()
        '                       End If
        '                       Threading.Thread.Sleep(Cycle)
        '                   End While
        '               End Sub)
        ' DBファイルの更新監視
        Dim task2 As Task = Task.Run(
        Sub()
            pooling = True
            While pooling
                UpdTime = File.GetLastWriteTime(FilePath)
                If UpdTime <> LastTime Then
                    ' ファイル更新を検出
                    Dim scheduler = TaskScheduler.FromCurrentSynchronizationContext()
                    Task.Factory.StartNew(
                      Sub()
                          ' 同期コンテキスト上で実行したい処理
                      End Sub, CancellationToken.None, TaskCreationOptions.None, scheduler)
                End If
                Thread.Sleep(Cycle)
            End While
        End Sub)
    End Sub

    ' ポーリング終了
    Friend Sub Pstop()
        pooling = False
    End Sub

    ' 自分自身が更新した時等、ファイル検出が不要なときに
    Friend Sub UpdateTime()
        LastTime = File.GetLastWriteTime(FilePath)
    End Sub

    ' 検出時の処理
    Private Sub Detection()
        UpdateTime()                ' タイムスタンプ更新
        ' SCA1.PoolingCallBack(pId)   ' コールバック
    End Sub
End Class
