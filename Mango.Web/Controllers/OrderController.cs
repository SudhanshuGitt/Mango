using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        public IActionResult OrderIndex()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int orderId)
        {
            OrderHeaderDto order = new OrderHeaderDto();
            string userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;

            var response = await _orderService.GetOrderByOrderId(orderId);

            if (response != null && response.IsSuccess)
            {
                order = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            }
            if (!User.IsInRole(((byte)SD.Role.ADMIN).ToString()) && userId != order.UserId)
            {
                return NotFound();
            }

            return View(order);

        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(string status)
        {
            string userId = "";
            IEnumerable<OrderHeaderDto> orders;

            if (!User.IsInRole(((byte)SD.Role.ADMIN).ToString()))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            }

            ResponseDto response = await _orderService.GetAllOrdersByUserId(userId);

            if (response != null && response.IsSuccess)
            {
                SD.OrderStatus statusEnum;
                orders = JsonConvert.DeserializeObject<IEnumerable<OrderHeaderDto>>(Convert.ToString(response.Result));
                switch (Enum.TryParse<SD.OrderStatus>(status, out statusEnum) ? statusEnum : SD.OrderStatus.ALL)
                {
                    case SD.OrderStatus.APPROVED:
                        orders = orders.Where(o => o.Status == (byte)SD.OrderStatus.APPROVED);
                        break;
                    case SD.OrderStatus.READYFORPICUP:
                        orders = orders.Where(o => o.Status == (byte)SD.OrderStatus.READYFORPICUP);
                        break;
                    case SD.OrderStatus.CANCELLED:
                        orders = orders.Where(o => o.Status == (byte)SD.OrderStatus.CANCELLED || o.Status == (byte)SD.OrderStatus.REFUNDED);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                orders = new List<OrderHeaderDto>();
            }

            orders.ToList().ForEach(u => u.StatusString = ((SD.OrderStatus)u.Status).ToString());

            return Json(new { data = orders });

        }

        [HttpPost]
        public async Task<IActionResult> OrderStatusUpdate(int orderId, OrderStatus toStatus)
        {

            ResponseDto response = await _orderService.UpdateOrderStatus(orderId, (byte)toStatus);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status Updated successfully!";
                return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
            }

            return View();

        }

    }
}
