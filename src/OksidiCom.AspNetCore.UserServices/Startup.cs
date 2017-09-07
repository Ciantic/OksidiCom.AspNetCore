using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCore.Common.Db;
using OksidiCom.AspNetCore.UserServices.Db;

namespace OksidiCom.AspNetCore.UserServices
{
    /// <summary>
    /// Startup for internally testing the UserServices
    /// </summary>
    internal class Startup
    {
        private IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUserServices(o =>
            {
                o.Configure(Configuration);
                o.AddDbContext((serviceProvider, dbContextOptionsBuilder) =>
                {
                    serviceProvider.GetService<DbContextConnectionConfiguration>().Configure(dbContextOptionsBuilder);
                });
            });
            services.AddMvc();

            if (Environment.IsDevelopment())
            {
                services.AddScoped(t => new DbContextConnectionConfiguration(
                    new SqliteConnection(Configuration.GetConnectionString("Default")),
                    (c, o) => o.UseSqlite(c)
                ));
                services.AddTransient<IInitDb, InitDbDev>();
            }
        }

        public void Configure(IApplicationBuilder app, IInitDb initDb)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseUserServices();
            app.UseMvc();

            initDb.InitAsync().Wait();
        }
    }
}
