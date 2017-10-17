using Echo.Management;
using UnityEngine;
namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class Beam : Impactable
    {
        #region Values
        // Determines if the line is currently showing
        private bool _showLine;

        // Defines the line render component associated with the game object of the beam.
        private LineRenderer _lineRenderer;

        [SerializeField] private float _impactRate;
        private float _nextImpactTime;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateVisibility();
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            Globals.singleton.Contain(this);
            _lineRenderer = this.GetComponent<LineRenderer>();
            _disallowSticking = true;
        }

        internal void Cast(float range)
        {
            RaycastHit raycastHit;
            Ray ray = new Ray(this.transform.position, this.transform.forward);
            bool hit = Physics.Raycast(ray, out raycastHit, range);
            if (hit)
            {
                if (Time.time > _nextImpactTime)
                {
                    Impact(raycastHit);
                    _nextImpactTime = Time.time + _impactRate;
                }
            }

            _showLine = true;
            SetLinePositions(this.transform.position, hit ? raycastHit.point : this.transform.position + (this.transform.forward * range));
        }

        private void UpdateVisibility()
        {
            _lineRenderer.enabled = _showLine;
            _showLine = false;
        }
        private void SetLinePositions(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }
        #endregion
    }
}