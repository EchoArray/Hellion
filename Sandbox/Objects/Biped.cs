using Echo.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ObjectProperties))]
    sealed public class Biped : Controllable
    {
        #region Values
        [SerializeField] internal CameraUnion cameraUnion;
        

        [Serializable]
        sealed internal class Movement
        {
            [Serializable]
            sealed internal class MovementBoost
            {
                [SerializeField] internal float cooldownDuration = 3;
                [SerializeField] internal float duration = 0.1215f;

                [Range(0, 1)]
                [SerializeField]
                internal float forwardToLookForwardScale = 0.75f;
                [SerializeField] internal ForceMode forceMode =  ForceMode.Impulse;
                [SerializeField] internal float force = 1.5f;
                [SerializeField] internal AnimationCurve forceScaleOverDuration = AnimationCurve.Linear(0, 1, 1, 1);
                [Range(0, 1)]
                [SerializeField]
                internal float existingVelocityScale = 0.5f;
                internal Vector3 direction;
                internal float boostEndTime;
                internal float cooledDownTime;
            }
            [SerializeField] internal MovementBoost boost;

            /// <summary>
            /// Indicates whether the biped uses flying or walking physics.
            /// </summary>
            [Header("Flying")]
            [SerializeField]
            internal bool usesFlyingPhysics;
            /// <summary>
            /// The acceleration with which the flying biped rises and falls.
            /// </summary>
            [SerializeField] internal float flyingRiseFallAcceleration;
            /// <summary>
            /// The maximum speed with which the biped can rise or fall.
            /// </summary>
            [SerializeField] internal float flyingRiseFallSpeed;
            /// <summary>
            /// The speed which is added when boost is fully held down for rise/fall and general movement.
            /// </summary>
            [SerializeField] internal float flyingBoostModifier;

            /// <summary>
            /// Indicates the normal for the ground on which the biped is standing on when grounded.
            /// </summary>
            [Header("Ground Slope")]
            internal Vector3 groundNormal;

            /// <summary>
            /// Indicates the angle limit on which the biped can climb up a given surface.
            /// </summary>
            [Header("Slopes")]
            [SerializeField]
            internal float groundSlopeClimbableLimit;
            /// <summary>
            /// Indicates the angle limit on which the biped can slide/bounce on.
            /// </summary>
            [SerializeField] internal float velocityConversionAngleLimit;
            /// <summary>
            /// A value which scales the biped's ability to convert it's velocity when striking a slope while standing.
            /// </summary>
            [SerializeField] internal float slopeStandingVelocityConversionScale;
            /// <summary>
            /// A value which scales the biped's ability to convert it's velocity when striking a slope while crouching.
            /// </summary>
            [SerializeField] internal float slopeCrouchingVelocityConversionScale;
            
            /// <summary>
            /// Indicates the height at which the camera should be when the biped is standing.
            /// </summary>
            [Header("Height")]
            [SerializeField]
            internal float standingCameraHeight;
            /// <summary>
            /// Indicates the height at which the camera should be when the biped is crouching.
            /// </summary>
            [SerializeField] internal float crouchingCameraHeight;
            /// <summary>
            /// Indicates the height at which the biped is currently at which can vary based off crouching state.
            /// </summary>
            internal float cameraHeight;
            /// <summary>
            /// The speed at which the biped will transition from standing height to crouching height and vice-versa.
            /// </summary>
            [SerializeField] internal float crouchingTransitionSpeed;
            /// <summary>
            /// Indicates if our crouching state is in the process of changing.
            /// </summary>
            internal bool crouchingChanging;
            private bool _crouching;
            /// <summary>
            /// Defines if our biped is transitioning into, or is crouching.
            /// </summary>
            internal bool crouching
            {
                get
                {
                    return _crouching;
                }
                set
                {
                    if (_crouching != value)
                        crouchingChanging = true;
                    _crouching = value;
                }
            }
            /// <summary>
            /// Indicates how much the biped is standing, where 0 is crouching and 1 is standing.
            /// </summary>
            internal float StandingFraction
            {
                get
                {
                    // Finds out how much we're standing. Crouching is 0, Standing is 1.
                    return (cameraHeight - crouchingCameraHeight) / (standingCameraHeight - crouchingCameraHeight);
                }
            }
            
            [Header("Vertical Movement")]
            /// <summary>
            /// The instantaneous velocity gained when the player attempts to jump.
            /// </summary>
            [SerializeField] internal float jumpVelocity;

            internal float jumpingFrameEnd;

            /// <summary>
            /// Indicates the time at which the jump button was hit, used to trigger regular and pre-landing jumps.
            /// </summary>
            internal float jumpAttemptTime;

            /// <summary>
            /// Defines the time that is allowed to pass from when our jump button is pressed till we have an available jump before we trigger it. 
            /// If the jump button is hit too long past this threshold, the jump will not be triggered. Used for pre-landing jump.
            /// </summary>
            [SerializeField] internal float jumpLandingDelayThreshold;
            /// <summary>
            /// Defines the time after walking off an edge which the user is allowed to trigger a late-jump.
            /// </summary>
            [SerializeField] internal float edgeJumpWindow;
            /// <summary>
            /// Indicates if the biped can edge jump. This is set to false if the biped has already edge jumped in this airborne session.
            /// </summary>
            internal bool edgeJumpedAlready;
            /// <summary>
            /// Indicates if the player is falling (not to be confused with airborne)
            /// </summary>
            internal bool falling;
            /// <summary>
            /// Indicates the duration which the player has been falling.
            /// </summary>
            internal float fallDuration;
            /// <summary>
            /// Indicates the duration which the player has been airborne.
            /// </summary>
            internal float airborneDuration;
            /// <summary>
            /// Indicates if our player is currently on the ground or airborne.
            /// </summary>
            internal bool grounded;
            internal float gorundedSetTime;

            /// <summary>
            /// Indicates the speed decay when airborne.
            /// </summary>
            [Header("Horizontal Movement")]
            [SerializeField]
            internal float drag;
            /// <summary>
            /// Indicates the speed decay when grounded.
            /// </summary>
            [SerializeField] internal float groundedSpeedDecay;
            /// <summary>
            /// Indicates the acceleration used when the biped is grounded.
            /// </summary>
            [SerializeField] internal float groundedAcceleration;

            [Serializable]
            internal class DirectionalSpeedFactor
            {
                [SerializeField] internal float forward;
                [SerializeField] internal float backward;
                [SerializeField] internal float sideways;
            }
            [SerializeField] internal DirectionalSpeedFactor standingSpeed;
            [SerializeField] internal DirectionalSpeedFactor crouchingSpeed;
            [SerializeField] internal DirectionalSpeedFactor airborneAcceleration;
            [SerializeField] internal DirectionalSpeedFactor airborneSpeed;

            /// <summary>
            /// Determines if airborne opposition scaling is enabled.
            /// </summary>
            [Space(15)]
            [SerializeField]
            internal bool airborneOppositionScaling = true;
            /// <summary>
            /// Assuming the biped is attempting to oppose the direction indicated, this is a threshold where if exceeded in the direction indicated, the player cannot oppose.
            /// </summary>
            [SerializeField] internal AnimationCurve airborneOpposingScale;

            internal Vector3 velocity;

            internal enum JumpDirection
            {
                TransformUp,
                SurfaceNormal
            }
        }
        /// <summary>
        /// Describes the biped's movement properties and states.
        /// </summary>
        [SerializeField] internal Movement movement;

        [Serializable]
        sealed private class Items
        {
            // Defines the selected weapon of the posessed weapons.
            [SerializeField] internal int maxWeaponCount = 2;
            internal int selectedWeaponIndex;
            [SerializeField] internal float weaponSwitchCooldown = 1f;
            internal float nextAllowWeaponSwitchTime;
            internal bool HasWeapon
            {
                get
                {
                    foreach (Weapon weapon in weapons)
                        if (weapon != null)
                            return true;
                    return false;
                }
            }
            // A collection of weapons possesed by the biped.
            [SerializeField] internal Weapon[] weapons;
        }
        [SerializeField] private Items _items;


        // Defines the rigid body component attached to the game object assocaited with the biped.
        private Rigidbody _rigidbody;

        // Defines the capsule collider component attached to the game object assocaited with the biped.
        private CapsuleCollider _capsuleCollider;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (_controller != null)
            {
                if (_controller.jump)
                    Jump();

                if (_controller.boost && _controller.movement.magnitude > 0)
                    Boost();

                if (_controller.switchWeapon)
                    SwitchWeapon();

                Crouch(_controller.crouch);
                Move(_controller.movement);
                UpdateInteraction();
                FindEquipment();
            }
            UpdateBoost();
            UpdateCrouch();
            UpdateSpeedDecay();
            UpdateVerticalValues();
        }
        private void LateUpdate()
        {
            UpdateWeaponPosition();
        }

        private void OnCollisionEnter(Collision collision)
        {
            SetGrounding(collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            SetGrounding(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            SetExitGrounding();
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            Globals.singleton.Contain(this);

            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
            _capsuleCollider = this.gameObject.GetComponent<CapsuleCollider>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            SetCameraHeight();
            _items.weapons = new Weapon[_items.maxWeaponCount];
        }

        private void UpdateInteraction()
        {
            Development.Debug.ShowValue("Interaction: ", string.Empty);

            Weapon weapon = FindWeapon();
            if (weapon != null)
            {
                Development.Debug.ShowValue("Interaction: ", weapon.PickupMessage);
                if (_controller.interact)
                    EquipWeapon(weapon);
            }

            Switch swtch = FindSwitch();
            if (swtch != null)
            {
                Development.Debug.ShowValue("Interaction: ", swtch.Message);
                if (_controller.interact)
                    swtch.Invoke();
            }
        }


        #region Weapons
        private Switch FindSwitch()
        {
            Switch[] switches = FindObjectsOfType<Switch>();
            foreach (Switch aSwitch in switches)
            {
                float sqrMagnitude = (aSwitch.transform.position - this.transform.position).sqrMagnitude;
                float radius = Globals.singleton.bipedDefaults.interactionRadius * Globals.singleton.bipedDefaults.interactionRadius;
                if (sqrMagnitude <= radius)
                {
                    if (aSwitch.Enabled)
                    {
                        if (_player == null)
                            return aSwitch;
                        else if (cameraUnion != null && cameraUnion.cameraController != null)
                        {
                            bool canSee = cameraUnion.cameraController.CanSeePosition(aSwitch.transform.position);
                            if (canSee)
                                return aSwitch;
                        }
                    }
                }
            }
            return null;
        }
        private Weapon FindWeapon()
        {
            Collider[] weaponColliders = Physics.OverlapSphere(this.transform.position, Globals.singleton.bipedDefaults.interactionRadius, 1 << Globals.WEAPON_LAYER);

            Weapon weapon = null;
            int priority = -1;
            foreach (Collider collider in weaponColliders)
            {
                bool canSee = !Physics.Linecast(this.transform.position, collider.transform.position, ~(1 << Globals.WEAPON_LAYER));
                if (!canSee)
                    continue;
                Weapon prospectWeapon = collider.gameObject.GetComponent<Weapon>();

                if (!prospectWeapon.AllowPickup)
                    continue;

                bool weaponAlreadyHeld = false;
                foreach (Weapon equiptWeapon in _items.weapons)
                {
                    if (equiptWeapon == null)
                        continue;
                    if (equiptWeapon.HasRoomForResources)
                        equiptWeapon.ReceiveResources(prospectWeapon);
                    weaponAlreadyHeld = prospectWeapon.UniqueId == equiptWeapon.UniqueId;
                }

                if (weaponAlreadyHeld)
                    continue;

                if ((int)prospectWeapon.PickupPriority > priority)
                    weapon = prospectWeapon;
            }
            return weapon;
        }

        private void FindEquipment()
        {
            Collider[] equipmentColliders = Physics.OverlapSphere(this.transform.position, Globals.singleton.bipedDefaults.interactionRadius, 1 << Globals.EQUIPMENT_LAYER);

            foreach (Collider collider in equipmentColliders)
            {
                bool canSee = !Physics.Linecast(this.transform.position, collider.transform.position, ~(1 << Globals.EQUIPMENT_LAYER));
                if (!canSee)
                    continue;
                Equipment equipment = collider.gameObject.GetComponent<Equipment>();

                foreach (Weapon weapon in _items.weapons)
                {
                    if (weapon == null)
                        continue;
                    if (!weapon.HasRoomForResources)
                        continue;
                    weapon.ReceiveResources(equipment);
                }
            }
        }

        private void SwitchWeapon()
        {
            int prospectSelection = _items.selectedWeaponIndex == _items.weapons.Length - 1 ? 0 : _items.selectedWeaponIndex + 1;
            if (_items.weapons[prospectSelection] != null)
                SelectWeapon(_items.weapons[prospectSelection]);
        }
        private void SelectWeapon(Weapon weapon)
        {
            for (int i = 0; i < _items.weapons.Length; i++)
            {
                Weapon heldWeapon = _items.weapons[i];
                if (heldWeapon == null)
                    continue;

                bool isWeapon = weapon == heldWeapon;
                if (isWeapon)
                    _items.selectedWeaponIndex = i;

                heldWeapon.AllowFire = isWeapon;
                heldWeapon.SetVisibility(isWeapon);
            }
        }
        internal void EquipWeapon(Weapon weapon)
        {
            weapon.Equipt(this);
            for (int i = 0; i < _items.weapons.Length; i++)
            {
                if (_items.weapons[i] == null)
                {
                    _items.weapons[i] = weapon;
                    SelectWeapon(weapon);
                    return;
                }
            }

            UnEquipWeapon(_items.weapons[_items.selectedWeaponIndex]);

            _items.weapons[_items.selectedWeaponIndex] = weapon;
            SelectWeapon(weapon);
        }
        private void UnEquipWeapon(Weapon weapon)
        {
            bool hasCamera = cameraUnion != null && cameraUnion.cameraController != null;
            Vector3 dropDirection = hasCamera ? cameraUnion.cameraController.transform.forward : this.transform.forward;
            Vector3 dropPosition = this.transform.position + (movement.cameraHeight / 2) * this.transform.up;
            dropPosition += this.transform.forward * _capsuleCollider.radius;
            _items.weapons[_items.selectedWeaponIndex].transform.position = dropPosition;

            _items.weapons[_items.selectedWeaponIndex].UnEquipt(dropDirection);
        }

        private void UpdateWeaponPosition()
        {
            if (!_items.HasWeapon)
                return;
            Weapon weapon = _items.weapons[_items.selectedWeaponIndex];
            Vector3 offset = cameraUnion.cameraController.transform.rotation * weapon.CameraOffset;

            weapon.transform.position = cameraUnion.cameraController.transform.position + offset;
            weapon.transform.rotation = cameraUnion.cameraController.transform.rotation;
        } 
        #endregion

        #region Movement
        private void Boost()
        {
            if (Time.time <= movement.boost.cooledDownTime)
                return;
            movement.boost.boostEndTime = Time.time + movement.boost.duration;
            movement.boost.cooledDownTime = Time.time + movement.boost.duration + movement.boost.cooldownDuration;
            Quaternion rotation = cameraUnion == null ? this.transform.rotation : Quaternion.Lerp(this.transform.rotation, cameraUnion.cameraController.transform.rotation, movement.boost.forwardToLookForwardScale);
            movement.boost.direction = rotation * new Vector3(_controller.movement.x, 0, _controller.movement.y);
            _rigidbody.velocity *= movement.boost.existingVelocityScale;
        }
        private void UpdateBoost()
        {
            if (Time.time <= movement.boost.boostEndTime)
            {
                float scale = movement.boost.forceScaleOverDuration.Evaluate(1 - ((movement.boost.boostEndTime - Time.time) / movement.boost.duration));
                _rigidbody.AddForce(movement.boost.direction * movement.boost.force * Time.deltaTime * scale, movement.boost.forceMode);
            }
        }

        private void UpdateSpeedDecay()
        {
            movement.velocity = _rigidbody.velocity;
            if (movement.usesFlyingPhysics)
            {
                if (movement.velocity.magnitude < movement.drag * Time.deltaTime)
                    movement.velocity = Vector3.zero;
                else
                    movement.velocity -= movement.velocity.normalized * movement.drag * Time.deltaTime;
            }
            else
            {
                float verticalVelocity = Vector3.Dot(transform.up, movement.velocity);

                Vector3 horizontalMovement = (movement.velocity - (transform.up * verticalVelocity));
                float decayOrDragVelocity = movement.grounded ? movement.groundedSpeedDecay : movement.drag;
                
                if (horizontalMovement.magnitude < decayOrDragVelocity * Time.deltaTime)
                    movement.velocity = transform.up * verticalVelocity;
                else
                    movement.velocity -= horizontalMovement.normalized * decayOrDragVelocity * Time.deltaTime;
            }
            _rigidbody.velocity = movement.velocity;
        }
        private void UpdateVerticalValues()
        {
            float verticalVelocity = Vector3.Dot(transform.up, _rigidbody.velocity);
            movement.falling = verticalVelocity < 0;

            if (movement.falling && !movement.grounded)
                movement.fallDuration += Time.deltaTime;

            if (!movement.grounded)
                movement.airborneDuration += Time.deltaTime;
        }

        private void SetGrounding(Collision collision)
        {
            if (movement.grounded && Time.fixedTime == movement.gorundedSetTime || movement.usesFlyingPhysics)
                return;

            bool previouslyGrounded = movement.grounded;
            bool grounded = false;
            foreach (ContactPoint contactPoint in collision.contacts)
            {
                float angle = Vector3.Angle(Vector3.up, contactPoint.normal);
                if (angle < movement.groundSlopeClimbableLimit)
                {
                    grounded = true;
                    if (!previouslyGrounded)
                        Ground(contactPoint.normal);
                    return;
                }
            }
            if (!grounded)
                movement.grounded = false;
        }
        private void SetExitGrounding()
        {
            if (movement.grounded && Time.fixedTime != movement.gorundedSetTime)
                movement.grounded = false;
        }

        private void Ground(Vector3 normal)
        {
            if (Time.time < movement.jumpingFrameEnd)
                return;
            movement.groundNormal = normal;
            movement.gorundedSetTime = Time.fixedTime;
            movement.grounded = true;
            movement.edgeJumpedAlready = false;
            movement.airborneDuration = 0;
            movement.fallDuration = 0;
            float angle = Vector3.Angle(Vector3.up, normal);
            if (movement.falling && angle < movement.groundSlopeClimbableLimit && angle >= movement.velocityConversionAngleLimit)
            {
                float scale = movement.crouching || movement.crouchingChanging ? movement.slopeCrouchingVelocityConversionScale : movement.slopeStandingVelocityConversionScale;
                Slide(movement.velocity, normal, scale);
            }
            CheckJumpAttempt();
        }
        private void Slide(Vector3 inVelocity, Vector3 normal, float scale)
        {
            float velocityMagnitude = inVelocity.magnitude;
            if (velocityMagnitude == 0)
                return;

            float groundVelocityScale = Vector3.Dot(normal, inVelocity);
            Vector3 direction = (inVelocity - (normal * groundVelocityScale)).normalized;

            float slideMagnitude = Vector3.Dot(direction, inVelocity.normalized);
            slideMagnitude = slideMagnitude * velocityMagnitude * scale;
            slideMagnitude = Mathf.Clamp(slideMagnitude, 0, velocityMagnitude);

            _rigidbody.velocity = slideMagnitude * direction;
        }

        public void Move(Vector2 input, float flyingBoostFraction = 0.0f)
        {
            // Grab our rigid body velocity.
            movement.velocity = _rigidbody.velocity;

            // Calculate how much movement speed we'll cap our biped at for each direction.
            float forwardSpeedMax;
            float backwardSpeedMax;
            float strafeSpeedMax;
            if (movement.grounded)
            {
                forwardSpeedMax = (movement.standingSpeed.forward * movement.StandingFraction) + (movement.crouchingSpeed.forward * (1 - movement.StandingFraction));
                backwardSpeedMax = (movement.standingSpeed.backward * movement.StandingFraction) + (movement.crouchingSpeed.backward * (1 - movement.StandingFraction));
                strafeSpeedMax = (movement.standingSpeed.sideways * movement.StandingFraction) + (movement.crouchingSpeed.sideways * (1 - movement.StandingFraction));
            }
            else
            {
                forwardSpeedMax = movement.airborneSpeed.forward;
                backwardSpeedMax = movement.airborneSpeed.backward;
                strafeSpeedMax = movement.airborneSpeed.sideways;
            }

            // Find out our distribution of x and y.
            // We do this because if our user is moving diagnolly, it should average the forward/backward speed and strafe speed, etc.
            float absX = Mathf.Abs(input.x);
            float absY = Mathf.Abs(input.y);
            float distributionX = absX / (absX + absY);
            float distributionY = 1 - distributionX;

            // Using the distributions as weights on forward/backward and strafe speed, calculate our overall total velocity.
            float currentTotalVelocityMax = (((input.y > 0) ? forwardSpeedMax : backwardSpeedMax) * distributionY) + (strafeSpeedMax * distributionX);

            // Make a normalized vector of our distributions to find out the forward and right components.
            // This is the same as having the hypotenuse of a triangle and finding opposite and adjacent sides based off an angle.
            // We can optimize this in the future using sin/cos, etc.
            Vector3 movementDistribution = new Vector3(distributionX, 0, distributionY).normalized;
            movementDistribution *= currentTotalVelocityMax;
            float currentForwardVelocityMax = movementDistribution.z * (input.y > 0 ? 1 : -1);
            float currentRightVelocityMax = movementDistribution.x * (input.x > 0 ? 1 : -1);

            // Create our state dependent acceleration modifiers for this update.
            float forwardAccelerationModifier;
            float rightAccelerationModifier;
            if (movement.grounded)
            {
                float accelerationModifier = movement.groundedAcceleration + (movement.flyingBoostModifier * flyingBoostFraction);
                accelerationModifier *= Time.deltaTime;
                forwardAccelerationModifier = accelerationModifier * input.y;
                rightAccelerationModifier = accelerationModifier * input.x;
            }
            else
            {
                forwardAccelerationModifier = (movement.flyingBoostModifier * flyingBoostFraction);
                forwardAccelerationModifier += input.y > 0 ? movement.airborneAcceleration.forward : movement.airborneAcceleration.backward;
                forwardAccelerationModifier *= input.y * Time.deltaTime;
                rightAccelerationModifier = movement.airborneAcceleration.sideways + (movement.flyingBoostModifier * flyingBoostFraction);
                rightAccelerationModifier *= input.x * Time.deltaTime;
            }
            // Next, we ONLY want to apply more velocity in that direction if we haven't reached the max speed.
            // This is because other forces in the game could push us faster than this max, so we don't want to simply
            // Just set the velocity in that direction as this max because we'd instantly be stopped.
            // This is why we broke it into components..

            // We'll get the direction of our right and left with respect to our current slope
            Vector3 forwardDirection = this.transform.forward;
            Vector3 rightDirection = this.transform.right;
            if (movement.grounded && Globals.singleton.bipedDefaults.rampUpVelocity)
            {
                // Grab our forward and right direction with respect to the slope.
                forwardDirection = forwardDirection.AtSlope(movement.groundNormal);
                rightDirection = rightDirection.AtSlope(movement.groundNormal);
            }

            // Figure out the current speed of forward and right to determine if we should add more velocity.
            float currentForwardVelocity = Vector3.Dot(movement.velocity, forwardDirection);
            float currentRightVelocity = Vector3.Dot(movement.velocity, rightDirection);

            // Check if our targets are in our max speed bounds.
            bool sameDirectionZ = ((forwardAccelerationModifier > 0) == (currentForwardVelocity > 0)) | (currentForwardVelocity == 0) | (forwardAccelerationModifier == 0);
            bool sameDirectionX = ((rightAccelerationModifier > 0) == (currentRightVelocity > 0)) | (currentRightVelocity == 0) | (rightAccelerationModifier == 0);
            float targetForwardVelocity = currentForwardVelocity + forwardAccelerationModifier;
            float targetRightVelocity = currentRightVelocity + rightAccelerationModifier;

            // If we're trying to accelerate beyond our max speed, we need to make sure we don't exceed it.
            if (sameDirectionZ && (Mathf.Abs(currentForwardVelocity) > Mathf.Abs(currentForwardVelocityMax)))
                // If we're already faster, don't add any more acceleration.
                forwardAccelerationModifier = 0;
            else if (((currentForwardVelocityMax > 0) && targetForwardVelocity > currentForwardVelocityMax) ||
                    ((currentForwardVelocityMax < 0) && targetForwardVelocity < currentForwardVelocityMax))
                // If our acceleration will exceed max, go to max directly instead.
                forwardAccelerationModifier = currentForwardVelocityMax - currentForwardVelocity;

            // If we're trying to accelerate beyond our max speed, we need to make sure we don't exceed it.
            if (sameDirectionX && (Mathf.Abs(currentRightVelocity) > Mathf.Abs(currentRightVelocityMax)))
                // If we're already faster, don't add any more acceleration.
                rightAccelerationModifier = 0;
            else if (((currentRightVelocityMax > 0) && targetRightVelocity > currentRightVelocityMax) ||
                    ((currentRightVelocityMax < 0) && targetRightVelocity < currentRightVelocityMax))
                // If our acceleration will exceed max, go to max directly instead.
                rightAccelerationModifier = currentRightVelocityMax - currentRightVelocity;

            // If we're airborne and trying to oppose our current velocity direction, use the opposition curves to scale our movement.
            if (movement.airborneOppositionScaling && !movement.grounded)
            {
                if (!sameDirectionZ)
                {
                    // Make sure we don't exceed our threshold in our forward/backward direction.
                    float deadzone = currentForwardVelocity >= 0 ? movement.airborneSpeed.forward : movement.airborneSpeed.backward;
                    // This value will scale our opposing scale with respect to movement acceleration. This is so since backward is slower than forward, it will still scale proportionately.
                    float scaleMultiplier = currentForwardVelocity >= 0 ? (movement.airborneSpeed.forward / movement.airborneSpeed.backward) : (movement.airborneSpeed.backward / movement.airborneSpeed.forward);
                    if (Mathf.Abs(currentForwardVelocity) > deadzone)
                        forwardAccelerationModifier = 0;
                    else
                        forwardAccelerationModifier *= Mathf.Clamp(movement.airborneOpposingScale.Evaluate((1 - (Mathf.Abs(currentForwardVelocity) / deadzone))), 0, 1);

                }
                if (!sameDirectionX)
                {
                    // Make sure we don't exceed our threshold in our strafing directions.
                    float deadzone = (movement.airborneSpeed.sideways);
                    if (Mathf.Abs(currentRightVelocity) > deadzone)
                        rightAccelerationModifier = 0;
                    else
                        rightAccelerationModifier *= Mathf.Clamp(movement.airborneOpposingScale.Evaluate((1 - (Mathf.Abs(currentRightVelocity) / deadzone))), 0, 1);
                }
            }

            // Add our controlled movement.
            Vector3 addedMovementVelocity = (forwardDirection * forwardAccelerationModifier) + (rightDirection * rightAccelerationModifier);
            Vector3 currentMovementVelocity = (forwardDirection * currentForwardVelocity) + (rightDirection * currentRightVelocity);
            //gizmoHorizontalVelocity = addedMovementVelocity + currentMovementVelocity;
            movement.velocity += addedMovementVelocity;

            // Set our modified velocity back.
            _rigidbody.velocity = movement.velocity;
        }
        public void RiseFall(bool riseInput, bool fallInput, float boostFractionInput)
        {
            // If we're not using flying physics, stop.
            if (!movement.usesFlyingPhysics)
                return;

            // Calculate our movement speed
            float movementSpeed = movement.flyingRiseFallAcceleration + (movement.flyingBoostModifier * boostFractionInput);

            // Calculate how much we should move up and down.
            float velocityChange = 0;
            if (riseInput)
                velocityChange = 1;
            if (fallInput)
                velocityChange += -1;
            velocityChange *= movementSpeed;

            // Get our current velocity in this direction
            float currentMovementSpeed = Vector3.Dot(transform.up, _rigidbody.velocity);
            bool sameDirection = (velocityChange > 0) == (currentMovementSpeed > 0);
            velocityChange *= Time.deltaTime;
            if (sameDirection)
            {
                // If we're going faster in that direction already, do nothing.
                // else, if our increase would set us over the speed, make our increase bring us directly to it.
                if (Mathf.Abs(currentMovementSpeed) >= movement.flyingRiseFallSpeed)
                    velocityChange = 0;
                else if (Mathf.Abs(currentMovementSpeed + velocityChange) > movement.flyingRiseFallSpeed)
                    velocityChange = (movement.flyingRiseFallSpeed - Mathf.Abs(currentMovementSpeed)) * (currentMovementSpeed >= 0 ? 1 : -1);
            }
            _rigidbody.velocity += (transform.up * velocityChange);
        }

        public void Jump()
        {
            if (movement.usesFlyingPhysics)
                return;
            if (Time.time < movement.jumpingFrameEnd)
                return;

            bool edgeJump = !movement.edgeJumpedAlready && !movement.grounded && movement.airborneDuration < movement.edgeJumpWindow;

            if (movement.grounded || edgeJump)
            {
                movement.jumpingFrameEnd = Time.time + (Time.deltaTime * 2);
                float currentJumpDirectionVelocity = Vector3.Dot(transform.up, _rigidbody.velocity);
                float jumpForceToAdd = movement.jumpVelocity - currentJumpDirectionVelocity;

                _rigidbody.velocity += transform.up * jumpForceToAdd;

                movement.grounded = false;
                movement.edgeJumpedAlready = true;
                movement.jumpAttemptTime = 0;
            }
            else
            {
                movement.jumpAttemptTime = Time.time + movement.jumpLandingDelayThreshold;
            }
        }
        private void CheckJumpAttempt()
        {
            if (Time.time <= movement.jumpAttemptTime)
                Jump();
        }
        
        private void UpdateCrouch()
        {
            // We're not updating crouching status, abort.
            if (!movement.crouchingChanging || movement.usesFlyingPhysics)
                return;

            RaycastHit raycastHit;
            Vector3 growingDirection = movement.grounded ? transform.up : -transform.up;
            float currentHeight = _capsuleCollider.height / 2;

            // TODO: This really needs some work
            bool canStand = true;
            if (movement.grounded)
                canStand = !Physics.SphereCast(transform.position, _capsuleCollider.radius - 0.01f, growingDirection, out raycastHit, (movement.standingCameraHeight * 2) - currentHeight, ~Globals.singleton.bipedDefaults.crouchStandCheckIgnoredLayers);


            float targetHeight = movement.crouching ? movement.crouchingCameraHeight : movement.standingCameraHeight;

            float distanceRemaining = Mathf.Abs((_capsuleCollider.height / 2) - targetHeight);

            if (!movement.crouching && !canStand)
                return;

            if (distanceRemaining == 0)
            {
                movement.crouchingChanging = false;
                return;
            }

            float movementFraction = movement.crouchingTransitionSpeed * Time.deltaTime;

            // Update the camera and collider height.
            _capsuleCollider.height = Mathf.MoveTowards(_capsuleCollider.height, targetHeight * 2, movementFraction);
            float movementValue = movement.cameraHeight;
            SetCameraHeight();
            movementValue = movement.cameraHeight - movementValue;


            this.transform.position += growingDirection * movementValue;
        }
        public void Crouch(bool input)
        {
            // If we're using flying physics do nothing
            if (movement.usesFlyingPhysics)
                return;
            if (input && movement.grounded && movement.velocity.magnitude > movement.crouchingSpeed.forward)
                return;

            // Set our crouching state.
            movement.crouching = input;
        }
        private void SetCameraHeight()
        {
            if (cameraUnion != null)
                cameraUnion.offset.y = (_capsuleCollider.height - _capsuleCollider.radius) / 2;
            movement.cameraHeight = (_capsuleCollider.height) / 2;
        }
        #endregion

        #endregion
    }
}