using Events.Level.EventArgs;
using LionStudios.Suite.Analytics;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Progress
{
    public class LevelCompleteEventArgsReflection : ReflectionEventBase
    {
        public EventType EventType { get; private set; }
        public int LevelNum { get; private set; }
        public int? AttemptNum { get; private set; }
        public int? Score { get; private set; }
        public string LevelCollection1 { get; private set; }
        public string LevelCollection2 { get; private set; }
        public string MissionType { get; private set; }
        public string MissionName { get; private set; }
        public bool? IsTutorial { get; private set; }
        public bool IncrementCounter { get; private set; }
        
        public Reward Reward { get; private set; } = null;

        public LevelCompleteEventArgsReflection(LevelCompleteEventArgs args)
        {
            EventType = args.EventType;
            LevelNum = args.LevelNum;
            AttemptNum = args.AttemptNum;
            Score = args.Score;
            LevelCollection1 = args.LevelCollection1;
            LevelCollection2 = args.LevelCollection2;
            MissionType = args.MissionType;
            MissionName = args.MissionName;
            IsTutorial = args.IsTutorial;
            IncrementCounter = args.IncrementCounter;
            Reward = args.Reward;
        }
    }
}