using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Advertising
{
    public class AdRewardEventArgsReflection : ReflectionEventBase
    {
        public string Placement { get; private set; }
        public object Reward { get; private set; } = null;
        public int? Level { get; private set; } = null;

        public AdRewardEventArgsReflection(AdRewardArgs args)
        {
            Placement = args.Placement;
            Reward = args.Reward;
            Level = args.Level;
        }
    }
}