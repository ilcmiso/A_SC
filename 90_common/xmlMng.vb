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


    Private Sub InitStats()
        If xmlData.CPathSW = "" Then xmlData.CPathSW = 1
        If xmlData.CPath1 = "" And xmlData.CPath2 <> "" Then xmlData.CPathSW = 2
    End Sub


    ' 提供IF
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
End Class