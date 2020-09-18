using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
	public class PatternOscillatorAnimator : MonoBehaviour
	{

		public LineCircle Target;

		public ValueType TargetValue;
		public ValueParameter TargetParameter;

		public Oscillator Oscillator;

		private float _t;

		public void Update()
		{
			_t += Time.deltaTime;
			var value = Oscillator.EvaluateAtTime(_t);
			var index = GetIndexFromValue(TargetValue);
			if (index == -1) return;

			switch (TargetParameter) {
				case ValueParameter.Center:
					Target.Pattern.Oscillators[index].Center = value;
					break;
				case ValueParameter.Amplitude:
					Target.Pattern.Oscillators[index].Amplitude = value;
					break;
				case ValueParameter.Period:
					Target.Pattern.Oscillators[index].Period = value;
					break;
				case ValueParameter.Phase:
					Target.Pattern.Oscillators[index].Phase = value;
					break;
			}
		}

		private int GetIndexFromValue(ValueType value)
		{
			var intIndex = (int) value;
			if (intIndex < 2) return -1;

			return intIndex - 2;
		}
	}
}