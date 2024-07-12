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

        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            retryButton.onClick.AddListener(NextButtonClick);
            reviveButton.onClick.AddListener(NextButtonClick);
        }

        public void Activate(LevelFailedType failedType)
        {
            loseScreenCanvasGroup.blocksRaycasts = true;
            loseScreenCanvasGroup.interactable = true;
            if (failedType == LevelFailedType.OutOfEmptyLots)
            {
                loseScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                {
                    _soundService.PlaySound(SoundTypeEnum.LoseSound);
                });
            }
            else
            {
                reviveScreenCanvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
                {
                    _soundService.PlaySound(SoundTypeEnum.LoseSound);
                });
            }
        }

        private void NextButtonClick()
        {
            _gamePlayService.LoadLevel();
            _soundService.PlaySound(SoundTypeEnum.ButtonClickedSound);
        }

        private void ReviveButtonClicked()
        {
        }
    }
}