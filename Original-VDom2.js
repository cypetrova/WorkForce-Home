/*Изчислява броя на лицата в домакинството, които са в момента на наблюдението*/
function persNumb() {
        var pNum = 0;
        var table = document.getElementById('Maincontent_tPersons');
        for (var r = 1, n = table.rows.length; r < n; r++) {
            var val = table.rows[r].cells[10].innerHTML;
            if (val == 'Да') {
                pNum = pNum + 1;
            }
            document.getElementById("Maincontent_HsHldNum").value = pNum;            
        }

    

        //const myRows = ["Maincontent_btn01", "Maincontent_btn02", "Maincontent_btn03", "Maincontent_btn04", "Maincontent_btn05", "Maincontent_btn06", "Maincontent_btn07", "Maincontent_btn08", "Maincontent_btn09", "Maincontent_btn10", "Maincontent_btn11", "Maincontent_btn12", "Maincontent_btn13", "Maincontent_btn14", "Maincontent_btn15", "Maincontent_btn16", "Maincontent_btn17", "Maincontent_btn18", "Maincontent_btn19"];
        //for (var i = 0; i < myRows.length; i++) {
        //    var myR = document.getElementById(myRows[i]);
        //    if (myR !== null) {
        //        vRows++;
        //    }
        //}
        return pNum;
}

/*За тези, които липсват в домакинството към момента на наблюдението*/
function persNumbD1a() {
    var pNum1 = 0;
    var table = document.getElementById('Maincontent_tPersons');
    for (var r = 1, n = table.rows.length; r < n; r++) {
        var val = table.rows[r].cells[10].innerHTML;
        if (val == 'Не') {
            pNum1 = pNum1 + 1;
        }
    }

    document.getElementById("Maincontent_NmbPrs").value = pNum1;
    return pNum1;    
}

/*Проверка на броя на лицата.Броят на лицата, за които не може да бъде предоставена информация е равен или по малък от общата бройка*/
function lica_Check() {
    var pers = $("#Maincontent_HsHldNum").val();
    var persN = $('#Maincontent_NmbPrs').val();
    pers = Number(pers);
    persN = Number(persN);
    if (persN > pers) {
        $("#Maincontent_LMessage").show();
        alert("Броя на членовете, за които не може да бъде предоставена информация е по-голям от броя на членовете в домакинството");
    } else if (persN === pers) {
        $("#Maincontent_LMessage").show();
        alert("Броя на членовете, за които не може да бъде предоставена информация е равен на броя на членовете в домакинството");
    } else {
        $("#Maincontent_LMessage").hide();
        return pers - persN;
    }
}

/*Добавяне на лице.Появява се таблицата с характеристиките за лицето*/
function AddPerson() {
    ClearPerson("Add");
    var tperson = document.getElementById("tPerson");
    var but = document.getElementById("ChkData");
    var nPersons = document.getElementById("Maincontent_tPersons").rows.length - 1;
    if (nPersons >= 19) {        
        alert("Броя на лицата е над 19.");
        document.getElementById("tPerson").style.display = "none";        
    }
    else {
        tperson.style.display = "block";        
    }
    but.style.display = "none";
    
    
    document.getElementById("Maincontent_hNPers").value = document.getElementById("Maincontent_tPersons").rows.length - 1;

    //var nPersons = parseInt(document.getElementById("Maincontent_hNPers").value);
    document.getElementById("Maincontent_HRowN").value = "Add";
   
            if ((nPersons == 0) || (nPersons < 9)) {
                nPersons = nPersons + 1;
                document.getElementById("Maincontent_PersonD2").value = "0" + String(nPersons)
            } else if (nPersons <= 19) {
                document.getElementById("Maincontent_PersonD2").value = nPersons + 1;                
            //} else {            
            }
   

}

vb code
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