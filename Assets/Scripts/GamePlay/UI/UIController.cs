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
        [SerializeField] private GameObject gamePlayScreen;


        [SerializeField] private Button nextLevelBtn;
        [SerializeField] private Button previousLevelBtn;
        private int _maxMoveCount;

        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _gamePlayService.LevelFinishedEvent += OnLevelFinished;
            _gamePlayService.OnVehicleMoved += OnVehicleMoved;
            _gamePlayService.SortCompleted += OnSortCompleted;

            _maxMoveCount = _gamePlayService.GetCurrentLevelData().moveCount;
            moveCountTxt.text = "Move: " + _maxMoveCount;

            SetLevelText();
            SetButtonBehaviours();
        }

        private void OnVehicleMoved(object sender, EventArgs e)
        {
            DecreaseMoveCountText();
        }


        private void OnSortCompleted(object sender, EventArgs e)
        {
            if (_maxMoveCount <= 0 && !_gamePlayService.IsSuccess())
            {
                OpenFailScreen(LevelFailedType.Move);
                _gamePlayService.SetInteractable(false);
            }
        }

        void DecreaseMoveCountText()
        {
            if (_maxMoveCount <= 0) return;

            _maxMoveCount--;
            moveCountTxt.text = "Move: " + _maxMoveCount;
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
                    _loseScreenController.Deactivate();

                    AdRewardArgs args = new AdRewardArgs()
                    {
                        Placement = "revive_move_count_rv",
                        Level = _gamePlayService.GetCurrentLevel(),
                        Reward = null
                    };
                    _gamePlayService.SetInteractable(true);
                    args.Fire();
                }

                var maxSdkService = ServiceLocator.Instance.Resolve<IMaxSDKService>();

                maxSdkService.ShowRewardedAd(OnDisplayed, OnSuccess);
                SetGamePlayScreenStatus(true);
            }

            _loseScreenController.Activate(failType, OnRevive);
            CloseSettingScreen();
            SetGamePlayScreenStatus(false);
        }

        public void SetGamePlayScreenStatus(bool activate)
        {
            gamePlayScreen.SetActive(activate);
        }

        private void OpenWinScreen()
        {
            _winScreenController.Activate();
            CloseSettingScreen();
            SetGamePlayScreenStatus(false);
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
            _gamePlayService.SortCompleted -= OnSortCompleted;
        }
    }
}