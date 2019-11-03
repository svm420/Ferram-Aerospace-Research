namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [ConfigParser("shaders")]
    public class FARShadersConfig : FARConfigParser<FARShadersConfig>
    {
        public StringConfigValue BundleLinux { get; } =
            new StringConfigValue("bundleLinux", "FerramAerospaceResearch/Assets/farshaders_linux.far");

        public StringConfigValue BundleWindows { get; } =
            new StringConfigValue("bundleWindows", "FerramAerospaceResearch/Assets/farshaders_windows.far");

        public StringConfigValue BundleMac { get; } =
            new StringConfigValue("bundleMac", "FerramAerospaceResearch/Assets/farshaders_macosx.far");

        public StringConfigValue DebugVoxel { get; } =
            new StringConfigValue("debugVoxel", "FerramAerospaceResearch/Debug Voxel Mesh");

        public StringConfigValue LineRenderer { get; } =
            new StringConfigValue("lineRenderer", "Hidden/Internal-Colored");

        public StringConfigValue DebugVoxelFallback { get; } =
            new StringConfigValue("debugVoxelFallback", "Sprites/Default");

        public FloatConfigValue DebugVoxelCutoff { get; } = new FloatConfigValue("debugVoxelCutoff", 0.45f);
    }
}
