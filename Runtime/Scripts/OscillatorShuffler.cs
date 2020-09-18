using System;
using UnityEngine;
using Random = UnityEngine.Random;

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


		/// <summary>
		/// The maximum value the oscillator's value will change each second (overrides min frequency, doesn't apply in Constant or Square waveforms)
		/// </summary>
		[Space(10)]
		[Tooltip(
			"The maximum value the oscillator's value will change each second (overrides min frequency, doesn't apply in Constant or Square waveforms)")]
		public float MaxChangePerSecond;


		/// <summary>
		/// Whether to override the minimum and maximum frequency settings of the parent Shuffler object
		/// </summary>
		[Space(10)]
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
		/// <param name="target">The oscillator to be modified</param>
		public void RandomiseRange(ref Oscillator target)
		{
			target.Amplitude = Random.Range(0, (AbsoluteMax - AbsoluteMin) * 0.5f);
			target.Center = Random.Range(AbsoluteMin + target.Amplitude, AbsoluteMax - target.Amplitude);
		}

		/// <summary>
		/// Ensures the target oscillator moves between zero and two pi (used for sawtooth waves)
		/// </summary>
		/// <param name="target">The oscillator to be modified</param>
		public static void EnforceTwoPi(ref Oscillator target)
		{
			target.Amplitude = Mathf.PI * 2f;
			target.Center = Mathf.PI;
		}

		/// <summary>
		/// Randomises the Center value of the target oscillator
		/// </summary>
		/// <param name="target">The oscillator to be modified</param>
		public void RandomiseCenter(ref Oscillator target)
		{
			target.Center = Random.Range(AbsoluteMin, AbsoluteMax);
		}

		/// <summary>
		/// Randomises the Phase value of the target oscillator
		/// </summary>
		/// <param name="target">The oscillator to be modified</param>
		public static void RandomisePhase(ref Oscillator target)
		{
			target.Phase = Random.Range(0f, 1f);
		}

		/// <summary>
		/// Randomises the Frequency value of the target oscillator using the Override Minimum and Maximum Frequency values for this OscillatorRandomiator
		/// </summary>
		/// <param name="target">The oscillator to be modified</param>
		public void RandomiseFrequency(ref Oscillator target)
		{
			RandomiseFrequency(ref target, OverrideMinFrequency, OverrideMaxFrequency);
		}

		/// <summary>
		/// Randomises the Frequency value of the target oscillator using the Override Minimum and Maximum Frequency values for this OscillatorRandomiator
		/// </summary>
		/// <param name="target">The oscillator to be modified</param>
		/// <param name="minimumFrequency">The minimum possible value for the target oscillator's frequency (in seconds)</param>
		/// <param name="maximumFrequency">The maximum possible value for the target oscillator's frequency (in seconds)</param>
		public static void RandomiseFrequency(ref Oscillator target, float minimumFrequency, float maximumFrequency)
		{
			target.Period = 1f / Random.Range(minimumFrequency, maximumFrequency);
		}

		/// <summary>
		/// Ensures that we never exceed the provided min and max frequency values (uses internal override values)
		/// </summary>
		public void EnforceMaxFrequency(ref Oscillator target)
		{
			//this is the coefficient of X at each local maxima of the derivative of the waveform function
			var coefficient = 1f;

			switch (target.Type) {
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
				default:
					throw new ArgumentOutOfRangeException($"Unexpected value {target.Type} for OscillatorShuffler EnforceMaxFrequency");
			}

			//this just makes sure that even at the minimum possible frequency, the function won't ever have a point where it's changing faster than MaxChangePerSecond
			var calculatedMaxFrequency = MaxChangePerSecond / (coefficient * target.Amplitude);
			var calculatedMinPeriod = 1f / calculatedMaxFrequency;
			target.Period = Mathf.Max(calculatedMinPeriod, target.Period);
		}
	}
}