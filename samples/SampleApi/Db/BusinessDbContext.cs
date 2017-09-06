using Microsoft.EntityFrameworkCore;
using SampleApi.Models;

namespace SampleApi.Db
{
    public class BusinessDbContext : DbContext
    {
        public DbSet<BusinessThing> BusinessThings { get; set; }

        public BusinessDbContext(DbContextOptions<BusinessDbContext> context) : base(context)
        {
            
        }
    }
}
