using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Echo.Management;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class CameraEffector : MonoBehaviour
    {
        #region Values
        /// <summary>
        /// Defines the base shader for the material applied to the cameras render texture.
        /// </summary>
        [SerializeField] private Shader _shader;
        [SerializeField] private Texture2D _glitchDistortionMap;
        [SerializeField] private Texture2D _glitchWaveMap;
        [SerializeField] private Texture2D _glitchStaticMap;

        /// Defines the base effect of the camera effect.
        private CameraEffect.Effect.Colors _baseEffect;
        // A collection of effects, to be combined and applied along with the base effect
        private List<CameraEffect.Effect> _effects = new List<CameraEffect.Effect>();

        /// <summary>
        /// Defines the material applied to the render texture of the camera.
        /// </summary>
        private Material _material;

        private Camera _camera;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateEffects();
        }

        private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
        {
            if (_material != null)
                Graphics.Blit(sourceTexture, destTexture, _material);
            else
                Graphics.Blit(sourceTexture, destTexture);
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            _material = new Material(_shader);
            _camera = this.gameObject.GetComponent<Camera>();
            SetBaseColors(Globals.singleton.baseCameraEffect.effect.colors);
            _material.SetTexture("_GlitchDistortionMap", _glitchDistortionMap);
            _material.SetTexture("_GlitchWaveMap", _glitchWaveMap);
            _material.SetTexture("_GlitchStaticMap", _glitchStaticMap);

            if (!SystemInfo.supportsImageEffects)
            {
                this.enabled = false;
                return;
            }
        }
        
        private void UpdateEffects()
        {
#if UNITY_EDITOR
            SetBaseColors(Globals.singleton.baseCameraEffect.effect.colors);
#endif

            if (_baseEffect == null)
                return;

            // Define base colors
            CameraEffect.Effect.Colors baseColors = new CameraEffect.Effect.Colors(_baseEffect);

            float intensity = 0;
            float shake = 0;

            for (int i = 0; i < _effects.Count; i++)
            {
                CameraEffect.Effect effectSettings = _effects[i];

                // Define scale
                float duration = effectSettings.properties.killTime - effectSettings.properties.startTime;
                float timeRemaining = effectSettings.properties.killTime - Time.time;

                float scale = duration == 0 ? 0 : timeRemaining / duration;

                float shakeScale = effectSettings.shake.intensityOverLifetime.Evaluate(1 - scale);
                intensity += effectSettings.shake.intensityMultiplier * shakeScale;
                float direction = 1;
                direction = direction.RandomDirection();
                shake += direction * intensity;

                float colorScale = effectSettings.colors.intensityOverLifetime.Evaluate(1 - scale) * effectSettings.colors.colorIntensity;
                // Combine effects into base colors
                CombineColors(effectSettings.colors, ref baseColors, colorScale);

                // Remove camera effect if it has become durated
                if (Time.time >= effectSettings.properties.killTime)
                {
                    i--;
                    _effects.Remove(effectSettings);
                }

            }
            ApplyColorEffects(baseColors);
            this.transform.eulerAngles += new Vector3(0, 0, shake);
            _camera.fieldOfView = _camera.fieldOfView + shake;

        }

        public void ClearAllEffects()
        {
            _effects.Clear();
        }
        internal void AddEffect(CameraEffect cameraEffect)
        {
            if (cameraEffect != null)
                AddEffect(cameraEffect.effect);
        }
        internal void AddEffect(CameraEffect.Effect effectSettings)
        {
            // Try to find an existing effect, and update its times
            for (int i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].uniqueId == effectSettings.uniqueId)
                {
                    _effects[i] = effectSettings;
                    return;
                }
            }

            // Create new instance of effect settings
            CameraEffect.Effect newEffectSettings = new CameraEffect.Effect(effectSettings);
            _effects.Add(newEffectSettings);
        }

        internal void SetBaseColors(CameraEffect.Effect.Colors colors)
        {
            _baseEffect = new CameraEffect.Effect.Colors(colors);
        }
        private void ApplyColorEffects(CameraEffect.Effect.Colors colorSettings)
        {
            _material.SetFloat("_Brightness", colorSettings.brightness);
            _material.SetFloat("_Saturation", colorSettings.saturation);
            _material.SetFloat("_Contrast", colorSettings.contrast);
            _material.SetColor("_LightenColor", colorSettings.lightenColor);
            _material.SetFloat("_RedLevel", colorSettings.redLevel);
            _material.SetFloat("_GreenLevel", colorSettings.greenLevel);
            _material.SetFloat("_BlueLevel", colorSettings.blueLevel);
            _material.SetFloat("_GlitchIntensity", colorSettings.glitchIntensity);
            _material.SetFloat("_Random", UnityEngine.Random.Range(-1.0f, 1.0f));
        }

        private void CombineColors(CameraEffect.Effect.Colors effectA, ref CameraEffect.Effect.Colors effectB, float scale)
        {
            // Add each color componenet of the effects together
            effectB.brightness = Mathf.Max(0, effectB.brightness + (effectA.brightness * scale));
            effectB.saturation = Mathf.Max(0, effectB.saturation + (effectA.saturation * scale));
            effectB.contrast = Mathf.Max(0, effectB.contrast + (effectA.contrast * scale));

            effectB.redLevel = Mathf.Max(0, effectB.redLevel + (effectA.redLevel * scale));
            effectB.greenLevel = Mathf.Max(0, effectB.greenLevel + (effectA.greenLevel * scale));
            effectB.blueLevel = Mathf.Max(0, effectB.blueLevel + (effectA.blueLevel * scale));

            effectB.glitchIntensity = Mathf.Clamp(effectB.glitchIntensity + (effectA.glitchIntensity * scale), 0, 1);

            effectB.lightenColor = Color.Lerp(effectB.lightenColor, effectA.lightenColor, scale);
        }
        #endregion
    }
}