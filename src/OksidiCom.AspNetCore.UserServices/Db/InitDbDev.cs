using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserServices.Models;
using Microsoft.EntityFrameworkCore.Storage;
using OpenIddict.Models;
using OpenIddict.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace OksidiCom.AspNetCore.UserServices.Db
{
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
}
