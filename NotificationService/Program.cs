using MassTransit;
using NotificationService.Consumers;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Register core services
builder.Services.AddSingleton<EmailSender>();
//builder.Services.AddHostedService<TransactionConsumer>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FraudAlertConsumer>();
    x.AddConsumer<TransactionCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("fraudulent-transactions", e =>
        {
            e.ConfigureConsumer<FraudAlertConsumer>(context);
        });
        cfg.ReceiveEndpoint("transaction-created-queue", e =>
        {
            e.ConfigureConsumer<TransactionCreatedConsumer>(context);
        });
    });
});


var app = builder.Build();

app.MapGet("/", () => "NotificationService running...");
app.Run();