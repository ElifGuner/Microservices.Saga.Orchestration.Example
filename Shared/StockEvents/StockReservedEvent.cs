using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.StockEvents
{
    // Servisler arasındaki iletişim artık servisler üzerinden değil,
    // State Machine adındaki merkezi yapılanmadan yürüyor.
    // Bu durumda OrderId yerine CorrelationId olacak. Çünkü State Machine
    // tek bir instance'a dair davranış sergilerken bunu OrderId üzerinden değil 
    // CorrelationId üzerinden koordine edecektir.
    public class StockReservedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }
        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
