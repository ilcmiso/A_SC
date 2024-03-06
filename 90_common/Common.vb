Imports System.Globalization
Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions

Public Class Common
    ' ディレクトリ名
    Public Const APPNAME As String = "A_SC"

    Public Const DIR_TEL As String = "TELLOG\"                    ' 着信ログファイルのディレクトリ名
    Public Const DIR_DB3 As String = "DB\"                        ' DB3ファイルののディレクトリ名
    Public Const DIR_MTX As String = "DB\Mutex\"                  ' DBの競合管理ディレクトリ名
    Public Const DIR_UPD As String = "Update\"                    ' 更新ファイルのディレクトリ名                               VerUpでも使用する
    Public Const DIR_FLE As String = "File\"                      ' ToDo用アップロードファイルディレクトリ名
    Public Const DIR_LOG As String = "Log\"                       ' ログフォルダ
    Public Const DIR_EXC As String = "Excel\"                     ' Excel格納ディレクトリ名
    Public Const DIR_BKU As String = "Backup\"

    Public Const DIR_EXCFMT1 As String = "01_送付物\"             ' Excel帳票01 送付物ディレクトリ名
    Public Const DIR_EXCFMT2 As String = "02_督促状\"             ' Excel帳票02 督促状ディレクトリ名
    Public Const DIR_EXCFMT3 As String = "03_交渉内容一覧\"       ' Excel帳票03 交渉内容ディレクトリ名
    Public Const DIR_EXCOUT1 As String = "91_送付物出力\"         ' Excel帳票01 送付物出力済みディレクトリ名

    Public Const EXE_NAME As String = "A_SC.exe"                  ' アプリケーション本体ファイル名
    Public Const FILE_TEL As String = "ILC_SCTEL.log"             ' 着信ログファイルのファイル名
    Public Const FILE_EXCBLANK As String = "FMT_Blank.xlsx"       ' Excelブランクフォーマットファイル名
    Public Const FILE_EXCREC As String = "FMT_交渉内容一覧.xlsx"  ' Excel交渉記録一覧ファイル名
    Public Const FILE_CSVREC As String = "FMT_記録一覧CSV.xlsx"   ' CSVl交渉記録ファイル名

    Public Const DUMMY_NO As String = "999999999999999"
    Public Const PARTITION As String = "`"
    Private ReadOnly culture As CultureInfo
    Public ReadOnly CurrentPath As String

    ' 部署一覧
    Public Enum DIV As Integer
        SC = 0       ' 0 債権管理部
        GA           ' 1 総務課
    End Enum

    ' コンストラクタ(初期化設定)
    Sub New()
        Dim xml As New XmlMng
        CurrentPath = xml.GetCPath()
        CreateCurrentDir()         ' 各ディレクトリ生成
        ' CreateDBFiles()     ' 各DBファイル生成
        culture = New CultureInfo("ja-JP", True)
        culture.DateTimeFormat.Calendar = New JapaneseCalendar()
    End Sub

#Region "バックアップ"
    ' デイリーバックアップ実行
    'Public Sub DailyBackupDB()
    '    ' 既にバックアップ済みなら終了(1回だけ)
    '    Dim DailyPath As String = CurrentPath & DIR_BKU & Date.Now.ToString("yyyyMMdd") & "\"
    '    If IO.Directory.Exists(DailyPath) Then Exit Sub
    '    If Not IO.Directory.Exists(DailyPath) Then IO.Directory.CreateDirectory(DailyPath)    ' 出力先ディレクトリ作成
    '    My.Computer.FileSystem.CopyDirectory(CurrentPath & DIR_DB3, DailyPath, True)
    'End Sub
#End Region

    ' 各ディレクトリ生成
    Private Sub CreateCurrentDir()
        Dim dirlist() As String = {     ' 作成ディレクトリリスト
            DIR_TEL, DIR_DB3, DIR_MTX, DIR_UPD, DIR_FLE, DIR_LOG, DIR_EXC, DIR_BKU, (DIR_EXC & DIR_EXCFMT1), (DIR_EXC & DIR_EXCFMT3)
        }
        For Each d In dirlist
            If Not Directory.Exists(CurrentPath & d) Then Directory.CreateDirectory(CurrentPath & d)
        Next
    End Sub

    ' ディレクトリ作成
    Public Sub CreateDir(path)
        If Not Directory.Exists(path) Then Directory.CreateDirectory(path)
    End Sub

    ' Cintのカバー、引数が不正文字列でエラーになった場合0を返す
    Public Function Int(str As String) As Integer
        If str = "" Then Return 0
        Try
            Return CInt(str)
        Catch ex As Exception
            Return 0
        End Try
    End Function

    ' DGVのちらつき防止
    Public Sub SetDoubleBufferDGV(ParamArray DGVList As DataGridView())
        ' ちらつき防止するDGV一覧
        Dim myType As Type = GetType(DataGridView)
        Dim myPropInfo As Reflection.PropertyInfo = myType.GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        For Each dgv In DGVList
            myPropInfo.SetValue(dgv, True, Nothing)
        Next
    End Sub

    ' 和暦変換 (R3.12.31)
    Public Function ConvJPDate(dat As DateTime) As String
        Dim gg As String = ""
        Select Case dat.ToString("gg", culture)
            Case "明治" : gg = "M"
            Case "大正" : gg = "T"
            Case "昭和" : gg = "S"
            Case "平成" : gg = "H"
            Case "令和" : gg = "R"
        End Select
        Return dat.ToString(gg & "y.M.d", culture)
    End Function

    ' 和暦変換 (令和XX,平成XX..)
    Public Function ConvJPYear(dat As DateTime) As String
        Return dat.ToString("ggyy", culture)
    End Function

    ' 文字列を日付(YYYY/MM/DD)形式に変換 20121231 -> 2012/12/31
    Public Function ToDate(str As String) As String
        If str.Length < 8 Then Return ""
        If Format(CInt(str), "0000/00/00").ToString = "0000/00/00" Then Return ""   ' 0000/00/00 ならブランクで返却
        Return Format(CInt(str), "0000/00/00").ToString
    End Function

    ' バイト単位で文字切り出しする
    ' 同じ関数を用意してるのは、同じ文字列(strLine)を再利用できる場合の処理高速化のため。
    Public Function MidB(ByRef strLine As String, ByVal iStart As Integer, ByVal iByteSize As Integer) As String
        Static Dim lastStr As String
        Static Dim btBytes As Byte()
        Static Dim hEncoding As Encoding = Encoding.GetEncoding("Shift_JIS")

        If strLine <> lastStr Then
            lastStr = strLine
            btBytes = hEncoding.GetBytes(strLine)
        End If
        Return hEncoding.GetString(btBytes, iStart - 1, iByteSize)
    End Function

    Public Function MidB1(ByRef strLine As String, ByVal iStart As Integer, ByVal iByteSize As Integer) As String
        Static Dim lastStr As String
        Static Dim btBytes As Byte()
        Static Dim hEncoding As Encoding = Encoding.GetEncoding("Shift_JIS")

        If strLine <> lastStr Then
            lastStr = strLine
            btBytes = hEncoding.GetBytes(strLine)
        End If
        Return hEncoding.GetString(btBytes, iStart - 1, iByteSize)
    End Function

    ' Nullチェック
    Public Sub NullCheck(ByRef row As DataRow)
        Dim cnt = 0
        For Each rdata As Object In row.ItemArray
            If rdata Is DBNull.Value Then row.Item(cnt) = String.Empty
            cnt += 1
        Next
    End Sub

    ' 作業ディレクトリ開く
    Public Sub OpenCurrentDir()
        Process.Start(CurrentPath)
    End Sub

    ' 正規表現
    Public Function RegReplace(word As String, before As String, after As String) As String
        If word Is Nothing Then Return ""
        Return Regex.Replace(word, before, after)
    End Function

    ' ファイル書き込み
    Public Sub WriteFile(saveDir As String, saveFile As String, msg As String)
        Using sr As StreamWriter = New StreamWriter(saveDir & "\" & saveFile, False, Encoding.Default)
            Try
                sr.Write(msg)
            Catch ex As Exception
                MsgBox("ファイル書き込みに失敗。" & vbCrLf &
                       saveDir & "\" & saveFile & vbCrLf &
                       ex.Message, 0 + 16, "")
            Finally
                sr.Close()
            End Try
        End Using
    End Sub

    ' ファイル読み込み
    Public Function ReadFile2List(filePath As String) As List(Of String)
        Dim rList As New List(Of String)
        If Not File.Exists(filePath) Then Return rList

        ' ファイルから1レコード分単位に取得
        Using sr As StreamReader = New StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"))
            ' データ部分を読み込み
            While (sr.Peek() > -1)
                rList.Add(sr.ReadLine())
            End While
        End Using
        Return rList
    End Function

    Public Function JpTimeString(dt As DateTime, format As String) As String
        Dim ci As New CultureInfo("ja-JP")
        Dim jp As New JapaneseCalendar

        ' 現在のカルチャで使用する暦を、和暦に設定します。
        ci.DateTimeFormat.Calendar = jp

        ' 「書式」「カルチャの書式情報」を使用し、文字列に変換します。
        Return dt.ToString(format, ci)
    End Function


    Public Function DialogReadFile(fileName As String, defaultPath As String) As String
        Dim ret As String = ""
        Dim ofd As New OpenFileDialog()
        ofd.FileName = fileName
        ofd.InitialDirectory = defaultPath
        ofd.Filter = "全てのファイル|*.*"
        ' [ファイルの種類]ではじめに選択されるものを指定する 2番目の「すべてのファイル」が選択されているようにする
        'ofd.FilterIndex = 2
        ' タイトルを設定する
        ofd.Title = "読み込むExcelファイルを選択してください"
        ' ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        ofd.RestoreDirectory = True

        ' ダイアログを表示する
        If ofd.ShowDialog() <> DialogResult.OK Then Return ret

        ' ファイルが存在してたら戻り値に返す
        If File.Exists(ofd.FileName) Then ret = ofd.FileName
        Return ret
    End Function
    Public Function DialogReadFile(fileName As String) As String
        Return DialogReadFile(fileName, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
    End Function

    Public Function DialogSaveFile(defaultFName As String) As String
        Dim ret As String = ""
        Dim sfd As New SaveFileDialog()


        sfd.FileName = defaultFName       ' はじめのファイル名
        sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        sfd.Filter = "全てのファイル|*.*"
        sfd.FilterIndex = 2
        sfd.Title = "保存先のファイルを選択してください"
        sfd.RestoreDirectory = True         ' ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        sfd.OverwritePrompt = True          ' 既に存在するファイル名を指定したとき警告する(デフォルトTrue)
        sfd.CheckPathExists = True          ' 存在しないパスが指定されたとき警告を表示する(デフォルトTrue)

        If sfd.ShowDialog() = DialogResult.OK Then ret = sfd.FileName
        Return ret
    End Function

    Public Function DataGridViewClone(dt As DataTable, targetDtgrdvw As DataGridView) As DataTable
        For i = 0 To targetDtgrdvw.ColumnCount - 1
            dt.Columns.Add(targetDtgrdvw.Columns(i).HeaderText)
        Next
        Return dt
    End Function

    ' 特殊漢字の文字化け変換
    Public Function FixSPName(str As String) As String
        Dim chkWords_b() As String = {"", "", "", ""}       ' 特殊漢字(文字化け)
        Dim chkWords_a() As String = {"髙", "𠮷", "釼", "祐"}       ' 通常漢字

        For cn = 0 To chkWords_a.Length - 1
            If str.Contains(chkWords_b(cn)) Then
                str = RegReplace(str, chkWords_b(cn), chkWords_a(cn))
            End If
        Next
        Return str
    End Function

    Public Function GetCurrentDateTime() As String
        Dim currentDateTime As DateTime = DateTime.Now
        Return currentDateTime.ToString("yyyyMMdd_HHmm_ssfff")
    End Function

    Public Function SetValueDefault(ByVal obj As Object, Optional ByVal defaultValue As Object = Nothing) As Object
        If obj Is DBNull.Value Then
            Return defaultValue
        Else
            Return obj
        End If
    End Function

    ' 指定されたカラム名に対応するDataGridViewのカラムインデックスを取得する
    ' カラムインデックス。見つからない場合は-1。
    Public Function GetColumnIndexByName(dgv As DataGridView, columnName As String) As Integer
        For i As Integer = 0 To dgv.Columns.Count - 1
            If dgv.Columns(i).Name.Equals(columnName, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        Return -1 ' カラムが見つからない場合
    End Function


    ' プログレスバー表示
    Public Sub StartPBar(progressCount As Integer)
        SCA_ProgressBar.Instance.StartProgress(progressCount)
    End Sub
    Public Sub UpdPBar(progress As Integer, message As String)
        SCA_ProgressBar.Instance.UpdateProgress(progress, message)
    End Sub
    Public Sub UpdPBar(message As String)
        SCA_ProgressBar.Instance.UpdateProgress(message)
    End Sub
    Public Sub EndPBar()
        SCA_ProgressBar.Instance.EndProgress()
    End Sub
    Public Sub DummyPBar()
        SCA_ProgressBar.Instance.DummyProgress()
    End Sub
End Class
