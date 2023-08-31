#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#define PI 3.1415926

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float4 tangent : TANGENT; //w向量,表示切线符号;uv反转时，targent space也需要反转

};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float3 normal : TEXCOORD1;
    float3 tangent : TEXCOORD2;
    float3 biTangent : TEXCOORD3;
    float3 wPos : TEXCOORD4;
    LIGHTING_COORDS(5, 6)
};

sampler2D _MainTex;
sampler2D _RockAlbdo;
sampler2D _NormalMap;
sampler2D _HeightMap;
float _SpecularGloss;
float4 _MainColor;
float _NormalIntensity;
float _DispStrength;
float4 _AmbientLight;

float2 Rotate(float2 v, float angRad)
{
    float ca = cos(angRad);
    float sa = sin(angRad);
    return float2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
}
v2f vert(appdata v)
{
    v2f o;

    float height = tex2Dlod(_HeightMap, float4(v.uv, 0, 0)).x * 2 - 1;
    v.vertex.xyz += v.normal * (height * _DispStrength);
    o.uv = v.uv;
    //o.uv = Rotate(o.uv,_Time.y * PI * 0.1);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.biTangent = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);
    TRANSFER_VERTEX_TO_FRAGMENT(o);
    return o;
}

/** 把法线坐标从切线空间转化到世界空间*/
float3 ObjectToWorldNormalFromTangentSpace(v2f o)
{
    
    float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalMap, o.uv));
    tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _NormalIntensity));
    float3x3 matrixTan = {
        o.tangent.x, o.biTangent.x, o.normal.x,
        o.tangent.y, o.biTangent.y, o.normal.y,
        o.tangent.z, o.biTangent.z, o.normal.z
    };
    float3 N = mul(matrixTan, tangentSpaceNormal);
    return N;
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
    float3 rock = tex2D(_RockAlbdo, i.uv).rgb;
    //float4 col = tex2D(_MainTex, i.uv);
    float4 surfaceCol = float4(rock, 1) * _MainColor;

    //float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalMap,i.uv));
    #ifdef USE_LIGHTING // 如果定义
        /** N:法向量  L: 入射光向量  V: 观察者方向向量  H: 半程向量*/
        //float3 N = lerp(float3(0,1 ,0),ObjectToWorldNormalFromTangentSpace(i),_NormalIntensity);
        float3 N = ObjectToWorldNormalFromTangentSpace(i);
        //float3 N = normalize(i.normal);
        float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
        float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
        float3 H = normalize(L + V);

        float attenuation = LIGHT_ATTENUATION(i);
        /** diffuse */
        float Lambert = saturate(dot(N, L));
        float3 diffuseLight = (Lambert * attenuation) * _LightColor0.xyz;
        #ifdef IS_IN_BASE_PASS
            diffuseLight += _AmbientLight;
        #endif
        /** specular */
        float specularExp = exp2(_SpecularGloss * 11) + 2;
        float3 specularLight = saturate(dot(H, N)) * (Lambert > 0);
        //specularLight = pow(specularLight, specularExp);
        specularLight = pow(specularLight, specularExp) * _SpecularGloss * attenuation;
        specularLight *= _LightColor0.xyz;
        return float4(specularLight.xxx, 1) + float4(diffuseLight, 0) * surfaceCol;
    #else
        #ifdef IS_IN_BASE_PASS
            return surfaceCol;
        #else
            return float4(1, 0, 1, 1);
        #endif
    #endif
}