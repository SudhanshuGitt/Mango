using Mango.Services.OrderAPI.Models.Dto;
using System.Collections;
namespace Mango.Services.OrderAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProduct();

    }
}
