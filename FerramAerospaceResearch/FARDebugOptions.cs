﻿/*
Ferram Aerospace Research v0.13.2.1
Copyright 2014, Michael Ferrara, aka Ferram4

    This file is part of Ferram Aerospace Research.

    Ferram Aerospace Research is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kerbal Joint Reinforcement is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Ferram Aerospace Research.  If not, see <http://www.gnu.org/licenses/>.

    Serious thanks:		a.g., for tons of bugfixes and code-refactorings
            			Taverius, for correcting a ton of incorrect values
            			sarbian, for refactoring code for working with MechJeb, and the Module Manager 1.5 updates
            			ialdabaoth (who is awesome), who originally created Module Manager
            			Duxwing, for copy editing the readme
 * 
 * Kerbal Engineer Redux created by Cybutek, Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 *      Referenced for starting point for fixing the "editor click-through-GUI" bug
 *
 * Part.cfg changes powered by sarbian & ialdabaoth's ModuleManager plugin; used with permission
 *	http://forum.kerbalspaceprogram.com/threads/55219
 *
 * Toolbar integration powered by blizzy78's Toolbar plugin; used with permission
 *	http://forum.kerbalspaceprogram.com/threads/60863
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;
using Toolbar;

namespace ferram4
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class FARDebugOptions : MonoBehaviour
    {

        public static KSP.IO.PluginConfiguration config;
        private IButton FARDebugButton;
        private bool debugMenu = false;
        private Rect debugWinPos = new Rect(50, 50, 700, 250);

        private enum MenuTab
        {
            DebugAndData,
            PartClassification,
            AeroStress,
            AtmComposition
        }

        private string[] MenuTab_str = new string[]
        {
            "Debug Options",
            "Part Classification",
            "Aerodynamic Failure",
            "Atm Composition",
        };

        private MenuTab activeTab = MenuTab.DebugAndData;

        public void Awake()
        {
            LoadConfigs();
            FARDebugButton = ToolbarManager.Instance.add("ferram4", "FARDebugButton");
            FARDebugButton.TexturePath = "FerramAerospaceResearch/Textures/icon_button";
            FARDebugButton.ToolTip = "FAR Debug Options";
            FARDebugButton.OnClick += (e) => debugMenu = !debugMenu;
        }

        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (debugMenu)
                debugWinPos = GUILayout.Window("FARDebug".GetHashCode(), debugWinPos, debugWindow, "FAR Debug Options, v0.13.2.1", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }


        private void debugWindow(int windowID)
        {

            GUIStyle thisStyle = new GUIStyle(GUI.skin.toggle);
            thisStyle.stretchHeight = true;
            thisStyle.stretchWidth = true;
            thisStyle.padding = new RectOffset(4, 0, 0, 0);
            thisStyle.margin = new RectOffset(4, 0, 0, 0);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.stretchHeight = true;
            buttonStyle.stretchWidth = true;
            buttonStyle.padding = new RectOffset(4, 0, 0, 0);
            buttonStyle.margin = new RectOffset(4, 0, 0, 0);

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.stretchHeight = true;
            boxStyle.stretchWidth = true;
            boxStyle.padding = new RectOffset(4, 0, 0, 0);
            boxStyle.margin = new RectOffset(4, 0, 0, 0);

            activeTab = (MenuTab)GUILayout.SelectionGrid((int)activeTab, MenuTab_str, 4);

            if (activeTab == MenuTab.DebugAndData)
                DebugAndDataTab(thisStyle);
            else if (activeTab == MenuTab.PartClassification)
                PartClassificationTab(buttonStyle, boxStyle);

            //            SaveWindowPos.x = windowPos.x;
            //            SaveWindowPos.y = windowPos.y;

            GUI.DragWindow();

            debugWinPos = FARGUIUtils.ClampToScreen(debugWinPos);
        }

        private void PartClassificationTab(GUIStyle buttonStyle, GUIStyle boxStyle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Greeble - Parts with low, orientation un-affected drag");

            //Greeble Title Section
            GUILayout.Label("Title Contains:");
            StringListUpdateGUI(FARPartClassification.greebleTitles, buttonStyle, boxStyle);

            //Greeble Modules Section
            GUILayout.Label("Part Modules:");
            StringListUpdateGUI(FARPartClassification.greebleModules, buttonStyle, boxStyle);

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Exempt - Parts that do not get a FAR drag model");

            //Exempt Modules Section
            GUILayout.Label("Part Modules:");
            StringListUpdateGUI(FARPartClassification.exemptModules, buttonStyle, boxStyle);

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Specialized Modules - Used to determine fairings and cargo bays");

            //Payload Fairing Section
            GUILayout.Label("Fairing Title Contains:");
            StringListUpdateGUI(FARPartClassification.payloadFairingTitles, buttonStyle, boxStyle);

            //Payload Fairing Section
            GUILayout.Label("Cargo Bay Title Contains:");
            StringListUpdateGUI(FARPartClassification.cargoBayTitles, buttonStyle, boxStyle);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void StringListUpdateGUI(List<string> stringList, GUIStyle thisStyle, GUIStyle boxStyle)
        {
            int removeIndex = -1;
            GUILayout.BeginVertical(boxStyle);
            for (int i = 0; i < stringList.Count; i++)
            {
                string tmp = stringList[i];
                GUILayout.BeginHorizontal();
                tmp = GUILayout.TextField(tmp, GUILayout.Height(30));
                if (GUILayout.Button("-", thisStyle, GUILayout.Width(30), GUILayout.Height(30)))
                    removeIndex = i;
                GUILayout.EndHorizontal();
                if (removeIndex >= 0)
                    break;

                stringList[i] = tmp;
            }
            if (removeIndex >= 0)
            {
                stringList.RemoveAt(removeIndex);
                removeIndex = -1;
            }
            if (GUILayout.Button("+", thisStyle, GUILayout.Width(30), GUILayout.Height(30)))
                stringList.Add("");

            GUILayout.EndVertical();
        }
        
        private void DebugAndDataTab(GUIStyle thisStyle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Part Right-Click Menu");
            FARDebugValues.displayForces = GUILayout.Toggle(FARDebugValues.displayForces, "Display Aero Forces", thisStyle);
            FARDebugValues.displayCoefficients = GUILayout.Toggle(FARDebugValues.displayCoefficients, "Display Coefficients", thisStyle);
            FARDebugValues.displayShielding = GUILayout.Toggle(FARDebugValues.displayShielding, "Display Shielding", thisStyle);
            GUILayout.Label("Debug / Cheat Options");
            FARDebugValues.useSplinesForSupersonicMath = GUILayout.Toggle(FARDebugValues.useSplinesForSupersonicMath, "Use Splines for Supersonic Math", thisStyle);
            FARDebugValues.allowStructuralFailures = GUILayout.Toggle(FARDebugValues.allowStructuralFailures, "Allow Aero-structural Failures", thisStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public static void LoadConfigs()
        {
            config = KSP.IO.PluginConfiguration.CreateForType<FARDebugOptions>();
            config.load();
            FARDebugValues.displayForces = Convert.ToBoolean(config.GetValue("displayForces", "false"));
            FARDebugValues.displayCoefficients = Convert.ToBoolean(config.GetValue("displayCoefficients", "false"));
            FARDebugValues.displayShielding = Convert.ToBoolean(config.GetValue("displayShielding", "false"));
            FARDebugValues.useSplinesForSupersonicMath = Convert.ToBoolean(config.GetValue("useSplinesForSupersonicMath", "true"));
            FARDebugValues.allowStructuralFailures = Convert.ToBoolean(config.GetValue("allowStructuralFailures", "true"));

            FARAeroStress.LoadStressTemplates();
            FARPartClassification.LoadClassificationTemplates();
            FARAeroUtil.LoadAeroDataFromConfig();
        }

        public static void SaveConfigs()
        {
            config.SetValue("displayForces", FARDebugValues.displayForces.ToString());
            config.SetValue("displayCoefficients", FARDebugValues.displayCoefficients.ToString());
            config.SetValue("displayShielding", FARDebugValues.displayShielding.ToString());

            config.SetValue("useSplinesForSupersonicMath", FARDebugValues.useSplinesForSupersonicMath.ToString());
            config.SetValue("allowStructuralFailures", FARDebugValues.allowStructuralFailures.ToString());

            FARPartClassification.SaveCustomClassificationTemplates();
            config.save();
        }
        void OnDestroy()
        {
            SaveConfigs();
            FARDebugButton.Destroy();
        }

    }

    public static class FARDebugValues
    {
        //Right-click menu options
        public static bool displayForces = false;
        public static bool displayCoefficients = false;
        public static bool displayShielding = false;

        public static bool useSplinesForSupersonicMath = true;
        public static bool allowStructuralFailures = true;
    }
}
