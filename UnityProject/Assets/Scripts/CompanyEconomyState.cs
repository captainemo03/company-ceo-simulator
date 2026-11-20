using System;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    /// <summary>Read-only bridge shape for the web simulator's economic state.</summary>
    [Serializable]
    public sealed class CompanyEconomyState
    {
        public string companyName = "New Company";
        public double cash = 1000000d;
        public double companyValue = 1000000d;
        public double personalWealth = 100000d;
        public int month = 1;
        public string currency = "TRY";

        public static CompanyEconomyState Default() => new CompanyEconomyState();

        public string Summary() => $"{companyName} · Nakit {cash:N0} {currency} · Değer {companyValue:N0} {currency}";
    }

    public sealed class CompanyEconomyBridge : MonoBehaviour
    {
        [SerializeField] private CompanyEconomyState state = new CompanyEconomyState();
        public CompanyEconomyState State => state;
        public event Action<CompanyEconomyState> Changed;

        public void Apply(CompanyEconomyState next)
        {
            state = next ?? CompanyEconomyState.Default();
            Changed?.Invoke(state);
        }
    }
}
