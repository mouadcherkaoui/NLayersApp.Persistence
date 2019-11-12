using Microsoft.Extensions.Options;

namespace NLayersApp.Persistence
{
    public class TypesResolverOptions: IOptions<TypesResolverOptions>
    {
        public TypeDefinition[] TypesDefinitions { get; set; }

        public TypesResolverOptions Value => this;
    }
}