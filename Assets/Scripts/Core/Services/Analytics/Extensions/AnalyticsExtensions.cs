using System;
using System.Collections.Generic;
using Core.Locator;
using Core.Services.Analytics;
using Events.InGame.EventArgs;
using Events.Level.EventArgs;
using Events.Mission.EventArgs;
using LionStudios.Suite.Analytics.Events.CrossPromo.EventArgs;
using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics.Data;
using Services.Analytics.Data.Args;
using Services.Analytics.Data.Args.Advertising;
using Services.Analytics.Data.Args.Economy;
using UnityEngine;

namespace Services.Analytics.Extensions
{
    public static class AnalyticsExtensions
    {
        private static readonly IAnalyticsService AnalyticsService;

        static AnalyticsExtensions()
        {
            AnalyticsService = ServiceLocator.Instance.Resolve<IAnalyticsService>();
        }
        
        
        public static void Fire(this LevelEventArgs args, LevelEventTypeEnum type, Dictionary<string, object> additionalData = null)
        {
            switch (type)
            {
                case LevelEventTypeEnum.Start:
                    AnalyticsService.LevelStart(args, additionalData);
                    break;
                case LevelEventTypeEnum.Fail:
                    AnalyticsService.LevelFail(args, additionalData);
                    break;
                case LevelEventTypeEnum.Restart:
                    AnalyticsService.LevelRestart(args, additionalData);
                    break;
                case LevelEventTypeEnum.Abandoned:
                    AnalyticsService.LevelAbandoned(args, additionalData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
        }

        public static void Fire(this LevelCompleteEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.LevelComplete(args,additionalData);
            
        }

        public static void Fire(this EconomyEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.EconomyEvent(args,additionalData);
        }
        
        public static void Fire(this InAppPurchaseEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.InAppPurchaseEvent(args,additionalData);
        }

        public static void Fire(this CrossPromoEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.CrossPromoShow(args,additionalData);
        }

        public static void Fire(this AdEventArgs args, AdEventType type, Dictionary<string, object> additionalData = null)
        {
            switch (type)
            {
               case AdEventType.Interstitial:
                   AnalyticsService.InterstitialShow(args,additionalData);
                   break;
               case AdEventType.RewardedVideo:
                   AnalyticsService.RewardedVideoShow(args,additionalData);
                   break;
            }
        }

        public static void Fire(this AdRewardArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.RewardedVideoCollect(args,additionalData);
        }

        public static void Fire(this FeatureUnlockedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.FeatureUnlocked(args,additionalData);
        }

        public static void Fire(this MissionEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (args.GetType() == typeof(MissionCompletedEventArgs))
            {
                var completeArgs = args as MissionCompletedEventArgs;
                AnalyticsService.MissionCompleted(completeArgs,additionalData);
            }
            else
            {
                AnalyticsService.MissionStarted(args,additionalData);
            }
        }

        public static void Fire(this PowerUpUsedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            AnalyticsService.PowerUpUsed(args,additionalData);
        }
        
        
    }
}