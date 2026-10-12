Shader "Campus/8 Bit Background"
{
    Properties {
        [PerRendererData] _MainTex ("Map", 2D) = "white" {}
        _PixelGrid ("Pixel grid (0 = native tile pixels)", Vector) = (512,288,0,0)
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CampusPixel.cginc"
            struct input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            output vert(input v) { output o; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            fixed4 frag(output i):SV_Target { return CampusEightBit(CampusPixelSample(i.uv))*i.color; }
            ENDCG
        }
    }
}
