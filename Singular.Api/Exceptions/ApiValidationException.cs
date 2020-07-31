using System;
using Balances;

namespace Singular.Api.Exceptions
{
    internal class ApiValidationException : Exception
    {
        public ErrorCode Error { get; }

        public ApiValidationException(ErrorCode errorCode)
        {
            Error = errorCode;
        }
    }
}