// Made with Amplify Shader Editor v1.9.9.12
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Ocean2"
{
	Properties
	{
		_Wave1Amplitude( "Wave1Amplitude", Float ) = 0.025
		_Wave2Amplitude( "Wave2Amplitude", Float ) = 0.0125
		_Wave3Amplitude( "Wave3Amplitude", Float ) = 0.00625
		_Wave1Direction( "Wave1Direction", Vector ) = ( 0, -1, 0, 0 )
		_Wave2Direction( "Wave2Direction", Vector ) = ( 0.866, 0.5, 0, 0 )
		_Wave3Direction( "Wave3Direction", Vector ) = ( -0.866, 0.5, 0, 0 )
		_Wave1Length( "Wave1Length", Float ) = 2
		_Wave2Length( "Wave2Length", Float ) = 1
		_Wave3Length( "Wave3Length", Float ) = 0.5
		_Wave1Steepness( "Wave1Steepness", Float ) = 8
		_Wave2Steepness( "Wave2Steepness", Float ) = 4
		_Wave3Steepness( "Wave3Steepness", Float ) = 2
		_Wave1Speed( "Wave1Speed", Float ) = 0.5
		_Wave2Speed( "Wave2Speed", Float ) = 1
		_Wave3Speed( "Wave3Speed", Float ) = 1.5
		_OceanTime( "OceanTime", Float ) = 0
		_CurrentTime( "CurrentTime", Float ) = 0
		_CurrentDirection( "CurrentDirection", Vector ) = ( 0, -1, 0, 0 )
		_VertexTimeMultiplier( "VertexTimeMultiplier", Float ) = 0.1
		_PixelDensity( "PixelDensity", Float ) = 1800
		_ShadowColor( "ShadowColor", Color ) = ( 0.3137255, 0.5764706, 0.6588235, 0 )
		_ShadowTileSpeed( "ShadowTileSpeed", Float ) = 0.001
		_FoamColor( "FoamColor", Color ) = ( 0.5960785, 0.8431373, 0.8156863, 0 )
		_FoamScale( "FoamScale", Float ) = 100
		_FoamTileSpeed( "FoamTileSpeed", Float ) = 0.001
		_FoamRotateSpeed( "FoamRotateSpeed", Float ) = 0.1
		_DepthColor( "DepthColor", Color ) = ( 0.7764706, 0.8745098, 0.8862745, 0 )
		_DepthDistance( "DepthDistance", Float ) = 0.5
		_OceanRadius( "OceanRadius", Float ) = 50
		_HorizonColor( "HorizonColor", Color ) = ( 0.9098039, 1, 0.5803922, 0 )
		_HorizonPower( "HorizonPower", Float ) = 3
		_HorizonEmission( "HorizonEmission", Range( 0, 1 ) ) = 1


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5

		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		//_InstancedTerrainNormals("Instanced Terrain Normals", Float) = 1.0

		[ToggleOff(_SPECULARHIGHLIGHTS_OFF)] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[ToggleOff] _ScreenSpaceReflections("Screen Space Reflections", Float) = 1.0
		[ToggleOff] _ScreenSpaceReflectionsContributeTransparent("Screen Space Reflections Contribute Transparent", Float) = 1.0
		[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
		[HideInInspector] _XRMotionVectorsPass("_XRMotionVectorsPass", Float) = 1

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		PackageRequirements
		{
			"com.unity.render-pipelines.universal": "[17.0,18.0]"
		}

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Lit" }

	LOD 0

		Cull Back
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#define ASE_ADJUST_CLIP_POSITION( x ) x

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300
			#define REQUIRE_DEPTH_TEXTURE 1


			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
			#endif

			#if defined(UNITY_PLATFORM_META_QUEST) && ( UNITY_VERSION >= 60050000 )
            #pragma multi_compile _ META_QUEST_LIGHTUNROLL
            #endif

			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#endif
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#else
			#pragma multi_compile _ _FORWARD_PLUS
			#endif

            #if defined(UNITY_PLATFORM_META_QUEST) && ( UNITY_VERSION >= 60050000 )
            #pragma multi_compile _ META_QUEST_ORTHO_PROJ
            #pragma multi_compile _ META_QUEST_NO_SPOTLIGHTS_LIGHT_LOOP
            #endif

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_FORWARD

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#if ( UNITY_VERSION >= 60010000 )
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
			#else
			#pragma multi_compile_fog
			#endif
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			#if ( UNITY_VERSION < 60010000 )
				#define USE_CLUSTER_LIGHT_LOOP USE_FORWARD_PLUS
				#define CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

					float2 voronoihash578( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi578( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash578( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			
					float2 voronoihash590( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi590( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash590( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				
				float3 vertexPos545 = ( input.positionOS.xyz + vertexOffset537 );
				float4 ase_positionCS545 = TransformObjectToHClip( ( vertexPos545 ).xyz );
				float4 screenPos545 = ComputeScreenPos( ase_positionCS545 );
				output.ase_texcoord8 = screenPos545;
				
				output.ase_texcoord7.xy = input.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord7.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif
				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				#ifdef ASE_CUSTOM_MOTION_VECTOR
					// Declared so the Motion Vector output port surfaces on the master node; only consumed by the motion vector passes.
					float3 aseCustomMotionVector = float3(0, 0, 0);
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						output.fogFactorAndVertexLight.x = ComputeFogFactor(vertexInput.positionCS.z);
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						#if ( UNITY_VERSION >= 60020000 )
						, out uint outRenderingLayers : SV_Target1
						#else
						, out float4 outRenderingLayers : SV_Target1
						#endif
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float4 color54 = IsGammaSpace() ? float4( 0.3921569, 0.7294118, 0.7686275, 0 ) : float4( 0.1274377, 0.4910209, 0.5520116, 0 );
				float time578 = 1.7;
				float2 voronoiSmoothId578 = 0;
				float2 texCoord576 = input.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv665 = ( floor( ( texCoord576 * _PixelDensity ) ) / _PixelDensity );
				float oceanTime669 = _OceanTime;
				float2 appendResult575 = (float2(0.0 , ( oceanTime669 * _ShadowTileSpeed )));
				float2 coords578 = ( uv665 - appendResult575 ) * 25.0;
				float2 id578 = 0;
				float2 uv578 = 0;
				float fade578 = 0.5;
				float voroi578 = 0;
				float rest578 = 0;
				for( int it578 = 0; it578 <2; it578++ ){
				voroi578 += fade578 * voronoi578( coords578, time578, id578, uv578, 0,voronoiSmoothId578 );
				rest578 += fade578;
				coords578 *= 2;
				fade578 *= 0.5;
				}//Voronoi578
				voroi578 /= rest578;
				float smoothstepResult581 = smoothstep( 0.2 , 0.0 , voroi578);
				float shadowBlend584 = smoothstepResult581;
				float3 lerpResult609 = lerp( color54.rgb , _ShadowColor.rgb , shadowBlend584);
				float time590 = ( ( oceanTime669 * _FoamRotateSpeed ) % ( 2.0 * PI ) );
				float2 voronoiSmoothId590 = 0;
				float currentTime635 = _CurrentTime;
				float2 appendResult597 = (float2(0.0 , ( currentTime635 * _FoamTileSpeed )));
				float2 coords590 = ( uv665 - appendResult597 ) * _FoamScale;
				float2 id590 = 0;
				float2 uv590 = 0;
				float fade590 = 0.5;
				float voroi590 = 0;
				float rest590 = 0;
				for( int it590 = 0; it590 <2; it590++ ){
				voroi590 += fade590 * voronoi590( coords590, time590, id590, uv590, 0,voronoiSmoothId590 );
				rest590 += fade590;
				coords590 *= 2;
				fade590 *= 0.5;
				}//Voronoi590
				voroi590 /= rest590;
				float smoothstepResult591 = smoothstep( 0.25 , 0.55 , voroi590);
				float foamBlend607 = smoothstepResult591;
				float3 lerpResult611 = lerp( lerpResult609 , _FoamColor.rgb , foamBlend607);
				float4 screenPos545 = input.ase_texcoord8;
				float4 ase_positionSSNorm = screenPos545 / screenPos545.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth545 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth545 = saturate( abs( ( screenDepth545 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthDistance ) ) );
				float depthBlend569 = distanceDepth545;
				float3 lerpResult550 = lerp( _DepthColor.rgb , lerpResult611 , depthBlend569);
				float emission562 = ( pow( ( length( PositionWS ) / _OceanRadius ) , _HorizonPower ) * _HorizonEmission );
				float3 lerpResult565 = lerp( lerpResult550 , _HorizonColor.rgb , emission562);
				float3 baseColor551 = lerpResult565;
				
				float3 temp_cast_0 = (emission562).xxx;
				

				float3 BaseColor = baseColor551;
				float3 Normal = float3(0, 0, 1);
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float3 Emission = temp_cast_0;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.viewDirectionWS = ViewDirWS;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = NormalWS;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE) && ( UNITY_VERSION >= 60030000 )
					#if ( UNITY_VERSION >= 60060000 )
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy, inputData.normalWS));
					#else
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
					#endif
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI( SH, GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask );
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
				#endif

				#ifdef ASE_LIGHTING_SIMPLE
					half4 color = UniversalFragmentBlinnPhong( inputData, surfaceData);
				#else
					half4 color = UniversalFragmentPBR( inputData, surfaceData);
				#endif

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					#define SUM_LIGHT_TRANSMISSION(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
						color.rgb += BaseColor * transmission;

					SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSMISSION( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSMISSION( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#define SUM_LIGHT_TRANSLUCENCY(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 lightDir = Light.direction + inputData.normalWS * normal;\
						half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
						half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
						color.rgb += BaseColor * translucency * strength;

					SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSLUCENCY( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSLUCENCY( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( NormalWS,0 ) ).xyz * ( 1.0 - dot( NormalWS, ViewDirWS ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						color.rgb = MixFog(color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					#if ( UNITY_VERSION >= 60020000 )
					outRenderingLayers = EncodeMeshRenderingLayer();
					#else
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
					#endif
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( color.rgb, color.a );
				#else
					return half4( color.rgb, OutputAlpha( color.a, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_VERT_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(	PackedVaryings input
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				

				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300
			#define REQUIRE_DEPTH_TEXTURE 1

			#pragma shader_feature EDITOR_VISUALIZATION

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD1;
					float4 LightCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

					float2 voronoihash578( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi578( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash578( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			
					float2 voronoihash590( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi590( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash590( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				
				float3 vertexPos545 = ( input.positionOS.xyz + vertexOffset537 );
				float4 ase_positionCS545 = TransformObjectToHClip( ( vertexPos545 ).xyz );
				float4 screenPos545 = ComputeScreenPos( ase_positionCS545 );
				output.ase_texcoord4 = screenPos545;
				
				output.ase_texcoord3.xy = input.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(input.positionOS.xyz, input.texcoord.xy, input.texcoord1.xy, input.texcoord2.xy, VizUV, LightCoord);
					output.VizUV = float4(VizUV, 0, 0);
					output.LightCoord = LightCoord;
				#endif

				output.positionCS = MetaVertexPosition( input.positionOS, input.texcoord1.xy, input.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				output.positionWS = TransformObjectToWorld( input.positionOS.xyz );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				output.texcoord1 = input.texcoord1;
				output.texcoord2 = input.texcoord2;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float4 color54 = IsGammaSpace() ? float4( 0.3921569, 0.7294118, 0.7686275, 0 ) : float4( 0.1274377, 0.4910209, 0.5520116, 0 );
				float time578 = 1.7;
				float2 voronoiSmoothId578 = 0;
				float2 texCoord576 = input.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv665 = ( floor( ( texCoord576 * _PixelDensity ) ) / _PixelDensity );
				float oceanTime669 = _OceanTime;
				float2 appendResult575 = (float2(0.0 , ( oceanTime669 * _ShadowTileSpeed )));
				float2 coords578 = ( uv665 - appendResult575 ) * 25.0;
				float2 id578 = 0;
				float2 uv578 = 0;
				float fade578 = 0.5;
				float voroi578 = 0;
				float rest578 = 0;
				for( int it578 = 0; it578 <2; it578++ ){
				voroi578 += fade578 * voronoi578( coords578, time578, id578, uv578, 0,voronoiSmoothId578 );
				rest578 += fade578;
				coords578 *= 2;
				fade578 *= 0.5;
				}//Voronoi578
				voroi578 /= rest578;
				float smoothstepResult581 = smoothstep( 0.2 , 0.0 , voroi578);
				float shadowBlend584 = smoothstepResult581;
				float3 lerpResult609 = lerp( color54.rgb , _ShadowColor.rgb , shadowBlend584);
				float time590 = ( ( oceanTime669 * _FoamRotateSpeed ) % ( 2.0 * PI ) );
				float2 voronoiSmoothId590 = 0;
				float currentTime635 = _CurrentTime;
				float2 appendResult597 = (float2(0.0 , ( currentTime635 * _FoamTileSpeed )));
				float2 coords590 = ( uv665 - appendResult597 ) * _FoamScale;
				float2 id590 = 0;
				float2 uv590 = 0;
				float fade590 = 0.5;
				float voroi590 = 0;
				float rest590 = 0;
				for( int it590 = 0; it590 <2; it590++ ){
				voroi590 += fade590 * voronoi590( coords590, time590, id590, uv590, 0,voronoiSmoothId590 );
				rest590 += fade590;
				coords590 *= 2;
				fade590 *= 0.5;
				}//Voronoi590
				voroi590 /= rest590;
				float smoothstepResult591 = smoothstep( 0.25 , 0.55 , voroi590);
				float foamBlend607 = smoothstepResult591;
				float3 lerpResult611 = lerp( lerpResult609 , _FoamColor.rgb , foamBlend607);
				float4 screenPos545 = input.ase_texcoord4;
				float4 ase_positionSSNorm = screenPos545 / screenPos545.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth545 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth545 = saturate( abs( ( screenDepth545 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthDistance ) ) );
				float depthBlend569 = distanceDepth545;
				float3 lerpResult550 = lerp( _DepthColor.rgb , lerpResult611 , depthBlend569);
				float emission562 = ( pow( ( length( PositionWS ) / _OceanRadius ) , _HorizonPower ) * _HorizonEmission );
				float3 lerpResult565 = lerp( lerpResult550 , _HorizonColor.rgb , emission562);
				float3 baseColor551 = lerpResult565;
				
				float3 temp_cast_0 = (emission562).xxx;
				

				float3 BaseColor = baseColor551;
				float3 Emission = temp_cast_0;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = input.VizUV.xy;
					metaInput.LightCoord = input.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300
			#define REQUIRE_DEPTH_TEXTURE 1


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

					float2 voronoihash578( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi578( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash578( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			
					float2 voronoihash590( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi590( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash590( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_TRANSFER_INSTANCE_ID( input, output );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				
				float3 vertexPos545 = ( input.positionOS.xyz + vertexOffset537 );
				float4 ase_positionCS545 = TransformObjectToHClip( ( vertexPos545 ).xyz );
				float4 screenPos545 = ComputeScreenPos( ase_positionCS545 );
				output.ase_texcoord2 = screenPos545;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord1.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float4 color54 = IsGammaSpace() ? float4( 0.3921569, 0.7294118, 0.7686275, 0 ) : float4( 0.1274377, 0.4910209, 0.5520116, 0 );
				float time578 = 1.7;
				float2 voronoiSmoothId578 = 0;
				float2 texCoord576 = input.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv665 = ( floor( ( texCoord576 * _PixelDensity ) ) / _PixelDensity );
				float oceanTime669 = _OceanTime;
				float2 appendResult575 = (float2(0.0 , ( oceanTime669 * _ShadowTileSpeed )));
				float2 coords578 = ( uv665 - appendResult575 ) * 25.0;
				float2 id578 = 0;
				float2 uv578 = 0;
				float fade578 = 0.5;
				float voroi578 = 0;
				float rest578 = 0;
				for( int it578 = 0; it578 <2; it578++ ){
				voroi578 += fade578 * voronoi578( coords578, time578, id578, uv578, 0,voronoiSmoothId578 );
				rest578 += fade578;
				coords578 *= 2;
				fade578 *= 0.5;
				}//Voronoi578
				voroi578 /= rest578;
				float smoothstepResult581 = smoothstep( 0.2 , 0.0 , voroi578);
				float shadowBlend584 = smoothstepResult581;
				float3 lerpResult609 = lerp( color54.rgb , _ShadowColor.rgb , shadowBlend584);
				float time590 = ( ( oceanTime669 * _FoamRotateSpeed ) % ( 2.0 * PI ) );
				float2 voronoiSmoothId590 = 0;
				float currentTime635 = _CurrentTime;
				float2 appendResult597 = (float2(0.0 , ( currentTime635 * _FoamTileSpeed )));
				float2 coords590 = ( uv665 - appendResult597 ) * _FoamScale;
				float2 id590 = 0;
				float2 uv590 = 0;
				float fade590 = 0.5;
				float voroi590 = 0;
				float rest590 = 0;
				for( int it590 = 0; it590 <2; it590++ ){
				voroi590 += fade590 * voronoi590( coords590, time590, id590, uv590, 0,voronoiSmoothId590 );
				rest590 += fade590;
				coords590 *= 2;
				fade590 *= 0.5;
				}//Voronoi590
				voroi590 /= rest590;
				float smoothstepResult591 = smoothstep( 0.25 , 0.55 , voroi590);
				float foamBlend607 = smoothstepResult591;
				float3 lerpResult611 = lerp( lerpResult609 , _FoamColor.rgb , foamBlend607);
				float4 screenPos545 = input.ase_texcoord2;
				float4 ase_positionSSNorm = screenPos545 / screenPos545.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth545 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth545 = saturate( abs( ( screenDepth545 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthDistance ) ) );
				float depthBlend569 = distanceDepth545;
				float3 lerpResult550 = lerp( _DepthColor.rgb , lerpResult611 , depthBlend569);
				float emission562 = ( pow( ( length( PositionWS ) / _OceanRadius ) , _HorizonPower ) * _HorizonEmission );
				float3 lerpResult565 = lerp( lerpResult550 , _HorizonColor.rgb , emission562);
				float3 baseColor551 = lerpResult565;
				

				float3 BaseColor = baseColor551;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				half4 color = half4(BaseColor, Alpha );

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
			//#define SHADERPASS SHADERPASS_DEPTHNORMALS

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				half4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(	PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_WRITE_DEPTH )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						#if ( UNITY_VERSION >= 60020000 )
						, out uint outRenderingLayers : SV_Target1
						#else
						, out float4 outRenderingLayers : SV_Target1
						#endif
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				

				float3 Normal = float3(0, 0, 1);
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					#if ( UNITY_VERSION >= 60020000 )
					outRenderingLayers = EncodeMeshRenderingLayer();
					#else
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
					#endif
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300
			#define REQUIRE_DEPTH_TEXTURE 1


			// Deferred Rendering Path does not support the OpenGL-based graphics API:
			// Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
			#pragma exclude_renderers glcore gles3 

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#if ( UNITY_VERSION >= 60000058 )
			#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#endif
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#endif
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#endif

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS
			#pragma multi_compile _ LIGHTMAP_ON
			#if ( UNITY_VERSION >= 60010000 )
			#pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
			#endif
			#if ( UNITY_VERSION >= 60030000 )
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#endif
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_GBUFFER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if ( UNITY_VERSION >= 60030016 && UNITY_VERSION < 60040000 ) || ( UNITY_VERSION >= 60040010 )
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutputFormat.hlsl"
			#endif

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			#if ( UNITY_VERSION >= 60010000 )
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
			#else
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
			#endif

					float2 voronoihash578( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi578( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash578( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			
					float2 voronoihash590( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi590( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0; int i, j;
						for ( j = -1; j <= 1; j++ )
						{
							for ( i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash590( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.707 * sqrt(dot( r, r ));
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						
						 		}
						 	}
						}
						return F1;
					}
			

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				
				float3 vertexPos545 = ( input.positionOS.xyz + vertexOffset537 );
				float4 ase_positionCS545 = TransformObjectToHClip( ( vertexPos545 ).xyz );
				float4 screenPos545 = ComputeScreenPos( ase_positionCS545 );
				output.ase_texcoord8 = screenPos545;
				
				output.ase_texcoord7.xy = input.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;
				input.tangentOS = input.tangentOS;

				#ifdef ASE_CUSTOM_MOTION_VECTOR
					// Declared so the Motion Vector output port surfaces on the master node; only consumed by the motion vector passes.
					float3 aseCustomMotionVector = float3(0, 0, 0);
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						// @diogo: no fog applied in GBuffer
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

		#if ( UNITY_VERSION >= 60010000 )
			GBufferFragOutput frag ( PackedVaryings input
		#else
			FragmentOutput frag ( PackedVaryings input
		#endif
								#if defined( ASE_WRITE_DEPTH )
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float4 color54 = IsGammaSpace() ? float4( 0.3921569, 0.7294118, 0.7686275, 0 ) : float4( 0.1274377, 0.4910209, 0.5520116, 0 );
				float time578 = 1.7;
				float2 voronoiSmoothId578 = 0;
				float2 texCoord576 = input.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv665 = ( floor( ( texCoord576 * _PixelDensity ) ) / _PixelDensity );
				float oceanTime669 = _OceanTime;
				float2 appendResult575 = (float2(0.0 , ( oceanTime669 * _ShadowTileSpeed )));
				float2 coords578 = ( uv665 - appendResult575 ) * 25.0;
				float2 id578 = 0;
				float2 uv578 = 0;
				float fade578 = 0.5;
				float voroi578 = 0;
				float rest578 = 0;
				for( int it578 = 0; it578 <2; it578++ ){
				voroi578 += fade578 * voronoi578( coords578, time578, id578, uv578, 0,voronoiSmoothId578 );
				rest578 += fade578;
				coords578 *= 2;
				fade578 *= 0.5;
				}//Voronoi578
				voroi578 /= rest578;
				float smoothstepResult581 = smoothstep( 0.2 , 0.0 , voroi578);
				float shadowBlend584 = smoothstepResult581;
				float3 lerpResult609 = lerp( color54.rgb , _ShadowColor.rgb , shadowBlend584);
				float time590 = ( ( oceanTime669 * _FoamRotateSpeed ) % ( 2.0 * PI ) );
				float2 voronoiSmoothId590 = 0;
				float currentTime635 = _CurrentTime;
				float2 appendResult597 = (float2(0.0 , ( currentTime635 * _FoamTileSpeed )));
				float2 coords590 = ( uv665 - appendResult597 ) * _FoamScale;
				float2 id590 = 0;
				float2 uv590 = 0;
				float fade590 = 0.5;
				float voroi590 = 0;
				float rest590 = 0;
				for( int it590 = 0; it590 <2; it590++ ){
				voroi590 += fade590 * voronoi590( coords590, time590, id590, uv590, 0,voronoiSmoothId590 );
				rest590 += fade590;
				coords590 *= 2;
				fade590 *= 0.5;
				}//Voronoi590
				voroi590 /= rest590;
				float smoothstepResult591 = smoothstep( 0.25 , 0.55 , voroi590);
				float foamBlend607 = smoothstepResult591;
				float3 lerpResult611 = lerp( lerpResult609 , _FoamColor.rgb , foamBlend607);
				float4 screenPos545 = input.ase_texcoord8;
				float4 ase_positionSSNorm = screenPos545 / screenPos545.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float screenDepth545 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_positionSSNorm.xy ),_ZBufferParams);
				float distanceDepth545 = saturate( abs( ( screenDepth545 - LinearEyeDepth( ase_positionSSNorm.z,_ZBufferParams ) ) / ( _DepthDistance ) ) );
				float depthBlend569 = distanceDepth545;
				float3 lerpResult550 = lerp( _DepthColor.rgb , lerpResult611 , depthBlend569);
				float emission562 = ( pow( ( length( PositionWS ) / _OceanRadius ) , _HorizonPower ) * _HorizonEmission );
				float3 lerpResult565 = lerp( lerpResult550 , _HorizonColor.rgb , emission562);
				float3 baseColor551 = lerpResult565;
				
				float3 temp_cast_0 = (emission562).xxx;
				

				float3 BaseColor = baseColor551;
				float3 Normal = float3(0, 0, 1);
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float3 Emission = temp_cast_0;
				float Alpha = 1;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = _Cutoff;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( TangentWS, BitangentWS, NormalWS ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = NormalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( ViewDirWS );

				#ifdef ASE_FOG
					// @diogo: no fog applied in GBuffer
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE) && ( UNITY_VERSION >= 60030000 )
					#if ( UNITY_VERSION >= 60060000 )
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy, inputData.normalWS));
					#else
						inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
					#endif
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI(SH,
						GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask);
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(input.positionCS,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);

			#if ( UNITY_VERSION >= 60010000 )
				color.rgb = GlobalIllumination(brdfData, (BRDFData)0, 0,
                              inputData.bakedGI, Occlusion, inputData.positionWS,
                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
			#else
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
			#endif

				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

			#if ( UNITY_VERSION >= 60010000 )
				return PackGBuffersBRDFData(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			#else
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			#endif
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_WRITE_DEPTH )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return half4( _ObjectId, _PassValue, 1.0, 1.0 );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#define ASE_TESSELLATION 1
			#pragma require tessellation tessHW
			#pragma hull HullFunction
			#pragma domain DomainFunction
			#define ASE_PHONG_TESSELLATION
			#define ASE_FIXED_TESSELLATION
			#define _EMISSION
			#define ASE_VERSION 19912
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			#if defined(ASE_WRITE_DEPTH_CONSERVATIVE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _HorizonColor;
			float4 _ShadowColor;
			float4 _DepthColor;
			float4 _FoamColor;
			float2 _Wave3Direction;
			float2 _CurrentDirection;
			float2 _Wave1Direction;
			float2 _Wave2Direction;
			float _CurrentTime;
			float _PixelDensity;
			float _ShadowTileSpeed;
			float _FoamScale;
			float _FoamTileSpeed;
			float _DepthDistance;
			float _OceanRadius;
			float _FoamRotateSpeed;
			float _Wave3Steepness;
			float _Wave3Length;
			float _Wave3Speed;
			float _HorizonPower;
			float _Wave2Steepness;
			float _Wave2Amplitude;
			float _Wave2Speed;
			float _Wave2Length;
			float _Wave1Steepness;
			float _Wave1Amplitude;
			float _Wave1Speed;
			float _OceanTime;
			float _Wave1Length;
			float _VertexTimeMultiplier;
			float _Wave3Amplitude;
			float _HorizonEmission;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 appendResult649 = (float2(input.positionOS.xyz.x , input.positionOS.xyz.z));
				float currentTime635 = _CurrentTime;
				float2 vertexPosition638 = ( appendResult649 - ( ( currentTime635 * _VertexTimeMultiplier ) * _CurrentDirection ) );
				float2 wave1Direction85 = _Wave1Direction;
				float dotResult16 = dot( vertexPosition638 , wave1Direction85 );
				float wave1Number96 = ( ( 2.0 * PI ) / _Wave1Length );
				float oceanTime669 = _OceanTime;
				float wave1Phase91 = ( ( dotResult16 * wave1Number96 ) - ( oceanTime669 * _Wave1Speed ) );
				float wave1Amplitude88 = _Wave1Amplitude;
				float wave1Steepness94 = _Wave1Steepness;
				float2 break53 = ( ( ( cos( wave1Phase91 ) * wave1Amplitude88 ) * wave1Steepness94 ) * wave1Direction85 );
				float3 appendResult28 = (float3(break53.x , ( sin( wave1Phase91 ) * wave1Amplitude88 ) , break53.y));
				float3 wave1Offset55 = appendResult28;
				float2 wave2Direction100 = _Wave2Direction;
				float dotResult142 = dot( vertexPosition638 , wave2Direction100 );
				float wave2Number143 = ( ( 2.0 * PI ) / _Wave2Length );
				float wave2Phase149 = ( ( dotResult142 * wave2Number143 ) - ( oceanTime669 * _Wave2Speed ) );
				float wave2Amplitude101 = _Wave2Amplitude;
				float wave2Steepness102 = _Wave2Steepness;
				float2 break156 = ( ( ( cos( wave2Phase149 ) * wave2Amplitude101 ) * wave2Steepness102 ) * wave2Direction100 );
				float3 appendResult157 = (float3(break156.x , ( sin( wave2Phase149 ) * wave2Amplitude101 ) , break156.y));
				float3 wave2Offset160 = appendResult157;
				float2 wave3Direction105 = _Wave3Direction;
				float dotResult260 = dot( vertexPosition638 , wave3Direction105 );
				float wave3Number277 = ( ( 2.0 * PI ) / _Wave3Length );
				float wave3Phase280 = ( ( dotResult260 * wave3Number277 ) - ( oceanTime669 * _Wave3Speed ) );
				float wave3Amplitude107 = _Wave3Amplitude;
				float wave3Steepness108 = _Wave3Steepness;
				float2 break269 = ( ( ( cos( wave3Phase280 ) * wave3Amplitude107 ) * wave3Steepness108 ) * wave3Direction105 );
				float3 appendResult270 = (float3(break269.x , ( sin( wave3Phase280 ) * wave3Amplitude107 ) , break269.y));
				float3 wave3Offset286 = appendResult270;
				float3 vertexOffset537 = ( ( wave1Offset55 + wave2Offset160 ) + wave3Offset286 );
				
				float2 break439 = wave1Direction85;
				float2 break454 = wave2Direction100;
				float2 break467 = wave3Direction105;
				float tangentZ477 = -( ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * break439.x ) * break439.y ) * sin( wave1Phase91 ) ) + ( ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * break454.x ) * break454.y ) * sin( wave2Phase149 ) ) ) + ( ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * break467.x ) * break467.y ) * sin( wave3Phase280 ) ) );
				float tangentY405 = ( ( ( ( ( wave1Amplitude88 * wave1Number96 ) * wave1Direction85.x ) * cos( wave1Phase91 ) ) + ( ( ( wave2Amplitude101 * wave2Number143 ) * wave2Direction100.x ) * cos( wave2Phase149 ) ) ) + ( ( ( wave3Amplitude107 * wave3Number277 ) * wave3Direction105.x ) * cos( wave3Phase280 ) ) );
				float temp_output_2_0_g5 = wave1Direction85.x;
				float temp_output_2_0_g3 = wave2Direction100.x;
				float temp_output_2_0_g4 = wave3Direction105.x;
				float tangentX332 = ( 1.0 - ( ( ( ( ( ( wave1Steepness94 * wave1Amplitude88 ) * wave1Number96 ) * ( temp_output_2_0_g5 * temp_output_2_0_g5 ) ) * sin( wave1Phase91 ) ) + ( ( ( ( wave2Steepness102 * wave2Amplitude101 ) * wave2Number143 ) * ( temp_output_2_0_g3 * temp_output_2_0_g3 ) ) * sin( wave2Phase149 ) ) ) + ( ( ( ( wave3Steepness108 * wave3Amplitude107 ) * wave3Number277 ) * ( temp_output_2_0_g4 * temp_output_2_0_g4 ) ) * sin( wave3Phase280 ) ) ) );
				float3 appendResult532 = (float3(tangentZ477 , tangentY405 , tangentX332));
				float3 appendResult528 = (float3(tangentX332 , tangentY405 , tangentZ477));
				float3 normalizeResult535 = normalize( cross( appendResult532 , appendResult528 ) );
				float3 vertexNormal540 = normalizeResult535;
				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = vertexOffset537;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = vertexNormal540;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = ASE_ADJUST_CLIP_POSITION( vertexInput.positionCS );
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_WRITE_DEPTH )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				

				surfaceDescription.Alpha = 1;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = _Cutoff;
				#endif

				#if defined( ASE_WRITE_DEPTH )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_WRITE_DEPTH )
					outputDepth = input.positionCS.z;
				#endif

				return unity_SelectionID;
			}
			ENDHLSL
		}

	
	}
	

	

	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19912
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":600,"pos":[-2464,-80],"params":["Inherit","False","Property","_CurrentTime","CurrentTime","16","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":635,"pos":[-2272,-80],"params":["Inherit","False","currentTime","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":651,"pos":[-1952,-80],"params":["Inherit","False","635","currentTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":654,"pos":[-1984,32],"params":["Inherit","False","Property","_VertexTimeMultiplier","VertexTimeMultiplier","18","0","Create","True","0","0","0","False","0","False","Object","-1","","0.1","0.1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":640,"pos":[-1576,-208],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":646,"pos":[-1600,32],"params":["Inherit","False","Property","_CurrentDirection","CurrentDirection","17","0","Create","True","0","0","0","False","0","False","Object","-1","","0,-1","0,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":653,"pos":[-1760,-80],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0.001","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":536,"pos":[-4016,-128],"params":["Inherit","False","1516","419","Wave Properties","18","85","100","105","26","97","45","88","98","101","106","94","102","103","107","108","13","99","104","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":649,"pos":[-1376,-208],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":652,"pos":[-1376,-80],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":137,"pos":[-4018,334],"params":["Inherit","False","3036","483","Wave1 Offset","28","86","16","96","19","18","20","17","24","91","0","40","41","90","46","49","87","53","28","27","93","55","25","89","95","22","639","670","671","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":139,"pos":[-4016,856],"params":["Inherit","False","3036","483","Wave2 Offset","27","165","163","162","161","160","159","158","157","156","155","154","153","152","151","150","149","148","147","146","145","144","143","142","141","643","672","673","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":660,"pos":[-1208,-208],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":13,"pos":[-3968,-80],"params":["Inherit","False","Property","_Wave1Direction","Wave1Direction","3","0","Create","True","0","0","0","False","0","False","Object","-1","","0,-1","1,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":99,"pos":[-3472,-80],"params":["Inherit","False","Property","_Wave2Direction","Wave2Direction","4","0","Create","True","0","0","0","False","0","False","Object","-1","","0.866,0.5","1,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":258,"pos":[-4016,1384],"params":["Inherit","False","3036","483","Wave3 Offset","27","286","285","284","283","282","281","280","279","278","277","276","272","271","270","269","268","267","266","265","264","263","262","261","260","645","674","676","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":638,"pos":[-1040,-208],"params":["Inherit","False","vertexPosition","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":19,"pos":[-3968,592],"params":["Inherit","False","1","0","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":144,"pos":[-3968,1112],"params":["Inherit","False","1","0","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":18,"pos":[-3968,672],"params":["Inherit","False","Property","_Wave1Length","Wave1Length","6","0","Create","True","0","0","0","False","0","False","Object","-1","","2","5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":145,"pos":[-3968,1192],"params":["Inherit","False","Property","_Wave2Length","Wave2Length","7","0","Create","True","0","0","0","False","0","False","Object","-1","","1","5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor","id":104,"pos":[-2976,-80],"params":["Inherit","False","Property","_Wave3Direction","Wave3Direction","5","0","Create","True","0","0","0","False","0","False","Object","-1","","-0.866,0.5","1,0","0","3","FLOAT2","0","FLOAT","1","FLOAT","2"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":85,"pos":[-3736,-80],"params":["Inherit","False","wave1Direction","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":100,"pos":[-3240,-80],"params":["Inherit","False","wave2Direction","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":668,"pos":[-2464,16],"params":["Inherit","False","Property","_OceanTime","OceanTime","15","0","Create","True","0","0","0","False","0","False","Object","-1","","0","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":478,"pos":[168,1912],"params":["Inherit","False","2236","803","TangentZ","44","446","447","448","436","433","434","437","438","439","432","435","444","445","440","441","442","450","451","454","456","457","458","460","461","463","464","467","469","470","473","474","455","449","452","453","459","468","462","465","466","471","472","475","477","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":86,"pos":[-3736,496],"params":["Inherit","False","85","wave1Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":141,"pos":[-3736,1016],"params":["Inherit","False","100","wave2Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":261,"pos":[-3968,1640],"params":["Inherit","False","1","0","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":278,"pos":[-3968,1720],"params":["Inherit","False","Property","_Wave3Length","Wave3Length","8","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":639,"pos":[-4000,384],"params":["Inherit","False","638","vertexPosition","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":643,"pos":[-4000,904],"params":["Inherit","False","638","vertexPosition","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":20,"pos":[-3768,592],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":146,"pos":[-3768,1112],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":105,"pos":[-2744,-80],"params":["Inherit","False","wave3Direction","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":669,"pos":[-2272,16],"params":["Inherit","False","oceanTime","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":333,"pos":[-4016,1912],"params":["Inherit","False","2268","811","TangentX","41","289","290","299","300","291","292","293","294","295","297","296","302","315","304","305","306","310","311","313","314","303","307","308","309","312","316","317","318","319","320","321","322","323","324","325","326","327","328","329","330","332","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":276,"pos":[-3736,1544],"params":["Inherit","False","105","wave3Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":645,"pos":[-4000,1432],"params":["Inherit","False","638","vertexPosition","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":16,"pos":[-3504,384],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":142,"pos":[-3504,904],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":262,"pos":[-3768,1640],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":22,"pos":[-3152,584],"params":["Inherit","False","Property","_Wave1Speed","Wave1Speed","12","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":671,"pos":[-3152,496],"params":["Inherit","False","669","oceanTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":165,"pos":[-3152,1096],"params":["Inherit","False","Property","_Wave2Speed","Wave2Speed","13","0","Create","True","0","0","0","False","0","False","Object","-1","","1","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":673,"pos":[-3152,1008],"params":["Inherit","False","669","oceanTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":96,"pos":[-3600,592],"params":["Inherit","False","wave1Number","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":143,"pos":[-3600,1112],"params":["Inherit","False","wave2Number","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":436,"pos":[216,2088],"params":["Inherit","False","88","wave1Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":432,"pos":[216,1968],"params":["Inherit","False","94","wave1Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":455,"pos":[216,2216],"params":["Inherit","False","102","wave2Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":449,"pos":[216,2336],"params":["Inherit","False","101","wave2Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":468,"pos":[216,2464],"params":["Inherit","False","108","wave3Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":462,"pos":[216,2584],"params":["Inherit","False","107","wave3Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor","id":260,"pos":[-3504,1432],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":676,"pos":[-3152,1544],"params":["Inherit","False","669","oceanTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":279,"pos":[-3152,1648],"params":["Inherit","False","Property","_Wave3Speed","Wave3Speed","14","0","Create","True","0","0","0","False","0","False","Object","-1","","1.5","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":277,"pos":[-3600,1640],"params":["Inherit","False","wave3Number","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":17,"pos":[-3368,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":147,"pos":[-3368,904],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":670,"pos":[-2960,496],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":672,"pos":[-2960,1008],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":303,"pos":[-3968,2216],"params":["Inherit","False","102","wave2Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":307,"pos":[-3968,2336],"params":["Inherit","False","101","wave2Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":309,"pos":[-3504,2336],"params":["Inherit","False","100","wave2Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":323,"pos":[-3968,2464],"params":["Inherit","False","108","wave3Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":324,"pos":[-3968,2584],"params":["Inherit","False","107","wave3Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":326,"pos":[-3504,2584],"params":["Inherit","False","105","wave3Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":289,"pos":[-3968,1960],"params":["Inherit","False","94","wave1Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":291,"pos":[-3968,2080],"params":["Inherit","False","88","wave1Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":293,"pos":[-3504,2080],"params":["Inherit","False","85","wave1Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":437,"pos":[448,2088],"params":["Inherit","False","96","wave1Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":438,"pos":[680,2088],"params":["Inherit","False","85","wave1Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":452,"pos":[448,2336],"params":["Inherit","False","143","wave2Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":453,"pos":[680,2336],"params":["Inherit","False","100","wave2Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":465,"pos":[448,2584],"params":["Inherit","False","277","wave3Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":466,"pos":[680,2584],"params":["Inherit","False","105","wave3Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":433,"pos":[448,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":450,"pos":[448,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":463,"pos":[448,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":431,"pos":[-1704,1912],"params":["Inherit","False","1836","811","TangentY","31","391","395","396","398","400","394","392","402","403","404","393","406","408","410","412","413","418","420","422","424","425","416","411","407","409","430","405","428","423","419","421","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":26,"pos":[-3968,72],"params":["Inherit","False","Property","_Wave1Amplitude","Wave1Amplitude","0","0","Create","True","0","0","0","False","0","False","Object","-1","","0.025","0.5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":97,"pos":[-3472,72],"params":["Inherit","False","Property","_Wave2Amplitude","Wave2Amplitude","1","0","Create","True","0","0","0","False","0","False","Object","-1","","0.0125","0.5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":263,"pos":[-3368,1432],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":674,"pos":[-2960,1544],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":24,"pos":[-2800,384],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":148,"pos":[-2800,904],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":310,"pos":[-3272,2336],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":308,"pos":[-3736,2336],"params":["Inherit","False","143","wave2Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":319,"pos":[-3272,2584],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":325,"pos":[-3736,2584],"params":["Inherit","False","277","wave3Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":292,"pos":[-3736,2080],"params":["Inherit","False","96","wave1Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":294,"pos":[-3272,2080],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":439,"pos":[912,2088],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":454,"pos":[912,2336],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":467,"pos":[912,2584],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":304,"pos":[-3736,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":316,"pos":[-3736,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":290,"pos":[-3736,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":434,"pos":[680,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":451,"pos":[680,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":464,"pos":[680,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":98,"pos":[-3472,176],"params":["Inherit","False","Property","_Wave2Steepness","Wave2Steepness","10","0","Create","True","0","0","0","False","0","False","Object","-1","","4","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":106,"pos":[-2976,72],"params":["Inherit","False","Property","_Wave3Amplitude","Wave3Amplitude","2","0","Create","True","0","0","0","False","0","False","Object","-1","","0.00625","0.5","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":45,"pos":[-3968,176],"params":["Inherit","False","Property","_Wave1Steepness","Wave1Steepness","9","0","Create","True","0","0","0","False","0","False","Object","-1","","8","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":88,"pos":[-3736,72],"params":["Inherit","False","wave1Amplitude","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":101,"pos":[-3240,72],"params":["Inherit","False","wave2Amplitude","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":264,"pos":[-2800,1432],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":91,"pos":[-2632,384],"params":["Inherit","False","wave1Phase","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":149,"pos":[-2632,904],"params":["Inherit","False","wave2Phase","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":311,"pos":[-3136,2336],"params":["Inherit","False","Square","-1","","3","fea980a1f68019543b2cd91d506986e8","0","1","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":312,"pos":[-2968,2336],"params":["Inherit","False","149","wave2Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":320,"pos":[-3136,2584],"params":["Inherit","False","Square","-1","","4","fea980a1f68019543b2cd91d506986e8","0","1","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":327,"pos":[-2968,2584],"params":["Inherit","False","280","wave3Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor","id":295,"pos":[-3136,2080],"params":["Inherit","False","Square","-1","","5","fea980a1f68019543b2cd91d506986e8","0","1","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":395,"pos":[-1424,2080],"params":["Inherit","False","85","wave1Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":394,"pos":[-1656,2080],"params":["Inherit","False","96","wave1Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":393,"pos":[-1656,1960],"params":["Inherit","False","88","wave1Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":416,"pos":[-1656,2216],"params":["Inherit","False","101","wave2Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":411,"pos":[-1656,2336],"params":["Inherit","False","143","wave2Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":407,"pos":[-1424,2336],"params":["Inherit","False","100","wave2Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":428,"pos":[-1656,2464],"params":["Inherit","False","107","wave3Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":423,"pos":[-1656,2584],"params":["Inherit","False","277","wave3Number","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":419,"pos":[-1424,2584],"params":["Inherit","False","105","wave3Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":445,"pos":[1080,2088],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":440,"pos":[1216,2088],"params":["Inherit","False","91","wave1Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":458,"pos":[1080,2336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":459,"pos":[1216,2336],"params":["Inherit","False","149","wave2Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":471,"pos":[1080,2584],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":472,"pos":[1216,2584],"params":["Inherit","False","280","wave3Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":297,"pos":[-2968,2080],"params":["Inherit","False","91","wave1Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":305,"pos":[-3504,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":317,"pos":[-3504,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":299,"pos":[-3504,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":435,"pos":[1048,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":456,"pos":[1048,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":469,"pos":[1048,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":103,"pos":[-2976,176],"params":["Inherit","False","Property","_Wave3Steepness","Wave3Steepness","11","0","Create","True","0","0","0","False","0","False","Object","-1","","2","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":94,"pos":[-3736,176],"params":["Inherit","False","wave1Steepness","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":102,"pos":[-3240,176],"params":["Inherit","False","wave2Steepness","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":107,"pos":[-2744,72],"params":["Inherit","False","wave3Amplitude","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":280,"pos":[-2632,1432],"params":["Inherit","False","wave3Phase","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":313,"pos":[-2768,2336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":321,"pos":[-2768,2584],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":296,"pos":[-2768,2080],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":396,"pos":[-1192,2080],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":398,"pos":[-888,2080],"params":["Inherit","False","91","wave1Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":408,"pos":[-1192,2336],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":420,"pos":[-1192,2584],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":409,"pos":[-888,2336],"params":["Inherit","False","149","wave2Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":421,"pos":[-888,2584],"params":["Inherit","False","280","wave3Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":441,"pos":[1416,2088],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":460,"pos":[1416,2336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":473,"pos":[1416,2584],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":306,"pos":[-2968,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":318,"pos":[-2968,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":300,"pos":[-2968,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":391,"pos":[-1424,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":406,"pos":[-1424,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":418,"pos":[-1424,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":444,"pos":[1216,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":457,"pos":[1216,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":470,"pos":[1216,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":40,"pos":[-2392,384],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":90,"pos":[-2488,496],"params":["Inherit","False","88","wave1Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":150,"pos":[-2392,904],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":152,"pos":[-2488,1016],"params":["Inherit","False","101","wave2Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":108,"pos":[-2744,176],"params":["Inherit","False","wave3Steepness","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":402,"pos":[-688,2080],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":413,"pos":[-688,2336],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":425,"pos":[-688,2584],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":314,"pos":[-2632,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":322,"pos":[-2632,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":302,"pos":[-2632,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":392,"pos":[-1056,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":412,"pos":[-1056,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":424,"pos":[-1056,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":442,"pos":[1552,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":461,"pos":[1552,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":474,"pos":[1552,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":95,"pos":[-2216,496],"params":["Inherit","False","94","wave1Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":163,"pos":[-2216,1016],"params":["Inherit","False","102","wave2Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor","id":265,"pos":[-2392,1432],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":281,"pos":[-2488,1544],"params":["Inherit","False","107","wave3Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":41,"pos":[-2256,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":151,"pos":[-2256,904],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":329,"pos":[-2456,2464],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":475,"pos":[1720,2464],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":315,"pos":[-2456,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":400,"pos":[-552,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":410,"pos":[-552,2216],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":422,"pos":[-552,2464],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":446,"pos":[1720,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":87,"pos":[-1920,496],"params":["Inherit","False","85","wave1Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":93,"pos":[-1888,600],"params":["Inherit","False","91","wave1Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":155,"pos":[-1920,1016],"params":["Inherit","False","100","wave2Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":159,"pos":[-1888,1120],"params":["Inherit","False","149","wave2Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":282,"pos":[-2216,1544],"params":["Inherit","False","108","wave3Steepness","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":46,"pos":[-1984,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":153,"pos":[-1984,904],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":266,"pos":[-2256,1432],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":430,"pos":[-384,2464],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":328,"pos":[-2312,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":403,"pos":[-384,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":447,"pos":[1856,1968],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":25,"pos":[-1696,600],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":89,"pos":[-1792,704],"params":["Inherit","False","88","wave1Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":161,"pos":[-1696,1120],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":162,"pos":[-1792,1224],"params":["Inherit","False","101","wave2Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":283,"pos":[-1920,1544],"params":["Inherit","False","105","wave3Direction","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":284,"pos":[-1888,1648],"params":["Inherit","False","280","wave3Phase","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":49,"pos":[-1696,384],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":154,"pos":[-1696,904],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":267,"pos":[-1984,1432],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor","id":330,"pos":[-2168,1960],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.NegateNode, AmplifyShaderEditor","id":448,"pos":[1992,1968],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":404,"pos":[-248,1960],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":53,"pos":[-1528,384],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":156,"pos":[-1528,904],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor","id":272,"pos":[-1696,1648],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":285,"pos":[-1792,1752],"params":["Inherit","False","107","wave3Amplitude","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":27,"pos":[-1560,600],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":158,"pos":[-1560,1120],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":268,"pos":[-1696,1432],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":542,"pos":[-4016,-1144],"params":["Inherit","False","1004","651","Vertex Normal","11","533","531","530","525","526","527","532","528","534","535","540","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":332,"pos":[-1992,1960],"params":["Inherit","False","tangentX","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":405,"pos":[-112,1960],"params":["Inherit","False","tangentY","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":477,"pos":[2160,1968],"params":["Inherit","False","tangentZ","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":28,"pos":[-1392,384],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":157,"pos":[-1392,904],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor","id":269,"pos":[-1528,1432],"params":["Inherit","False","FLOAT2","1","0","FLOAT2","0,0","False","16","FLOAT","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT","5","FLOAT","6","FLOAT","7","FLOAT","8","FLOAT","9","FLOAT","10","FLOAT","11","FLOAT","12","FLOAT","13","FLOAT","14","FLOAT","15"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":271,"pos":[-1560,1648],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":539,"pos":[-4016,-456],"params":["Inherit","False","836","283","Vertex Offset","6","56","168","287","288","537","169","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":533,"pos":[-3960,-904],"params":["Inherit","False","332","tangentX","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":531,"pos":[-3960,-1000],"params":["Inherit","False","405","tangentY","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":530,"pos":[-3960,-1096],"params":["Inherit","False","477","tangentZ","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":525,"pos":[-3960,-800],"params":["Inherit","False","332","tangentX","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":526,"pos":[-3960,-704],"params":["Inherit","False","405","tangentY","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":527,"pos":[-3960,-608],"params":["Inherit","False","477","tangentZ","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":55,"pos":[-1224,384],"params":["Inherit","False","wave1Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":160,"pos":[-1224,904],"params":["Inherit","False","wave2Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":270,"pos":[-1392,1432],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":532,"pos":[-3752,-1096],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":528,"pos":[-3752,-800],"params":["Inherit","False","FLOAT3","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":286,"pos":[-1224,1432],"params":["Inherit","False","wave3Offset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":56,"pos":[-3968,-400],"params":["Inherit","False","55","wave1Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":169,"pos":[-3968,-280],"params":["Inherit","False","160","wave2Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.CrossProductOpNode, AmplifyShaderEditor","id":534,"pos":[-3584,-1096],"params":["Inherit","False","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":287,"pos":[-3768,-280],"params":["Inherit","False","286","wave3Offset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":168,"pos":[-3768,-400],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor","id":535,"pos":[-3416,-1096],"params":["Inherit","False","False","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":288,"pos":[-3560,-400],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":608,"pos":[-4016,-3104],"params":["Inherit","False","1612","795","Foam","15","598","590","591","607","597","596","599","605","588","589","593","594","636","666","679","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":637,"pos":[-4016,-3880],"params":["Inherit","False","1860","739","Base Color","14","609","54","580","610","592","612","611","544","550","570","565","564","567","551","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":571,"pos":[-4016,-1832],"params":["Inherit","False","956","323","Depth Fade","6","549","547","546","545","548","569","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":568,"pos":[-4016,-1472],"params":["Inherit","False","1668","283","Horizon Emission","9","554","562","555","557","561","560","559","556","558","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor","id":585,"pos":[-4016,-2272],"params":["Inherit","False","1532","403","Shadow","9","575","578","581","577","573","584","667","677","678","","1,1,1,1","0","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":540,"pos":[-3248,-1096],"params":["Inherit","False","vertexNormal","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":537,"pos":[-3424,-400],"params":["Inherit","False","vertexOffset","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor","id":576,"pos":[-3120,-400],"params":["Inherit","False","0","-1","2","3","2","SAMPLER2D","","False","0","FLOAT2","1,1","False","1","FLOAT2","0,0","False","5","FLOAT2","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":595,"pos":[-3088,-256],"params":["Inherit","False","Property","_PixelDensity","PixelDensity","19","0","Create","True","0","0","0","False","0","False","Object","-1","","1800","1800","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":661,"pos":[-2896,-400],"params":["Inherit","False","2","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.FloorOpNode, AmplifyShaderEditor","id":662,"pos":[-2736,-400],"params":["Inherit","False","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RelayNode, AmplifyShaderEditor","id":663,"pos":[-2736,-256],"params":["Inherit","False","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":599,"pos":[-3800,-2704],"params":["Inherit","False","Property","_FoamTileSpeed","FoamTileSpeed","24","0","Create","True","0","0","0","False","0","False","Object","-1","","0.001","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":636,"pos":[-3800,-2800],"params":["Inherit","False","635","currentTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":664,"pos":[-2600,-400],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":665,"pos":[-2432,-400],"params":["Inherit","False","uv","-1","True","1","0","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":596,"pos":[-3600,-2800],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":679,"pos":[-3824,-2608],"params":["Inherit","False","669","oceanTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":605,"pos":[-3856,-2512],"params":["Inherit","False","Property","_FoamRotateSpeed","FoamRotateSpeed","25","0","Create","True","0","0","0","False","0","False","Object","-1","","0.1","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":597,"pos":[-3440,-2800],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.PiNode, AmplifyShaderEditor","id":593,"pos":[-3464,-2496],"params":["Inherit","False","1","0","FLOAT","2","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":666,"pos":[-3472,-3056],"params":["Inherit","False","665","uv","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":588,"pos":[-3632,-2608],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":598,"pos":[-3264,-3056],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.SimpleRemainderNode, AmplifyShaderEditor","id":589,"pos":[-3264,-2608],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":594,"pos":[-3264,-2496],"params":["Inherit","False","Property","_FoamScale","FoamScale","23","0","Create","True","0","0","0","False","0","False","Object","-1","","100","100","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.VoronoiNode, AmplifyShaderEditor","id":590,"pos":[-3096,-3056],"params":["Inherit","True","0","1","1","0","2","False","1","False","False","False","4","0","FLOAT2","0,0","False","1","FLOAT","45","False","2","FLOAT","100","False","3","FLOAT","0","False","3","FLOAT","0","FLOAT2","1","FLOAT2","2"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":591,"pos":[-2896,-3056],"params":["Inherit","True","3","0","FLOAT","0","False","1","FLOAT","0.25","False","2","FLOAT","0.55","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":607,"pos":[-2648,-3056],"params":["Inherit","False","foamBlend","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":552,"pos":[0,-8],"params":["Inherit","False","551","baseColor","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor","id":555,"pos":[-3576,-1416],"params":["Inherit","False","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PowerNode, AmplifyShaderEditor","id":557,"pos":[-3224,-1416],"params":["Inherit","False","False","2","0","FLOAT","0","False","1","FLOAT","3","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LengthOpNode, AmplifyShaderEditor","id":561,"pos":[-3752,-1416],"params":["Inherit","False","1","0","FLOAT3","0,0,0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor","id":560,"pos":[-3968,-1416],"params":["Inherit","False","0","4","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":559,"pos":[-3784,-1296],"params":["Inherit","False","Property","_OceanRadius","OceanRadius","28","0","Create","True","0","0","0","False","0","False","Object","-1","","50","50","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":556,"pos":[-3416,-1296],"params":["Inherit","False","Property","_HorizonPower","HorizonPower","30","0","Create","True","0","0","0","False","0","False","Object","-1","","3","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor","id":549,"pos":[-3960,-1776],"params":["Inherit","False","0","0","5","FLOAT3","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":546,"pos":[-3760,-1616],"params":["Inherit","False","Property","_DepthDistance","DepthDistance","27","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5","1","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DepthFade, AmplifyShaderEditor","id":545,"pos":[-3560,-1776],"params":["Inherit","False","True","True","True","2","1","FLOAT3","0,0,0","False","0","FLOAT","1","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":548,"pos":[-3960,-1616],"params":["Inherit","False","537","vertexOffset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":569,"pos":[-3296,-1776],"params":["Inherit","False","depthBlend","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor","id":575,"pos":[-3504,-2072],"params":["Inherit","False","FLOAT2","4","0","FLOAT","0","False","1","FLOAT","0","False","2","FLOAT","0","False","3","FLOAT","0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.VoronoiNode, AmplifyShaderEditor","id":578,"pos":[-3168,-2216],"params":["Inherit","True","0","0","1","0","2","False","1","False","False","False","4","0","FLOAT2","0,0","False","1","FLOAT","1.7","False","2","FLOAT","25","False","3","FLOAT","0","False","3","FLOAT","0","FLOAT2","1","FLOAT2","2"]}
{"type":"AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor","id":581,"pos":[-2968,-2216],"params":["Inherit","True","3","0","FLOAT","0","False","1","FLOAT","0.2","False","2","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor","id":577,"pos":[-3336,-2216],"params":["Inherit","False","2","0","FLOAT2","0,0","False","1","FLOAT2","0,0","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":584,"pos":[-2728,-2216],"params":["Inherit","False","shadowBlend","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":609,"pos":[-3728,-3608],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":54,"pos":[-3968,-3824],"params":["Inherit","False","Constant","_WaterColor","WaterColor","1","0","Create","True","0","0","0","False","0","False","Object","-1","","0.3921569,0.7294118,0.7686275,0","0.3921569,0.7294118,0.7686275,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":580,"pos":[-3968,-3608],"params":["Inherit","False","Property","_ShadowColor","ShadowColor","20","0","Create","True","0","0","0","False","0","False","Object","-1","","0.3137255,0.5764706,0.6588235,0","0.2352652,0.6053795,0.6528301,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":610,"pos":[-3968,-3392],"params":["Inherit","False","584","shadowBlend","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":592,"pos":[-3568,-3464],"params":["Inherit","False","Property","_FoamColor","FoamColor","22","0","Create","True","0","0","0","False","0","False","Object","-1","","0.5960785,0.8431373,0.8156863,0","0.5145389,0.7958222,0.8339623,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":612,"pos":[-3536,-3248],"params":["Inherit","False","607","foamBlend","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":611,"pos":[-3336,-3608],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":544,"pos":[-3176,-3824],"params":["Inherit","False","Property","_DepthColor","DepthColor","26","0","Create","True","0","0","0","False","0","False","Object","-1","","0.7764706,0.8745098,0.8862745,0","1,1,1,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":550,"pos":[-2944,-3608],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":570,"pos":[-3144,-3464],"params":["Inherit","False","569","depthBlend","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.LerpOp, AmplifyShaderEditor","id":565,"pos":[-2568,-3608],"params":["Inherit","False","3","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","2","FLOAT","0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.ColorNode, AmplifyShaderEditor","id":564,"pos":[-2784,-3472],"params":["Inherit","False","Property","_HorizonColor","HorizonColor","29","0","Create","True","0","0","0","False","0","False","Object","-1","","0.9098039,1,0.5803922,0","1,0.9817406,0.4858491,0","True","True","0","6","COLOR","0","FLOAT","1","FLOAT","2","FLOAT","3","FLOAT","4","FLOAT3","5"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":567,"pos":[-2752,-3264],"params":["Inherit","False","562","emission","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":551,"pos":[-2400,-3608],"params":["Inherit","False","baseColor","-1","True","1","0","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":667,"pos":[-3536,-2216],"params":["Inherit","False","665","uv","1","0","OBJECT","","False","1","FLOAT2","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":573,"pos":[-3904,-1984],"params":["Inherit","False","Property","_ShadowTileSpeed","ShadowTileSpeed","21","0","Create","True","0","0","0","False","0","False","Object","-1","","0.001","0","0","0","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":554,"pos":[-2760,-1416],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor","id":547,"pos":[-3760,-1776],"params":["Inherit","False","2","2","0","FLOAT3","0,0,0","False","1","FLOAT3","0,0,0","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor","id":677,"pos":[-3672,-2072],"params":["Inherit","False","2","2","0","FLOAT","0","False","1","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":678,"pos":[-3872,-2072],"params":["Inherit","False","669","oceanTime","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor","id":558,"pos":[-3064,-1296],"params":["Inherit","False","Property","_HorizonEmission","HorizonEmission","31","0","Create","True","0","0","0","False","0","False","Object","-1","","1","0","0","1","0","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":563,"pos":[0,112],"params":["Inherit","False","562","emission","1","0","OBJECT","","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":538,"pos":[0,160],"params":["Inherit","False","537","vertexOffset","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor","id":541,"pos":[-32,184],"params":["Inherit","False","540","vertexNormal","1","0","OBJECT","","False","1","FLOAT3","0"]}
{"type":"AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor","id":562,"pos":[-2592,-1416],"params":["Inherit","False","emission","-1","True","1","0","FLOAT","0","False","1","FLOAT","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":1,"pos":[200,-8],"params":["Float","False","True","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","Custom/Ocean2","94348b07e5e8bab40bd6c8a1e3df54cd","True","Forward","0","1","Forward","22","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","2","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Transparent=RenderType","Queue=Transparent=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","5","False","","10","False","","1","1","False","","10","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalForward","False","False","0","","0","0","Standard","52","Category","0","0","  Instanced Terrain Normals","1","0","Lighting Model","0","0","Workflow","1","0","Surface","1","639278976688766176","  Keep Alpha","0","0","  Refraction Model","0","0","  Blend","0","0","Two Sided","1","0","Alpha Clipping","0","0","  Use Shadow Threshold","0","0","Fragment Normal Space","0","0","Forward Only","0","0","Transmission","0","0","  Transmission Shadow","0.5,False,","0","Translucency","0","0","  Translucency Strength","1,False,","0","  Normal Distortion","0.5,False,","0","  Scattering","2,False,","0","  Direct","0.9,False,","0","  Ambient","0.1,False,","0","  Shadow","0.5,False,","0","Cast Shadows","0","639278904560964071","Receive Shadows","2","0","Specular Highlights","2","0","Environment Reflections","2","0","Receive SSAO","1","0","Motion Vectors","0","0","  Additional Motion Vectors","1","0","  Alembic Motion Vectors","0","0","  XR Motion Vectors","0","0","GPU Instancing","1","0","LOD CrossFade","1","0","Built-in Fog","1","0","_FinalColorxAlpha","0","0","Meta Pass","1","0","Override Baked GI","0","0","Extra Pre Pass","0","0","Tessellation","1","639278201039166366","  Phong","1","639278903198082745","  Strength","0.5,False,","0","  Type","0","639278955187925925","  Tess","8,False,","0","  Min","10,False,","0","  Max","25,False,","0","  Edge Length","16,False,","0","  Max Displacement","25,False,","0","Write Depth","0","0","  Conservative","0","0","Vertex Position","1","0","Debug Display","1","0","Clear Coat","0","0","0","12","False","True","False","True","True","True","True","True","True","True","False","False","False","","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":2,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ShadowCaster","0","2","ShadowCaster","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","False","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=ShadowCaster","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":3,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthOnly","0","3","DepthOnly","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","True","True","False","False","False","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","False","False","False","True","1","LightMode=DepthOnly","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":4,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Meta","0","4","Meta","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Meta","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":5,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","Universal2D","0","5","Universal2D","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","5","False","","10","False","","1","1","False","","10","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=Universal2D","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":6,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","DepthNormals","0","6","DepthNormals","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","False","","True","3","False","","False","False","True","1","LightMode=DepthNormals","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":7,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","GBuffer","0","7","GBuffer","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","5","False","","10","False","","1","1","False","","10","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","1","LightMode=UniversalGBuffer","False","True","12","d3d11","gles","metal","vulkan","xboxone","xboxseries","playstation","ps4","ps5","switch","switch2","webgpu","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":8,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","SceneSelectionPass","0","8","SceneSelectionPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","2","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=SceneSelectionPass","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":9,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ScenePickingPass","0","9","ScenePickingPass","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=Picking","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":10,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","MotionVectors","0","10","MotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","False","False","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","False","True","1","LightMode=MotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":11,"pos":[0,0],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","XRMotionVectors","0","11","XRMotionVectors","0","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","True","1","False","","255","False","","1","False","","7","False","","3","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","False","False","False","False","True","1","LightMode=XRMotionVectors","False","False","0","","0","0","Standard","0","False","0"]}
{"type":"AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor","id":0,"pos":[-1440,704],"params":["Float","False","False","-1","3","UnityEditor.ShaderGraphLitGUI","0","15","New Amplify Shader","94348b07e5e8bab40bd6c8a1e3df54cd","True","ExtraPrePass","0","0","ExtraPrePass","6","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","0","False","","False","False","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","4","RenderPipeline=UniversalPipeline","RenderType=Opaque=RenderType","Queue=Geometry=Queue=0","UniversalMaterialType=Lit","True","5","True","14","all","0","False","True","1","1","False","","0","False","","0","1","False","","0","False","","False","False","False","False","False","False","False","False","False","False","False","False","True","0","False","","False","True","True","True","True","True","0","False","","False","False","False","False","False","False","False","True","False","0","False","","255","False","","255","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","0","False","","False","True","1","False","","True","3","False","","True","True","0","False","","0","False","","False","True","0","False","False","0","","0","0","Standard","0","False","0"]}
{"wire":[635,0,600,0]}
{"wire":[653,0,651,0]}
{"wire":[653,1,654,0]}
{"wire":[649,0,640,1]}
{"wire":[649,1,640,3]}
{"wire":[652,0,653,0]}
{"wire":[652,1,646,0]}
{"wire":[660,0,649,0]}
{"wire":[660,1,652,0]}
{"wire":[638,0,660,0]}
{"wire":[85,0,13,0]}
{"wire":[100,0,99,0]}
{"wire":[20,0,19,0]}
{"wire":[20,1,18,0]}
{"wire":[146,0,144,0]}
{"wire":[146,1,145,0]}
{"wire":[105,0,104,0]}
{"wire":[669,0,668,0]}
{"wire":[16,0,639,0]}
{"wire":[16,1,86,0]}
{"wire":[142,0,643,0]}
{"wire":[142,1,141,0]}
{"wire":[262,0,261,0]}
{"wire":[262,1,278,0]}
{"wire":[96,0,20,0]}
{"wire":[143,0,146,0]}
{"wire":[260,0,645,0]}
{"wire":[260,1,276,0]}
{"wire":[277,0,262,0]}
{"wire":[17,0,16,0]}
{"wire":[17,1,96,0]}
{"wire":[147,0,142,0]}
{"wire":[147,1,143,0]}
{"wire":[670,0,671,0]}
{"wire":[670,1,22,0]}
{"wire":[672,0,673,0]}
{"wire":[672,1,165,0]}
{"wire":[433,0,432,0]}
{"wire":[433,1,436,0]}
{"wire":[450,0,455,0]}
{"wire":[450,1,449,0]}
{"wire":[463,0,468,0]}
{"wire":[463,1,462,0]}
{"wire":[263,0,260,0]}
{"wire":[263,1,277,0]}
{"wire":[674,0,676,0]}
{"wire":[674,1,279,0]}
{"wire":[24,0,17,0]}
{"wire":[24,1,670,0]}
{"wire":[148,0,147,0]}
{"wire":[148,1,672,0]}
{"wire":[310,0,309,0]}
{"wire":[319,0,326,0]}
{"wire":[294,0,293,0]}
{"wire":[439,0,438,0]}
{"wire":[454,0,453,0]}
{"wire":[467,0,466,0]}
{"wire":[304,0,303,0]}
{"wire":[304,1,307,0]}
{"wire":[316,0,323,0]}
{"wire":[316,1,324,0]}
{"wire":[290,0,289,0]}
{"wire":[290,1,291,0]}
{"wire":[434,0,433,0]}
{"wire":[434,1,437,0]}
{"wire":[451,0,450,0]}
{"wire":[451,1,452,0]}
{"wire":[464,0,463,0]}
{"wire":[464,1,465,0]}
{"wire":[88,0,26,0]}
{"wire":[101,0,97,0]}
{"wire":[264,0,263,0]}
{"wire":[264,1,674,0]}
{"wire":[91,0,24,0]}
{"wire":[149,0,148,0]}
{"wire":[311,2,310,0]}
{"wire":[320,2,319,0]}
{"wire":[295,2,294,0]}
{"wire":[445,0,439,1]}
{"wire":[458,0,454,1]}
{"wire":[471,0,467,1]}
{"wire":[305,0,304,0]}
{"wire":[305,1,308,0]}
{"wire":[317,0,316,0]}
{"wire":[317,1,325,0]}
{"wire":[299,0,290,0]}
{"wire":[299,1,292,0]}
{"wire":[435,0,434,0]}
{"wire":[435,1,439,0]}
{"wire":[456,0,451,0]}
{"wire":[456,1,454,0]}
{"wire":[469,0,464,0]}
{"wire":[469,1,467,0]}
{"wire":[94,0,45,0]}
{"wire":[102,0,98,0]}
{"wire":[107,0,106,0]}
{"wire":[280,0,264,0]}
{"wire":[313,0,312,0]}
{"wire":[321,0,327,0]}
{"wire":[296,0,297,0]}
{"wire":[396,0,395,0]}
{"wire":[408,0,407,0]}
{"wire":[420,0,419,0]}
{"wire":[441,0,440,0]}
{"wire":[460,0,459,0]}
{"wire":[473,0,472,0]}
{"wire":[306,0,305,0]}
{"wire":[306,1,311,0]}
{"wire":[318,0,317,0]}
{"wire":[318,1,320,0]}
{"wire":[300,0,299,0]}
{"wire":[300,1,295,0]}
{"wire":[391,0,393,0]}
{"wire":[391,1,394,0]}
{"wire":[406,0,416,0]}
{"wire":[406,1,411,0]}
{"wire":[418,0,428,0]}
{"wire":[418,1,423,0]}
{"wire":[444,0,435,0]}
{"wire":[444,1,445,0]}
{"wire":[457,0,456,0]}
{"wire":[457,1,458,0]}
{"wire":[470,0,469,0]}
{"wire":[470,1,471,0]}
{"wire":[40,0,91,0]}
{"wire":[150,0,149,0]}
{"wire":[108,0,103,0]}
{"wire":[402,0,398,0]}
{"wire":[413,0,409,0]}
{"wire":[425,0,421,0]}
{"wire":[314,0,306,0]}
{"wire":[314,1,313,0]}
{"wire":[322,0,318,0]}
{"wire":[322,1,321,0]}
{"wire":[302,0,300,0]}
{"wire":[302,1,296,0]}
{"wire":[392,0,391,0]}
{"wire":[392,1,396,0]}
{"wire":[412,0,406,0]}
{"wire":[412,1,408,0]}
{"wire":[424,0,418,0]}
{"wire":[424,1,420,0]}
{"wire":[442,0,444,0]}
{"wire":[442,1,441,0]}
{"wire":[461,0,457,0]}
{"wire":[461,1,460,0]}
{"wire":[474,0,470,0]}
{"wire":[474,1,473,0]}
{"wire":[265,0,280,0]}
{"wire":[41,0,40,0]}
{"wire":[41,1,90,0]}
{"wire":[151,0,150,0]}
{"wire":[151,1,152,0]}
{"wire":[329,0,322,0]}
{"wire":[475,0,474,0]}
{"wire":[315,0,302,0]}
{"wire":[315,1,314,0]}
{"wire":[400,0,392,0]}
{"wire":[400,1,402,0]}
{"wire":[410,0,412,0]}
{"wire":[410,1,413,0]}
{"wire":[422,0,424,0]}
{"wire":[422,1,425,0]}
{"wire":[446,0,442,0]}
{"wire":[446,1,461,0]}
{"wire":[46,0,41,0]}
{"wire":[46,1,95,0]}
{"wire":[153,0,151,0]}
{"wire":[153,1,163,0]}
{"wire":[266,0,265,0]}
{"wire":[266,1,281,0]}
{"wire":[430,0,422,0]}
{"wire":[328,0,315,0]}
{"wire":[328,1,329,0]}
{"wire":[403,0,400,0]}
{"wire":[403,1,410,0]}
{"wire":[447,0,446,0]}
{"wire":[447,1,475,0]}
{"wire":[25,0,93,0]}
{"wire":[161,0,159,0]}
{"wire":[49,0,46,0]}
{"wire":[49,1,87,0]}
{"wire":[154,0,153,0]}
{"wire":[154,1,155,0]}
{"wire":[267,0,266,0]}
{"wire":[267,1,282,0]}
{"wire":[330,0,328,0]}
{"wire":[448,0,447,0]}
{"wire":[404,0,403,0]}
{"wire":[404,1,430,0]}
{"wire":[53,0,49,0]}
{"wire":[156,0,154,0]}
{"wire":[272,0,284,0]}
{"wire":[27,0,25,0]}
{"wire":[27,1,89,0]}
{"wire":[158,0,161,0]}
{"wire":[158,1,162,0]}
{"wire":[268,0,267,0]}
{"wire":[268,1,283,0]}
{"wire":[332,0,330,0]}
{"wire":[405,0,404,0]}
{"wire":[477,0,448,0]}
{"wire":[28,0,53,0]}
{"wire":[28,1,27,0]}
{"wire":[28,2,53,1]}
{"wire":[157,0,156,0]}
{"wire":[157,1,158,0]}
{"wire":[157,2,156,1]}
{"wire":[269,0,268,0]}
{"wire":[271,0,272,0]}
{"wire":[271,1,285,0]}
{"wire":[55,0,28,0]}
{"wire":[160,0,157,0]}
{"wire":[270,0,269,0]}
{"wire":[270,1,271,0]}
{"wire":[270,2,269,1]}
{"wire":[532,0,530,0]}
{"wire":[532,1,531,0]}
{"wire":[532,2,533,0]}
{"wire":[528,0,525,0]}
{"wire":[528,1,526,0]}
{"wire":[528,2,527,0]}
{"wire":[286,0,270,0]}
{"wire":[534,0,532,0]}
{"wire":[534,1,528,0]}
{"wire":[168,0,56,0]}
{"wire":[168,1,169,0]}
{"wire":[535,0,534,0]}
{"wire":[288,0,168,0]}
{"wire":[288,1,287,0]}
{"wire":[540,0,535,0]}
{"wire":[537,0,288,0]}
{"wire":[661,0,576,0]}
{"wire":[661,1,595,0]}
{"wire":[662,0,661,0]}
{"wire":[663,0,595,0]}
{"wire":[664,0,662,0]}
{"wire":[664,1,663,0]}
{"wire":[665,0,664,0]}
{"wire":[596,0,636,0]}
{"wire":[596,1,599,0]}
{"wire":[597,1,596,0]}
{"wire":[588,0,679,0]}
{"wire":[588,1,605,0]}
{"wire":[598,0,666,0]}
{"wire":[598,1,597,0]}
{"wire":[589,0,588,0]}
{"wire":[589,1,593,0]}
{"wire":[590,0,598,0]}
{"wire":[590,1,589,0]}
{"wire":[590,2,594,0]}
{"wire":[591,0,590,0]}
{"wire":[607,0,591,0]}
{"wire":[555,0,561,0]}
{"wire":[555,1,559,0]}
{"wire":[557,0,555,0]}
{"wire":[557,1,556,0]}
{"wire":[561,0,560,0]}
{"wire":[545,1,547,0]}
{"wire":[545,0,546,0]}
{"wire":[569,0,545,0]}
{"wire":[575,1,677,0]}
{"wire":[578,0,577,0]}
{"wire":[581,0,578,0]}
{"wire":[577,0,667,0]}
{"wire":[577,1,575,0]}
{"wire":[584,0,581,0]}
{"wire":[609,0,54,5]}
{"wire":[609,1,580,5]}
{"wire":[609,2,610,0]}
{"wire":[611,0,609,0]}
{"wire":[611,1,592,5]}
{"wire":[611,2,612,0]}
{"wire":[550,0,544,5]}
{"wire":[550,1,611,0]}
{"wire":[550,2,570,0]}
{"wire":[565,0,550,0]}
{"wire":[565,1,564,5]}
{"wire":[565,2,567,0]}
{"wire":[551,0,565,0]}
{"wire":[554,0,557,0]}
{"wire":[554,1,558,0]}
{"wire":[547,0,549,0]}
{"wire":[547,1,548,0]}
{"wire":[677,0,678,0]}
{"wire":[677,1,573,0]}
{"wire":[562,0,554,0]}
{"wire":[1,0,552,0]}
{"wire":[1,2,563,0]}
{"wire":[1,8,538,0]}
{"wire":[1,10,541,0]}
ASEEND*/
//CHKSM=62C41F58C04F1594EBCC66044BDE196E55697A9B