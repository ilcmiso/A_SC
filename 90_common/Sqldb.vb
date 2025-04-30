Imports System.Data.SqlClient
Imports System.Data.SQLite
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Public Class Sqldb
    Private ReadOnly log As New Log
    Private ReadOnly mtx As New Mutex
    Private ReadOnly cmn As New Common
    Private ReadOnly sqlsv As New Sqlsv

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
        {DB_FKSCASSIST, TBL_STANDARD, 32, "C", DBLO, True},
        {DB_AUTOCALL, TBL_STANDARD, 4, "C", DBSV, True},
        {DB_MNGREQ, TBL_STANDARD, 22, "C", DBSV, True},
        {DB_MRITEM, TBL_STANDARD, 5, "C", DBSV, True},
        {DB_USERLIST, TBL_STANDARD, 5, "C", DBSV, True},
        {DB_OVERTAX, TBL_STANDARD, 103, "C", DBSV, True}
    }
    ' DBテーブルのDB種別 SC_DBTableの[ 列数 ]とリンクする必要がある
    Public Enum TID As Integer
        SC = 0       '  0 FKSC
        SCR          '  1 FKSCREM
        SCD          '  2 FKSCD
        PI           '  3 PINFO
        FPCOS        '  4 FPINFO 融資物件 顧客情報
        FPDATA       '  5 FPINFO 融資物件 登録情報
        FPDATA2      '  6 FPINFO 融資物件 登録情報 移行で使ったが現在は使われてない
        FPITEM       '  7 FPINFO ITEM
        PIM          '  8 PINFO MASTER(ITEM)
        SCAS         '  9 ASSIST
        AC           ' 10 AutoCall
        MR           ' 11 MngRequest 申請物管理
        MRM          ' 12 MngRequest(ITEM)
        USER         ' 13 UserList
        OTAX         ' 14 OverTax
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
    Public OrgDataTablePlusAssist As DataTable                             ' FKSC+Assist のマスターテーブル
    Public gDGV1SearchCache As DataTable                                   ' DGV1の検索用キャッシュ

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
        'InitSQLServerConnection()
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
        '【変更】対象がFKSC_LOG（TID.SCRまたはTID.SCD）かつスイッチONならSQL Serverへ切替
        If IsSQLServerDB(TableID) Then
            Return sqlsv.ExeSQLServerFKSCLog(New List(Of String) From {SqlCmd})
        Else
            AddSQL(TableID, SqlCmd)
            Dim ret As Long
            If DBTbl(TableID, DBID.DBSTRG) = DBLO Then
                ret = CommonSQLExe(TableID, loCon(TableID), loCmd(TableID))
            Else
                ret = CommonSQLExe(TableID, svCon(TableID), svCmd(TableID))
            End If
            Return ret
        End If
    End Function
    Public Function ExeSQL(TableID As Integer) As Long
        '【変更】対象がFKSC_LOGの場合、コマンドリスト(cmdl)のSQLをSQL Serverで実行
        If IsSQLServerDB(TableID) Then
            Dim ret As Long = sqlsv.ExeSQLServerFKSCLog(cmdl(TableID))
            cmdl(TableID).Clear()
            RegIDList.Clear()
            Return ret
        Else
            Dim ret As Long
            If DBTbl(TableID, DBID.DBSTRG) = DBLO Then
                ret = CommonSQLExe(TableID, loCon(TableID), loCmd(TableID))
            Else
                ret = CommonSQLExe(TableID, svCon(TableID), svCmd(TableID))
            End If
            Return ret
        End If
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
        If IsSQLServerDB(TableID) Then
            Return GetSelect(TableID, WhereCmd)
        End If

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

    '【変更】汎用SQL文結果取得：対象がFKSC_LOGの場合はSQL Serverへ切替
    Public Function GetSelect(TableID As Integer, SqlCmd As String) As DataTable
        If IsSQLServerDB(TableID) Then
            Return sqlsv.SqlServerSelectFKSCLog(SqlCmd)
        End If
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
            If dtr IsNot Nothing Then Exit For
            Threading.Thread.Sleep(RETRYDELAY)
        Next
        If dtr IsNot Nothing Then
            If Not dtr.IsClosed Then dtr.Close()
        End If
        con.Close()
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

    ' オリジナルDTの更新
    Public Sub UpdateOrigDT()
        For Each tid In [Enum].GetValues(GetType(TID))
            If tid = tid.SCD Then Continue For
            If DBTbl(tid, DBID.READTGT) Then UpdateOrigDT(tid)         ' SC_DBTableの「読み込み対象」がTrueのものだけを読み込む
        Next
    End Sub
    Public Sub UpdateOrigDT(tid As TID)
        log.cLog($"UpdateOrigDT:{[Enum].GetName(GetType(TID), tid)}")
        cmn.UpdPBar("顧客情報の構築中")
        Dim tempDt As DataTable = ReadOrgDtSelect(tid)
        OrgDataTable(tid) = tempDt
    End Sub

    ' オリジナルDT(アシスト)の更新 FKSC+AssistのDataTableを作成
    ' FKSCに存在したら、アシストのデータ追記。 存在しなければ全データ生成
    Public Sub UpdateOrigDT_ASsist()
        log.cLog("UpdateOrigDT:Assist")

        Dim dt As DataTable = OrgDataTable(TID.SC).Copy
        ' ダミー顧客を生成
        Dim newr As DataRow = dt.NewRow
        With newr
            .Item(0) = Common.DUMMY_NO & "_000"
            .Item(1) = Common.DUMMY_NO
            .Item(9) = "ダミー"
            .Item(10) = "ダミー"
            .Item(54) = "999999999"
        End With
        dt.Rows.Add(newr)

        Dim fk02dt As DataTable = dt.DefaultView.ToTable(False, {"FK02", "FK69"})   ' FKSCから顧客番号(FK02)とIndex値(FK69)のみ抽出
        For Each aRow As DataRow In OrgDataTable(Sqldb.TID.SCAS).Rows     'FKSC_ASSISTを全て参照
            ' 既にDataTableに同じ機構番号が存在するか確認
            Dim dupRows As DataRow() = fk02dt.Select(String.Format("FK02 = '{0}'", aRow.Item(1)))

            ' DBNullだったら空欄におきかえる
            For i As Integer = 0 To aRow.ItemArray.Length - 1
                If IsDBNull(aRow(i)) Then aRow(i) = ""
            Next

            If dupRows.Length > 0 Then
                ' 既に存在する  必要項目だけ追加する
                Dim idx As Integer = dupRows(0).Item(1)


                With dt.Rows(idx)
                    .Item(2) = "3"              ' ローン識別子 1=FKのみ 2=アシストのみ 3=FK,アシスト
                    .Item(8) = aRow.Item(11)   ' アシスト 証券番号
                    .Item(58) = aRow.Item(27)   ' アシスト 金消契約日
                    .Item(59) = aRow.Item(23)   ' 貸付金額
                    .Item(60) = aRow.Item(24)   ' 延滞回数
                    .Item(61) = aRow.Item(25)   ' 返済額
                    .Item(62) = aRow.Item(26)   ' 貸付残高

                    .Item(21) = aRow.Item(28)   ' アシスト 貸付金額(B) 2025/02/28 add
                    .Item(22) = aRow.Item(29)   ' アシスト 残高(B)     2025/02/28 add
                    .Item(23) = aRow.Item(30)   ' アシスト 返済額(B)   2025/02/28 add
                    .Item(24) = aRow.Item(31)   ' アシスト 完済日      2025/02/28 add
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
                    .Item(61) = aRow.Item(25)               ' アシスト 返済額
                    .Item(62) = aRow.Item(26)               ' アシスト 貸付残高
                    .Item(68) = dt.Rows.Count               ' [後付] DataTableのIndex値

                    .Item(21) = aRow.Item(28)               ' アシスト 貸付金額(B) 2025/02/28 add
                    .Item(22) = aRow.Item(29)               ' アシスト 残高(B)     2025/02/28 add
                    .Item(23) = aRow.Item(30)               ' アシスト 返済額(B)   2025/02/28 add
                    .Item(24) = aRow.Item(31)               ' アシスト 完済日      2025/02/28 add
                End With
                dt.Rows.Add(newRow)
            End If
        Next
        OrgDataTablePlusAssist = dt
    End Sub

    ' DB最適化
    Public Sub SQLReflesh(TableID As Integer)
        Using con As SQLiteConnection = svCon(TableID)
            Using cmd As SQLiteCommand = svCmd(TableID)
                con.Open()
                cmd.CommandText = "VACUUM"
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

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

    ' ユーザーリスト登録
    Public Sub RegUserList(uName As String)
        Dim macAddr As String = cmn.GetMacAddress
        Dim pcName As String = My.Computer.Name
        ' ユーザーデータ取得
        Dim dr() As DataRow = OrgDataTable(Sqldb.TID.USER).Select($"C05 = '{macAddr}'")
        If dr.Length = 0 Then
            ' MACアドレスでHITしない
            dr = OrgDataTable(Sqldb.TID.USER).Select($"C01 = '{pcName}'")
            If dr.Length = 0 Then
                ' 新規ユーザー登録 ユーザー識別子を最大値+1の値を設定
                ExeSQL(Sqldb.TID.USER, $"INSERT INTO {GetTable(Sqldb.TID.USER)} VALUES('{pcName}','{GetNextID(Sqldb.TID.USER, "C02"):D5}','{uName}','{SCA1.xml.GetDiv}','{macAddr}')")
            Else
                ' MACアドレスを追記する
                ExeSQL(Sqldb.TID.USER, $"UPDATE {GetTable(Sqldb.TID.USER)} SET C05 = '{macAddr}' WHERE C02 = '{dr(0)(1)}'")
            End If
        Else
            ' 既に登録済み
            If uName <> dr(0)(2) Then
                ' ユーザー名が変更されていたら更新
                ExeSQL(Sqldb.TID.USER, $"UPDATE {GetTable(Sqldb.TID.USER)} SET C03 = '{uName}' WHERE C05 = '{macAddr}'")
            End If
        End If
        SCA1.xml.SetUserName(uName)
    End Sub

    '#### SQL Server関連 #################################################

    ' 指定したTableIDがSQL Server対象かどうか判定
    Public Function IsSQLServerDB(TableID As TID) As Boolean
        Dim xml As New XmlMng()
        ' SQL Server 対象の TableID 一覧
        Dim sqlServerTargetIDs As TID() = {
            TID.SCR,
            TID.SCD
        }
        Return xml.GetDBSwitch() AndAlso sqlServerTargetIDs.Contains(TableID)
    End Function

End Class
