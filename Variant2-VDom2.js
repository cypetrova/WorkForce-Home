function persNumb() {
    var fieldValue = document.getElementById('Maincontent_HsHldNum').value.trim(); // Вземете стойността от полето за въвеждане и я изчистете от начални и крайни интервали
    var pNum = 0;

    // Проверка дали стойността на полето е по-голяма от 0 и по-малка или равна на 19
    if (parseInt(fieldValue) > 0 && parseInt(fieldValue) <= 19) {
        pNum = parseInt(fieldValue);
    } else {
        alert("Въведената стойност надвишава допустимата (максимален брой 19) , моля коригирайте!");
    }

    // Задайте стойността на същото поле равна на стойността на брояча pNum
    document.getElementById("Maincontent_HsHldNum").value = pNum;

    return pNum;
}

function persNumbD1a() {
    var fieldValue = document.getElementById('Maincontent_NmbPrs').value.trim(); // Вземете стойността от полето за въвеждане и я изчистете от начални и крайни интервали
    var pNum1 = 0;

    // Проверка дали стойността на полето е по-голяма от 0 и по-малка или равна на 19
    if (parseInt(fieldValue) > 0 && parseInt(fieldValue) <= 19) {
        // Проверка дали стойността е по-голяма от стойността на брояча pNum
        if (parseInt(fieldValue) > persNumb()) {
            pNum1 = parseInt(fieldValue);
        } else {
            alert("Стойността на полето трябва да бъде по-голяма от броя на членовете, за които вече имате информация.");
        }
    } else {
        alert("Стойността на полето трябва да бъде по-голяма от 0 и по-малка или равна на 19.");
    }

    // Задайте стойността на елемента с идентификатор "Maincontent_NmbPrs" равна на стойността на брояча pNum1
    document.getElementById("Maincontent_NmbPrs").value = pNum1;

    return pNum1;
}

function lica_Check() {
    var pNum = persNumb(); // Извикваме функцията persNumb() и вземаме стойността на променливата pNum
    var pNum1 = persNumbD1a(); // Извикваме функцията persNumbD1a() и вземаме стойността на променливата pNum1

    // Проверка дали броят на лицата, за които няма информация, е по-голям от броя на членовете в домакинството
    if (pNum1 > pNum) {
        $("#Maincontent_LMessage").show();
        alert("Броят на членовете, за които не може да бъде предоставена информация, е по-голям от броя на членовете в домакинството, моля коригирайте!");
    } 
    // Проверка дали броят на лицата, за които няма информация, е равен на броя на членовете в домакинството
    else if (pNum1 === pNum) {
        $("#Maincontent_LMessage").show();
        alert("Броят на членовете, за които не може да бъде предоставена информация, е равен на броя на членовете в домакинството, моля коригирайте!");
    } 
    // Показване на съобщението, ако броят на лицата, за които няма информация, е по-малък от броя на членовете в домакинството
    else {
        $("#Maincontent_LMessage").hide();
        var difference = pNum - pNum1;
        return difference;
    }
}

function AddPerson() {
    ClearPerson("Add");
    var tperson = document.getElementById("tPerson");
    var but = document.getElementById("ChkData");
    var nPersons = document.getElementById("Maincontent_tPersons").rows.length - 1;

    alert("AddPerson");
	
	var difference = lica_Check();

    if (difference <= 0) {
        return; // Прекратяваме изпълнението на функцията, ако няма лица, които да бъдат добавени
    }

    var difference = lica_Check(); // Извикваме функцията за проверка на разликата в броя на лицата
    if (difference !== undefined) {
        if (difference > nPersons) {
            alert("Броят на лицата е над " + difference + ". Ако желаете да продължите, моля коригирайте общия брой на лицата в домакинството!");
            document.getElementById("tPerson").style.display = "none";
            return; // Прекратяваме изпълнението на функцията, тъй като броят на лицата е над разликата
        } else if (difference < nPersons) {
            alert("Имайте предвид, че има още лица за добавяне.");
        }
    }

    tperson.style.display = "block";
    but.style.display = "none";

    document.getElementById("Maincontent_hNPers").value = document.getElementById("Maincontent_tPersons").rows.length - 1;
    document.getElementById("Maincontent_HRowN").value = "Add";

    if ((nPersons == 0) || (nPersons < 9)) {
        nPersons = nPersons + 1;
        document.getElementById("Maincontent_PersonD2").value = "0" + String(nPersons);
    } else if (nPersons <= difference) { // Променено условие за сравнение с разликата
        document.getElementById("Maincontent_PersonD2").value = nPersons + 1;
    }
}

