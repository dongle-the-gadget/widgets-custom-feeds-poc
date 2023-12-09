#include "pch.h"
#include "WidgetFeedProvider.h"

WidgetFeedProvider::WidgetFeedProvider()
{
}

void WidgetFeedProvider::OnCustomQueryParametersRequested(CustomQueryParametersRequestedArgs args)
{
	printf("Custom query parmaeters requested for feed provider %ls!\n", args.FeedProviderDefinitionId().c_str());
	FeedManager::GetDefault().SetCustomQueryParameters(CustomQueryParametersUpdateOptions(args.FeedProviderDefinitionId(), L"?widgets=True"));
}

void WidgetFeedProvider::OnFeedDisabled(FeedDisabledArgs args)
{
	printf("Feed %ls disabled!\n", args.FeedDefinitionId().c_str());
}

void WidgetFeedProvider::OnFeedEnabled(FeedEnabledArgs args)
{
	printf("Feed %ls enabled!\n", args.FeedDefinitionId().c_str());
}

void WidgetFeedProvider::OnFeedProviderDisabled(FeedProviderDisabledArgs args)
{
	printf("Feed provider %ls disabled!\n", args.FeedProviderDefinitionId().c_str());
}

void WidgetFeedProvider::OnFeedProviderEnabled(FeedProviderEnabledArgs args)
{
	printf("Feed provider %ls enabled!\n", args.FeedProviderDefinitionId().c_str());
}