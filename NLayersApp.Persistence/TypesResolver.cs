using NLayersApp.Persistence.Abstractions;
using System;
using System.Linq;

namespace NLayersApp.Persistence
{
    public class TypesResolver : ITypesResolver
    {
        Func<string, Type> resolveAction; 
        public TypesResolver(Func<Type[]> registerAction)
        {
            RegisteredTypes = registerAction?.Invoke();
            resolveAction = (s) => RegisteredTypes.FirstOrDefault(t => t.Name == s); 
        }
        public Type[] RegisteredTypes { get ; }
        public Type Resolve(string typeName) => this.asResolver().ResolveAction(typeName);
        Func<string, Type> ITypesResolver.ResolveAction => resolveAction;

        ITypesResolver asResolver() => ((ITypesResolver)this);
    }
}