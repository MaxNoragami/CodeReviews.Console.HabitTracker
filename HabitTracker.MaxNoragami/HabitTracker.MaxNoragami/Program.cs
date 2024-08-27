using System;
using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

class Program 
{
    static string connectionString = @"Data Source=HabitTracker.db";
    static List<string> HabitsData = new List<string>();
    static List<string> MeasurementData = new List<string>();

    static void Main()
    {

        using(var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS database(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Habit TEXT,
                    Date TEXT,
                    Quantity INTEGER,
                    Measurement TEXT
                )";
            
            tableCmd.ExecuteNonQuery();
            connection.Close();
        }

        GetUserInput();
    }

    static void GetUserInput()
    {
        bool stopProgram = false;

        do{
        DisplayMenu();
        int choice = GetIntegerInput();

        switch(choice)
        {
            case 1:
                GetAllData();
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
                stopProgram = true;
                break;
            default:
                Console.WriteLine("WARNING: Invalid input!");
                break;   
        }
        }while(!stopProgram);

    }

    internal static void DisplayMenu()
    {
        Console.WriteLine("\n\nPick an option: ");
        Console.WriteLine("\n1. View All Records");
        Console.WriteLine("2. Insert a Record");
        Console.WriteLine("3. Delete a Record");
        Console.WriteLine("4. Update a Record");
        Console.WriteLine("0. Exit the program\n");
        Console.Write("Choice: ");
    }

    static void GetAllData()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText = "SELECT * FROM database";

            tableCmd.ExecuteNonQuery();

            List<DrinkingWater> tableData = new();

            SqliteDataReader reader = tableCmd.ExecuteReader();

            if(reader.HasRows)
            {
                while(reader.Read())
                {
                    tableData.Add(new DrinkingWater
                    {
                        Id = reader.GetInt32(0),
                        Habit = reader.GetString(1),
                        Date = DateTime.ParseExact(reader.GetString(2), "dd-MM-yy", new CultureInfo("en-US")),
                        Quantity = reader.GetInt32(3),
                        Measurement = reader.GetString(4)
                    });
                }
            }
            else
            {
                Console.WriteLine("WARNING: No rows available!");
            }
            connection.Close();

            Console.WriteLine("------------------------------------------\n");
            foreach(DrinkingWater item in tableData)
            {
                Console.WriteLine($"{item.Id} -- {item.Habit} -- {item.Date.ToString("dd-MMM-yyyy")} -- {item.Quantity} {item.Measurement}");
            }
            Console.WriteLine("-------------------------------\n");
        }
    
    }

    static void Insert()
    {
        
        string habitInput = GetHabitData();
        string dateInput = GetDateInput();
        int quantityInput = GetIntegerInput("Enter the quantity: ");
        string measurement = GetMeasurementData();

        using(var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.Parameters.Add("@Habit", SqliteType.Text).Value = habitInput;
            tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = dateInput;
            tableCmd.Parameters.Add("@Quantity", SqliteType.Integer).Value = quantityInput;
            tableCmd.Parameters.Add("@Measurement", SqliteType.Text).Value = measurement;
            tableCmd.CommandText = $"INSERT INTO database(habit, date, quantity, measurement) VALUES(@Habit, @Date, @Quantity, @Measurement)";

            tableCmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    static void Delete()
    {
        GetAllData();

        int recordId = GetIntegerInput("Enter the Id of the row you want to DELETE, enter 0 to exit to the Main Menu: ");

        if(recordId == 0) GetUserInput();

        using(var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.Parameters.Add("@RecordId", SqliteType.Integer).Value = recordId;
            tableCmd.CommandText = $"DELETE FROM database WHERE id == @RecordId";

            int success = tableCmd.ExecuteNonQuery();

            if(success == 0)
            {
                Console.WriteLine($"\nWARNING: No rows were found with Id: {recordId}\n");
                connection.Close();
                Delete();
            }

            connection.Close();
        }
    }

    static void Update()
    {
        GetAllData();
        int recordId = GetIntegerInput("Enter the Id of the row you want to UPDATE, enter 0 to exit to the Main Menu: ");
        if (recordId == 0) GetUserInput();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var checkCmd = connection.CreateCommand();

            checkCmd.Parameters.Add("@RecordId", SqliteType.Integer).Value = recordId;
            checkCmd.CommandText = $"SELECT id FROM database WHERE id == @RecordId";

            int idAvailable = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (idAvailable == 0)
            {
                Console.WriteLine($"\nWARNING: No rows were found with Id: {recordId}\n");
                connection.Close();
                Update();
            }

            string habitInput = GetHabitData();
            string dateInput = GetDateInput();
            int quantityInput = GetIntegerInput("Enter the quantity: ");
            string measurement = GetMeasurementData();

            var tableCmd = connection.CreateCommand();

            tableCmd.Parameters.Add("@Habit", SqliteType.Text).Value = habitInput;
            tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = dateInput;
            tableCmd.Parameters.Add("@Quantity", SqliteType.Integer).Value = quantityInput;
            tableCmd.Parameters.Add("@RecordId", SqliteType.Integer).Value = recordId;
            tableCmd.Parameters.Add("@Measurement", SqliteType.Text).Value = measurement;

            tableCmd.CommandText = $"UPDATE database SET habit = @Habit, date = @Date, quantity = @Quantity, measurement = @Measurement WHERE id == @RecordId";
            tableCmd.ExecuteNonQuery();

            connection.Close();
        }
    }

    internal static int GetIntegerInput(string message = "")
    {
        if(message != "") Console.Write(message);

        string? inputNum = Console.ReadLine();
        bool validateInput = false;
        int num = -1;

        do
        {
            validateInput = int.TryParse(inputNum, out num);
        }while(!validateInput);

        return num;
    }

    internal static string GetDateInput()
    {
        string? date = "";

        do
        {
            Console.Write("Enter the date, format dd-mm-yy, 0 to exit to Main Menu: ");
            date = Console.ReadLine();

            if(date == "0") GetUserInput();

        }while(!DateTime.TryParseExact(date, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _));

        return date;
    }

    internal static string GetStringInput(string message = "")
    {
        if(message != "") Console.Write(message);
        string? strInput = "";
        do{
            strInput = Console.ReadLine();
        }while(strInput == null);
        strInput = strInput.ToLower();
        char[] input = strInput.ToCharArray();
        input[0] = char.ToUpper(input[0]);
        strInput = string.Join("", input);
        return strInput;
    }

    internal static string GetHabitData()
    {
        string? habitInput = "";
        if(HabitsData.Count == 0)
        {
            habitInput = GetStringInput("Enter the habit you'd like to track: ");
            HabitsData.Add(habitInput);
        }
        else
        {
            Console.WriteLine("Quick Habit Picker: ");

            int counter = 1;
            foreach(string habit in HabitsData)
            {
                Console.WriteLine($"{counter}. {habit}");
                counter++;
            }
            Console.WriteLine("0. New Custom Habit\n");
            Console.Write("Choice: ");
            int choice = GetIntegerInput();

            if(choice == 0)
            {
                habitInput = GetStringInput("Enter the habit you'd like to track: ");
                HabitsData.Add(habitInput);
            }
            
            counter = 1;
            foreach(string habit in HabitsData)
            {
                if(counter == choice)
                {
                    habitInput = habit;
                    break;
                }
                counter++;
            }
            
        }

        return habitInput;

    }

    internal static string GetMeasurementData()
    {
        string? mInput = "";
        if (MeasurementData.Count == 0)
        {
            mInput = GetStringInput("Enter the measurement suitable for the habit: ");
            MeasurementData.Add(mInput);
        }
        else
        {
            Console.WriteLine("Quick Measurement Picker: ");

            int counter = 1;
            foreach (string measurement in MeasurementData)
            {
                Console.WriteLine($"{counter}. {measurement}");
                counter++;
            }
            Console.WriteLine("0. New Custom Measurement\n");
            Console.Write("Choice: ");
            int choice = GetIntegerInput();

            if (choice == 0)
            {
                mInput = GetStringInput("Enter the measurement suitable for the habit: ");
                MeasurementData.Add(mInput);
            }

            counter = 1;
            foreach (string measurement in MeasurementData)
            {
                if (counter == choice)
                {
                    mInput = measurement;
                    break;
                }
                counter++;
            }

        }

        return mInput;

    }
}

public class DrinkingWater
{
    public int Id {get; set;}
    public string? Habit {get; set;}
    public DateTime Date {get; set;}
    public int Quantity {get; set;}
    public string? Measurement {get; set;}
}