using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.Message;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.RewardAPI.Services
{
    // we want to configure this API is whey api is running start will be invoked and if we stop Stop will be invoked
    // so we can add singleton dependency injection so in that way we ahve just one copy
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        // here we cannot use applicaton dbcontext using DI because that is scoped implementation
        // we need to registem implemention of email service that is singleton

        public async Task UpdateReward(RewardMessage rewardMessage)
        {
            try
            {
                Reward reward = new Reward()
                {
                    UserId = rewardMessage.UserId,
                    OrderId = rewardMessage.OrderId,
                    RewardActivity = rewardMessage.RewardActivity,
                    RewardDate = DateTime.Now
                };

                await using var _db = new AppDbContext(_dbOptions);

                await _db.Rewards.AddAsync(reward);
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
