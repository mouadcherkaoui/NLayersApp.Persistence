using System;

namespace NLayersApp.Persistence.Abstractions
{
    public interface ITypesResolver 
    {
        Type[] RegisteredTypes { get; }
        Type Resolve(string typeName);
        Func<string, Type> ResolveAction { get; }
    }    
}