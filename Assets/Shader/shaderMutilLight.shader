Shader "Unlit/shaderMutilLight"
{
    Properties
    {
        [NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" { }
        [NoScaleOffset]
        _RockAlbdo("Rock Albdo",2D) = "white"{ }
        [NoScaleOffset]
        _NormalMap("Normal Texture",2D) = "bump"{ }
        _NormalIntensity("Normal Intensity",Range(0,1)) = 1
        [NoScaleOffset]
        _HeightMap("Height Texture",2D) = "gray"{}
        _DispStrength("Displacement Strength",Range(0,0.2)) = 0
        _SpecularGloss ("SpecularGloss", Range(0, 1)) = 1
        _AmbientLight("Ambient Light",Color) = (0,0,0,0)
        _MainColor("MainColor",color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #define IS_IN_BASE_PASS
            #define USE_LIGHTING
            #pragma vertex vert
            #pragma fragment frag
            #include "FGshader.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            CGPROGRAM
            #define USE_LIGHTING
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_ compile_fwdadd
            #include "FGshader.cginc"
            ENDCG
        }
        
    }
}
