using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserServices.Models;
using OksidiCom.AspNetCore.UserServices.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCore.Common.Db;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.EntityFrameworkCore;

namespace OksidiCom.AspNetCore.UserServices
{

    public static class IServiceCollectionExtension
    {
        public class AddUserServicesOptions
        {
            /// <summary>
            /// Configure UserServices from appsettings.json, assumes "UserServices" property.
            /// </summary>
            /// <param name="configuration"></param>
            public void Configure(IConfiguration configuration)
            {
                configuration.GetSection("UserServices").Bind(_configuration);
            }

            /// <summary>
            /// Configure manually the UserServices
            /// </summary>
            /// <param name="userServicesConfiguration"></param>
            public void Configure(UserServicesConfiguration userServicesConfiguration)
            {
                _configuration = userServicesConfiguration;
            }

            /// <summary>
            /// Add DbContext options for UserServicesContext
            /// </summary>
            /// <param name="builder"></param>
            public void AddDbContext(Action<DbContextOptionsBuilder> builder)
            {
                _dbContextBuilder = (s, o) => builder(o);
            }

            /// <summary>
            /// Add DbContext options for UserServicesContext
            /// </summary>
            /// <param name="builder"></param>
            public void AddDbContext(Action<IServiceProvider, DbContextOptionsBuilder> builder)
            {
                _dbContextBuilder = builder;
            }

            internal Action<IServiceProvider, DbContextOptionsBuilder> _dbContextBuilder { get; set; }

            internal UserServicesConfiguration _configuration = new UserServicesConfiguration();
        }

        /// <summary>
        /// User services middleware
        /// 
        /// Note: This must be called before `UseMvc()`
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddUserServices(this IServiceCollection services, Action<AddUserServicesOptions> createOptions = null)
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

            var opts = new AddUserServicesOptions();
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
                .AddIdentity<ApplicationUser, ApplicationRole>(o =>
                {
                    o.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                    //options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                    //options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
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

            var auth = services.AddAuthentication()
                .AddJwtBearer(o =>
                {
                    o.Audience = conf.Jwt.Audience;
                    o.Authority = conf.Jwt.Authority;
                });

            if (opts._configuration?.Google != null)
            {
                auth.AddGoogle(o =>
                {
                    o.ClientId = opts._configuration.Google.ClientId;
                    o.ClientSecret = opts._configuration.Google.ClientSecret;
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
                            .EnableLogoutEndpoint("/connect/logout")
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
