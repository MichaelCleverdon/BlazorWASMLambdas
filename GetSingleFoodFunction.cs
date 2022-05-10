using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FoodLibrary;
using FoodLibrary.Objects;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetSingleFoodItemLambda;

public class GetSingleFoodFunction
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        string email = request.RequestContext.Authorizer.Jwt.Claims["Email"];
        if (email == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "Please validate your email and ensure that you are logged in"
            };
        }
        string connectionString = "Server=blazor-demo-db.cndiwnkoxgqw.us-west-2.rds.amazonaws.com;database=BlazorDemo;user=admin;password=PasswordForBlazorDemo321";
        FoodRepository repo = new FoodRepository(connectionString);
        FoodObject retFood = new();
        request.QueryStringParameters.TryGetValue("name", out string foodItemName);
        if(foodItemName == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "Please supply a valid food item name"
            };
        }
        try
        {
            retFood = repo.GetFoodByName(email, foodItemName).Result!;
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.Message);
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 400,
                Body = "We were unable to retrieve the items. Please try again in a little while"
            };
        }
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = JsonSerializer.Serialize(retFood, options),
            StatusCode = 200

        };
    }
}
