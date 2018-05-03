using System;
using System.Collections.Generic;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    class StabilityDerivExportVariables
    {
        public double craftmass;

        public double envpressure; // in kPa
        public double envtemperature; // in Kelvin
        public double envdensity;
        public double envsoundspeed;
        public double envg;

        public double sitmach;
        public double sitdynpres;
        public double siteffg; // local gravity corrected for speed
    }

    class StabilityDerivExportOutput
    {
        public StabilityDerivOutput outputvals;
        public StabilityDerivExportVariables exportvals;

        public StabilityDerivExportOutput(StabilityDerivOutput outputvalues, StabilityDerivExportVariables exportvalues)
        {
            this.outputvals = outputvalues;
            this.exportvals = exportvalues;
        }
    }
}
