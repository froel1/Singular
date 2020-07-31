using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Singular.Api.Exceptions;
using Singular.Api.Models;

namespace Singular.Api.Helpers
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problem = GetProblemDetails(exception);
            problem.SetTraceId(context);

            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

            if (string.IsNullOrEmpty(problem.Instance))
            {
                problem.Instance = context.Request.Path;
            }

            if (!_env.IsProduction())
            {
                //problem.StackTrace = exception.StackTrace;//TODO:test
            }

            var result = JsonSerializer.Serialize(problem);

            return context.Response.WriteAsync(result, Encoding.UTF8);
        }

        private static ApiProblemDetails GetProblemDetails(Exception exception)
        {
            if (exception is ApiValidationException ex)
            {
                return new ApiProblemDetails(ex.Error.ToHttpStatusCode())
                {
                    Code = ex.Error.ToString("D"),
                    Title = ex.Error.ToResponsePhrase(),
                    Detail = ex.Message
                };
            }

            return new ApiProblemDetails(HttpStatusCode.InternalServerError)
            {
                Code = HttpStatusCode.InternalServerError.ToString("D"),
                Title = ReasonPhrases.GetReasonPhrase((int)HttpStatusCode.InternalServerError),
                Detail = exception.Message
            };
        }
    }
}