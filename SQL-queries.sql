   За база данни dom
 SELECT 
     ppg AS year,
     ppm AS month,
     ppd AS day
 FROM 
     dom;

   За база данни lica
 SELECT 
     yb AS year,
     mb AS month,
     db AS day
 FROM 
     lica;

     UPDATE lica
 SET age = CASE 
                 WHEN lica.mb = '99' OR lica.db = '99' THEN DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), GETDATE()) + 1
                 WHEN lica.mb = '00' OR lica.db = '00' THEN age
                 ELSE DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), GETDATE())
             END
 FROM lica
 JOIN dom ON lica.oblast = dom.oblast
     AND lica.grs = dom.grs
     AND lica.gnezdo = dom.gnezdo
     AND lica.domak = dom.domak
     AND lica.tm = dom.tm
 WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;

 SELECT oblast, grs, gnezdo, domak, tm, db, mb, yb, age
 FROM lica;

 UPDATE lica
 SET age = CASE 
                 WHEN lica.mb = '00' AND lica.db = '00' THEN DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), GETDATE()) + 1
                 WHEN lica.mb = '99' OR lica.db = '99' THEN age
                 ELSE DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), GETDATE())
             END
 FROM lica
 JOIN dom ON lica.oblast = dom.oblast
     AND lica.grs = dom.grs
     AND lica.gnezdo = dom.gnezdo
     AND lica.domak = dom.domak
     AND lica.tm = dom.tm
 WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;

 SELECT oblast, grs, gnezdo, domak, tm, db, mb, yb, age
 FROM lica;

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

*********************************************************************************************************
UPDATE lica
SET age = DATEDIFF(YEAR, CASE WHEN ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1 THEN CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) ELSE NULL END, 
                   CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)))
FROM lica
JOIN dom ON lica.oblast = dom.oblast
    AND lica.grs = dom.grs
    AND lica.gnezdo = dom.gnezdo
    AND lica.domak = dom.domak
    AND lica.tm = dom.tm
WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;
SELECT oblast, grs, gnezdo, domak, tm, db, mb, yb, age
FROM lica;
***************************************************************************************************************
SET age = CASE 
                WHEN lica.mb = '00' AND lica.db = '00' THEN DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm)) + 1
                WHEN lica.mb = '99' OR lica.db = '99' THEN age
                ELSE DATEDIFF(YEAR, CONVERT(DATE, CONCAT(lica.yb, '-', lica.mb, '-', lica.db)), (SELECT CONVERT(DATE, CONCAT(dom.ppg, '-', dom.ppm, '-', dom.ppd)) FROM dom WHERE lica.oblast = dom.oblast AND lica.grs = dom.grs AND lica.gnezdo = dom.gnezdo AND lica.domak = dom.domak AND lica.tm = dom.tm))
            END
FROM lica
WHERE ISDATE(CONCAT(lica.yb, '-', lica.mb, '-', lica.db)) = 1;

SELECT oblast, grs, gnezdo, domak, tm, db, mb, yb, age
FROM lica;