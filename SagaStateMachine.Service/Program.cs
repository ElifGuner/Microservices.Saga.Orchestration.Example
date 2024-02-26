using SagaStateMachine.Service;
using MassTransit;
using SagaStateMachine.Service.StateMachine;
using SagaStateMachine.Service.StateInstances;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using Shared.Settings;
/*
IHost host = Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
services.AddHostedService<Worker>();
})
.Build();
*/

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
    .EntityFrameworkRepository(options => 
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder)
            =>
        {
            _builder.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer"));
        });
    });

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.StateMachineQueue, e => e.ConfigureSaga<OrderStateInstance>(context));
    });
});

var host = builder.Build();
host.Run();
