using LionStudios.Suite.Analytics.Events.CrossPromo.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args
{
    public class CrossPromoEventArgsReflection : ReflectionEventBase
    {
        public string Placement { get; private set; }
        public string Network { get; private set; }
        public int? Level { get; private set; } = null;

        public CrossPromoEventArgsReflection(CrossPromoEventArgs args)
        {
            Placement = args.Placement;
            Network = args.Network;
            Level = args.Level;
        }
    }
}