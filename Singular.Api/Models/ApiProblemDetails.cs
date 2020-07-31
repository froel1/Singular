using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Singular.Api.Models
{
    public class ApiProblemDetails : ProblemDetails
    {
        /// <summary>
        /// Error code defined in application
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Request trace identifier
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; private set; } = default!;

        /// <summary>
        /// Exception Stack Trace
        /// </summary>
        [JsonPropertyName("stackTrace")]
        public string? StackTrace { get; set; }

        public ApiProblemDetails(HttpStatusCode status)
        {
            Status = (int)status;
            Code = "SystemError";
            Type = $"https://httpstatuses.com/{Status}";
        }

        public void SetTraceId(HttpContext context)
        {
            TraceId = Activity.Current?.Id ?? context?.TraceIdentifier ?? Guid.NewGuid().ToString();
        }
    }
}