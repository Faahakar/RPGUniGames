Shader "Custom/ObjectCenter"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Shift ("Shift", Float) = 1.0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
    Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
    Lighting Off
    Blend SrcAlpha OneMinusSrcAlpha
    Cull Off
        Pass
        {
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    float4 _Color;
    sampler2D _MainTex;
    float4 _Fade;
    float _Shift;
    
    struct v2f {
        float4 pos : POSITION;
        float4 color : COLOR0;
        float2  uv : TEXCOORD0;
        float alpha:TEXCOORD1;
        float4 fragPos : TEXCOORD2;
        float4 fade : TEXCOORD3;
    };
    
    float4 _MainTex_ST;
    
    v2f vert (appdata_base v)
    {
        v2f o;
        o.color = _Color;
        o.pos = UnityObjectToClipPos (v.vertex);
        o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
        o.fragPos = mul (UNITY_MATRIX_MV, v.vertex);
        o.fade = mul (UNITY_MATRIX_MV, _Fade);
        return o;
    }
    
    half4 frag (v2f i) : COLOR
    
    {
        float4 outColor = i.color;
        half4 texcol = tex2D (_MainTex, i.uv);
        float dist = distance(i.fade, i.fragPos);
        float4 objectOrigin = unity_ObjectToWorld[3];
        float dist2 = distance(objectOrigin, _WorldSpaceCameraPos) + _Shift;
        if (dist < dist2)
                    {
                        discard;
                    }
            return texcol * outColor;
    }
    ENDCG
        }
    }
    FallBack "VertexLit"
}
