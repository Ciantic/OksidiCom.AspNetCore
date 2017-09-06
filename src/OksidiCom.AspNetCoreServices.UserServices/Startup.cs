using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityServer4.Models;
using IdentityServer4;
using IdentityServer4.Test;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCoreServices.UserServices.Models;
using Microsoft.EntityFrameworkCore;
using OksidiCom.AspNetCoreServices.UserServices.Db;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using OksidiCom.AspNetCoreServices.Common.Db;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Internal;
using OpenIddict.Models;
using OpenIddict.Core;
using AspNet.Security.OpenIdConnect.Primitives;

namespace OksidiCom.AspNetCoreServices.UserServices.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
    }

    public class ApplicationRole : IdentityRole<Guid>
    {

    }
}

namespace OksidiCom.AspNetCoreServices.UserServices
{
    public class GoogleConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class AuthConfiguration
    {
        public GoogleConfiguration Google { get; set; }
    }

    public static class IApplicationBuilderExtension
    {
        public static void UseUserServices(this IApplicationBuilder app)
        {
            // TODO: Verify that UseMvc has NOT been called yet

            app.UseAuthentication();
        }
    }

    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// User services middleware
        /// 
        /// Note: This must be called before `UseMvc()`
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddUserServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Ensure that AddMvcCore/AddMvc has not been called before registering user services
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                if (service.ServiceType == typeof(MvcRouteHandler))
                {
                    throw new InvalidOperationException("AddUserServices() must be called before AddMvc()");
                }
            }

            // Add views provided in this assembly.     
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(Startup).GetTypeInfo().Assembly));
            });

            services.AddDbContext<UserServiceContext>();
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<UserServiceContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            var auth = services.AddAuthentication()
                //.AddOAuthValidation()
                .AddJwtBearer(o =>
                {
                    o.Audience = "http://localhost:5002/";
                    o.Authority = "http://localhost:5002/";
                });/*.AddCookie(o =>
                {
                    o.LoginPath = "/Account/Login";
                    o.LogoutPath = "/Account/Logout";
                });*/
            

            var authConfig = new AuthConfiguration();
            configuration.GetSection("Auth").Bind(authConfig);

            if (authConfig?.Google != null)
            {
                auth.AddGoogle(o =>
                {
                    o.ClientId = authConfig.Google.ClientId;
                    o.ClientSecret = authConfig.Google.ClientSecret;
                    o.SignInScheme = "Identity.External";
                });
            }

            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<UserServiceContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the token endpoint (required to use the password flow).
                //options.EnableTokenEndpoint("/connect/token");

                options.AllowImplicitFlow()
                    .EnableAuthorizationEndpoint("/connect/authorize")
                    .EnableTokenEndpoint("/connect/token")
                    .AllowRefreshTokenFlow();

                // Allow client applications to use the grant_type=password flow.
                //options.AllowPasswordFlow();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();
                options.AddDevelopmentSigningCertificate();
            });

 
        }
    }
}

namespace OksidiCom.AspNetCoreServices.UserServices.Db
{
    public class UserServiceContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly IDbContextConnectionConfiguration connectionConfiguration;

        public UserServiceContext(IDbContextConnectionConfiguration connectionConfiguration) : base()
        {
            this.connectionConfiguration = connectionConfiguration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            connectionConfiguration.Configure(optionsBuilder);
            optionsBuilder.UseOpenIddict();
        }
    }

    internal class InitDbDev : IInitDb
    {
        private readonly UserServiceContext userServiceContext;
        private readonly UserManager<ApplicationUser> userManager;

        public InitDbDev(UserServiceContext userServiceContext, UserManager<ApplicationUser> userManager)
        {
            this.userServiceContext = userServiceContext;
            this.userManager = userManager;
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

        public async Task InitAsync()
        {
            var userServiceDbCreator = userServiceContext.GetService<IRelationalDatabaseCreator>();
            await userServiceDbCreator.DeleteAsync();
            await userServiceDbCreator.CreateAsync();
            await userServiceDbCreator.CreateTablesAsync();
            await CreateUsers();
            await CreateOpenIdClients();
        }
    }

    internal interface IInitDb
    {
        Task InitAsync();
    }
}

namespace OksidiCom.AspNetCoreServices.UserServices
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

            if (Environment.IsDevelopment())
            {
                services.AddScoped<IDbContextConnectionConfiguration>(t => new DbContextConnectionConfiguration(
                    new SqliteConnection(Configuration.GetConnectionString("Default"))
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
