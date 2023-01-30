Imports System.IO.Ports

Public Class SCC1

#Region "定義"

    Private ReadOnly xml As New XmlMng
    Private Const ALOHA_ST = Chr(2)                                     ' ALOHA通信の開始文字
    Private Const ALOHA_ED = Chr(3)                                     ' ALOHA通信の終端文字
    Private Const ALOHA_PING_SEND = ALOHA_ST & "CON" & ALOHA_ED         ' ALOHAへのping送信文字
    Private Const ALOHA_TIMEOUT = 1000                                  ' ALOHAからの応答タイムアウト(ping用)

    Private Const DEBUGF = "C:\A_SC\DebugALOHA.log"                     ' デバッグログ・ファイル
    Private title = ""                                                  ' 画面のタイトルを保存したいだけ
    Private ping_ok = False                                             ' ALOHAからのping受信したらTrue
    Private PATH_TEL As String
#End Region

#Region "イベント"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' フォーム位置設定
        'Me.Top = SC.Top + SC.Height

        ' リストボックスに有効なCOM表示
        Dim ports() As String = SerialPort.GetPortNames()   ' すべてのシリアル・ポート名を取得する
        For Each port In ports
            ListBox1.Items.Add(port)
        Next
        If ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = 0
        End If
        IconInit()
        title = Me.Text

        ' データ格納パスを読み込み
        RadioButton2.Checked = (xml.xmlData.CPathSW = "2")
        TextBox1.Text = xml.xmlData.CPath1
        TextBox2.Text = xml.xmlData.CPath2
        PATH_TEL = xml.GetCPath() & Common.DIR_TEL & Common.FILE_TEL
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        XmlSave()                               ' 固有情報(Xml)にデータ格納パスを保存する
        'xml.GetXml()                           ' A_SC内にあるconfigファイル読み込み
        TaskDispose()
        SP.Close()
        SC.Activate()
    End Sub

    ' 接続開始ボタン
    Private Sub BtnClear_Click(ByVal sender As Object, ByVal e As EventArgs) Handles BT_A1.Click
        If ListBox1.Items.Count = 0 Then
            MsgBox(PATH_TEL)
            MsgBox("USBが繋がっていないようです。")
            Exit Sub
        End If
        BT_A1.Enabled = False
        ConnectALOHA(ListBox1.SelectedItem)
    End Sub

    ' 最小化ボタン
    Private Sub Form1_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Normal
            Tasktray_down()
        End If
    End Sub

    ' 更新ボタン リストボックスの更新
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ListBox1.Items.Clear()
        ' リストボックスに有効なCOM表示
        Dim ports() As String = SerialPort.GetPortNames()   ' すべてのシリアル・ポート名を取得する
        For Each port In ports
            ListBox1.Items.Add(port)
        Next
        If ListBox1.Items.Count > 0 Then
            ListBox1.SelectedIndex = 0
        End If
    End Sub

    ' 着信ログ場所変更ボタン
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim dir = GetDirDialog()
        If dir <> "" Then TextBox1.Text = dir
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim dir = GetDirDialog()
        If dir <> "" Then TextBox2.Text = dir
    End Sub
#End Region

#Region "xml"
    Private Sub XmlSave()
        ' 固有情報の保存処理
        If Not TextBox1.Text.EndsWith("\") Then TextBox1.Text += "\"    ' ディレクトリの末尾が\じゃなければ\つける
        If Not TextBox2.Text.EndsWith("\") Then TextBox2.Text += "\"    ' ディレクトリの末尾が\じゃなければ\つける
        If Not IO.Directory.Exists(TextBox1.Text) Then
            MsgBox("着信記録ファイルの保存場所" & vbCrLf &
                   TextBox1.Text & " が見つかりません。" & vbCrLf &
                   "正しい場所を設定しないと着信を検出できません。", MessageBoxIcon.Warning)
        End If

        xml.xmlData.CPath1 = TextBox1.Text
        xml.xmlData.CPath2 = TextBox2.Text
        If RadioButton1.Checked Then xml.xmlData.CPathSW = 1
        If RadioButton2.Checked Then xml.xmlData.CPathSW = 2
        xml.SetXml()        ' 固有情報(Xml)に保存
    End Sub

    'Private Sub XmlLoad()
    '    ' 固有情報(Xml)の読み出し
    '    xml.GetXml()
    '    TextBox1.Text = xml.xmlData.TelLogPath
    'End Sub
#End Region

#Region "タスクトレイ関連"
    ' 初期設定 タスクトレイアイコンクリックイベントの生成
    Private Sub IconInit()
        AddHandler nIcon.Click,
            New EventHandler(AddressOf NotifyIcon_Click)
    End Sub

    ' タスクトレイクリックイベント
    Private Sub NotifyIcon_Click(sender As Object, e As EventArgs)
        Tasktray_up()
    End Sub

    ' タスクトレイから復帰
    Private Sub Tasktray_up()
        ShowInTaskbar = True
        nIcon.Visible = False
        Visible = True
    End Sub

    ' タスクトレイ格納(最小化)
    Private Sub Tasktray_down()
        ShowInTaskbar = False
        nIcon.Visible = True
        Visible = False
        nIcon.ShowBalloonTip(500)
    End Sub

    Public Sub TaskDispose()
        nIcon.Visible = False
        nIcon.Icon = Nothing
        nIcon.Dispose()
    End Sub

#End Region

#Region "接続関連"
    ' 接続開始
    Private Sub ConnectALOHA(port As String)
        If SP.IsOpen Then SP.Close()
        ping_ok = False

        Try
            'シリアルポートの設定
            SP.PortName = port                  ' ポート名
            SP.BaudRate = 9600                  ' 通信速度指定
            SP.Parity = Parity.Even             ' パリティ指定(Even=偶数)
            SP.DataBits = 7                     ' ビット数指定
            SP.StopBits = StopBits.One          ' ストップビット指定
            SP.ReadTimeout = ALOHA_TIMEOUT      ' タイムアウト値

            SP.Open()                  ' シリアルポートのオープン  
            SP.DiscardInBuffer()       ' 余分なバッファ削除
            Me.Text = title & " - 接続中.."

            ' 非同期でALOHAへping送信する。100ms間隔で10回
            Dim task1 As Task = Task.Run(
                Sub()
                    For i = 1 To 10
                        If ping_ok Then Exit Sub     ' 応答があったら終了する
                        SendCOM(ALOHA_PING_SEND)
                        Threading.Thread.Sleep(100)
                    Next
                    ' 接続タイムアウト UI操作するためInvoke(ConnectFailureをコールする)
                    BeginInvoke(New MethodInvoker(AddressOf ConnectFailure))
                End Sub
            )
        Catch ex As Exception
            ConnectFailure()
        End Try
    End Sub

    ' 接続失敗処理
    Private Sub ConnectFailure()
        ping_ok = False
        BT_A1.Enabled = True
        Me.Text = title
        MsgBox("接続に失敗しました。")
    End Sub

    ' 接続成功処理
    Private Sub ConnectSuccess()
        ping_ok = True
        Me.Text = title & " - 電話受信待ち"
        nIcon.Text = title & " - 接続中"

        ' 時刻設定コマンド送信
        SendCOM(ALOHA_ST & "C" & DateTime.Now.ToString("yyyyMMddHHmm") & ALOHA_ED)
        MsgBox("接続成功しました。")
        Tasktray_down()
    End Sub
#End Region

#Region "ALOHAコマンド"
    Private Sub SubmitALOHA(cmd As String)
        ' 正常コマンドフォーマット
        '   月(2) 日(2) 曜日(1) 時間(2) 分(2)  + [ 電話番号(20) or フック情報(2) ]
        '   ※電話番号の空きはスペース(x20)
        '   ※曜日は日曜日(0)～土曜日(6)
        '   ※全てASCIIではなく、SJISで受信
        '   例) 02071175008019186436         

        If cmd.Length < 18 Then ' 電話番号が短いものは解析できない。他のALOHAコマンドもスルーする
            Exit Sub
        End If
        Try
            ' MM/DD HH:mm-080XXXXXXXX 形式で着信ログファイルに保存
            Dim recvTel = DateTime.Now.ToString("yyyy") & "/" & cmd.Substring(0, 2) & "/" & cmd.Substring(2, 2) & " " & cmd.Substring(5, 2) & ":" & cmd.Substring(7, 2) & "-" & cmd.Substring(9, 20).Replace(" ", "")
            Fwrite(recvTel)
        Catch ex As Exception
            MsgBox("認識できないコマンドを受信。ログの書き込みに失敗。")
        End Try
    End Sub

#End Region

#Region "データ受信関連"

    ' データ受信したらコールされる(非同期)
    Private Sub SerialPort1_DataReceived(ByVal sender As System.Object, ByVal e As SerialDataReceivedEventArgs) Handles SP.DataReceived
        Try
            Dim dat As Byte() = New Byte(SP.BytesToRead - 1) {}
            SP.Read(dat, 0, dat.GetLength(0))

            Dim dlg As New DisplayTextDelegate(AddressOf DisplayText)   ' デリゲート(非同期スレッド)生成
            Dim str As String = System.Text.Encoding.GetEncoding("SHIFT-JIS").GetString(dat) ' 受信バイト配列を文字列変換
            Me.Invoke(dlg, New Object() {str}) 'デリゲート関数をコールする  

        Catch ex As InvalidOperationException
            MsgBox(ex.Message)
        End Try
    End Sub

    'Invokeメソッドで使用するデリゲート宣言  
    Delegate Sub DisplayTextDelegate(ByVal str As String)
    Private Sub DisplayText(ByVal str As String)
        Static recvDat = ""       ' データ格納領域
        Static cmd_f = False      ' コマンド中(開始文字を受信済み)ならTrue
        Dbgwrite("R:" & str)

        ' ping以外のデータ受信
        recvDat += str
        ' 受信情報を解析して、『開始(x02)』～『終端(x03)』までを取得。それ以外の範囲外は捨てる
        ' コマンドが分割して送信されることを想定して、『終端(x03)』が来るまで recvDatにコマンドをためておく。
        For Each c As Char In recvDat
            Select Case c
                Case ALOHA_ST
                    If cmd_f Then
                        ' 開始文字の後にまた開始文字があった、先発の開始文字(x02)を無視する
                        recvDat = recvDat.remove(0, recvDat.indexof(ALOHA_ST) + 1)
                    Else
                        ' 開始文字を検出、コマンド読み取り開始
                        cmd_f = True
                        recvDat = recvDat.remove(0, 1)  ' 開始文字自体は削除
                    End If
                Case ALOHA_ED
                    If cmd_f Then
                        ' コマンド正常パターン検出
                        Dim rcmd = recvDat.Substring(0, recvDat.IndexOf(ALOHA_ED))
                        recvDat = recvDat.remove(0, recvDat.IndexOf(ALOHA_ED) + 1)  ' 正常に処理したコマンドは削除
                        cmd_f = False

                        ' ALOHAコマンドがping応答
                        If rcmd = "ALOHA" Then
                            ConnectSuccess()
                            Exit Sub
                        End If

                        ' ALOHAコマンド実行
                        SubmitALOHA(rcmd)
                    Else
                        recvDat = recvDat.remove(0, 1)  ' 終端文字自体は削除
                    End If
                Case Else
                    ' 範囲外なので捨てる
                    If Not cmd_f Then recvDat = recvDat.remove(0, 1)
            End Select
        Next

    End Sub

    ' シリアルポートにデータ送信
    Private Sub SendCOM(msg As String)
        Try
            Dim dat As Byte() = System.Text.Encoding.GetEncoding("SHIFT-JIS").GetBytes(msg)
            Dbgwrite("S:" & msg)
            SP.Write(dat, 0, dat.GetLength(0))
        Catch ex As Exception
            MsgBox("送信失敗")
        End Try
    End Sub

#End Region

#Region "便利関数"
    ' ファイル書き込み
    Private Sub Fwrite(str As String)
        Dim sw As New IO.StreamWriter(PATH_TEL, True, System.Text.Encoding.GetEncoding("shift_jis"))
        sw.WriteLine(str)
        sw.Close()
    End Sub

    ' ファイル書き込み(DEBUG)
    Private Sub Dbgwrite(str As String)
        Dim sw As New IO.StreamWriter(DEBUGF, True, System.Text.Encoding.GetEncoding("shift_jis"))
        sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") & str)
        sw.Close()
    End Sub

    ' ディレクトリダイアログ表示してパス取得
    Private Function GetDirDialog()
        Dim dir = ""
        Using fbd As FolderBrowserDialog = New FolderBrowserDialog
            If fbd.ShowDialog() = DialogResult.OK Then
                ' 末尾が\じゃなかったら\をつける
                If Not fbd.SelectedPath.EndsWith("\") Then fbd.SelectedPath += "\"
                dir = fbd.SelectedPath
            End If
        End Using
        Return dir
    End Function
#End Region
    ' ショートカット F1
    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles BT_A1.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                Dim f As Form = New SCC1_S1_MNG
                f.ShowDialog()
                f.Dispose()

            Case Keys.F2
            Case Keys.F3
        End Select
    End Sub

End Class
