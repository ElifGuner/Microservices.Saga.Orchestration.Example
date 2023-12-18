using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using StockAPI.Context;
using StockAPI.Models;

namespace StockAPI.Consumers
{
    public class StockRollbackMessageConsumer : IConsumer<StockRollbackMessage>
    {
        public StockDBContext _stockDBContext { get; set; }
        public StockRollbackMessageConsumer(StockDBContext stockDBContext)
        {
            _stockDBContext = stockDBContext;
        }
        public async Task Consume(ConsumeContext<StockRollbackMessage> context)
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                Stock stock = await _stockDBContext.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId);
                stock.Count += orderItem.Count;
                await _stockDBContext.SaveChangesAsync();
            }
        }
    }
}
