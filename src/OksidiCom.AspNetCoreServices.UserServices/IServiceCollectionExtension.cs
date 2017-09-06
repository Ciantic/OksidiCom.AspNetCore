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

namespace OksidiCom.AspNetCoreServices.UserServices
{
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

            services.AddDbContext<UserServiceContext>((s, o) =>
            {
                o.UseOpenIddict();
                s.GetService<IDbContextConnectionConfiguration>().Configure(o);
            });
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
