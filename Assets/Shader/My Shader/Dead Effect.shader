Shader "MyShader/Dead Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Blend ("Grayscale Blend", Range(0, 1)) = 0

        _Brightness ("Brightness", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Blend;
            float _Brightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 uvFromCenter = i.uv - float2(0.5, 0.5);

                float dis = length(uvFromCenter);

                float vignette = smoothstep(0.5, 0.1, dis);

                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));

                col.rgb = lerp(col.rgb, float3(gray, gray, gray), _Blend);

                col.rgb = lerp(col.rgb, col.rgb * 0.05f, _Brightness * (1.0 - vignette));

                return col;
            }
            ENDCG
        }
    }
}
