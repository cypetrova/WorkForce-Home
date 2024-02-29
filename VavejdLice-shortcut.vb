Imports System.Data
Imports System.Configuration
Imports System.Data.SqlClient
Partial Class VavejdLice
    Inherits System.Web.UI.Page

    Dim varArea As String
    Dim varTextPlace As String
    Dim varNestArea As String
    Dim varHouseNest As String
    Dim dtArea As New DataTable
    Dim dtNestNumb As New DataTable
    Dim dtHsHold As New DataTable
    Dim dtPrsHold As New DataTable

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim SessionFlag As Boolean = True
        rolia.Text = Session("rol")
        potr.Text = Session("potr")
        oblast.Text = Session("cRsu")

        If Session("rol") = "" Then
            Response.Redirect("~/Default.aspx")
        End If

        MandatoryCheck.Visible = False


        If Not Me.IsPostBack And SessionFlag Then
            Try
                LoadArea()
                LoadHsHold()
            Catch ex As Exception
                Response.Redirect("CommonErr.aspx")
            End Try
        End If

    End Sub
    Protected Sub ddlArea_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlArea.SelectedIndexChanged
        MandatoryCheck.Visible = False
    End Sub
    Protected Sub ddlPlace_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPlace.SelectedIndexChanged
        MandatoryCheck.Visible = False
        ddlNestNumb.Items.Clear()
        If ddlArea.Text.Trim <> "" Then
            varArea = ddlArea.Text.Remove(2)
            varTextPlace = ddlPlace.Text
            LoadNestArea()
        Else
            MyLbl.Text = "Не е попълнена областта.."
            MandatoryCheck.Visible = True
        End If

    End Sub
    Protected Sub ddlNestNumb_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlNestNumb.SelectedIndexChanged
        MandatoryCheck.Visible = False
    End Sub
    Protected Sub LoadHsHold()
        'Зарежда само наличните домакинства от табл. d
        Dim sSq1 As String
        Dim cnn As New SqlConnection
        Dim cmd As SqlCommand

        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_FinActConnectionString").ConnectionString
        cnn.ConnectionString = sCnn
        sSq1 = "SELECT dom FROM d ORDER BY 1"
        cmd = New SqlCommand(sSq1, cnn)
        Dim ad1 As New SqlDataAdapter
        ad1.SelectCommand = cmd
        Try
            cnn.Open()
            ad1.Fill(dtHsHold)
            cnn.Close()
        Catch ex As Exception
            ex.Message.ToString()
        End Try
        cnn.Dispose()
        cmd.Dispose()

        ddlHouseHold.Items.Add(" ")
        Dim dr As DataRow
        For Each dr In dtHsHold.Rows
            ddlHouseHold.Items.Add(dr.Item(0))
        Next
    End Sub
    Protected Sub LoadPersonHsHold()
        'domak
        ddlPersonHouseHold.Items.Clear()
        Dim sSq1 As String
        Dim cnn As New SqlConnection
        Dim oblast As String = ddlArea.Text.Trim.Substring(0, 2)
        Dim grs As Integer = Val(ddlPlace.Text.Trim)
        Dim gnezdo As String = ddlNestNumb.Text.Trim
        Dim domak As String = ddlHouseHold.Text.Trim
        Dim tm As Integer = Val(GetObsTime(domak))
        Session.Add("ObsNumb", tm)
        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_FinActConnectionString").ConnectionString

        cnn.ConnectionString = sCnn
        sSq1 = "SELECT lice FROM lica WHERE oblast = @oblast AND grs = @grs  AND gnezdo = @gnezdo  AND domak = @domak AND tm=@tm ORDER BY 1"
        Dim cmd As New SqlCommand(sSq1, cnn)
        cmd.CommandType = CommandType.Text
        cmd.Parameters.Add("@oblast", SqlDbType.NVarChar)
        cmd.Parameters("@oblast").Value = oblast
        cmd.Parameters.Add("@grs", SqlDbType.TinyInt)
        cmd.Parameters("@grs").Value = grs
        cmd.Parameters.Add("@gnezdo", SqlDbType.NVarChar)
        cmd.Parameters("@gnezdo").Value = gnezdo
        cmd.Parameters.Add("@domak", SqlDbType.NVarChar)
        cmd.Parameters("@domak").Value = domak
        cmd.Parameters.Add("@tm", SqlDbType.TinyInt)
        cmd.Parameters("@tm").Value = tm
        Dim ad1 As New SqlDataAdapter
        ad1.SelectCommand = cmd
        Try
            cnn.Open()
            ad1.Fill(dtPrsHold)
            cnn.Close()
        Catch ex As Exception
            ex.Message.ToString()
            End Try
        cnn.Dispose()
        cmd.Dispose()

        ddlPersonHouseHold.Items.Add(" ")
        Dim dr As DataRow
        For Each dr In dtPrsHold.Rows
            ddlPersonHouseHold.Items.Add(dr.Item(0))
        Next

    End Sub
    Protected Sub LoadNestArea()
        Dim NumbFrom As Int16
        Dim NumbTo As Int16

        Try
            Dim cnn As New SqlConnection
            Dim cmd As SqlCommand
            Dim MySQL As String

            Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_FinActConnectionString").ConnectionString
            cnn.ConnectionString = sCnn
            MySQL = "Select NumbFrom,NumbTo FROM NomGnezdoDom WHERE Area = " + varArea + " And " + "PlaceType = " + varTextPlace + " ORDER BY 1"
            cmd = New SqlCommand(MySQL, cnn)
            Try
                cnn.Open()
                Dim dr As SqlDataReader = cmd.ExecuteReader
                While dr.Read()
                    NumbFrom = dr("NumbFrom")
                    NumbTo = dr("NumbTo")
                End While
                dr.Close()
                cnn.Close()
            Catch ex As Exception
                ex.ToString()
            End Try
            cnn.Dispose()
            cmd.Dispose()
            ddlNestNumb.Items.Add(" ")
            For i As Integer = CType(NumbFrom, Integer) To CType(NumbTo, Integer) Step 1
                ddlNestNumb.Items.Add(i.ToString)
            Next

        Catch ex As Exception
            ex.ToString()
            Response.Redirect("CommonErr.aspx")
        End Try
    End Sub
    Sub ClearText()
        ddlArea.SelectedIndex = -1
        ddlPlace.SelectedIndex = -1
        ddlNestNumb.SelectedIndex = -1
        ddlHouseHold.SelectedIndex = -1
        ddlPersonHouseHold.SelectedIndex = -1
    End Sub
    Sub LoadArea()
        Dim sSq1 As String
        Dim cnn As New SqlConnection
        Dim cmd As SqlCommand

        Dim sCnn As String = ConfigurationManager.ConnectionStrings("nsi_FinActConnectionString").ConnectionString
        cnn.ConnectionString = sCnn

        Select Case Session("cRsu")
            Case "00"
                sSq1 = "SELECT tsb, ime FROM tsb ORDER BY 1"
            Case <> "00"
                sSq1 = "SELECT tsb, ime FROM tsb where  tsb = '" + Session("cRsu") + "'  "
        End Select
        cmd = New SqlCommand(sSq1, cnn)
        Dim ad1 As New SqlDataAdapter
        ad1.SelectCommand = cmd
        Try
            cnn.Open()
            ad1.Fill(dtArea)
            cnn.Close()
        Catch ex As Exception
            ex.Message.ToString()
        End Try
        cnn.Dispose()
        cmd.Dispose()

        ddlArea.Items.Add(" ")
        Dim dr As DataRow
        For Each dr In dtArea.Rows
            ddlArea.Items.Add(dr.Item(0) + "-" + dr.Item(1))
        Next
    End Sub
    Protected Sub ddlHouseHold_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlHouseHold.SelectedIndexChanged
        LoadPersonHsHold()
    End Sub
    Sub liceprov()
        MandatoryCheck.Visible = False
        lstbMErr.Visible = False
        lstbMErr.Items.Clear()

        If String.IsNullOrEmpty(ddlArea.Text.Trim) Then
            lstbMErr.Items.Add("Не е попълненa областта")
        End If
        If String.IsNullOrEmpty(ddlPlace.Text.Trim) Then
            lstbMErr.Items.Add("Не е попълнено населеното място")
        End If
        If String.IsNullOrEmpty(ddlNestNumb.Text.Trim) Then
            lstbMErr.Items.Add("Не е попълнен номера на гнездото")
        End If
        If String.IsNullOrEmpty(ddlHouseHold.Text.Trim) Then
            lstbMErr.Items.Add("Не е попълнен номера на домакинството")
        End If
        If String.IsNullOrEmpty(ddlPersonHouseHold.Text.Trim) Then
            lstbMErr.Items.Add("Не е попълнен номера на лицето в домакинството")
        End If

        If lstbMErr.Items.Count() > 0 Then
            lstbMErr.Visible = True
            MandatoryCheck.Visible = True
            MyLbl.Visible = False
        End If

    End Sub
    Protected Sub PrevPage_Click(sender As Object, e As EventArgs) Handles PrevPage.Click
        Server.Transfer("~\MainMenu.aspx")
    End Sub
    Protected Sub NextPage_Click(sender As Object, e As EventArgs) Handles NextPage.Click

        liceprov()
        If lstbMErr.Items.Count() <= 0 Then
            Dim sObl As String = ddlArea.Text.Remove(2, (ddlArea.Text.Length - 2))
            Dim ObsNumb As Integer = Val(Session("ObsNumb"))
            Session.Add("oblast", sObl)
            Session.Add("grs", ddlPlace.Text.Trim)
            Session.Add("gnezdo", ddlNestNumb.Text)
            Session.Add("domak", ddlHouseHold.Text.Trim)
            Session.Add("lice", ddlPersonHouseHold.Text.Trim)

            If Not (Session("oblast").Equals("") Or Session("grs").Equals("") Or Session("gnezdo").Equals("") Or Session("domak").Equals("")) Then
                Dim aPrsExAct() As Integer = PersChk(Session("oblast"), Session("grs"), Session("gnezdo"), Session("domak"), Session("lice"), ObsNumb)
                If aPrsExAct(0) = 1 Then
                    If aPrsExAct(1) = 1 Then
                        'Проверка в таблица lica за възрастта на лицето
                        Dim mAge = GetAge(Session("oblast"), Session("grs"), Session("gnezdo"), Session("domak"), Session("lice"), Session("ObsNumb"))
                        Session.Add("Age", mAge)
                        'If 15 <= mAge <= 89 Then
                        If mAge >= 15 And mAge <= 89 Then
                            Session("ObsNumb") = ObsNumb
                            'Наблюдение
                            If ObsNumb = "3" Then
                                Response.Redirect("~\web_v1p1.aspx")
                            ElseIf ObsNumb = "1" Or ObsNumb = "2" Or ObsNumb = "4" Then
                                Response.Redirect("~\web_v2p1.aspx")
                            Else
                                Response.Redirect("~\CommonErr.aspx")
                            End If
                        Else
                            Dim cstype As Type = Me.[GetType]()
                            Dim csname1 As [String] = "checkAge"
                            Dim cs As ClientScriptManager = Page.ClientScript

                            If Not cs.IsStartupScriptRegistered(cstype, csname1) Then
                                Dim cstext1 As New StringBuilder()
                                cstext1.Append("<script type=text/javascript> alert('Лицето не подлежи на анкетиране, тъй като е под 15 години или над 89 години.') </")
                                cstext1.Append("script>")
                                Page.ClientScript.RegisterStartupScript(cstype, csname1, cstext1.ToString())
                            End If
                        End If
                        ClearText()
                    Else
                        Dim cstype As Type = Me.[GetType]()
                        Dim csname2 As [String] = "checkPerson"
                        Dim cs As ClientScriptManager = Page.ClientScript

                        If Not cs.IsStartupScriptRegistered(cstype, csname2) Then
                            Dim cstext2 As New StringBuilder()
                            cstext2.Append("<script type=text/javascript> alert('Няма такъв номер лице в домакинството.') </")
                            cstext2.Append("script>")
                            Page.ClientScript.RegisterStartupScript(cstype, csname2, cstext2.ToString())
                        End If
                        ClearText()
                    End If
                Else
                    Dim cstype3 As Type = Me.[GetType]()
                    Dim csname3 As [String] = "checkActive"
                    Dim cs1 As ClientScriptManager = Page.ClientScript

                    If Not cs1.IsStartupScriptRegistered(cstype3, csname3) Then
                        Dim cstext3 As New StringBuilder()
                        cstext3.Append("<script type=text/javascript> alert('Лицето не е активно в домакинството.') </")
                        cstext3.Append("script>")
                        Page.ClientScript.RegisterStartupScript(cstype3, csname3, cstext3.ToString())
                    End If
                End If
            Else
                MandatoryCheck.Visible = True
                ClearText()
            End If

        End If


    End Sub

End Class
