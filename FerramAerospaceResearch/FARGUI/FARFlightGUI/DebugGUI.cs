using FerramAerospaceResearch.FARAeroComponents;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FARFlightGUI
{
    public class DebugGUI
    {
        private bool exposure = true;

        public void Display()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            Vessel vessel = FlightGlobals.ActiveVessel;
            string prefix = exposure ? "▼ " : "▶ ";
            exposure = GUILayout.Toggle(exposure,
                                        prefix + LocalizerExtensions.Get("FARDebugExposureLabel"),
                                        GUIDropDownStyles.ToggleButton);

            // ReSharper disable once InvertIf
            if (exposure)
            {
                if (vessel.FindVesselModuleImplementing<FARVesselAero>() is { } module)
                    module.Exposure.Display();
            }
            GUILayout.EndVertical();
        }
    }
}
