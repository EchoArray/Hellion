using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class SurfaceResponse : MonoBehaviour
    {
        [Serializable]
        public class ResponseEffect
        {
            [SerializeField] internal string name;
            [SerializeField] internal SurfaceType surfaceType;
            [SerializeField] internal bool disableBaseEffect;
            [Space(5)]
            [SerializeField] internal Vector2 velocityRange;
            [SerializeField] internal Castable castable;
            [Space(15)]
            
            [SerializeField] internal bool stick;
            [SerializeField] [Range(0, 1)] internal float bounciness;
            [SerializeField] [Range(0, 1)] internal float friction;

            [Space(5)]
            [SerializeField] internal ForceMode forceMode;
            [SerializeField] internal float force;

        }
        /// Defines a collection of surface types and thier pairing effects.
        [SerializeField] private ResponseEffect[] _responseEffects;

        internal ResponseEffect GetResponseEffect(RaycastHit raycastHit, float velocityMagnitude = 0)
        {
            if (raycastHit.collider == null)
                return null;
            SurfaceType surfaceType = SurfaceType.Generic;
            SurfaceSet surface = raycastHit.collider.gameObject.GetComponent<SurfaceSet>();
            if (surface != null)
                surfaceType = surface.GetSurfaceTypeByTriangle(raycastHit.triangleIndex);

            foreach (ResponseEffect responseEffect in _responseEffects)
                if (responseEffect.surfaceType == surfaceType)
                {
                    bool inVelocityRange = velocityMagnitude >= responseEffect.velocityRange.x && velocityMagnitude <= responseEffect.velocityRange.y;
                    if (responseEffect.velocityRange.magnitude == 0 || inVelocityRange)
                        return responseEffect;
                }

            return null;
        }
    }
}