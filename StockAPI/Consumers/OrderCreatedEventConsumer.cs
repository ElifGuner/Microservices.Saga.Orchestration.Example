using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.Settings;
using Shared.StockEvents;
using StockAPI.Context;
using System;

namespace StockAPI.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        public StockDBContext _stockDBContext { get; set; }
        public ISendEndpointProvider _sendEndPointProvider { get; set; }
        public OrderCreatedEventConsumer(StockDBContext stockDBContext, ISendEndpointProvider sendEndPointProvider)
        {
            _stockDBContext = stockDBContext;
            _sendEndPointProvider = sendEndPointProvider;
        }
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            // OrderItem'ların hepsinin stock miktarlarını kontrol edeceğiz.
            // Stock eksik olan varsa false olacak.
            List<bool> stockResult = new();

            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await _stockDBContext.Stocks.AnyAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count));
            }

            var sendEndPoint = await _sendEndPointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await _stockDBContext.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId);
                    stock.Count -= orderItem.Count;
                    await _stockDBContext.SaveChangesAsync();
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };

                sendEndPoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok yetersiz..."
                };

                sendEndPoint.Send(stockNotReservedEvent);
            }
        }
    }
}
