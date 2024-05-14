using Events.InGame.EventArgs;
using Services.Analytics.Data.Args.BaseReflection;

namespace Services.Analytics.Data.Args.Progress
{
    public class FeatureUnlockedEventArgsReflection : ReflectionEventBase
    {
        public string FeatureName { get; private set; }
        public string FeatureType { get; private set; }

        public FeatureUnlockedEventArgsReflection(FeatureUnlockedEventArgs args)
        {
            FeatureName = args.FeatureName;
            FeatureType = args.FeatureType;
        }
    }
}