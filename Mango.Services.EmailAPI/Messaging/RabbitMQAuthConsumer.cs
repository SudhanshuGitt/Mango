
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
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly string? registerUserQueue;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(registerUserQueue, false, false, false, null);
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
                string email = JsonConvert.DeserializeObject<string>(content);
                HandleMessage(email).GetAwaiter().GetResult();

                // once we consume the message we need to return an acknowladgement message is recived and consumed
                _channel.BasicAck(e.DeliveryTag, false);

            };

            // we need to assign the event handler to channel
            // we are using basic comnsume as we are using standard queue with rabbit mq
            _channel.BasicConsume(registerUserQueue, false, consumer);

            return Task.CompletedTask;

        }

        private async Task HandleMessage(string email)
        {
            await _emailService.RegisterUserEmailAndLog(email);
        }
    }
}
