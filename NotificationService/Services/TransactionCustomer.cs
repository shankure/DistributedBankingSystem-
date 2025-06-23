using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Models;
using Shared.Messages;

namespace NotificationService.Services
{
    public class TransactionConsumer : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public TransactionConsumer(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:Host"] };

            IConnection? connection = null;
            int retries = 10;

            while (connection == null && retries > 0)
            {
                try
                {
                    connection = factory.CreateConnection();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] Retry in 3s... ({10 - retries + 1}/10)");
                    retries--;
                    Thread.Sleep(3000);

                    if (retries == 0)
                    {
                        Console.WriteLine("[RabbitMQ] Failed to connect after 10 attempts.");
                        throw;
                    }
                }
            }

            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "transaction-created-queue", durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[RabbitMQ] Message received: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var wrapper = JsonSerializer.Deserialize<Wrapper<TransactionCreatedEvent>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    var transaction = wrapper?.Message;

                    if (transaction == null || string.IsNullOrWhiteSpace(transaction.SenderAccountId))
                    {
                        Console.WriteLine("[ERROR] Invalid transaction or SenderAccountId is null. Skipping.");
                        return;
                    }

                    using var scope = _services.CreateScope();
                    var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                    var subject = $"Transaction Alert: ${transaction.Amount} Sent";
                    var message = $"You sent ${transaction.Amount} to {transaction.ReceiverAccountId} on {transaction.Timestamp}.";

                    // Add HTML version for improved formatting
                    var htmlMessage = $"<p>{message}</p>";

                    await emailSender.SendEmailAsync(transaction.SenderAccountId, subject, message, htmlMessage);
                    Console.WriteLine($"[Email] Sent to {transaction.SenderAccountId}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to process message: {ex.Message}");
                }
            };


            channel.BasicConsume(queue: "transaction-created-queue", autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}