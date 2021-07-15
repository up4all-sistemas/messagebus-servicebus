
using Microsoft.Azure.ServiceBus;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IServiceBusClient
    {
        private readonly TopicClient _client;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName) : base(connectionString, topicName)
        {
            _client = CreateClient();
        }

        public override async Task Send(MessageBusMessage message)
        {
            await _client.SendAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _client.SendAsync(sbMessages.ToList());
        }

        private TopicClient CreateClient()
        {
            var client = new TopicClient(ConnectionString, TopicName, RetryPolicy.Default);
            return client;
        }
    }
}
