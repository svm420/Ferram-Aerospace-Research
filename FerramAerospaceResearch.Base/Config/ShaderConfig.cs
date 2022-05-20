using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("Shaders")]
    public class ShaderConfig
    {
        [ConfigValue("bundleLinux")]
        public Observable<string> BundleLinux { get; } = new("FerramAerospaceResearch/Assets/farshaders_linux.far",
                                                             PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("bundleWindows")]
        public Observable<string> BundleWindows { get; } = new("FerramAerospaceResearch/Assets/farshaders_windows.far",
                                                               PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("bundleMac")]
        public Observable<string> BundleMac { get; } = new("FerramAerospaceResearch/Assets/farshaders_macosx.far",
                                                           PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("debugVoxel")]
        public DebugVoxelNode DebugVoxel { get; } = new("FerramAerospaceResearch/Debug Voxel Mesh");

        [ConfigValue("lineRenderer")] public ShaderNode LineRenderer { get; } = new("Hidden/Internal-Colored");

        [ConfigValue("debugVoxelFallback")] public ShaderNode DebugVoxelFallback { get; } = new("Sprites/Default");

        [ConfigValue("exposedSurface")]
        public ShaderNode ExposedSurface { get; } = new("FerramAerospaceResearch/Exposed Surface");

        [ConfigValue("countPixels")] public ComputeShaderNode CountColors { get; } = new("CountPixels");
    }
}
