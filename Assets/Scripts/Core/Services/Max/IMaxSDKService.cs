using System;
using Core.Locator;
using UnityEngine.Events;

namespace Services.Max
{
    public interface IMaxSDKService : IService
    {
        public event EventHandler OnRewardNotReady;
        public void ShowInterstitialAd();
        public void ShowBannerAd();
        public void HideBannerAd();
        public void ShowRewardedAd(Action<MaxSdkBase.AdInfo> onAdDisplayedCallback, Action onSuccessCallback, Action onFailedCallback = null);
    }
}