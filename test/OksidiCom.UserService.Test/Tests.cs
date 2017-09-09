using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OksidiCom.AspNetCore.UserService.Mvc;
using System;
using Xunit;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCore.UserService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OksidiCom.AspNetCore.UserService.Db;
using OpenIddict.Core;
using OpenIddict.Models;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserService.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Antiforgery.Internal;
using System.Text.RegularExpressions;

namespace OksidiCom.UserService.Test
{
    [Authorize]
    [Route("[controller]")]
    public class RequestUserController
    {
        [HttpGet("[action]")]
        public bool TestRequest()
        {
            return false;
        }
    }

    public class TestStartup
    {
        private SqliteConnection connection = new SqliteConnection("Data Source=:memory:");

        public void ConfigureServices(IServiceCollection services)
        {
            connection.Open();
            services.AddUserService(opts =>
            {
                opts.AddDbContext(o => o.UseSqlite(connection));
            });
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

    public class Tests
    {
        private readonly TestServer _testServer;
        private readonly HttpClient _client;

        public Tests()
        {
            _testServer = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            _client = _testServer.CreateClient();
        }

        [Fact]
        public async Task AuthorizeRedirectsToLogin()
        {
            var response = await _client.GetAsync("/connect/authorize?response_type=id_token%20token&client_id=example-client&scope=openid&nonce=NONCE&state=STATE&display=popup&redirect_uri=http://example.com/o2c.html");

            Assert.Equal(response.Headers.Location.ToString(), "http://localhost/connect/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%3Fresponse_type%3Did_token%2520token%26client_id%3Dexample-client%26scope%3Dopenid%26nonce%3DNONCE%26state%3DSTATE%26display%3Dpopup%26redirect_uri%3Dhttp:%2F%2Fexample.com%2Fo2c.html");
        }

        [Fact]
        public async Task LoginIssuesToken()
        {
            
            var post = new Dictionary<string, string>()
            {
                { "__RequestVerificationToken", await GetAntiForgeryToken("/connect/Account/Login") },
                { "Email", "test@example.com" },
                { "Password", "!Test1" },
                { "RememberMe", "" },
                { "ReturnUrl", "/Account/LoggedIn" }
            };
            var loginContent = new FormUrlEncodedContent(post);

            var loginResponse = await _client.PostAsync("/connect/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%3Fresponse_type%3Did_token%2520token%26client_id%3Dexample-client%26scope%3Dopenid%26nonce%3DNONCE%26state%3DSTATE%26display%3Dpopup%26redirect_uri%3Dhttp:%2F%2Fexample.com%2Fo2c.html", loginContent);

            Assert.Equal(false, true);
        }

        private async Task<string> GetAntiForgeryToken(string path)
        {
            var response = await _client.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();
            var matches = new Regex(@"name=""__RequestVerificationToken"" type=""hidden"" value=""(.*?)""", RegexOptions.IgnoreCase).Match(content);
            var group = matches.Groups[0];
            return group.Value;
        }
    }
}
