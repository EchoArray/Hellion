using UnityEngine;

namespace Echo
{
    public abstract class Impactable : Castable
    {
        #region Values
        // Determines if the impactable is disallowed from sticking.
        protected bool _disallowSticking;
        // Defines the stick node of the impactable.
        protected Transform _stickNode;
        // Determines if the impactable is stuck to a rigid object.
        protected bool _stuckToRigid;
        // Determines if the impactable has stuck.
        protected bool _stuck;

        // Defines the currenct collision of the impactable.
        protected Collision _currentCollision;
        // Determines if the impactable is currently colliding.
        protected bool _colliding;

        // Defines the effect instatiated upon impacting a generic object.
        [Space(10)]
        [SerializeField] protected Castable _impactBase;
        // Defines the surface response, used to determine what effect to spawn for the hit surface.
        [SerializeField] protected SurfaceResponse _impactSurfaceResponse;

        // Defines the relative impact velocity of the most recent enter collision.
        protected Vector3 _relativeImpactVelocity;
        // Defines the rigidbody component of the game object associated with the impactable.
        protected Rigidbody _rigidbody;
        #endregion

        #region Unity Functions
        protected virtual void LateUpdate()
        {
            UpdateStick();
        }

        protected virtual void OnDestroy()
        {
            if (_stickNode != null)
                Destroy(_stickNode.gameObject);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            _currentCollision = collision;
            _colliding = true;
            _relativeImpactVelocity = collision.relativeVelocity;
            Contact(collision.contacts[0]);
        }
        protected virtual void OnCollisionStay(Collision collision)
        {
            _currentCollision = collision;
            _colliding = true;
        }
        protected virtual void OnCollisionExit(Collision collision)
        {
            if (!_stuck)
            {
                _currentCollision = null;
                _colliding = false;
            }
        }
        #endregion

        #region Functions
        private void UpdateStick()
        {
            if (!_stuck)
                return;

            if (_stickNode == null && _stuckToRigid)
                UnStick();
            if (_stickNode != null)
            {
                this.transform.position = _stickNode.transform.position;
                this.transform.rotation = _stickNode.transform.rotation;
            }

        }
        private void StickTo(GameObject gameObject)
        {
            if (_disallowSticking)
                return;

            if (IsRigid(gameObject))
            {
                if (IsObject(this.gameObject) && IsProjectile(gameObject))
                    return;

                _stuckToRigid = true;
                SetAllCollidersState(false);

                _stickNode = new GameObject().transform;
                _stickNode.name = this.gameObject.name + "_stick_node";
                _stickNode.transform.position = this.transform.position;
                _stickNode.transform.rotation = this.transform.rotation;
                _stickNode.transform.SetParent(gameObject.transform);
            }
            _rigidbody.isKinematic = true;
            _stuck = true;
        }
        private void UnStick()
        {
            if (_stickNode != null)
                Destroy(_stickNode.gameObject);

            _rigidbody.isKinematic = false;
            SetAllCollidersState(true);
            _stuck = false;
            _stuckToRigid = false;
        }

        private bool IsRigid(GameObject gameObject)
        {
            return gameObject.GetComponent<Rigidbody>() != null;
        }
        private bool IsObject(GameObject gameObject)
        {
            return gameObject.GetComponent<ObjectProperties>() != null;
        }
        private bool IsProjectile(GameObject gameObject)
        {
            return gameObject.GetComponent<Projectile>() != null;
        }

        private void SetAllCollidersState(bool state)
        {
            Collider[] colliders = this.gameObject.GetComponents<Collider>();
            foreach (Collider collider in colliders)
                collider.enabled = state;
        }

        protected virtual void Impact(RaycastHit raycastHit)
        {
            if (_stuck)
                return;

            // Determine if there is a collider to the raycast
            if (raycastHit.collider == null)
                return;
            // Define lightmap color
            Color lightmapColor = LightmapHelper.GetColor(raycastHit);

            // Create effects
            SurfaceResponse.ResponseEffect responseEffect = GetResponseEffect(raycastHit, _impactSurfaceResponse);

            //Impactable impactable = raycastHit.collider.GetComponent<Impactable>();
            //if(impactable != null)
            //    impactable.SetCaster(_caster);

            bool showBase = true;
            // Apply physics dynamics
            if (responseEffect != null)
            {
                CreateEffect(responseEffect.castable, raycastHit);
                ApplyResponseDynamics(responseEffect, raycastHit);
                showBase = !responseEffect.disableBaseEffect;
            }
            if (showBase)
                CreateEffect(_impactBase, raycastHit);
        }
        protected virtual void Contact(ContactPoint contactPoint)
        {
            RaycastHit raycastHit = RaycastContact(contactPoint);
            Impact(raycastHit);
        }
        protected virtual void ContactRange(Collision collision, int range)
        {
            int length = Mathf.Min(collision.contacts.Length, range);
            for (int i = 0; i < length; i++)
                Contact(collision.contacts[i]);
        }
        protected RaycastHit RaycastContact(ContactPoint contactPoint)
        {
            Ray ray = new Ray(this.transform.position, contactPoint.point - this.transform.position);
            RaycastHit raycastHit;
            contactPoint.otherCollider.Raycast(ray, out raycastHit, Mathf.Infinity);
            return raycastHit;
        }

        protected void CreateEffect(Castable castable, RaycastHit raycastHit)
        {
            Vector3 position = raycastHit.point + (raycastHit.normal * Physics.defaultContactOffset);
            Quaternion rotation = Quaternion.LookRotation(raycastHit.normal);

            CreateEffect(castable, position, rotation);
        }
        protected void CreateEffect(Castable castable, Vector3 position, Quaternion rotation)
        {
            if (castable == null)
                return;
            // castable caster and instantiate effect
            castable.SetCaster(_caster);
            GameObject newCastableGameObject = Instantiate(castable.gameObject, position, rotation);
        }
        protected void ApplyResponseDynamics(SurfaceResponse.ResponseEffect responseEffect, RaycastHit raycastHit)
        {
            if (responseEffect.stick)
            {
                StickTo(raycastHit.collider.gameObject);
                return;
            }

            if (responseEffect.bounciness != 0)
                _rigidbody.velocity += (_relativeImpactVelocity.magnitude * responseEffect.bounciness) * raycastHit.normal;

            // _rigidbody.velocity += Vector3.Reflect(-_relativeImpactVelocity, raycastHit.normal) * responseEffect.bounciness;

            _rigidbody.AddForce(raycastHit.normal * responseEffect.force, responseEffect.forceMode);

            float frictionScale = 1f - responseEffect.friction;
            _rigidbody.velocity *= frictionScale;
            _rigidbody.angularVelocity *= frictionScale;
        }
        protected SurfaceResponse.ResponseEffect GetResponseEffect(RaycastHit raycastHit, SurfaceResponse surfaceResponse)
        {
            // Determine if there is a surface response
            if (surfaceResponse == null)
                return null;

            // Determine if there is a response effect
            SurfaceResponse.ResponseEffect responseEffect = surfaceResponse.GetResponseEffect(raycastHit, _relativeImpactVelocity.magnitude);
            if (responseEffect == null)
                return null;
            return responseEffect;
        }
        #endregion
    }
}