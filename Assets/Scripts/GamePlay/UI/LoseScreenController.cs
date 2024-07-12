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
    public class LoseScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup loseScreenCanvasGroup;
        [SerializeField] private CanvasGroup reviveScreenCanvasGroup;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button reviveButton;

        private IGamePlayService _gamePlayService;
        private ISoundService _soundService;

        private Action _onRevive;
        
        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            retryButton.onClick.AddListener(GiveUpButtonClick);
            reviveButton.onClick.AddListener(ReviveButtonClicked);
        }

        public void Activate(LevelFailedType failedType, Action onRevive = null)
        {
            _onRevive = onRevive;
            loseScreenCanvasGroup.blocksRaycasts = true;
            loseScreenCanvasGroup.interactable = true;

            switch (failedType)
            {
                case LevelFailedType.Move:
                    reviveScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                    {
                        _soundService.PlaySound(SoundTypeEnum.LoseSound);
                    });
                    break;
                case LevelFailedType.Space:
                    loseScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                    {
                        _soundService.PlaySound(SoundTypeEnum.LoseSound);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(failedType), failedType, null);
            }
        }

        private void GiveUpButtonClick()
        {
            _gamePlayService.LoadLevel();
            _soundService.PlaySound(SoundTypeEnum.ButtonClickedSound);
        }

        private void ReviveButtonClicked()
        {
            _onRevive?.Invoke();
        }
    }
}