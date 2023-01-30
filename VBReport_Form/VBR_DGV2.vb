Public Class VBR_DGV2

    Private Const ADDPAGE_COUNT = 3               ' �ǉ��y�[�W�̍��ڐ�
    Private ReadOnly cmn As New Common
    Private ReadOnly db As New Sqldb
    Private Sub ME_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim cNum As Integer = SCA1.DGV2.SelectedRows.Count                          ' �������ڋq��
        Dim totalPage As Integer = Math.Floor((cNum + 1) / ADDPAGE_COUNT) + 1       ' �y�[�W���ꐔ
        ViewerControl1.Clear()
        CellReport1.FileName = cmn.CurrentPath & Common.DIR_EXC & "SC01.xls"
        CellReport1.ScaleMode = AdvanceSoftware.VBReport8.ScaleMode.Pixel
        CellReport1.ApplyFormula = True
        CellReport1.Report.Start()
        CellReport1.Report.File()

        If SCA1.DGV1.CurrentRow.Cells(0).Value = Common.DUMMY_NO Then
            ' �_�~�[�ڋq
            totalPage = Math.Floor((cNum - 1) / ADDPAGE_COUNT) + 1     ' �y�[�W���ꐔ
            For p = 0 To totalPage - 1
                CellReport1.Page.Start("Sheet3", "1")
                WriteSheet2(p * ADDPAGE_COUNT + 0, 0)
                WriteSheet2(p * ADDPAGE_COUNT + 1, 1)
                WriteSheet2(p * ADDPAGE_COUNT + 2, 2)
                CellReport1.Page.End()
            Next
        Else
            ' ���o���y�[�W
            CellReport1.Page.Start("Sheet1", "1")
            WriteSheet1(1, totalPage)
            CellReport1.Page.End()

            ' ���L�^�������I���̏ꍇ��2�y�[�W�ȍ~
            For p = 0 To totalPage - 2
                CellReport1.Page.Start("Sheet2", "1")
                WriteSheet2(p * ADDPAGE_COUNT + 1, 0)
                WriteSheet2(p * ADDPAGE_COUNT + 2, 1)
                WriteSheet2(p * ADDPAGE_COUNT + 3, 2)
                CellReport1.Page.End()
            Next
        End If

        CellReport1.Report.End()
        ViewerControl1.Document = CellReport1.Document
    End Sub

    ' ���o���y�[�W�`��
    Private Sub WriteSheet1(page1 As Integer, page2 As Integer)
        ' �ǉ��d�b�ԍ�(TEL3)�������l�u�d�b�ԍ���ǉ��v��������󔒂�ݒ�
        Dim tel3 As String = SCA1.TB_B11.Text
        If tel3 = SCA1.ADDTEL_WORD Then tel3 = ""

        CellReport1.Cell("A1").Value = SCA1.DGV9(1, 0).Value   ' ���ԍ�
        CellReport1.Cell("A2").Value = SCA1.DGV9(3, 0).Value   ' �،��ԍ�(�A�V�X�g)

        CellReport1.Cell("B1").Value = SCA1.DGV9(1, 1).Value   ' ���Җ���
        CellReport1.Cell("B2").Value = SCA1.DGV9(1, 2).Value   ' ���Җ�
        CellReport1.Cell("B3").Value = SCA1.DGV9(3, 1).Value   ' TEL1
        CellReport1.Cell("B4").Value = ""                      ' TEL2
        CellReport1.Cell("B5").Value = ""                      ' TEL3
        CellReport1.Cell("B6").Value = ""                      ' TEL4
        CellReport1.Cell("B7").Value = SCA1.DGV9(1, 3).Value   ' �X�֔ԍ�
        CellReport1.Cell("B8").Value = SCA1.DGV9(1, 4).Value   ' �Z��
        CellReport1.Cell("B9").Value = SCA1.DGV9(1, 5).Value   ' �Ζ���
        CellReport1.Cell("B10").Value = SCA1.DGV9(3, 5).Value         ' �Ζ���TEL
        CellReport1.Cell("B11").Value = SCA1.DGV9(3, 2).Value         ' ���N����

        CellReport1.Cell("C1").Value = SCA1.DGV9(1, 6).Value    ' �A�э��Җ���
        CellReport1.Cell("C2").Value = SCA1.DGV9(1, 7).Value    ' �A�э��Җ�
        CellReport1.Cell("C3").Value = SCA1.DGV9(3, 6).Value    ' TEL1
        CellReport1.Cell("C4").Value = ""                       ' TEL2
        CellReport1.Cell("C5").Value = ""                       ' TEL3
        CellReport1.Cell("C6").Value = ""                       ' TEL4
        CellReport1.Cell("C7").Value = SCA1.DGV9(1, 8).Value        ' �X�֔ԍ�
        CellReport1.Cell("C8").Value = SCA1.DGV9(1, 9).Value        ' �Z��
        CellReport1.Cell("C9").Value = SCA1.DGV9(1, 10).Value        ' �Ζ���
        CellReport1.Cell("C10").Value = SCA1.DGV9(3, 10).Value        ' �Ζ���TEL
        CellReport1.Cell("C11").Value = SCA1.DGV9(3, 7).Value        ' ���N����

        ' �t���b�g35
        CellReport1.Cell("D1").Value = SCA1.DGV9(5, 2).Value         ' �ݕt���z
        CellReport1.Cell("D2").Value = SCA1.DGV9(5, 4).Value         ' �ԍϊz
        CellReport1.Cell("D3").Value = SCA1.DGV9(5, 7).Value         ' �X�V���c��
        CellReport1.Cell("D4").Value = SCA1.DGV9(5, 3).Value         ' �ݕt���z(B)
        CellReport1.Cell("D5").Value = SCA1.DGV9(5, 5).Value         ' �ԍϊz(B)
        CellReport1.Cell("D6").Value = SCA1.DGV9(5, 6).Value         ' �c���X�V��
        CellReport1.Cell("D7").Value = SCA1.DGV9(5, 8).Value         ' ���،���
        CellReport1.Cell("D8").Value = SCA1.DGV9(5, 9).Value         ' ���؍��v�z
        CellReport1.Cell("D9").Value = SCA1.DGV9(5, 1).Value         ' �����_���
        ' �A�V�X�g
        CellReport1.Cell("E1").Value = SCA1.DGV9(6, 2).Value         ' �ݕt���z
        CellReport1.Cell("E2").Value = SCA1.DGV9(6, 4).Value         ' �ԍϊz
        CellReport1.Cell("E3").Value = SCA1.DGV9(6, 7).Value         ' �X�V���c��
        CellReport1.Cell("E4").Value = SCA1.DGV9(6, 3).Value         ' �ݕt���z(B)
        CellReport1.Cell("E5").Value = SCA1.DGV9(6, 5).Value         ' �ԍϊz(B)
        CellReport1.Cell("E6").Value = SCA1.DGV9(6, 6).Value         ' �c���X�V��
        CellReport1.Cell("E7").Value = SCA1.DGV9(6, 8).Value         ' ���،���
        CellReport1.Cell("E8").Value = SCA1.DGV9(6, 9).Value         ' ���؍��v�z
        CellReport1.Cell("E9").Value = SCA1.DGV9(6, 1).Value         ' ���ϓ�

        CellReport1.Cell("G1").Value = SCA1.TB_FreeMemo.Text          ' �t���[����

        If SCA1.DGV2.Rows.Count > 0 Then    ' ���L�^��1���ł�����ꍇ�̂�
            Dim hassouType As String = ""
            Console.WriteLine(SCA1.DGV2.SelectedRows(0).Cells(0).Value)
            Dim dt As DataTable = db.GetSelect(Sqldb.TID.SCD, String.Format("Select FKD17 From FKSCD Where FKD01 = '{0}'", SCA1.DGV2.SelectedRows(0).Cells(0).Value))
            If dt.Rows.Count > 0 Then
                hassouType = dt.Rows(0).Item(0)
            End If
            With SCA1.DGV2.SelectedRows(0)
                CellReport1.Cell("F1").Value = .Cells(1).Value      ' ���L�^ ����
                CellReport1.Cell("F2").Value = .Cells(2).Value      ' ���L�^ ����
                CellReport1.Cell("F3").Value = .Cells(5).Value      ' ���L�^ �Ή���
                CellReport1.Cell("F4").Value = .Cells(6).Value      ' ���L�^ �Ή���2
                CellReport1.Cell("F5").Value = hassouType           ' ���L�^ ���� �� �������
                CellReport1.Cell("F6").Value = .Cells(4).Value      ' ���L�^ ��@
                CellReport1.Cell("F7").Value = .Cells(3).Value      ' ���L�^ �T�v
                CellReport1.Cell("F8").Value = .Cells(8).Value      ' ���L�^ ���l
            End With
        End If
        CellReport1.Cell("Z1").Value = page1        ' �y�[�W���@���q
        CellReport1.Cell("Z2").Value = page2        ' �y�[�W���@����
    End Sub

    ' �ڍ׃y�[�W�`��
    Private Sub WriteSheet2(row As Integer, idx As Integer)
        If row >= SCA1.DGV2.SelectedRows.Count Then Exit Sub
        Dim hassouType As String = ""
        Dim dt As DataTable = db.GetSelect(Sqldb.TID.SCD, String.Format("Select FKD17 From FKSCD Where FKD01 = '{0}'", SCA1.DGV2.SelectedRows(row).Cells(0).Value))
        If dt.Rows.Count > 0 Then
            hassouType = dt.Rows(0).Item(0)
        End If
        With SCA1.DGV2.SelectedRows(row)
            CellReport1.Cell("A1", idx, 0).Value = .Cells(1).Value      ' ���L�^ ����
            CellReport1.Cell("A1", idx, 1).Value = .Cells(2).Value      ' ���L�^ ����
            CellReport1.Cell("A1", idx, 2).Value = .Cells(5).Value      ' ���L�^ �Ή���
            CellReport1.Cell("A1", idx, 3).Value = .Cells(6).Value      ' ���L�^ �Ή���2
            CellReport1.Cell("A1", idx, 4).Value = hassouType           ' ���L�^ ���� �� �������
            CellReport1.Cell("A1", idx, 5).Value = .Cells(4).Value      ' ���L�^ ��@
            CellReport1.Cell("A1", idx, 6).Value = .Cells(3).Value      ' ���L�^ �T�v
            CellReport1.Cell("A1", idx, 7).Value = .Cells(8).Value      ' ���L�^ ���l
            If .Cells(10).Value <> "" Then
                CellReport1.Cell("A1", idx, 9).Value = .Cells(10).Value ' ���L�^ �_�~�[��
            End If
        End With
        'CellReport1.Cell("Z1").Value = page1             ' �y�[�W���@���q
        'CellReport1.Cell("Z2").Value = page2             ' �y�[�W���@����
    End Sub
End Class