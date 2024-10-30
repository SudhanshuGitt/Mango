using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;
        private readonly string? emailCartQueue;
        private readonly string? registerUserQueue;
        private readonly IConfiguration _configuration;
        private readonly ServiceBusProcessor _emailCartProcessor;
        private readonly ServiceBusProcessor _registerUserProcessor;
        private readonly EmailService _emailService;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");

            var client = new ServiceBusClient(serviceBusConnectionString);

            // if we need to listen queue we need processor
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);
            _registerUserProcessor = client.CreateProcessor(registerUserQueue);

        }

        // we need to register our handler for proccesor we created
        // this handler is responsible to proccessong the message that is recived from queue or topic

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

            _registerUserProcessor.ProcessMessageAsync += OnRegisterUserRequestReceived;
            _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _registerUserProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            await _registerUserProcessor.StopProcessingAsync();
            await _registerUserProcessor.DisposeAsync();
        }

        private async Task OnRegisterUserRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;

            var body = Encoding.UTF8.GetString(message.Body);
            string? email = JsonConvert.DeserializeObject<string>(Convert.ToString(body));
            try
            {
                // try to log email
                await _emailService.RegisterUserEmailAndLog(email);
                // it will tell this message is proccessed successfully and you can remove that from queue
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<Task> ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where we will recive the messgae

            var message = args.Message;

            var body = Encoding.UTF8.GetString(message.Body);
            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(body));
            try
            {
                // try to log email
                await _emailService.EmailCartAndLog(objMessage);
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
