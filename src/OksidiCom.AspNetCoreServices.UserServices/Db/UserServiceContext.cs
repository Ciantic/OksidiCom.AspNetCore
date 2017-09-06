using System;
using OksidiCom.AspNetCoreServices.UserServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OksidiCom.AspNetCoreServices.UserServices.Db
{
    public class UserServiceContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> opts) : base(opts)
        {
        }
    }
}
