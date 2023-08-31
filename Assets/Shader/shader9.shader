Shader "Unlit/shader9"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor("OutlineColor",Color) = (0,0.5,0.5,1)
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
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal: TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                // 在世界空间中会获得正确的菲涅尔效果，在屏幕空间中会出现偏差; 这里观察者指世界空间中的观察者
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float fresnel = dot(V,N); // 观察者与物体法线夹角的余弦值,3维物体边缘的法线方向与观察者方向垂直，余弦值为0
                return (1 - fresnel) * _OutlineColor;
            }
            ENDCG
        }
    }
}
