// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Sample.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Sample.Data
{
    public class ProductDAC
    {
        public List<Product> Select()
        {
            // Usually this is how the connection string is read inside the DAC.
            var connectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;

            // Note: This is where you would generally have your SQL statement and then execute
            //       queries to the database to return data but in this sample, we will just fake
            //       the process by returning a hardcoded list.
            var results = new List<Product>()
            {
                new Product() { Id=1001, Name="Teddy Bear Plush", Price=89.90m },
                new Product() { Id=1002, Name="Tabby Cat Plush", Price=59.90m },
                new Product() { Id=1003, Name="Unicorn Plush", Price=129.90m },
            };

            return results;
        }
    }
}
