using Microsoft.Extensions.Options;
using Singular.CorePlatform.Adapter.Interfaces;
using Singular.CorePlatform.Adapter.Models;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Common.Configuration;
using Singular.CorePlatform.FreeSpins.Abstracts;
using Singular.CorePlatform.FreeSpins.Dtos.Requests;
using Singular.CorePlatform.FreeSpins.Helpers;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using Singular.CorePlatform.Game.EGT.Core.Models.Requests;
using Singular.CorePlatform.Game.EGT.Core.Models.Responses;
using Singular.CorePlatform.Games.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Services
{
    public class IntegrationService : IIntegrationService
    {
        private readonly ICasinoUserAdapter _casinoUserAdapter;

        private readonly IPayAdapter _payAdapter;

        private readonly IGameDirectory _gameDirectory;

        private readonly IFreeSpins _freeSpins;

        private readonly IntegrationOptions _settings;

        private readonly IIntegrationInstance _integrationInstance;

        public IntegrationService(ICasinoUserAdapter casinoUserAdapter, IPayAdapter payAdapter, IGameDirectory gameDirectory, IFreeSpins freeSpins, IOptionsMonitor<IntegrationOptions> integrationOptionsMonitor, IIntegrationInstance integrationInstance)
        {
            _casinoUserAdapter = casinoUserAdapter;
            _payAdapter = payAdapter;
            _gameDirectory = gameDirectory;
            _freeSpins = freeSpins;
            _settings = integrationOptionsMonitor.Get(integrationInstance.Name);
            _integrationInstance = integrationInstance;
        }

        public async Task<OperationResult<GetBalanceResponse>> GetBalanceAsync(GetBalanceRequest request)
        {
            var gameId = 0u;
            if (!string.IsNullOrEmpty(request.GameId))
            {
                var gameResult = await _gameDirectory.GetGameByProviderGameId(_integrationInstance.IntegrationLabel, request.GameId);
                if (gameResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult<GetBalanceResponse>(gameResult.ErrorCode, gameResult.Message);

                gameId = gameResult.Data.GameId;
            }

            var platformCurrency = GetPlatformCurrency(request.Currency);
            if (platformCurrency == null)
                return new OperationResult<GetBalanceResponse>(AdapterErrorCodes.CurrencyNotFound, $"{request.Currency} currency is not supported");

            var balanceResult = await _casinoUserAdapter.GetBalanceAsync(new Adapter.Models.Requests.BalanceRequest((uint)request.UserId, platformCurrency, gameId, false, _integrationInstance.AdapterConfigurationPath));
            if (balanceResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult<GetBalanceResponse>(balanceResult.ErrorCode, balanceResult.Message);

            var providerCurrency = GetProviderCurrency(balanceResult.Data.Currency);
            if (providerCurrency == null)
                return new OperationResult<GetBalanceResponse>(AdapterErrorCodes.CurrencyNotFound, $"{request.Currency} currency is not supported");

            var response = new GetBalanceResponse
            {
                Balance = balanceResult.Data.BalanceAmount + balanceResult.Data.LockedAmount + balanceResult.Data.BonusAmount,
                Currency = providerCurrency
            };

            return new OperationResult<GetBalanceResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult<WithdrawResponse>> WithdrawAsync(WithdrawRequest request)
        {
            uint? gameId = null;
            if (!string.IsNullOrEmpty(request.GameId))
            {
                var gameResult = await _gameDirectory.GetGameByProviderGameId(_integrationInstance.IntegrationLabel, request.GameId);
                if (gameResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult<WithdrawResponse>(gameResult.ErrorCode, gameResult.Message);

                gameId = gameResult.Data.GameId;
            }

            var platformCurrency = GetPlatformCurrency(request.Currency);
            if (platformCurrency == null)
                return new OperationResult<WithdrawResponse>(AdapterErrorCodes.CurrencyNotFound, $"{request.Currency} currency is not supported");

            var adapterWithdrawRequest = new Adapter.Models.Requests.WithdrawRequest(
                (uint)request.UserId,
                request.Amount,
                0,
                platformCurrency,
                false,
                request.ProviderTransactionId,
                request.RoundId,
                gameId,
                null,
                request.AdditionalData,
                request.Channel,
                request.ClientIp,
                request.RoundId,
                _integrationInstance.AdapterConfigurationPath);
            var withdrawResult = await _payAdapter.WithdrawAsync(adapterWithdrawRequest);
            if (withdrawResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult<WithdrawResponse>(withdrawResult.ErrorCode, withdrawResult.Message);

            var response = new WithdrawResponse
            {
                TransactionId = withdrawResult.Data.CoreTransactionID
            };

            return new OperationResult<WithdrawResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult<DepositResponse>> DepositAsync(DepositRequest request)
        {
            uint? gameId = null;
            if (!string.IsNullOrEmpty(request.GameId))
            {
                var gameResult = await _gameDirectory.GetGameByProviderGameId(_integrationInstance.IntegrationLabel, request.GameId);
                if (gameResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult<DepositResponse>(gameResult.ErrorCode, gameResult.Message);

                gameId = gameResult.Data.GameId;
            }

            var platformCurrency = GetPlatformCurrency(request.Currency);
            if (platformCurrency == null)
                return new OperationResult<DepositResponse>(AdapterErrorCodes.CurrencyNotFound, $"{request.Currency} currency is not supported");

            string destinationCurrency = null;
            if (_settings.CurrencyConversions != null && _settings.CurrencyConversions.ContainsValue(request.Currency))
            {
                var userInfoResult = await _casinoUserAdapter.GetUserInfoAsync(new Adapter.Models.Requests.UserInfoRequest((uint)request.UserId, null, _integrationInstance.AdapterConfigurationPath));
                if (userInfoResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult<DepositResponse>(userInfoResult.ErrorCode, userInfoResult.Message);

                if (userInfoResult.Data.PreferredCurrency != platformCurrency)
                    destinationCurrency = userInfoResult.Data.PreferredCurrency;
            }

            var adapterDepositRequest = new Adapter.Models.Requests.DepositRequest(
                (uint)request.UserId,
                request.Amount,
                0,
                platformCurrency,
                destinationCurrency,
                false,
                false,
                request.ProviderTransactionId,
                request.RoundId,
                gameId,
                null,
                request.AdditionalData,
                request.Channel,
                request.ClientIp,
                request.RoundId,
                _integrationInstance.AdapterConfigurationPath);
            var depositResult = await _payAdapter.DepositAsync(adapterDepositRequest);
            if (depositResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult<DepositResponse>(depositResult.ErrorCode, depositResult.Message);

            var response = new DepositResponse
            {
                TransactionId = depositResult.Data.CoreTransactionID
            };

            return new OperationResult<DepositResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult<CheckTransactionStatusResponse>> CheckTransactionStatusAsync(CheckTransactionStatusRequest request)
        {
            var adapterGetTransactionRequest = new Adapter.Models.Requests.GetTransactionRequest(request.ProviderTransactionId, false, _integrationInstance.AdapterConfigurationPath);
            var getTransactionResult = await _payAdapter.GetTransactionAsync(adapterGetTransactionRequest);
            if (getTransactionResult.ErrorCode != ErrorCodes.Success && getTransactionResult.ErrorCode != AdapterErrorCodes.TransactionStatusRollback)
                return new OperationResult<CheckTransactionStatusResponse>(getTransactionResult.ErrorCode, getTransactionResult.Message);

            var response = new CheckTransactionStatusResponse
            {
                UserId = (int)getTransactionResult.Data.UserID,
                TransactionId = getTransactionResult.Data.CoreTransactionID
            };

            return new OperationResult<CheckTransactionStatusResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult<RollbackResponse>> RollbackAsync(RollbackRequest request)
        {
            var adapterRollbackRequest = new Adapter.Models.Requests.RollbackRequest(request.ProviderTransactionId, false, null, _integrationInstance.AdapterConfigurationPath);
            var rollbackTransactionResult = await _payAdapter.RollbackTransactionAsync(adapterRollbackRequest);
            if (rollbackTransactionResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult<RollbackResponse>(rollbackTransactionResult.ErrorCode, rollbackTransactionResult.Message);

            var response = new RollbackResponse();

            return new OperationResult<RollbackResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult<GetAwardDetailsResponse>> GetAwardDetailsAsync(GetAwardDetailsRequest request)
        {
            var freeSpinResult = await _freeSpins.GetAwardInfoAsync(new GetAwardInfoDto((uint)request.UserId, (uint)request.AwardId));
            if (freeSpinResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult<GetAwardDetailsResponse>(freeSpinResult.ErrorCode, freeSpinResult.Message);

            var response = new GetAwardDetailsResponse
            {
                AvailableGames = freeSpinResult.Data.AvailableGames.ToArray(),
                AwardType = "freespin",
                BetAmount = freeSpinResult.Data.BetAmountPerLine * freeSpinResult.Data.NumberOfLines,
                BetPerLine = freeSpinResult.Data.BetAmountPerLine,
                Currency = freeSpinResult.Data.Currency,
                ExpiryDate = freeSpinResult.Data.ExpiryDate,
                MaxBetAmount = freeSpinResult.Data.MaxBetAmount,
                NumberOfFreeGames = freeSpinResult.Data.NumberOfFreeSpins,
                NumberOfLines = freeSpinResult.Data.NumberOfLines,
                ProviderBonusId = freeSpinResult.Data.ProviderBonusId,
                ValidityPeriod = null
            };

            return new OperationResult<GetAwardDetailsResponse>(ErrorCodes.Success, null, response);
        }

        public async Task<OperationResult> ConfirmAwardAsync(ConfirmAwardRequest request)
        {
            var freeSpinResult = await _freeSpins.UpdateFreeSpinsAsync(new UpdateFreeSpinDto(FreeSpinStatus.Awarded, new uint[] { (uint)request.AwardId }));
            if (freeSpinResult.ErrorCode != ErrorCodes.Success)
                return new OperationResult(freeSpinResult.ErrorCode, freeSpinResult.Message);

            return new OperationResult(ErrorCodes.Success, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetPlatformCurrency(string providerCurrency)
        {
            return _settings.SupportedCurrencies?.FirstOrDefault(c => c.Value == providerCurrency).Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetProviderCurrency(string platformCurrency)
        {
            return _settings.SupportedCurrencies.GetValueOrDefault(platformCurrency);
        }
    }
}
