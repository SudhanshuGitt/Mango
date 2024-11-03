using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.Message;
using Mango.Services.RewardAPI.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;
        private readonly string? orderCreatedTopic;
        private readonly string? orderCreatedRewardSubscription;
        private readonly IConfiguration _configuration;
        private readonly ServiceBusProcessor _rewardProcessor;
        private readonly RewardService _rewardService;

        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

            var client = new ServiceBusClient(serviceBusConnectionString);

            // if we need to listen topic we need processor
            _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);

        }

        // we need to register our handler for proccesor we created
        // this handler is responsible to proccessong the message that is recived from queue or topic

        public async Task Start()
        {
            _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardRequestReceived;
            _rewardProcessor.ProcessErrorAsync += ErrorHandler;
            await _rewardProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _rewardProcessor.StopProcessingAsync();
            await _rewardProcessor.DisposeAsync();

        }

        private async Task<Task> ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnNewOrderRewardRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where we will recive the messgae

            var message = args.Message;

            var body = Encoding.UTF8.GetString(message.Body);
            RewardMessage objMessage = JsonConvert.DeserializeObject<RewardMessage>(Convert.ToString(body));
            try
            {
                // try to log email
                await _rewardService.UpdateReward(objMessage);
                // it will tell this message is proccessed successfully and you can remove that from queue
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


    }
}
