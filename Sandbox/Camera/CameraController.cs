using Echo.Management;
using Echo.User;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class CameraController : Controllable
    {
        #region Values
        internal Camera camera;
        private CameraUnion _cameraUnion;
        internal CameraEffector cameraEffector;

        internal enum Mode
        {
            Anchored,
            Free
        }
        [SerializeField] private Mode _mode = Mode.Anchored;
        [Header("Flycam")]
        [SerializeField] private float _flycamMovementSpeed = 18f;
        [SerializeField] private float _flycamBoostModifier = 100f;
        [SerializeField] private float _flycamFieldOfView = 72f;

        [HideInInspector]
        public float fieldOfView;
        
        private float Sensitivity
        {
            get { return _player == null ? 0 : _player.profile.inputSettings.lookSensitivity * Globals.singleton.cameraDefaults.sensitivityMultiplier; }
        }
        private Vector3 _lookingRotation;

        #endregion

        #region Unity Functions
        public void Start()
        {            // Obtain our camera
            camera = transform.GetComponent<Camera>();
            if (camera == null)
                camera = transform.gameObject.AddComponent<Camera>();
            fieldOfView = camera.fieldOfView;
        }
        
        private void Update()
        {
            if (_cameraUnion == null)
                _mode = Mode.Free;
            if (_controller != null)
            {
                Move(_controller.movement, _controller.rise, _controller.fall, _controller.boost);
                Look(_controller.looking);
            }
            
            UpdateOrientation();
            UpdateFieldOfView();
        }

        private void OnPreRender()
        {
            SetObliqueness();
        }
        #endregion

        #region Functions

        public bool CanSeePosition(Vector3 position)
        {
            bool canSee = camera.WorldToScreenPoint(position).z > 0;
            canSee &= !Physics.Linecast(this.transform.position, position);
            return canSee;
        }
        internal void SetMode(Mode mode)
        {
            _mode = mode;
            Development.Debug.Out(this, string.Format("Changed camera mode to {0}", mode), Development.Debug.MessagePriority.Information);
        }
        internal override void SetAspects(Player player)
        {
            base.SetAspects(player);
            _cameraUnion = player.biped.cameraUnion;
            _cameraUnion.cameraController = this;
        }

        public void Look(Vector2 input)
        {
            float rate = Sensitivity;
            input *= (rate * Time.deltaTime);
            _lookingRotation.x = MathHelpers.ClampAngle(_lookingRotation.x + input.y, Globals.singleton.cameraDefaults.minVerticalAngle, Globals.singleton.cameraDefaults.maxVerticalAngle);
            _lookingRotation.y += input.x;
        }
        public void Move(Vector2 input, bool rise, bool fall, bool boost)
        {
            float movementSpeed = _flycamMovementSpeed + (boost ? _flycamBoostModifier : 0);

            // Move forward/sideways
            transform.position += ((transform.forward * input.y) * movementSpeed) * Time.deltaTime;
            transform.position += ((transform.right * input.x) * movementSpeed) * Time.deltaTime;

            // Calculate how much we should move up and down.
            float posUpChange = rise ? 1 : 0;
            posUpChange += fall ? -1 : 0;

            transform.position += ((transform.up * posUpChange) * movementSpeed) * Time.deltaTime;
        }

        private void SetObliqueness()
        {
            Matrix4x4 matrix = camera.projectionMatrix;
            matrix[1, 2] = _mode != Mode.Free ? Globals.singleton.cameraDefaults.verticalObliqueOffset : 0;
            camera.projectionMatrix = matrix;
        }

        private void UpdateOrientation()
        {
            this.transform.rotation = Quaternion.Euler(_lookingRotation);
            if (_mode == Mode.Anchored)
            {
                this.transform.position = _cameraUnion.Position;
                if (_cameraUnion.anchorTransform)
                    _cameraUnion.anchorTransform.eulerAngles = new Vector3(_cameraUnion.anchorTransform.eulerAngles.x, _lookingRotation.y, _cameraUnion.anchorTransform.eulerAngles.z);
            }
        }
        private void UpdateFieldOfView()
        {
            if (_mode != Mode.Free)
                fieldOfView = _cameraUnion.CurrentFieldOfView;
            else
                fieldOfView = _flycamFieldOfView;

            camera.fieldOfView = camera.fieldOfView.Towards(fieldOfView, Time.deltaTime * Globals.singleton.cameraDefaults.fieldOfViewTransitionSpeed);
        }
        #endregion
    }
}