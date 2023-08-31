Shader "Unlit/shader5"
{
    Properties
    {
        _MainTexture("MainTexture",2D) = "white"{}
        _Health("Health",Range(0,1)) = 1
        _StartColor("StartColor",Color) = (0,1,0,1)
        _EndColor("EndColor",Color) = (1,0,0,1)
        _BgColor("BgColor",Color) = (0,0,0,1)
        _StartThresholds("StartThresholds",float) = 0.8
        _EndThresholds("EndThresholds",float) = 0.2

    }
    SubShader
    {
        /*
        Tags { "RenderType" = "Transparent"
                "Queue" = "Transparent"
         }*/

        Tags {
            "RenderType" = "Opaque"
        }

        Pass
        {
            //公式 ： src * x + dst * y
            //blend SrcAlpha OneMinusSrcAlpha // <=> scr * scrAlphaChannelValue  dst * (1 - scrAlphaChannelValue)
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

            float  _Health;
            float4 _StartColor;
            float4 _EndColor;
            float4 _BgColor;
            float _StartThresholds;
            float _EndThresholds;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            /** 根据插值找到在插值区间的位置*/
            float InverseLerp(float a, float b, float v){
                return (v - a) / ( b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 healthbarCol;
                /**
                if(_Health >= _StartThresholds)
                    healthbarCol = _StartColor;
                else if(_Health <= _EndThresholds)
                    healthbarCol = _EndColor;
                else
                    healthbarCol = lerp(_EndColor,_StartColor,_Health);
                */
                float tHealth = saturate(InverseLerp(0.2,0.8,_Health));
                healthbarCol = lerp(_EndColor,_StartColor,tHealth);
                float mask = _Health > floor(i.uv.x *8) / 8; // 把x轴上的uv坐标分为了8等份
                //float4 finalCol = lerp(float4(healthbarCol.xyz,0),healthbarCol,mask);
                float4 finalCol = lerp(_BgColor,healthbarCol,mask);
                clip(mask-0.5);
                /*clip方式实现透明效果
                    优点: 不需要考虑排序问题,它把被丢弃的片元从渲染管线中剔除
                    缺点：只是简单的渲染/不渲染，无法实现渐入渐出的效果
                */

                return finalCol;
            }
            ENDCG
        }
    }
}
