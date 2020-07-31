using Balances;

namespace Singular.Api.Helpers
{
    public delegate IBalanceManager ServiceResolver(BalanceSource key);

    public enum BalanceSource
    {
        Casino = 1,
        Game = 2
    }
}