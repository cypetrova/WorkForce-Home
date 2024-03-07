 //за бебета да не се отключва Д15 
 
 var errMsg = "Лице " + document.getElementById("Maincontent_TextBox2") + " - Д15 (години в България) трябва да е по-малко или равно от Д10 (възраст)";
 
 mErrors.push({ merror: 'Лице ' + i + 'номер на лице (Д2) - Д15 (години в България) трябва да е по-малко или равно от Д10 (възраст) });
 
 function D15(val1) {

    var mVal = 1;    
    
    //var errMsg = "Д15<= Д10(възраст).Задължителна проверка.";
    var errMsg = "Д15<= Д10(възраст).Задължителна проверка.";
    var errMsg = "Възрастта на лицето (Д10) трябва да е по-голяма или равна на годините в България (Д15) ";
    var cMTable = 0;
    var mD15 = document.getElementById("Maincontent_TextBox15").value;
    mD15 = mD15.slice(0, 2);
    var age = parseInt(document.getElementById("Maincontent_TextBox10").value);
    if (age != 0) { 
        if (mD15 >= age) {
            cMTable = 1;
            mVal = OtherErrors(cMTable, errMsg);
            return mVal;
        } else {
            OtherErrors(cMTable, "");
            if (val1 != "Rec") {
                OtherErrors(cMTable, "");
                if ((mD15 >= 11) && (mD15 < 99)) {
                    document.getElementById("Maincontent_DDL16").disabled = true;
                    document.getElementById("Maincontent_DDL16").value = " ";
                    document.getElementById("Maincontent_HDDL16").value = "L";
                    return mVal;
                } else {
                    document.getElementById("Maincontent_DDL16").disabled = false;
                    document.getElementById("Maincontent_DDL16").value = " ";
                    document.getElementById("Maincontent_HDDL16").value = "U";
                    return mVal;
                }
            }
        }
    }

    return mVal;

}