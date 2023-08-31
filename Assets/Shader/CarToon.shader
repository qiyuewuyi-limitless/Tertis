Shader "Custom/CarToon"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        [HDR]
        _AmbientLight("Ambient",Color) = (0.4,0.4,0.4,1)
        [HDR]
        _SpecularColor("Sepcualr Color",Color) = (0.9,0.9,0.9,1)
        _Gloss("Specular Gloss",Range(0, 1)) = 1
        _FresnelOutline("Fresnel Outline",Range(0,1)) = 0.3
        _FresnelStrength("Fresnel Strength",Range(0,1)) = 0.2

        _CilpRate("CilpRate",Range(0,1)) = 0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent"
            "Queue" = "Transparent"
            "LightMode" = "ForwardBase"
            "PassFlags" = "OnlyDirectional" 
            }

        Pass
        {
            
            Blend SrcAlpha OneMinusSrcAlpha

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
                float3 noraml : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 noraml: TEXCOORD1;
                float4 wPos : TEXCOORD4;
                float3 viewDir : TEXCOORD5;
                LIGHTING_COORDS(2,3)
            };

            float4 _Color;
            float4 _AmbientLight;
            float4 _SpecularColor;
            float _Gloss;
            float _FresnelOutline;
            float _FresnelStrength;
            float _CilpRate;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.noraml = mul(UNITY_MATRIX_M,v.noraml);
                TRANSFER_VERTEX_TO_FRAGMENT(O);
                o.wPos = mul(UNITY_MATRIX_M,v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
                float3 N = normalize(i.noraml);
                float3 V = normalize(_WorldSpaceCameraPos.xyz - i.wPos.xyz);
                float3 H = normalize( L + V );
                float3 baseColor = float3(1,1,1);

                /** 漫反射和环境光 */
                float NdotL = dot(N,L); 
                float diffuseIntensity = smoothstep(0,0.01,NdotL);
                float3  diffuseLight = baseColor * diffuseIntensity;
                diffuseLight *= _LightColor0;
                diffuseLight += _AmbientLight;

                /** 镜面反射 */
                float NdotH = dot(N,H);
                float gloss = exp2(_Gloss * 11) + 2;
                float specularIntensity = saturate(pow(NdotH,gloss)) * (diffuseIntensity > 0); // 物体背面不产生镜面反射
                specularIntensity = smoothstep(0.005,0.01,specularIntensity);
                float3 specularLight = baseColor * specularIntensity;
                specularLight *= _SpecularColor;

                /** 菲涅尔 */
                float NdotV = dot(N,V);
                float fresnelIntensity = (1 - NdotV) * (diffuseIntensity > 0);
                float fresneloutline = 1 - _FresnelOutline;
                fresnelIntensity = smoothstep(fresneloutline - 0.01,fresneloutline + 0.01,fresnelIntensity);
                //fresnelIntensity = step(1 - _FresnelOutline,fresnelIntensity);
                float3 fresnelLight = baseColor * fresnelIntensity * _FresnelStrength;
                //return 1;

                /** 透明效果(脚本控制) */
                float currentValue = 1 - _CilpRate;
                float3 outColor = _Color * diffuseLight + fresnelLight + specularLight;
                return float4(outColor,currentValue);
            }
            ENDCG
        }
    }
}
