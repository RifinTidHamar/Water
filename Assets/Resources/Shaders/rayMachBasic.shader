#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced '_WorldSpaceCameraPos.w' with '1.0'

Shader "Unlit/rayMarchBasic"
{
    Properties
    {
        _MainTex("3D tex", 3D) = "white"{}
        _StepSize("Step size", Range(0.0, 0.05)) = 0.5
        //_Amnt("Amount", Range(-1, 1)) = 1
        _TestX("test-x", Range(-2, 2)) = 1
        _TestY("test-y", Range(-2, 2)) = 1
        _TestZ("test-z", Range(-2, 2)) = 1
        _TestW("test-w", Range(-2, 2)) = 1

        _Alpha("alpha", Range(0, 1)) = 0.5
        _Color("Color", Color) = (1,0,0,0)
    }
        SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
            Blend SrcAlpha OneMinusSrcAlpha
            //Cull Off ZWrite Off ZTest Off

            
            Pass
            {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "UnityLightingCommon.cginc" // for _LightColor0

                // Maximum amount of raymarching samples
                #define MAX_STEP_COUNT 75
                #define EPSILON 0.00001f

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 objectVertex : TEXCOORD1;
                    float3 rayDirection : TEXCOORD2;
                    float4 screenSpace : TEXCOORD3;
                };

                sampler3D _MainTex;
                sampler2D _CameraDepthTexture;
                float _StepSize;
                float _TestX;
                float _TestY;
                float _TestZ;
                float _TestW;

                float _Amnt;
                float _Alpha;
                float4 _Color;
            

                v2f vert(appdata v)
                {
                    v2f o;
                    // Vertex in object space this will be the starting point of raymarching
                    //v.vertex.y *= 5;
                    o.objectVertex = v.vertex.xyz; // mul(unity_ObjectToWorld, v.vertex);// +float3(0, 0, 0.5);
                    //o.objectVertex.y /= 256;
                    //float4 newVert = float4(v.vertex.x, o.objectVertex.y, v.vertex.zw);
                    // Calculate vector from camera to vertex in world space
                    float3 worldVertex = mul(unity_ObjectToWorld, v.vertex);// * 30 + float3(0,5,0); //*2 will need to be change to whatever object scale is
                    //o.objectVertex = worldVertex;
                    o.rayDirection = mul(unity_WorldToObject, worldVertex - _WorldSpaceCameraPos);
                    //o.rayDirection.y /= 5;
                    o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1));

                    o.screenSpace = ComputeScreenPos(o.vertex);
                    return o;
                }

                float ComputeDepth(float3 objPos) 
                {
                    float4 viewPos = mul(UNITY_MATRIX_MV, float4(objPos,1));
                    float depth = length(viewPos.xyz);
                    return depth;
                }



                fixed4 frag(v2f i) : SV_Target
                {
                    float2 screenSpaceUV = i.screenSpace.xy / i.screenSpace.w;
                    float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenSpaceUV));

                    float4 backCol = float4(0, 0, 0, 0);
                    float3 rayOrigin = i.objectVertex + 0.5;

                    float3 unalteredRay = i.objectVertex;//mul(unity_ObjectToWorld, float4(i.objectVertex,1));
                    float3 worldRayDirection = normalize(i.rayDirection);
                    for (int x = 0; x < MAX_STEP_COUNT; x++)
                    {
                        float4 frontCol = tex3D(_MainTex, rayOrigin);
                        if(ComputeDepth(unalteredRay) > depth)
                            frontCol.a = 0;

                        //blends the color at each step to porperly look through the object as a volume
                        float val = step(0, rayOrigin.y) * step(rayOrigin.y, 1) * step(0, rayOrigin.z) * step(0, rayOrigin.x) * step(rayOrigin.z, 1) * step(rayOrigin.x, 1)/* * smoothstep(0, 1, (1-rayOrigin.z)*5) * smoothstep(0, 1, rayOrigin.z*5) */;
                        
                        backCol.rgb += (1.0 - backCol.a) * frontCol.a * frontCol.rgb * _Alpha * val;
                        backCol.a += (1.0 - backCol.a) * frontCol.a *  _Alpha * val;
                     
                        rayOrigin += _StepSize * worldRayDirection;
                        unalteredRay += _StepSize * worldRayDirection;
                    }
                    backCol.rgb *= _Color.rgb;

                    float4 newCol = backCol;

                    newCol.a = (newCol.a - 0.52) * 10 + 0.2;

                    //newCol.a *= 1 - pow((abs(worldRayDirection.z) * 2),5);
                    //newCol.a *= i.objectVertex.z;
  
                    //newCol.rbg = (newCol.rgb - 0.5) * 2 + 0.5;
                    //float fog = depth/200;
                    //newCol = float4(fog, fog, fog,1);
                    newCol = round(newCol * 30)/30;


                    return newCol;
                    //return float4(depth, 0, 0, 1);
                }
                ENDCG
            }
        }
}