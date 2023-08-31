Shader "Unlit/AmbienShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Ambient ("Ambient Texture",2D) ="white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Ambient;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            float2 tranlsteDir(float3 dir){
                float x = atan2(dir.z,dir.x) / TAU + 0.5;
                float y =dir.y * 0.5 + 0.5;
                return float2(x,y);
            }

            float3 frag (v2f i) : SV_Target
            {
                float3 col = tex2Dlod(_Ambient,float4(tranlsteDir(i.normal),0,0));
                return col;
            }
            ENDCG
        }
    }
}
