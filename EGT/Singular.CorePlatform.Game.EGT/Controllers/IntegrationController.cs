using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Singular.CorePlatform.Adapter.Models;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using Singular.CorePlatform.Game.EGT.Core.Models.Requests;
using Singular.CorePlatform.Game.EGT.Filters;
using Singular.CorePlatform.Game.EGT.Models.Requests;
using Singular.CorePlatform.Game.EGT.Models.Responses;
using Singular.CorePlatform.Persistence.Interfaces;

namespace Singular.CorePlatform.Game.EGT.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("game-egt/api/v{version:apiVersion}")]
    [ServiceFilter(typeof(CheckIpAttribute))]
    [ServiceFilter(typeof(CheckCredentialsAttribute))]
    [ServiceFilter(typeof(HandleExceptionAttribute))]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;

        private readonly ITokenFactory _tokenFactory;

        public IntegrationController(IIntegrationService integrationService, ITokenFactory tokenFactory)
        {
            _integrationService = integrationService;
            _tokenFactory = tokenFactory;
        }

        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("user/authenticate-user")]
        public async Task<OperationResult> AuthenticateUserAsync(EGTRequest request)
        {
            var tokenResult = await _tokenFactory.GetPersistentTokenAsync<CasinoUser>("request.Token");
            if (tokenResult.Data != null)
                await tokenResult.Data.DeleteAsync();

            if (tokenResult.ErrorCode == ErrorCodes.Success)
            {
                // TODO create new token if needed

                var getBalanceRequest = new GetBalanceRequest
                {
                    // TODO map properties from request
                };

                var result = await _integrationService.GetBalanceAsync(getBalanceRequest);
            }

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("transaction/withdraw")]
        public async Task<OperationResult> WithdrawAsync(EGTRequest request)
        {
            var withdrawRequest = new WithdrawRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.WithdrawAsync(withdrawRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("transaction/deposit")]
        public async Task<OperationResult> DepositAsync(EGTRequest request)
        {
            var depositRequest = new DepositRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.DepositAsync(depositRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        [HttpPost("transaction/deposit")]
        public async Task<OperationResult> WithdrawAndDepositAsync(EGTRequest request)
        {
            var depositRequest = new DepositRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.DepositAsync(depositRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        /*
        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("user/get-balance")]
        public async Task<OperationResult> GetBalanceAsync(EGTRequest request)
        {
            var getBalanceRequest = new GetBalanceRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.GetBalanceAsync(getBalanceRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("transaction/check-transaction-status")]
        public async Task<OperationResult> CheckTransactionStatusAsync(EGTRequest request)
        {
            var checkTransactionStatusRequest = new CheckTransactionStatusRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.CheckTransactionStatusAsync(checkTransactionStatusRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        /// <summary>
        /// This is default route. Route should changed per integration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("transaction/rollback")]
        public async Task<OperationResult> RollbackAsync(EGTRequest request)
        {
            var rollbackRequest = new RollbackRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.RollbackAsync(rollbackRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        [HttpPost("user/get-award-details")]
        public async Task<OperationResult> GetAwardDetailsAsync(EGTRequest request)
        {
            var getAwardDetailsRequest = new GetAwardDetailsRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.GetAwardDetailsAsync(getAwardDetailsRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }

        [HttpPost("user/confirm-award")]
        public async Task<OperationResult> ConfirmAwardAsync(EGTRequest request)
        {
            var confirmAwardRequest = new ConfirmAwardRequest
            {
                // TODO map properties from request
            };

            var result = await _integrationService.ConfirmAwardAsync(confirmAwardRequest);

            return new OperationResult<EGTResponse>
            {
                // TODO map properties from result
            };
        }
        */
    }
}