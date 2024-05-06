using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] CanvasGroup failPanel;
    [SerializeField] CanvasGroup successPanel;

    private void Start()
    {
        GameManager.instance.LevelFailedEvent += OnLevelFailed;
        GameManager.instance.LevelSuccessEvent += OnLevelSuccess;
    }

    private void OnLevelSuccess()
    {
        successPanel.gameObject.SetActive(true);
        successPanel.DOFade(1, .5f);
    }

    private void OnLevelFailed()
    {
        failPanel.gameObject.SetActive(true);
        failPanel.DOFade(1, .5f);
    }

    public void OnTapRestart()
    {
        GameManager.instance.OnTapRestart();
    }

    public void OnTapNext()
    {
        GameManager.instance.OnTapNext();
    }
}
