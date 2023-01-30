Public Class SCA1_S3

    Private ReadOnly cmn As New Common
    Private items As S3Items                ' 戻り値アイテム
    Private row As DataGridViewRow          ' タスクリストの選択行情報

    Private Sub SCA1_S3_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' 選択中のタスクリスト(ノード番号)取得
        TB_A1.Text = "全体"
        If SCA1.TV_A1.SelectedNode IsNot Nothing Then
            items.p_list = SCA1.TV_A1.SelectedNode.Name     ' NodeXX
            TB_A1.Text = SCA1.TV_A1.SelectedNode.Text       ' ノード名称
        End If

        ' 初期値設定
        If row Is Nothing Then
            ' 追加
            Pic1.AllowDrop = True
            L_STS.Text = "添付ファイルは右上にドラッグ＆ドロップ"
        Else
            Pic1.Visible = False
            L_FILE.Visible = False
            L_STS.Text = "添付ファイルは右上にドラッグ＆ドロップ"
            ' 編集  選択中情報読み込み
            CB_A2.Text = row.Cells(3).Value         ' 分類
            If row.Cells(4).Value <> "" Then
                DTP_A4.Value = row.Cells(4).Value   ' 期限
                DTP_A4.Enabled = True
            End If
            TB_A5.Text = row.Cells(5).Value         ' タスク
            CB_A3.Text = row.Cells(6).Value         ' 担当
            ' 添付と作成日は変更しない
        End If
            Me.ActiveControl = TB_A5
    End Sub

#Region "ボタン"
    ' 確定ボタン(追加 or 編集)
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        Me.DialogResult = DialogResult.OK
        SetOutputParam()
        Me.Close()
    End Sub

    ' キャンセルボタン
    Private Sub BT_A2_Click(sender As Object, e As EventArgs) Handles BT_A2.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
#End Region

    ' 戻り値設定
    Private Sub SetOutputParam()
        items.p_date = Date.Now.ToString("yyyy/MM/dd")
        items.p_group = CB_A2.Text
        items.p_person = CB_A3.Text
        If DTP_A4.Enabled Then items.p_limit = DTP_A4.Text
        items.p_content = TB_A5.Text
    End Sub

    ' ファイルドラッグ＆ドロップ
    Private Sub ListBox1_DragEnter(sender As Object, e As DragEventArgs) Handles Pic1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            'ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
            e.Effect = DragDropEffects.Copy
        Else
            'ファイル以外は受け付けない
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub ListBox1_DragDrop(sender As Object, e As DragEventArgs) Handles Pic1.DragDrop
        Dim flist As New List(Of String)
        Dim fsize As Integer = 0
        Const SIZE_OVER As Integer = 200 * 1024 * 1024 ' 200Mバイト
        ' ドロップされたすべてのファイル名を取得する
        Dim fileName As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())
        For Each f As String In fileName
            If Not IO.File.Exists(f) Then
                MsgBox("フォルダは格納できません。", MessageBoxIcon.Warning)
                Exit Sub
            End If
            Dim fi As New IO.FileInfo(f)
            fsize += fi.Length
            flist.Add(f)
        Next

        ' ファイルサイズが大きい場合の注意喚起
        If fsize > SIZE_OVER Then
            Dim r = MessageBox.Show("添付ファイルのサイズが大きいので、アップロードに時間がかかります。" & vbCrLf &
                                    "このまま添付しても良いですか？",
                                    "ご確認ください",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question)
            If r = vbNo Then Exit Sub
        End If
        ' ファイルリストに設定(アップロードするのはSCA1に戻ってから)
        items.p_file = flist
        L_STS.Text = items.p_file.Count & " 個のファイルを添付"
    End Sub

    ' 親Formコール用のパラメータ取得
    Public Sub SetItems(r As DataGridViewRow)
        row = r
    End Sub
    Public Function GetItems()
        Return items
    End Function

    ' 期限の有無チェック
    Private Sub CB_Limit_CheckedChanged(sender As Object, e As EventArgs) Handles CB_Limit.CheckedChanged
        DTP_A4.Enabled = CB_Limit.Checked
    End Sub
End Class

' 親(SCA1)用パラメータ構造体
Structure S3Items
    Public p_date As String             ' 追加日時
    Public p_list As String             ' リスト名
    Public p_group As String            ' 分類
    Public p_person As String           ' 担当者
    Public p_limit As String            ' 期限
    Public p_content As String          ' 内容
    Public p_file As List(Of String)    ' 添付ファイル
End Structure


