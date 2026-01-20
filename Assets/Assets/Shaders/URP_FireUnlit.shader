Shader "Custom/URP_FireUnlit"
{
    Properties
    {
        _MainTex ("Main (RGBA)", 2D) = "white" {}
        _NoiseTex ("Noise (grayscale)", 2D) = "gray" {}
        _ColorA ("Base Color A", Color) = (1,0.6,0.2,1)
        _ColorB ("Base Color B", Color) = (1,0.15,0.05,1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0,1,0,0)
        _NoiseScale ("Noise Scale", Float) = 3
        _QuantizeLevels ("Quantize Levels", Float) = 4
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.2
        _Brightness ("Brightness", Float) = 1.5
        _Softness ("Edge Softness", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _ColorA;
            float4 _ColorB;
            float4 _ScrollSpeed;
            float _NoiseScale;
            float _QuantizeLevels;
            float _Cutoff;
            float _Brightness;
            float _Softness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                return o;
            }

            // Simple quantize function: takes [0,1] value and returns stepped value
            float Quantize(float x, float steps)
            {
                if (steps <= 1) return x;
                return floor(x * steps) / (steps - 1);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                // scrolling UV for noise
                float2 scroll = _ScrollSpeed.xy * _Time.y;

                // sample main texture for base silhouette / gradient
                float2 uvMain = IN.uv;
                float4 baseSample = tex2D(_MainTex, uvMain);

                // sample noise
                float2 uvNoise = uvMain * _NoiseScale + scroll;
                float n = tex2D(_NoiseTex, uvNoise).r;

                // quantize noise for low-poly/blocky look
                float q = Quantize(n, max(1.0, _QuantizeLevels));

                // create vertical mask (stronger at bottom, fade upwards)
                float heightMask = saturate(1.0 - (uvMain.y - 0.0)*1.5);

                // mix colors by noise and height for stylized flame
                float mixT = saturate(q * 1.2 + (1.0 - heightMask) * 0.4);
                float4 col = lerp(_ColorA, _ColorB, mixT);

                // combine with main texture alpha if present (use as shape)
                float shape = baseSample.a;

                // final alpha: shape * quantized noise * height influence
                float alpha = shape * q * heightMask;

                // soften edges a little
                alpha = smoothstep(_Cutoff - _Softness, _Cutoff + _Softness, alpha);

                // apply brightness
                col.rgb *= _Brightness;

                return float4(col.rgb, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
