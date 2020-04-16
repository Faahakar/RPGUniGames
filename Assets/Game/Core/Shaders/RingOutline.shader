Shader "Unlit/RingOutline"
{
    Properties
    {
        _RingColor ( "Ring Color", Color) = (1,1,1,1) 
        _MaskRadius ("Mask Radius", Float) = 0.5 
        _MaskFalloff ("Mask Falloff", Float) = 0.5 
        _RingSize ("Ring Size", Float) = 0.5 
        _ClipValue ("Clip Value", Float) = 0.5 
    }
    SubShader
    {
        Pass {        
                ZWrite Off
                ZTest Greater
                Cull Front
                Name "Behind"
                Blend SrcAlpha OneMinusSrcAlpha
                LOD 200                    
               
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 vertWorldPos : TEXCOORD2;
                float4 objectWorld : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _RingColor;

            float _MaskRadius;
            float _MaskFalloff;
            float _RingSize;
            float _ClipValue;

            float CircleMask(float3 worldPos, float3 objectPos) {
                float mask = distance(worldPos, objectPos) / _MaskRadius;
                mask = pow(abs(mask), _MaskFalloff);
                return mask;
            }

            float RingMask(float mask) {

                float ring = distance(mask, 0.5) / _RingSize;
                ring = 1 - smoothstep(0, 1, ring);
                return ring;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertWorldPos = mul(unity_ObjectToWorld, v.vertex);
                o.objectWorld = mul(unity_ObjectToWorld, float4(0,0,0,1));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float value = RingMask(CircleMask(i.vertWorldPos, i.objectWorld));
                float alpha = smoothstep(0.5,0.9,value);
                
                return fixed4(value * _RingColor.rgb,alpha);
            }
            ENDCG
            }
      
        Pass
        {               
            ZTest LEqual                // this checks for depth of the pixel being less than or equal to the shader
            ZWrite Off                  // and if the depth is ok, it renders the main texture.
            Cull Back
            LOD 200
            Blend SrcAlpha OneMinusSrcAlpha  
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 vertWorldPos : TEXCOORD2;
                float4 objectWorld : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _RingColor;

            float _MaskRadius;
            float _MaskFalloff;
            float _RingSize;
            float _ClipValue;

            float CircleMask(float3 worldPos, float3 objectPos) {
                float mask = distance(worldPos, objectPos) / _MaskRadius;
                mask = pow(abs(mask), _MaskFalloff);
                return mask;
            }

            float RingMask(float mask) {

                float ring = distance(mask, 0.5) / _RingSize;
                ring = 1 - smoothstep(0, 1, ring);
                return ring;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertWorldPos = mul(unity_ObjectToWorld, v.vertex);
                o.objectWorld = mul(unity_ObjectToWorld, float4(0,0,0,1));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float value = RingMask(CircleMask(i.vertWorldPos, i.objectWorld));
                float alpha = smoothstep(0.5,0.9,value);
                
                return fixed4(value * _RingColor.rgb,alpha);
            }
            ENDCG
        }
        
        
    }
}
