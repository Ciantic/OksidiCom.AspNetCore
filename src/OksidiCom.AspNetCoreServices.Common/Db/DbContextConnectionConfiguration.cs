using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace OksidiCom.AspNetCoreServices.Common.Db
{

    /// <summary>
    /// This allows to share a DbConnection within a scope, e.g. between DbContexts so they can perform transactions 
    /// </summary>
    public class DbContextConnectionConfiguration : IDisposable
    {
        private readonly DbConnection _dbConnection;
        private readonly Action<DbConnection, DbContextOptionsBuilder> _configuration;
        private readonly bool _dispose;

        public DbContextConnectionConfiguration(DbConnection dbConnection, Action<DbConnection, DbContextOptionsBuilder> configuration, bool dispose = true)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
            _dispose = dispose;
        }

        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            _configuration(_dbConnection, optionsBuilder);
        }

        public void Dispose()
        {
            if (_dispose) { 
                _dbConnection.Dispose();
            }
        }
    }
}
