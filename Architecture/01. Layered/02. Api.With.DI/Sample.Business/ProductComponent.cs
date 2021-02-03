// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Data;
using Sample.Entities;
using System;
using System.Collections.Generic;

namespace Sample.Business
{
    public class ProductComponent
    {
        private readonly ILogger<ProductComponent> _logger;
        private readonly IConfiguration _config;

        private readonly ProductDAC _productDAC;

        public ProductComponent(ILogger<ProductComponent> logger,
            IConfiguration config, ProductDAC productDAC 
            )
        {
            this._logger = logger;
            this._config = config;
            this._productDAC = productDAC;
        }

        public List<Product> ListProducts()
        {
            //Note: This is not how we would usually do things but for illustration purposes, 
            //      we will just create an excuse to do some business logic in this sample.

            // This is how you can read the configuration in the business components now.
            decimal tax = Convert.ToDecimal(_config.GetSection("Business:TaxPercentage").Value) / 100;

            _logger.LogInformation($"Tax Percentage: {tax}");

            // Uses the injected Data Access Component
            var results = _productDAC.Select();

            // Warning: Don't do this if you value performance (and your job).
            //          This is just a sorry excuse to show some business logic.
            results.ForEach(p => p.PriceAfterTax = Math.Round(p.Price + (p.Price * tax), 2));

            return results;
        }
    }
}
