using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.StockEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachine.Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        //State Machine'e gelecek olan eventler.
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        //State tanımları
        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            // DB'deki OrderId'ler ile OrderStartedEvent ile gelen OrderId'yi karşılaştır.
            // Eşleşme yoksa yeni bir correlationId oluştur select fonk. ile
            Event(() => OrderStartedEvent, // Eğer gelen event OrderStartedEvent ise
                OrderStateInstance => OrderStateInstance.CorrelateBy<int>(database =>
                    database.OrderId, @event=> @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid())); // yeni bir state instance üretmiş oluyoruz.

            Event(() => StockReservedEvent,
                OrderStateInstance => OrderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => StockNotReservedEvent,
                OrderStateInstance => OrderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentCompletedEvent,
                OrderStateInstance => OrderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentFailedEvent,
                OrderStateInstance => OrderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.UtcNow;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                    context => new OrderCreatedEvent(context.Instance.CorrelationId)
                    { 
                        OrderItems = context.Data.OrderItems
                    })
                );

            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                    context => new PaymentStartedEvent(context.Instance.CorrelationId)
                    {
                        TotalPrice = context.Instance.TotalPrice,
                        OrderItems = context.Data.OrderItems
                    }),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent
                    {
                        OrderId = context.Instance.OrderId,
                        Message = context.Data.Message
                    })
                );

            During(StockReserved,
                When(PaymentCompletedEvent)
                    .TransitionTo(PaymentCompleted)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                        context => new OrderCompletedEvent
                        {
                            OrderId = context.Instance.OrderId
                        })
                    .Finalize(), //Başarılı işlemden sonra bu fonk. çağrılır.
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent
                    {
                        OrderId = context.Instance.OrderId,
                        Message = context.Data.Message
                    })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                    context => new StockRollbackMessage
                    {
                        OrderItems = context.Data.OrderItems
                    })
                );
            SetCompletedWhenFinalized(); // Finalize'a çektiklerimizi DB'den sildirmek istiyorsak.
        }
    }
}
