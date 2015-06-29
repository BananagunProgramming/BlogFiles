using System;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ReceiveFromQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusRecieve();
            //TopicReceive(2);
            //EventHubReceive();
        }

        private static void EventHubReceive()
        {
            var ehName = "davnaeventhub";
            var connection = "Endpoint=sb://alewife.servicebus.windows.net/;SharedAccessKeyName=Receiver;SharedAccessKey=3jaTeykwRu84x93OpW3DftRO1WSHt4oi7QB0FRyHoyg=;TransportType=Amqp";
            
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);

            EventHubClient ehub = factory.CreateEventHubClient(ehName);
            EventHubConsumerGroup group = ehub.GetDefaultConsumerGroup();
            EventHubReceiver receiver = group.CreateReceiver("1");

            while (true)
            {
                EventData data = receiver.Receive();

                if (data != null)
                {
                    var message = Encoding.UTF8.GetString(data.GetBytes());

                    Console.WriteLine("PartitionKey: {0}", data.PartitionKey);
                    Console.WriteLine("SequenceNumber: {0}", data.SequenceNumber);
                    Console.WriteLine(message);
                }
            }
        }

        private static void TopicReceive(int subscription)
        {
            var topicName = "topicdemo";
            var subToProcess = "Subscription" + subscription;
            var connection = "Endpoint=sb://alewife.servicebus.windows.net/;SharedAccessKeyName=Receiver;SharedAccessKey=Vqa3SwP4qH5/+8ikB4B/BNMPQCRDuV/AiUYsUofWebo=;TransportType=Amqp";

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);
            SubscriptionClient clientA = factory.CreateSubscriptionClient(topicName, subToProcess);

            while (true)
            {
                BrokeredMessage message = clientA.Receive();
                if (message != null)
                {
                    try
                    {
                        Console.WriteLine("MessageId:{0}", message.MessageId);
                        Console.WriteLine(message.GetBody<string>());
                        message.Complete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        message.Abandon();
                    }
                }
            }
        }

        private static void ServiceBusRecieve()
        {
            string qName = "76BusQueue";
            string connection =
                "Endpoint=sb://alewife.servicebus.windows.net/;SharedAccessKeyName=76BusReceiver;SharedAccessKey=/ot1r8HCUNnLScKABbOHharCzOBQqoNtvbxlfVKPyRQ=;TransportType=Amqp";

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(connection);

            QueueClient queue = factory.CreateQueueClient(qName);

            var counter = 1;
            while (true)
            {
                BrokeredMessage bm = queue.Receive();

                if (bm != null)
                {
                    try
                    {
                        Console.WriteLine("MessageId {0}", bm.MessageId);
                        Console.WriteLine(counter);
                        Console.WriteLine(bm.GetBody<string>());
                        bm.Complete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        bm.Abandon();
                    }
                }
                counter++;
            }
        }
    }
}
