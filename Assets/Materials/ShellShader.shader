Shader "Custom/ShellShader"
{
    Properties
    {
        _Noise ("Noise (R)", 2D) = "white" {}
        _Height ("Height", Float) = 1
        _Grass ("Top Color", Color) = (1,1,1,1)
        _LowerGrass ("Bottom Color", Color) = (1,1,1,1)
        _Quality ("Quality", Range(0.0000001,1)) = 1
        _Depth ("Depth", Int) = 250
        _Scale ("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            Cull back
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0
            #pragma require geometry

            //tell the compiler which function handles the vertex shader
            #pragma vertex vert
            //tell the compiler which function handles the geometry shader
            #pragma geometry geom
            //tell the compiler which function handles the fragment shader
            #pragma fragment frag

            #include "UnityCG.cginc"

            //declare the vertex input structure
            struct appdata
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            //declare the vertex output structure this is what is passed to the geometry shader
            struct v2g
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };
            //declare the geometry output structure this is what is passed to the fragment shader
            struct g2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 p[6] : TEXCOORD2;
                float2 uvVertex[3] : TEXCOORD8;
                #ifdef _DEBUG
                    int wallIndex : TEXCOORD11;
                #endif
            };

            //redefine inputs for the shader
            sampler2D _Noise;
            float4 _Noise_ST;
            float _Height;
            float4 _Grass;
            float4 _LowerGrass;
            float _Quality;
            int _Depth;
            float4 _Noise_TexelSize;
            float _Scale;
            
            //very basic vertex shader
            v2g vert (appdata v)
            {
                v2g o;
                o.uv = TRANSFORM_TEX(v.uv, _Noise);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            //helper functions to convert from world space to barycentric coordinates
            float3 toPrism(float3 p, float3 a, float3 e, float3 tetra[12], float3 size) {
                //we use the tetrahedra to convert from world space to barycentric coordinates
                float3 vap = p - a;
                float3 vbp = p - e;
                //this is the tetrahedron defined by aefb
                float4 bary = float4(dot(vbp,tetra[0]),dot(vap,tetra[1]), dot(vap,tetra[2]), dot(vap,tetra[3]))*size[0];
                float tetraindex = 0;
                //we can test if it is in the bounds of the other tetrahedra by only checking the sign of the barycentric coordinate to b
                if(bary.w<0) {
                    tetraindex = 2;
                    //we now check the third tetrahedra aebc, this is done because it also has a very simple check
                    bary = float4(dot(vbp,tetra[8]),dot(vap,tetra[9]), dot(vap,tetra[10]), dot(vap,tetra[11]))*size[2];
                    //we check if it is in the bounds of the tetrahedra defined by aebc by checking the sign of the barycentric coordinate to b
                    if(bary.z<0) {
                        tetraindex = 1;
                        //we now check the second tetrahedra aefc, this is done because it also has a slightly more complex check which we can ignore since it will be caught by the last bounds check
                        bary = float4(dot(vbp,tetra[4]),dot(vap,tetra[5]), dot(vap,tetra[6]), dot(vap,tetra[7]))*size[1];
                        return float3(bary.y, bary.z+bary.w, bary.y + bary.z);
                    }
                    return float3(bary.y + bary.z, bary.w, bary.y);
                }
                return float3(bary.y, bary.z, 1.-bary.x);
            }
            //helper function, pretty much entirely unused / used for unused variables
            float3 getNormal(float3 a, float3 b, float3 c) {
                return normalize(cross(b - a, c - a));
            }
            //helper function to convert from barycentric coordinates to uv space which helps with keeping the noise consistent
            float2 map(float2 p, float2 uv[3]) {
                float neg = 1-p.x-p.y;
                return uv[0]*neg + uv[1]*p.x + uv[2]*p.y;
            }
            //helper function to convert from uv space to barycentric coordinates. this time on a triangle instead of a prism
            //used for bounds checks
            float2 unmap(float2 p, float2 uv[3]) {
                float2 v0 = uv[1]-uv[0];
                float2 v1 = uv[2]-uv[0];
                float2 v2 = p-uv[0];
                float d00 = dot(v0, v0);
                float d01 = dot(v0, v1);
                float d11 = dot(v1, v1);
                float d20 = dot(v2, v0);
                float d21 = dot(v2, v1);
                float denom = d00 * d11 - d01 * d01;
                float v = (d11 * d20 - d01 * d21) / denom;
                float w = (d00 * d21 - d01 * d20) / denom;
                float u = 1.0 - v - w;
                return float2(u,v);
            }

            // https://iquilezles.org/articles/distfunctions/
            float sdCone( float3 p, float2 c )
            {
                // c is the sin/cos of the angle
                float2 q = float2( length(p.xz), -p.y );
                float d = length(q-c*max(dot(q,c), 0.0));
                return d * ((q.x*c.y-q.y*c.x<0.0)?-1.0:1.0);
            }
            //geometry shader
            //we use the triangle stream to output triangles

            //maxvertexcount is the maximum number of vertices that can be outputted
            //we declare 7 triangles, 1 for the top and 6 for the sides
            //7*3 = 21
            [maxvertexcount(21)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
            {
                //we store all vertices of the prism to later compute the barycentric coordinates
                float2 uvVertex[3] = {i[0].uv, i[1].uv, i[2].uv};
                float3 p[6] = {
                    i[0].worldPos,
                    i[1].worldPos,
                    i[2].worldPos,
                    i[0].worldPos + i[0].worldNormal * _Height,
                    i[1].worldPos + i[1].worldNormal * _Height,
                    i[2].worldPos + i[2].worldNormal * _Height
                };

                //create our output data and populate it
                g2f o;
                o.p = p;
                o.uvVertex = uvVertex;
                
                //with the walls it is very important to make sure they align with our tetrahedra
                //if the walls are not aligned with the tetrahedra, the barycentric coordinates will be wrong
                //this is because the walls do not need to be flat and can have a bump where the edge will be, if the tetrahedra are built for a different edge
                //than the triangles of the wall we might start raytracing from outside the prism

                //top
                #ifdef _DEBUG
                    o.wallIndex = 0;
                #endif
                o.worldNormal = getNormal(p[3],p[4],p[5]);
                o.worldPos = p[3];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[4];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[5];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                
                //wall1 abde
                #ifdef _DEBUG
                    o.wallIndex = 1;
                #endif
                triStream.RestartStrip();
                o.worldPos = p[0];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[4];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[3];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                triStream.RestartStrip();
                o.worldPos = p[0];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[1];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[4];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);

                //wall2 bcef
                #ifdef _DEBUG
                    o.wallIndex = 2;
                #endif
                triStream.RestartStrip();
                o.worldPos = p[1];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[2];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[4];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                triStream.RestartStrip();
                o.worldPos = p[2];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[5];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[4];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);

                //wall3 cafd
                #ifdef _DEBUG
                    o.wallIndex = 3;
                #endif
                triStream.RestartStrip();
                o.worldPos = p[2];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[0];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[5];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                triStream.RestartStrip();
                o.worldPos = p[0];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[3];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
                o.worldPos = p[5];
                o.vertex = UnityWorldToClipPos(o.worldPos);
                triStream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                //precompute the tetrahedra. This could be done in a compute shader but this is a one file solution
                float3 ab = i.p[0] - i.p[1], ac = i.p[0] - i.p[2], ad = i.p[0] - i.p[3], 
                ae = i.p[0] - i.p[4], af = i.p[0] - i.p[5], 
                eb = i.p[4] - i.p[1], ec = i.p[4] - i.p[2], ed = i.p[4] - i.p[3], ef = i.p[4] - i.p[5];
                float3 afad = cross(af, ad), afac = cross(af, ac), abac = cross(ab, ac), 
                acae = cross(ac, ae), aeaf = cross(ae, af);
                float3 tetra[12] = {
                    cross(ed,ef), afad, cross(ad, ae), aeaf,
                    cross(ec,ef), afac, acae, aeaf,
                    cross(ec,eb), abac, acae, cross(ae, ab)
                };
                float3 size = float3(1/dot(-afad,ae),1/dot(-afac,ae),1/dot(-abac,ae));
                //end of precomputation

                //retrieve the world position and normal from the camera
                float3 p = i.worldPos;
                float3 n = normalize(i.worldPos - _WorldSpaceCameraPos);

                //convert n and p to barycentric coordinates and then to "prismspace" (both done in toPrism)
                n = toPrism(n+p, i.p[0], i.p[4], tetra, size);
                p = toPrism(p, i.p[0], i.p[4], tetra, size); 

                #ifdef _DEBUG                    
                    return float4(p,1);
                #else
                    //convert prism space to uv space
                    p.xy = map(p, i.uvVertex)/_Noise_ST.xy*_Scale;
                    n.xy = map(n, i.uvVertex)/_Noise_ST.xy*_Scale;
                    n = normalize(n-p);

                    //very shrimple raymarcher
                    bool hit = false;
                    bool disc = false;
                    
                    float dist = distance(p, _WorldSpaceCameraPos);
                    float q = _Height/(_Depth*_Quality*(abs(dot(n,i.worldNormal))));
                    int it = 0;
                    [loop]
                    while(!disc && !hit && it < _Depth) {
                        it++;
                        float h = tex2D(_Noise, floor(p.xy)/_Scale*_Noise_ST.xy).r;
                        float d = sdCone(float3(abs(p.x)%1-0.5,(p.z-h),abs(p.y)%1-0.5), normalize(float2(0.71,h)));
                        if(d<_Quality && !hit && !disc) {
                            hit = true;
                        }
                        p += n*d;
                        //convert uv to prism space to more simply check if out of bounds
                        float2 u = unmap(p*_Noise_ST.xy/_Scale, i.uvVertex);
                        disc = u.x+u.y>1|| u.x<0 || u.y<0 || p.z<0 || p.z>1;

                    }
                    if(!hit)
                    discard;
                    return float4((_Grass.rgb-_LowerGrass.rgb) * max(0,p.z)+_LowerGrass,1);
                #endif
            }
            ENDHLSL
        }
    }
}