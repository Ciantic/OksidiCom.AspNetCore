using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCoreServices.UserServices;
using Microsoft.Extensions.Configuration;
using OksidiCom.AspNetCoreServices.UserServices.Db;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCoreServices.Common.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SampleApi.Db;


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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUserServices(Configuration);
            services.AddMvc();
            services.AddDbContext<BusinessDbContext>((s, o) =>
            {
                s.GetService<IDbContextConnectionConfiguration>().Configure(o);
            });

            if (Environment.IsDevelopment())
            {
                services.AddScoped<IDbContextConnectionConfiguration>(t => new DbContextConnectionConfiguration(
                    new SqliteConnection(Configuration.GetConnectionString("Default"))
                ));
                services.AddTransient<IInitDb, InitDbDev>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IInitDb initDb, UserServiceContext us, BusinessDbContext bs)
        {
            
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseUserServices();
            app.UseMvc();

            var a = us.Database.GetDbConnection().GetHashCode();
            var b = bs.Database.GetDbConnection().GetHashCode();

            initDb.InitAsync().Wait();
            initDb.PopulateAsync().Wait();
        }
    }
}
