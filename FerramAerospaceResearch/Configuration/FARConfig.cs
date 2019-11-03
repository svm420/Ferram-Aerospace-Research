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
using System.Reflection;
using System.Text;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    public class FARConfig
    {
        public const string ConfigNodeName = "FARConfig";
        public const string ConfigSectionName = "CONFIG";
        public const string CustomConfigFilename = "CustomFARConfig.cfg";
        public const string ModDirectoryName = "FerramAerospaceResearch";
        private static FARConfig instance;

        private readonly Dictionary<string, IConfigNode> configs = new Dictionary<string, IConfigNode>();

        private FARConfig()
        {
            SetupDefault();
        }

        public static FARConfigProvider Provider { get; internal set; }

        public FARConfigParser this[string key]
        {
            get { return Provider.Parsers[key]; }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global - trigger lazy instantiation
        public Dictionary<string, FARConfigParser> Parsers
        {
            get { return Provider.Parsers; }
        }

        public static string FARRootPath { get; } =
            Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "..", ".."));

        public static string FARRelativePath { get; } = Path.Combine("GameData", ModDirectoryName);
        public static string GameDataPath { get; } = Path.GetFullPath(Path.Combine(FARRootPath, ".."));
        public static string KSPRootPath { get; } = Path.GetFullPath(Path.Combine(GameDataPath, ".."));

        public static FARConfig Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                instance = new FARConfig();

                // delay loading until FARConfig has been instantiated, this allows parsers to access
                // FARConfig instance in their parse methods
                instance.LoadConfig();
                instance.PrintConfig();
                return instance;
            }
        }

        public bool ContainsConfig(string name)
        {
            return Parsers.ContainsKey(name);
        }

        private void SetupDefault()
        {
            if (Provider == null)
            {
                FARLogger.Error("Something went wrong, FARConfig is accessed before it could be properly setup");
                return;
            }

            foreach (KeyValuePair<string, FARConfigParser> pair in Parsers)
                pair.Value.Reset();
        }

        private static string Combine(string path, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, filename);
        }

        private static string Combine(string path, string dir1, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, dir1, filename);
        }

        private static string Combine(string path, string dir1, string dir2, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, dir1, dir2, filename);
        }

        public static string CombineFARRoot(string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(FARRootPath, filename);
        }

        public static string CombineFARRoot(string dir1, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(FARRootPath, dir1, filename);
        }

        public static string CombineFARRoot(string dir1, string dir2, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(FARRootPath, dir1, dir2, filename);
        }

        public static string FARGameDataRelative(string filename)
        {
            return $"{ModDirectoryName}/{filename}";
        }

        public static string FARGameDataRelative(string dir1, string filename)
        {
            return $"{ModDirectoryName}/{dir1}/{filename}";
        }

        public static string FARGameDataRelative(string dir1, string dir2, string filename)
        {
            return $"{ModDirectoryName}/{dir1}/{dir2}/{filename}";
        }

        public static string CombineKSPRoot(string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(KSPRootPath, filename);
        }

        public static string CombineKSPRoot(string dir1, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(KSPRootPath, dir1, filename);
        }

        public static string CombineKSPRoot(string dir1, string dir2, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(KSPRootPath, dir1, dir2, filename);
        }

        public static string CombineGameData(string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(GameDataPath, filename);
        }

        public static string CombineGameData(string dir1, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(GameDataPath, dir1, filename);
        }

        public static string CombineGameData(string dir1, string dir2, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Combine(GameDataPath, dir1, dir2, filename);
        }

        public void LoadConfig()
        {
            IConfigNode[] nodes = Provider.LoadConfigs(ConfigNodeName);

            string name = string.Empty;
            int count = 0;
            foreach (IConfigNode config in nodes)
            {
                if (!config.TryGetValue("name", ref name))
                {
                    FARLogger.Warning($"Nameless {ConfigNodeName} encountered");
                    name = "default" + (count++).ToString();
                }

                configs.Add(name, config);

                FARLogger.Debug($"Loading config {name}");
                LoadConfig(config, name);
            }
        }

        public void LoadConfig(string name)
        {
            if (!configs.TryGetValue(name, out IConfigNode config))
            {
                FARLogger.Error($"Could not find {ConfigNodeName} with name '{name}'");
                return;
            }

            FARLogger.Debug($"Loading config '{name}'");
            LoadConfig(config, name);
        }

        private void LoadConfig(IConfigNode config, string name)
        {
            IConfigNode[] nodes = config.GetNodes(ConfigSectionName);
            if (nodes.Length == 0)
            {
                FARLogger.Warning($"{ConfigNodeName} '{name}' does not contain any sections");
                return;
            }

            string sectionName = string.Empty;
            foreach (IConfigNode node in nodes)
            {
                if (!node.TryGetValue("name", ref sectionName))
                {
                    FARLogger.Warning($"Invalid section in {ConfigNodeName} '{name}' - missing section name");
                    continue;
                }

                if (!Parsers.TryGetValue(sectionName, out FARConfigParser parser))
                {
                    FARLogger.Warning($"Invalid section name '{sectionName}'");
                    continue;
                }

                FARLogger.Debug($"Parsing config node '{sectionName}'");
                parser.Parse(node);
            }
        }

        public void SaveConfig()
        {
            SaveConfig(CombineGameData(CustomConfigFilename));
        }

        public void SaveConfig(string filename)
        {
            IConfigNode node = Provider.CreateNode($"@{ConfigNodeName}[default]:FOR[FerramAerospaceResearch]");

            foreach (KeyValuePair<string, FARConfigParser> pair in Parsers)
            {
                IConfigNode section = Provider.CreateNode($"@{ConfigSectionName}[{pair.Key}]");
                pair.Value.SaveTo(section);
                node.AddNode(section);
            }

            IConfigNode saveNode = Provider.CreateNode();
            saveNode.AddNode(node);
            saveNode.Save(CombineKSPRoot(filename));
        }

        [Conditional("DEBUG")]
        private void PrintConfig()
        {
            var sb = new StringBuilder();
            sb.AppendLine("FAR was loaded with config values:");
            foreach (KeyValuePair<string, FARConfigParser> pair in Parsers)
            {
                sb.Append("  ").Append(pair.Key).AppendLine(":");
                pair.Value.DebugString(sb);
            }

            FARLogger.Info(sb.ToString());
        }
    }
}
