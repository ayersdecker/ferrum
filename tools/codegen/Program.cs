using Ferrum.Codegen;
using Ferrum.Codegen.Emitter;
using Ferrum.Codegen.Parser;

// ---------------------------------------------------------------------------
// ferrum-codegen
// Parses a C header file and emits [LibraryImport] P/Invoke bindings.
//
// Usage:
//   ferrum-codegen --input <header.h> --output <Bindings.cs> [options]
//
// Options:
//   --input   <path>    Path to the C header file to parse (required)
//   --output  <path>    Path for the generated .cs file (default: stdout)
//   --ns      <name>    C# namespace for generated types (default: Ferrum.Generated)
//   --class   <name>    Name of the partial bindings class (default: NativeBindings)
//   --lib     <name>    Library name for [LibraryImport] (default: __Internal)
//   --help              Show this help text
// ---------------------------------------------------------------------------

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    PrintHelp();
    return 0;
}

// ── Parse arguments ──────────────────────────────────────────────────────────
string? inputPath  = null;
string? outputPath = null;
var options = new EmitterOptions();

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--input":   inputPath            = NextArg(args, ref i, "--input");   break;
        case "--output":  outputPath           = NextArg(args, ref i, "--output");  break;
        case "--ns":      options.Namespace    = NextArg(args, ref i, "--ns");      break;
        case "--class":   options.ClassName    = NextArg(args, ref i, "--class");   break;
        case "--lib":     options.LibraryName  = NextArg(args, ref i, "--lib");     break;
        default:
            Console.Error.WriteLine($"ERROR: Unknown argument '{args[i]}'.");
            PrintHelp();
            return 2;
    }
}

if (inputPath is null)
{
    Console.Error.WriteLine("ERROR: --input is required.");
    PrintHelp();
    return 2;
}

// ── Read input ───────────────────────────────────────────────────────────────
string headerText;
try
{
    headerText = File.ReadAllText(inputPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: Cannot read '{inputPath}': {ex.Message}");
    return 1;
}

// ── Parse ────────────────────────────────────────────────────────────────────
var parser = new HeaderParser();
Ferrum.Codegen.Models.ParsedHeader parsed;
try
{
    parsed = parser.Parse(headerText, inputPath);
}
catch (CodegenException ex)
{
    // Fail loudly with a clear diagnostic — never emit a broken binding.
    Console.Error.WriteLine($"CODEGEN ERROR: {ex.Message}");
    return 1;
}

if (parsed.Functions.Count == 0 && parsed.Structs.Count == 0)
{
    Console.Error.WriteLine(
        $"WARNING: No bindable declarations found in '{inputPath}'. "
        + "Ensure the header contains extern function prototypes.");
}

// ── Emit ─────────────────────────────────────────────────────────────────────
var emitter = new BindingEmitter(options);
string output;
try
{
    output = emitter.Emit(parsed);
}
catch (CodegenException ex)
{
    Console.Error.WriteLine($"CODEGEN ERROR during emission: {ex.Message}");
    return 1;
}

// ── Write output ─────────────────────────────────────────────────────────────
if (outputPath is not null)
{
    try
    {
        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(outputPath, output);
        Console.WriteLine($"Generated: {outputPath}");
        Console.WriteLine($"  Functions : {parsed.Functions.Count}");
        Console.WriteLine($"  Structs   : {parsed.Structs.Count}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"ERROR: Cannot write '{outputPath}': {ex.Message}");
        return 1;
    }
}
else
{
    Console.Write(output);
}

return 0;

// ── Helpers ───────────────────────────────────────────────────────────────────
static string NextArg(string[] args, ref int i, string flag)
{
    if (++i >= args.Length)
    {
        Console.Error.WriteLine($"ERROR: {flag} requires a value.");
        Environment.Exit(2);
    }
    return args[i];
}

static void PrintHelp()
{
    Console.WriteLine(@"ferrum-codegen — C header to [LibraryImport] binding generator

Usage:
  ferrum-codegen --input <header.h> --output <Bindings.cs> [options]

Options:
  --input   <path>   Path to the C header file to parse (required)
  --output  <path>   Path for the generated .cs file (default: stdout)
  --ns      <name>   C# namespace for generated types (default: Ferrum.Generated)
  --class   <name>   Name of the partial bindings class (default: NativeBindings)
  --lib     <name>   Library name for [LibraryImport] (default: __Internal)
  --help             Show this help text

Exit codes:
  0  Success
  1  Parse or I/O error (binding NOT generated — check stderr)
  2  Bad arguments

Notes:
  The tool FAILS LOUDLY (exit code 1) on any construct it cannot safely bind:
  function pointers, char* parameters, platform-dependent 'long' types, etc.
  Fix the header or add a hand-written binding for those functions.
");
}
