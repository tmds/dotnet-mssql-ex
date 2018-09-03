## Adding SQL Server to OpenShift

There is no pre-built SQL Server image. We can make OpenShift build this image for us. This requires OpenShift to be configured with subscription credentials and the Docker strategy allowed. 

Use the following command to build the SQL Server image in OpenShift:

```
$ oc create -f https://raw.githubusercontent.com/tmds/dotnet-mssql-ex/master/openshift/imagestreams.json
```

**note**: this image is not automatically rebuild for security fixes. To trigger a build execute: `oc start-build mssql2017`.

To facilitate setting up SQL Server instances, you can import this template:

```
$ oc create -f https://raw.githubusercontent.com/tmds/dotnet-mssql-ex/master/openshift/template.json
```

## Deploying SQL Server

Using the template:

```
$ oc new-app --template=mssql -p NAME=mssql1 -p ACCEPT_EULA=Y -p NAMESPACE=`oc project -q`
```

To see all parameters of the template, you can use `oc process --parameters mssql`.

```
$ oc process --parameters mssql
NAME                DESCRIPTION                                                                  GENERATOR           VALUE
NAME                The name assigned to all of the frontend objects defined in this template.                       mssql
MEMORY_LIMIT        Maximum amount of memory the container can use.                                                  512Mi
IMAGE               The SQL Server image tag.                                                                        mssql:2017
NAMESPACE           The OpenShift namespace where the SQL Server image resides.                                      openshift
MSSQL_SA_PASSWORD                                                                                expression          [a-zA-Z0-9]{8}
ACCEPT_EULA         'Y' to accept the EULA (https://go.microsoft.com/fwlink/?linkid=857698).                         
VOLUME_CAPACITY     Volume space available for data, e.g. 512Mi, 2Gi                                                 512Mi
```

## Using the server from .NET Core

The following code shows how to use the SQL server from ASP.NET Core.
The `DB_PROVIDER` variable is used to switch between SQL server and InMemory.
The `MSSQL_SERVER`, `MSSQL_SA_PASSWORD` variables are used to build the connection string.

```cs
        enum DbProvider
        {
            Mssql,
            Memory
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
                    break;
                default:
                    throw new Exception($"Unknown db provider: {dbProvider}");
            }

            services.AddMvc();
        }

        private static bool IsOpenShift => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENSHIFT_BUILD_NAME"));
```

Database creation and migrations are triggered from `Configure`:

```cs
        public void Configure(IApplicationBuilder app)
        {
            UpdateDatabase(app);

            app.UseMvc();
        }

        private static void UpdateDatabase(IApplicationBuilder app)
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
```

Migrations can be created using the `dotnet ef migrations` command. For example `dotnet ef migrations add Initial` is used to create the initial migration.

A sample application can be deployed using the following template:

```
$ oc create -f https://raw.githubusercontent.com/tmds/dotnet-mssql-ex/master/openshift/template.json
```

You can instantiate the template as follows:

```sh
$ oc new-app --template=mssql -p NAME=dotnet-mssql -p MSSQL_SERVER=mssql1 -p MSSQL_SECRET_NAME=mssql1-secret # -p NAMESPACE=`oc project -q`
```

Add the `NAMESPACE` parameter when the .NET Core imagestreams are installed in the current project.