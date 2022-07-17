//======================================
//======================================
//         NO LONGER USED!!
//======================================
//======================================

Shader "LineCircles/Geometry Lines"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_LineCount("Number Lines", Range(1,18)) = 12
		_Spherical("Spherical Coordinates", int) = 0
		_LineInterval("Line Interval", int) = 0
		_IntervalOffset("Interval Offset", int) = 0
		_TimeSpan("Time span", int) = 0
		_Position("Position", vector) = (0, 0, 0)
		_Rotation("Rotation", vector) = (0, 0, 0)
		_Scale("Scale", vector) = (0, 0, 0)
		_SrcMode("SrcMode", Float) = 0
		_DstMode("DstMode", Float) = 0
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		Blend[_SrcMode][_DstMode]
		ZWrite Off
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



#define VTXCNT 36

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
	int _LineInterval;
	int _IntervalOffset;
	int _TimeSpan;
	float3 _Position;
	float3 _Rotation;
	float3 _Scale;

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

		if(_LineInterval > 1 && ((uint)lc.EmitTime + _IntervalOffset) % _LineInterval != 0) {
			o.rotLength.w = 0;
		}
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
	void geom(point v2g vert[1], inout LineStream<g2f> lineStream)
	{
		if(vert[0].rotLength.w == 0) {
			return;
		}

		float3 rawPos = vert[0].posRad.xyz;
		float3 pos = float3(0, 0, 0);

		if (_Spherical == 0) {
			pos = float3(rawPos.x, rawPos.y, rawPos.z);
		}
		else if (_Spherical == 1) {
			pos = SpherToCart(rawPos);
		}

		float rad = vert[0].posRad.w;
		float3 rot = vert[0].rotLength.xyz;
		float length = vert[0].rotLength.w;
		float3 lineRot = vert[0].lineRot.xyz;
		float colRange = vert[0].col.x;
		float colOffset = vert[0].col.y;

		float4 zv = float4(0,0,0,0);
		float4 startVerts[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};
		float4 endVerts[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};
		float4 vertCols[18] = {
			zv,zv,zv,zv,zv,zv,zv,zv,zv,
			zv,zv,zv,zv,zv,zv,zv,zv,zv
		};

		//note - linecount must not go above 18 or this will break (2 x 18 = 36 max verts)
		int i;
		for (i = 0; i < _LineCount; i++) {
			float iF = (float)i / (float)_LineCount;
			float iT = iF * 2.0 * 3.141592653589794;

			float3 defaultPosition = float3(cos(iT), sin(iT), 0);

			startVerts[i] = float4(rotVec(rad * defaultPosition, rot) + pos, 1);
			//note sure about endVerts here...
			endVerts[i] = float4(rotVec(rotVec(length * defaultPosition, lineRot), rot), 1) + startVerts[i];

			//apply transformation
			
			startVerts[i] = TransformPoint(startVerts[i], _Position, _Rotation, _Scale);
			endVerts[i] = TransformPoint(endVerts[i], _Position, _Rotation, _Scale);

			vertCols[i] = float4(hsv2rgb(float3(frac(colOffset + colRange * iF), 1.0, 1.0)), 1.0);
		}

		const float4 outputVerts[VTXCNT] = {
			startVerts[0],	endVerts[0],	startVerts[1],	endVerts[1],	startVerts[2],	endVerts[2],
			startVerts[3],	endVerts[3],	startVerts[4],	endVerts[4],	startVerts[5],	endVerts[5],
			startVerts[6],	endVerts[6],	startVerts[7],	endVerts[7],	startVerts[8],	endVerts[8],
			startVerts[9],	endVerts[9],	startVerts[10],	endVerts[10],	startVerts[11], endVerts[11],
			startVerts[12],	endVerts[12],	startVerts[13],	endVerts[13],	startVerts[14], endVerts[14],
			startVerts[15],	endVerts[15],	startVerts[16],	endVerts[16],	startVerts[17], endVerts[17]
		};
		const float4 outputCols[VTXCNT] = {
			vertCols[0],	vertCols[0],	vertCols[1],	vertCols[1],	vertCols[2],	vertCols[2],
			vertCols[3],	vertCols[3],	vertCols[4],	vertCols[4],	vertCols[5],	vertCols[5],
			vertCols[6],	vertCols[6],	vertCols[7],	vertCols[7],	vertCols[8],	vertCols[8],
			vertCols[9],	vertCols[8],	vertCols[10],	vertCols[10],	vertCols[11],	vertCols[11],
			vertCols[12],	vertCols[12],	vertCols[13],	vertCols[13],	vertCols[14],	vertCols[14],
			vertCols[15],	vertCols[15],	vertCols[16],	vertCols[16],	vertCols[17],	vertCols[17]
		};

		g2f v[VTXCNT];
		for (i = 0; i < VTXCNT; i++) {
			v[i].pos = UnityObjectToClipPos(outputVerts[i]);
			v[i].col = outputCols[i];
		}
		for (i = 0; i < _LineCount; i++) {
			lineStream.Append(v[i * 2 + 0]);
			lineStream.Append(v[i * 2 + 1]);

			lineStream.RestartStrip();
		}
	}

	fixed4 frag(g2f i) : SV_Target
	{
		return i.col * _Color;
	}
		ENDCG
	} Pass{

	}
	}
}
