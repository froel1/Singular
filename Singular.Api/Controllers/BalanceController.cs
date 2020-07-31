using System.Net;
using Microsoft.AspNetCore.Mvc;
using Singular.Api.Interfaces;
using Singular.Api.Models;

namespace Singular.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [ProducesErrorResponseType(typeof(ApiProblemDetails))]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceService _service;

        public BalanceController(IBalanceService service) => _service = service;

        [HttpGet("balance")]
        public ActionResult<decimal> GetBalance() => _service.GetBalance();

        [HttpPost("withdraw/{transactionid}/{amount}")]
        public ActionResult Withdraw([FromRoute] TransactionModel model)
        {
            _service.Withdraw(model);
            return Ok();
        }

        [HttpPost("deposit/{transactionid}/{amount}")]
        public ActionResult Deposit([FromRoute] TransactionModel model)
        {
            _service.Deposit(model);
            return Ok();
        }
    }
}