using Microsoft.Extensions.Options;

namespace NLayersApp.Persistence
{
    public class TypesResolverOptions: IOptions<TypesResolverOptions>
    {
        public string[] Types { get; set; }
        public string Assembly { get; set; }

        public TypesResolverOptions Value => this;
    }
}