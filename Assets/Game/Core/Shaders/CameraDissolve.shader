Shader "Unlit/CameraDissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Color", Color) = (1,1,1,1)
        _NoiseScale ("Noise Scale", Float) = 2
        _MaskRadius ("Mask Radius", Float) = 0.5 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            fixed4 _MainColor;
            float _NoiseScale;
            float _DissolveValue;
            float _MaskRadius;

            
            //Copied from Unity's Shadergraph procedural gradient noise node
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            //Copied from Unity's Shadergraph procedural gradient noise node
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 triPlanarBlend = pow(abs(i.worldNormal),1);
                triPlanarBlend /= dot(triPlanarBlend, 1.0);

                float distCam = distance(i.worldPos, _WorldSpaceCameraPos.xyz) / _MaskRadius;

                float3 noiseValues = 0;
                Unity_GradientNoise_float(i.worldPos.zy, _NoiseScale, noiseValues.x);
                Unity_GradientNoise_float(i.worldPos.xz, _NoiseScale, noiseValues.y);
                Unity_GradientNoise_float(i.worldPos.xy, _NoiseScale, noiseValues.z);
                noiseValues = saturate(noiseValues);
                
                clip(distCam - noiseValues * triPlanarBlend - 0.5);

				fixed4 tex = tex2D(_MainTex, i.uv) * _MainColor;

                return tex;
            }
            ENDCG
        }
    }
}
