# Overview of Feed Providers

## Implementation
Feed providers are implemented using three components:
- APPX app extensions
- A COM server (the widget feed provider)
- A website, rendering in the Widget Board using an `<iframe>`

## Behavior
When Widget launches or detects a new packaged app, it will look into the app's manifest for an app extension for `com.microsoft.windows.widgets.feeds`.
If the app extension is declared properly, Widgets will show the feed provider to the user.

When a feed provider is enabled, it will:
1. Activate the COM server declared within the manifest.
2. Create an instance of `IFeedProvider` using that COM server.
3. Call `IFeedProvider.OnFeedProviderEnabled` and `IFeedProvider.OnFeedEnabled`.
4. Call `IFeedProvider.OnCustomQueryParametersRequested`.
5. Combining the declared `ContentUri` and the query parameters retrieved from step 4, create a final content URI.
6. Navigate to that content URI using an `<iframe>` and diplay it.

## APPX app extensions
Feed providers are declared using APPX app extensions, which are defined in the package's `AppxManifest.xml`. A psuedo implementation would like this:
```xml
<uap3:Extension Category="windows.appExtension">
    <uap3:AppExtension Name="com.microsoft.windows.widgets.feeds" DisplayName="Custom Feed" Id="com.example.customFeedProvider" PublicFolder="Public">
       <uap3:Properties>
            <FeedProvider
                SettingsUri="https://www.example.com"
                Icon="Images\FeedLogo.png"
                Description="Lorem ipsum dolor sit amet">
                <Activation>
                    <CreateInstance ClassId="606dbaa8-4054-4865-8ddf-4d518a770a4c" />
                </Activation>
                <Definitions>
                    <Definition
                        Id="com.example.customFeed"
                        DisplayName="Custom Feed"
                        Description="Lorem ipsum door sit amet"
                        ContentUri="https://www.example.com"
                        AllowMultiple="false"
                        Icon="Images\FeedLogo.png" />
                </Definitions>
            </FeedProvider>
        </uap3:Properties>
    </uap3:AppExtension>
</uap3:Extension>
```

### `FeedProvider`
Declares a feed provider.

#### Attributes
| Attribute   | Type   | Required | Description
|-------------|--------|----------|---------------------------------------------------------------------------------------
| SettingsUri | String | Yes      | The URI the Widget Board should open when the user chooses to customize the provider.
| Icon        | String | Yes      | A path relative to your package root that is the feed provider icon.
| Description | String | Yes      | A description of the feed provider.

#### Child elements
- [`Activation`](#activation)
- [`Definitions`](#definitions)

### `Activation`
Declares how the widget provider should be activated.

#### Child elements
- [`CreateInstance`](#createinstance)

### `CreateInstance`
Declares that the widget provider should be activated through a COM server.

#### Attributes
| Attribute | Type   | Required | Description
|-----------|--------|----------|-----------------------------------------------------------------------------------
| ClassId   | GUID   | Yes      | The CLSID of the COM server to an `IClassFactory` that creates an `IFeedProvider`

### `Definitions`
A list of widget feeds.

#### Child elements
- [`Definition`](#definition)

### `Definition`
Declares a widget feed.

#### Attributes
| Attribute     | Type    | Required | Description
|---------------|---------|----------|----------------------------------------------------------------------
| Id            | String  | Yes      | The unique identifier of the widget feed.
| Icon          | String  | Yes      | A path relative to your package root that is the feed provider icon.
| Description   | String  | Yes      | A description of the feed.
| DisplayName   | String  | Yes      | The display name of the feed.
| ContentUri    | String  | Yes      | The URI of the content feed.
| AllowMultiple | Boolean | No       | ?????

## The COM server
The CLSID passed in to `<CreateInstance>` must declare the [`IClassFactory`](https://learn.microsoft.com/windows/win32/api/unknwn/nn-unknwn-iclassfactory) interface.

Upon activation, Widgets will call `IClassFactory.CreateInstance`. The server is expected to return an `IFeedProvider`, queried using the IID passed in.
A pseudo implementation of such `IClassFactory` might look like this:

```cs
class FeedFactory : IClassFactory
{
    IFeedProvider instance;
    Mutex mutex;

    HRESULT CreateInstance(IUnknown* outer, GUID* iid, void** result)
    {
        *result = null;

        LockMutex(mutex);

        if (outer != null)
            return CLASS_E_NOAGGREGATION;
        
        if (instance != null)
            instance = new FeedProvider();
        
        return instance.As(iid, result);
    }

    HRESULT LockServer(bool fLock)
    {
        return S_OK;
    }
}
```

Here's a pseudo implementation of such `IWidgetFactory`.
Note that when `OnCustomQueryParametersRequested` is called, the provider should call `FeedManager.SetCustomQueryParameters`
```cs
using Microsoft.Windows.Widgets.Feeds.Providers;

class FeedProvider : IFeedProvider
{
    public void OnCustomQueryParametersRequested(CustomQueryParametersRequestedArgs args)
    {
        string queryParameters = "?example=example";
        FeedManager.GetDefault().SetCustomQueryParameters(new CustomQueryParametersUpdateOptions(args.FeedProviderDefinitionId, queryParameters));
    }

    public void OnFeedProviderEnabled(FeedProviderEnabledArgs args)
    {

    }

    public void OnFeedProviderDisabled(FeedProviderDisabledArgs args)
    {
        
    }

    public void OnFeedEnabled(FeedEnabledArgs args)
    {
        
    }

    public void OnFeedDisabled(FeedDisabledArgs args)
    {
        
    }
}
```