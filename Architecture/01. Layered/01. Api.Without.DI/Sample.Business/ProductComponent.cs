// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Sample.Data;
using Sample.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Sample.Business
{
    public class ProductComponent
    {
        public List<Product> ListProducts()
        {
            //Note: This is not how we would usually do things but for illustration purposes, 
            //      we will just create an excuse to do some business logic in this sample.

            // This is usually how the configuration is read in the business components.
            decimal tax = Convert.ToDecimal(ConfigurationManager.AppSettings["TaxPercentage"]) / 100;

            // Calls Data Access Component
            var dac = new ProductDAC();
            var results = dac.Select();

            // Warning: Don't do this if you value performance (and your job).
            //          This is just a sorry excuse to show some business logic.
            results.ForEach(p => p.PriceAfterTax = Math.Round(p.Price + (p.Price * tax), 2));

            return results;
        }
    }
}
