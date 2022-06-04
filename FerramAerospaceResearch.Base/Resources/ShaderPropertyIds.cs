using UnityEngine;

// ReSharper disable InconsistentNaming

namespace FerramAerospaceResearch.Resources
{
    public static class ShaderPropertyIds
    {
        public static readonly int _Color = Shader.PropertyToID("_Color");
        public static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        public static readonly int _ExposedColor = Shader.PropertyToID("_ExposedColor");
        public static readonly int InputTexture = Shader.PropertyToID("InputTexture");
        public static readonly int OutputBuffer = Shader.PropertyToID("OutputBuffer");
        public static readonly int _ColorTex = Shader.PropertyToID("_ColorTex");
        public static readonly int _BackgroundColor = Shader.PropertyToID("_BackgroundColor");
        public static readonly int _VPMatrix = Shader.PropertyToID("_VPMatrix");
    }
}
