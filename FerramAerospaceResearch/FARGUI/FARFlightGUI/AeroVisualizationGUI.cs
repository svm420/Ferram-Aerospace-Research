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

using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FARFlightGUI
{
    public class AeroVisualizationGUI
    {
        public AeroVisualizationGUI()
        {
            LoadSettings();
        }

        public bool TintForCl { get; private set; }

        public bool TintForCd { get; private set; }

        public bool TintForStall { get; private set; }

        // Cl for full tinting (wings)
        public double FullySaturatedCl { get; private set; } = 0.5;

        // Cd for full tinting (wings)
        public double FullySaturatedCd { get; private set; } = 0.1;

        // Cl for full tinting (non-wing parts)
        public double FullySaturatedClBody { get; private set; } = 0.05;

        // Cd for full tinting (non-wing parts)
        public double FullySaturatedCdBody { get; private set; } = 0.01;

        // Stalled % for full tinting
        public double FullySaturatedStall { get; private set; } = 10.0;

        public bool AnyVisualizationActive
        {
            get { return TintForCl || TintForCd || TintForStall; }
        }

        public void SettingsDisplay()
        {
            GUILayout.Label(Localizer.Format("FARFlightAeroVizTitle"));

            GUILayout.BeginVertical(FlightGUI.boxStyle);
            TintForCl = GUILayout.Toggle(TintForCl, Localizer.Format("FARFlightAeroVizTintCl"));
            FullySaturatedCl =
                GUIUtils.TextEntryForDouble(Localizer.Format("FARFlightAeroVizTintClSatWing"), 125, FullySaturatedCl);
            FullySaturatedClBody =
                GUIUtils.TextEntryForDouble(Localizer.Format("FARFlightAeroVizTintClSatBody"),
                                            125,
                                            FullySaturatedClBody);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FlightGUI.boxStyle);
            TintForCd = GUILayout.Toggle(TintForCd, Localizer.Format("FARFlightAeroVizTintCd"));
            FullySaturatedCd =
                GUIUtils.TextEntryForDouble(Localizer.Format("FARFlightAeroVizTintCdSatWing"), 125, FullySaturatedCd);
            FullySaturatedCdBody =
                GUIUtils.TextEntryForDouble(Localizer.Format("FARFlightAeroVizTintCdSatBody"),
                                            125,
                                            FullySaturatedCdBody);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FlightGUI.boxStyle);
            TintForStall = GUILayout.Toggle(TintForStall, Localizer.Format("FARFlightAeroVizTintStall"));
            FullySaturatedStall =
                GUIUtils.TextEntryForDouble(Localizer.Format("FARFlightAeroVizTintStallSat"), 125, FullySaturatedStall);
            GUILayout.EndVertical();

            // Allowing toggling arrows here because why not...
            PhysicsGlobals.AeroForceDisplay =
                GUILayout.Toggle(PhysicsGlobals.AeroForceDisplay, Localizer.Format("FARFlightAeroVizToggleArrows"));
        }

        public void SaveSettings()
        {
            List<ConfigNode> flightGUISettings = FARSettingsScenarioModule.FlightGUISettings;
            if (flightGUISettings == null)
            {
                FARLogger.Error("Could not save Aero Visualization Settings because settings config list was null");
                return;
            }

            ConfigNode node = flightGUISettings.FirstOrDefault(t => t.name == "AeroVizSettings");

            if (node == null)
            {
                node = new ConfigNode("AeroVizSettings");
                flightGUISettings.Add(node);
            }

            node.ClearData();

            node.AddValue("fullySaturatedCl", FullySaturatedCl);
            node.AddValue("fullySaturatedCd", FullySaturatedCd);
            node.AddValue("fullySaturatedClBody", FullySaturatedClBody);
            node.AddValue("fullySaturatedCdBody", FullySaturatedCdBody);
            node.AddValue("fullySaturatedStall", FullySaturatedStall);
        }

        private void LoadSettings()
        {
            List<ConfigNode> flightGUISettings = FARSettingsScenarioModule.FlightGUISettings;

            ConfigNode node = flightGUISettings.FirstOrDefault(t => t.name == "AeroVizSettings");

            if (node == null)
                return;
            if (double.TryParse(node.GetValue("fullySaturatedCl"), out double tmp))
                FullySaturatedCl = tmp;
            if (double.TryParse(node.GetValue("fullySaturatedCd"), out tmp))
                FullySaturatedCd = tmp;
            if (double.TryParse(node.GetValue("fullySaturatedClBody"), out tmp))
                FullySaturatedClBody = tmp;
            if (double.TryParse(node.GetValue("fullySaturatedCdBody"), out tmp))
                FullySaturatedCdBody = tmp;
            if (double.TryParse(node.GetValue("fullySaturatedStall"), out tmp))
                FullySaturatedStall = tmp;
        }
    }
}
