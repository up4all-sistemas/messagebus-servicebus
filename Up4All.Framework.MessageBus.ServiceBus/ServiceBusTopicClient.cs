
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
        private readonly ServiceBusSender _topicClient;

        public ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _topicClient = CreateClient(messageOptions.Value);
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

        private ServiceBusSender CreateClient(MessageBusOptions opts)
        {
            var client = new ServiceBusClient(opts.ConnectionString);
            var topicClient = client.CreateSender(opts.TopicName);
            return topicClient;
        }
    }
}
