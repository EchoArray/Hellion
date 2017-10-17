using Echo.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class ObjectProperties : Tumblable
    {
        #region Values
        [Space(10)]
        [SerializeField] private int _maxContactsPerImpact = 1;


        [Space(10)]
        [SerializeField] float _fallPhysicalEffectVelocity = 10f;
        [SerializeField] private PhysicalEffect _fallPhysicalEffect;

        // Determines if the object is allowed to die or not.
        [Space(15)]
        [SerializeField] private bool _invulnerable = true;
        [SerializeField] private float _healthRegenerationRate;
        // Defines the duration in-which the health will wait before it regenerates.
        [SerializeField] private float _healthRegenerationDelay;
        // Defines the time in-which the health will begin to regenerate.
        private float _healthRegenerationTime;
        // Defines the maximum amount of health the object can have.
        [SerializeField] private float _maxHealth;
        // Defines the active amount of health for the object.
        [SerializeField] private float _health;
        // Defines the effect instantiated upon the death of the object.
        [SerializeField] private Castable _deathEffect;
        [SerializeField] private bool _instantlyDestroyOnDeath;
        [SerializeField] private float _deathDelay;

        /// <summary>
        /// Determines if the health of the object is full.
        /// </summary>
        internal bool HealthFull { get { return _health == _maxHealth; } }
        /// <summary>
        /// Determines is the health of the object is empty.
        /// </summary>
        internal bool HealthEmpty { get { return _health == 0; } }

        // Defines the rate in-which the shield will regenerate.
        [Space(10)]
        [SerializeField] private float _shieldRegenerationRate;
        // Defines the duration in-which the shield will wait before it regenerates.
        [SerializeField] private float _shieldRegenerationDelay;
        // Defines the time in-which the shield will begin to regenerate.
        private float _shieldRegenerationTime;
        // Defines the maximum amount of shield the object can have.
        [SerializeField] private float _maxShield;
        // Defines the active amount of shield for the object.
        private float _shield;
        // Defines the effect instantiated upon the full shield depletion of the object.
        [SerializeField] private Castable _shieldBreakEffect;
        /// <summary>
        /// Determines if the shield of the object is full.
        /// </summary>
        internal bool ShieldFull { get { return _shield == _maxShield; } }
        /// <summary>
        /// Determines if the shield of the object is empty.
        /// </summary>
        internal bool ShieldEmpty { get { return _shield == 0; } }

        private Transform _startingParent;
        [Serializable]
        sealed internal class DamageSection
        {
            [SerializeField] internal string name;
            [SerializeField] internal Bounds bounds;
            [SerializeField] internal MeshFilter meshFilter;
            [SerializeField] internal MeshRenderer meshRenderer;
            [Range(0, 1)]
            [SerializeField] internal float damageScale = 1;
            [SerializeField] internal float health;
            internal bool destroyed;

            [Serializable]
            sealed internal class CosmeticState
            {
                [SerializeField] internal string name;
                internal float health;
                internal bool destroyed;
                internal bool active;

                [SerializeField] internal Castable activeEffect;

                [Space(10)]
                [SerializeField] internal Mesh mesh;
                [Space(10)]
                /// <summary>
                /// Defines the materials of the renderer associated with the object properties game object when the state is active.
                /// </summary>
                [SerializeField] internal Material[] materials;
            }
            [SerializeField] internal CosmeticState[] cosmeticStates;
        }
        [Space(10)]
        [SerializeField] private DamageSection[] _damageSections;
        

        // Defines a renderer component of the game object assoaciated with the object.
        private Renderer _renderer;
        // Defines a mesh filter component of the game object assoaciated with the object.
        private MeshFilter _meshFilter;
        #endregion

        #region Unity Functions
        private void Start()
        {
            _startingParent = this.transform.parent;
        }
        private void Awake()
        {
            Initialize();
        }
        private void Update()
        {
            if (_rigidbody != null)
            {
                Development.Debug.ShowLabel(this, "velocity", _rigidbody.velocity.ToString());
                Development.Debug.ShowLabel(this, "velocity_magnitude", _rigidbody.velocity.magnitude.ToString());
            }
        }
        private void OnDrawGizmos()
        {
            if (_damageSections == null)
                return;
            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            foreach (DamageSection damageSection in _damageSections)
            {
                Color color = Color.magenta;
                Gizmos.color = color;
                Gizmos.DrawWireCube(damageSection.bounds.center, damageSection.bounds.extents * 2);
                color.a = 0.4f;
                Gizmos.color = color;
                Gizmos.DrawCube(damageSection.bounds.center, damageSection.bounds.extents * 2);
            }

            Gizmos.matrix = matrixBackup;
        }

        protected override void OnCollisionEnter(Collision collision)
        {           
            _colliding = true;
            _currentCollision = collision;
            ContactRange(collision, _maxContactsPerImpact);
            _relativeImpactVelocity = collision.relativeVelocity;

            if (collision.gameObject.layer == Globals.MACHINE_LAYER)
                this.transform.SetParent(collision.gameObject.transform);
            else
                this.transform.SetParent(_startingParent);
        }

        protected override void OnCollisionExit(Collision collision)
        {
            base.OnCollisionExit(collision);
        }

        #endregion

        #region Functions
        private void Initialize()
        {
            Globals.singleton.Contain(this);

            _rigidbody = this.GetComponent<Rigidbody>();
            _renderer = this.gameObject.GetComponent<Renderer>();
            _meshFilter = this.gameObject.GetComponent<MeshFilter>();
            _rigidbody.maxDepenetrationVelocity = 0;
            _health = _maxHealth;
            _shield = _maxShield;
            InitializeDamageSections();
        }

        
        protected override void ContactRange(Collision collision, int range)
        {
            base.ContactRange(collision, range);

            if(_fallPhysicalEffect != null && collision.relativeVelocity.magnitude >= _fallPhysicalEffectVelocity)
                _fallPhysicalEffect.Affect(this.gameObject, this.transform);
        }

        #region Damage
        private void UpdateRegeneration()
        {
            if (_shieldRegenerationTime <= Time.time && !ShieldFull)
                _shield = Mathf.Min(_shield + (Time.deltaTime * _shieldRegenerationRate), _maxShield);
            if (_healthRegenerationTime <= Time.time || !HealthFull)
                _health = Mathf.Min(_health + (Time.deltaTime * _healthRegenerationRate), _maxHealth);
        }

        internal void Damage(float value, Vector3 position, Biped responsibleCaster = null)
        {
            float remainingDamage = DamageShield(value, position, responsibleCaster);
            DamageHealth(remainingDamage, position, responsibleCaster);
        }

        private void BreakShield()
        {
            if (_shieldBreakEffect != null)
                Instantiate(_deathEffect, this.transform.position, this.transform.rotation);
        }
        internal float DamageShield(float value, Vector3 position, Biped responsibleCaster = null)
        {
            _shieldRegenerationTime = Time.time + _shieldRegenerationDelay;
            if (ShieldEmpty)
                return value;

            if (_shield >= value)
                _shield -= value;
            else if (!ShieldEmpty)
            {
                _shield = 0;
                BreakShield();
                return value - _shield;
            }
            return 0;
        }

        public void Die(Biped responsibleCaster = null)
        {
            StartCoroutine(StartDeath(responsibleCaster));
        }
        internal IEnumerator StartDeath(Biped responsibleCaster)
        {
            yield return new WaitForSeconds(_deathDelay);

            if (_deathEffect != null)
                Instantiate(_deathEffect, this.transform.position, this.transform.rotation);

            Projectile projectile = this.gameObject.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Detonate();

            if (_instantlyDestroyOnDeath)
                Destroy(this.gameObject);
            else
            {
                FullyDamageAllSections();
                DisposalManager.Add(this.gameObject);
            }
        }
        internal void DamageHealth(float value, Vector3 position, Biped responsibleCaster = null)
        {
            if (_damageSections.Length != 0)
            {
                DamageSection damageSection = FindClosestDamageSection(position);
                value *= ApplyDamageToDamageSection(damageSection, value);
            }

            if (HealthEmpty)
                return;

            _health = Mathf.Max(_health - value, 0);
            _healthRegenerationTime = Time.time + _healthRegenerationDelay;

            if (HealthEmpty && !_invulnerable)
                Die(responsibleCaster);
        }

        private void InitializeDamageSections()
        {
            foreach (DamageSection damageSection in _damageSections)
            {
                foreach (DamageSection.CosmeticState cosmenticState in damageSection.cosmeticStates)
                    cosmenticState.health = damageSection.health;
            }
        }
        private void FullyDamageAllSections()
        {
            foreach (DamageSection damageSection in _damageSections)
            {
                if (damageSection.cosmeticStates.Length == 0)
                    continue;

                // Loop with destroyed bias to ensure all effects are created
                foreach (DamageSection.CosmeticState cosmeticState in damageSection.cosmeticStates)
                {
                    if (!cosmeticState.destroyed)
                    {
                        cosmeticState.destroyed = true;
                        ApplyCosmeticState(damageSection, cosmeticState);
                    }
                }
            }
        }
        private DamageSection FindClosestDamageSection(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);

            float closestDistance = Mathf.Infinity;
            DamageSection closestDamageSection = null;
            foreach (DamageSection damageSection in _damageSections)
            {
                if (damageSection.destroyed)
                    continue;

                if (damageSection.bounds.Contains(localPosition))
                {
                    closestDamageSection = damageSection;
                    break;
                }
                float sqrMagnitude = (damageSection.bounds.center - localPosition).sqrMagnitude;
                float radius = closestDistance * closestDistance;
                if (sqrMagnitude <= radius)
                {
                    closestDistance = sqrMagnitude;
                    closestDamageSection = damageSection;
                }
            }
            return closestDamageSection;
        }
        private float ApplyDamageToDamageSection(DamageSection damageSection, float value)
        {
            if (damageSection == null)
                return 1;

            for (int i = 0; i < damageSection.cosmeticStates.Length; i++)
            {
                if (value == 0)
                    return 1;

                DamageSection.CosmeticState cosmeticState = damageSection.cosmeticStates[i];
                if (cosmeticState.destroyed)
                    continue;
                if (i == damageSection.cosmeticStates.Length - 1 && cosmeticState.destroyed)
                    damageSection.destroyed = true;

                value = DamageCosmeticState(damageSection, cosmeticState, value);
            }
            return damageSection.damageScale;
        }
        private void ApplyCosmeticState(DamageSection damageSection, DamageSection.CosmeticState cosmeticState)
        {
            if (damageSection == null || cosmeticState == null)
                return;
            damageSection.meshFilter.mesh = cosmeticState.mesh;
            damageSection.meshRenderer.materials = cosmeticState.materials;
            if (cosmeticState.activeEffect != null)
            {
                cosmeticState.activeEffect.SetCaster(_caster);
                Instantiate(cosmeticState.activeEffect, this.transform.TransformPoint(damageSection.bounds.center), Quaternion.identity);
            }
        }
        private float DamageCosmeticState(DamageSection damageSection, DamageSection.CosmeticState cosmeticState, float value)
        {
            if (cosmeticState.health == value)
            {
                cosmeticState.destroyed = true;
                cosmeticState.health = 0;
                return 1;
            }
            else if (cosmeticState.health > value)
            {
                cosmeticState.health -= value;
                if (cosmeticState.active)
                    return 0;
                cosmeticState.active = true;
                ApplyCosmeticState(damageSection, cosmeticState);
                return 0;
            }
            else
            {
                value -= cosmeticState.health;
                cosmeticState.destroyed = true;
                return value;
            }
            return value;
        }
        #endregion

        #region Velocity
        internal Vector3 GetVelocity()
        {
            return _rigidbody == null ? Vector3.zero : _rigidbody.velocity;
        }
        internal void SetVelocity(Vector3 velocity)
        {
            if (_rigidbody != null)
                _rigidbody.velocity = velocity;
        }
        internal void AddForce(Vector3 force, ForceMode forceMode)
        {
            if (_rigidbody != null)
                _rigidbody.AddForce(force, forceMode);
        }
        #endregion
        #endregion
    }
}