Shader "Custom/PageMaterial"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200
        //Cull Off
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows  vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        

        struct appdata
        {
            float4 vertex : SV_POSITION;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;

            uint id : SV_VertexID;
            uint inst : SV_InstanceID;
        };

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

         struct Vertex
        {
            float3 pos;
            float3 norm;
            float2 uv;
        };
        #ifdef SHADER_API_D3D11            
            StructuredBuffer<Vertex> verts;
        #endif

        // // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // // #pragma instancing_options assumeuniformscaling
        // UNITY_INSTANCING_BUFFER_START(Props)
        //     // put more per-instance properties here
        // UNITY_INSTANCING_BUFFER_END(Props)

       

        void vert (inout appdata v)
        {
        #ifdef SHADER_API_D3D11                
            v.vertex = float4(verts[v.id].pos, 0);
            v.normal = float4(verts[v.id].pos, 1);
            v.texcoord = verts[v.id].uv;
        #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
