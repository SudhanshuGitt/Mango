using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            IList<ProductDto> products = new List<ProductDto>();
            ResponseDto? response = await _productService.GetAllProdutsAsync();

            if (response != null && response.IsSuccess)
            {
                products = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ProductCreateUpdate(int productId)
        {
            if (productId != 0)
            {
                ResponseDto? response = await _productService.GetProductByIdAsync(productId);
                if (response != null && response.IsSuccess)
                {
                    ProductDto? product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                    ViewBag.Update = true;
                    return View(product);
                }
            }
            ViewBag.Update = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreateUpdate(ProductDto product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductId == 0)
                {
                    ResponseDto? response = await _productService.CreateProductAsync(product);

                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = "Product created successfully!";
                        return RedirectToAction(nameof(ProductIndex));
                    }
                    else
                    {
                        TempData["error"] = response?.Message;
                    }
                }
                else
                {
                    ResponseDto? response = await _productService.UpdateProductAsync(product);

                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = "Product updated successfully!";
                        return RedirectToAction(nameof(ProductIndex));
                    }
                    else
                    {
                        TempData["error"] = response?.Message;
                    }
                }

            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> ProductDelete(int productId)
        {
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDto? product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(product);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto product)
        {
            ResponseDto? response = await _productService.DeleteProductAsync(product.ProductId);

            if (response != null && response.IsSuccess)
            {
                CouponDto? coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                TempData["success"] = "Product deleted successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }
    }
}
