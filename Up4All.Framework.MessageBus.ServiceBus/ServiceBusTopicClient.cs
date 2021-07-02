
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicClient : MessageBusTopicClient, IServiceBusClient
    {
        private readonly TopicClient _client;

        public ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
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
            var client = new TopicClient(MessageBusOptions.ConnectionString, MessageBusOptions.TopicName, RetryPolicy.Default);
            return client;
        }
    }
}
