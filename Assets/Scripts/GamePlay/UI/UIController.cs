using System;
using System.Threading.Tasks;
using Core.Locator;
using Core.Services.GamePlay;
using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics.Data.Args.Advertising;
using Services.Analytics.Extensions;
using Services.Max;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class UIController : MonoBehaviour
    {
        private IGamePlayService _gamePlayService;
        [SerializeField] private TextMeshProUGUI levelTxt;
        [SerializeField] private TextMeshProUGUI moveCountTxt;
        [SerializeField] private Button settingsBtn;

        [SerializeField] private LoseScreenController _loseScreenController;
        [SerializeField] private WinScreenController _winScreenController;
        [SerializeField] private SettingsController _settingsController;


        [SerializeField] private Button nextLevelBtn;
        [SerializeField] private Button previousLevelBtn;
        private int _maxMoveCount;

        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _gamePlayService.LevelFinishedEvent += OnLevelFinished;
            _gamePlayService.OnVehicleMoved += OnVehicleMoved;

            _maxMoveCount = _gamePlayService.GetCurrentLevelData().moveCount;
            moveCountTxt.text = _maxMoveCount.ToString();

            SetLevelText();
            SetButtonBehaviours();
        }

        private void OnVehicleMoved(object sender, EventArgs e)
        {
            DecreaseMoveCountText();
        }

        async void DecreaseMoveCountText()
        {
            if (_maxMoveCount <= 0) return;
            
            _maxMoveCount--;
            if (_maxMoveCount <= 0)
            {
                await Task.Delay(1000);
                if (!_gamePlayService.IsSucceeded())
                    OpenFailScreen(LevelFailedType.Move);
            }

            moveCountTxt.text = _maxMoveCount.ToString();
        }

        private void OnLevelFinished(object sender, LevelFinishedType e)
        {
            switch (e)
            {
                case LevelFinishedType.Fail:
                    OpenFailScreen(LevelFailedType.Space);
                    break;
                case LevelFinishedType.Complete:
                    OpenWinScreen();
                    break;
                case LevelFinishedType.Restart:
                    // NO ACTION FOR NOW 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        private void OpenFailScreen(LevelFailedType failType)
        {
            void OnRevive()
            {
                void OnDisplayed(MaxSdkBase.AdInfo info)
                {
                    AdEventArgs args = new AdEventArgs()
                    {
                        Network = info.NetworkName, Level = _gamePlayService.GetCurrentLevel(),
                        Placement = "revive_move_count_rv"
                    };
                    args.Fire(AdEventType.RewardedVideo);
                }
                
                void OnSuccess()
                {
                    _maxMoveCount += 10;
                    moveCountTxt.text = _maxMoveCount.ToString();
                    Destroy(_loseScreenController.gameObject);
                    
                    AdRewardArgs args = new AdRewardArgs()
                    {
                        Placement = "revive_move_count_rv",
                        Level = _gamePlayService.GetCurrentLevel(),
                        Reward = null
                    };
                    args.Fire();
                }
                
                var maxSdkService = ServiceLocator.Instance.Resolve<IMaxSDKService>();
                
                maxSdkService.ShowRewardedAd(OnDisplayed, OnSuccess);
            }
            
            _loseScreenController.Activate(failType, OnRevive);
            CloseSettingScreen();
        }

        private void OpenWinScreen()
        {
            _winScreenController.Activate();
            CloseSettingScreen();
        }

        private void OpenSettingsScreen()
        {
            _settingsController.Activate();
        }

        private void CloseSettingScreen()
        {
            _settingsController.Deactivate();
        }

        private void SetLevelText()
        {
            levelTxt.text = "LV " + _gamePlayService.GetCurrentLevel();
        }

        private void SetButtonBehaviours()
        {
            nextLevelBtn.onClick.AddListener(() => { _gamePlayService.LoadNext(); });

            previousLevelBtn.onClick.AddListener(() => { _gamePlayService.LoadPrevious(); });
            settingsBtn.onClick.AddListener(() => { OpenSettingsScreen(); });
        }

        private void OnDestroy()
        {
            _gamePlayService.LevelFinishedEvent -= OnLevelFinished;
            _gamePlayService.OnVehicleMoved -= OnVehicleMoved;
        }
    }
}