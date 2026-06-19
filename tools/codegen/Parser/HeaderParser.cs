using System.Text.RegularExpressions;
using Ferrum.Codegen.Models;

namespace Ferrum.Codegen.Parser;

/// <summary>
/// Parses a C header file and extracts function prototypes and struct
/// declarations that can be safely bound via <c>[LibraryImport]</c>.
/// </summary>
/// <remarks>
/// This is an intentionally simple tokeniser, not a full C parser. It handles
/// the subset of C headers that is relevant for P/Invoke generation:
/// <list type="bullet">
///   <item>Function prototypes: <c>return_type name(params);</c></item>
///   <item>Struct declarations with blittable fields</item>
///   <item>Single-level typedefs for primitive aliases</item>
/// </list>
/// Constructs that cannot be safely bound cause an immediate
/// <see cref="CodegenException"/> rather than being silently skipped.
/// </remarks>
public sealed class HeaderParser
{
    // ── Preprocessing patterns ────────────────────────────────────────────
    private static readonly Regex LineCommentRe    = new(@"//[^\n]*",           RegexOptions.Compiled);
    private static readonly Regex BlockCommentRe   = new(@"/\*.*?\*/",          RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex PreprocessorRe   = new(@"^\s*#[^\n]*",        RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex ExternCBlockRe   = new(@"extern\s+""C""\s*\{", RegexOptions.Compiled);
    private static readonly Regex WhitespaceNormRe = new(@"\s+",                RegexOptions.Compiled);

    // ── Function prototype: return_type name ( params ) ;
    // Handles optional storage-class / inline keywords and const qualifiers.
    private static readonly Regex FunctionRe = new(
        @"(?:extern\s+|static\s+|inline\s+)*"            +  // optional qualifiers
        @"(?<ret>[^(;]+?)"                                +  // return type
        @"\s+(?<name>[A-Za-z_]\w*)\s*"                   +  // function name
        @"\(\s*(?<params>[^)]*)\s*\)\s*;",                   // ( params ) ;
        RegexOptions.Compiled | RegexOptions.Singleline);

    // ── Struct: struct Name { fields } ;  or  typedef struct { fields } Name;
    private static readonly Regex StructBodyRe = new(
        @"(?:typedef\s+)?struct\s+(?<tag>[A-Za-z_]\w*)?\s*\{(?<body>[^}]*)\}\s*(?<alias>[A-Za-z_]\w*)?\s*;",
        RegexOptions.Compiled | RegexOptions.Singleline);

    // ── Individual struct field: type name ;
    private static readonly Regex FieldRe = new(
        @"(?<type>[^;]+?)\s+(?<name>[A-Za-z_]\w*)\s*;",
        RegexOptions.Compiled);

    // ── Parameter: type name  (or just type for unnamed params)
    private static readonly Regex ParamRe = new(
        @"^(?<type>.+?)\s+(?<name>[A-Za-z_]\w*)$",
        RegexOptions.Compiled);

    // ── Function-pointer detection (fail loudly)
    private static readonly Regex FuncPtrRe = new(
        @"\(\s*\*",
        RegexOptions.Compiled);

    // ── Array parameter detection (fail loudly — size unknown)
    private static readonly Regex ArrayParamRe = new(
        @"\[",
        RegexOptions.Compiled);

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Parses <paramref name="headerText"/> (the full text of a C header file)
    /// and returns a <see cref="ParsedHeader"/> containing all successfully
    /// extracted declarations.
    /// </summary>
    /// <param name="headerText">Contents of the .h file.</param>
    /// <param name="sourceFile">
    /// File name used in error messages (may be empty).
    /// </param>
    /// <exception cref="CodegenException">
    /// Thrown on the first construct that cannot be safely bound.
    /// </exception>
    public ParsedHeader Parse(string headerText, string sourceFile = "")
    {
        var parsed = new ParsedHeader { SourceFile = sourceFile };

        string cleaned = StripComments(headerText);
        cleaned = StripPreprocessorDirectives(cleaned);
        cleaned = StripExternCWrapper(cleaned);

        // Collect known struct names so pointer-to-struct resolves correctly
        var knownStructs = new HashSet<string>(StringComparer.Ordinal);
        ParseStructs(cleaned, parsed, knownStructs);
        ParseFunctions(cleaned, parsed, knownStructs);

        return parsed;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static string StripComments(string src)
    {
        string noBlock = BlockCommentRe.Replace(src, " ");
        return LineCommentRe.Replace(noBlock, "");
    }

    private static string StripPreprocessorDirectives(string src)
        => PreprocessorRe.Replace(src, "");

    private static string StripExternCWrapper(string src)
    {
        // Remove the opening  extern "C" {  brace (but keep the inner body)
        // and the matching closing brace.  Simple approach: remove the line
        // with extern "C" { and the final lone } in the file.
        if (!ExternCBlockRe.IsMatch(src)) return src;

        src = ExternCBlockRe.Replace(src, "");
        // Remove the last } that closes the extern "C" block
        int last = src.LastIndexOf('}');
        if (last >= 0) src = src[..last] + src[(last + 1)..];
        return src;
    }

    private void ParseStructs(string src, ParsedHeader parsed, HashSet<string> knownStructs)
    {
        foreach (Match m in StructBodyRe.Matches(src))
        {
            string tag   = m.Groups["tag"].Value.Trim();
            string alias = m.Groups["alias"].Value.Trim();
            string name  = string.IsNullOrEmpty(alias) ? tag : alias;

            if (string.IsNullOrEmpty(name))
                continue; // anonymous struct without typedef alias — skip

            int sourceLine = CountLines(src, m.Index);

            var fields = new List<FieldDecl>();
            string body = m.Groups["body"].Value;

            foreach (Match fm in FieldRe.Matches(body))
            {
                string fieldType = fm.Groups["type"].Value.Trim();
                string fieldName = fm.Groups["name"].Value.Trim();

                // Reject array members (e.g. int data[16])
                if (ArrayParamRe.IsMatch(fieldType))
                {
                    throw new CodegenException(
                        $"Line {sourceLine}: Struct '{name}' contains array field "
                        + $"'{fieldName}'. Fixed-size arrays in structs are not "
                        + "automatically supported — declare the struct manually.",
                        sourceLine);
                }

                // Reject function-pointer members
                if (FuncPtrRe.IsMatch(fieldType))
                {
                    throw new CodegenException(
                        $"Line {sourceLine}: Struct '{name}' contains a function "
                        + $"pointer field '{fieldName}'. Function pointers cannot "
                        + "be automatically bound.", sourceLine);
                }

                var csType = ResolveTypeWithStructs(fieldType, knownStructs, sourceLine);
                fields.Add(new FieldDecl { Name = fieldName, Type = csType });
            }

            knownStructs.Add(name);
            parsed.Structs.Add(new StructDecl
            {
                Name       = name,
                Fields     = fields,
                SourceLine = sourceLine,
            });
        }
    }

    private void ParseFunctions(string src, ParsedHeader parsed, HashSet<string> knownStructs)
    {
        // Pre-scan for function-pointer patterns that would confuse the main regex.
        // Function pointers defeat the [^)]* params group, so we catch them here
        // and fail loudly rather than silently skipping the declaration.
        foreach (Match fpMatch in FuncPtrRe.Matches(src))
        {
            int line = CountLines(src, fpMatch.Index);
            throw new CodegenException(
                $"Line {line}: Function pointer detected. Function pointers cannot be "
                + "automatically bound. Declare a managed delegate and use "
                + "[UnmanagedFunctionPointer] manually.", line);
        }

        foreach (Match m in FunctionRe.Matches(src))
        {
            string retTypeStr  = m.Groups["ret"].Value.Trim();
            string funcName    = m.Groups["name"].Value.Trim();
            string paramsStr   = m.Groups["params"].Value.Trim();
            int    sourceLine  = CountLines(src, m.Index);

            // Reject function-pointer return types
            if (FuncPtrRe.IsMatch(retTypeStr))
            {
                throw new CodegenException(
                    $"Line {sourceLine}: Function '{funcName}' returns a function "
                    + "pointer, which cannot be automatically bound.", sourceLine);
            }

            var retType = ResolveTypeWithStructs(retTypeStr, knownStructs, sourceLine);
            var parameters = ParseParameters(paramsStr, funcName, knownStructs, sourceLine);

            parsed.Functions.Add(new FunctionDecl
            {
                Name       = funcName,
                ReturnType = retType,
                Parameters = parameters,
                SourceLine = sourceLine,
            });
        }
    }

    private static List<ParameterDecl> ParseParameters(
        string paramsStr, string funcName,
        HashSet<string> knownStructs, int sourceLine)
    {
        if (string.IsNullOrWhiteSpace(paramsStr) || paramsStr == "void")
            return [];

        var result = new List<ParameterDecl>();
        int idx = 0;
        string[] parts = paramsStr.Split(',');

        foreach (string part in parts)
        {
            string p = part.Trim();
            if (string.IsNullOrEmpty(p)) continue;

            // Reject function-pointer parameters
            if (FuncPtrRe.IsMatch(p))
            {
                throw new CodegenException(
                    $"Line {sourceLine}: Function '{funcName}' has a function-pointer "
                    + $"parameter '{p}'. Function pointers cannot be automatically bound.",
                    sourceLine);
            }

            // Reject array parameters
            if (ArrayParamRe.IsMatch(p))
            {
                throw new CodegenException(
                    $"Line {sourceLine}: Function '{funcName}' has an array parameter "
                    + $"'{p}'. Use a pointer (T*) instead of an array declarator.",
                    sourceLine);
            }

            // Split "type name" — the last word is the parameter name
            Match pm = ParamRe.Match(p);
            string typePart, namePart;
            if (pm.Success)
            {
                typePart = pm.Groups["type"].Value.Trim();
                namePart = pm.Groups["name"].Value.Trim();
            }
            else
            {
                // Unnamed parameter — synthesize a name
                typePart = p;
                namePart = $"param{idx}";
            }

            var csType = ResolveTypeWithStructs(typePart, knownStructs, sourceLine);
            result.Add(new ParameterDecl { Name = namePart, Type = csType });
            idx++;
        }

        return result;
    }

    private static Models.CSharpType ResolveTypeWithStructs(
        string cType, HashSet<string> knownStructs, int sourceLine)
    {
        string norm    = CTypeMapping.Normalise(cType);
        bool   isConst = norm.StartsWith("const ", StringComparison.Ordinal);
        string stripped = isConst ? norm["const ".Length..].TrimStart() : norm;
        bool   isPtr   = stripped.EndsWith('*');
        string base_   = isPtr ? stripped[..^1].TrimEnd() : stripped;

        // Strip the 'struct' keyword — C code may write 'struct Point*' but we
        // store the struct name as just 'Point' in knownStructs.
        if (base_.StartsWith("struct ", StringComparison.Ordinal))
            base_ = base_["struct ".Length..].TrimStart();

        // Pointer-to-struct or struct-by-value
        if (knownStructs.Contains(base_))
        {
            return new Models.CSharpType
            {
                TypeName  = isPtr ? base_ + "*" : base_,
                IsPointer = isPtr,
                IsVoid    = false,
            };
        }

        // Delegate to primitive type table
        return CTypeMapping.Resolve(cType, sourceLine);
    }

    private static int CountLines(string src, int index)
        => src[..index].Count(c => c == '\n') + 1;
}
