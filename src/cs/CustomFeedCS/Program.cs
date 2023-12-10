using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CustomFeedCS;

internal partial class Program
{
    [LibraryImport("ole32.dll")]
    private static partial int CoRegisterClassObject(
                    Guid rclsid,
                    nint pUnk,
                    uint dwClsContext,
                    uint flags,
                    out uint lpdwRegister);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBoxW(
        nint hWnd,
        string? lpText,
        string? lpCaption,
        uint uType);

    private static FeedProviderFactory providerFactory;
    private static StrategyBasedComWrappers comWrappers;

    private static readonly Guid factoryGuid = new("50F82A28-5EAD-4E04-BF8A-72EA4CA3D73C");

    static async Task Main(string[] args)
    {
        if (args.Length == 0 || args[0] != "-RegisterProcessAsComServer")
        {
            MessageBoxW(0, "The sample should be launched from Widgets", null, 0);
            return;
        }

        providerFactory = new(new FeedProvider());
        comWrappers = new();

        var hr = CoRegisterClassObject(
            factoryGuid,
            comWrappers.GetOrCreateComInterfaceForObject(providerFactory, CreateComInterfaceFlags.None),
            0x4,
            0x1,
            out uint _);

        if (hr != 0)
        {
            var exp = Marshal.GetExceptionForHR(hr)!;
            MessageBoxW(0, $"COM error: {exp.Message}", null, 0x10);
        }

        await Task.Delay(-1);
    }
}