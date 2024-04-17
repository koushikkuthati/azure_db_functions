using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public class DatabaseConnection
{
    // Private static instance variable
private static DatabaseConnection? instance;

    List<object[]> userDetailsArray = new List<object[]>();
    
    // Private connection string
    private string connectionString = "Server=tcp:fishdbinstance.database.windows.net,1433;Initial Catalog=fishDatabase;Persist Security Info=False;User ID=Gowtham;Password=Team4Fish;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    // Private SQL connection object
    private SqlConnection connection;

    // Private constructor to prevent instantiation from outside
    private DatabaseConnection()
    {
        connection = new SqlConnection(connectionString);
    }

    // Public static method to get the instance
    public static DatabaseConnection GetInstance()
    {
        // Lazy initialization: create the instance if it doesn't exist
        if (instance == null)
        {
            instance = new DatabaseConnection();
        }
        return instance;
    }

    // Method to open the database connection
    public void OpenConnection()
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                Console.WriteLine("Database connection opened successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Error opening database connection: " + ex.Message);
        }
    }

    // Method to close the database connection
    public void CloseConnection()
    {
        try
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
                Console.WriteLine("Database connection closed successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Error closing database connection: " + ex.Message);
        }
    }

    // Method to execute a SQL query
   public async Task<List<string>> ExecuteQueryAsync(string query)
{
    SqlDataReader reader = null;
    List<string> jsonArray = new List<string>();

    try
    {
        Console.WriteLine(query);
        SqlCommand command = new SqlCommand(query, connection);
        reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Dictionary<string, object> row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader[i];
            }

            string json = JsonConvert.SerializeObject(row);
            jsonArray.Add(json);
        }
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Error executing SQL query: " + ex.Message);
    }
    finally
    {
        // Ensure the reader is closed
        if (reader != null)
        {
            reader.Close();
        }
    }

    Console.WriteLine("Qiery successful");

    return jsonArray;
}

}

