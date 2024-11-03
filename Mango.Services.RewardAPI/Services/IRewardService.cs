using Mango.Services.RewardAPI.Models.Message;

namespace Mango.Services.RewardAPI.Services
{
    public interface IRewardService
    {
        Task UpdateReward(RewardMessage rewardMessage);
    }
}
