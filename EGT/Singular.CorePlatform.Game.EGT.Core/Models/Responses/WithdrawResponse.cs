using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Core.Models.Responses
{
    public class WithdrawResponse
    {
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
    }
}
