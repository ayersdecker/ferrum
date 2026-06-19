# Open Questions

This document tracks architectural decisions that have **not** been resolved
yet. Each item must be discussed and decided before the related work is merged.

Add a ✅ and a resolution note when a decision is made.

---

## 1. Package / Distribution Model

**Question:** Should Ferrum be distributed as a NuGet package, a source
template (e.g. `dotnet new ferrum`), or both?

**Options:**

| Option | Pros | Cons |
|---|---|---|
| NuGet package (`Ferrum.Framework`) | Easy `dotnet add package` consumption; versioned; CI-friendly | Native build pipeline still needs manual steps; consumer must build and reference the native lib separately |
| Source template (`dotnet new ferrum`) | Complete skeleton delivered; consumer can customise everything | Harder to update; forks diverge |
| Both | Best consumer experience | More maintenance surface |

**Recommendation to discuss:** Start with a NuGet package for `Ferrum.Framework`
and `Ferrum.Codegen` (dotnet tool), and add a source template in a future
milestone once the API stabilises.

**Status:** ⬜ Unresolved

---

## 2. Windows / macOS Desktop MAUI Targets

**Question:** Should Ferrum support Windows (`net9.0-windows`) and macOS
(`net9.0-maccatalyst` / `net9.0-macos`) MAUI targets in v1?

**Considerations:**
- iOS and Android are the primary targets (mobile-first mandate).
- The CMake native build scripts currently support only iOS XCFramework and
  Android NDK toolchains. Adding Windows (MSVC / Clang-cl) and macOS
  (host toolchain) requires additional CMake configuration.
- The `InteropConstants.FerrumLibrary` constant would need a Windows/macOS
  branch.
- NativeAOT on macOS is supported by .NET 8+; Windows desktop NativeAOT is
  also available.

**Status:** ⬜ Unresolved — flag as **out of scope for v1** unless a
  contributor actively volunteers to own the CMake + CI work for that platform.

---

## 3. License

**Question:** MIT or Apache-2.0?

**Current state:** The repository already has an MIT license
(`LICENSE`, © 2026 Decker Ayers).

**Recommendation:** Keep MIT unless there is a specific reason to prefer
Apache-2.0 (e.g. if the repo will include patent-encumbered contributions
where Apache-2.0's explicit patent grant is desired).

**Status:** ✅ MIT (established in repo)

---

## 4. Project Name

**Question:** Is "Ferrum" the permanent name?

**Considerations:**
- "Ferrum" is Latin for iron — consistent with the "bare-metal" theme.
- NuGet package namespace: `Ferrum.Framework`, `Ferrum.Codegen`.
- CLI tool name: `ferrum-codegen`.
- No obvious naming conflicts on NuGet.org as of project inception.

**Status:** ✅ Ferrum (established in repo and README)

---

## 5. Codegen Tool — Full Clang AST vs. Simple Regex Parser

**Question:** Should the codegen tool use libclang / ClangSharp for accurate
C parsing, or stay with the current regex/tokenizer approach?

**Current approach:** Simple tokenizer sufficient for the P/Invoke subset.
Fails loudly on unsupported constructs.

**Arguments for libclang:**
- Handles preprocessor expansion, nested types, platform-specific macros.
- Fewer edge-case bugs in complex headers.

**Arguments against (for now):**
- Adds a native dependency (libclang) that complicates CI and the dotnet tool
  distribution.
- The current approach is sufficient for the stated use case (blittable types,
  simple function prototypes).

**Status:** ⬜ Unresolved — revisit when a real consumer reports a header the
  simple parser cannot handle.

---

## 6. Buffer Ownership — Consumer-Allocated vs. Framework-Allocated

**Question:** Should `NativeBuffer<T>` support a "framework-allocates" mode
(where the framework allocates native/unmanaged memory directly), in addition
to the current "pin managed memory" mode?

**Current design:** `NativeBuffer<T>` only pins managed memory (GC heap or
stack). Native allocation (e.g. `Marshal.AllocHGlobal`) is left to the consumer.

**Consideration:** Some native libraries require memory that is not on the GC
heap (e.g. device memory, DMA-able memory). A `NativeBuffer<T>.AllocUnmanaged()`
factory could address this, but it changes the ownership model.

**Status:** ⬜ Unresolved — defer until a concrete consumer requirement arises.
