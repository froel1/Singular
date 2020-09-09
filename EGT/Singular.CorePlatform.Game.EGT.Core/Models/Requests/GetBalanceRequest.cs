using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Requests
{
    public class GetBalanceRequest
    {
        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }
    }
}
