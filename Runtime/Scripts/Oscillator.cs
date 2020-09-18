using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Oscillates a value using an envelope function
	/// </summary>
	[System.Serializable]
	public class Oscillator
	{

		/// <summary>
		/// Human-readable name for the parameter this oscillator controls
		/// </summary>
		[Tooltip("Human-readable name for the parameter this oscillator controls")]
		public string Name;

		/// <summary>
		/// Oscillator type (0 = holds at [center], 1 = sine, 2 = triangle, 3 = sawtooth, 4 = square)
		/// </summary>
		[Tooltip("Oscillator type (0 = holds at [center], 1 = sine, 2 = triangle, 3 = sawtooth, 4 = square)")]
		public OscillatorShape Type;

		/// <summary>
		/// Center value of oscillator
		/// </summary>
		[Tooltip("Center value of oscillator")]
		public float Center = 0;

		/// <summary>
		/// Amplitude of oscillator
		/// </summary>
		[Tooltip("Amplitude of oscillator")]
		public float Amplitude = 1;

		/// <summary>
		/// Period of oscillator in seconds
		/// </summary>
		[Tooltip("Period of oscillator in seconds")]
		public float Period = 1;

		/// <summary>
		/// Phase offset of oscillator (0 - 1)
		/// </summary>
		[Tooltip("Phase of oscillator (0 - 1)")]
		public float Phase = 0;

		/// <summary>
		/// Creates a new Oscillator
		/// </summary>
		/// <param name="type">Type ID (0 = holds at [center], 1 = sine, 2 = triangle, 3 = sawtooth, 4 = square)</param>
		/// <param name="center">Center value of oscillator</param>
		/// <param name="amplitude">Amplitude of oscillator</param>
		/// <param name="period">Period of oscillator in seconds</param>
		/// <param name="phase">Phase offset of oscillator (0 - 1)</param>
		public Oscillator(string name, int type, float center, float amplitude, float period, float phase)
		{
			Name = name;
			Type = (OscillatorShape) type;
			Center = center;
			Amplitude = amplitude;
			Period = period;
			Phase = phase;
		}

		/// <summary>
		/// Creates a new Oscillator copying from an existing one
		/// </summary>
		/// <param name="CopyTarget">The existing oscillator to duplicate</param>
		public Oscillator(Oscillator CopyTarget)
		{
			Name = CopyTarget.Name;
			Type = CopyTarget.Type;
			Center = CopyTarget.Center;
			Amplitude = CopyTarget.Amplitude;
			Period = CopyTarget.Period;
			Phase = CopyTarget.Phase;
		}

		/// <summary>
		/// Creates a new Oscillator which is the interpolation of two existing ones
		/// </summary>
		/// <param name="OscillatorA">The first Oscillator to lerp between</param>
		/// <param name="OscillatorB">The second Oscillator to lerp between</param>
		/// <param name="LerpFactor">The lerp value - a value of 0 creates a copy of OscillatorA, a value of 1 creates a copy of OscillatorB, 0.5 creates one exactly between them, etc...</param>
		public Oscillator(Oscillator OscillatorA, Oscillator OscillatorB, float LerpFactor)
		{
			Name = OscillatorA.Name;
			Type = LerpFactor < 0.5f ? OscillatorA.Type : OscillatorB.Type;
			Center = Mathf.Lerp(OscillatorA.Center, OscillatorB.Center, LerpFactor);
			Amplitude = Mathf.Lerp(OscillatorA.Amplitude, OscillatorB.Amplitude, LerpFactor);
			Period = Mathf.Lerp(OscillatorA.Period, OscillatorB.Period, LerpFactor);
			Phase = Mathf.Lerp(OscillatorA.Phase, OscillatorB.Phase, LerpFactor);
		}

		/// <summary>
		/// Evaluates the oscillator function
		/// </summary>
		/// <param name="time">The time in seconds to evaluate at</param>
		/// <returns>The function value at time</returns>
		public float EvaluateAtTime(float time)
		{
			var scaledTime = (time / Period) + Phase;
			scaledTime %= 1f;
			scaledTime *= Mathf.PI * 2f;
			switch ((int) Type) {
				case 0: //constant
					return Center;
				case 1: //sine
					return Center + Amplitude * Mathf.Sin(scaledTime);
				case 2: //triangle
					return scaledTime < Mathf.PI 
						? Mathf.Lerp(-Amplitude, Amplitude, scaledTime / Mathf.PI) 
						: Mathf.Lerp(Amplitude, -Amplitude, (scaledTime - Mathf.PI) / Mathf.PI);
				case 3: //sawtooth
					return Mathf.Lerp(-Amplitude, Amplitude, scaledTime / (Mathf.PI * 2f));
				case 4: //square
					return scaledTime < Mathf.PI ? -Amplitude : Amplitude;
			}

			return Center;
		}

	}
}