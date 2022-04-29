
using Azure.Messaging.ServiceBus;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IServiceBusClient
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _topicClient;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            (_client,_topicClient) = this.CreateClient(connectionString, topicName, connectionAttempts);
        }

        public override async Task Send(MessageBusMessage message)
        {
            await _topicClient.SendMessageAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _topicClient.SendMessagesAsync(sbMessages.ToList());
        }

    }
}
