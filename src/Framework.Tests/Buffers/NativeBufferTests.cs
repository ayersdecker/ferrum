using Xunit;
using Ferrum.Framework.Buffers;

namespace Ferrum.Framework.Tests.Buffers;

public sealed unsafe class NativeBufferTests
{
    [Fact]
    public void Constructor_Length_AllocatesCorrectSize()
    {
        using var buf = new NativeBuffer<int>(16);
        Assert.Equal(16, buf.Length);
    }

    [Fact]
    public void Constructor_Memory_WrapsExistingMemory()
    {
        int[] arr = [1, 2, 3, 4];
        using var buf = new NativeBuffer<int>(arr.AsMemory());
        Assert.Equal(4, buf.Length);
        Assert.Equal(1, buf.Span[0]);
        Assert.Equal(4, buf.Span[3]);
    }

    [Fact]
    public void Span_ReflectsMutations()
    {
        using var buf = new NativeBuffer<float>(4);
        buf.Span[0] = 1.0f;
        buf.Span[1] = 2.0f;
        buf.Span[2] = 3.0f;
        buf.Span[3] = 4.0f;

        Assert.Equal(1.0f, buf.Span[0]);
        Assert.Equal(4.0f, buf.Span[3]);
    }

    [Fact]
    public void Pointer_IsNonNull()
    {
        using var buf = new NativeBuffer<byte>(8);
        Assert.True(buf.Pointer != null);
    }

    [Fact]
    public void TypedPointer_MatchesPointer()
    {
        using var buf = new NativeBuffer<int>(4);
        Assert.Equal((nint)buf.Pointer, (nint)buf.TypedPointer);
    }

    [Fact]
    public void Pointer_ReflectsSpanValues()
    {
        using var buf = new NativeBuffer<int>(3);
        buf.Span[0] = 10;
        buf.Span[1] = 20;
        buf.Span[2] = 30;

        int* ptr = buf.TypedPointer;
        Assert.Equal(10, ptr[0]);
        Assert.Equal(20, ptr[1]);
        Assert.Equal(30, ptr[2]);
    }

    [Fact]
    public void Dispose_PreventsAccessToSpan()
    {
        var buf = new NativeBuffer<int>(4);
        buf.Dispose();
        Assert.Throws<ObjectDisposedException>(() => { _ = buf.Span; });
    }

    [Fact]
    public void Dispose_PreventsAccessToPointer()
    {
        var buf = new NativeBuffer<int>(4);
        buf.Dispose();
        Assert.Throws<ObjectDisposedException>(() => { _ = buf.Pointer; });
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var buf = new NativeBuffer<int>(4);
        buf.Dispose();
        buf.Dispose(); // Must not throw
    }

    [Fact]
    public void Constructor_ZeroLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBuffer<int>(0));
    }

    [Fact]
    public void Constructor_NegativeLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBuffer<int>(-1));
    }

    [Fact]
    public void ReadOnlySpan_MatchesSpan()
    {
        using var buf = new NativeBuffer<long>(2);
        buf.Span[0] = 100L;
        buf.Span[1] = 200L;
        Assert.Equal(100L, buf.ReadOnlySpan[0]);
        Assert.Equal(200L, buf.ReadOnlySpan[1]);
    }
}
