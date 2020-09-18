using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	/// <summary>
	/// Updates LineCircle time offset without moving lines
	/// </summary>
	[RequireComponent(typeof(LineCircle))]
	public class TimeStepper : MonoBehaviour
	{

		//target line circle
		private LineCircle _lineCircle;

		//time offset
		private float _t;

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
		[Range(1f, 60f)] public float TimeSpanOscillationPeriod = 30f;

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

		private bool _hasShuffler;
		private Shuffler _shuffler;

		public void OnEnable()
		{
			_lineCircle = GetComponent<LineCircle>();
			if (_lineCircle == null) {
				Debug.LogError("TimeStepper couldn't find an attached LineCircle component! Disabling.");
				enabled = false;
				return;
			}
			_lineCircle.OnPatternChanged += HandleNewPattern;

			_shuffler = GetComponent<Shuffler>();
			_hasShuffler = _shuffler != null;
		}
		
		void Update()
		{
			if (_lineCircle == null) return;

			//every [interval] frames...
			if (Time.frameCount % Interval != 0 || Pause) return;
			
			if (OscillateTimespan) {
				_timeSpanOscillationT += Time.deltaTime * 2f * Mathf.PI / TimeSpanOscillationPeriod;

				var lastFrontEdge = _lineCircle.Pattern.TimeSpan + _lineCircle.Pattern.TimeOffset;
				var oscillationLerp = 0.5f - 0.5f * Mathf.Cos(_timeSpanOscillationT);
				
				//Update Timespan
				_lineCircle.Pattern.TimeSpan = Mathf.Lerp(
					Mathf.Lerp(0f, _lineCircle.Pattern.MaxTimePossible, MaxTimeLerp),
					_lineCircle.Pattern.MaxTimePossible,
					oscillationLerp);
				
				//If we've finished an oscillation period, shuffle to the next pattern
				if (_timeSpanOscillationT >= Mathf.PI * 2f) {
					if(_hasShuffler) _shuffler.NextPattern();
				}
				
				//Ensure that the front-facing edge is exactly on a line
				_lineCircle.Pattern.TimeSpan -= _lineCircle.Pattern.TimeSpan % _lineCircle.Pattern.TimeStep;
				var desiredFrontEdge = lastFrontEdge + _lineCircle.Pattern.TimeStep * Count;
				if(!float.IsNaN(desiredFrontEdge) && !float.IsNaN(_lineCircle.Pattern.TimeSpan)) _lineCircle.Pattern.TimeOffset = desiredFrontEdge - _lineCircle.Pattern.TimeSpan;
				//LineCircle.Pattern.TimeOffset += LineCircle.Pattern.TimeStep * Count; //increase time offset
			} else {
				if (StepOffset) _lineCircle.Pattern.TimeOffset += _lineCircle.Pattern.TimeStep * Count;

				//increase draw count
				if (!StepCount) return;
				
				if (_lineCircle.Pattern.TimeSpan >= _lineCircle.Pattern.MaxTimePossible) {
					_lineCircle.Pattern.TimeSpan = _lineCircle.Pattern.MaxTimePossible;
					_lineCircle.Pattern.TimeOffset += _lineCircle.Pattern.TimeStep * Count;
				} else {
					_lineCircle.Pattern.TimeSpan += _lineCircle.Pattern.TimeStep * Count;
				}
			}


		}

		private void HandleNewPattern(object sender, EventArgs e)
		{
			_timeSpanOscillationT = 0f;
		}
	}
}