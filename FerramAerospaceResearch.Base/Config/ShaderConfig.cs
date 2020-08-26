using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("Shaders")]
    public class ShaderConfig
    {
        [ConfigValue("bundleLinux")]
        public Observable<string> BundleLinux { get; } =
            new Observable<string>("FerramAerospaceResearch/Assets/farshaders_linux.far",
                                   PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("bundleWindows")]
        public Observable<string> BundleWindows { get; } =
            new Observable<string>("FerramAerospaceResearch/Assets/farshaders_windows.far",
                                   PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("bundleMac")]
        public Observable<string> BundleMac { get; } =
            new Observable<string>("FerramAerospaceResearch/Assets/farshaders_macosx.far",
                                   PathUtil.CombineDelegate(PathUtil.ParentDir));

        [ConfigValue("debugVoxel")]
        public DebugVoxelNode DebugVoxel { get; } = new DebugVoxelNode("FerramAerospaceResearch/Debug Voxel Mesh");

        [ConfigValue("lineRenderer")]
        public ShaderNode LineRenderer { get; } = new ShaderNode("Hidden/Internal-Colored");

        [ConfigValue("debugVoxelFallback")]
        public ShaderNode DebugVoxelFallback { get; } = new ShaderNode("Sprites/Default");
    }
}
