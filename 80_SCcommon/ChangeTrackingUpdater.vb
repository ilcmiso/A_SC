Imports System.Data.SqlClient
Imports System.Windows.Forms

Public Class ChangeTrackingUpdater
    Private log As New Log
    Private connectionString As String = "Server=localhost;Database=A_SC;User Id=ilc;Password=ilcmng;"
    Private lastSyncVersion As Long = 0

    ' DGVごとのデータ変換処理を保持する辞書
    Private dgvMapping As Dictionary(Of DataGridView, Func(Of DataTable, DataTable))
    Private trackingTimer As Timer  ' ポーリング用タイマー

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
        trackingTimer = New Timer()
        AddHandler trackingTimer.Tick, AddressOf CheckForChanges
        trackingTimer.Interval = 1000 ' 1秒間隔
        trackingTimer.Start()
        log.cLog("POLLING:START[SQL Server]")
    End Sub

    ''' <summary>
    ''' 変更監視を停止する（フォーム終了時などに呼び出す）
    ''' </summary>
    Public Sub StopMonitoring()
        If trackingTimer IsNot Nothing Then
            trackingTimer.Stop()
            trackingTimer.Dispose()
            trackingTimer = Nothing
            log.cLog("POLLING:STOP[SQL Server]")
        End If
    End Sub

    ''' <summary>
    ''' SQL Server の CHANGETABLE を使って前回以降の変更を取得し、DataGridView に反映する
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

                ' 変更が検出された場合
                If changesTable.Rows.Count > 0 Then
                    log.cLog("POLLING:変更検出")
                    For Each kvp In dgvMapping
                        Dim dgv As DataGridView = kvp.Key
                        Dim transformFunc As Func(Of DataTable, DataTable) = kvp.Value

                        ' 現在のDataGridViewのデータソース（DataTable）を取得
                        Dim currentTable As DataTable = TryCast(dgv.DataSource, DataTable)
                        If currentTable Is Nothing Then Continue For

                        ' 変換関数で変更分を整形
                        Dim updatedTable As DataTable = transformFunc(changesTable)

                        ' UIスレッドで更新処理を実行（ハンドル有無などチェック）
                        If dgv.IsHandleCreated AndAlso Not dgv.Disposing AndAlso Not dgv.IsDisposed Then
                            dgv.Invoke(Sub()
                                           For Each updatedRow As DataRow In updatedTable.Rows
                                               Dim key As String = updatedRow("FKD01").ToString()
                                               Dim targetRow As DataGridViewRow = Nothing

                                               If dgv.Columns.Contains("FKD01") Then
                                                   targetRow = dgv.Rows.Cast(Of DataGridViewRow)().FirstOrDefault(Function(r) r.Cells("FKD01").Value?.ToString() = key)
                                               Else
                                                   log.cLog("Column 'FKD01' not found in DataGridView: " & dgv.Name)
                                                   Continue For
                                               End If

                                               If targetRow IsNot Nothing Then
                                                   ' 既存行を更新
                                                   For Each col As DataColumn In updatedTable.Columns
                                                       ' SYS_CHANGE_OPERATION などシステム列や、現在のDataTableに存在しない列はスキップする
                                                       If col.ColumnName = "SYS_CHANGE_OPERATION" Then Continue For
                                                       If dgv.Columns.Contains(col.ColumnName) AndAlso CType(dgv.DataSource, DataTable).Columns.Contains(col.ColumnName) Then
                                                           targetRow.Cells(col.ColumnName).Value = updatedRow(col.ColumnName)
                                                       End If
                                                   Next
                                               Else
                                                   ' 新しい行を追加
                                                   Dim newRow As DataRow = currentTable.NewRow()
                                                   For Each col As DataColumn In updatedTable.Columns
                                                       If col.ColumnName = "SYS_CHANGE_OPERATION" Then Continue For
                                                       If currentTable.Columns.Contains(col.ColumnName) Then
                                                           newRow(col.ColumnName) = updatedRow(col.ColumnName)
                                                       End If
                                                   Next
                                                   currentTable.Rows.Add(newRow)
                                               End If
                                           Next
                                           dgv.Refresh() ' UI更新
                                       End Sub)

                        Else
                            log.cLog("DataGridView ハンドルが無効: " & dgv.Name)
                        End If
                    Next
                End If
            End If

            ' 次回以降の検出のため、同期バージョンを更新
            lastSyncVersion = currentVersion
        End Using
    End Sub
End Class
