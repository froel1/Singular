using Singular.Api.Models;

namespace Singular.Api.Interfaces
{
    public interface IBalanceService
    {
        void Withdraw(TransactionModel model);

        void Deposit(TransactionModel model);

        decimal GetBalance();
    }
}