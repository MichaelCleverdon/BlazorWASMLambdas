using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodLibrary.Objects
{
    public class FoodObject
    {
        public FoodObject()
        {
            Username = "";
            Name = "";
        }

        public FoodObject(string username, string name, int quantity)
        {
            Username = username;
            Name = name;
            Quantity = quantity;
        }

        public string Username { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
