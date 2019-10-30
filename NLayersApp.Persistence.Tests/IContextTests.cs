using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLayersApp.Persistence.Abstractions;
using NLayersApp.Persistence.Extensions;

namespace NLayersApp.Persistence.Tests
{
    [TestClass]
    public class IContextTests
    {
        private class TestModel: IAuditable, ISoftDelete
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private IContext GetContext(ITypesResolver typesResolver = null,Type[] types = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext<IdentityUser, IdentityRole, string>>();
            optionsBuilder.UseInMemoryDatabase("NlayerappDb");

            return new TDbContext<IdentityUser, IdentityRole, string>(optionsBuilder.Options, typesResolver);
        }

        private ITypesResolver GetResolver(params Type[] types)
        {
            return new TypesResolver(() => types);
        }

        [TestMethod]
        public void Shadow_Properties_Exists()
        {
            var typesResolver = GetResolver(typeof(TestModel));

            using (var context = GetContext(typesResolver))
            {
                context.Set<TestModel>().Add(new TestModel());
                context.SaveChangesAsync(CancellationToken.None);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = GetContext(typesResolver))
            {
                var entityType = context.Model.FindRuntimeEntityType(typeof(TestModel));

                Assert.IsFalse(entityType.FindProperty("CreatedOn") is null);
                Assert.IsFalse(entityType.FindProperty("CreatedBy") is null);
                
                Assert.IsFalse(entityType.FindProperty("ModifiedOn") is null);
                Assert.IsFalse(entityType.FindProperty("ModifiedBy") is null);
                
                Assert.IsFalse(entityType.FindProperty("IsDeleted") is null);
            }
        }

        [TestMethod]
        public void Shadow_Properties_Assignement()
        {
            var typesResolver = GetResolver(typeof(TestModel));

            using (var context = GetContext(typesResolver))
            {
                var entry = context
                    .Set<TestModel>()
                    .Add(
                    new TestModel() { 
                        Name = nameof(TestModel)
                    });

                var timestamp = DateTime.UtcNow;
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("mouadcherkaoui"), new string[] { "Admin" });

                context.SaveChangesAsync(CancellationToken.None);


                var createdBy_CurrentValue = entry.CurrentValues.GetValue<Guid>("CreatedBy");
                var createdOn_CurrentValue = entry.CurrentValues.GetValue<DateTime>("CreatedOn");

                var createdBy_ExpectedValue = Thread.CurrentPrincipal.Identity.Name.ToCriptedGuid();
                var createdOn_ExpectedValue = timestamp;

                var entityType = context.Model.FindRuntimeEntityType(typeof(TestModel));

                var createdOnProperty = entityType.FindProperty("CreatedOn").PropertyInfo;
                var createdByProperty = entityType.FindProperty("CreatedBy").PropertyInfo;

                Assert.AreEqual(createdBy_CurrentValue, createdBy_ExpectedValue);
            }
        }

        [TestMethod]
        public async Task SoftDelete_Extension_AssignTrue()
        {
            var typesResolver = GetResolver(typeof(TestModel));

            using (var context = GetContext(typesResolver))
            {
                var entry = context
                    .Set<TestModel>()
                    .Add(
                    new TestModel() { 
                        Name = nameof(TestModel)
                    });

                var timestamp = DateTime.UtcNow;
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("mouadcherkaoui"), new string[] { "Admin" });

                await context.SaveChangesAsync(CancellationToken.None);


                Assert.IsFalse(entry.Property<bool>("IsDeleted").CurrentValue);

                context.Set<TestModel>().Remove(entry.Entity);
                
                await context.SaveChangesAsync(CancellationToken.None);

                entry = context.Set<TestModel>().Attach(entry.Entity);
                entry.Reload();

                var modifiedBy_CurrentValue = entry.CurrentValues.GetValue<Guid>("ModifiedBy");
                var modifiedOn_CurrentValue = entry.CurrentValues.GetValue<DateTime>("ModifiedOn");

                var modifiedBy_ExpectedValue = Thread.CurrentPrincipal.Identity.Name.ToCriptedGuid();
                var modifiedOn_ExpectedValue = timestamp;

                Assert.AreEqual(modifiedBy_ExpectedValue, modifiedBy_CurrentValue);
                Assert.AreEqual(modifiedOn_ExpectedValue.ToShortDateString(), modifiedOn_CurrentValue.ToShortDateString());
                Assert.AreEqual(modifiedOn_ExpectedValue.ToShortTimeString(), modifiedOn_CurrentValue.ToShortTimeString());
                Assert.IsTrue(entry.Property<bool>("IsDeleted").CurrentValue);
            }
        }        

        [TestMethod]
        public void RegisteredTypes_Tests() 
        {
            var typesResolver = GetResolver(typeof(TestModel));

            using (var context = GetContext(typesResolver))
            {
                context.Set<TestModel>().Add(new TestModel());
                context.SaveChangesAsync(CancellationToken.None);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = GetContext(typesResolver))
            {
                foreach (var current in typesResolver.RegisteredTypes)
                {
                    var entityType = context.Model.FindRuntimeEntityType(current);
                    Assert.IsNotNull(entityType);
                }

            }            
        }
    }
}
