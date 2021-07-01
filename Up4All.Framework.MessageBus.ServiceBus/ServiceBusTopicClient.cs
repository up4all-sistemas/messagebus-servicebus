using Azure.Messaging.ServiceBus;

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
        private readonly ServiceBusClient _client;

        public ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _client = this.GetConnection(MessageBusOptions);
        }

        public override async Task Send(MessageBusMessage message)
        {
            var sender = _client.CreateSender(MessageBusOptions.TopicName);
            await sender.SendMessageAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sender = _client.CreateSender(MessageBusOptions.TopicName);
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await sender.SendMessagesAsync(sbMessages);
        }
    }
}
