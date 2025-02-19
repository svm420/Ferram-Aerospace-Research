#include "ExposedSurface.cginc"

uniform float4 _BackgroundColor;
Texture2D _ColorTex;
uniform float4 _ColorTex_TexelSize;
uniform SamplerState point_repeat_sampler;

fixed4 debug_color(uint value)
{
    if (!is_valid(value))
    {
        // remove fully transparent background pixels
        clip(_BackgroundColor.a - 1.f / 255.f);
        return _BackgroundColor;
    }

    int index = get_index(value);
    float2 uv = float2((index + 0.5) / _ColorTex_TexelSize.z, 0.5 / _ColorTex_TexelSize.w);
    return _ColorTex.Sample(point_repeat_sampler, uv);
}
