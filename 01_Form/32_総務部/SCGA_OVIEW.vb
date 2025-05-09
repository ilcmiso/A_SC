Imports System.IO

Public Class SCGA_OVIEW

    Private ReadOnly cmn As New Common
    Private ReadOnly sccmn As New SCcommon
    Private ReadOnly xml As New XmlMng
    Private ReadOnly log As New Log
    Private ReadOnly db As Sqldb = SCA1.db
    Private ReadOnly ROWMAXCOUNT As Integer = 10

    Public Sub ShowOVIEW(cid As String)
        log.TimerST()
        DGV.Rows.Clear()

        'db.UpdateOrigDT(Sqldb.TID.MR)
        Dim dt As DataTable = db.OrgDataTable(Sqldb.TID.MR)

        ' 顧客番号が一致する、郵便発送(MAIL_SEND)と郵便受領(MAIL_RECV)以外の行を抽出＆並び替え
        Dim selectedRows = dt.AsEnumerable().
        Where(Function(r) r("C06").ToString() = cid AndAlso
            Val(r("C02").ToString()) <> CInt(SCcommon.MRITEMID.MAIL_SEND) AndAlso
            Val(r("C02").ToString()) <> CInt(SCcommon.MRITEMID.MAIL_RECV)
        ).
        OrderByDescending(Function(r) r("C01").ToString()).
        Take(ROWMAXCOUNT)

        For Each row In selectedRows
            Dim index As Integer = Val(row("C02").ToString())
            Dim item As String = sccmn.MRITEMLIST(index)
            Dim col4 As String = ""
            Dim col5 As String = ""

            Select Case index
                Case SCcommon.MRITEMID.REPAY_F, SCcommon.MRITEMID.REPAY_A
                    col4 = row("C08").ToString()
                    col5 = row("C11").ToString()
                Case SCcommon.MRITEMID.PARTIAL_REPAY_F
                    col4 = row("C11").ToString()
                    col5 = row("C12").ToString()
                Case SCcommon.MRITEMID.PARTIAL_REPAY_A
                    col4 = row("C12").ToString()
                    col5 = row("C13").ToString()
                Case SCcommon.MRITEMID.FULL_REPAY
                    col4 = row("C08").ToString()
                    col5 = row("C11").ToString()
                Case SCcommon.MRITEMID.CONTACT_CHANGE
                    col4 = row("C08").ToString()
                    col5 = row("C09").ToString()
                Case SCcommon.MRITEMID.ACCOUNT_CHANGE
                    col4 = row("C08").ToString()
                    col5 = row("C10").ToString()
            End Select
            DGV.Rows.Add(row("C01"), row("C04"), row("C03"), item, row("C05"), col4, col5)
        Next
        log.TimerED("ShowOVIEW")
    End Sub

    ' ジャンプボタン
    Private Sub BT_PI4FIX_Click(sender As Object, e As EventArgs) Handles BT_PI4FIX.Click
        If DGV.Rows.Count = 0 Then Exit Sub
        SCA1.ShowSelectMR1(DGV.CurrentRow.Cells(0).Value, DGV.CurrentRow.Cells(3).Value)
    End Sub
End Class