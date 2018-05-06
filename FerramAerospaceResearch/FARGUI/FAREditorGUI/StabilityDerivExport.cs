using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI
{
    class StabilityDerivativeExportFileElement
    {
        public string name;

        public StabilityDerivativeExportFileElement(string name)
        {
            this.name = name;
        }

        public virtual List<string> Lines
        { get { return new List<string>(); } }
    }

    class StructElement : StabilityDerivativeExportFileElement
    {
        public List<StabilityDerivativeExportFileElement> subelems;

        public StructElement(string name) : base(name)
        {
            subelems = new List<StabilityDerivativeExportFileElement>();
        }

        public void AddElement(StabilityDerivativeExportFileElement element)
        {
            subelems.Add(element);
        }

        public void AddScalar(string name, double value)
        {
            ScalarElement scalar = new ScalarElement(name, value);
            this.AddElement(scalar);
        }

        public override List<string> Lines
        {
            get
            {
                List<string> result = new List<string>();
                result.Add("# name: " + this.name);
                result.Add("# type: scalar struct");
                result.Add("# ndims: 2");
                result.Add(" 1 1");
                result.Add("# length: " + subelems.Count);
                foreach (StabilityDerivativeExportFileElement elem in subelems)
                {
                    result.Add("");
                    foreach (string s in elem.Lines)
                        result.Add(s);
                }
                return result;
            }
        }
    }

    class StringElement : StabilityDerivativeExportFileElement
    {
        public string value;

        static public string OnlyASCII(string s)
        {
            if (s == null || s == "")
                return "";
            var sb = new System.Text.StringBuilder(s.Length, s.Length);
            foreach (char c in s)
                if (31 < (int)c && (int)c < 127)
                    sb.Append(c);
            return sb.ToString();
        }

        public StringElement(string name, string value) : base(name)
        {
            this.value = OnlyASCII(value);
        }

        public override List<string> Lines
        {
            get
            {
                List<string> result = new List<string>();
                result.Add("# name: " + this.name);
                result.Add("# type: string");
                result.Add("# elements: 1");
                result.Add("# length: " + value.Length);
                result.Add(value);
                return result;
            }
        }
    }

    class ScalarElement : StabilityDerivativeExportFileElement
    {
        public double value;
        private System.Globalization.CultureInfo enus;

        public ScalarElement(string name, double value) : base(name)
        {
            this.value = value;
            this.enus = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
        }

        public override List<string> Lines
        {
            get
            {
                List<string> result = new List<string>();
                result.Add("# name: " + this.name);
                result.Add("# type: scalar");
                result.Add(value.ToString("E16", enus));
                return result;
            }
        }
    }

    class StabilityDerivativeExportFile
    {
        private StructElement cellelement;
        private List<string> bodytext;
        private int bodytextcount;

        public StabilityDerivativeExportFile()
        {
            cellelement = new StructElement("<cell-element>");
            bodytext = new List<string>();
            bodytextcount = -1;
        }

        static public string ConfigFilePath
        {
            get
            {
            string path = KSPUtil.ApplicationRootPath;
            path += "GameData/FerramAerospaceResearch/Plugins/PluginData/FerramAerospaceResearch/";
            path += "sdexpcfg.txt";
            return path;
            }
        }

        static public List<Vector2> LoadConfigList()
        {
            var resultlist = new List<Vector2>();
            string path = ConfigFilePath;
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path, System.Text.Encoding.Default);
                if (lines.Length >= 6)
                {
                    bool b1 = lines[0].StartsWith("# Created by");
                    bool b2 = lines[1] == "# name: altmach";
                    bool b3 = lines[2] == "# type: matrix";
                    bool b4 = lines[3].StartsWith("# rows: ");
                    bool b5 = lines[4] == "# columns: 2";
                    if (b1 && b2 && b3 && b4 && b5)
                    {
                        var enus = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                        var floatstyle = System.Globalization.NumberStyles.Float;
                        int n = int.Parse(lines[3].Remove(0, 8));
                        if (lines.Length >= 5 + n)
                        {
                            for (int i = 5; i < 5 + n; i++)
                            {
                                string[] line = lines[i].Trim().Split(new char[]{' '});
                                if (line.Length == 2)
                                {
                                    float alt; float mach;
                                    if (float.TryParse(line[0], floatstyle, enus, out alt)
                                     && float.TryParse(line[1], floatstyle, enus, out mach))
                                    {
                                        resultlist.Add(new Vector2(alt, mach));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return resultlist;
        }

        static public string TextFilePath
        {
            get
            {
            string path = KSPUtil.ApplicationRootPath;
            path += "GameData/FerramAerospaceResearch/Plugins/PluginData/";
            path += "sdexport.txt";
            return path;
            }
        }

        static public string[] LoadTextFile()
        {
            string path = TextFilePath;
            if (File.Exists(path))
                return File.ReadAllLines(path, System.Text.Encoding.Default);
            else
                return new string[] {};
        }

        static public void SaveTextFile(List<string> lines)
        {
            string path = TextFilePath;
            File.WriteAllLines(path, lines.ToArray());
        }

        public void LoadBodyText()
        {
            bodytext = new List<string>();
            string[] lines = LoadTextFile();
            if (lines.Length == 0)
                bodytextcount = 0;
            else if (lines.Length < 5)
                bodytextcount = -1;
            else
            {
                bool b1 = lines[0].StartsWith("# Created by");
                bool b2 = lines[1] == "# name: data";
                bool b3 = lines[2] == "# type: cell";
                bool b4 = lines[3] == "# ndims: 3";
                bool b5 = lines[4].StartsWith(" 1 1 ");
                if (b1 && b2 && b3 && b4 && b5)
                {
                    bodytextcount = int.Parse(lines[4].Remove(0, 5));
                    int firstidx = 5;
                    int lastidx = lines.Length - 1;
                    for (int i = firstidx; i == firstidx && i < lastidx; i++)
                        if (lines[i] == "")
                            firstidx = i + 1;
                    for (int i = lastidx; i == lastidx && i > firstidx; i--)
                        if (lines[i] == "")
                            lastidx = i - 1;
                    for (int i = firstidx; i <= lastidx; i++)
                        bodytext.Add(lines[i]);
                }
                else
                    bodytextcount = -1;
            }
        }

        public bool BodyTextLoaded()
        {
            return bodytextcount >= 0;
        }

        public void AddElement(StabilityDerivativeExportFileElement element)
        {
            cellelement.AddElement(element);
        }

        public List<string> GetAllLines()
        {
            List<string> result = new List<string>();
            result.Add("# Created by Ferram Aerospace Research plugin for Kerbal Space Program");
            result.Add("# name: data");
            result.Add("# type: cell");
            result.Add("# ndims: 3");
            result.Add(" 1 1 " + (bodytextcount + 1));
            result.Add("");
            foreach (string s in bodytext)
                result.Add(s);
            result.Add("");
            foreach (string s in cellelement.Lines)
                result.Add(s);
            return result;
        }

        public void SaveAllText()
        {
            if (BodyTextLoaded())
                SaveTextFile(GetAllLines());
            else
                throw new InvalidOperationException("Cannot save result file because the file was not loaded properly in the first place.");
        }

        static public string EditorShipName
        {
            get
            {
                EditorLogic logic = EditorLogic.fetch;
                if (logic == null || logic.shipNameField == null || logic.shipNameField.text == null)
                    return "";
                else
                    return logic.shipNameField.text;
            }
        }

        public void AddResultElements(Simulation.StabilityDerivExportOutput output)
        {
            if (cellelement.subelems.Count > 0)
                throw new InvalidOperationException("Cannot add result elements; top cell element was not empty.");
            else
            {
                StructElement craft = new StructElement("craft");
                StructElement env = new StructElement("env");
                StructElement deriv = new StructElement("deriv");
                StructElement inertia = new StructElement("inertia");
                
                craft.AddElement(new StringElement("name", EditorShipName));
                craft.AddScalar("mass", output.exportvals.craftmass);
                craft.AddScalar("span", output.outputvals.b);
                craft.AddScalar("chord", output.outputvals.MAC);
                craft.AddScalar("area", output.outputvals.area);
                craft.AddElement(inertia);
                inertia.AddScalar("lxx", output.outputvals.stabDerivs[0]);
                inertia.AddScalar("lyy", output.outputvals.stabDerivs[1]);
                inertia.AddScalar("lzz", output.outputvals.stabDerivs[2]);
                inertia.AddScalar("lxy", output.outputvals.stabDerivs[24]);
                inertia.AddScalar("lyz", output.outputvals.stabDerivs[25]);
                inertia.AddScalar("lxz", output.outputvals.stabDerivs[26]);
                
                env.AddElement(new StringElement("body", output.outputvals.body.name));
                env.AddScalar("altitude", output.outputvals.altitude);
                env.AddScalar("mach", output.exportvals.sitmach);
                env.AddScalar("pressure", output.exportvals.envpressure);
                env.AddScalar("temperature", output.exportvals.envtemperature);
                env.AddScalar("density", output.exportvals.envdensity);
                env.AddScalar("soundspeed", output.exportvals.envsoundspeed);
                env.AddScalar("g", output.exportvals.envg);
                env.AddScalar("speed", output.outputvals.nominalVelocity);
                env.AddScalar("dynpres", output.exportvals.sitdynpres);
                env.AddScalar("effg", output.exportvals.siteffg);
                
                deriv.AddScalar("Cl", output.outputvals.stableCl);
                deriv.AddScalar("Cd", output.outputvals.stableCd);
                deriv.AddScalar("AoA", output.outputvals.stableAoA);
                deriv.AddScalar("Zw", output.outputvals.stabDerivs[3]);
                deriv.AddScalar("Xw", output.outputvals.stabDerivs[4]);
                deriv.AddScalar("Mw", output.outputvals.stabDerivs[5]);
                deriv.AddScalar("Zu", output.outputvals.stabDerivs[6]);
                deriv.AddScalar("Xu", output.outputvals.stabDerivs[7]);
                deriv.AddScalar("Mu", output.outputvals.stabDerivs[8]);
                deriv.AddScalar("Zq", output.outputvals.stabDerivs[9]);
                deriv.AddScalar("Xq", output.outputvals.stabDerivs[10]);
                deriv.AddScalar("Mq", output.outputvals.stabDerivs[11]);
                deriv.AddScalar("Ze", output.outputvals.stabDerivs[12]);
                deriv.AddScalar("Xe", output.outputvals.stabDerivs[13]);
                deriv.AddScalar("Me", output.outputvals.stabDerivs[14]);
                deriv.AddScalar("Yb", output.outputvals.stabDerivs[15]);
                deriv.AddScalar("Lb", output.outputvals.stabDerivs[16]);
                deriv.AddScalar("Nb", output.outputvals.stabDerivs[17]);
                deriv.AddScalar("Yp", output.outputvals.stabDerivs[18]);
                deriv.AddScalar("Lp", output.outputvals.stabDerivs[19]);
                deriv.AddScalar("Np", output.outputvals.stabDerivs[20]);
                deriv.AddScalar("Yr", output.outputvals.stabDerivs[21]);
                deriv.AddScalar("Lr", output.outputvals.stabDerivs[22]);
                deriv.AddScalar("Nr", output.outputvals.stabDerivs[23]);

                AddElement(craft);
                AddElement(env);
                AddElement(deriv);
            }
        }

        static public bool Export(Simulation.StabilityDerivExportOutput output)
        {
            StabilityDerivativeExportFile body = new StabilityDerivativeExportFile();
            body.LoadBodyText();
            if (body.BodyTextLoaded())
            {
                body.AddResultElements(output);
                body.SaveAllText();
                return true;
            }
            else
                return false;
        }
    }
}
