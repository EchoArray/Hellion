using UnityEngine;
using System.Collections;
using System;
using Echo.Management;
using System.Collections.Generic;

namespace Echo
{
    sealed public class EffectCaster : Livable
    {
        #region Values
        // Determines if the effect is to destroy all spawned objects along with its own destruction.
        [SerializeField] private bool _coDestroySpanwed = true;

        [Serializable]
        sealed private class Spawn
        {
            /// <summary>
            /// Defines the spawned game castable.
            /// </summary>
            [SerializeField] internal Castable castable;
            /// <summary>
            /// Defines the position offset the spawed castable.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Vector3 localPosition;
            /// <summary>
            /// Defines the rotation offset the spawed castable.
            /// </summary>
            [SerializeField] internal Vector3 localRotation;
        }
        /// <summary>
        /// A collection of effects to be instantiated upon cast.
        /// </summary>
        [SerializeField] private Spawn[] _spawns;
        // A collection of the game objects spawned.
        private List<GameObject> _spawnedGameObjects = new List<GameObject>();
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
        }
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            DestroyEffects();
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            Globals.singleton.Contain(this);
            Cast();
        }

        internal override void Cast()
        {
            SpawnEffects();
            SoftStartLifespan();
        }

        private void SpawnEffects()
        {
            foreach (Spawn spawn in _spawns)
            {
                if (spawn.castable == null)
                    continue;

                // Define effect position and rotation
                Quaternion localRotation = Quaternion.Euler(spawn.localRotation);
                Quaternion rotation = this.transform.rotation * localRotation;
                Vector3 position = (localRotation * spawn.localPosition) + this.transform.position;

                spawn.castable.SetCaster(_caster);
                GameObject newCastableGameObject = Instantiate(spawn.castable.gameObject, position, rotation, this.transform);
                newCastableGameObject.name = spawn.castable.gameObject.name;
                _spawnedGameObjects.Add(newCastableGameObject);
            }
        }
        private void DestroyEffects()
        {
            if (!_coDestroySpanwed)
                return;

            foreach (GameObject gameObject in _spawnedGameObjects)
                if (gameObject != null)
                    Destroy(gameObject);
        }
        #endregion
    }
}