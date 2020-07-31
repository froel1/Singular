using Balances;
using Singular.Api.Exceptions;
using Singular.Api.Helpers;
using Singular.Api.Interfaces;
using Singular.Api.Models;

namespace Singular.Api.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly ServiceResolver _serviceResolver;

        public BalanceService(ServiceResolver serviceResolver) => _serviceResolver = serviceResolver;

        public decimal GetBalance() => _serviceResolver(BalanceSource.Casino).GetBalance();

        public void Withdraw(TransactionModel model)
        {
            var source = _serviceResolver(BalanceSource.Casino);
            var destination = _serviceResolver(BalanceSource.Game);
            Transfer(source, destination, model);
        }

        public void Deposit(TransactionModel model)
        {
            var source = _serviceResolver(BalanceSource.Game);
            var destination = _serviceResolver(BalanceSource.Casino);
            Transfer(source, destination, model);
        }

        private static void Transfer(IBalanceManager source, IBalanceManager destination, TransactionModel model)
        {
            //decrease balance from source
            var decreaseResult = source.DecreaseBalance(model.Amount, model.TransactionId);

            if (decreaseResult == ErrorCode.UnknownError)
            {
                var transactionResult = source.CheckTransaction(model.TransactionId);
                if (transactionResult != ErrorCode.Success)
                    throw new ApiValidationException(transactionResult);

                decreaseResult = ErrorCode.Success;
            }

            if (decreaseResult != ErrorCode.Success)
                throw new ApiValidationException(decreaseResult);

            // increase balance to destination
            var increaseResult = destination.IncreaseBalance(model.Amount, model.TransactionId);
            if (increaseResult == ErrorCode.Success)
                return;

            if (increaseResult == ErrorCode.DuplicateTransactionId)
                throw new ApiValidationException(increaseResult);

            if (increaseResult == ErrorCode.UnknownError)
            {
                var transactionResult = destination.CheckTransaction(model.TransactionId);
                if (transactionResult == ErrorCode.Success)
                    return;

                increaseResult = transactionResult;
            }

            var rollbackResult = source.Rollback(model.TransactionId);
            if (rollbackResult == ErrorCode.Success)
                throw new ApiValidationException(increaseResult);

            throw new ApiValidationException(rollbackResult);
        }
    }
}