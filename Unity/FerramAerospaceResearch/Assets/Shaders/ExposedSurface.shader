Shader "FerramAerospaceResearch/Exposed Surface"
{
    Properties
    {
        [PerRendererData] _ExposedColor ("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            // pragmas
            #pragma vertex vert
            #pragma fragment frag

            uniform float4 _ExposedColor;

            struct i2f
            {
                float4 vertex: POSITION;
            };

            struct v2f
            {
                float4 vertex: SV_POSITION;
            };

            float4x4 _VPMatrix;

            inline float4 ObjectToClipPos(in float3 pos)
            {
                return mul(_VPMatrix, mul(unity_ObjectToWorld, float4(pos, 1.0)));
            }

            inline float4 ObjectToClipPos(float4 pos)
            {
                return ObjectToClipPos(pos.xyz);
            }

            v2f vert(i2f v)
            {
                v2f o;
                o.vertex = ObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                return _ExposedColor;
            }
            ENDCG
        }
    }
}
