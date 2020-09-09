using Singular.CorePlatform.Common;
using Singular.CorePlatform.Game.EGT.Core.Models.Requests;
using Singular.CorePlatform.Game.EGT.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Interfaces
{
    public interface IIntegrationService
    {
        Task<OperationResult<GetBalanceResponse>> GetBalanceAsync(GetBalanceRequest request);

        Task<OperationResult<WithdrawResponse>> WithdrawAsync(WithdrawRequest request);

        Task<OperationResult<DepositResponse>> DepositAsync(DepositRequest request);

        Task<OperationResult<CheckTransactionStatusResponse>> CheckTransactionStatusAsync(CheckTransactionStatusRequest request);

        Task<OperationResult<RollbackResponse>> RollbackAsync(RollbackRequest request);

        Task<OperationResult<GetAwardDetailsResponse>> GetAwardDetailsAsync(GetAwardDetailsRequest request);

        Task<OperationResult> ConfirmAwardAsync(ConfirmAwardRequest request);
    }
}
