# Suggested title

[BUG] False CS0122 for internal types exposed via InternalsVisibleTo, while dotnet build succeeds

# Suggested body

## Describe the issue

VS Code reports CS0122 when accessing an internal type and its fields from a friend assembly, even though `dotnet build` succeeds with 0 warnings and 0 errors.

Reproduction repository: https://github.com/littletoxic/InternalsVisibleToRepro

The repro contains only two class libraries:

- `Engine.Slang.Interop` defines an internal `SlangGlobalSessionDesc` struct and declares `<InternalsVisibleTo Include="Engine.Shaders" />`.
- `Engine.Shaders` references that project and accesses the internal struct and its fields.

The generated `Engine.Slang.Interop.AssemblyInfo.cs` contains:

```csharp
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Engine.Shaders")]
```

Despite this, the editor reports CS0122 in `src/Engine.Shaders/SlangCompiler.cs` at lines 10, 12, and 15, for the struct and its `MinLanguageVersion` field. See the attached screenshot.

## Collect logs

Attach the ZIP produced by `.NET: Collect C# Dev Kit Logs` in the affected VS Code window before submitting.

## Steps to reproduce

1. Clone `https://github.com/littletoxic/InternalsVisibleToRepro.git` and enter the `InternalsVisibleToRepro` directory.
2. With .NET SDK `11.0.100-rc.2.26504.105` installed, run `dotnet build InternalsVisibleToRepro.slnx` in that directory. The build succeeds.
3. Open that directory in VS Code with C# Dev Kit enabled and wait for the solution to load.
4. Open `src/Engine.Shaders/SlangCompiler.cs` and observe the CS0122 diagnostics.

The project already contains the `InternalsVisibleTo` item. Adding or removing it is not part of these steps.

## Expected behavior

The editor should permit friend-assembly access, consistent with the successful command-line build.

## Environment information

- OS: Windows x64, OS build `10.0.26300`
- VS Code: `1.142.0` (`6d62b1d5dace93a33b3a6d3eca1317bd5e15bcb1`)
- C# Dev Kit: `11.5.47` (pre-release)
- C# extension: `11.3.18`
- .NET SDK: `11.0.100-rc.2.26504.105`, pinned by `global.json`
- Target framework: `net11.0`
- C# language version: `preview`

## Additional context

The repro has no third-party package dependencies or native calls. It retains these feature settings from the original project:

- `Directory.Build.props` enables `updated-memory-safety-rules`.
- `Engine.Slang.Interop.csproj` removes that feature for this project only.

The current logs show both projects loaded by C# Dev Kit using the pinned SDK.

This resembles https://github.com/microsoft/vscode-dotnettools/issues/603, which was closed because it could no longer be reproduced. I am linking it as a related report; I have not established whether the root cause is the same or whether this is a regression.

Other extension versions, disabling C# Dev Kit, and removing the feature settings above have not been compared yet.
