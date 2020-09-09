using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Responses
{
    public class GetAwardDetailsResponse
    {
        [JsonProperty("award_type")]
        public string AwardType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("number_of_freegames")]
        public int? NumberOfFreeGames { get; set; }

        [JsonProperty("bet_amount")]
        public decimal? BetAmount { get; set; }

        [JsonProperty("bet_per_line")]
        public decimal? BetPerLine { get; set; }

        [JsonProperty("max_bet_amount")]
        public decimal? MaxBetAmount { get; set; }

        [JsonProperty("number_of_lines")]
        public int? NumberOfLines { get; set; }

        [JsonProperty("available_games")]
        public string[] AvailableGames { get; set; }

        [JsonProperty("validity_period")]
        public int? ValidityPeriod { get; set; }

        [JsonProperty("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [JsonProperty("provider_bonus_id")]
        public string ProviderBonusId { get; set; }
    }
}
