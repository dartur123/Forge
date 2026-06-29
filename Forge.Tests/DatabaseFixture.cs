using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace Forge.Tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
                                                            .WithDatabase("forge_test")
                                                            .WithUsername("postgres")
                                                            .WithPassword("postgres")
                                                            .Build();

        public ForgeDbContext DbContext { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<ForgeDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            DbContext = new ForgeDbContext(options);
            await DbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _container.DisposeAsync();
        }
    }
}
