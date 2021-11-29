using Sentinel.TestApp.Models;
using Sentinel.TestApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Sentinel.TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IDistributedCache _cache;
        private readonly CategoryRepository _repository;

        public CategoryController(ILogger<CategoryController> logger, 
            IDistributedCache cache, CategoryRepository categoryRepository)
        {
            this._logger = logger;
            this._cache = cache;
            this._repository = categoryRepository;
        }

        [HttpGet]
        public ActionResult Get()
        {
            // WARNING! Becareful when caching large tables. 
            // There is also the performance of deserialization to consider.
            string key = $"Categories";

            List<Category> category = GetFromCache<List<Category>>(key, () => _repository.Select(),
                absoluteExpiration: DateTime.Now.AddMinutes(10));

            if (category is null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet("{id}")]
        public ActionResult Get(long id)
        {
            // Note: Trying out row caching.

            string key = $"Category.{id}";
           
            Category category = GetFromCache<Category>(key, () => _repository.Select(id),
                slidingExpiration: TimeSpan.FromMinutes(10));

            if (category is null)
                return NotFound();

            return Ok(category);
        }

        private T GetFromCache<T>(string key, Func<T> queryResult,
            DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            T result = default(T);

            // Read from cache.
            string json = _cache.GetString(key);

            if (!string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("Loading data from CACHE at {time}", DateTime.Now);

                // Deserialize the json back to objects.
                result = JsonSerializer.Deserialize<T>(json);
            }
            else
            {
                _logger.LogWarning("Loading data from DATABASE at {time}", DateTime.Now);

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
