Public Class SCcommon
    ' 団信加入サインステータス
    Public ReadOnly GROUPCREDIT() As String = {"未加入", "加入(単生)", "加入(連生)", "脱退済"}
    Public ReadOnly MRITEMLIST() As String = {"団信弁済", "一部繰り上げ返済(フラット)", "一部繰り上げ返済(アシスト)", "完済管理", "契約条件変更", "口座変更", "郵便発送簿", "郵便受領簿"}
    Public ReadOnly FPITEMLIST() As String = {"基本情報", "任売", "競売", "破産", "再生", "差押", "相続", "債務引受", "条件変更", "抵当権解除", "原状用途変更", "持分変更"}

    Public Enum MRITEMID As Integer
        REPAY = 0           ' 弁済
        PARTIAL_REPAY_F     ' 一部繰り上げフラット
        PARTIAL_REPAY_A     ' 一部繰り上げアシスト
        FULL_REPAY          ' 完済管理
        CONTACT_CHANGE      ' 契約条件変更
        ACCOUNT_CHANGE      ' 口座変更
        MAIL_SEND           ' 郵便発送
        MAIL_RECV           ' 郵便受領
    End Enum

    ' 団信加入サインの値取得
    Public Function GetGroupCredit(status As String) As String
        Dim cmn As New Common
        Dim val As Integer
        If status = "" Then Return ""           ' データ読み取り前などのときは未加入ではなく空白で表す
        val = cmn.Int(status)
        ' 値が範囲外のときは空白を返却
        If val >= GROUPCREDIT.Length Then Return ""

        ' 正常値返却
        Return GROUPCREDIT(val)
    End Function
End Class
