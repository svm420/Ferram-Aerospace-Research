using UnityEngine;

namespace FerramAerospaceResearch.FARUtils
{
    public static class ShaderPropertyIds
    {
        public static int Color { get; } = Shader.PropertyToID("_Color");
        public static int Cutoff { get; } = Shader.PropertyToID("_Cutoff");
    }
}
