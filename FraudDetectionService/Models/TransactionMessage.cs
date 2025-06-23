namespace FraudDetectionService.Models
{
    public class TransactionMessage
    {
        public Guid TransactionId { get; set; } 
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
