using Echo.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    sealed public class PhysicalEffect : Livable
    {
        #region Values
        [SerializeField] private CameraEffect _cameraEffect;
        
        // Determines if the effect directly affects its castor.
        [Space(15)]
        [SerializeField]
        private bool _directlyAffectCaster;

        [SerializeField] private bool _ignoreCaster;
        
        // Defines the radius of the effect
        [Space(15)]
        [SerializeField] private float _radius;


        [Serializable]
        sealed internal class ForceSettings
        {
            /// <summary>
            /// Determines the direction in-which force will be applied to the affected objects rigid body.
            /// </summary>
            [SerializeField] internal ForceDirection forceDirection;
            /// <summary>
            /// Defines a custom force direction, only used when custom (direction) is selected.
            /// </summary>
            [SerializeField] internal Vector3 customDirection;
            [Range(0, 1)]
            [SerializeField] internal float upwardConversionScale;

            /// <summary>
            /// Determines how to apply force to an affected object
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal ForceMode forceMode = ForceMode.Force;
            /// <summary>
            /// Determines if force application will be clamped if the affected objects has reached or is within range of the applied force.
            /// </summary>
            [SerializeField] internal bool clampForce;
            /// <summary>
            /// Determines if the volicity of the affected object will be striped previous to the new force application.
            /// </summary>
            [SerializeField] internal bool removeExistingVelocity;

            /// <summary>
            /// Defines the force added to the affected object.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal float force;

            /// <summary>
            /// Determines how force will be scaled based on the affected objects distance from the center of the effect with-in its radius.
            /// </summary>
            [SerializeField] internal AnimationCurve distanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
        }
        [Space(10)]
        [SerializeField]
        internal ForceSettings force;

        [Serializable]
        sealed internal class Damage
        {
            /// <summary>
            /// Defines the maximum damage applied to an object affected with the effect
            /// </summary>
            [SerializeField] internal float damage;
            /// <summary>
            /// Determines how damage will be scaled based on the affected objects distance from the center of the effect.
            /// </summary>
            [SerializeField] internal AnimationCurve distanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
        }
        [SerializeField] internal Damage damage;

        [Serializable]
        sealed internal class Stun
        {
            /// <summary>
            /// Defines the intensity of the stun applied to the affecties camera.
            /// </summary>
            [SerializeField] internal float intensity;
            /// <summary>
            /// Defines the duration of the stun.
            /// </summary>
            [SerializeField] internal float duration;
            /// <summary>
            /// Determines how screen shake intensity and duration will be scaled based on the affected objects distance from the center of the effect.
            /// </summary>
            [SerializeField] internal AnimationCurve distanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
        }
        [SerializeField] internal Stun stun;

        [Serializable]
        sealed internal class Vibration
        {
            /// <summary>
            /// Defines the intensity of the vibration applied to the affecties input device.
            /// </summary>
            [SerializeField] internal float intensity;
            /// <summary>
            /// Defines the duration of the vibration.
            /// </summary>
            [SerializeField] internal float duration;
            /// <summary>
            /// Determines how vibration intensity and duration will be scaled based on the affected objects distance from the center of the effect.
            /// </summary>
            [SerializeField] internal AnimationCurve distanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
        }
        [SerializeField] internal Vibration vibration;
        
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (_lifespan > 0)
                Emit();
        }

        private void OnDrawGizmos()
        {
            Color color = Color.red;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(this.transform.position, _radius);

            Gizmos.DrawWireSphere(this.transform.position, 0.5f);
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(this.transform.position, 0.5f);
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            Globals.singleton.Contain(this);
            SoftStartLifespan();
            Cast();
        }

        internal override void Cast()
        {
          //  Development.Debug.ShowTimedSphereGizmo(this.transform.position, _radius, Color.red, 3);
            Emit();
        }

        private void Emit()
        {
            if (_directlyAffectCaster)
            {
                if (_caster != null)
                    Affect(_caster.gameObject, this.transform);
            }
            else
            {
                // Find and apply to all objects in radius
                Collider[] objectsInRange = Physics.OverlapSphere(this.transform.position, _radius);
                List<GameObject> previouslyAffectedGameObjects = new List<GameObject>();
                foreach (Collider objectInRange in objectsInRange)
                {
                    if (!previouslyAffectedGameObjects.Contains(objectInRange.gameObject))
                    {
                        // Avoid duplicate applications to objects with mutiple colliders
                        previouslyAffectedGameObjects.Add(objectInRange.gameObject);

                        // Apply
                        Affect(objectInRange.gameObject, this.transform);
                    }
                }
            }
        }

        internal void Affect(GameObject gameObject)
        {
            Affect(gameObject, gameObject.transform);
        }
        internal void Affect(GameObject gameObject, Transform effectTransform)
        {
            if (_directlyAffectCaster && _caster != null && gameObject != _caster.gameObject || _ignoreCaster && _caster != null && gameObject == _caster.gameObject)
                return;
            // Determine affected object
            ObjectProperties objectProperties = gameObject.GetComponent<ObjectProperties>();

            if (objectProperties != null)
            {
                // Define square maginitude
                float sqrMagnitude = (effectTransform.position - gameObject.transform.position).sqrMagnitude;
                float radius = _radius * _radius;

                Biped biped = gameObject.GetComponent<Biped>();
                if (biped != null)
                {
                    ApplyVibration(biped, sqrMagnitude, radius, vibration);
                    ApplyCameraEffect(biped, _cameraEffect);
                }

                ApplyForce(objectProperties, effectTransform, sqrMagnitude, radius, force);
                ApplyDamage(objectProperties, _caster, effectTransform.position, sqrMagnitude, radius, damage);
            }
        }

        internal static void ApplyCameraEffect(Biped biped, CameraEffect cameraEffect)
        {
            if (cameraEffect != null && biped.cameraUnion != null)
                biped.cameraUnion.cameraController.cameraEffector.AddEffect(cameraEffect);
        }
        internal static void ApplyVibration(Biped biped, float distance, float radius, Vibration vibrationSettings)
        {
            // Applies vibration to a local players controller.

            if (biped.cameraUnion == null || vibrationSettings.intensity == 0)
                return;
            // Determine scale
            
            float scale = vibrationSettings.distanceFalloff.EvaluateByDistance(distance, radius);

            float intensity = vibrationSettings.intensity * scale;
            float duration = vibrationSettings.duration * scale;

            // biped.Player.AddVibration(intensity, duration);
        }
        internal static void ApplyDamage(ObjectProperties objectProperties, Biped caster, Vector3 position, float distance, float radius, Damage damageSettings)
        {
            // Applies damage to an object.

            if (damageSettings.damage == 0)
                return;


            // If the object is an object, scale damage based on distance and apply
            float scale = damageSettings.distanceFalloff.EvaluateByDistance(distance, radius);
            // Scale damage
            float damage = damageSettings.damage * scale;

            if (damage != 0)
                objectProperties.Damage(damage, position, caster);
        }
        internal static void ApplyForce(ObjectProperties objectProperties, Transform effectTransform, float distance, float radius, ForceSettings forceSettings)
        {
            // Determine direciton
            Vector3 forceDirection = Vector3.zero;
            switch (forceSettings.forceDirection)
            {
                case ForceDirection.Omni:
                    forceDirection = (objectProperties.transform.position - effectTransform.transform.position).normalized;
                    break;
                case ForceDirection.LocalUp:
                    forceDirection = effectTransform.up;
                    break;
                case ForceDirection.LocalForward:
                    forceDirection = effectTransform.forward;
                    break;
                case ForceDirection.LocalRight:
                    forceDirection = effectTransform.right;
                    break;
                case ForceDirection.Up:
                    forceDirection = Vector3.up;
                    break;
                case ForceDirection.Forward:
                    forceDirection = Vector3.forward;
                    break;
                case ForceDirection.Right:
                    forceDirection = Vector3.right;
                    break;
                case ForceDirection.Custom:
                    forceDirection = forceSettings.customDirection;
                    break;
            }

            forceDirection = Vector3.Lerp(forceDirection, Vector3.up, forceSettings.upwardConversionScale);
            // Scale force
            float scale = forceSettings.distanceFalloff.EvaluateByDistance(distance, radius);
            // Scale force
            float force = forceSettings.force * scale;

            if (forceSettings.clampForce)
            {
                // Clamp force, as to avoid exceeding the applied velocity
                float existingVelocity = Vector3.Dot(objectProperties.GetVelocity(), forceDirection);
                if (existingVelocity >= force)
                    return;
                else
                {
                    force = force - Mathf.Abs(existingVelocity);

                    if (forceSettings.force > 0 && force < 0 || forceSettings.force < 0 && force > 0)
                        force = 0;
                }
            }

            if (forceSettings.removeExistingVelocity)
                objectProperties.SetVelocity(Vector3.zero);


            objectProperties.AddForce(force * forceDirection, forceSettings.forceMode);

        }
        #endregion
    }
}