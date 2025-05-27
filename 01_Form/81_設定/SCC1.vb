Imports System.IO
Imports System.Net.WebRequestMethods

Public Class SCC1

#Region "定義"

    Private ReadOnly xml As New XmlMng
#End Region

#Region "イベント"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.KeyPreview = True
        PictureBox1.AllowDrop = True
        CB_DBSW.Checked = xml.GetDBSwitch

        ' データ格納パスを読み込み
        RadioButton2.Checked = (xml.xmlData.CPathSW = "2")
        TextBox1.Text = xml.xmlData.CPath1
        TextBox2.Text = xml.xmlData.CPath2
        TB_SQLAddr.Text = xml.xmlData.SQLSvAddr
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        XmlSave()                               ' 固有情報(Xml)にデータ格納パスを保存する
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim r = MessageBox.Show($"ILCのサーバーから、A_SCの最新アプリを取得しますか？",
                                "ご確認ください",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
        If r = vbYes Then
            Dim ftp As New FtpMng
            Dim fileVer As String = ftp.GetNewFile()
            If fileVer <> "" Then
                MsgBox($"最新バージョン {fileVer} のアプリを取得しました。{vbCrLf}ZIPファイルを展開して、更新してください。")
                ftp.OpenFTPDir()
            Else
                MsgBox($"最新のアプリはありませんでした。")
            End If
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
        xml.GetXml()
        xml.xmlData.CPath1 = TextBox1.Text
        xml.xmlData.CPath2 = TextBox2.Text
        xml.xmlData.SQLSvAddr = TB_SQLAddr.Text
        If RadioButton1.Checked Then xml.xmlData.CPathSW = 1
        If RadioButton2.Checked Then xml.xmlData.CPathSW = 2
        xml.SetXml()        ' 固有情報(Xml)に保存
    End Sub
#End Region

    Private Sub CB_DBSW_CheckedChanged(sender As Object, e As EventArgs) Handles CB_DBSW.CheckedChanged
        Dim xml As New XmlMng
        xml.SetDBSwitch(CB_DBSW.Checked)
        xml.SetXml()
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

    Private Sub PictureBox1_DragEnter(sender As Object, e As DragEventArgs) Handles PictureBox1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub PictureBox1_DragDrop(sender As Object, e As DragEventArgs) Handles PictureBox1.DragDrop
        Dim ftp As New FtpMng
        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())

        ' 1つ目のファイルだけ使う
        If files.Length > 0 Then
            Dim filePath As String = files(0)

            Try
                ftp.UploadFile(filePath)
                MessageBox.Show($"ILCのサーバーに配置できました。 {vbCrLf}{Path.GetFileName(filePath)}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show($"ILCのサーバーに配置できませんでした。{vbCrLf}{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    ' ショートカット F1
    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles Me.KeyDown
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
