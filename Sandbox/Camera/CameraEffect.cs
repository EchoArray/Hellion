using UnityEngine;
using System.Collections;
using System;

namespace Echo
{
    sealed public class CameraEffect : Livable
    {
        #region Values
        [Serializable]
        sealed internal class Effect
        {
            /// <summary>
            /// Defines the unique identifier of the effect.
            /// </summary>
            internal int uniqueId;


            [Serializable]
            sealed internal class Properties
            {
                /// <summary>
                /// Defines the radius of the effect.
                /// </summary>
                [SerializeField] internal float radius;
                /// <summary>
                /// Defines the duration of the camera effect.
                /// </summary>
                [SerializeField] internal float duration;
                /// <summary>
                /// Defines the time in-which the effect started.
                /// </summary>
                internal float startTime;
                /// <summary>
                /// Defines the time in-which the effect will end.
                /// </summary>
                internal float killTime;

                internal Properties() { }
                internal Properties(Properties properties)
                {
                    radius = properties.radius;
                    duration = properties.duration;
                    startTime = Time.time;
                    killTime = Time.time + duration;
                }
            }
            [SerializeField] internal Properties properties;

            [Serializable]
            sealed internal class Colors
            {
                /// <summary>
                /// Defines the brightness for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float brightness = 0f;
                /// <summary>
                /// Defines the saturation for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float saturation = 0f;
                /// <summary>
                /// Defines the contrast for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float contrast = 0f;

                /// <summary>
                /// Defines the red levels for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float redLevel;
                /// <summary>
                /// Defines the green levels for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float greenLevel;
                /// <summary>
                /// Defines the blue levels for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float blueLevel;


                /// <summary>
                /// Defines the lighten color for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal Color lightenColor;

                /// <summary>
                /// Defines the intensity of the glitch effect for the shader applied to the render texture for the camera.
                /// </summary>
                [SerializeField] internal float glitchIntensity;
                [SerializeField] internal float colorIntensity = 1;

                [SerializeField] internal AnimationCurve intensityOverLifetime = AnimationCurve.Linear(0, 1, 1, 0);

                public Colors() { }
                public Colors(Colors colors)
                {
                    brightness = colors.brightness;
                    saturation = colors.saturation;
                    contrast = colors.contrast;
                    lightenColor = colors.lightenColor;

                    redLevel = colors.redLevel;
                    greenLevel = colors.greenLevel;
                    blueLevel = colors.blueLevel;
                    colorIntensity = colors.colorIntensity;


                    glitchIntensity = colors.glitchIntensity;
                    intensityOverLifetime = colors.intensityOverLifetime;
                }
            }
            [SerializeField] internal Colors colors;

            [Serializable]
            sealed internal class Shake
            {
                /// <summary>
                /// Defines the intensity of the shake.
                /// </summary>
                [SerializeField] internal float intensityMultiplier;
                /// <summary>
                /// Determines the intensity of the shake along its duration.
                /// </summary>
                [SerializeField] internal AnimationCurve intensityOverLifetime = AnimationCurve.Linear(0, 1, 1, 0);
                public Shake() { }
                public Shake(Shake shake)
                {
                    intensityMultiplier = shake.intensityMultiplier;
                    intensityOverLifetime = shake.intensityOverLifetime;
                }
            }
            [SerializeField] internal Shake shake;

            internal Effect() { }
            internal Effect(Effect effect)
            {
                uniqueId = effect.uniqueId;
                properties =  new Properties(effect.properties);
                colors = new Colors(effect.colors);
                shake = new Shake(effect.shake);
            }
            public static Effect operator *(Effect effect, float scale)
            {
                Effect newEffect = new Effect(effect);
                newEffect.colors.glitchIntensity *= scale;
                newEffect.colors.colorIntensity *= scale;
                newEffect.shake.intensityMultiplier *= scale;
                return newEffect;
            }
        }
        [SerializeField] internal Effect effect;
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            Cast();
        }

        private void Update()
        {
            Emit();
        }
        private void OnDrawGizmos()
        {
            Color color = Color.cyan;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(this.transform.position, effect.properties.radius);
            Gizmos.DrawWireSphere(this.transform.position, 0.5f);
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(this.transform.position, 0.5f);
        }
        #endregion

        #region Functions
        internal override void Cast()
        {
            Emit();
        }
        public override void SetUnique(int id)
        {
            effect.uniqueId = id;
        }

        private void Emit()
        {
            CameraEffector[] cameraEffectors = FindObjectsOfType<CameraEffector>();
            foreach (CameraEffector cameraEffector in cameraEffectors)
            {
                float sqrMagnitude = (this.transform.position - cameraEffector.transform.position).sqrMagnitude;
                float radius = effect.properties.radius * effect.properties.radius;
                float scale = 1 - (sqrMagnitude / radius);
                if (sqrMagnitude <= radius)
                    ApplyTo(cameraEffector, scale);
            }
            SoftStartLifespan();
        }
        internal void ApplyTo(CameraEffector cameraEffector, float scale)
        {
            Effect newEffect = effect * scale;
            cameraEffector.AddEffect(newEffect);
        }
        #endregion
    }
}