// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Sample.Business;
using Sample.Entities;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Sample.Service.Controllers
{
    public class ProductController : ApiController
    {
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            // Calls Business component.
            var bc = new ProductComponent();
            return bc.ListProducts();
        }

    }
}
