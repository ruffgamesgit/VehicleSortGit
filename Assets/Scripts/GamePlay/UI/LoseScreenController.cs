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
        [SerializeField]private CanvasGroup canvasGroup;
        [SerializeField]private Button retryButton;
        
        private IGamePlayService _gamePlayService;
        private ISoundService _soundService;
        
        private void Awake()
        {
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            retryButton.onClick.AddListener(NextButtonClick);
        }

        public void Activate()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1, 0.5f).SetDelay(1f).OnComplete(() =>
            {
                _soundService.PlaySound(SoundTypeEnum.LoseSound);
            });
        }

        private void NextButtonClick()
        {
            _gamePlayService.LoadLevel();
        }
    }
}