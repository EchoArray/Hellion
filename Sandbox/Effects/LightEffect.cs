using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    [RequireComponent(typeof(Light))]
    sealed public class LightEffect : Livable
    {
        #region Value
        // Defines the time in-which the object awoke.
        private float _awakeTime;
        // Defines the time in-which the object is to be killed.
        private float _killTime;

        // Defines the multiplier applied to the lifetime range.
        [Space(10)]
        [SerializeField]
        private float _rangeMultiplier = 1;
        // Defines the range of the light over its life time.
        [SerializeField] private AnimationCurve _rangeOverLifetime = AnimationCurve.Linear(0, 1, 1, 1);

        // Defines the multiplier applied to the lifetime intensity.
        [Space(10)]
        [SerializeField]
        private float _intensityMultiplier = 1;
        // Defines the intensity of the light over its life time.
        [SerializeField] private AnimationCurve _intensityOverLifetime = AnimationCurve.Linear(0, 1, 1, 1);

        // Defines the color of the light over its life time.
        [Space(10)]
        [SerializeField]
        private Gradient _colorOverLifetime;
        // Defines the light componenent of the game object associated with the light effect.
        private Light _light;
        #endregion
        
        #region Unity Functions
        protected override void Awake()
        {
            base.Awake();
            _light = this.gameObject.GetComponent<Light>();
            _awakeTime = Time.time;
            _killTime = _awakeTime + _lifespan;
        }

        private void Update()
        {
            UpdateLightEffect();
        }
        #endregion

        #region Functions
        private void UpdateLightEffect()
        {
            float timeRemaining = _killTime - Time.time;
            float scale = 1 - (timeRemaining / _lifespan);
            UpdateRange(scale);
            UpdateIntensity(scale);
            UpdateColor(scale);
            if (_livesForever && scale >= 1f)
                _killTime = Time.time + _lifespan;

        }

        private void UpdateRange(float scale)
        {
            _light.range = _rangeOverLifetime.Evaluate(scale) * _rangeMultiplier;
        }
        private void UpdateColor(float scale)
        {
            _light.color = _colorOverLifetime.Evaluate(scale);
        }
        private void UpdateIntensity(float scale)
        {
            _light.intensity = _intensityOverLifetime.Evaluate(scale) * _intensityMultiplier;
        } 
        #endregion
    }
}
