namespace TransactionService.Services
{
    public interface ITransactionService
    {
        Task<bool> UnblockTransactionAsync(Guid transactionId);
    }
}
