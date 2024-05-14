using System.Collections.Generic;
using Events.InGame.EventArgs;
using Events.Mission.EventArgs;

namespace Services.Analytics.Interfaces
{
    public interface IProgressionAnalyticEvents
    {
        public void FeatureUnlocked(FeatureUnlockedEventArgs args, Dictionary<string, object> additionalData = null);
        public void MissionStarted(MissionEventArgs args, Dictionary<string, object> additionalData = null);
        public void MissionCompleted(MissionCompletedEventArgs args, Dictionary<string, object> additionalData = null);
        public void PowerUpUsed(PowerUpUsedEventArgs args, Dictionary<string, object> additionalData = null);
    }
} 