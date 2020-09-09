using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Common.Crypto;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singular.CorePlatform.Game.EGT.Filters
{
    public class CheckCredentialsAttribute : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger _logger;

        private readonly IIntegrationInstance _integrationInstance;

        public CheckCredentialsAttribute(IConfiguration configuration, ILoggerFactory loggerFactory, IIntegrationInstance integrationInstance)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<CheckCredentialsAttribute>();
            _integrationInstance = integrationInstance;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            // TODO Modify code according to integration needs
            try
            {
                var nonce = context.HttpContext.Request.Headers["nonce"];
                var hash = context.HttpContext.Request.Headers["hash"];
                
                if (string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(hash))
                {
                    context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, "Missing mandatory headers"));
                }
                else
                {
                    var body = "";
                    context.HttpContext.Request.EnableBuffering();
                    context.HttpContext.Request.Body.Position = 0;
                    using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, false, 1024, true))
                    {
                        body = await reader.ReadToEndAsync();
                    }
                    context.HttpContext.Request.Body.Position = 0;

                    var computedHash = Sha256.Create(body + nonce + _configuration[$"Instances:{_integrationInstance.Name}:Extended:ApiSecret"]);
                    if (hash != computedHash)
                        context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, "Invalid credentials"));
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Exception during credentials check: {Exception}", ex);

                context.Result = new UnauthorizedObjectResult(new OperationResult(ErrorCodes.AccessDenied, "Exception"));
            }

            base.OnActionExecuting(context);
        }
    }
}
