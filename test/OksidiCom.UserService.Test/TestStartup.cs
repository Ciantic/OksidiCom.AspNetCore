using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCore.UserService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OksidiCom.AspNetCore.UserService.Db;
using OpenIddict.Core;
using OpenIddict.Models;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserService.Models;
using Microsoft.AspNetCore.Antiforgery;
using Moq;

namespace OksidiCom.UserService.Test
{
    public class TestStartup
    {
        private SqliteConnection connection = new SqliteConnection("Data Source=:memory:");

        public void ConfigureServices(IServiceCollection services)
        {
            connection.Open();
            services.AddUserService(opts =>
            {
                opts.AddDbContext(o => o.UseSqlite(connection));
                opts.Configure(new UserServiceConfiguration()
                {
                    //Jwt = new UserServiceConfiguration.JwtConfiguration()
                    //{
                    //    Audience = "http://localhost",
                    //    Authority = "http://localhost",
                    //    RequireHttpsMetadata = false,
                    //}
                });
            });
            services.AddSingleton(Mock.Of<IAntiforgery>());
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, UserServiceContext userServiceContext, OpenIddictApplicationManager<OpenIddictApplication> applicationManager, UserManager<ApplicationUser> userManager)
        {
            app.UseUserService();
            app.UseMvc();

            // Create database
            userServiceContext.Database.EnsureCreated();

            // OpenId application 
            applicationManager.CreateAsync(new OpenIddictApplication()
            {
                ClientId = "example-client",
                DisplayName = "Example Client",
                LogoutRedirectUri = "http://example.com/logout.html",
                RedirectUri = "http://example.com/o2c.html",
                Type = OpenIddictConstants.ClientTypes.Public

            }, new CancellationToken()).GetAwaiter().GetResult();

            // User
            userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "test@example.com",
                Email = "test@example.com",
            }, "!Test1").GetAwaiter().GetResult();
        }
    }
}
