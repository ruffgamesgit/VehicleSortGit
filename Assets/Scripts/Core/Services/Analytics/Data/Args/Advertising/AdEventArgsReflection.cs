using LionStudios.Suite.Analytics.Events.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Advertising
{
    public class AdEventArgsReflection : ReflectionEventBase
    {
        public string Network { get; private set; } = "unknown";
        public int? Level { get; private set; } = null;
        public string Placement { get; private set; } = "no_placement";

        public AdEventArgsReflection(AdEventArgs args)
        {
            Network = args.Network;
            Level = args.Level;
            Placement = args.Placement;
        }
    }
}