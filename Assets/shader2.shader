Shader "Unlit/shader2" {
    Properties{ //input data
        _ColourA("Colour A", Color) = (1, 1, 1, 1)
        _ColourB("Colour B", Color) = (1, 1, 1, 1)

        _ColourStart("Colour Start", Range(0, 1)) = 0
        _ColourEnd("Colour End", Range(0, 1)) = 1
    }
        SubShader{
            Tags {
                "RenderType" = "Transparent" //tag so the pipeline knows what it is (mostly for post-processing)
                "Queue" = "Transparent" //changes the render order
            }

            Pass {

                /*BLENDING
                Blending is managed by the formula (src * A) ± (dst * B) where
                src (source) is the colour of your frag shader output and dst (destination) is whatever is behind it
                To pick a blending mode, you set A, B and the operation (+ or -)
                Additive Blending: A = 1, B = 1
                Multiplicative Blending: A = dst, B = 0
                When writing a shader that produces anything other than 100% opaque, you need to make sure you
                don't write to the depth buffer. We do that by setting ZWrite Off
                */
                ZWrite Off
                Blend One One //additive blending
                //Blend DstColor Zero //multiply
                
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                #define TAU 6.28318530718
                
                float4 _ColourA;
                float4 _ColourB;

                float _ColourStart;
                float _ColourEnd;
                
            //per vertex mesh data
            //automatically filled out by Unity
            struct MeshData { 
                float4 vertex : POSITION; //vertex position
                float3 normals : NORMAL; //normals
                //float4 tangent : TANGENT; //tangents
                //float4 colour : COLOR; //colour
                //uv channels are just data. often float4 is used to cram more data in
                //(e.g. for procedural generation). Can have many uv channels per mesh
                float2 uv0 : TEXCOORD0; //uv0 coords, could be normal/diffuse map textures
                //float2 uv1 : TEXCOORD1; //uv1 coords, could be baked lightmap coords
                //within MeshData, TEXCOORD0 vs TEXCOORD1 actually refers to different UV channels
                //within Interpolators (below) it's just a way to differentiate different data (confusing)
            };
                
            //v2f is Unity's default name for the data that gets passed from the vertex to fragment shader//
            //I've changed the name to Interpolators
            struct Interpolators {
                float4 vertex : SV_POSITION; //clip space position
                //within Interpolators, TEXTCOORD0 vs TEXCOORD1 doesn't actually refer to different UV channels
                //it's just confusing boilerplate Unity stuff to allow to you differentiate data
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };
            //vertex shader returns Interpolators
            //because (most of the time) you have far fewer vertices than fragments (pixels), it's faster to
            //do as much as you can in the vertex shader. Keep the fragment shader as lean as possible
            Interpolators vert (MeshData v) {
                Interpolators o;
                //Converts local space to clip space by multiplying by the MVP matrix.
                //This makes our shader stick to the position of the object.
                //If we just used o.vertex = v.vertex, our shader would stick to the camera's view,
                //which is useful for post-processing
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normals;
                o.uv = v.uv0;// * _Scale + _Offset;
                return o;
            }

            //float# = Vector# (32-bit float). Not often required, but usually you've got the headroom to use it
            //half# (16-bit float). Mostly for mobile
            //fixed# = (lower precision). Only good between -1 and 1
            //naming convention persists for matrices: float4x4, half4x4, etc.

            float InverseLerp(float a, float b, float v)
            {
                return (v-a)/(b-a);
            }
                
            //fragment shader takes Interpolators as input
            //often fragment shaders are erroneously referred to as "pixel shaders" online
            //This is inaccurate, but may be a useful Google term to find tutorials
            float4 frag (Interpolators i) : SV_Target { //SV_Target is the frame buffer, that's our render target

                //saturate() is a terribly named function that is equivalent to Math.Clamp(0,1)
                //float t = saturate(InverseLerp(_ColourStart, _ColourEnd, i.uv.x));

                //float t = abs(frac(i.uv.x * 5) * 2 - 1);

                float2 uvsCentered = i.uv * 2 - 1;
                float radialDistance = saturate(length(uvsCentered));
                
                const float xOffset = cos(i.uv.x * TAU * 5) * 0.01f; //wiggle wiggle
                
                float t = cos((radialDistance + xOffset - _Time.y * 0.1f) * TAU * 5)*0.5 + 0.5; //stripes

                t *= 1 - radialDistance; //fade out at the top
                
                //frac(n) = n - floor(n)
                //t = frac(t); can be used to output a repeating pattern between 0 and 1.
                //This can also help troubleshoot if an issue is occuring because of values outside [0,1]
                
                float4 gradient = lerp(_ColourA, _ColourB, radialDistance);
                
                return t * gradient;
            }
            ENDCG
        }
    }
}
