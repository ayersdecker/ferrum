# Contributing to Ferrum

Thank you for your interest in contributing to Ferrum! We welcome contributions from the community to make native interop in .NET MAUI better for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Building the Project](#building-the-project)
- [Running Tests](#running-tests)
- [Code Style](#code-style)
- [Pull Request Process](#pull-request-process)

---

## Code of Conduct

This project follows the standard open-source code of conduct:
- Be respectful and inclusive
- Focus on constructive feedback
- Assume good intent
- Report unacceptable behavior to the project maintainers

---

## How Can I Contribute?

### Reporting Bugs

Before creating a bug report:
1. Check [existing issues](https://github.com/ayersdecker/ferrum/issues) to avoid duplicates
2. Collect information about the bug (error messages, steps to reproduce, environment)

When filing a bug report, include:
- **Title**: Short, descriptive summary
- **Environment**: OS, .NET version, MAUI workload version, Xcode/NDK version
- **Steps to reproduce**: Minimal code sample or link to a repo
- **Expected behavior**: What you expected to happen
- **Actual behavior**: What actually happened
- **Logs/stack traces**: Build logs, exception stack traces

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:
- Use a clear, descriptive title
- Provide a detailed description of the proposed feature
- Explain why this feature would be useful to Ferrum users
- Include code examples if applicable

### Your First Code Contribution

Unsure where to start? Look for issues labeled:
- `good first issue` - smaller, well-defined tasks
- `help wanted` - features or bugs that need attention
- `documentation` - improvements to docs or examples

---

## Development Setup

### Prerequisites

| Tool | Minimum Version | Notes |
|------|----------------|-------|
| .NET SDK | 9.0 | `dotnet --version` |
| .NET MAUI workload | 9.0 | `dotnet workload install maui` |
| CMake | 3.21 | For building native libraries |
| Xcode (macOS) | 15 | Required for iOS development |
| Android NDK | r25 (API 24+) | Set `ANDROID_NDK_HOME` environment variable |
| Git | Any recent version | For source control |

### Clone the Repository

```bash
git clone https://github.com/ayersdecker/ferrum.git
cd ferrum
```

### IDE Setup

**Visual Studio 2022 (Windows/Mac)**
- Install the .NET MAUI workload via Visual Studio Installer
- Open `Ferrum.sln`

**VS Code**
- Install C# Dev Kit extension
- Install .NET MAUI extension
- Open the `ferrum` folder

**JetBrains Rider**
- Open `Ferrum.sln`
- Ensure .NET MAUI plugin is installed

---

## Building the Project

### Framework and Codegen Tool

```bash
# Build the framework library (multi-targeted)
dotnet build src/Framework/Ferrum.Framework.csproj

# Build the codegen tool
dotnet build tools/codegen/Ferrum.Codegen.csproj

# Build everything (solution-level)
dotnet build Ferrum.sln
```

**Note:** If you don't have MAUI workloads installed or are on Linux, skip mobile targets:

```bash
dotnet build -p:FerrumSkipMobile=true
```

### Native Test Stub

The native test stub is a simple C library used by tests and samples.

**iOS (macOS + Xcode required):**
```bash
./native/scripts/build_ios.sh
# Output: artifacts/ios/libferrum_test_stub.xcframework
```

**Android (NDK required):**
```bash
export ANDROID_NDK_HOME=/path/to/ndk  # e.g., ~/Library/Android/sdk/ndk/25.2.9519653
./native/scripts/build_android.sh
# Output: artifacts/android/jniLibs/{arm64-v8a,armeabi-v7a,x86_64}/libferrum_test_stub.so
```

### MinimalDemo Sample

```bash
cd samples/MinimalDemo

# Build for Android
dotnet build -f net9.0-android

# Build for iOS (macOS only)
dotnet build -f net9.0-ios
```

Before building the sample, ensure the native test stub is built (see above).

---

## Running Tests

### Unit Tests (xUnit)

```bash
# Run all framework and codegen tests
dotnet test src/Framework.Tests/Ferrum.Framework.Tests.csproj

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Integration Tests

The MinimalDemo sample serves as an integration test. After building the native stub and sample:

```bash
# Deploy and run on Android emulator/device
dotnet build samples/MinimalDemo/MinimalDemo.csproj -f net9.0-android -t:Run

# Deploy and run on iOS simulator (macOS only)
dotnet build samples/MinimalDemo/MinimalDemo.csproj -f net9.0-ios -t:Run
```

---

## Code Style

### C# Guidelines

- **Language version**: C# latest
- **Nullable reference types**: Enabled
- **Indentation**: 4 spaces (no tabs)
- **Naming conventions**:
  - PascalCase for types, methods, properties
  - camelCase for local variables, parameters
  - `_camelCase` for private fields
- **Unsafe code**: Allowed where necessary for P/Invoke (`AllowUnsafeBlocks=true`)
- **Comments**: XML doc comments on public APIs; inline comments for complex logic only

### EditorConfig

An `.editorconfig` file will be added to enforce consistent formatting. Use your IDE's "Format Document" feature before committing.

### CMake/C++ Guidelines (Native Code)

- **Standard**: C++17
- **Public API**: Must be plain C (`extern "C"`)
- **Indentation**: 4 spaces
- **Naming**: `snake_case` for functions and variables to match C conventions

---

## Pull Request Process

### Before Submitting

1. **Sync with latest main**: Rebase your branch on the latest `main`
   ```bash
   git checkout main
   git pull origin main
   git checkout your-feature-branch
   git rebase main
   ```

2. **Run tests**: Ensure all tests pass
   ```bash
   dotnet test
   ```

3. **Build without warnings**: Check for build warnings
   ```bash
   dotnet build --no-incremental
   ```

4. **Format code**: Ensure code follows style guidelines
   ```bash
   dotnet format
   ```

### Submitting a PR

1. **Push your branch** to your fork
2. **Open a pull request** against the `main` branch
3. **Fill out the PR template** (description, related issue, testing done)
4. **Ensure CI passes**: GitHub Actions will run builds and tests
5. **Respond to feedback**: Address review comments promptly

### PR Checklist

- [ ] Code builds without errors or warnings
- [ ] All tests pass
- [ ] New code has appropriate test coverage
- [ ] Public APIs have XML documentation comments
- [ ] CHANGELOG.md updated (if user-facing change)
- [ ] No unrelated formatting changes (reduces noise in diff)

---

## Versioning and Releases

Ferrum follows [Semantic Versioning](https://semver.org/) (SemVer):
- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality in a backward-compatible manner
- **PATCH** version for backward-compatible bug fixes

Pre-release versions (e.g., `0.1.0-alpha`) indicate work in progress.

---

## Questions?

If you have questions about contributing:
- **GitHub Discussions**: [Start a discussion](https://github.com/ayersdecker/ferrum/discussions)
- **Issues**: For bugs or feature requests
- **README**: [Main documentation](README.md)
- **Docs**: [Architecture](docs/architecture.md) | [Getting Started](docs/getting-started.md)

---

Thank you for contributing to Ferrum! 🚀
