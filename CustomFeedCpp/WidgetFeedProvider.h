#pragma once
#include <winrt/Microsoft.Windows.Widgets.Feeds.Providers.h>

using namespace winrt::Microsoft::Windows::Widgets::Feeds::Providers;

struct WidgetFeedProvider : winrt::implements<WidgetFeedProvider, IFeedProvider>
{
	WidgetFeedProvider();

	void OnCustomQueryParametersRequested(CustomQueryParametersRequestedArgs args);
	void OnFeedDisabled(FeedDisabledArgs args);
	void OnFeedEnabled(FeedEnabledArgs args);
	void OnFeedProviderDisabled(FeedProviderDisabledArgs args);
	void OnFeedProviderEnabled(FeedProviderEnabledArgs args);
};

