using Ferrum.Framework.Buffers;

namespace FerrumApp;

public partial class MainPage : ContentPage
{
    private int _count;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        _count++;

        if (_count == 1)
            CounterBtn.Text = $"Clicked {_count} time";
        else
            CounterBtn.Text = $"Clicked {_count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private unsafe void OnNativeBufferClicked(object sender, EventArgs e)
    {
        // Example: Create a zero-copy buffer for native interop
        using var buffer = new NativeBuffer<float>(1024);

        // Fill the buffer with data
        for (int i = 0; i < buffer.Length; i++)
            buffer.Span[i] = i * 0.5f;

        // TODO: Call your native function here
        // Example: MyNativeBindings.process(buffer.TypedPointer, buffer.Length);

        // For now, just demonstrate we can access the buffer
        float sum = 0;
        for (int i = 0; i < Math.Min(100, buffer.Length); i++)
            sum += buffer.Span[i];

        BufferResultLabel.Text = $"Buffer created with {buffer.Length} elements. Sum of first 100: {sum:F2}";
        SemanticScreenReader.Announce(BufferResultLabel.Text);
    }
}
