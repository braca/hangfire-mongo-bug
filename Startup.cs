using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire_Mongo_Bug.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Hangfire_Mongo_Bug
{
    public static class QueueNames
    {
        public const string Long = "long";
        public const string Short = "short";
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire_Mongo_Bug", Version = "v1" });
            });
            services.AddTransient<ITestService, TestService>();

            var mongoConnectionString = "mongodb://mongo:27017/SVC_HANGFIRE";
            var storageOptions = new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new NoneMongoBackupStrategy(),
                },
                CheckConnection = true,
                ConnectionCheckTimeout = TimeSpan.FromSeconds(1),
                InvisibilityTimeout = TimeSpan.FromMinutes(1),
                JobExpirationCheckInterval = TimeSpan.FromMinutes(5),
                Prefix = "test",
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll
            };

            services
                .AddHangfire(configuration =>
                {
                    configuration
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseFilter(new AutomaticRetryAttribute { Attempts = 10 })
                        .UseMongoStorage(mongoConnectionString, storageOptions);
                });

            services.AddHangfireServer(
                (serviceProvider, serverOptions) =>
                {
                    serverOptions.Queues = new string[] { QueueNames.Long, QueueNames.Short };
                    serverOptions.CancellationCheckInterval = TimeSpan.FromSeconds(5);
                    serverOptions.ServerName = $"{Environment.MachineName}:Test";
                    serverOptions.WorkerCount = 2;
                    serverOptions.ShutdownTimeout = TimeSpan.FromSeconds(15);
                    serverOptions.SchedulePollingInterval = TimeSpan.FromSeconds(15);
                    serverOptions.ServerCheckInterval = TimeSpan.FromMinutes(5);
                    serverOptions.ServerTimeout = TimeSpan.FromMinutes(5);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire_Mongo_Bug v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
