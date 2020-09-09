using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Requests
{
    public class CheckTransactionStatusRequest
    {
        [Required]
        [JsonProperty("provider_transaction_id")]
        public string ProviderTransactionId { get; set; }
    }
}
