using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Requests
{
    public class ConfirmAwardRequest
    {
        [Required]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonProperty("award_id")]
        public int AwardId { get; set; }
    }
}
