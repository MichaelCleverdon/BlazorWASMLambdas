using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using FoodLibrary.Objects;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoodLibrary
{
    public class FoodRepository
    {
        private AmazonDynamoDBClient dynamoDBClient;
        public FoodRepository()
        {
            AmazonDynamoDBConfig config = new AmazonDynamoDBConfig()
            {   
                RegionEndpoint = Amazon.RegionEndpoint.USWest2
            };
            dynamoDBClient = new AmazonDynamoDBClient(config);
        }

        /// <summary>
        /// Returns all of the food attached to a specific user
        /// </summary>
        /// <param name="username">The email address of the user. Gotten from the JWT token through API gateway</param>
        /// <returns>A list of FoodObjects</returns>
        public async Task<List<FoodObject>> GetAllFoodByUsername(string username)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            List<FoodObject> retFood = new();
            var command = context.QueryAsync<FoodObject>(username);
            while (!command.IsDone)
            {
                retFood.AddRange(await command.GetNextSetAsync());
            }
            return retFood;
        }

        /// <summary>
        /// Gets a single FoodObject with specified username and itemName of the row
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <returns>The FoodObject if it exists or an empty FoodObject if nothing was found</returns>
        public async Task<FoodObject> GetFoodByName(string username, string name)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            List<FoodObject> retFood = new();

            var food = await context.LoadAsync<FoodObject>(username, name);

            return food;
        }

        /// <summary>
        /// Create a new food row if the food does not exist or update the quantity on key match of (username + name)
        /// </summary>
        /// <param name="food"></param>
        /// <returns>The food if something happened. If there was an error, it will throw an exception</returns>
        /// <exception cref="Exception">Returns the Error message</exception>
        public async Task<FoodObject?> CreateOrUpdateFood(FoodObject food)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient, new DynamoDBContextConfig() { ConsistentRead = true });
            //Perform an update by deleting the old record and inserting the new one
            //due to how the sort key works
            if(await context.LoadAsync(food) != null)
            {
                await DeleteFood(food);
            }
            await context.SaveAsync(food);
            return food;
        }

        /// <summary>
        /// Will try to delete the passed in FoodObject from the database
        /// </summary>
        /// <param name="food"></param>
        /// <returns>The FoodObject that was deleted if something was deleted</returns>
        /// <exception cref="Exception">If nothing was deleted (FoodObject doesn't exist or something), throw an exception</exception>
        public async Task<FoodObject> DeleteFood(FoodObject food)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient, new DynamoDBContextConfig() { ConsistentRead = true});
            await context.DeleteAsync<FoodObject>(food.Username, food.Name);
            return food;
        }
    }
}