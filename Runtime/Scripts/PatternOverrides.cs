using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
    [RequireComponent(typeof(LineCircle))]
    public class PatternOverrides : MonoBehaviour
    {

        [Range(1, 20)] public int LineInterval;
        [Range(0.001f, 0.05f)] public float TimeStep;
        public bool DrawLines;
        [Range(0f, 1f)] public float LineOpacity;
        public bool DrawFill;
        [Range(0f, 1f)] public float FillOpacity;
        [Range(0f, 1f)] public bool AutoScaleLines;
        
        private LineCircle _lineCircle;

        private void Awake()
        {
            _lineCircle = GetComponent<LineCircle>();
            if (_lineCircle == null) {
                Debug.LogError("PatternOverrides couldn't find LineCircle component! Disabling.");
                return;
            }
            LineInterval = _lineCircle.Pattern.LineInterval;
            TimeStep = _lineCircle.Pattern.TimeStep;
            DrawLines = _lineCircle.Pattern.DrawLines;
            LineOpacity = _lineCircle.Pattern.LineColor.a;
            DrawFill = _lineCircle.Pattern.DrawFill;
            FillOpacity = _lineCircle.Pattern.FillColor.a;
            //AutoScaleLines = _lineCircle.Pattern.AutoScaleLines; //TODO
        }

        private void Update()
        {
            _lineCircle.Pattern.LineInterval = LineInterval;
            _lineCircle.Pattern.TimeStep = TimeStep;
            _lineCircle.Pattern.DrawLines = DrawLines;
            _lineCircle.Pattern.LineColor = new Color(1f, 1f, 1f, LineOpacity);
            _lineCircle.Pattern.DrawFill = DrawFill;
            _lineCircle.Pattern.FillColor = new Color(1f, 1f, 1f, FillOpacity);
            //_lineCircle.Pattern.AutoScaleLines = AutoScaleLines; //TODO
        }
    }
}