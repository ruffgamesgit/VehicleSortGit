using Core.Locator;
using Services.Analytics.Interfaces;

namespace Core.Services.Analytics
{
    public interface IAnalyticsService : IProgressionAnalyticEvents, IEconomyAnalyticsEvents, 
        ILevelAnalyticsEvents,IAdvertisingAnalyticEvents, IService
    {
        
    }


}