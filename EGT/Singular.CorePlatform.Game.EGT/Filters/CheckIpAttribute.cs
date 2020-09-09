using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Filters
{
    public class CheckIpAttribute : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger _logger;

        private readonly IIntegrationInstance _integrationInstance;

        public CheckIpAttribute(IConfiguration configuration, ILoggerFactory loggerFactory, IIntegrationInstance integrationInstance)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<CheckIpAttribute>();
            _integrationInstance = integrationInstance;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var requestIp = context.HttpContext.Request.Headers["X-Forwarded-For"];
                var whitelist = _configuration.GetSection($"Instances:{_integrationInstance.Name}:IpWhiteList").GetChildren().Select(x => x.Value);

                if (!whitelist.Any())
                {
                    context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, $"IP whitelist not configured for {_integrationInstance.Name}"));
                    return;
                }

                if (!whitelist.Any(ip => ip == "*" || ip == requestIp))
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("IP not whitelisted: {IP}", requestIp);

                    context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, "IP not whitelisted"));
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Exception during ip check: {Exception}", ex);

                context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, "Exception"));
            }

            base.OnActionExecuting(context);
        }
    }
}
