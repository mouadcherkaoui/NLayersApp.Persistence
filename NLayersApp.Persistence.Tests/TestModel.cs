using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NLayersApp.Persistence.Tests
{
    public class TestModelConfig : IEntityTypeConfiguration<TestModel>
    {
        public void Configure(EntityTypeBuilder<TestModel> builder)
        {
            builder.HasKey(p => p.Id);
        }
    }
    public class TestModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Auditable : IAuditable
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SoftDeletable : ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AuditableSoftDeletable : IAuditable, ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
