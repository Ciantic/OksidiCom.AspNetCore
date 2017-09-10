using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OksidiCom.AspNetCore.UserService.Mvc;
using System;
using Xunit;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OksidiCom.AspNetCore.UserService.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Antiforgery.Internal;
using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;

namespace OksidiCom.UserService.Test
{
    [Authorize]
    public class RequestUserController
    {
        [HttpGet("~/RequestUser")]
        public ApplicationUser TestRequest([RequestUser] ApplicationUser applicationUser)
        {
            return applicationUser;
        }
    }

    public class UserServicesIntegrationTests
    {
        private readonly TestServer _testServer;
        private readonly HttpClient _client;

        public UserServicesIntegrationTests()
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
                { "Email", "test@example.com" },
                { "Password", "!Test1" },
                { "RememberMe", "true" },
                { "ReturnUrl", "/connect/authorize?response_type=id_token%20token&client_id=example-client&scope=openid&nonce=NONCE&state=STATE&display=popup&redirect_uri=http://example.com/o2c.html" }
            };
            var loginResponse = await _client.PostAsync("/connect/Account/Login", 
                new FormUrlEncodedContent(post));
            var authResponse = await _client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, loginResponse.Headers.Location)
                    .SetCookies(loginResponse.GetCookies())
            );

            var authValues = QueryHelpers.ParseQuery(authResponse.Headers.Location.Fragment.Substring(1));
            Assert.Equal(authResponse.Headers.Location.Host, "example.com");
            Assert.Equal(authValues.GetValueOrDefault("resource"), "resource_server");
            Assert.Equal(authValues.GetValueOrDefault("token_type"), "Bearer");
            Assert.Equal(authValues.GetValueOrDefault("access_token").Count(), 1);
            Assert.Equal(authValues.GetValueOrDefault("expires_in"), "3600");
            Assert.Equal(authValues.GetValueOrDefault("id_token").Count(), 1);
            Assert.Equal(authValues.GetValueOrDefault("state"), "STATE");
        }

        [Fact]
        public async Task RequestUserAttributeTest()
        {
            var response = await AuthenticatedRequest(HttpMethod.Get, "/RequestUser");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var applicationUser = JsonConvert.DeserializeObject<ApplicationUser>(content);
            Assert.Equal(applicationUser.Email, "test@example.com");
        }

        private async Task<HttpResponseMessage> AuthenticatedRequest(HttpMethod method, string path)
        {
            var accessToken = await GetAccessToken();
            var request = new HttpRequestMessage(method, path);
            //request.Headers.Accept = "application/json";
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            return await _client.SendAsync(request);
        }

        private async Task<string> GetAccessToken()
        {
            var post = new Dictionary<string, string>()
            {
                { "Email", "test@example.com" },
                { "Password", "!Test1" },
                { "RememberMe", "true" },
                { "ReturnUrl", "/connect/authorize?response_type=id_token%20token&client_id=example-client&scope=openid&nonce=NONCE&state=STATE&display=popup&redirect_uri=http://example.com/o2c.html" }
            };
            var loginResponse = await _client.PostAsync("/connect/Account/Login",
                new FormUrlEncodedContent(post));
            var authResponse = await _client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, loginResponse.Headers.Location)
                    .SetCookies(loginResponse.GetCookies())
            );

            var authValues = QueryHelpers.ParseQuery(authResponse.Headers.Location.Fragment.Substring(1));
            return authValues.GetValueOrDefault("access_token");
        }
    }
}
