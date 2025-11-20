
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MaterialManagementSystem
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["MaterialDbConnection"].ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки строки подключения: {ex.Message}\n\nПроверьте файл App.config",
                    "Ошибка конфигурации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    MessageBox.Show("Подключение к базе данных успешно установлено!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (SqlException ex)
            {
                string errorMessage = $"Ошибка подключения к базе данных: {ex.Message}\n\n";

                if (ex.Number == 53) // Network related error
                {
                    errorMessage += "Проверьте:\n" +
                                   "1. Запущена ли служба SQL Server\n" +
                                   "2. Правильность имени сервера\n" +
                                   "3. Доступность сервера в сети";
                }
                else if (ex.Number == 4060) // Cannot open database
                {
                    errorMessage += "База данных MaterialManagementDB не найдена.\n" +
                                   "Создайте базу данных с помощью скрипта SQL.";
                }
                else if (ex.Number == 18456) // Login failed
                {
                    errorMessage += "Ошибка аутентификации.\n" +
                                   "Проверьте правильность учетных данных.";
                }

                MessageBox.Show(errorMessage, "Ошибка подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void InitializeDatabase()
        {
            if (!TestConnection())
            {
                return;
            }

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string createUsersTable = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                        BEGIN
                            CREATE TABLE Users (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                Username NVARCHAR(50) UNIQUE NOT NULL,
                                Password NVARCHAR(100) NOT NULL,
                                FullName NVARCHAR(100) NOT NULL,
                                Role NVARCHAR(50) DEFAULT 'User',
                                CreatedDate DATETIME DEFAULT GETDATE()
                            )
                            PRINT 'Таблица Users создана успешно.'
                        END";

                    SqlCommand cmd = new SqlCommand(createUsersTable, connection);
                    cmd.ExecuteNonQuery();

                    string createMaterialsTable = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Materials')
                        BEGIN
                            CREATE TABLE Materials (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                Name NVARCHAR(100) NOT NULL,
                                MaterialType NVARCHAR(50) NOT NULL,
                                Quantity FLOAT NOT NULL,
                                Unit NVARCHAR(20) NOT NULL,
                                PackageQuantity INT NOT NULL,
                                MinQuantity INT NOT NULL,
                                Price DECIMAL(18,2) NOT NULL,
                                CreatedDate DATETIME DEFAULT GETDATE(),
                                UpdatedDate DATETIME DEFAULT GETDATE()
                            )
                            PRINT 'Таблица Materials создана успешно.'
                        END";

                    cmd = new SqlCommand(createMaterialsTable, connection);
                    cmd.ExecuteNonQuery();

                    string checkAdmin = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
                    cmd = new SqlCommand(checkAdmin, connection);
                    int adminCount = (int)cmd.ExecuteScalar();

                    if (adminCount == 0)
                    {
                        string insertAdmin = @"
                            INSERT INTO Users (Username, Password, FullName, Role)
                            VALUES ('admin', 'admin123', 'Администратор Системы', 'Admin')";
                        cmd = new SqlCommand(insertAdmin, connection);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Создан пользователь admin с паролем admin123", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    string checkMaterials = "SELECT COUNT(*) FROM Materials";
                    cmd = new SqlCommand(checkMaterials, connection);
                    int materialCount = (int)cmd.ExecuteScalar();

                    if (materialCount == 0)
                    {
                        string insertMaterials = @"
                            INSERT INTO Materials (Name, MaterialType, Quantity, Unit, PackageQuantity, MinQuantity, Price) VALUES
                            ('Сталь листовая 3мм', 'Сырье', 1500.5, 'кг', 50, 100, 45.75),
                            ('Пластик ABS белый', 'Сырье', 1200.0, 'кг', 25, 100, 120.30),
                            ('Краска эмаль белая', 'Вспомогательные материалы', 200.0, 'л', 10, 20, 85.00),
                            ('Корпус устройства А-100', 'Полуфабрикат', 350, 'шт', 1, 50, 250.00),
                            ('Картонная коробка малая', 'Упаковка', 1200, 'шт', 100, 200, 15.50),
                            ('Алюминиевый профиль', 'Сырье', 800.0, 'м', 10, 50, 89.90),
                            ('Готовое изделие Г-100', 'Готовая продукция', 100, 'шт', 1, 10, 1250.00)";
                        cmd = new SqlCommand(insertMaterials, connection);
                        int rowsInserted = cmd.ExecuteNonQuery();
                        MessageBox.Show($"Добавлено {rowsInserted} тестовых материалов", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
