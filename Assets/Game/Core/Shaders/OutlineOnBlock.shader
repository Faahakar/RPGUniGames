Shader "Custom/OutlineOnBlock"
{
	Properties {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _RimCol ("Rim Colour" , Color) = (1,0,0,1)
        _RimPow ("Rim Power", Float) = 1.0     
        _Distort("Distort", vector) = (0.5, 0.5, 1.0, 1.0)
		_OuterRadius ("Outer Radius", float) = 0.5
		_InnerRadius ("Inner Radius", float) = -0.5
		_Hardness("Hardness", float) = 1.0
      
    }
    SubShader {
        
            Pass {
                Name "Regular"
                Tags { "RenderType"="Opaque" }
                ZTest LEqual                // this checks for depth of the pixel being less than or equal to the shader
                ZWrite On                   // and if the depth is ok, it renders the main texture.
                Cull Back
                LOD 200
               
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
               
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };
               
                sampler2D _MainTex;
                float4 _MainTex_ST;
               
                v2f vert (appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }
               
                half4 frag (v2f i) : COLOR
                {
                    half4 texcol = tex2D(_MainTex,i.uv);
                    return texcol;
                }
                ENDCG
            }        



            CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows 
        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			return fixed4(s.Albedo, s.Alpha);
		}
 
                // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
 
        sampler2D _MainTex;
 
    struct Input 
    {
        float2 uv_MainTex;
    };
 
    half _Glossiness;
    half _Metallic;
    half _DissolvePercentage;
    half _ShowTexture;
    fixed4 _Color;
    float4 _Distort;
    float _OuterRadius, _InnerRadius, _Hardness;

 
    void surf(Input IN, inout SurfaceOutputStandard o)
    {       
        // Albedo comes from a texture tinted by color
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
 
			float x = length((_Distort.xy - IN.uv_MainTex.xy) * _Distort.zw);
 
			float rc = (_OuterRadius + _InnerRadius) * 0.5f; // "central" radius
			float rd = _OuterRadius - rc; // distance from "central" radius to edge radii
 
			float circleTest = saturate(abs(x - rc) / rd);
 
			o.Albedo = _Color.rgb * c.rgb;
			o.Alpha = (1.0f - pow(circleTest, _Hardness)) * _Color.a * c.a;
    }
    ENDCG  
        }
    FallBack "VertexLit"
}

