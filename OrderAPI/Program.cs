using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Context;
using OrderAPI.Models;
using OrderAPI.ViewModels;
using OrderAPI.Enums;
using Shared.OrderEvents;
using Shared.Messages;
using MassTransit.Transports;
using Shared.Settings;
using OrderAPI.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCompletedEventConsumer>();
    configurator.AddConsumer<OrderFailedEventConsumer>();

    configurator.UsingRabbitMq((context, _configure) => 
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue,e => 
                e.ConfigureConsumer<OrderCompletedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue, e => 
                e.ConfigureConsumer<OrderFailedEventConsumer>(context));
    });
});


builder.Services.AddDbContext<OrderDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order",async (CreateOrderVM model, OrderDBContext context, SendEndpointProvider sendEndpointProvider) =>
{
    Order order = new()
    {
        BuyerId = model.BuyerId,
        OrderStatus = OrderStatus.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new OrderItem
        {
            ProductId = oi.ProductId,
            Count = oi.Count,
            Price = oi.Price,
        }).ToList(),
    };
    
    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        OrderId = order.Id,
        BuyerId = order.BuyerId,
        TotalPrice = order.TotalPrice,
        OrderItems = model.OrderItems.Select(oi => new OrderItemMessage
        {
            ProductId = oi.ProductId,
            Count = oi.Count,
            Price = oi.Price,
        }).ToList()
    };

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
    await sendEndpoint.Send<OrderStartedEvent>(orderStartedEvent);

});

app.Run();
