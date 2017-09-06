using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace OksidiCom.AspNetCoreServices.Common.Db
{

    /// <summary>
    /// This allows to share a DbConnection within a scope, e.g. between DbContexts so they can perform transactions 
    /// </summary>
    public class DbContextConnectionConfiguration : IDbContextConnectionConfiguration, IDisposable
    {
        private readonly DbConnection _dbConnection;
        private readonly bool _dispose;

        public DbContextConnectionConfiguration(DbConnection dbConnection, bool dispose = true)
        {
            _dbConnection = dbConnection;
            _dispose = dispose;
        }

        public object Configure(DbContextOptionsBuilder optionsBuilder)
        {
            if (_dbConnection is SqliteConnection)
            {
                optionsBuilder.UseSqlite(_dbConnection);
            }
            else
            {
                throw new Exception("Unknown connection for ConnectionProvider");
            }
            return null;
        }

        public void Dispose()
        {
            if (_dispose) { 
                _dbConnection.Dispose();
            }
        }
    }
}
