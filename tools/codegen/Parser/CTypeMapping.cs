namespace Ferrum.Codegen.Parser;

/// <summary>
/// Maps C primitive types and common stdint/stddef aliases to their C# blittable
/// equivalents.
/// </summary>
/// <remarks>
/// Only types that are <em>unambiguously blittable</em> across all supported
/// platforms (iOS arm64, Android arm64/x86_64) are accepted. Types whose
/// size is platform-dependent (e.g. <c>long</c>, <c>size_t</c> on 32-bit) are
/// mapped to architecture-neutral equivalents where possible or rejected.
///
/// If a type is not in this table the parser will throw a
/// <see cref="CodegenException"/> rather than emitting an incorrect binding.
/// </remarks>
public static class CTypeMapping
{
    // ------------------------------------------------------------------
    // Map: canonical C type token → (C# type name, isPointer, isVoid)
    // ------------------------------------------------------------------
    private static readonly Dictionary<string, (string csType, bool isPtr, bool isVoid)> Map =
        new(StringComparer.Ordinal)
    {
        // ── Void ────────────────────────────────────────────────────────
        ["void"]                = ("void",   false, true),

        // ── Boolean ─────────────────────────────────────────────────────
        // _Bool / bool: marshalled as byte (1 = true, 0 = false) to stay
        // blittable and AOT-safe.  Do not use [MarshalAs(UnmanagedType.Bool)]
        // — that uses reflection internally on some platforms.
        ["bool"]                = ("byte",   false, false),
        ["_Bool"]               = ("byte",   false, false),

        // ── Character / byte ─────────────────────────────────────────────
        // char is intentionally mapped to byte (not sbyte) since it is most
        // commonly used as a raw byte in native buffers.  char* is rejected
        // (non-blittable string semantics — use byte* + explicit encoding).
        ["char"]                = ("byte",   false, false),
        ["signed char"]         = ("sbyte",  false, false),
        ["unsigned char"]       = ("byte",   false, false),
        ["int8_t"]              = ("sbyte",  false, false),
        ["uint8_t"]             = ("byte",   false, false),

        // ── 16-bit ───────────────────────────────────────────────────────
        ["short"]               = ("short",  false, false),
        ["short int"]           = ("short",  false, false),
        ["signed short"]        = ("short",  false, false),
        ["signed short int"]    = ("short",  false, false),
        ["unsigned short"]      = ("ushort", false, false),
        ["unsigned short int"]  = ("ushort", false, false),
        ["int16_t"]             = ("short",  false, false),
        ["uint16_t"]            = ("ushort", false, false),

        // ── 32-bit ───────────────────────────────────────────────────────
        ["int"]                 = ("int",    false, false),
        ["signed int"]          = ("int",    false, false),
        ["signed"]              = ("int",    false, false),
        ["unsigned int"]        = ("uint",   false, false),
        ["unsigned"]            = ("uint",   false, false),
        ["int32_t"]             = ("int",    false, false),
        ["uint32_t"]            = ("uint",   false, false),

        // ── 64-bit ───────────────────────────────────────────────────────
        ["long long"]           = ("long",   false, false),
        ["long long int"]       = ("long",   false, false),
        ["signed long long"]    = ("long",   false, false),
        ["unsigned long long"]  = ("ulong",  false, false),
        ["int64_t"]             = ("long",   false, false),
        ["uint64_t"]            = ("ulong",  false, false),

        // ── Platform-width integers ──────────────────────────────────────
        // size_t / ptrdiff_t → nint/nuint (pointer-sized, safe on 64-bit)
        ["size_t"]              = ("nuint",  false, false),
        ["ptrdiff_t"]           = ("nint",   false, false),
        ["intptr_t"]            = ("nint",   false, false),
        ["uintptr_t"]           = ("nuint",  false, false),

        // ── Floating point ────────────────────────────────────────────────
        ["float"]               = ("float",  false, false),
        ["double"]              = ("double", false, false),

        // ── void* ─────────────────────────────────────────────────────────
        ["void*"]               = ("void*",  true,  false),
        ["const void*"]         = ("void*",  true,  false),
    };

    /// <summary>
    /// Returns the C# type for a C type token, stripping leading/trailing
    /// whitespace and normalising multi-keyword types (e.g. "unsigned int").
    /// </summary>
    /// <param name="cType">
    /// A normalised C type string, e.g. <c>"int"</c>, <c>"uint32_t"</c>,
    /// <c>"float*"</c>, <c>"const int*"</c>.
    /// </param>
    /// <param name="sourceLine">Line number used in error messages.</param>
    /// <exception cref="CodegenException">
    /// Thrown if the type is not mappable to a blittable C# type.
    /// </exception>
    public static Models.CSharpType Resolve(string cType, int sourceLine = 0)
    {
        string normalised = Normalise(cType);

        // ── Pointer types ────────────────────────────────────────────────
        bool isConst   = normalised.StartsWith("const ", StringComparison.Ordinal);
        string stripped = isConst ? normalised["const ".Length..].TrimStart() : normalised;

        bool isPointer = stripped.EndsWith('*');
        if (isPointer)
        {
            string pointee = stripped[..^1].TrimEnd();

            // char* / signed char* — reject (string semantics, non-blittable)
            if (pointee is "char" or "signed char")
            {
                throw new CodegenException(
                    $"Line {sourceLine}: 'char*' is not blittable. "
                    + "Use 'byte*' in C (change to unsigned char*) or handle "
                    + "string marshalling manually.", sourceLine);
            }

            // Function pointer — reject loudly
            if (pointee.Contains('('))
            {
                throw new CodegenException(
                    $"Line {sourceLine}: Function pointer type '{cType}' cannot be "
                    + "automatically bound. Declare a managed delegate and use "
                    + "[UnmanagedFunctionPointer] manually.", sourceLine);
            }

            // void* is already in the table; others become T* if T is blittable
            if (Map.TryGetValue($"{(isConst ? "const " : "")}{stripped}", out var entry) ||
                Map.TryGetValue(stripped, out entry))
            {
                return new Models.CSharpType
                {
                    TypeName  = entry.csType,
                    IsPointer = true,
                    IsVoid    = entry.isVoid,
                };
            }

            // Try resolving the pointee type, then make it a pointer
            if (Map.TryGetValue(pointee, out var pointeeEntry))
            {
                return new Models.CSharpType
                {
                    TypeName  = pointeeEntry.csType + "*",
                    IsPointer = true,
                    IsVoid    = false,
                };
            }

            throw new CodegenException(
                $"Line {sourceLine}: Cannot map pointer type '{cType}' to a blittable C# type. "
                + "Ensure the pointee is a primitive or a struct defined in the same header.",
                sourceLine);
        }

        // ── Non-pointer scalar ────────────────────────────────────────────
        // Strip leading 'const' for value types (const int → int)
        string lookup = isConst ? stripped : normalised;
        if (Map.TryGetValue(lookup, out var result))
        {
            return new Models.CSharpType
            {
                TypeName  = result.csType,
                IsPointer = result.isPtr,
                IsVoid    = result.isVoid,
            };
        }

        // long / unsigned long — platform-dependent, reject
        if (lookup is "long" or "long int" or "signed long" or "signed long int")
        {
            throw new CodegenException(
                $"Line {sourceLine}: 'long' is 32-bit on Windows but 64-bit on "
                + "Linux/iOS/Android. Use 'int32_t' or 'int64_t' instead.", sourceLine);
        }
        if (lookup is "unsigned long" or "unsigned long int")
        {
            throw new CodegenException(
                $"Line {sourceLine}: 'unsigned long' has platform-dependent size. "
                + "Use 'uint32_t' or 'uint64_t' instead.", sourceLine);
        }

        throw new CodegenException(
            $"Line {sourceLine}: Unknown or unsupported C type '{cType}'. "
            + "Only primitive types and their pointer variants are supported. "
            + "Structs defined in the same header are also supported.", sourceLine);
    }

    // -----------------------------------------------------------------------
    // Internal helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Collapses multiple spaces and removes redundant qualifiers so that
    /// "const   unsigned   int" becomes "unsigned int" (const is handled
    /// separately in Resolve).
    /// </summary>
    internal static string Normalise(string cType)
    {
        // Collapse internal whitespace
        var parts = cType.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }
}
