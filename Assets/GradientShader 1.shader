Shader "Custom/GradientShader 1"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,0,0,1)
        _BottomColor ("Bottom Color", Color) = (0,0,1,1)
        _NoiseScale ("Noise Scale", Float) = 10.0
        _TimeScale ("Time Scale", Float) = 1.0
        _CustomTime ("Custom Time", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _NoiseScale;
            float _TimeScale;
            float _CustomTime;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Simple noise function
            float noise(float2 uv) {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Function to convert RGB to HSV
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            // Function to convert HSV to RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            // Function to calculate middle color
            fixed4 calculateMiddleColor(fixed4 bottomColor, fixed4 topColor)
            {
                float3 bottomHSV = rgb2hsv(bottomColor.rgb);
                float3 topHSV = rgb2hsv(topColor.rgb);
                
                // Average the hue
                float middleHue = (bottomHSV.x + topHSV.x) * 0.5;
                
                // Use the higher saturation and value
                float middleSaturation = max(bottomHSV.y, topHSV.y);
                float middleValue = max(bottomHSV.z, topHSV.z);
                
                float3 middleHSV = float3(middleHue, middleSaturation, middleValue);
                float3 middleRGB = hsv2rgb(middleHSV);
                
                return fixed4(middleRGB, 1.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 noiseUV = i.uv * _NoiseScale + _CustomTime * _TimeScale;
                float noiseValue = noise(noiseUV) * 0.2 - 0.1; // Range: -0.1 to 0.1
                float lerpFactor = i.uv.y + noiseValue;
                
                fixed4 middleColor = calculateMiddleColor(_BottomColor, _TopColor);
                
                fixed4 color;
                if (lerpFactor < 0.5) {
                    color = lerp(_BottomColor, middleColor, lerpFactor * 2);
                } else {
                    color = lerp(middleColor, _TopColor, (lerpFactor - 0.5) * 2);
                }
                
                return color;
            }
            ENDCG
        }
    }
}