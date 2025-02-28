Imports System.Xml.Serialization

Public Class XmlMng
    Private ReadOnly xmlSerializer As XmlSerializer
    Public xmlData As XmlList
    Private ReadOnly filePath As String = SC.CurrentAppPath & "A_SC_config.xml"

    ' コンストラクタ
    Public Sub New()
        xmlSerializer = New XmlSerializer(GetType(XmlList))
        ' xmlシリアルするクラスのオブジェクト生成
        xmlData = New XmlList()
        GetXml()
        InitStats()       ' 初期設定
    End Sub

    Public Sub SetXml()
        ' xmlシリアル処理
        Using fileStream As New IO.FileStream(filePath, IO.FileMode.Create)
            xmlSerializer.Serialize(fileStream, xmlData)
        End Using
    End Sub

    Public Sub GetXml()
        ' デシリアル
        If Not IO.File.Exists(filePath) Then Exit Sub
        Using fileStream As New IO.FileStream(filePath, IO.FileMode.Open)
            xmlData = xmlSerializer.Deserialize(fileStream)
        End Using
    End Sub

    Public Function GetCPath() As String
        Select Case xmlData.CPathSW
            Case 1 : Return xmlData.CPath1
            Case 2 : Return xmlData.CPath2
            Case Else : Return ""
        End Select
    End Function

    Public Function GetDebugMode() As Boolean
        Return xmlData.DebugMode
    End Function
    Public Sub SetDebugMode(sw As Boolean)
        xmlData.DebugMode = sw
        SetXml()
    End Sub

    Public Function GetAplUpdOff() As Boolean
        Return xmlData.AplUpdOff
    End Function
    Public Sub SetAplUpdOff(sw As Boolean)
        xmlData.AplUpdOff = sw
        SetXml()
    End Sub

    Private Sub InitStats()
        If xmlData.CPathSW = "" Then xmlData.CPathSW = 1
        If xmlData.CPath1 = "" And xmlData.CPath2 <> "" Then xmlData.CPathSW = 2
    End Sub


    ' 提供IF
    Public Sub SetUserName(uName As String)
        Dim db As Sqldb = SCA1.db
        db.UpdateOrigDT(Sqldb.TID.USER)
        Dim regList As String() = {
            My.Computer.Name,
            "",
            uName,
            GetDiv(),
            ""
        }
        Dim dr As DataRow() = db.OrgDataTable(Sqldb.TID.USER).Select($"C01 = '{My.Computer.Name}'")
        If dr.Length > 0 Then
            ' 既に登録済みユーザーは、ユーザー識別子をそのままの値で設定
            regList(1) = dr(0)(1)       ' 
        Else
            ' 新規ユーザーの場合は、ユーザー識別子を最大値+1の値を設定
            regList(1) = db.GetNextID(Sqldb.TID.USER, "C02").ToString("D5")
        End If

        ' UserListDB更新
        db.ExeSQLInsUpd(Sqldb.TID.USER, regList)
        db.ExeSQL(Sqldb.TID.USER)
        xmlData.UserName = uName
        SetXml()
    End Sub
    Public Function GetUserName() As String
        If xmlData.UserName Is Nothing Then xmlData.UserName = ""
        Return xmlData.UserName
    End Function
    Public Sub SetUserName2(name As String)
        xmlData.UserName2 = name
        SetXml()
    End Sub
    Public Function GetUserName2() As String
        If xmlData.UserName2 Is Nothing Then xmlData.UserName2 = ""
        Return xmlData.UserName2
    End Function

    Public Sub SetAutoUpd(sw As Boolean)
        xmlData.AutoUpdCB = sw
        SetXml()
    End Sub
    Public Function GetAutoUpd() As Boolean
        Return xmlData.AutoUpdCB
    End Function

    Public Sub SetNoticeTell(sw As Boolean)
        xmlData.NoticeTell = sw
        SetXml()
    End Sub
    Public Function GetNoticeTell() As Boolean
        Return xmlData.NoticeTell
    End Function
    Public Sub SetDiv(No As Integer)
        xmlData.DivisionNo = No
        SetXml()
    End Sub
    Public Function GetDiv() As Integer
        Return xmlData.DivisionNo
    End Function
    Public Sub SetDBSwitch(sw As Boolean)
        xmlData.DBSwitch = sw
        SetXml()
    End Sub
    Public Function GetDBSwitch() As Boolean
        Return xmlData.DBSwitch
    End Function
    Public Sub SetSQLSvAddr(addr As String)
        xmlData.SQLSvAddr = addr
        SetXml()
    End Sub
    Public Function GetSQLSvAddr() As String
        Return xmlData.SQLSvAddr
    End Function

End Class

' XMLファイルのシリアライゼーション(メンバー一覧)
Public Class XmlList
    ' つなぎ融資
    Public CPath1 As String     ' カレントパス1
    Public CPath2 As String     ' カレントパス2
    Public CPathSW As String    ' カレントパスのスイッチ1or2
    Public UserName As String   ' PC使用者の名前
    Public UserName2 As String  ' PC使用者の名前2

    Public AutoUpdCB As Boolean  ' 自動更新フラグ
    Public NoticeTell As Boolean ' 受話通知表示のフラグ
    Public DebugMode As Boolean  ' デバッグログ出力モード
    Public AplUpdOff As Boolean  ' アプリ自動更新OFF
    Public DivisionNo As Integer ' 部署番号 0:債権管理部 1:総務課
    Public DBSwitch As Boolean   ' SQLite:False SQL Server:True
    Public SQLSvAddr As String   ' SQL Serverアドレス 
End Class