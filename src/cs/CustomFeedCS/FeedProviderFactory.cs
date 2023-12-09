using Microsoft.Windows.Widgets.Feeds.Providers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using WinRT;

namespace CustomFeedCS;

[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IClassFactory
{
    void CreateInstance(nint pUnkOuter, in Guid riid, out nint ppvObject);

    void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
}

[GeneratedComClass]
public partial class FeedProviderFactory : IClassFactory
{
    private readonly IFeedProvider instance;

    public FeedProviderFactory(IFeedProvider instance)
        => this.instance = instance;

    public void CreateInstance(nint pUnkOuter, in Guid riid, out nint ppvObject)
    {
        ppvObject = 0;

        if (pUnkOuter != 0)
            throw new COMException(string.Empty, -2147221232); // CLASS_E_NOAGGREGATION

        ppvObject = MarshalInspectable<IFeedProvider>.FromManaged(instance);
    }

    public void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
    { }
}