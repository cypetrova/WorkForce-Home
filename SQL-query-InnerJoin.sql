--взима данни, който се различават от данните в другата таблица 
SELECT dom.ppd, dom.ppm, dom.ppg, lica.db, lica.mb, lica.yb
FROM dom
INNER JOIN lica ON dom.tm = lica.tm AND dom.oblast = lica.oblast AND dom.grs = lica.grs;


 


