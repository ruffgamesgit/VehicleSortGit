using System;
using System.Threading.Tasks;
using Core.Locator;
using Core.Services.GamePlay;
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
        private LevelFailedType _failedType;

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
                    _gamePlayService.LevelFinished(LevelFinishedType.Fail, LevelFailedType.OutOfMoveCount);
            }

            moveCountTxt.text = _maxMoveCount.ToString();
        }

        private void OnLevelFinished(object sender, LevelFinishedType e, LevelFailedType failedType = LevelFailedType.OutOfEmptyLots)
        {
            switch (e)
            {
                case LevelFinishedType.Fail:
                    OpenFailScreen();
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

        private void OpenFailScreen(LevelFailedType failedType = LevelFailedType.OutOfEmptyLots)
        {
            _loseScreenController.Activate(failedType);
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