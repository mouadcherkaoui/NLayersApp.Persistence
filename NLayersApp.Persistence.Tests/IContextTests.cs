using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
        private IContext GetContext(ITypesResolver typesResolver)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext<IdentityUser, IdentityRole, string>>();
            optionsBuilder.UseInMemoryDatabase("NlayerappDb");

            var context = new TDbContext<IdentityUser, IdentityRole, string>(optionsBuilder.Options, typesResolver);
            ((DbContext)context).Database.EnsureCreated();
            // ((DbContext)context).Database.Migrate();
            return context;
        }

        private ITypesResolver GetResolver(params Type[] types)
        {
            
            var resolverOptions = new TypesResolverOptions()
            {
                Assembly = Assembly.GetExecutingAssembly().GetName().Name,
                Types = types.Select(t => t.Name).ToArray()
            };

            return new TypesResolver(resolverOptions);
        }
        ITypesResolver typesResolver;
        
        [TestInitialize] 
        public void Test_Initialization()
        {
            typesResolver = GetResolver(typeof(TestModel), typeof(Auditable), typeof(SoftDeletable), typeof(AuditableSoftDeletable));
        }


        [TestMethod]
        public void Shadow_Properties_Doesnt_Exist_On_Types_NotImplementing_IAuditable_ISoftDelete()
        {
            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = GetContext(typesResolver))
            {
                var testEntityType = context.Model.FindRuntimeEntityType(typeof(TestModel));

                Assert.IsTrue(testEntityType.FindProperty("IsDeleted") is null);

                Assert.IsTrue(testEntityType.FindProperty("CreatedOn") is null);
                Assert.IsTrue(testEntityType.FindProperty("CreatedBy") is null);

                Assert.IsTrue(testEntityType.FindProperty("ModifiedOn") is null);
                Assert.IsTrue(testEntityType.FindProperty("ModifiedBy") is null);
            }
        }

        [TestMethod]
        public void Audit_Properties_Exists_On_Types_Implementing_IAuditable()
        {
            using (var context = GetContext(typesResolver))
            {
                context.Set<Auditable>().Add(new Auditable());
                context.SaveChangesAsync(CancellationToken.None);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = GetContext(typesResolver))
            {
                var auditableEntityType = context.Model.FindRuntimeEntityType(typeof(Auditable));

                Assert.IsFalse(auditableEntityType.FindProperty("CreatedOn") is null);
                Assert.IsFalse(auditableEntityType.FindProperty("CreatedBy") is null);
                
                Assert.IsFalse(auditableEntityType.FindProperty("ModifiedOn") is null);
                Assert.IsFalse(auditableEntityType.FindProperty("ModifiedBy") is null);
            }
        }

        [TestMethod]
        public void IsDeleted_Property_Exists_On_Types_Implementing_ISoftDelete()
        {
            using (var context = GetContext(typesResolver))
            {
                context.Set<SoftDeletable>().Add(new SoftDeletable());
                context.SaveChangesAsync(CancellationToken.None);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = GetContext(typesResolver))
            {
                var softDeletableEntityType = context.Model.FindRuntimeEntityType(typeof(SoftDeletable));
                
                Assert.IsFalse(softDeletableEntityType.FindProperty("IsDeleted") is null);
            }
        }

        [TestMethod]
        public void Shadow_Properties_Assignement()
        {
            using (var context = GetContext(typesResolver))
            {
                var entry = context
                    .Set<AuditableSoftDeletable>()
                    .Add(new AuditableSoftDeletable() { Name = nameof(AuditableSoftDeletable) });

                var timestamp = DateTime.UtcNow;
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("mouadcherkaoui"), new string[] { "Admin" });

                context.SaveChangesAsync(CancellationToken.None);

                var createdBy_CurrentValue = entry.CurrentValues.GetValue<Guid>("CreatedBy");
                var createdOn_CurrentValue = entry.CurrentValues.GetValue<DateTime>("CreatedOn");

                var createdBy_ExpectedValue = Thread.CurrentPrincipal.Identity.Name.ToCriptedGuid();
                var createdOn_ExpectedValue = timestamp;

                var entityType = context.Model.FindRuntimeEntityType(typeof(AuditableSoftDeletable));

                var createdOnProperty = entityType.FindProperty("CreatedOn").PropertyInfo;
                var createdByProperty = entityType.FindProperty("CreatedBy").PropertyInfo;

                Assert.AreEqual(createdBy_CurrentValue, createdBy_ExpectedValue);
            }
        }

        [TestMethod]
        public async Task SoftDelete_Extension_AssignTrue()
        {
            using (var context = GetContext(typesResolver))
            {
                var entry = context
                    .Set<AuditableSoftDeletable>()
                    .Add(new AuditableSoftDeletable() { Name = nameof(AuditableSoftDeletable) });

                var timestamp = DateTime.UtcNow;
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("mouadcherkaoui"), new string[] { "Admin" });

                await context.SaveChangesAsync(CancellationToken.None);

                // testing the default SoftDelete value
                Assert.IsFalse(entry.Property<bool>("IsDeleted").CurrentValue);

                context.Set<AuditableSoftDeletable>().Remove(entry.Entity);
                
                await context.SaveChangesAsync(CancellationToken.None);

                entry = context.Set<AuditableSoftDeletable>().Attach(entry.Entity);
                entry.Reload();
                
                // get current values after the SoftDelete operation
                var modifiedBy_CurrentValue = entry.CurrentValues.GetValue<Guid>("ModifiedBy");
                var modifiedOn_CurrentValue = entry.CurrentValues.GetValue<DateTime>("ModifiedOn");

                // preparing expected test values
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
            using (var context = GetContext(typesResolver))
            {
                Assert.IsNotNull(context.Model.FindRuntimeEntityType(typeof(TestModel)));
                Assert.IsNotNull(context.Model.FindRuntimeEntityType(typeof(Auditable)));
                Assert.IsNotNull(context.Model.FindRuntimeEntityType(typeof(SoftDeletable)));                
                
                foreach (var current in typesResolver.RegisteredTypes)
                {
                    var entityType = context.Model.FindRuntimeEntityType(current);
                    Assert.IsNotNull(entityType);
                }          
            }
        }
    }
}
