using FraudDetectionService.Consumers;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FraudDetectionConsumer>(); // Listens for TransactionCreatedEvent

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("transaction-initiated-queue", e =>
        {
            e.ConfigureConsumer<FraudDetectionConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
