using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NLayersApp.Persistence.Abstractions;
using NLayersApp.Persistence.Extensions;

namespace NLayersApp.Persistence
{
    public class TDbContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly Type[] _types;
        private readonly ITypesResolver _innerTypesResolver;
        public TDbContext(DbContextOptions options) : base(options)
        {
        }

        public TDbContext(DbContextOptions options, ITypesResolver typesResolver) : this(options)
        {
            _innerTypesResolver = typesResolver;
        }

        public IMutableModel ExternalModel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.RegisterTypes(_innerTypesResolver.RegisteredTypes);
            base.OnModelCreating(builder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.SetShadowProperties();
            return base.SaveChangesAsync(cancellationToken);
        }
        Task IContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return this.SaveChangesAsync(cancellationToken);
        }
    }

    public class TDbContext : TDbContext<IdentityUser, IdentityRole, string>
    {
        public TDbContext(DbContextOptions options, ITypesResolver typesResolver) : base(options, typesResolver)
        {
        }
    }

    public class TDbContext<TUser, TKey> : TDbContext<TUser, IdentityRole<TKey>, TKey>
        where TUser: IdentityUser<TKey>
        where TKey: IEquatable<TKey>
    {
        public TDbContext(DbContextOptions options, ITypesResolver typesResolver) : base(options, typesResolver)
        {
        }
    }

    public class TDbContext<TUser> : TDbContext<TUser, string>
        where TUser : IdentityUser<string>
    {
        public TDbContext(DbContextOptions options, ITypesResolver typesResolver) : base(options, typesResolver)
        {
        }
    }
}
