using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserService.Models;
using OksidiCom.AspNetCore.UserService.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCore.Common.Db;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OksidiCom.AspNetCore.UserService
{

    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// UserService options
        /// </summary>
        public class AddUserServiceOptions
        {
            /// <summary>
            /// Configure UserService from appsettings.json, assumes "UserService" property.
            /// </summary>
            /// <param name="configuration"></param>
            public void Configure(IConfiguration configuration)
            {
                configuration.GetSection("UserService").Bind(_configuration);
            }

            /// <summary>
            /// Configure manually the UserService
            /// </summary>
            /// <param name="UserServiceConfiguration"></param>
            public void Configure(UserServiceConfiguration UserServiceConfiguration)
            {
                _configuration = UserServiceConfiguration;
            }

            /// <summary>
            /// Add DbContext options for UserServiceContext
            /// </summary>
            /// <param name="builder"></param>
            public void AddDbContext(Action<DbContextOptionsBuilder> builder)
            {
                _dbContextBuilder = (s, o) => builder(o);
            }

            /// <summary>
            /// Add DbContext options for UserServiceContext
            /// </summary>
            /// <param name="builder"></param>
            public void AddDbContext(Action<IServiceProvider, DbContextOptionsBuilder> builder)
            {
                _dbContextBuilder = builder;
            }

            internal Action<IServiceProvider, DbContextOptionsBuilder> _dbContextBuilder { get; set; }

            internal UserServiceConfiguration _configuration = new UserServiceConfiguration();
        }

        /// <summary>
        /// User services middleware
        /// 
        /// Note: This must be called before `UseMvc()`
        /// </summary>
        /// <param name="services"></param>
        /// <param name="createOptions"></param>
        public static void AddUserService(this IServiceCollection services, Action<AddUserServiceOptions> createOptions = null)
        {
            // Ensure that AddMvcCore/AddMvc has not been called before registering user services
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                if (service.ServiceType == typeof(MvcRouteHandler))
                {
                    throw new InvalidOperationException("AddUserService() must be called before AddMvc()");
                }
            }

            var opts = new AddUserServiceOptions();
            createOptions?.Invoke(opts);
            var conf = opts._configuration;
            var dbContextBuilder = opts._dbContextBuilder;

            // Add views provided in this assembly.     
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.FileProviders.Add(new EmbeddedFileProvider(typeof(Startup).GetTypeInfo().Assembly));
            });

            services
                .AddDbContext<UserServiceContext>((contextServices, builder) =>
                {
                    builder.UseOpenIddict();
                    dbContextBuilder(contextServices, builder);
                })
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                    options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                })
                .AddEntityFrameworkStores<UserServiceContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Path = "/connect";
                options.LoginPath = "/connect/Account/Login";
                options.LogoutPath = "/connect/Account/Logout";
            });

            services.AddAntiforgery(o =>
            {
                o.Cookie.Path = "/connect";
            });

            var auth = services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.Audience = conf.Jwt.Audience;
                    o.Authority = conf.Jwt.Authority;
                    o.RequireHttpsMetadata = false;
                });

            // External Google provider
            if (conf?.Google != null)
            {
                auth.AddGoogle(o =>
                {
                    o.ClientId = conf.Google.ClientId;
                    o.ClientSecret = conf.Google.ClientSecret;
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

                options.AllowImplicitFlow()
                        .EnableAuthorizationEndpoint("/connect/authorize")
                        .EnableTokenEndpoint("/connect/token")
                        .EnableLogoutEndpoint("/connect/logout")
                        .AllowRefreshTokenFlow();

                // Allow client applications to use the grant_type=password flow.
                // options.AllowPasswordFlow();

                // Jwt
                options.UseJsonWebTokens();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();
                options.AddDevelopmentSigningCertificate();

            });
        }
    }
}
