Shader "MyShader/DeadEyeFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanLine ("Scan Progress", Range(0, 1)) = 0.0
        _FilterColor ("Filter Color", Color) = (0.8, 0.6, 0.2, 1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            sampler2D _MainTex;
            float4 _FilterColor;
            float _ScanLine;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 originalCol = tex2D(_MainTex, i.uv);

                float luminance = dot(originalCol.rgb, float3(0.299, 0.587, 0.114));

                fixed3 specialCol = luminance * _FilterColor.rgb;

                if (i.uv.x < _ScanLine)
                {
                    return fixed4(specialCol, 1.0);
                }
                else
                {
                    return originalCol;
                }
            }
            ENDCG
        }
    }
}
