Imports System.Data.SqlClient

Public Class Sqlsv

    ' SQL Server接続文字列
    Private Shared connectionString As String = "Server=localhost;Database=aaa;User Id=ilc;Password=ilcmng;Encrypt=True;TrustServerCertificate=True;"

    ' SqlConnectionオブジェクト
    Private Shared connection As SqlConnection

    ' SqlConnectionオブジェクトを取得する
    Public Shared Function GetConnection() As SqlConnection
        If connection Is Nothing Then
            connection = New SqlConnection(connectionString)
        End If
        Return connection
    End Function

    ' SQL Server接続を開く
    Public Shared Sub OpenConnection()
        Dim conn As SqlConnection = GetConnection()
        If conn.State <> ConnectionState.Open Then
            Try
                conn.Open()
            Catch ex As Exception
                Throw New Exception("SQL Serverへの接続オープンに失敗しました: " & ex.Message)
            End Try
        End If
    End Sub

    ' SQL Server接続を閉じる
    Public Shared Sub CloseConnection()
        Dim conn As SqlConnection = GetConnection()
        If conn.State <> ConnectionState.Closed Then
            Try
                conn.Close()
            Catch ex As Exception
                Throw New Exception("SQL Server接続のクローズに失敗しました: " & ex.Message)
            End Try
        End If
    End Sub

    ' 非クエリSQLコマンド（INSERT、UPDATE、DELETEなど）を実行する
    Public Shared Function ExecuteNonQuery(ByVal sql As String, Optional ByVal parameters As List(Of SqlParameter) = Nothing) As Integer
        OpenConnection()
        Using cmd As New SqlCommand(sql, GetConnection())
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters.ToArray())
            End If
            Try
                Return cmd.ExecuteNonQuery()
            Catch ex As Exception
                Throw New Exception("SQL ServerのExecuteNonQueryに失敗しました: " & ex.Message)
            Finally
                CloseConnection()
            End Try
        End Using
    End Function

    ' SELECTクエリを実行し、結果をDataTableとして返す
    Public Shared Function ExecuteQuery(ByVal sql As String, Optional ByVal parameters As List(Of SqlParameter) = Nothing) As DataTable
        Dim dt As New DataTable()
        OpenConnection()
        Using cmd As New SqlCommand(sql, GetConnection())
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters.ToArray())
            End If
            Try
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            Catch ex As Exception
                Throw New Exception("SQL ServerのExecuteQueryに失敗しました: " & ex.Message)
            Finally
                CloseConnection()
            End Try
        End Using
        Return dt
    End Function

    ' FKSCDテーブルのデータを取得するサンプルメソッド
    Public Shared Function GetFKSCD() As DataTable
        Dim sql As String = "SELECT * FROM FKSCD"
        Return ExecuteQuery(sql)
    End Function

    ' FKSCREMテーブルのデータを取得するサンプルメソッド
    Public Shared Function GetFKSCREM() As DataTable
        Dim sql As String = "SELECT * FROM FKSCREM"
        Return ExecuteQuery(sql)
    End Function

    ' 必要に応じて、その他のSQL Server操作用メソッドを追加してください

End Class
