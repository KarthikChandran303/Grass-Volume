Shader "Unlit/ShellTexture"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SecondaryColor ("SecondaryColor", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _WindTex ("WindTexture", 2D) = "white" {}
        _ParticleTexture ("ParticleTexture", 2D) = "white" {}
        _ScrollSpeed ("ScrollSpeed", Float) = 0.2
        _WindStrength ("WindStrength", Float) = 0.2
        _OrthographicCamSize ("OrthographicCamSize", Float) = 15
        _Position ("Position", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _SecondaryColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _WindTex;
            float4 _WindTex_ST;
            sampler2D _ParticleTexture;
            float4 _ParticleTexture_ST;
            float _ScrollSpeed;
            float _WindStrength;
            float _OrthographicCamSize;
            float3 _Position;

            struct DrawVertex
            {
                float3 position;
                float3 normal;
                float2 uv;
                float4 vertexColor;
            };

            struct DrawTriangle
            {
                DrawVertex drawVertices[3];
            };
            
            StructuredBuffer<DrawTriangle> _DrawTrianglesBuffer;

            v2f vert (uint vertexID : SV_VertexID)
            {
                v2f o;
                DrawTriangle tri = _DrawTrianglesBuffer[vertexID / 3];
                DrawVertex v = tri.drawVertices[vertexID % 3];

                o.worldPos = mul(unity_ObjectToWorld, float4(v.position, 1));
                o.vertex = UnityObjectToClipPos(v.position);
                o.uv = v.uv * _MainTex_ST.xy;;
                o.color = v.vertexColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                
                float wind = tex2D(_WindTex, i.uv + float2(0, _Time.y * _ScrollSpeed)).x;
                wind = wind * 2 - 1;
                float windCol = smoothstep(0.1, 0.5, wind);

                float tex = tex2D(_MainTex, i.uv + float2(0, wind * _WindStrength * (i.color.x * i.color.x))).x;

                float2 particleuv = i.worldPos.xz - _Position.xz;
                
                particleuv = particleuv/ (_OrthographicCamSize * 2);
                particleuv += 0.5;

                float square = step(particleuv.x, 1.0) * step(particleuv.y, 1.0) * step(0.0, particleuv.x) * step(0.0, particleuv.y);

                float particle = tex2D(_ParticleTexture, particleuv).b;
                
                particle = particle * square;
                
                clip(tex - i.color.x - (particle * 0.35));

                float ratio = smoothstep(tex - 0.15, tex, i.color.x) * windCol;
                float4 col = lerp(_SecondaryColor, _Color, i.color.x);
                float4 finalCol = (col) + (ratio * 0.065);

                return finalCol;
            }
            ENDCG
        }
    }
}
