using System.Collections.Generic;
using GamePlay.Data.Grid;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Services
{
    public class GamePlayService : IGamePlayService
    {
        private List<LevelData> _levelData;
        
        public GamePlayService()
        {
            _levelData = Resources.Load<LevelDataScriptable>("LevelData").levelData;
        }

        public int GetCurrentLevelIndex()
        {
            return 0;
        }
        
        public LevelData GetCurrentLevelData()
        {
            return _levelData[GetCurrentLevelIndex()];
        }

        public void LoadLevel()
        {
            var currentLevelData = GetCurrentLevelData();
            Scene scene = SceneManager.GetSceneByName(currentLevelData.scene);
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}