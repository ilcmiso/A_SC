Public Class SCcommon
    ' 団信加入サインステータス
    Public ReadOnly GROUPCREDIT() As String = {"未加入", "加入(単生)", "加入(連生)", "脱退済"}

    ' 団信加入サインの値取得
    Public Function GetGroupCredit(status As String) As String
        Dim cmn As New Common
        Dim val As Integer = cmn.Int(status)

        ' 値が範囲外のときは空白を返却
        If val >= GROUPCREDIT.Length Then Return ""

        ' 正常値返却
        Return GROUPCREDIT(val)
    End Function
End Class
