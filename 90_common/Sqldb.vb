Imports System.Data.SqlClient
Imports System.Data.SQLite
Imports System.IO
Imports AdvanceSoftware.PDF.Drawing.EMF.Records
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Office.Word

Public Class Sqldb
    Private ReadOnly log As New Log
    Private ReadOnly mtx As New Mutex
    Private ReadOnly cmn As New Common

    ' カレントパス
    Public ReadOnly CurrentPath_LO As String = SC.CurrentAppPath    ' ローカル
    Public ReadOnly CurrentPath_SV As String                        ' サーバー　SCA1で参照

    ' 区切り文字
    Public Const DELIMITER As String = "#"
    ' DBファイル名
    Public Const DB_FKSC As String = "FKSC.db3"
    Public Const DB_FKSCLOG As String = "FKSC_LOG.db3"
    Public Const DB_FKSCPI As String = "FKSC_PINFO.db3"
    Public Const DB_FKSCFPI As String = "FKSC_FPINFO.db3"
    Public Const DB_FKSCASSIST As String = "FKSC_ASSIST.db3"
    Public Const DB_AUTOCALL As String = "FKSC_AutoCall.db3"
    Public Const DB_MNGREQ As String = "FKGA_MngRequest.db3"
    Public Const DB_MRITEM As String = "FKGA_MRItem.db3"
    Public Const DB_USERLIST As String = "MNG_UserList.db3"
    Public Const DB_OVERTAX As String = "FKSC_OverTax.db3"
    ' テーブル名
    Public Const TBL_FKSC As String = "FKSC"
    Public Const TBL_FKSCREM As String = "FKSCREM"
    Public Const TBL_FKSCD As String = "FKSCD"
    Public Const TBL_ITEM As String = "ITEM"
    Public Const TBL_STANDARD As String = "TBL"
    Public Const TBL_DATA As String = "DATA"
    ' DB保管場所の識別子
    Private Const DBLO As Integer = 0       ' ローカルに保管
    Private Const DBSV As Integer = 1       ' サーバーに保管

    ' リトライ回数、間隔
    Private Const RETRYCNT = 50
    Private Const RETRYDELAY = 200

    Private ReadOnly RegIDList As List(Of String)      ' 連続登録用 顧客IDリスト  連続登録時の同じ顧客IDを多重Insert防止

    ' DBテーブル
    '   カラム数の値を増やせば起動時に ColumnInitでDBカラムを増やしている
    '   [ DBファイル名, テーブル名, カラム数, カラムタグ DB保管場所, 共有読み込み対象]     カラム数、カラムタグは使ってない。コマンド生成で使うかも
    Public ReadOnly DBTbl(,) As String = {
        {DB_FKSC, TBL_FKSC, 70, "FK", DBLO, True},
        {DB_FKSCLOG, TBL_FKSCREM, 6, "FKR", DBSV, True},
        {DB_FKSCLOG, TBL_FKSCD, 17, "FKD", DBSV, True},
        {DB_FKSCPI, TBL_STANDARD, 11, "C", DBSV, True},
        {DB_FKSCFPI, TBL_STANDARD, 15, "C", DBSV, True},
        {DB_FKSCFPI, TBL_DATA, 35, "C", DBSV, True},
        {DB_FKSCFPI, "DATA2", 35, "C", DBSV, True},
        {DB_FKSCFPI, TBL_ITEM, 5, "C", DBSV, True},
        {DB_FKSCPI, TBL_ITEM, 5, "C", DBSV, True},
        {DB_FKSCASSIST, TBL_STANDARD, 28, "C", DBLO, True},
        {DB_AUTOCALL, TBL_STANDARD, 4, "C", DBSV, True},
        {DB_MNGREQ, TBL_STANDARD, 21, "C", DBSV, True},
        {DB_MRITEM, TBL_STANDARD, 5, "C", DBSV, True},
        {DB_USERLIST, TBL_STANDARD, 5, "C", DBSV, True},
        {DB_OVERTAX, TBL_STANDARD, 103, "C", DBSV, True}
    }
    ' DBテーブルのDB種別 SC_DBTableの[ 列数 ]とリンクする必要がある
    Public Enum TID As Integer
        SC = 0       ' FKSC
        SCR          ' FKSCREM
        SCD          ' FKSCD
        PI           ' PINFO
        FPCOS        ' FPINFO 融資物件 顧客情報
        FPDATA       ' FPINFO 融資物件 登録情報
        FPDATA2      ' FPINFO 融資物件 登録情報
        FPITEM       ' FPINFO ITEM
        PIM          ' PINFO MASTER(ITEM)
        SCAS         ' ASSIST
        AC           ' AutoCall
        MR           ' MngRequest 申請物管理
        MRM          ' MngRequest(ITEM)
        USER         ' UserList
        OTAX         ' OverTax
    End Enum

    ' DBテーブルの識別子 SC_DBTableの[ 行数 ]とリンクする必要がある
    Public Enum DBID As Integer
        DBNAME = 0
        TABLE
        CNUM
        CTAG
        DBSTRG
        READTGT
    End Enum

    Public OrgDataTable(DBTbl.GetLength(0) - 1) As DataTable               ' 各DBテーブルのマスターテーブル
    Public OrgDataTablePlusAssist As DataTable                                  ' FKSC+Assist のマスターテーブル
    Public DTLastUpdateTime(DBTbl.GetLength(0) - 1) As DateTime            ' 各DBテーブルの最終更新日
    Private ReadOnly svCon(DBTbl.GetLength(0) - 1) As SQLiteConnection     ' サーバーコネクション
    Private ReadOnly svCmd(DBTbl.GetLength(0) - 1) As SQLiteCommand
    Private ReadOnly loCon(DBTbl.GetLength(0) - 1) As SQLiteConnection     ' ローカルコネクション
    Private ReadOnly loCmd(DBTbl.GetLength(0) - 1) As SQLiteCommand
    Private cmdl() As List(Of String)


    ' コンストラクタ(初期化設定)
    Sub New()
        Dim xml As New XmlMng
        RegIDList = New List(Of String)
        CurrentPath_SV = xml.GetCPath()                             ' サーバーパスをconfigから取得
        Dim DBCSPath = CurrentPath_SV & Common.DIR_DB3
        If DBCSPath.StartsWith("\\") Then DBCSPath = "\\" & DBCSPath        ' ConnectingStringには、\\で始まるアドレスにエスケープ対策で\\を追加する ex \\192 → \\\\192

        ' 各DBの接続パス初期設定
        For n = 0 To DBTbl.GetLength(0) - 1
            svCon(n) = New SQLiteConnection
            svCmd(n) = New SQLiteCommand
            loCon(n) = New SQLiteConnection
            loCmd(n) = New SQLiteCommand

            svCon(n).ConnectionString = "Data Source =" & DBCSPath & DBTbl(n, DBID.DBNAME)          ' サーバー パス設定
            loCon(n).ConnectionString = "Data Source =" & CurrentPath_LO & DBTbl(n, DBID.DBNAME)    ' ローカル パス設定
            svCmd(n).Connection = svCon(n)
            loCmd(n).Connection = loCon(n)
        Next

        Dim typeCount As Integer = [Enum].GetValues(GetType(TID)).Length
        ReDim cmdl(typeCount - 1)
        For i As Integer = 0 To typeCount - 1
            cmdl(i) = New List(Of String)()
        Next

        ' ColumnsInit()
        CreateDBFiles()         ' DBファイルの新規作成
        InitSQLServerConnection()
    End Sub

    ' 新規DBファイルの生成  既にある場合は何もしない
    ' ResourceのDBファイルをコピーする
    Private Sub CreateDBFiles()
        Dim DBPath = CurrentPath_SV & Common.DIR_DB3
        ' 作成するDBリスト
        Dim DBFileList() = {DB_FKSCLOG}
        Dim DBRsrcList() = {My.Resources.FKSC_LOG, My.Resources.FKSC_TODO, My.Resources.HISTORY}

        For n = 0 To DBFileList.Count - 1
            Dim dbf As String = DBFileList(n)
            If File.Exists(DBPath & dbf) Then Continue For              ' 既にDBあれば何もしない
            Using f = File.OpenWrite(DBPath & dbf)
                f.Write(DBRsrcList(n), 0, DBRsrcList(n).Length)         ' 新規DBをコピーする
                log.cLog("[DB作成]:" & DBFileList(n))
            End Using
        Next
    End Sub

    ' 新規DBファイル作成 任意ファイル名  EmptyDB.db3をコピー
    Public Sub CreateDBFiles(fileName As String, filePath As String)
        If Not filePath.EndsWith("\") Then filePath &= "\"
        If File.Exists(filePath & fileName) Then Exit Sub                         ' 既にファイルが存在したらコピーしない
        Using f = File.OpenWrite(filePath & fileName)
            f.Write(My.Resources.EmptyDB, 0, My.Resources.EmptyDB.Length)         ' 新規DBをコピーする
        End Using
    End Sub

    ' 新規DBテーブル作成
    Public Sub CreateDBTables(fileName As String, filePath As String, tblName As String, columnCnt As Integer)
        If columnCnt <= 0 Then
            MsgBox("CreateDBTables: columnCntが0なのでテーブル作成が出来ませんでした。")
            Exit Sub
        End If

        Dim con As New SQLiteConnection
        Dim cmd As New SQLiteCommand
        If Not filePath.EndsWith("\") Then filePath &= "\"
        con.ConnectionString = "Data Source =" & filePath & fileName
        cmd.Connection = con
        Try
            con.Open()
            cmd.CommandText = "Create Table If Not Exists " & tblName & "("
            For n = 1 To columnCnt
                cmd.CommandText += "C" & n.ToString("00") & ","
            Next
            cmd.CommandText = cmd.CommandText.TrimEnd(CType(",", Char)) & ");"      ' 余分なカンマを削除
            cmd.CommandText += "Delete From " & tblName                             ' テーブル内容を初期化
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        End Try
        con.Close()
    End Sub

    ' DBテーブル名の変更 既にある同名テーブルは削除(上書き)
    Public Sub CreateDBTables(fileName As String, filePath As String, srcTblName As String, dstTblName As String)
        Dim con As New SQLiteConnection
        Dim cmd As New SQLiteCommand
        If Not filePath.EndsWith("\") Then filePath &= "\"
        con.ConnectionString = "Data Source =" & filePath & fileName
        cmd.Connection = con
        Try
            con.Open()
            cmd.CommandText = "Drop Table If Exists " & dstTblName & ";"                    ' テーブル削除
            cmd.CommandText += "Alter Table " & srcTblName & " Rename To " & dstTblName     ' リネーム
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        End Try
        con.Close()
    End Sub

    ' SQLコマンド実行(サーバー)
    ' [使い方] 単体 SQLExe(Sqldb.DBSC, "Update FKL ..")  
    '          複数 AddSQL(Sqldb.DBSC, "Insert ..")
    '               SQLExe(Sqldb.DBSC)
    Public Function ExeSQL(TableID As Integer, SqlCmd As String) As Long
        AddSQL(TableID, SqlCmd)
        Dim ret As Long
        If DBTbl(TableID, DBID.DBSTRG) = DBLO Then
            ret = CommonSQLExe(TableID, loCon(TableID), loCmd(TableID))
        Else
            ret = CommonSQLExe(TableID, svCon(TableID), svCmd(TableID))
        End If
        Return ret
    End Function
    Public Function ExeSQL(TableID As Integer) As Long
        Dim ret As Long
        If DBTbl(TableID, DBID.DBSTRG) = DBLO Then
            ret = CommonSQLExe(TableID, loCon(TableID), loCmd(TableID))
        Else
            ret = CommonSQLExe(TableID, svCon(TableID), svCmd(TableID))
        End If
        Return ret
    End Function
    Private Function CommonSQLExe(TableID As Integer, con As SQLiteConnection, cmd As SQLiteCommand) As Long
        'mtx.Lock(Mutex.MTX_LOCK_W, TableID)
        If cmdl(TableID) Is Nothing Then Return -1
        Dim newId As Long = -1 ' 失敗した場合に返す値
        DBFileDL(TableID)
        Try
            con.Open()
            cmd.Transaction = con.BeginTransaction()            ' トランザクション開始
            For Each c In cmdl(TableID)
                cmd.CommandText = c
                log.cLog($"SQLExe: [{[Enum].GetName(GetType(TID), TableID)}] {c}")
                cmd.ExecuteNonQuery()
                ' INSERT文の後にlast_insert_rowid()を実行
                If c.ToUpper.StartsWith("INSERT") Then
                    cmd.CommandText = "SELECT last_insert_rowid()"
                    newId = Convert.ToInt64(cmd.ExecuteScalar())
                End If
            Next
            cmd.Transaction.Commit()                            ' トランザクション終了
            Select Case TableID
                Case TID.PI
                Case Else
                    log.D(Log.DB, cmdl(TableID))                                 ' DBログ書き込み => DB変更通知になる
            End Select
        Catch ex As Exception
            log.D(Log.ERR, ex.Message & vbCrLf & cmd.CommandText)
            newId = -1
            cmd.Transaction.Rollback()
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        End Try
        con.Close()
        cmdl(TableID).Clear()
        RegIDList.Clear()
        'DBFileUP(TableID)
        'mtx.UnLock(TableID)
        'DBFileDL(TableID)
        Return newId
    End Function


    ' SQLコマンド用リスト追加IF
    Public Sub AddSQL(TableID As Integer, SqlCmd As String)
        cmdl(TableID).Add(SqlCmd)
    End Sub

    ' DBファイルダウンロード
    Public Sub DBFileDL(TableID As Integer)
        Dim svPath = CurrentPath_SV & Common.DIR_DB3 & DBTbl(TableID, DBID.DBNAME)
        Dim loPath = CurrentPath_LO & DBTbl(TableID, DBID.DBNAME)
        log.TimerST()
        If Not File.Exists(svPath) Then Exit Sub  ' ファイルがサーバーになければ終了
        If File.GetLastWriteTime(svPath) <> File.GetLastWriteTime(loPath) Then
            File.Copy(svPath, loPath, True)       ' 更新時刻が異なればダウンロード
            log.cLog("DB-DL Comp: " & loPath)
        End If
        'log.TimerED("DBFileDL:" & loPath)
    End Sub

    ' DBファイル強制ダウンロード
    Public Sub DBFileFDL(TableID As Integer)
        Dim svPath = CurrentPath_SV & Common.DIR_DB3 & DBTbl(TableID, DBID.DBNAME)
        Dim loPath = CurrentPath_LO & DBTbl(TableID, DBID.DBNAME)
        If Not File.Exists(svPath) Then
            log.cLog($"DBFileFDL: ファイルが見つからない -> {svPath}")
            Exit Sub  ' ファイルがサーバーになければ終了
        End If
        File.Copy(svPath, loPath, True)
        log.cLog("DB-DL Comp: " & loPath)
    End Sub

    ' DBファイルアップロード
    Public Sub DBFileUP(TableID As Integer)
        Dim svPath = CurrentPath_SV & Common.DIR_DB3 & DBTbl(TableID, DBID.DBNAME)
        Dim loPath = CurrentPath_LO & DBTbl(TableID, DBID.DBNAME)
        If Not File.Exists(loPath) Then
            log.cLog($"DBFileUP: ファイルが見つからない -> {loPath}")
            Exit Sub  ' ファイルがローカルになければ終了
        End If
        If File.GetLastWriteTime(svPath) <> File.GetLastWriteTime(loPath) Then
            File.Copy(loPath, svPath, True)       ' 更新時刻が異なればダウンロード
            log.cLog("Sqldb:DB-UP: " & svPath)
        End If
    End Sub

    ' SQL SELECTでDB読み込み
    Public Function ReadOrgDtSelect(TableID As Integer) As DataTable
        Return ReadOrgDtSelect(TableID, "Select * From " & DBTbl(TableID, DBID.TABLE))
    End Function
    Public Function ReadOrgDtSelect(TableID As Integer, WhereCmd As String) As DataTable

        'mtx.Lock(Mutex.MTX_LOCK_R, TableID)
        DBFileDL(TableID)                               ' ローカルを最新にする
        Dim con As SQLiteConnection = loCon(TableID)
        Dim cmd As SQLiteCommand = loCmd(TableID)
        Dim dt As DataTable = New DataTable()
        Dim dtr As SQLiteDataReader = Nothing
        Dim SqlCmd = WhereCmd
        For cnt = 1 To RETRYCNT
            Try
                con.Open()
                cmd.CommandText = SqlCmd
                dtr = cmd.ExecuteReader                                  ' SQL結果取得

                ' Loadだと遅いから手動で結果を読み込み
                ' DataTableに列設定
                For c As Integer = 1 To DBTbl(TableID, DBID.CNUM)
                    Dim typ As Type = Type.GetType("System.String")
                    Dim clmName As String = DBTbl(TableID, DBID.CTAG) & c.ToString("00")
                    If clmName = "FK55" Then typ = Type.GetType("System.Decimal")       ' 延滞合計額(FK55)はDGVでソートする為にDecimal型に
                    dt.Columns.Add(clmName, typ)
                Next

                ' 取得したSelect結果をDataTableに設定
                log.TimerST()
                If dtr.HasRows Then
                    Dim idx As Integer = 0
                    Dim dr As DataRow
                    While dtr.Read()
                        dr = dt.NewRow()
                        For n = 0 To dtr.FieldCount - 1
                            dr(n) = dtr.Item(n)
                        Next
                        If TableID = TID.SC Then dr(dtr.FieldCount - 2) = idx       ' FKSCにだけ、FK69にDataTableのIndex値を入れる(検索を早くするため)
                        idx += 1
                        dt.Rows.Add(dr)
                    End While
                End If
                'dt.Load(dtr)                                             ' DataTableに読み込み
            Catch ex As Exception
                log.cLog(":::dtr3 " & (dtr Is Nothing))
                log.D(Log.ERR, String.Format("{0}{1}TableID[{2}] {3}", ex.Message, vbCrLf, TableID, SqlCmd))
            End Try
            ' まれに競合更新？でcmd.ExexuteReaderでException発生するので、その場合にはリトライ
            If dtr IsNot Nothing Then Exit For
            Threading.Thread.Sleep(RETRYDELAY)
        Next
        If dtr IsNot Nothing Then
            If Not dtr.IsClosed Then dtr.Close()
        End If
        con.Close()
        'mtx.UnLock(TableID)
        Return dt
    End Function

    ' FKSCREMに指定した機構番号が存在する場合True
    Public Function IsExistREM(id As String) As Boolean
        Dim dt As DataTable = GetSelect(TID.SCR, "Select * From " & DBTbl(TID.SCR, DBID.TABLE) & " Where FKR01 = '" & id & "'")
        Return dt.Rows.Count > 0
    End Function
    ' FKSCDに指定した機構番号が存在する場合True
    Public Function IsExistSCD(id As String) As Boolean
        Dim dt As DataTable = GetSelect(TID.SCD, "Select * From " & DBTbl(TID.SCD, DBID.TABLE) & " Where FKD02 = '" & id & "'")
        Return dt.Rows.Count > 0
    End Function

    ' カラム追加
    Public Sub AddColumns(TableID As String, columnName As String)
        ExeSQL(TableID, "Alter Table " & DBTbl(TableID, DBID.TABLE) & " Add Column " & columnName & " Not Null Default ''")
        'ExeSQL(TableID, "UPDATE " & SC_DBTable(TableID, DBID.TABLE) & " set " & columnName & " = ''")
    End Sub
    Public Sub AddColumns(TableID As String)
        Dim dt As DataTable = GetSelect(TableID, "select * FROM " & DBTbl(TableID, DBID.TABLE) & " limit 1")
        Dim columnName As String = DBTbl(TableID, DBID.CTAG) & (dt.Columns.Count + 1).ToString("00")
        AddColumns(TableID, columnName)
    End Sub

    ' データ削除
    Public Sub DeleteAllData(TableID As String)
        ExeSQL(TableID, $"Delete From {DBTbl(TableID, DBID.TABLE)}")
    End Sub

    ' 汎用SQL文結果取得
    Public Function GetSelect(TableID As Integer, SqlCmd As String) As DataTable
        'mtx.Lock(Mutex.MTX_LOCK_R, TableID)
        Dim con As SQLiteConnection = loCon(TableID)
        Dim cmd As SQLiteCommand = loCmd(TableID)
        Dim dtr As SQLiteDataReader = Nothing
        Dim dt As New DataTable
        DBFileDL(TableID)
        For cnt = 1 To RETRYCNT
            Try
                con.Open()
                cmd.CommandText = SqlCmd
                dtr = cmd.ExecuteReader                                  ' SQL結果取得
                dt.Load(dtr)
            Catch ex As Exception
                log.D(Log.ERR, String.Format("{0}{1}TableID[{2}] {3}", ex.Message, vbCrLf, TableID, SqlCmd))
            End Try
            ' まれに競合更新？でcmd.ExexuteReaderでException発生するので、その場合にはリトライ
            If dtr IsNot Nothing Then Exit For
            Threading.Thread.Sleep(RETRYDELAY)
        Next
        If dtr IsNot Nothing Then
            If Not dtr.IsClosed Then dtr.Close()
        End If
        con.Close()
        'mtx.UnLock(TableID)
        Return dt
    End Function

    ' オリジナルDTの更新
    Public Sub UpdateOrigDT()
        For Each tid In [Enum].GetValues(GetType(TID))
            If DBTbl(tid, DBID.READTGT) Then UpdateOrigDT(tid)         ' SC_DBTableの「読み込み対象」がTrueのものだけを読み込む
        Next
    End Sub
    Public Sub UpdateOrigDT(tid As TID)
        log.cLog($"UpdateOrigDT:{[Enum].GetName(GetType(TID), tid)}")
        cmn.UpdPBar("顧客情報の構築中")
        OrgDataTable(tid) = ReadOrgDtSelect(tid)
        DTLastUpdateTime(tid) = Now     ' 最終更新日を設定
    End Sub

    ' オリジナルDT(アシスト)の更新 FKSC+AssistのDataTableを作成
    ' FKSCに存在したら、アシストのデータ追記。 存在しなければ全データ生成
    Public Sub UpdateOrigDT_ASsist()
        log.cLog("UpdateOrigDT:Assist")

        Dim dt As DataTable = OrgDataTable(TID.SC).Copy
        Dim fk02dt As DataTable = dt.DefaultView.ToTable(False, {"FK02", "FK69"})   ' FKSCから顧客番号(FK02)とIndex値(FK69)のみ抽出
        For Each aRow As DataRow In OrgDataTable(Sqldb.TID.SCAS).Rows     'FKSC_ASSISTを全て参照
            ' 既にDataTableに同じ機構番号が存在するか確認
            Dim dupRows As DataRow() = fk02dt.Select(String.Format("FK02 = '{0}'", aRow.Item(1)))
            If dupRows.Length > 0 Then
                ' 既に存在する  必要項目だけ追加する
                Dim idx As Integer = dupRows(0).Item(1)
                With dt.Rows(idx)
                    .Item(2) = "3"              ' ローン識別子 1=FKのみ 2=アシストのみ 3=FK,アシスト
                    .Item(8) = aRow.Item(11)   ' アシスト 証券番号
                    .Item(58) = aRow.Item(27)   ' アシスト 金消契約日
                    .Item(59) = aRow.Item(23)   ' 貸付金額
                    .Item(60) = aRow.Item(24)   ' 延滞回数
                    .Item(61) = aRow.Item(25)   ' 入金額
                    .Item(62) = aRow.Item(26)   ' 貸付残高
                End With
            Else
                ' 存在しない    DataTableに債権者情報を追加
                Dim newRow As DataRow = dt.NewRow
                With newRow
                    ' 個人情報
                    .Item(0) = aRow.Item(1) & "_2"          ' 機構番号_ローン識別子 ユニーク		ex) 25XXXXXXXXXXXX_1
                    .Item(1) = aRow.Item(1)                 ' 機構番号
                    .Item(2) = "2"                          ' ローン識別子 1=FKのみ 2=アシストのみ 3=FK,アシスト
                    .Item(8) = aRow.Item(11)                ' アシスト 証券番号
                    .Item(9) = aRow.Item(2)                 ' 債務者 氏名
                    .Item(10) = aRow.Item(3)                ' 債務者 ﾖﾐｶﾅ
                    .Item(11) = aRow.Item(4)                ' 債務者 生年月日
                    ' .Item(12) = aRow.Item(1)              ' 債務者 性別
                    .Item(13) = aRow.Item(5)                ' 債務者 TEL1
                    .Item(14) = aRow.Item(6)                ' 債務者 TEL2
                    .Item(15) = aRow.Item(7)                ' 債務者 郵便番号
                    .Item(16) = aRow.Item(8)                ' 債務者 住所 
                    .Item(17) = aRow.Item(9)                ' 債務者 勤務先 
                    .Item(18) = aRow.Item(10)               ' 債務者 勤務先TEL1 
                    .Item(19) = ""                          ' 債務者 勤務先TEL2 は存在しないのでブランク
                    .Item(29) = aRow.Item(12)               ' 連帯債務者 氏名 
                    .Item(30) = aRow.Item(13)               ' 連帯債務者 ﾖﾐｶﾅ 
                    .Item(31) = aRow.Item(14)               ' 連帯債務者 生年月日 
                    ' .Item(32) = aRow.Item(1)              ' 連帯債務者 性別 
                    .Item(33) = aRow.Item(15)               ' 連帯債務者 TEL1 
                    .Item(34) = aRow.Item(16)               ' 連帯債務者 TEL2 
                    .Item(35) = aRow.Item(17)               ' 連帯債務者 郵便番号 
                    .Item(36) = aRow.Item(18)               ' 連帯債務者 住所 
                    .Item(37) = aRow.Item(19)               ' 連帯債務者 勤務先 
                    .Item(38) = aRow.Item(20)               ' 連帯債務者 勤務先TEL1 
                    .Item(39) = ""                          ' 連帯債務者 勤務先TEL2 は存在しないのでブランク
                    ' 債権情報
                    .Item(49) = ""                          ' F35 毎月返済額
                    .Item(50) = ""                          ' F35 残高更新日
                    .Item(51) = ""                          ' F35 延滞回数
                    .Item(52) = ""                          ' F35 ボーナス返済額
                    .Item(53) = ""                          ' F35 更新日残高
                    .Item(54) = 0                           ' F35 延滞合計額
                    .Item(55) = ""                          ' F35 貸付金額
                    .Item(56) = ""                          ' F35 貸付金額(ボーナス)
                    .Item(57) = ""                          ' F35 完済日
                    .Item(58) = aRow.Item(27)               ' アシスト 金消契約日
                    .Item(59) = aRow.Item(23)               ' アシスト 貸付金額
                    .Item(60) = aRow.Item(24)               ' アシスト 延滞回数
                    .Item(61) = aRow.Item(25)               ' アシスト 入金額
                    .Item(62) = aRow.Item(26)               ' アシスト 貸付残高
                    .Item(68) = dt.Rows.Count               ' [後付] DataTableのIndex値
                End With
                dt.Rows.Add(newRow)
            End If
        Next
        OrgDataTablePlusAssist = dt
    End Sub

    ' DB最適化
    Public Function SQLReflesh(TableID As Integer)
        'mtx.Lock(Mutex.MTX_LOCK_W, TableID)
        Dim con As SQLiteConnection = svCon(TableID)
        Dim cmd As SQLiteCommand = svCmd(TableID)
        Dim ret As Boolean = True
        Try
            con.Open()
            cmd.CommandText = "VACUUM"
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        End Try
        con.Close()
        'mtx.UnLock(TableID)
        Return ret
    End Function

    ' 簡易Insert/Update
    ' Insert - 可変引数で、DB定義より不足した項目は空白登録
    ' Update - 可変引数で、DB定義より不足した項目は空白登録
    Public Function ExeSQLInsUpd(TableID As Integer, ParamArray args As String()) As Boolean
        If args.Length = 0 Then Return False
        Dim cid As String = args(0)
        Dim arg As String = Nothing
        Dim cmd As String
        If cid = "" Then Return False

        ' IsExistでDB内のID重複確認、RegIDListで一括登録時のID重複確認
        If (IsExist(TableID, "C01", cid)) Or (RegIDList.Contains(cid)) Then
            ' --- [ Update ]
            ' 可変引数を整形
            ReDim Preserve args(DBTbl(TableID, DBID.CNUM) - 1)
            For n = 1 To args.Length - 1                              ' 引数をこの形式に加工 C02 = 'arg1', C03 = 'arg2',   C01はキーなので更新しない
                arg += String.Format("{0}{1} = '{2}',", DBTbl(TableID, DBID.CTAG), (n + 1).ToString("00"), args(n))
            Next
            arg = cmn.RegReplace(arg, ",$", "")  ' 末尾の カンマ を削除
            cmd = String.Format("Update [{0}] Set {1} Where {2}01 = '{3}'", DBTbl(TableID, DBID.TABLE), arg, DBTbl(TableID, DBID.CTAG), cid)
        Else
            ' --- [ Insert ]
            ' 引数確認
            ' 可変引数を整形
            arg = $"'{String.Join("','", args)}'"           ' 引数をこの形式に加工 'arg0','arg1','arg2'
            For n = 0 To (DBTbl(TableID, DBID.CNUM) - args.Length) - 1
                arg += ",''"        ' 引数がDB数より少ない場合は空白を付与
            Next
            cmd = $"Insert Into [{DBTbl(TableID, DBID.TABLE)}] Values({arg})"
            RegIDList.Add(cid)
        End If
        'ExeSQL(TableID, cmd)
        AddSQL(TableID, cmd)
        Return True
    End Function
    ' 簡易Insert/Update (List型)
    Public Function ExeSQLInsUpd(TableID As Integer, args As List(Of String)) As Boolean
        Return ExeSQLInsUpd(TableID, args.ToArray)
    End Function

    ' 簡易Insert
    Public Function ExeSQLInsert(TableID As Integer, ParamArray args As String()) As Boolean
        If args.Length = 0 Then Return False
        Dim cid As String = args(0)
        Dim arg As String
        Dim cmd As String
        If cid = "" Then Return False

        ' 引数確認
        ' 可変引数を整形
        arg = $"'{String.Join("','", args)}'"           ' 引数をこの形式に加工 'arg0','arg1','arg2'
        For n = 0 To (DBTbl(TableID, DBID.CNUM) - args.Length) - 1
            arg += ",''"        ' 引数がDB数より少ない場合は空白を付与
        Next
        cmd = $"Insert Into [{DBTbl(TableID, DBID.TABLE)}] Values({arg})"
        AddSQL(TableID, cmd)
        Return True
    End Function

    ' 重複する場合True返却
    Public Function IsExist(TableID As String, ColumnName As String, cid As String) As Boolean
        Dim cmd As String = $"Select C01 From {DBTbl(TableID, DBID.TABLE)} Where {ColumnName} = '{cid}'"
        Dim dt As DataTable = GetSelect(TableID, cmd)
        Return dt.Rows.Count > 0
    End Function
    ' 指定カラムの+1の値取得
    Public Function GetNextID(TableID As String, ColumnName As String) As Integer
        Dim num As Integer = 1
        Dim dt As DataTable = GetSelect(TableID, $"Select Max({ColumnName}) From {DBTbl(TableID, DBID.TABLE)}")
        If dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0).Item(0)) Then
            num = CType(dt.Rows(0).Item(0), Integer) + 1
        End If
        Return num
    End Function

    ' 旧DBから新DBへデータ移行する。 PINFO->FPINFO
    Public Sub DataTransferFPINFO()
        ' 現在のデータベースデータを取得
        Dim dt As DataTable = GetSelect(TID.PI, "SELECT * FROM TBL")
        If dt.Rows.Count = 0 Then Exit Sub

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ' もとのデータを一旦削除
        DeleteAllData(TID.FPCOS)  ' テーブル1のデータを削除
        DeleteAllData(TID.FPDATA)  ' テーブル2のデータを削除

        ' テーブル1のIDカウンタ
        Dim TBLIndex As Integer = 1
        Dim ValIndex As Integer = 1

        UpdateOrigDT(TID.SC)

        ' 新たなデータベースに書き換えるため変換
        For Each row As DataRow In dt.Rows
            ' C01取得
            Dim cosNumber As String = row("C01").ToString()
            ' C02の値を抽出して、'`'で分割
            Dim itemsC02 As String() = row("C02").ToString().Split("`"c)

            Dim cosName As String = ""
            Dim cosDt As DataRow() = OrgDataTable(TID.SC).Select($"FK02 = '{cosNumber}'")
            If cosDt.Length > 0 Then
                cosName = cosDt(0)(9)
            End If
            ' FPCOSにデータを挿入
            Dim tblArr As String() = {TBLIndex.ToString, cosNumber.ToString, cosName}.Concat(itemsC02).ToArray
            Dim value As String = $"'{String.Join("','", tblArr)}'"
            ExeSQLInsert(TID.FPCOS, tblArr)

            ' C03からC11までのカラムに対する処理（テーブル2にデータを挿入）
            For i As Integer = 3 To 11
                Dim columnData As String = row($"C{i:D2}")

                If Not String.IsNullOrEmpty(columnData) Then
                    Dim items As String() = columnData.Split(New Char() {"`"c})
                    ' itemsが空だったら登録しない
                    Dim allEmpty As Boolean = True
                    For Each item As String In items
                        If item.Length <> 0 Then
                            allEmpty = False
                            Exit For
                        End If
                    Next
                    ' FPDATAにデータを挿入
                    If Not allEmpty Then
                        tblArr = {ValIndex.ToString, Today.ToString("yyyy/MM/dd 00:00"), TBLIndex.ToString, (i - 3).ToString}.Concat(items).ToArray
                        ExeSQLInsert(TID.FPDATA, tblArr)
                        ValIndex += 1
                    End If
                End If
            Next
            TBLIndex += 1
        Next
        ExeSQL(TID.FPCOS)
        ExeSQL(TID.FPDATA)
    End Sub

    ' データ移行後の項目間調整
    Public Sub DataTransferFPINFO2()
        Dim cmd As String
        Dim regSet As String = ""
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に

        ' FPCOS
        Dim clearColumnsCos As Integer() = {7, 14, 15}
        For Each clm In clearColumnsCos
            regSet += $"C{clm:00} = '',"
        Next
        regSet = cmn.DelLastChar(regSet)    ' 末尾カラム削除
        cmd = $"UPDATE {GetTable(Sqldb.TID.FPCOS)} SET {regSet};"
        ExeSQL(TID.FPCOS, cmd)

        ' FPDATA
        cmd = ""
        Dim changeList As New List(Of Integer())
        changeList.Add({3, 0, 0, 0, 0, 1, 2, 4, 8, 0, 11, 15, 16})                                   ' 任売
        changeList.Add({11, 0, 0, 0, 0, 1, 4, 5, 0, 2, 7, 8, 9, 12, 13, 10})                         ' 競売1
        changeList.Add({11, 0, 0, 0, 0, 1, 4, 5, 0, 2, 7, 8, 9, 12, 13, 10})                         ' 競売2
        changeList.Add({0, 0, 0, 0, 0, 1, 2, 19, 0, 3, 4, 5, 6, 7, 0, 0, 8, 9, 16, 0, 0, 10})   ' 破産(再生1)
        changeList.Add({0, 0, 0, 0, 0, 1, 2, 19, 0, 3, 4, 5, 6, 7, 0, 0, 8, 9, 16, 0, 0, 10})   ' 破産(再生2)
        changeList.Add({0, 0, 0, 0, 0, 1, 2, 19, 0, 3, 4, 5, 6, 7, 0, 0, 8, 9, 16, 0, 0, 10})   ' 破産(再生3)
        changeList.Add({0, 0, 0, 0, 0, 1, 3, 4, 5, 9, 0})                                            ' 差押
        changeList.Add({0, 0, 0, 0, 0, 1, 3, 4, 5, 9, 0})                                            ' 差押
        changeList.Add({0, 0, 0, 0, 0, 1, 3, 4, 5, 9, 0})                                            ' 差押

        ' 旧タイプ(任売,競売1) から 新タイプ(任売,競売,破産)に移行するための、新タイプ置き換えIndex
        Dim SetType As Integer() = {0, 1, 1, 3, 3, 3, 4, 4, 4}

        Dim dt As DataTable
        Dim regData As String
        Dim keyId As String
        For type = 0 To changeList.Count - 1
            ' 大項目ごとにデータ取得
            dt = GetSelect(TID.FPDATA, $"SELECT * FROM {GetTable(TID.FPDATA)} WHERE C04 = '{type}' ORDER BY C01")
            For record = 0 To dt.Rows.Count - 1
                keyId = dt.Rows(record)(2)
                ' 登録日時、顧客キー、大項目種別を設定
                regData = $"'{Today:yyyy/MM/dd 00:00}','{keyId}','{SetType(type)}',"

                For c = 0 To changeList(type).Count - 1
                    Dim idx As Integer = changeList(type)(c)
                    If idx = 0 Then
                        regData += "'',"
                    Else
                        regData += $"'{dt.Rows(record)(idx + 3)}',"
                    End If
                Next
                regData = cmn.DelLastChar(regData)    ' 末尾カラム削除
                cmd += $"INSERT INTO {GetTable(Sqldb.TID.FPDATA2)} ({cmn.GetColumnsStr(2, changeList(type).Count + 4)}) VALUES ({regData});"
            Next
        Next

        ' 移管調整の実行
        ExeSQL(TID.FPDATA2, cmd)
        ' 後処理
        ExeSQL(TID.FPDATA, $"ALTER TABLE {GetTable(TID.FPDATA)} RENAME TO DATA_OLD;")
        ExeSQL(TID.FPDATA, $"ALTER TABLE {GetTable(TID.FPDATA2)} RENAME TO {GetTable(TID.FPDATA)};")
        ExeSQL(TID.FPDATA, $"ALTER TABLE DATA_OLD RENAME TO {GetTable(TID.FPDATA2)};")
        ExeSQL(TID.FPDATA, $"DELETE FROM {GetTable(TID.FPDATA2)}")
    End Sub

    Public Sub DataTranceferMR()
        Dim setdata As New List(Of String())
        setdata.Add(New String() {0, "00", "登録番号", 0, ""})
        setdata.Add(New String() {0, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {0, "02", "番号", 50, ""})
        setdata.Add(New String() {0, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {0, "04", "担当者", 65, ",*"})
        setdata.Add(New String() {0, "05", "顧客番号", 100, ""})
        setdata.Add(New String() {0, "06", "債務者", 85, ""})
        setdata.Add(New String() {0, "07", "A団信加入", 70, "無,有"})
        setdata.Add(New String() {0, "08", "F新旧", 50, "新団信,旧団信"})
        setdata.Add(New String() {0, "09", "F届出内容", 70, "死亡,高度障害,３大疾病,身体障害,介護"})
        setdata.Add(New String() {0, "10", "F審査状況", 110, "案内送付(返送待ち),機構登録・書類送付,確認中(機構連絡有),審査終了"})
        setdata.Add(New String() {0, "11", "F審査完了日", 82, "Cal:Blank"})
        setdata.Add(New String() {0, "12", "F審査結果", 70, "審査待ち,弁済決定,任意売却済,支払不可"})
        setdata.Add(New String() {0, "13", "F完済日", 82, "Cal:Blank"})
        setdata.Add(New String() {0, "14", "A届出内容", 70, "死亡,高度障害,リビ"})
        setdata.Add(New String() {0, "15", "A審査状況", 150, "案内送付(必要書類返送待ち),あいおい審査依頼,調査：確認中(あいおい連絡有),審査終了"})
        setdata.Add(New String() {0, "16", "A審査完了日", 82, "Cal:Blank"})
        setdata.Add(New String() {0, "17", "A審査結果", 70, "審査待ち,弁済決定,任意売却済,支払不可"})
        setdata.Add(New String() {0, "18", "A完済日", 82, "Cal:Blank"})
        setdata.Add(New String() {0, "19", "A団信返金", 70, "有,無"})
        setdata.Add(New String() {0, "20", "備考", 90, ""})
        setdata.Add(New String() {1, "00", "登録番号", 0, ""})
        setdata.Add(New String() {1, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {1, "02", "番号", 50, ""})
        setdata.Add(New String() {1, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {1, "04", "顧客番号", 100, ""})
        setdata.Add(New String() {1, "05", "債務者", 85, ""})
        setdata.Add(New String() {1, "06", "ローン種類", 90, "フラット,アシスト,新保証型,旧保証型,バックアップ,FKローン"})
        setdata.Add(New String() {1, "07", "実施年月", 80, ",1月,2月,3月,4月,5月,6月,7月,8月,9月,10月,11月,12月"})
        setdata.Add(New String() {1, "08", "繰上金額(万円)", 100, "NUM"})
        setdata.Add(New String() {1, "09", "翌月完済<Aのみ入力>", 130, ",翌日完済有"})
        setdata.Add(New String() {1, "10", "申請書返却(登録)日", 120, "Cal:Blank"})
        setdata.Add(New String() {1, "11", "キャンセル日", 110, "Cal:Blank"})
        setdata.Add(New String() {2, "00", "登録番号", 0, ""})
        setdata.Add(New String() {2, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {2, "02", "番号", 50, ""})
        setdata.Add(New String() {2, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {2, "04", "担当者", 60, ",*"})
        setdata.Add(New String() {2, "05", "顧客番号", 100, ""})
        setdata.Add(New String() {2, "06", "債務者", 85, ""})
        setdata.Add(New String() {2, "07", "ローン種類", 82, "フラット,アシスト,新保証型,旧保証型,バックアップ,FKローン"})
        setdata.Add(New String() {2, "08", "アシスト同時完済", 120, "無,有,完済済"})
        setdata.Add(New String() {2, "09", "実行日", 82, "Cal:"})
        setdata.Add(New String() {2, "10", "完済日", 82, "Cal:"})
        setdata.Add(New String() {2, "11", "完済資金", 70, "借換,売却,自己資金,満期完済,団信弁済,FF(S)借換"})
        setdata.Add(New String() {2, "12", "抹消受取", 120, "郵送,司来店スイング),司来店(大阪 営業所),司来店(神戸 営業所),司来店(名古屋 営業所),司来店(仙台 営業所),司来店(柏 営業所),司来店(千葉 営業所),司来店(札幌 営業所),司来店(宇都宮 営業所),司来店(福岡 営業所),司来店(高崎 営業所),司来店(広島 営業所),司来店(高松 営業所),司来店(郡山 営業所),司来店(熊本 営業所),司来店(京都 営業所),司来店(新潟 営業所),司来店(静岡 営業所),司来店(沖縄 営業所),司来店(大宮 営業所),司来店(相模原 営業所),司来店(世田谷 営業所),司来店(吉祥寺 営業所),司来店(横浜 営業所),司来店(所沢 営業所),司来店(銀座 営業所),本人来店(大阪 営業所),本人来店(神戸 営業所),本人来店(名古屋 営業所),本人来店(仙台 営業所),本人来店(柏 営業所),本人来店(千葉 営業所),本人来店(札幌 営業所),本人来店(宇都宮 営業所),本人来店(福岡 営業所),本人来店(高崎 営業所),本人来店(広島 営業所),本人来店(高松 営業所),本人来店(郡山 営業所),本人来店(熊本 営業所),本人来店(京都 営業所),本人来店(新潟 営業所),本人来店(静岡 営業所),本人来店(沖縄 営業所),本人来店(大宮 営業所),本人来店(相模原 営業所),本人来店(世田谷 営業所),本人来店(吉祥寺 営業所),本人来店(横浜 営業所),本人来店(所沢 営業所),本人来店(銀座 営業所),"})
        setdata.Add(New String() {2, "13", "キャンセル日", 90, "Cal:Blank"})
        setdata.Add(New String() {2, "14", "完済証明書希望", 100, "無,有"})
        setdata.Add(New String() {2, "15", "申請書登録日", 90, "Cal:"})
        setdata.Add(New String() {2, "16", "抹消出庫", 60, "未,済,営業所済"})
        setdata.Add(New String() {2, "17", "備考", 82, ""})
        setdata.Add(New String() {3, "00", "登録番号", 0, ""})
        setdata.Add(New String() {3, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {3, "02", "番号", 50, ""})
        setdata.Add(New String() {3, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {3, "04", "担当者", 60, ",*"})
        setdata.Add(New String() {3, "05", "顧客番号", 100, ""})
        setdata.Add(New String() {3, "06", "主債務者名", 85, ""})
        setdata.Add(New String() {3, "07", "条件変更内容", 150, "ボーナス併用,ボーナス取止,ボーナス月変更,増額による期間短縮,元利⇔元利,法定期限内期間延長"})
        setdata.Add(New String() {3, "08", "ステータス", 200, "申請書発送(返送待ち),条件保存(変更契約証書発送),登録待ち,登録完了"})
        setdata.Add(New String() {3, "09", "処理日", 82, "Cal:"})
        setdata.Add(New String() {3, "10", "登録変更予定月", 95, "Cal:Format:yyyy年MM月"})
        setdata.Add(New String() {3, "11", "検印日", 82, "Cal:Blank"})
        setdata.Add(New String() {3, "12", "備考", 120, ""})
        setdata.Add(New String() {4, "00", "登録番号", 0, ""})
        setdata.Add(New String() {4, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {4, "02", "番号", 50, ""})
        setdata.Add(New String() {4, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {4, "04", "担当者", 60, ",*"})
        setdata.Add(New String() {4, "05", "顧客番号", 100, ""})
        setdata.Add(New String() {4, "06", "ローン種類", 110, "フラット,フラット・アシスト,アシスト,新保証型,旧保証型,バックアップ,FKローン"})
        setdata.Add(New String() {4, "07", "依頼書返却日", 100, "Cal:Blank"})
        setdata.Add(New String() {4, "08", "金融機関送付日", 100, "Cal:Blank"})
        setdata.Add(New String() {4, "09", "送付先", 200, ""})
        setdata.Add(New String() {4, "10", "返却日(銀行のみ)", 120, "Cal:Blank"})
        setdata.Add(New String() {5, "00", "登録番号", 0, ""})
        setdata.Add(New String() {5, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {5, "02", "番号", 0, ""})
        setdata.Add(New String() {5, "03", "発送日", 82, "Cal:"})
        setdata.Add(New String() {5, "04", "担当者", 80, ",*"})
        setdata.Add(New String() {5, "05", "発送先", 160, ""})
        setdata.Add(New String() {5, "06", "内容", 290, "完済申請書,一部繰上返済申請書,変更届,支払利息証明書,団信任意脱退届,口座振替用紙（口座変更）,返済予定表,金消コピー,現在残高証明書"})
        setdata.Add(New String() {5, "07", "ローン種類", 100, "フラット,アシスト,FA両方,保証型"})
        setdata.Add(New String() {5, "08", "発送方法", 120, "簡易書留,普通郵便,特定記録,ヤマト便,佐川急便"})
        setdata.Add(New String() {5, "09", "速達", 90, "速達なし,速達あり"})
        setdata.Add(New String() {5, "10", "再鑑者", 80, ",*"})
        setdata.Add(New String() {5, "11", "備考", 120, ""})
        setdata.Add(New String() {6, "00", "登録番号", 0, ""})
        setdata.Add(New String() {6, "01", "カテゴリ", 0, ""})
        setdata.Add(New String() {6, "02", "番号", 0, ""})
        setdata.Add(New String() {6, "03", "受付日", 82, "Cal:"})
        setdata.Add(New String() {6, "04", "担当者", 80, ",*"})
        setdata.Add(New String() {6, "05", "発送元区分", 130, "お客様,司法書士関係,機構,弁護士関係,*"})
        setdata.Add(New String() {6, "06", "発送元名称", 160, ""})
        setdata.Add(New String() {6, "07", "内容", 450, "返済予定表戻り,完済戻り,未開封,口座依頼書,団信脱退届,変更届,一繰戻り,*"})
        setdata.Add(New String() {6, "08", "発送方法", 100, "簡易書留,普通郵便,返付物返却,速達証明,返信用,ヤマト着払"})
        setdata.Add(New String() {6, "09", "受領者", 80, ",*"})

        DeleteAllData(TID.MRM)
        For n = 0 To setdata.Count - 1
            ExeSQLInsert(TID.MRM, setdata(n))
        Next
        ExeSQL(TID.MRM)
        ' カテゴリ6以外は消していいそうなので6以外は削除して、6だけデータ残して全削除
        ExeSQL(TID.MR, $"DELETE FROM {GetTable(TID.MR)} WHERE C02 <> '6'")

        ' カラムを追加
        Dim dt As DataTable = GetSelect(TID.MR, $"SELECT * FROM {GetTable(TID.MR)} LIMIT 1")
        If GetColumCount(TID.MR) > dt.Columns.Count Then
            AddColumns(TID.MR)
        End If
    End Sub

    Public Sub DeleteMRData()
        ' 口座変更だけ削除
        ExeSQL(TID.MR, $"DELETE FROM {GetTable(TID.MR)} WHERE C02 = '4'")
    End Sub

    ' 再生から破産にデータ移行
    Const TR_SAISEI As Boolean = True
    Const TR_HASAN As Boolean = False
    Public Sub TransferSaiseiHasan(sw As Boolean)
        Dim dt As DataTable
        Dim PIIndex As Integer()
        Dim PIID As Integer()
        Dim PICID As String()
        Select Case sw
            Case TR_SAISEI
                PIIndex = {0, 0, 0, 0, 0, 1, 2, 19, 0, 3, 4, 5, 6, 7, 0, 0, 8, 9, 16, 0, 0, 10}                    ' 再生
                PIID = {1396, 1287, 1275, 1067, 1249, 1272, 1069, 1071, 1072, 1230, 1385, 1077, 1297, 1235, 1366, 1290, 1370, 1081, 1213, 1082, 1392, 1084, 1085, 1227, 1300, 1086, 1088, 1089, 1090, 1345, 1092, 1404, 1341, 1095, 1411, 1237, 1267, 1391, 1281, 1097, 1240, 1098, 1099, 1274, 1101, 1106, 1328, 1311, 1252, 1110, 1111, 1414, 1112, 1288, 1351, 1271, 1261, 1116, 1117, 1333, 1358, 1346, 1309, 1277, 1433, 1119, 1214, 1398, 1125, 1128, 1257, 1130, 1266, 1131, 1313, 1293, 1216, 1256, 1389, 1276, 1224, 1357, 1138, 1340, 1232, 1428, 1312, 1438, 1140, 1217, 1304, 1387, 1144, 1145, 1296, 1282, 1148, 1246, 1318, 1228, 1151, 1153, 1279, 1157, 1395, 1394, 1339, 1379, 1383, 1163, 1165, 1273, 1221, 1326, 1166, 1337, 1386, 1241, 1220, 1171, 1239, 1356, 1177, 1302, 1336, 1180, 1226, 1183, 1280, 1314, 1439, 1185, 1292, 1354, 1401, 1421, 1316, 1253, 1368, 1355, 1189, 1283, 1191, 1243, 1429, 1210, 1375, 1306, 1196, 1229, 1322, 1347, 1353, 1332, 1268, 1432, 1342, 1219, 1199, 1422, 1390, 1200, 1423, 1447, 1202, 1425, 1234, 1204, 1405, 1278, 1359, 1262, 1378, 1218, 1325, 1344, 1330, 1291, 1208, 1388, 1397, 1298, 1363, 1374, 1380, 1303, 1367}
                PICID = {"051803220000046", "051910100000021", "250804230001266", "251008040005691", "251102070006561", "251102210010531", "251105160009335", "251109270018544", "251110030022711", "251110050014047", "251203090006324", "251203120008371", "251203260009035", "251203280006023", "251204100007083", "251205230006034", "251206250008812", "251207040004258", "251207100007321", "251207300009017", "251208030005519", "251208080005733", "251208290005756", "251212170001930", "251301280007771", "251302210000989", "251303180000399", "251304010003671", "251305280007995", "251311250005441", "251402240008011", "251404070002073", "251406240005678", "251408260004401", "251408260004401", "251410140001610", "251410160000490", "251503090011351", "251503160007439", "251504200000128", "251504240001770", "251504270006419", "251505070005581", "251505190005147", "251506050006581", "251507240000379", "251508130001000", "251509030002100", "251509170004768", "251511090010127", "251511240001659", "251511240001659", "251512280002794", "251601080002511", "251601080004026", "251602150002505", "251603070001004", "251603070001098", "251603110007971", "251603240007237", "251604080000769", "251605190007553", "251605300010219", "251606020002460", "251606020002460", "251606300000726", "251607210004933", "251609140000965", "251609230002428", "251611070004063", "251611150009258", "251611250000694", "251611280008715", "251612010000698", "251612260015781", "251701130000742", "251701160004088", "251702070001461", "251703090001227", "251703130001125", "251703170005415", "251704100004604", "251704170013173", "251704180002221", "251704270001118", "251704270001118", "251704270002882", "251704270002882", "251705080001450", "251706010006507", "251706050003428", "251707060004344", "251707250005759", "251708240000531", "251709190006718", "251709200002051", "251709250002110", "251710230008370", "251710240004652", "251711160005472", "251711300005224", "251712180006472", "251801110002321", "251801180001862", "251803050001907", "251803150003847", "251803200007212", "251804030003392", "251804090014421", "251804240003941", "251805140007412", "251805170004128", "251805180000206", "251805210003461", "251805230000204", "251806110003522", "251806250009124", "251808210005159", "251808270010558", "251808280008785", "251810150003122", "251810170000200", "251811200010435", "251812200004417", "251901280008636", "251901290002584", "251903110003991", "251903250007182", "251903260011697", "251904090004201", "251904090004201", "251904120005757", "251904220016238", "251905200018608", "251905280005869", "251907090012241", "251907110001338", "251908150000705", "251908300000012", "251909100009561", "251909300012345", "251910180004344", "251910240001397", "251911070008432", "251911070008432", "251911110005773", "251912240012085", "252001100001571", "252003020004422", "252003050007730", "252003060008101", "252003230002779", "252003270001242", "252005110006242", "252005180016255", "252005180016255", "252007060005589", "252007140002080", "252007160001330", "252007160001330", "252007310008458", "252008110007782", "252008110007782", "252008110007782", "252008200003352", "252008200003352", "252008280000061", "252009280003557", "252010050017690", "252010230001856", "252010270002357", "252011060002430", "252012140015307", "252012220013874", "252101070005481", "252103080007637", "252104150001223", "252104230002048", "252106170002892", "252107080000708", "252107300007765", "252110150001511", "252201200000987", "252204050003336", "252205310003321", "252208090000071", "252210140000136"}
            Case Else
                PIIndex = {0, 0, 0, 0, 0, 1, 2, 19, 0, 0, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 0, 18}  ' 破産
                PIID = {1295, 1066, 1334, 1382, 1308, 1068, 1407, 1400, 1245, 1364, 1443, 1070, 1233, 1212, 1305, 1073, 1408, 1074, 1409, 1075, 1076, 1078, 1079, 1080, 1329, 1083, 1348, 1087, 1410, 1444, 1263, 1091, 1248, 1403, 1223, 1093, 1094, 1096, 1399, 1100, 1102, 1307, 1103, 1104, 1105, 1412, 1445, 1107, 1247, 1108, 1413, 1406, 1109, 1343, 1377, 1113, 1114, 1115, 1258, 1270, 1360, 1118, 1120, 1121, 1415, 1122, 1123, 1124, 1255, 1430, 1236, 1251, 1289, 1434, 1244, 1126, 1127, 1129, 1132, 1416, 1133, 1134, 1135, 1136, 1137, 1139, 1317, 1141, 1372, 1142, 1242, 1299, 1436, 1448, 1143, 1211, 1426, 1146, 1147, 1284, 1149, 1150, 1215, 1152, 1154, 1155, 1156, 1158, 1319, 1440, 1159, 1250, 1160, 1417, 1161, 1162, 1164, 1167, 1168, 1371, 1435, 1362, 1169, 1418, 1446, 1170, 1260, 1431, 1172, 1338, 1173, 1174, 1175, 1176, 1419, 1222, 1178, 1301, 1179, 1310, 1437, 1181, 1182, 1184, 1420, 1349, 1352, 1384, 1269, 1402, 1186, 1187, 1188, 1264, 1190, 1365, 1376, 1192, 1193, 1320, 1441, 1194, 1195, 1327, 1285, 1259, 1197, 1324, 1369, 1198, 1361, 1225, 1254, 1201, 1424, 1203, 1238, 1205, 1286, 1350, 1294, 1206, 1335, 1442, 1207, 1393, 1331, 1209, 1321, 1323, 1231, 1427, 1265, 1381}
                PICID = {"250706180001616", "250908120002411", "251002120005681", "251006150009113", "251103230010004", "251104180008981", "251104180008981", "251105020007305", "251106080006150", "251107060004664", "251107060004664", "251107190011032", "251108220010550", "251109210014127", "251111160002513", "251112260007523", "251112260007523", "251112290001957", "251112290001957", "251201160006569", "251202220005405", "251204160009015", "251206130005883", "251206200004538", "251207130006381", "251208070007270", "251302220004278", "251302260005828", "251302260005828", "251302260005828", "251304250004701", "251307290007773", "251309020002015", "251312240001280", "251403240008559", "251404140005971", "251406300001479", "251409290000615", "251503060007836", "251505210001265", "251506080000998", "251507020001082", "251507100002336", "251507160000666", "251507210000176", "251507210000176", "251507210000176", "251508130002235", "251508170000101", "251508310008720", "251508310008720", "251509240002807", "251511060006601", "251512010005766", "251601080004182", "251601280004259", "251602120003821", "251602190000112", "251602230005608", "251603070008289", "251604260008903", "251604280000894", "251607040001432", "251607150000686", "251607150000686", "251608010001688", "251608080014177", "251608220003172", "251608260009741", "251608260009741", "251609050004104", "251609080008752", "251609260002230", "251609260002230", "251610030003033", "251610170003885", "251610280000376", "251611170000876", "251612120002636", "251612120002636", "251612150000887", "251612270002542", "251701190002156", "251702070001592", "251702140001766", "251705020004380", "251705080011935", "251705120000149", "251705220003131", "251705230011765", "251706130008817", "251706230007871", "251706230007871", "251706230007871", "251706290003287", "251708030006193", "251708030006193", "251709040003209", "251709080003251", "251709280009091", "251711060001173", "251711100002761", "251712080001631", "251712110002508", "251712250002039", "251801110000719", "251801150011070", "251801290003011", "251802050004207", "251802050004207", "251802220000690", "251804020001931", "251804130004889", "251804130004889", "251804160000253", "251804230003841", "251805110003494", "251805240001033", "251805280005226", "251805310006136", "251806070001141", "251808060010462", "251808270003748", "251808270003748", "251808270003748", "251808270013611", "251809070007896", "251809070007896", "251809110011376", "251810050007461", "251810300002444", "251810300011656", "251811010005767", "251811020008734", "251811020008734", "251811130005217", "251811270005635", "251812110009495", "251812130000493", "251812130006670", "251812130006670", "251903050004481", "251903110006299", "251904010004641", "251904010004641", "251904150003922", "251905160007511", "251905200012553", "251907010001409", "251907080001050", "251907090012241", "251907220005770", "251907230012744", "251908200002889", "251910110004027", "251910210001119", "251910280005351", "251911110003894", "251911120009852", "251912090001024", "251912090001024", "252001310004011", "252002120003691", "252004020002142", "252004130002031", "252004210009351", "252004210010936", "252005190010861", "252005280001578", "252005290000531", "252007170005028", "252007270010996", "252008110003396", "252008110007812", "252008110007812", "252009230006901", "252010120016361", "252010190008880", "252010270014690", "252011090004546", "252011160001826", "252102150013772", "252103250000743", "252103250000743", "252105140006387", "252107020002119", "252107260007654", "252108100021789", "252110080003642", "252110210001903", "252110220007343", "252110220007343", "252202240007956", "252211280002601"}
        End Select

        For n = 0 To PICID.Length - 1
            dt = GetSelect(TID.PI, $"SELECT C06, C07, C08 FROM {GetTable(Sqldb.TID.PI)} WHERE C01 = '{PICID(n)}';")
            If dt.Rows.Count = 0 Then
                log.cLog($"PI Nothing : {PICID(n)}")
                Continue For
            End If

            Dim PIData As String() = dt(0)(0).Split("`"c)
            ' 2件目のデータがあった場合、再生2のデータを取得
            If n > 1 AndAlso PICID(n) = PICID(n - 2) Then
                log.cLog($" ## 3件データあり : {PICID(n)} ")
                PIData = dt(0)(2).Split("`"c)
            End If
            If n > 0 AndAlso PICID(n) = PICID(n - 1) Then
                PIData = dt(0)(1).Split("`"c)
            End If

            ' PIになかったら登録しない
            Dim allEmpty As Boolean = True
            For Each item As String In PIData
                If item.Length <> 0 Then
                    allEmpty = False
                    Exit For
                End If
            Next
            If allEmpty Then Continue For

            Dim regData As String = ""
            For c = 0 To PIIndex.Count - 1
                Dim idx As Integer = PIIndex(c)
                If idx = 0 Then
                    ' 「元データが空白なら設定しない」時はコメントアウトすること
                    regData += $"C{(5 + c):00} = '',"
                Else
                    regData += $"C{(5 + c):00} = '{PIData(idx - 1)}',"
                End If
            Next
            If sw = TR_HASAN Then
                regData += "C04 = '2',"
            End If
            regData = cmn.DelLastChar(regData)    ' 末尾カラム削除
            AddSQL(TID.FPDATA, $"UPDATE {GetTable(Sqldb.TID.FPDATA)} SET {regData} WHERE C01 = '{PIID(n)}';")
        Next
        ExeSQL(TID.FPDATA)
    End Sub

    ' データベースの最終更新日を確認して、更新直後ならキャッシュがなくLINQを使用したほうが処理速度が早いことを利用するための判定。
    ' Return : True  更新直後ではなくキャッシュあり
    '          False 更新直後　※DB更新から1秒未満
    Public Function CheckDBUpdateCache(tid As Integer) As Boolean
        Dim uptimeDiff As Double = (DateTime.Now - DTLastUpdateTime(tid)).TotalSeconds
        log.cLog($" uptimeDiff({tid}) : {uptimeDiff >= 1} {uptimeDiff}")
        Return uptimeDiff >= 1
    End Function

    ' 物件情報(FPIB) 顧客番号からの顧客キーを取得
    Public Function GetFPCOSKeyId(cid As String) As Integer
        Dim dt As DataTable = GetSelect(Sqldb.TID.FPCOS, $"Select C01 From {DBTbl(Sqldb.TID.FPCOS, Sqldb.DBID.TABLE)} Where C02 = '{cid}'")
        Dim ret As Integer = -1
        If dt.Rows.Count = 1 Then
            ret = dt.Rows(0)(0)
        End If
        Return ret
    End Function

    ' TIDからテーブル名取得
    Public Function GetTable(tid As TID) As String
        Return DBTbl(tid, Sqldb.DBID.TABLE)
    End Function

    ' TIDからカラム数取得
    Public Function GetColumCount(tid As TID) As Integer
        Return DBTbl(tid, Sqldb.DBID.CNUM)
    End Function

    '#### SQL Server関連 #################################################
    Private SqlServerCon As String

    ' SQL Server ConnectionString生成
    Private Sub InitSQLServerConnection()
        ' SQL Server設定
        Dim localPath As String
        Dim serverPath As String

        Dim serverName1 = "MISO"
        Dim serverName2 = "MISO-NOTE\ILCSERVER"
        Dim dbName = "A_SCDB"
        Dim userName = "fls"
        Dim password = "flsuser"

        ' 接続文字列の設定
        serverPath = $"Server={serverName2};Database={dbName};User Id={userName};Password={password};"
        localPath = $"Server={serverName1};Database={dbName};Integrated Security=True;"
        SqlServerCon = localPath

        ' serverPathでの接続テストで成功したらサーバーパスを設定
        'If TestSQLConnection(serverPath) Then SQLServerConnectionStr = serverPath
        log.cLog($"SQL Server connection = '{SqlServerCon}'")
    End Sub

    Private Function TestSQLConnection(ByVal connString As String) As Boolean
        Using conn As New SqlConnection(connString)
            Try
                conn.Open()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Using
    End Function


    Public Sub RestoreSQLServer()
        Dim dt As DataTable

        ' 全てのDBを読み込み、SQL ServerにInsertする
        'For Each value As Sqldb.TID In [Enum].GetValues(GetType(Sqldb.TID))
        Dim value As Sqldb.TID = TID.SCD
        Console.WriteLine(value.ToString & " = " & value.ToString("D"))
        dt = GetSelect(value, $"SELECT * FROM {DBTbl(value, Sqldb.DBID.TABLE)}")
        ExeInsertDataTable(dt, value.ToString)
        'Next
    End Sub

    Public Function ExeInsertDataTable(dt As DataTable, tableName As String) As Boolean
        Using connection As New SqlConnection(SqlServerCon)
            connection.Open()

            ' 新規テーブル作成用のコマンド文字列を生成
            Dim createTableCommandText As String = GenerateCreateTableCommand(dt, tableName)

            ' テーブル作成
            Using createTableCommand As New SqlCommand(createTableCommandText, connection)
                createTableCommand.ExecuteNonQuery()
            End Using

            ' SqlBulkCopyを使用してDataTableのデータを新規テーブルにコピー
            Using bulkCopy As New SqlBulkCopy(connection)
                bulkCopy.DestinationTableName = tableName
                Try
                    bulkCopy.WriteToServer(dt)
                    Return True
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Return False
                End Try
            End Using
        End Using
    End Function

    ' DataTableのスキーマからCREATE TABLEコマンド文字列を生成するメソッド
    Private Function GenerateCreateTableCommand(dt As DataTable, tableName As String) As String
        Dim command As New Text.StringBuilder($"CREATE TABLE [{tableName}] (")
        Dim columnDefinitions As New List(Of String)

        For Each column As DataColumn In dt.Columns
            Dim columnType As String = GetSqlColumnType(column.DataType)
            columnDefinitions.Add($"[{column.ColumnName}] {columnType}")
        Next

        command.Append(String.Join(", ", columnDefinitions))
        command.Append(")")

        Return command.ToString()
    End Function

    ' .NETのデータ型をSQL Serverのデータ型にマッピングするメソッド
    Private Function GetSqlColumnType(type As Type) As String
        If type Is GetType(Integer) Then
            Return "INT"
        ElseIf type Is GetType(String) Then
            Return "NVARCHAR(MAX)"
        ElseIf type Is GetType(Boolean) Then
            Return "BIT"
        ElseIf type Is GetType(DateTime) Then
            Return "DATETIME"
            ' 他のデータ型についても必要に応じてマッピングを追加
        Else
            Return "NVARCHAR(MAX)"
        End If
    End Function

    Public Function SqlServerSelect_Manual(sqlCommand As String) As DataTable
        Dim dt As New DataTable
        Using connection As New SqlConnection(SqlServerCon)
            Try
                connection.Open()
                Using command As New SqlCommand(sqlCommand, connection)
                    Using reader As SqlDataReader = command.ExecuteReader()
                        ' 列の構造を設定する
                        For i As Integer = 0 To reader.FieldCount - 1
                            Dim columnName As String = reader.GetName(i)
                            Dim columnType As Type = reader.GetFieldType(i)
                            dt.Columns.Add(columnName, columnType)
                        Next

                        ' データを読み込む
                        While reader.Read()
                            Dim row As DataRow = dt.NewRow()
                            For i As Integer = 0 To reader.FieldCount - 1
                                row(i) = reader(i)
                            Next
                            dt.Rows.Add(row)
                        End While
                    End Using
                End Using
            Catch ex As Exception
                log.D(Log.ERR, $"SQLServerSelect Error: {sqlCommand}{vbCrLf}{ex.Message}")
            End Try
        End Using
        Return dt
    End Function

    Public Function SqlServerSelect(sqlCommand As String) As DataTable
        Dim dt As New DataTable
        Using connection As New SqlConnection(SqlServerCon)
            Try
                connection.Open()
                Using command As New SqlCommand(sqlCommand, connection)
                    Using adapter As New SqlDataAdapter(command)
                        adapter.Fill(dt)
                    End Using
                End Using
            Catch ex As Exception
                log.D(Log.ERR, $"SQLServerSelect Error: {sqlCommand}{vbCrLf}{ex.Message}")
            End Try
        End Using
        Return dt
    End Function

    Public Function ExeSQLServer(connectionString As String, sqlCommands As List(Of String)) As Long
        Dim newId As Long = -1 ' 失敗した場合またはINSERT文がない場合に返す値
        Using con As New SqlConnection(connectionString)
            con.Open()
            Using tran As SqlTransaction = con.BeginTransaction()
                Try
                    For Each commandText In sqlCommands
                        Using cmd As New SqlCommand(commandText, con, tran)
                            cmd.ExecuteNonQuery()

                            ' INSERT文の後でSCOPE_IDENTITY()を実行してIDを取得
                            If commandText.Trim().ToUpper().StartsWith("INSERT") Then
                                cmd.CommandText = "SELECT SCOPE_IDENTITY()"
                                Dim result As Object = cmd.ExecuteScalar()
                                If result IsNot Nothing Then
                                    newId = Convert.ToInt64(result)
                                End If
                            End If
                        End Using
                    Next
                    tran.Commit()
                    Return newId ' 最後に挿入されたIDを返す
                Catch ex As Exception
                    log.D(Log.ERR, $"ExeSQLServer Error: {sqlCommands(0)}{vbCrLf}{ex.Message}")
                    tran.Rollback()
                    Return newId ' エラーが発生した場合は-1を返す
                End Try
            End Using
        End Using
    End Function

    Public Sub SQLServerSpeedDiff()
        Dim tid As TID = TID.SCD
        log.TimerST()
        ReadOrgDtSelect(tid)
        log.TimerED("SQLite")
        log.TimerST()
        SqlServerSelect_Manual($"SELECT * FROM SCD")
        log.TimerED("SQL Server(手動)")
        log.TimerST()
        SqlServerSelect($"SELECT * FROM SCD")
        log.TimerED("SQL Server(Fill)")
    End Sub

End Class
