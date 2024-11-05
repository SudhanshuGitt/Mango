
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Models.Message;
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
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly string? orderCreatedExchange;
        private IConnection _connection;
        private IModel _channel;
        string queueName = string.Empty;

        // for direct exchange
        private const string ExchangeName = "";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            orderCreatedExchange = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare(orderCreatedExchange, ExchangeType.Fanout);
            // in fanout we need to create the queue and from that we can feth the message
            // we will fetch default queue name
            // it will automatically declare a queue in exchnage and return back the queue name
            //queueName = _channel.QueueDeclare().QueueName;
            // we need to bind the queue to channel
            //_channel.QueueBind(queueName, orderCreatedExchange, string.Empty);

            // for direct exchange
            _channel.ExchangeDeclare(orderCreatedExchange, ExchangeType.Direct);
            _channel.QueueDeclare(OrderCreated_EmailUpdateQueue, false, false, false, null);
            _channel.QueueBind(OrderCreated_EmailUpdateQueue, orderCreatedExchange, "EmailUpdateQueue");
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
                RewardMessage rewardMessage = JsonConvert.DeserializeObject<RewardMessage>(content);
                HandleMessage(rewardMessage).GetAwaiter().GetResult();

                // once we consume the message we need to return an acknowladgement message is recived and consumed
                _channel.BasicAck(e.DeliveryTag, false);

            };

            // we need to assign the event handler to channel
            // we are using basic comnsume as we are using standard queue with rabbit mq
            //_channel.BasicConsume(queueName, false, consumer);

            // for direct exchange
            _channel.BasicConsume(OrderCreated_EmailUpdateQueue, false, consumer);

            return Task.CompletedTask;

        }

        private async Task HandleMessage(RewardMessage rewardMessage)
        {
            await _emailService.LogPlacedOrder(rewardMessage);
        }
    }
}
