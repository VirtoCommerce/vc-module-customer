namespace VirtoCommerce.CustomerModule.Core.Model
{
    public interface IHasPersonName
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        string FullName { get; set; }
    }
}
