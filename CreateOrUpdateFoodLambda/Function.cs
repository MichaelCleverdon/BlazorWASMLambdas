using Amazon.Lambda.Core;
using FoodLibrary;
using FoodLibrary.Objects;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CreateOrUpdateFoodLambda;

public class Function
{
    
    /// <summary>
    /// A simple function that CreatesOrUpdates a FoodObject based on the input
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public FoodObject FunctionHandler(FoodObject input, ILambdaContext context)
    {
        string connectionString = "Server=blazor-demo-db.cndiwnkoxgqw.us-west-2.rds.amazonaws.com;database=BlazorDemo;user=admin;password=PasswordForBlazorDemo321";
        FoodRepository repo = new FoodRepository(connectionString);
        FoodObject fo = new FoodObject();
        try
        {
            fo = repo.CreateOrUpdateFood(input).Result!;
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.Message);
        }
        return fo;
    }
}
