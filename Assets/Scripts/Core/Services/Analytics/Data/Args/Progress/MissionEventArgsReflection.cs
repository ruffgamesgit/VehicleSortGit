using Events.Mission.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Progress
{
    public class MissionEventArgsReflection : ReflectionEventBase
    {
        public bool IsTutorial { get; private set; }
        public string MissionType { get; private set; }
        public string MissionName { get; private set; }
        public string MissionID { get; private set; }
        public int? MissionAttempt { get; private set; }
        public int UserScore { get; private set; }
        public bool IncrementCounter { get; private set; }
        
        public MissionEventArgsReflection(MissionEventArgs args)
        {
            IsTutorial = args.IsTutorial;
            MissionName = args.MissionName;
            MissionType = args.MissionType;
            MissionID = args.MissionID;
            MissionAttempt = args.MissionAttempt;
            UserScore = args.UserScore;
            IncrementCounter = args.IncrementCounter;
        }
    }
}