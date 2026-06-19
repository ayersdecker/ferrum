using System.Buffers;
using System.Runtime.InteropServices;

namespace Ferrum.Framework.Buffers;

/// <summary>
/// A zero-copy buffer that pins a <see cref="Memory{T}"/> region and exposes a
/// raw pointer for use in native P/Invoke calls.
/// </summary>
/// <typeparam name="T">
/// The element type. Must be <c>unmanaged</c> (i.e., blittable — no managed
/// references). This constraint is enforced at compile time and is required for
/// NativeAOT compatibility.
/// </typeparam>
/// <remarks>
/// <para>
/// The buffer keeps the underlying memory pinned for its lifetime. Callers
/// <b>must</b> dispose the buffer after the native call returns to release the
/// GC pin. Do not let a <see cref="NativeBuffer{T}"/> outlive the
/// <see cref="Memory{T}"/> it was constructed from.
/// </para>
/// <para>
/// Typical usage:
/// <code>
///   float[] samples = new float[1024];
///   using var buf = new NativeBuffer&lt;float&gt;(samples);
///   NativeSignalProcessor.Process(buf.Pointer, buf.Length);
/// </code>
/// </para>
/// </remarks>
public sealed unsafe class NativeBuffer<T> : IDisposable where T : unmanaged
{
    private readonly Memory<T> _memory;
    private MemoryHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Allocates a new managed array of <paramref name="length"/> elements and
    /// pins it. The buffer owns the underlying array.
    /// </summary>
    public NativeBuffer(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        _memory = new T[length];
        _handle = _memory.Pin();
    }

    /// <summary>
    /// Pins an existing <see cref="Memory{T}"/> region. The caller retains
    /// ownership of the underlying memory; the buffer holds only a GC pin.
    /// </summary>
    public NativeBuffer(Memory<T> memory)
    {
        _memory = memory;
        _handle = _memory.Pin();
    }

    /// <summary>
    /// Returns a <see cref="Span{T}"/> over the buffer contents. Safe for
    /// managed reads and writes; the span is valid as long as this
    /// <see cref="NativeBuffer{T}"/> has not been disposed.
    /// </summary>
    public Span<T> Span
    {
        get
        {
            ThrowIfDisposed();
            return _memory.Span;
        }
    }

    /// <summary>
    /// Returns a read-only <see cref="ReadOnlySpan{T}"/> over the buffer.
    /// </summary>
    public ReadOnlySpan<T> ReadOnlySpan
    {
        get
        {
            ThrowIfDisposed();
            return _memory.Span;
        }
    }

    /// <summary>Number of elements in the buffer.</summary>
    public int Length => _memory.Length;

    /// <summary>
    /// Raw pointer to the first element, valid for the lifetime of this
    /// <see cref="NativeBuffer{T}"/>. Pass this to native functions via
    /// <c>[LibraryImport]</c> parameters typed as <c>T*</c> or <c>void*</c>.
    /// </summary>
    public void* Pointer
    {
        get
        {
            ThrowIfDisposed();
            return _handle.Pointer;
        }
    }

    /// <summary>
    /// Typed pointer to the first element. Equivalent to
    /// <c>(T*)Pointer</c>.
    /// </summary>
    public T* TypedPointer
    {
        get
        {
            ThrowIfDisposed();
            return (T*)_handle.Pointer;
        }
    }

    /// <summary>
    /// Releases the GC pin. After disposal the <see cref="Pointer"/> and
    /// <see cref="Span"/> properties must not be used.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
