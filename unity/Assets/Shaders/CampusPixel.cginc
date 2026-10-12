#ifndef CAMPUS_PIXEL_INCLUDED
#define CAMPUS_PIXEL_INCLUDED
sampler2D _MainTex;
float4 _MainTex_TexelSize;
float4 _PixelGrid;
fixed4 CampusPixelSample(float2 uv)
{
    float2 grid = _PixelGrid.x > 0 ? _PixelGrid.xy : _MainTex_TexelSize.zw;
    return tex2D(_MainTex, (floor(uv * grid) + .5) / max(grid, 1));
}
fixed4 CampusEightBit(fixed4 color)
{
    // RGB332: 8 red × 8 green × 4 blue values, exactly 256 colours.
    #ifndef UNITY_COLORSPACE_GAMMA
    color.rgb = LinearToGammaSpace(color.rgb);
    #endif
    color.rgb = floor(saturate(color.rgb) * float3(7,7,3) + .5) / float3(7,7,3);
    #ifndef UNITY_COLORSPACE_GAMMA
    color.rgb = GammaToLinearSpace(color.rgb);
    #endif
    return color;
}
#endif
