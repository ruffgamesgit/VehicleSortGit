using Core.Locator;
using GamePlay.Data.Grid;

namespace Core.Services
{
    public interface IGamePlayService : IService
    {
        public LevelData GetCurrentLevelData();
    }
}