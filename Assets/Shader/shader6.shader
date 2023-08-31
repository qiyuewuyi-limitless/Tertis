Shader "Unlit/shader6"
{
    Properties
    {
        [NoScaleOffset]
        _MainTexture("MainTexture",2D) = "white"{}
        [NoScaleOffset]
        _BorderTexture("BorderTexture",2D) = "Red"{}
        _BorderSize("BorderSize",Range(0,0.5)) = 0.2
        _Health("Health",Range(0,1)) = 1

    }
    SubShader
    {


        Tags {
            "RenderType" = "Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            sampler2D _MainTexture;
            sampler2D _BorderTexture;
            float _BorderSize;
            float  _Health;

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


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // 原始值
                return o;
            }

            float InverseLerp(float a, float b, float v){
                return (v - a) / ( b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 coords = i.uv;
                coords.x *= 10;
                float2 pointOnLineSeg = float2(clamp(coords.x,0.5,9.5),0.5);
                float dist = distance(pointOnLineSeg,coords) - 0.5;
                //clip(-dist);
                float borderSdf = dist + _BorderSize;
                float pd = fwidth(borderSdf); // screen space partial derivativer
                float borderMask = saturate(borderSdf / pd); // 变化率比较小时使用borderSdf值，变化率比较大时降低borderSdf的值
                //borderSdf = step(0,borderSdf);
                //float4 borderCol = tex2D(_BorderTexture,i.uv);
                // i是原始值o的插值,不是原始值
                float mask = _Health > i.uv.x; 
                
                float4 col = tex2D(_MainTexture,float2(_Health,i.uv.y));
                // if语句在一个渲染流程中拥有几乎相同的执行流程时,不会造成额外的性能开销
                if(_Health < 0.2){
                    float flash = cos(_Time.y * 4) * 0.4 + 1;
                    col *= flash;
                }
                //return col * mask;
                // return col * mask * borderSdf * 20;
                return col * mask * (1-step(0,borderSdf)); // 0-1的跳变,明显锯齿
                //return col * mask * borderMask; //用梯度区分了0-1的渐变,改善了锯齿
                //return col * mask * (1- borderSdf) + borderSdf * borderCol;
            }
            ENDCG
        }
    }
}
