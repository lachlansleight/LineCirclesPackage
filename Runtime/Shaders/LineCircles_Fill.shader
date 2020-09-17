Shader "LineCircles/Geometry Fill"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_LineCount("Number Lines", Range(1,18)) = 12
		_Spherical("Spherical Coordinates", int) = 0
		_TimeSpan("Time span", int) = 0
		_Position("Position", vector) = (0, 0, 0)
		_Rotation("Rotation", vector) = (0, 0, 0)
		_Scale("Scale", vector) = (0, 0, 0)
		_SrcMode ("SrcMode", Float) = 0
		_DstMode ("DstMode", Float) = 0
		_UseNormalColors ("Use Normal Colors", int) = 0
		_ForceOpaque ("Force Opaque", int) = 0
	}
		SubShader
	{
		//for opaque - make render type transparent, comment out blend and zwrite
		Tags{ "RenderType" = "Transparent" }
		Blend [_SrcMode] [_DstMode]
		ZWrite Off
		Cull Off
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma only_renderers d3d11
#pragma target 4.0

#include "UnityCG.cginc"			

#pragma vertex vert
#pragma geometry geom
#pragma fragment frag



#define VTXCNT 72
#define LNCNT 36

		struct LineCircleSnapshot {
		float3 CirclePosition;
		float3 CircleRotation;
		float CircleRadius;
		float3 LineRotation;
		float LineLength;
		float ColorRange;
		float ColorOffset;
		float EmitTime;
		float2 Padding;
	};

	StructuredBuffer<LineCircleSnapshot> _Buffer;
	float4 _Color;
	int _LineCount;
	int _Spherical;
	int _TimeSpan;
	float3 _Position;
	float3 _Rotation;
	float3 _Scale;

	int _UseNormalColors;
	int _ForceOpaque;

	struct v2g
	{
		float4 posRad : SV_POSITION;
		float4 rotLength : TANGENT;
		float3 lineRot : NORMAL;
		float2 col : TEXCOORD0;
	};

	struct g2f
	{
		float4 pos : SV_POSITION;
		float3 normal : NORMAL;
		float4 col : COLOR0;
	};

	v2g vert(uint id : SV_VertexID)
	{
		v2g o;
		LineCircleSnapshot lc = _Buffer[id];
		o.posRad = float4(lc.CirclePosition, lc.CircleRadius);
		o.rotLength = float4(lc.CircleRotation, lc.LineLength);
		o.lineRot = lc.LineRotation;
		o.col = float2(lc.ColorRange, lc.ColorOffset);

		if (lc.EmitTime > _TimeSpan) {
			o.rotLength.w = 0;
		}

		return o;
	}

	float3 rotVec(float3 input, float3 eulers) {
		float3x3 rotX = float3x3(
			1,				0,				0,
			0,				cos(eulers.x),	-sin(eulers.x),
			0,				sin(eulers.x),	cos(eulers.x)
			);
		float3x3 rotY = float3x3(
			cos(eulers.y),	0,				sin(eulers.y),
			0,				1,				0,
			-sin(eulers.y),	0,				cos(eulers.y)
			);
		float3x3 rotZ = float3x3(
			cos(eulers.z),	-sin(eulers.z),	0,
			sin(eulers.z),	cos(eulers.z),	0,
			0,				0,				1
			);

		float3x3 rotXYZ = mul(rotX, mul(rotY, rotZ));
		return mul(rotXYZ, input);
	}

	float4 TransformPoint(float4 input, float3 position, float3 rotation, float3 scale) {
		float3 input3 = input.xyz;
		float3 rotated = rotVec(input3, rotation);
		float3 scaled = float3(rotated.x * scale.x, rotated.y * scale.y, rotated.z * scale.z);
		return float4(scaled.x + position.x, scaled.y + position.y, scaled.z + position.z, 1);
	}

	float3 hsv2rgb(float3 c)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
		return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	}

	float3 SpherToCart(float3 c) {
		float x = c.x * cos(c.y) * sin(c.z);
		float y = c.y * sin(c.y) * sin(c.z);
		float z = c.x * cos(c.z);
		return float3(x, z, y);
	}

	[maxvertexcount(VTXCNT)]
	// ----------------------------------------------------
	// Using "point" type as input, not "triangle"
	void geom(line v2g vert[2], inout TriangleStream<g2f> triStream)
	{
		if (vert[0].rotLength.w == 0) {
			return;
		}

		int i;

		float3 rawPosA = vert[0].posRad.xyz;
		float3 posA = float3(0, 0, 0);

		float3 rawPosB = vert[1].posRad.xyz;
		float3 posB = float3(0, 0, 0);

		if (_Spherical == 0) {
			posA = float3(rawPosA.x, rawPosA.y, rawPosA.z);
			posB = float3(rawPosB.x, rawPosB.y, rawPosB.z);
		}
		else if (_Spherical == 1) {
			posA = SpherToCart(rawPosA);
			posB = SpherToCart(rawPosB);
		}

		float radA = vert[0].posRad.w;
		float3 rotA = vert[0].rotLength.xyz;
		float lengthA = vert[0].rotLength.w;
		float3 lineRotA = vert[0].lineRot.xyz;
		float colRangeA = vert[0].col.x;
		float colOffsetA = vert[0].col.y;

		float radB = vert[1].posRad.w;
		float3 rotB = vert[1].rotLength.xyz;
		float lengthB = vert[1].rotLength.w;
		float3 lineRotB = vert[1].lineRot.xyz;
		float colRangeB = vert[1].col.x;
		float colOffsetB = vert[1].col.y;

		float4 zv = float4(0,0,0,0);
		float3 zv3 = float3(0, 0, 0);

		float4 startVertsA[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};
		float4 startVertsB[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};

		float4 endVertsA[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};
		float4 endVertsB[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};

		float3 norms[18] = {
			zv3,zv3,zv3,zv3,zv3,zv3,zv3,zv3,zv3,
			zv3,zv3,zv3,zv3,zv3,zv3,zv3,zv3,zv3
		};

		float4 vertColsA[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};
		float4 vertColsB[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};

		//note - linecount must not go above 18 or this will break (2 x 18 = 36 max verts)
		for (i = 0; i < _LineCount; i++) {
			float iF = (float)i / (float)_LineCount;
			float iT = iF * 2.0 * 3.141592653589794;

			float3 defaultPosition = float3(cos(iT), sin(iT), 0);

			startVertsA[i] = float4(rotVec(radA * defaultPosition, rotA) + posA, 1);
			startVertsB[i] = float4(rotVec(radB * defaultPosition, rotB) + posB, 1);
			endVertsA[i] = float4(rotVec(rotVec(lengthA * defaultPosition, lineRotA), rotA), 1) + startVertsA[i];
			endVertsB[i] = float4(rotVec(rotVec(lengthB * defaultPosition, lineRotB), rotB), 1) + startVertsB[i];
			
			//apply transformations
			startVertsA[i] = TransformPoint(startVertsA[i], _Position, _Rotation, _Scale);
			startVertsB[i] = TransformPoint(startVertsB[i], _Position, _Rotation, _Scale);
			endVertsA[i] = TransformPoint(endVertsA[i], _Position, _Rotation, _Scale);
			endVertsB[i] = TransformPoint(endVertsB[i], _Position, _Rotation, _Scale);

			norms[i] = cross(normalize(endVertsA[i] - startVertsA[i]), normalize(startVertsB[i] - startVertsA[i]));

			vertColsA[i] = float4(hsv2rgb(float3(frac(colOffsetA + colRangeA * iF), 1.0, 1.0)), 1.0);
			vertColsB[i] = float4(hsv2rgb(float3(frac(colOffsetB + colRangeB * iF), 1.0, 1.0)), 1.0);
		}

		const float4 outputVertsA[LNCNT] = {
			startVertsA[0],		endVertsA[0],	startVertsA[1],		endVertsA[1],	startVertsA[2],	endVertsA[2],
			startVertsA[3],		endVertsA[3],	startVertsA[4],		endVertsA[4],	startVertsA[5],	endVertsA[5],
			startVertsA[6],		endVertsA[6],	startVertsA[7],		endVertsA[7],	startVertsA[8],	endVertsA[8],
			startVertsA[9],		endVertsA[9],	startVertsA[10],	endVertsA[10],	startVertsA[11], endVertsA[11],
			startVertsA[12],	endVertsA[12],	startVertsA[13],	endVertsA[13],	startVertsA[14], endVertsA[14],
			startVertsA[15],	endVertsA[15],	startVertsA[16],	endVertsA[16],	startVertsA[17], endVertsA[17]
		};
		const float4 outputVertsB[LNCNT] = {
			startVertsB[0],		endVertsB[0],	startVertsB[1],		endVertsB[1],	startVertsB[2],	endVertsB[2],
			startVertsB[3],		endVertsB[3],	startVertsB[4],		endVertsB[4],	startVertsB[5],	endVertsB[5],
			startVertsB[6],		endVertsB[6],	startVertsB[7],		endVertsB[7],	startVertsB[8],	endVertsB[8],
			startVertsB[9],		endVertsB[9],	startVertsB[10],	endVertsB[10],	startVertsB[11], endVertsB[11],
			startVertsB[12],	endVertsB[12],	startVertsB[13],	endVertsB[13],	startVertsB[14], endVertsB[14],
			startVertsB[15],	endVertsB[15],	startVertsB[16],	endVertsB[16],	startVertsB[17], endVertsB[17]
		};

		const float3 outputNorms[LNCNT] = {
			norms[0],			norms[0],		norms[1],			norms[1],		norms[2],		norms[2],
			norms[3],			norms[3],		norms[4],			norms[4],		norms[5],		norms[5],
			norms[6],			norms[6],		norms[7],			norms[7],		norms[8],		norms[8],
			norms[9],			norms[9],		norms[10],			norms[10],		norms[11],		norms[11],
			norms[12],			norms[12],		norms[13],			norms[13],		norms[14],		norms[14],
			norms[15],			norms[15],		norms[16],			norms[16],		norms[17],		norms[17]
		};


		const float4 outputColsA[LNCNT] = {
			vertColsA[0],	vertColsA[0],	vertColsA[1],	vertColsA[1],	vertColsA[2],	vertColsA[2],
			vertColsA[3],	vertColsA[3],	vertColsA[4],	vertColsA[4],	vertColsA[5],	vertColsA[5],
			vertColsA[6],	vertColsA[6],	vertColsA[7],	vertColsA[7],	vertColsA[8],	vertColsA[8],
			vertColsA[9],	vertColsA[8],	vertColsA[10],	vertColsA[10],	vertColsA[11],	vertColsA[11],
			vertColsA[12],	vertColsA[12],	vertColsA[13],	vertColsA[13],	vertColsA[14],	vertColsA[14],
			vertColsA[15],	vertColsA[15],	vertColsA[16],	vertColsA[16],	vertColsA[17],	vertColsA[17]
		};
		const float4 outputColsB[LNCNT] = {
			vertColsB[0],	vertColsB[0],	vertColsB[1],	vertColsB[1],	vertColsB[2],	vertColsB[2],
			vertColsB[3],	vertColsB[3],	vertColsB[4],	vertColsB[4],	vertColsB[5],	vertColsB[5],
			vertColsB[6],	vertColsB[6],	vertColsB[7],	vertColsB[7],	vertColsB[8],	vertColsB[8],
			vertColsB[9],	vertColsB[8],	vertColsB[10],	vertColsB[10],	vertColsB[11],	vertColsB[11],
			vertColsB[12],	vertColsB[12],	vertColsB[13],	vertColsB[13],	vertColsB[14],	vertColsB[14],
			vertColsB[15],	vertColsB[15],	vertColsB[16],	vertColsB[16],	vertColsB[17],	vertColsB[17]
		};

		g2f vA[LNCNT];
		g2f vB[LNCNT];
		for (i = 0; i < LNCNT; i++) {
			vA[i].pos = UnityObjectToClipPos(outputVertsA[i]);
			vA[i].normal = outputNorms[i];
			vA[i].col = outputColsA[i];

			vB[i].pos = UnityObjectToClipPos(outputVertsB[i]);
			vB[i].normal = outputNorms[i];
			vB[i].col = outputColsB[i];
		}
		for (i = 0; i < LNCNT; i++) {
			if (i <= _LineCount) {
				triStream.Append(vA[i * 2 + 0]);
				triStream.Append(vA[i * 2 + 1]);
				triStream.Append(vB[i * 2 + 0]);

				triStream.Append(vB[i * 2 + 0]);
				triStream.Append(vA[i * 2 + 1]);
				triStream.Append(vB[i * 2 + 1]);

				triStream.RestartStrip();
			}
		}

	}

	fixed4 frag(g2f i) : SV_Target
	{

		if (_ForceOpaque == 1) {
			if (_UseNormalColors) {
				return float4((i.normal + float3(1, 1, 1)) * 0.5, 1.0);
			}
			else {
				return float4(i.col.xyz * _Color.xyz, 1.0);
			}
		}
		else {
			if (_UseNormalColors) {
				return float4((i.normal + float3(1, 1, 1)) * 0.5, i.col.a * _Color.a);
			}
			else {
				return i.col * _Color;
			}
		}
		
	}
		ENDCG
	} Pass{

	}
	}
}
