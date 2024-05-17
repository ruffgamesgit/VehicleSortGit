using System;

using Core.Locator;
using Core.Services.GamePlay;
using DG.Tweening;
using Services.Sound;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class WinScreenController : MonoBehaviour
    {
        [SerializeField]private CanvasGroup canvasGroup;
        [SerializeField] private Button nextButton;
        [SerializeField] private GameObject confetti;
        
        private IGamePlayService _gamePlayService;
        private ISoundService _soundService;
        
        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            nextButton.onClick.AddListener(NextButtonClick);
        }

        public void Activate()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            if(confetti != null)
                confetti.SetActive(true);
            canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
            {
                _soundService.PlaySound(SoundTypeEnum.WinSound);
            }).SetDelay(1f);
        }

        private void NextButtonClick()
        {
            _gamePlayService.LoadLevel();
        }
    }
}