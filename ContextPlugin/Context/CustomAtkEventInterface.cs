using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ContextPlugin.Context;

internal unsafe class CustomAtkEventInterface : IDisposable {
    public delegate int* ReceiveEventDelegate(void* self, int* a2, AtkValue* eventParams, nint a4, long param);
    
    private ReceiveEventDelegate? m_HandlerFuncRef;

    private nint[]? m_HandlerStore;
    private void* m_HandlerPtr;

    public nint FunctionAddress {
        get => m_HandlerStore?[1] ?? 0;
        set {
            if (m_HandlerStore == null)
                throw new ObjectDisposedException(nameof(CustomAtkEventInterface));
            m_HandlerStore[1] = value;
        }
    }

    public void* Pointer {
        get {
            if (m_HandlerStore == null)
                throw new ObjectDisposedException(nameof(CustomAtkEventInterface));
            return m_HandlerPtr;
        }
    }

    private CustomAtkEventInterface() {
        // "simulate" a class with the ReceiveEvent function in the vtable using a pinned array
        // index 1 is the function pointer, index 0 is a pointer to the function pointer
        m_HandlerStore = GC.AllocateArray<nint>(2, true);
        fixed (nint* vtbl = &m_HandlerStore[1])
            m_HandlerStore[0] = (nint)vtbl;
        fixed (nint* handler = &m_HandlerStore[0])
            m_HandlerPtr = handler;
    }

    public CustomAtkEventInterface(ReceiveEventDelegate handler) : this() {
        // need to keep this as a reference so the gc doesn't kill the pointer
        m_HandlerFuncRef = handler;
        m_HandlerStore![1] = Marshal.GetFunctionPointerForDelegate(m_HandlerFuncRef);
    }

    public CustomAtkEventInterface(delegate* unmanaged<void*, int*, AtkValue*, nint, long, int*> handler) : this() {
        m_HandlerFuncRef = null;
        m_HandlerStore![1] = (nint)handler;
    }

    public void Dispose() {
        m_HandlerPtr = null;
        m_HandlerFuncRef = null;
        m_HandlerStore = null;
    }
}
