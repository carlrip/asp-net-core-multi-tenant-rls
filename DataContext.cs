using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIWithRSL
{
    public class DataContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Product> Products { get; set; }

        private SqlConnection _connection;

        private readonly string _connectionString;
        private readonly Guid _apiKey;

        public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(options)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];

            if (httpContextAccessor.HttpContext != null)
            {
                _apiKey = (Guid)httpContextAccessor.HttpContext.Items["APIKey"];
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>().ToTable("Tenant");
            modelBuilder.Entity<Product>().ToTable("Product");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _connection = new SqlConnection(_connectionString);
            _connection.StateChange += (sender, e) =>
            {
                if (e.CurrentState == ConnectionState.Open)
                {
                    var tenantId = GetTenantId();
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = @"exec sp_set_session_context @key=N'TenantId', @value=@TenantId";
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.ExecuteNonQuery();
                }
            };

            optionsBuilder.UseSqlServer(_connection);

            base.OnConfiguring(optionsBuilder);
        }

        public Guid GetTenantId()
        {
            var tenant = Tenants.Where(t => t.APIKey == _apiKey).FirstOrDefault();
            if (tenant == null)
            {
                throw new Exception("Unknown tenant");
            }
            return tenant.TenantId;
        }
    }
}
