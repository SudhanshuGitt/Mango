using Mango.Services.RewardAPI.Messaging;
using System.Reflection.Metadata;

namespace Mango.Services.RewardAPI.Extension
{
    public static class ApllicationBuilderExtension
    {
        private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder builder)
        {

            ServiceBusConsumer = builder.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            // notify use about the appplication life time when its started and stopped
            var hostApplicationLife = builder.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(OnStart);
            hostApplicationLife.ApplicationStopped.Register(OnStop);

            return builder;

        }

        private static void OnStop()
        {
            ServiceBusConsumer.Stop();
        }

        private static void OnStart()
        {
            ServiceBusConsumer.Start();
        }
    }
}
