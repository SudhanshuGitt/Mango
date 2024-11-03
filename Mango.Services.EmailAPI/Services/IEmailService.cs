using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Models.Message;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task RegisterUserEmailAndLog(string email); 
        Task LogPlacedOrder(RewardMessage rewardMessage);
    }
}
