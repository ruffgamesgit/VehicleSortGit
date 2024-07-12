using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Locator;
using Core.Services.Analytics;
using Core.Services.GamePlay;
using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics.Data.Args.Advertising;
using Services.Analytics.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Services.Max
{
    public class MaxSDKService : IMaxSDKService
    {
        private const string InterstitialTimeStampKey = "InterstitialTimeStamp";
        private readonly MaxSDKServiceContainer _serviceContainer;
        private readonly IAnalyticsService _analyticsService;

        private Action _onSuccessCallback;
        private Action _onFailedCallback;
        private Action<MaxSdkBase.AdInfo> _onAdDisplayedCallback;

        public MaxSDKService(MaxSDKServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
            InitializeRewardedAd();
            InitializeInterstitialAds();
        }

        #region Inter

        public event EventHandler OnRewardNotReady;

        private void InitializeInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialAdDisplayedEvent;
        }

        private void OnInterstitialAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            AdEventArgs args = new AdEventArgs()
            {
                Network = arg2.NetworkName,
                Placement = "interstitial",
                Level = ServiceLocator.Instance.Resolve<IGamePlayService>().GetCurrentLevel()
            };
            args.Fire(AdEventType.Interstitial);
        }

        public void ShowInterstitialAd()
        {
            var lastShowedInterstitialTimeStamp = PlayerPrefs.GetString(InterstitialTimeStampKey, string.Empty);
            if (!string.IsNullOrEmpty(lastShowedInterstitialTimeStamp))
            {
                var lastShowedDateTime = DateTime.Parse(lastShowedInterstitialTimeStamp, CultureInfo.InvariantCulture);
                var currentTime = DateTime.Now;
                var timeDifference = currentTime - lastShowedDateTime;
                var timeDifferenceAsSeconds = timeDifference.TotalSeconds;
                if (timeDifferenceAsSeconds < 120)
                {
                    return;
                }
            }

            PlayerPrefs.SetString(InterstitialTimeStampKey, DateTime.Now.ToString(CultureInfo.InvariantCulture));
            _serviceContainer.ShowInterstitial();
        }

       

        #endregion

        #region Banner

        public void ShowBannerAd()
        {
            if (ServiceLocator.Instance.Resolve<IGamePlayService>().GetCurrentLevel() <= 4) return;
            _serviceContainer.ToggleBannerVisibility(true);
        }

        public void HideBannerAd()
        {
            _serviceContainer.ToggleBannerVisibility(false);
        }

        #endregion

        #region Rewarded

        public void ShowRewardedAd(Action<MaxSdkBase.AdInfo> onAdDisplayedCallback, Action onSuccessCallback,
            Action onFailedCallback = null)
        {
            _onSuccessCallback = onSuccessCallback;
            _onFailedCallback = onFailedCallback;
            _onAdDisplayedCallback = onAdDisplayedCallback;
            bool isReady = _serviceContainer.ShowRewardedAd();
            if (!isReady)
            {
                OnRewardNotReady?.Invoke(this, EventArgs.Empty);
                OnRewardedAdFailedEvent(string.Empty, null);
            }
        }

        private void InitializeRewardedAd()
        {
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        private void OnAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            _onAdDisplayedCallback?.Invoke(arg2);
        }

        private void OnRewardedAdFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            _onFailedCallback?.Invoke();
            _onFailedCallback = null;
            _onSuccessCallback = null;
            _onAdDisplayedCallback = null;
        }

        private void OnRewardedAdFailedToDisplayEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            _onFailedCallback?.Invoke();
            _onFailedCallback = null;
            _onSuccessCallback = null;
            _onAdDisplayedCallback = null;
        }

        private void OnRewardedAdDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            _onFailedCallback?.Invoke();
            _onFailedCallback = null;
            _onSuccessCallback = null;
            _onAdDisplayedCallback = null;
        }

        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
        {
            _onSuccessCallback?.Invoke();
            _onFailedCallback = null;
            _onSuccessCallback = null;
            _onAdDisplayedCallback = null;
            var newTime = DateTime.Now.AddMinutes(-1);
            PlayerPrefs.SetString(InterstitialTimeStampKey, newTime.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}