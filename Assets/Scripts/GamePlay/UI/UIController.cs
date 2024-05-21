using System;
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
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button restartBtn;
        
        
        [SerializeField] private LoseScreenController _loseScreenController;
        [SerializeField] private WinScreenController _winScreenController;
        [SerializeField] private SettingsController _settingsController;
        
        
        [SerializeField] private Button nextLevelBtn;
        [SerializeField] private Button previousLevelBtn;
        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _gamePlayService.LevelFinishedEvent += OnLevelFinished;
            SetLevelText();
            SetButtonBehaviours();
        }
        
        

        private void OnLevelFinished(object sender, LevelFinishedType e)
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

        private void OpenFailScreen()
        {
            _loseScreenController.Activate();
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

        void OnRestartButtonClicked()
        {
       _gamePlayService.LoadLevel();

        }
        
        private void CloseSettingScreen()
        {
            _settingsController.Deactivate();
        }
        
        private void SetLevelText()
        {
            levelTxt.text ="LV " +_gamePlayService.GetCurrentLevel();
        }

        private void SetButtonBehaviours()
        {
            nextLevelBtn.onClick.AddListener(() =>
            {
                _gamePlayService.LoadNext();
            });
            
            previousLevelBtn.onClick.AddListener(() =>
            {
                _gamePlayService.LoadPrevious();
            });
            settingsBtn.onClick.AddListener(() =>
            {
                OpenSettingsScreen();
            });
            
            restartBtn.onClick.AddListener(() =>
            {
                OnRestartButtonClicked();
            });
        }

        private void OnDestroy()
        {
            _gamePlayService.LevelFinishedEvent -= OnLevelFinished;
        }
    }
}