using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messages.Models;
using NotificationService.Services;

public class FraudAlertConsumer : IConsumer<FraudAlertMessage>
{
    private readonly EmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly ILogger<FraudAlertConsumer> _logger;

    public FraudAlertConsumer(EmailSender emailSender, IConfiguration config, ILogger<FraudAlertConsumer> logger)
    {
        _emailSender = emailSender;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FraudAlertMessage> context)
    {
        var alert = context.Message;
        _logger.LogInformation("📩 Received FraudAlertMessage for {TransactionId}", alert.TransactionId);

        var baseUrl = _config["BaseUrl"];
        var subject = "🚨 [FRAUD] Blocked Transaction Alert";

        var bodyText = $"""
        🚨 A transaction was flagged as fraudulent:

        Transaction ID: {alert.TransactionId}
        Sender: {alert.SenderEmail}
        Receiver: {alert.ReceiverEmail}
        Amount: ${alert.Amount}
        Time: {alert.Timestamp:yyyy-MM-dd HH:mm}
        Reason: {alert.Reason}

        ⚠️ This transaction is currently blocked.
        👉 Confirm: {baseUrl}/api/fraud/unblock/{alert.TransactionId}
        """;

        var htmlBody = $"""
        <h3>🚨 A transaction was flagged as <span style="color:red;">fraudulent</span>:</h3>
        <ul>
            <li><strong>Transaction ID:</strong> {alert.TransactionId}</li>
            <li><strong>Sender:</strong> {alert.SenderEmail}</li>
            <li><strong>Receiver:</strong> {alert.ReceiverEmail}</li>
            <li><strong>Amount:</strong> ${alert.Amount}</li>
            <li><strong>Time:</strong> {alert.Timestamp:yyyy-MM-dd HH:mm}</li>
            <li><strong>Reason:</strong> {alert.Reason}</li>
        </ul>
        <p style="color:red;"><strong>⚠️ This transaction is currently BLOCKED.</strong></p>
        <p>
            <a href="{baseUrl}/api/fraud/unblock/{alert.TransactionId}" 
               style="background-color:#28a745;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;">
               ✅ Confirm This Was Me
            </a>
        </p>
        """;

        await _emailSender.SendEmailAsync(alert.ReceiverEmail, subject, bodyText, htmlBody);
        _logger.LogInformation("📤 Email sent to {ReceiverEmail}", alert.ReceiverEmail);
    }
}