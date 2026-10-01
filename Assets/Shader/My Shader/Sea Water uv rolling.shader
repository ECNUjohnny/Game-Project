Shader "Custom/UVScrollWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.4, 0.8, 0.8)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _ScrollSpeedX ("X axis speed", Range(-2.0, 2.0)) = 0.1
        _ScrollSpeedY ("Y axis speed", Range(-2.0, 2.0)) = 0.1
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _BumpScale ("Wave Strength", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        // 设置为透明队列
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        // 关闭深度写入防止透明排序闪烁，开启Alpha混合
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard alpha:fade fullforwardshadows
        #pragma target 3.0

        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_BumpMap;
        };

        fixed4 _Color;
        float _ScrollSpeedX;
        float _ScrollSpeedY;
        half _Glossiness;
        half _Metallic;
        float _BumpScale;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 1. 根据时间计算 UV 的偏移量
            float2 scrollUV = IN.uv_BumpMap;
            scrollUV.x += _Time.y * _ScrollSpeedX;
            scrollUV.y += _Time.y * _ScrollSpeedY;

            // 2. 基础颜色
            o.Albedo = _Color.rgb;
            
            // 3. 读取法线贴图，并应用偏移后的 UV 和 波纹强度
            float3 normal = UnpackNormal(tex2D(_BumpMap, scrollUV));
            normal.xy *= _BumpScale; // 放大或缩小波纹的凹凸感
            o.Normal = normalize(normal);
            
            // 4. 设置物理质感
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
