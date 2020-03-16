Shader "Custom/LitCameraDissolve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _NoiseScale ("Noise Scale", Float) = 2
        _MaskRadius ("Mask Radius", Float) = 0.5 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float3 worldPos;
            float4 screenPos;

            float3 worldN;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _NoiseScale;
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

        void vert(inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.worldN = UnityObjectToWorldNormal(v.normal);
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            float3 triPlanarBlend = pow(abs(IN.worldN),1);
            triPlanarBlend /= dot(triPlanarBlend, 1.0);

            float distCam = distance(IN.worldPos, _WorldSpaceCameraPos.xyz) / _MaskRadius;

            float3 noiseValues = 0;
            Unity_GradientNoise_float(IN.worldPos.zy, _NoiseScale, noiseValues.x);
            Unity_GradientNoise_float(IN.worldPos.xz, _NoiseScale, noiseValues.y);
            Unity_GradientNoise_float(IN.worldPos.xy, _NoiseScale, noiseValues.z);
            noiseValues = saturate(noiseValues);
            
            clip(distCam - noiseValues * triPlanarBlend - 0.5);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
