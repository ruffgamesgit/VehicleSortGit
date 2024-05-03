using System;
using System.Collections.Generic;
using Core.Locator;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Launcher
{
    public class Launcher : MonoBehaviour
    {
        private static readonly HashSet<GameObject> DontDestroyOnLoadObjects = new ();
        
        #region Initialize
        // #if UNITY_EDITOR
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // public static void Initialize()
        // {
        //     Debug.Log("ServicesInitializeEditor");
        //     Application.targetFrameRate = 60;
        //     InitializeServices();
        // }
        // #else
        public void Awake()
        {
            Debug.Log("ServicesInitialize");
            Application.targetFrameRate = 60;
            InitializeServices();
        }
        private void Start()
        {
            Debug.Log("ByteBrewInitialize");
            //ByteBrew.InitializeByteBrew();   
        }
        //#endif

        
        private static void InitializeServices()
        {
            ServiceLocator.Initialize();
        }
        

        // private static TutorialServiceContainer InstantiateTutorialServiceContainer()
        // {
        //     var container = new GameObject("TutorialServiceContainer");
        //     SetDontDestroyOnLoad(container);
        //     return container.AddComponent<TutorialServiceContainer>();
        // }
        //
        // private static MaxSDKServiceContainer InstantiateMaxSDKServiceContainer()
        // {
        //     var container = new GameObject("MaxSDKServiceContainer");
        //     SetDontDestroyOnLoad(container);
        //     return container.AddComponent<MaxSDKServiceContainer>();
        // }
        //
        // private static SoundServiceContainer InstantiateSoundServiceContainer()
        // {
        //     var container = new GameObject("SoundServiceContainer");
        //     SetDontDestroyOnLoad(container);
        //     return container.AddComponent<SoundServiceContainer>();
        // }
        //
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