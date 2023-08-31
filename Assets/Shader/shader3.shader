Shader "Unlit/shader3"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Rock("Rock",2D) = "white"{}
        _Pattern("Pattern",2D) = "white"{}
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
                float4 worldPos: TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Rock;
            sampler2D _Pattern;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(UNITY_MATRIX_M,v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 topDownProjection = i.worldPos.xz;
                fixed4 mainCol = tex2D(_MainTex,topDownProjection);
                fixed4 rockCol = tex2D(_Rock,topDownProjection);
                fixed4 patternCol = tex2D(_Pattern,i.uv);
                float4 res = lerp(rockCol,mainCol,patternCol);
                return res;
            }
            ENDCG
        }
    }
}
