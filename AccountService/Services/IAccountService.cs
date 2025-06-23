using System;
using System.Threading.Tasks;

namespace AccountService.Services
{
    public interface IAccountService
    {
        Task UpdateBalanceAsync(Guid accountId, decimal amount);
    }
}