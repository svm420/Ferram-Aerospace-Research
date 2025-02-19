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
using KSP.Localization;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FARFlightGUI
{
    internal class FlightStatusGUI
    {
        private string statusString;
        private Color statusColor;
        private double statusOverrideTimer;
        private double statusBlinkerTimer;
        private bool statusBlinker;
        private GUIStyle stallStyle;

        private VesselFlightInfo infoParameters;


        public void UpdateInfoParameters(VesselFlightInfo info)
        {
            infoParameters = info;
            SetFlightStatusWindow();
        }

        public void AerodynamicFailureStatus()
        {
            statusString = Localizer.Format("FARFlightStatus5");
            statusColor = Color.yellow;
            statusOverrideTimer = 5;
            statusBlinker = true;
        }

        private void SetFlightStatusWindow()
        {
            if (statusOverrideTimer > 0)
            {
                statusOverrideTimer -= TimeWarp.deltaTime;
                return;
            }

            if (infoParameters.dynPres < 0.01)
            {
                statusString = Localizer.Format("FARFlightStatus0");
                statusColor = Color.green;
                statusBlinker = false;
            }
            else if (infoParameters.stallFraction > 0.5)
            {
                statusString = Localizer.Format("FARFlightStatus1");
                statusColor = Color.yellow;
                statusBlinker = true;
            }
            else if (infoParameters.stallFraction > 0.005)
            {
                statusString = Localizer.Format("FARFlightStatus2");
                statusColor = Color.yellow;
                statusBlinker = false;
            }
            else if (Math.Abs(infoParameters.aoA) > 20 && Math.Abs(infoParameters.aoA) < 160 ||
                     Math.Abs(infoParameters.sideslipAngle) > 20 && Math.Abs(infoParameters.sideslipAngle) < 160)
            {
                statusString = Localizer.Format("FARFlightStatus3");
                statusColor = Color.yellow;
                statusBlinker = false;
            }
            else if (infoParameters.dynPres > 40)
            {
                statusString = Localizer.Format("FARFlightStatus4");
                statusColor = Color.yellow;
                statusBlinker = false;
            }
            else
            {
                statusString = Localizer.Format("FARFlightStatus0");
                statusColor = Color.green;
                statusBlinker = false;
            }
        }

        public void Display()
        {
            var minorTitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };

            if (stallStyle == null)
                stallStyle = new GUIStyle(FlightGUI.boxStyle);

            GUILayout.Label(Localizer.Format("FARFlightStatusLabel"), minorTitle, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            if (statusBlinker)
            {
                stallStyle.normal.textColor = stallStyle.focused.textColor =
                                                  stallStyle.hover.textColor =
                                                      stallStyle.active.textColor =
                                                          stallStyle.onActive.textColor =
                                                              stallStyle.onNormal.textColor =
                                                                  stallStyle.onFocused.textColor =
                                                                      stallStyle.onHover.textColor =
                                                                          stallStyle.onActive.textColor = statusColor;
                GUILayout.Box(statusBlinkerTimer < 0.5 ? statusString : "", stallStyle, GUILayout.ExpandWidth(true));

                if (statusBlinkerTimer < 1)
                    statusBlinkerTimer += TimeWarp.deltaTime;
                else
                    statusBlinkerTimer = 0;
            }
            else
            {
                stallStyle.normal.textColor = stallStyle.focused.textColor =
                                                  stallStyle.hover.textColor =
                                                      stallStyle.active.textColor =
                                                          stallStyle.onActive.textColor =
                                                              stallStyle.onNormal.textColor =
                                                                  stallStyle.onFocused.textColor =
                                                                      stallStyle.onHover.textColor =
                                                                          stallStyle.onActive.textColor = statusColor;
                GUILayout.Box(statusString, stallStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndHorizontal();
        }
    }
}
