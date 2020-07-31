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

            // transaction completed successfully
            if (increaseResult == ErrorCode.Success)
                return;

            if (increaseResult == ErrorCode.DuplicateTransactionId)
                throw new ApiValidationException(increaseResult);

            if (increaseResult == ErrorCode.UnknownError)
            {
                var transactionResult = destination.CheckTransaction(model.TransactionId);
                // transaction completed successfully
                if (transactionResult == ErrorCode.Success)
                    return;

                increaseResult = transactionResult;
            }

            // destination error, rollback source
            var rollbackResult = source.Rollback(model.TransactionId);

            // rollback successfully, transaction canceled
            if (rollbackResult == ErrorCode.Success)
                throw new ApiValidationException(increaseResult);

            if (rollbackResult == ErrorCode.UnknownError)
            {
                var rollbackTransactionResult = source.CheckTransaction(model.TransactionId);
                if (rollbackTransactionResult == ErrorCode.Success ||
                    rollbackTransactionResult == ErrorCode.TransactionAlreadyMarkedAsRollback)
                {
                    // rollback successfully, transaction canceled
                    throw new ApiValidationException(increaseResult);
                }

                //this should not happen
                throw new ApiValidationException(rollbackTransactionResult);
            }

            //this should not happen
            throw new ApiValidationException(rollbackResult);
        }
    }
}