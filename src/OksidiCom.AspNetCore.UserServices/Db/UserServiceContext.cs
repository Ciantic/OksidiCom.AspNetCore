using System;
using OksidiCom.AspNetCore.UserServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OksidiCom.AspNetCore.UserServices.Db
{
    public class UserServiceContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> opts) : base(opts)
        {
        }
    }
}
