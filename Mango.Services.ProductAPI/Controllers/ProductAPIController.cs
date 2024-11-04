using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private ResponseDto _response;
        private readonly IMapper _mapper;

        public ProductAPIController(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _response = new();
            _mapper = mapper;
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> products = _appDbContext.Products.ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Product product = _appDbContext.Products.First(d => d.ProductId == id);
                if (product == null)
                {
                    _response.IsSuccess = false;
                }
                _response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [HttpPost]
        [Authorize(Roles = "1")]
        public ResponseDto Post(ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                _appDbContext.Products.Add(product);
                _appDbContext.SaveChanges();

                if (productDto.Image != null)
                {

                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    // this willl give complete location to our wwwroot folder
                    var fileDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStrem = new FileStream(fileDirectoryPath, FileMode.Create))
                    {
                        // we want to copy to new location inside file stream
                        productDto.Image.CopyTo(fileStrem);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                    product.ImageLocalPath = filePath;

                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                }


                _appDbContext.Update(product);
                _appDbContext.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(product);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPut]
        [Authorize(Roles = "1")]
        public ResponseDto Put(ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                if (productDto.Image != null)
                {

                    if (!string.IsNullOrEmpty(product.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;
                    // this willl give complete location to our wwwroot folder
                    var fileDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStrem = new FileStream(fileDirectoryPath, FileMode.Create))
                    {
                        // we want to copy to new location inside file stream
                        productDto.Image.CopyTo(fileStrem);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                    product.ImageLocalPath = filePath;



                }
                else
                {
                    product.ImageUrl = "https://placehold.co/600x400";
                }
                _appDbContext.Products.Update(product);
                _appDbContext.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "1")]
        public ResponseDto Delete(int id)
        {
            try
            {
                Product product = _appDbContext.Products.First(d => d.ProductId == id);

                if (!string.IsNullOrEmpty(product.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                _appDbContext.Remove(product);
                _appDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
