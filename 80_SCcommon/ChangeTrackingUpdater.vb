Imports System.Data.SqlClient

Public Class ChangeTrackingUpdater
    Private log As New Log
    Private connectionString As String = "Server=localhost;Database=A_SC;User Id=ilc;Password=ilcmng;"
    Private lastSyncVersion As Long = 0

    ' DGVごとのデータ変換処理を保持する辞書
    Private dgvMapping As Dictionary(Of DataGridView, Func(Of DataTable, DataTable))

    ''' <summary>
    ''' コンストラクタ: 反映対象のDGVと、それぞれのデータ変換ロジックを受け取る
    ''' </summary>
    Public Sub New(ByVal mappings As Dictionary(Of DataGridView, Func(Of DataTable, DataTable)))
        dgvMapping = mappings
    End Sub

    ''' <summary>
    ''' 変更監視を開始
    ''' </summary>
    Public Sub StartMonitoring()
        Dim timer As New Timer()
        AddHandler timer.Tick, AddressOf CheckForChanges
        timer.Interval = 1000 ' 1秒間隔でポーリング
        timer.Start()
        log.cLog("POLLING:START[SQL Server]")
    End Sub

    ''' <summary>
    ''' SQL Server の CHANGETABLE を使い、前回以降の変更を取得
    ''' </summary>
    Private Sub CheckForChanges(sender As Object, e As EventArgs)
        Dim changesTable As New DataTable()

        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' 現在の変更トラッキングバージョンを取得
            Dim currentVersion As Long
            Using versionCmd As New SqlCommand("SELECT CHANGE_TRACKING_CURRENT_VERSION()", connection)
                currentVersion = CLng(versionCmd.ExecuteScalar())
            End Using

            ' 初回実行時は lastSyncVersion が 0 のため、変更検出をスキップ
            If lastSyncVersion > 0 Then
                Dim query As String = "SELECT CT.SYS_CHANGE_OPERATION, T.* " &
                                      "FROM CHANGETABLE(CHANGES dbo.FKSCD, @lastSyncVersion) AS CT " &
                                      "JOIN dbo.FKSCD AS T ON T.FKD01 = CT.FKD01"
                Using cmd As New SqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@lastSyncVersion", lastSyncVersion)
                    Using adapter As New SqlDataAdapter(cmd)
                        adapter.Fill(changesTable)
                    End Using
                End Using

                ' 変更がない場合は何もしない
                If changesTable.Rows.Count = 0 Then
                Else
                    ' 変更をすべてのDGVに適用
                    log.cLog("POLLING:変更検出")

                    For Each kvp In dgvMapping
                        Dim dgv As DataGridView = kvp.Key
                        Dim transformFunc As Func(Of DataTable, DataTable) = kvp.Value

                        ' DGVのデータを取得
                        Dim currentTable As DataTable = TryCast(dgv.DataSource, DataTable)
                        If currentTable Is Nothing Then Continue For ' データなしならスキップ

                        ' 変更データを変換
                        Dim updatedTable As DataTable = transformFunc(changesTable)

                        dgv.Invoke(Sub()
                                       ' 変更のある行だけを更新
                                       For Each updatedRow As DataRow In updatedTable.Rows
                                           Dim key As String = updatedRow("FKD01").ToString()

                                           ' DGVの既存行を探す
                                           Dim targetRow As DataGridViewRow = dgv.Rows.Cast(Of DataGridViewRow)().
                                                                            FirstOrDefault(Function(r) r.Cells("FKD01").Value?.ToString() = key)

                                           If targetRow IsNot Nothing Then
                                               ' 既存行を更新
                                               For Each col As DataColumn In updatedTable.Columns
                                                   If dgv.Columns.Contains(col.ColumnName) Then
                                                       targetRow.Cells(col.ColumnName).Value = updatedRow(col.ColumnName)
                                                   End If
                                               Next
                                           Else
                                               ' 新しい行を追加
                                               Dim newRow As DataRow = currentTable.NewRow()
                                               For Each col As DataColumn In updatedTable.Columns
                                                   newRow(col.ColumnName) = updatedRow(col.ColumnName)
                                               Next
                                               currentTable.Rows.Add(newRow)
                                           End If
                                       Next
                                       dgv.Refresh() ' UIを更新
                                   End Sub)
                    Next
                End If
            End If

            ' 同期バージョンを更新
            lastSyncVersion = currentVersion
        End Using
    End Sub
End Class
