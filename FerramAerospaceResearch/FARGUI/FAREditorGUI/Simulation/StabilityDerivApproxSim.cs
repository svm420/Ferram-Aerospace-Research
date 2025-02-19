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

using System;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal static class StabilityDerivLinearSim
    {
        public static GraphData RunTransientSimLateral(
            StabilityDerivOutput vehicleData,
            double endTime,
            double initDt,
            double[] InitCond
        )
        {
            var A = new SimMatrix(4, 4);

            A.PrintToConsole();

            int i = 0;
            int j = 0;
            int num = 0;
            var Derivs = new double[27];

            vehicleData.stabDerivs.CopyTo(Derivs, 0);

            Derivs[15] = Derivs[15] / vehicleData.nominalVelocity;
            Derivs[18] = Derivs[18] / vehicleData.nominalVelocity;
            Derivs[21] = Derivs[21] / vehicleData.nominalVelocity - 1;

            double Lb = Derivs[16] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));
            double Nb = Derivs[17] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));

            double Lp = Derivs[19] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));
            double Np = Derivs[20] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));

            double Lr = Derivs[22] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));
            double Nr = Derivs[23] / (1 - Derivs[26] * Derivs[26] / (Derivs[0] * Derivs[2]));

            Derivs[16] = Lb + Derivs[26] / Derivs[0] * Nb;
            Derivs[17] = Nb + Derivs[26] / Derivs[2] * Lb;

            Derivs[19] = Lp + Derivs[26] / Derivs[0] * Np;
            Derivs[20] = Np + Derivs[26] / Derivs[2] * Lp;

            Derivs[22] = Lr + Derivs[26] / Derivs[0] * Nr;
            Derivs[23] = Nr + Derivs[26] / Derivs[2] * Lr;

            foreach (double f in Derivs)
            {
                if (num < 15)
                {
                    num++; //Avoid Ix, Iy, Iz and long derivs
                    continue;
                }

                num++;
                FARLogger.Info("" + i + "," + j);
                if (i <= 2)
                    A.Add(f, i, j);

                if (j < 2)
                {
                    j++;
                }
                else
                {
                    j = 0;
                    i++;
                }
            }

            A.Add(InstantConditionSim.CalculateAccelerationDueToGravity(vehicleData.body, vehicleData.altitude) *
                  Math.Cos(vehicleData.stableAoA * Math.PI / 180) /
                  vehicleData.nominalVelocity,
                  3,
                  0);
            A.Add(1, 1, 3);


            A.PrintToConsole(); //We should have an array that looks like this:

            /*             i --------------->
             *       j  [ Yb / u0 , Yp / u0 , -(1 - Yr/ u0) ,  g Cos(θ0) / u0 ]
             *       |  [   Lb    ,    Lp   ,      Lr       ,          0          ]
             *       |  [   Nb    ,    Np   ,      Nr       ,          0          ]
             *      \ / [    0    ,    1    ,      0        ,          0          ]
             *       V                              //And one that looks like this:
             *
             *          [ Z e ]
             *          [ X e ]
             *          [ M e ]
             *          [  0  ]
             *
             *
             */
            var transSolve = new RungeKutta4(endTime, initDt, A, InitCond);
            transSolve.Solve();

            var lines = new GraphData {xValues = transSolve.time};

            double[] yVal = transSolve.GetSolution(0);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.LdColor, "β", true);

            yVal = transSolve.GetSolution(1);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.CmColor, "p", true);

            yVal = transSolve.GetSolution(2);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.CdColor, "r", true);

            yVal = transSolve.GetSolution(3);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.ClColor, "φ", true);

            return lines;
        }

        public static GraphData RunTransientSimLongitudinal(
            StabilityDerivOutput vehicleData,
            double endTime,
            double initDt,
            double[] InitCond
        )
        {
            var A = new SimMatrix(4, 4);
            var B = new SimMatrix(1, 4);

            A.PrintToConsole();

            int i = 0;
            int j = 0;
            int num = 0;

            foreach (double f in vehicleData.stabDerivs)
            {
                if (num < 3 || num >= 15)
                {
                    num++; //Avoid Ix, Iy, Iz
                    continue;
                }

                num++;
                FARLogger.Info(i + "," + j);
                if (i <= 2)
                    if (num == 10)
                        A.Add(f + vehicleData.nominalVelocity, i, j);
                    else
                        A.Add(f, i, j);
                else
                    B.Add(f, 0, j);
                if (j < 2)
                {
                    j++;
                }
                else
                {
                    j = 0;
                    i++;
                }
            }

            A.Add(-InstantConditionSim.CalculateAccelerationDueToGravity(vehicleData.body, vehicleData.altitude), 3, 1);
            A.Add(1, 2, 3);


            A.PrintToConsole(); //We should have an array that looks like this:

            /*             i --------------->
             *       j  [ Z w , Z u , Z q  + u,  0 ]
             *       |  [ X w , X u , X q     , -g ]
             *       |  [ M w , M u , M q     ,  0 ]
             *      \ / [  0  ,  0  ,  1      ,  0 ]
             *       V                              //And one that looks like this:
             *
             *          [ Z e ]
             *          [ X e ]
             *          [ M e ]
             *          [  0  ]
             *
             *
             */

            var transSolve = new RungeKutta4(endTime, initDt, A, InitCond);
            transSolve.Solve();

            var lines = new GraphData {xValues = transSolve.time};

            double[] yVal = transSolve.GetSolution(0);
            ScaleAndClampValues(yVal, 1, 50);
            lines.AddData(yVal, FARConfig.GUIColors.LdColor, "w", true);

            yVal = transSolve.GetSolution(1);
            ScaleAndClampValues(yVal, 1, 50);
            lines.AddData(yVal, FARConfig.GUIColors.CmColor, "u", true);

            yVal = transSolve.GetSolution(2);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.CdColor, "q", true);

            yVal = transSolve.GetSolution(3);
            ScaleAndClampValues(yVal, 180 / Math.PI, 50);
            lines.AddData(yVal, FARConfig.GUIColors.ClColor, "θ", true);

            return lines;
        }


        private static void ScaleAndClampValues(double[] yVal, double scalingFactor, double clampValue)
        {
            for (int k = 0; k < yVal.Length; k++)
            {
                yVal[k] = yVal[k] * scalingFactor;
                if (yVal[k] > clampValue)
                    yVal[k] = clampValue;
                else if (yVal[k] < -clampValue)
                    yVal[k] = -clampValue;
            }
        }
    }
}
