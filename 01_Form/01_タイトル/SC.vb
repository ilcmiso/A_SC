Imports System.IO
Imports System.Text

Public Class SC

#Region " Open Close "
    Public Const SCVer As String = "25050"                         ' A_SC バージョン
    ' 起動アプリパス
    Public ReadOnly CurrentAppPath As String = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location) & "\"
    Private ReadOnly HISTORY As String = CurrentAppPath & "History.txt"
    Private log As Log
    Public DEBUG_MODE = False

    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim loadTime = Now
        log = New Log

        Me.Text = String.Format(" {0}  Ver {1}", Common.APPNAME, SCVer)     ' タイトルのバージョン設定

        ' 変更履歴表示
        Dim Path_hist As String = HISTORY
        If File.Exists(Path_hist) Then
            Using sr As StreamReader = New StreamReader(Path_hist, Encoding.GetEncoding("Shift_JIS"))
                ' データ部分を読み込み
                While (sr.Peek() > -1)
                    TB_History.Text += sr.ReadLine() & vbCrLf
                End While
            End Using
        End If

        ' アプリ最新版があればアプリ更新ボタンを表示
        BT_APPUPDATE.Visible = AppUpdate.IsUpdateAvailable

        ' プログレスバーのインスタンス生成
        SCA_ProgressBar.Instance = New SCA_ProgressBar()

        log.cLog("--- SC Load完了: " & (Date.Now - loadTime).ToString("ss\.fff"))
    End Sub

    ' SCA1が閉じた後にアプリ更新ボタンの状態を更新
    Private Sub SCA1_Closed(sender As Object, e As FormClosedEventArgs) 
        BT_APPUPDATE.Visible = AppUpdate.IsUpdateAvailable
    End Sub

    Private Sub ME_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Button3.Enabled = False Then
            Dim r = MessageBox.Show("電話接続中です。" & vbCrLf &
                                    "アプリケーションを終了すると電話接続も終了しますがよろしいですか？",
                                    "ご確認ください",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question)
            If r = vbNo Then
                e.Cancel = True
                Exit Sub
            End If
            SCC1.TaskDispose()
        End If
        Me.Dispose()
    End Sub

    ' ボタンクリックで各フォーム起動
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click
        Dim fm As New Form
        Select Case sender.name
            Case Button1.Name   ' 顧客検索メイン画面
                fm = SCA1
                AddHandler fm.FormClosed, AddressOf SCA1_Closed ' フォーム閉じたときの処理を追加
            Case Button2.Name   ' F35読み込み画面
                fm = SCB1
                fm.ShowInTaskbar = False
            Case Button4.Name   ' 交渉記録画面
                fm = SCD1
                fm.ShowInTaskbar = False
            Case Button3.Name   ' 設定画面
                Button3.Enabled = False
                Dim fmsub As Form = New SCC1
                fmsub.ShowInTaskbar = False
                AddHandler fmsub.FormClosed, AddressOf SCC1_closed           ' 電話接続画面のフォーム閉じたときのイベント関数登録
                fmsub.Show()
                Exit Sub
            Case Else
                fm.Dispose()
                Exit Sub
        End Select

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        fm.ShowDialog()
        fm.Dispose()
        If Not Me.IsDisposed Then Me.Visible = True     ' 非表示になっていたTOP画面を再表示 ただし、アプリ更新時は既に破棄されているから表示できない
    End Sub
    ' 電話接続画面フォーム終了イベント受信
    Private Sub SCC1_closed(sender As Object, e As FormClosedEventArgs)
        Button3.Enabled = True
    End Sub

    Public Sub RestartApl()
        Close()
        Application.Restart()
    End Sub

    Private IlcND As New ILCNetDrive            ' ILCネットワークドライブ宣言

    ' ショートカット F1
    Private Sub Button1_KeyDown(sender As Object, e As KeyEventArgs) Handles Button1.KeyDown
        Static debugcnt As Integer
        Select Case e.KeyCode
            Case Keys.F1
                Dim cmn As New Common
                cmn.OpenCurrentDir()
            Case Keys.F9
                debugcnt += 1
                If debugcnt > 5 Then
                    DEBUG_MODE = Not DEBUG_MODE
                    MsgBox($"デバッグモード:{DEBUG_MODE}")
                    If DEBUG_MODE Then Process.Start(CurrentAppPath & "DebugLog.log")
                End If
        End Select
    End Sub

    ' アプリ更新ボタン
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles BT_APPUPDATE.Click
        If AppUpdate.PerformUpdate() Then
            AppUpdate.AppRestart()
        End If
    End Sub

#End Region

End Class
