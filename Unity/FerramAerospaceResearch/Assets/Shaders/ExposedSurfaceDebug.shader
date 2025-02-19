Shader "FerramAerospaceResearch/Exposed Surface Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _ColorTex ("Color Texture", 2D) = "white" {}
        _BackgroundColor ("Background", Color) = (0,0,0,0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "include/ExposedSurfaceDebug.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                uint value = pack_uint(col);

                return debug_color(value);
            }
            ENDCG
        }
    }
    FallBack "Hidden/Internal-GUITexture"
}
