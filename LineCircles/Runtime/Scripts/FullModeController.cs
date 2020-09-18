using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LineCircles;

namespace Mountaineer
{
    public class FullModeController : MonoBehaviour
    {

        [Header("Required")]
        public TimeStepper TimeStepper;
        public LineCircleSpherePositioner SpherePositioner;
        

        /// <summary>
        /// Whether to attempt to force the pattern to fill the screen (for use as a reading / writing light)
        /// </summary>
        [Tooltip("Whether to attempt to force the pattern to fill the screen (for use as a reading / writing light)")]
        public bool ForceFullscreen = false;
        /// <summary>
        /// Speed at which to interpolate into and out of full-screen mode
        /// </summary>
        [Tooltip("Speed at which to interpolate into and out of full-screen mode")]
        [Range(1f, 20f)] public float ForceFullscreenLerpSpeed = 8f;
        private float _forceFullscreenT = 0f;

        public bool ControlTimeLerp = true;

        public void Start()
        {

        }

        public void Update()
        {
            _forceFullscreenT = Mathf.Lerp(
                _forceFullscreenT, 
                ForceFullscreen ? 1f : 0f, 
                Time.deltaTime * ForceFullscreenLerpSpeed);

            TimeStepper.MaxTimeLerp = _forceFullscreenT;
            SpherePositioner.UseCurrentBounds = ForceFullscreen;
            SpherePositioner.ActiveLevel = 1f;
        }

        public void SetFullscreen(bool value)
        {
            ForceFullscreen = value;
        }
    }
}