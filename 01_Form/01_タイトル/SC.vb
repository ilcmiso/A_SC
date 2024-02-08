Imports System.IO
Imports System.Text

Public Class SC

#Region " Open Close "
    Public Const SCVer As String = "2401B"                         ' A_SC バージョン
    ' 起動アプリパス
    Public ReadOnly CurrentAppPath As String = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location) & "\"

    Private ReadOnly HISTORY As String = CurrentAppPath & "History.txt"
    Private log As Log
    Private vup As Verup

    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim loadTime = Now
        log = New Log
        vup = New Verup

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

        ' アプリケーション自動バージョンアップ ＆ 再起動
        If vup.Update(SCVer) Then RestartApl()

        ' プログレスバーのインスタンス生成
        SCA_ProgressBar.Instance = New SCA_ProgressBar()

        log.cLog("--- SC Load完了: " & (Date.Now - loadTime).ToString("ss\.fff"))
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
                CheckUserName
                fm = SCA1
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

    Private Sub CheckUserName()
        Dim db As New Sqldb
        db.UpdateOrigDT(Sqldb.TID.USER)
        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.USER).Select($"C01 = '{My.Computer.Name}'")
        If dr.Length = 0 Then
            MsgBox($"新たにユーザー名を設定するようになりました。{vbCrLf}　ご自身の名前を入力してください。")
            Dim fm As Form = SCA_SetUserName
            fm.ShowInTaskbar = False
            fm.ShowDialog()
            fm.Dispose()
        End If
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
        If e.KeyCode = Keys.F1 Then
            Dim cmn As New Common
            cmn.OpenCurrentDir()
        ElseIf e.KeyCode = Keys.F2 Then
            Dim fff As New SCE_S1
            fff.Show()
            Exit Sub
        End If
    End Sub


#End Region

End Class
