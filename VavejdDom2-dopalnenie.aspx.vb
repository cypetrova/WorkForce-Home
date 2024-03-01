Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    ' Останалите части от кода, които вече имате в Page_Load



' Вашата връзка към базата данни
    Dim connectionString As String = ConfigurationManager.ConnectionStrings("nsi_rabsilaConnectionString").ConnectionString
    Using connection As New SqlConnection(connectionString)
        ' Отваряте връзката към базата данни
        connection.Open()

        ' SQL заявка за обновяване на данните в tPersons
        Dim sql As String = "
            UPDATE tPersons
            SET age = DATEDIFF(year, 
                              CAST(CONCAT(ppd, '-', ppm, '-', ppg) AS DATE), 
                              CAST(CONCAT(db, '-', dm, '-', yb) AS DATE))
            FROM tPersons
            INNER JOIN dbo.lica AS l ON tPersons.oblast = l.oblast 
                                      AND tPersons.grs = l.grs 
                                      AND tPersons.gnezdo = l.gnezdo 
                                      AND tPersons.domak = l.domak 
                                      AND tPersons.tm = l.tm
            INNER JOIN dbo.dom AS d ON l.oblast = d.oblast 
                                    AND l.grs = d.grs 
                                    AND l.gnezdo = d.gnezdo 
                                    AND l.domak = d.domak 
                                    AND l.tm = d.tm
            WHERE tPersons.age != DATEDIFF(year, 
                                           CAST(CONCAT(ppd, '-', ppm, '-', ppg) AS DATE), 
                                           CAST(CONCAT(db, '-', dm, '-', yb) AS DATE))"
        
        ' Създавате команда за изпълнение на SQL заявката
        Using command As New SqlCommand(sql, connection)
            ' Изпълнявате заявката
            Dim rowsAffected As Integer = command.ExecuteNonQuery()
            ' Проверявате дали са засегнати редове
            If rowsAffected > 0 Then
                ' Ако има засегнати редове, може да направите някакво уведомление или действие за потвърждение
                Response.Write("Обновяването на възрастта е успешно!")
            Else
                ' Ако няма засегнати редове, може да направите съответно уведомление
                Response.Write("Няма данни за обновяване на възрастта.")
            End If
        End Using

        ' Затваряте връзката към базата данни
        connection.Close()
    End Using
End Sub
