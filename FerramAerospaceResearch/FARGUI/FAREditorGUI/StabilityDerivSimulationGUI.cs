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
using System.Linq;
using System.Text.RegularExpressions;
using ferram4;
using FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation;
using KSP.Localization;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI
{
    internal class StabilityDerivSimulationGUI : IDisposable
    {
        private static readonly string[] SimMode_str =
        {
            Localizer.Format("FAREditorSimModeLong"), Localizer.Format("FAREditorSimModeLat")
        };

        private readonly InitialConditions lonConditions;
        private readonly InitialConditions latConditions;
        private SimMode simMode = 0;

        private EditorSimManager simManager;
        private ferramGraph _graph = new ferramGraph(400, 200);

        public StabilityDerivSimulationGUI(EditorSimManager simManager)
        {
            this.simManager = simManager;

            lonConditions = new InitialConditions(new[] {"0", "0", "0", "0"},
                                                  new[] {"w", "u", "q", "θ"},
                                                  new[] {1, 1, Math.PI / 180, Math.PI / 180},
                                                  "0.01",
                                                  "10");
            latConditions = new InitialConditions(new[] {"0", "0", "0", "0"},
                                                  new[] {"β", "p", "r", "φ"},
                                                  new[] {Math.PI / 180, Math.PI / 180, Math.PI / 180, Math.PI / 180},
                                                  "0.01",
                                                  "10");

            _graph.SetBoundaries(0, 10, 0, 2);
            _graph.SetGridScaleUsingValues(1, 0.25);
            _graph.horizontalLabel = Localizer.Format("FAREditorSimGraphTime");
            _graph.verticalLabel = Localizer.Format("FAREditorSimGraphParams");
            _graph.Update();
        }

        public void Dispose()
        {
            simManager = null;
            _graph = null;
        }

        public void Display()
        {
            GUILayout.BeginHorizontal();
            simMode = (SimMode)GUILayout.SelectionGrid((int)simMode, SimMode_str, 2);

            GUILayout.EndHorizontal();
            StabilityDerivOutput vehicleData = simManager.vehicleData;


            if (simMode == SimMode.LONG)
            {
                LongitudinalGUI(vehicleData);
                DataInput(lonConditions, vehicleData, true);
            }
            else
            {
                LateralGUI(vehicleData);
                DataInput(latConditions, vehicleData, false);
            }

            var BackgroundStyle = new GUIStyle(GUI.skin.box);
            BackgroundStyle.hover = BackgroundStyle.active = BackgroundStyle.normal;

            _graph.Display(0, 0);

            DrawTooltip();
        }

        private static void LongitudinalGUI(StabilityDerivOutput vehicleData)
        {
            var BackgroundStyle = new GUIStyle(GUI.skin.box);
            BackgroundStyle.hover = BackgroundStyle.active = BackgroundStyle.normal;

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("FAREditorLongDeriv"), GUILayout.Width(160));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("FAREditorDownVelDeriv"), GUILayout.Width(160));
            GUILayout.Label(Localizer.Format("FAREditorFwdVelDeriv"), GUILayout.Width(160));
            GUILayout.Label(Localizer.Format("FAREditorPitchRateDeriv"), GUILayout.Width(160));
            GUILayout.Label(Localizer.Format("FAREditorPitchCtrlDeriv"), GUILayout.Width(160));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(BackgroundStyle);
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorZw"),
                           vehicleData.stabDerivs[3],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorZwExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorZu"),
                           vehicleData.stabDerivs[6],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorZuExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorZq"),
                           vehicleData.stabDerivs[9],
                           " " + Localizer.Format("FARUnitMPerSec"),
                           Localizer.Format("FAREditorZqExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorZDeltae"),
                           vehicleData.stabDerivs[12],
                           " " + Localizer.Format("FARUnitMPerSecSq"),
                           Localizer.Format("FAREditorZDeltaeExp"),
                           160,
                           0);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorXw"),
                           vehicleData.stabDerivs[4],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorXwExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorXu"),
                           vehicleData.stabDerivs[7],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorXuExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorXq"),
                           vehicleData.stabDerivs[10],
                           " " + Localizer.Format("FARUnitMPerSec"),
                           Localizer.Format("FAREditorXqExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorXDeltae"),
                           vehicleData.stabDerivs[13],
                           " " + Localizer.Format("FARUnitMPerSecSq"),
                           Localizer.Format("FAREditorXDeltaeExp"),
                           160,
                           0);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorMw"),
                           vehicleData.stabDerivs[5],
                           " " + Localizer.Format("FARUnitInvMSec"),
                           Localizer.Format("FAREditorMwExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorMu"),
                           vehicleData.stabDerivs[8],
                           " " + Localizer.Format("FARUnitInvMSec"),
                           Localizer.Format("FAREditorMuExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorMq"),
                           vehicleData.stabDerivs[11],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorMqExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorMDeltae"),
                           vehicleData.stabDerivs[14],
                           " " + Localizer.Format("FARUnitInvSecSq"),
                           Localizer.Format("FAREditorMDeltaeExp"),
                           160,
                           1);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private static void LateralGUI(StabilityDerivOutput vehicleData)
        {
            var BackgroundStyle = new GUIStyle(GUI.skin.box);
            BackgroundStyle.hover = BackgroundStyle.active = BackgroundStyle.normal;

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("FAREditorLatDeriv"), GUILayout.Width(160));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("FAREditorSideslipDeriv"), GUILayout.Width(160));
            GUILayout.Label(Localizer.Format("FAREditorRollRateDeriv"), GUILayout.Width(160));
            GUILayout.Label(Localizer.Format("FAREditorYawRateDeriv"), GUILayout.Width(160));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(BackgroundStyle);
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorYβ"),
                           vehicleData.stabDerivs[15],
                           " " + Localizer.Format("FARUnitMPerSecSq"),
                           Localizer.Format("FAREditorYβExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorYp"),
                           vehicleData.stabDerivs[18],
                           " " + Localizer.Format("FARUnitMPerSec"),
                           Localizer.Format("FAREditorYpExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorYr"),
                           vehicleData.stabDerivs[21],
                           " " + Localizer.Format("FARUnitMPerSec"),
                           Localizer.Format("FAREditorYrExp"),
                           160,
                           1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorLβ"),
                           vehicleData.stabDerivs[16],
                           " " + Localizer.Format("FARUnitInvSecSq"),
                           Localizer.Format("FAREditorLβExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorLp"),
                           vehicleData.stabDerivs[19],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorLpExp"),
                           160,
                           -1);
            StabilityLabel(Localizer.Format("FAREditorLr"),
                           vehicleData.stabDerivs[22],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorLrExp"),
                           160,
                           1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            StabilityLabel(Localizer.Format("FAREditorNβ"),
                           vehicleData.stabDerivs[17],
                           " " + Localizer.Format("FARUnitInvSecSq"),
                           Localizer.Format("FAREditorNβExp"),
                           160,
                           1);
            StabilityLabel(Localizer.Format("FAREditorNp"),
                           vehicleData.stabDerivs[20],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorNpExp"),
                           160,
                           0);
            StabilityLabel(Localizer.Format("FAREditorNr"),
                           vehicleData.stabDerivs[23],
                           " " + Localizer.Format("FARUnitInvSec"),
                           Localizer.Format("FAREditorNrExp"),
                           160,
                           -1);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void DataInput(InitialConditions inits, StabilityDerivOutput vehicleData, bool longitudinal)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < inits.inits.Length; i++)
            {
                GUILayout.Label(Localizer.Format("FAREditorSimInit") + inits.names[i] + ": ");
                inits.inits[i] = GUILayout.TextField(inits.inits[i], GUILayout.ExpandWidth(true));
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("FAREditorSimEndTime"));
            inits.maxTime = GUILayout.TextField(inits.maxTime, GUILayout.ExpandWidth(true));
            GUILayout.Label(Localizer.Format("FAREditorSimTimestep"));
            inits.dt = GUILayout.TextField(inits.dt, GUILayout.ExpandWidth(true));
            GUI.enabled = !EditorGUI.Instance.VoxelizationUpdateQueued;
            if (GUILayout.Button(Localizer.Format("FAREditorSimRunButton"),
                                 GUILayout.Width(150.0F),
                                 GUILayout.Height(25.0F)))
            {
                for (int i = 0; i < inits.inits.Length; i++)
                    inits.inits[i] = Regex.Replace(inits.inits[i], @"[^-?[0-9]*(\.[0-9]*)?]", "");
                inits.maxTime = Regex.Replace(inits.maxTime, @"[^-?[0-9]*(\.[0-9]*)?]", "");
                inits.dt = Regex.Replace(inits.dt, @"[^-?[0-9]*(\.[0-9]*)?]", "");

                var initCond = new double[inits.inits.Length];
                for (int i = 0; i < initCond.Length; i++)
                    initCond[i] = Convert.ToDouble(inits.inits[i]) * inits.scaling[i];


                GraphData data = longitudinal
                                     ? StabilityDerivLinearSim.RunTransientSimLongitudinal(vehicleData,
                                                                                           Convert
                                                                                               .ToDouble(inits.maxTime),
                                                                                           Convert.ToDouble(inits.dt),
                                                                                           initCond)
                                     : StabilityDerivLinearSim.RunTransientSimLateral(vehicleData,
                                                                                      Convert.ToDouble(inits.maxTime),
                                                                                      Convert.ToDouble(inits.dt),
                                                                                      initCond);

                UpdateGraph(data,
                            Localizer.Format("FAREditorSimGraphTime"),
                            Localizer.Format("FAREditorSimGraphParams"),
                            0,
                            Convert.ToDouble(inits.maxTime),
                            50);
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private static void StabilityLabel(string text1, double val, string text2, string tooltip, int width, int sign)
        {
            Color color = Color.white;
            if (sign != 0)
                color = !double.IsNaN(val) && Math.Sign(val) == sign ? Color.green : Color.red;

            var style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = style.hover.textColor = color;

            GUILayout.Label(new GUIContent(text1 + val.ToString("G6") + text2, tooltip), style, GUILayout.Width(width));
        }

        private static void DrawTooltip()
        {
            if (GUI.tooltip == "")
                return;

            var BackgroundStyle = new GUIStyle(GUI.skin.box);
            BackgroundStyle.hover = BackgroundStyle.active = BackgroundStyle.normal;

            Vector3 mousePos = GUIUtils.GetMousePos();
            Rect windowRect = EditorGUI.GUIRect;

            var tooltipRect = new Rect(Mathf.Clamp(mousePos.x - windowRect.x, 0, windowRect.width - 300),
                                       Mathf.Clamp(mousePos.y - windowRect.y, 0, windowRect.height - 80),
                                       300,
                                       80);

            GUIStyle toolTipStyle = BackgroundStyle;
            toolTipStyle.normal.textColor = toolTipStyle.active.textColor =
                                                toolTipStyle.hover.textColor =
                                                    toolTipStyle.focused.textColor =
                                                        toolTipStyle.onNormal.textColor =
                                                            toolTipStyle.onHover.textColor =
                                                                toolTipStyle.onActive.textColor =
                                                                    toolTipStyle.onFocused.textColor =
                                                                        new Color(1, 0.75f, 0);

            GUI.Box(tooltipRect, GUI.tooltip, toolTipStyle);
        }

        private void UpdateGraph(
            GraphData data,
            string horizontalLabel,
            string verticalLabel,
            double lowerXBound,
            double upperXBound,
            double clampYBounds
        )
        {
            double minBounds = double.PositiveInfinity;
            double maxBounds = double.NegativeInfinity;

            foreach (double[] yValues in data.yValues)
            {
                minBounds = Math.Min(minBounds, yValues.Min());
                maxBounds = Math.Max(maxBounds, yValues.Max());
            }

            minBounds *= 2;
            maxBounds *= 2;

            if (minBounds < -clampYBounds)
                minBounds = -clampYBounds;
            if (maxBounds > clampYBounds)
                maxBounds = clampYBounds;

            // To allow switching between two graph setups to observe differences,
            // use both the current and the previous shown graph to estimate scale

            double realMin = Math.Min(Math.Floor(minBounds), -0.25);
            double realMax = Math.Max(Math.Ceiling(maxBounds), 0.25);

            _graph.Clear();
            _graph.SetBoundaries(lowerXBound, upperXBound, realMin, realMax);
            _graph.SetGridScaleUsingValues(5, 0.5);

            for (int i = 0; i < data.yValues.Count; i++)
                _graph.AddLine(data.lineNames[i],
                               data.xValues,
                               data.yValues[i],
                               data.lineColors[i],
                               1,
                               data.lineNameVisible[i]);

            _graph.horizontalLabel = horizontalLabel;
            _graph.verticalLabel = verticalLabel;
            _graph.Update();
        }

        // ReSharper disable once UnusedMember.Local
        private enum SimMode
        {
            LONG,
            LAT
        }

        private class InitialConditions
        {
            public readonly string[] inits;
            public readonly string[] names;
            public readonly double[] scaling;

            public string dt;
            public string maxTime;

            public InitialConditions(string[] inits, string[] names, double[] scaling, string dt, string maxTime)
            {
                this.inits = inits;
                this.names = names;
                this.scaling = scaling;
                this.dt = dt;
                this.maxTime = maxTime;
            }
        }
    }
}
