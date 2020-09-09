using Newtonsoft.Json;
using Singular.CorePlatform.Adapter.Models.Requests;
using Singular.CorePlatform.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Requests
{
    public class DepositRequest
    {
        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [Required]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("jackpot_amount")]
        public decimal? JackpotAmount { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [Required]
        [JsonProperty("provider_transaction_id")]
        public string ProviderTransactionId { get; set; }

        [Required]
        [JsonProperty("round_id")]
        public string RoundId { get; set; }

        [Required]
        [JsonProperty("round_closed")]
        public bool RoundClosed { get; set; }

        [JsonProperty("award_id")]
        public string AwardId { get; set; }

        [Required]
        [JsonProperty("client_ip")]
        public string ClientIp { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("additional_data")]
        public GameAdditionalData AdditionalData { get; set; }
    }
}
