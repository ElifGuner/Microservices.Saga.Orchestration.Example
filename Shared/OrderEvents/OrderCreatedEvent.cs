using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.OrderEvents
{
    // Bir siparişi başlattıktan sonra oluşturduğumuzu ifade eden event
    // Oluşturulmuş olan siparişe ait bilgileri ilgili servislere göndereceğiz.
    // Sipariş oluşturulduğu takdirde, Saga State Machine'de bir correlation Id tanımlamamız gerekecek.
    // Bunun için CorrelatedBy interface'inden istifa edeceğiz.Generic parametresine Id'nin türünü veriyoruz.
    // Bunu kullanmak için MassTransit kütüphanesi yükleyeceğiz.
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; }
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
