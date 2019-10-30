using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NLayersApp.Persistence.Extensions
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker)
        {
            changeTracker.DetectChanges();
            var context = changeTracker.Context;
            var userId = Thread.CurrentPrincipal?.Identity.Name.ToCriptedGuid() ?? Guid.NewGuid();
            var timeStamp = DateTime.UtcNow;

            //if(userId != null)
            //{
            //    userId = context.Set<IdentityUser>().FirstOrDefault(p => p.UserName == userId).Id;
            //}

            foreach (var entry in changeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Deleted when entry.Entity is ISoftDelete:
                        entry.Property("IsDeleted").CurrentValue = true;
                        if(entry.Entity is IAuditable)
                        {
                            entry.Property("ModifiedBy").CurrentValue = userId;
                            entry.Property("ModifiedOn").CurrentValue = timeStamp;
                        }
                        entry.State = EntityState.Modified;
                        break;
                    case EntityState.Modified when entry.Entity is IAuditable:
                        entry.Property("ModifiedBy").CurrentValue = userId;
                        entry.Property("ModifiedOn").CurrentValue = timeStamp;
                        break;
                    case EntityState.Added when entry.Entity is IAuditable:
                        entry.Property("CreatedBy").CurrentValue = userId;
                        entry.Property("CreatedOn").CurrentValue = timeStamp;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
