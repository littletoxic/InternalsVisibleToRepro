# InternalsVisibleTo 编辑器诊断复现

两个类库通过 `ProjectReference` 引用。`Engine.Slang.Interop` 用 MSBuild 的
`InternalsVisibleTo` 项向 `Engine.Shaders` 开放内部结构体及其字段。
复现代码没有 Slang、Vulkan、原生库或第三方包依赖。

保留原仓库的 .NET 11 RC2、C# preview 配置，以及两个项目对
`updated-memory-safety-rules` 开关的差异，以便比较编辑器表现。

原问题环境：Windows x64，C# Dev Kit 11.5.47 预览版，C# 扩展 11.3.18。

2026-10-21 已验证命令行构建成功，0 个警告、0 个错误，且生成的
`Engine.Slang.Interop.AssemblyInfo.cs` 包含预期的 `InternalsVisibleTo("Engine.Shaders")`。
同日用户在独立 VS Code 窗口中打开此项目并提供截图，确认编辑器仍报告三处 CS0122：

- `SlangCompiler.cs:10`：创建 `SlangGlobalSessionDesc`。
- `SlangCompiler.cs:12`：将该内部类型用于 `Unsafe.SizeOf<T>()`。
- `SlangCompiler.cs:15`：读取 `MinLanguageVersion` 字段。

本机日志确认 C# Dev Kit 已加载两个项目，选用的 SDK 为
`11.0.100-rc.2.26504.105`。这确认了当前环境下的复现现象，尚不能据此确定根因，
也尚未对比禁用 Dev Kit、其他扩展版本或移除 `Features` 开关后的表现。

## 检查方式

在此目录执行：

```powershell
dotnet build InternalsVisibleToRepro.slnx
code --new-window .
```

等待解决方案加载完成，然后打开
`src/Engine.Shaders/SlangCompiler.cs`，观察 `SlangGlobalSessionDesc`
是否被错误标记为 CS0122。

预期：命令行构建成功，编辑器也允许访问内部类型及字段。
已观察到的异常：命令行构建成功，但编辑器仍报告该类型不可访问。

## 增量刷新场景

如果首次打开没有误报，可以按旧 issue 的步骤检查：

1. 在 `Engine.Slang.Interop.csproj` 中暂时删除
   `<InternalsVisibleTo Include="Engine.Shaders" />`，保存后等待诊断刷新。
   此时出现访问权限错误是正常行为。
2. 添加回同一行并保存，保持 VS Code 窗口打开。
3. 再次运行 `dotnet build InternalsVisibleToRepro.slnx`。
   构建应成功；观察编辑器中的 CS0122 是否仍然保留。
4. 若诊断保留，完全退出 VS Code 后重开，比较结果。

可进一步移除 `Directory.Build.props` 中的 `Features` 以及
`Engine.Slang.Interop.csproj` 中对应的 `PropertyGroup`，比较该差异是否影响复现。

相关报告：[C# Dev Kit #603](https://github.com/microsoft/vscode-dotnettools/issues/603)、
[C# #5970](https://github.com/dotnet/vscode-csharp/issues/5970)。
