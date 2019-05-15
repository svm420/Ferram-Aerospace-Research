using UnityEngine;

namespace FerramAerospaceResearch
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class FARKSPAddonSpaceCenterScene : MonoBehaviour
    {
        private void Awake()
        {
            if (FARDebugAndSettings.FARDebugButtonStock)
                FARDebugAndSettings.ForceCloseDebugWindow();
        }
    }
}
