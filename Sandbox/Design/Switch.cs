using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Echo
{
    sealed internal class Switch : MonoBehaviour
    {
        #region Values
        [SerializeField] private bool _enabled = true;
        internal bool Enabled { get { return _enabled; } }
        [SerializeField] private float _timedDisableDuration;
        internal string Message
        {
            get { return _enabled ? _states[_activeState].message : string.Empty; }
        }

        private int _activeState;
        [Serializable]
        sealed private class State
        {
            [SerializeField] internal string message;
            [SerializeField] internal bool disableOnInvoke;
            [SerializeField] internal UnityEvent subscribers;
        }
        [SerializeField] private State[] _states; 
        #endregion

        #region Unity Functions
        private void OnDrawGizmos()
        {
            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Color color = Color.cyan;
            color.a = 0.6f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.1f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.4f, 1, 1f));
            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, new Vector3(0.4f, 1, 1f));
            Gizmos.matrix = matrixBackup;
        }
        private void OnDrawGizmosSelected()
        {
            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Color color = Color.white;
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.4f, 1, 1f));

            color.a = 0.5f;
            Gizmos.color = color;

            Gizmos.DrawCube(Vector3.zero, new Vector3(0.4f, 1, 1f));
            Gizmos.matrix = matrixBackup;
        }
        #endregion

        #region Functions
        internal void Invoke()
        {
            State state = _states[_activeState];
            state.subscribers.Invoke();

            if (state.disableOnInvoke)
                Disable();

            if (_activeState < (_states.Length - 1))
                _activeState++;
            else
                _activeState = 0;
        }

        public void Enable()
        {
            _enabled = true;
        }

        private void Disable()
        {
            if (_timedDisableDuration > 0)
                StartCoroutine(TimedDisable());
            else
                _enabled = false;
        }
        private IEnumerator TimedDisable()
        {
            _enabled = false;
            yield return new WaitForSeconds(_timedDisableDuration);
            _enabled = true;
        } 
        #endregion
    }
}
