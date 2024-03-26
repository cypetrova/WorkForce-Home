/*Изчислява броя на лицата в домакинството, които са в момента на наблюдението*/
function persNumb() {
    // взимане на стойността на елемента и премахване на празни места около нея
    var HsHldPrs = document.getElementById("Maincontent_HsHldPrs").value.trim();


    // конвертиране на стойността в число
    HsHldPrs = parseInt(HsHldPrs);

    // проверка дали HsHldPrs е между 1 и 19
    if (HsHldPrs < 1 || HsHldPrs > 19) {
        alert("Грешка: Общият брой на лицата е недопустим, моля коригирайте! Допустима бройка - от 1 до 19 вкл.");
    } else {
        // продължава напред
        return HsHldPrs;
    }
    return HsHldPrs;
    alert("HsHldPrs");
}

function persNumbD1a() {

    var NumbPrs = document.getElementById("Maincontent_NumbPrs").value.trim();

    // Проверка дали NumbPrs е валидно число
    if (isNaN(NumbPrs)) {
        alert("Грешка: Въведете валидно число.");
        return;
    }

    NumbPrs = parseInt(NumbPrs);

    // Проверка дали NumbPrs е между 0 и 20
    if (NumbPrs < 0 || NumbPrs >= 20) {
        alert("Грешка: Броят на лицата, които липсват в домакинството е недопустим, моля коригирайте! Допустима бройка - от 0 до 19 вкл.");
        return;
    }
    else {
        alert("Въведеното число е валидно");
    }
    return NumbPrs;
    alert(NumbPrs);
}

/*Проверка на броя на лицата.Броят на лицата, за които не може да бъде предоставена информация е равен или по малък от общата бройка*/
function lica_Check() {
    /* Трябва да се появят толкова редове в таблицата tPersons,
     колкото е разликата от предходните две функции 
     Ако разликата е положителна (т.е. difference = HsHldPrs - NumbPrs >= 0 */
    var HsHldPrs = persNumb();
    var NumbPrs = persNumbD1a();

    var difference = HsHldPrs - NumbPrs;

    if (difference >= 0) {
        // Извикване на функция за добавяне на лице
        document.getElementById("Maincontent_AddPrs").click();
        AddPerson();

        // Запълване на таблица tPerson и запис в БД
        document.getElementById("Maincontent_bRecData").click();
        RecPerson(); // Предполагам, че това е функция, която записва данните в базата данни

        document.getElementById("Maincontent_ChkData").click();
        CheckData();

        // Скриване на излишните редове в таблицата tPersons
        var tableRows = document.getElementById("tPersons").getElementsByTagName("tr");
        for (var i = difference + 1; i < tableRows.length; i++) {
            tableRows[i].style.display = "none";
        }
    } else {
        alert("Грешка: Разликата е отрицателно число, моля коригирайте общия брой на лицата в домакинството или на липсващите лица!");
    }
    document.getElementById("Maincontent_ChkData").click();
    CheckData();
}



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