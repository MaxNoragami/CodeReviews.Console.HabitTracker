using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Data.Sqlite;

namespace HabbitTracker
{
    class Program
    {
        static string connectionString = @"Data Source=HabitTracker.db";

        static void Main()
        {

           using(var connection = new SqliteConnection(connectionString))
           {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = 
                            @"CREATE TABLE IF NOT EXISTS drinking_water (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Date TEXT,
                                Quantity INTEGER
                            )";
                tableCmd.ExecuteNonQuery();

                connection.Close();
           }

           GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;
            while(!closeApp)
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("1. View All Records");
                Console.WriteLine("2. Insert Record");
                Console.WriteLine("3. Delete Record");
                Console.WriteLine("4. Update Record");
                Console.WriteLine("\n0. Close Application");
                Console.WriteLine("---------------------------------------\n");

                string? commandInput = "";
                bool validInput = false;
                int command = -1;
                do
                {
                    commandInput = Console.ReadLine();
                    validInput = int.TryParse(commandInput, out command);
                }while(!validInput);

                switch(command)
                {
                    case 1:
                        GetAllRecords();
                        break;
                    case 2:
                        Insert();
                        break;
                    case 3:
                        Delete();
                        break;
                    case 4:
                        Update();
                        break;
                    case 0:
                        Console.WriteLine("\nGoodbye\n");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("\nWARNING: Invalid command!");
                        break;
                }


            }

        }

        private static void Update()
        {
            
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type Id of the record you'd like to update, 0 to exit to Main Menu\n\n");

            using(var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\n\nPlease insert number of gasses or other measure of your choice (integers only)\n\n");
                
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} where Id = {recordId}";

                tableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type the Id of the record you wanr to delete, 0 to exit to Main Menu!");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";

                int rowCount = tableCmd.ExecuteNonQuery();

                if(rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
                    Delete();
                }
                connection.Close();
            }

            Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");
            GetUserInput();
        }

        private static void GetAllRecords()
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = 
                    $"SELECT * FROM drinking_water ";

                List<DrinkingWater> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if(reader.HasRows)
                {
                    while(reader.Read())
                    {
                        tableData.Add(
                            new DrinkingWater
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            }
                        );
                    }
                }
                else
                {
                    Console.WriteLine("No rows were found!");
                }

                connection.Close();

                Console.WriteLine("--------------------\n");
                foreach(var dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MMM-yyyy")} - Quantity: {dw.Quantity}");
                }
                Console.WriteLine("--------------------\n");

            }
        }

        private static void Insert()
        {
            string date = GetDateInput();
            int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (integer numbers only)");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = 
                $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity} )";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to the main menu!");
            string? dateInput = "";
            dateInput = Console.ReadLine();

            if(dateInput == "0") GetUserInput();

            while(!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to exit to Main Menu or try again: \n\n");
                dateInput = Console.ReadLine();

            }

            return dateInput;
        }

        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            bool validInput = false;
            int num = 0;
            string? numInput = "";
            do
            {
                numInput = Console.ReadLine();
                validInput = int.TryParse(numInput, out num);
            } while (!validInput && num > 0);

            if (num == 0) GetUserInput();

            return num;
        }
    }
}

public class DrinkingWater
{
    public int Id {get; set;}
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}