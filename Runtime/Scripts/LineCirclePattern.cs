using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Complete list of settings necessary to replicate a Line Circle visual pattern
	/// </summary>
	[System.Serializable]
	public class LineCirclePattern
	{
		
		/// <summary>
		/// The number of independent time steps
		/// </summary>
		[Header("Settings")]
		[Range(1, 10000)]
		[Tooltip("The number of independent time steps")]
		public int Count = 1000;

		/// <summary>
		/// The number of lines distributed around the circle
		/// </summary>
		[Range(1, 16)]
		[Tooltip("The number of lines distributed around the circle")]
		public int LineCount = 12;

		/// <summary>
		/// Whether to use Spherical or Cartesian Coordinates with CirclePos[XYZ]
		/// </summary>
		[Tooltip("Whether to use Spherical or Cartesian Coordinates with CirclePos[XYZ]")]
		public bool SphericalCoordinates;

		/// <summary>
		/// Draw a line every [n] time steps
		/// </summary>
		[Tooltip("Draw a line every [n] time steps")]
		[Range(1, 20)]
		public int LineInterval = 4;


		/// <summary>
		/// Time in seconds to advance every time snapshot of the pattern
		/// </summary>
		[Space(10)]
		[Tooltip("Time in seconds to advance every time snapshot of the pattern")]
		[Range(0.001f, 0.1f)]
		public float TimeStep = 0.1f;

		/// <summary>
		/// The amount of time from pattern beginning to draw
		/// </summary>
		[Tooltip("The amount of time from pattern beginning to draw")]
		public float TimeSpan = 10f;

		public float MaxTimePossible;

		/// <summary>
		/// Offset time in seconds of beginning of the pattern
		/// </summary>
		[Tooltip("Offset time in seconds of beginning of the pattern")]
		public float TimeOffset = 0;

		/// <summary>
		/// Whether to display lines
		/// </summary>
		[Space(10)]
		[Tooltip("Whether to display lines")]
		public bool DrawLines;

		/// <summary>
		/// Whether to display fill
		/// </summary>
		[Tooltip("Whether to display fill")]
		public bool DrawFill;

		/// <summary>
		/// Color tint for lines
		/// </summary>
		[Tooltip("Color tint for lines")]
		public Color LineColor = new Color(1f, 1f, 1f, 0.1f);

		/// <summary>
		/// Color tint for fill
		/// </summary>
		[Tooltip("Color tint for fill")]
		public Color FillColor = new Color(1f, 1f, 1f, 0.1f);

		/// <summary>
		/// The Source Mode for Blending
		/// </summary>
		[Tooltip("The Source Mode for Blending")]
		public UnityEngine.Rendering.BlendMode SrcMode;

		/// <summary>
		/// The Destination Mode for Blending
		/// </summary>
		[Tooltip("The Destination Mode for Blending")]
		public UnityEngine.Rendering.BlendMode DstMode;

		/// <summary>
		/// List of oscillators controlling pattern visuals
		/// </summary>
		[Tooltip("List of oscillators controlling pattern visuals")]
		public Oscillator[] Oscillators;

		/// <summary>
		/// Creates a new LineCirclePattern
		/// </summary>
		public LineCirclePattern()
		{
			Count = 10000;
			LineCount = 12;
			SphericalCoordinates = true;
			LineInterval = 4;

			TimeStep = 0.01f;
			TimeSpan = 0;
			TimeOffset = 0;

			DrawLines = true;
			DrawFill = true;
			LineColor = new Color(1f, 1f, 1f, 0.1f);
			FillColor = new Color(1f, 1f, 1f, 0.1f);

			SrcMode = UnityEngine.Rendering.BlendMode.SrcAlpha;
			DstMode = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;

			Oscillators = new Oscillator[ID.Count];
			Oscillators[ID.CirclePosX] = new Oscillator("CirclePosX", 1, 0, 1, 1, 0);
			Oscillators[ID.CirclePosY] = new Oscillator("CirclePosY", 1, 0, 1, 3.1f, 0);
			Oscillators[ID.CirclePosZ] = new Oscillator("CirclePosZ", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotX] = new Oscillator("CircleRotX", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotY] = new Oscillator("CircleRotY", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotZ] = new Oscillator("CircleRotZ", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRad] = new Oscillator("CircleRad", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotX] = new Oscillator("LineRotX", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotY] = new Oscillator("LineRotY", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotZ] = new Oscillator("LineRotZ", 0, 0, 1, 1, 0);
			Oscillators[ID.LineLength] = new Oscillator("LineLength", 0, 0, 1, 1, 0);
			Oscillators[ID.ColorOffset] = new Oscillator("ColorOffset", 0, 0, 1, 1, 0);
			Oscillators[ID.ColorRange] = new Oscillator("ColorRange", 0, 0, 1, 1, 0);
		}

		/// <summary>
		/// Creates a new LineCirclePattern with the specified SnapshotCount
		/// </summary>
		/// <param name="SnapshotCount">SnapshotCount to use</param>
		public LineCirclePattern(int SnapshotCount)
		{
			Count = SnapshotCount;
			LineCount = 12;
			SphericalCoordinates = true;
			LineInterval = 4;

			TimeStep = 0.01f;
			TimeSpan = 0;
			TimeOffset = 0;

			DrawLines = true;
			DrawFill = true;
			LineColor = new Color(1f, 1f, 1f, 0.1f);
			FillColor = new Color(1f, 1f, 1f, 0.1f);

			SrcMode = UnityEngine.Rendering.BlendMode.SrcAlpha;
			DstMode = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;

			Oscillators = new Oscillator[ID.Count];
			Oscillators[ID.CirclePosX] = new Oscillator("CirclePosX", 1, 0, 1, 1, 0);
			Oscillators[ID.CirclePosY] = new Oscillator("CirclePosY", 1, 0, 1, 3.1f, 0);
			Oscillators[ID.CirclePosZ] = new Oscillator("CirclePosZ", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotX] = new Oscillator("CircleRotX", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotY] = new Oscillator("CircleRotY", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRotZ] = new Oscillator("CircleRotZ", 0, 0, 1, 1, 0);
			Oscillators[ID.CircleRad] = new Oscillator("CircleRad", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotX] = new Oscillator("LineRotX", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotY] = new Oscillator("LineRotY", 0, 0, 1, 1, 0);
			Oscillators[ID.LineRotZ] = new Oscillator("LineRotZ", 0, 0, 1, 1, 0);
			Oscillators[ID.LineLength] = new Oscillator("LineLength", 0, 0, 1, 1, 0);
			Oscillators[ID.ColorOffset] = new Oscillator("ColorOffset", 0, 0, 1, 1, 0);
			Oscillators[ID.ColorRange] = new Oscillator("ColorRange", 0, 0, 1, 1, 0);
		}

		/// <summary>
		/// Creates a new LineCirclePattern, duplicating an existing one
		/// </summary>
		/// <param name="CopyTarget">The LineCirclePattern to duplicate</param>
		public LineCirclePattern(LineCirclePattern CopyTarget)
		{
			Count = CopyTarget.Count;
			LineCount = CopyTarget.LineCount;
			SphericalCoordinates = CopyTarget.SphericalCoordinates;
			LineInterval = CopyTarget.LineInterval;

			TimeStep = CopyTarget.TimeStep;
			TimeSpan = CopyTarget.TimeSpan;
			TimeOffset = CopyTarget.TimeOffset;

			DrawLines = CopyTarget.DrawLines;
			DrawFill = CopyTarget.DrawFill;
			LineColor = CopyTarget.LineColor;
			FillColor = CopyTarget.FillColor;

			SrcMode = CopyTarget.SrcMode;
			DstMode = CopyTarget.DstMode;

			Oscillators = new Oscillator[CopyTarget.Oscillators.Length];
			for (int i = 0; i < Oscillators.Length; i++) {
				Oscillators[i] = new Oscillator(CopyTarget.Oscillators[i]);
			}
		}

		/// <summary>
		/// Initialises a new LineCirclePattern which is the interpolation of two other LineCirclePatterns
		/// </summary>
		/// <param name="PatternA"></param>
		/// <param name="PatternB"></param>
		/// <param name="LerpFactor"></param>
		public LineCirclePattern(LineCirclePattern PatternA, LineCirclePattern PatternB, float LerpFactor)
		{
			Count = Mathf.RoundToInt(Mathf.Lerp(PatternA.Count, PatternB.Count, LerpFactor));
			LineCount = Mathf.RoundToInt(Mathf.Lerp(PatternA.LineCount, PatternB.LineCount, LerpFactor));
			SphericalCoordinates = LerpFactor < 0.5f ? PatternA.SphericalCoordinates : PatternB.SphericalCoordinates;
			LineInterval = Mathf.RoundToInt(Mathf.Lerp(PatternA.LineInterval, PatternB.LineInterval, LerpFactor));

			TimeStep = Mathf.Lerp(PatternA.TimeStep, PatternB.TimeStep, LerpFactor);
			TimeSpan = Mathf.Lerp(PatternA.TimeSpan, PatternB.TimeSpan, LerpFactor);
			TimeOffset = Mathf.Lerp(PatternA.TimeOffset, PatternB.TimeOffset, LerpFactor);

			DrawLines = LerpFactor < 0.5f ? PatternA.DrawLines : PatternB.DrawLines;
			DrawFill = LerpFactor < 0.5f ? PatternA.DrawFill : PatternB.DrawFill;
			LineColor = Color.Lerp(PatternA.LineColor, PatternB.LineColor, LerpFactor);
			FillColor = Color.Lerp(PatternA.FillColor, PatternB.FillColor, LerpFactor);

			SrcMode = LerpFactor < 0.5f ? PatternA.SrcMode : PatternB.SrcMode;
			DstMode = LerpFactor < 0.5f ? PatternA.DstMode : PatternB.DstMode;

			Oscillators = new Oscillator[PatternA.Oscillators.Length];
			for (int i = 0; i < Oscillators.Length; i++) {
				Oscillators[i] = new Oscillator(PatternA.Oscillators[i], PatternB.Oscillators[i], LerpFactor);
			}
		}

		public Bounds GetMaxPossibleBounds()
		{
			var circleMin = Vector3.zero;
			var circleMax = Vector3.zero;
			if (SphericalCoordinates) {
				//In spherical coordinates, we only care about the circle position radius
				//We can't make any assumptions about angles, so we just assume that it'll hit the spherical limits
				//in all directions
				circleMin = -Mathf.Abs(Oscillators[ID.CirclePosX].Center) * Vector3.one;
				circleMax = Mathf.Abs(Oscillators[ID.CirclePosX].Center) * Vector3.one;
				
				if (Oscillators[ID.CirclePosX].Type != OscillatorShape.Constant) {
					circleMin -= Mathf.Abs(Oscillators[ID.CirclePosX].Amplitude) * Vector3.one;
					circleMax += Mathf.Abs(Oscillators[ID.CirclePosX].Amplitude) * Vector3.one;
				}
			} else {
				//In cartesian coordinates, we can use each oscillator to determine min and max values for each axis
				circleMin.x = Oscillators[ID.CirclePosX].Center;
				circleMax.x = Oscillators[ID.CirclePosX].Center;
				if (Oscillators[ID.CirclePosX].Type != OscillatorShape.Constant) {
					circleMin.x -= Mathf.Abs(Oscillators[ID.CirclePosX].Amplitude);
					circleMax.x += Mathf.Abs(Oscillators[ID.CirclePosX].Amplitude);
				}
				
				circleMin.y = Oscillators[ID.CirclePosY].Center;
				circleMax.y = Oscillators[ID.CirclePosY].Center;
				if (Oscillators[ID.CirclePosY].Type != OscillatorShape.Constant) {
					circleMin.y -= Mathf.Abs(Oscillators[ID.CirclePosY].Amplitude);
					circleMax.y += Mathf.Abs(Oscillators[ID.CirclePosY].Amplitude);
				}
				
				circleMin.z = Oscillators[ID.CirclePosZ].Center;
				circleMax.z = Oscillators[ID.CirclePosZ].Center;
				if (Oscillators[ID.CirclePosZ].Type != OscillatorShape.Constant) {
					circleMin.z -= Mathf.Abs(Oscillators[ID.CirclePosZ].Amplitude);
					circleMax.z += Mathf.Abs(Oscillators[ID.CirclePosZ].Amplitude);
				}
			}

			//get max circle radius
			var lineExtents = Oscillators[ID.CircleRad].Center;
			if (Oscillators[ID.CircleRad].Type != OscillatorShape.Constant) {
				lineExtents += Mathf.Abs(Oscillators[ID.CircleRad].Amplitude);
			}
			var maxRad = lineExtents;
			
			//get max line length multiplier
			var maxLine = Oscillators[ID.LineLength].Center;
			if (Oscillators[ID.LineLength].Type != OscillatorShape.Constant) {
				maxLine += Mathf.Abs(Oscillators[ID.LineLength].Amplitude);
			}

			//combine them to get maximum offset from circle position, radially outwards
			lineExtents += maxLine * maxRad;

			//finally, add those extents to the circle position extents to get the total pattern extents
			circleMin -= lineExtents * Vector3.one;
			circleMax += lineExtents * Vector3.one;
			
			return new Bounds((circleMin + circleMax) * 0.5f, circleMax - circleMin);
		}
	}
}