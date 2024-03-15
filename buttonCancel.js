</td><td></td><td><button type="button" id="bCancel" onclick="javascript:bCancelClick();">Отказ</button></td>


function bCancelClick() {
    document.getElementById("Maincontent_HRowN").value = 'N';
    document.getElementById('Maincontent_PersonD2').value = "";
    document.getElementById("Maincontent_DDL4").selectedIndex = 0;
    document.getElementById("Maincontent_DDL5").selectedIndex = 0;
    document.getElementById("Maincontent_DDL6").selectedIndex = 0;
    document.getElementById("Maincontent_DDL7").selectedIndex = 0;
    document.getElementById("Maincontent_DDL8").selectedIndex = 0;
    document.getElementById("Maincontent_TextBox7").value = "";
    document.getElementById("Maincontent_TextBox8").value = "";
    document.getElementById("Maincontent_TextBox9").value = "";
    document.getElementById("Maincontent_TextBox10").value = "";
    document.getElementById("Maincontent_DDL11").selectedIndex = 0;
    document.getElementById("Maincontent_DDL12").selectedIndex = 0;
    document.getElementById("Maincontent_DDL13").selectedIndex = 0;
    document.getElementById("Maincontent_DDL14").selectedIndex = 0;
    document.getElementById("Maincontent_TextBox15").value = "";
    document.getElementById("Maincontent_DDL16").selectedIndex = 0;
    document.getElementById("Maincontent_DDL17").selectedIndex = 0;
    document.getElementById("Maincontent_DDL18").selectedIndex = 0;
    document.getElementById("Maincontent_DDL19").selectedIndex = 0;
    document.getElementById("Maincontent_DDL20").selectedIndex = 0;

    document.getElementById("tPerson").classList.add("hidden");
	//за момента таблицата не се скрива
}