/*
Ferram Aerospace Research v0.15.11.3 "Mach"
=========================
Aerodynamics model for Kerbal Space Program

Copyright 2019, Michael Ferrara, aka Ferram4

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
using System.Text;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI
{
    // ReSharper disable once ClassNeverInstantiated.Global - instantiated through reflection
    [ConfigParserAttribute("guiColors")]
    internal class GUIColors : FARConfigParser<GUIColors>
    {
        private const string ClColorName = "ClColor";
        private const string CdColorName = "CdColor";
        private const string CmColorName = "CmColor";
        private const string LDColorName = "L_DColor";
        private static readonly char[] separators = {',', ' ', ';'};

        private readonly List<Color> colors = new List<Color>();

        public Color this[int index]
        {
            get { return colors[index]; }
            set { colors[index] = value; }
        }

        public static Color GetColor(int index)
        {
            return Instance[index];
        }

        private static Color ReadColor(string input)
        {
            string[] splitValues = input.Split(separators);

            int curIndex = 0;
            var color = new Color {a = 1};
            foreach (string s in splitValues)
            {
                if (s.Length <= 0)
                    continue;
                if (!float.TryParse(s, out float val))
                    continue;
                switch (curIndex)
                {
                    case 0:
                        color.r = val;
                        break;
                    case 1:
                        color.g = val;
                        break;
                    default:
                        color.b = val;
                        return color;
                }

                curIndex++;
            }

            return color;
        }

        private static string SaveColor(Color color)
        {
            var builder = new StringBuilder();

            //Should return string in format of color.r, color.g, color.b
            builder.Append(color.r);
            builder.Append(",");
            builder.Append(color.g);
            builder.Append(",");
            builder.Append(color.b);

            return builder.ToString();
        }

        public override void Reset()
        {
            colors.Clear();
        }

        public override void Parse(IConfigNode node)
        {
            if (node.HasValue(ClColorName))
                colors.Add(ReadColor(node.GetValue(ClColorName)));

            if (node.HasValue(CdColorName))
                colors.Add(ReadColor(node.GetValue(CdColorName)));

            if (node.HasValue(CmColorName))
                colors.Add(ReadColor(node.GetValue(CmColorName)));

            if (node.HasValue(LDColorName))
                colors.Add(ReadColor(node.GetValue(LDColorName)));
        }

        public override void SaveTo(IConfigNode node)
        {
            node.AddValue($"%{ClColorName}", SaveColor(colors[0]));
            node.AddValue($"%{CdColorName}", SaveColor(colors[1]));
            node.AddValue($"%{CmColorName}", SaveColor(colors[2]));
            node.AddValue($"%{LDColorName}", SaveColor(colors[3]));
        }

        public override void DebugString(StringBuilder sb)
        {
            AppendEntry(sb, ClColorName, colors[0]);
            AppendEntry(sb, CdColorName, colors[1]);
            AppendEntry(sb, CmColorName, colors[2]);
            AppendEntry(sb, LDColorName, colors[3]);
        }
    }
}
