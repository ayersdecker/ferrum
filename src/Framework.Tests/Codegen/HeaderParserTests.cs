using Xunit;
using Ferrum.Codegen;
using Ferrum.Codegen.Parser;

namespace Ferrum.Framework.Tests.Codegen;

public sealed class HeaderParserTests
{
    private readonly HeaderParser _parser = new();

    // ── Basic function parsing ──────────────────────────────────────────────

    [Fact]
    public void Parse_SimpleFunction_ExtractsFunctionName()
    {
        var header = _parser.Parse("int ferrum_add(int a, int b);");
        Assert.Single(header.Functions);
        Assert.Equal("ferrum_add", header.Functions[0].Name);
    }

    [Fact]
    public void Parse_SimpleFunction_MapsReturnType()
    {
        var header = _parser.Parse("int ferrum_add(int a, int b);");
        Assert.Equal("int", header.Functions[0].ReturnType.TypeName);
    }

    [Fact]
    public void Parse_SimpleFunction_MapsParameters()
    {
        var header = _parser.Parse("int ferrum_add(int a, int b);");
        var fn = header.Functions[0];
        Assert.Equal(2, fn.Parameters.Count);
        Assert.Equal("int", fn.Parameters[0].Type.TypeName);
        Assert.Equal("a",   fn.Parameters[0].Name);
        Assert.Equal("int", fn.Parameters[1].Type.TypeName);
        Assert.Equal("b",   fn.Parameters[1].Name);
    }

    [Fact]
    public void Parse_VoidReturn_MapsToVoid()
    {
        var header = _parser.Parse("void do_work(int n);");
        Assert.Equal("void", header.Functions[0].ReturnType.TypeName);
        Assert.True(header.Functions[0].ReturnType.IsVoid);
    }

    [Fact]
    public void Parse_VoidParams_YieldsEmptyParameterList()
    {
        var header = _parser.Parse("void reset(void);");
        Assert.Empty(header.Functions[0].Parameters);
    }

    [Fact]
    public void Parse_FloatPointerParam_MapsToPointer()
    {
        var header = _parser.Parse("void process(float* buf, int len);");
        var fn = header.Functions[0];
        Assert.Equal("float*", fn.Parameters[0].Type.TypeName);
        Assert.True(fn.Parameters[0].Type.IsPointer);
    }

    [Fact]
    public void Parse_ConstPointerParam_MapsToPointer()
    {
        var header = _parser.Parse("void read_buf(const float* src, int len);");
        var fn = header.Functions[0];
        Assert.True(fn.Parameters[0].Type.IsPointer);
    }

    [Fact]
    public void Parse_Uint32ReturnType_MapsCorrectly()
    {
        var header = _parser.Parse("uint32_t get_version(void);");
        Assert.Equal("uint", header.Functions[0].ReturnType.TypeName);
    }

    [Fact]
    public void Parse_MultipleDeclarations_ExtractsAll()
    {
        const string src = @"
int add(int a, int b);
float scale(float x, float factor);
void reset(void);
";
        var header = _parser.Parse(src);
        Assert.Equal(3, header.Functions.Count);
    }

    // ── Struct parsing ──────────────────────────────────────────────────────

    [Fact]
    public void Parse_SimpleStruct_ExtractsStruct()
    {
        var header = _parser.Parse("struct Point { int x; int y; };");
        Assert.Single(header.Structs);
        Assert.Equal("Point", header.Structs[0].Name);
    }

    [Fact]
    public void Parse_SimpleStruct_ExtractsFields()
    {
        var header = _parser.Parse("struct Vec2 { float x; float y; };");
        var s = header.Structs[0];
        Assert.Equal(2, s.Fields.Count);
        Assert.Equal("float", s.Fields[0].Type.TypeName);
        Assert.Equal("x",     s.Fields[0].Name);
    }

    [Fact]
    public void Parse_StructPointerParam_Resolves()
    {
        const string src = @"
struct Point { int x; int y; };
void translate(struct Point* p, int dx, int dy);
";
        var header = _parser.Parse(src);
        Assert.Single(header.Functions);
        Assert.Equal("Point*", header.Functions[0].Parameters[0].Type.TypeName);
    }

    // ── Error cases — the tool must fail loudly ────────────────────────────

    [Fact]
    public void Parse_CharPointerParam_ThrowsCodegenException()
    {
        Assert.Throws<CodegenException>(() =>
            _parser.Parse("void log_msg(const char* msg);"));
    }

    [Fact]
    public void Parse_FunctionPointerParam_ThrowsCodegenException()
    {
        Assert.Throws<CodegenException>(() =>
            _parser.Parse("void set_callback(int (*cb)(int, int));"));
    }

    [Fact]
    public void Parse_LongReturnType_ThrowsCodegenException()
    {
        Assert.Throws<CodegenException>(() =>
            _parser.Parse("long get_size(void);"));
    }

    [Fact]
    public void Parse_ArrayParam_ThrowsCodegenException()
    {
        Assert.Throws<CodegenException>(() =>
            _parser.Parse("void fill(int buf[16]);"));
    }

    [Fact]
    public void Parse_UnknownType_ThrowsCodegenException()
    {
        Assert.Throws<CodegenException>(() =>
            _parser.Parse("SomeUnknownType do_thing(void);"));
    }

    // ── Comments and preprocessor ──────────────────────────────────────────

    [Fact]
    public void Parse_LineComments_Ignored()
    {
        const string src = @"
// This is a function
int add(int a, int b); // returns sum
";
        var header = _parser.Parse(src);
        Assert.Single(header.Functions);
    }

    [Fact]
    public void Parse_BlockComments_Ignored()
    {
        const string src = @"
/* Block comment */
int add(int a, int b);
";
        var header = _parser.Parse(src);
        Assert.Single(header.Functions);
    }

    [Fact]
    public void Parse_IncludeGuards_Ignored()
    {
        const string src = @"
#ifndef MY_HEADER_H
#define MY_HEADER_H
int add(int a, int b);
#endif
";
        var header = _parser.Parse(src);
        Assert.Single(header.Functions);
    }

    [Fact]
    public void Parse_ExternCWrapper_Ignored()
    {
        const string src = @"
#ifdef __cplusplus
extern ""C"" {
#endif
int add(int a, int b);
#ifdef __cplusplus
}
#endif
";
        var header = _parser.Parse(src);
        Assert.Single(header.Functions);
    }
}
