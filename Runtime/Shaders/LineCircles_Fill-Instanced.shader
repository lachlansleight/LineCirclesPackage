Shader "LineCircles/Fill Instanced"
{
    Properties
    {
        [Toggle(LINES)] _Lines ("Lines", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Cull Off

        CGPROGRAM

        #pragma surface surf NoLighting noambient noforwardadd vertex:vert alpha:fade
        #pragma instancing_options procedural:setup
        #pragma target 3.5
        #pragma shader_feature LINES

        #if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL) || defined(SHADER_API_SWITCH) || defined(SHADER_API_VULKAN) || (defined(SHADER_API_METAL) && defined(UNITY_COMPILER_HLSLCC)))
            #define SUPPORT_STRUCTUREDBUFFER
        #endif

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) && defined(SUPPORT_STRUCTUREDBUFFER)
            #define ENABLE_INSTANCING
        #endif
        
        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            return fixed4(s.Albedo, s.Alpha);
        }
        
        struct LineCircleSnapshot {
            float3 CirclePosition;
            float3 CircleRotation;
            float CircleRadius;
            float3 LineRotation;
            float LineLength;
            float ColorRange;
            float ColorOffset;
            float EmitTime;
            float Hidden;
            float LineHidden;
        };
        
        struct LineCircleVertex {
            float4 Position;
            float4 Color;
        };

        struct appdata
        {
            float4 vertex : POSITION;
            float4 color : COLOR;
            float3 normal : NORMAL;
            uint vid : SV_VertexID;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        
        struct Input {
            float4 color;
            float2 hidden;
        };

        int _LineCount;
        float _Alpha; 
        float4x4 _LocalToWorld;
        float4x4 _WorldToLocal;


        #if defined(ENABLE_INSTANCING)

        StructuredBuffer<LineCircleSnapshot> _SnapshotBuffer;
        StructuredBuffer<LineCircleVertex> _VertexBuffer;

        #endif

        void vert(inout appdata v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            #if defined(ENABLE_INSTANCING)
            

            uint vertexID = unity_InstanceID * 6 * _LineCount + v.vid;
            uint snapshotID = unity_InstanceID;

            v.vertex = _VertexBuffer[vertexID].Position;
            
            o.color = _VertexBuffer[vertexID].Color;
            o.hidden = float2(_SnapshotBuffer[snapshotID].Hidden, _SnapshotBuffer[snapshotID].LineHidden);
            #endif
            
        }
        
        void setup()
        {
            unity_ObjectToWorld = _LocalToWorld;
            unity_WorldToObject = _WorldToLocal;
        }
        
        

        void surf(Input IN, inout SurfaceOutput o)
        {            
            if(IN.hidden.x > 0) {
                discard;
            }
            #ifdef LINES
            if(IN.hidden.y > 0) {
                discard;
            }
            #endif
            
            o.Albedo = IN.color;
            o.Alpha = _Alpha;
        }

        ENDCG
    }
    FallBack "Diffuse"
}