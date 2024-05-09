using System.Collections.Generic;
using GamePlay.Data.Grid;
using UnityEngine;

namespace Core.Services
{
    public class GamePlayService : IGamePlayService
    {
        private List<LevelData> _levelData;
        
        public GamePlayService()
        {
            _levelData = Resources.Load<LevelDataScriptable>("LevelData").levelData;
        }
        
        public LevelData GetCurrentLevelData()
        {
            return _levelData[0];
        }
    }
}