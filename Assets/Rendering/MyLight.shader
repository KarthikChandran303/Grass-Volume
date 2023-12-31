Shader "Unlit/MyLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "LightMode"="ForwardBase"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityStandardBRDF.cginc"

            struct Interpolator
            {
                float4 vertex: SV_POSITION;
                float3 normal: TEXCOORD1;
                float2 uv: TEXCOORD0;
                float3 worldPos: TEXCOORD2;
            };

            struct VertexInput
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float2 uv: TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Smoothness;
            
            Interpolator vert (VertexInput v)
            {
                Interpolator i;
                i.vertex = UnityObjectToClipPos(v.vertex);
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.normal = normalize(i.normal.xyz);
                i.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return i;
            }

            float4 frag (Interpolator i) : SV_Target
            {
                i.normal = normalize(i.normal);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float NDotL = DotClamped(lightDir, i.normal);

                float specular = DotClamped(viewDir, reflect(-lightDir, i.normal));
                specular = pow(specular, _Smoothness * 100.0f);
                return specular;
            }
            
            ENDCG
        }
    }
}
