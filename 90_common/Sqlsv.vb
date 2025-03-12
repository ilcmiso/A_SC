Imports System.Data.SqlClient

Public Class Sqlsv

    Private ReadOnly log As New Log
    ' SQL Server接続文字列
    Private Shared serverAddr As String
    Private Shared databaseID As String = "A_SC"
    Private Shared UserID As String = "ilc"
    Private Shared UserPW As String = "ilcmng"
    Private Shared connectionString As String

    Public Sub New()
        Dim xml As New XmlMng
        serverAddr = xml.GetSQLSvAddr()
        If serverAddr.Length <= 3 Then serverAddr = "localhost"
        connectionString = $"Server={serverAddr};Database={databaseID};User Id={UserID};Password={UserPW};Encrypt=True;TrustServerCertificate=True;"
    End Sub

    '【SQL Server用 FKSC_LOG専用】接続文字列（指定の条件に合わせています）
    Private Function GetSQLServerFKSCLogConStr() As String
        Return connectionString
    End Function

    '【SQL Server用 FKSC_LOG専用】SELECT実行（DataTable取得）
    Public Function SqlServerSelectFKSCLog(sqlCommand As String) As DataTable
        Dim retryCount As Integer = 0
        Dim dt As DataTable = New DataTable()
        While retryCount < 5
            Using connection As New SqlConnection(GetSQLServerFKSCLogConStr())
                Try
                    connection.Open()
                    Using command As New SqlCommand(sqlCommand, connection)
                        Using adapter As New SqlDataAdapter(command)
                            adapter.Fill(dt)
                        End Using
                    End Using
                    ' 成功したらDataTableを返して終了
                    Return dt
                Catch ex As Exception
                    log.cLog($"SQLServerSelectFKSCLog Error: {sqlCommand}{vbCrLf}{ex.Message}")
                    retryCount += 1
                End Try
            End Using
        End While
        MsgBox($"接続に失敗しました。{vbCrLf}[cmd] {sqlCommand}{vbCrLf}[IP] {serverAddr}")
        Return Nothing
    End Function

    '【SQL Server用 FKSC_LOG専用】非クエリ実行（INSERT/UPDATE/DELETE 等）
    Public Function ExeSQLServerFKSCLog(sqlCommands As List(Of String)) As Long
        Dim newId As Long = -1 ' 失敗時またはINSERTがない場合の戻り値
        Using con As New SqlConnection(GetSQLServerFKSCLogConStr())
            con.Open()
            Using tran As SqlTransaction = con.BeginTransaction()
                Try
                    For Each commandText In sqlCommands
                        Using cmd As New SqlCommand(commandText, con, tran)
                            cmd.ExecuteNonQuery()
                            ' INSERTの場合、SCOPE_IDENTITY()で新規IDを取得
                            If commandText.Trim().ToUpper().StartsWith("INSERT") Then
                                cmd.CommandText = "SELECT SCOPE_IDENTITY()"
                                Dim result As Object = cmd.ExecuteScalar()
                                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                                    newId = Convert.ToInt64(result)
                                End If
                            End If
                        End Using
                    Next
                    tran.Commit()
                    Return newId
                Catch ex As Exception
                    log.D(Log.ERR, $"ExeSQLServerFKSCLog Error: {sqlCommands(0)}{vbCrLf}{ex.Message}")
                    tran.Rollback()
                    Return newId
                End Try
            End Using
        End Using
    End Function

End Class
