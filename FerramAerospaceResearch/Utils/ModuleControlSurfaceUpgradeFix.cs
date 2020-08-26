using System.Reflection;
using UnityEngine;

namespace FerramAerospaceResearch
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ModuleControlSurfaceUpgradeFix : MonoBehaviour
    {
        private void Awake()
        {
            PropertyInfo pipelineProperty = typeof(KSPUpgradePipeline).GetProperty("Pipeline",
                                                                                   BindingFlags.Static |
                                                                                   BindingFlags.NonPublic |
                                                                                   BindingFlags.Public |
                                                                                   BindingFlags.IgnoreCase);

            if (!(pipelineProperty?.GetValue(null) is SaveUpgradePipeline.SaveUpgradePipeline pipeline))
            {
                FARLogger.Error("Could not find SaveUpgradePipeline");
                return;
            }

            FARLogger.Info("Removing ModuleControlSurface upgrade script from SaveUpgradePipeline");
            pipeline.upgradeScripts.RemoveAll(script => script.Name.Contains("ModuleControlSurface"));
        }
    }
}
