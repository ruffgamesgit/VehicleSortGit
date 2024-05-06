using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    public event System.Action LevelEndedEvent;
    public event System.Action LevelFailedEvent;
    public event System.Action LevelSuccessEvent;

    [Header("Debug")]
    public bool isLevelActive;
    [SerializeField] int totalSceneCount;
    [SerializeField] int sortableVehicleCount;


    protected override void Awake()
    {
        base.Awake();

        isLevelActive = true;
        totalSceneCount = SceneManager.sceneCountInBuildSettings;

    }

    private void Start()
    {
        SetSortableVehicleCount(LevelGenerator.instance.DesiredPassengerStackCount);
    }

    void SetSortableVehicleCount(int count)
    {
        sortableVehicleCount = count / 4;
    }

    public void OnVehicleDisappears()
    {

        sortableVehicleCount--;

        if (sortableVehicleCount == 0)
            EndGame(success: true, .75f);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            EndGame(true);
        }
    }
    #region Level Management Related

    public void EndGame(bool success, float delayAsSeconds = 0)
    {
        if (!isLevelActive) return;

        isLevelActive = false;

        if (!success) LevelFailedEvent?.Invoke();
        else LevelSuccessEvent?.Invoke();

    }
    public void OnTapRestart()
    {
        LevelEndedEvent?.Invoke();

        isLevelActive = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
    public void OnTapNext()
    {
        LevelEndedEvent?.Invoke();
        isLevelActive = false;

        #region  Cumulative Next Level

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= totalSceneCount) nextSceneIndex = 0;
        SceneManager.LoadScene(nextSceneIndex);

        #endregion
    }

    #endregion
}
