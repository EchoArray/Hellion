using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo.Management
{
    [DisallowMultipleComponent]
    sealed public class DecalManager : MonoBehaviour
    {
        internal static DecalManager singleton;

        #region Values
        [SerializeField] private int _maxDecalsPerFrame = 2;

        /// <summary>
        /// Determines if the game is currently allowing for decals to be cast.
        /// </summary>
        [SerializeField] private bool _allowDecals = true;
        internal bool AllowDecals
        {
            get { return _allowDecals; }
        }

        private List<Decal> _queue = new List<Decal>(); 
        #endregion

        #region Unity Functions
        private void Update()
        {
            UpdateQueue();
        }

        private void Awake()
        {
            singleton = this;
        }
        #endregion

        #region Functions
        private void UpdateQueue()
        {
            Development.Debug.ShowValue("Queued Decal Count: ", _queue.Count);
            Development.Debug.ShowValue("Active Decal Count: ", Globals.singleton.GetContainerOfType(typeof(Decal)).childCount - _queue.Count);
            if (_queue.Count > 0)
            {
                int length = _maxDecalsPerFrame;
                if (_queue.Count < length)
                    length = _queue.Count;

                for (int i = 0; i < length; i++)
                {
                    _queue[0].Project();
                    _queue.RemoveAt(0);
                }
            }
        }
        internal void AddToQueue(Decal decal)
        {
            _queue.Add(decal);
        }

        internal void SetAllowDecals(bool state)
        {
            _allowDecals = state;
            if (!state)
            {
                _queue.Clear();
                Globals.singleton.ClearContainerOfType(typeof(Decal));
            }
        }

        #endregion
    }
}
