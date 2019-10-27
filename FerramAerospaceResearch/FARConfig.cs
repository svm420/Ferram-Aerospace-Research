/*
Ferram Aerospace Research v0.15.11.3 "Mach"
=========================
Aerodynamics model for Kerbal Space Program

Copyright 2019, Daumantas Kavolis, aka dkavolis

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
using System.Diagnostics;
using System.IO;
using FerramAerospaceResearch.FARGUI;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    public class FARConfig
    {
        private const string LogsDirectoryName = "logsDirectory";
        private const string LogNameFormatName = "logNameFormat";
        private const string LogDatetimeFormatName = "logDatetimeFormat";

        private static FARConfig instance;

        public static FARConfig Instance
        {
            get { return instance ??= new FARConfig(); }
        }

        internal static GUIColors Colors
        {
            get { return GUIColors.Instance; }
        }

        private string logsDirectory;
        private string logDateTimeFormat;

        private readonly Dictionary<string, ConfigNode> configs = new Dictionary<string, ConfigNode>();

        public string LogsDirectory
        {
            get { return logsDirectory; }
        }

        public string LogDateTimeFormat
        {
            get { return logDateTimeFormat; }
        }

        public StringFormatter LogNameFormat { get; private set; }

        public string KSPRootPath { get; private set; }
        public string FARRootPath { get; private set; }

        private FARConfig()
        {
            SetupDefault();
            LoadConfig();
            PrintConfig();
        }

        private void SetupDefault()
        {
            KSPRootPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
            FARRootPath = Path.Combine(KSPRootPath, "GameData/FerramAerospaceResearch");
            logsDirectory = Path.Combine(FARRootPath, "logs");
            logDateTimeFormat = "yyyy_MM_dd_HH_mm_ss";
            LogNameFormat = new StringFormatter("<<<VESSEL_NAME>>>_<<<DATETIME>>>.csv");
        }

        public string FilenameFARRoot(string filename)
        {
            return Path.Combine(FARRootPath, filename);
        }

        public string FilenameKSPRoot(string filename)
        {
            return Path.Combine(KSPRootPath, filename);
        }

        public void LoadConfig()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("FARConfig");

            if (nodes.Length == 0)
            {
                FARLogger.Error("Could not find FARConfig node in GameDatabase");
                return;
            }

            if (nodes.Length > 1)
                FARLogger.Info($"Found {nodes.Length.ToString()} FARConfig nodes");

            string name = string.Empty;
            int count = 0;
            foreach (ConfigNode config in nodes)
            {
                if (!config.TryGetValue("name", ref name))
                    name = "default" + (count++).ToString();
                configs.Add(name, config);

                FARLogger.Debug($"Loading config {name}");
                LoadConfig(config);
            }
        }

        public void LoadConfig(string name)
        {
            if (!configs.TryGetValue(name, out ConfigNode config))
                FARLogger.Error($"Could not find FARConfig with name {name}");
            FARLogger.Debug($"Loading config {name}");
            LoadConfig(config);
        }

        private void LoadConfig(ConfigNode config)
        {
            if (config.TryGetValue(LogsDirectoryName, ref logsDirectory))
                if (!Path.IsPathRooted(logsDirectory))
                    logsDirectory = Path.Combine(KSPRootPath, logsDirectory);

            config.TryGetValue(LogDatetimeFormatName, ref logDateTimeFormat);

            string str = string.Empty;
            if (config.TryGetValue(LogNameFormatName, ref str))
                LogNameFormat.FormatString = str;
        }

        public void SaveConfig(string filename = "GameData/FerramAerospaceResearch/CustomFARConfig.cfg")
        {
            var node = new ConfigNode("@FARConfig[default]:FOR[FerramAerospaceResearch]");
            node.AddValue($"%{LogsDirectoryName}", logsDirectory);
            node.AddValue($"%{LogNameFormatName}", LogNameFormat.FormatString);
            node.AddValue($"%{LogDatetimeFormatName}", logDateTimeFormat);

            var saveNode = new ConfigNode();
            saveNode.AddNode(node);
            saveNode.Save(FilenameKSPRoot(filename));
        }

        [Conditional("DEBUG")]
        private void PrintConfig()
        {
            FARLogger.Info($@"FAR was loaded with config values:
    Logs directory: {logsDirectory}
    Logs filename format: {LogNameFormat.FormatString}
    Logs datetime suffix: {logDateTimeFormat}");
        }
    }
}
