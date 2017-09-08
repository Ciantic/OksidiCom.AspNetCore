using System;
using OksidiCom.AspNetCore.UserService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OksidiCom.AspNetCore.UserService.Db
{
    public class UserServiceContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> opts) : base(opts)
        {
        }
    }
}
