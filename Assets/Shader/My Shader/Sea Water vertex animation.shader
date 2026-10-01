Shader "Unlit/Sea Water"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.4, 0.8, 1.0)
        _WaveAmp ("Wave Amplitude", Range(0.01, 5.0)) = 0.2
        _WaveFreq ("Wave Frequency", Range(0.1, 10)) = 2.0
        _WaveSpeed ("Wave Speed", Range(0.1, 10)) = 1.5
    
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _WaveAmp;
            float _WaveFreq;
            float _WaveSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                
                float time = _Time.y * _WaveSpeed;
                float waveX = sin(v.vertex.x * _WaveFreq + time);
                float waveZ = cos(v.vertex.z * _WaveFreq + time * 0.8);

                v.vertex.y += (waveX + waveZ) * _WaveAmp;

                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
