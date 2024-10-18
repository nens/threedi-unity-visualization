Shader "GISTech/GISTerrainLoaderWaterShader"
{
	Properties
	{
	    [HideInInspector]_TerrainScale("Terrain Scale", Float) = 1
		[NoScaleOffset] _WaterTexture("Water Texture", 2D) = "white" {}
		[NoScaleOffset] _Noise ("Water Noise", 2D) = "white" {}
		[NoScaleOffset] _WaveNormal ("Wave Normal", 2D) = "bump" {}
 	   
		_WaterColor ("Water Color", Color) = (1,1,1,1)
	    _FakeUnderwaterColor ("Under Water Color", Color) = (0.196, 0.262, 0.196, 1)
 		_Ambient("Ambient", Color) = (0,0,0,0)
		_FresnelCol("Fresnel Col", Color) = (0,0,0,0)
	    _SurfaceColor ("SurfaceColor", Color) = (1,1,1,1)

		_WaterLod1Alpha ("Water Transparency", Range(0,1)) = 0.95
		_SpecularSmoothness ("Specular Smoothness", Float) = 0
		_WaveNormalScale ("Wave Normal Scale", Float) = 1
		_WaveStrength ("Wave Strength", Range(0, 1)) = 1
		_WaveSpeed ("Wave Speed", Float) = 1

		_FresnelWeight("Fresnel Weight", Float) = 0
		_FresnelPower("Fresnel Power", Float) = 0


		//foam
	    [NoScaleOffset]_Bump ("Foam Bump", 2D) = "bump" {}
		[NoScaleOffset]_Foam("Foam Texture", 2D) = "white" {}
		[NoScaleOffset] _FoamGradient ("Foam gradient ", 2D) = "white" {}
 
	    [HideInInspector]_SunColor ("SunColor", Color) = (1,1,0.901,1)
		_Specularity ("Specularity", Range(0.01,8)) = 0.3
		_SpecPower("Specularity Power", Range(0,1)) = 1
		_FoamFactor("Foam Factor", Range(0,3)) = 1.8
		_Size ("Foam Speed", Range(0,30)) = 0.015625
		_FoamSize ("Foam UVSize", Float) = 2
		[HideInInspector] _SunDir ("SunDir", Vector) = (0.3, -0.6, -1, 0)
		_ShoreDistance("Shore Distance", Range(0,20)) = 4
		_ShoreStrength("Shore Strength", Range(1,4)) = 1.5

	}

SubShader
	{
		Pass
		{
			Offset 1, 1 
			Tags { "LightMode" = "ForwardBase" "Queue" = "Geometry"}

		Blend One OneMinusSrcAlpha 
 			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			//foam
		    #pragma multi_compile SHORE_ON SHORE_OFF
			#pragma multi_compile FOGON FOGOFF
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"
 
  			#include "Assets/GIS Tech/GIS Terrain Loader/Resources/Shaders/Lib/GISTerrainLoaderShaderLib.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			    float4 texcoord : TEXCOORD1;
 			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float3 worldPos : TEXCOORD1;
 //foam
 
				half3 floatVec : TEXCOORD2;
    			float4  bumpTexCoord : TEXCOORD3;
				#ifdef SHORE_ON
				float4 ref : TEXCOORD4;
				#endif
    			half3  lightDir : TEXCOORD5;
				float4 buv : TEXCOORD6;
				half3 normViewDir : TEXCOORD7;

				#ifdef FOGON
				half dist : TEXCOORD8;
				#endif
				LIGHTING_COORDS(4,5)
			};
 
 			float _TerrainScale;
			sampler2D _WaterTexture;
			float4 _OceanCol_TexelSize;

			float _SpecularSmoothness;
			float _WaveNormalScale, _WaveStrength, _WaveSpeed;
			sampler2D _WaveNormal, _Noise;
 
			float4 _Ambient;
			
			float4 _FresnelCol;
			float _FresnelWeight, _FresnelPower;

			// foam 

			half _Size;
			half _FoamFactor;
			half4 _SunDir;
			half4 _FakeUnderwaterColor;
			#ifdef FOGON
   			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;
			#endif


		    sampler2D _Bump;
			sampler2D _Foam;
			half _FoamSize;
			#ifdef SHORE_ON
			uniform sampler2D _CameraDepthTexture;
			sampler2D _FoamGradient;
			half _ShoreDistance;
			half _ShoreStrength;
			#endif
			half4 _WaterColor; 
			half4 _SurfaceColor;
            half _WaterLod1Alpha;
			half _Specularity;
			half _SpecPower;
            half4 _SunColor;
 
			//foam
			float3 calculateWaveNormals(float3 pos, float3 sphereNormal, out float3 tang) {
				float noise = triplanar(sphereNormal, sphereNormal, 0.15, _Noise).r;
	
				float waveSpeed = 0.35 * _WaveSpeed;
				float2 waveOffsetA = float2(_Time.x * waveSpeed, _Time.x * waveSpeed * 0.8);
 
				float3 waveA = triplanarNormal(_WaveNormal, pos, sphereNormal, _WaveNormalScale/_TerrainScale, waveOffsetA,_WaveStrength);
  				float3 waveNormal = triplanarNormal(_WaveNormal, pos, lerp(waveA, waveA, noise), (_WaveNormalScale/_TerrainScale) * 1.25, waveOffsetA, _WaveStrength, tang);
 				return waveNormal;
			}
 //Foam
float calculateSpecular(float3 normal, float3 viewDir, float3 dirToSun, float smoothness) {
	float specularAngle = acos(dot(normalize(dirToSun - viewDir), normal));
	float specularExponent = specularAngle / smoothness;
	float specularHighlight = exp(-max(0,specularExponent) * specularExponent);
	return specularHighlight;
}
			float calculateGeoMipLevel(float2 texCoord, int2 texSize) {
	// * Calculate mip level (doing manually to avoid mipmap seam where texture wraps on x axis -- there's probably a better way?)
	float2 dx, dy;
	if (texCoord.x < 0.75 && texCoord.x > 0.25) {
		dx = ddx(texCoord);
		dy = ddy(texCoord);
	}
	else {
		// Shift texCoord so seam is on other side of world
		dx = ddx((texCoord + float2(0.5, 0)) % 1);
		dy = ddy((texCoord + float2(0.5, 0)) % 1);
	}
	float mipMapWeight = 0.5f;
	dx *= texSize * mipMapWeight;
	dy *= texSize * mipMapWeight;

	// Thanks to https://community.khronos.org/t/mipmap-level-calculation-using-dfdx-dfdy/67480/2
	float maxSqrLength = max(dot(dx, dx), dot(dy, dy));
	float mipLevel = 0.5 * log2(maxSqrLength); // 0.5 * log2(x^2) == log2(x)
	// Clamp mip level to prevent value blowing up at poles
	const int maxMipLevel = 8;
	mipLevel = min(maxMipLevel, mipLevel);
	return mipLevel;
}

 			v2f vert (appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos =  mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
 

 //foam
			    o.bumpTexCoord.xy = v.vertex.xz*_Size; 
    			o.pos = UnityObjectToClipPos (v.vertex);
    			o.bumpTexCoord.z = v.tangent.w * _FoamFactor;
				half3 objSpaceViewDir = ObjSpaceViewDir(v.vertex);
    			half3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) );
				half3x3 rotation = half3x3( v.tangent.xyz, binormal, v.normal );
    
    			half3 viewDir = mul(rotation, objSpaceViewDir);
    			o.lightDir = mul(rotation, half3(_SunDir.xyz));

				o.bumpTexCoord.w = _SinTime.y * 0.5;

				float Speedx = _CosTime.x * 0.1;
				float Speedy =_CosTime.y * 0.01;
				float Speedz =  o.bumpTexCoord.w;
				o.buv = float4(o.bumpTexCoord.x + Speedx, o.bumpTexCoord.y +0.5, o.bumpTexCoord.x +Speedy, o.bumpTexCoord.y + Speedz );

				o.normViewDir = normalize(viewDir);

				o.floatVec = normalize(o.normViewDir - normalize(o.lightDir));

				  	#ifdef SHORE_ON
				o.ref = ComputeScreenPos(o.pos);
				#endif
							
				#ifdef FOGON
 
				o.dist = (unity_FogEnd.x - length(o.pos.xyz)) / (unity_FogEnd.x - unity_FogStart.x);
				#endif

				TRANSFER_VERTEX_TO_FRAGMENT(o);
  
				return o;
			}
			float4 frag (v2f i) : SV_Target
			{
			
				float3 pointOnUnitSphere = i.worldNormal;
				float3 sphereNormal = pointOnUnitSphere;

				float2 texCoord = i.uv;
				float mipLevel = calculateGeoMipLevel(texCoord, _OceanCol_TexelSize.zw);

				//If you face a problem in Android or WebGL Platform replace 'LIGHT_ATTENUATION' by 'UNITY_LIGHT_ATTENUATION'
				float shadows = LIGHT_ATTENUATION(i);

				float3 dirToSun = i.worldNormal;
				float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos.xyz);
 
				float3 tang;
				float3 waveNormal = calculateWaveNormals(i.worldPos, i.worldNormal, tang);
 
				float2 oceanRefractionTexCoord = texCoord + tang.xy * 0.0005;
				float3 oceanCol = tex2Dlod(_WaterTexture, float4(oceanRefractionTexCoord.xy, 0, mipLevel));
 
				float3 specularNormal = waveNormal;
				float specularHighlight = saturate(calculateSpecular(specularNormal, viewDir, dirToSun, _SpecularSmoothness));
				float specularStrength = lerp(0, 1, saturate(shadows * 5));
				specularStrength *= smoothstep(0.4f, 0.5, shadows);
				specularHighlight *= specularStrength;
 
				float shading = dot(sphereNormal, dirToSun) * 0.5 + 0.5;
				shading = shading * shading;
				float waveShading = dot(waveNormal, dirToSun);

				float waveShadeMask = lerp(0.4, 0.95, smoothstep(0.2, 1, dot(sphereNormal, dirToSun)));

				float ripple = saturate(smoothstep(-0.53,0.54,dot(waveNormal, viewDir)));
 
				oceanCol += ripple * 0.15;
				shading += ripple *1;

				oceanCol = saturate(oceanCol * (1-specularHighlight) * shading) + specularHighlight * _LightColor0.rgb;
 
				float nightT = saturate(dot(sphereNormal,-dirToSun)); 
				float nightShadowFixT = smoothstep(0.2,0.3,nightT);
				shadows = lerp(shadows, 0, smoothstep(0.2,0.3,nightT));
 				oceanCol *= lerp(1, shadows, 10);
				oceanCol = saturate(oceanCol + _Ambient * 0.1);
 
				float fresnel = saturate(_FresnelWeight * pow(1 + dot(viewDir, pointOnUnitSphere), _FresnelPower));
				oceanCol += fresnel * _FresnelCol;
 
				//foam 
			    half3 tangentNormal0 = (tex2D(_Bump, i.buv.xy) * 2.0) + (tex2D(_Bump, i.buv.zw) * 2.0) ;
				half3 tangentNormal = normalize(tangentNormal0);
				half4 result = half4(0, 0, 0, 1)*_SunColor;
				half fresnelTerm = 1.0 - saturate(dot (i.normViewDir, tangentNormal0));
				half _foam = tex2D(_Foam, -i.buv.xy *_FoamSize);
 
				half foam = clamp(_foam  - 0.5, 0.0, 0.0) * i.bumpTexCoord.z;

				#ifdef SHORE_ON
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) * _SpecPower;
				#else
				half specular = pow(max(dot(i.floatVec,  tangentNormal) , 0.0), 250.0 * _Specularity ) *(1.2-foam) * _SpecPower;
				#endif
   
				//SHORELINES
				#ifdef SHORE_ON
				float zdepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
                float intensityFactor = 1 - saturate((zdepth - i.ref.w) / (_ShoreDistance*_TerrainScale));
                half3 foamGradient = _ShoreStrength - tex2D(_FoamGradient, float2(intensityFactor - i.bumpTexCoord.w, 0) + tangentNormal.xy);
                foam += foamGradient * intensityFactor * _foam;
				#endif
				             
				result.rgb = lerp(_WaterColor*_FakeUnderwaterColor,_SurfaceColor*0.85, fresnelTerm*0.65) + clamp(foam.r, 0.0, 1.0) + specular;

                result.a = _WaterLod1Alpha;
 
				#ifdef FOGON
 
				float ff = saturate(i.dist);
				oceanCol.rgb += lerp(unity_FogColor.rgb, result.rgb, ff);
				result.rgb = lerp(unity_FogColor.rgb, result.rgb, ff);
								result += float4(oceanCol, 1);
 
				#endif



				return result;
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}



































