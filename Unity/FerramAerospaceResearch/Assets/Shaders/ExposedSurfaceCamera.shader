Shader "FerramAerospaceResearch/Exposed Surface Camera"
{
    Properties
    {
        _ColorTex ("Color Texture", 2D) = "white" {}
        _BackgroundColor ("Background", Color) = (0,0,0,0)
        [PerRendererData] _ExposedColor ("Color", Color) = (0,0,0,0)
        [HideInInspector] _Tag ("Tag", Int) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma fragment frag
            #pragma vertex vert

            #include "UnityCG.cginc"
            #include "include/ExposedSurfaceDebug.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            uniform float4 _ExposedColor;

            fixed4 frag(v2f i) : SV_Target
            {
                uint value = pack_uint(_ExposedColor);
                return debug_color(value);
            }
            ENDCG
        }
    }
}
