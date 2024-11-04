using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        // we will not be using that when we create product but will be using it when update
        public string? ImageLocalPath { get; set; }
        public IFormFile Image { get; set; }
    }
}
