using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorPagesContacts.Data;

namespace RazorPagesContacts
{
    enum DbProvider
    {
        Mssql,
        Memory
    }

    public class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }
        private bool _migrateDatabase = true;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            DbProvider? dbProvider = Configuration.GetValue<DbProvider?>("DB_PROVIDER");

            if (dbProvider == null && !IsOpenShift)
            {
                dbProvider = DbProvider.Memory;
            }

            switch (dbProvider)
            {
                case DbProvider.Mssql:
                    string server = Configuration["MSSQL_SERVER"] ?? "localhost";
                    string password = Configuration["MSSQL_SA_PASSWORD"];
                    string user = "sa";
                    string dbName = "myContacts";
                    string connectionString = $@"Server={server};Database={dbName};User Id={user};Password={password};";

                    Logger.LogInformation($"Using SQL Server: {server}");
                    services.AddDbContext<AppDbContext>(options =>
                                options.UseSqlServer(connectionString));
                    break;
                case DbProvider.Memory:
                    Logger.LogInformation("Using InMemory database");
                    services.AddDbContext<AppDbContext>(options =>
                              options.UseInMemoryDatabase("name"));
                    _migrateDatabase = false;
                    break;
                default:
                    throw new Exception($"Unknown db provider: {dbProvider}");
            }

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_migrateDatabase)
            {
                MigrateDatabase(app);
            }

            app.UseMvc();
        }

        private static void MigrateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<AppDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

        private static bool IsOpenShift => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENSHIFT_BUILD_NAME"));
    }
}