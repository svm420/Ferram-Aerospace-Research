using UnityEngine;

namespace FerramAerospaceResearch.Resources
{
    public static class ShaderPropertyIds
    {
        public static readonly int Color = Shader.PropertyToID("_Color");
        public static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
        public static readonly int ExposedColor = Shader.PropertyToID("_ExposedColor");
        public static readonly int InputTexture = Shader.PropertyToID("InputTexture");
        public static readonly int SortedIds = Shader.PropertyToID("SortedIds");
        public static readonly int IdCount = Shader.PropertyToID("IdCount");
        public static readonly int OutputBuffer = Shader.PropertyToID("OutputBuffer");
    }
}
