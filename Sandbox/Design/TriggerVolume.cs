using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using Echo.Management;

namespace Echo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    sealed public class TriggerVolume : MonoBehaviour
    {
        #region Values
        // Determines which layers the volume will ignore.
        private LayerMask _ignoredLayers;
        
        // Defines the game object teleported into the volume that is to be ignored on enter - to avoid teleportation send-back.
        internal GameObject teleportationEnterIgnore;

        // Defines the seconday trigger volume in-which an interseting object will be sent to.
        [SerializeField] private TriggerVolume _teleportationTarget;
        // Determines if the teleported objects rigid body's velocity is to be zeroed out.
        [SerializeField] private bool _zeroOutVelocityOnTeleport;

        [Serializable]
        internal enum OnTrigger
        {
            Enter,
            Stay,
            Exit
        }
        [Serializable]
        internal struct TriggerEvent
        {
            /// <summary>
            /// Defines the condition in-which the event is called.
            /// </summary>
            [SerializeField] internal OnTrigger condition;
            [SerializeField] internal bool triggeredByInput;

            [SerializeField] internal bool destroyIntersecting;
            [SerializeField] internal string[] messagesToIntersecting;
            [SerializeField] internal UnityEvent subscribers;

        }
        [SerializeField] private TriggerEvent[] _events;

        private Collider[] _colliders;
        // A collection of the box collider components of this game object.
        private Collider[] _boxColliders;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {

        }

        private void OnDrawGizmos()
        {
            BoxCollider trigger = null;

            BoxCollider[] boxColliders = this.gameObject.GetComponents<BoxCollider>();
            foreach (BoxCollider boxCollider in boxColliders)
            {
                if (!boxCollider.isTrigger)
                    continue;
                trigger = boxCollider;
                break;
            }

            if (trigger == null)
                return;

            Color color = Color.yellow;
            color.a = 0.6f;

            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawWireCube(trigger.center, trigger.size);

            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(trigger.center, trigger.size);

            Gizmos.matrix = matrixBackup;
        }
        private void OnDrawGizmosSelected()
        {
            BoxCollider trigger = null;
            BoxCollider[] boxColliders = this.gameObject.GetComponents<BoxCollider>();
            foreach (BoxCollider boxCollider in boxColliders)
            {
                if (!boxCollider.isTrigger)
                    continue;
                trigger = boxCollider;
                break;
            }
            if (trigger == null)
                return;

            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(trigger.center, trigger.size);

            Color color = Color.red;
            color.a = 0.5f;
            Gizmos.color = color;

            Gizmos.DrawCube(trigger.center, trigger.size);

            Gizmos.matrix = matrixBackup;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject == teleportationEnterIgnore)
            {
                teleportationEnterIgnore = null;
                return;
            }
            // If the object pertains to an ignored layer, abort
            if (_ignoredLayers == (_ignoredLayers | (1 << collider.gameObject.layer)))
                return;

            // Call triggers
            ProcessTriggers(collider.gameObject, OnTrigger.Enter);
        }
        private void OnTriggerStay(Collider collider)
        {
            // If the object pertains to an ignored layer, abort
            if (_ignoredLayers == (_ignoredLayers | (1 << collider.gameObject.layer)))
                return;

            // Call triggers
            ProcessTriggers(collider.gameObject, OnTrigger.Stay);
        }
        private void OnTriggerExit(Collider collider)
        {
            // If the object pertains to an ignored layer, abort
            if (_ignoredLayers == (_ignoredLayers | (1 << collider.gameObject.layer)))
                return;

            // Call triggers
            if (!collider.GetComponent<Rigidbody>().isKinematic)
                ProcessTriggers(collider.gameObject, OnTrigger.Exit);
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            this.gameObject.layer = Globals.singleton.GetLayerOfType(this);
            _colliders = this.gameObject.GetComponents<Collider>();
            _boxColliders = this.gameObject.GetComponents<BoxCollider>();
        }

        private void ProcessTriggers(GameObject gameObject, OnTrigger condition)
        {
            if(condition == OnTrigger.Enter)
                TeleportGameObject(gameObject);

            foreach (TriggerEvent triggerEvent in _events)
            {
                if (triggerEvent.condition == condition)
                {
                    SendMessagesToIntersecting(gameObject, triggerEvent.messagesToIntersecting);
                    triggerEvent.subscribers.Invoke();
                    if(triggerEvent.destroyIntersecting)
                        Destroy(gameObject);
                }
            }
        }

        internal void TeleportGameObject(GameObject gameObject)
        {
            if (_teleportationTarget == null)
                return;

            if (_zeroOutVelocityOnTeleport)
            {
                Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                    rigidbody.velocity = Vector3.zero;
            }
            _teleportationTarget.teleportationEnterIgnore = gameObject;
            gameObject.transform.position = _teleportationTarget.transform.position;
        }
        internal void SendMessagesToIntersecting(GameObject gameObject, string[] messages)
        {
            foreach (string message in messages)
                gameObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
        }
        
        internal bool BoxesContainPoint(Vector3 point)
        {
            foreach (BoxCollider boxCollider in _boxColliders)
            {
                if (!boxCollider.isTrigger)
                    continue;
                if (BoxContainsPoint(boxCollider, point))
                    return true;
            }
            return false;
        }
        internal bool BoxContainsPoint(BoxCollider boxCollider, Vector3 point)
        {
            point = this.transform.InverseTransformPoint(point) - boxCollider.center;
            Bounds bounds = new Bounds(boxCollider.center, boxCollider.size);
            return bounds.Contains(point);
        }
        #endregion
    }
}