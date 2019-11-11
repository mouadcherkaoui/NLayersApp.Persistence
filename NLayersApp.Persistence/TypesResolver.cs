using Microsoft.Extensions.Options;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace NLayersApp.Persistence
{
    public class TypesResolver : ITypesResolver
    {
        Func<string, Type> resolveAction;
        public TypesResolver(IOptions<TypesResolverOptions> options)
        {
            var assembly = options.Value.Assembly;
            var types = options.Value.Types;

            RegisteredTypes = Assembly.LoadFrom($"{assembly}.dll")
                .ExportedTypes
                .Where(type => types.Any(t => type.Name.Equals(t, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }

        public TypesResolver(Func<Type[]> registerAction)
        {
            RegisteredTypes = registerAction?.Invoke();
            resolveAction = (s) => RegisteredTypes.FirstOrDefault(t => t.Name == s); 
        }
        public Type[] RegisteredTypes { get ; }
        public Type Resolve(string typeName) => asResolver().ResolveAction(typeName);
        Func<string, Type> ITypesResolver.ResolveAction => resolveAction;

        ITypesResolver asResolver() => this;
    }
}