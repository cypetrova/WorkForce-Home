
Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing.Imaging
Imports System.Net
Imports AjaxControlToolkit
Imports AjaxControlToolkit.AsyncFileUpload.Constants
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Partial Class VavejdDom2
    Inherits System.Web.UI.Page
    Dim iPersCharact As Integer = 11
    Dim dtCountrygraj As New DataTable
    Dim dtCountry As New DataTable
    Dim iPersons As Integer
    Dim myTbl As String = "lica"
    Dim myTblDom As String = "dom"
    Dim PageName As String = ""

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        rolia.Text = Session("rol")
        potr.Text = Session("potr")
        oblast.Text = Session("cRsu")

        If Session("rol") = "" Then
            Response.Redirect("~/Default.aspx")
        End If


        'MandatoryCheck.Visible = False
        PageName = GetCurrentPageName()
        HObsTime.Value = Session("ObsNumb")

        If Not IsPostBack Then
            d1.Attributes.Add("style", "display:none;")
            NextPage.Attributes.Add("style", "display:none;")
            txtArea.Text = CType(Session("oblast"), String)
            txtPlace.Text = CType(Session("grs"), String)
            txtAreaNest.Text = CType(Session("gnezdo"), String)
            txtHouseHoldNest.Text = CType(Session("domak"), String)

            Dim NumbPersons = 0
            Dim lDom As New Domakinstvo()
            Dim cnn As New SqlConnection
            Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
            cnn.ConnectionString = sCnn
            Dim MyDom As ArrayList = New ArrayList() From {Session("oblast"), CType(Session("grs"), Integer), Session("gnezdo"), Session("domak"), Session("ObsNumb")}
            lDom.ReadData2(sCnn, MyDom)

            If Not String.IsNullOrEmpty(Session("ObsDate")) Then
                txtDate.Text = Session("ObsDate")
            Else
                txtDate.Text = ""
            End If

            If lDom.Phhsize = "99" Then
                NmbPrs.Text = ""
                NumbPersons = 0
            ElseIf lDom.Phhsize = "0" Then
                NmbPrs.Text = ""
                NumbPersons = 0
            Else
                'Не е сигурно дали трябва да останат
                NmbPrs.Text = lDom.Phhsize
                HsHldNum.Text = lDom.Phhsize
                Session("NumbPersons") = lDom.Phhsize
            End If

            If lDom.Pdl = "99" Then
                NmbPrs.Text = ""
            ElseIf lDom.Phhsize = "0" Then
                NmbPrs.Text = ""
            Else
                NmbPrs.Text = lDom.Pdl
            End If

            iPersons = CalculatePersonsInDB()
            hNPers.Value = iPersons
            If iPersons >= 0 Then
                CreateTPersons(iPersons, iPersCharact, True)
            End If

            LoadCountry_grajdanstvo()
            LoadCountries()

            ' извикване на SQL скрипта за обновяване на възрастта
            Dim sqlCommand As String = "
                UPDATE lica
                SET age = CASE 
                                WHEN lica.mb = '00' AND lica.db = '00' THEN 
                                    CASE WHEN MONTH(GETDATE()) * 100 + DAY(GETDATE()) >= lica.mb * 100 + lica.db THEN 
                                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm)) + 1
                                    ELSE 
                                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm))
                                    END
                                WHEN lica.mb = '99' OR lica.db = '99' THEN age
                                ELSE 
                                    CASE WHEN MONTH(GETDATE()) * 100 + DAY(GETDATE()) >= lica.mb * 100 + lica.db THEN 
                                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm))
                                    ELSE 
                                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm)) - 1
                                    END
                            END
                FROM lica
                WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;
            "
            Dim cmd As New SqlCommand(sqlCommand, cnn)
            Try
                cnn.Open()
                cmd.ExecuteNonQuery()
            Catch ex As Exception
                ' Обработка на грешката
                Response.Write("Грешка при изпълнение на SQL заявката: " & ex.Message)
            Finally
                If cnn.State = ConnectionState.Open Then
                    cnn.Close()
                End If
            End Try


        End If

    End Sub

    Private Function CreateTPersons(nRows As Integer, nCells As Integer, pl As Boolean) As Boolean

        tPersons.Border = 3

        'header
        Dim headerRow As New HtmlTableRow

        Dim htmlTableCell1 As New HtmlTableCell
        htmlTableCell1 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell1)
        htmlTableCell1.InnerText = "Д2.Пореден номер на лицето"

        Dim htmlTableCell2 As New HtmlTableCell
        htmlTableCell2 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell2)
        htmlTableCell2.InnerText = "Д4.Отношение към главата на домакинството"

        Dim htmlTableCell3 As New HtmlTableCell
        htmlTableCell3 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell3)
        htmlTableCell3.InnerText = "Д5.Пореден номер на съпругата/съпруга на лицето"

        Dim htmlTableCell4 As New HtmlTableCell
        htmlTableCell4 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell4)
        htmlTableCell4.InnerText = "Д6.Пореден номер на бащата на лицето"

        Dim htmlTableCell5 As New HtmlTableCell
        htmlTableCell5 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell5)
        htmlTableCell5.InnerText = "Д7.Пореден номер на майката на лицето"

        Dim htmlTableCell6 As New HtmlTableCell
        htmlTableCell6 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell6)
        htmlTableCell6.InnerText = "Д8.Пол"

        Dim htmlTableCell7 As New HtmlTableCell
        htmlTableCell7 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell7)
        htmlTableCell7.InnerText = "Д9.Ден на раждане"

        Dim htmlTableCell8 As New HtmlTableCell
        htmlTableCell8 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell8)
        htmlTableCell8.InnerText = "Д9.Месец на раждане"

        Dim htmlTableCell9 As New HtmlTableCell
        htmlTableCell9 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell9)
        htmlTableCell9.InnerText = "Д9.Година на раждане"

        Dim htmlTableCell10 As New HtmlTableCell
        htmlTableCell10 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell10)
        htmlTableCell10.InnerText = "Д10.Възраст"

        Dim htmlTableCell11 As New HtmlTableCell
        htmlTableCell11 = New HtmlTableCell("th")
        headerRow.Cells.Add(htmlTableCell11)
        htmlTableCell11.InnerText = "Лицето е в домакинството към момента на наблюдението"

        tPersons.Rows.Add(headerRow)

        If nRows > 0 Then
            Dim j As Integer
            For j = 0 To nRows - 1
                Dim row As New HtmlTableRow()
                Dim i As Integer
                For i = 0 To nCells - 1
                    Dim mAL As New ArrayList()
                    If pl = True Then
                        mAL = PersonsFromDB(j)
                    Else
                        mAL = PersonsFromHFs(j)
                    End If

                    Dim c As New HtmlTableCell()

                    ' Create a new HtmlButton control.

                    Dim b As New HtmlButton()
                    'If (c.Controls.Contains(b)) Then
                    '    MsgBox(b.ID)
                    '    Controls.Remove(b)
                    'End If
                    Dim mVal As String = mAL.Item(i).ToString()
                    If i = 0 Then
                        Dim prsN As Integer = j + 1
                        c.Controls.Add(b)
                        b.ID = "btn" + mVal
                        'MsgBox(mVal)
                        b.InnerHtml = "Лице " + mVal
                    Else
                        c.Controls.Add(New LiteralControl(mVal))
                    End If
                    row.Cells.Add(c)
                Next i

                tPersons.Rows.Add(row)

            Next j
        End If
        Return 1
    End Function

    Private Function PersonsFromDB(j As Integer) As ArrayList
        Dim mAL As New ArrayList()
        Dim mDic As New Dictionary(Of String, String)
        j = j + 1
        Dim PersN As String
        If j <= 9 Then
            PersN = "0" + j.ToString()
        Else
            PersN = j
        End If

        Dim MyLice As New Lice()
        MyLice.StConn = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        MyLice.ReadData(Session("oblast"), Session("grs"), Session("gnezdo"), Session("domak"), PersN, Val(Session("ObsNumb")), "Lica")

        mAL.Add(MyLice.Stlice)
        mAL.Add(MyLice.Strel)
        mAL.Add(MyLice.Sthusb)
        mAL.Add(MyLice.Stfather)
        mAL.Add(MyLice.Stmother)
        mAL.Add(MyLice.Stsex)
        mAL.Add(MyLice.Stdb)
        mAL.Add(MyLice.Stmb)
        mAL.Add(MyLice.Styb)
        mAL.Add(MyLice.Stage)
        If MyLice.Stactv = "True" Then
            mAL.Add("Да")
        Else
            mAL.Add("Не")
        End If

        Select Case j
            Case 1
                HPerson1.Value = JsonConvert.SerializeObject(MyLice)
            Case 2
                HPerson2.Value = JsonConvert.SerializeObject(MyLice)
            Case 3
                HPerson3.Value = JsonConvert.SerializeObject(MyLice)
            Case 4
                HPerson4.Value = JsonConvert.SerializeObject(MyLice)
            Case 5
                HPerson5.Value = JsonConvert.SerializeObject(MyLice)
            Case 6
                HPerson6.Value = JsonConvert.SerializeObject(MyLice)
            Case 7
                HPerson7.Value = JsonConvert.SerializeObject(MyLice)
            Case 8
                HPerson8.Value = JsonConvert.SerializeObject(MyLice)
            Case 9
                HPerson9.Value = JsonConvert.SerializeObject(MyLice)
            Case 10
                HPerson10.Value = JsonConvert.SerializeObject(MyLice)
            Case 11
                HPerson11.Value = JsonConvert.SerializeObject(MyLice)
            Case 12
                HPerson12.Value = JsonConvert.SerializeObject(MyLice)
            Case 13
                HPerson13.Value = JsonConvert.SerializeObject(MyLice)
            Case 14
                HPerson14.Value = JsonConvert.SerializeObject(MyLice)
            Case 15
                HPerson15.Value = JsonConvert.SerializeObject(MyLice)
            Case 16
                HPerson16.Value = JsonConvert.SerializeObject(MyLice)
            Case 17
                HPerson17.Value = JsonConvert.SerializeObject(MyLice)
            Case 18
                HPerson18.Value = JsonConvert.SerializeObject(MyLice)
            Case 19
                HPerson19.Value = JsonConvert.SerializeObject(MyLice)
        End Select

        Return mAL
    End Function

    Private Function PersonsFromHFs(j As Integer) As ArrayList
        Dim MyLice As New Lice()
        Dim mAL As New ArrayList()
        j = j + 1

        Select Case j
            Case 1

                Dim sP As String = HPerson1.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                'MsgBox(S12)
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If

                Session("mLice1") = mLice
                'MsgBox(mLice)


            Case 2
                Dim sP As String = HPerson2.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice2") = mLice
            Case 3
                Dim sP As String = HPerson3.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice3") = mLice
            Case 4
                Dim sP As String = HPerson4.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice4") = mLice
            Case 5
                Dim sP As String = HPerson5.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice5") = mLice
            Case 6
                Dim sP As String = HPerson6.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice6") = mLice
            Case 7
                Dim sP As String = HPerson7.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice7") = mLice
            Case 8
                Dim sP As String = HPerson8.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice8") = mLice
            Case 9
                Dim sP As String = HPerson9.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice9") = mLice
            Case 10
                Dim sP As String = HPerson10.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice10") = mLice
            Case 11
                Dim sP As String = HPerson11.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice11") = mLice
            Case 12
                Dim sP As String = HPerson12.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice12") = mLice
            Case 13
                Dim sP As String = HPerson13.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice13") = mLice
            Case 14
                Dim sP As String = HPerson14.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice14") = mLice
            Case 15
                Dim sP As String = HPerson15.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice15") = mLice
            Case 16
                Dim sP As String = HPerson16.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice16") = mLice
            Case 17
                Dim sP As String = HPerson17.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice17") = mLice
            Case 18
                Dim sP As String = HPerson18.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice18") = mLice
            Case 19
                Dim sP As String = HPerson19.Value
                Dim mLice As New Lice()
                Dim jP As New JObject()
                If sP <> "" Then
                    jP = JObject.Parse(sP)
                End If
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.StConn = jP.SelectToken(".StConn")
                mLice.StUsp = jP.SelectToken(".StUsp")
                mLice.Stlice = jP.SelectToken(".Stlice")
                mLice.Strel = jP.SelectToken(".Strel")
                mLice.Sthusb = jP.SelectToken(".Sthusb")
                mLice.Stfather = jP.SelectToken(".Stfather")
                mLice.Stmother = jP.SelectToken(".Stmother")
                mLice.Stsex = jP.SelectToken(".Stsex")
                mLice.Stdb = jP.SelectToken(".Stdb")
                mLice.Stmb = jP.SelectToken(".Stmb")
                mLice.Styb = jP.SelectToken(".Styb")
                mLice.Stage = jP.SelectToken(".Stage")
                Dim S11 As String = jP.SelectToken(".Std11")
                mLice.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice.Std12 = S12.Substring(0, 3)
                Else
                    mLice.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice.Std13 = 0
                Else
                    mLice.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice.Std14 = 0
                Else
                    mLice.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice.Std15 = 0
                Else
                    mLice.Std15 = S15
                End If

                Dim S16 As String = jP.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice.Std16 = 0
                Else
                    mLice.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice.Std17 = 0
                Else
                    mLice.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice.Std18 = 0
                Else
                    mLice.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice.Std19 = 0
                Else
                    mLice.Std19 = S19.Substring(0, 1)
                End If
                mLice.Stactv = jP.SelectToken(".Stactv")

                mLice.Sttm = jP.SelectToken(".Sttm")
                mLice.inPoslBroj = jP.SelectToken(".inPoslBroj")

                mAL.Add(MyLice.Stlice)
                mAL.Add(MyLice.Strel)
                mAL.Add(MyLice.Sthusb)
                mAL.Add(MyLice.Stfather)
                mAL.Add(MyLice.Stmother)
                mAL.Add(MyLice.Stsex)
                mAL.Add(MyLice.Stdb)
                mAL.Add(MyLice.Stmb)
                mAL.Add(MyLice.Styb)
                mAL.Add(MyLice.Stage)
                If MyLice.Stactv = "True" Then
                    mAL.Add("Да")
                Else
                    mAL.Add("Не")
                End If
                Session("mLice19") = mLice
        End Select

        Return mAL
    End Function

    Private Function CalculatePersonsInHF() As Integer
        Dim iCount As Integer = 0
        For Each mP In {HPerson1.Value, HPerson2.Value, HPerson3.Value, HPerson4.Value, HPerson5.Value, HPerson6.Value, HPerson7.Value, HPerson8.Value, HPerson9.Value, HPerson10.Value, HPerson11.Value, HPerson12.Value, HPerson13.Value, HPerson14.Value, HPerson15.Value, HPerson16.Value, HPerson17.Value, HPerson18.Value, HPerson19.Value}
            If mP <> "" Then
                iCount = iCount + 1
            End If
        Next
        Return iCount
    End Function

    Private Function CalculatePersonsInDB() As Integer
        Dim iNPersons As Integer = 99
        Dim ObsNumb As Integer = Val(Session("ObsNumb"))
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_FinActConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim myTbl As String = "lica"
        Dim mySel As String = "SELECT COUNT(*) FROM " + myTbl + " WHERE oblast=@oblast and grs=@grs and gnezdo=@gnezdo and domak=@domak and tm=@tm"
        If ObsNumb = 1 Then
            Using cmd As New SqlCommand(mySel, con)
                cmd.CommandType = CommandType.Text
                cmd.Parameters.Add("@oblast", SqlDbType.NVarChar)
                cmd.Parameters("@oblast").Value = Session("oblast")
                cmd.Parameters.Add("@grs", SqlDbType.TinyInt)
                cmd.Parameters("@grs").Value = Session("grs")
                cmd.Parameters.Add("@gnezdo", SqlDbType.NVarChar)
                cmd.Parameters("@gnezdo").Value = Session("gnezdo")
                cmd.Parameters.Add("@domak", SqlDbType.NVarChar)
                cmd.Parameters("@domak").Value = Session("domak")
                cmd.Parameters.Add("@tm", SqlDbType.TinyInt)
                cmd.Parameters("@tm").Value = ObsNumb
                con.Open()

                Try
                    iNPersons = cmd.ExecuteScalar()
                Catch ex As Exception
                    ex.Message.ToString()
                End Try

                con.Close()
            End Using
        Else
            Do While ObsNumb >= 1 And iNPersons = 99 Or ObsNumb >= 1 And iNPersons = 0
                Using cmd As New SqlCommand(mySel, con)
                    cmd.CommandType = CommandType.Text
                    cmd.Parameters.Add("@oblast", SqlDbType.NVarChar)
                    cmd.Parameters("@oblast").Value = Session("oblast")
                    cmd.Parameters.Add("@grs", SqlDbType.TinyInt)
                    cmd.Parameters("@grs").Value = Session("grs")
                    cmd.Parameters.Add("@gnezdo", SqlDbType.NVarChar)
                    cmd.Parameters("@gnezdo").Value = Session("gnezdo")
                    cmd.Parameters.Add("@domak", SqlDbType.NVarChar)
                    cmd.Parameters("@domak").Value = Session("domak")
                    cmd.Parameters.Add("@tm", SqlDbType.TinyInt)
                    cmd.Parameters("@tm").Value = ObsNumb
                    con.Open()

                    Try
                        iNPersons = cmd.ExecuteScalar()
                    Catch ex As Exception
                        ex.Message.ToString()
                    End Try

                    con.Close()
                End Using
                ObsNumb = ObsNumb - 1
            Loop

        End If

        Return iNPersons

    End Function

    'Зареждане на код на страни от таблица classgraj
    Sub LoadCountry_grajdanstvo()
        Dim sSq1 As String
        Dim cnn As New SqlConnection
        Dim cmd As SqlCommand
        Dim sKod As String = "kod"
        Dim sStrana As String = "strana"
        Dim sTclassgraj As String = "classgraj"

        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        cnn.ConnectionString = sCnn
        sSq1 = "SELECT " + sKod + " ," + sStrana + " FROM " + sTclassgraj + " ORDER BY strana"
        cmd = New SqlCommand(sSq1, cnn)
        Dim ad1 As New SqlDataAdapter
        ad1.SelectCommand = cmd
        Try
            cnn.Open()
            ad1.Fill(dtCountrygraj)
            cnn.Close()
        Catch ex As Exception
            ex.Message.ToString()
        End Try
        cnn.Dispose()
        cmd.Dispose()
        Dim iMyEmpty As Integer = 1

        If DDL11.Items.Count < iMyEmpty Then
            DDL11.Items.Add("")
        End If

        Dim dr As DataRow
        Dim iMyCounter = dtCountrygraj.Rows.Count + 1
        For Each dr In dtCountrygraj.Rows

            If DDL11.Items.Count < iMyCounter Then
                DDL11.Items.Add(dr.Item(0) + "-" + dr.Item(1))
            End If

        Next
    End Sub

    'Зареждане на код на страни от таблица Class
    Sub LoadCountries()
        Dim sSq1 As String
        Dim cnn As New SqlConnection
        Dim cmd As SqlCommand

        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        cnn.ConnectionString = sCnn
        sSq1 = "SELECT kod, strana FROM class ORDER BY strana"
        cmd = New SqlCommand(sSq1, cnn)
        Dim ad1 As New SqlDataAdapter
        ad1.SelectCommand = cmd
        Try
            cnn.Open()
            ad1.Fill(dtCountry)
            cnn.Close()
        Catch ex As Exception
            ex.Message.ToString()
        End Try
        cnn.Dispose()
        cmd.Dispose()

        Dim iMyEmpty As Integer = 1
        If DDL12.Items.Count < iMyEmpty Then
            DDL12.Items.Add("")
        End If
        If DDL16.Items.Count < iMyEmpty Then
            DDL16.Items.Add("")
        End If
        If DDL17.Items.Count < iMyEmpty Then
            DDL17.Items.Add("")
        End If
        If DDL18.Items.Count < iMyEmpty Then
            DDL18.Items.Add("")
        End If

        Dim dr As DataRow
        Dim iMyCounter = dtCountry.Rows.Count + 1
        For Each dr In dtCountry.Rows

            If DDL12.Items.Count < iMyCounter Then
                DDL12.Items.Add(dr.Item(0) + "-" + dr.Item(1))
            End If
            If DDL16.Items.Count < iMyCounter Then
                DDL16.Items.Add(dr.Item(0) + "-" + dr.Item(1))
            End If
            If DDL16.Items.Count >= 2 Then
                DDL16.Items(1).Enabled = False
            End If
            If DDL17.Items.Count < iMyCounter Then
                DDL17.Items.Add(dr.Item(0) + "-" + dr.Item(1))
            End If
            If DDL18.Items.Count < iMyCounter Then
                DDL18.Items.Add(dr.Item(0) + "-" + dr.Item(1))
            End If
        Next
    End Sub


    Protected Sub PrevPage_Click(sender As Object, e As EventArgs) Handles PrevPage.Click
        Session("HasSurvey") = ""
        Response.Redirect("~\VavejdDom1.aspx")
    End Sub

    Protected Sub NextPage_Click(sender As Object, e As EventArgs) Handles NextPage.Click
        'MsgBox("VB")
        'If MErrors_Check() Then
        'Dim mP As Integer = Val(HCheckDataPass.Value)
        'If mP = 1 Then
        'If  Then

        'End If
        If RecordData() Then
            Response.Redirect("~\VavejdDom3.aspx")
        End If
        'End If
    End Sub

    Private Function RecordData() As Boolean

        Dim myResult = False
        Dim Persons As Integer = 0

        hNPers.Value = CalculatePersonsInHF()
        Persons = CInt(hNPers.Value)

        Dim sP1 As String = HPerson1.Value
        Dim mLice1 As New Lice()
        Dim jP1 As New JObject()
        If sP1 <> "" Then
            jP1 = JObject.Parse(sP1)
        End If

        Dim sP2 As String = HPerson2.Value
        Dim mLice2 As New Lice()
        Dim jP2 As New JObject()
        If sP2 <> "" Then
            jP2 = JObject.Parse(sP2)
        End If

        Dim sP3 As String = HPerson3.Value
        Dim mLice3 As New Lice()
        Dim jP3 As New JObject()
        If sP3 <> "" Then
            jP3 = JObject.Parse(sP3)
        End If

        Dim sP4 As String = HPerson4.Value
        Dim mLice4 As New Lice()
        Dim jP4 As New Object()
        If sP4 <> "" Then
            jP4 = JObject.Parse(sP4)
        End If


        Dim sP5 As String = HPerson5.Value
        Dim mLice5 As New Lice()
        Dim jP5 As New JObject()
        If sP5 <> "" Then
            jP5 = JObject.Parse(sP5)
        End If


        Dim sP6 As String = HPerson6.Value
        Dim mLice6 As New Lice()
        Dim jP6 As New JObject()
        If sP6 <> "" Then
            jP6 = JObject.Parse(sP6)
        End If


        Dim sP7 As String = HPerson7.Value
        Dim mLice7 As New Lice()
        Dim jP7 As New JObject()
        If sP7 <> "" Then
            jP7 = JObject.Parse(sP7)
        End If


        Dim sP8 As String = HPerson8.Value
        Dim mLice8 As New Lice()
        Dim jP8 As New JObject()
        If sP8 <> "" Then
            jP8 = JObject.Parse(sP8)
        End If

        Dim sP9 As String = HPerson9.Value
        Dim mLice9 As New Lice()
        Dim jP9 As New JObject()
        If sP9 <> "" Then
            jP9 = JObject.Parse(sP9)
        End If

        Dim sP10 As String = HPerson10.Value
        Dim mLice10 As New Lice()
        Dim jP10 As New JObject()
        If sP10 <> "" Then
            jP10 = JObject.Parse(sP10)
        End If


        Dim sP11 As String = HPerson11.Value
        Dim mLice11 As New Lice()
        Dim jP11 As New JObject()
        If sP11 <> "" Then
            jP11 = JObject.Parse(sP11)
        End If

        Dim sP12 As String = HPerson12.Value
        Dim mLice12 As New Lice()
        Dim jP12 As New JObject()
        If sP12 <> "" Then
            jP12 = JObject.Parse(sP12)
        End If

        Dim sP13 As String = HPerson13.Value
        Dim mLice13 As New Lice()
        Dim jP13 As New JObject()
        If sP13 <> "" Then
            jP13 = JObject.Parse(sP13)
        End If

        Dim sP14 As String = HPerson14.Value
        Dim mLice14 As New Lice()
        Dim jP14 As New JObject()
        If sP14 <> "" Then
            jP14 = JObject.Parse(sP14)
        End If


        Dim sP15 As String = HPerson15.Value
        Dim mLice15 As New Lice()
        Dim jP15 As New JObject()
        If sP15 <> "" Then
            jP15 = JObject.Parse(sP15)
        End If


        Dim sP16 As String = HPerson16.Value
        Dim mLice16 As New Lice()
        Dim jP16 As New JObject()
        If sP16 <> "" Then
            jP16 = JObject.Parse(sP16)
        End If

        Dim sP17 As String = HPerson17.Value
        Dim mLice17 As New Lice()
        Dim jP17 As New JObject()
        If sP17 <> "" Then
            jP17 = JObject.Parse(sP17)
        End If

        Dim sP18 As String = HPerson18.Value
        Dim mLice18 As New Lice()
        Dim jP18 As New JObject()
        If sP18 <> "" Then
            jP18 = JObject.Parse(sP18)
        End If

        Dim sP19 As String = HPerson19.Value
        Dim mLice19 As New Lice()
        Dim jP19 As New JObject()
        If sP19 <> "" Then
            jP19 = JObject.Parse(sP19)
        End If

        Select Case Persons
            Case 1
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1

            Case 2
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '2
            Case 3
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1

                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3


            Case 4
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2

                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3

                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4

            Case 5
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(1, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4

                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5

            Case 6
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If

                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If

                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If

                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6

            Case 7
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5

                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If

                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If

                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If

                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6

                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)

                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If

                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If

                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If

                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")

                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7

            Case 8
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If

                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If

                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If

                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)

                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If

                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If

                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If

                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")

                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)

                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If

                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If

                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If

                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If

                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")

                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8

            Case 9
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If

                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If

                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If

                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)

                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If

                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If

                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If

                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")

                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)

                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If

                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If

                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If

                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If

                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")

                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)

                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If

                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If

                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If

                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If

                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If

                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If

                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")

                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9

            Case 10
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If

                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)

                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If

                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If

                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")

                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If

                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If

                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If

                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If

                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)

                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If

                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If

                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If

                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")

                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)

                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If

                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If

                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If

                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If

                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")

                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)

                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If

                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If

                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If

                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If

                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If

                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If

                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")

                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)

                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If

                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If

                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If

                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If

                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If

                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If

                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")

                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

            Case 11
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If
                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If
                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If
                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If

                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If
                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If
                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")

                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If
                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9

                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If
                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If

                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If

                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If

                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
            Case 12
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If

                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If

                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If

                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If
                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If
                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If

                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If
                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If

                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If

                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If

                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")

                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
            Case 13
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If
                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If
                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If

                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If
                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If

                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")
                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If

                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If

                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If
                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If
                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If

                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)

                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If

                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If
                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If

                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If

                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")

                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If

                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If
                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")
                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
            Case 14
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If
                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If

                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If

                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")
                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If

                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If

                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If
                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If

                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If

                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If

                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If

                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If

                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")

                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If
                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If
                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If
                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If
                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If
                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")

                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
            Case 15
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If
                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If
                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)

                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If

                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If

                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If
                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)

                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If
                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If

                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If
                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If
                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")

                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If

                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If

                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If

                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If
                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If
                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")
                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If
                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If
                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If

                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If
                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If
                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")
                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
                '15
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.StConn = jP15.SelectToken(".StConn")
                mLice15.StUsp = jP15.SelectToken(".StUsp")
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.Strel = jP15.SelectToken(".Strel")
                mLice15.Sthusb = jP15.SelectToken(".Sthusb")
                mLice15.Stfather = jP15.SelectToken(".Stfather")
                mLice15.Stmother = jP15.SelectToken(".Stmother")
                mLice15.Stsex = jP15.SelectToken(".Stsex")
                mLice15.Stdb = jP15.SelectToken(".Stdb")
                mLice15.Stmb = jP15.SelectToken(".Stmb")
                mLice15.Styb = jP15.SelectToken(".Styb")
                mLice15.Stage = jP15.SelectToken(".Stage")
                Dim S1511 As String = jP15.SelectToken(".Std11")
                mLice15.Std11 = S1511.Substring(0, 3)
                Dim S1512 As String = jP15.SelectToken(".Std12")
                If S1512 = "" Or S1512 = "0" Or S1512 Is Nothing Then
                    S1512 = 0
                    mLice15.Std12 = S1512.Substring(0, 3)
                Else
                    mLice15.Std12 = S1512.Substring(0, 3)
                End If

                Dim S1513 As String = jP15.SelectToken(".Std13")
                If S1513 = "" Or S1513 Is Nothing Then
                    mLice15.Std13 = 0
                Else
                    mLice15.Std13 = S1513.Substring(0, 1)
                End If
                Dim S1514 As String = jP15.SelectToken(".Std14")
                If S1514 = "" Or S1514 Is Nothing Then
                    mLice15.Std14 = 0
                Else
                    mLice15.Std14 = S1514.Substring(0, 1)
                End If
                Dim S1515 As String = jP15.SelectToken(".Std15")
                If S1515 = "" Or S1515 Is Nothing Then
                    mLice15.Std15 = 0
                Else
                    mLice15.Std15 = S1515
                End If
                Dim S1516 As String = jP15.SelectToken(".Std16")
                If S1516 = "" Or S1516 = "0" Or S1516 Is Nothing Then
                    mLice15.Std16 = 0
                Else
                    mLice15.Std16 = S1516.Substring(0, 3)
                End If
                Dim S1517 As String = jP15.SelectToken(".Std17")
                If S1517 = "" Or S1517 = "0" Or S1517 Is Nothing Then
                    mLice15.Std17 = 0
                Else
                    mLice15.Std17 = S1517.Substring(0, 3)
                End If

                Dim S1518 As String = jP15.SelectToken(".Std18")
                If S1518 = "" Or S1518 = "0" Or S1518 Is Nothing Then
                    mLice15.Std18 = 0
                Else
                    mLice15.Std18 = S1518.Substring(0, 3)
                End If
                Dim S1519 As String = jP15.SelectToken(".Std19")
                If S1519 = "" Or S1519 = "0" Or S1519 Is Nothing Then
                    mLice15.Std19 = 0
                Else
                    mLice15.Std19 = S1519.Substring(0, 1)
                End If
                mLice15.Stactv = jP15.SelectToken(".Stactv")
                mLice15.Sttm = jP15.SelectToken(".Sttm")
                mLice15.inPoslBroj = jP15.SelectToken(".inPoslBroj")
                Session("mLice15") = mLice15
            Case 16
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If
                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If
                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If

                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")
                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If

                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If

                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If
                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)

                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If
                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If

                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If

                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If
                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")

                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If

                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)

                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If

                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If
                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")
                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If
                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If

                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If
                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If
                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If

                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")

                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
                '15
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.StConn = jP15.SelectToken(".StConn")
                mLice15.StUsp = jP15.SelectToken(".StUsp")
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.Strel = jP15.SelectToken(".Strel")
                mLice15.Sthusb = jP15.SelectToken(".Sthusb")
                mLice15.Stfather = jP15.SelectToken(".Stfather")
                mLice15.Stmother = jP15.SelectToken(".Stmother")
                mLice15.Stsex = jP15.SelectToken(".Stsex")
                mLice15.Stdb = jP15.SelectToken(".Stdb")
                mLice15.Stmb = jP15.SelectToken(".Stmb")
                mLice15.Styb = jP15.SelectToken(".Styb")
                mLice15.Stage = jP15.SelectToken(".Stage")
                Dim S1511 As String = jP15.SelectToken(".Std11")
                mLice15.Std11 = S1511.Substring(0, 3)
                Dim S1512 As String = jP15.SelectToken(".Std12")
                If S1512 = "" Or S1512 = "0" Or S1512 Is Nothing Then
                    S1512 = 0
                    mLice15.Std12 = S1512.Substring(0, 3)
                Else
                    mLice15.Std12 = S1512.Substring(0, 3)
                End If
                Dim S1513 As String = jP15.SelectToken(".Std13")
                If S1513 = "" Or S1513 Is Nothing Then
                    mLice15.Std13 = 0
                Else
                    mLice15.Std13 = S1513.Substring(0, 1)
                End If
                Dim S1514 As String = jP15.SelectToken(".Std14")
                If S1514 = "" Or S1514 Is Nothing Then
                    mLice15.Std14 = 0
                Else
                    mLice15.Std14 = S1514.Substring(0, 1)
                End If
                Dim S1515 As String = jP15.SelectToken(".Std15")
                If S1515 = "" Or S1515 Is Nothing Then
                    mLice15.Std15 = 0
                Else
                    mLice15.Std15 = S1515
                End If
                Dim S1516 As String = jP15.SelectToken(".Std16")
                If S1516 = "" Or S1516 = "0" Or S1516 Is Nothing Then
                    mLice15.Std16 = 0
                Else
                    mLice15.Std16 = S1516.Substring(0, 3)
                End If
                Dim S1517 As String = jP15.SelectToken(".Std17")
                If S1517 = "" Or S1517 = "0" Or S1517 Is Nothing Then
                    mLice15.Std17 = 0
                Else
                    mLice15.Std17 = S1517.Substring(0, 3)
                End If

                Dim S1518 As String = jP15.SelectToken(".Std18")
                If S1518 = "" Or S1518 = "0" Or S1518 Is Nothing Then
                    mLice15.Std18 = 0
                Else
                    mLice15.Std18 = S1518.Substring(0, 3)
                End If
                Dim S1519 As String = jP15.SelectToken(".Std19")
                If S1519 = "" Or S1519 = "0" Or S1519 Is Nothing Then
                    mLice15.Std19 = 0
                Else
                    mLice15.Std19 = S1519.Substring(0, 1)
                End If
                mLice15.Stactv = jP15.SelectToken(".Stactv")
                mLice15.Sttm = jP15.SelectToken(".Sttm")
                mLice15.inPoslBroj = jP15.SelectToken(".inPoslBroj")
                Session("mLice15") = mLice15
                '16
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.StConn = jP16.SelectToken(".StConn")
                mLice16.StUsp = jP16.SelectToken(".StUsp")
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.Strel = jP16.SelectToken(".Strel")
                mLice16.Sthusb = jP16.SelectToken(".Sthusb")
                mLice16.Stfather = jP16.SelectToken(".Stfather")
                mLice16.Stmother = jP16.SelectToken(".Stmother")
                mLice16.Stsex = jP16.SelectToken(".Stsex")
                mLice16.Stdb = jP16.SelectToken(".Stdb")
                mLice16.Stmb = jP16.SelectToken(".Stmb")
                mLice16.Styb = jP16.SelectToken(".Styb")
                mLice16.Stage = jP16.SelectToken(".Stage")
                Dim S1611 As String = jP16.SelectToken(".Std11")
                mLice16.Std11 = S1611.Substring(0, 3)
                Dim S1612 As String = jP16.SelectToken(".Std12")
                If S1612 = "" Or S1612 = "0" Or S1612 Is Nothing Then
                    S1612 = 0
                    mLice16.Std12 = S1612.Substring(0, 3)
                Else
                    mLice16.Std12 = S1612.Substring(0, 3)
                End If
                Dim S1613 As String = jP16.SelectToken(".Std13")
                If S1613 = "" Or S1613 Is Nothing Then
                    mLice16.Std13 = 0
                Else
                    mLice16.Std13 = S1613.Substring(0, 1)
                End If

                Dim S1614 As String = jP16.SelectToken(".Std14")
                If S1614 = "" Or S1614 Is Nothing Then
                    mLice16.Std14 = 0
                Else
                    mLice16.Std14 = S1614.Substring(0, 1)
                End If
                Dim S1615 As String = jP16.SelectToken(".Std15")
                If S1615 = "" Or S1615 Is Nothing Then
                    mLice16.Std15 = 0
                Else
                    mLice16.Std15 = S1515
                End If
                Dim S1616 As String = jP16.SelectToken(".Std16")
                If S1616 = "" Or S1616 = "0" Or S1616 Is Nothing Then
                    mLice16.Std16 = 0
                Else
                    mLice16.Std16 = S1616.Substring(0, 3)
                End If
                Dim S1617 As String = jP16.SelectToken(".Std17")
                If S1617 = "" Or S1617 = "0" Or S1617 Is Nothing Then
                    mLice16.Std17 = 0
                Else
                    mLice16.Std17 = S1617.Substring(0, 3)
                End If
                Dim S1618 As String = jP16.SelectToken(".Std18")
                If S1618 = "" Or S1618 = "0" Or S1618 Is Nothing Then
                    mLice16.Std18 = 0
                Else
                    mLice16.Std18 = S1618.Substring(0, 3)
                End If
                Dim S1619 As String = jP16.SelectToken(".Std19")
                If S1619 = "" Or S1619 = "0" Or S1619 Is Nothing Then
                    mLice16.Std19 = 0
                Else
                    mLice16.Std19 = S1619.Substring(0, 1)
                End If
                mLice16.Stactv = jP16.SelectToken(".Stactv")
                mLice16.Sttm = jP16.SelectToken(".Sttm")
                mLice16.inPoslBroj = jP16.SelectToken(".inPoslBroj")
                Session("mLice16") = mLice16

            Case 17
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If
                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If

                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If
                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If

                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")

                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")
                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If

                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If
                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")

                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If

                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")

                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If
                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If

                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If
                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If

                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")

                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If

                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If

                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If

                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If

                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If
                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")
                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If
                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If
                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If

                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If

                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If

                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")

                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
                '15
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.StConn = jP15.SelectToken(".StConn")
                mLice15.StUsp = jP15.SelectToken(".StUsp")
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.Strel = jP15.SelectToken(".Strel")
                mLice15.Sthusb = jP15.SelectToken(".Sthusb")
                mLice15.Stfather = jP15.SelectToken(".Stfather")
                mLice15.Stmother = jP15.SelectToken(".Stmother")
                mLice15.Stsex = jP15.SelectToken(".Stsex")
                mLice15.Stdb = jP15.SelectToken(".Stdb")
                mLice15.Stmb = jP15.SelectToken(".Stmb")
                mLice15.Styb = jP15.SelectToken(".Styb")
                mLice15.Stage = jP15.SelectToken(".Stage")
                Dim S1511 As String = jP15.SelectToken(".Std11")
                mLice15.Std11 = S1511.Substring(0, 3)
                Dim S1512 As String = jP15.SelectToken(".Std12")
                If S1512 = "" Or S1512 = "0" Or S1512 Is Nothing Then
                    S1512 = 0
                    mLice15.Std12 = S1512.Substring(0, 3)
                Else
                    mLice15.Std12 = S1512.Substring(0, 3)
                End If
                Dim S1513 As String = jP15.SelectToken(".Std13")
                If S1513 = "" Or S1513 Is Nothing Then
                    mLice15.Std13 = 0
                Else
                    mLice15.Std13 = S1513.Substring(0, 1)
                End If

                Dim S1514 As String = jP15.SelectToken(".Std14")
                If S1514 = "" Or S1514 Is Nothing Then
                    mLice15.Std14 = 0
                Else
                    mLice15.Std14 = S1514.Substring(0, 1)
                End If
                Dim S1515 As String = jP15.SelectToken(".Std15")
                If S1515 = "" Or S1515 Is Nothing Then
                    mLice15.Std15 = 0
                Else
                    mLice15.Std15 = S1515
                End If
                Dim S1516 As String = jP15.SelectToken(".Std16")
                If S1516 = "" Or S1516 = "0" Or S1516 Is Nothing Then
                    mLice15.Std16 = 0
                Else
                    mLice15.Std16 = S1516.Substring(0, 3)
                End If
                Dim S1517 As String = jP15.SelectToken(".Std17")
                If S1517 = "" Or S1517 = "0" Or S1517 Is Nothing Then
                    mLice15.Std17 = 0
                Else
                    mLice15.Std17 = S1517.Substring(0, 3)
                End If
                Dim S1518 As String = jP15.SelectToken(".Std18")
                If S1518 = "" Or S1518 = "0" Or S1518 Is Nothing Then
                    mLice15.Std18 = 0
                Else
                    mLice15.Std18 = S1518.Substring(0, 3)
                End If

                Dim S1519 As String = jP15.SelectToken(".Std19")
                If S1519 = "" Or S1519 = "0" Or S1519 Is Nothing Then
                    mLice15.Std19 = 0
                Else
                    mLice15.Std19 = S1519.Substring(0, 1)
                End If
                mLice15.Stactv = jP15.SelectToken(".Stactv")
                mLice15.Sttm = jP15.SelectToken(".Sttm")
                mLice15.inPoslBroj = jP15.SelectToken(".inPoslBroj")
                Session("mLice15") = mLice15
                '16
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.StConn = jP16.SelectToken(".StConn")
                mLice16.StUsp = jP16.SelectToken(".StUsp")
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.Strel = jP16.SelectToken(".Strel")
                mLice16.Sthusb = jP16.SelectToken(".Sthusb")
                mLice16.Stfather = jP16.SelectToken(".Stfather")
                mLice16.Stmother = jP16.SelectToken(".Stmother")
                mLice16.Stsex = jP16.SelectToken(".Stsex")
                mLice16.Stdb = jP16.SelectToken(".Stdb")
                mLice16.Stmb = jP16.SelectToken(".Stmb")
                mLice16.Styb = jP16.SelectToken(".Styb")
                mLice16.Stage = jP16.SelectToken(".Stage")
                Dim S1611 As String = jP16.SelectToken(".Std11")
                mLice16.Std11 = S1611.Substring(0, 3)
                Dim S1612 As String = jP16.SelectToken(".Std12")
                If S1612 = "" Or S1612 = "0" Or S1612 Is Nothing Then
                    S1612 = 0
                    mLice16.Std12 = S1612.Substring(0, 3)
                Else
                    mLice16.Std12 = S1612.Substring(0, 3)
                End If
                Dim S1613 As String = jP16.SelectToken(".Std13")
                If S1613 = "" Or S1613 Is Nothing Then
                    mLice16.Std13 = 0
                Else
                    mLice16.Std13 = S1613.Substring(0, 1)
                End If

                Dim S1614 As String = jP16.SelectToken(".Std14")
                If S1614 = "" Or S1614 Is Nothing Then
                    mLice16.Std14 = 0
                Else
                    mLice16.Std14 = S1614.Substring(0, 1)
                End If
                Dim S1615 As String = jP16.SelectToken(".Std15")
                If S1615 = "" Or S1615 Is Nothing Then
                    mLice16.Std15 = 0
                Else
                    mLice16.Std15 = S1515
                End If
                Dim S1616 As String = jP16.SelectToken(".Std16")
                If S1616 = "" Or S1616 = "0" Or S1616 Is Nothing Then
                    mLice16.Std16 = 0
                Else
                    mLice16.Std16 = S1616.Substring(0, 3)
                End If

                Dim S1617 As String = jP16.SelectToken(".Std17")
                If S1617 = "" Or S1617 = "0" Or S1617 Is Nothing Then
                    mLice16.Std17 = 0
                Else
                    mLice16.Std17 = S1617.Substring(0, 3)
                End If
                Dim S1618 As String = jP16.SelectToken(".Std18")
                If S1618 = "" Or S1618 = "0" Or S1618 Is Nothing Then
                    mLice16.Std18 = 0
                Else
                    mLice16.Std18 = S1618.Substring(0, 3)
                End If
                Dim S1619 As String = jP16.SelectToken(".Std19")
                If S1619 = "" Or S1619 = "0" Or S1619 Is Nothing Then
                    mLice16.Std19 = 0
                Else
                    mLice16.Std19 = S1619.Substring(0, 1)
                End If
                mLice16.Stactv = jP16.SelectToken(".Stactv")
                mLice16.Sttm = jP16.SelectToken(".Sttm")
                mLice16.inPoslBroj = jP16.SelectToken(".inPoslBroj")
                Session("mLice16") = mLice16
                '17
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.StConn = jP17.SelectToken(".StConn")
                mLice17.StUsp = jP17.SelectToken(".StUsp")
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.Strel = jP17.SelectToken(".Strel")
                mLice17.Sthusb = jP17.SelectToken(".Sthusb")
                mLice17.Stfather = jP17.SelectToken(".Stfather")
                mLice17.Stmother = jP17.SelectToken(".Stmother")
                mLice17.Stsex = jP17.SelectToken(".Stsex")
                mLice17.Stdb = jP17.SelectToken(".Stdb")
                mLice17.Stmb = jP17.SelectToken(".Stmb")
                mLice17.Styb = jP17.SelectToken(".Styb")
                mLice17.Stage = jP17.SelectToken(".Stage")
                Dim S1711 As String = jP17.SelectToken(".Std11")
                mLice17.Std11 = S1711.Substring(0, 3)
                Dim S1712 As String = jP17.SelectToken(".Std12")
                If S1712 = "" Or S1712 = "0" Or S1712 Is Nothing Then
                    S1712 = 0
                    mLice17.Std12 = S1712.Substring(0, 3)
                Else
                    mLice17.Std12 = S1712.Substring(0, 3)
                End If
                Dim S1713 As String = jP17.SelectToken(".Std13")
                If S1713 = "" Or S1713 Is Nothing Then
                    mLice17.Std13 = 0
                Else
                    mLice17.Std13 = S1713.Substring(0, 1)
                End If
                Dim S1714 As String = jP17.SelectToken(".Std14")
                If S1714 = "" Or S1714 Is Nothing Then
                    mLice17.Std14 = 0
                Else
                    mLice17.Std14 = S1714.Substring(0, 1)
                End If
                Dim S1715 As String = jP17.SelectToken(".Std15")
                If S1715 = "" Or S1715 Is Nothing Then
                    mLice17.Std15 = 0
                Else
                    mLice17.Std15 = S1715
                End If

                Dim S1716 As String = jP17.SelectToken(".Std16")
                If S1716 = "" Or S1716 = "0" Or S1716 Is Nothing Then
                    mLice17.Std16 = 0
                Else
                    mLice17.Std16 = S1716.Substring(0, 3)
                End If
                Dim S1717 As String = jP17.SelectToken(".Std17")
                If S1717 = "" Or S1717 = "0" Or S1717 Is Nothing Then
                    mLice17.Std17 = 0
                Else
                    mLice17.Std17 = S1717.Substring(0, 3)
                End If
                Dim S1718 As String = jP17.SelectToken(".Std18")
                If S1718 = "" Or S1718 = "0" Or S1718 Is Nothing Then
                    mLice17.Std18 = 0
                Else
                    mLice17.Std18 = S1718.Substring(0, 3)
                End If
                Dim S1719 As String = jP17.SelectToken(".Std19")
                If S1719 = "" Or S1719 = "0" Or S1719 Is Nothing Then
                    mLice17.Std19 = 0
                Else
                    mLice17.Std19 = S1719.Substring(0, 1)
                End If
                mLice17.Stactv = jP17.SelectToken(".Stactv")

                mLice17.Sttm = jP17.SelectToken(".Sttm")
                mLice17.inPoslBroj = jP17.SelectToken(".inPoslBroj")
                Session("mLice17") = mLice17
            Case 18
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)
                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If
                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If
                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If

                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")
                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If
                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If
                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If
                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)

                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If

                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")

                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If

                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If

                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If

                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If

                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If
                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If
                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)
                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If
                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If
                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")

                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)
                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If
                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If

                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If
                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If
                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")

                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If

                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If
                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If
                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If

                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)

                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If

                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If
                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If
                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If

                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If
                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If
                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If

                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If

                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If
                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If

                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If
                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If
                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If

                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If
                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")
                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If
                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If
                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If
                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If
                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If
                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")

                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
                '15
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.StConn = jP15.SelectToken(".StConn")
                mLice15.StUsp = jP15.SelectToken(".StUsp")
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.Strel = jP15.SelectToken(".Strel")
                mLice15.Sthusb = jP15.SelectToken(".Sthusb")
                mLice15.Stfather = jP15.SelectToken(".Stfather")
                mLice15.Stmother = jP15.SelectToken(".Stmother")
                mLice15.Stsex = jP15.SelectToken(".Stsex")
                mLice15.Stdb = jP15.SelectToken(".Stdb")
                mLice15.Stmb = jP15.SelectToken(".Stmb")
                mLice15.Styb = jP15.SelectToken(".Styb")
                mLice15.Stage = jP15.SelectToken(".Stage")
                Dim S1511 As String = jP15.SelectToken(".Std11")
                mLice15.Std11 = S1511.Substring(0, 3)

                Dim S1512 As String = jP15.SelectToken(".Std12")
                If S1512 = "" Or S1512 = "0" Or S1512 Is Nothing Then
                    S1512 = 0
                    mLice15.Std12 = S1512.Substring(0, 3)
                Else
                    mLice15.Std12 = S1512.Substring(0, 3)
                End If
                Dim S1513 As String = jP15.SelectToken(".Std13")
                If S1513 = "" Or S1513 Is Nothing Then
                    mLice15.Std13 = 0
                Else
                    mLice15.Std13 = S1513.Substring(0, 1)
                End If

                Dim S1514 As String = jP15.SelectToken(".Std14")
                If S1514 = "" Or S1514 Is Nothing Then
                    mLice15.Std14 = 0
                Else
                    mLice15.Std14 = S1514.Substring(0, 1)
                End If
                Dim S1515 As String = jP15.SelectToken(".Std15")
                If S1515 = "" Or S1515 Is Nothing Then
                    mLice15.Std15 = 0
                Else
                    mLice15.Std15 = S1515
                End If
                Dim S1516 As String = jP15.SelectToken(".Std16")
                If S1516 = "" Or S1516 = "0" Or S1516 Is Nothing Then
                    mLice15.Std16 = 0
                Else
                    mLice15.Std16 = S1516.Substring(0, 3)
                End If

                Dim S1517 As String = jP15.SelectToken(".Std17")
                If S1517 = "" Or S1517 = "0" Or S1517 Is Nothing Then
                    mLice15.Std17 = 0
                Else
                    mLice15.Std17 = S1517.Substring(0, 3)
                End If
                Dim S1518 As String = jP15.SelectToken(".Std18")
                If S1518 = "" Or S1518 = "0" Or S1518 Is Nothing Then
                    mLice15.Std18 = 0
                Else
                    mLice15.Std18 = S1518.Substring(0, 3)
                End If
                Dim S1519 As String = jP15.SelectToken(".Std19")
                If S1519 = "" Or S1519 = "0" Or S1519 Is Nothing Then
                    mLice15.Std19 = 0
                Else
                    mLice15.Std19 = S1519.Substring(0, 1)
                End If
                mLice15.Stactv = jP15.SelectToken(".Stactv")
                mLice15.Sttm = jP15.SelectToken(".Sttm")
                mLice15.inPoslBroj = jP15.SelectToken(".inPoslBroj")
                Session("mLice15") = mLice15
                '16
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.StConn = jP16.SelectToken(".StConn")
                mLice16.StUsp = jP16.SelectToken(".StUsp")
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.Strel = jP16.SelectToken(".Strel")
                mLice16.Sthusb = jP16.SelectToken(".Sthusb")
                mLice16.Stfather = jP16.SelectToken(".Stfather")
                mLice16.Stmother = jP16.SelectToken(".Stmother")
                mLice16.Stsex = jP16.SelectToken(".Stsex")
                mLice16.Stdb = jP16.SelectToken(".Stdb")
                mLice16.Stmb = jP16.SelectToken(".Stmb")
                mLice16.Styb = jP16.SelectToken(".Styb")
                mLice16.Stage = jP16.SelectToken(".Stage")
                Dim S1611 As String = jP16.SelectToken(".Std11")
                mLice16.Std11 = S1611.Substring(0, 3)
                Dim S1612 As String = jP16.SelectToken(".Std12")
                If S1612 = "" Or S1612 = "0" Or S1612 Is Nothing Then
                    S1612 = 0
                    mLice16.Std12 = S1612.Substring(0, 3)
                Else
                    mLice16.Std12 = S1612.Substring(0, 3)
                End If
                Dim S1613 As String = jP16.SelectToken(".Std13")
                If S1613 = "" Or S1613 Is Nothing Then
                    mLice16.Std13 = 0
                Else
                    mLice16.Std13 = S1613.Substring(0, 1)
                End If
                Dim S1614 As String = jP16.SelectToken(".Std14")
                If S1614 = "" Or S1614 Is Nothing Then
                    mLice16.Std14 = 0
                Else
                    mLice16.Std14 = S1614.Substring(0, 1)
                End If
                Dim S1615 As String = jP16.SelectToken(".Std15")
                If S1615 = "" Or S1615 Is Nothing Then
                    mLice16.Std15 = 0
                Else
                    mLice16.Std15 = S1515
                End If

                Dim S1616 As String = jP16.SelectToken(".Std16")
                If S1616 = "" Or S1616 = "0" Or S1616 Is Nothing Then
                    mLice16.Std16 = 0
                Else
                    mLice16.Std16 = S1616.Substring(0, 3)
                End If
                Dim S1617 As String = jP16.SelectToken(".Std17")
                If S1617 = "" Or S1617 = "0" Or S1617 Is Nothing Then
                    mLice16.Std17 = 0
                Else
                    mLice16.Std17 = S1617.Substring(0, 3)
                End If
                Dim S1618 As String = jP16.SelectToken(".Std18")
                If S1618 = "" Or S1618 = "0" Or S1618 Is Nothing Then
                    mLice16.Std18 = 0
                Else
                    mLice16.Std18 = S1618.Substring(0, 3)
                End If
                Dim S1619 As String = jP16.SelectToken(".Std19")
                If S1619 = "" Or S1619 = "0" Or S1619 Is Nothing Then
                    mLice16.Std19 = 0
                Else
                    mLice16.Std19 = S1619.Substring(0, 1)
                End If
                mLice16.Stactv = jP16.SelectToken(".Stactv")
                mLice16.Sttm = jP16.SelectToken(".Sttm")
                mLice16.inPoslBroj = jP16.SelectToken(".inPoslBroj")
                Session("mLice16") = mLice16
                '17
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.StConn = jP17.SelectToken(".StConn")
                mLice17.StUsp = jP17.SelectToken(".StUsp")
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.Strel = jP17.SelectToken(".Strel")
                mLice17.Sthusb = jP17.SelectToken(".Sthusb")
                mLice17.Stfather = jP17.SelectToken(".Stfather")
                mLice17.Stmother = jP17.SelectToken(".Stmother")
                mLice17.Stsex = jP17.SelectToken(".Stsex")
                mLice17.Stdb = jP17.SelectToken(".Stdb")
                mLice17.Stmb = jP17.SelectToken(".Stmb")
                mLice17.Styb = jP17.SelectToken(".Styb")
                mLice17.Stage = jP17.SelectToken(".Stage")
                Dim S1711 As String = jP17.SelectToken(".Std11")
                mLice17.Std11 = S1711.Substring(0, 3)
                Dim S1712 As String = jP17.SelectToken(".Std12")
                If S1712 = "" Or S1712 = "0" Or S1712 Is Nothing Then
                    S1712 = 0
                    mLice17.Std12 = S1712.Substring(0, 3)
                Else
                    mLice17.Std12 = S1712.Substring(0, 3)
                End If
                Dim S1713 As String = jP17.SelectToken(".Std13")
                If S1713 = "" Or S1713 Is Nothing Then
                    mLice17.Std13 = 0
                Else
                    mLice17.Std13 = S1713.Substring(0, 1)
                End If
                Dim S1714 As String = jP17.SelectToken(".Std14")
                If S1714 = "" Or S1714 Is Nothing Then
                    mLice17.Std14 = 0
                Else
                    mLice17.Std14 = S1714.Substring(0, 1)
                End If
                Dim S1715 As String = jP17.SelectToken(".Std15")
                If S1715 = "" Or S1715 Is Nothing Then
                    mLice17.Std15 = 0
                Else
                    mLice17.Std15 = S1715
                End If
                Dim S1716 As String = jP17.SelectToken(".Std16")
                If S1716 = "" Or S1716 = "0" Or S1716 Is Nothing Then
                    mLice17.Std16 = 0
                Else
                    mLice17.Std16 = S1716.Substring(0, 3)
                End If
                Dim S1717 As String = jP17.SelectToken(".Std17")
                If S1717 = "" Or S1717 = "0" Or S1717 Is Nothing Then
                    mLice17.Std17 = 0
                Else
                    mLice17.Std17 = S1717.Substring(0, 3)
                End If

                Dim S1718 As String = jP17.SelectToken(".Std18")
                If S1718 = "" Or S1718 = "0" Or S1718 Is Nothing Then
                    mLice17.Std18 = 0
                Else
                    mLice17.Std18 = S1718.Substring(0, 3)
                End If

                Dim S1719 As String = jP17.SelectToken(".Std19")
                If S1719 = "" Or S1719 = "0" Or S1719 Is Nothing Then
                    mLice17.Std19 = 0
                Else
                    mLice17.Std19 = S1719.Substring(0, 1)
                End If
                mLice17.Stactv = jP17.SelectToken(".Stactv")

                mLice17.Sttm = jP17.SelectToken(".Sttm")
                mLice17.inPoslBroj = jP17.SelectToken(".inPoslBroj")
                Session("mLice17") = mLice17
                '18
                mLice18.Stlice = jP18.SelectToken(".Stlice")
                mLice18.StConn = jP18.SelectToken(".StConn")
                mLice18.StUsp = jP18.SelectToken(".StUsp")
                mLice18.Stlice = jP18.SelectToken(".Stlice")
                mLice18.Strel = jP18.SelectToken(".Strel")
                mLice18.Sthusb = jP18.SelectToken(".Sthusb")
                mLice18.Stfather = jP18.SelectToken(".Stfather")
                mLice18.Stmother = jP18.SelectToken(".Stmother")
                mLice18.Stsex = jP18.SelectToken(".Stsex")
                mLice18.Stdb = jP18.SelectToken(".Stdb")
                mLice18.Stmb = jP18.SelectToken(".Stmb")
                mLice18.Styb = jP18.SelectToken(".Styb")
                mLice18.Stage = jP18.SelectToken(".Stage")
                Dim S1811 As String = jP18.SelectToken(".Std11")
                mLice18.Std11 = S1811.Substring(0, 3)
                Dim S1812 As String = jP18.SelectToken(".Std12")
                If S1812 = "" Or S1812 = "0" Or S1812 Is Nothing Then
                    S1812 = 0
                    mLice18.Std12 = S1812.Substring(0, 3)
                Else
                    mLice18.Std12 = S1812.Substring(0, 3)
                End If
                Dim S1813 As String = jP18.SelectToken(".Std13")
                If S1813 = "" Or S1813 Is Nothing Then
                    mLice18.Std13 = 0
                Else
                    mLice18.Std13 = S1813.Substring(0, 1)
                End If
                Dim S1814 As String = jP18.SelectToken(".Std14")
                If S1814 = "" Or S1814 Is Nothing Then
                    mLice18.Std14 = 0
                Else
                    mLice18.Std14 = S1814.Substring(0, 1)
                End If
                Dim S1815 As String = jP18.SelectToken(".Std15")
                If S1815 = "" Or S1815 Is Nothing Then
                    mLice18.Std15 = 0
                Else
                    mLice18.Std15 = S1815
                End If
                Dim S1816 As String = jP18.SelectToken(".Std16")
                If S1816 = "" Or S1816 = "0" Or S1816 Is Nothing Then
                    mLice18.Std16 = 0
                Else
                    mLice18.Std16 = S1816.Substring(0, 3)
                End If

                Dim S1817 As String = jP18.SelectToken(".Std17")
                If S1817 = "" Or S1817 = "0" Or S1817 Is Nothing Then
                    mLice18.Std17 = 0
                Else
                    mLice18.Std17 = S1817.Substring(0, 3)
                End If
                Dim S1818 As String = jP18.SelectToken(".Std18")
                If S1818 = "" Or S1818 = "0" Or S1818 Is Nothing Then
                    mLice18.Std18 = 0
                Else
                    mLice18.Std18 = S1818.Substring(0, 3)
                End If
                Dim S1819 As String = jP18.SelectToken(".Std19")
                If S1819 = "" Or S1819 = "0" Or S1819 Is Nothing Then
                    mLice18.Std19 = 0
                Else
                    mLice18.Std19 = S1819.Substring(0, 1)
                End If
                mLice18.Stactv = jP18.SelectToken(".Stactv")
                mLice18.Sttm = jP18.SelectToken(".Sttm")
                mLice18.inPoslBroj = jP18.SelectToken(".inPoslBroj")
                Session("mLice18") = mLice18
            Case 19
                '1
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.StConn = jP1.SelectToken(".StConn")
                mLice1.StUsp = jP1.SelectToken(".StUsp")
                mLice1.Stlice = jP1.SelectToken(".Stlice")
                mLice1.Strel = jP1.SelectToken(".Strel")
                mLice1.Sthusb = jP1.SelectToken(".Sthusb")
                mLice1.Stfather = jP1.SelectToken(".Stfather")
                mLice1.Stmother = jP1.SelectToken(".Stmother")
                mLice1.Stsex = jP1.SelectToken(".Stsex")
                mLice1.Stdb = jP1.SelectToken(".Stdb")
                mLice1.Stmb = jP1.SelectToken(".Stmb")
                mLice1.Styb = jP1.SelectToken(".Styb")
                mLice1.Stage = jP1.SelectToken(".Stage")
                Dim S11 As String = jP1.SelectToken(".Std11")
                mLice1.Std11 = S11.Substring(0, 3)

                Dim S12 As String = jP1.SelectToken(".Std12")
                If S12 = "" Or S12 = "0" Or S12 Is Nothing Then
                    S12 = 0
                    mLice1.Std12 = S12.Substring(0, 3)
                Else
                    mLice1.Std12 = S12.Substring(0, 3)
                End If

                Dim S13 As String = jP1.SelectToken(".Std13")
                If S13 = "" Or S13 Is Nothing Then
                    mLice1.Std13 = 0
                Else
                    mLice1.Std13 = S13.Substring(0, 1)
                End If
                Dim S14 As String = jP1.SelectToken(".Std14")
                If S14 = "" Or S14 Is Nothing Then
                    mLice1.Std14 = 0
                Else
                    mLice1.Std14 = S14.Substring(0, 1)
                End If

                Dim S15 As String = jP1.SelectToken(".Std15")
                If S15 = "" Or S15 Is Nothing Then
                    mLice1.Std15 = 0
                Else
                    mLice1.Std15 = S15
                End If
                Dim S16 As String = jP1.SelectToken(".Std16")
                If S16 = "" Or S16 = "0" Or S16 Is Nothing Then
                    mLice1.Std16 = 0
                Else
                    mLice1.Std16 = S16.Substring(0, 3)
                End If
                Dim S17 As String = jP1.SelectToken(".Std17")
                If S17 = "" Or S17 = "0" Or S17 Is Nothing Then
                    mLice1.Std17 = 0
                Else
                    mLice1.Std17 = S17.Substring(0, 3)
                End If
                Dim S18 As String = jP1.SelectToken(".Std18")
                If S18 = "" Or S18 = "0" Or S18 Is Nothing Then
                    mLice1.Std18 = 0
                Else
                    mLice1.Std18 = S18.Substring(0, 3)
                End If
                Dim S19 As String = jP1.SelectToken(".Std19")
                If S19 = "" Or S19 = "0" Or S19 Is Nothing Then
                    mLice1.Std19 = 0
                Else
                    mLice1.Std19 = S19.Substring(0, 1)
                End If
                mLice1.Stactv = jP1.SelectToken(".Stactv")

                mLice1.Sttm = jP1.SelectToken(".Sttm")
                mLice1.inPoslBroj = jP1.SelectToken(".inPoslBroj")
                Session("mLice1") = mLice1
                '2

                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.StConn = jP2.SelectToken(".StConn")
                mLice2.StUsp = jP2.SelectToken(".StUsp")
                mLice2.Stlice = jP2.SelectToken(".Stlice")
                mLice2.Strel = jP2.SelectToken(".Strel")
                mLice2.Sthusb = jP2.SelectToken(".Sthusb")
                mLice2.Stfather = jP2.SelectToken(".Stfather")
                mLice2.Stmother = jP2.SelectToken(".Stmother")
                mLice2.Stsex = jP2.SelectToken(".Stsex")
                mLice2.Stdb = jP2.SelectToken(".Stdb")
                mLice2.Stmb = jP2.SelectToken(".Stmb")
                mLice2.Styb = jP2.SelectToken(".Styb")
                mLice2.Stage = jP2.SelectToken(".Stage")
                Dim S211 As String = jP2.SelectToken(".Std11")
                mLice2.Std11 = S211.Substring(0, 3)
                Dim S212 As String = jP2.SelectToken(".Std12")
                If S212 = "" Or S212 = "0" Or S12 Is Nothing Then
                    S212 = 0
                    mLice2.Std12 = S212.Substring(0, 3)
                Else
                    mLice2.Std12 = S212.Substring(0, 3)
                End If

                Dim S213 As String = jP2.SelectToken(".Std13")
                If S213 = "" Or S213 Is Nothing Then
                    mLice2.Std13 = 0
                Else
                    mLice2.Std13 = S213.Substring(0, 1)
                End If
                Dim S214 As String = jP2.SelectToken(".Std14")
                If S214 = "" Or S214 Is Nothing Then
                    mLice2.Std14 = 0
                Else
                    mLice2.Std14 = S214.Substring(0, 1)
                End If
                Dim S215 As String = jP2.SelectToken(".Std15")
                If S215 = "" Or S215 Is Nothing Then
                    mLice2.Std15 = 0
                Else
                    mLice2.Std15 = S15
                End If

                Dim S216 As String = jP2.SelectToken(".Std16")
                If S216 = "" Or S216 = "0" Or S216 Is Nothing Then
                    mLice2.Std16 = 0
                Else
                    mLice2.Std16 = S16.Substring(0, 3)
                End If
                Dim S217 As String = jP2.SelectToken(".Std17")
                If S217 = "" Or S217 = "0" Or S217 Is Nothing Then
                    mLice2.Std17 = 0
                Else
                    mLice2.Std17 = S217.Substring(0, 3)
                End If
                Dim S218 As String = jP2.SelectToken(".Std18")
                If S218 = "" Or S218 = "0" Or S218 Is Nothing Then
                    mLice2.Std18 = 0
                Else
                    mLice2.Std18 = S218.Substring(0, 3)
                End If

                Dim S219 As String = jP2.SelectToken(".Std19")
                If S219 = "" Or S219 = "0" Or S219 Is Nothing Then
                    mLice2.Std19 = 0
                Else
                    mLice2.Std19 = S219.Substring(0, 1)
                End If
                mLice2.Stactv = jP2.SelectToken(".Stactv")
                mLice2.Sttm = jP2.SelectToken(".Sttm")
                mLice2.inPoslBroj = jP2.SelectToken(".inPoslBroj")
                Session("mLice2") = mLice2
                '3
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.StConn = jP3.SelectToken(".StConn")
                mLice3.StUsp = jP3.SelectToken(".StUsp")
                mLice3.Stlice = jP3.SelectToken(".Stlice")
                mLice3.Strel = jP3.SelectToken(".Strel")
                mLice3.Sthusb = jP3.SelectToken(".Sthusb")
                mLice3.Stfather = jP3.SelectToken(".Stfather")
                mLice3.Stmother = jP3.SelectToken(".Stmother")
                mLice3.Stsex = jP3.SelectToken(".Stsex")
                mLice3.Stdb = jP3.SelectToken(".Stdb")
                mLice3.Stmb = jP3.SelectToken(".Stmb")
                mLice3.Styb = jP3.SelectToken(".Styb")
                mLice3.Stage = jP3.SelectToken(".Stage")
                Dim S311 As String = jP3.SelectToken(".Std11")
                mLice3.Std11 = S311.Substring(0, 3)
                Dim S312 As String = jP3.SelectToken(".Std12")
                If S312 = "" Or S312 = "0" Or S312 Is Nothing Then
                    S312 = 0
                    mLice3.Std12 = S312.Substring(0, 3)
                Else
                    mLice3.Std12 = S312.Substring(0, 3)
                End If
                Dim S313 As String = jP3.SelectToken(".Std13")
                If S313 = "" Or S313 Is Nothing Then
                    mLice3.Std13 = 0
                Else
                    mLice3.Std13 = S313.Substring(0, 1)
                End If
                Dim S314 As String = jP3.SelectToken(".Std14")
                If S314 = "" Or S314 Is Nothing Then
                    mLice3.Std14 = 0
                Else
                    mLice3.Std14 = S314.Substring(0, 1)
                End If
                Dim S315 As String = jP3.SelectToken(".Std15")
                If S315 = "" Or S315 Is Nothing Then
                    mLice3.Std15 = 0
                Else
                    mLice3.Std15 = S315
                End If

                Dim S316 As String = jP3.SelectToken(".Std16")
                If S316 = "" Or S316 = "0" Or S316 Is Nothing Then
                    mLice3.Std16 = 0
                Else
                    mLice3.Std16 = S316.Substring(0, 3)
                End If
                Dim S317 As String = jP3.SelectToken(".Std17")
                If S317 = "" Or S317 = "0" Or S317 Is Nothing Then
                    mLice3.Std17 = 0
                Else
                    mLice3.Std17 = S317.Substring(0, 3)
                End If
                Dim S318 As String = jP3.SelectToken(".Std18")
                If S318 = "" Or S318 = "0" Or S318 Is Nothing Then
                    mLice3.Std18 = 0
                Else
                    mLice3.Std18 = S318.Substring(0, 3)
                End If
                Dim S319 As String = jP3.SelectToken(".Std19")
                If S319 = "" Or S319 = "0" Or S319 Is Nothing Then
                    mLice3.Std19 = 0
                Else
                    mLice3.Std19 = S319.Substring(0, 1)
                End If
                mLice3.Stactv = jP3.SelectToken(".Stactv")
                mLice3.Sttm = jP3.SelectToken(".Sttm")
                mLice3.inPoslBroj = jP3.SelectToken(".inPoslBroj")
                Session("mLice3") = mLice3
                '4
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.StConn = jP4.SelectToken(".StConn")
                mLice4.StUsp = jP4.SelectToken(".StUsp")
                mLice4.Stlice = jP4.SelectToken(".Stlice")
                mLice4.Strel = jP4.SelectToken(".Strel")
                mLice4.Sthusb = jP4.SelectToken(".Sthusb")
                mLice4.Stfather = jP4.SelectToken(".Stfather")
                mLice4.Stmother = jP4.SelectToken(".Stmother")
                mLice4.Stsex = jP4.SelectToken(".Stsex")
                mLice4.Stdb = jP4.SelectToken(".Stdb")
                mLice4.Stmb = jP4.SelectToken(".Stmb")
                mLice4.Styb = jP4.SelectToken(".Styb")
                mLice4.Stage = jP4.SelectToken(".Stage")
                Dim S411 As String = jP4.SelectToken(".Std11")
                mLice4.Std11 = S411.Substring(0, 3)
                Dim S412 As String = jP4.SelectToken(".Std12")
                If S412 = "" Or S412 = "0" Or S412 Is Nothing Then
                    S412 = 0
                    mLice4.Std12 = S412.Substring(0, 3)
                Else
                    mLice4.Std12 = S412.Substring(0, 3)
                End If
                Dim S413 As String = jP4.SelectToken(".Std13")
                If S413 = "" Or S413 Is Nothing Then
                    mLice4.Std13 = 0
                Else
                    mLice4.Std13 = S413.Substring(0, 1)
                End If
                Dim S414 As String = jP4.SelectToken(".Std14")
                If S414 = "" Or S414 Is Nothing Then
                    mLice4.Std14 = 0
                Else
                    mLice4.Std14 = S414.Substring(0, 1)
                End If
                Dim S415 As String = jP4.SelectToken(".Std15")
                If S415 = "" Or S415 Is Nothing Then
                    mLice4.Std15 = 0
                Else
                    mLice4.Std15 = S415
                End If
                Dim S416 As String = jP4.SelectToken(".Std16")
                If S416 = "" Or S416 = "0" Or S416 Is Nothing Then
                    mLice4.Std16 = 0
                Else
                    mLice4.Std16 = S416.Substring(0, 3)
                End If
                Dim S417 As String = jP4.SelectToken(".Std17")
                If S417 = "" Or S417 = "0" Or S417 Is Nothing Then
                    mLice4.Std17 = 0
                Else
                    mLice4.Std17 = S417.Substring(0, 3)
                End If
                Dim S418 As String = jP4.SelectToken(".Std18")
                If S418 = "" Or S418 = "0" Or S418 Is Nothing Then
                    mLice4.Std18 = 0
                Else
                    mLice4.Std18 = S418.Substring(0, 3)
                End If
                Dim S419 As String = jP4.SelectToken(".Std19")
                If S419 = "" Or S419 = "0" Or S419 Is Nothing Then
                    mLice4.Std19 = 0
                Else
                    mLice4.Std19 = S419.Substring(0, 1)
                End If
                mLice4.Stactv = jP4.SelectToken(".Stactv")
                mLice4.Sttm = jP4.SelectToken(".Sttm")
                mLice4.inPoslBroj = jP4.SelectToken(".inPoslBroj")
                Session("mLice4") = mLice4
                '5
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.StConn = jP5.SelectToken(".StConn")
                mLice5.StUsp = jP5.SelectToken(".StUsp")
                mLice5.Stlice = jP5.SelectToken(".Stlice")
                mLice5.Strel = jP5.SelectToken(".Strel")
                mLice5.Sthusb = jP5.SelectToken(".Sthusb")
                mLice5.Stfather = jP5.SelectToken(".Stfather")
                mLice5.Stmother = jP5.SelectToken(".Stmother")
                mLice5.Stsex = jP5.SelectToken(".Stsex")
                mLice5.Stdb = jP5.SelectToken(".Stdb")
                mLice5.Stmb = jP5.SelectToken(".Stmb")
                mLice5.Styb = jP5.SelectToken(".Styb")
                mLice5.Stage = jP5.SelectToken(".Stage")
                Dim S511 As String = jP5.SelectToken(".Std11")
                mLice5.Std11 = S511.Substring(0, 3)
                Dim S512 As String = jP5.SelectToken(".Std12")
                If S512 = "" Or S512 = "0" Or S512 Is Nothing Then
                    S512 = 0
                    mLice5.Std12 = S512.Substring(0, 3)
                Else
                    mLice5.Std12 = S512.Substring(0, 3)
                End If
                Dim S513 As String = jP5.SelectToken(".Std13")
                If S513 = "" Or S513 Is Nothing Then
                    mLice5.Std13 = 0
                Else
                    mLice5.Std13 = S513.Substring(0, 1)
                End If
                Dim S514 As String = jP5.SelectToken(".Std14")
                If S514 = "" Or S514 Is Nothing Then
                    mLice5.Std14 = 0
                Else
                    mLice5.Std14 = S514.Substring(0, 1)
                End If
                Dim S515 As String = jP5.SelectToken(".Std15")
                If S515 = "" Or S515 Is Nothing Then
                    mLice5.Std15 = 0
                Else
                    mLice5.Std15 = S515
                End If
                Dim S516 As String = jP5.SelectToken(".Std16")
                If S516 = "" Or S516 = "0" Or S516 Is Nothing Then
                    mLice5.Std16 = 0
                Else
                    mLice5.Std16 = S516.Substring(0, 3)
                End If
                Dim S517 As String = jP5.SelectToken(".Std17")
                If S517 = "" Or S517 = "0" Or S517 Is Nothing Then
                    mLice5.Std17 = 0
                Else
                    mLice5.Std17 = S517.Substring(0, 3)
                End If
                Dim S518 As String = jP5.SelectToken(".Std18")
                If S518 = "" Or S518 = "0" Or S518 Is Nothing Then
                    mLice5.Std18 = 0
                Else
                    mLice5.Std18 = S518.Substring(0, 3)
                End If

                Dim S519 As String = jP5.SelectToken(".Std19")
                If S519 = "" Or S519 = "0" Or S519 Is Nothing Then
                    mLice5.Std19 = 0
                Else
                    mLice5.Std19 = S519.Substring(0, 1)
                End If
                mLice5.Stactv = jP5.SelectToken(".Stactv")
                mLice5.Sttm = jP5.SelectToken(".Sttm")
                mLice5.inPoslBroj = jP5.SelectToken(".inPoslBroj")
                Session("mLice5") = mLice5
                '6
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.StConn = jP6.SelectToken(".StConn")
                mLice6.StUsp = jP6.SelectToken(".StUsp")
                mLice6.Stlice = jP6.SelectToken(".Stlice")
                mLice6.Strel = jP6.SelectToken(".Strel")
                mLice6.Sthusb = jP6.SelectToken(".Sthusb")
                mLice6.Stfather = jP6.SelectToken(".Stfather")
                mLice6.Stmother = jP6.SelectToken(".Stmother")
                mLice6.Stsex = jP6.SelectToken(".Stsex")
                mLice6.Stdb = jP6.SelectToken(".Stdb")
                mLice6.Stmb = jP6.SelectToken(".Stmb")
                mLice6.Styb = jP6.SelectToken(".Styb")
                mLice6.Stage = jP6.SelectToken(".Stage")
                Dim S611 As String = jP6.SelectToken(".Std11")
                mLice6.Std11 = S611.Substring(0, 3)

                Dim S612 As String = jP6.SelectToken(".Std12")
                If S612 = "" Or S612 = "0" Or S612 Is Nothing Then
                    S612 = 0
                    mLice6.Std12 = S612.Substring(0, 3)
                Else
                    mLice6.Std12 = S612.Substring(0, 3)
                End If

                Dim S613 As String = jP6.SelectToken(".Std13")
                If S613 = "" Or S613 Is Nothing Then
                    mLice6.Std13 = 0
                Else
                    mLice6.Std13 = S613.Substring(0, 1)
                End If
                Dim S614 As String = jP6.SelectToken(".Std14")
                If S614 = "" Or S614 Is Nothing Then
                    mLice6.Std14 = 0
                Else
                    mLice6.Std14 = S614.Substring(0, 1)
                End If
                Dim S615 As String = jP6.SelectToken(".Std15")
                If S615 = "" Or S615 Is Nothing Then
                    mLice6.Std15 = 0
                Else
                    mLice6.Std15 = S615
                End If

                Dim S616 As String = jP6.SelectToken(".Std16")
                If S616 = "" Or S616 = "0" Or S616 Is Nothing Then
                    mLice6.Std16 = 0
                Else
                    mLice6.Std16 = S616.Substring(0, 3)
                End If
                Dim S617 As String = jP6.SelectToken(".Std17")
                If S617 = "" Or S617 = "0" Or S617 Is Nothing Then
                    mLice6.Std17 = 0
                Else
                    mLice6.Std17 = S617.Substring(0, 3)
                End If
                Dim S618 As String = jP6.SelectToken(".Std18")
                If S618 = "" Or S618 = "0" Or S618 Is Nothing Then
                    mLice6.Std18 = 0
                Else
                    mLice6.Std18 = S618.Substring(0, 3)
                End If
                Dim S619 As String = jP6.SelectToken(".Std19")
                If S619 = "" Or S619 = "0" Or S619 Is Nothing Then
                    mLice6.Std19 = 0
                Else
                    mLice6.Std19 = S619.Substring(0, 1)
                End If
                mLice6.Stactv = jP6.SelectToken(".Stactv")
                mLice6.Sttm = jP6.SelectToken(".Sttm")
                mLice6.inPoslBroj = jP6.SelectToken(".inPoslBroj")
                Session("mLice6") = mLice6
                '7
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.StConn = jP7.SelectToken(".StConn")
                mLice7.StUsp = jP7.SelectToken(".StUsp")
                mLice7.Stlice = jP7.SelectToken(".Stlice")
                mLice7.Strel = jP7.SelectToken(".Strel")
                mLice7.Sthusb = jP7.SelectToken(".Sthusb")
                mLice7.Stfather = jP7.SelectToken(".Stfather")
                mLice7.Stmother = jP7.SelectToken(".Stmother")
                mLice7.Stsex = jP7.SelectToken(".Stsex")
                mLice7.Stdb = jP7.SelectToken(".Stdb")
                mLice7.Stmb = jP7.SelectToken(".Stmb")
                mLice7.Styb = jP7.SelectToken(".Styb")
                mLice7.Stage = jP7.SelectToken(".Stage")
                Dim S711 As String = jP7.SelectToken(".Std11")
                mLice7.Std11 = S711.Substring(0, 3)

                Dim S712 As String = jP7.SelectToken(".Std12")
                If S712 = "" Or S712 = "0" Or S712 Is Nothing Then
                    S712 = 0
                    mLice7.Std12 = S712.Substring(0, 3)
                Else
                    mLice7.Std12 = S712.Substring(0, 3)
                End If
                Dim S713 As String = jP7.SelectToken(".Std13")
                If S713 = "" Or S713 Is Nothing Then
                    mLice7.Std13 = 0
                Else
                    mLice7.Std13 = S713.Substring(0, 1)
                End If

                Dim S714 As String = jP7.SelectToken(".Std14")
                If S714 = "" Or S714 Is Nothing Then
                    mLice7.Std14 = 0
                Else
                    mLice7.Std14 = S714.Substring(0, 1)
                End If
                Dim S715 As String = jP7.SelectToken(".Std15")
                If S715 = "" Or S715 Is Nothing Then
                    mLice7.Std15 = 0
                Else
                    mLice7.Std15 = S715
                End If
                Dim S716 As String = jP7.SelectToken(".Std16")
                If S716 = "" Or S716 = "0" Or S716 Is Nothing Then
                    mLice7.Std16 = 0
                Else
                    mLice7.Std16 = S716.Substring(0, 3)
                End If
                Dim S717 As String = jP7.SelectToken(".Std17")
                If S717 = "" Or S717 = "0" Or S717 Is Nothing Then
                    mLice7.Std17 = 0
                Else
                    mLice7.Std17 = S717.Substring(0, 3)
                End If

                Dim S718 As String = jP7.SelectToken(".Std18")
                If S718 = "" Or S718 = "0" Or S718 Is Nothing Then
                    mLice7.Std18 = 0
                Else
                    mLice7.Std18 = S718.Substring(0, 3)
                End If
                Dim S719 As String = jP7.SelectToken(".Std19")
                If S719 = "" Or S719 = "0" Or S719 Is Nothing Then
                    mLice7.Std19 = 0
                Else
                    mLice7.Std19 = S719.Substring(0, 1)
                End If
                mLice7.Stactv = jP7.SelectToken(".Stactv")
                mLice7.Sttm = jP7.SelectToken(".Sttm")
                mLice7.inPoslBroj = jP7.SelectToken(".inPoslBroj")
                Session("mLice7") = mLice7
                '8
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.StConn = jP8.SelectToken(".StConn")
                mLice8.StUsp = jP8.SelectToken(".StUsp")
                mLice8.Stlice = jP8.SelectToken(".Stlice")
                mLice8.Strel = jP8.SelectToken(".Strel")
                mLice8.Sthusb = jP8.SelectToken(".Sthusb")
                mLice8.Stfather = jP8.SelectToken(".Stfather")
                mLice8.Stmother = jP8.SelectToken(".Stmother")
                mLice8.Stsex = jP8.SelectToken(".Stsex")
                mLice8.Stdb = jP8.SelectToken(".Stdb")
                mLice8.Stmb = jP8.SelectToken(".Stmb")
                mLice8.Styb = jP8.SelectToken(".Styb")
                mLice8.Stage = jP8.SelectToken(".Stage")
                Dim S811 As String = jP8.SelectToken(".Std11")
                mLice8.Std11 = S811.Substring(0, 3)
                Dim S812 As String = jP8.SelectToken(".Std12")
                If S812 = "" Or S812 = "0" Or S812 Is Nothing Then
                    S812 = 0
                    mLice8.Std12 = S812.Substring(0, 3)
                Else
                    mLice8.Std12 = S812.Substring(0, 3)
                End If

                Dim S813 As String = jP8.SelectToken(".Std13")
                If S813 = "" Or S813 Is Nothing Then
                    mLice8.Std13 = 0
                Else
                    mLice8.Std13 = S813.Substring(0, 1)
                End If
                Dim S814 As String = jP8.SelectToken(".Std14")
                If S814 = "" Or S814 Is Nothing Then
                    mLice8.Std14 = 0
                Else
                    mLice8.Std14 = S814.Substring(0, 1)
                End If
                Dim S815 As String = jP8.SelectToken(".Std15")
                If S815 = "" Or S815 Is Nothing Then
                    mLice8.Std15 = 0
                Else
                    mLice8.Std15 = S815
                End If

                Dim S816 As String = jP8.SelectToken(".Std16")
                If S816 = "" Or S816 = "0" Or S816 Is Nothing Then
                    mLice8.Std16 = 0
                Else
                    mLice8.Std16 = S816.Substring(0, 3)
                End If
                Dim S817 As String = jP8.SelectToken(".Std17")
                If S817 = "" Or S817 = "0" Or S817 Is Nothing Then
                    mLice8.Std17 = 0
                Else
                    mLice8.Std17 = S817.Substring(0, 3)
                End If
                Dim S818 As String = jP8.SelectToken(".Std18")
                If S818 = "" Or S818 = "0" Or S818 Is Nothing Then
                    mLice8.Std18 = 0
                Else
                    mLice8.Std18 = S818.Substring(0, 3)
                End If
                Dim S819 As String = jP8.SelectToken(".Std19")
                If S819 = "" Or S819 = "0" Or S819 Is Nothing Then
                    mLice8.Std19 = 0
                Else
                    mLice8.Std19 = S819.Substring(0, 1)
                End If
                mLice8.Stactv = jP8.SelectToken(".Stactv")
                mLice8.Sttm = jP8.SelectToken(".Sttm")
                mLice8.inPoslBroj = jP8.SelectToken(".inPoslBroj")
                Session("mLice8") = mLice8
                '9
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.StConn = jP9.SelectToken(".StConn")
                mLice9.StUsp = jP9.SelectToken(".StUsp")
                mLice9.Stlice = jP9.SelectToken(".Stlice")
                mLice9.Strel = jP9.SelectToken(".Strel")
                mLice9.Sthusb = jP9.SelectToken(".Sthusb")
                mLice9.Stfather = jP9.SelectToken(".Stfather")
                mLice9.Stmother = jP9.SelectToken(".Stmother")
                mLice9.Stsex = jP9.SelectToken(".Stsex")
                mLice9.Stdb = jP9.SelectToken(".Stdb")
                mLice9.Stmb = jP9.SelectToken(".Stmb")
                mLice9.Styb = jP9.SelectToken(".Styb")
                mLice9.Stage = jP9.SelectToken(".Stage")
                Dim S911 As String = jP9.SelectToken(".Std11")
                mLice9.Std11 = S911.Substring(0, 3)
                Dim S912 As String = jP9.SelectToken(".Std12")
                If S912 = "" Or S912 = "0" Or S912 Is Nothing Then
                    S912 = 0
                    mLice9.Std12 = S912.Substring(0, 3)
                Else
                    mLice9.Std12 = S912.Substring(0, 3)
                End If
                Dim S913 As String = jP9.SelectToken(".Std13")
                If S913 = "" Or S913 Is Nothing Then
                    mLice9.Std13 = 0
                Else
                    mLice9.Std13 = S913.Substring(0, 1)
                End If
                Dim S914 As String = jP9.SelectToken(".Std14")
                If S914 = "" Or S914 Is Nothing Then
                    mLice9.Std14 = 0
                Else
                    mLice9.Std14 = S914.Substring(0, 1)
                End If
                Dim S915 As String = jP9.SelectToken(".Std15")
                If S915 = "" Or S915 Is Nothing Then
                    mLice9.Std15 = 0
                Else
                    mLice9.Std15 = S915
                End If

                Dim S916 As String = jP9.SelectToken(".Std16")
                If S916 = "" Or S916 = "0" Or S916 Is Nothing Then
                    mLice9.Std16 = 0
                Else
                    mLice9.Std16 = S916.Substring(0, 3)
                End If
                Dim S917 As String = jP9.SelectToken(".Std17")
                If S917 = "" Or S917 = "0" Or S917 Is Nothing Then
                    mLice9.Std17 = 0
                Else
                    mLice9.Std17 = S917.Substring(0, 3)
                End If

                Dim S918 As String = jP9.SelectToken(".Std18")
                If S918 = "" Or S918 = "0" Or S918 Is Nothing Then
                    mLice9.Std18 = 0
                Else
                    mLice9.Std18 = S918.Substring(0, 3)
                End If
                Dim S919 As String = jP9.SelectToken(".Std19")
                If S919 = "" Or S919 = "0" Or S919 Is Nothing Then
                    mLice9.Std19 = 0
                Else
                    mLice9.Std19 = S919.Substring(0, 1)
                End If
                mLice9.Stactv = jP9.SelectToken(".Stactv")
                mLice9.Sttm = jP9.SelectToken(".Sttm")
                mLice9.inPoslBroj = jP9.SelectToken(".inPoslBroj")
                Session("mLice9") = mLice9
                '10
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.StConn = jP10.SelectToken(".StConn")
                mLice10.StUsp = jP10.SelectToken(".StUsp")
                mLice10.Stlice = jP10.SelectToken(".Stlice")
                mLice10.Strel = jP10.SelectToken(".Strel")
                mLice10.Sthusb = jP10.SelectToken(".Sthusb")
                mLice10.Stfather = jP10.SelectToken(".Stfather")
                mLice10.Stmother = jP10.SelectToken(".Stmother")
                mLice10.Stsex = jP10.SelectToken(".Stsex")
                mLice10.Stdb = jP10.SelectToken(".Stdb")
                mLice10.Stmb = jP10.SelectToken(".Stmb")
                mLice10.Styb = jP10.SelectToken(".Styb")
                mLice10.Stage = jP10.SelectToken(".Stage")
                Dim S1011 As String = jP10.SelectToken(".Std11")
                mLice10.Std11 = S1011.Substring(0, 3)
                Dim S1012 As String = jP10.SelectToken(".Std12")
                If S1012 = "" Or S1012 = "0" Or S1012 Is Nothing Then
                    S1012 = 0
                    mLice10.Std12 = S1012.Substring(0, 3)
                Else
                    mLice10.Std12 = S1012.Substring(0, 3)
                End If
                Dim S1013 As String = jP10.SelectToken(".Std13")
                If S1013 = "" Or S1013 Is Nothing Then
                    mLice10.Std13 = 0
                Else
                    mLice10.Std13 = S1013.Substring(0, 1)
                End If

                Dim S1014 As String = jP10.SelectToken(".Std14")
                If S1014 = "" Or S1014 Is Nothing Then
                    mLice10.Std14 = 0
                Else
                    mLice10.Std14 = S1014.Substring(0, 1)
                End If
                Dim S1015 As String = jP10.SelectToken(".Std15")
                If S1015 = "" Or S1015 Is Nothing Then
                    mLice10.Std15 = 0
                Else
                    mLice10.Std15 = S915
                End If
                Dim S1016 As String = jP10.SelectToken(".Std16")
                If S1016 = "" Or S1016 = "0" Or S1016 Is Nothing Then
                    mLice10.Std16 = 0
                Else
                    mLice10.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1017 As String = jP10.SelectToken(".Std17")
                If S1017 = "" Or S1017 = "0" Or S1017 Is Nothing Then
                    mLice10.Std17 = 0
                Else
                    mLice10.Std17 = S1017.Substring(0, 3)
                End If
                Dim S1018 As String = jP10.SelectToken(".Std18")
                If S1018 = "" Or S1018 = "0" Or S1018 Is Nothing Then
                    mLice10.Std18 = 0
                Else
                    mLice10.Std18 = S1018.Substring(0, 3)
                End If

                Dim S1019 As String = jP10.SelectToken(".Std19")
                If S1019 = "" Or S1019 = "0" Or S1019 Is Nothing Then
                    mLice10.Std19 = 0
                Else
                    mLice10.Std19 = S1019.Substring(0, 1)
                End If
                mLice10.Stactv = jP10.SelectToken(".Stactv")
                mLice10.Sttm = jP10.SelectToken(".Sttm")
                mLice10.inPoslBroj = jP10.SelectToken(".inPoslBroj")
                Session("mLice10") = mLice10

                '11
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.StConn = jP11.SelectToken(".StConn")
                mLice11.StUsp = jP11.SelectToken(".StUsp")
                mLice11.Stlice = jP11.SelectToken(".Stlice")
                mLice11.Strel = jP11.SelectToken(".Strel")
                mLice11.Sthusb = jP11.SelectToken(".Sthusb")
                mLice11.Stfather = jP11.SelectToken(".Stfather")
                mLice11.Stmother = jP11.SelectToken(".Stmother")
                mLice11.Stsex = jP11.SelectToken(".Stsex")
                mLice11.Stdb = jP11.SelectToken(".Stdb")
                mLice11.Stmb = jP11.SelectToken(".Stmb")
                mLice11.Styb = jP11.SelectToken(".Styb")
                mLice11.Stage = jP11.SelectToken(".Stage")
                Dim S1111 As String = jP11.SelectToken(".Std11")
                mLice11.Std11 = S1111.Substring(0, 3)
                Dim S1112 As String = jP11.SelectToken(".Std12")
                If S1112 = "" Or S1112 = "0" Or S1112 Is Nothing Then
                    S1112 = 0
                    mLice11.Std12 = S1112.Substring(0, 3)
                Else
                    mLice11.Std12 = S1112.Substring(0, 3)
                End If
                Dim S1113 As String = jP11.SelectToken(".Std13")
                If S1113 = "" Or S1113 Is Nothing Then
                    mLice11.Std13 = 0
                Else
                    mLice11.Std13 = S1113.Substring(0, 1)
                End If
                Dim S1114 As String = jP11.SelectToken(".Std14")
                If S1114 = "" Or S1114 Is Nothing Then
                    mLice11.Std14 = 0
                Else
                    mLice11.Std14 = S1114.Substring(0, 1)
                End If

                Dim S1115 As String = jP11.SelectToken(".Std15")
                If S1115 = "" Or S1115 Is Nothing Then
                    mLice11.Std15 = 0
                Else
                    mLice11.Std15 = S915
                End If
                Dim S1116 As String = jP11.SelectToken(".Std16")
                If S1116 = "" Or S1116 = "0" Or S1116 Is Nothing Then
                    mLice11.Std16 = 0
                Else
                    mLice11.Std16 = S1016.Substring(0, 3)
                End If
                Dim S1117 As String = jP11.SelectToken(".Std17")
                If S1117 = "" Or S1117 = "0" Or S1117 Is Nothing Then
                    mLice11.Std17 = 0
                Else
                    mLice11.Std17 = S1117.Substring(0, 3)
                End If
                Dim S1118 As String = jP11.SelectToken(".Std18")
                If S1118 = "" Or S1118 = "0" Or S1118 Is Nothing Then
                    mLice11.Std18 = 0
                Else
                    mLice11.Std18 = S1118.Substring(0, 3)
                End If
                Dim S1119 As String = jP11.SelectToken(".Std19")
                If S1119 = "" Or S1119 = "0" Or S1119 Is Nothing Then
                    mLice11.Std19 = 0
                Else
                    mLice11.Std19 = S1119.Substring(0, 1)
                End If
                mLice11.Stactv = jP11.SelectToken(".Stactv")
                mLice11.Sttm = jP11.SelectToken(".Sttm")
                mLice11.inPoslBroj = jP11.SelectToken(".inPoslBroj")
                Session("mLice11") = mLice11
                '12
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.StConn = jP12.SelectToken(".StConn")
                mLice12.StUsp = jP12.SelectToken(".StUsp")
                mLice12.Stlice = jP12.SelectToken(".Stlice")
                mLice12.Strel = jP12.SelectToken(".Strel")
                mLice12.Sthusb = jP12.SelectToken(".Sthusb")
                mLice12.Stfather = jP12.SelectToken(".Stfather")
                mLice12.Stmother = jP12.SelectToken(".Stmother")
                mLice12.Stsex = jP12.SelectToken(".Stsex")
                mLice12.Stdb = jP12.SelectToken(".Stdb")
                mLice12.Stmb = jP12.SelectToken(".Stmb")
                mLice12.Styb = jP12.SelectToken(".Styb")
                mLice12.Stage = jP12.SelectToken(".Stage")
                Dim S1211 As String = jP12.SelectToken(".Std11")
                mLice12.Std11 = S1211.Substring(0, 3)
                Dim S1212 As String = jP12.SelectToken(".Std12")
                If S1212 = "" Or S1212 = "0" Or S1212 Is Nothing Then
                    S1212 = 0
                    mLice12.Std12 = S1212.Substring(0, 3)
                Else
                    mLice12.Std12 = S1212.Substring(0, 3)
                End If
                Dim S1213 As String = jP12.SelectToken(".Std13")
                If S1213 = "" Or S1213 Is Nothing Then
                    mLice12.Std13 = 0
                Else
                    mLice12.Std13 = S1213.Substring(0, 1)
                End If
                Dim S1214 As String = jP12.SelectToken(".Std14")
                If S1214 = "" Or S1214 Is Nothing Then
                    mLice12.Std14 = 0
                Else
                    mLice12.Std14 = S1214.Substring(0, 1)
                End If

                Dim S1215 As String = jP12.SelectToken(".Std15")
                If S1215 = "" Or S1215 Is Nothing Then
                    mLice12.Std15 = 0
                Else
                    mLice12.Std15 = S1215
                End If
                Dim S1216 As String = jP12.SelectToken(".Std16")
                If S1216 = "" Or S1216 = "0" Or S1216 Is Nothing Then
                    mLice12.Std16 = 0
                Else
                    mLice12.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1217 As String = jP12.SelectToken(".Std17")
                If S1217 = "" Or S1217 = "0" Or S1217 Is Nothing Then
                    mLice12.Std17 = 0
                Else
                    mLice12.Std17 = S1217.Substring(0, 3)
                End If
                Dim S1218 As String = jP12.SelectToken(".Std18")
                If S1218 = "" Or S1218 = "0" Or S1218 Is Nothing Then
                    mLice12.Std18 = 0
                Else
                    mLice12.Std18 = S1218.Substring(0, 3)
                End If
                Dim S1219 As String = jP12.SelectToken(".Std19")
                If S1219 = "" Or S1219 = "0" Or S1219 Is Nothing Then
                    mLice12.Std19 = 0
                Else
                    mLice12.Std19 = S1219.Substring(0, 1)
                End If
                mLice12.Stactv = jP12.SelectToken(".Stactv")
                mLice12.Sttm = jP12.SelectToken(".Sttm")
                mLice12.inPoslBroj = jP12.SelectToken(".inPoslBroj")
                Session("mLice12") = mLice12
                '13
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.StConn = jP13.SelectToken(".StConn")
                mLice13.StUsp = jP13.SelectToken(".StUsp")
                mLice13.Stlice = jP13.SelectToken(".Stlice")
                mLice13.Strel = jP13.SelectToken(".Strel")
                mLice13.Sthusb = jP13.SelectToken(".Sthusb")
                mLice13.Stfather = jP13.SelectToken(".Stfather")
                mLice13.Stmother = jP13.SelectToken(".Stmother")
                mLice13.Stsex = jP13.SelectToken(".Stsex")
                mLice13.Stdb = jP13.SelectToken(".Stdb")
                mLice13.Stmb = jP13.SelectToken(".Stmb")
                mLice13.Styb = jP13.SelectToken(".Styb")
                mLice13.Stage = jP13.SelectToken(".Stage")
                Dim S1311 As String = jP13.SelectToken(".Std11")
                mLice13.Std11 = S1311.Substring(0, 3)
                Dim S1312 As String = jP13.SelectToken(".Std12")
                If S1312 = "" Or S1312 = "0" Or S1312 Is Nothing Then
                    S1312 = 0
                    mLice13.Std12 = S1312.Substring(0, 3)
                Else
                    mLice13.Std12 = S1312.Substring(0, 3)
                End If
                Dim S1313 As String = jP13.SelectToken(".Std13")
                If S1313 = "" Or S1313 Is Nothing Then
                    mLice13.Std13 = 0
                Else
                    mLice13.Std13 = S1313.Substring(0, 1)
                End If

                Dim S1314 As String = jP13.SelectToken(".Std14")
                If S1314 = "" Or S1314 Is Nothing Then
                    mLice13.Std14 = 0
                Else
                    mLice13.Std14 = S1314.Substring(0, 1)
                End If
                Dim S1315 As String = jP13.SelectToken(".Std15")
                If S1315 = "" Or S1315 Is Nothing Then
                    mLice13.Std15 = 0
                Else
                    mLice13.Std15 = S1215
                End If
                Dim S1316 As String = jP13.SelectToken(".Std16")
                If S1316 = "" Or S1316 = "0" Or S1316 Is Nothing Then
                    mLice13.Std16 = 0
                Else
                    mLice13.Std16 = S1216.Substring(0, 3)
                End If
                Dim S1317 As String = jP13.SelectToken(".Std17")
                If S1317 = "" Or S1317 = "0" Or S1317 Is Nothing Then
                    mLice13.Std17 = 0
                Else
                    mLice13.Std17 = S1317.Substring(0, 3)
                End If
                Dim S1318 As String = jP13.SelectToken(".Std18")
                If S1318 = "" Or S1318 = "0" Or S1318 Is Nothing Then
                    mLice13.Std18 = 0
                Else
                    mLice13.Std18 = S1318.Substring(0, 3)
                End If

                Dim S1319 As String = jP13.SelectToken(".Std19")
                If S1319 = "" Or S1319 = "0" Or S1319 Is Nothing Then
                    mLice13.Std19 = 0
                Else
                    mLice13.Std19 = S1319.Substring(0, 1)
                End If
                mLice13.Stactv = jP13.SelectToken(".Stactv")

                mLice13.Sttm = jP13.SelectToken(".Sttm")
                mLice13.inPoslBroj = jP13.SelectToken(".inPoslBroj")
                Session("mLice13") = mLice13
                '14
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.StConn = jP14.SelectToken(".StConn")
                mLice14.StUsp = jP14.SelectToken(".StUsp")
                mLice14.Stlice = jP14.SelectToken(".Stlice")
                mLice14.Strel = jP14.SelectToken(".Strel")
                mLice14.Sthusb = jP14.SelectToken(".Sthusb")
                mLice14.Stfather = jP14.SelectToken(".Stfather")
                mLice14.Stmother = jP14.SelectToken(".Stmother")
                mLice14.Stsex = jP14.SelectToken(".Stsex")
                mLice14.Stdb = jP14.SelectToken(".Stdb")
                mLice14.Stmb = jP14.SelectToken(".Stmb")
                mLice14.Styb = jP14.SelectToken(".Styb")
                mLice14.Stage = jP14.SelectToken(".Stage")
                Dim S1411 As String = jP14.SelectToken(".Std11")
                mLice14.Std11 = S1411.Substring(0, 3)
                Dim S1412 As String = jP14.SelectToken(".Std12")
                If S1412 = "" Or S1412 = "0" Or S1412 Is Nothing Then
                    S1412 = 0
                    mLice14.Std12 = S1412.Substring(0, 3)
                Else
                    mLice14.Std12 = S1412.Substring(0, 3)
                End If
                Dim S1413 As String = jP14.SelectToken(".Std13")
                If S1413 = "" Or S1413 Is Nothing Then
                    mLice14.Std13 = 0
                Else
                    mLice14.Std13 = S1413.Substring(0, 1)
                End If

                Dim S1414 As String = jP14.SelectToken(".Std14")
                If S1414 = "" Or S1414 Is Nothing Then
                    mLice14.Std14 = 0
                Else
                    mLice14.Std14 = S1414.Substring(0, 1)
                End If
                Dim S1415 As String = jP14.SelectToken(".Std15")
                If S1415 = "" Or S1415 Is Nothing Then
                    mLice14.Std15 = 0
                Else
                    mLice14.Std15 = S1415
                End If
                Dim S1416 As String = jP14.SelectToken(".Std16")
                If S1416 = "" Or S1416 = "0" Or S1416 Is Nothing Then
                    mLice14.Std16 = 0
                Else
                    mLice14.Std16 = S1416.Substring(0, 3)
                End If
                Dim S1417 As String = jP14.SelectToken(".Std17")
                If S1417 = "" Or S1417 = "0" Or S1417 Is Nothing Then
                    mLice14.Std17 = 0
                Else
                    mLice14.Std17 = S1417.Substring(0, 3)
                End If

                Dim S1418 As String = jP14.SelectToken(".Std18")
                If S1418 = "" Or S1418 = "0" Or S1418 Is Nothing Then
                    mLice14.Std18 = 0
                Else
                    mLice14.Std18 = S1418.Substring(0, 3)
                End If

                Dim S1419 As String = jP14.SelectToken(".Std19")
                If S1419 = "" Or S1419 = "0" Or S1419 Is Nothing Then
                    mLice14.Std19 = 0
                Else
                    mLice14.Std19 = S1419.Substring(0, 1)
                End If
                mLice14.Stactv = jP14.SelectToken(".Stactv")

                mLice14.Sttm = jP14.SelectToken(".Sttm")
                mLice14.inPoslBroj = jP14.SelectToken(".inPoslBroj")
                Session("mLice14") = mLice14
                '15
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.StConn = jP15.SelectToken(".StConn")
                mLice15.StUsp = jP15.SelectToken(".StUsp")
                mLice15.Stlice = jP15.SelectToken(".Stlice")
                mLice15.Strel = jP15.SelectToken(".Strel")
                mLice15.Sthusb = jP15.SelectToken(".Sthusb")
                mLice15.Stfather = jP15.SelectToken(".Stfather")
                mLice15.Stmother = jP15.SelectToken(".Stmother")
                mLice15.Stsex = jP15.SelectToken(".Stsex")
                mLice15.Stdb = jP15.SelectToken(".Stdb")
                mLice15.Stmb = jP15.SelectToken(".Stmb")
                mLice15.Styb = jP15.SelectToken(".Styb")
                mLice15.Stage = jP15.SelectToken(".Stage")
                Dim S1511 As String = jP15.SelectToken(".Std11")
                mLice15.Std11 = S1511.Substring(0, 3)
                Dim S1512 As String = jP15.SelectToken(".Std12")
                If S1512 = "" Or S1512 = "0" Or S1512 Is Nothing Then
                    S1512 = 0
                    mLice15.Std12 = S1512.Substring(0, 3)
                Else
                    mLice15.Std12 = S1512.Substring(0, 3)
                End If
                Dim S1513 As String = jP15.SelectToken(".Std13")
                If S1513 = "" Or S1513 Is Nothing Then
                    mLice15.Std13 = 0
                Else
                    mLice15.Std13 = S1513.Substring(0, 1)
                End If
                Dim S1514 As String = jP15.SelectToken(".Std14")
                If S1514 = "" Or S1514 Is Nothing Then
                    mLice15.Std14 = 0
                Else
                    mLice15.Std14 = S1514.Substring(0, 1)
                End If
                Dim S1515 As String = jP15.SelectToken(".Std15")
                If S1515 = "" Or S1515 Is Nothing Then
                    mLice15.Std15 = 0
                Else
                    mLice15.Std15 = S1515
                End If
                Dim S1516 As String = jP15.SelectToken(".Std16")
                If S1516 = "" Or S1516 = "0" Or S1516 Is Nothing Then
                    mLice15.Std16 = 0
                Else
                    mLice15.Std16 = S1516.Substring(0, 3)
                End If
                Dim S1517 As String = jP15.SelectToken(".Std17")
                If S1517 = "" Or S1517 = "0" Or S1517 Is Nothing Then
                    mLice15.Std17 = 0
                Else
                    mLice15.Std17 = S1517.Substring(0, 3)
                End If

                Dim S1518 As String = jP15.SelectToken(".Std18")
                If S1518 = "" Or S1518 = "0" Or S1518 Is Nothing Then
                    mLice15.Std18 = 0
                Else
                    mLice15.Std18 = S1518.Substring(0, 3)
                End If
                Dim S1519 As String = jP15.SelectToken(".Std19")
                If S1519 = "" Or S1519 = "0" Or S1519 Is Nothing Then
                    mLice15.Std19 = 0
                Else
                    mLice15.Std19 = S1519.Substring(0, 1)
                End If
                mLice15.Stactv = jP15.SelectToken(".Stactv")
                mLice15.Sttm = jP15.SelectToken(".Sttm")
                mLice15.inPoslBroj = jP15.SelectToken(".inPoslBroj")
                Session("mLice15") = mLice15
                '16
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.StConn = jP16.SelectToken(".StConn")
                mLice16.StUsp = jP16.SelectToken(".StUsp")
                mLice16.Stlice = jP16.SelectToken(".Stlice")
                mLice16.Strel = jP16.SelectToken(".Strel")
                mLice16.Sthusb = jP16.SelectToken(".Sthusb")
                mLice16.Stfather = jP16.SelectToken(".Stfather")
                mLice16.Stmother = jP16.SelectToken(".Stmother")
                mLice16.Stsex = jP16.SelectToken(".Stsex")
                mLice16.Stdb = jP16.SelectToken(".Stdb")
                mLice16.Stmb = jP16.SelectToken(".Stmb")
                mLice16.Styb = jP16.SelectToken(".Styb")
                mLice16.Stage = jP16.SelectToken(".Stage")
                Dim S1611 As String = jP16.SelectToken(".Std11")
                mLice16.Std11 = S1611.Substring(0, 3)
                Dim S1612 As String = jP16.SelectToken(".Std12")
                If S1612 = "" Or S1612 = "0" Or S1612 Is Nothing Then
                    S1612 = 0
                    mLice16.Std12 = S1612.Substring(0, 3)
                Else
                    mLice16.Std12 = S1612.Substring(0, 3)
                End If
                Dim S1613 As String = jP16.SelectToken(".Std13")
                If S1613 = "" Or S1613 Is Nothing Then
                    mLice16.Std13 = 0
                Else
                    mLice16.Std13 = S1613.Substring(0, 1)
                End If
                Dim S1614 As String = jP16.SelectToken(".Std14")
                If S1614 = "" Or S1614 Is Nothing Then
                    mLice16.Std14 = 0
                Else
                    mLice16.Std14 = S1614.Substring(0, 1)
                End If
                Dim S1615 As String = jP16.SelectToken(".Std15")
                If S1615 = "" Or S1615 Is Nothing Then
                    mLice16.Std15 = 0
                Else
                    mLice16.Std15 = S1515
                End If
                Dim S1616 As String = jP16.SelectToken(".Std16")
                If S1616 = "" Or S1616 = "0" Or S1616 Is Nothing Then
                    mLice16.Std16 = 0
                Else
                    mLice16.Std16 = S1616.Substring(0, 3)
                End If
                Dim S1617 As String = jP16.SelectToken(".Std17")
                If S1617 = "" Or S1617 = "0" Or S1617 Is Nothing Then
                    mLice16.Std17 = 0
                Else
                    mLice16.Std17 = S1617.Substring(0, 3)
                End If
                Dim S1618 As String = jP16.SelectToken(".Std18")
                If S1618 = "" Or S1618 = "0" Or S1618 Is Nothing Then
                    mLice16.Std18 = 0
                Else
                    mLice16.Std18 = S1618.Substring(0, 3)
                End If

                Dim S1619 As String = jP16.SelectToken(".Std19")
                If S1619 = "" Or S1619 = "0" Or S1619 Is Nothing Then
                    mLice16.Std19 = 0
                Else
                    mLice16.Std19 = S1619.Substring(0, 1)
                End If
                mLice16.Stactv = jP16.SelectToken(".Stactv")
                mLice16.Sttm = jP16.SelectToken(".Sttm")
                mLice16.inPoslBroj = jP16.SelectToken(".inPoslBroj")
                Session("mLice16") = mLice16
                '17
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.StConn = jP17.SelectToken(".StConn")
                mLice17.StUsp = jP17.SelectToken(".StUsp")
                mLice17.Stlice = jP17.SelectToken(".Stlice")
                mLice17.Strel = jP17.SelectToken(".Strel")
                mLice17.Sthusb = jP17.SelectToken(".Sthusb")
                mLice17.Stfather = jP17.SelectToken(".Stfather")
                mLice17.Stmother = jP17.SelectToken(".Stmother")
                mLice17.Stsex = jP17.SelectToken(".Stsex")
                mLice17.Stdb = jP17.SelectToken(".Stdb")
                mLice17.Stmb = jP17.SelectToken(".Stmb")
                mLice17.Styb = jP17.SelectToken(".Styb")
                mLice17.Stage = jP17.SelectToken(".Stage")
                Dim S1711 As String = jP17.SelectToken(".Std11")
                mLice17.Std11 = S1711.Substring(0, 3)
                Dim S1712 As String = jP17.SelectToken(".Std12")
                If S1712 = "" Or S1712 = "0" Or S1712 Is Nothing Then
                    S1712 = 0
                    mLice17.Std12 = S1712.Substring(0, 3)
                Else
                    mLice17.Std12 = S1712.Substring(0, 3)
                End If
                Dim S1713 As String = jP17.SelectToken(".Std13")
                If S1713 = "" Or S1713 Is Nothing Then
                    mLice17.Std13 = 0
                Else
                    mLice17.Std13 = S1713.Substring(0, 1)
                End If
                Dim S1714 As String = jP17.SelectToken(".Std14")
                If S1714 = "" Or S1714 Is Nothing Then
                    mLice17.Std14 = 0
                Else
                    mLice17.Std14 = S1714.Substring(0, 1)
                End If
                Dim S1715 As String = jP17.SelectToken(".Std15")
                If S1715 = "" Or S1715 Is Nothing Then
                    mLice17.Std15 = 0
                Else
                    mLice17.Std15 = S1715
                End If
                Dim S1716 As String = jP17.SelectToken(".Std16")
                If S1716 = "" Or S1716 = "0" Or S1716 Is Nothing Then
                    mLice17.Std16 = 0
                Else
                    mLice17.Std16 = S1716.Substring(0, 3)
                End If
                Dim S1717 As String = jP17.SelectToken(".Std17")
                If S1717 = "" Or S1717 = "0" Or S1717 Is Nothing Then
                    mLice17.Std17 = 0
                Else
                    mLice17.Std17 = S1717.Substring(0, 3)
                End If
                Dim S1718 As String = jP17.SelectToken(".Std18")
                If S1718 = "" Or S1718 = "0" Or S1718 Is Nothing Then
                    mLice17.Std18 = 0
                Else
                    mLice17.Std18 = S1718.Substring(0, 3)
                End If
                Dim S1719 As String = jP17.SelectToken(".Std19")
                If S1719 = "" Or S1719 = "0" Or S1719 Is Nothing Then
                    mLice17.Std19 = 0
                Else
                    mLice17.Std19 = S1719.Substring(0, 1)
                End If
                mLice17.Stactv = jP17.SelectToken(".Stactv")
                mLice17.Sttm = jP17.SelectToken(".Sttm")
                mLice17.inPoslBroj = jP17.SelectToken(".inPoslBroj")
                Session("mLice17") = mLice17
                '18
                mLice18.Stlice = jP18.SelectToken(".Stlice")
                mLice18.StConn = jP18.SelectToken(".StConn")
                mLice18.StUsp = jP18.SelectToken(".StUsp")
                mLice18.Stlice = jP18.SelectToken(".Stlice")
                mLice18.Strel = jP18.SelectToken(".Strel")
                mLice18.Sthusb = jP18.SelectToken(".Sthusb")
                mLice18.Stfather = jP18.SelectToken(".Stfather")
                mLice18.Stmother = jP18.SelectToken(".Stmother")
                mLice18.Stsex = jP18.SelectToken(".Stsex")
                mLice18.Stdb = jP18.SelectToken(".Stdb")
                mLice18.Stmb = jP18.SelectToken(".Stmb")
                mLice18.Styb = jP18.SelectToken(".Styb")
                mLice18.Stage = jP18.SelectToken(".Stage")
                Dim S1811 As String = jP18.SelectToken(".Std11")
                mLice18.Std11 = S1811.Substring(0, 3)
                Dim S1812 As String = jP18.SelectToken(".Std12")
                If S1812 = "" Or S1812 = "0" Or S1812 Is Nothing Then
                    S1812 = 0
                    mLice18.Std12 = S1812.Substring(0, 3)
                Else
                    mLice18.Std12 = S1812.Substring(0, 3)
                End If
                Dim S1813 As String = jP18.SelectToken(".Std13")
                If S1813 = "" Or S1813 Is Nothing Then
                    mLice18.Std13 = 0
                Else
                    mLice18.Std13 = S1813.Substring(0, 1)
                End If
                Dim S1814 As String = jP18.SelectToken(".Std14")
                If S1814 = "" Or S1814 Is Nothing Then
                    mLice18.Std14 = 0
                Else
                    mLice18.Std14 = S1814.Substring(0, 1)
                End If
                Dim S1815 As String = jP18.SelectToken(".Std15")
                If S1815 = "" Or S1815 Is Nothing Then
                    mLice18.Std15 = 0
                Else
                    mLice18.Std15 = S1815
                End If
                Dim S1816 As String = jP18.SelectToken(".Std16")
                If S1816 = "" Or S1816 = "0" Or S1816 Is Nothing Then
                    mLice18.Std16 = 0
                Else
                    mLice18.Std16 = S1816.Substring(0, 3)
                End If
                Dim S1817 As String = jP18.SelectToken(".Std17")
                If S1817 = "" Or S1817 = "0" Or S1817 Is Nothing Then
                    mLice18.Std17 = 0
                Else
                    mLice18.Std17 = S1817.Substring(0, 3)
                End If
                Dim S1818 As String = jP18.SelectToken(".Std18")
                If S1818 = "" Or S1818 = "0" Or S1818 Is Nothing Then
                    mLice18.Std18 = 0
                Else
                    mLice18.Std18 = S1818.Substring(0, 3)
                End If
                Dim S1819 As String = jP18.SelectToken(".Std19")
                If S1819 = "" Or S1819 = "0" Or S1819 Is Nothing Then
                    mLice18.Std19 = 0
                Else
                    mLice18.Std19 = S1819.Substring(0, 1)
                End If
                mLice18.Stactv = jP18.SelectToken(".Stactv")

                mLice18.Sttm = jP18.SelectToken(".Sttm")
                mLice18.inPoslBroj = jP18.SelectToken(".inPoslBroj")
                Session("mLice18") = mLice18
                '19
                mLice19.Stlice = jP19.SelectToken(".Stlice")
                mLice19.StConn = jP19.SelectToken(".StConn")
                mLice19.StUsp = jP19.SelectToken(".StUsp")
                mLice19.Stlice = jP19.SelectToken(".Stlice")
                mLice19.Strel = jP19.SelectToken(".Strel")
                mLice19.Sthusb = jP19.SelectToken(".Sthusb")
                mLice19.Stfather = jP19.SelectToken(".Stfather")
                mLice19.Stmother = jP19.SelectToken(".Stmother")
                mLice19.Stsex = jP19.SelectToken(".Stsex")
                mLice19.Stdb = jP19.SelectToken(".Stdb")
                mLice19.Stmb = jP19.SelectToken(".Stmb")
                mLice19.Styb = jP19.SelectToken(".Styb")
                mLice19.Stage = jP19.SelectToken(".Stage")
                Dim S1911 As String = jP19.SelectToken(".Std11")
                mLice19.Std11 = S1811.Substring(0, 3)

                Dim S1912 As String = jP19.SelectToken(".Std12")
                If S1912 = "" Or S1912 = "0" Or S1912 Is Nothing Then
                    S1912 = 0
                    mLice19.Std12 = S1912.Substring(0, 3)
                Else
                    mLice19.Std12 = S1912.Substring(0, 3)
                End If
                Dim S1913 As String = jP19.SelectToken(".Std13")
                If S1913 = "" Or S1913 Is Nothing Then
                    mLice19.Std13 = 0
                Else
                    mLice19.Std13 = S1913.Substring(0, 1)
                End If
                Dim S1914 As String = jP19.SelectToken(".Std14")
                If S1914 = "" Or S1914 Is Nothing Then
                    mLice19.Std14 = 0
                Else
                    mLice19.Std14 = S1914.Substring(0, 1)
                End If
                Dim S1915 As String = jP19.SelectToken(".Std15")
                If S1915 = "" Or S1915 Is Nothing Then
                    mLice19.Std15 = 0
                Else
                    mLice19.Std15 = S1915
                End If

                Dim S1916 As String = jP19.SelectToken(".Std16")
                If S1916 = "" Or S1916 = "0" Or S1916 Is Nothing Then
                    mLice19.Std16 = 0
                Else
                    mLice19.Std16 = S1916.Substring(0, 3)
                End If
                Dim S1917 As String = jP19.SelectToken(".Std17")
                If S1917 = "" Or S1917 = "0" Or S1917 Is Nothing Then
                    mLice19.Std17 = 0
                Else
                    mLice19.Std17 = S1917.Substring(0, 3)
                End If
                Dim S1918 As String = jP19.SelectToken(".Std18")
                If S1918 = "" Or S1918 = "0" Or S1918 Is Nothing Then
                    mLice19.Std18 = 0
                Else
                    mLice19.Std18 = S1918.Substring(0, 3)
                End If
                Dim S1919 As String = jP19.SelectToken(".Std19")
                If S1919 = "" Or S1919 = "0" Or S1919 Is Nothing Then
                    mLice19.Std19 = 0
                Else
                    mLice19.Std19 = S1919.Substring(0, 1)
                End If
                mLice19.Stactv = jP19.SelectToken(".Stactv")
                mLice19.Sttm = jP19.SelectToken(".Sttm")
                mLice19.inPoslBroj = jP19.SelectToken(".inPoslBroj")
                Session("mLice19") = mLice19
        End Select

        'за проверка

        'NPers.Value = 1

        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim dbRes As Boolean = True
        Dim rowRes As Integer = 0
        Dim Mq As Object = Nothing
        Dim cmd2Res As Integer = 0
        Dim Lica As String() = New String(37) {"01", "N", "02", "N", "03", "N", "04", "N", "05", "N", "06", "N", "07", "N", "08", "N", "09", "N", "10", "N", "11", "N", "12", "N", "13", "N", "14", "N", "15", "N", "16", "N", "17", "N", "18", "N", "19", "N"}
        Dim NumbPersons As Integer = Val(hNPers.Value)
        Dim MyLArrayEnd As Integer = (NumbPersons * 2) - 1
        For i As Integer = 0 To MyLArrayEnd
            Dim mLice As String = ""
            mLice = Lica(i)
            Dim mySel As String = "Select COUNT(*) FROM " + myTbl + " WHERE oblast=@oblast And grs=@grs And gnezdo=@gnezdo And domak = @domak And lice= @lice And tm=@tm"
            rowRes = 0
            Using connection As New SqlConnection(sCnn)
                Dim cmd As New SqlCommand(mySel, connection)
                cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
                cmd.Parameters.AddWithValue("@grs", CType(Session("grs"), Integer))
                cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
                cmd.Parameters.AddWithValue("@domak", Session("domak"))
                cmd.Parameters.AddWithValue("@lice", mLice)
                cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
                connection.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                Try
                    While reader.Read()
                        rowRes = reader(0)
                    End While
                Finally
                    reader.Close()
                End Try
                connection.Close()
            End Using
            If rowRes <> 0 Then
                Lica(i + 1) = "Y"
            End If
            i += 1
        Next

        'Дом
        Dim rowsResDom As Integer = 0
        Dim anketa As Integer
        'Dim Persons As Integer = CInt(HsHldNum.Text)
        If Not String.IsNullOrEmpty(Session("HasSurvey")) Then
            anketa = CType(Session("HasSurvey"), Integer)
        End If
        Dim nach As Integer
        If Not String.IsNullOrEmpty(Session("SurvType")) Then
            nach = CType(Session("SurvType"), Integer)
        End If
        Dim sD As String = Session("DenNabl")
        If sD.Length < 2 Then
            sD = "0" + sD
        End If
        Dim sM As String = Session("MesecNabl")
        If sM.Length < 2 Then
            sM = "0" + sM
        End If
        con.Open()
        Dim mySelDom As String = "Select COUNT(*) FROM " + myTblDom + " WHERE oblast=@oblast And grs=@grs And gnezdo=@gnezdo And domak=@domak And tm=@tm"
        rowsResDom = 0
        Using connection As New SqlConnection(sCnn)
            Dim cmd As New SqlCommand(mySelDom, connection)
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", CType(Session("grs"), Integer))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            Try
                connection.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                Try
                    While reader.Read()
                        rowsResDom = reader(0)
                    End While
                Catch ex As Exception
                    ex.ToString()
                Finally
                    reader.Close()
                End Try
            Catch ex As Exception
                ex.ToString()
            Finally
                connection.Close()
            End Try
        End Using
        If rowsResDom = 0 Then
            Dim myInsDom As String = "INSERT INTO " + myTblDom + " (oblast,grs,gnezdo,domak,nv,datad,datam,datag,anketa,hhsize,nad,ppd,ppm,ppg,anketor,tel,dl,nach,vp,tm)  VALUES ( @oblast, @grs, @gnezdo, @domak,@nv, @datad, @datam,@datag,@anketa, @hhsize,@nad, @ppd, @ppm, @ppg, @anketor,@tel, @dl, @nach, @vp, @tm)"
            Using cmd As New SqlCommand(myInsDom, con)
                cmd.CommandType = CommandType.Text
                cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
                cmd.Parameters.AddWithValue("@grs", Session("grs"))
                cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
                cmd.Parameters.AddWithValue("@domak", Session("domak"))
                cmd.Parameters.AddWithValue("@nv", "0")
                cmd.Parameters.AddWithValue("@datad", "0")
                cmd.Parameters.AddWithValue("@datam", "0")
                cmd.Parameters.AddWithValue("@datag", "0")
                cmd.Parameters.AddWithValue("@anketa", anketa)
                'cmd.Parameters.AddWithValue("@hhsize", Persons.ToString())
                cmd.Parameters.AddWithValue("@hhsize", HsHldNum.Text)
                cmd.Parameters.AddWithValue("@nad", 0)
                cmd.Parameters.AddWithValue("@ppd", sD)
                cmd.Parameters.AddWithValue("@ppm", sM)
                cmd.Parameters.AddWithValue("@ppg", Session("GodNabl"))
                cmd.Parameters.AddWithValue("@anketor", 0)
                cmd.Parameters.AddWithValue("@tel", "0")
                cmd.Parameters.AddWithValue("@dl", CType(NumbPersons, Integer))
                'cmd.Parameters.AddWithValue("@dl", CType(1, Integer))
                cmd.Parameters.AddWithValue("@nach", nach)
                cmd.Parameters.AddWithValue("@vp", "0")
                cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
                rowsResDom = cmd.ExecuteNonQuery()
            End Using
        Else
            Dim mDom As New Domakinstvo() With {.Poblast = Session("oblast"),
                                                .Pgrs = CType(Session("grs"), Integer),
                                                .Pgnezdo = Session("gnezdo"),
                                                .Pdomak = Session("domak"),
                                                .Phhsize = Request("ctl00$Maincontent$HsHldNum"),
                                                .Panketa = anketa,
                                                .Pnad = 0,
                                                .Pppd = sD,
                                                .Pppm = sM,
                                                .Pppg = Session("GodNabl"),
                                                .Pdl = CType(Request("ctl00$Maincontent$NmbPrs"), Integer),
                                                .Pnach = nach}
            Dim MyDom As ArrayList = New ArrayList() From {Session("oblast"), CType(Session("grs"), Integer), Session("gnezdo"), Session("domak"), Session("ObsNumb")}
            mDom.UpdateDataP2(sCnn, MyDom)
            'Dim myInsDom As String = "UPDATE " + myTblDom + " SET  oblast=@oblast, grs=@grs, gnezdo = @gnezdo, domak = @domak,  nv = @nv, datad = @datad, datam = @datam, datag = @datag, anketa = @anketa, hhsize = @hhsize, nad = @nad, ppd = @ppd, ppm = @ppm, ppg = @ppg, anketor = @anketor, tel = @tel, dl = @dl, nach = @nach, vp = @vp, tm=@tm WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak and tm=@tm"
            'Using cmd As New SqlCommand(myInsDom, con)
            '    cmd.CommandType = CommandType.Text
            '    cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            '    cmd.Parameters.AddWithValue("@grs", Session("grs"))
            '    cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            '    cmd.Parameters.AddWithValue("@domak", Session("domak"))
            '    cmd.Parameters.AddWithValue("@nv", "0")
            '    cmd.Parameters.AddWithValue("@datad", "0")
            '    cmd.Parameters.AddWithValue("@datam", "0")
            '    cmd.Parameters.AddWithValue("@datag", "0")
            '    cmd.Parameters.AddWithValue("@anketa", anketa)
            '    'cmd.Parameters.AddWithValue("@hhsize", Persons.ToString())
            '    cmd.Parameters.AddWithValue("@hhsize", Request("ctl00$Maincontent$HsHldNum"))
            '    cmd.Parameters.AddWithValue("@nad", 0)
            '    cmd.Parameters.AddWithValue("@ppd", sD)
            '    cmd.Parameters.AddWithValue("@ppm", sM)
            '    cmd.Parameters.AddWithValue("@ppg", Session("GodNabl"))
            '    cmd.Parameters.AddWithValue("@anketor", 0)
            '    cmd.Parameters.AddWithValue("@tel", "0")
            '    cmd.Parameters.AddWithValue("@dl", CType(NumbPersons, Integer))
            '    'cmd.Parameters.AddWithValue("@dl", CType(1, Integer))
            '    cmd.Parameters.AddWithValue("@nach", nach)
            '    cmd.Parameters.AddWithValue("@vp", "0")
            '    cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            '    rowsResDom = cmd.ExecuteNonQuery()
            'End Using
        End If
        con.Close()

        'Лица
        Dim ind As Integer = 0
        If NumbPersons <> 0 Then
            If Lica(ind + 1) = "N" Then
                InRow1(Session("mLice1"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 1) = "Y" Then
                UpdRow1(Session("mLice1"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 3) = "N" Then
                InRow2(Session("mLice2"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 3) = "Y" Then
                UpdRow2(Session("mLice2"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 5) = "N" Then
                InRow3(Session("mLice3"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 3) = "Y" Then
                UpdRow3(Session("mLice3"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 7) = "N" Then
                InRow4(Session("mLice4"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 7) = "Y" Then
                UpdRow4(Session("mLice4"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 9) = "N" Then
                InRow5(Session("mLice5"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 9) = "Y" Then
                UpdRow5(Session("mLice5"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If

        If NumbPersons <> 0 Then
            If Lica(ind + 11) = "N" Then
                InRow6(Session("mLice6"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 11) = "Y" Then
                UpdRow6(Session("mLice6"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If

        If NumbPersons <> 0 Then
            If Lica(ind + 13) = "N" Then
                InRow7(Session("mLice7"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 13) = "Y" Then
                UpdRow7(Session("mLice7"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 15) = "N" Then
                InRow8(Session("mLice8"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 15) = "Y" Then
                UpdRow8(Session("mLice8"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 17) = "N" Then
                InRow9(Session("mLice9"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 17) = "Y" Then
                UpdRow9(Session("mLice9"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If

        If NumbPersons <> 0 Then
            If Lica(ind + 19) = "N" Then
                InRow10(Session("mLice10"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 19) = "Y" Then
                UpdRow10(Session("mLice10"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 21) = "N" Then
                InRow11(Session("mLice11"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 21) = "Y" Then
                UpdRow11(Session("mLice11"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 23) = "N" Then
                InRow12(Session("mLice12"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 23) = "Y" Then
                UpdRow12(Session("mLice12"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 25) = "N" Then
                InRow13(Session("mLice13"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 25) = "Y" Then
                UpdRow13(Session("mLice13"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 27) = "N" Then
                InRow14(Session("mLice14"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 27) = "Y" Then
                UpdRow14(Session("mLice14"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 29) = "N" Then
                InRow15(Session("mLice15"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 29) = "Y" Then
                UpdRow15(Session("mLice15"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If

        If NumbPersons <> 0 Then
            If Lica(ind + 31) = "N" Then
                InRow16(Session("mLice16"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 31) = "Y" Then
                NumbPersons = NumbPersons - 1
                UpdRow16(Session("mLice16"))
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 33) = "N" Then
                InRow17(Session("mLice17"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 33) = "Y" Then
                UpdRow17(Session("mLice17"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 35) = "N" Then
                InRow18(Session("mLice18"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 35) = "Y" Then
                NumbPersons = NumbPersons - 1
                UpdRow18(Session("mLice18"))
                myResult = True
            End If
        End If
        If NumbPersons <> 0 Then
            If Lica(ind + 37) = "N" Then
                InRow19(Session("mLice19"))
                NumbPersons = NumbPersons - 1
                myResult = True
            ElseIf Lica(ind + 37) = "Y" Then
                UpdRow19(Session("mLice19"))
                NumbPersons = NumbPersons - 1
                myResult = True
            End If
        End If
        Return myResult
    End Function
    'Вмъкване
    Sub InRow1(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False
        Dim actv As Boolean = False
        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("01", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow1(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year = @year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("02", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow2(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("02", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow2(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year = @year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("02", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow3(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("03", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow3(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("03", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow4(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("04", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow4(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("04", "Редакция на лице", " '")
    End Sub


    'Вмъкване
    Sub InRow5(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("05", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow5(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False
        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("05", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow6(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("06", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow6(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("06", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow7(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("07", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow7(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("07", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow8(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False
        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("08", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow8(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("08", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow9(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("09", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow9(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("09", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow10(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("10", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow10(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False
        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("10", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow11(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("11", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow11(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("11", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow12(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("12", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow12(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("12", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow13(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False
        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("13", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow13(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("13", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow14(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("14", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow14(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("14", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow15(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("15", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow15(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("15", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow16(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("16", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow16(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("16", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow17(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("17", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow17(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))

            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("17", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow18(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("18", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow18(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("18", "Редакция на лице", " '")
    End Sub

    'Вмъкване
    Sub InRow19(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsRes As Integer = 0
        Dim MyRes As Boolean = False

        Dim myIns As String = "INSERT INTO " + myTbl + " (oblast, grs, gnezdo, domak, lice,  rel, husb, father, mother, sex, db, mb, yb, age, actv, d11, d12, d13, d14, d15, d16, d17, d18, d19, tm, year)  VALUES ( @oblast, @grs, @gnezdo, @domak, @lice, @rel, @husb, @father, @mother, @sex, @db, @mb, @yb, @age, @actv, @d11, @d12, @d13, @d14, @d15, @d16, @d17, @d18, @d19, @tm, @year )"
        Using cmd As New SqlCommand(myIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", Val(Session("ObsNumb")))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsRes = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("19", "Въвеждане на лице", " '")
    End Sub

    'Добавяне
    Sub UpdRow19(ByVal mLice As Object)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim rowsResD As Integer = 0
        Dim rowsResL As Integer = 0
        Dim MyRes As Boolean = False

        Dim myUpdL As String = "UPDATE " + myTbl + " SET  lice=@lice, rel=@rel, husb=@husb, father=@father, mother=@mother, sex=@sex, db=@db, mb=@mb, yb=@yb, age=@age, actv=@actv, d11=@d11, d12=@d12, d13=@d13, d14=@d14, d15=@d15, d16=@d16, d17=@d17, d18=@d18, d19=@d19, year=@year WHERE oblast = @oblast AND grs=@grs AND gnezdo = @gnezdo AND domak = @domak AND lice=@lice and tm=@tm"
        Using cmd As New SqlCommand(myUpdL, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@tm", CType(Session("ObsNumb"), Integer))
            cmd.Parameters.AddWithValue("@lice", mLice.Stlice)
            cmd.Parameters.AddWithValue("@rel", mLice.Strel)
            cmd.Parameters.AddWithValue("@husb", mLice.Sthusb)
            cmd.Parameters.AddWithValue("@father", mLice.Stfather)
            cmd.Parameters.AddWithValue("@mother", mLice.Stmother)
            cmd.Parameters.AddWithValue("@sex", mLice.Stsex)
            cmd.Parameters.AddWithValue("@db", mLice.Stdb)
            cmd.Parameters.AddWithValue("@mb", mLice.Stmb)
            cmd.Parameters.AddWithValue("@yb", mLice.Styb)
            cmd.Parameters.AddWithValue("@age", mLice.Stage)
            cmd.Parameters.AddWithValue("@d11", mLice.Std11)
            cmd.Parameters.AddWithValue("@d12", mLice.Std12)
            cmd.Parameters.AddWithValue("@d13", mLice.Std13)
            cmd.Parameters.AddWithValue("@d14", mLice.Std14)
            cmd.Parameters.AddWithValue("@d15", mLice.Std15)
            cmd.Parameters.AddWithValue("@d16", mLice.Std16)
            cmd.Parameters.AddWithValue("@d17", mLice.Std17)
            cmd.Parameters.AddWithValue("@d18", mLice.Std18)
            cmd.Parameters.AddWithValue("@d19", mLice.Std19)
            cmd.Parameters.AddWithValue("@actv", mLice.Stactv)
            cmd.Parameters.AddWithValue("@year", Session("GodNabl"))
            con.Open()
            Try
                rowsResL = cmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try
            con.Close()
            MyRes = True
        End Using
        InsertLog("19", "Редакция на лице", " '")
    End Sub

    Private Function InsertLog(sLice As String, action1 As String, action2 As String) As Boolean
        Dim rowsRes As Integer = 0
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
        Dim con As New SqlConnection(sCnn)
        Dim LogIns As String = "INSERT INTO Log (cName,cRol,oblast, grs, gnezdo, domak, lice, data, page, action,IPAddr) VALUES (@cName,@cRol, @oblast, @grs, @gnezdo, @domak, @lice, @data, @page, @action, @IPAddr )"
        Using cmd As New SqlCommand(LogIns, con)
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@cName", Session("potr"))
            cmd.Parameters.AddWithValue("@cRol", Session("rol"))
            cmd.Parameters.AddWithValue("@oblast", Session("oblast"))
            cmd.Parameters.AddWithValue("@grs", Session("grs"))
            cmd.Parameters.AddWithValue("@gnezdo", Session("gnezdo"))
            cmd.Parameters.AddWithValue("@domak", Session("domak"))
            cmd.Parameters.AddWithValue("@lice", sLice)
            cmd.Parameters.AddWithValue("@data", DateTime.Now.ToLocalTime())
            cmd.Parameters.AddWithValue("@page", PageName)
            cmd.Parameters.AddWithValue("@action", action1 + " " + sLice + " " + action2)
            cmd.Parameters.AddWithValue("@IPAddr", Session("IPAddress"))
            con.Open()
            rowsRes = cmd.ExecuteNonQuery()
            con.Close()
        End Using

        Return 1
    End Function

    Public Function GetCurrentPageName() As String
        Dim Path As String = System.Web.HttpContext.Current.Request.Url.AbsolutePath
        Dim Info As System.IO.FileInfo = New System.IO.FileInfo(Path)
        Dim pageName As String = Info.Name
        Return pageName
    End Function



    Protected Sub txtDate_TextChanged(sender As Object, e As EventArgs) Handles txtDate.TextChanged

    End Sub

    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        RecordData()
        Dim script As String = "window.onload = function() { validatePers(myPers); };"
        ClientScript.RegisterStartupScript(Me.GetType(), "validatePers", script, True)
        Response.Redirect("~/VavejdDom2.aspx")
    End Sub
End Class
