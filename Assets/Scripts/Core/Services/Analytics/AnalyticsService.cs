using System.Collections.Generic;
using Events.InGame.EventArgs;
using Events.Level.EventArgs;
using Events.Mission.EventArgs;
using LionStudios.Suite.Analytics;
using LionStudios.Suite.Analytics.Events.CrossPromo.EventArgs;
using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics;
using Services.Analytics.Data.Args;
using Services.Analytics.Data.Args.Economy;
using Services.Analytics.Extensions;
using UnityEngine;

namespace Core.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly bool _shouldSendToLionAnalytics;
        public AnalyticsService()
        {
            _shouldSendToLionAnalytics = !Application.isEditor && !Debug.isDebugBuild;
        }
        #region ProgressionAnalyticEvents
        public void LevelStart(LevelEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.LevelStart(args, additionalData);      
            }
            args.RedirectDirectToByteBrew("LevelStart", additionalData);
        }
        public void LevelFail(LevelEventArgs args, Dictionary<string, object> additionalData = null)
        { 
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.LevelFail(args, additionalData);    
            }
            args.RedirectDirectToByteBrew("LevelFail", additionalData);
        }
        public void LevelRestart(LevelEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.LevelRestart(args, additionalData);
            }
            args.RedirectDirectToByteBrew("LevelRestart", additionalData);
        }
        public void LevelAbandoned(LevelEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.LevelAbandoned(args, additionalData);
            }
            args.RedirectDirectToByteBrew("LevelAbandoned", additionalData);
        }
        public void LevelComplete(LevelCompleteEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.LevelComplete(args, additionalData);
            }
            args.RedirectDirectToByteBrew("LevelCompleted", additionalData);
        }
        public void FeatureUnlocked(FeatureUnlockedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.FeatureUnlocked(args, additionalData);
            }
            args.RedirectDirectToByteBrew("FeatureUnlocked", additionalData);

        }

        public void MissionStarted(MissionEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.MissionStarted(args, additionalData);
            }
            args.RedirectDirectToByteBrew("MissionStarted", additionalData);
        }

        public void MissionCompleted(MissionCompletedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.MissionCompleted(args, additionalData);
            }
            args.RedirectDirectToByteBrew("MissionCompleted", additionalData);
        }

        public void PowerUpUsed(PowerUpUsedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.PowerUpUsed(args, additionalData);
            }
            args.RedirectDirectToByteBrew("PowerUpUsed", additionalData);
        }

        #endregion
        
        #region EconomyAnalyticEvents
        public void EconomyEvent(EconomyEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                if  (!string.IsNullOrEmpty(args.VirtualCurrencyName))
                {
                    LionAnalytics.EconomyEvent(args.VirtualCurrencyAmount,
                        args.VirtualCurrencyName,args.VirtualCurrencyType,
                        args.RealCurrencyType,args.RealCurrencyAmount,
                        args.PurchaseName,args.ProductID,args.TransactionID,
                        args.PurchaseLocation,additionalData,args.ReceiptStatus);
                }
                else if (!string.IsNullOrEmpty(args.PurchaseName))
                {
                    LionAnalytics.EconomyEvent(args.PurchaseName,args.SpentProducts,
                        args.ReceivedProducts,args.PurchaseLocation,args.ProductID,
                        args.TransactionID,additionalData,args.ReceiptStatus);
                }
            
                else if (args.Transaction != null)
                {
                    LionAnalytics.EconomyEvent(args.Transaction,args.ProductID,
                        args.TransactionID,args.PurchaseLocation,
                        additionalData,args.ReceiptStatus);
                }
            }
            
            args.RedirectDirectToByteBrew("EconomyEvent", additionalData);
        }

        public void InAppPurchaseEvent(InAppPurchaseEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                if (!string.IsNullOrEmpty(args.VirtualCurrencyName))
                {
                    LionAnalytics.EconomyEvent(args.VirtualCurrencyAmount,
                        args.VirtualCurrencyName, args.VirtualCurrencyType,
                        args.RealCurrencyType, args.RealCurrencyAmount,
                        args.PurchaseName, args.ProductID, args.TransactionID,
                        args.PurchaseLocation, additionalData, args.ReceiptStatus);
                }
                else if (!string.IsNullOrEmpty(args.PurchaseName))
                {
                    LionAnalytics.EconomyEvent(args.PurchaseName, args.SpentProducts,
                        args.ReceivedProducts, args.PurchaseLocation, args.ProductID,
                        args.TransactionID, additionalData, args.ReceiptStatus);
                }
                else if (args.Transaction != null)
                {
                    LionAnalytics.EconomyEvent(args.Transaction, args.ProductID,
                        args.TransactionID, args.PurchaseLocation,
                        additionalData, args.ReceiptStatus);
                }
            }
            args.RedirectDirectToByteBrew("InAppPurchase", additionalData);
        }
        
        #endregion
        
        #region AdvertisingAnalyticEvents
        
        public void CrossPromoShow(CrossPromoEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if(_shouldSendToLionAnalytics)
            {
                LionAnalytics.CrossPromoShow(args);
            }
            
            args.RedirectDirectToByteBrew("CrossPromoShow", additionalData);
        }

        public void RewardedVideoShow(AdEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.RewardVideoShow(args,additionalData);
            }
            args.RedirectDirectToByteBrew("RewardedVideoShow", additionalData);
        }

        public void RewardedVideoCollect(AdRewardArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.RewardVideoCollect(args,additionalData);
            }
            args.RedirectDirectToByteBrew("RewardedVideoCollect", additionalData);
        }
        
        public void InterstitialShow(AdEventArgs args, Dictionary<string, object> additionalData = null)
        {
            if (_shouldSendToLionAnalytics)
            {
                LionAnalytics.InterstitialShow(args ,additionalData);
            }
            args.RedirectDirectToByteBrew("InterstitialShow", additionalData);
        }

        #endregion

        
    }
}