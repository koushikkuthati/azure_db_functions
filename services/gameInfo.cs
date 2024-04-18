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

    public async Task<string> CreateGame(string userId)
    {

        // Generate gameId and currentDateTime
        string gameId = generateUniquegameId(userId);
        // Create JObject for game object
        JObject gameObject = new JObject();
        gameObject["userId"] = userId;
        gameObject["gameId"] = gameId;
        gameObject["gameCompleted"] = 0;
        gameObject["noOfCorrectAnswers"] = 0;
        gameObject["noOfWrongAnswers"] = 0;

        // database insertion
        string insertQuery = "INSERT INTO GameData (userId, gameId, gameCompleted, noOfCorrectAnswers, noOfWrongAnswers) VALUES ('"
                     + userId + "', '"
                     + gameId + "', "
                     + "0, 0, 0);";


        await dbConnection.ExecuteQueryAsync(insertQuery);

        // Create success response
        var response = new
        {
                gameId = gameObject["gameId"],
                userId = gameObject["userId"],
        };

        // Serialize the response object to JSON
        string jsonResponse = JsonConvert.SerializeObject(response);

        return jsonResponse;
    }


    private string generateUniquegameId(string deviceId)
    {
        string currentTime = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Current time in yyyyMMddHHmmssfff format
        string gameId = deviceId + "_" + currentTime;
        return gameId;
    }
    public async Task UpdateUserResponse(string gameId, bool isValidUserResponse)
    {
        JObject gameObject = await getCurrentStatsOfGameAsync(gameId);


        int noOfCorrectAnswers = (int?)gameObject["noOfCorrectAnswers"] ?? 0;
        int noOfWrongAnswers = (int?)gameObject["noOfWrongAnswers"] ?? 0;

        _logger.LogInformation(gameObject.ToString());

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


public async Task UpdateGameCompleted(string gameId, string accuracy, string completion)
{

    JObject gameObject = await getCurrentStatsOfGameAsync(gameId);

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
        string query = "SELECT * FROM GameData WHERE gameId = '" + gameId + "'";
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
        string updateQuery = "UPDATE GameData SET " +
                     "userId = '" + gameObject["userId"] + "', " +
                     "noOfCorrectAnswers = " + gameObject["noOfCorrectAnswers"] + ", " +
                     "noOfWrongAnswers = " + gameObject["noOfWrongAnswers"] + ", " +
                     "gameCompleted = " + (Convert.ToBoolean(gameObject["gameCompleted"]) ? 1 : 0) + ", " +
                     "accuracyRate = " + (gameObject["accuracyRate"].Type != JTokenType.Null ? gameObject["accuracyRate"] : "NULL") + ", " +
                     "completionRate = " + (gameObject["completionRate"].Type != JTokenType.Null ? gameObject["completionRate"] : "NULL") +
                     " WHERE gameId = '" + gameObject["gameId"] + "'";


        _logger.LogInformation(updateQuery);
        await dbConnection.ExecuteQueryAsync(updateQuery);
    }

    public async Task<JObject> RetriveUserStats(string userId, string gameId)
    {
        string selectQuery = "SELECT * FROM GameData WHERE gameId = '" + gameId + "' AND userId = '" + userId + "' AND gameCompleted = 1";

        List<string> gameObjects = await dbConnection.ExecuteQueryAsync(selectQuery);
        _logger.LogInformation(gameObjects.Count.ToString());
        if (gameObjects.Count > 0)
        {
            JObject gameObject = JObject.Parse(gameObjects[0]); // Parse the JSON string into a JObject
            return gameObject;
        }

        return new JObject();
    }


    public async Task<JArray> RetrieveUserStatsForAllGames(string userId)
    {
        string selectQuery = "SELECT * FROM GameData WHERE userId = '" + userId + "' AND gameCompleted = 1";

        List<string> gameObjects = await dbConnection.ExecuteQueryAsync(selectQuery);

        JArray resultArray = new JArray();

        if (gameObjects.Count > 0)
        {
            foreach (var gameObject in gameObjects)
            {
                JObject gameJson = JObject.Parse(gameObject); // Parse each JSON string into a JObject
                if (gameJson["gameId"] != null)
                {
                    _logger.LogInformation(gameJson["gameId"].ToString());
                }
                resultArray.Add(gameJson); // Add each game object to the result JArray
            }
        }
        _logger.LogInformation(resultArray.ToString());
        return resultArray;
    }

    public async Task<JObject> RetriveUserHighestScore(string userId)
   {
       string selectQuery = "SELECT TOP 1 * FROM GameData WHERE userId = '" + userId + "' AND gameCompleted = 1 ORDER BY noOfCorrectAnswers DESC;";


       List<string> gameObjects = await dbConnection.ExecuteQueryAsync(selectQuery);
        _logger.LogInformation(gameObjects.Count.ToString());
        if (gameObjects.Count > 0)
        {


            string query = "SELECT TOP 1 Name FROM UserData WHERE userID = '" + userId + "';"; // Concatenated query with TOP 1
            List<string> userObjects = await dbConnection.ExecuteQueryAsync(query);
            JObject userObject = JObject.Parse(userObjects[0]); // Parse the JSON string into a JObject
            string name = userObject["Name"].ToString(); // Convert JToken to string
            JObject gameObject = JObject.Parse(gameObjects[0]); // Parse the JSON string into a JObject
            gameObject.Add("Name", name);
            return gameObject;

        }


        return new JObject();
   }


}
