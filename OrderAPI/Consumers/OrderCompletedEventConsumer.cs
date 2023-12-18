using MassTransit;
using OrderAPI.Context;
using OrderAPI.Models;
using Shared.OrderEvents;

namespace OrderAPI.Consumers
{
    public class OrderCompletedEventConsumer: IConsumer<OrderCompletedEvent>
    {
        public OrderDBContext _orderDBContext { get; set; }
        public OrderCompletedEventConsumer(OrderDBContext orderDBContext)
        {
            _orderDBContext = orderDBContext;
        }
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            // Event içinde gelen OrderId'ye ait Status'u Completed'a çekeceğiz.
            Order order = await _orderDBContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            { 
                order.OrderStatus = Enums.OrderStatus.Completed;
                await _orderDBContext.SaveChangesAsync();
            }
        }
    }
}
