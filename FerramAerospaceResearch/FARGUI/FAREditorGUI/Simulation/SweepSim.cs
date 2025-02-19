/*
Ferram Aerospace Research v0.16.1.2 "Marangoni"
=========================
Aerodynamics model for Kerbal Space Program

Copyright 2022, Michael Ferrara, aka Ferram4

   This file is part of Ferram Aerospace Research.

   Ferram Aerospace Research is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   Ferram Aerospace Research is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with Ferram Aerospace Research.  If not, see <http://www.gnu.org/licenses/>.

   Serious thanks:		a.g., for tons of bugfixes and code-refactorings
				stupid_chris, for the RealChuteLite implementation
            			Taverius, for correcting a ton of incorrect values
				Tetryds, for finding lots of bugs and issues and not letting me get away with them, and work on example crafts
            			sarbian, for refactoring code for working with MechJeb, and the Module Manager updates
            			ialdabaoth (who is awesome), who originally created Module Manager
                        	Regex, for adding RPM support
				DaMichel, for some ferramGraph updates and some control surface-related features
            			Duxwing, for copy editing the readme

   CompatibilityChecker by Majiir, BSD 2-clause http://opensource.org/licenses/BSD-2-Clause

   Part.cfg changes powered by sarbian & ialdabaoth's ModuleManager plugin; used with permission
	http://forum.kerbalspaceprogram.com/threads/55219

   ModularFLightIntegrator by Sarbian, Starwaster and Ferram4, MIT: http://opensource.org/licenses/MIT
	http://forum.kerbalspaceprogram.com/threads/118088

   Toolbar integration powered by blizzy78's Toolbar plugin; used with permission
	http://forum.kerbalspaceprogram.com/threads/60863
 */

using KSP.Localization;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal class SweepSim
    {
        private readonly InstantConditionSim _instantCondition;

        public SweepSim(InstantConditionSim instantConditionSim)
        {
            _instantCondition = instantConditionSim;
        }

        public bool IsReady()
        {
            return _instantCondition.Ready;
        }

        public GraphData MachNumberSweep(
            double aoAdegrees,
            double pitch,
            double lowerBound,
            double upperBound,
            int numPoints,
            int flapSetting,
            bool spoilers,
            CelestialBody body
        )
        {
            FARAeroUtil.UpdateCurrentActiveBody(body);

            FARAeroUtil.ResetEditorParts();

            var ClValues = new double[numPoints];
            var CdValues = new double[numPoints];
            var CmValues = new double[numPoints];
            var LDValues = new double[numPoints];
            var AlphaValues = new double[numPoints];

            var input = new InstantConditionSimInput(aoAdegrees, 0, 0, 0, 0, 0, 0, pitch, flapSetting, spoilers);

            for (int i = 0; i < numPoints; i++)
            {
                input.machNumber = i / (double)numPoints * (upperBound - lowerBound) + lowerBound;

                if (input.machNumber.NearlyEqual(0))
                    input.machNumber = 0.001;

                _instantCondition.GetClCdCmSteady(input, out InstantConditionSimOutput output, i == 0);
                AlphaValues[i] = input.machNumber;
                ClValues[i] = output.Cl;
                CdValues[i] = output.Cd;
                CmValues[i] = output.Cm;
                LDValues[i] = output.Cl * 0.1 / output.Cd;
            }

            var data = new GraphData {xValues = AlphaValues};
            data.AddData(ClValues, FARConfig.GUIColors.ClColor, Localizer.Format("FARAbbrevCl"), true);
            data.AddData(CdValues, FARConfig.GUIColors.CdColor, Localizer.Format("FARAbbrevCd"), true);
            data.AddData(CmValues, FARConfig.GUIColors.CmColor, Localizer.Format("FARAbbrevCm"), true);
            data.AddData(LDValues, FARConfig.GUIColors.LdColor, Localizer.Format("FARAbbrevL_D"), true);

            return data;
        }

        public GraphData AngleOfAttackSweep(
            double machNumber,
            double pitch,
            double lowerBound,
            double upperBound,
            int numPoints,
            int flapSetting,
            bool spoilers,
            CelestialBody body
        )
        {
            if (machNumber.NearlyEqual(0))
                machNumber = 0.001;

            var input = new InstantConditionSimInput(0, 0, 0, 0, 0, 0, machNumber, pitch, flapSetting, spoilers);

            FARAeroUtil.UpdateCurrentActiveBody(body);

            FARAeroUtil.ResetEditorParts();


            var ClValues = new double[numPoints];
            var CdValues = new double[numPoints];
            var CmValues = new double[numPoints];
            var LDValues = new double[numPoints];
            var AlphaValues = new double[numPoints];
            var ClValues2 = new double[numPoints];
            var CdValues2 = new double[numPoints];
            var CmValues2 = new double[numPoints];
            var LDValues2 = new double[numPoints];

            for (int i = 0; i < 2 * numPoints; i++)
            {
                double angle;
                if (i < numPoints)
                    angle = i / (double)numPoints * (upperBound - lowerBound) + lowerBound;
                else
                    angle = (i - (double)numPoints + 1) / numPoints * (lowerBound - upperBound) + upperBound;

                input.alpha = angle;

                _instantCondition.GetClCdCmSteady(input, out InstantConditionSimOutput output, i == 0);

                if (i < numPoints)
                {
                    AlphaValues[i] = angle;
                    ClValues[i] = output.Cl;
                    CdValues[i] = output.Cd;
                    CmValues[i] = output.Cm;
                    LDValues[i] = output.Cl * 0.1 / output.Cd;
                }
                else
                {
                    ClValues2[numPoints * 2 - 1 - i] = output.Cl;
                    CdValues2[numPoints * 2 - 1 - i] = output.Cd;
                    CmValues2[numPoints * 2 - 1 - i] = output.Cm;
                    LDValues2[numPoints * 2 - 1 - i] = output.Cl * 0.1 / output.Cd;
                }
            }

            var data = new GraphData {xValues = AlphaValues};
            data.AddData(ClValues2, FARConfig.GUIColors.ClColor * 0.5f, "Cl2", false);
            data.AddData(ClValues, FARConfig.GUIColors.ClColor, Localizer.Format("FARAbbrevCl"), true);

            data.AddData(CdValues2, FARConfig.GUIColors.CdColor * 0.5f, "Cd2", false);
            data.AddData(CdValues, FARConfig.GUIColors.CdColor, Localizer.Format("FARAbbrevCd"), true);

            data.AddData(CmValues2, FARConfig.GUIColors.CmColor * 0.5f, "Cm2", false);
            data.AddData(CmValues, FARConfig.GUIColors.CmColor, Localizer.Format("FARAbbrevCm"), true);

            data.AddData(LDValues2, FARConfig.GUIColors.LdColor * 0.5f, "L/D2", false);
            data.AddData(LDValues, FARConfig.GUIColors.LdColor, Localizer.Format("FARAbbrevL_D"), true);


            return data;
        }
    }
}
