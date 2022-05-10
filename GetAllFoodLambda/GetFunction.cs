using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FoodLibrary;
using FoodLibrary.Objects;
using System.Text.Json;
using System.Text.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetAllFoodLambda;

public class GetFunction
{
    
    /// <summary>
    /// A simple function that GETs all the food for a specific person based on the request's email from the JWT token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Console.WriteLine(string.Join(",", request.RequestContext.Authorizer.Jwt.Claims));
        context.Logger.LogLine(string.Join(",", request.RequestContext.Authorizer.Jwt.Claims));
        string email = request.RequestContext.Authorizer.Jwt.Claims["http://example.com/email"];
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
        List<FoodObject> retFood = new();
        try
        {
            retFood = repo.GetAllFoodByUsername(email).Result!;
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
