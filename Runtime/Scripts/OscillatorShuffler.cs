using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Used to randomise the parameters of an Oscillator object
	/// </summary>
	[System.Serializable]
	public class OscillatorShuffler
	{

		/// <summary>
		/// Human-readable name for the parameter this oscillator controls
		/// </summary>
		[Tooltip("Human-readable name for the parameter this oscillator controls")]
		public string Name;

		/// <summary>
		/// The absolute minimum the oscillator's value will be allowed to reach
		/// </summary>
		[Tooltip("The absolute minimum the oscillator's value will be allowed to reach")]
		public float AbsoluteMin;

		/// <summary>
		/// The absolute maximum the oscillator's value will be allowed to reach
		/// </summary>
		[Tooltip("The absolute maximum the oscillator's value will be allowed to reach")]
		public float AbsoluteMax;

		[Space(10)]

		/// <summary>
		/// The maximum value the oscillator's value will change each second (overrides min frequency, doesn't apply in Constant or Square waveforms)
		/// </summary>
		[Tooltip(
			"The maximum value the oscillator's value will change each second (overrides min frequency, doesn't apply in Constant or Square waveforms)")]
		public float MaxChangePerSecond;

		[Space(10)]

		/// <summary>
		/// Whether to override the minimum and maximum frequency settings of the parent Shuffler object
		/// </summary>
		[Tooltip("Whether to override the minimum and maximum frequency settings of the parent Shuffler object")]
		public bool OverrideFrequency;

		/// <summary>
		/// The minimum possible value for the oscillator's frequency (used only if OverrideFrequency is true)
		/// </summary>
		[Tooltip("The minimum possible value for the oscillator's frequency (used only if OverrideFrequency is true)")]
		public float OverrideMinFrequency;

		/// <summary>
		/// The maximum possible value for the oscillator's frequency (used only if OverrideFrequency is true)
		/// </summary>
		[Tooltip("The maximum possible value for the oscillator's frequency (used only if OverrideFrequency is true)")]
		public float OverrideMaxFrequency;

		/// <summary>
		/// Randomises the Amplitude and Center values of the target oscillator
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		public void RandomiseRange(ref Oscillator Target)
		{
			Target.Amplitude = Random.Range(0, (AbsoluteMax - AbsoluteMin) * 0.5f);
			Target.Center = Random.Range(AbsoluteMin + Target.Amplitude, AbsoluteMax - Target.Amplitude);
		}

		/// <summary>
		/// Ensures the target oscillator moves between zero and two pi (used for sawtooth waves)
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		public void EnforceTwoPi(ref Oscillator Target)
		{
			Target.Amplitude = Mathf.PI * 2f;
			Target.Center = Mathf.PI;
		}

		/// <summary>
		/// Randomises the Center value of the target oscillator
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		public void RandomiseCenter(ref Oscillator Target)
		{
			Target.Center = Random.Range(AbsoluteMin, AbsoluteMax);
		}

		/// <summary>
		/// Randomises the Phase value of the target oscillator
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		public void RandomisePhase(ref Oscillator Target)
		{
			Target.Phase = Random.Range(0f, 1f);
		}

		/// <summary>
		/// Randomises the Frequency value of the target oscillator using the Override Minimum and Maximum Frequency values for this OscillatorRandomiator
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		public void RandomiseFrequency(ref Oscillator Target)
		{
			RandomiseFrequency(ref Target, OverrideMinFrequency, OverrideMaxFrequency);
		}

		/// <summary>
		/// Randomises the Frequency value of the target oscillator using the Override Minimum and Maximum Frequency values for this OscillatorRandomiator
		/// </summary>
		/// <param name="Target">The oscillator to be modified</param>
		/// <param name="MinimumFrequency">The minimum possible value for the target oscillator's frequency (in seconds)</param>
		/// <param name="MaximumFrequency">The maximum possible value for the target oscillator's frequency (in seconds)</param>
		public void RandomiseFrequency(ref Oscillator Target, float MinimumFrequency, float MaximumFrequency)
		{
			Target.Period = 1f / Random.Range(MinimumFrequency, MaximumFrequency);
		}

		/// <summary>
		/// Ensures that we never exceed the provided min and max frequency values (uses internal override values)
		/// </summary>
		public void EnforceMaxFrequency(ref Oscillator Target)
		{
			//this is the coefficient of X at each local maxima of the derivative of the waveform function
			var coefficient = 1f;

			switch (Target.Type) {
				case OscillatorShape.Constant:
					//we don't need to check change per second because it's not changing!
					return;
				case OscillatorShape.Sine:
					coefficient = 2f * Mathf.PI;
					break;
				case OscillatorShape.Sawtooth:
					coefficient = 2f;
					break;
				case OscillatorShape.Triangle:
					coefficient = 4f;
					break;
				case OscillatorShape.Square:
					//we don't need to check change per second because it's changing instantly!
					return;
			}

			//this just makes sure that even at the minimum possible frequency, the function won't ever have a point where it's changing faster than MaxChangePerSecond
			var calculatedMaxFrequency = MaxChangePerSecond / (coefficient * Target.Amplitude);
			var calculatedMinPeriod = 1f / calculatedMaxFrequency;
			Target.Period = Mathf.Max(calculatedMinPeriod, Target.Period);
		}
	}
}