using Microsoft.EntityFrameworkCore;

namespace OksidiCom.AspNetCoreServices.Common.Db
{
    public interface IDbContextConnectionConfiguration
    {
        object Configure(DbContextOptionsBuilder optionsBuilder);
    }
}
