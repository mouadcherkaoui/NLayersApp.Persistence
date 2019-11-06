using Microsoft.EntityFrameworkCore;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
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
                builder.Entity(current);
                builder.applyProperties(current);
            }
        }
        public static ModelBuilder AddAuditProperties<TType, TKey>(this ModelBuilder builder) 
            where TType: class
        {
            if (typeof(TType).IsAssignableFrom(typeof(IAuditable)))
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
            if(typeof(TType).IsAssignableFrom(typeof(ISoftDelete))) 
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
    }
}
