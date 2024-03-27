using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class userInformationServices
{

    private readonly ILogger _logger;
    private static DatabaseConnection dbConnection = DatabaseConnection.GetInstance();

    public userInformationServices(ILogger logger)
    {
        _logger = logger;
        dbConnection = DatabaseConnection.GetInstance();
        // Open the database connection
        dbConnection.OpenConnection();
    }

    public async Task<string> GetOrCreateUser(string deviceID, string name)
    {

        JObject userObject = await IsExistingUserBasedOnDeviceIDAndName(deviceID, name);
        if (userObject?.HasValues == true)
        {
            return userObject["UserID"]?.ToString() ?? ""; // Convert JToken to string, or return empty string if null
        }
        else
        {
            userObject = await CreateUser(deviceID, name);
            return userObject["UserID"]?.ToString() ?? ""; // Convert JToken to string, or return empty string if null
        }
    }


    public async Task<string> GetAllUsers(string deviceID)
    {
        string query = "SELECT * FROM Playerinfo WHERE DeviceID = '" + deviceID + "'";

        // Assuming dbConnection is an instance of IDbConnection, replace it with your actual database connection
        List<string> userDetails = await dbConnection.ExecuteQueryAsync(query);

        // Convert each user detail string into a JObject
        List<JObject> userObjects = new List<JObject>();
        foreach (string userDetail in userDetails)
        {
            JObject userObject = JObject.Parse(userDetail);
            userObjects.Add(userObject);
        }

        // Serialize the array of JObject into a JSON string
        string jsonUserDetails = JsonConvert.SerializeObject(userObjects);

        return jsonUserDetails;
    }


    private async Task<JObject> CreateUser(string deviceID, string name)
    {
        JObject userObject = new JObject();
        string UserID = deviceID + "_" + name;
        userObject["UserID"] = UserID;
        userObject["DeviceID"] = deviceID;
        userObject["Name"] = name;
        userObject["IsSoundEnabled"] = 1;
        userObject["IsMusicEnabled"] = 1;
        userObject["Age"] = 9;
        string query = $"INSERT INTO Playerinfo (UserID, DeviceID, Name, IsSoundEnabled, IsMusicEnabled, Age) " +
               $"VALUES ('{UserID}', '{deviceID}', '{userObject["Name"]}', '{userObject["IsSoundEnabled"]}', '{userObject["IsMusicEnabled"]}', '{userObject["Age"]}')";
        await dbConnection.ExecuteQueryAsync(query);
        return userObject;
    }


    public async Task<JObject> IsExistingUserBasedOnDeviceIDAndName(string DeviceID, string name)
    {
         string query = "SELECT * FROM Playerinfo WHERE DeviceID = '" + DeviceID + "' AND Name = '" + name + "'";
        // Assuming dbConnection is an instance of IDbConnection, replace it with your actual database connection
        List<string> userDetails = await dbConnection.ExecuteQueryAsync(query);

        if (userDetails.Count > 0)
        {
            // Parsing the JSON string safely
            JObject userObject = JObject.Parse(userDetails[0]);
            return userObject;
        }

        return null;
    }

    public async Task<JObject> IsExistingUserBasedOnUserID(string userID)
    {
         string query = "SELECT * FROM Playerinfo WHERE UserID = '" + userID + "'";
        // Assuming dbConnection is an instance of IDbConnection, replace it with your actual database connection
        List<string> userDetails = await dbConnection.ExecuteQueryAsync(query);

        if (userDetails.Count > 0)
        {
            // Parsing the JSON string safely
            JObject userObject = JObject.Parse(userDetails[0]);
            return userObject;
        }

        return null;
    }

    public async Task UpdateName(string userID, string name)
    {

        JObject userObject = await IsExistingUserBasedOnUserID(userID);

        if (!(userObject != null && userObject.HasValues))
        {
            throw new Exception("Error: Invalid user");
        }
        string query = $"UPDATE Playerinfo SET Name = '{name}' WHERE UserID = '{userID}'";
        await dbConnection.ExecuteQueryAsync(query);
    }

    public async Task UpdateAge(string userID, int age)
    {

        JObject userObject = await IsExistingUserBasedOnUserID(userID);

        if (!(userObject != null && userObject.HasValues))
        {
            throw new Exception("Error: Invalid user");
        }
        string query = $"UPDATE Playerinfo SET Age = '{age}' WHERE UserID = '{userID}'";
        await dbConnection.ExecuteQueryAsync(query);
    }

    public async Task UpdateMusic(string userID, bool music)
    {
        JObject userObject = await IsExistingUserBasedOnUserID(userID);

        if (!(userObject != null && userObject.HasValues))
        {
            throw new Exception("Error: Invalid user");
        }
        int musicEnabled = music ? 1 : 0;
        string query = $"UPDATE Playerinfo SET IsMusicEnabled = '{musicEnabled}' WHERE UserID = '{userID}'";
        await dbConnection.ExecuteQueryAsync(query);
    }

    public async Task UpdateSound(string userID, bool sound)
    {
        JObject userObject = await IsExistingUserBasedOnUserID(userID);

        if (!(userObject != null && userObject.HasValues))
        {
            throw new Exception("Error: Invalid user");
        }
        int soundEnabled = sound ? 1 : 0;
        string query = $"UPDATE Playerinfo SET IsSoundEnabled = '{soundEnabled}' WHERE UserID = '{userID}'";
        await dbConnection.ExecuteQueryAsync(query);
    }
}
