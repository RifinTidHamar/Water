Shader "Unlit/transCol2Tex"
{
   Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _ColorTex ("_ColorTex", 2D) = "white" {}
        _Color ("_Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "QUEUE" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

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
            float4 _MainTex_ST;
            
            sampler2D _ColorTex;
            float4 _ColorTex_ST;

            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                            // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col += tex2D(_ColorTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
