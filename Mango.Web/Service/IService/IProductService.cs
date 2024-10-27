using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IProductService
    {
        Task<ResponseDto?> GetAllProdutsAsync();
        Task<ResponseDto?> GetProductByIdAsync(int id);
        Task<ResponseDto?> CreateProductAsync(ProductDto product);
        Task<ResponseDto?> DeleteProductAsync(int id);
        Task<ResponseDto?> UpdateProductAsync(ProductDto product);
    }
}
