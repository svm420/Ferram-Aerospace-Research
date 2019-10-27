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
using System.IO;

namespace FerramAerospaceResearch.FARUtils
{
    public class CsvWriter : IDisposable
    {
        private const string Separator = ", ";

        public CsvWriter(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; private set; }

        protected StreamWriter Writer { get; private set; }
        public bool NewFile { get; private set; }

        public bool IsOpen
        {
            get { return Writer != null; }
        }

        public void Open()
        {
            Open(Filename);
        }

        public void Open(string filename)
        {
            Filename = filename;
            NewFile = !File.Exists(filename);
            Writer = File.AppendText(filename);
            Writer.AutoFlush = false;
        }

        public void Close()
        {
            Writer?.Close();
            Writer?.Dispose();
            Writer = null;
        }

        public void Write<T>(T[] values)
        {
            foreach (T value in values)
                Write(value);
        }

        public void Write<T>(T value)
        {
            Writer.Write(value.ToString());
            Writer.Write(Separator);
        }

        public void WriteLine<T>(T[] values)
        {
            Write(values);
            Writer.WriteLine();
        }

        public void WriteLine<T>(T value)
        {
            Write(value);
            Writer.WriteLine();
        }

        public void WriteLine()
        {
            Writer.WriteLine();
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            Close();
        }
    }
}
