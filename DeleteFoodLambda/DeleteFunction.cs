using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FoodLibrary;
using FoodLibrary.Objects;
using System.Text.Json;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeleteFoodLambda;

public class DeleteFunction
{
    
    /// <summary>
    /// A simple function that deletes a specific food item based on name and email. Name from request, email from JWT token
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if (request.Body == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "Please provide some data to the server"
            };
        }
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        FoodObject foodObject = JsonSerializer.Deserialize<FoodObject>(request.Body, options)!;
        string email = request.RequestContext.Authorizer.Jwt.Claims["http://example.com/email"];
        if (email == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "Please validate your email and ensure that you are logged in"
            };
        }

        foodObject.Username = email;
        FoodRepository repo = new FoodRepository();
        FoodObject retFood = new FoodObject();
        context.Logger.Log($"Username: {email}, Name: {foodObject.Name}, Quantity: {foodObject.Quantity}");
        try
        {
            retFood = repo.DeleteFood(foodObject).Result!;
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.Message);
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "The item was unable to be deleted. Please try again in a little while"
            };
        }
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = JsonSerializer.Serialize<FoodObject>(retFood, options),
            StatusCode = 200

        };
    }
}
