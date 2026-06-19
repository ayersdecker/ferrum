namespace Ferrum.Codegen.Models;

/// <summary>
/// A single parsed C function declaration (forward declaration or definition
/// with a prototype visible to the binding generator).
/// </summary>
public sealed record FunctionDecl
{
    public required string Name          { get; init; }
    public required CSharpType ReturnType { get; init; }
    public required IReadOnlyList<ParameterDecl> Parameters { get; init; }

    /// <summary>Source line number, for error reporting.</summary>
    public int SourceLine { get; init; }
}

/// <summary>A single parameter in a C function declaration.</summary>
public sealed record ParameterDecl
{
    public required string     Name  { get; init; }
    public required CSharpType Type  { get; init; }
}

/// <summary>
/// A parsed C struct declaration whose fields are all blittable.
/// </summary>
public sealed record StructDecl
{
    public required string Name                           { get; init; }
    public required IReadOnlyList<FieldDecl> Fields       { get; init; }

    /// <summary>Source line number, for error reporting.</summary>
    public int SourceLine { get; init; }
}

/// <summary>A single field inside a C struct.</summary>
public sealed record FieldDecl
{
    public required string     Name  { get; init; }
    public required CSharpType Type  { get; init; }
}

/// <summary>
/// The C# type string that maps to a given C type, together with whether it
/// requires an <c>unsafe</c> context (i.e. it is a pointer type).
/// </summary>
public sealed record CSharpType
{
    public required string TypeName    { get; init; }
    public          bool   IsPointer   { get; init; }
    public          bool   IsVoid      { get; init; }

    public override string ToString() => TypeName;
}

/// <summary>
/// All declarations successfully extracted from a single C header file.
/// </summary>
public sealed class ParsedHeader
{
    public string SourceFile                         { get; set; } = string.Empty;
    public List<FunctionDecl> Functions              { get; } = [];
    public List<StructDecl>   Structs                { get; } = [];
}
