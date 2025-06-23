using System.Text.Json.Serialization;

namespace Shared.Messages;

public class TransactionCreatedEvent
{
        public Guid TransactionId { get; set; }
        public string SenderAccountId { get; set; }
        public string ReceiverAccountId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

    public string? SenderEmail { get; set; } 
    public string? ReceiverEmail { get; set; } 
}
