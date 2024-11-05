
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    // background service will automatically start the service
    public class RabbitMQCartConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly string? emailCartQueue;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCartConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(emailCartQueue, false, false, false, null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // it will stop right there if cancellation is requested
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            // event is fired ehrn delivery arrives for consumer

            consumer.Received += (ch, e) =>
            {
                var content = Encoding.UTF8.GetString(e.Body.ToArray());
                CartDto cart = JsonConvert.DeserializeObject<CartDto>(content);
                HandleMessage(cart).GetAwaiter().GetResult();

                // once we consume the message we need to return an acknowladgement message is recived and consumed
                _channel.BasicAck(e.DeliveryTag, false);

            };

            // we need to assign the event handler to channel
            // we are using basic comnsume as we are using standard queue with rabbit mq
            _channel.BasicConsume(emailCartQueue, false, consumer);

            return Task.CompletedTask;

        }

        private async Task HandleMessage(CartDto cart)
        {
            await _emailService.EmailCartAndLog(cart);
        }
    }
}
