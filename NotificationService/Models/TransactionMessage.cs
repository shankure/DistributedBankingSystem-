namespace NotificationService.Models
{
    public class TransactionMessage
    {
        public string? TransactionId { get; set; }
        public string? SenderEmail { get; set; }
        public string? ReceiverEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
