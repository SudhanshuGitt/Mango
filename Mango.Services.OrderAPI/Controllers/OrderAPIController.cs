using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Utility;
using Stripe;
using Stripe.Checkout;
using Mango.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {

        private readonly AppDbContext _appDbContext;
        private ResponseDto _response;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;

        public OrderAPIController(AppDbContext appDbContext, IMapper mapper, IProductService productService,
            IConfiguration configuration, IMessageBus messageBus)
        {
            _appDbContext = appDbContext;
            _response = new();
            _mapper = mapper;
            _productService = productService;
            _configuration = configuration;
            _messageBus = messageBus;
        }

        [Authorize]
        [HttpGet("GetOrders/{userId?}")]
        public async Task<ResponseDto> Get(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders;

                if (User.IsInRole(((byte)SD.Role.ADMIN).ToString()))
                {
                    orderHeaders = _appDbContext.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                else
                {
                    orderHeaders = _appDbContext.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orderHeaders);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpGet("GetOrder/{orderId:int}")]
        public async Task<ResponseDto> Get(int orderId)
        {
            try
            {
                OrderHeader orderHeader = _appDbContext.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == orderId);
                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] byte newStatus)
        {
            try
            {
                OrderHeader orderHeader = await _appDbContext.OrderHeaders.FirstAsync(o => o.OrderHeaderId == orderId);

                if (orderHeader != null)
                {
                    if (newStatus == (byte)SD.OrderStatus.CANCELLED)
                    {
                        // we will give refund
                        var option = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId,

                        };

                        var service = new RefundService();
                        Refund refund = service.Create(option);


                    }

                    orderHeader.Status = newStatus;
                    await _appDbContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = (byte)SD.OrderStatus.PENDING;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

                OrderHeader orderCreated = _appDbContext.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _appDbContext.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }


        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                };

                var DiscountsObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions()
                    {
                        Coupon=stripeRequestDto.OrderHeader.CouponCode
                    }
                };

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                            },


                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts = DiscountsObj;
                }

                var service = new SessionService();
                // creates a new session
                Session session = service.Create(options);
                // oue web application will know where we need to redirect to capture the payment
                stripeRequestDto.StripeSessionUrl = session.Url;
                // we can store the sessionid in the db so incase of refund or tacking if payment was successfull
                OrderHeader orderHeader = _appDbContext.OrderHeaders.First(d => d.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _appDbContext.SaveChangesAsync();
                _response.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }


        [Authorize]
        [HttpPost("VaildateStripeSession")]
        public async Task<ResponseDto> VaildateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _appDbContext.OrderHeaders.First(d => d.OrderHeaderId == orderHeaderId);
                var service = new SessionService();
                // need to create session
                Session session = service.Get(orderHeader.StripeSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent.Status == "succeeded")
                {
                    // payment was successfull
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = (byte)SD.OrderStatus.APPROVED;
                    await _appDbContext.SaveChangesAsync();
                    RewardDto rewardDto = new RewardDto()
                    {
                        OrderId = orderHeader.OrderHeaderId,
                        RewardActivity = Convert.ToInt32(orderHeader.OrderTotal),
                        UserId = orderHeader.UserId
                    };
                    string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

                    await _messageBus.PublishMessage(rewardDto, topicName);
                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }

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
