using Events.InGame.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Progress
{
    public class PowerUpUsedEventArgsReflection : ReflectionEventBase
    {
        public string MissionID { set; private get; }
        public string MissionType { set; private get; }
        public int MissionAttempt { set; private get; }
        public string PowerUpName { set; private get; }
        public string MissionName { set; private get; }
        
        public PowerUpUsedEventArgsReflection(PowerUpUsedEventArgs args)
        {
            MissionID = args.MissionID;
            MissionType = args.MissionType;
            MissionAttempt = args.MissionAttempt;
            PowerUpName = args.PowerUpName;
            MissionName = args.MissionName;
        }
    }
}