using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineCircles
{
    public class LineCirclePauser : MonoBehaviour
    {

        private TimeStepper _timeStepper;
        private Shuffler _shuffler;
        private PatternOscillatorAnimator[] _animators;

        private bool _prePauseOscillateTimespan;
        private bool _prePauseAutoShuffle;

        private bool _paused;
        
        private void Awake()
        {
            _timeStepper = FindObjectOfType<TimeStepper>();
            _shuffler = FindObjectOfType<Shuffler>();
            _animators = FindObjectsOfType<PatternOscillatorAnimator>();
        }

        public void Pause()
        {
            _timeStepper.StepCount = false;
            _timeStepper.StepOffset = false;
            
            _prePauseOscillateTimespan = _timeStepper.OscillateTimespan;
            _timeStepper.OscillateTimespan = false;

            _prePauseAutoShuffle = _shuffler.DoAutoShuffle;
            _shuffler.DoAutoShuffle = false;

            foreach (var animator in _animators) animator.enabled = false;

            _paused = true;
        }

        public void Unpause()
        {
            _timeStepper.StepCount = true;
            _timeStepper.StepOffset = true;
            _timeStepper.OscillateTimespan = _prePauseOscillateTimespan;
            
            _shuffler.DoAutoShuffle = _prePauseAutoShuffle;
            
            foreach (var animator in _animators) animator.enabled = true;

            _paused = false;
        }

        public void SetDefaultOscillateTimespan(bool value)
        {
            _prePauseOscillateTimespan = value;
        }

        public void SetDefaultAutoShuffle(bool value)
        {
            _prePauseAutoShuffle = value;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (_paused) Unpause();
                else Pause();
            }
        }
    }
}