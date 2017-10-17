using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Echo.User;

namespace Echo.Management
{
    [DisallowMultipleComponent]
    sealed public class Globals : MonoBehaviour
    {
        #region Values
        [SerializeField] internal static Globals singleton;

        [SerializeField] internal const int BIPED_LAYER = 8;
        [SerializeField] internal const int WEAPON_LAYER = 9;
        [SerializeField] internal const int EQUIPMENT_LAYER = 10;
        [SerializeField] internal const int PROJECTILE_LAYER = 11;
        [SerializeField] internal const int MACHINE_LAYER = 29;
        [SerializeField] internal const int STRUCTURE_LAYER = 30;
        [SerializeField] internal const int TRIGGER_VOLUME_LAYER = 31;

        [SerializeField] internal CameraEffect baseCameraEffect;

        
        [Serializable]
        sealed internal class BipedDefaults
        {
            [SerializeField] internal LayerMask crouchStandCheckIgnoredLayers;
            [SerializeField] internal float gravity = 20f;
            [SerializeField] internal bool rampUpVelocity = true;
            [SerializeField] internal float interactionRadius = 2f;
        }
        [SerializeField] internal BipedDefaults bipedDefaults;

        [Serializable]
        sealed internal class WeaponDefaults
        {
            [SerializeField] internal float dropForce = 3f;
            [SerializeField] internal float dropTorque = 3f;
        }
        [SerializeField] internal WeaponDefaults weaponDefaults;

        [Serializable]
        sealed internal class CameraDefaults
        {
            public float verticalObliqueOffset = 0.2222222f;
            [Space(10)]
            [SerializeField]
            internal float minVerticalAngle = -90f;
            [SerializeField] internal float maxVerticalAngle = 90f;
            [SerializeField] internal float fieldOfViewTransitionSpeed = 30f;
            [SerializeField] internal float sensitivityMultiplier = 30f;
        }
        [SerializeField] internal CameraDefaults cameraDefaults;

        [Serializable]
        sealed internal class ObjectDefaults
        {
            [SerializeField] internal LayerMask machineLayer;
        }
        [SerializeField] internal ObjectDefaults objectDefaults;


        [Serializable]
        internal struct Containers
        {
            [SerializeField] internal Transform objects;

            [SerializeField] internal Transform players;

            [SerializeField] internal Transform cameras;

            [SerializeField] internal Transform bipeds;
            [SerializeField] internal Transform weapons;
            [SerializeField] internal Transform equipment;
            [SerializeField] internal Transform toys;

            [SerializeField] internal Transform projectiles;
            [SerializeField] internal Transform beams;

            [SerializeField] internal Transform lights;
            [SerializeField] internal Transform decals;
            [SerializeField] internal Transform effectCasters;
            [SerializeField] internal Transform particleEffects;
            [SerializeField] internal Transform physicalEffects;
            [SerializeField] internal Transform cameraEffects;
            [SerializeField] internal Transform flares;
            [SerializeField] internal Transform machines;

        }
        [SerializeField] internal Containers containers;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Application.runInBackground = true;
            Initialize();
        }

        private void Initialize()
        {
            singleton = this;
        }
        #endregion

        #region Functions
        internal void Contain(MonoBehaviour monoBehaviour)
        {
            Type type = monoBehaviour.GetType();
            monoBehaviour.transform.SetParent(GetContainerOfType(type));
        }
        internal void Contain(GameObject gameObject)
        {
            Type type = GetTypeOfGameObject(gameObject);
            gameObject.transform.SetParent(GetContainerOfType(type));
        }
        internal void ClearContainerOfType(Type type)
        {
            Transform transform = GetContainerOfType(type);
            for (int i = 0; i < transform.childCount; i++)
                Destroy(transform.GetChild(i).gameObject);
        }
        internal Transform GetContainerOfType(Type type)
        {
            if (type == typeof(Player))
                return containers.players;
            else if (type == typeof(CameraController))
                return containers.cameras;
            else if (type == typeof(Biped))
                return containers.bipeds;
            else if (type == typeof(Weapon))
                return containers.weapons;
            else if (type == typeof(Equipment))
                return containers.equipment;
            else if (type == typeof(Beam))
                return containers.beams;
            else if (type == typeof(Projectile))
                return containers.projectiles;
            else if (type == typeof(EffectCaster))
                return containers.effectCasters;
            else if (type == typeof(Decal))
                return containers.decals;
            else if (type == typeof(Machine))
                return containers.machines;
            else
                return containers.objects;
        }
        internal Type GetTypeOfGameObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<Player>() != null)
                return typeof(Player);
            else if (gameObject.GetComponent<CameraController>() != null)
                return typeof(CameraController);
            else if (gameObject.GetComponent<Biped>() != null)
                return typeof(Biped);
            else if (gameObject.GetComponent<Weapon>() != null)
                return typeof(Weapon);
            else if (gameObject.GetComponent<Equipment>() != null)
                return typeof(Equipment);
            else if (gameObject.GetComponent<Beam>() != null)
                return typeof(Beam);
            else if (gameObject.GetComponent<Projectile>() != null)
                return typeof(Projectile);
            else if (gameObject.GetComponent<EffectCaster>() != null)
                return typeof(EffectCaster);
            else if (gameObject.GetComponent<Decal>() != null)
                return typeof(Decal);
            else if (gameObject.GetComponent<Machine>() != null)
                return typeof(Machine);

            return null;
        }

        internal int GetLayerOfType(MonoBehaviour monoBehaviour)
        {
            Type type = monoBehaviour.GetType();

            if (type == typeof(Biped))
                return BIPED_LAYER;
            else if (type == typeof(Weapon))
                return WEAPON_LAYER;
            else if (type == typeof(Equipment))
                return EQUIPMENT_LAYER;
            else if (type == typeof(Projectile))
                return PROJECTILE_LAYER;
            else if (type == typeof(TriggerVolume))
                return TRIGGER_VOLUME_LAYER;
            else if (type == typeof(Machine))
                return MACHINE_LAYER;
            return 0;
        }
        #endregion
    }
}