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

using System;
using System.Collections.Generic;
using System.Text;

namespace FerramAerospaceResearch.FARUtils
{
    public class StringFormatter
    {
        private const string LeftBrace = "<<<";
        private const string RightBrace = ">>>";

        private readonly List<Field> fields = new List<Field>();
        private readonly StringBuilder stringBuilder = new StringBuilder();
        protected string formatString;

        public StringFormatter(string formatString)
        {
            Parse(formatString);
        }

        public string FormatString
        {
            get { return formatString; }
            set { Parse(value); }
        }

        public void Parse(string str)
        {
            fields.Clear();
            formatString = str;

            int begin = 0;
            while (begin < str.Length)
            {
                int replaceBegin = str.IndexOf(LeftBrace, begin, StringComparison.Ordinal);

                // no replacements found
                if (replaceBegin < 0)
                {
                    fields.Add(new Field
                    {
                        Type = FieldType.Text,
                        Str = str.Substring(begin, str.Length - begin)
                    });
                    break;
                }

                int paramBegin = replaceBegin + LeftBrace.Length;
                int paramEnd = str.IndexOf(RightBrace, paramBegin, StringComparison.Ordinal);
                // no closing brace found
                if (paramEnd < 0)
                {
                    fields.Add(new Field
                    {
                        Type = FieldType.Text,
                        Str = str.Substring(begin, str.Length - begin)
                    });
                    break;
                }

                // found braced parameter
                fields.Add(new Field
                {
                    Type = FieldType.Text,
                    Str = str.Substring(begin, replaceBegin - begin)
                });

                fields.Add(new Field
                {
                    Type = FieldType.Replacement,
                    Str = str.Substring(paramBegin, paramEnd - paramBegin)
                });

                begin = paramEnd + RightBrace.Length;
            }
        }

        public string ToString(Dictionary<string, object> replacements)
        {
            stringBuilder.Clear();
            foreach (Field field in fields)
            {
                if (field.Type == FieldType.Text)
                    stringBuilder.Append(field.Str);
                else if (replacements.TryGetValue(field.Str, out object o))
                    stringBuilder.Append(o.ToString());
                else
                {
                    stringBuilder.Append(LeftBrace);
                    stringBuilder.Append(field.Str);
                    stringBuilder.Append(RightBrace);
                }
            }

            return stringBuilder.ToString();
        }

        private enum FieldType
        {
            Text,
            Replacement
        }

        private struct Field
        {
            public FieldType Type;
            public string Str;
        }
    }
}
