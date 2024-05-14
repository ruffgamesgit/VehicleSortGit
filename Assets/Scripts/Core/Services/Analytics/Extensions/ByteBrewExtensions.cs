using System;
using System.Collections.Generic;
using System.Reflection;
using ByteBrewSDK;
using Events.InGame.EventArgs;
using Events.Level.EventArgs;
using Events.Mission.EventArgs;
using LionStudios.Suite.Analytics.Events.CrossPromo.EventArgs;
using LionStudios.Suite.Analytics.Events.EventArgs;
using Newtonsoft.Json;
using Services.Analytics.Data.Args;
using Services.Analytics.Data.Args.Advertising;
using Services.Analytics.Data.Args.BaseReflection;
using Services.Analytics.Data.Args.Progress;
using UnityEngine;

namespace Services.Analytics.Extensions
{
    public static class ByteBrewExtensions
    {
        public static void RedirectDirectToByteBrew  (this EventArgs args, string eventName, Dictionary<string, object> additionalData = null)
        {
            EventArgs arguments = null;
            var argType = args.GetType();

            if (argType != typeof(ReflectionEventBase))
            {
                if (argType == typeof(LevelEventArgs))
                {
                    arguments = new LevelEventArgsReflection(args as LevelEventArgs);
                }
                else if (argType == typeof(LevelCompleteEventArgs))
                {
                    arguments = new LevelCompleteEventArgsReflection(args as LevelCompleteEventArgs);
                }
                else if (argType == typeof(AdEventArgs))
                {
                    arguments = new AdEventArgsReflection(args as AdEventArgs);
                }
                else if (argType == typeof(AdRewardArgs))
                {
                    arguments = new AdRewardEventArgsReflection(args as AdRewardArgs);
                }
                else if (argType == typeof(CrossPromoEventArgs))
                {
                    arguments = new CrossPromoEventArgsReflection(args as CrossPromoEventArgs);
                }
                else if (argType == typeof(FeatureUnlockedEventArgs))
                {
                    arguments = new FeatureUnlockedEventArgsReflection(args as FeatureUnlockedEventArgs);
                }
                else if (argType == typeof(MissionCompletedEventArgs))
                {
                    arguments = new MissionCompletedEventArgsReflection(args as MissionCompletedEventArgs);
                }
                else if(argType == typeof(MissionEventArgs))
                {
                    arguments = new MissionEventArgsReflection(args as MissionEventArgs);
                }
                else if(argType == typeof(PowerUpUsedEventArgs))
                {
                    arguments = new PowerUpUsedEventArgsReflection(args as PowerUpUsedEventArgs);
                }
                else
                {
                    throw new("ByteBrew Event Args is not implemented : " + eventName);
                }
            }

            var byteBrewArgs = arguments.GenerateDictionary();
             if (additionalData != null)
             {
                 foreach (var data in additionalData)
                 {
                     byteBrewArgs.Add(data.Key, data.Value.ToString());
                 }
            }

            if (byteBrewArgs.Count == 0)
            {
                throw new("ByteBrew Event Args is empty : " + eventName);
            }
            
            #if UNITY_EDITOR
            string debugString = eventName + " : ";
            foreach (var arg in byteBrewArgs)
            {
                debugString += arg.ToString() + " ";
            }
            Debug.Log(eventName + " : " + debugString);
            #endif
            ByteBrew.NewCustomEvent(eventName, byteBrewArgs);
        }

        private static Dictionary<string, string> GenerateDictionary(this object obj)
        {
            Dictionary<string, string> variableDictionary = new Dictionary<string, string>();

            if (obj != null)
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (var property in properties)
                {
                    string propertyName = property.Name;
                    object propertyValue = property.GetValue(obj);

                    if (propertyValue != null)
                    {
                        variableDictionary.Add(propertyName, propertyValue.ToString());
                    }
                    else
                    {
                        variableDictionary.Add(propertyName, "null");
                    }
                }
            }

            return variableDictionary;
        }
    }
}