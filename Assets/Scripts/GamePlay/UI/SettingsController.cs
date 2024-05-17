using Core.Locator;
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

        [SerializeField] private GameObject soundActiveState;
        [SerializeField] private GameObject soundInactiveState;
        [SerializeField] private GameObject hapticActiveState;
        [SerializeField] private GameObject hapticInactiveState;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private ISoundService _soundService;
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            _soundService = ServiceLocator.Instance.Resolve<ISoundService>();
            SetSettingsStates();
            
            hapticButton.onClick.AddListener(() =>
            {
                //_soundService.SetHapticEnabled(!Taptic.tapticOn);
                SetSettingsStates();
            });
            soundButton.onClick.AddListener(() =>
            {
                _soundService.SetSoundEnabled(!_soundService.IsSoundEnabled());
                SetSettingsStates();
            });
            exitButton.onClick.AddListener(() =>
            {
                canvasGroup.DOFade(0, 0.25f).OnComplete(() =>
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                });
            });
        }

        private void SetSettingsStates()
        {
            soundActiveState.SetActive(_soundService.IsSoundEnabled());
            soundInactiveState.SetActive(!_soundService.IsSoundEnabled());
           // hapticActiveState.SetActive(Taptic.tapticOn);
            //hapticInactiveState.SetActive(!Taptic.tapticOn);
        }
        
    }
}