UPDATE lica
SET age = CASE 
                WHEN lica.mb = '00' AND lica.db = '00' THEN 
                    CASE WHEN MONTH(GETDATE()) * 100 + DAY(GETDATE()) >= lica.mb * 100 + lica.db THEN 
                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm)) + 1
                    ELSE 
                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm))
                    END
                WHEN lica.mb = '99' OR lica.db = '99' THEN age
                ELSE 
                    CASE WHEN MONTH(GETDATE()) * 100 + DAY(GETDATE()) >= lica.mb * 100 + lica.db THEN 
                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm))
                    ELSE 
                        DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm)) - 1
                    END
            END
FROM lica
WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;

SELECT oblast, grs, gnezdo, domak, tm, db, mb, yb, age
FROM lica;

--пресмята правилно годините и ги презаписва 

