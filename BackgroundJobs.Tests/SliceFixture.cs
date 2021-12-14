using BackgroundJobs.Data.DbContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundJobs.Tests
{
    public static class SliceFixture
    {
        private static readonly Checkpoint _checkpoint;
        public static readonly IConfigurationRoot _configuration;
        private static readonly IServiceScopeFactory _scopeFactory;
        public static ApplicationDbContext _dbContext;
        public static IServiceProvider _provider;
        public static string connectionString;

        static SliceFixture()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();

            var startup = new Startup(_configuration);
            var services = new ServiceCollection();
            startup.ConfigureServices(services);
            _provider = services.AddEntityFrameworkSqlServer().BuildServiceProvider();
            _scopeFactory = _provider.GetService<IServiceScopeFactory>();

            // Configuration for AccessRegister
            connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            dbBuilder.UseSqlServer(connectionString)
                    .UseInternalServiceProvider(_provider);
            _dbContext = new ApplicationDbContext(dbBuilder.Options);



            _checkpoint = new Checkpoint();
        }

        public static Task ResetCheckpoint() => _checkpoint.Reset(_configuration.GetConnectionString("DefaultConnection"));

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                    await action(scope.ServiceProvider).ConfigureAwait(false);

                    await dbContext.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }
        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    await dbContext.BeginTransactionAsync().ConfigureAwait(false);

                    var result = await action(scope.ServiceProvider).ConfigureAwait(false);

                    await dbContext.CommitTransactionAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }


        public static Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<ApplicationDbContext>()));

        public static Task ExecuteDbContextAsync(Func<ApplicationDbContext, IMediator, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<ApplicationDbContext>(), sp.GetService<IMediator>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<ApplicationDbContext>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, IMediator, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<ApplicationDbContext>(), sp.GetService<IMediator>()));

        public static Task InsertAsync<T>(params T[] entities) where T : class
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.Set<T>().Add(entity);
                }
                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
            where TEntity : class
            where TEntity2 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);

                return db.SaveChangesAsync();
            });
        }

        public static Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4)
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
            where TEntity4 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);
                db.Set<TEntity4>().Add(entity4);

                return db.SaveChangesAsync();
            });
        }

        //public static Task<T> FindAsync<T>(int id)
        //    where T : class, IIdentity
        //{
        //    return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id));
        //}

        public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        public static Task SendAsync(IRequest request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }
    }

    public class TestMessages
    {
        public const string DbWarning = @"Warning: Most of the integration tests resets the database (clears records), for a proper isolated test execution. So make sure that the 'DefaultConnection' 
connection string setting, in the appsettings.json file, always points to your local database.";
    }
}
