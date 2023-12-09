using Microsoft.Windows.Widgets.Feeds.Providers;

namespace CustomFeedCS;

public class FeedProvider : IFeedProvider
{
    public void OnCustomQueryParametersRequested(CustomQueryParametersRequestedArgs args)
    {
        FeedManager.GetDefault().SetCustomQueryParameters(new(args.FeedProviderDefinitionId, "?widgets=True"));
    }

    public void OnFeedDisabled(FeedDisabledArgs args)
    {
    }

    public void OnFeedEnabled(FeedEnabledArgs args)
    {
    }

    public void OnFeedProviderDisabled(FeedProviderDisabledArgs args)
    {
    }

    public void OnFeedProviderEnabled(FeedProviderEnabledArgs args)
    {
    }
}
