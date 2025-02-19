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
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal class RungeKutta4
    {
        private readonly Vector4 c = new Vector4(1f / 6, 1f / 3, 1f / 3, 1f / 6);

        private readonly double dt;
        private readonly double endTime;
        private readonly double[] initCond;

        private readonly SimMatrix stateEquations;

        public readonly double[,] soln;
        public readonly double[] time;

        public RungeKutta4(double endTime, double dt, SimMatrix eqns, double[] initCond)
        {
            this.endTime = endTime;
            this.dt = dt;
            stateEquations = eqns;
            this.initCond = initCond;
            soln = new double[initCond.Length, (int)Math.Ceiling(endTime / dt)];
            time = new double[(int)Math.Ceiling(endTime / dt)];
        }

        public void Solve()
        {
            double t = 0;
            double[] currentState = initCond;
            int j = 0;

            while (j < time.Length)
            {
                for (int i = 0; i < currentState.Length; i++)
                    soln[i, j] = currentState[i];
                time[j] = t;

                currentState = NextState(currentState);

                for (int k = 0; k < currentState.Length; k++)
                    if (double.IsNaN(currentState[k]) || double.IsInfinity(currentState[k]))
                    {
                        currentState[k] = 0;
                        j = time.Length;
                        t = endTime;
                    }

                j++;
                t += dt;
            }
        }

        public double[] GetSolution(int i)
        {
            if (i + 1 > soln.GetLength(0))
            {
                FARLogger.Info("Error; Index out of bounds");
                return new double[time.Length];
            }

            var solution = new double[time.Length];
            for (int j = 0; j < solution.Length; j++)
                solution[j] = soln[i, j];
            return solution;
        }

        private double[] NextState(double[] currentState)
        {
            var next = new double[currentState.Length];
            // dkavolis: f1-f4 where all pointing to the same array...
            var f1 = new double[currentState.Length];
            var f2 = new double[currentState.Length];
            var f3 = new double[currentState.Length];
            var f4 = new double[currentState.Length];

            for (int j = 0; j < next.Length; j++)
            {
                for (int i = 0; i < currentState.Length; i++)
                    f1[j] += currentState[i] * stateEquations.Value(i, j);
            }

            for (int j = 0; j < next.Length; j++)
            {
                for (int i = 0; i < currentState.Length; i++)
                    f2[j] += (currentState[i] + 0.5f * dt * f1[i]) * stateEquations.Value(i, j);
            }

            for (int j = 0; j < next.Length; j++)
            {
                for (int i = 0; i < currentState.Length; i++)
                    f3[j] += (currentState[i] + 0.5f * dt * f2[i]) * stateEquations.Value(i, j);
            }

            for (int j = 0; j < next.Length; j++)
            {
                for (int i = 0; i < currentState.Length; i++)
                    f4[j] += (currentState[i] + dt * f3[i]) * stateEquations.Value(i, j);
            }

            for (int i = 0; i < next.Length; i++)
                next[i] = currentState[i] + dt * (c[0] * f1[i] + c[1] * f2[i] + c[2] * f3[i] + c[3] * f4[i]);

            return next;
        }
    }
}
