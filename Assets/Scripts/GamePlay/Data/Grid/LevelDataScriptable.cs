using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Data.Grid
{
    [CreateAssetMenu(fileName = "Data", menuName = "LevelData", order = 0)]
    public class LevelDataScriptable : ScriptableObject
    {
        public List<LevelData> levelData;
    }
}