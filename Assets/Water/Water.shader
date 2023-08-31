// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Water"
{
    Properties
    {	
        _WaterShallowColor("WaterShallowColor",Color) = (0.325,0.807,0.971,0.725)
        _WaterDeepColor("WaterDeepColor",Color) = (0.086,0.407,1,0.749)

        _WaveNoise("WaveNoise",2D) = "white"{}
        _WaveNoiseStrength("WaveNoise Strength",Range(0,1)) = 0.777
        _FoamMinDistance("Foam MinDistance",float) = 0.04
        _FoamMaxDistance("Foam MaxDistance",float) = 0.4
    }
    SubShader
    {
        Pass
        {
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            /* 全局属性 */
            sampler2D _CameraDepthTexture;
            sampler2D _CameraNormalsTexture;
            float4 _WaterDeepColor;
            float4 _WaterShallowColor;
            
            /* 取样器和偏移 */
            sampler2D _WaveNoise;
            float4 _WaveNoise_ST;

            float _WaveNoiseStrength;
            float _DistortionStrength;
            float _FoamMinDistance;
            float _FoamMaxDistance;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 clipSpacePos : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float2 noiseUV : TEXCOORD2;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.clipSpacePos = ComputeScreenPos(o.vertex);
                //o.clipSpacePos = normalize(mul(UNITY_MATRIX_MV, v.vertex));
                o.normal = mul(UNITY_MATRIX_MV,v.vertex);
                
                o.noiseUV = TRANSFORM_TEX(v.uv,_WaveNoise);
                return o;
            }
            float InverseLerp(float a, float b,float v){
                return (v - a) / (b - a);
            }
            float4 frag (v2f i) : SV_Target
            {   
                /** 比较渲染纹理法线与水面法线*/
                float3 normalBuffer = tex2Dproj(_CameraNormalsTexture,i.clipSpacePos).rgb;
                float3 normal = mul(UNITY_MATRIX_V,float3(0,1,0));
                float dotRs = saturate(dot(normalBuffer,normal));
                //return dotRs;
                /** 计算水深 */
                float depthBuffer = tex2Dproj(_CameraDepthTexture,i.clipSpacePos).r;
                float linearDepth = LinearEyeDepth(depthBuffer);
                float waterHeight = saturate(linearDepth - i.vertex.w);
                float4 waterColor = lerp(_WaterShallowColor,_WaterDeepColor,waterHeight);
                //return  waterColor;
                /** 噪点采样 */
                float2 noiseUV = i.noiseUV + _Time.y * 0.04;
                float2 noiseSample = tex2D(_WaveNoise,noiseUV);

                /** 水面效果 */
                float foamDistance = lerp(_FoamMinDistance,_FoamMaxDistance,1 - dotRs);
                //float foam = saturate(InverseLerp(0,_FoamMinDistance,waterHeight));
                float foam = saturate(InverseLerp(0,foamDistance,waterHeight));
                float noiseRange =  _WaveNoiseStrength * foam;
                float effect = noiseSample > noiseRange ? 1 : 0;
                return  waterColor + effect;
            }
            ENDCG
        }
    }
}