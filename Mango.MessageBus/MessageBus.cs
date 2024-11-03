using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string connectionString = "";

        //In the queue the FIFO first message send by client will recived by the reciever(point to point communication)
        //if there are multitple recivers who want to recive the message for that there is topic and subscription (publish subscriber senario)
        // when we publish a message there can be more than one sunbscribers in an queue
        // we will send messgae to topic and topic will have multitple subscription
        public async Task PublishMessage(object message, string topic_queue_Name)
        {
            await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(topic_queue_Name);

            var jsonMessage = JsonConvert.SerializeObject(message);
            // messgae we will send will be of type ServiceBusMessage
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                // identifier
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalMessage);
            await client.DisposeAsync();
        }
    }
}
