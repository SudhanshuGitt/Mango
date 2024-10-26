using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;

namespace Mango.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        public async Task<IActionResult> CouponIndex()
        {
            IList<CouponDto>? coupons = new List<CouponDto>();

            ResponseDto? response = await _couponService.GetAllCouponsAsync();

            if (response != null && response.IsSuccess)
            {
                coupons = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(coupons);
        }

        [HttpGet]
        public async Task<IActionResult> CouponCreateUpdate(int couponId)
        {
            if (couponId != 0)
            {
                ResponseDto? response = await _couponService.GetCouponbyIdAsync(couponId);
                if (response != null && response.IsSuccess)
                {
                    CouponDto? coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                    TempData["update"] = "update";
                    return View(coupon);
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CouponCreateUpdate(CouponDto coupon)
        {
            if (ModelState.IsValid)
            {
                if (coupon.CouponId == 0)
                {
                    ResponseDto? response = await _couponService.CreateCouponAsync(coupon);

                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = "Coupon created successfully!";
                        return RedirectToAction(nameof(CouponIndex));
                    }
                    else
                    {
                        TempData["error"] = response?.Message;
                    }
                }
                else
                {
                    ResponseDto? response = await _couponService.UpdateCouponAsync(coupon);

                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = "Coupon updated successfully!";
                        return RedirectToAction(nameof(CouponIndex));
                    }
                    else
                    {
                        TempData["error"] = response?.Message;
                    }
                }

            }
            return View(coupon);
        }


        [HttpGet]
        public async Task<IActionResult> CouponDelete(int couponId)
        {
            ResponseDto? response = await _couponService.GetCouponbyIdAsync(couponId);

            if (response != null && response.IsSuccess)
            {
                CouponDto? coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                return View(coupon);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CouponDelete(CouponDto couponDto)
        {
            ResponseDto? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

            if (response != null && response.IsSuccess)
            {
                CouponDto? coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                TempData["success"] = "Coupon deleted successfully!";
                return RedirectToAction(nameof(CouponIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(couponDto);
        }
    }
}
