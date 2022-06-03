using FerramAerospaceResearch.FARAeroComponents;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FARFlightGUI
{
    public class DebugGUI
    {
        private bool exposure = true;

        public void Display(ref Rect windowRect)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            Vessel vessel = FlightGlobals.ActiveVessel;
            string prefix = exposure ? "▼ " : "▶ ";
            bool previous = exposure;
            exposure = GUILayout.Toggle(exposure,
                                        prefix + LocalizerExtensions.Get("FARDebugExposureLabel"),
                                        GUIDropDownStyles.ToggleButton);

            // changed, recompute the rect
            if (previous != exposure)
                windowRect.size = Vector2.zero;

            // ReSharper disable once InvertIf
            if (exposure)
            {
                if (vessel.FindVesselModuleImplementing<FARVesselAero>() is { } module)
                    if (module.Exposure.Display())
                    {
                        windowRect.size = Vector2.zero;
                    }
            }

            GUILayout.EndVertical();
        }
    }
}
