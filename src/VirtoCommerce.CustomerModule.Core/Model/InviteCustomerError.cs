namespace VirtoCommerce.CustomerModule.Core.Model;

public class InviteCustomerError
{
    public string Code { get; set; }

    public string Description { get; set; }

    public string Parameter { get; set; }

    public InviteCustomerError()
    {
    }

    public InviteCustomerError(string code, string description, string parameter)
    {
        Code = code;
        Description = description;
        Parameter = parameter;
    }
}
