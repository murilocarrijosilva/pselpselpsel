Shader "Custom/Lamp2D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _OffsetFrom("Value from which the offset will begin", Range(0, 1)) = 1
        _OffsetValue("Offset value", Range(0, 1)) = 1
        _Frequency("Frequency", Float) = 1
    }
        SubShader
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}

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

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                sampler2D _MainTex;
                fixed4 _Color;
                float _OffsetFrom;
                float _OffsetValue;
                float _Frequency;

                fixed4 frag(v2f i) : SV_Target
                {
                    float intensity = _OffsetValue * sin(_Time.y * _Frequency) + _OffsetFrom;
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                    col.a = clamp(intensity * log10(1 / (sqrt(abs(pow(0.5f - i.uv.r, 2) + pow(0.5f - i.uv.g, 2))))), 0, 1);
                    return col;
                }
                ENDCG
            }
        }
}
