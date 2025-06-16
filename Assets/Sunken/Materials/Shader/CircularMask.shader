Shader "Custom/SpriteCircularMaskBlackBase"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {} // 기본 스프라이트 텍스처
        _MaskCenter ("Mask Center", Vector) = (0.5, 0.5, 0, 0) // 마스크 중심 위치 (UV 기준)
        _MaskRadius ("Mask Radius", Float) = 0.3 // 마스크 반지름
        _MaskSoftness ("Mask Softness", Float) = 0.1 // 마스크 경계 부드러움 정도
        _AlphaFactor ("Alpha Multiply Factor", Float) = 1.0 // 알파 값 추가 조절용
        _MaskColor ("Mask Color", Color) = (0, 0, 0, 1) // 마스크 색상 (RGB)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" } // 투명 렌더링 큐에 배치
        ZWrite Off                       // 깊이 쓰기 끔
        Blend SrcAlpha OneMinusSrcAlpha // 일반적인 알파 블렌딩

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // ===== 유니티 전달 변수 =====
            sampler2D _MainTex;
            float4 _MaskCenter;
            float _MaskRadius;
            float _MaskSoftness;
            float _AlphaFactor;
            fixed4 _MaskColor;    // 마스크 색상 (C#에서 동적으로 변경 가능)

            // ===== 버텍스 입력 구조체 =====
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // ===== 버텍스 → 프래그먼트 전달 구조체 =====
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            // ===== 버텍스 셰이더 =====
            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // 화면 공간으로 변환
                o.uv = v.uv;                            // 텍스처 좌표 그대로 전달
                return o;
            }

            // ===== 프래그먼트 셰이더 =====
            fixed4 frag (v2f i) : SV_Target
            {
                // 마스크 중심과의 거리 계산 (UV 공간)
                float distance = length(i.uv - _MaskCenter.xy);

                // 부드러운 원형 마스크 알파 계산
                float alpha = smoothstep(_MaskRadius, _MaskRadius + _MaskSoftness, distance);

                // 색상 지정 + 알파 조절
               fixed4 baseColor = _MaskColor;  
                baseColor.a *= alpha * _AlphaFactor;

                return baseColor;
            }
            ENDCG
        }
    }
}
