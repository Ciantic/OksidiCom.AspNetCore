using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCoreServices.UserServices.Models;
using OksidiCom.AspNetCoreServices.UserServices.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCoreServices.Common.Db;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.EntityFrameworkCore;

namespace OksidiCom.AspNetCoreServices.UserServices
{
    public static class IServiceCollectionExtension
    {
        public class AddUserServicesOptions
        {
            public Action<IServiceProvider, DbContextOptionsBuilder> DbContextOptions { get; set; } = (s, c) =>
            {
                s.GetService<DbContextConnectionConfiguration>().Configure(c);
            };
        }

        /// <summary>
        /// User services middleware
        /// 
        /// Note: This must be called before `UseMvc()`
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddUserServices(this IServiceCollection services, IConfiguration configuration, AddUserServicesOptions addUserServicesOptions = null)
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

            // Read the UserServices from appsettings.json
            var userServicesConfiguration = new UserServicesConfiguration();
            configuration.GetSection("UserServices").Bind(userServicesConfiguration);

            // Code provided options
            if (addUserServicesOptions == null)
            {
                addUserServicesOptions = new AddUserServicesOptions();
            }

            // Add views provided in this assembly.     
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.FileProviders.Add(new EmbeddedFileProvider(typeof(Startup).GetTypeInfo().Assembly));
            });

            services
                .AddDbContext<UserServiceContext>((s, o) =>
                {
                    o.UseOpenIddict();
                    addUserServicesOptions.DbContextOptions(s, o);
                })
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

            if (userServicesConfiguration?.Google != null)
            {
                auth.AddGoogle(o =>
                {
                    o.ClientId = userServicesConfiguration.Google.ClientId;
                    o.ClientSecret = userServicesConfiguration.Google.ClientSecret;
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
