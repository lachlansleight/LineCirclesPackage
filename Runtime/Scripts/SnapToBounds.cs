using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
    [RequireComponent(typeof(LineCircle))]
    public class SnapToBounds : MonoBehaviour
    {

        public bool DrawBounds;
        public bool SetPositionOnUpdate;
        public bool SetScaleOnUpdate;
        [Range(0f, 2f)] public float SizeMultiplier = 1f;
        public float PerspectiveCenterDistance = 5f;
        
        private LineCircle _lineCircle;
        private Camera _camera;
        private float _targetSize;
        public Bounds _bounds;
        
        public void OnEnable()
        {
            _camera = FindObjectOfType<Camera>();
            _lineCircle = GetComponent<LineCircle>();
            if (_lineCircle == null) {
                Debug.LogError("SnapToBounds didn't find attached LineCircle component! Disabling.");
                enabled = false;
                return;
            }

            _lineCircle.OnPatternChanged += HandlePatternChange;
        }

        public void Update()
        {
            //_bounds = _lineCircle.Pattern.GetMaxPossibleBounds();
            if(DrawBounds) DrawDebugBounds(_bounds);
            SetFromBounds(_bounds);
        }
        
        public void DrawDebugBounds(Bounds bounds)
        {
            //Debug dynamic bounds
         
            var mmm = transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
            var mmM = transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
            var mMm = transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
            var mMM = transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
                     
            var Mmm = transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
            var MmM = transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
            var MMm = transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
            var MMM = transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));
         
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

        private void HandlePatternChange(object sender, EventArgs e)
        {
            _bounds = _lineCircle.GetMaxPossibleBounds();
            SetFromBounds(_bounds, true);
        }

        private void SetFromBounds(Bounds bounds, bool force = false)
        {
            var camRatio = (float) _camera.pixelWidth / _camera.pixelHeight;
            if (_camera.orthographic) {
                //simply ensure the final bounds XY box fits in the camera's orthographic box
                var orthoSize = _camera.orthographicSize;
                _targetSize = 2f * Mathf.Min(
                    orthoSize / bounds.size.y,
                    orthoSize * camRatio / bounds.size.x);
            } else {
                var frustumHeight = 2.0f * PerspectiveCenterDistance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                var frustumWidth = frustumHeight * camRatio;
                _targetSize = Mathf.Min(
                    frustumWidth / bounds.size.x,
                    frustumHeight / bounds.size.y);
            }

            var t = transform;
            if (SetScaleOnUpdate || force) {
                t.localScale = SizeMultiplier * _targetSize * Vector3.one;
            }
            if (SetPositionOnUpdate || force) {
                var scale = t.localScale.x;
                var zPos = _camera.orthographic 
                    ? _camera.nearClipPlane + bounds.size.z * scale
                    : PerspectiveCenterDistance - bounds.center.z * scale;
                t.position = new Vector3(bounds.center.x * -1f * scale, bounds.center.y * -1f * scale, zPos);
            }
        }
    }
}