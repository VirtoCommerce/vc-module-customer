using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model;

public class FavoriteAddressEntity : Entity
{
    [Required]
    [StringLength(128)]
    public string UserId { get; set; }

    public string AddressId { get; set; }
    public AddressEntity Address { get; set; }
}
