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
        [SerializeField] private Button reviveRetryButton;

        private IGamePlayService _gamePlayService;
        private ISoundService _soundService;

        private Action _onRevive;

        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            retryButton.onClick.AddListener(GiveUpButtonClick);
            reviveButton.onClick.AddListener(ReviveButtonClicked);
            reviveRetryButton.onClick.AddListener(GiveUpButtonClick);
        }

        public void Activate(LevelFailedType failedType, Action onRevive = null)
        {
            _onRevive = onRevive;


            _soundService.PlaySound(SoundTypeEnum.LoseSound);
            switch (failedType)
            {
                case LevelFailedType.Move:
                    reviveScreenCanvasGroup.blocksRaycasts = true;
                    reviveScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                    {
                        reviveScreenCanvasGroup.interactable = true;
                    });
                    break;
                case LevelFailedType.Space:
                    loseScreenCanvasGroup.blocksRaycasts = true;
                    loseScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                    {
                        loseScreenCanvasGroup.interactable = true;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(failedType), failedType, null);
            }
        }

        public void Deactivate()
        {
            reviveScreenCanvasGroup.blocksRaycasts = false;
            reviveScreenCanvasGroup.interactable = false;
            reviveScreenCanvasGroup.DOFade(0, 0.15f).SetDelay(1f);

            loseScreenCanvasGroup.blocksRaycasts = false;
            loseScreenCanvasGroup.interactable = false;
            loseScreenCanvasGroup.DOFade(0, 0.15f).SetDelay(1f);
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