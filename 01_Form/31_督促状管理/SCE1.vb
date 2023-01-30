Imports System.IO
Imports System.Text
Imports System.Data.SQLite

Public Class SCE1

#Region "定義"
    Private ReadOnly cmn As New Common
    Private ReadOnly log As New Log
    Private ReadOnly db As New Sqldb

#End Region

#Region "イベント"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
    End Sub
#End Region

#Region "表示"
    ' 各DGVの表示
    Private Sub ShowDGVList(dgv As DataGridView)
        ShowDGVList(dgv, "")
    End Sub
    Private Sub ShowDGVList(dgv As DataGridView, FilterWord As String)
        Dim idx As Integer = 0
        Dim dt As DataTable
        Dim bindID As Integer

        If dgv.Rows.Count > 0 Then idx = dgv.CurrentRow.Index           ' 元の選択中行を覚えておく

        Select Case True
            Case dgv Is DGV1                                ' ## 顧客情報タブ リスト
                bindID = 0
                dt = db.OrgDataTablePlusAssist.Copy         ' DataTableをオリジナルからコピー
                AddREMtoDGV1(dt)                            ' REMをDGVに追加

                ' ダミー顧客を生成
                Dim newRow As DataRow = dt.NewRow
                With newRow
                    .Item(0) = "999999999999999_000"
                    .Item(1) = "999999999999999"
                    .Item(9) = "ダミー"
                    .Item(10) = "ダミー"
                    .Item(54) = "999999999"
                End With
                dt.Rows.Add(newRow)
            Case dgv Is DGV2                                ' ## 顧客情報タブ 交渉記録
            Case Else
                Exit Sub
        End Select
        BindDGVList(dgv, dt, bindID)                         ' Bind(DataTableとDGV紐付け)
        FilterWordsDGV(dt, FilterWord, dgv)                  ' 検索ワードフィルタ

        dgv.AutoGenerateColumns = False                      ' DataTable設定時に自動で列追加されないようにする
        dgv.DataSource = dt

        ' DGVの選択行を元に戻す
        Select Case dgv.Rows.Count
            Case 0                                            ' 表示結果、列0ならそのまま一番上
            Case <= idx : dgv.CurrentCell = dgv(2, 0)         ' 表示結果、列が減って表示外なら一番上
            Case Else : dgv.CurrentCell = dgv(2, idx)         ' 表示可能なら元の位置に戻す               ' 可視化されてる列じゃないとエラーになる
        End Select

        ' DGV毎の後処理
        Select Case True
            Case dgv Is DGV1
                DGV1.Sort(DGV1.Columns(5), ComponentModel.ListSortDirection.Descending)
                AddREMtoDGV1(dt)                                ' DGV1に督促日設定
            Case Else
        End Select
    End Sub

    ' Bind列の設定
    Private Sub BindDGVList(dgv As DataGridView, ByRef dt As DataTable, bindID As Integer)
        ' DGVにBindするカラムリスト
        Dim BindList(,) As String = {
            {"FK02", "FK10", "FK11", "FK51", "FK70", "FK55", "", ""}                                        ' DGV1   債権情報一覧
        }
        ' Bind列の設定  DGVとDataTableを紐付ける　BindListの設定するカラムだけをBind設定
        For clmSetNum = 0 To BindList.GetLength(1) - 1
            For dtNum = 0 To dt.Columns.Count - 1
                If BindList(bindID, clmSetNum) = "" Then Exit For                                   ' BindTblのメンバーが "" の場合はバインド列を設定しない Skip
                If BindList(bindID, clmSetNum) <> dt.Columns(dtNum).ToString Then Continue For
                dgv.Columns(clmSetNum).DataPropertyName = dt.Columns(dtNum).ToString                ' Bind列の設定
                Exit For
            Next
        Next
    End Sub
    ' 検索ワードフィルタ
    Private Sub FilterWordsDGV(ByRef dt As DataTable, FilterWord As String, dgv As DataGridView)
        If FilterWord = "" Then Exit Sub
        FilterWord = FilterWord.Replace(" ", "").Replace("　", "").Replace(",", "")   ' 検索ワードから半角全角のスペースを除去
        Dim word As String
        Dim sb As New StringBuilder

        For r = dt.Rows.Count - 1 To 0 Step -1
            ' 検索したときの検索対象列 DGV毎
            Select Case True
                Case dgv Is DGV1
                    With sb
                        '.Append(cmn.C_int(dt.Rows(r).Item(48).ToString) + cmn.C_int(dt.Rows(r).Item(49).ToString)).Append(",")        ' 返済額
                        '.Replace(" ", "").Replace("　", "").Replace("-", "")
                    End With
                    word = sb.ToString
                    sb.Clear()
                Case Else
                    Exit Sub
            End Select
            If word.IndexOf(FilterWord) < 0 And
               word.IndexOf(StrConv(FilterWord, VbStrConv.Katakana Or VbStrConv.Narrow)) < 0 Then dt.Rows(r).Delete()        ' 含まれていなければ削除(非表示)
        Next
    End Sub

#End Region


End Class
