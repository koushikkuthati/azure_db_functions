using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameInformationServices
{

    private readonly ILogger _logger;
    private static DatabaseConnection dbConnection = DatabaseConnection.GetInstance();

    public GameInformationServices(ILogger logger)
    {
        _logger = logger;
        dbConnection = DatabaseConnection.GetInstance();
        // Open the database connection
        dbConnection.OpenConnection();
    }

    public async Task<string> CreateGame(string userID)
    {

        // Generate gameID and currentDateTime
        string gameID = generateUniqueGameId(userID);
        // Create JObject for game object
        JObject gameObject = new JObject();
        gameObject["userID"] = userID;
        gameObject["gameID"] = gameID;
        gameObject["gameCompleted"] = 0;
        gameObject["noOfCorrectAnswers"] = 0;
        gameObject["noOfWrongAnswers"] = 0;

        // database insertion
        string insertQuery = "INSERT INTO GameStatistics (userID, gameID, gameCompleted, noOfCorrectAnswers, noOfWrongAnswers) VALUES ('"
                     + userID + "', '"
                     + gameID + "', "
                     + "0, 0, 0);";


        await dbConnection.ExecuteQueryAsync(insertQuery);

        // Create success response
        var response = new
        {
            message = "Success",
            data = new
            {
                gameID = gameObject["gameID"],
                userID = gameObject["userID"],
            }
        };

        // Serialize the response object to JSON
        string jsonResponse = JsonConvert.SerializeObject(response);

        return jsonResponse;
    }


    private string generateUniqueGameId(string deviceId)
    {
        string currentTime = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Current time in yyyyMMddHHmmssfff format
        string gameId = deviceId + "_" + currentTime;
        return gameId;
    }
    public async Task UpdateUserResponse(string gameID, bool isValidUserResponse)
    {
        JObject gameObject = await getCurrentStatsOfGameAsync(gameID);


        int noOfCorrectAnswers = (int?)gameObject["noOfCorrectAnswers"] ?? 0;
        int noOfWrongAnswers = (int?)gameObject["noOfWrongAnswers"] ?? 0;

        if (isValidUserResponse)
        {
            noOfCorrectAnswers++;
            gameObject["noOfCorrectAnswers"] = noOfCorrectAnswers;
        }
        else
        {
            noOfWrongAnswers++;
            gameObject["noOfWrongAnswers"] = noOfWrongAnswers;
        }

        await SaveGameStatus(gameObject);
    }


public async Task UpdateGameCompleted(string gameID, string accuracy, string completion)
{

    JObject gameObject = await getCurrentStatsOfGameAsync(gameID);

    if (gameObject != null)
    {
        // Update the properties
        double accuracyRate = double.Parse(accuracy);
        double completionRate = double.Parse(completion);
        gameObject["accuracyRate"] = accuracyRate;
        gameObject["gameCompleted"] = true;
        gameObject["completionRate"] = completionRate;
        // Save the updated game status
        await SaveGameStatus(gameObject);
    }
    else
    {
        throw new Exception("Couldnt Find the game");
    }
}

    private async Task<JObject> getCurrentStatsOfGameAsync(string gameId)
    {
        string query = "SELECT * FROM GameStatistics WHERE gameID = '" + gameId + "'";
        List<string> gameObjects = await dbConnection.ExecuteQueryAsync(query);
        if (gameObjects.Count > 0)
        {
            JObject gameObject = JObject.Parse(gameObjects[0]); // Parse the JSON string into a JObject
            return gameObject;
        }

        return new JObject();
    }

    private async Task SaveGameStatus(JObject gameObject)
    {
        string updateQuery = "UPDATE GameStatistics SET " +
                     "userID = '" + gameObject["userID"] + "', " +
                     "noOfCorrectAnswers = " + gameObject["noOfCorrectAnswers"] + ", " +
                     "noOfWrongAnswers = " + gameObject["noOfWrongAnswers"] + ", " +
                     "gameCompleted = " + (Convert.ToBoolean(gameObject["gameCompleted"]) ? 1 : 0) + ", " +
                     "accuracyRate = " + (gameObject["accuracyRate"].Type != JTokenType.Null ? gameObject["accuracyRate"] : "NULL") + ", " +
                     "completionRate = " + (gameObject["completionRate"].Type != JTokenType.Null ? gameObject["completionRate"] : "NULL") +
                     " WHERE gameID = '" + gameObject["gameID"] + "'";


        _logger.LogInformation(updateQuery);
        await dbConnection.ExecuteQueryAsync(updateQuery);
    }

    public async Task<JObject> RetriveUserStats(string userID, string gameID)
    {
        string selectQuery = "SELECT * FROM GameStatistics WHERE gameID = '" + gameID + "' AND userID = '" + userID + "' AND gameCompleted = 1";

        List<string> gameObjects = await dbConnection.ExecuteQueryAsync(selectQuery);
        _logger.LogInformation(gameObjects.Count.ToString());
        if (gameObjects.Count > 0)
        {
            JObject gameObject = JObject.Parse(gameObjects[0]); // Parse the JSON string into a JObject
            return gameObject;
        }

        return new JObject();
    }


    public async Task<JArray> RetrieveUserStatsForAllGames(string userID)
    {
        string selectQuery = "SELECT * FROM GameStatistics WHERE userID = '" + userID + "' AND gameCompleted = 1";

        List<string> gameObjects = await dbConnection.ExecuteQueryAsync(selectQuery);

        JArray resultArray = new JArray();

        if (gameObjects.Count > 0)
        {
            foreach (var gameObject in gameObjects)
            {
                JObject gameJson = JObject.Parse(gameObject); // Parse each JSON string into a JObject
                if (gameJson["gameID"] != null)
                {
                    _logger.LogInformation(gameJson["gameID"].ToString());
                }
                resultArray.Add(gameJson); // Add each game object to the result JArray
            }
        }
        _logger.LogInformation(resultArray.ToString());
        return resultArray;
    }

}
