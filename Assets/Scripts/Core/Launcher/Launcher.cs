using System;
using System.Collections.Generic;
using Core.Locator;
using Core.Services;
using Core.Services.Analytics;
using Core.Services.GamePlay;
using Services.Analytics;
using Services.Max;
using Services.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Launcher
{
    public class Launcher : MonoBehaviour
    {
        private static readonly HashSet<GameObject> DontDestroyOnLoadObjects = new ();
        
        #region Initialize
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Debug.Log("ServicesInitializeEditor");
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            InitializeServices();
        }
        
        private static void InitializeServices()
        {
            ServiceLocator.Initialize();
            SoundService soundService = new SoundService(InstantiateSoundServiceContainer());
            ServiceLocator.Instance.Register<ISoundService>(soundService);
            AnalyticsService analyticsService = new AnalyticsService();
            ServiceLocator.Instance.Register<IAnalyticsService>(analyticsService);
            GamePlayService gamePlayService = new GamePlayService();
            ServiceLocator.Instance.Register<IGamePlayService>(gamePlayService);
            MaxSDKService maxSDKService = new MaxSDKService(InstantiateMaxSDKServiceContainer());
            ServiceLocator.Instance.Register<IMaxSDKService>(maxSDKService);
        }
        

        // private static TutorialServiceContainer InstantiateTutorialServiceContainer()
        // {
        //     var container = new GameObject("TutorialServiceContainer");
        //     SetDontDestroyOnLoad(container);
        //     return container.AddComponent<TutorialServiceContainer>();
        // }
        //
        private static MaxSDKServiceContainer InstantiateMaxSDKServiceContainer()
        {
            var container = new GameObject("MaxSDKServiceContainer");
            SetDontDestroyOnLoad(container);
            return container.AddComponent<MaxSDKServiceContainer>();
        }
        
        private static SoundServiceContainer InstantiateSoundServiceContainer()
        {
            var container = new GameObject("SoundServiceContainer");
            SetDontDestroyOnLoad(container);
            return container.AddComponent<SoundServiceContainer>();
        }
        
        private static void SetDontDestroyOnLoad(GameObject gameObject)
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoadObjects.Add(gameObject);
        }

        private static void DestroyDontDestroyOnLoads()
        {
            foreach (var obj in DontDestroyOnLoadObjects)
            {
                Destroy(obj);
            }
        }

        public void ReLaunch()
        {
            DestroyDontDestroyOnLoads();
           // Initialize();
        }
        
        #endregion

    }
}