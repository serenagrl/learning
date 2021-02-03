// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Business;
using Sample.Entities;
using System.Collections.Generic;

namespace Sample.Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ProductComponent _productComponent;

        public ProductController(ILogger<ProductController> logger,
            ProductComponent productComponent)
        {
            _logger = logger;
            this._productComponent = productComponent;
        }

        [HttpGet]
        public IEnumerable<Product> Get()
        {
            // Uses the injected Business Component.
            return _productComponent.ListProducts();
        }
    }
}
