using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    [Serializable]
    sealed internal class CameraUnion
    {
        internal Vector3 Position
        {
            get {
                if (anchorTransform == null)
                    return offset;
                else
                    return anchorTransform.position + (anchorTransform.rotation * offset);
            }
        }
        internal bool HasPivot
        {
            get
            {
                return anchorTransform != null;
            }
        }
        [SerializeField] internal Transform anchorTransform;
        internal CameraController cameraController;
        [SerializeField] internal Vector3 offset;
        [SerializeField] internal float fieldOfView = 72f;
        internal float fieldOfViewMagnification = 1;
        public float CurrentFieldOfView
        {
            get
            {
                return fieldOfView / fieldOfViewMagnification;
            }
        }
    }
}
