using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
    public class TransactionInitiatedEvent
    {
        public Guid TransactionId { get; set; }
        public string SenderAccountId { get; set; } = string.Empty;
        public string? ReceiverAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public string? SenderEmail { get; set; }
        public string? ReceiverEmail { get; set; }
    }
}
