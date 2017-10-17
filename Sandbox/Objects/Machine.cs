using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Echo
{
    sealed internal class Machine : MonoBehaviour
    {
        #region Values
        // Determines if the machine will activate upon awake.
        [SerializeField] private bool _activeOnAwake;

        // Defines the duration in-which the machine will wait to become active.
        [SerializeField] private float _activationDelay;

        // Determines the what the machine will do upon reaching its destination.
        public enum ReachedDestinationEventType
        {
            HaltForever,
            WaitForNextActive,
            Alternate
        }
        [SerializeField] private ReachedDestinationEventType _reachedDestinationEvent;

        [SerializeField] private UnityEvent _haultSubscribers;
        
        public enum MovementType
        {
            Translation,
            Velocity
        }

        // Determines the movement type of the machine.
        [SerializeField] private MovementType _movementType;
        // Determines the direction in-which the machine will travel.
        [SerializeField] private Direction _direction;

        // Defines the speed in-whicn the machine will travel.
        [SerializeField] private float _speed;

        // Defines the travel distance of the machine.
        [SerializeField] private float _travelDistance;

        // Defines the current velocity of the machine.
        private Vector3 _currentVelocity;
        public Vector3 CurrentVelocity { get { return _currentVelocity; } }

        // Defines the rigid body component attached to the game object of the machine
        private Rigidbody _rigidbody;
        // Defines the starting position of the machine.
        private Vector3 _startingPosition;
        // Defines the destination of the machine/
        private Vector3 _destinationPosition;

        // Determines if the machine is currently active.
        private bool _active;
        // Defines the time in-which the machine will be fully active.
        private float _activeTime;
        // Determines if the machine is currently returning to the start position.
        private bool _returnToStart;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateMovement();
            this.transform.Rotate(Vector3.forward, 20f * Time.deltaTime);
        }

        private void OnDestroy()
        {
            this.transform.DetachChildren();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_destinationPosition, 0.25f);
                Gizmos.DrawWireSphere(_startingPosition, 0.25f);
                Gizmos.DrawLine(_startingPosition, _destinationPosition);
            }
            else
            {
                Vector3 destinationPosition = this.transform.position + (Vector3Helper.GetDirection(_direction, this.transform) * _travelDistance);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(destinationPosition, 0.25f);
                Gizmos.DrawLine(this.transform.position, destinationPosition);
            }
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.mass = 10;
                _rigidbody.angularDrag = 0;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                if (_direction != Direction.LocalUp && _direction != Direction.WorldUp && _direction != Direction.LocalDown && _direction != Direction.WorldDown)
                    _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            }

            // Define default positions
            _startingPosition = this.transform.position;
            _destinationPosition = this.transform.position + (Vector3Helper.GetDirection(_direction, this.transform) * _travelDistance);

            _active = _activeOnAwake;
        }

        private void UpdateMovement()
        {
            if (_active && _activeTime <= Time.timeSinceLevelLoad)
            {
                // Define the destination of the machine
                Vector3 destination = _returnToStart ? _startingPosition : _destinationPosition;

                float distance = Vector3.Distance(this.transform.position, destination);
                bool reachedDestination = distance < 0.01f;

                // Move the machine toward its destination
                if (_movementType == MovementType.Velocity)
                {
                    _rigidbody.isKinematic = false;
                    // Move the machine toward its destination using velocity

                    // Define direction, distance, and speed based on distance
                    Vector3 direction = (destination - this.transform.position).normalized;
                    float speed = this._speed * (distance > 1 ? 1 : distance);

                    // Set velocity
                    _rigidbody.velocity = (direction * speed) * Time.deltaTime;
                    
                    _currentVelocity = _rigidbody.velocity;
                }
                else
                {
                    // Move the machine toward its desination using translation
                    this.transform.position = Vector3.MoveTowards(this.transform.position, destination, _speed * Time.deltaTime);
                }

                if (reachedDestination)
                    Hault();

                if (_reachedDestinationEvent == ReachedDestinationEventType.Alternate && reachedDestination)
                    _returnToStart = !_returnToStart;

                if (_reachedDestinationEvent != ReachedDestinationEventType.Alternate)
                    _active = !reachedDestination;
            }
        }

        private void Hault()
        {
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.velocity = Vector3.zero;
            }
            _haultSubscribers.Invoke();
        }

        public void Activate()
        {
            if (_active)
                return;

            _activeTime = Time.timeSinceLevelLoad + _activationDelay;

            // If the machine has reached its destination and doesnt allow for toggle, abort
            float destinationDistance = Vector3.Distance(this.transform.position, _destinationPosition);
            bool reachedDestination = destinationDistance < 0.01f;
            if (reachedDestination && _reachedDestinationEvent != ReachedDestinationEventType.WaitForNextActive)
                return;

            // Set return to start state based on location
            _returnToStart = reachedDestination;

            // Activate the machine
            _active = true;
        }
        public void Deactivate()
        {
            _active = false;
        }
        #endregion
    }
}