function persNumb() {
    var fieldValue = document.getElementById('Maincontent_HsHldNum').value.trim(); // Вземете стойността от полето за въвеждане и я изчистете от начални и крайни интервали
    var pNum = 0;

    // Проверка дали променливата nPersons има стойност и дали тя е по-голяма от 0
    if (typeof nPersons !== 'undefined' && nPersons > 0) {
        // Проверка дали стойността на полето е по-голяма от 0 и по-малка или равна на 19
        if (parseInt(fieldValue) > 0 && parseInt(fieldValue) <= 19) {
            pNum = parseInt(fieldValue);
        } else {
            alert("Въведената стойност надвишава допустимата (максимален брой 19) , моля коригирайте!");
            // Ако въведената стойност не отговаря на критериите, задайте стойността на pNum равна на nPersons
            pNum = nPersons;
        }
    } else {
        // Ако променливата nPersons няма стойност или е по-малка или равна на 0, задайте стойността на pNum на 0
        pNum = 0;
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
        var existingPersons = persNumb(); // Вземете броя на вече въведените членове

        // Проверка дали стойността е по-голяма от стойността на брояча pNum
        if (parseInt(fieldValue) > existingPersons) {
            pNum1 = parseInt(fieldValue);
        } else if (parseInt(fieldValue) < existingPersons) {
            // Ако въведената стойност е по-малка от вече съществуващия брой членове, изведете съобщение за грешка
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
    var pNum = persNumb();
    var pNum1 = persNumbD1a();
    var difference = pNum - pNum1;

    if (difference <= 0) {
        alert("Грешка: Не могат да бъдат добавени повече лица. Ако желаете да продължите, моля, коригирайте 'Брой на лицата в домакинството'");
        return; // Прекратяваме изпълнението на функцията, ако няма лица за добавяне
    } else if (difference > 0) {
        if (difference <= nPersons) {
            alert("Грешка: Броят на лицата, за които няма информация, е по-голям или равен на общия брой лица.");
            return; // Прекратяваме изпълнението на функцията, ако броят на лицата е по-голям или равен на общия брой лица
        } else {
            return difference; // Връщаме разликата, за да я използва AddPerson функцията
        }
    }
}

function AddPerson() {
    ClearPerson("Add");
    var tperson = document.getElementById("tPerson");
    var but = document.getElementById("ChkData");
    var nPersons = document.getElementById("Maincontent_tPersons").rows.length - 1;

    var difference = lica_Check();

    if (difference === undefined) {
        return; // Прекратяваме изпълнението на функцията, ако difference е неопределено
    }

    if (difference > 0) {
        if (difference <= nPersons) {
            alert("Броят на лицата е над " + difference + ". Ако желаете да продължите, моля коригирайте общия брой на лицата в домакинството!");
            document.getElementById("tPerson").style.display = "none";
            return; // Прекратяваме изпълнението на функцията, тъй като броят на лицата е над разликата
        } else if (difference < nPersons) {
            alert("Имайте предвид, че има още лица за добавяне.");
            errMsg("Имайте предвид, че има още лица за добавяне.");
        }
    }

    tperson.style.display = "block";
    but.style.display = "none";

    document.getElementById("Maincontent_hNPers").value = nPersons;
    document.getElementById("Maincontent_HRowN").value = "Add";

    if ((nPersons == 0) || (nPersons < 9)) {
        nPersons = nPersons + 1;
        document.getElementById("Maincontent_PersonD2").value = "0" + String(nPersons);
    } else if (nPersons <= difference) {
        document.getElementById("Maincontent_PersonD2").value = nPersons + 1;
    }
}