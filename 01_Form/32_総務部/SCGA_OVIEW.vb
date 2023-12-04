Imports System.IO

Public Class SCGA_OVIEW

    Private ReadOnly cmn As New Common
    Private ReadOnly sccmn As New SCcommon
    Private ReadOnly xml As New XmlMng
    Private ReadOnly log As New Log
    Private ReadOnly db As Sqldb = SCA1.db
    Private ReadOnly ROWMAXCOUNT As Integer = 10

    Private Sub FLS_Shown(sender As Object, e As EventArgs) Handles MyBase.Load
        ShowOVIEW()
    End Sub

    Public Sub ShowOVIEW()
        DGV.Rows.Clear()
        Dim selectedRows As DataRow() = db.OrgDataTable(Sqldb.TID.MR).Select("", "C01 DESC")

        For Each row As DataRow In selectedRows
            Dim index As Integer = Convert.ToInt32(row("C02"))
            Dim item As String = sccmn.MRITEMLIST(index)
            Dim column4Value As String = ""
            Dim column5Value As String = ""

            Select Case index
                Case 0
                    column4Value = row("C07").ToString()
                    column5Value = row("C11").ToString()
                Case 1, 6
                    column4Value = row("C07").ToString()
                    column5Value = row("C08").ToString()
                Case 2, 3
                    column4Value = row("C07").ToString()
                    column5Value = row("C09").ToString()
                Case 4
                    column4Value = row("C07").ToString()
                    column5Value = row("C14").ToString()
                Case 5
                    column4Value = row("C06").ToString()
                    column5Value = row("C07").ToString()
            End Select

            DGV.Rows.Add(row("C01"), row("C04"), row("C03"), item, row("C05"), column4Value, column5Value)
            If DGV.Rows.Count = ROWMAXCOUNT Then Exit For
        Next
    End Sub

    ' 参照中データ移動ボタン
    Private Sub BT_PI4FIX_Click(sender As Object, e As EventArgs) Handles BT_PI4FIX.Click
        SCA1.ViewSelectedMR(DGV.CurrentRow.Cells(3).Value, DGV.CurrentRow.Cells(0).Value)
    End Sub
End Class