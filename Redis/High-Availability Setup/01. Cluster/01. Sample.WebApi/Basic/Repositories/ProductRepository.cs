using Cluster.TestApp.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cluster.TestApp.Repositories
{
    // Just a simple repository to query data.
    public class ProductRepository
    {
        private string _connectionString;

        public ProductRepository(IConfiguration configuration) => 
            _connectionString = configuration.GetConnectionString("Database");

        public List<Product> Select()
        {
            const string SQL_STATEMENT = "SELECT * FROM Products";

            using var cn = new SqlConnection(_connectionString);
            var result = cn.Query<Product>(SQL_STATEMENT).ToList();
            cn.Close(); // Play safe.

            return result;
        }

        public Product Select(long id)
        {
            const string SQL_STATEMENT = "SELECT * FROM Products WHERE Id=@Id";

            using var cn = new SqlConnection(_connectionString);
            var result = cn.QuerySingleOrDefault<Product>(SQL_STATEMENT, new { Id = id });
            cn.Close(); // Play safe.

            return result;
        }
    }
}
