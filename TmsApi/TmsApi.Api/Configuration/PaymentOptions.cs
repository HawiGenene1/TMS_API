using System.ComponentModel.DataAnnotations;

namespace TmsApi.Api.Configuration;

public class PaymentOptions
{
   [Required]
    public required string GatewayUrl { get; init; } = "https://payments.example.com";

    [Range(100, 100000)]
    public decimal MaxDepositBirr { get; init; }
}
