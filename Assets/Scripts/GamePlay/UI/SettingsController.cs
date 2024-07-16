using Core.Locator;
using Core.Services.GamePlay;
using DG.Tweening;
using Services.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button hapticButton;
        [SerializeField] private Button restartButton;

        [SerializeField] private GameObject soundActiveState;
        [SerializeField] private GameObject soundInactiveState;
        [SerializeField] private GameObject hapticActiveState;
        [SerializeField] private GameObject hapticInactiveState;
        [SerializeField] private CanvasGroup canvasGroup;

        private ISoundService _soundService;
        private IGamePlayService _gamePlayService;
        private Sequence _sequence;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            _gamePlayService = ServiceLocator.Instance.Resolve<IGamePlayService>();
            SetSettingsStates();

            hapticButton.onClick.AddListener(() =>
            {
                _soundService.SetHapticEnabled(!Taptic.tapticOn);
                _soundService.PlaySound(SoundTypeEnum.PassengerMoveSound);
                SetSettingsStates();
            });
            soundButton.onClick.AddListener(() =>
            {
                _soundService.SetSoundEnabled(!_soundService.IsSoundEnabled());
                _soundService.PlaySound(SoundTypeEnum.PassengerMoveSound);
                SetSettingsStates();
            });
            exitButton.onClick.AddListener(() =>
            {
                Deactivate();
                _soundService.PlaySound(SoundTypeEnum.PassengerMoveSound);
            });
            restartButton.onClick.AddListener(() =>
            {
                _gamePlayService.LoadLevel();
                _soundService.PlaySound(SoundTypeEnum.PassengerMoveSound);
                Deactivate();
            });
        }

        public void Activate()
        {
            _sequence?.Kill(true);
            _gamePlayService.SettingsEnabled(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            _soundService.PlaySound(SoundTypeEnum.PassengerMoveSound);
            _sequence = DOTween.Sequence();
            _sequence.Join(canvasGroup.DOFade(1, 0.5f));
        }

        public void Deactivate()
        {
            if (canvasGroup.alpha == 0) return;
            _sequence?.Kill(true);
            _sequence = DOTween.Sequence();

            _sequence.Join(canvasGroup.DOFade(0, 0.25f).OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                _gamePlayService.SettingsEnabled(false);
            }));
        }

        private void SetSettingsStates()
        {
            soundActiveState.SetActive(_soundService.IsSoundEnabled());
            soundInactiveState.SetActive(!_soundService.IsSoundEnabled());
            hapticActiveState.SetActive(Taptic.tapticOn);
            hapticInactiveState.SetActive(!Taptic.tapticOn);
        }
    }
}