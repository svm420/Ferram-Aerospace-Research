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
using ferram4;
using FerramAerospaceResearch.FARAeroComponents;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal class InstantConditionSim
    {
        private readonly InstantConditionSimInput iterationInput = new InstantConditionSimInput();
        private List<FARAeroSection> _currentAeroSections;
        private List<FARAeroPartModule> _currentAeroModules;
        private List<FARWingAerodynamicModel> _wingAerodynamicModel;

        public double _maxCrossSectionFromBody;
        public double _bodyLength;

        private double neededCl;
        public InstantConditionSimOutput iterationOutput;

        public bool Ready
        {
            get { return _currentAeroSections != null && _currentAeroModules != null && _wingAerodynamicModel != null; }
        }

        public void UpdateAeroData(
            List<FARAeroPartModule> aeroModules,
            List<FARAeroSection> aeroSections,
            VehicleAerodynamics vehicleAero,
            List<FARWingAerodynamicModel> wingAerodynamicModel
        )
        {
            _currentAeroModules = aeroModules;
            _currentAeroSections = aeroSections;
            _wingAerodynamicModel = wingAerodynamicModel;
            _maxCrossSectionFromBody = vehicleAero.MaxCrossSectionArea;
            _bodyLength = vehicleAero.Length;
        }

        public static double CalculateAccelerationDueToGravity(CelestialBody body, double alt)
        {
            double radius = body.Radius + alt;
            double mu = body.gravParameter;

            double accel = radius * radius;
            accel = mu / accel;
            return accel;
        }

        public void GetClCdCmSteady(
            InstantConditionSimInput input,
            out InstantConditionSimOutput output,
            bool clear,
            bool reset_stall = false
        )
        {
            output = new InstantConditionSimOutput();

            double area = 0;
            double MAC = 0;
            double b_2 = 0;

            Vector3d forward = Vector3.forward;
            Vector3d up = Vector3.up;
            Vector3d right = Vector3.right;

            Vector3d CoM = Vector3d.zero;

            if (EditorDriver.editorFacility == EditorFacility.VAB)
            {
                forward = Vector3.up;
                up = -Vector3.forward;
            }

            double mass = 0;
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
            }

            CoM /= mass;

            // Rodhern: The original reference directions (velocity, liftVector, sideways) did not form an orthonormal
            //  basis. That in turn produced some counterintuitive calculation results, such as coupled yaw and pitch
            //  derivatives. A more thorough discussion of the topic can be found on the KSP forums:
            //  https://forum.kerbalspaceprogram.com/index.php?/topic/19321-131-ferram-aerospace-research-v01591-liepmann-4218/&do=findComment&comment=2781270
            //  The reference directions have been replaced by new ones that are orthonormal by construction.
            //  In dkavolis branch Vector3.Cross() and Vector3d.Normalize() are used explicitly. There is no apparent
            //  benefit to this other than possibly improved readability.

            double sinAlpha = Math.Sin(input.alpha * Math.PI / 180);
            double cosAlpha = Math.Sqrt(Math.Max(1 - sinAlpha * sinAlpha, 0));

            double sinBeta = Math.Sin(input.beta * Math.PI / 180);
            double cosBeta = Math.Sqrt(Math.Max(1 - sinBeta * sinBeta, 0));

            double sinPhi = Math.Sin(input.phi * Math.PI / 180);
            double cosPhi = Math.Sqrt(Math.Max(1 - sinPhi * sinPhi, 0));

            double alphaDot = input.alphaDot * Math.PI / 180;
            double betaDot = input.betaDot * Math.PI / 180;
            double phiDot = input.phiDot * Math.PI / 180;

            Vector3d velocity = forward * cosAlpha * cosBeta;
            velocity += right * (sinPhi * sinAlpha * cosBeta + cosPhi * sinBeta);
            velocity += -up * (cosPhi * sinAlpha * cosBeta - sinPhi * sinBeta);
            velocity.Normalize();

            Vector3d liftDown = -forward * sinAlpha;
            liftDown += right * sinPhi * cosAlpha;
            liftDown += -up * cosPhi * cosAlpha;
            liftDown.Normalize();

            Vector3d sideways = Vector3.Cross(velocity, liftDown);
            sideways.Normalize();

            Vector3d angVel = forward * (phiDot - sinAlpha * betaDot);
            angVel += right * (cosPhi * alphaDot + cosAlpha * sinPhi * betaDot);
            angVel += up * (sinPhi * alphaDot - cosAlpha * cosPhi * betaDot);


            foreach (FARWingAerodynamicModel w in _wingAerodynamicModel)
            {
                if (!(w && w.part))
                    continue;

                w.ComputeForceEditor(velocity, input.machNumber, 2);

                if (clear)
                    w.EditorClClear(reset_stall);

                Vector3d relPos = w.GetAerodynamicCenter() - CoM;

                Vector3d vel = velocity + Vector3d.Cross(angVel, relPos);

                if (w is FARControllableSurface controllableSurface)
                    controllableSurface.SetControlStateEditor(CoM,
                                                              vel,
                                                              (float)input.pitchValue,
                                                              0,
                                                              0,
                                                              input.flaps,
                                                              input.spoilers);
                else if (w.isShielded)
                    continue;

                Vector3d force = w.ComputeForceEditor(vel.normalized, input.machNumber, 2) * 1000;

                output.Cl += -Vector3d.Dot(force, liftDown);
                output.Cy += Vector3d.Dot(force, sideways);
                output.Cd += -Vector3d.Dot(force, velocity);

                Vector3d moment = -Vector3d.Cross(relPos, force);

                output.Cm += Vector3d.Dot(moment, sideways);
                output.Cn += Vector3d.Dot(moment, liftDown);
                output.C_roll += Vector3d.Dot(moment, velocity);

                area += w.S;
                MAC += w.GetMAC() * w.S;
                b_2 += w.Getb_2() * w.S;
            }

            var center = new FARCenterQuery();
            foreach (FARAeroSection aeroSection in _currentAeroSections)
                aeroSection.PredictionCalculateAeroForces(2,
                                                          (float)input.machNumber,
                                                          10000,
                                                          0,
                                                          0.005f,
                                                          velocity.normalized,
                                                          center);

            Vector3d centerForce = center.force * 1000;

            output.Cl += -Vector3d.Dot(centerForce, liftDown);
            output.Cy += Vector3d.Dot(centerForce, sideways);
            output.Cd += -Vector3d.Dot(centerForce, velocity);

            Vector3d centerMoment = -center.TorqueAt(CoM) * 1000;

            output.Cm += Vector3d.Dot(centerMoment, sideways);
            output.Cn += Vector3d.Dot(centerMoment, liftDown);
            output.C_roll += Vector3d.Dot(centerMoment, velocity);

            if (area.NearlyEqual(0))
            {
                area = _maxCrossSectionFromBody;
                b_2 = 1;
                MAC = _bodyLength;
            }

            double recipArea = 1 / area;

            MAC *= recipArea;
            b_2 *= recipArea;
            output.Cl *= recipArea;
            output.Cd *= recipArea;
            output.Cm *= recipArea / MAC;
            output.Cy *= recipArea;
            output.Cn *= recipArea / b_2;
            output.C_roll *= recipArea / b_2;
        }

        public void SetState(double machNumber, double Cl, Vector3d CoM, double pitch, int flapSetting, bool spoilers)
        {
            iterationInput.machNumber = machNumber;
            neededCl = Cl;
            iterationInput.pitchValue = pitch;
            iterationInput.flaps = flapSetting;
            iterationInput.spoilers = spoilers;

            iterationInput.alphaDot = 0;
            iterationInput.beta = 0;
            iterationInput.betaDot = 0;
            iterationInput.phi = 0;
            iterationInput.phiDot = 0;

            foreach (FARWingAerodynamicModel w in _wingAerodynamicModel)
            {
                if (w.isShielded)
                    continue;

                if (w is FARControllableSurface controllableSurface)
                    controllableSurface.SetControlStateEditor(CoM,
                                                              Vector3.up,
                                                              (float)pitch,
                                                              0,
                                                              0,
                                                              flapSetting,
                                                              spoilers);
            }
        }

        public double FunctionIterateForAlpha(double alpha)
        {
            iterationInput.alpha = alpha;
            GetClCdCmSteady(iterationInput, out iterationOutput, true, true);
            return iterationOutput.Cl - neededCl;
        }
    }
}
