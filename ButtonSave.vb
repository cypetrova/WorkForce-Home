//Button Save

 //<tr><td></td><td><button type="button" id="bRecData" onclick="javascript:RecPerson();">Запис</button></td>
   //            <td></td><td><button type="button" id="bCancel" onclick="javascript:CancelPerson();">Отказ</button></td>
			   
			    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        RecordData()
        Response.Redirect("~/VavejdDom2.aspx")
    End Sub