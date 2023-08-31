Shader "Unlit/shader7"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderSize("BorderSize",Range(0,0.5)) = 0.1
        _BorderTexture("BorderTexture",2D) = "bulue"{}
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
            sampler2D _BorderTexture;
            float _BorderSize;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float InverseLerp(float a,float b,float v){
                return (v - a) / (b - a);
            }
            fixed4 frag (v2f i) : SV_Target
            {   
                float2 coords = i.uv;
                coords.x *= 10;
                float2 pointOnLineSeg = float2(clamp(coords.x,0.5,9.5),0.5);
                float dist = distance(pointOnLineSeg,coords) - 0.5;
                clip(-dist);
                float sdfMask = dist + _BorderSize;
                sdfMask = step(0,sdfMask);
                float4 sampleCol = tex2D(_BorderTexture,i.uv);
                //return sampleCol;
                return sdfMask * sampleCol;
            }
            ENDCG
        }
    }
}
