using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Widgets.Feeds.Providers;
using WinRT;

namespace CustomFeedProvider;

[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
public partial interface IClassFactory
{
    [PreserveSig]
    int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

    [PreserveSig]
    int LockServer([MarshalAs(UnmanagedType.Bool)]bool fLock);
}


[GeneratedComClass]
public partial class WidgetFeedProviderFactory<T> : IClassFactory where T : IFeedProvider, new()
{
    private static readonly Guid IUnknownGuid = new Guid(0x00000000, 0x0000, 0x0000, [0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46]);

    public int CreateInstance(nint pUnkOuter, ref Guid riid, out nint ppvObject)
    {
        Console.WriteLine("Create Instance called");

        ppvObject = IntPtr.Zero;
        if (pUnkOuter != IntPtr.Zero)
        {
            return -2147221232; // CLASS_E_NOAGGREGATION
        }

        if (riid == typeof(T).GUID || riid == IUnknownGuid)
        {
            ppvObject = MarshalInspectable<IFeedProvider>.FromManaged(new T());
        }
        else
        {
            return -2147467262; // E_NOINTERFACE
        }

        return 0; // S_OK
    }

    public int LockServer(bool fLock)
    {
        return 0; // S_OK
    }
}
