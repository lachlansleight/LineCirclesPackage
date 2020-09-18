using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Class for storing a stack of Patterns - kind of temporary
	/// </summary>
	[System.Serializable]
	public class PatternMemory
	{

		/// <summary>
		/// List of previous patterns
		/// </summary>
		public List<LineCirclePattern> Patterns;

		/// <summary>
		/// Current position within the Patterns list
		/// </summary>
		public int CurrentPosition;

		public PatternMemory()
		{
			Patterns = new List<LineCirclePattern>();
			CurrentPosition = 0;
		}
	}
}