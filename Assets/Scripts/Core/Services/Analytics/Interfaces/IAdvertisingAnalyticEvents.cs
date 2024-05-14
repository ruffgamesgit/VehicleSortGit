using System.Collections.Generic;
using LionStudios.Suite.Analytics.Events.CrossPromo.EventArgs;
using LionStudios.Suite.Analytics.Events.EventArgs;

namespace Services.Analytics.Interfaces
{
    public interface IAdvertisingAnalyticEvents
    {
        public void CrossPromoShow(CrossPromoEventArgs args, Dictionary<string, object> additionalData = null);
        public void RewardedVideoShow(AdEventArgs args, Dictionary<string, object> additionalData = null);
        public void RewardedVideoCollect(AdRewardArgs args, Dictionary<string, object> additionalData = null);
        public void InterstitialShow(AdEventArgs args, Dictionary<string, object> additionalData = null);
    }
}