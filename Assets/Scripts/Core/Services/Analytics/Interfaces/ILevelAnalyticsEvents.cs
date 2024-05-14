using System.Collections.Generic;
using Events.Level.EventArgs;

namespace Services.Analytics.Interfaces
{
    public interface  ILevelAnalyticsEvents
    {
        public void LevelStart(LevelEventArgs args, Dictionary<string, object> additionalData = null);

        public void LevelFail(LevelEventArgs args, Dictionary<string, object> additionalData = null);

        public void LevelRestart(LevelEventArgs args, Dictionary<string, object> additionalData = null);

        public void LevelAbandoned(LevelEventArgs args, Dictionary<string, object> additionalData = null);

        public void LevelComplete(LevelCompleteEventArgs args, Dictionary<string, object> additionalData = null);
    }
}