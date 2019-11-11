using Microsoft.EntityFrameworkCore;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NLayersApp.Persistence.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void RegisterTypes(this ModelBuilder builder, params Type[] types)
        {
            foreach (var current in types)
            {
                if (!_isTypeConfiguration(current))
                {
                    builder.Entity(current);
                    builder.applyProperties(current);
                }
                else
                {
                    applyConfiguration(ref builder, current);
                }
            }
        }
        public static ModelBuilder AddAuditProperties<TType, TKey>(this ModelBuilder builder) 
            where TType: class
        {
            var entityType = builder.Model.FindRuntimeEntityType(typeof(TType)).ClrType;
            if (typeof(IAuditable).IsAssignableFrom(entityType))
            {
                builder.Entity<TType>().Property<TKey>("CreatedBy");
                builder.Entity<TType>().Property<TKey>("ModifiedBy");
                builder.Entity<TType>().Property<DateTime>("CreatedOn");
                builder.Entity<TType>().Property<DateTime>("ModifiedOn");
            }

            return builder;
        }

        public static ModelBuilder AddIsDeletedProperty<TType>(this ModelBuilder builder)
            where TType: class
        {
            var entityType = builder.Model.FindRuntimeEntityType(typeof(TType)).ClrType;
            if (typeof(ISoftDelete).IsAssignableFrom(entityType)) 
                builder.Entity<TType>().Property<bool>("IsDeleted");
            return builder;
        }

        private static void applyProperties(this ModelBuilder builder, Type type)
            => typeof(ModelBuilderExtensions)
            .GetMethod(nameof(_applyProperties), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(type)
            .Invoke(builder, new[] { builder });

        private static void _applyProperties<TType>(ref ModelBuilder builder)
            where TType: class
        {
            builder
                .AddAuditProperties<TType, Guid>()
                .AddIsDeletedProperty<TType>();
        }
        private static void applyConfiguration(ref ModelBuilder builder, Type current)
        {
            var configInstance = Activator.CreateInstance(current);
            var entityType = current.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(i => i.Name.Contains("IEntityTypeConfiguration"))?.GetGenericArguments().First();
            typeof(ModelBuilderExtensions)
                .GetMethod("_applyConfiguration", BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(entityType)
                .Invoke(builder, new[] { builder, configInstance });
        }

        private static void _applyConfiguration<TType>(ref ModelBuilder builder, IEntityTypeConfiguration<TType> config)            
            where TType: class
        {
            builder.ApplyConfiguration(config);
        }



        private static bool _isTypeConfiguration(Type type)
        {
            return type.GetInterfaces().Any(i => i.Name.Contains("IEntityTypeConfiguration"));
        }
    }
}
