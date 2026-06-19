using System.Runtime.InteropServices;

namespace Ferrum.Framework.Interop;

/// <summary>
/// Platform-specific constants and utilities for Ferrum P/Invoke interop.
/// </summary>
/// <remarks>
/// On iOS, all code is statically linked into the app binary under NativeAOT.
/// The special library name <c>__Internal</c> tells the runtime to look up
/// symbols in the current process rather than loading an external shared library.
///
/// On Android, the shared library is loaded by name (e.g. <c>libferrum_foo.so</c>
/// maps to the import name <c>"ferrum_foo"</c>).
///
/// Use <see cref="FerrumLibrary"/> as the first argument to
/// <c>[LibraryImport]</c> when binding a library that follows the Ferrum
/// naming convention (i.e. built by <c>ferrum_add_static_library</c>).
/// </remarks>
public static class InteropConstants
{
    /// <summary>
    /// The library name to use in <c>[LibraryImport]</c> for a native library
    /// on the current platform.
    /// </summary>
    /// <remarks>
    /// On iOS this is always <c>"__Internal"</c> (NativeAOT static linkage).
    /// On Android this resolves at runtime via
    /// <see cref="NativeLibrary.SetDllImportResolver"/>.
    /// On other platforms (macOS/Windows desktop preview) this is the library
    /// file name without extension.
    /// </remarks>
#if IOS || MACCATALYST
    public const string FerrumLibrary = "__Internal";
#else
    // Android and host fallback: use the bare library name.
    // The actual .so must be placed in jniLibs/<ABI>/ before building.
    public const string FerrumLibrary = "ferrum_test_stub";
#endif
}
