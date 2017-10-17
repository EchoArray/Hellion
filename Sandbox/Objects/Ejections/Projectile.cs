using Echo.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class Projectile : Tumblable
    {
        #region Values
        // Determines if the projectile is to die upon impact.
        [Space(10)]
        [SerializeField] protected bool _dieOnImpact;
        // Determines if the projectile lives forever.
        [SerializeField] protected bool _livesForever;

        // Defines the duration in-which the projectile is allowed to exist.
        [SerializeField] protected float _lifespan = 1f;

        // Determines if the projectile uses detonation.
        [Space(15)]
        [SerializeField] private bool _useDetonation;
        // Defines the duration in-which the projectile will wait to detonation post impact.
        [SerializeField] private float _detonationDelay;
        // Determines the time in-which the projectile will detonation.
        private float _detonationTime;
        // Determines if the projectile has detonatated.
        private bool _detonated;

        [SerializeField] private Castable _detonationCastable;
        // Defines the surface response, used to determine what effect to spawn for the hit surface.
        [SerializeField] private SurfaceResponse _detonationSurfaceResponse;


        // Defines the target of the projectile.
        internal Transform target;
        // Defines the rate in-which the projectile rotates toward its target.
        [Space(15)]
        [SerializeField]
        private float _targetingRate;
        // Defines the rate at which the projectile will rotate toward its velocity
        [Space(10)]
        [SerializeField] private float _rotateTowardVelocityRate;

        // Determines when velocity is applied to the projectile.
        [Space(15)]
        [SerializeField]
        private VelocityType _velocityType;
        // Defines the velocity of the projectile.
        [SerializeField] private Vector2 _velocityRange;
        private float _velocity;
        // Determines if the projectile uses gravity.
        [SerializeField] private bool _useGravity;


        // Determines if collision is updated on fixed update.
        [Space(15)]
        [SerializeField]
        private bool _useFixedUpdateCastCollision = true;
        private enum CollisionType
        {
            Cast,
            CapsuleCollider,
        }
        [SerializeField] private CollisionType _collisionType;
        // Defines half of the length of the projectiles collision.
        [SerializeField] private float _collisionLength;
        // Defines the offset of the starting position of the projectiles.
        [SerializeField] private float _collisionForwardOffset;
        // Defines the radius of the projectiles collision.
        [SerializeField] private float _collisionRadius;

        // Defines the capsule collider component of the game object associated with the projectile.
        private CapsuleCollider _capsuleCollider;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateVelocity();
            UpdateRotation();
            UpdateDetonation();

            if (!_useFixedUpdateCastCollision)
                UpdateCastCollision();
        }

        private void FixedUpdate()
        {
            if (_useFixedUpdateCastCollision)
                UpdateCastCollision();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_useDetonation)
                Detonate();
        }
        #endregion
        
        #region Functions
        private void Initialize()
        {
            _velocity = Random.Range(_velocityRange.x, _velocityRange.y);

            Globals.singleton.Contain(this);
            this.gameObject.layer = Globals.singleton.GetLayerOfType(this);

            SetCapsuleCollider();
            SetRigidbody();
            ApplyVelocity();
            StartLifespan();
        }
        
        private void StartLifespan()
        {
            Destroy(this.gameObject, _lifespan);
        }

        internal void Detonate()
        {
            if (_detonated || !_useDetonation)
                return;
            _detonated = true;

            bool showBaseEffect = true;
            if (_detonationSurfaceResponse != null && _currentCollision != null)
            {
                RaycastHit raycastHit = RaycastContact(_currentCollision.contacts[0]);
                SurfaceResponse.ResponseEffect responseEffect = GetResponseEffect(raycastHit, _detonationSurfaceResponse);
                if (responseEffect != null)
                {
                    showBaseEffect = !responseEffect.disableBaseEffect;
                    CreateEffect(responseEffect.castable, raycastHit);
                }
            }

            if (showBaseEffect && _detonationCastable != null)
            {
                _detonationCastable.SetCaster(_caster);
                CreateEffect(_detonationCastable, this.transform.position, this.transform.rotation);
            }

            Destroy(this.gameObject);
        }
        private void UpdateDetonation()
        {
            if (_useDetonation && _detonationTime != 0 && Time.time >= _detonationTime)
                Detonate();
        }

        private void UpdateRotation()
        {
            // Rotate the projectile toward its velocity
            if (_rotateTowardVelocityRate > 0 && _rigidbody.velocity.magnitude > 1f)
                _rigidbody.MoveRotation(Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(_rigidbody.velocity.normalized), Time.deltaTime * _rotateTowardVelocityRate));

            if (target != null)
            {
                Quaternion targetingDestination = Quaternion.LookRotation(target.position - this.transform.position);
                _rigidbody.MoveRotation(Quaternion.RotateTowards(this.transform.rotation, targetingDestination, _targetingRate * Time.deltaTime));
            }
        }

        private void ApplyVelocity()
        {
            _rigidbody.velocity = this.transform.forward * _velocity;
        }
        private void UpdateVelocity()
        {
            // If constant velocity is active, always add velocity
            if (_velocityType == VelocityType.Constant)
                ApplyVelocity();
        }
        private void UpdateCastCollision()
        {
            if (_collisionType == CollisionType.CapsuleCollider || _collisionLength <= 0)
                return;

            // Cast forward to determine if the projectile has hit
            Vector3 castPosition = this.transform.position + (this.transform.forward * _collisionForwardOffset);

            RaycastHit raycastHit;
            bool hit = Physics.Raycast(castPosition, this.transform.forward, out raycastHit, _collisionLength);
            if (!hit && _collisionRadius > 0)
            {
                castPosition = this.transform.position + (this.transform.forward * (_collisionForwardOffset - _collisionRadius));
                hit = Physics.SphereCast(castPosition, _collisionRadius, this.transform.forward, out raycastHit, _collisionLength);
            }

            if (hit)
                Impact(raycastHit);
        }

        private void SetRigidbody()
        {
            // Define rigid body and its defaults
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
            if (_rigidbody == null)
                _rigidbody = this.gameObject.AddComponent<Rigidbody>();
            _rigidbody.useGravity = _useGravity;
            _rigidbody.mass = 0;
            _rigidbody.drag = 0.5f;
            _rigidbody.angularDrag = 1f;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        private void SetCapsuleCollider()
        {
            if (_collisionType != CollisionType.CapsuleCollider)
                return;
            _capsuleCollider = this.gameObject.GetComponent<CapsuleCollider>();
            if (_capsuleCollider == null)
                _capsuleCollider = this.gameObject.AddComponent<CapsuleCollider>();

            _capsuleCollider.radius = _collisionRadius;
            _capsuleCollider.height = _collisionLength;
            _capsuleCollider.direction = 2;
            _capsuleCollider.center = new Vector3(0, 0, _collisionForwardOffset);
        }
        
        protected override void Contact(ContactPoint contactPoint)
        {
            base.Contact(contactPoint);
            if (_detonationTime == 0)
                _detonationTime = Time.time + _detonationDelay;
        }
        protected override void Impact(RaycastHit raycastHit)
        {
            base.Impact(raycastHit);
            if (_dieOnImpact)
                Destroy(this.gameObject);
        }
        #endregion
    }
}