using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// A complete snapshot of all values necessary to draw a line circle at a given point in time
	/// </summary>
	public struct LineCircleSnapshot
	{
		public Vector3 CirclePos;
		public Vector3 CircleRotation;
		public float CircleRadius;
		public Vector3 LineRotation;
		public float LineLength;
		public float ColorRange;
		public float ColorOffset;
		public float EmitTime;
		public float Hidden;
		public float LineHidden;
	}
	
	/// <summary>
	/// A single vertex in a LineCircle mesh
	/// </summary>
#pragma warning disable 649
	public struct LineCircleVertex
	{
		public Vector4 Position;
		public Color Color;
	}
#pragma warning restore 649

	/// <summary>
	/// A list of all possible float parameters to change of a LineCircle Pattern's oscillators
	/// </summary>
	public enum ValueType
	{
		TimeStep,
		TimeOffset,
		CirclePosX,
		CirclePosY,
		CirclePosZ,
		CircleRotX,
		CircleRotY,
		CircleRotZ,
		CircleRadius,
		LineRotX,
		LineRotY,
		LineRotZ,
		LineLength,
		ColorRange,
		ColorOffset
	}

	/// <summary>
	/// A list of the changeable parameters of an oscillator
	/// </summary>
	public enum ValueParameter
	{
		Center,
		Amplitude,
		Period,
		Phase
	}

	public enum CameraMode
	{
		Free,
		Pan,
		Follow
	}

	public enum BloomMode
	{
		Off,
		Low,
		Medium,
		High
	}

	public enum VignetteMode
	{
		Off,
		Low,
		Medium,
		High
	}

	public enum GrainMode
	{
		Off,
		Low,
		Medium,
		High
	}

	public enum ShuffleInterval
	{
		Short,
		Medium,
		Long,
		VeryLong
	}

	public enum ShuffleVariability
	{
		Low,
		Medium,
		High
	}

	[System.Serializable]
	public struct ShuffleVariabilitySetting
	{
		public float GlobalShuffleChance;
		public float SphericalSwapChance;
		public float FreezeChance;
		public float SawtoothChance;
	}

	/// <summary>
	/// A set of static constants for array indexes
	/// </summary>
	public class ID
	{
		/// <summary>
		/// How many array indexes to make for a complete description of a Pattern Parameter list
		/// </summary>
		public const int Count = 13;

		/// <summary>
		/// X or R position of Circle Center
		/// </summary>
		public const int CirclePosX = 0;

		/// <summary>
		/// Y or Theta position of Circle Center
		/// </summary>
		public const int CirclePosY = 1;

		/// <summary>
		/// Z or Phi position of Circle Center
		/// </summary>
		public const int CirclePosZ = 2;


		/// <summary>
		/// X Euler Angle of Circle Center rotation
		/// </summary>
		public const int CircleRotX = 3;

		/// <summary>
		/// Y Euler Angle of Circle Center rotation
		/// </summary>
		public const int CircleRotY = 4;

		/// <summary>
		/// Z Euler Angle of Circle Center rotation
		/// </summary>
		public const int CircleRotZ = 5;


		/// <summary>
		/// Radius of circle
		/// </summary>
		public const int CircleRad = 6;


		/// <summary>
		/// X Euler Angle of Line Offset rotation
		/// </summary>
		public const int LineRotX = 7;

		/// <summary>
		/// Y Euler Angle of Line Offset rotation
		/// </summary>
		public const int LineRotY = 8;

		/// <summary>
		/// Z Euler Angle of Line Offset rotation
		/// </summary>
		public const int LineRotZ = 9;


		/// <summary>
		/// Length of lines in meters
		/// </summary>
		public const int LineLength = 10;


		/// <summary>
		/// Range of colour hue around circle (1 = full hue spectrum)
		/// </summary>
		public const int ColorRange = 11;

		/// <summary>
		/// Start colour hue around circle (0 - 1)
		/// </summary>
		public const int ColorOffset = 12;


		public const int Constant = 0;
		public const int Sine = 1;
		public const int Triangle = 2;
		public const int Sawtooth = 3;
		public const int Square = 4;
	}

	public enum OscillatorShape
	{
		Constant = 0,
		Sine = 1,
		Triangle = 2,
		Sawtooth = 3,
		Square = 4,
		Inactive = 100,
	}
}