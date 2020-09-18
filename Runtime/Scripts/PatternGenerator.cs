using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Class used to shuffle the oscillator and other values of a LineCircle
	/// </summary>
	[System.Serializable]
	public class PatternGenerator
	{

		[Header("Settings")]
		/// <summary>
		/// Snapshot count to use in generated patterns
		/// </summary>
		[Tooltip("Snapshot count to use in generated patterns")]
		public int Count;
		
		/// <summary>
		/// Maximum number of lines per circle
		/// </summary>
		[Tooltip("Maximum number of lines per circle")]
		public int MaxLineCount;

		/// <summary>
		/// The chance that a given parameter will be shuffled
		/// </summary>
		[Tooltip("The chance that a given parameter will be shuffled")]
		public float GlobalShuffleChance = 1f;

		/// <summary>
		/// The chance that we will swap between cartesian and polar coordinates on a shuffle
		/// </summary>
		[Tooltip("The chance that we will swap between cartesian and polar coordinates on a shuffle")]
		public float SphericalSwapChance = 0.5f;

		/// <summary>
		/// The chance that we will reset an oscillator to hold its value on a shuffle
		/// </summary>
		[Tooltip("The chance that we will reset an oscillator to hold its value on a shuffle")]
		public float FreezeChance = 0.3f;

		/// <summary>
		/// The chance that we will set an eligable oscillator to be a sawtooth wave on shuffle
		/// </summary>
		[Tooltip("The chance that we will set an eligable oscillator to be a sawtooth wave on shuffle")]
		public float SawtoothChance = 0.5f;


		[Space(10)]


		/// <summary>
		/// The minimum frequency for all oscillators
		/// </summary>
		[Tooltip("The minimum frequency for all oscillators")]
		public float GlobalMinimumFrequency = 0f;

		/// <summary>
		/// The maximum frequency for all oscillators (overridden by MaxChangePerSecond)
		/// </summary>
		[Tooltip("The maximum frequency for all oscillators (overridden by MaxChangePerSecond)")]
		public float GlobalMaximumFrequency = 0.2f;


		[Space(10)]


		/// <summary>
		/// Whether to disallow any oscillators from moving in the Z dimension (i.e. 2D mode toggle)
		/// </summary>
		[Tooltip("Whether to disallow any oscillators from moving in the Z dimension (i.e. 2D mode toggle)")]
		public bool RestrictThirdDimension = true;

		/// <summary>
		/// Shufflers for each target Oscillator
		/// </summary>
		[Tooltip("Shufflers for each target Oscillator")]
		public OscillatorShuffler[] Shufflers;

		/// <summary>
		/// Generates a new LineCircle pattern, shuffling parameters of a previous pattern
		/// </summary>
		/// <param name="PreviousPattern">Previous pattern to use as shuffle base</param>
		/// <returns>A shuffled version of the previous pattern</returns>
		public LineCirclePattern GenerateNewPattern(LineCirclePattern PreviousPattern)
		{
			LineCirclePattern NewPattern = new LineCirclePattern(PreviousPattern);

			//first we decide whether we should be spherical or cartesian
			if (Roll(SphericalSwapChance)) {
				NewPattern.SphericalCoordinates = true;

				//Shuffle X (aka radius)
				RollShuffler(ID.CirclePosX, ref NewPattern);

				//Shuffle Y (aka theta) if in 3D mode
				if (!RestrictThirdDimension) {
					RollShufflerLoopable(ID.CirclePosY, ref NewPattern);
				} else {
					//reset to make sure everything is in 2D
					string savedname = NewPattern.Oscillators[ID.CirclePosY].Name;
					NewPattern.Oscillators[ID.CirclePosY] = new Oscillator("CirclePosY", 0, 0, 0, 1, 0);
					NewPattern.Oscillators[ID.CirclePosY].Name = savedname;
				}

				//Shuffle Z (aka phi)
				RollShufflerLoopable(ID.CirclePosZ, ref NewPattern);

			} else {
				NewPattern.SphericalCoordinates = false;

				//Shuffle X and Y
				RollShuffler(ID.CirclePosX, ref NewPattern);
				RollShuffler(ID.CirclePosY, ref NewPattern);

				//Shuffle Z if in 3D mode
				if (!RestrictThirdDimension) RollShuffler(ID.CirclePosZ, ref NewPattern);
				else {
					//reset to make sure everything is in 2D
					string savedname = NewPattern.Oscillators[ID.CirclePosZ].Name;
					NewPattern.Oscillators[ID.CirclePosZ] = new Oscillator("CirclePosZ", 0, 0, 0, 1, 0);
					NewPattern.Oscillators[ID.CirclePosZ].Name = savedname;
				}
			}

			//Shuffle rotation about X and Y axes if in 3D mode
			if (!RestrictThirdDimension) {
				RollShufflerLoopable(ID.CircleRotX, ref NewPattern);
				RollShufflerLoopable(ID.CircleRotY, ref NewPattern);
			} else {
				//reset to make sure everything is in 2D
				string savedname = NewPattern.Oscillators[ID.CircleRotX].Name;
				NewPattern.Oscillators[ID.CircleRotX] = new Oscillator("CircleRotX", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.CircleRotX].Name = savedname;

				savedname = NewPattern.Oscillators[ID.CircleRotY].Name;
				NewPattern.Oscillators[ID.CircleRotY] = new Oscillator("CircleRotY", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.CircleRotY].Name = savedname;
			}

			//Shuffle rotation about Z axis
			RollShufflerLoopable(ID.CircleRotZ, ref NewPattern);

			//Shuffle radius
			RollShuffler(ID.CircleRad, ref NewPattern);

			//Shuffle offset about X and Y axes if in 3D mode
			if (!RestrictThirdDimension) {
				RollShufflerLoopable(ID.LineRotX, ref NewPattern);
				RollShufflerLoopable(ID.LineRotY, ref NewPattern);
			} else {
				//reset to make sure everything is in 2D
				string savedname = NewPattern.Oscillators[ID.LineRotX].Name;
				NewPattern.Oscillators[ID.LineRotX] = new Oscillator("LineRotX", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.LineRotX].Name = savedname;

				savedname = NewPattern.Oscillators[ID.LineRotY].Name;
				NewPattern.Oscillators[ID.LineRotY] = new Oscillator("LineRotY", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.LineRotY].Name = savedname;
			}

			//Shuffle offset about Z axis
			RollShufflerLoopable(ID.LineRotZ, ref NewPattern);

			//custom - line length and colors

			//for line length we only randomise the total length
			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.LineLength] = new Oscillator("LineLength", 0,
					Random.Range(Shufflers[ID.LineLength].AbsoluteMin, Shufflers[ID.LineLength].AbsoluteMax), 0, 1, 0);
				NewPattern.Oscillators[ID.LineLength].Name = "Line Length";
			}


			//we keep amplitude for both color values at zero because otherwise the pattern is just a rainbow mess
			//mmm...rainbow mess
			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.ColorRange].Amplitude = 0f;
				NewPattern.Oscillators[ID.ColorRange].Center = Random.Range(0f, 1f);
			}

			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.ColorOffset].Amplitude = 0f;
				NewPattern.Oscillators[ID.ColorOffset].Center = Random.Range(0f, 1f);
			}

			//Shuffle line count
			if (Roll(GlobalShuffleChance)) {
				NewPattern.LineCount = Random.Range(3, MaxLineCount + 1);
			}

			return NewPattern;
		}

		/// <summary>
		/// Generates a new LineCircle pattern completely randomly (within Shuffler parameters)
		/// </summary>
		/// <returns>A random LineCircle Pattern</returns>
		public LineCirclePattern GenerateNewPattern()
		{
			LineCirclePattern NewPattern = new LineCirclePattern(Count);

			//temporarily set shuffle chance to 1
			float SavedShuffleChance = GlobalShuffleChance;
			GlobalShuffleChance = 1f;

			//first we decide whether we should be spherical or cartesian
			NewPattern.SphericalCoordinates = Random.Range(0, 2) == 0 ? true : false;

			if (NewPattern.SphericalCoordinates) {
				//Shuffle X (aka radius)
				RollShuffler(ID.CirclePosX, ref NewPattern);

				//Shuffle Y (aka theta) if in 3D mode
				if (!RestrictThirdDimension) {
					RollShufflerLoopable(ID.CirclePosY, ref NewPattern);
				} else {
					//reset to make sure everything is in 2D
					string savedname = NewPattern.Oscillators[ID.CirclePosY].Name;
					NewPattern.Oscillators[ID.CirclePosY] = new Oscillator("CirclePosY", 0, 0, 0, 1, 0);
					NewPattern.Oscillators[ID.CirclePosY].Name = savedname;
				}

				//Shuffle Z (aka phi)
				RollShufflerLoopable(ID.CirclePosZ, ref NewPattern);
			} else {
				//Shuffle X and Y
				RollShuffler(ID.CirclePosX, ref NewPattern);
				RollShuffler(ID.CirclePosY, ref NewPattern);

				//Shuffle Z if in 3D mode
				if (!RestrictThirdDimension) RollShuffler(ID.CirclePosZ, ref NewPattern);
				else {
					//reset to make sure everything is in 2D
					string savedname = NewPattern.Oscillators[ID.CirclePosZ].Name;
					NewPattern.Oscillators[ID.CirclePosZ] = new Oscillator("CirclePosZ", 0, 0, 0, 1, 0);
					NewPattern.Oscillators[ID.CirclePosZ].Name = savedname;
				}
			}

			//Shuffle rotation about X and Y axes if in 3D mode
			if (!RestrictThirdDimension) {
				RollShufflerLoopable(ID.CircleRotX, ref NewPattern);
				RollShufflerLoopable(ID.CircleRotY, ref NewPattern);
			} else {
				//reset to make sure everything is in 2D
				string savedname = NewPattern.Oscillators[ID.CircleRotX].Name;
				NewPattern.Oscillators[ID.CircleRotX] = new Oscillator("CircleRotX", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.CircleRotX].Name = savedname;

				savedname = NewPattern.Oscillators[ID.CircleRotY].Name;
				NewPattern.Oscillators[ID.CircleRotY] = new Oscillator("CircleRotY", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.CircleRotY].Name = savedname;
			}

			//Shuffle rotation about Z axis
			RollShufflerLoopable(ID.CircleRotZ, ref NewPattern);

			//Shuffle radius
			RollShuffler(ID.CircleRad, ref NewPattern);

			//Shuffle offset about X and Y axes if in 3D mode
			if (!RestrictThirdDimension) {
				RollShufflerLoopable(ID.LineRotX, ref NewPattern);
				RollShufflerLoopable(ID.LineRotY, ref NewPattern);
			} else {
				//reset to make sure everything is in 2D
				string savedname = NewPattern.Oscillators[ID.LineRotX].Name;
				NewPattern.Oscillators[ID.LineRotX] = new Oscillator("LineRotX", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.LineRotX].Name = savedname;

				savedname = NewPattern.Oscillators[ID.LineRotY].Name;
				NewPattern.Oscillators[ID.LineRotY] = new Oscillator("LineRotY", 0, 0, 0, 1, 0);
				NewPattern.Oscillators[ID.LineRotY].Name = savedname;
			}

			//Shuffle offset about Z axis
			RollShufflerLoopable(ID.LineRotZ, ref NewPattern);

			//custom - line length and colors

			//for line length we only randomise the total length
			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.LineLength] = new Oscillator("LineLength", 0,
					Random.Range(Shufflers[ID.LineLength].AbsoluteMin, Shufflers[ID.LineLength].AbsoluteMax), 0, 1, 0);
				NewPattern.Oscillators[ID.LineLength].Name = "Line Length";
			}


			//we keep amplitude for both color values at zero because otherwise the pattern is just a rainbow mess
			//mmm...rainbow mess
			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.ColorRange].Amplitude = 0f;
				Shufflers[ID.ColorRange].RandomiseCenter(ref NewPattern.Oscillators[ID.ColorRange]);
			}

			if (Roll(GlobalShuffleChance)) {
				NewPattern.Oscillators[ID.ColorOffset].Amplitude = 0f;
				Shufflers[ID.ColorOffset].RandomiseCenter(ref NewPattern.Oscillators[ID.ColorOffset]);
			}

			//Shuffle line count
			if (Roll(GlobalShuffleChance)) {
				NewPattern.LineCount = Random.Range(3, MaxLineCount + 1);
			}

			//reset shuffle chance to whatever it was before we started this method
			GlobalShuffleChance = SavedShuffleChance;

			return NewPattern;
		}

		/// <summary>
		/// Reusable code snippet to shuffle a given oscillator
		/// </summary>
		/// <param name="index">Index of oscillator shuffler to target</param>
		public void RollShuffler(int index, ref LineCirclePattern pattern)
		{

			//check whether we should shuffle at all
			if (Roll(GlobalShuffleChance)) {

				//check whether we should freeze
				if (Roll(FreezeChance)) {
					Shufflers[index].RandomiseCenter(ref pattern.Oscillators[index]);
					pattern.Oscillators[index].Amplitude = 0;
				} else {
					Shufflers[index].RandomiseRange(ref pattern.Oscillators[index]);
					Shufflers[index].RandomisePhase(ref pattern.Oscillators[index]);

					//check whether we should use override frequency values
					if (Shufflers[index].OverrideFrequency)
						Shufflers[index].RandomiseFrequency(ref pattern.Oscillators[index]);
					else
						Shufflers[index].RandomiseFrequency(ref pattern.Oscillators[index], GlobalMinimumFrequency,
							GlobalMaximumFrequency);
				}

				Shufflers[index].EnforceMaxFrequency(ref pattern.Oscillators[index]);
			}
		}

		/// <summary>
		/// Reusable code snippet to shuffle a given oscillator (used for angular oscillators that can be a sawtooth wave)
		/// </summary>
		/// <param name="index">Index of oscillator shuffler to target</param>
		public void RollShufflerLoopable(int index, ref LineCirclePattern pattern)
		{
			//check to see if we should be a sawtooth
			if (Roll(SawtoothChance))
				pattern.Oscillators[index].Type = OscillatorShape.Sawtooth;
			else
				pattern.Oscillators[index].Type = OscillatorShape.Sine;

			//do normal stuff
			RollShuffler(index, ref pattern);

			//if sawtooth, make sure we go from 0 to 2 pi (so one full revolution)
			if (pattern.Oscillators[index].Type == OscillatorShape.Sawtooth) {
				Shufflers[index].EnforceTwoPi(ref pattern.Oscillators[index]);
				Shufflers[index].EnforceMaxFrequency(ref pattern.Oscillators[index]);
			}
		}

		/// <summary>
		/// Shortcut function to make code neater
		/// </summary>
		/// <param name="Chance">Chance from zero to one we will return true</param>
		/// <returns>Result of Random.Range call</returns>
		private bool Roll(float Chance)
		{
			return Random.Range(0f, 1f) < Chance;
		}
	}
}