Shader "FerramAerospaceResearch/Exposed Surface Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _ColorTex ("Colors", 2D) = "white" {}
        _BackgroundColor ("Background", Color) = (0,0,0,0)
        [HideInInspector] _Tag ("Tag", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "include/ExposedSurface.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uint _Tag;
            Texture2D _ColorTex;
            float4 _ColorTex_TexelSize; // zw is (width, height) of the texture
            SamplerState point_repeat_sampler; // point repeat sampler
            float4 _BackgroundColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                uint value = pack_uint(col);

                if (get_tag(value) != _Tag) {
                    // remove fully transparent background pixels
                    clip(_BackgroundColor.a - 1.f / 255.f);
                    return _BackgroundColor;
                }
                int index = get_index(value);
                float2 uv = float2(index / _ColorTex_TexelSize.z, 0.5);
                return _ColorTex.Sample(point_repeat_sampler, uv);
            }
            ENDCG
        }
    }
}
