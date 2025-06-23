using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Messages.Models
{
    public class FraudAlertMessage
    {
        public Guid TransactionId { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsBlocked { get; set; } = true;
    }
}

public class Wrapper<T>
{
    public T Message { get; set; }
}
