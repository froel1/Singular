using System.Net;
using Balances;

namespace Singular.Api.Helpers
{
    public static class Extensions
    {
        public static HttpStatusCode ToHttpStatusCode(this ErrorCode code) => code switch
        {
            ErrorCode.Success => HttpStatusCode.OK,
            ErrorCode.TransactionRejected => HttpStatusCode.BadRequest,
            ErrorCode.NotEnoughtBalance => HttpStatusCode.PreconditionFailed,
            ErrorCode.DuplicateTransactionId => HttpStatusCode.Conflict,
            ErrorCode.TransactionNotFound => HttpStatusCode.BadRequest,
            ErrorCode.TransactionAlreadyMarkedAsRollback => HttpStatusCode.Conflict,
            ErrorCode.TransactionRollbacked => HttpStatusCode.InternalServerError,
            ErrorCode.UnknownError => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError
        };

        public static string ToResponsePhrase(this ErrorCode code) => code switch
        {
            ErrorCode.Success => string.Empty,
            ErrorCode.TransactionRejected => Resources.ErrorCodeResource.TransactionRejected,
            ErrorCode.NotEnoughtBalance => Resources.ErrorCodeResource.NotEnoughtBalance,
            ErrorCode.DuplicateTransactionId => Resources.ErrorCodeResource.DuplicateTransactionId,
            ErrorCode.TransactionNotFound => Resources.ErrorCodeResource.TransactionNotFound,
            ErrorCode.TransactionAlreadyMarkedAsRollback => Resources.ErrorCodeResource.TransactionAlreadyMarkedAsRollback,
            ErrorCode.TransactionRollbacked => Resources.ErrorCodeResource.TransactionRollbacked,
            ErrorCode.UnknownError => Resources.ErrorCodeResource.UnknownError,
            _ => string.Empty
        };
    }
}