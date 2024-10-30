using Mango.Services.ShoppingCartAPI.Models.Dto;
using System.Collections;
namespace Mango.Services.ShoppingCartAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProduct();

    }
}
