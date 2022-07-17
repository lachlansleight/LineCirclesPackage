using System.Collections.Generic;
using LineCircles;
using UnityEngine;

public class LineCirclePatternEvaluator : MonoBehaviour
{
	private struct LineCircleSnapshot
	{
		public Vector3 CirclePosition;
		public Vector3 CircleRotation;
		public float CircleRadius;
		public Vector3 LineRotation;
		public float LineLength;
		public float ColorRange;
		public float ColorOffset;

		public override string ToString()
		{
			return $"cp{CirclePosition:0.0} cr{CircleRotation:0} rad{CircleRadius:0.0} lr{LineRotation:0.0} ll{LineLength:0.0}";
		}
	}

	public static int GetLinesAtT(LineCirclePattern pattern, float t, ref Vector3[] start, ref Vector3[] end, ref Color[] colors)
	{
		var snapshot = EvaluateAtT(pattern, t);
		
		var rawPos = snapshot.CirclePosition;
		var pos = !pattern.SphericalCoordinates ? new Vector3(rawPos.x, rawPos.y, rawPos.z) : SpherToCart(rawPos);

		var rad = snapshot.CircleRadius;
		var rot = snapshot.CircleRotation;
		var length = snapshot.LineLength;;
		var lineRot = snapshot.LineRotation;
		float colRange = snapshot.ColorRange;
		float colOffset = snapshot.ColorOffset;

		for (var lineId = 0; lineId < pattern.LineCount; lineId++) {
			var iF = (float)lineId / pattern.LineCount;
			var iT = iF * 2f * 3.141592653589794f;

			var defaultPosition = new Vector3(Mathf.Cos(iT), Mathf.Sin(iT), 0);
			length *= 1f + (iF * pattern.LineScaleMultiplier);
			lineRot *= (iF * pattern.LineRotationMultiplier);

			var vPos = RotVec(rad * defaultPosition, rot) + pos;
			var vOuter = vPos + RotVec(RotVec(length * defaultPosition, lineRot), rot);

			colors[lineId] = Color.HSVToRGB(Frac(colOffset + colRange * iF), 1f, 1f);

			start[lineId] = vPos;
			end[lineId] = vOuter;
		}

		return pattern.LineCount;
	}
	
	private static LineCircleSnapshot EvaluateAtT(LineCirclePattern pattern, float t)
	{
		t = t * pattern.TimeStep + pattern.TimeOffset;
		
		var snapshot = new LineCircleSnapshot();
		
		snapshot.CirclePosition = new Vector3(
			EvaluateAtT(t, pattern.Oscillators[ID.CirclePosX]),
			EvaluateAtT(t, pattern.Oscillators[ID.CirclePosY]),
			EvaluateAtT(t, pattern.Oscillators[ID.CirclePosZ])
		);
		snapshot.CircleRotation = new Vector3(
			EvaluateAtT(t, pattern.Oscillators[ID.CircleRotX]),
			EvaluateAtT(t, pattern.Oscillators[ID.CircleRotY]),
			EvaluateAtT(t, pattern.Oscillators[ID.CircleRotZ])
		);
		snapshot.CircleRadius = EvaluateAtT(t, pattern.Oscillators[ID.CircleRad]);

		snapshot.LineLength = EvaluateAtT(t, pattern.Oscillators[ID.LineLength]);

		//optional - scale line length with radius
		if(pattern.AutoScaleLines) {
			snapshot.LineLength *= snapshot.CircleRadius;
		}

		snapshot.LineRotation = new Vector3(
			EvaluateAtT(t, pattern.Oscillators[ID.LineRotX]),
			EvaluateAtT(t, pattern.Oscillators[ID.LineRotY]),
			EvaluateAtT(t, pattern.Oscillators[ID.LineRotZ])
		);
		
		snapshot.ColorRange = EvaluateAtT(t, pattern.Oscillators[ID.ColorRange]);
		snapshot.ColorOffset = EvaluateAtT(t, pattern.Oscillators[ID.ColorOffset]);
		
		return snapshot;
	}

	private static float EvaluateAtT(float t, Oscillator oscillator)
	{
		return EvaluateAtT(t, (int) oscillator.Type, oscillator.Center, oscillator.Amplitude, oscillator.Period, oscillator.Phase);
	}
	
	private static float EvaluateAtT(float t, int type, float c, float a, float p, float ph) {
		//hold still
		if (type == 0) {
			return c;
		}
		//we need this for lots of stuff
		var tp = (t / p) + ph;

		//sine wave
		if (type == 1) return c + a * Mathf.Sin(tp * 3.1415926535897f * 2f);
		if (type == 2) return (Mathf.Abs(4f * a * (Frac(Mathf.Abs((t / p) + (ph + 0.75f))) - 0.5f)) - a) + c;
		if (type == 3) return ((2f * a * Frac((t / p) + (ph + 0.5f))) - a) + c;
		if (type == 4) return (Mathf.Sign(Mathf.Abs(Frac(Mathf.Abs((t / p) + (ph + 0.75f))) - 0.5f) - 0.25f) * a) + c;

		return c;
	}
	
	private static float Frac(float n) => n % 1f;
	
	private static Vector3 SpherToCart(Vector3 c) {
		var x = c.x * Mathf.Cos(c.y) * Mathf.Sin(c.z);
		var y = c.y * Mathf.Sin(c.y) * Mathf.Sin(c.z);
		var z = c.x * Mathf.Cos(c.z);
		return new Vector3(x, z, y);
	}
	
	private static Vector3 RotVec(Vector3 input, Vector3 eulers)
	{
		eulers *= Mathf.Rad2Deg;
		return Quaternion.Euler(eulers) * input;
		// float3x3 rotX = float3x3(
		// 	1,				0,				0,
		// 	0,				cos(eulers.x),	-sin(eulers.x),
		// 	0,				sin(eulers.x),	cos(eulers.x)
		// );
		// float3x3 rotY = float3x3(
		// 	cos(eulers.y),	0,				sin(eulers.y),
		// 	0,				1,				0,
		// 	-sin(eulers.y),	0,				cos(eulers.y)
		// );
		// float3x3 rotZ = float3x3(
		// 	cos(eulers.z),	-sin(eulers.z),	0,
		// 	sin(eulers.z),	cos(eulers.z),	0,
		// 	0,				0,				1
		// );
		//
		// float3x3 rotXYZ = mul(rotX, mul(rotY, rotZ));
		// return mul(rotXYZ, input);
	}
}