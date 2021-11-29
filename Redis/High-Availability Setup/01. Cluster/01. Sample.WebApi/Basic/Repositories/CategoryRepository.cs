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
    public class CategoryRepository
    {
        private string _connectionString;

        public CategoryRepository(IConfiguration configuration) =>
            _connectionString = configuration.GetConnectionString("Database");

        public List<Category> Select()
        {
            const string SQL_STATEMENT = "SELECT * FROM Categories";

            using var cn = new SqlConnection(_connectionString);
            var result = cn.Query<Category>(SQL_STATEMENT).ToList();
            cn.Close(); // Play safe.

            return result;
        }

        public Category Select(long id)
        {
            const string SQL_STATEMENT = "SELECT * FROM Categories WHERE Id=@Id";

            using var cn = new SqlConnection(_connectionString);
            var result = cn.QuerySingleOrDefault<Category>(SQL_STATEMENT, new { Id = id });
            cn.Close(); // Play safe.

            return result;
        }
    }
}
