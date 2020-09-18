using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	public class PatternOscillatorAnimator : MonoBehaviour
	{
		public ValueType TargetValue;
		public ValueParameter TargetParameter;

		public Oscillator Oscillator;
		
		private LineCircle _lineCircle;
		private float _t;

		public void Awake()
		{
			_lineCircle = GetComponent<LineCircle>();
			if (_lineCircle == null) {
				Debug.LogError("PatternOscillatorAnimator couldn't find attached LineCircle component! Disabling.");
				enabled = false;
				return;
			}
		}
		
		public void Update()
		{
			_t += Time.deltaTime;
			var value = Oscillator.EvaluateAtTime(_t);
			var index = GetIndexFromValue(TargetValue);
			if (index == -1) return;

			switch (TargetParameter) {
				case ValueParameter.Center:
					_lineCircle.Pattern.Oscillators[index].Center = value;
					break;
				case ValueParameter.Amplitude:
					_lineCircle.Pattern.Oscillators[index].Amplitude = value;
					break;
				case ValueParameter.Period:
					_lineCircle.Pattern.Oscillators[index].Period = value;
					break;
				case ValueParameter.Phase:
					_lineCircle.Pattern.Oscillators[index].Phase = value;
					break;
				default:
					throw new ArgumentOutOfRangeException($"Unexpected target parameter {TargetParameter} for PatternOscillatorAnimator");
			}
		}

		private static int GetIndexFromValue(ValueType value)
		{
			var intIndex = (int) value;
			if (intIndex < 2) return -1;

			return intIndex - 2;
		}
	}
}