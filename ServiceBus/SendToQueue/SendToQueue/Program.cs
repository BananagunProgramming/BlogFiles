using System;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace SendToQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusSend();
            //ServiceTopicSend();
            //SendToEventHub();
        }

        private static void SendToEventHub()
        {
            var ehName = "davnaeventhub";
            var connection = "Endpoint=sb://<yournamespacehere>.servicebus.windows.net/;SharedAccessKeyName=Sender;SharedAccessKey=<yourkeyhere>;TransportType=Amqp";

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);

            EventHubClient client = factory.CreateEventHubClient(ehName);

            for (var i = 0; i < 10000; i++)
            {
                var message = i + " event hub message";

                EventData data = new EventData(Encoding.UTF8.GetBytes(message));

                client.Send(data);
   
                Console.WriteLine(message);
            }
        }

        private static void ServiceTopicSend()
        {
            var topicName = "topicdemo";
            var connection = "Endpoint=sb://<yournamespacehere>.servicebus.windows.net/;SharedAccessKeyName=Sender;SharedAccessKey=<yourkeyhere>;TransportType=Amqp";

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);

            TopicClient topic = factory.CreateTopicClient(topicName);

            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
                topic.Send(new BrokeredMessage("topic message" + i));
            }
        }

        private static void ServiceBusSend()
        {
            var qName = "76BusQueue";
            var connection = "Endpoint=sb://<yournamespacehere>.servicebus.windows.net/;SharedAccessKeyName=76BusSender;SharedAccessKey=<yourkeyhere>;TransportType=Amqp";

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);

            QueueClient queue = factory.CreateQueueClient(qName);

            for (var i = 0; i < 100; i++)
            {
                string message = "Thurman Thomas is old -" + DateTime.UtcNow.Ticks;
                BrokeredMessage bm = new BrokeredMessage(message);

                queue.Send(bm);

                Console.WriteLine(i);
            }
        }
    }
}
