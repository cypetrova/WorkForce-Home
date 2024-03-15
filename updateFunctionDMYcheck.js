function DMYcheck(sDay, sMonth, sYear) {
    
    let currentYear = new Date().getFullYear();

    if (sDay === "00" || sDay === "99" || sMonth === "00" || sMonth === "99") {
        return 1;
    }

    let day = parseInt(sDay, 10);
    let month = parseInt(sMonth, 10);
    let year = parseInt(sYear, 10);

    if (month < 1 || month > 12) {
        return 0; 
    }

    if (year < 1900 || year > currentYear) {
        return 0;
    }

    let maxDaysInMonth = new Date(year, month, 0).getDate();
    if (day < 1 || day > maxDaysInMonth) {
        return 0;
    }

    return 1; 
}