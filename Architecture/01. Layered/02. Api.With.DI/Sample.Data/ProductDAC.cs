// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Entities;
using System;
using System.Collections.Generic;

namespace Sample.Data
{
    public class ProductDAC
    {
        private readonly ILogger<ProductDAC> _logger;
        private readonly IConfiguration _config;

        public ProductDAC(ILogger<ProductDAC> logger, IConfiguration config)
        {
            this._logger = logger;
            this._config = config;
        }

        public List<Product> Select()
        {
            // You can now read the connection string from the injected config.
            var connectionString = _config.GetConnectionString("Database");
            
            _logger.LogInformation($"Connection String: {connectionString}");

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
