using Xunit;
using Ferrum.Codegen.Emitter;
using Ferrum.Codegen.Parser;

namespace Ferrum.Framework.Tests.Codegen;

/// <summary>
/// Exercises ferrum-codegen against the DSP fixture header, which contains two
/// real-consumer patterns not covered by the trivial <c>ferrum_add(int,int)</c>
/// test stub:
/// <list type="bullet">
///   <item>A function taking a <c>float*</c> buffer and an <c>int32_t</c> length.</item>
///   <item>A function with a blittable struct out-parameter.</item>
/// </list>
/// </summary>
public sealed class DspFixtureCodegenTests
{
    // ── Inline copy of native/dsp_fixture/include/ferrum_dsp.h ───────────────
    // Using an inline copy makes the tests self-contained and runnable without
    // requiring the native tree to be present in the test working directory.
    private const string DspHeader = @"
#ifndef FERRUM_DSP_H
#define FERRUM_DSP_H

#include <stdint.h>

#ifdef __cplusplus
extern ""C"" {
#endif

typedef struct {
    float   min_val;
    float   max_val;
    float   mean;
    int32_t count;
} FerrumDspStats;

void ferrum_dsp_scale(float* buf, int32_t len, float factor);
void ferrum_dsp_stats(const float* buf, int32_t len, FerrumDspStats* result);

#ifdef __cplusplus
}
#endif

#endif
";

    private readonly HeaderParser _parser = new();

    // ── Struct extraction ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_DspFixture_ExtractsOneStruct()
    {
        var header = _parser.Parse(DspHeader);
        Assert.Single(header.Structs);
        Assert.Equal("FerrumDspStats", header.Structs[0].Name);
    }

    [Fact]
    public void Parse_DspFixture_StructHasFourFields()
    {
        var header = _parser.Parse(DspHeader);
        Assert.Equal(4, header.Structs[0].Fields.Count);
    }

    [Fact]
    public void Parse_DspFixture_StructFloatFieldsMapped()
    {
        var header = _parser.Parse(DspHeader);
        var s = header.Structs[0];
        Assert.Equal("float", s.Fields[0].Type.TypeName); // min_val
        Assert.Equal("float", s.Fields[1].Type.TypeName); // max_val
        Assert.Equal("float", s.Fields[2].Type.TypeName); // mean
    }

    [Fact]
    public void Parse_DspFixture_StructInt32FieldMapped()
    {
        var header = _parser.Parse(DspHeader);
        var s = header.Structs[0];
        Assert.Equal("int", s.Fields[3].Type.TypeName); // count (int32_t → int)
        Assert.Equal("count", s.Fields[3].Name);
    }

    // ── Function extraction ───────────────────────────────────────────────────

    [Fact]
    public void Parse_DspFixture_ExtractsTwoFunctions()
    {
        var header = _parser.Parse(DspHeader);
        Assert.Equal(2, header.Functions.Count);
    }

    [Fact]
    public void Parse_DspFixture_ScaleFunction_FloatBufferParam()
    {
        var header = _parser.Parse(DspHeader);
        var fn = header.Functions.Single(f => f.Name == "ferrum_dsp_scale");
        Assert.Equal("float*", fn.Parameters[0].Type.TypeName);
        Assert.True(fn.Parameters[0].Type.IsPointer);
        Assert.Equal("buf", fn.Parameters[0].Name);
    }

    [Fact]
    public void Parse_DspFixture_ScaleFunction_LenParam()
    {
        var header = _parser.Parse(DspHeader);
        var fn = header.Functions.Single(f => f.Name == "ferrum_dsp_scale");
        Assert.Equal("int", fn.Parameters[1].Type.TypeName);
        Assert.Equal("len", fn.Parameters[1].Name);
    }

    [Fact]
    public void Parse_DspFixture_ScaleFunction_FactorParam()
    {
        var header = _parser.Parse(DspHeader);
        var fn = header.Functions.Single(f => f.Name == "ferrum_dsp_scale");
        Assert.Equal("float", fn.Parameters[2].Type.TypeName);
        Assert.Equal("factor", fn.Parameters[2].Name);
    }

    [Fact]
    public void Parse_DspFixture_StatsFunction_ConstFloatPointerParam()
    {
        var header = _parser.Parse(DspHeader);
        var fn = header.Functions.Single(f => f.Name == "ferrum_dsp_stats");
        // const float* maps to float* (const is stripped — no MarshalAs, blittable only)
        Assert.True(fn.Parameters[0].Type.IsPointer);
        Assert.Equal("buf", fn.Parameters[0].Name);
    }

    [Fact]
    public void Parse_DspFixture_StatsFunction_StructOutParam()
    {
        var header = _parser.Parse(DspHeader);
        var fn = header.Functions.Single(f => f.Name == "ferrum_dsp_stats");
        Assert.Equal("FerrumDspStats*", fn.Parameters[2].Type.TypeName);
        Assert.True(fn.Parameters[2].Type.IsPointer);
        Assert.Equal("result", fn.Parameters[2].Name);
    }

    [Fact]
    public void Parse_DspFixture_BothFunctionsReturnVoid()
    {
        var header = _parser.Parse(DspHeader);
        foreach (var fn in header.Functions)
        {
            Assert.Equal("void", fn.ReturnType.TypeName);
            Assert.True(fn.ReturnType.IsVoid);
        }
    }

    // ── Emitter round-trip ────────────────────────────────────────────────────

    [Fact]
    public void Emit_DspFixture_ContainsStructLayout()
    {
        var header = _parser.Parse(DspHeader);
        var emitter = new BindingEmitter(new EmitterOptions
        {
            Namespace   = "Ferrum.Tests.Generated",
            ClassName   = "DspBindings",
            LibraryName = "__Internal",
        });
        string output = emitter.Emit(header);

        Assert.Contains("[StructLayout(LayoutKind.Sequential)]", output);
        Assert.Contains("public unsafe struct FerrumDspStats", output);
    }

    [Fact]
    public void Emit_DspFixture_ContainsLibraryImportForBothFunctions()
    {
        var header = _parser.Parse(DspHeader);
        var emitter = new BindingEmitter(new EmitterOptions
        {
            Namespace   = "Ferrum.Tests.Generated",
            ClassName   = "DspBindings",
            LibraryName = "__Internal",
        });
        string output = emitter.Emit(header);

        Assert.Contains("ferrum_dsp_scale", output);
        Assert.Contains("ferrum_dsp_stats", output);
        Assert.Contains("[LibraryImport(LibraryName)]", output);
    }

    [Fact]
    public void Emit_DspFixture_UsesInternalModifier()
    {
        var header = _parser.Parse(DspHeader);
        var emitter = new BindingEmitter(new EmitterOptions
        {
            Namespace   = "Ferrum.Tests.Generated",
            ClassName   = "DspBindings",
            LibraryName = "__Internal",
        });
        string output = emitter.Emit(header);

        // Generated class must be internal (not public) to follow least-privilege convention
        Assert.Contains("internal static unsafe partial class DspBindings", output);
    }

    [Fact]
    public void Emit_DspFixture_OutputMatchesCommittedBindings()
    {
        // Regression guard: the pre-generated FerrumDspBindings.cs committed to
        // the repo must stay in sync with what the tool would actually produce.
        // If this test fails, re-run ferrum-codegen against the fixture header
        // and commit the updated output.
        var header = _parser.Parse(DspHeader, "ferrum_dsp.h");
        var emitter = new BindingEmitter(new EmitterOptions
        {
            Namespace   = "Ferrum.Tests.Generated",
            ClassName   = "DspBindings",
            LibraryName = "__Internal",
        });
        string generated = emitter.Emit(header);

        // Load the committed file
        string committedPath = FindCommittedBindingsFile();
        string committed = File.ReadAllText(committedPath);

        Assert.Equal(
            committed.ReplaceLineEndings("\n"),
            generated.ReplaceLineEndings("\n"),
            StringComparer.Ordinal);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Locates the committed <c>FerrumDspBindings.cs</c> by walking up from the
    /// test assembly's directory until the repo root is found.
    /// </summary>
    private static string FindCommittedBindingsFile()
    {
        string? dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            string candidate = Path.Combine(
                dir,
                "src", "Framework.Tests", "Codegen", "Generated", "FerrumDspBindings.cs");
            if (File.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        throw new FileNotFoundException(
            "Cannot locate FerrumDspBindings.cs. "
            + "Ensure the repo root is an ancestor of the test output directory.");
    }
}
