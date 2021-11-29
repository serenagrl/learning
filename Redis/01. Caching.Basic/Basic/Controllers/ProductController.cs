// ==================================================================================
// Developed by Serena Yeoh - November 2021
// Disclaimer: 
//   I wrote this for my self-learning and some parts may not be that accurate.
//   So follow at your own risks ;p
// ==================================================================================
using Basic.Models;
using Basic.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Basic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IDistributedCache _cache;
        private readonly ProductRepository _repository;

        public ProductController(ILogger<ProductController> logger, 
            IDistributedCache cache, ProductRepository productRepository)
        {
            this._logger = logger;
            this._cache = cache;
            this._repository = productRepository;
        }

        [HttpGet]
        public ActionResult Get()
        {
            // WARNING! Becareful when caching large tables. 
            // There is also the performance of deserialization to consider.
            string key = $"Products";

            List<Product> products = GetFromCache<List<Product>>(key, () => _repository.Select(),
                absoluteExpiration: DateTime.Now.AddMinutes(1));

            if (products is null)
                return NotFound();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult Get(long id)
        {
            // Note: Trying out row caching.

            string key = $"Product.{id}";
           
            Product product = GetFromCache<Product>(key, () => _repository.Select(id),
                slidingExpiration: TimeSpan.FromSeconds(30));

            if (product is null)
                return NotFound();

            return Ok(product);
        }

        private T GetFromCache<T>(string key, Func<T> queryResult,
            DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            T result = default(T);

            // Read from cache.
            string json = _cache.GetString(key);

            if (!string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("Loading data from cache at {time}", DateTime.Now);

                // Deserialize the json back to objects.
                result = JsonSerializer.Deserialize<T>(json);
            }
            else
            {
                _logger.LogWarning("Loading data from database at {time}", DateTime.Now);

                // Query result from DB using the supplied delegate.
                result = queryResult();

                if (result is not null)
                {
                    // Serialize it.
                    json = JsonSerializer.Serialize(result);

                    // Set expiration.
                    var options = new DistributedCacheEntryOptions();
                    options.AbsoluteExpiration = absoluteExpiration;
                    options.SlidingExpiration = slidingExpiration;

                    // Cache the data.
                    _cache.SetString(key, json, options);
                }
            }

            return result;
        }
    }
}
