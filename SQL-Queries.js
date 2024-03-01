SELECT dom.ppd, dom.ppm, dom.ppg, lica.db, lica.mb, lica.yb
FROM dom
INNER JOIN lica ON dom.tm = lica.tm AND dom.oblast = lica.oblast AND dom.grs = lica.grs;

//трябва да изглежда 
UPDATE tPersons
SET age = DATEDIFF(year, 
                  CAST(CONCAT(ppd, '-', ppm, '-', ppg) AS DATE), 
                  CAST(CONCAT(db, '-', dm, '-', yb) AS DATE))
WHERE EXISTS (
    SELECT 1 
    FROM dbo.lica AS l
    INNER JOIN dbo.dom AS d ON l.oblast = d.oblast 
                            AND l.grs = d.grs 
                            AND l.gnezdo = d.gnezdo 
                            AND l.domak = d.domak 
                            AND l.tm = d.tm
    WHERE l.oblast = tPersons.oblast 
      AND l.grs = tPersons.grs 
      AND l.gnezdo = tPersons.gnezdo 
      AND l.domak = tPersons.domak 
      AND l.tm = tPersons.tm
)
AND age != DATEDIFF(year, 
                    CAST(CONCAT(ppd, '-', ppm, '-', ppg) AS DATE), 
                    CAST(CONCAT(db, '-', dm, '-', yb) AS DATE));
					
