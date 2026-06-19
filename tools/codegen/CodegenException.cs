namespace Ferrum.Codegen;

/// <summary>
/// Thrown when the codegen tool encounters a C construct that cannot be
/// safely bound via <c>[LibraryImport]</c>.
/// </summary>
/// <remarks>
/// The tool always fails loudly rather than silently emitting an incorrect
/// binding. The <see cref="SourceLine"/> property points to the problematic
/// line in the input header.
/// </remarks>
public sealed class CodegenException : Exception
{
    /// <summary>Line number in the source header (1-based, 0 if unknown).</summary>
    public int SourceLine { get; }

    public CodegenException(string message, int sourceLine = 0)
        : base(message)
    {
        SourceLine = sourceLine;
    }
}
