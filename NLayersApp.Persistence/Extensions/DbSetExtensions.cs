using Microsoft.EntityFrameworkCore;

namespace NLayersApp.Persistence.Extensions
{
    public static class DbSetExtensions
    {
        public static bool SoftDelete<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity: class
        {
            var current = dbSet.Remove(entity);
            // current.Property<bool>("IsDeleted").CurrentValue = true;
            // current.Context.SaveChanges();
            return true;
        }
    }
}