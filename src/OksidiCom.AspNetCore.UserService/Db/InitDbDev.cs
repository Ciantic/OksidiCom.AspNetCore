using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OksidiCom.AspNetCore.UserService.Models;
using Microsoft.EntityFrameworkCore.Storage;
using OpenIddict.Models;
using OpenIddict.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;

namespace OksidiCom.AspNetCore.UserService.Db
{
    internal class InitDbDev : IInitDb
    {
        private readonly UserServiceContext userServiceContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> applicationManager;

        public InitDbDev(UserServiceContext userServiceContext, UserManager<ApplicationUser> userManager, OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            this.userServiceContext = userServiceContext;
            this.userManager = userManager;
            this.applicationManager = applicationManager;
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

        private async Task CreateOpenIdClients()
        {
            await applicationManager.CreateAsync(new OpenIddictApplication()
            {
                ClientId = "example-client",
                DisplayName = "Example Client",
                RedirectUri = "http://localhost:5002/o2c.html",
                LogoutRedirectUri = "http://localhost:5002/o2c-logout.html",
                Type = OpenIddictConstants.ClientTypes.Public
            }, new CancellationToken());
        }

        private async Task CreateDatabaseAsync()
        {

            var userServiceDbCreator = userServiceContext.GetService<IRelationalDatabaseCreator>();
            await userServiceDbCreator.DeleteAsync();
            await userServiceDbCreator.CreateAsync();
            await userServiceDbCreator.CreateTablesAsync();
        }

        public async Task InitAsync()
        {
            await CreateDatabaseAsync();
            await CreateUsers();
            await CreateOpenIdClients();
        }
    }
}
