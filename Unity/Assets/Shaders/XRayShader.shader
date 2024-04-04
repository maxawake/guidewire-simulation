Shader "Unlit/XRayShader"
{
    Properties
    {
        /**
            Textures needed for the Xray Shader: body(3D), aorta(3D), guidewire(3D) and transferfunction(2D)
        */
        _TextureBody("Texture Body", 3D) = "white" {}
        _TextureAorta("Texture Aorta", 3D) = "white" {}
        _TextureGuidewire("Texture Guidewire", 3D) = "white" {} 
        _ContrastFactor("Contrast Factor", Float) = 2.0
    }
        SubShader
        {
                Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
                Blend SrcAlpha OneMinusSrcAlpha
                Cull OFF
                LOD 100
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float4 color : COLOR0;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float4 objectvertex: TEXCOORD1;
                    float4 color: COLOR0;
                    float2 uv : TEXCOORD0;
                };

                sampler3D _TextureBody;
                sampler3D _TextureAorta;
                sampler3D _TextureGuidewire;
                sampler2D _Transferfunction;

                float _Thickness;

                /**
                * Sent from the navigation script
                */
                uniform float4x4 _TransformationMatrix;
                uniform float _ContrastFactor;

                uniform int _xrayOn = 1;
                uniform int _contrastDyeInserted = 0;
                uniform int _guideWireInserted = 0;
                
                // vertex shader
                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);                
                    o.uv = v.uv;

                    return o;
                }

                //fragment shader: XRay simulation will be performed here.
                fixed4 frag(v2f i) : SV_Target
                {
                    float4 color = float4(0,0,0,1); // Default Color of Screen is black.

                    float2 textureCoordinate = i.uv; // Get texture coordinate.
                    float3 planePosition = float3((textureCoordinate.x-0.5), 0, (textureCoordinate.y-0.5) ); // Transform coordinate to plane position.

                    /*
                    * Perform Transformationmatrix on plane position and ray source.
                    */
                    planePosition = mul(float4(planePosition, 1.0f), _TransformationMatrix).xyz;
                    float3 raySource= mul(float4(0.0, 1.0, 0.0, 1.0), _TransformationMatrix).xyz;
                    
                    /*
                    * Create XRay.
                    */
                    float3 rayStart = raySource;
                    float3 rayDirection = normalize(planePosition - rayStart);
                    float3 rayPosition = rayStart;

                    float sum = 0.0;
                   
                    int N = 200; // Define number of steps.
                    float division = 1.0 / N;
                    float d = division * _ContrastFactor; // Define Thickness.

                    for (int i = 0; i < N; i++) // iterate over N steps 
                    {
                        rayPosition = rayStart + 0.01 * i * rayDirection;
                        //rayPosition = raySource + division * i * rayDirection;
                        
                        // Check if ray Position is in normalized cube.
                        if (max(abs(rayPosition.x), max(abs(rayPosition.y), abs(rayPosition.z))) < 0.5f + 0.00001f)
                        {
                            /*
                            * Get attenuation values by accessing the 3D textures (Note that the texture range is [0,1] x [0,1] x [0,1].
                            */
                            float4 bodyColor = tex3D(_TextureBody, rayPosition + float3(0.5f, 0.5f, 0.5f));
                            float4 aortaColor = tex3D(_TextureAorta, rayPosition + float3(0.5f, 0.5f, 0.5f));
                            float4 wireColor = tex3D(_TextureGuidewire, rayPosition + float3(0.5f, 0.5f, 0.5f));
                            
                            sum += bodyColor.a * d;
                            
                            if(_contrastDyeInserted == 1) // check if contrast dye was inserted.
                            {
                                sum += aortaColor.a;
                            }
                            
                            sum += wireColor.a;
                        }
                    }
                    
                    if(_xrayOn == 1) // Check if XRay is activated.
                    {
                        color.a = exp(-sum);
                        color.rgba = float4(1-color.a, 1-color.a, 1-color.a,1.0);
                    }
                    else {
                        color.a = 1.0;
                    }
                   
                    return color;

                }
                ENDCG
            }
        }
}
