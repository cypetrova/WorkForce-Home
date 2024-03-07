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