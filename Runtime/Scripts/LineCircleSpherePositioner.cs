using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{

    [RequireComponent(typeof(LineCircle))]
    public class LineCircleSpherePositioner : MonoBehaviour
    {
        [Header("Velocities")]
        public float ScaleAcceleration = 1f;
        public float ScaleMaxVelocity = 1f;
        public float ScaleSlowdownTime = 1f;
        [Space(10)] 
        public float PositionAcceleration = 1f;
        public float PositionMaxVelocity = 1f;
        public float PositionSlowdownTime = 1f;

        private float _scaleVelocity;
        private Vector3 _positionVelocity;

        private LineCircle _lineCircle;
        private Bounds _bounds;
        private Bounds _currentBounds;
        private Bounds _maxBounds;

        [Header("Settings")]
        public bool UseCurrentBounds = true;
        [Range(0f, 1f)] public float ActiveLevel;
        public float TargetFillSize = 1f;
        public Vector3 CenterPosition = Vector3.zero;

        [Header("Output")]
        public float TargetScale;

        public float Scale;
        public Vector3 TargetPosition;
        public Vector3 Position;

        [Header("Debug")] 
        public bool VisualizeBounds = false;

        public void Awake()
        {
            _lineCircle = GetComponent<LineCircle>();
        }

        public void Update()
        {
            _currentBounds = _lineCircle.GetBounds();
            _maxBounds = _lineCircle.GetMaxPossibleBounds();
            _bounds = UseCurrentBounds ? _currentBounds : _maxBounds;
            
            var longestSide = Mathf.Max(_bounds.size.x, Mathf.Max(_bounds.size.y, _bounds.size.z));
            if (longestSide > 0f) {
                TargetScale = (0.8f * 0.78539816f * TargetFillSize / longestSide);
            } else {
                TargetScale = 0f;
            }
            if (float.IsNaN(TargetScale)) TargetScale = 0f;

            var centerPoint = _bounds.center;
            centerPoint.Scale(Vector3.one * TargetScale);
            TargetPosition = -centerPoint;
            TargetPosition += CenterPosition;
            if (float.IsNaN(TargetPosition.x) || float.IsNaN(TargetPosition.y) || float.IsNaN(TargetPosition.z))
                TargetPosition = Vector3.zero;
            
            var currentScale = Scale;
            _scaleVelocity += (TargetScale - currentScale) * ScaleAcceleration * Time.deltaTime;
            if (Mathf.Abs(_scaleVelocity) > ScaleMaxVelocity)
                _scaleVelocity = Mathf.Sign(_scaleVelocity) * ScaleMaxVelocity;

            if (Mathf.Abs(_scaleVelocity) > 0f) {
                var scaleArriveTime = (TargetScale - currentScale) / _scaleVelocity;
                _scaleVelocity *= Mathf.Clamp01(scaleArriveTime / ScaleSlowdownTime);

                Scale += _scaleVelocity * Time.deltaTime;
                transform.localScale = Vector3.one * Mathf.Lerp(1f, Scale, ActiveLevel);
            }

            var currentPosition = Position;
            _positionVelocity += (TargetPosition - currentPosition) * PositionAcceleration * Time.deltaTime;
            if (_positionVelocity.sqrMagnitude > PositionMaxVelocity * PositionMaxVelocity)
                _positionVelocity = _positionVelocity.normalized * PositionMaxVelocity;

            if (_positionVelocity.sqrMagnitude > 0f) {
                var positionArriveTime = (TargetPosition - currentPosition).magnitude / _positionVelocity.magnitude;
                _positionVelocity *= Mathf.Clamp01(positionArriveTime / PositionSlowdownTime);

                Position += _positionVelocity * Time.deltaTime;
                transform.localPosition = Vector3.Lerp(CenterPosition, Position, ActiveLevel);
            }
            
            if(VisualizeBounds) DrawDebugBounds();
        }

        public void DrawDebugBounds()
                 {
                     //Debug dynamic bounds
         
                     var mmm = transform.TransformPoint(new Vector3(_bounds.min.x, _bounds.min.y, _bounds.min.z));
                     var mmM = transform.TransformPoint(new Vector3(_bounds.min.x, _bounds.min.y, _bounds.max.z));
                     var mMm = transform.TransformPoint(new Vector3(_bounds.min.x, _bounds.max.y, _bounds.min.z));
                     var mMM = transform.TransformPoint(new Vector3(_bounds.min.x, _bounds.max.y, _bounds.max.z));
                     
                     var Mmm = transform.TransformPoint(new Vector3(_bounds.max.x, _bounds.min.y, _bounds.min.z));
                     var MmM = transform.TransformPoint(new Vector3(_bounds.max.x, _bounds.min.y, _bounds.max.z));
                     var MMm = transform.TransformPoint(new Vector3(_bounds.max.x, _bounds.max.y, _bounds.min.z));
                     var MMM = transform.TransformPoint(new Vector3(_bounds.max.x, _bounds.max.y, _bounds.max.z));
         
                     //bottom
                     Debug.DrawLine(mmm, mmM, Color.white);
                     Debug.DrawLine(Mmm, MmM, Color.white);
                     Debug.DrawLine(mmm, Mmm, Color.white);
                     Debug.DrawLine(mmM, MmM, Color.white);
                     
                     //top
                     Debug.DrawLine(mMm, mMM, Color.white);
                     Debug.DrawLine(MMm, MMM, Color.white);
                     Debug.DrawLine(mMm, MMm, Color.white);
                     Debug.DrawLine(mMM, MMM, Color.white);
                     
                     //edges
                     Debug.DrawLine(mmm, mMm, Color.white);
                     Debug.DrawLine(mmM, mMM, Color.white);
                     Debug.DrawLine(Mmm, MMm, Color.white);
                     Debug.DrawLine(MmM, MMM, Color.white);
                 }
        

    }
}