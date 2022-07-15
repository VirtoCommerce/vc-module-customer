using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Services
{
    public interface IIconService
    {
        Task ResizeIcon(IconResizeRequest request);
    }
}
