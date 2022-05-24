Shader "FerramAerospaceResearch/Exposed Surface"
{
    Properties
    {
        [PerRendererData] _ExposedColor ("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Lighting Off Fog { Mode Off }
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

                v2f vert(i2f v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);

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
