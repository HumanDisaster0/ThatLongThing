Shader "Custom/SpriteCircularMaskBlackBase"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskCenter ("Mask Center", Vector) = (0.5, 0.5, 0, 0) // �ؽ�ó ��ǥ (UV) ����
        _MaskRadius ("Mask Radius", Float) = 0.3 // ���� ����ũ ũ��
        _MaskSoftness ("Mask Softness", Float) = 0.1 // ��� �ε巯�� ����
        _AlphaFactor ("Alpha Multiply Factor", Float) = 1.0 // �߰�
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MaskCenter;
            float _MaskRadius;
            float _MaskSoftness;
            float _AlphaFactor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance = length(i.uv - _MaskCenter.xy);

                // ����ũ ���� ���
                float alpha = smoothstep(_MaskRadius, _MaskRadius + _MaskSoftness, distance);

                // ���� ��� + �ؽ�ó ���� ����
                fixed4 baseColor = fixed4(0, 0, 0, alpha * _AlphaFactor);
                return baseColor;
            }
            ENDCG
        }
    }
}