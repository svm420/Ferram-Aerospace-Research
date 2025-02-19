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
using System.Collections.Generic;
using System.Globalization;
using ferram4;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal class StabilityDerivCalculator
    {
        private readonly InstantConditionSim _instantCondition;

        public StabilityDerivCalculator(InstantConditionSim instantConditionSim)
        {
            _instantCondition = instantConditionSim;
        }

        public StabilityDerivOutput CalculateStabilityDerivs(
            CelestialBody body,
            double alt,
            double machNumber,
            int flapSetting,
            bool spoilers,
            double alpha,
            double beta,
            double phi
        )
        {
            GasProperties properties = FARAtmosphere.GetGasProperties(body,
                                                                      new Vector3d(0, 0, alt),
                                                                      Planetarium.GetUniversalTime());

            double density = properties.Density;
            double sspeed = properties.SpeedOfSound;
            double u0 = sspeed * machNumber;
            double q = u0 * u0 * density * 0.5f;

            var stabDerivOutput = new StabilityDerivOutput
            {
                nominalVelocity = u0,
                altitude = alt,
                body = body
            };

            Vector3d CoM = Vector3d.zero;
            double mass = 0;

            double MAC = 0;
            double b = 0;
            double area = 0;

            double Ix = 0;
            double Iy = 0;
            double Iz = 0;
            double Ixy = 0;
            double Iyz = 0;
            double Ixz = 0;

            var input = new InstantConditionSimInput(alpha, beta, phi, 0, 0, 0, machNumber, 0, flapSetting, spoilers);
            var pertOutput = new InstantConditionSimOutput();

            _instantCondition.GetClCdCmSteady(input, out InstantConditionSimOutput nominalOutput, true);

            List<Part> partsList = EditorLogic.SortedShipList;
            foreach (Part p in partsList)
            {
                if (FARAeroUtil.IsNonphysical(p))
                    continue;
                double partMass = p.mass;
                if (p.Resources.Count > 0)
                    partMass += p.GetResourceMass();

                // If you want to use GetModuleMass, you need to start from p.partInfo.mass, not p.mass
                CoM += partMass * (Vector3d)p.transform.TransformPoint(p.CoMOffset);
                mass += partMass;
                FARWingAerodynamicModel w = p.GetComponent<FARWingAerodynamicModel>();
                if (w == null)
                    continue;
                if (w.isShielded)
                    continue;

                area += w.S;
                MAC += w.GetMAC() * w.S;
                b += w.Getb_2() * w.S;
                if (w is FARControllableSurface controllableSurface)
                    controllableSurface.SetControlStateEditor(CoM,
                                                              p.transform.up,
                                                              0,
                                                              0,
                                                              0,
                                                              input.flaps,
                                                              input.spoilers);
            }

            if (area.NearlyEqual(0))
            {
                area = _instantCondition._maxCrossSectionFromBody;
                MAC = _instantCondition._bodyLength;
                b = 1;
            }

            MAC /= area;
            b /= area;
            CoM /= mass;
            mass *= 1000;

            stabDerivOutput.b = b;
            stabDerivOutput.MAC = MAC;
            stabDerivOutput.area = area;

            foreach (Part p in partsList)
            {
                if (p == null || FARAeroUtil.IsNonphysical(p))
                    continue;
                //This section handles the parallel axis theorem
                Vector3 relPos = p.transform.TransformPoint(p.CoMOffset) - CoM;
                double x2 = relPos.z * relPos.z;
                double y2 = relPos.x * relPos.x;
                double z2 = relPos.y * relPos.y;
                double x = relPos.z;
                double y = relPos.x;
                double z = relPos.y;

                double partMass = p.mass;
                if (p.Resources.Count > 0)
                    partMass += p.GetResourceMass();

                // If you want to use GetModuleMass, you need to start from p.partInfo.mass, not p.mass

                Ix += (y2 + z2) * partMass;
                Iy += (x2 + z2) * partMass;
                Iz += (x2 + y2) * partMass;

                Ixy += -x * y * partMass;
                Iyz += -z * y * partMass;
                Ixz += -x * z * partMass;

                //And this handles the part's own moment of inertia
                Vector3 principalInertia = p.Rigidbody.inertiaTensor;
                Quaternion prncInertRot = p.Rigidbody.inertiaTensorRotation;

                //The rows of the direction cosine matrix for a quaternion
                var Row1 =
                    new Vector3(prncInertRot.x * prncInertRot.x -
                                prncInertRot.y * prncInertRot.y -
                                prncInertRot.z * prncInertRot.z +
                                prncInertRot.w * prncInertRot.w,
                                2 * (prncInertRot.x * prncInertRot.y + prncInertRot.z * prncInertRot.w),
                                2 * (prncInertRot.x * prncInertRot.z - prncInertRot.y * prncInertRot.w));

                var Row2 = new Vector3(2 * (prncInertRot.x * prncInertRot.y - prncInertRot.z * prncInertRot.w),
                                       -prncInertRot.x * prncInertRot.x +
                                       prncInertRot.y * prncInertRot.y -
                                       prncInertRot.z * prncInertRot.z +
                                       prncInertRot.w * prncInertRot.w,
                                       2 * (prncInertRot.y * prncInertRot.z + prncInertRot.x * prncInertRot.w));

                var Row3 = new Vector3(2 * (prncInertRot.x * prncInertRot.z + prncInertRot.y * prncInertRot.w),
                                       2 * (prncInertRot.y * prncInertRot.z - prncInertRot.x * prncInertRot.w),
                                       -prncInertRot.x * prncInertRot.x -
                                       prncInertRot.y * prncInertRot.y +
                                       prncInertRot.z * prncInertRot.z +
                                       prncInertRot.w * prncInertRot.w);


                //And converting the principal moments of inertia into the coordinate system used by the system
                Ix += principalInertia.x * Row1.x * Row1.x +
                      principalInertia.y * Row1.y * Row1.y +
                      principalInertia.z * Row1.z * Row1.z;
                Iy += principalInertia.x * Row2.x * Row2.x +
                      principalInertia.y * Row2.y * Row2.y +
                      principalInertia.z * Row2.z * Row2.z;
                Iz += principalInertia.x * Row3.x * Row3.x +
                      principalInertia.y * Row3.y * Row3.y +
                      principalInertia.z * Row3.z * Row3.z;

                Ixy += principalInertia.x * Row1.x * Row2.x +
                       principalInertia.y * Row1.y * Row2.y +
                       principalInertia.z * Row1.z * Row2.z;
                Ixz += principalInertia.x * Row1.x * Row3.x +
                       principalInertia.y * Row1.y * Row3.y +
                       principalInertia.z * Row1.z * Row3.z;
                Iyz += principalInertia.x * Row2.x * Row3.x +
                       principalInertia.y * Row2.y * Row3.y +
                       principalInertia.z * Row2.z * Row3.z;
            }

            Ix *= 1000;
            Iy *= 1000;
            Iz *= 1000;

            stabDerivOutput.stabDerivs[0] = Ix;
            stabDerivOutput.stabDerivs[1] = Iy;
            stabDerivOutput.stabDerivs[2] = Iz;

            stabDerivOutput.stabDerivs[24] = Ixy;
            stabDerivOutput.stabDerivs[25] = Iyz;
            stabDerivOutput.stabDerivs[26] = Ixz;

            //This is the effect of gravity
            double effectiveG = InstantConditionSim.CalculateAccelerationDueToGravity(body, alt);
            //This is the effective reduction of gravity due to high velocity
            effectiveG -= u0 * u0 / (alt + body.Radius);
            double neededCl = mass * effectiveG / (q * area);


            _instantCondition.GetClCdCmSteady(input, out pertOutput, true, true);
            //Longitudinal Mess
            _instantCondition.SetState(machNumber, neededCl, CoM, 0, input.flaps, input.spoilers);
            FARMathUtil.OptimizationResult optResult =
                FARMathUtil.Secant(_instantCondition.FunctionIterateForAlpha,
                                   0,
                                   10,
                                   1e-4,
                                   1e-4,
                                   minLimit: -90,
                                   maxLimit: 90);
            int calls = optResult.FunctionCalls;

            // if stable AoA doesn't exist, calculate derivatives at 0 incidence
            if (!optResult.Converged)
            {
                FARLogger.Info("Stable angle of attack not found, calculating derivatives at 0 incidence instead");
                alpha = 0;
                _instantCondition.FunctionIterateForAlpha(alpha);
                calls += 1;
            }
            else
            {
                alpha = optResult.Result;
            }

            input.alpha = alpha;
            nominalOutput = _instantCondition.iterationOutput;

            input.alpha = alpha + 2;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true, true);

            stabDerivOutput.stableCl = neededCl;
            stabDerivOutput.stableCd = nominalOutput.Cd;
            stabDerivOutput.stableAoA = alpha;
            stabDerivOutput.stableAoAState = "";
            if (Math.Abs((nominalOutput.Cl - neededCl) / neededCl) > 0.1)
                stabDerivOutput.stableAoAState = nominalOutput.Cl > neededCl ? "<" : ">";

            FARLogger.Info("Cl needed: " +
                           neededCl.ToString(CultureInfo.InvariantCulture) +
                           ", AoA: " +
                           stabDerivOutput.stableAoA.ToString(CultureInfo.InvariantCulture) +
                           ", Cl: " +
                           nominalOutput.Cl.ToString(CultureInfo.InvariantCulture) +
                           ", Cd: " +
                           nominalOutput.Cd.ToString(CultureInfo.InvariantCulture) +
                           ", function calls: " +
                           calls.ToString());

            //vert vel derivs
            pertOutput.Cl = (pertOutput.Cl - nominalOutput.Cl) / (2 * FARMathUtil.deg2rad);
            pertOutput.Cd = (pertOutput.Cd - nominalOutput.Cd) / (2 * FARMathUtil.deg2rad);
            pertOutput.Cm = (pertOutput.Cm - nominalOutput.Cm) / (2 * FARMathUtil.deg2rad);

            pertOutput.Cl += nominalOutput.Cd;
            pertOutput.Cd -= nominalOutput.Cl;

            pertOutput.Cl *= -q * area / (mass * u0);
            pertOutput.Cd *= -q * area / (mass * u0);
            pertOutput.Cm *= q * area * MAC / (Iy * u0);

            stabDerivOutput.stabDerivs[3] = pertOutput.Cl; //Zw
            stabDerivOutput.stabDerivs[4] = pertOutput.Cd; //Xw
            stabDerivOutput.stabDerivs[5] = pertOutput.Cm; //Mw

            // Rodhern: The motivation for the revised stability derivatives sign interpretations of Zq, Xq, Ze and Xe
            //  is to align the sign conventions used for Zu, Zq, Ze, Xu, Xq and Xe. Further explanation can be found
            //  here: https://forum.kerbalspaceprogram.com/index.php?/topic/109098-official-far-craft-repository/&do=findComment&comment=2425057

            input.alpha = alpha;
            input.machNumber = machNumber + 0.05;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);

            //fwd vel derivs
            pertOutput.Cl = (pertOutput.Cl - nominalOutput.Cl) / 0.05 * machNumber;
            pertOutput.Cd = (pertOutput.Cd - nominalOutput.Cd) / 0.05 * machNumber;
            pertOutput.Cm = (pertOutput.Cm - nominalOutput.Cm) / 0.05 * machNumber;

            pertOutput.Cl += 2 * nominalOutput.Cl;
            pertOutput.Cd += 2 * nominalOutput.Cd;

            pertOutput.Cl *= -q * area / (mass * u0);
            pertOutput.Cd *= -q * area / (mass * u0);
            pertOutput.Cm *= q * area * MAC / (u0 * Iy);

            stabDerivOutput.stabDerivs[6] = pertOutput.Cl; //Zu
            stabDerivOutput.stabDerivs[7] = pertOutput.Cd; //Xu
            stabDerivOutput.stabDerivs[8] = pertOutput.Cm; //Mu

            input.machNumber = machNumber;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true, true);

            input.alphaDot = -0.05;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);

            //pitch rate derivs
            pertOutput.Cl = (pertOutput.Cl - nominalOutput.Cl) / 0.05;
            pertOutput.Cd = (pertOutput.Cd - nominalOutput.Cd) / 0.05;
            pertOutput.Cm = (pertOutput.Cm - nominalOutput.Cm) / 0.05;

            pertOutput.Cl *= -q * area * MAC / (2 * u0 * mass); // Rodhern: Replaced 'q' by '-q', so that formulas
            pertOutput.Cd *= -q * area * MAC / (2 * u0 * mass); //  for Zq and Xq match those for Zu and Xu.
            pertOutput.Cm *= q * area * MAC * MAC / (2 * u0 * Iy);

            stabDerivOutput.stabDerivs[9] = pertOutput.Cl;  //Zq
            stabDerivOutput.stabDerivs[10] = pertOutput.Cd; //Xq
            stabDerivOutput.stabDerivs[11] = pertOutput.Cm; //Mq

            input.alphaDot = 0;
            input.pitchValue = 0.1;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);

            //elevator derivs
            pertOutput.Cl = (pertOutput.Cl - nominalOutput.Cl) / 0.1;
            pertOutput.Cd = (pertOutput.Cd - nominalOutput.Cd) / 0.1;
            pertOutput.Cm = (pertOutput.Cm - nominalOutput.Cm) / 0.1;

            pertOutput.Cl *= -q * area / mass; // Rodhern: Replaced 'q' by '-q', so that formulas
            pertOutput.Cd *= -q * area / mass; //  for Ze and Xe match those for Zu and Xu.
            pertOutput.Cm *= q * area * MAC / Iy;

            stabDerivOutput.stabDerivs[12] = pertOutput.Cl; //Ze
            stabDerivOutput.stabDerivs[13] = pertOutput.Cd; //Xe
            stabDerivOutput.stabDerivs[14] = pertOutput.Cm; //Me

            //Lateral Mess

            input.pitchValue = 0;
            input.beta = beta + 2;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);
            //sideslip angle derivs
            pertOutput.Cy = (pertOutput.Cy - nominalOutput.Cy) / (2 * FARMathUtil.deg2rad);
            pertOutput.Cn = (pertOutput.Cn - nominalOutput.Cn) / (2 * FARMathUtil.deg2rad);
            pertOutput.C_roll = (pertOutput.C_roll - nominalOutput.C_roll) / (2 * FARMathUtil.deg2rad);

            pertOutput.Cy *= q * area / mass;
            pertOutput.Cn *= q * area * b / Iz;
            pertOutput.C_roll *= q * area * b / Ix;

            stabDerivOutput.stabDerivs[15] = pertOutput.Cy;     //Yb
            stabDerivOutput.stabDerivs[17] = pertOutput.Cn;     //Nb
            stabDerivOutput.stabDerivs[16] = pertOutput.C_roll; //Lb

            input.beta = beta;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true, true);

            input.phiDot = -0.05;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);

            //roll rate derivs
            pertOutput.Cy = (pertOutput.Cy - nominalOutput.Cy) / 0.05;
            pertOutput.Cn = (pertOutput.Cn - nominalOutput.Cn) / 0.05;
            pertOutput.C_roll = (pertOutput.C_roll - nominalOutput.C_roll) / 0.05;

            pertOutput.Cy *= q * area * b / (2 * mass * u0);
            pertOutput.Cn *= q * area * b * b / (2 * Iz * u0);
            pertOutput.C_roll *= q * area * b * b / (2 * Ix * u0);

            stabDerivOutput.stabDerivs[18] = pertOutput.Cy;     //Yp
            stabDerivOutput.stabDerivs[20] = pertOutput.Cn;     //Np
            stabDerivOutput.stabDerivs[19] = pertOutput.C_roll; //Lp


            input.phiDot = 0;

            _instantCondition.GetClCdCmSteady(input, out pertOutput, true, true);

            input.betaDot = -0.05;

            //yaw rate derivs
            _instantCondition.GetClCdCmSteady(input, out pertOutput, true);
            pertOutput.Cy = (pertOutput.Cy - nominalOutput.Cy) / 0.05f;
            pertOutput.Cn = (pertOutput.Cn - nominalOutput.Cn) / 0.05f;
            pertOutput.C_roll = (pertOutput.C_roll - nominalOutput.C_roll) / 0.05f;

            pertOutput.Cy *= q * area * b / (2 * mass * u0);
            pertOutput.Cn *= q * area * b * b / (2 * Iz * u0);
            pertOutput.C_roll *= q * area * b * b / (2 * Ix * u0);

            stabDerivOutput.stabDerivs[21] = pertOutput.Cy;     //Yr
            stabDerivOutput.stabDerivs[23] = pertOutput.Cn;     //Nr
            stabDerivOutput.stabDerivs[22] = pertOutput.C_roll; //Lr

            return stabDerivOutput;
        }
    }
}
