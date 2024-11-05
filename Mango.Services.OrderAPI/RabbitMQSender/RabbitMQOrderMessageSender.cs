using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    //Fanout exchange we eill not send a meesga to queue but to an exchange
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        // we need username password and hostname to setup and establish a connection to rabbit mq
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        // for direct exchange
        private const string OrderCreated_RewardUpdateQueue = "RewardUpdateQueue";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public RabbitMQOrderMessageSender()
        {
            _hostName = "localhost";
            _username = "guest";
            _password = "guest";
        }

        public void SendMessage(object message, string exchangeName)
        {
            if (ConnectionExist())
            {

                // we need to establish the cahannel to communicate or pass any message
                using var channel = _connection.CreateModel();

                // we have to declare exhange
                // durable means if application restarts that exhange will remain
                //channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: false);

                // for direct exchange
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: false);
                channel.QueueDeclare(OrderCreated_EmailUpdateQueue, false, false, false, null);
                channel.QueueDeclare(OrderCreated_RewardUpdateQueue, false, false, false, null);
                // we need to bind the to our exhange
                // using exhange key it will publish to particular queue
                channel.QueueBind(OrderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");
                channel.QueueBind(OrderCreated_RewardUpdateQueue, exchangeName, "RewardUpdate");
                //


                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // sending message
                //channel.BasicPublish(exchange: exchangeName, string.Empty, null, body: body);

                // for direct exchange
                // it will publish to indivigul queue that we have binded and published here
                channel.BasicPublish(exchange: exchangeName, "EmailUpdate", null, body: body);
                channel.BasicPublish(exchange: exchangeName, "RewardUpdate", null, body: body);
            }
        }

        // previously whenever we need to send the message we were creating new connection every time
        // if connection already exist we msut reuse that

        private void CreateConnection()
        {
            try
            {
                // we need to create connection factoey
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _username
                };

                // it will establish the connection to RabbitMQ factory
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
            }
        }

        private bool ConnectionExist()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();
            return true;
        }
    }
}
