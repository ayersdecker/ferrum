# Changelog

All notable changes to Ferrum will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Production-ready documentation and developer experience improvements
- Comprehensive CONTRIBUTING.md with build, test, and PR guidelines
- Detailed README for MinimalDemo sample
- .editorconfig for consistent code formatting across contributors
- `dotnet new` template (`maui-ferrum`) for scaffolding new projects
- Troubleshooting section in main README
- Roadmap section in main README
- NuGet and CI badges in README

### Changed
- Enhanced README with three quick-start options (template, sample, existing app)
- Improved NuGet package metadata with README inclusion
- Reorganized Quick Start section for better discoverability

### Fixed
- NuGet packages now properly include README.md

## [0.1.0-alpha] - TBD

### Added
- Initial alpha release
- `Ferrum.Framework` NuGet package with `NativeBuffer<T>` for zero-copy interop
- `ferrum-codegen` dotnet tool for generating `[LibraryImport]` bindings from C headers
- NativeAOT-compatible P/Invoke using `[LibraryImport]` (source-generated)
- CMake build scripts for iOS (XCFramework) and Android (.so per ABI)
- MinimalDemo sample demonstrating end-to-end native interop
- Cross-platform support for .NET 9 MAUI on iOS and Android
- Blittable-only type system (loud failures on unsupported constructs)
- xUnit test suite for framework and codegen validation
- CI/CD workflows:
  - `codegen-tests.yml` - Runs xUnit tests for codegen tool
  - `native-build.yml` - Builds native libraries for iOS/Android
  - `maui-build.yml` - Builds MAUI sample project
- Comprehensive documentation:
  - Architecture overview
  - Getting started guide
  - Open questions for architectural decisions

### Design Constraints
- Framework-only (no domain-specific logic)
- NativeAOT-first (no runtime codegen or reflection-based marshalling)
- Loud failures (codegen exits non-zero rather than emitting incorrect bindings)
- Blittable types only (1:1 memory layout between C and C#)

### Known Limitations (v0.1.0-alpha)
- iOS and Android only (Windows/macOS desktop MAUI not supported)
- Simple regex-based codegen parser (no libclang integration)
- No official NuGet package publication yet (local build only)
- Template must be installed locally from repository

---

## Release Guidelines (for maintainers)

### Pre-release Checklist
- [ ] Update version in all `.csproj` files
- [ ] Update CHANGELOG.md with release date
- [ ] Run full test suite: `dotnet test`
- [ ] Build NuGet packages: `dotnet pack --configuration Release`
- [ ] Verify package contents
- [ ] Test template installation and usage
- [ ] Update README badges if needed
- [ ] Create and push Git tag: `git tag v0.1.0-alpha && git push origin v0.1.0-alpha`

### Publishing
```bash
# Publish to NuGet.org
dotnet nuget push artifacts/packages/Ferrum.Framework.*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push artifacts/packages/Ferrum.Codegen.*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json

# Optional: Publish template (when packaged)
dotnet nuget push artifacts/packages/Ferrum.Templates.*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

### Versioning Strategy
- **0.x.y**: Pre-1.0 releases, breaking changes allowed in minor versions
- **1.x.y**: Stable releases, breaking changes require major version bump
- **x.y.z-alpha/beta/rc**: Pre-release tags for testing

---

[Unreleased]: https://github.com/ayersdecker/ferrum/compare/v0.1.0-alpha...HEAD
[0.1.0-alpha]: https://github.com/ayersdecker/ferrum/releases/tag/v0.1.0-alpha
