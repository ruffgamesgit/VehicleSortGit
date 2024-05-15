using System;
using Core.Locator;
using GamePlay.Data.Grid;

namespace Core.Services.GamePlay
{
    public interface IGamePlayService : IService
    {
        public event EventHandler<LevelFinishedType> LevelFinishedEvent;
        public void LevelFinished(LevelFinishedType type);
        public LevelData GetCurrentLevelData();
        public int GetCurrentLevel();
        public void LoadLevel();

        public void LoadPrevious();
        public void LoadNext();

    }
}