Public Class SCA_ProgressBar
    Inherits Form

    Public Shared Instance As SCA_ProgressBar
    Private ReadOnly log As New Log
    Private PrgCount As Integer
    Private PrgMaxCount As Integer

    Public Sub New()
        ' SCA_ProgressBarのインスタンスを設定
        InitializeComponent()
        Instance = Me
        PrgCount = 0
        Me.Visible = False
    End Sub

    Private Sub SCA_ProgressBar_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' フォームの閉じる動作をキャンセル
        e.Cancel = True
        Me.Hide()
    End Sub

    ' プログレスバーを更新するメソッド
    Public Sub StartProgress(progressCount As Integer)
        PrgMaxCount = progressCount
        PrgCount = 0
        ' フォームを表示
        If Not Me.Visible Then Me.Show()
    End Sub

    Public Sub EndProgress()
        ' 進捗が100%に達したらフォームを非表示に
        Me.Hide()
    End Sub

    Public Sub UpdateProgress(loadItemName As String)
        PrgCount += 1
        If PrgMaxCount < PrgCount Then
            EndProgress()
        Else
            UpdateProgress((PrgCount / PrgMaxCount) * 100, loadItemName)
        End If
    End Sub

    Public Sub UpdateProgress(progressValue As Integer, loadItemName As String)
        If progressValue < 0 OrElse progressValue > 100 Then
            Throw New ArgumentException("進捗度は0～100の間でなければなりません。")
        End If

        PBar.Value = progressValue
        Me.Text = loadItemName
        Me.Refresh()
    End Sub

    Public Sub DummyProgress()
        StartProgress(1)
        For n = 1 To 10
            UpdateProgress(n * 10, "情報の読み込み中")
            System.Threading.Thread.Sleep(40)
        Next
        EndProgress()
    End Sub

End Class
