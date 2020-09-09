using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Singular.CorePlatform.Adapter;
using Singular.CorePlatform.Adapter.Interfaces;
using Singular.CorePlatform.Adapter.Models;
using Singular.CorePlatform.Adapter.Models.Requests;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Common.Game.Requests;
using Singular.CorePlatform.Common.Game.Responses;
using Singular.CorePlatform.FreeSpins.Abstracts;
using Singular.CorePlatform.FreeSpins.Dtos.Requests;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using Singular.CorePlatform.Game.EGT.Models;
using Singular.CorePlatform.Persistence.Interfaces;

namespace Singular.CorePlatform.Game.EGT.Controllers
{
    [ApiController]
    [ApiVersionNeutral]
    public class GameController : ControllerBase
    {
        private readonly ICasinoUserAdapter _casinoUserAdapter;

        private readonly IFreeSpins _freeSpins;

        private readonly ITokenFactory _tokenFactory;

        private readonly EGTSettings _settings;

        private readonly IIntegrationInstance _integrationInstance;

        public GameController(ICasinoUserAdapter casinoUserAdapter, IFreeSpins freeSpins, ITokenFactory tokenFactory, IOptionsMonitor<EGTSettings> egtOptionsMonitor, IIntegrationInstance integrationInstance)
        {
            _casinoUserAdapter = casinoUserAdapter;
            _freeSpins = freeSpins;
            _tokenFactory = tokenFactory;
            _settings = egtOptionsMonitor.Get(integrationInstance.Name);
            _integrationInstance = integrationInstance;
        }

        [HttpPost("/api/init")]
        public async Task<OperationResult> InitAsync(InitGameRequest request)
        {
            var currency = "";
            var token = "";
            var awardId = "";

            if (!request.IsDemoPlay)
            {
                var authResult = await _casinoUserAdapter.AuthAsync(new AuthRequest(Guid.Parse(request.Token), IntegrationLabels.Sis));
                if (authResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult(authResult.ErrorCode, authResult.Message);

                currency = _settings.SupportedCurrencies?.GetValueOrDefault(authResult.Data.PreferredCurrency) ?? _settings.CurrencyConversions?.GetValueOrDefault(authResult.Data.PreferredCurrency);
                if (currency == null)
                    return new OperationResult(AdapterErrorCodes.CurrencyNotFound, $"{authResult.Data.PreferredCurrency} currency is not supported");

                var persistentToken = _tokenFactory.GetPersistentToken(authResult.Data);
                var persistResult = await persistentToken.PersistAsync((uint)_settings.Extended.TokenTtl);
                if (persistResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResult(persistResult.ErrorCode, persistResult.Message);

                token = persistentToken.Token;

                var freeSpinResult = await _freeSpins.GetFreeSpinAsync(new GetFreeSpinDto(authResult.Data.UserID, request.GameId.ToString(), _integrationInstance.IntegrationLabel, authResult.Data.PreferredCurrency));
                if (freeSpinResult.ErrorCode == ErrorCodes.Success)
                {
                    awardId = freeSpinResult.Data.BonusId.ToString();
                    // TODO Maybe we need to change freespin status to init
                    // TODO send bonuses award to provider url if needed and set bonusAwarded value accordingly
                    var bonusAwarded = true;

                    if (bonusAwarded)
                    {
                        var updateResult = await _freeSpins.UpdateFreeSpinsAsync(new UpdateFreeSpinDto(FreeSpins.Helpers.FreeSpinStatus.Awarded, new uint[] { freeSpinResult.Data.BonusId }));
                        if (updateResult.ErrorCode != ErrorCodes.Success)
                            return new OperationResult(updateResult.ErrorCode, updateResult.Message);
                    }
                }
            }

            var launchUrl = _settings.Extended.LaunchUrl
                .Replace("{BRAND}", request.Brand)
                .Replace("{CURRENCY}", currency)
                .Replace("{GAME_ID}", request.LaunchParameters.GameId)
                .Replace("{TOKEN}", token)
                .Replace("{LANGUAGE}", request.Language)
                .Replace("{CHANNEL}", ((int)request.Channel).ToString())
                .Replace("{DEMO_PLAY}", request.IsDemoPlay.ToString().ToLower())
                .Replace("{EXIT_URL}", request.ExitUrl)
                .Replace("{CASHIER_URL}", request.CashierUrl)
                .Replace("{AWARD_ID}", awardId);

            return new OperationResult<InitGameResponse>(ErrorCodes.Success, null, new InitGameResponse(launchUrl));
        }

        private string GetLaunchUrl(InitGameRequest request, CasinoUser user, string currency, string token, string awardId)
        {
            /*var launchUrl = _settings.Extended.LaunchUrl
                .Replace("{BRAND}", request.Brand)
                .Replace("{CURRENCY}", currency)
                .Replace("{GAME_ID}", request.LaunchParameters.GameId)
                .Replace("{TOKEN}", token)
                .Replace("{LANGUAGE}", request.Language)
                .Replace("{CHANNEL}", ((int)request.Channel).ToString())
                .Replace("{DEMO_PLAY}", request.IsDemoPlay.ToString().ToLower())
                .Replace("{EXIT_URL}", request.ExitUrl)
                .Replace("{CASHIER_URL}", request.CashierUrl)
                .Replace("{AWARD_ID}", awardId);*/

            var uriBuilder = new UriBuilder(new Uri(_settings.Extended.LaunchUrl));
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query["defenceCode"] = token; //TODO: ?
            query["playerId"] = user.UserID.ToString();
            query["portalCode"] = request.LaunchParameters.GameId; //TODO: ?
            query["screenName"] = user.UserName;
            query["language"] = request.Language;
            query["country"] = user.TransactionLimit.ToString();
            query["gameId"] = request.GameId.ToString();
            query["client"] = request.Channel.ToString("G").ToLower();
            query["closeurl"] = request.ExitUrl;

            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }
    }
}