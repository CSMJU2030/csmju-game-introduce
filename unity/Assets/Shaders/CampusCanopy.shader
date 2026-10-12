Shader "Campus/Tree Canopy"
{
    Properties { _MainTex ("Campus", 2D) = "white" {} _PixelGrid ("Pixel grid", Vector) = (512,288,0,0) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CampusPixel.cginc"
            struct input { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            output vert(input v) { output o; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            fixed4 frag(output i):SV_Target {
                fixed4 c=CampusPixelSample(i.uv);
                // Only vegetation is rendered in front; paths remain on the ground layer.
                clip(min(min(c.g-c.r*1.03,c.g-c.b*1.16),c.g-c.b-.04));
                return CampusEightBit(c)*i.color;
            }
            ENDCG
        }
    }
}
