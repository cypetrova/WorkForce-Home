$(document).ready(function () {

    if ($("#Maincontent_hNPers").length > 0) {       
        $("#tPersons").show();
        $("#tPerson").hide();
        $("#SError").hide();
        //var myrows = $("#Maincontent_NPers").val();
        //ShowRows(myrows);
        /*$("#Maincontent_btn01").click(PersonClick);*/
        attachEvents();
    } else {
        $("#tPerson").hide();
    }

    if (($("#tPersons").length > 0) && ($(".navbar").length > 0) && ($(".footer").length > 0)) {

        var tblW = $("#tPersons").width();
        $(".navbar").width(tblW);
        $(".footer").width(tblW);
    }
});
/*Тази функция слуша събитието за промяна на размера на прозореца и след това задава ширината на елементите с клас "navbar" и "footer" да бъде равна на ширината на таблицата с идентификатор "#tPersons".*/
$(window).resize(function () {
    var tblW = $("#tPersons").width();
    $(".navbar").width(tblW);
    $(".footer").width(tblW);

}); 
/*Функция за ширината на табл. tPersons*/

function getw() {
    var tblW = document.getElementById("tPersons").offsetWidth;
    document.getElementById("site-header").style.width = tblW + "px";
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
/*Скриване на елемент*/
function HideTPerson() {
    document.getElementById("Maincontent_tPerson").style.display = "none";
}
/*function myDay(mDay, mMonth) – Проверка на деня.Автоматично попълване с 00 или 99, function myMonth(mMonth, mDay) – Проверка на месеца. Автоматично попълване с 00 или 99 
function myYear(mYear, mMonth, mDay) - Прави проверка за въведени месец и година и спазване на ТЗ за 1900 година*/

function myDay(mDay, mMonth) {
    const personName = document.getElementById("Maincontent_PersonD2").value;
    const dayInput = document.getElementById(mDay).value;

    if (dayInput.length === 0) {
        alert(`Лице ${personName} - не е въведен ден на раждане`);
        document.getElementById(mDay).value = "";
        return 0;
    }

    if (dayInput.length < 2) {
        alert(`Лице ${personName} - неправилно въведени дата и/или месец на раждане`);
        return 0;
    }

    const day = parseInt(dayInput);

    if (day === 0) {
        document.getElementById("Maincontent_TextBox8").value = "00";
        return 1;
    }

    if (day === 99) {
        document.getElementById("Maincontent_TextBox8").value = "99";
        return 1;
    }

    if (day > 31) {
        alert(`Лице ${personName} - неправилно въведени дата и/или месец на раждане`);
        return 0;
    }

    return 1;
}
/*Функция за месеца */
function myMonth(mMonth, mDay) {
    const personName = document.getElementById("Maincontent_PersonD2").value;
    const dayInput = document.getElementById(mDay).value;
    const monthInput = document.getElementById(mMonth).value;

    if (dayInput.length === 0) {
        alert(`Лице ${personName} - не е въведен дeн на раждане`);
        document.getElementById(mDay).value = "";
        return 0;
    }

    if (monthInput.length < 2) {
        alert(`Лице ${personName} - неправилно въведени дата и/или месец на раждане`);
        return 0;
    }

    const month = parseInt(monthInput);

    if (month === 0) {
        document.getElementById(mDay).value = "00";
        return 1;
    }

    if (month === 99) {
        document.getElementById(mDay).value = "99";
        return 1;
    }

    if (month > 12) {
        alert(`Лице ${personName} - неправилно въведени дата и/или месец на раждане`);
        return 0;
    }
    return 1;
}
/*Функция за годината*/
function myYear(mYear, mMonth) {
    const cYear = new Date().getFullYear();
    const yearInput = document.getElementById(mYear).value;
    const monthInput = document.getElementById(mMonth).value;
    const personName = document.getElementById("Maincontent_PersonD2").value;

    if (monthInput.length === 0) {
        alert(`Лице ${personName} - не е въведен месец на раждане`);
        return 0;
    }

    if (yearInput.length === 0 || isNaN(yearInput)) {
        alert(`Лице ${personName} - неправилно въведена година на раждане`);
        document.getElementById(mYear).value = "";
        return 0;
    }

    const year = parseInt(yearInput);

    if (year < 1900) {
        alert(`Лице ${personName} - годината на раждане не трябва да е по-малка от 1900 година`);
        document.getElementById(mYear).value = "";
        return 0;
    }

    if (year > cYear) {
        alert(`Лице ${personName} - годината на раждане не трябва да е по-голяма от ${cYear} година`);
        document.getElementById(mYear).value = "";
        return 0;
    }

    return 1;
}
/*Валидация на полетата за ден,месец и година */

function myDMYNotEmpty(mDay, mMonth, mYear) {
    let dayValue = document.getElementById(mDay).value;
    let monthValue = document.getElementById(mMonth).value;
    let yearValue = document.getElementById(mYear).value;
    
    if (dayValue.length === 2 && monthValue.length === 2 && yearValue.length === 4) {
       
        let day = parseInt(dayValue);
        let month = parseInt(monthValue);

        if (!isNaN(day) && !isNaN(month) && day > 0 && day <= 31 && month > 0 && month <= 12) {
            return 1; 
        }
    }

    return 0; 
}
/*Функции за възрастта*/
function calculateAge(mElem) {
    let dayElem = document.getElementById("Maincontent_TextBox7").value;
    let monthElem = document.getElementById("Maincontent_TextBox8").value;
    let yearElem = document.getElementById("Maincontent_TextBox9").value;
    let ageElem = document.getElementById("Maincontent_TextBox10");

    if (!myDay(dayElem)) {
        alert("Моля, въведете валиден ден.");
        return 0;
    }

    if (!myMonth(monthElem, dayElem)) {
        alert("Моля, въведете валиден месец.");
        return 0;
    }

    if (!myYear(yearElem, monthElem)) {
        alert("Моля, въведете валидна година.");
        return 0;
    }

    let day = parseInt(dayElem);
    let month = parseInt(monthElem);
    let year = parseInt(yearElem);

    if (day === 0 && month === 0) {
        ageElem.value = CalculateAge(dayElem, monthElem, yearElem);
    } else if (day === 99 && month === 99) {
        ageElem.value = CalculateAge(dayElem, monthElem, yearElem);
    } else {
        let myAge = CalculateAge(dayElem, monthElem, yearElem);
        if (myAge < 0) {
            alert("Възрастта не може да се пресметне при невалиден ден, месец или невалидна година. Моля коригирайте...");
            ageElem.value = "";
        } else {
            ageElem.value = myAge;
        }
    }
    age74(parseInt(ageElem.value));
    return 1;
}
/*Функция за изчисление на години между две дати */
function calcAge(NablDate, myBDay) {
    let nablDateObj = new Date(NablDate);
    let myBDayObj = new Date(myBDay);

    let nablYear = nablDateObj.getFullYear();
    let myBDayYear = myBDayObj.getFullYear();

    let age = nablYear - myBDayYear;

    // Проверка дали датата на раждане е преди датата на измерване на възрастта
    // Ако е така, намаляваме възрастта с 1
    if (nablDateObj < myBDayObj) {
        age--;
    }

    return age;
}
/*Функцията включва проверки за валидност на датата, изчисляване на възрастта и корекции, когато възрастта трябва да бъде ограничена до 99 години. */ 
function CalculateAge(day, month, year) {
    let iDMYcheck = DMYcheck(day, month, year);
    
    if (iDMYcheck === 1) {
        let MyAge = 0;       
       
        const sMyDates = DateSplit();
        let NablYear = Number(sMyDates[2]);        

        if (day.length === 1) {
            day = "0" + day;
        }

        if (month.length === 1) {
            month = "0" + month;
        }

        if (day === "00" || month === "00") {
            MyAge = Number(NablYear) - Number(year);
            return Hundr(MyAge);            
        }

        if (day === "99" || month === "99") {
            MyAge = Number(NablYear) - Number(year) - 1;
            return Hundr(MyAge);            
        }

        let MyDOB = year + month + day;
        let iAgeDiff = Number(CalculateAgeDiff(MyDOB));
        MyAge = Number(NablYear) - Number(year) - iAgeDiff;
        return Hundr(MyAge);       
    } else {
        return 0;       
    }
    
    
}
/*Изчисление за възрастта дали е >= 100/ трябва само промяна в името на фукцията - Hunder = LimitAgeTo99*/
function Hundr(iAge) {
    if (iAge >= 100) {
        iAge = 99
        return iAge
    }
    return iAge
}

/*Функция за сравняване на рожденна дата и текуща дата */
function CalculateAgeDiff(MyDOB) {
    const sMyDates = DateSplit();
    const MyNablMonths = Number(sMyDates[1]);
    const MyNablDays = Number(sMyDates[0]);
    const MyDOBMonths = Number(MyDOB.substring(4, 6));
    const MyDOBDays = Number(MyDOB.substring(6));

    if (MyDOBMonths < MyNablMonths || (MyDOBMonths === MyNablMonths && MyDOBDays <= MyNablDays)) {
        return 0;
    } else {
        return 1;
    }
}

/*Функция за изчисление на годината дали е високосна или не */
function MonthDayYCheck(sDay, sMonth, sYear) {
    let iLeap = (sYear % 4 == 0 && sYear % 100 != 0) || (sYear % 400 == 0);

    if ((sDay != "00" || sDay != "99") && (sMonth === "00" || sMonth === "99")) {
        return 1;
    }
    if ((sMonth === "01" || sMonth === "03" || sMonth === "05" || sMonth === "07" || sMonth === "08" || sMonth === "10" || sMonth === "12") && sDay > 31) {
        return 0;
    }
    else if ((sMonth === "04" || sMonth === "06" || sMonth === "09" || sMonth === "11") && sDay > 30) {
        return 0;
    }
    else if (sMonth === "02" && sDay > 29 && iLeap) {
        return 0;
    }
    else if (sMonth === "02" && sDay > 28 && !iLeap) {
        return 0;
    }
    else if (sMonth > 12) {
        return 0;
    }
    return 1;
}

/*Спомагателна функция за проверка и изчисляване на възрастта на лицето, като проверява за месеците дали са над 12 и дните в месеца са над 31*/
function DMYcheck(sDay, sMonth, sYear) {
    let cYear = new Date().getFullYear();
    let iDay = parseInt(sDay, 10); // Парсиране на числовите стойности
    let iMonth = parseInt(sMonth, 10);
    let iYear = parseInt(sYear, 10);

    // Проверка за невалидни дати (00 и 99)
    if ((iDay === 0 || iDay === 99) || (iMonth === 0 || iMonth === 99)) {
        return false;
    }

    // Проверка на валидността на месеца
    if (iMonth < 1 || iMonth > 12) {
        return false;
    }

    // Проверка на валидността на годината
    if (iYear < 1900 || iYear > cYear) {
        return false;
    }

    // Проверка на валидността на деня
    if (iDay < 1 || iDay > 31) {
        return false;
    }

    // Проверка за високосна година
    let isLeapYear = (iYear % 4 === 0 && iYear % 100 !== 0) || (iYear % 400 === 0);

    // Проверка на дните за валидност в месеца
    if (iMonth === 2) {
        if (isLeapYear && iDay > 29) {
            return false;
        }
        if (!isLeapYear && iDay > 28) {
            return false;
        }
    } else if ((iMonth === 4 || iMonth === 6 || iMonth === 9 || iMonth === 11) && iDay > 30) {
        return false;
    }

    return true;
}
function DateSplit() {
    let sDate = document.getElementById('Maincontent_txtDate').value;
    sDate = sDate.replace(" г.", "");
    const sMyDates = sDate.split(".");
    let sDay = sMyDates[0];
    let sMonth = sMyDates[1];
    let sYear = sMyDates[2];

    if (sDay.length == 1) {
        sDay = "0" + sDay;
    }

    if (sMonth.length == 1) {
        sMonth = "0" + sMonth;
    }

    return {
        day: sDay,
        month: sMonth,
        year: sYear
    };
}
function calculateNewDate() {
    //Взима стойността на старата дата
    var olddateinputElement = document.getElementById("Maincontent_txtDate").value;
    olddateinputElement = olddateinputElement.replace(" г.", "");
    const sMyDates = olddateinputElement.split(".");
    //var oldDate = new Date(olddateinputElement);
    //var oldDate = olddateinputElement;  
    let oldDate = sMyDates[2];       
    
    //Взима текущата година
    let courrentYear = new Date().getFullYear();    
  //alert(courrentYear);
    //Преизчисляване на дата спрямо текуща година
    var newDate = new Date(oldDate);
    newDate.setFullYear(courrentYear);

    //Новата година
    var newDateElement = document.getElementById(newDate);
    newDateElement.textContent = newDate.toISOString().split()[0];
}

/*Ако лицето е над 74 години да се скриват въпроси 17 и 18*/

function age74(myAge) {
    var ddl17 = document.getElementById('Maincontent_DDL17');
    var ddl18 = document.getElementById('Maincontent_DDL18');
    var hddl17 = document.getElementById('Maincontent_HDDL17');

    if (myAge <= 74) {
        ddl17.disabled = false;
        ddl18.disabled = false;
        hddl17.value = "U";
    } else {
        ddl17.disabled = true;
        ddl18.disabled = true;
        hddl17.value = "L";
    }
}
/*Проверка на броя на лицата.Броят на лицата, за които не може да бъде предоставена информация е равен или по малък от общата бройка*/

function lica_Check() {
    var pers = parseInt($("#Maincontent_HsHldNum").val());
    var persN = parseInt($('#Maincontent_NmbPrs').val());

    if (persN > pers) {
        $("#Maincontent_LMessage").show();
        alert("Броят на членовете, за които не може да бъде предоставена информация, е по-голям от броя на членовете в домакинството");
    } else if (persN === pers) {
        $("#Maincontent_LMessage").show();
        alert("Броят на членовете, за които не може да бъде предоставена информация, е равен на броя на членовете в домакинството");
    } else {
        $("#Maincontent_LMessage").hide();
        return pers - persN;
    }
}

/*проверява за определени букви в стринг*/
function afterCh(string, char) {
    return string.split(char)[1] || string;
}

function D15Check() {
    const textBox15Value = $('#Maincontent_TextBox15').val();

    if (textBox15Value.length < 2) {
        $('#Maincontent_TextBox15').val("");
        alert("Моля въведете 2 цифри, като ако числото е едноцифрено, то първата ще бъде 0");
    }
}


function D12(mElem) {
    const mD12 = document.getElementById(mElem).value.slice(0, 3);

    const ddl13 = document.getElementById("Maincontent_DDL13");
    const ddl14 = document.getElementById("Maincontent_DDL14");

    if (mD12 === '001') {
        ddl13.disabled = true;
        ddl13.value = "";
        ddl14.disabled = false;
        ddl14.value = "";
    } else {
        ddl13.disabled = false;
    }
}

function D13(mElem) {
    let mD13 = document.getElementById(mElem).value;
    mD13 = mD13.slice(0, 1);

    if (mD13.trim().length == 0) {
        document.getElementById("Maincontent_DDL14").disabled = true;
        document.getElementById("Maincontent_DDL14").value = "";
        document.getElementById("Maincontent_TextBox15").disabled = false;
        document.getElementById("Maincontent_TextBox15").value = "";
    } else {
        document.getElementById("Maincontent_DDL14").disabled = true;
        document.getElementById("Maincontent_DDL14").value = "";
        document.getElementById("Maincontent_TextBox15").disabled = false;
        document.getElementById("Maincontent_TextBox15").value = "";
    }
}

function afterCh(string, char) {
    return string.split(char)[1] || string;
}

function D14() {
    var mD14 = document.getElementById("Maincontent_DDL14").value.slice(0, 1);
    var page = parseInt(document.getElementById("Maincontent_TextBox10").value);

    // Инициализираме всички елементи, които ще променяме
    var ddl14 = document.getElementById("Maincontent_DDL14");
    var textBox15 = document.getElementById("Maincontent_TextBox15");
    var ddl16 = document.getElementById("Maincontent_DDL16");
    var ddl17 = document.getElementById("Maincontent_DDL17");
    var ddl18 = document.getElementById("Maincontent_DDL18");
    var ddl19 = document.getElementById("Maincontent_DDL19");

    // По подразбиране забраняваме всички полета
    ddl14.disabled = true;
    textBox15.disabled = true;
    ddl16.disabled = true;
    ddl17.disabled = true;
    ddl18.disabled = true;
    ddl19.disabled = true;

    // Ако страницата е по-голяма или равна на 75 и mD14 е празно
    if (page >= 75 && mD14 == " ") {
        ddl14.value = "";
        textBox15.value = " ";
    } else if (page < 75 && mD14 == " ") {
        ddl14.value = "";
        textBox15.value = " ";
        ddl18.disabled = false;
        ddl19.disabled = false;
    } else if (mD14 == '1') {
        textBox15.disabled = false;
        ddl16.disabled = false;
        ddl17.disabled = false;
        ddl18.disabled = false;
        ddl19.disabled = false;
    } else if (mD14 === '2') {
        ddl17.disabled = false;
        ddl18.disabled = false;
    }
}

function D15(val1) {
    var mVal = 1;
    var errMsg = "Д15 <= Д10 (възраст). Задължителна проверка.";
    var errMsg2 = "Лице - Д15 (години в България) трябва да е по-малко или равно на Д10 (възраст).";

    var mD15 = parseInt(document.getElementById("Maincontent_TextBox15").value.slice(0, 2));
    var age = parseInt(document.getElementById("Maincontent_TextBox10").value);

    if (mD15 >= age) {
        OtherErrors(1, errMsg2);
        return mVal;
    } else {
        OtherErrors(0, ""); // Скриваме съобщението за грешка
        if (val1 != "Rec") {
            if (mD15 >= 11 && mD15 < 99) {
                document.getElementById("Maincontent_DDL16").disabled = true;
                document.getElementById("Maincontent_HDDL16").value = "L";
            } else {
                document.getElementById("Maincontent_DDL16").disabled = false;
                document.getElementById("Maincontent_HDDL16").value = "U";
            }
        }
    }

    return mVal;
}

function OtherErrors(cMTable, mErrMsg) {
    const tableContainerM = document.getElementById('MEtable-container');
    if (cMTable == 1) {
        tableContainerM.style.display = "block";
        tableContainerM.innerHTML = createTable([{ merror: mErrMsg }], 'Задължителни проверки');
        return 0;
    } else {
        tableContainerM.style.display = "none";
        return 1;
    }
}

function D19() {
    const chcks = document.querySelectorAll('input[name="MyCheck"]:checked');
    const clicks = chcks.length;
    // document.getElementById("Maincontent_HsHldNum").value = clicks;
}

/*Временен запис на данни в таблицата с членовете от домакинството*/
function RecPerson() {
    var mD15 = D15("Rec");

    if (mD15 == 1) {
        const myPerson = {
            StConn: '0',
            StUsp: '0',
            Stlice: document.getElementById("Maincontent_PersonD2").value,
            Strel: document.getElementById("Maincontent_DDL4").value,
            Sthusb: document.getElementById("Maincontent_DDL5").value,
            Stfather: document.getElementById("Maincontent_DDL6").value,
            Stmother: document.getElementById("Maincontent_DDL7").value,
            Stsex: document.getElementById("Maincontent_DDL8").value,
            Stdb: document.getElementById("Maincontent_TextBox7").value,
            Stmb: document.getElementById("Maincontent_TextBox8").value,
            Styb: document.getElementById("Maincontent_TextBox9").value,
            Stage: document.getElementById("Maincontent_TextBox10").value,
            Sttm: document.getElementById("Maincontent_HObsTime").value,
            Stactv: document.getElementById("Maincontent_CheckBox1").checked,
            Std11: document.getElementById("Maincontent_DDL11").value,
            Std12: document.getElementById("Maincontent_DDL12").value,
            Std13: document.getElementById("Maincontent_DDL13").value,
            Std14: document.getElementById("Maincontent_DDL14").value,
            Std15: document.getElementById("Maincontent_TextBox15").value,
            Std16: document.getElementById("Maincontent_DDL16").value,
            Std17: document.getElementById("Maincontent_DDL17").value,
            Std18: document.getElementById("Maincontent_DDL18").value,
            Std19: document.getElementById("Maincontent_DDL19").value,
            inPoslBroj: '0',
        };

        // Нулиране на празните стойности
        for (const key in myPerson) {
            if (myPerson.hasOwnProperty(key) && typeof myPerson[key] === "string") {
                if (myPerson[key].trim() === "") {
                    myPerson[key] = "0";
                }
            }
        }

        // Показване на бутона
        document.getElementById("ChkData").style.display = "block";

        if (Boolean(validatePers(myPerson)) && ErrorsCheck(myPerson)) {
            sessionStorage.setItem("myPerson", JSON.stringify(myPerson));

            // Задаване на стойност в зависимост от 'Stlice'
            const index = parseInt(myPerson.Stlice, 10);
            if (index >= 1 && index <= 19) {
                document.getElementById(`Maincontent_HPerson${index}`).value = JSON.stringify(myPerson);
            }

            // Добавяне на ред към таблицата
            var myTable = document.getElementById("Maincontent_tPersons");
            var rowsInTbl = myTable.rows.length;
            if (rowsInTbl < 10) {
                rowsInTbl = "0" + rowsInTbl;
            }

            // Проверка за добавяне или редактиране на ред
            if (document.getElementById("Maincontent_HRowN").value == 'Add') {
                if (rowsInTbl <= 20) {
                    var row = myTable.insertRow(rowsInTbl);
                    const button = document.createElement('button');
                    button.id = `Maincontent_btn${rowsInTbl}`;
                    button.innerText = `Лице ${rowsInTbl}`;
                    const cells = [
                        myPerson.Stlice, myPerson.Strel, myPerson.Sthusb, myPerson.Stfather, myPerson.Stmother,
                        myPerson.Stsex, myPerson.Stdb, myPerson.Stmb, myPerson.Styb, myPerson.Stage,
                        decode(myPerson.Stactv)
                    ];
                    for (let i = 0; i < cells.length; i++) {
                        var cell = row.insertCell(i);
                        cell.innerHTML = cells[i];
                    }
                    var lastCell = row.insertCell(cells.length);
                    lastCell.appendChild(button);
                } else {
                    alert("Лицата в домакинството не може да надвишават 19.");
                }
            } else {
                var rowIndex = document.getElementById("Maincontent_HRowN").value;
                const row = myTable.rows[rowIndex];
                const cells = [myPerson.Strel, myPerson.Sthusb, myPerson.Stfather, myPerson.Stmother,
                myPerson.Stsex, myPerson.Stdb, myPerson.Stmb, myPerson.Styb, myPerson.Stage,
                decode(myPerson.Stactv)];
                for (let i = 0; i < cells.length; i++) {
                    row.cells[i].innerHTML = cells[i];
                }
            }

            // Изчистване на полетата и промени в интерфейса
            ClearPerson("Rec");
            document.getElementById("tPerson").style.display = 'none';
            attachEvents();
            document.getElementById("AddPrs").style.display = 'block';
        } else {
            //alert ("Има непопълнено поле. Попълнете за да продължите...");
        }

        document.getElementById("Maincontent_hNPers").value = myTable.rows.length - 1;

    }

    var myAge = calculateNewDate();
    alert(myAge);
    document.getElementById("Maincontent_TextBox10").value = myAge;
}

/*Предназначението на функцията е да почисти въведените данни от потребителя, ако е допуснал грешки и желае да направи това*/
function ClearPerson(mComm) {
    document.getElementById("Maincontent_HRowN").value = 'N';

    var elementsToClear = [
        "Maincontent_DDL4", "Maincontent_DDL5", "Maincontent_DDL6", "Maincontent_DDL7",
        "Maincontent_DDL8", "Maincontent_TextBox7", "Maincontent_TextBox8", "Maincontent_TextBox9",
        "Maincontent_TextBox10", "Maincontent_DDL11", "Maincontent_DDL12", "Maincontent_DDL13",
        "Maincontent_DDL14", "Maincontent_TextBox15", "Maincontent_DDL16", "Maincontent_DDL17",
        "Maincontent_DDL18", "Maincontent_DDL19"
    ];

    for (var i = 0; i < elementsToClear.length; i++) {
        var element = document.getElementById(elementsToClear[i]);
        element.selectedIndex = 0; // За селекторите на избор
        element.value = ""; // За текстовите полета
    }

    document.getElementById("Maincontent_CheckBox1").checked = false;

    if (mComm !== "Add") {
        document.getElementById('Maincontent_PersonD2').value = "";
    }
}

/*function PersonClick(), function CalculateN(rowsInTbl) – Не се ползват*/
function PersonClick() {
    var tperson = document.getElementById("tPerson");
    tperson.style.display = "block";
}
/*Функцията CalculateN връща стойност, която представлява броя на редовете в таблицата, форматиран като двуцифрен номер с водеща нула, ако броят на редовете е по-малък от 10. */
function CalculateN(rowsInTbl) {
    var i = String(rowsInTbl);
    if (rowsInTbl < 10) {
         i = "0" + i;
    }
    return i;
}


function decode(mVal) {

    if (typeof mVal === undefined) {        
        return "Не";
    }
    else if (Boolean(mVal)) {       
        return "Да";
    }
    else {
        return "Не";
    }
}
/*Брои отметките*/
function CalcCurrentPersonsT() {
   
        document.getElementById("Maincontent_tPersons").rows.value;
        return visPrs;
}
/*Проверка дали са попълнени полетата за дадено лице*/
function validatePers(myPers) {

    var bP = true;
    if (myPers.Stlice.trim() == "" || myPers.Stlice == null) {
        $("#d2_error").show();
        bP = false;
    } else {
        $("#d2_error").hide();
    }
    if (myPers.Strel.trim() == "" || myPers.Strel == null) {
        $("#d4_error").show();
        bP = false;
    } else {
        $("#d4_error").hide();
    }

    if (myPers.Sthusb.trim() == "" || myPers.Sthusb == null) {
        $("#d5_error").show();
        bP = false;
    } else {
        $("#d5_error").hide();
    }

    if (myPers.Stfather.trim() == "" || myPers.Stfather == null) {
        $("#d6_error").show();
        bP = false;
    } else {
        $("#d6_error").hide();
    }
    if (myPers.Stmother.trim() == "" || myPers.Stmother == null) {
        $("#d7_error").show();
        bP = false;
    } else {
        $("#d7_error").hide();
    }
    if (myPers.Stage.trim() == "" || myPers.Stage == null) {
        $("#d8_error").show();
        bP = false;
    } else {
        $("#d8_error").hide();
    }

    if (myPers.Stdb.trim() == "" || myPers.Stdb == null) {
        $("#d9d_error").show();
        bP = false;
    } else {
        $("#d9d_error").hide();
    }

    if (myPers.Stmb.trim() == "" || myPers.Stmb == null) {
        $("#d9m_error").show();
        bP = false;
    } else {
        $("#d9m_error").hide();
    }

    if (myPers.Styb.trim() == "" || myPers.Styb == null) {
        $("#d9y_error").show();
        bP = false;
    } else {
        $("#d9y_error").hide();
    }

    if (myPers.Stage.trim() == "" || myPers.Stage == null) {
        $("#d10_error").show();
        bP = false;
    } else {
        $("#d10_error").hide();
    }

    if (myPers.Std11.trim() == "" || myPers.Std11 == null) {        
        $("#d11_error").show();
        bP = false;
    } else {
        $("#d11_error").hide();
    }


    if (myPers.Std12.trim() == "" || myPers.Std12 == null || myPers.Std12 == 0) {        
        $("#d12_error").show();
        bP = false;
    } else {
        $("#d12_error").hide();
    }

    if (document.getElementById("Maincontent_DDL13").disabled == false) {
        if (myPers.Std13.trim() == "" || myPers.Std13 == null || myPers.Std13 == "0") {

            $("#d13_error").show();
            bP = false;
        }
    } else {
        $("#d13_error").hide();
    }
    if (document.getElementById("Maincontent_DDL14").disabled == false) {
        if (myPers.Std14.trim() == "" || myPers.Std14 == null || myPers.Std14 == "0") {
            $("#d14_error").show();
            bP = false;
        } else {
            $("#d14_error").hide();
        }
    }

    if (document.getElementById("Maincontent_TextBox15").disabled == false) {

        if (myPers.Std15.trim() == "" || myPers.Std15 == null || myPers.Std15 == "0") {
            $("#d15_error").show();
            bP = false;
        }
    } else {
        $("#d15_error").hide();
    }
    if (document.getElementById("Maincontent_DDL16").disabled == false) {
        if (myPers.Std16.trim() == "" || myPers.Std16 == null || myPers.Std16 == "0") {
            $("#d16_error").show();
            bP = false;
        }
    } else {
        $("#d16_error").hide();
    }

    if (document.getElementById("Maincontent_DDL17").disabled == false) {
        if (myPers.Std17.trim() == "" || myPers.Std17 == null || myPers.Std17 == "0") {
            $("#d17_error").show();
            bP = false;
        }
    } else {
        $("#d17_error").hide();
    }

    if (document.getElementById("Maincontent_DDL18").disabled == false) {
        if (myPers.Std18.trim() == "" || myPers.Std18 == null || myPers.Std18 == "0") {
            $("#d18_error").show();
            bP = false;
        }
    } else {
        $("#d18_error").hide();
    }

    if (document.getElementById("Maincontent_DDL19").disabled == false) {
        if (myPers.Std19.trim() == "" || myPers.Std19 == null) {
            $("#d19_error").show();
            bP = false;
        } else {
            $("#d19_error").hide();
        }
    }
    return bP;
}
/*Проверка за Д19*/
function ErrorsCheck(myPers) {

    var bP = true;
    const mErrors = [];
    const sErrors = [];
    var cMTable = 0;
    var cSTable = 0;
    var age = parseInt(myPers.Stage);


    if ((age < 15 && myPers.Std13 == "1") || (age < 15 && myPers.Std13 == "2") || (age < 15 && myPers.Std13 == "5")) {
        cMTable = 1;
        mErrors.push({
            merror: 'Лице ' + myPers.Stlice + ' ... - възрастта трябва да е >=15 , Д13 (причината за преселване в България) не трябва да е с код 1, 2 и 5. Задължителна проверка.'
        });
    }

    if ((myPers.Std19 == "2" && age < 6) || (myPers.Std19 == "2" && age > 20)) {
        cSTable = 1;
        sErrors.push({ merror: 'Лицето е ученик (Д19=2), а възрастта му е под 6 г. или над 20 г. Така ли да остане? Сигнална проверка.' });
    }

    if ((myPers.Std19 == "3" && age < 15)) {
        cSTable = 1;
        sErrors.push({ merror: 'Лицето е заминало на работа в друго населено място (Д19=3), а възрастта му е под 15 години. Така ли да остане? Сигнална проверка.' });
    }


    if (cMTable == 1) {
        document.getElementById('MEtable-container').style.display = "block";
        const tableContainerM = document.getElementById('MEtable-container');
        tableContainerM.innerHTML = createTable(mErrors, 'Задължителни проверки');
        bP = false;
    } else {
        document.getElementById('MEtable-container').style.display = "none";

    }

    
    if (cSTable == 1) {
        document.getElementById('SEtable-container').style.display = "block";
        const tableContainerS = document.getElementById('SEtable-container');
        tableContainerS.innerHTML = createTable(sErrors, 'Сигнални проверки');
        bP = false;
        document.getElementById("Maincontent_d1").style.display = '';
    } else {
        document.getElementById('SEtable-container').style.display = "none";
        document.getElementById("Maincontent_d1").style.display = "none";
    }

    return bP;

}

/*Всяко лице е бутон и при натискане на бутона се появява таблицата за въпроси и отговори за конкретното лице*/
function attachEvents() {
    var p1 = document.getElementById("Maincontent_btn01");
    var p2 = document.getElementById("Maincontent_btn02");
    var p3 = document.getElementById("Maincontent_btn03");
    var p4 = document.getElementById("Maincontent_btn04");
    var p5 = document.getElementById("Maincontent_btn05");
    var p6 = document.getElementById("Maincontent_btn06");
    var p7 = document.getElementById("Maincontent_btn07");
    var p8 = document.getElementById("Maincontent_btn08");
    var p9 = document.getElementById("Maincontent_btn09");
    var p10 = document.getElementById("Maincontent_btn10");
    var p11 = document.getElementById("Maincontent_btn11");
    var p12 = document.getElementById("Maincontent_btn12");
    var p13 = document.getElementById("Maincontent_btn13");
    var p14 = document.getElementById("Maincontent_btn14");
    var p15 = document.getElementById("Maincontent_btn15");
    var p16 = document.getElementById("Maincontent_btn16");
    var p17 = document.getElementById("Maincontent_btn17");
    var p18 = document.getElementById("Maincontent_btn18");
    var p19 = document.getElementById("Maincontent_btn19");

    if (p1) {
        p1.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p1);
    })
    }
    if (p2) {
        p2.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p2);
    })
    }
    if (p3) {
        p3.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p3);
    })
    }
    if (p4) {
        p4.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p4);
    })
    }
    if (p5) {
        p5.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p5);
    })
    }
    if (p6) {
        p6.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p6);
    })
    }
    if (p7) {
        p7.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p7);
    })
    }
    if (p8) {
        p8.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p8);
    })
    }
    if (p9) {
        p9.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p9);
    })
    }
    if (p10) {
        p10.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p10);
    })
    }
    if (p11) {
        p11.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p11);
    })
    }
    if (p12) {
        p12.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p12);
    })
    }
    if (p13) {
        p13.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p13);
    })
    }
    if (p14) {
        p14.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p14);
    })
    }
    if (p15) {
        p15.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p15);
    })
    }
    if (p16) {
        p16.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p16);
    })
    }
    if (p17) {
        p17.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p17);
    })
    }
    if (p18) {
        p18.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p18);
    })
    }
    if (p19) {
        p19.addEventListener("click", (e) => {
            e.preventDefault();
            EditPerson(p19);
        })
    }
}
/*Редакция на данни за избраното лице*/
function EditPerson(mElem) {

    document.getElementById("AddPrs").style.display = "none";
    var tperson = document.getElementById("tPerson");
    var but = document.getElementById("ChkData");
    tperson.style.display = "block";
    but.style.display = "none";
    const mBH = new Map();
    mBH.set('Maincontent_btn01', 'Maincontent_HPerson1');
    mBH.set('Maincontent_btn02', 'Maincontent_HPerson2');
    mBH.set('Maincontent_btn03', 'Maincontent_HPerson3');
    mBH.set('Maincontent_btn04', 'Maincontent_HPerson4');
    mBH.set('Maincontent_btn05', 'Maincontent_HPerson5');
    mBH.set('Maincontent_btn06', 'Maincontent_HPerson6');
    mBH.set('Maincontent_btn07', 'Maincontent_HPerson7');
    mBH.set('Maincontent_btn08', 'Maincontent_HPerson8');
    mBH.set('Maincontent_btn09', 'Maincontent_HPerson9');
    mBH.set('Maincontent_btn10', 'Maincontent_HPerson10');
    mBH.set('Maincontent_btn11', 'Maincontent_HPerson11');
    mBH.set('Maincontent_btn12', 'Maincontent_HPerson12');
    mBH.set('Maincontent_btn13', 'Maincontent_HPerson13');
    mBH.set('Maincontent_btn14', 'Maincontent_HPerson14');
    mBH.set('Maincontent_btn15', 'Maincontent_HPerson15');
    mBH.set('Maincontent_btn16', 'Maincontent_HPerson16');
    mBH.set('Maincontent_btn17', 'Maincontent_HPerson17');
    mBH.set('Maincontent_btn18', 'Maincontent_HPerson18');
    mBH.set('Maincontent_btn19', 'Maincontent_HPerson19');

    const myPerson = JSON.parse(document.getElementById(mBH.get(mElem.id)).value);    
    
    document.getElementById("Maincontent_PersonD2").value = myPerson.Stlice;
    document.getElementById("Maincontent_DDL4").value = myPerson.Strel;
    document.getElementById("Maincontent_DDL5").value = myPerson.Sthusb;
    document.getElementById("Maincontent_DDL6").value = myPerson.Stfather;
    document.getElementById("Maincontent_DDL7").value = myPerson.Stmother;
    document.getElementById("Maincontent_DDL8").value = myPerson.Stsex;
    document.getElementById("Maincontent_TextBox7").value = myPerson.Stdb;
    document.getElementById("Maincontent_TextBox8").value = myPerson.Stmb;
    document.getElementById("Maincontent_TextBox9").value = myPerson.Styb;
    document.getElementById("Maincontent_TextBox10").value = myPerson.Stage;
    assignVal("Maincontent_DDL11", myPerson.Std11);
    if ((myPerson.Std12 == '0') || (myPerson.Std12.trim().length == '')) {
        document.getElementById("Maincontent_DDL12").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL12").disabled = false;
       ct2('val', myPerson.Std12);*/
        let d12 = myPerson.Std12;
        assignVal("Maincontent_DDL12", d12);
        
        alert(myPerson.Std12);
    }
    assignVal("Maincontent_DDL13", myPerson.Std13);
    if ((myPerson.Std13 == '0') || (myPerson.Std13.trim().length === 0)) {
        document.getElementById("Maincontent_DDL13").disabled = true;
        //document.getElementById("Maincontent_DDL13").value = "";
    } else {
        document.getElementById("Maincontent_DDL13").disabled = false;
        assignVal("Maincontent_DDL13", myPerson.Std13);
    }
    if ((myPerson.Std14 == '0') || (myPerson.Std14.trim().length == '')) {
        document.getElementById("Maincontent_DDL14").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL14").disabled = false;
        assignVal("Maincontent_DDL14", myPerson.Std14);
    }

    assignVal("Maincontent_DDL14", myPerson.Std14);

    if ((myPerson.Std15 == '0') || (myPerson.Std15.trim().length === 0)) {
        document.getElementById("Maincontent_TextBox15").disabled = true;
    } else {
        document.getElementById("Maincontent_TextBox15").disabled = false;
        document.getElementById("Maincontent_TextBox15").value = myPerson.Std15;        
    }

    if ((myPerson.Std16 == '0') || (myPerson.Std16.trim().length == '')) {
        document.getElementById("Maincontent_DDL16").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL16").disabled = false;
        assignVal("Maincontent_DDL16", myPerson.Std16);
    }

    if ((myPerson.Std17 == '0') || (myPerson.Std17.trim().length == '')) {
        document.getElementById("Maincontent_DDL17").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL17").disabled = false;
        assignVal("Maincontent_DDL17", myPerson.Std17);
    }

    if ((myPerson.Std18 == '0') || (myPerson.Std18.trim().length == '')) {
        document.getElementById("Maincontent_DDL18").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL18").disabled = false;
        assignVal("Maincontent_DDL18", myPerson.Std18);
    }

    if ((myPerson.Std19 == '0') || (myPerson.Std19.trim().length == '')) {
        document.getElementById("Maincontent_DDL19").disabled = true;
    } else {
        document.getElementById("Maincontent_DDL19").disabled = false;
        assignVal("Maincontent_DDL19", myPerson.Std19);
    }

    //if (myPerson.Stactv = 'True') { 
    if (decode(myPerson.Stactv) === "Да") {       
        document.getElementById("Maincontent_CheckBox1").checked = true;
    } else {
        document.getElementById("Maincontent_CheckBox1").checked = false;
    }

    document.getElementById("Maincontent_HRowN").value = afterCh(mBH.get(mElem.id), 'son');

    document.getElementById("Maincontent_hNPers").value = document.getElementById("Maincontent_tPersons").rows.length - 1;

}
/*Редакция на данни за избраното лице*/
function assignVal(mElName, mVal) {
    var opt = document.getElementById(mElName);
    for (i = 0; i < document.getElementById(mElName).options.length; i++) {
        var mT = opt.options[i].text;
        if (mT.includes(mVal)) {
            opt.selectedIndex = i;
            return;
        } else {
            opt.selectedIndex = 0;
        }
    }
}

/*Логически обвръзки в картата на домакинството след като потребителя е въвел всички лица в домакинството*/
function CheckData() {
    var bP = true;
   const mErrors = [];
    const sErrors = [];
    let a = [];
    var tPersons = document.getElementById("Maincontent_tPersons");
    for (var i = 0, row; row = tPersons.rows[i]; i++) {
        a[i] = [];
        for (var j = 0, col; col = row.cells[j]; j++) {
            a[i][j] = col.innerText;             
        }
    }

    var cMTable = 0;
    var cSTable = 0;
    var dage = 0;
    var parentage = 0;

    //var hsb = '00';    

    if (a.length > 0) {
        var rws = a.length - 1;
        var c1 = 0;
        var count = 0;


        for (var i = 1; i < a.length; i++) {
            if (a[i][1] == '01') {
                c1 = c1 + 1;
            }
            if (a[i][1] == '02') {
                count = count + 1;
            }

            for (var j = 0; j < a[i].length; j++) {
                var spouse = 1;
                var father = 1;
                var mother = 1;
                var d2 = a[i][0];
                d2 = d2.substring(d2.length - 2);
               
                if (a[i][1] == '01') {
                    dage = a[i][9];
                    dage = parseInt(dage);
                }
                if (a[i][1] == '07') {
                    parentage = a[i][9];
                    parentage = parseInt(parentage);
                }

                //if (a[i][2] != '99') {
                //    hsb = a[i][2];
                //}

                if (a[i][3] != '99') {
                    fth = a[i][3];
                }

                if (a[i][4] != '99') {
                    mth = a[i][4];
                }

            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][2] != '99') {
                    var r = removeZero(a[i][2]);
                    r = parseInt(r);
                    if (r > rws) {
                        spouse = 0;
                        cMTable = 1;
                        mErrors.push({ merror: 'За лицето ' + i + ' поредния номер на съпруга/съпруг трябва да е пореден номер на лице (Д2), записано в списъка на домакинството или да има стойност 99. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99') {
                    var r = removeZero(a[i][3]);
                    r = parseInt(r);
                    if (r > rws) {
                        father = 0;
                        cMTable = 1;
                        mErrors.push({ merror: 'За лицето ' + i + ' поредния номер на бащата трябва да е пореден номер на лице, записано в списъка на домакинството или да има стойност 99. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99') {
                    var r = removeZero(a[i][4]);
                    r = parseInt(r);
                    if (r > rws) {
                        mother = 0;
                        cMTable = 1;
                        mErrors.push({ merror: 'За лицето ' + i + ' поредния номер на майката трябва да е пореден номер на лице, записано в списъка на домакинството или да има стойност 99. Задължителна проверка' });
                        break;
                    }
                }
            }

        }
            if (c1 == 0) {
                cMTable = 1;
                mErrors.push({ merror: 'Липсва глава на домакинството. Задължителна проверка' });
            }

            if (c1 >= 2) {
                cMTable = 1;
                mErrors.push({ merror: 'Само едно лице трябва да е глава на домакинството. Задължителна проверка' });

            }
     
            if (count >= 2) {
                cMTable = 1;
                mErrors.push({ merror: 'Само едно лице трябва да е съпруг/а, партньор на главата на домакинството. Задължителна проверка' });
            }


        for (var i = 1; i < a.length; i++) {


            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '01' && a[i][9] < 15) {
                        cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + '  е глава на домакинството, възрастта трябва да е >= 15 .Задължителна проверка' });
                        break;
                }
            }
            
            for (var j = 0; j < a[i].length; j++) {                
                      /*if ((father == 1) && (a[i][3] == a[i][2] && a[i][3] == a[i][4]))*/
                if (a[i][3] != '99') {                   
                    if (a[i][3] == a[i][2] || a[i][3] == a[i][4]) {
                        cMTable = 1;
                        mErrors.push({ merror: 'За лицето ' + i + ' номерът на бащата трябва да е различен от номера на съпруга/съпругата и от номера на майката .Задължителна проверка' });
                        break;
                    }
                }
            }
            /*j - колона, i - ред*/
            var selfspouse = 0;  
            for (var j = 0; j < a[i].length; j++) {
                var d2 = a[i][0];
                d2 = d2.substring(d2.length - 2);
                var d5 = a[i][2];
                   if (d2 == d5) {                    
                    selfspouse = 1;
                    cMTable = 1;
                    mErrors.push({ merror: 'За лицето ' + i + ' номерa на съпруга трябва да е различен от номера на лицето, т.е. не може да е съпруг/а на себе си. Задължителна проверка' });
                    break;
                }
            }
        
            /*spouse, selfspouse - съпруг, съпруг на себе си*/
            for (var j = 0; j < a[i].length; j++) {
                if (a[i][2] != '99' && spouse == 1 && selfspouse == 0) {
                    var s = removeZero(a[i][2]);
                    s = parseInt(s);
                    if (a[i][5] == a[s][5]) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' полът на съпрузите е еднакъв.Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (father == 1 && d2 == a[i][3]) {
                    cMTable = 1;
                    mErrors.push({
                        merror: 'Лице ' + i + ' - номерът на бащата не може да съвпада с номера на лицето. Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (mother == 1 && d2 == a[i][3]) {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + ' номерът на майката не може да съвпада с номера на лицето.Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if ((a[i][1] == '02' && a[i][9] < 15) || (a[i][2] != '99' && a[i][9] < 15)) {
                    cSTable = 1;
                    sErrors.push({
                        merror: 'Лице ' + i + ' е под 15 год., но има съпруг/а. Taka ли да остане?.Сигнална проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                var sage = parseInt(a[i][9]);
                if (sage < 12 && a[i][2] != '99') {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + ' възрастта на лицето е по-малка от 12 години, поредният номер на съпруг/а трябва да е 99. Задължителна проверка' });
                        break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                var sage = parseInt(a[i][9]);
                if (sage < 15 && a[i][2] != '99') {
                    cSTable = 1;
                    sErrors.push({ merror: 'Ако лицето ' + i + ' е на възраст под 15 години, поредният номер на съпруг/съпругат трябва да е 99. Така ли да остане? Сигнална проверка.' });
                    break;
                }
            }


            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '02' && a[i][9] < 15) {
                    cSTable = 1;
                    sErrors.push({ merror: 'За лицето ' + i + ' възрастта трябва да е по-голяма или равно на 15.Сигнална проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {                
                if (a[i][1] == '02' && a[i][9] < 13) {                    
                    cMTable = 1;
                    mErrors.push({ merror: 'За лицето ' + i + ' възрастта трябва да е по-голяма или равно на 13.Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '05' && a[i][9] >= 60) {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + ' е внук/внучка на главата на домакинството, възрастта трябва да е <60.Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '06' && a[i][9] <= 30) {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + '  е родител на съпруг/а на главата на домакинството, възрастта трябва да е >=30.Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '07' && a[i][9] <= 30) {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + '  е родител на съпруг/а на главата на домакинството, възрастта трябва да е >=30.Задължителна проверка' });
                    break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '08' && a[i][9] <= 45) {
                    cMTable = 1;
                    mErrors.push({ merror: 'Лице ' + i + ' е баба/дядо на главата на домакинството, възрастта трябва да е >=45.Задължителна проверка' });
                    break;
                }
            }


            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99' && father == 1) {
                    var f = removeZero(a[i][3]);
                    f = parseInt(f);
                    if (a[f][5] != '1') {
                        cSTable = 1;
                        sErrors.push({ merror: 'За лицето ' + i + ' бащата не е мъж (Д8 = 1). Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && mother == 1) {
                    var m = removeZero(a[i][4]);
                    m = parseInt(m);
                    if (a[m][5] != '2') {
                        cSTable = 1;
                        sErrors.push({ merror: 'За лицето ' + i + ' майката не е жена (Д8 = 2). Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99' && father == 1) {
                    var f = removeZero(a[i][3]);
                    f = parseInt(f);
                    fage = a[f][9];
                    if (fage < 13) {
                        cMTable = 1;
                        mErrors.push({ merror: 'Лицето ' + f + ' е баща и трябва да е на 13 и повече навършени години. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99' && father == 1) {
                    var f = removeZero(a[i][3]);
                    f = parseInt(f);
                    fage = a[f][9];
                    if (fage < 15) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лицето ' + f + ' е баща и трябва да е на 15 навършени години. Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99' && father == 1) {
                    var page = removeZero(a[i][9]);
                    var f = removeZero(a[i][3]);
                    f = parseInt(f);
                    fage = a[f][9];
                    if (fage - page < 13) {
                        cMTable = 1;
                        mErrors.push({ merror: 'Лице ' + i + '- разликата във възрастта с бащата трябва да е >= 13 години. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][3] != '99' && father == 1) {
                    var page = removeZero(a[i][9]);
                    var f = removeZero(a[i][3]);
                    f = parseInt(f);
                    fage = a[f][9];
                    if (fage - page < 15) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' бащата e по-възрастен с по-малко от 15 години. Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }
            

            for (var j = 0; j < a[i].length; j++) {               
                if (a[i][3] != '99' && father == 1) {                    
                    var pNumb = persNumb();
                    var pNumb1 = persNumbD1a();                    
                    if (father == 1 && d2 == a[i][3]) {                        
                        cMTable = 1;
                        mErrors.push({ merror: 'За лицето ' + i + ' номерът на бащата трябва да е различен от номера на лицето (Д6<>Д2). Задължителна проверка' });
                        break;
                    }
                }
            }

            
            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && mother == 1) {
                    var m = removeZero(a[i][4]);
                    m = parseInt(m);
                    mage = a[m][9];
                    if (mother == 1 && mage < 12) {
                        cMTable = 1;
                        mErrors.push({ merror: 'Лицето ' + m + ' е майка и трябва да е на 12 повече години. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && mother == 1) {
                    var m = removeZero(a[i][4]);
                    m = parseInt(m);
                    mage = parseInt(a[m][9]);
                    if (mage < 15) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лицето ' + m + ' е майка и трябва да е на 15 и повече години. Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && mother == 1) {
                    var page = removeZero(a[i][9]);
                    var m = removeZero(a[i][4]);
                    m = parseInt(m);
                    mage = a[m][9];
                    if (mage - page < 12) {
                        cMTable = 1;
                        mErrors.push({ merror: 'Лице ' + i + ' - разликата във възрастта с майката трябва да е >= 12 години. Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && mother == 1) {
                    var page = removeZero(a[i][9]);
                    var m = removeZero(a[i][4]);
                    m = parseInt(m);
                    mage = a[m][9];
                    if (mage - page < 15) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' - майката e по-възрастна от лицето с по-малко от 15 години. Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {               
                if (a[i][1] == '03') {                  
                    var bashta = (a[i][3]);
                    var maika = (a[i][4]);
                    //m = parseInt(m);
                    if (bashta === '99' || maika === '99') {                        
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' е син/дъщеря главата на домакинството, поредният номер на бащата или на майката трябва да е различен от 99. Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][4] != '99' && a[i][4] == a[i][2] && a[i][4] == a[i][3]) {
                        mETable = 1;
                        mErrors.push({ merror: 'Номерът на майката трябва да е различен от номера на съпруга и номера на бащата (Д7<>Д5; Д7<>Д6). Задължителна проверка' });
                        break;
                    }
            }

            for (var j = 0; j < a[i].length; j++) {
                //if (a[i][4] != '99' && a[i][4] == a[i][2] && a[i][4] == a[i][3]) {
                if (a[i][4] != '99' && (a[i][4] == a[i][2] || a[i][4] == a[i][3])) { 
                        cMTable = 1;
                        mErrors.push({ merror: 'Лице ' + i + ' номерът на майката трябва да е различен от номера на съпруга и номера на бащата (Д7<>Д5;Д7<>Д6). Задължителна проверка' });
                        break;
                }
            }

            var c1 = 0;

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '03') {
                    var sage = removeZero(a[i][9]);
                    sage = parseInt(sage);
                    if (dage - sage < 12)
                    {
                        cMTable = 1;
                        mErrors.push({ merror: 'Лице ' + i + ' възрастта на главата на домакинството (код 01) да е с 12 години по-голяма от възрастта на син, дъщеря (код 03). Задължителна проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '03') {
                    var sage = removeZero(a[i][9]);
                    sage = parseInt(sage);
                    if (dage - sage < 15) {
                        sMTable = 1;
                        sErrors.push({ merror: 'Ako лицето ' + i + ' възрастта на главата на домакинството (код 01) да е с 15 години по-голяма от възрастта на син, дъщеря (код 03). Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '06') {
                    var page = removeZero(a[i][9]);
                    page = parseInt(page);
                    if (page - dage < 15 && dage != 0) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' възрастта на главата на домакинството (код 01) да е с 15 години по-малка от възрастта на родителя му (код 06). Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '02') {
                    var sage = removeZero(a[i][9]);
                    sage = parseInt(sage);
                    if (parentage - sage < 15 && parentage != 0) {
                        cSTable = 1;
                        sErrors.push({ merror: 'Лице ' + i + ' възрастта на съпруга/съпругата на главата на домакинството (код 02) да е с 15 години по-малка от възрастта на на родителя му/и код (07). Така ли да остане? Сигнална проверка' });
                        break;
                    }
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][1] == '02' && a[i][2] == '99') {
                    cSTable = 1;
                    sErrors.push({ merror: 'Ако отношението към главата на домакинството е с код 02, поредния номер на съпруг/съпруга трябва да е различен от 99 (Ako Д4=02, Д5<>99). Сигнална проверка' });
                        break;
                }
            }

            for (var j = 0; j < a[i].length; j++) {
                if (a[i][2] != '99' && spouse == 1) {
                    var s = removeZero(a[i][2]);
                    s = parseInt(s);
                    if (a[s][2] == '99') {
                        cSTable = 1;
                        sErrors.push({merror: 'Лице ' + i +  ' - съпругът/ата е с несъществуващ номер.Така ли да остане? Сигнална проверка. ' });
                        break;
                    }
                }
            }
            var pNumb = persNumb();
            var pNumb1 = persNumbD1a();  
        }
               
        /*Таблиците със сигнални проверки - не се появява чекбокса*/
        if (cMTable == 1) {
            document.getElementById('MEtable-container').style.display = "block";
            const tableContainerM = document.getElementById('MEtable-container');
            tableContainerM.innerHTML = createTable(mErrors, 'Задължителни проверки');
            bP = false;            
        } else {
            document.getElementById('MEtable-container').style.display = "none";

        }

        if (cSTable == 1) {
            document.getElementById('SEtable-container').style.display = "block";
            const tableContainerS = document.getElementById('SEtable-container');            
            tableContainerS.innerHTML = createTable(sErrors, 'Сигнални проверки');            
            document.getElementById("Maincontent_d1").style.display = '';  
            //bP = false;            
        } else {
            document.getElementById('SEtable-container').style.display = "none";
            document.getElementById("Maincontent_d1").style.display = "none";
            bP = true;
        }

        if (mErrors.length > 0) {
            document.getElementById('Maincontent_HCheckDataPass').value = 1;
        } else {
            document.getElementById('Maincontent_HCheckDataPass').value = 0;
        }
        
    }
    
    if ((bP == true) || (document.getElementById("Maincontent_SignalChk").checked = true) ) {        
        document.getElementById('Maincontent_NextPage').style.display = "block";
    }
}
/*Създава таблиците със задължителни и сигнални проверки, като визуализира в тях съответните грешки, ако има такива*/
function createTable(data, errType) {
    let errtable = '';
    if (errType == 'Задължителни проверки') {
        errtable = `<table id ='MErrors'>`;
    } else {
        errtable = `<table id ='SErrors'>`;
    }
    errtable += `<tr><th>${errType}</th></tr>`;
    errtable += '<tr><tbody></tbody></tr>';
    data.forEach(item => {
        errtable += `<tr><td>${item.merror}</td></tr>`;
    });
    errtable += '</table>';
    return errtable;
}
/*Изчислява броя на редовете в таблицата за домакинството*/
function tblRows() {
        var vRows = 0;
        const myRows = ["Maincontent_btn01", "Maincontent_btn02", "Maincontent_btn03", "Maincontent_btn04", "Maincontent_btn05", "Maincontent_btn06", "Maincontent_btn07", "Maincontent_btn08", "Maincontent_btn09", "Maincontent_btn10", "Maincontent_btn11", "Maincontent_btn12", "Maincontent_btn13", "Maincontent_btn14", "Maincontent_btn15", "Maincontent_btn16", "Maincontent_btn17", "Maincontent_btn18", "Maincontent_btn19"];
        for (var i = 0; i < myRows.length; i++) {
            var myR = document.getElementById(myRows[i]);
            if (myR !== null) {
                vRows++;
            }
        }
        return vRows;
    }
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

        return pNum;
}

// Функцията, брои колко пъти среща в таблица с идентификатор "Maincontent_tPersons" 
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
/*Скрива формата за попълване – не се ползва*/
function CancelPerson() {
        document.getElementById("tPerson").style.display = 'none';
}
/*Маха нула от стринга в началото*/
function removeZero(mVal) {
    var num;
    if (mVal.search("0") == 0) { 
        num = mVal.substring(1);
        return num;
    }
    return mVal;
}


