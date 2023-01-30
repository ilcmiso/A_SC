Public Class SCA1_S1
#Region " OPEN CLOSE "

    Public recvCID = ""                 ' 着信顧客識別番号(SCA1が値を設定する)
    Public recvTelNo = ""               ' 着信電話番号(SCA1が値を設定する)

    Private Sub FLS_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        PopupColor()
    End Sub

    Private Sub FLS_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
    End Sub

    ' 選択ボタン
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        ' DGV検索して同じ機構番号の顧客情報を表示する
        Dim hit = False
        For Each dr As DataGridViewRow In SCA1.DGV1.Rows
            If dr.Cells(0).Value = recvCID Then
                SCA1.DGV1.CurrentCell = SCA1.DGV1(1, dr.Index)
                hit = True
                Exit For
            End If
        Next
        'If Not hit Then SCA1.ShowInfoDetail(recvCID)         ' もし検索等しててDGV1に対象が表示されていなかったら、DGV2にだけ情報表示

        ' 顧客検索画面の電話番号一致カラーリング
        SCA1.SearchColor(recvTelNo)
    End Sub

    ' ポップアップ画面の電話番号一致カラーリング
    Private Sub PopupColor()
        Dim search_txt() = {TB_A3, TB_A4, TB_B3, TB_B4}   ' カラーリングするテキストリスト
        For Each t In search_txt
            If t.Text.Replace("-", "").Length = 0 Then      ' 電話番号欄が空白なら白いまま
                t.BackColor = Color.White
            Else                                            ' 電話番号が一致したらカラーリング
                If recvTelNo = t.Text.Replace("-", "") Then t.BackColor = Color.LightSalmon Else t.BackColor = Color.White
            End If
        Next
    End Sub

#End Region

End Class