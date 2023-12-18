using MassTransit;
using MassTransit.Transports;
using Shared.PaymentEvents;
using Shared.Settings;

namespace PaymentAPI.Consumers
{
    public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
    {
        public ISendEndpointProvider _sendEndPointProvider { get; set; }
        public PaymentStartedEventConsumer(ISendEndpointProvider sendEndPointProvider)
        {
            _sendEndPointProvider = sendEndPointProvider;
        }

        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndPoint = await _sendEndPointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                { 
                
                };
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Yetersiz Bakiye...",
                    OrderItems = context.Message.OrderItems
                };
            }
        }
    }
}
