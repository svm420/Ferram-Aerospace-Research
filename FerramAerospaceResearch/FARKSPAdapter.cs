using FerramAerospaceResearch.Resources;

namespace FerramAerospaceResearch
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class FARKSPAdapter : MonoSingleton<FARKSPAdapter>
    {
        protected override void OnAwake()
        {
            FARAddonLoader loader = FARAddonLoader.Instance;
            loader.Load(() => FARAssets.Instance.Loaders.Textures.Add(new GameDatabaseTextureLoader()));
        }

        public void ModuleManagerPostLoad()
        {
            StartCoroutine(FARAddonLoader.Instance.Reload());
        }
    }
}
