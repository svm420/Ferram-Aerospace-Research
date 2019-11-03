using System.Text;

namespace FerramAerospaceResearch
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [ConfigParser("shaders")]
    public class FARShadersConfig : FARConfigParser<FARShadersConfig>
    {
        private string bundleLinux;
        private string bundleWindows;
        private string bundleMac;
        private string debugVoxel;
        private string lineRenderer;
        private string debugVoxelFallback;
        private float debugVoxelCutoff;

        public float DebugVoxelCutoff
        {
            get { return debugVoxelCutoff; }
        }

        public string BundleLinux
        {
            get { return bundleLinux; }
        }

        public string BundleWindows
        {
            get { return bundleWindows; }
        }

        public string BundleMac
        {
            get { return bundleMac; }
        }

        public string DebugVoxel
        {
            get { return debugVoxel; }
        }

        public string LineRenderer
        {
            get { return lineRenderer; }
        }

        public string DebugVoxelFallback
        {
            get { return debugVoxelFallback; }
        }

        public override void Reset()
        {
            base.Reset();
            bundleLinux = Defaults.BundleLinux.Value;
            bundleWindows = Defaults.BundleWindows.Value;
            bundleMac = Defaults.BundleMac.Value;
            debugVoxel = Defaults.DebugVoxel.Value;
            lineRenderer = Defaults.LineRenderer.Value;
            debugVoxelFallback = Defaults.DebugVoxelFallback.Value;
            debugVoxelCutoff = Defaults.DebugVoxelCutoff.Value;
        }

        public override void Parse(IConfigNode node)
        {
            node.TryGetValue(Defaults.BundleLinux.Name, ref bundleLinux);
            node.TryGetValue(Defaults.BundleWindows.Name, ref bundleWindows);
            node.TryGetValue(Defaults.BundleMac.Name, ref bundleMac);
            node.TryGetValue(Defaults.DebugVoxel.Name, ref debugVoxel);
            node.TryGetValue(Defaults.LineRenderer.Name, ref lineRenderer);
            node.TryGetValue(Defaults.DebugVoxelFallback.Name, ref debugVoxelFallback);
            node.TryGetValue(Defaults.DebugVoxelCutoff.Name, ref debugVoxelCutoff);
        }

        public override void SaveTo(IConfigNode node)
        {
            base.SaveTo(node);
            node.AddValue(Defaults.BundleLinux.EditableName, bundleLinux);
            node.AddValue(Defaults.BundleWindows.EditableName, bundleWindows);
            node.AddValue(Defaults.BundleMac.EditableName, bundleMac);
            node.AddValue(Defaults.DebugVoxel.EditableName, debugVoxel);
            node.AddValue(Defaults.LineRenderer.EditableName, lineRenderer);
            node.AddValue(Defaults.DebugVoxelFallback.EditableName, debugVoxelFallback);
            node.AddValue(Defaults.DebugVoxelCutoff.EditableName, debugVoxelCutoff);
        }

        public override void DebugString(StringBuilder sb)
        {
            base.DebugString(sb);
            AppendEntry(sb, Defaults.BundleLinux.Name, bundleLinux);
            AppendEntry(sb, Defaults.BundleWindows.Name, bundleWindows);
            AppendEntry(sb, Defaults.BundleMac.Name, bundleMac);
            AppendEntry(sb, Defaults.DebugVoxel.Name, debugVoxel);
            AppendEntry(sb, Defaults.LineRenderer.Name, lineRenderer);
            AppendEntry(sb, Defaults.DebugVoxelFallback.Name, debugVoxelFallback);
            AppendEntry(sb, Defaults.DebugVoxelCutoff.Name, debugVoxelCutoff);
        }

        public static class Defaults
        {
            public static readonly ConfigValue<string> BundleLinux =
                new ConfigValue<string>("bundleLinux", "FerramAerospaceResearch/Assets/farshaders_linux.far");

            public static readonly ConfigValue<string> BundleWindows =
                new ConfigValue<string>("bundleWindows", "FerramAerospaceResearch/Assets/farshaders_windows.far");

            public static readonly ConfigValue<string> BundleMac =
                new ConfigValue<string>("bundleMac", "FerramAerospaceResearch/Assets/farshaders_macosx.far");

            public static readonly ConfigValue<string> DebugVoxel =
                new ConfigValue<string>("debugVoxel", "FerramAerospaceResearch/Debug Voxel Mesh");

            public static readonly ConfigValue<string> LineRenderer =
                new ConfigValue<string>("lineRenderer", "Hidden/Internal-Colored");

            public static readonly ConfigValue<string> DebugVoxelFallback =
                new ConfigValue<string>("debugVoxelFallback", "Sprites/Default");

            public static readonly ConfigValue<float> DebugVoxelCutoff =
                new ConfigValue<float>("debugVoxelCutoff", 0.45f);
        }
    }
}
