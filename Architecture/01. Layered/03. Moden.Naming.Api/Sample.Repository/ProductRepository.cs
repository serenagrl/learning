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
    public class ProductRepository
    {
        // Note: Usually there is only 1 type of storage for your repository but if you have
        //       multiple types of storage to support the same repository, then it is best
        //       to create an interface for your repository i.e. IProductRepository.

        private readonly ILogger<ProductRepository> _logger;
        private readonly IConfiguration _config;

        public ProductRepository(ILogger<ProductRepository> logger, IConfiguration config)
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
