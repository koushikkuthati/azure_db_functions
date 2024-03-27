using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameInformationRouter
{
    private readonly ILogger _logger;
    private GameInformationServices _gameService;

    public GameInformationRouter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GameInformationRouter>();
        _gameService = new GameInformationServices(_logger);
    }

    [Function("createNewGame")]
    public async Task<IActionResult> CreateNewGame(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "game/create")] HttpRequestData req)
    {

        try
        {
            _logger.LogInformation("Processing a request to create a new game");

            string? userID = req.Query["userID"];

            if (string.IsNullOrEmpty(userID))
            {
                var errorResponse = new { message = "Error: userID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            string jsonResponse = await _gameService.CreateGame(userID);

            return new ContentResult
            {
                Content = jsonResponse,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }


    [Function("getGameStats")]
    public async Task<IActionResult> retrieveUserStats(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "game/{gameID}")] HttpRequestData req, string gameID)
    {
        try
        {
            _logger.LogInformation("Processing a request to retrive particular game Information");

            string? userID = req.Query["userID"];

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(gameID))
            {
                var errorResponse = new { message = "Error: userID/gameID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            JObject jsonResponse = await _gameService.RetriveUserStats(userID, gameID);

            string jsonResult = jsonResponse.ToString();
            return new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }


    [Function("getAllGameStats")]
    public async Task<IActionResult> retrieveUserStatsForAllGames(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "game")] HttpRequestData req, string gameID)
    {
        try
        {
            _logger.LogInformation("Processing a request to retrive all the list of game");

            string? userID = req.Query["userID"];

            if (string.IsNullOrEmpty(userID))
            {
                var errorResponse = new { message = "Error: userID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            JArray jsonResponse = await _gameService.RetrieveUserStatsForAllGames(userID);


            string jsonResult = jsonResponse.ToString();
            return new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }

    }


    [Function("updateUserResponse")]
    public async Task<IActionResult> UpdateUserResponse(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "game/{gameId}/update")] HttpRequestData req, string gameId)
    {
        try
        {
            _logger.LogInformation("Processing a request to update user response");

            string? validUserResponse = req.Query["validUserResponse"];

            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(validUserResponse))
            {
                throw new Exception("Error: Invalid request parameters.");
            }


             bool validUserResponseFlag = bool.Parse(validUserResponse);

            await _gameService.UpdateUserResponse(gameId, validUserResponseFlag);

            return new ContentResult
            {
                Content = "Updated Successfully",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult(ex);
        }
    }

    [Function("updateGameCompleted")]
    public async Task<IActionResult> UpdateGameCompletionInformation(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "game/{gameId}/complete")] HttpRequestData req, string gameId)
    {
        try
        {
            _logger.LogInformation("Processing a request to update game completion");

            string? accuracy = req.Query["accuracy"];
            string? completion = req.Query["completion"];

            if (string.IsNullOrEmpty(accuracy) || string.IsNullOrEmpty(completion))
            {
                throw new Exception("Error: Invalid request parameters.");
            }


            await _gameService.UpdateGameCompleted(gameId, accuracy, completion);

            return new ContentResult
            {
                Content = "Updated Successfully",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult(ex);
        }
    }

}