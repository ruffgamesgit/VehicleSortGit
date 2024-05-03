using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] CanvasGroup failPanel;

    private void Start()
    {
        GameManager.instance.LevelFailedEvent += OnLevelFailed;
    }

    private void OnLevelFailed()
    {
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
