using FoodLibrary.Objects;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoodLibrary
{
    public class FoodRepository
    {
        private string _connectionString;
        public FoodRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Returns all of the food attached to a specific user
        /// </summary>
        /// <param name="username">The email address of the user. Gotten from the JWT token through API gateway</param>
        /// <returns>A list of FoodObjects</returns>
        public async Task<List<FoodObject>> GetAllFoodByUsername(string username)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT USERNAME, NAME, QUANTITY FROM Food WHERE Username=@p";
                    cmd.Parameters.AddWithValue("p", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        List<FoodObject> retList = new List<FoodObject>();
                        while(await reader.ReadAsync())
                        {
                            retList.Add(new FoodObject(reader.GetString(reader.GetOrdinal("Username")), reader.GetString(reader.GetOrdinal("Name")), reader.GetInt32(reader.GetOrdinal("Quantity"))));
                        }
                        //If nothing is found, it'll just be an empty list, which should work for the UI implementation
                        return retList;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a single FoodObject with specified username and itemName of the row
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <returns>The FoodObject if it exists or an empty FoodObject if nothing was found</returns>
        public async Task<FoodObject> GetFoodByName(string username, string name)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT USERNAME, NAME, QUANTITY FROM Food WHERE Username=@p AND Name=@q";
                    cmd.Parameters.AddWithValue("p", username);
                    cmd.Parameters.AddWithValue("q", name);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new FoodObject(reader.GetString(reader.GetOrdinal("Username")), reader.GetString(reader.GetOrdinal("Name")), reader.GetInt32(reader.GetOrdinal("Quantity")));
                        }
                        else
                        {
                            //No data found, return empty FoodObject
                            return new FoodObject();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new food row if the food does not exist or update the quantity on key match of (username + name)
        /// </summary>
        /// <param name="food"></param>
        /// <returns>The food if something happened. If there was an error, it will throw an exception</returns>
        /// <exception cref="Exception">Returns the Error message</exception>
        public async Task<FoodObject?> CreateOrUpdateFood(FoodObject food)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "INSERT INTO Food (username, name, quantity) VALUES(@p, @q, @r) ON DUPLICATE KEY UPDATE quantity=@r";
                    cmd.Parameters.AddWithValue("p", food.Username);
                    cmd.Parameters.AddWithValue("q", food.Name);
                    cmd.Parameters.AddWithValue("r", food.Quantity);

                    if(await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        return food;
                    }
                    else
                    {
                        throw new Exception("Unable to update the food item, please try again in just a second");
                    }
                }
            }
        }

        /// <summary>
        /// Will try to delete the passed in FoodObject from the database
        /// </summary>
        /// <param name="food"></param>
        /// <returns>The FoodObject that was deleted if something was deleted</returns>
        /// <exception cref="Exception">If nothing was deleted (FoodObject doesn't exist or something), throw an exception</exception>
        public async Task<FoodObject> DeleteFood(FoodObject food)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "DELETE FROM Food WHERE username=@p AND name=@q AND quantity=@r";
                    cmd.Parameters.AddWithValue("p", food.Username);
                    cmd.Parameters.AddWithValue("q", food.Name);
                    cmd.Parameters.AddWithValue("r", food.Quantity);

                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        return food;
                    }
                    else
                    {
                        throw new Exception("Unable to delete the food item, please try again in just a second");
                    }
                }
            }
        }
    }
}