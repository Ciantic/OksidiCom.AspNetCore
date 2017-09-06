﻿using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OksidiCom.AspNetCoreServices.UserServices;
using Microsoft.Extensions.Configuration;
using OksidiCom.AspNetCoreServices.UserServices.Db;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OksidiCom.AspNetCoreServices.Common.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SampleApi.Db;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.SqlClient;
using System;

namespace SampleApi
{
    internal class Startup
    {
        private IHostingEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;
            Configuration = configuration;
        }

        private void SharedConnection(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            serviceProvider.GetService<DbContextConnectionConfiguration>().Configure(dbContextOptionsBuilder);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUserServices(opts =>
            {
                opts.AddDbContext(SharedConnection);
                opts.Configure(Configuration);
            });
            services.AddMvc();
            services.AddDbContext<BusinessDbContext>(SharedConnection);

            if (Environment.IsDevelopment())
            {
                services.AddScoped(t => new DbContextConnectionConfiguration(
                    new SqliteConnection(Configuration.GetConnectionString("Default")),
                    (c, o) => o.UseSqlite(c)
                ));
                services.AddTransient<IInitDb, InitDbDev>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IInitDb initDb)
        {
            
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseUserServices();
            app.UseMvc();

            initDb.InitAsync().Wait();
            initDb.PopulateAsync().Wait();
        }
    }
}
