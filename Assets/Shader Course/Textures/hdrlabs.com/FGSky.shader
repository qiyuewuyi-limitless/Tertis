Shader "Unlit/FGSky"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #define TAU 6.28318530718
            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float2 DirToRectLiner(float3 dir){
                float x = atan2(dir.z,dir.x) / TAU + 0.5;
                float y = dir.y * 0.5 + 0.5;
                return float2(x,y);
            }
            float3 frag (v2f i) : SV_Target
            {
                float3 col = tex2Dlod(_MainTex,float4(DirToRectLiner(i.uv),0,0));
                return col;
            }
            ENDCG
        }
    }
}
