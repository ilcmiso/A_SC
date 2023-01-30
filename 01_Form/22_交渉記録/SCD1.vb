Imports System.IO
Imports System.Text
Imports System.Data.SQLite

Public Class SCD1

#Region "定義"
    Private ReadOnly cmn As New Common
    Private Const DELIMITER As String = ","
    Private Const TEL_MOBILE As String = "携　帯"
    Private Const TEL_HOME As String = "自宅"
    Private Const TEL_COMPANY As String = "会　社"
    Private Const STS_OFF As String = "電源ＯＦＦ"
    Private Const STS_ABS As String = "留守電"
    Private Const STS_NON As String = "出ず"
#End Region

#Region "イベント"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' 「時間」のフォーマット設定
        'DTP_D1.CustomFormat = "HH:mm"
        'DTP_D2.CustomFormat = "HH:mm"
        ' 「日付」の初期値設定
        DTP_B1.Value = Today.ToString("yyyy/MM/01")
        DTP_B2.Value = DTP_B1.Value.AddMonths(1).AddDays(-1)
        LB_J1.SelectedIndex = 0
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
    End Sub

    ' 作成ボタン
    Private Sub BT_A1_Click(sender As Object, e As EventArgs) Handles BT_A1.Click
        'Dim f As New VBR_SCD1
        'f.ShowDialog()
        'f.Dispose()
        Dim dateList As New List(Of String)         ' 記録する日付のリスト
        Dim outList As New List(Of String)          ' 出力する記録のリスト 最終的にExcelになるもの
        Console.WriteLine("-----------------------")
        Dim stDate As Date = DTP_B1.Value
        Dim periodDay As Integer = DateDiff("d", DTP_B1.Value, DTP_B2.Value)       ' 期間の日数
        Dim contentsR1() As String = Nothing
        Dim contentsR2() As String = Nothing

        ' ランダム配列設定  R1とR2はセットで出力される
        Select Case LB_J1.SelectedIndex
            Case 0  ' 自宅＆携帯
                contentsR1 = {TEL_HOME, TEL_HOME, TEL_MOBILE, TEL_MOBILE, TEL_MOBILE, TEL_COMPANY}
                contentsR2 = {STS_NON, STS_ABS, STS_OFF, STS_ABS, STS_NON, STS_NON}
            Case 1  ' 自宅
                contentsR1 = {TEL_HOME, TEL_HOME, TEL_COMPANY}
                contentsR2 = {STS_NON, STS_ABS, STS_NON}
            Case 2  ' 携帯
                contentsR1 = {TEL_MOBILE, TEL_MOBILE, TEL_MOBILE, TEL_COMPANY}
                contentsR2 = {STS_OFF, STS_ABS, STS_NON, STS_NON}
        End Select


        ' 全部の日付を記録リストに設定する
        For n As Integer = 0 To periodDay
            'dateList.Add(stDate.AddDays(n).ToString("yyyy/MM/dd"))
            dateList.Add(cmn.ConvJPDate(stDate.AddDays(n)))
        Next

        ' 記録日を選定。指定日数になるまでランダムに減らしていく
        Dim r As New Random()
        For n As Integer = periodDay To NUD_C1.Value Step -1
            Dim rand = r.Next(n - 1)
            dateList.RemoveAt(rand)            ' ランダムの位置を削除
        Next

        ' 督促状送付の挿入位置をランダムで設定
        Dim rday As Integer = -4
        Dim rList As New List(Of Integer)
        For n As Integer = 0 To NUD_D1.Value - 1
            rday = r.Next(rday + 5, rday + 10)
            rList.Add(rday)                     ' 取得した番号を、outListのIndex位置にあとで督促状送付を追加する
        Next

        ' 記録日にパラメータを付与
        Dim dateStr As String
        For n As Integer = 0 To dateList.Count - 1
            dateStr = dateList(n)
            Dim cnt As Integer = r.Next(2) + 1
            Dim Rtime() As String = GetRTime(r)
            For id = 0 To cnt
                If id > 0 Then dateStr = ""       ' 日の最初(id:0)だけは日付ありで、それ以降は省略(空白文字)
                Dim rIdx As Integer = r.Next(contentsR1.Length)
                outList.Add(dateStr & DELIMITER & Rtime(id) & DELIMITER & contentsR1(rIdx) & DELIMITER & DELIMITER & DELIMITER &
                                      TB_H1.Text & DELIMITER & DELIMITER & contentsR2(rIdx))
            Next
            ' 督促状送付　rListに含まれているIndexのときだけ付与
            If rList.Contains(n) Then
                outList.RemoveAt(outList.Count - 1)
                outList.Add("" & DELIMITER & "" & DELIMITER & TEL_HOME & DELIMITER & DELIMITER & DELIMITER &
                                 TB_H1.Text & DELIMITER & "督促状送付" & DELIMITER & DELIMITER & "")
            End If
        Next

        ' 最終出力
        Dim f As New VBR_SCD1
        f.SetList(outList)         ' リストをパラメータ渡し
        f.ShowDialog()
        f.Dispose()
    End Sub

    ' ランダム時間取得  id 0:午前中 1:午後 2:夕方
    Private Function GetRTime(r As Random) As String()
        'Dim edTime As DateTime = DateTime.Parse(TB_D2.Text)
        'Dim periodMin As Integer = DateDiff("n", stTime, edTime)              ' 差分の時間(分)
        'Dim r As New Random
        Dim retTime(3) As String                    ' 戻り値

        Dim nowTime As DateTime = Nothing
        Dim rndTime As Integer = 0
        Dim stTime As DateTime = DateTime.Parse(TB_D1.Text)
        Dim stockTime As Integer = 4 * 60 + 10      ' 余り時間 4時間10分

        ' 1st :     08:30 + 乱数(0~4:10)
        rndTime = r.Next(stockTime)
        Dim t1 As DateTime = stTime.AddMinutes(rndTime)
        stockTime -= rndTime

        ' 2nd :     1st   + 4時間 + (0~1st余り)
        rndTime = r.Next(stockTime)
        Dim t2 As DateTime = t1.AddHours(4).AddMinutes(rndTime)
        stockTime -= rndTime

        ' 3rd :     2nd   + 4時間 + (0~2nd余り)
        rndTime = r.Next(stockTime)
        Dim t3 As DateTime = t2.AddHours(4).AddMinutes(rndTime)

        retTime(0) = t1.ToString("HH:mm")
        retTime(1) = t2.ToString("HH:mm")
        retTime(2) = t3.ToString("HH:mm")
        Return retTime
        'Dim ret As String = stTime.AddHours(id * 4).AddMinutes(r.Next(60 * 3.8)).ToString("HH:mm")      ' 4時間毎(60 * 4)だと、id:0と1で1分差とかもありえるので、3.8で調整
    End Function



#End Region

End Class
