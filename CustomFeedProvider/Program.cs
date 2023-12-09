using Microsoft.Windows.Widgets.Feeds.Hosts;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Foundation;
using WinRT;

namespace CustomFeedProvider;

internal partial class Program
{
    [DllImport("kernel32.dll")]
    public static extern nint GetConsoleWindow();

    [LibraryImport("ole32.dll")]
    public static partial int CoRegisterClassObject(
        Guid rclsId,
        nint pUnk,
        uint dwClsContext,
        uint flags,
        out uint lpdwRegister);

    [LibraryImport("ole32.dll")]
    public static unsafe partial int CoCreateInstance(ref Guid rclsid, nint pUnkOuter, uint dwClsContext, Guid* riid, void** ppv);

    [DllImport("ole32.dll")]
    public static extern int CoRevokeClassObject(uint dwRegister);

    /// <summary>A type that allows working with pointers to COM objects more securely.</summary>
    /// <typeparam name="T">The type to wrap in the current <see cref="ComPtr{T}"/> instance.</typeparam>
    /// <remarks>While this type is not marked as <see langword="ref"/> so that it can also be used in fields, make sure to keep the reference counts properly tracked if you do store <see cref="ComPtr{T}"/> instances on the heap.</remarks>
    public unsafe struct ComPtr<T> : IDisposable
        where T : unmanaged, IUnknown.Interface
    {
        /// <summary>Retrieves the GUID of of a specified type.</summary>
        /// <param name="value">A value of type <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type to retrieve the GUID for.</typeparam>
        /// <returns>A <see cref="UuidOfType"/> value wrapping a pointer to the GUID data for the input type. This value can be either converted to a <see cref="Guid"/> pointer, or implicitly assigned to a <see cref="Guid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UuidOfType __uuidof<T>(T value) // for type inference similar to C++'s __uuidof
            where T : unmanaged
        {
            return new UuidOfType(UUID<T>.RIID);
        }

        /// <summary>Retrieves the GUID of of a specified type.</summary>
        /// <param name="value">A pointer to a value of type <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type to retrieve the GUID for.</typeparam>
        /// <returns>A <see cref="UuidOfType"/> value wrapping a pointer to the GUID data for the input type. This value can be either converted to a <see cref="Guid"/> pointer, or implicitly assigned to a <see cref="Guid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UuidOfType __uuidof<T>(T* value) // for type inference similar to C++'s __uuidof
            where T : unmanaged
        {
            return new UuidOfType(UUID<T>.RIID);
        }

        /// <summary>Retrieves the GUID of of a specified type.</summary>
        /// <typeparam name="T">The type to retrieve the GUID for.</typeparam>
        /// <returns>A <see cref="UuidOfType"/> value wrapping a pointer to the GUID data for the input type. This value can be either converted to a <see cref="Guid"/> pointer, or implicitly assigned to a <see cref="Guid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UuidOfType __uuidof<T>()
            where T : unmanaged
        {
            return new UuidOfType(UUID<T>.RIID);
        }

        /// <summary>A proxy type that wraps a pointer to GUID data. Values of this type can be implicitly converted to and assigned to <see cref="Guid"/>* or <see cref="Guid"/> parameters.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly unsafe ref struct UuidOfType
        {
            private readonly Guid* riid;

            internal UuidOfType(Guid* riid)
            {
                this.riid = riid;
            }

            /// <summary>Reads a <see cref="Guid"/> value from the GUID buffer for a given <see cref="UuidOfType"/> instance.</summary>
            /// <param name="guid">The input <see cref="UuidOfType"/> instance to read data for.</param>
            public static implicit operator Guid(UuidOfType guid) => *guid.riid;

            /// <summary>Returns the <see cref="Guid"/>* pointer to the GUID buffer for a given <see cref="UuidOfType"/> instance.</summary>
            /// <param name="guid">The input <see cref="UuidOfType"/> instance to read data for.</param>
            public static implicit operator Guid*(UuidOfType guid) => guid.riid;
        }

        /// <summary>A helper type to provide static GUID buffers for specific types.</summary>
        /// <typeparam name="T">The type to allocate a GUID buffer for.</typeparam>
        private static unsafe class UUID<T>
            where T : unmanaged
        {
            /// <summary>The pointer to the <see cref="Guid"/> value for the current type.</summary>
            /// <remarks>The target memory area should never be written to.</remarks>
            public static readonly Guid* RIID = CreateRIID();

            /// <summary>Allocates memory for a <see cref="Guid"/> value and initializes it.</summary>
            /// <returns>A pointer to memory holding the <see cref="Guid"/> value for the current type.</returns>
            private static Guid* CreateRIID()
            {
                var p = (Guid*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(T), sizeof(Guid));
                *p = typeof(T).GUID;
                return p;
            }
        }

        /// <summary>The raw pointer to a COM object, if existing.</summary>
        private T* ptr_;

        /// <summary>Creates a new <see cref="ComPtr{T}"/> instance from a raw pointer and increments the ref count.</summary>
        /// <param name="other">The raw pointer to wrap.</param>
        public ComPtr(T* other)
        {
            ptr_ = other;
            InternalAddRef();
        }

        /// <summary>Creates a new <see cref="ComPtr{T}"/> instance from a second one and increments the ref count.</summary>
        /// <param name="other">The other <see cref="ComPtr{T}"/> instance to copy.</param>
        public ComPtr(ComPtr<T> other)
        {
            ptr_ = other.ptr_;
            InternalAddRef();
        }

        /// <summary>Converts a raw pointer to a new <see cref="ComPtr{T}"/> instance and increments the ref count.</summary>
        /// <param name="other">The raw pointer to wrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ComPtr<T>(T* other)
            => new ComPtr<T>(other);

        /// <summary>Unwraps a <see cref="ComPtr{T}"/> instance and returns the internal raw pointer.</summary>
        /// <param name="other">The <see cref="ComPtr{T}"/> instance to unwrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(ComPtr<T> other)
            => other.Get();

        /// <summary>Converts the current object reference to type <typeparamref name="U"/> and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <typeparam name="U">The interface type to use to try casting the current COM object.</typeparam>
        /// <param name="p">A raw pointer to the target <see cref="ComPtr{T}"/> value to write to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target type <typeparamref name="U"/>.</returns>
        /// <remarks>This method will automatically release the target COM object pointed to by <paramref name="p"/>, if any.</remarks>
        public readonly int As<U>(ComPtr<U>* p)
            where U : unmanaged, IUnknown.Interface
        {
            return ptr_->QueryInterface(__uuidof<U>(), (void**)p->ReleaseAndGetAddressOf());
        }

        /// <summary>Converts the current object reference to type <typeparamref name="U"/> and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <typeparam name="U">The interface type to use to try casting the current COM object.</typeparam>
        /// <param name="other">A reference to the target <see cref="ComPtr{T}"/> value to write to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target type <typeparamref name="U"/>.</returns>
        /// <remarks>This method will automatically release the target COM object pointed to by <paramref name="other"/>, if any.</remarks>
        public readonly int As<U>(ref ComPtr<U> other)
            where U : unmanaged, IUnknown.Interface
        {
            U* ptr;
            int result = ptr_->QueryInterface(__uuidof<U>(), (void**)&ptr);

            other.Attach(ptr);
            return result;
        }

        /// <summary>Converts the current object reference to a type indicated by the given IID and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <param name="riid">The IID indicating the interface type to convert the COM object reference to.</param>
        /// <param name="other">A raw pointer to the target <see cref="ComPtr{T}"/> value to write to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target IID.</returns>
        /// <remarks>This method will automatically release the target COM object pointed to by <paramref name="other"/>, if any.</remarks>
        public readonly int AsIID(Guid* riid, ComPtr<IUnknown>* other)
        {
            return ptr_->QueryInterface(riid, (void**)other->ReleaseAndGetAddressOf());
        }

        /// <summary>Converts the current object reference to a type indicated by the given IID and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <param name="riid">The IID indicating the interface type to convert the COM object reference to.</param>
        /// <param name="other">A reference to the target <see cref="ComPtr{T}"/> value to write to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target IID.</returns>
        /// <remarks>This method will automatically release the target COM object pointed to by <paramref name="other"/>, if any.</remarks>
        public readonly int AsIID(Guid* riid, ref ComPtr<IUnknown> other)
        {
            IUnknown* ptr;
            int result = ptr_->QueryInterface(riid, (void**)&ptr);

            other.Attach(ptr);
            return result;
        }

        /// <summary>Releases the current COM object, if any, and replaces the internal pointer with an input raw pointer.</summary>
        /// <param name="other">The input raw pointer to wrap.</param>
        /// <remarks>This method will release the current raw pointer, if any, but it will not increment the references for <paramref name="other"/>.</remarks>
        public void Attach(T* other)
        {
            if (ptr_ != null)
            {
                var @ref = ptr_->Release();
                Debug.Assert((@ref != 0) || (ptr_ != other));
            }
            ptr_ = other;
        }

        /// <summary>Returns the raw pointer wrapped by the current instance, and resets the current <see cref="ComPtr{T}"/> value.</summary>
        /// <returns>The raw pointer wrapped by the current <see cref="ComPtr{T}"/> value.</returns>
        /// <remarks>This method will not change the reference count for the COM object in use.</remarks>
        public T* Detach()
        {
            T* ptr = ptr_;
            ptr_ = null;
            return ptr;
        }

        /// <summary>Increments the reference count for the current COM object, if any, and copies its address to a target raw pointer.</summary>
        /// <param name="ptr">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>This method always returns <see cref="S_OK"/>.</returns>
        public readonly int CopyTo(T** ptr)
        {
            InternalAddRef();
            *ptr = ptr_;
            return 0;
        }

        /// <summary>Increments the reference count for the current COM object, if any, and copies its address to a target <see cref="ComPtr{T}"/>.</summary>
        /// <param name="p">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>This method always returns <see cref="S_OK"/>.</returns>
        public readonly int CopyTo(ComPtr<T>* p)
        {
            InternalAddRef();
            *p->ReleaseAndGetAddressOf() = ptr_;
            return 0;
        }

        /// <summary>Increments the reference count for the current COM object, if any, and copies its address to a target <see cref="ComPtr{T}"/>.</summary>
        /// <param name="other">The target reference to copy the address of the current COM object to.</param>
        /// <returns>This method always returns <see cref="S_OK"/>.</returns>
        public readonly int CopyTo(ref ComPtr<T> other)
        {
            InternalAddRef();
            other.Attach(ptr_);
            return 0;
        }

        /// <summary>Converts the current COM object reference to a given interface type and assigns that to a target raw pointer.</summary>
        /// <param name="ptr">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target type <typeparamref name="U"/>.</returns>
        public readonly int CopyTo<U>(U** ptr)
            where U : unmanaged, IUnknown.Interface
        {
            return ptr_->QueryInterface(__uuidof<U>(), (void**)ptr);
        }

        /// <summary>Converts the current COM object reference to a given interface type and assigns that to a target <see cref="ComPtr{T}"/>.</summary>
        /// <param name="p">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target type <typeparamref name="U"/>.</returns>
        public readonly int CopyTo<U>(ComPtr<U>* p)
            where U : unmanaged, IUnknown.Interface
        {
            return ptr_->QueryInterface(__uuidof<U>(), (void**)p->ReleaseAndGetAddressOf());
        }

        /// <summary>Converts the current COM object reference to a given interface type and assigns that to a target <see cref="ComPtr{T}"/>.</summary>
        /// <param name="other">The target reference to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target type <typeparamref name="U"/>.</returns>
        public readonly int CopyTo<U>(ref ComPtr<U> other)
            where U : unmanaged, IUnknown.Interface
        {
            U* ptr;
            int result = ptr_->QueryInterface(__uuidof<U>(), (void**)&ptr);

            other.Attach(ptr);
            return result;
        }

        /// <summary>Converts the current object reference to a type indicated by the given IID and assigns that to a target address.</summary>
        /// <param name="riid">The IID indicating the interface type to convert the COM object reference to.</param>
        /// <param name="ptr">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target IID.</returns>
        public readonly int CopyTo(Guid* riid, void** ptr)
        {
            return ptr_->QueryInterface(riid, ptr);
        }

        /// <summary>Converts the current object reference to a type indicated by the given IID and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <param name="riid">The IID indicating the interface type to convert the COM object reference to.</param>
        /// <param name="p">The target raw pointer to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target IID.</returns>
        public readonly int CopyTo(Guid* riid, ComPtr<IUnknown>* p)
        {
            return ptr_->QueryInterface(riid, (void**)p->ReleaseAndGetAddressOf());
        }

        /// <summary>Converts the current object reference to a type indicated by the given IID and assigns that to a target <see cref="ComPtr{T}"/> value.</summary>
        /// <param name="riid">The IID indicating the interface type to convert the COM object reference to.</param>
        /// <param name="other">The target reference to copy the address of the current COM object to.</param>
        /// <returns>The result of <see cref="IUnknown.QueryInterface"/> for the target IID.</returns>
        public readonly int CopyTo(Guid* riid, ref ComPtr<IUnknown> other)
        {
            IUnknown* ptr;
            int result = ptr_->QueryInterface(riid, (void**)&ptr);

            other.Attach(ptr);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            T* pointer = ptr_;

            if (pointer != null)
            {
                ptr_ = null;
                _ = pointer->Release();
            }
        }

        /// <summary>Gets the currently wrapped raw pointer to a COM object.</summary>
        /// <returns>The raw pointer wrapped by the current <see cref="ComPtr{T}"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T* Get()
        {
            return ptr_;
        }

        /// <summary>Gets the address of the current <see cref="ComPtr{T}"/> instance as a raw <typeparamref name="T"/> double pointer. This method is only valid when the current <see cref="ComPtr{T}"/> instance is on the stack or pinned.
        /// </summary>
        /// <returns>The raw pointer to the current <see cref="ComPtr{T}"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T** GetAddressOf()
        {
            return (T**)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
        }

        /// <summary>Gets the address of the current <see cref="ComPtr{T}"/> instance as a raw <typeparamref name="T"/> double pointer.</summary>
        /// <returns>The raw pointer to the current <see cref="ComPtr{T}"/> instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [UnscopedRef]
        public readonly ref readonly T* GetPinnableReference() => ref ptr_;

        /// <summary>Releases the current COM object in use and gets the address of the <see cref="ComPtr{T}"/> instance as a raw <typeparamref name="T"/> double pointer. This method is only valid when the current <see cref="ComPtr{T}"/> instance is on the stack or pinned.</summary>
        /// <returns>The raw pointer to the current <see cref="ComPtr{T}"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T** ReleaseAndGetAddressOf()
        {
            _ = InternalRelease();
            return GetAddressOf();
        }

        /// <summary>Resets the current instance by decrementing the reference count for the target COM object and setting the internal raw pointer to <see langword="null"/>.</summary>
        /// <returns>The updated reference count for the COM object that was in use, if any.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Reset()
        {
            return InternalRelease();
        }

        /// <summary>Swaps the current COM object reference with that of a given <see cref="ComPtr{T}"/> instance.</summary>
        /// <param name="r">The target <see cref="ComPtr{T}"/> instance to swap with the current one.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap(ComPtr<T>* r)
        {
            T* tmp = ptr_;
            ptr_ = r->ptr_;
            r->ptr_ = tmp;
        }

        /// <summary>Swaps the current COM object reference with that of a given <see cref="ComPtr{T}"/> instance.</summary>
        /// <param name="other">The target <see cref="ComPtr{T}"/> instance to swap with the current one.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap(ref ComPtr<T> other)
        {
            T* tmp = ptr_;
            ptr_ = other.ptr_;
            other.ptr_ = tmp;
        }

        // Increments the reference count for the current COM object, if any
        private readonly void InternalAddRef()
        {
            T* temp = ptr_;

            if (temp != null)
            {
                _ = temp->AddRef();
            }
        }

        // Decrements the reference count for the current COM object, if any
        private uint InternalRelease()
        {
            uint @ref = 0;
            T* temp = ptr_;

            if (temp != null)
            {
                ptr_ = null;
                @ref = temp->Release();
            }

            return @ref;
        }
    }

    /// <include file='IUnknown.xml' path='doc/member[@name="IUnknown"]/*' />
    [Guid("00000000-0000-0000-C000-000000000046")]
    public unsafe partial struct IUnknown : IUnknown.Interface
    {
        public void** lpVtbl;

        /// <include file='IUnknown.xml' path='doc/member[@name="IUnknown.QueryInterface"]/*' />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int QueryInterface(Guid* riid, void** ppvObject)
        {
            return ((delegate* unmanaged[MemberFunction]<IUnknown*, Guid*, void**, int>)(lpVtbl[0]))((IUnknown*)Unsafe.AsPointer(ref this), riid, ppvObject);
        }


        /// <include file='IUnknown.xml' path='doc/member[@name="IUnknown.AddRef"]/*' />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint AddRef()
        {
            return ((delegate* unmanaged[MemberFunction]<IUnknown*, uint>)(lpVtbl[1]))((IUnknown*)Unsafe.AsPointer(ref this));
        }


        /// <include file='IUnknown.xml' path='doc/member[@name="IUnknown.Release"]/*' />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Release()
        {
            return ((delegate* unmanaged[MemberFunction]<IUnknown*, uint>)(lpVtbl[2]))((IUnknown*)Unsafe.AsPointer(ref this));
        }


        public interface Interface
        {
            int QueryInterface(Guid* riid, void** ppvObject);
            uint AddRef();
            uint Release();
        }


        public partial struct Vtbl<TSelf>
            where TSelf : unmanaged, Interface
        {
            public delegate* unmanaged[MemberFunction]<TSelf*, Guid*, void**, int> QueryInterface;
            public delegate* unmanaged[MemberFunction]<TSelf*, uint> AddRef;
            public delegate* unmanaged[MemberFunction]<TSelf*, uint> Release;
        }
    }

    // 740FE937-01F7-4482-AA62-C83F0AD3D6D0
    private static Guid CLSID_FeedHost = new Guid(0x740FE937, 0x01F7, 0x4482, [0xAA, 0x62, 0xC8, 0x3F, 0x0A, 0xD3, 0xD6, 0xD0]);
    private static readonly Guid CLSID_Factory = new Guid(0x606dbaa8, 0x4054, 0x4865, [0x8d, 0xdf, 0x4d, 0x51, 0x8a, 0x77, 0x0a, 0x4c]);

    static async Task Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();

        Console.WriteLine("Registering Widget Provider");
        uint cookie;

        StrategyBasedComWrappers cw = new();
        nint factory = cw.GetOrCreateComInterfaceForObject(new WidgetFeedProviderFactory<WidgetFeedProvider>(), CreateComInterfaceFlags.None);

        var result = CoRegisterClassObject(CLSID_Factory, factory, 0x4, 0x1, out cookie);
        Marshal.ThrowExceptionForHR(result);
        /*
        FeedHost host;
        using ComPtr<IUnknown> comPtr = new();
        unsafe
        {
            var createResult = CoCreateInstance(ref CLSID_FeedHost, 0, 0x4, ComPtr<IUnknown>.__uuidof<IUnknown>(), (void**)comPtr.GetAddressOf());
            Marshal.ThrowExceptionForHR(createResult);
            Guid inspectable = new("AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90");
            ComPtr<IUnknown> other = null;
            comPtr.AsIID(&inspectable, ref other);
            ulong a;
            Guid* b;
            ((delegate* unmanaged<IUnknown*, ulong*, Guid**, int>)other.Get()->lpVtbl[3])(other.Get(), &a, &b);
            ComPtr<IUnknown> yn = null;
            int aksdg = other.AsIID(b, ref yn);
            host = FeedHost.FromAbi((nint)yn.Get());
        }

        await host.CreateFeedCoordinatorAsync("com.example.customFeed");
        */

        if (GetConsoleWindow() != IntPtr.Zero)
        {
            Console.WriteLine("Registered successfully.");
            Console.ReadLine();
        }
    }
}