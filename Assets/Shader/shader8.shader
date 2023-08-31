Shader "Unlit/shader8"
{
    /*  光照练习 */
    Properties
    {
        [NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" { }
        _SpecularGloss ("SpecularGloss", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float _SpecularGloss;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            /**
                intensity = diffuse + specular + ambient
                diffuse:
                    慢反射光强 = 漫反射系数  * 光强 * 入射光方向向量 · 片元法向向量
                specular:
                    镜面反射光强 = 镜面反射系数 * 光强 * pow(反射方向向量 · 观察者方向向量，镜面高光指数)
                ambient：
                    环境光强 = 环境光系数 * 光强

            */
            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                /** N:法向量  L: 入射光向量  V: 观察者方向向量  H: 半程向量*/
                float3 N = normalize(i.normal);
                float3 L = _WorldSpaceLightPos0.xyz; // 对于平行光,这个向量是代表平行光方向的单位向量
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float3 H = normalize(L + V);
                /** diffuse */
                float Lambert = saturate(dot(N, L));
                float3 diffuseLight = Lambert * _LightColor0.xyz;
                /** specular */
                float specularExp = exp2(_SpecularGloss * 11) + 2;
                float3 specularLight = saturate(dot(H, N)) * (Lambert > 0);
                //specularLight = pow(specularLight, specularExp);
                specularLight = pow(specularLight, specularExp) * _SpecularGloss;
                specularLight *=  _LightColor0.xyz;

                return float4(specularLight.xxx, 1) + float4(diffuseLight, 0) * col;
            }
            ENDCG
        }
    }
}
