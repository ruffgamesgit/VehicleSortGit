using System.Collections.Generic;
using Services.Analytics.Data.Args;
using Services.Analytics.Data.Args.Economy;

namespace Services.Analytics.Interfaces
{
    public interface IEconomyAnalyticsEvents
    {
        public void EconomyEvent(EconomyEventArgs args, Dictionary<string, object> additionalData = null);
        public void InAppPurchaseEvent(InAppPurchaseEventArgs args, Dictionary<string, object> additionalData = null);
    }
}