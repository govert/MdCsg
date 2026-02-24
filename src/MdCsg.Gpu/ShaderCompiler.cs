using System.Reflection;
using System.Text;
using Silk.NET.Shaderc;

namespace MdCsg.Gpu;

/// <summary>
/// Compiles embedded GLSL shader sources to SPIR-V bytecode at runtime using Silk.NET.Shaderc.
/// Caches compiled SPIR-V to avoid recompilation.
/// </summary>
public static unsafe class ShaderCompiler
{
    private static readonly Dictionary<string, byte[]> Cache = new();
    private static readonly object Lock = new();

    /// <summary>
    /// Compiles an embedded GLSL compute shader to SPIR-V.
    /// </summary>
    /// <param name="resourceName">Embedded resource name (e.g., "MdCsg.Gpu.Shaders.confident_point.comp").</param>
    /// <returns>SPIR-V bytecode.</returns>
    public static byte[] CompileCompute(string resourceName)
    {
        lock (Lock)
        {
            if (Cache.TryGetValue(resourceName, out var cached))
                return cached;

            var glslSource = LoadEmbeddedGlsl(resourceName);
            var spirv = CompileGlslToSpirv(glslSource, resourceName);
            Cache[resourceName] = spirv;
            return spirv;
        }
    }

    private static string LoadEmbeddedGlsl(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded GLSL resource not found: {resourceName}. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static byte[] CompileGlslToSpirv(string source, string filename)
    {
        using var shaderc = Shaderc.GetApi();

        var compiler = shaderc.CompilerInitialize();
        var options = shaderc.CompileOptionsInitialize();

        try
        {
            shaderc.CompileOptionsSetTargetEnv(options, TargetEnv.Vulkan, (uint)EnvVersion.Vulkan10);
            shaderc.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);

            var result = shaderc.CompileIntoSpv(
                compiler,
                source,
                (nuint)source.Length,
                ShaderKind.ComputeShader,
                filename,
                "main",
                options);

            try
            {
                var status = shaderc.ResultGetCompilationStatus(result);
                if (status != CompilationStatus.Success)
                {
                    var errorMsg = shaderc.ResultGetErrorMessageS(result);
                    throw new InvalidOperationException(
                        $"GLSL compilation failed for {filename}: {errorMsg}");
                }

                var length = (int)shaderc.ResultGetLength(result);
                var bytes = shaderc.ResultGetBytes(result);
                var spirv = new byte[length];
                new ReadOnlySpan<byte>(bytes, length).CopyTo(spirv);
                return spirv;
            }
            finally
            {
                shaderc.ResultRelease(result);
            }
        }
        finally
        {
            shaderc.CompileOptionsRelease(options);
            shaderc.CompilerRelease(compiler);
        }
    }
}
