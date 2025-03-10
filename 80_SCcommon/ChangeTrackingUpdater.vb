﻿Imports System.Data.SqlClient
Imports System.Windows.Forms

Public Class ChangeTrackingUpdater
    Private log As New Log
    Private connectionString As String = "Server=localhost;Database=A_SC;User Id=ilc;Password=ilcmng;"
    Private lastSyncVersion As Long = 0

    ' DGVごとのデータ変換処理を保持する辞書
    Private dgvMapping As Dictionary(Of DataGridView, Func(Of DataTable, DataTable))
    Private trackingTimer As Timer  ' ポーリング用タイマー

    ''' <summary>
    ''' コンストラクタ: 反映対象の DataGridView と、それぞれのデータ変換ロジックを受け取る
    ''' </summary>
    ''' <param name="mappings">
    ''' キー: 更新対象の DataGridView
    ''' 値: 変更データの変換関数
    ''' </param>
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
                ' LEFT JOIN を使用して、DELETE イベントも含める
                Dim query As String = "SELECT CT.SYS_CHANGE_OPERATION, CT.FKD01, T.* " &
                                      "FROM CHANGETABLE(CHANGES dbo.FKSCD, @lastSyncVersion) AS CT " &
                                      "LEFT JOIN dbo.FKSCD AS T ON T.FKD01 = CT.FKD01"
                Using cmd As New SqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@lastSyncVersion", lastSyncVersion)
                    Using adapter As New SqlDataAdapter(cmd)
                        adapter.Fill(changesTable)
                    End Using
                End Using

                ' 変更が検出された場合
                If changesTable.Rows.Count > 0 Then
                    log.cLog($"POLLING:変更検出[{lastSyncVersion}]")
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
                                               Dim op As String = updatedRow("SYS_CHANGE_OPERATION").ToString()
                                               Dim key As String = updatedRow("FKD01").ToString()

                                               ' DGV2 の場合のみ、グローバル変数 SCA1.CurrentCID と更新レコードの顧客ID(FKD02) を比較
                                               If dgv.Name = "DGV2" Then
                                                   ' SCA1.CurrentCID が設定されている前提
                                                   If String.IsNullOrEmpty(SCA1.CurrentCID) Then
                                                       Continue For
                                                   End If
                                                   ' DELETE でなければ、更新された顧客IDをチェック
                                                   If op <> "D" Then
                                                       Dim updatedCustomerId As String = updatedRow("FKD02").ToString()
                                                       If updatedCustomerId <> SCA1.CurrentCID Then
                                                           Continue For
                                                       End If
                                                   End If
                                               End If

                                               ' DELETE の場合の処理
                                               If op = "D" Then
                                                   ' DataGridView から該当行を削除するため、DataPropertyName "FKD01" を基準に検索
                                                   Dim colIndex As Integer = GetColumnIndexByDataPropertyName(dgv, "FKD01")
                                                   If colIndex = -1 Then
                                                       log.cLog("DataPropertyName 'FKD01' not found in DataGridView: " & dgv.Name)
                                                       Continue For
                                                   End If
                                                   Dim targetRow As DataGridViewRow = dgv.Rows.Cast(Of DataGridViewRow)() _
                                                       .FirstOrDefault(Function(r) r.Cells(colIndex).Value?.ToString() = key)
                                                   If targetRow IsNot Nothing Then
                                                       ' DataBoundItem は DataRowView であるため、DataRow を削除
                                                       Dim drv As DataRowView = TryCast(targetRow.DataBoundItem, DataRowView)
                                                       If drv IsNot Nothing Then
                                                           currentTable.Rows.Remove(drv.Row)
                                                           log.cLog("POLLING: " & dgv.Name & " 行削除 key=" & key)
                                                       End If
                                                   End If
                                               Else
                                                   ' INSERT / UPDATE の場合
                                                   Dim colIndex As Integer = GetColumnIndexByDataPropertyName(dgv, "FKD01")
                                                   If colIndex = -1 Then
                                                       log.cLog("DataPropertyName 'FKD01' not found in DataGridView: " & dgv.Name)
                                                       Continue For
                                                   End If

                                                   Dim targetRow As DataGridViewRow = dgv.Rows.Cast(Of DataGridViewRow)() _
                                                                                      .FirstOrDefault(Function(r) r.Cells(colIndex).Value?.ToString() = key)

                                                   If targetRow IsNot Nothing Then
                                                       ' 既存行を更新
                                                       For Each col As DataColumn In updatedTable.Columns
                                                           ' システム列はスキップ
                                                           If col.ColumnName = "SYS_CHANGE_OPERATION" Then Continue For
                                                           If dgv.Columns.Contains(col.ColumnName) AndAlso currentTable.Columns.Contains(col.ColumnName) Then
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
                                                       log.cLog("POLLING: " & dgv.Name & " 行追加 key=" & key)
                                                   End If
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

    ' DataPropertyName に一致する列を返すヘルパー関数
    Private Function GetColumnIndexByDataPropertyName(dgv As DataGridView, dataPropName As String) As Integer
        For Each col As DataGridViewColumn In dgv.Columns
            If col.DataPropertyName = dataPropName Then
                Return col.Index
            End If
        Next
        Return -1
    End Function

End Class
