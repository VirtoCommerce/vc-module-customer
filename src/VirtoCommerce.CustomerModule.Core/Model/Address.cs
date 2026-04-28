using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerModule.Core.Model
{

    [SwaggerSchemaId("CustomerAddress")]
    public class Address : CoreModule.Core.Common.Address, IEntity
    {
        public string Id { get => Key; set => Key = value; }
    }
}
