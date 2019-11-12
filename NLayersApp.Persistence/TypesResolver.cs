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
            RegisteredTypes =
            options.Value.TypesDefinitions.Select(t =>
            {
                var assembly = t.Assembly;
                return Assembly.LoadFrom($"{assembly}")
                    .ExportedTypes
                    .Where(type => t.Types.Any(t => type.Name.Equals(t, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
            }).Aggregate(new Type[0], (ac, t) => ac.Concat(t).ToArray());
            var types = options.Value.TypesDefinitions;

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