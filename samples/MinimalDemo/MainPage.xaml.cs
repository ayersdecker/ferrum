using Ferrum.Framework.Buffers;
using MinimalDemo.Interop;

namespace MinimalDemo;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCallNativeClicked(object sender, EventArgs e)
    {
        // This call crosses the managed/native boundary via [LibraryImport].
        // On iOS the symbol is resolved from the statically-linked XCFramework.
        // On Android it is loaded from libferrum_test_stub.so at runtime.
        int result = AddInterop.ferrum_add(21, 21);
        ResultLabel.Text = $"ferrum_add(21, 21) = {result}";
    }

    private unsafe void OnBufferDemoClicked(object sender, EventArgs e)
    {
        // Demonstrate zero-copy pinned buffer usage.
        // A real app would pass buf.Pointer to a native DSP or ML function.
        using var buf = new NativeBuffer<float>(256);

        // Write some values into the managed span
        for (int i = 0; i < buf.Length; i++)
            buf.Span[i] = i * 0.1f;

        // The pointer is valid and stable for the lifetime of 'buf'
        BufferLabel.Text =
            $"Pinned {buf.Length} floats at 0x{(nint)buf.Pointer:X}\n" +
            $"buf[0]={buf.Span[0]:F1}  buf[255]={buf.Span[255]:F1}";
    }
}
