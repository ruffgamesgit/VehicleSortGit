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
        [SerializeField] private Button nextLevelBtn;
        [SerializeField] private Button previousLevelBtn;
        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            SetLevelText();
            SetButtonBehaviours();
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
        }
    }
}