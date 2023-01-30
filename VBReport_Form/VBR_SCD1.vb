Public Class VBR_SCD1

    Private sList As New List(Of String)

    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ViewerControl1.Clear()

        CellReport1.FileName = SC.CurrentAppPath & "\Excel\02_å⁄ãqåè¬åoâﬂãLò^.xlsx"

        CellReport1.ScaleMode = AdvanceSoftware.VBReport8.ScaleMode.Pixel
        CellReport1.ApplyFormula = True
        CellReport1.Report.Start()
        CellReport1.Report.File()
        CellReport1.Page.Start("Sheet1", "1")

        CellReport1.Cell("B5").Value = SCD1.TB_A1.Text      ' å⁄ãqéÅñº

        Dim str() As String
        For row = 0 To sList.Count - 1
            str = sList(row).Split(",")
            For clm = 0 To str.Count - 1
                CellReport1.Cell("A9", clm, row).Value = str(clm)
            Next
        Next

        CellReport1.Page.End()
        CellReport1.Report.End()
        ViewerControl1.Document = CellReport1.Document
    End Sub

    Public Sub SetList(list As List(Of String))
        sList = list
    End Sub


End Class