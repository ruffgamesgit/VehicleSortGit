using System;

using Core.Locator;
using Core.Services.GamePlay;
using Cysharp.Threading.Tasks;
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
            var sequence = DOTween.Sequence();
            sequence.Insert(1f,canvasGroup.DOFade(1, 0.5f));
            sequence.InsertCallback(0.5f,() =>
            {
                if(confetti != null)
                    confetti.SetActive(true);
                _soundService.PlaySound(SoundTypeEnum.WinSound);
            });
            
        }

        private void NextButtonClick()
        {
            _soundService.PlaySound(SoundTypeEnum.ButtonClickedSound);
            _gamePlayService.LoadLevel();
        }
    }
}