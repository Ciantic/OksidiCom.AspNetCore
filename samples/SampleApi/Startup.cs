using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCoreServices.UserServices;
using Microsoft.Extensions.Configuration;
using OksidiCom.AspNetCoreServices.UserServices.Db;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCoreServices.Common.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SampleApi.Models;
using SampleApi.Db;
using OksidiCom.AspNetCoreServices.UserServices.Models;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Models;
using OpenIddict.Core;

namespace SampleApi.Db
{
    public class BusinessDbContext : DbContext
    {
        public DbSet<BusinessThing> BusinessThings { get; set; }

        public BusinessDbContext(DbContextOptions<BusinessDbContext> context) : base(context)
        {
            
        }
    }

    internal class InitDbDev : IInitDb
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UserServiceContext userServiceContext;
        private readonly BusinessDbContext businessDbContext;

        public InitDbDev(UserManager<ApplicationUser> userManager, UserServiceContext userServiceContext, BusinessDbContext businessDbContext)
        {
            this.userManager = userManager;
            this.userServiceContext = userServiceContext;
            this.businessDbContext = businessDbContext;
        }

        public async Task InitAsync()
        {
            var userServiceDbCreator = userServiceContext.GetService<IRelationalDatabaseCreator>();
            await userServiceDbCreator.DeleteAsync();
            await userServiceDbCreator.CreateAsync();
            await userServiceDbCreator.CreateTablesAsync();

            var businessServiceDbCreator = businessDbContext.GetService<IRelationalDatabaseCreator>();
            await businessServiceDbCreator.CreateTablesAsync();
        }

        private async Task<List<ApplicationUser>> CreateUsers()
        {
            var newUsers = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "test@example.com",
                    Email = "test@example.com",
                },
                new ApplicationUser()
                {
                    UserName = "test2@example.com",
                    Email = "test2@example.com"
                }
            };
            await userManager.CreateAsync(newUsers[0], "!Test1");
            await userManager.CreateAsync(newUsers[1], "!Test1");
            //await userServiceContext.Users.AddRangeAsync(newUsers);
            return newUsers;
        }

        private async Task<List<OpenIddictApplication>> CreateOpenIdClients()
        {
            var newClients = new List<OpenIddictApplication>()
            {
                new OpenIddictApplication()
                {
                    ClientId = "example-client",
                    ClientSecret = "example-secret",
                    DisplayName = "Example Client",
                    RedirectUri = "http://localhost:5002/o2c.html",
                    LogoutRedirectUri = "http://localhost:5002/o2c-logout.html",
                    Type = OpenIddictConstants.ClientTypes.Public
                }
            };

            userServiceContext.AddRange(newClients);
            await userServiceContext.SaveChangesAsync();

            return newClients;
        }

        public async Task PopulateAsync()
        {
            var newUsers = await CreateUsers();

            await CreateOpenIdClients();

            var businessThings = new List<BusinessThing>()
            {
                new BusinessThing()
                {
                    Name = "Acme Corp",
                    OwnerUserId = newUsers[0].Id
                },
                new BusinessThing()
                {
                    Name = "Corporation Inc",
                    OwnerUserId = newUsers[1].Id
                }
            };
            await businessDbContext.BusinessThings.AddRangeAsync(businessThings);
        }
    }

    internal interface IInitDb
    {
        Task InitAsync();
        Task PopulateAsync();
    }
}

namespace SampleApi.Models
{
    public class BusinessThing
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public Guid OwnerUserId { get; set; } = Guid.Empty;

        // It's not possible to reference `ApplicationUser` 
        // as foreign key since it resides in different DbContext
    }
}


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
