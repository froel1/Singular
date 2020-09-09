using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Singular.CorePlatform.Game.EGT.Core.Services
{
    public class IntegrationInstance : IIntegrationInstance
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public IntegrationInstance(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Name => _httpContextAccessor.HttpContext.Request.Headers["Host"].ToString().Split('.', 2)[^1];

        public string IntegrationLabel => _configuration["Integration_Label"];

        public string AdapterConfigurationPath => $"{IntegrationLabel}/instances/{Name}";
    }
}
