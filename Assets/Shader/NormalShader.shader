Shader "Unlit/NormalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Normals("Normal Texuture",2D) = "bump"{}
        _NormalIntensity("Normal Intensity",Range(0,1)) = 1
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
                float3 normal : NORMAL;
                float4 tangent: TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float3 tangent:TEXCOORD2;
                float3 bi_tangent:TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Normals;
            float _NormalIntensity;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);               
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.tangent  = v.tangent.xyz;
                o.bi_tangent = cross(o.normal,o.tangent) * (v.tangent.w *unity_WorldTransformParams.w);
                return o;
            }

            float3 translateSpaceFromTangentToWorld(v2f o){
                float3 tangentSpaceNormal = UnpackNormal(tex2D(_Normals,o.uv)).rgb;
                //float3 tangentSpaceNormal = tex2D(_Normals,o.uv).rgb;
                tangentSpaceNormal = normalize(lerp(float3(0,0,1),tangentSpaceNormal,_NormalIntensity));
                float3x3 translateMatrix = {
                    o.tangent.x,o.bi_tangent.x,o.normal.x,
                    o.tangent.y,o.bi_tangent.y,o.normal.y,
                    o.tangent.z,o.bi_tangent.z,o.normal.z
                };
                float3 N = mul(translateMatrix,tangentSpaceNormal);
                return N;
            }

            float diffuse(float3 N,float3 L){
                return saturate(dot(N,L));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 N = translateSpaceFromTangentToWorld(i);
                float diffuseLight = diffuse(N,_WorldSpaceLightPos0.xyz);
                return col * diffuseLight;
            }
            ENDCG
        }
    }
}
