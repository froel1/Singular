using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Singular.Api.Models
{
    public class TransactionModel
    {
        [BindProperty(Name = "transactionid")]
        [Required(AllowEmptyStrings = false)]
        public string? TransactionId { get; set; }

        [BindProperty(Name = "amount")]
        public decimal Amount { get; set; }
    }
}