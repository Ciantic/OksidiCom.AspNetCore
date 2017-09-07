using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCore.UserServices;
using Microsoft.Extensions.Configuration;
using OksidiCom.AspNetCore.UserServices.Db;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCore.Common.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SampleApi.Db;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.SqlClient;
using System;

namespace SampleApi
{
    internal class Startup
    {
        private IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        private void SharedConnection(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            serviceProvider.GetService<DbContextConnectionConfiguration>().Configure(dbContextOptionsBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUserServices(opts =>
            {
                // Normally you would add DbContext connections settings like this
                // opts.AddDbContext(o => {
                //     o.UseSqlite(Configuration.GetConnectionString("Default"));
                // });

                // If you want to share the connection between instances, you do it like this
                opts.AddDbContext(SharedConnection);

                // Read the configurations from appsettings.json UserServices property
                opts.Configure(Configuration);
            });
            services.AddMvc();
            services.AddDbContext<BusinessDbContext>(SharedConnection);

            if (Environment.IsDevelopment())
            {
                services.AddScoped(t => new DbContextConnectionConfiguration(
                    new SqliteConnection(Configuration.GetConnectionString("Default")),
                    (c, o) => o.UseSqlite(c)
                ));
                services.AddTransient<IInitDb, InitDbDev>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IInitDb initDb)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            // Remember to call UseUserServices before UseMvc
            app.UseUserServices();
            app.UseMvc();

            initDb.CreateAsync().Wait();
            initDb.PopulateAsync().Wait();
        }
    }
}
