using System.Runtime.CompilerServices;
using Engine.Slang.Interop;

namespace Engine.Shaders;

public static class SlangCompiler
{
    public static uint GetMinimumLanguageVersion()
    {
        var desc = new SlangGlobalSessionDesc
        {
            StructureSize = (uint)Unsafe.SizeOf<SlangGlobalSessionDesc>(),
            MinLanguageVersion = 2025,
        };
        return desc.MinLanguageVersion;
    }
}
