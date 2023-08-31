Shader "Roystan/Toon/Water"
{
    Properties
    {	
        _DepthGradientShallow("DepthGradientShallow",Color) = (0.325,0.807,0.971,0.725)
        _DepthGradientDeep("DepthGradientDeep",Color) = (0.086,0.407,1,0.749)
        _DepthMaxDistance("DepthMaxDistance",Float) = 1
        _SurfaceNoise("SurfaceNoise",2D) =  "white"{}
        _SurfaceNoiseCutOff("SurfaceNoise",Range(0,1)) = 0.777
        _FoamMinDistance("Foam MinDistance",Float) = 0.04
        _FoamMaxDistance("Foam MaxDistance",Float) = 0.4
        _SurfaceNoiseScroll("Surface Noise Scroll Amount",vector) = (0.03,0.03,0,0)
        _SurfaceDistortion("Surface Distorion",2D) = "white"{}
        _SurfaceDistortionAmount("Surface Distortion Amount",Range(0,1)) = 0.27
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 noiseUV : TEXCOORD0;
                float2 distorUV: TEXCOORD1;
                float4 screenPosition:TEXCOORD2;
                float3 viewNormal: NORMAL;
                
            };
            
            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            float2 _SurfaceNoiseScroll;
            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;
            float _SurfaceDistortionAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                o.noiseUV = TRANSFORM_TEX(v.uv,_SurfaceNoise);
                o.distorUV = TRANSFORM_TEX(v.uv,_SurfaceDistortion);
                o.viewNormal =  COMPUTE_VIEW_NORMAL;
                return o;
            }
            
            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;
            float _SurfaceNoiseCutOff;
            float _FoamMinDistance;
            float _FoamMaxDistance;

            float InverseLerp(float a,float b, float v){
                return  (v - a) / (b - a);
            }

            float4 frag (v2f i) : SV_Target
            {
                /** 获取屏幕空间法线 */
                float3 screenNormalBuffer = tex2Dproj(_CameraNormalsTexture,UNITY_PROJ_COORD(i.screenPosition)).xyz;
                float3 screenNormal = i.viewNormal;
                float angleCos = 1 - saturate(dot(screenNormal,screenNormalBuffer));
                float foamDistance = lerp(_FoamMinDistance,_FoamMaxDistance,angleCos);
                /** 计算水深 */
                float existingDepth01 = tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.screenPosition)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth01);
                float depthDifference = existingDepthLinear - i.screenPosition.w;// 与深度缓冲区内的值做差，得到水体得高度/深度
                float waterDepthDifference = saturate(InverseLerp(0,_DepthMaxDistance,depthDifference));
                float4 col = lerp(_DepthGradientShallow,_DepthGradientDeep,waterDepthDifference);
                
                float2 distortSample = (tex2D(_SurfaceDistortion,i.distorUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV  = i.noiseUV + _SurfaceNoiseScroll.xy * _Time.y + distortSample;

                /** 计算噪声 */
                float surfaceNoiseSample = tex2D(_SurfaceNoise,noiseUV); // 采样，噪声有灰度值
                float foamDepthDifference01 = saturate(InverseLerp(0,foamDistance,depthDifference));
                //float foamDepthDifference01 = saturate(InverseLerp(0,_FoamDistance,depthDifference));
                float surfaceNoiseCutOff = foamDepthDifference01 * _SurfaceNoiseCutOff;
                float surfaceNoise =  surfaceNoiseSample > surfaceNoiseCutOff  ? 1 : 0;
                return col + surfaceNoise;
				return float4(1, 1, 1, 0.5);
            }
            ENDCG
        }
    }
}