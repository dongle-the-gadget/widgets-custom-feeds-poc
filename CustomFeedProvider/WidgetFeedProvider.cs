using Microsoft.Windows.Widgets.Feeds.Providers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CustomFeedProvider;

[Guid("606dbaa8-4054-4865-8ddf-4d518a770a4c")]
[ComVisible(true)]
[ComDefaultInterface(typeof(IFeedProvider))]
public partial class WidgetFeedProvider : IFeedProvider
{
    public void OnCustomQueryParametersRequested(CustomQueryParametersRequestedArgs args)
    {
        Console.WriteLine("Custom query parameters requested for feed provider: {0}", args.FeedProviderDefinitionId);
    }

    public void OnFeedDisabled(FeedDisabledArgs args)
    {
        Console.WriteLine("Feed disabled: {0}, provider: {1}", args.FeedDefinitionId, args.FeedProviderDefinitionId);
    }

    public void OnFeedEnabled(FeedEnabledArgs args)
    {
        Console.WriteLine("Feed enabled: {0}, provider: {1}", args.FeedDefinitionId, args.FeedProviderDefinitionId);
    }

    public void OnFeedProviderDisabled(FeedProviderDisabledArgs args)
    {
        Console.WriteLine("Feed provider disabled: {0}", args.FeedProviderDefinitionId);
    }

    public void OnFeedProviderEnabled(FeedProviderEnabledArgs args)
    {
        Console.WriteLine("Feed provider enabled: {0}", args.FeedProviderDefinitionId);
    }
}