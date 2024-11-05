using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        // we need username password and hostname to setup and establish a connection to rabbit mq
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        public RabbitMQCartMessageSender()
        {
            _hostName = "localhost";
            _username = "guest";
            _password = "guest";
        }

        public void SendMessage(object message, string queueName)
        {
            if (ConnectionExist())
            {

                // we need to establish the cahannel to communicate or pass any message
                using var channel = _connection.CreateModel();

                // we have to declare queue
                channel.QueueDeclare(queueName, false, false, false, null);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // sending message
                channel.BasicPublish(exchange: "", routingKey: queueName, null, body: body);
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
