using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FoodLibrary;
using FoodLibrary.Objects;
using System.Text.Json;
using System.Text.Json.Serialization;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateOrUpdateFoodLambda;

public class CreateOrUpdateFunction
{
    
    /// <summary>
    /// A simple function that CreatesOrUpdates a FoodObject based on the input
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if(request.Body == null)
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
        if(email == null)
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
        try
        {
            retFood = repo.CreateOrUpdateFood(foodObject).Result!;
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.Message);
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "The item was unable to be created. Please try again in a little while"
            };
        }
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = JsonSerializer.Serialize<FoodObject>(retFood, options),
            StatusCode = 200

        };
    }
}
