using Singular.CorePlatform.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Models
{
    public class EGTExtendedOptions : IValidatable
    {
        public string ApiSecret { get; set; }

        public string LaunchUrl { get; set; }

        public int TokenTtl { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(ApiSecret))
                return false;

            if (string.IsNullOrEmpty(LaunchUrl))
                return false;

            if (TokenTtl <= 0)
                return false;

            return true;
        }
    }

    public class EGTSettings : IntegrationOptions<EGTExtendedOptions> { }
}
