using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Updates LineCircle time offset without moving lines
	/// </summary>
	public class TimeStepper : MonoBehaviour
	{

		//target line circle
		public LineCircle LineCircle;

		//time offset
		private float t;

		/// <summary>
		/// Whether to step through the number of drawn time steps
		/// </summary>
		[Tooltip("Whether to step through the number of drawn time steps")]
		public bool StepCount = false;

		/// <summary>
		/// Whether to step through the pattern offset time
		/// </summary>
		[Tooltip("Whether to step through the pattern offset time")]
		public bool StepOffset = true;

		/// <summary>
		/// Whether to oscillate the timespan, rather than just stepping it forward
		/// </summary>
		[Tooltip("Whether to oscillate the timespan, rather than just stepping it forward")]
		public bool OscillateTimespan = true;

		/// <summary>
		/// Duration of time span oscillations
		/// </summary>
		[Tooltip("Duration of time span oscillations")]
		[Range(1f, 60f)] public float TimeSpanOscillationPeriod = 10f;

		/// <summary>
		/// Increase count every [n] frames
		/// </summary>
		[Range(1, 10)]
		[Tooltip("Increase offset by [n] [timestep] per frame")]
		public int Interval = 1;

		/// <summary>
		/// Increase offset by [n] [timestep] per frame
		/// </summary>
		[Range(-20, 20)]
		[Tooltip("Increase offset by [n] [timestep] per frame")]
		public int Count = 1;

		/// <summary>
		/// A way to manually Pause the TimeStepper
		/// </summary>
		[Tooltip("A way to manually Pause the TimeStepper")]
		public bool Pause;

		private float _timeSpanOscillationT;

		[Range(0f, 1f)] public float MaxTimeLerp = 0f;

		private Shuffler _shuffler;

		public void Awake()
		{
			if (LineCircle == null) {
				this.enabled = false;
				return;
			}
			
			//make sure we have a line circle
			if (LineCircle == null) {
				LineCircle = GameObject.FindObjectOfType<LineCircle>();
			}
			LineCircle.OnPatternChanged += HandleNewPattern;
		}
		
		void Update()
		{
			if (LineCircle == null) return;

			//every [interval] frames...
			if (Time.frameCount % Interval == 0 && !Pause) {
				if (OscillateTimespan) {
					_timeSpanOscillationT += Time.deltaTime * 2f * Mathf.PI / TimeSpanOscillationPeriod;

					var lastFrontEdge = LineCircle.Pattern.TimeSpan + LineCircle.Pattern.TimeOffset;
					var oscillationLerp = 0.5f - 0.5f * Mathf.Cos(_timeSpanOscillationT);
					
					LineCircle.Pattern.TimeSpan = Mathf.Lerp(
						Mathf.Lerp(0f, LineCircle.Pattern.MaxTimePossible, MaxTimeLerp),
						LineCircle.Pattern.MaxTimePossible,
						oscillationLerp);
					if (_timeSpanOscillationT >= Mathf.PI * 2f) {
						if (_shuffler == null) _shuffler = GetComponent<Shuffler>();
						_shuffler.NextPattern();
					}
					LineCircle.Pattern.TimeSpan -= LineCircle.Pattern.TimeSpan % LineCircle.Pattern.TimeStep;
					var desiredFrontEdge = lastFrontEdge + LineCircle.Pattern.TimeStep * Count;
					if(!float.IsNaN(desiredFrontEdge) && !float.IsNaN(LineCircle.Pattern.TimeSpan)) LineCircle.Pattern.TimeOffset = desiredFrontEdge - LineCircle.Pattern.TimeSpan;
					//LineCircle.Pattern.TimeOffset += LineCircle.Pattern.TimeStep * Count; //increase time offset
				} else {
					if (StepOffset) LineCircle.Pattern.TimeOffset += LineCircle.Pattern.TimeStep * Count;

					//increase draw count
					if (StepCount) {
						if (LineCircle.Pattern.TimeSpan >= LineCircle.Pattern.MaxTimePossible) {
							LineCircle.Pattern.TimeSpan = LineCircle.Pattern.MaxTimePossible;
							LineCircle.Pattern.TimeOffset += LineCircle.Pattern.TimeStep * Count;
						} else {
							LineCircle.Pattern.TimeSpan += LineCircle.Pattern.TimeStep * Count;
						}
					}
				}
			}
			
			
		}

		private void HandleNewPattern(object sender, EventArgs e)
		{
			_timeSpanOscillationT = 0f;
		}
	}
}