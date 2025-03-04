Imports System.IO
Imports System.Text
Imports System.Data.SQLite
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class SCB1

#Region "定義"
    Private ReadOnly cmn As New Common
    Private ReadOnly db As New Sqldb
    Private ReadOnly log As New Log
    Private ReadOnly id_name As String() = {"1レコード区分", "2機構支店コード", "4現地機構支店コード", "6金融機関コード", "10金融機関支店コード", "13CIF番号", "21顧客番号", "36複合債権顧客番号", "51債権分類コード", "54融資種別コード", "58振込区分", "60約定日", "62増額返済をする月", "66併用償還有無", "67償還方法変更サイン", "68償還方法変更サイン", "69繰上償還請求期限日", "77カナ氏名", "103漢字氏名", "129生年月日", "137郵便番号", "145漢字住所", "305電話番号", "318勤務先カナ名", "358勤務先電話番号", "371団信加入サイン", "372連帯債務者数", "374カナ氏名", "400漢字氏名", "426生年月日", "434同居サイン", "435郵便番号", "443漢字住所", "603電話番号", "616勤務先カナ名", "656勤務先電話番号", "669団信加入サイン", "670保証区分", "671保証委託契約証書番号", "686カナ氏名", "712漢字氏名", "738生年月日", "746郵便番号", "754漢字住所", "914電話番号", "927勤務先カナ名", "967勤務先電話番号", "980被保険者番号", "995保証期限", "1003カナ氏名", "1029漢字氏名", "1055生年月日", "1063団信加入サイン", "1064自ら住居サイン", "1065郵便番号", "1073カナ住所", "1233漢字住所", "1393建物担保区分", "1394土地担保区分", "1395返済終了区分", "1396申込受理日", "1404金消契約年月日", "1412最終回資金交付年月日", "1420金融機関口座支店コード", "1428預金種別コード", "1429口座番号", "1437口座名義人カナ", "1467金融機関口座支店コード", "1475預金種別コード", "1476口座番号", "1484口座名義人カナ", "1514金利口数", "1515完済日", "1523金融機関使用欄", "1543メモ欄", "1623買取区分", "1624フィー設定区分", "1625控除利率", "1630最終フィー情報設定日", "1638初回本設定日", "1646民間団信契約有無", "1647民間団信契約者番号", "1667勘定区分", "1668スペース", "1949識別コード", "1954債権番号", "1974利子補給区分", "1975ステップ（ゆとり）償還区分", "1976断金適用区分・金利変動区分", "1977第２利率開始年月日", "1985最終利率開始年月日", "1993返済終了年月日", "2001据置開始年月日", "2009次回返済年月日", "2017ステップ終了年月日", "2025据置後初回返済年月日", "2033利息償還済", "2041最終返済年月日", "2049次回返済年月日", "2057ステップ終了年月日", "2065据置後初回返済年月日", "2073利息償還済", "2081最終返済年月日", "2089第１利率", "2094第２利率", "2099最終利率", "2104第１割賦金", "2115第２割賦金", "2126第３割賦金", "2137第４割賦金", "2148償還予定回数", "2151償還残回数", "2154第１割賦金", "2165第２割賦金", "2176第３割賦金", "2187第４割賦金", "2198償還予定回数", "2201償還残回数", "2204貸付金額", "2215貸付金残高", "2226未収利息残高", "2237当月回収元金（プラス・マイナス）", "2238当月回収元金", "2248当月回収利息（プラス・マイナス）", "2249当月回収利息", "2259当月回収延滞損害金（プラス・マイナス）", "2260当月回収延滞損害金", "2270次回償還金", "2281次回償還元金", "2292次回償還利息", "2303次々回償還金", "2314次々回償還元金", "2325次々回償還利息", "2336貸付金額", "2347貸付金残高", "2358未収利息残高", "2369当月回収元金（プラス・マイナス）", "2370当月回収元金", "2380当月回収利息（プラス・マイナス）", "2381当月回収利息", "2391当月回収延滞損害金（プラス・マイナス）", "2392当月回収延滞損害金", "2402次回償還金", "2413次回償還元金", "2424次回償還利息", "2435次々回償還金", "2446次々回償還元金", "2457次々回償還利息", "2468延滞月数", "2471延滞金額", "2482繰上延滞月数", "2485繰上延滞元金", "2496繰上延滞利息", "2507現在金利", "2512次回金利", "2517次回金利変動日", "2525現在割賦金", "2536次回割賦金切替日", "2544現在割賦金", "2555次回割賦金切替日", "2563スペース", "2693識別コード", "2698債権番号", "2718利子補給区分", "2719ステップ（ゆとり）償還区分", "2720断金適用区分・金利変動区分", "2721第２利率開始年月日", "2729最終利率開始年月日", "2737返済終了年月日", "2745据置開始年月日", "2753次回返済年月日", "2761ステップ終了年月日", "2769据置後初回返済年月日", "2777利息償還済", "2785最終返済年月日", "2793次回返済年月日", "2801ステップ終了年月日", "2809据置後初回返済年月日", "2817利息償還済", "2825最終返済年月日", "2833第１利率", "2838第２利率", "2843最終利率", "2848第１割賦金", "2859第２割賦金", "2870第３割賦金", "2881第４割賦金", "2892償還予定回数", "2895償還残回数", "2898第１割賦金", "2909第２割賦金", "2920第３割賦金", "2931第４割賦金", "2942償還予定回数", "2945償還残回数", "2948貸付金額", "2959貸付金残高", "2970未収利息残高", "2981当月回収元金（プラス・マイナス）", "2982当月回収元金", "2992当月回収利息（プラス・マイナス）", "2993当月回収利息", "3003当月回収延滞損害金（プラス・マイナス）", "3004当月回収延滞損害金", "3014次回償還金", "3025次回償還元金", "3036次回償還利息", "3047次々回償還金", "3058次々回償還元金", "3069次々回償還利息", "3080貸付金額", "3091貸付金残高", "3102未収利息残高", "3113当月回収元金（プラス・マイナス）", "3114当月回収元金", "3124当月回収利息（プラス・マイナス）", "3125当月回収利息", "3135当月回収延滞損害金（プラス・マイナス）", "3136当月回収延滞損害金", "3146次回償還金", "3157次回償還元金", "3168次回償還利息", "3179次々回償還金", "3190次々回償還元金", "3201次々回償還利息", "3212延滞月数", "3215延滞金額", "3226繰上延滞月数", "3229繰上延滞元金", "3240繰上延滞利息", "3251現在金利", "3256次回金利", "3261次回金利変動日", "3269現在割賦金", "3280次回割賦金切替日", "3288現在割賦金", "3299次回割賦金切替日", "3307スペース", "3437識別コード", "3442債権番号", "3462利子補給区分", "3463ステップ（ゆとり）償還区分", "3464断金適用区分・金利変動区分", "3465第２利率開始年月日", "3473最終利率開始年月日", "3481返済終了年月日", "3489据置開始年月日", "3497次回返済年月日", "3505ステップ終了年月日", "3513据置後初回返済年月日", "3521利息償還済", "3529最終返済年月日", "3537次回返済年月日", "3545ステップ終了年月日", "3553据置後初回返済年月日", "3561利息償還済", "3569最終返済年月日", "3577第１利率", "3582第２利率", "3587最終利率", "3592第１割賦金", "3603第２割賦金", "3614第３割賦金", "3625第４割賦金", "3636償還予定回数", "3639償還残回数", "3642第１割賦金", "3653第２割賦金", "3664第３割賦金", "3675第４割賦金", "3686償還予定回数", "3689償還残回数", "3692貸付金額", "3703貸付金残高", "3714未収利息残高", "3725当月回収元金（プラス・マイナス）", "3726当月回収元金", "3736当月回収利息（プラス・マイナス）", "3737当月回収利息", "3747当月回収延滞損害金（プラス・マイナス）", "3748当月回収延滞損害金", "3758次回償還金", "3769次回償還元金", "3780次回償還利息", "3791次々回償還金", "3802次々回償還元金", "3813次々回償還利息", "3824貸付金額", "3835貸付金残高", "3846未収利息残高", "3857当月回収元金（プラス・マイナス）", "3858当月回収元金", "3868当月回収利息（プラス・マイナス）", "3869当月回収利息", "3879当月回収延滞損害金（プラス・マイナス）", "3880当月回収延滞損害金", "3890次回償還金", "3901次回償還元金", "3912次回償還利息", "3923次々回償還金", "3934次々回償還元金", "3945次々回償還利息", "3956延滞月数", "3959延滞金額", "3970繰上延滞月数", "3973繰上延滞元金", "3984繰上延滞利息", "3995現在金利", "4000次回金利", "4005次回金利変動日", "4013現在割賦金", "4024次回割賦金切替日", "4032現在割賦金", "4043次回割賦金切替日", "4051スペース", "4181識別コード", "4186債権番号", "4206利子補給区分", "4207ステップ（ゆとり）償還区分", "4208断金適用区分・金利変動区分", "4209第２利率開始年月日", "4217最終利率開始年月日", "4225返済終了年月日", "4233据置開始年月日", "4241次回返済年月日", "4249ステップ終了年月日", "4257据置後初回返済年月日", "4265利息償還済", "4273最終返済年月日", "4281次回返済年月日", "4289ステップ終了年月日", "4297据置後初回返済年月日", "4305利息償還済", "4313最終返済年月日", "4321第１利率", "4326第２利率", "4331最終利率", "4336第１割賦金", "4347第２割賦金", "4358第３割賦金", "4369第４割賦金", "4380償還予定回数", "4383償還残回数", "4386第１割賦金", "4397第２割賦金", "4408第３割賦金", "4419第４割賦金", "4430償還予定回数", "4433償還残回数", "4436貸付金額", "4447貸付金残高", "4458未収利息残高", "4469当月回収元金（プラス・マイナス）", "4470当月回収元金", "4480当月回収利息（プラス・マイナス）", "4481当月回収利息", "4491当月回収延滞損害金（プラス・マイナス）", "4492当月回収延滞損害金", "4502次回償還金", "4513次回償還元金", "4524次回償還利息", "4535次々回償還金", "4546次々回償還元金", "4557次々回償還利息", "4568貸付金額", "4579貸付金残高", "4590未収利息残高", "4601当月回収元金（プラス・マイナス）", "4602当月回収元金", "4612当月回収利息（プラス・マイナス）", "4613当月回収利息", "4623当月回収延滞損害金（プラス・マイナス）", "4624当月回収延滞損害金", "4634次回償還金", "4645次回償還元金", "4656次回償還利息", "4667次々回償還金", "4678次々回償還元金", "4689次々回償還利息", "4700延滞月数", "4703延滞金額", "4714繰上延滞月数", "4717繰上延滞元金", "4728繰上延滞利息", "4739現在金利", "4744次回金利", "4749次回金利変動日", "4757現在割賦金", "4768次回割賦金切替日", "4776現在割賦金", "4787次回割賦金切替日", "4795スペース"}
    Private ReadOnly id_st As Integer() = {1, 2, 4, 6, 10, 13, 21, 36, 51, 54, 58, 60, 62, 66, 67, 68, 69, 77, 103, 129, 137, 145, 305, 318, 358, 371, 372, 374, 400, 426, 434, 435, 443, 603, 616, 656, 669, 670, 671, 686, 712, 738, 746, 754, 914, 927, 967, 980, 995, 1003, 1029, 1055, 1063, 1064, 1065, 1073, 1233, 1393, 1394, 1395, 1396, 1404, 1412, 1420, 1428, 1429, 1437, 1467, 1475, 1476, 1484, 1514, 1515, 1523, 1543, 1623, 1624, 1625, 1630, 1638, 1646, 1647, 1667, 1668, 1949, 1954, 1974, 1975, 1976, 1977, 1985, 1993, 2001, 2009, 2017, 2025, 2033, 2041, 2049, 2057, 2065, 2073, 2081, 2089, 2094, 2099, 2104, 2115, 2126, 2137, 2148, 2151, 2154, 2165, 2176, 2187, 2198, 2201, 2204, 2215, 2226, 2237, 2238, 2248, 2249, 2259, 2260, 2270, 2281, 2292, 2303, 2314, 2325, 2336, 2347, 2358, 2369, 2370, 2380, 2381, 2391, 2392, 2402, 2413, 2424, 2435, 2446, 2457, 2468, 2471, 2482, 2485, 2496, 2507, 2512, 2517, 2525, 2536, 2544, 2555, 2563, 2693, 2698, 2718, 2719, 2720, 2721, 2729, 2737, 2745, 2753, 2761, 2769, 2777, 2785, 2793, 2801, 2809, 2817, 2825, 2833, 2838, 2843, 2848, 2859, 2870, 2881, 2892, 2895, 2898, 2909, 2920, 2931, 2942, 2945, 2948, 2959, 2970, 2981, 2982, 2992, 2993, 3003, 3004, 3014, 3025, 3036, 3047, 3058, 3069, 3080, 3091, 3102, 3113, 3114, 3124, 3125, 3135, 3136, 3146, 3157, 3168, 3179, 3190, 3201, 3212, 3215, 3226, 3229, 3240, 3251, 3256, 3261, 3269, 3280, 3288, 3299, 3307, 3437, 3442, 3462, 3463, 3464, 3465, 3473, 3481, 3489, 3497, 3505, 3513, 3521, 3529, 3537, 3545, 3553, 3561, 3569, 3577, 3582, 3587, 3592, 3603, 3614, 3625, 3636, 3639, 3642, 3653, 3664, 3675, 3686, 3689, 3692, 3703, 3714, 3725, 3726, 3736, 3737, 3747, 3748, 3758, 3769, 3780, 3791, 3802, 3813, 3824, 3835, 3846, 3857, 3858, 3868, 3869, 3879, 3880, 3890, 3901, 3912, 3923, 3934, 3945, 3956, 3959, 3970, 3973, 3984, 3995, 4000, 4005, 4013, 4024, 4032, 4043, 4051, 4181, 4186, 4206, 4207, 4208, 4209, 4217, 4225, 4233, 4241, 4249, 4257, 4265, 4273, 4281, 4289, 4297, 4305, 4313, 4321, 4326, 4331, 4336, 4347, 4358, 4369, 4380, 4383, 4386, 4397, 4408, 4419, 4430, 4433, 4436, 4447, 4458, 4469, 4470, 4480, 4481, 4491, 4492, 4502, 4513, 4524, 4535, 4546, 4557, 4568, 4579, 4590, 4601, 4602, 4612, 4613, 4623, 4624, 4634, 4645, 4656, 4667, 4678, 4689, 4700, 4703, 4714, 4717, 4728, 4739, 4744, 4749, 4757, 4768, 4776, 4787, 4795}
    Private ReadOnly id_len As Integer() = {1, 2, 2, 4, 3, 8, 15, 15, 3, 4, 2, 2, 4, 1, 1, 1, 8, 26, 26, 8, 8, 160, 13, 40, 13, 1, 2, 26, 26, 8, 1, 8, 160, 13, 40, 13, 1, 1, 15, 26, 26, 8, 8, 160, 13, 40, 13, 15, 8, 26, 26, 8, 1, 1, 8, 160, 160, 1, 1, 1, 8, 8, 8, 8, 1, 8, 30, 8, 1, 8, 30, 1, 8, 20, 80, 1, 1, 5, 8, 8, 1, 20, 1, 281, 5, 20, 1, 1, 1, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 5, 5, 11, 11, 11, 11, 3, 3, 11, 11, 11, 11, 3, 3, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 3, 11, 3, 11, 11, 5, 5, 8, 11, 8, 11, 8, 130, 5, 20, 1, 1, 1, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 5, 5, 11, 11, 11, 11, 3, 3, 11, 11, 11, 11, 3, 3, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 3, 11, 3, 11, 11, 5, 5, 8, 11, 8, 11, 8, 130, 5, 20, 1, 1, 1, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 5, 5, 11, 11, 11, 11, 3, 3, 11, 11, 11, 11, 3, 3, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 3, 11, 3, 11, 11, 5, 5, 8, 11, 8, 11, 8, 130, 5, 20, 1, 1, 1, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 5, 5, 11, 11, 11, 11, 3, 3, 11, 11, 11, 11, 3, 3, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 10, 1, 10, 1, 10, 11, 11, 11, 11, 11, 11, 3, 11, 3, 11, 11, 5, 5, 8, 11, 8, 11, 8, 130}

    Private ReadOnly GBFILE_LISTCNT = 44                ' GBFile格納リストの要素数
    Private ReadOnly DB_OUTPUT = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) & "\" & Sqldb.DB_FKSC

    Private ReadOnly SCList As New List(Of String)               ' SCデータリスト
    Private Const F35NAME As String = "kokyaku.txt"
    Private Const SEARCH_DAYS As Integer = 30 * 6                  ' 検索日数
    Private Const GE_PATH As String = "X:\"                     ' GEファイルのパス
    Private Const GE_SAVEPATH As String = "ILC\GE\"             ' GEファイルのUSB保存パス
    Private Const GET_FILE_MONTHS As Integer = 6                ' GEフィアルを取得する月数
#End Region

#Region "イベント"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PB_TXT.AllowDrop = True
        PB_CSV.AllowDrop = True
        PB_AC.AllowDrop = True
        PB_OverTax.AllowDrop = True
        ShowLastUpdateTimes()
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
    End Sub

#End Region

    ' DB作成
    Private Sub DBwrite_GB()
        Dim con As New SQLiteConnection()
        Dim cmd As New SQLiteCommand()
        Try
            con.ConnectionString = "Data source = " & db.CurrentPath_LO & Sqldb.DB_FKSC
            cmd.Connection = con
            cmd.CommandText = ""
            con.Open()
            cmd.Transaction = con.BeginTransaction()

            For i = 0 To SCList.Count - GBFILE_LISTCNT Step GBFILE_LISTCNT
                cmd.CommandText = "Replace Into FKSC Values("
                cmd.CommandText += "'" & SCList(i) & "_1'," &                        ' FK01 機構番号_1
                                   "'" & SCList(i) & "'," &                          ' FK02 機構番号
                                   "'1'," &                                          ' FK03 ローン種別(1) F35
                                   "'" & cmn.ToDate(SCList(i + 1)) & "'," &          ' FK04 金消契約日
                                   "'" & cmn.Int(SCList(i + 29)) & "'," &          ' FK05 当月回収元金
                                   "'" & cmn.Int(SCList(i + 30)) & "'," &          ' FK06 次回償還金
                                   "'" & cmn.Int(SCList(i + 31)) & "'," &          ' FK07 次々回償還金
                                   "'" & SCList(i + 32) & "'," &                     ' FK08 増額返済月
                                   "''," &                                           ' FK09
                                   "'" & Trim(SCList(i + 2)) & "'," &                ' FK10 債務者 氏名
                                   "'" & Trim(SCList(i + 3)) & "'," &                ' FK11 債務者 ﾖﾐｶﾅ
                                   "'" & cmn.ToDate(SCList(i + 4)) & "'," &          ' FK12 債務者 生年月日
                                   "''," &                                           ' FK13 債務者 性別(情報なし)
                                   "'" & Trim(SCList(i + 5)) & "'," &                ' FK14 債務者 TEL1
                                   "''," &                                           ' FK15 債務者 TEL2(情報なし)
                                   "'" & Trim(SCList(i + 6)) & "'," &                ' FK16 債務者 郵便番号
                                   "'" & Trim(SCList(i + 7)) & "'," &                ' FK17 債務者 住所 
                                   "'" & Trim(SCList(i + 8)) & "'," &                ' FK18 債務者 勤務先 
                                   "'" & Trim(SCList(i + 9)) & "'," &                ' FK19 債務者 勤務先TEL1
                                   "''," &                                           ' FK20 債務者 勤務先TEL2(情報なし)
                                   "'" & Trim(SCList(i + 37)) & "'," &               ' FK21 債務者 団信加入サイン
                                   "'','','','','','','',''," &                      ' FK20～29
                                   "'" & Trim(SCList(i + 10)) & "'," &               ' FK30 連債者 氏名
                                   "'" & Trim(SCList(i + 11)) & "'," &               ' FK31 連債者 ﾖﾐｶﾅ
                                   "'" & cmn.ToDate(SCList(i + 12)) & "'," &         ' FK32 連債者 生年月日
                                   "''," &                                           ' FK33 連債者 性別(情報なし)
                                   "'" & Trim(SCList(i + 13)) & "'," &               ' FK34 連債者 TEL1
                                   "''," &                                           ' FK35 連債者 TEL2(情報なし)
                                   "'" & Trim(SCList(i + 14)) & "'," &               ' FK36 連債者 郵便番号
                                   "'" & Trim(SCList(i + 15)) & "'," &               ' FK37 連債者 住所 
                                   "'" & Trim(SCList(i + 16)) & "'," &               ' FK38 連債者 勤務先 
                                   "'" & Trim(SCList(i + 17)) & "'," &               ' FK39 連債者 勤務先TEL1
                                   "''," &                                           ' FK40 連債者 勤務先TEL2(情報なし)
                                   "'" & Trim(SCList(i + 38)) & "'," &               ' FK41 連債者 団信加入サイン
                                   "'" & Trim(SCList(i + 39)) & "'," &               ' FK42 償還金口座情報 口座番号
                                   "'" & Trim(SCList(i + 40)) & "'," &               ' FK43 償還金口座情報 口座名義人ｶﾅ
                                   "'" & Trim(SCList(i + 41)) & "'," &               ' FK44 償還金口座情報 金融機関口座コード
                                   "'" & Trim(SCList(i + 42)) & "'," &               ' FK45 償還金口座情報 金融機関口座支店コード
                                   "''," &                                           ' FK46
                                   "'" & SCList(i + 43) & "'," &                     ' FK47 メモ欄
                                   "'" & cmn.Int(SCList(i + 36)) & "'," &          ' FK48 更新日残高(ボーナス)
                                   "'" & cmn.Int(SCList(i + 18)) & "'," &          ' FK49 返済額2(顧客番号05始まり用)
                                   "'" & cmn.Int(SCList(i + 19)) & "'," &          ' FK50 返済額
                                   "'" & cmn.ToDate(SCList(i + 20)) & "'," &         ' FK51 残高更新日
                                   "'" & cmn.Int(SCList(i + 21)) & "'," &          ' FK52 延滞月数
                                   "'" & cmn.Int(SCList(i + 22)) & "'," &          ' FK53 ボーナス返済額
                                   "'" & cmn.Int(SCList(i + 23)) & "'," &          ' FK54 更新日残高
                                   "'" & cmn.Int(SCList(i + 24)) & "'," &          ' FK55 延滞合計額
                                   "'" & cmn.Int(SCList(i + 25)) & "'," &          ' FK56 貸付金額
                                   "'" & cmn.Int(SCList(i + 26)) & "'," &          ' FK57 貸付金額(ボーナス)
                                   "'" & cmn.ToDate(SCList(i + 27)) & "'," &         ' FK58 完済日(使ってない)
                                   "''," &                                           ' FK59 償還残回数(使ってない)
                                   "'','','',''," &                                  ' FK60～63
                                   "'" & SCList(i + 33) & "'," &                     ' FK64 物件情報 住居サイン
                                   "'" & Trim(SCList(i + 34)) & "'," &               ' FK65 物件情報 郵便番号
                                   "'" & Trim(SCList(i + 35)) & "'," &               ' FK66 物件情報 漢字住所
                                   "'','','',''"                                     ' FK67～FK70
                cmd.CommandText += ")"
                cmd.CommandText = cmn.FixSPName(cmd.CommandText)        ' 特殊漢字の文字化け変換
                cmd.ExecuteNonQuery()
            Next
            cmd.Transaction.Commit()
        Catch ex As Exception
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    ' ファイルドラッグ＆ドロップ
    Private Sub ListBox1_DragEnter(sender As Object, e As DragEventArgs) Handles PB_TXT.DragEnter, PB_CSV.DragEnter, PB_AC.DragEnter, PB_OverTax.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            'ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
            e.Effect = DragDropEffects.Copy
        Else
            'ファイル以外は受け付けない
            e.Effect = DragDropEffects.None
        End If
    End Sub
    ' kokyaku.txt D&D
    Private Sub ListBox1_DragDrop(sender As Object, e As DragEventArgs) Handles PB_TXT.DragDrop
        log.TimerST()
        'ドロップされたすべてのファイル名を取得する
        Dim fileName As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())
        If Path.GetFileName(fileName(0)) <> F35NAME Then
            MsgBox("ファイルが違います。" & vbCrLf &
                    F35NAME & " をドラッグ＆ドロップしてください。")
            Exit Sub
        End If
        If CB_A1.Checked Then AnalyzeGBFileDbg(fileName(0)) : Exit Sub      ' 解析用

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        AnalyzeGBFile(fileName(0))                      ' ファイル解析
        DBwrite_GB()                                    ' DB書き込み

        CopyCDBtoServer(Sqldb.DB_FKSC)                  ' DBファイルをローカルからサーバーにコピー
        Close()
        ShowLastUpdateTimes()
        log.TimerED("kokyaku.txt read comp")
    End Sub
    ' アシスト D&D
    Private Sub ListBox2_DragDrop(sender As Object, e As DragEventArgs) Handles PB_CSV.DragDrop
        'ドロップされたすべてのファイル名を取得する
        Dim fileName As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())
        If Path.GetFileName(fileName(0)) = "GE380101" Then
            AnalyzeGEFile(fileName(0))
            Exit Sub
        End If

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ReadAssistData(fileName(0))

        ' CopyCDBtoServer(Sqldb.DB_FKSCASSIST)            ' DBファイルをローカルからサーバーにコピー
        ' Close()
        MsgBox("アシストファイルの読み込みが完了しました。" & vbCrLf &
               "4ファイル全て読み込みが完了したら、下のボタンを押してください。")
        ShowLastUpdateTimes()
    End Sub
    ' オートコール D&D
    Private Sub PB_AC_DragDrop(sender As Object, e As DragEventArgs) Handles PB_AC.DragDrop
        ' ドロップされたファイルのリストを取得
        Dim files As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())

        ' 1つのファイルのみを取り扱う場合
        If files.Length = 1 Then
            ' ファイルをDGVにロード
            Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
            LoadFileToDGV(files(0))
            db.UpdateOrigDT(Sqldb.TID.AC)
            MsgBox("オートコールファイルの読み込みが完了しました。" & vbCrLf & $"オートコールのデータ数は {DGV1.RowCount} 件です。")
        End If
    End Sub
    ' 延滞損害情報 D&D
    Private Sub PB_OverTax_DragDrop(sender As Object, e As DragEventArgs) Handles PB_OverTax.DragDrop
        ' ドロップされたファイルのリストを取得
        Dim files As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())

        Cursor.Current = Cursors.WaitCursor     ' マウスカーソルを砂時計に
        ' 複数ファイルの内容をLoadOverTaxに渡してデータベースに読み込む
        LoadOverTax(files)
        MsgBox(files.Length.ToString() & "個の延滞情報ファイルの読み込みが完了しました。")
    End Sub


    ' ローカルの顧客DBをサーバーにコピー
    Private Sub CopyCDBtoServer(dbFile As String)
        If File.Exists(db.CurrentPath_SV & Common.DIR_UPD & dbFile) Then
            File.Copy(db.CurrentPath_LO & dbFile, db.CurrentPath_SV & Common.DIR_UPD & dbFile, True)      ' サーバーにコピー
            MsgBox("加入者データの更新が完了しました。 [ " & dbFile & " ]")
        Else
            File.Copy(db.CurrentPath_LO & dbFile, DB_OUTPUT, True)                                        ' サーバー設定がないPCはデスクトップにコピー
            MsgBox("加入者データの更新が完了しました。 [ " & dbFile & " ]" & vbCrLf &
                   "デスクトップに、ファイル[ " & dbFile & " ]ができましたので、共有サーバのDBにコピーして下さい。")
        End If
    End Sub

    ' ファイル解析
    Private Sub AnalyzeGBFile(fname As String)
        SCList.Clear()
        ' ファイルから1レコード分単位に取得
        Using sr As StreamReader = New StreamReader(fname, Encoding.GetEncoding("Shift_JIS"))
            ' データ部分を読み込み
            Dim recode As String = ""
            While (sr.Peek() > -1)
                recode = sr.ReadLine()

                ' 返済額2が有効(返済回数が0以上)かを判定して、有効なら返済額2を取得する
                Dim hensai2val As String = 0
                '                If cmn.MidB(recode, 2895, 3) <> "000" Then hensai2val = cmn.MidB(recode, 2848, 11)
                If cmn.MidB(recode, 2895, 3) <> "000" Then hensai2val = cmn.MidB(recode, 3014, 11)      ' 引き落とし金額が違ったと報告があったので修正  第一割賦金 -> 次回償還金

                SCList.Add(cmn.MidB(recode, 21, 15))        ' 01 機構番号
                SCList.Add(cmn.MidB(recode, 1404, 8))       ' 02 金消契約日
                SCList.Add(cmn.MidB(recode, 103, 26))       ' 03 主債務者 氏名漢字
                SCList.Add(cmn.MidB(recode, 77, 26))        ' 04 主債務者 氏名カナ
                SCList.Add(cmn.MidB(recode, 129, 8))        ' 05 主債務者 生年月日
                SCList.Add(cmn.MidB(recode, 305, 13))       ' 06 主債務者 電話番号
                SCList.Add(cmn.MidB(recode, 137, 8))        ' 07 主債務者 郵便番号
                SCList.Add(cmn.MidB(recode, 145, 160))      ' 08 主債務者 漢字住所
                SCList.Add(cmn.MidB(recode, 318, 40))       ' 09 主債務者 勤務先カナ
                SCList.Add(cmn.MidB(recode, 358, 13))       ' 10 主債務者 勤務先電話番号
                SCList.Add(cmn.MidB(recode, 400, 26))       ' 11 連帯者 氏名漢字
                SCList.Add(cmn.MidB(recode, 374, 26))       ' 12 連帯者 氏名カナ
                SCList.Add(cmn.MidB(recode, 426, 8))        ' 13 連帯者 生年月日
                SCList.Add(cmn.MidB(recode, 603, 13))       ' 14 連帯者 電話番号
                SCList.Add(cmn.MidB(recode, 435, 8))        ' 15 連帯者 郵便番号
                SCList.Add(cmn.MidB(recode, 443, 160))      ' 16 連帯者 漢字住所
                SCList.Add(cmn.MidB(recode, 616, 40))       ' 17 連帯者 勤務先カナ
                SCList.Add(cmn.MidB(recode, 656, 13))       ' 18 連帯者 勤務先電話番号
                SCList.Add(hensai2val)                      ' 19 毎月返済額2
                SCList.Add(cmn.MidB(recode, 2270, 11))      ' 20 毎月返済額
                SCList.Add(cmn.MidB(recode, 2033, 8))       ' 21 残高更新日
                SCList.Add(cmn.MidB(recode, 2468, 3))       ' 22 延滞月数
                SCList.Add(cmn.MidB(recode, 2402, 11))      ' 23 ボーナス返済額
                SCList.Add(cmn.MidB(recode, 2215, 11))      ' 24 更新日残高
                SCList.Add(cmn.MidB(recode, 2471, 11))      ' 25 延滞合計額
                SCList.Add(cmn.MidB(recode, 2204, 11))      ' 26 貸付金額
                SCList.Add(cmn.MidB(recode, 2336, 11))      ' 27 貸付金額(ボーナス)
                SCList.Add(cmn.MidB(recode, 1515, 8))       ' 28 完済日
                SCList.Add(cmn.MidB(recode, 2151, 3))       ' 29 償還残回数
                SCList.Add(cmn.MidB(recode, 2238, 10))      ' 30 当月回収元金
                SCList.Add(cmn.MidB(recode, 2281, 11))      ' 31 次回償還元金
                SCList.Add(cmn.MidB(recode, 2314, 11))      ' 32 次々回償還元金
                SCList.Add(cmn.MidB(recode, 62, 4))         ' 33 増額返済月
                SCList.Add(cmn.MidB(recode, 1064, 1))       ' 34 物件情報 自ら住居サイン
                SCList.Add(cmn.MidB(recode, 1065, 8))       ' 35 物件情報 郵便番号
                SCList.Add(cmn.MidB(recode, 1233, 160))     ' 36 物件情報 漢字住所
                SCList.Add(cmn.MidB(recode, 2347, 11))      ' 37 更新日残高(ボーナス)
                SCList.Add(cmn.MidB(recode, 371, 1))        ' 38 債務者 団信加入サイン
                SCList.Add(cmn.MidB(recode, 669, 1))        ' 39 連債者 団信加入サイン
                SCList.Add(cmn.MidB(recode, 1429, 8))       ' 40 償還金口座情報 口座番号
                SCList.Add(cmn.MidB(recode, 1437, 30))      ' 41 償還金口座情報 口座名義人カナ
                SCList.Add(cmn.MidB(recode, 1421, 4))       ' 42 償還金口座情報 金融機関口座支店コード
                SCList.Add(cmn.MidB(recode, 1425, 3))       ' 43 償還金口座情報 預金種別コード
                SCList.Add(cmn.MidB(recode, 1543, 80))      ' 44 メモ欄
                ' *** ↑ここの数を変更したら GBFILE_LISTCNT の値も合わせて変更すること ***
            End While
        End Using
    End Sub

    Private ReadOnly AssistHeaderTbl() As String = {
        "顧客番号",                              ' C01
        "機構顧客番号",                          ' C02
        "債務者名漢字",                          ' C03
        "債務者名カナ",                          ' C04
        "生年月日",                              ' C05  ※19870626の形式なのでスラッシュを追加 1987/06/26
        "電話番号１",                            ' C06
        "携帯電話番号",                          ' C07
        "郵便番号",                              ' C08
        "住所１",                                ' C09  ※住所1,2,3をあわせたもの
        "勤務先名",                              ' C10
        "勤務先電話番号",                        ' C11
        "証書番号",                              ' C12
        "連保人氏姓",                            ' C13  ※連保人姓,名をあわせたもの
        "連保人氏姓カナ",                        ' C14  ※連保人姓カナ,名カナをあわせたもの
        "連保人生年月日",                        ' C15  ※19870626の形式なのでスラッシュを追加 1987/06/26
        "連保人自宅電話番号",                    ' C16
        "連保人携帯電話番号",                    ' C17
        "連保人郵便番号",                        ' C18
        "連保人住所",                            ' C19
        "連保人勤務先",                          ' C20
        "連保人勤務先電話番号",                  ' C21
        "-",                                     ' C22
        "延滞合計額",                            ' C23
        "債権総額月賦元本",                      ' C24  ※2025/03/04 貸付金額 → 取得元を変更
        "延滞回数",                              ' C25
        "支払額月賦",                            ' C26  ※2025/03/04 入金額   → 取得元を変更
        "月末残高月賦元本",                      ' C27  ※2025/03/04 貸付残高 → 取得元を変更
        "金消契約年月日",                        ' C28
        "債権総額半年賦元本",                    ' C29  ※2025/03/04 追加
        "月末残高半年賦元本",                    ' C30  ※2025/03/04 追加
        "支払額半年賦",                          ' C31  ※2025/03/04 追加
        "完済日"                                 ' C32  ※2025/03/04 追加
    }
    Const NOT_FOUND As Integer = -1
    ' アシストデータ読み込み
    Private Sub ReadAssistData(fname As String)
        Dim AssistCmdList As New List(Of String)                        ' アシストデータDB書き込みリスト
        Try
            Using sr As StreamReader = New StreamReader(fname, Encoding.GetEncoding("Shift_JIS"))
                Dim tableNames() As String = sr.ReadLine().Split(","c)      ' 1行目 テーブル名(使う?)
                Dim headerNames() As String = sr.ReadLine().Split(","c)     ' 2行目 ヘッダー(項目)名
                Dim costomInfo As New List(Of String())                     ' 3行目以降 顧客情報
                While (sr.Peek() > -1)
                    ' 債務者情報の一部が分割されている項目があるので、もしあれば結合するためヘッダーから検索する
                    Dim addStr() As String = sr.ReadLine().Split(","c)
                    ' 住所１に、住所１＋２＋３を結合して保存
                    Dim jusho1Idx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(8))
                    If jusho1Idx > -1 Then
                        addStr(jusho1Idx) = (addStr(jusho1Idx) & addStr(jusho1Idx + 1) & addStr(jusho1Idx + 2)).Replace("　", "").Replace(" ", "")
                    End If
                    ' 連保人姓に、連保人姓、連保人名を結合して保存
                    Dim renpoSeiIdx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(12))
                    If renpoSeiIdx > -1 Then
                        addStr(renpoSeiIdx) = (addStr(renpoSeiIdx) & "　" & addStr(renpoSeiIdx + 1)).Replace("　", "").Replace(" ", "")
                    End If
                    ' 連保人姓に、連保人姓、連保人名を結合して保存
                    Dim renpoSeiKanaIdx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(13))
                    If renpoSeiKanaIdx > -1 Then
                        addStr(renpoSeiKanaIdx) = (addStr(renpoSeiKanaIdx) & " " & addStr(renpoSeiKanaIdx + 1)).Replace("　", "").Replace(" ", "")
                    End If

                    ' 生年月日がスラッシュなし8桁になってるので区切り文字スラッシュを追加
                    Dim birthIdx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(4))
                    If birthIdx > -1 Then
                        addStr(birthIdx) = cmn.ToDate(addStr(birthIdx))
                    End If
                    ' 連保人生年月日がスラッシュなし8桁になってるので区切り文字スラッシュを追加
                    Dim birth2Idx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(14))
                    If birth2Idx > -1 Then
                        addStr(birth2Idx) = cmn.ToDate(addStr(birth2Idx))
                    End If
                    ' 完済日の日付にスラッシュを追加
                    Dim birth3Idx As Integer = Array.IndexOf(headerNames, AssistHeaderTbl(31))
                    If birth3Idx > -1 Then
                        addStr(birth3Idx) = cmn.ToDate(addStr(birth3Idx))
                    End If

                    costomInfo.Add(addStr)
                End While
                If costomInfo.Count = 0 Then Exit Try

                ' アシストテーブルのヘッダー項目(AssistHeaderTbl)が、CSVの何列目idxにあるか取得
                Dim idxList As New List(Of Integer)
                For Each header In AssistHeaderTbl
                    Dim headerIdx As Integer = Array.IndexOf(headerNames, header)       ' ヘッダー名の一致情報取得
                    If headerIdx >= 0 Then
                        idxList.Add(headerIdx)                       ' 存在してたらIdxを設定
                    Else
                        idxList.Add(NOT_FOUND)                       ' 存在しない場合はNOT_FOUND
                    End If
                Next

                db.UpdateOrigDT(Sqldb.TID.SCAS)
                Dim dupList As New List(Of String)                      ' 重複登録防止用、登録者リスト
                ' 3行目以降の加入者情報を全て読み出す(最後尾から順に)
                For n = costomInfo.Count - 1 To 0 Step -1
                    Dim cId As String                                   ' 顧客番号          ex 10000000021
                    Dim mecId As String = ""                            ' 機構顧客番号      ex 251511060002258
                    cId = costomInfo(n)(Array.IndexOf(headerNames, AssistHeaderTbl(0))).Trim           ' [顧客番号] の文字が含まれるヘッダーindexから顧客番号の値取得してる  主キー必須param
                    If Array.IndexOf(headerNames, AssistHeaderTbl(1)) > -1 Then
                        mecId = costomInfo(n)(Array.IndexOf(headerNames, AssistHeaderTbl(1))).Trim     ' [機構顧客番号] の文字が含まれるヘッダーindexから顧客番号の値取得してる  任意parma
                    End If

                    ' アシストデータの生年月日が、1900/1/1(-2日)からの経過日数になっているのでyyyy/mm/dd形式に変換する
                    Dim chgItems() As String = {"申込人生年月日", "連債者生年月日", "金消契約年月日"}
                    Dim cVal As String
                    Dim arridx As Integer
                    For Each itm In chgItems
                        arridx = Array.IndexOf(headerNames, itm)
                        If arridx < 0 Then Continue For
                        cVal = costomInfo(n)(arridx)
                        If cVal <> "" Then costomInfo(n)(Array.IndexOf(headerNames, itm)) = CDate("1899/12/30").AddDays(cVal).ToShortDateString
                    Next

                    ' たまに同じ機構顧客番号で二度申込み登録されている。(恐らく入力誤りの訂正？)
                    ' 二重登録しないように、既に登録した機構顧客番号と重複している場合は登録しない。 ※最後尾データから参照してるので、顧客番号の大きいが優先される
                    If (mecId <> "") And (dupList.IndexOf(mecId) >= 0) Then Continue For
                    dupList.Add(mecId)

                    Dim cmd As String = ""                          ' 各顧客の登録or更新コマンド作成
                    If db.OrgDataTable(Sqldb.TID.SCAS).Select(String.Format("C01 = {0}", cId)).Length > 0 Then
                        ' 既にDBに存在するのでUpdate
                        cmd += "Update TBL Set "
                        For idx As Integer = 0 To idxList.Count - 1
                            If idx = 0 Then Continue For                    ' C01でDBの主キーだから更新しない
                            If idxList(idx) = NOT_FOUND Then Continue For   ' 読み込みデータに存在しない項目は更新できない
                            ' アポストロフィが含まれていた場合SQL構文エラーになるので削除する
                            If costomInfo(n)(idxList(idx)).IndexOf("'") >= 0 Then costomInfo(n)(idxList(idx)) = cmn.RegReplace(costomInfo(n)(idxList(idx)), "'", "")

                            cmd += "C" & (idx + 1).ToString("D2") & "='" & costomInfo(n)(idxList(idx)).Trim & "',"
                        Next

                        cmd = cmd.TrimEnd(CType(",", Char))     ' 余分なカンマを削除
                        cmd += " Where C01 ='" & cId & "'"
                    Else
                        ' DBにないのでInsert
                        cmd += "Insert Into TBL Values("
                        For Each idx As Integer In idxList
                            If idx = NOT_FOUND Then
                                cmd += "'',"
                                Continue For
                            End If
                            ' アポストロフィが含まれていた場合SQL構文エラーになるので削除する
                            If costomInfo(n)(idx).IndexOf("'") >= 0 Then costomInfo(n)(idx) = cmn.RegReplace(costomInfo(n)(idx), "'", "")

                            cmd += "'" & costomInfo(n)(idx).Trim & "',"
                        Next
                        cmd = cmd.TrimEnd(CType(",", Char))     ' 余分なカンマを削除
                        cmd += ")"
                    End If
                    AssistCmdList.Add(cmd)   ' アシストレコードデータを設定
                Next
                log.TimerED("CreateCmd")
            End Using
            ' DB書き込み
            WriteAssist(AssistCmdList)
        Catch ex As Exception
            MsgBox("アシストデータのファイル形式が正しくありません。" & vbCrLf & ex.Message)
        End Try
    End Sub

    Private Sub WriteAssist(aCmdList As List(Of String))
        Dim con As New SQLiteConnection()
        Dim cmd As New SQLiteCommand()
        Try
            con.ConnectionString = "Data source = " & db.CurrentPath_LO & Sqldb.DB_FKSCASSIST
            cmd.Connection = con
            cmd.CommandText = ""
            con.Open()
            cmd.Transaction = con.BeginTransaction()
            Dim idx As Integer = 0
            For Each d In aCmdList
                cmd.CommandText = d
                cmd.ExecuteNonQuery()
            Next
            cmd.Transaction.Commit()
        Catch ex As Exception
            MsgBox("DB書き込みで異常が見つかりました。" & vbCrLf & ex.Message)
        Finally
            con.Close()
        End Try
    End Sub

    '' アシスト項目がDBに既にあるかSelectで結果をリスト取得
    'Private Function GetAssistIdList() As List(Of String)
    '    Dim ret As New List(Of String)
    '    Dim SqlCmd = "Select C01 From TBL"
    '    Dim con As New SQLiteConnection()
    '    Dim cmd As New SQLiteCommand()
    '    Try
    '        con.ConnectionString = "Data source = " & db.CurrentPath_LO & Sqldb.DB_FKSCASSIST
    '        cmd.Connection = con
    '        con.Open()
    '        cmd.CommandText = SqlCmd
    '        Dim dtr As SQLiteDataReader = cmd.ExecuteReader                                  ' SQL結果取得
    '        If dtr.HasRows Then
    '            While dtr.Read()
    '                For n = 0 To dtr.FieldCount - 1
    '                    ret.Add(dtr.Item(n))
    '                Next
    '            End While
    '        End If
    '    Catch ex As Exception
    '        MsgBox("アシスト GetAssistIdList失敗" & vbCrLf & ex.Message)
    '    End Try
    '    con.Close()
    '    Return ret
    'End Function

    ' デバッグ解析用
    Private Sub AnalyzeGBFileDbg(fname As String)
        Dim dt As New DataTable
        Dim item(id_st.Count - 1) As Object

        Dim myType As Type = GetType(DataGridView)
        Dim myPropInfo As Reflection.PropertyInfo = myType.GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        myPropInfo.SetValue(DGV1, True, Nothing)
        DGV1.Size = New Size(Me.Size.Width - 20, Me.Size.Height - 35)
        DGV1.Visible = True
        DGV1.Columns.Clear()
        DGV1.Rows.Clear()

        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        ' ヘッダ生成
        For Each id In id_name
            dt.Columns.Add(id, Type.GetType("System.String"))
        Next
        ' ファイルから1レコード分単位に取得
        Using sr As StreamReader = New StreamReader(fname, Encoding.GetEncoding("Shift_JIS"))
            ' データ部分を読み込み
            Dim recode As String = ""
            Dim rline As String = ""
            While (sr.Peek() > -1)
                recode = sr.ReadLine()
                For num = 0 To id_st.Count - 1 Step 1
                    item(num) = cmn.MidB(recode, id_st(num), id_len(num))
                Next
                dt.Rows.Add(item)
            End While
            DGV1.DataSource = dt
        End Using
    End Sub


    ' GEファイル解析
    Const LINE_OFFSET As Integer = 350
    Const ID_HR As String = "1"
    Const ID_DR As String = "2"
    Const ID_TR As String = "8"
    Const ID_ER As String = "9"

    Private Sub AnalyzeGEFile(fname As String)
        Cursor.Current = Cursors.WaitCursor             ' マウスカーソルを砂時計に
        Dim dt As New DataTable
        Dim clen() As Integer = {1, 4, 15, 3, 15, 4, 1, 7, 30, 10, 1, 20, 1, 8, 1, 1, 8, 8, 2, 1, 9, 4, 10, 2, 4, 3, 8, 20, 13, 20, 13, 1, 11, 11, 1, 3, 3, 8, 11, 6, 9, 5, 6, 1, 1, 26}
        Dim myType As Type = GetType(DataGridView)
        Dim myPropInfo As Reflection.PropertyInfo = myType.GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        myPropInfo.SetValue(DGV1, True, Nothing)
        DGV1.Size = New Size(Me.Size.Width - 20, Me.Size.Height - 20)
        DGV1.Visible = True
        DGV1.Columns.Clear()
        DGV1.Rows.Clear()

        ' DGVヘッダ設定
        Dim cname() As String = {"1レコード区分", "2取引銀行番号", "6取引銀行名", "21取引支店番号", "24取引支店名", "39予備", "43預金種目", "44口座番号", "51預金者名", "81引落金額", "91新規コード", "92顧客番号", "112振替結果コード", "113予備", "121融資区分", "122再抽出区分", "123約定日", "131補正後約定日", "139債権優先順位", "141回収引落データ区分", "142延滞損害金単価", "151延滞日数", "155延滞損害金合計額", "165機構支店コード", "167金融機関コード", "171金融機関支店コード", "174CIF番号", "182主債務者氏名カナ", "202現在住所電話番号", "215債権番号", "235連絡先電話番号", "248保証区分", "249貸付金残高", "260予定貸付金残高", "271ボーナス償還月1", "272償還回次", "275予定償還回次", "278残高現在日", "286延滞元利金（累計）", "297延滞損害金（累計）", "303延滞損害金単価（今後）", "312識別番号", "317ロケーションキー", "323買取識別区分", "324勘定区分", "325予備"}
        For Each c In cname
            dt.Columns.Add(c, Type.GetType("System.String"))
        Next

        ' ファイルから1レコード分単位に取得
        Using sr As StreamReader = New StreamReader(fname, Encoding.GetEncoding("Shift_JIS"))
            Dim recode As String = ""
            While (sr.Peek() > -1)
                recode = sr.ReadLine()
                ' レコード単位(350文字毎)
                For n As Integer = 1 To recode.Length / LINE_OFFSET Step 1
                    Dim line As String = cmn.MidB(recode, ((n - 1) * LINE_OFFSET) + 1, LINE_OFFSET)
                    'Dim rList As New List(Of String)
                    'rList.Add(cmn.MidB(recode, ((n - 1) * LINE_OFFSET) + 1, LINE_OFFSET))
                    ' log.DBGLOG("LINE:" & n.ToString("000") & ":" & line)
                    Select Case line.First
                        Case ID_HR  ' 必要ない？
                        Case ID_DR
                            Dim offset As Integer = 1
                            Dim newRow As DataRow = dt.NewRow
                            Dim num As Integer = 0
                            For Each length In clen
                                newRow.Item(num) = cmn.MidB(line, offset, length)
                                offset += length
                                num += 1
                            Next
                            dt.Rows.Add(newRow)
                        Case ID_TR  ' 必要ない
                        Case ID_ER  ' 必要ない
                    End Select
                Next
            End While
        End Using
        DGV1.DataSource = dt
    End Sub


    ' 更新日の表示(更新)
    Private Sub ShowLastUpdateTimes()
        L_UptimeTXT.Text = "[最終更新日] " & File.GetLastWriteTime(db.CurrentPath_LO & Sqldb.DB_FKSC).ToShortDateString
        L_UptimeCSV.Text = "[最終更新日] " & File.GetLastWriteTime(db.CurrentPath_LO & Sqldb.DB_FKSCASSIST).ToShortDateString
        L_UptimeAutoCall.Text = "[最終更新日] " & File.GetLastWriteTime(db.CurrentPath_LO & Sqldb.DB_AUTOCALL).ToShortDateString
        L_UptimeOverTax.Text = "[最終更新日] " & File.GetLastWriteTime(db.CurrentPath_LO & Sqldb.DB_OVERTAX).ToShortDateString
    End Sub


    ' ショートカット F1
    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                cmn.OpenCurrentDir()
            Case Keys.F2
            Case Keys.F3
            Case Keys.F4
        End Select
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Cursor.Current = Cursors.WaitCursor  ' マウスカーソルを砂時計に
        db.ExeSQL(Sqldb.TID.SCAS, "Delete From TBL Where C02 = ''")
        CopyCDBtoServer(Sqldb.DB_FKSCASSIST)            ' DBファイルをローカルからサーバーにコピー
    End Sub

    Private Sub WriteToSQLite(dataTable As DataTable)
        For Each row As DataRow In dataTable.Rows
            Dim C01Value As String = row("顧客番号").ToString()
            Dim C02Value As String = row("延滞元利金（累計）").ToString()
            Dim C03Value As String = row("延滞損害金（累計）").ToString()
            Dim C04Value As String = row("延滞損害金単価（今後）").ToString()

            ' 既にC01の値が存在するかのチェック
            Dim checkCommand As String = $"SELECT * FROM TBL WHERE C01='{C01Value}'"
            Dim dt As DataTable = db.GetSelect(Sqldb.TID.AC, checkCommand)

            If dt.Rows.Count > 0 Then
                ' 既存の行をアップデート
                Dim updateCommand As String = $"UPDATE TBL SET C02='{row("延滞元利金（累計）").ToString()}', C03='{row("延滞損害金（累計）").ToString()}', C04='{row("延滞損害金単価（今後）").ToString()}' WHERE C01='{C01Value}'"
                db.AddSQL(Sqldb.TID.AC, updateCommand)
            Else
                ' 新しい行を挿入
                Dim insertCommand As String = $"INSERT INTO TBL (C01, C02, C03, C04) VALUES ('{C01Value}', '{row("延滞元利金（累計）").ToString()}', '{row("延滞損害金（累計）").ToString()}', '{row("延滞損害金単価（今後）").ToString()}')"
                db.AddSQL(Sqldb.TID.AC, insertCommand)
            End If
        Next
        db.ExeSQL(Sqldb.TID.AC)
    End Sub


    ' オートコール読み込み
    Private Sub LoadFileToDGV(filePath As String)
        Try
            ' データレコードの情報
            Dim records As New List(Of RecordInfo) From {
                New RecordInfo("レコード区分", 0, 1),
                New RecordInfo("引落銀行番号", 1, 4),
                New RecordInfo("引落銀行支店番号", 5, 3),
                New RecordInfo("預金種目", 8, 1),
                New RecordInfo("口座番号", 9, 7),
                New RecordInfo("顧客番号", 21, 15),             ' 本来はIndex16から20文字だが、現在顧客番号は15桁固定なので15文字ちょうどを取得
                New RecordInfo("振替結果コード", 36, 1),
                New RecordInfo("主債務者氏名（カナ）", 37, 20),
                New RecordInfo("現在住所電話番号", 57, 13),
                New RecordInfo("債権番号", 70, 20),
                New RecordInfo("連絡先電話番号", 90, 13),
                New RecordInfo("保証区分", 103, 1),
                New RecordInfo("貸付金残高", 104, 11),
                New RecordInfo("予定貸付金残高", 115, 11),
                New RecordInfo("ボーナス償還月1", 126, 1),
                New RecordInfo("償還回次", 127, 3),
                New RecordInfo("予定償還回次", 130, 3),
                New RecordInfo("残高現在日", 133, 8),
                New RecordInfo("延滞元利金（累計）", 141, 11),
                New RecordInfo("延滞損害金（累計）", 152, 6),
                New RecordInfo("延滞損害金単価（今後）", 158, 9),
                New RecordInfo("予備", 167, 83)
            }

            ' 解析ONのときだけDGV表示
            If CB_A1.Checked Then
                DGV1.Visible = True
            End If
            DGV1.Size = New Size(Me.Size.Width - 20, Me.Size.Height - 35)
            DGV1.Columns.Clear()
            DGV1.Rows.Clear()

            ' ファイルの内容を全て読み込む
            Dim allText As String = File.ReadAllText(filePath, Encoding.GetEncoding("shift_jis"))

            ' 250文字ごとのデータを格納するリストを作成
            Dim dataList As New List(Of String)
            For i As Integer = 0 To allText.Length - 1 Step 250
                Dim record As String = allText.Substring(i, 250)
                If record.Substring(0, 1) = "2" Then
                    dataList.Add(record)
                End If
            Next

            ' データを格納するDataTableを初期化
            Dim dt As New DataTable

            ' レコード情報に基づいてカラムを追加
            For Each rInfo As RecordInfo In records
                dt.Columns.Add(rInfo.Name)
            Next

            ' データリストからDataTableを作成
            For Each data As String In dataList
                Dim row As DataRow = dt.NewRow()
                For Each rInfo As RecordInfo In records
                    Dim cellData As String = data.Substring(rInfo.Offset, rInfo.Size).Trim()
                    ' cellData = cellData.TrimStart("0"c) ' 先頭の0を削除
                    row(rInfo.Name) = cellData
                Next
                dt.Rows.Add(row)
            Next

            ' DataTableをDGVにセット
            DGV1.DataSource = dt

            ' SQL出力
            WriteToSQLite(dt)

        Catch ex As Exception
            MessageBox.Show("エラー: " & ex.Message)
        End Try
    End Sub

    Public Sub LoadOverTax(files As String())
        Try
            db.DeleteAllData(Sqldb.TID.OTAX)
            For Each filePath In files
                ' CSVファイルの各行を読み込む
                Using sr As StreamReader = New StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"))
                    While Not sr.EndOfStream
                        Dim line As String = sr.ReadLine()
                        Dim elements As String() = line.Split(","c).Select(Function(s) s.Trim()).ToArray()

                        ' 分割された各要素をリストに追加
                        db.ExeSQLInsert(Sqldb.TID.OTAX, elements)
                    End While
                End Using
            Next
            db.ExeSQL(Sqldb.TID.OTAX)
        Catch ex As Exception
            Console.WriteLine("エラーが発生しました: " & ex.Message)
        End Try
    End Sub

    Private Class RecordInfo
        Public Property Name As String
        Public Property Offset As Integer
        Public Property Size As Integer

        Public Sub New(ByVal name As String, ByVal offset As Integer, ByVal size As Integer)
            Me.Name = name
            Me.Offset = offset
            Me.Size = size
        End Sub
    End Class

End Class

