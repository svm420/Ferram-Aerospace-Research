using FerramAerospaceResearch.Reflection;
using UnityEngine;

namespace FerramAerospaceResearch.Config
{
    [ConfigNode("GuiColors")]
    public class GUIColors
    {
        private readonly Color[] colors = {Color.cyan, Color.red, Color.yellow, Color.green};

        [ConfigValue]
        public Color ClColor
        {
            get { return colors[0]; }
            set { colors[0] = value; }
        }

        [ConfigValue]
        public Color CdColor
        {
            get { return colors[1]; }
            set { colors[1] = value; }
        }

        [ConfigValue]
        public Color CmColor
        {
            get { return colors[2]; }
            set { colors[2] = value; }
        }

        [ConfigValue("L_DColor")]
        public Color LdColor
        {
            get { return colors[3]; }
            set { colors[3] = value; }
        }

        public Color GetColor(int index)
        {
            return colors[index];
        }
    }
}
