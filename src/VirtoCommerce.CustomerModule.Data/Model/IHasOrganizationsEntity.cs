namespace VirtoCommerce.CustomerModule.Data.Model;

public interface IHasOrganizationsEntity
{
    string DefaultOrganizationId { get; set; }
    string CurrentOrganizationId { get; set; }
}
