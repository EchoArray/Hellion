using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Echo.Management;
using System;

namespace Echo
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ObjectProperties))]
    [DisallowMultipleComponent]
    sealed public class Weapon : Controllable
    {
        #region Values
        /// <summary>
        /// Determines if the weapons uses clip and heat resources.
        /// </summary>
        internal static bool bottomLessClip;
        /// <summary>
        /// Determines if the wepaon uses ammunition and energy resources.
        /// </summary>
        internal static bool infiniteAmmo;

        [SerializeField] protected int _uniqueId;
        internal int UniqueId { get { return _uniqueId; } }

        [SerializeField] private Priority _pickupPriority;
        internal Priority PickupPriority { get { return _pickupPriority; } }

        [SerializeField] private Vector3 _cameraOffset;
        internal Vector3 CameraOffset{ get { return _cameraOffset; } }
        [SerializeField] private string _pickupMessage;
        internal string PickupMessage
        {
            get { return _pickupMessage; }
        }

        [SerializeField] private bool _allowDualWielding;
        internal bool AllowDualWielding { get { return _allowDualWielding; } }
        [SerializeField] private Vector3 _dualWieldCameraOffset;
        internal Vector3 DualWieldCameraOffset { get { return _dualWieldCameraOffset; } }
        [SerializeField] private string _dualWieldPickupMessage;
        internal string DualWieldPickupMessage
        {
            get { return _dualWieldPickupMessage; }
        }

        private bool _held;

        internal bool AllowFire
        {
            get; set;
        }
        internal bool AllowPickup
        {
            get { return !_held && HasResources; }
        }
        internal bool AllowReload
        {
            get
            {
                foreach (Magazine magazine in _magazines)
                    if (magazine.AllowReload)
                        return true;
                return false;
            }
        }
        internal bool HasRoomForResources
        {
            get
            {
                foreach (Magazine magazine in _magazines)
                    if (!magazine.FullAmmo)
                        return true;
                foreach (Battery battery in _batteries)
                    if (!battery.Full)
                        return true;
                return false;
            }
        }
        internal bool HasResources
        {
            get
            {
                foreach (Magazine magazine in _magazines)
                    if (!magazine.AmmoEmpty || !magazine.ClipEmpty)
                        return true;
                foreach (Battery battery in _batteries)
                    if (!battery.Empty)
                        return true;
                return false;
            }
        }


        /// <summary>
        /// Defines the range of the weapon.
        /// </summary>
        [SerializeField] internal float range;


        /// <summary>
        /// Determines if the weapon is reloading.
        /// </summary>
        internal bool Reloading
        {
            get { return _reloadingFinishTime > Time.time; }
        }
        internal bool Overheated
        {
            get {
                if (!HasBattery)
                    return false;
                else
                {
                    foreach (Battery battery in _batteries)
                    {
                        if (battery.overheated)
                            return true;
                    }
                }
                return false;
                ; }
        }
        internal bool HasBattery
        {
            get
            {
                return _batteries.Length != 0;
            }
        }
        internal bool HasMagazine
        {
            get
            {
                return _magazines.Length != 0;
            }
        }
        // Defines the duration in-which it takes to reload the weapon.
        [SerializeField] private float _reloadDuraton;
        // Determines the time in-which the reloading of the weapon will be finished.
        private float _reloadingFinishTime;
        // Determines if the weapon is reloading
        private bool _reloading;

        [Serializable]
        internal sealed class Barrel
        {
            internal enum FiringButton
            {
                AlwaysFire,
                ButtonA,
                ButtonB,
                ButtonC,
                ButtonD,
                UpTest
            }
            /// <summary>
            /// Determines the firing button of the barrel.
            /// </summary>
            [SerializeField] internal FiringButton firingButton;

            /// <summary>
            /// Determines if the firing button was let up the last frame.
            /// </summary>
            internal bool buttonUpLastFrame;
            /// <summary>
            /// Determines if the barrel was fired this frame.
            /// </summary>
            internal bool firedThisFrame;

            internal enum TriggerType
            {
                Single,
                Automatic,
                Charged
            }
            /// <summary>
            /// Determines the trigger type of the button.
            /// </summary>
            [SerializeField] internal TriggerType triggerType;

            /// <summary>
            /// Determines if a charge is released upon button up.
            /// </summary>
            [SerializeField] internal bool ejectChargedOnUp;

            internal bool charged
            {
                get { return chargedDuration >= chargeDuration; }
            }
            /// <summary>
            /// Defines the duration in-which it takes the barrel to charge.
            /// </summary>
            [SerializeField] internal float chargeDuration;
            /// <summary>
            /// Defines the duration of which the barrel has been charged.
            /// </summary>
            [SerializeField] internal float chargedDuration;


            /// <summary>
            /// Defines the marker of the barrel.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Transform barrelMarker;
            /// <summary>
            /// Defines the effect created upon firing the barrel.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal Castable fireEffect;
            /// <summary>
            /// Defines the effect created upon charging the barrel.
            /// </summary>
            [SerializeField]
            internal Castable chargeEffect;
            /// <summary>
            /// Defines the duration of which the barrel will hult fire in range of a recent fire.
            /// </summary>
            [SerializeField] internal float fireRecoveryDuration = 0.01f;
            /// <summary>
            /// Defines the time in-which the barrel will be allowed to fire again.
            /// </summary>
            internal float fireRecoveredTime;
            internal bool FireRecovering { get { return Time.time < fireRecoveredTime; } }
            /// <summary>
            /// Defines the spread of each ejection from the barrel.
            /// </summary>
            [SerializeField]
            internal float spread;

            /// <summary>
            /// Defines the magazine of which the barrel uses.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal int magazineIndex;
            /// <summary>
            /// Defines the batter of which the barrel uses.
            /// </summary>
            [SerializeField] internal int batteryIndex;

            internal bool UsesMagazine { get { return magazineIndex != -1; } }
            internal bool UsesBattery { get { return batteryIndex != -1; } }


            /// <summary>
            /// Defines the energy used from the battery of the barrel.
            /// </summary>
            [SerializeField] internal float energyUse;
            /// <summary>
            /// Defines the head added to the battery of the barrel.
            /// </summary>
            [SerializeField] internal float heatAddition;

            /// <summary>
            /// Determines if the barrel will instantaite an ejection of the type beam for continuous use.
            /// </summary>
            [Space(10)]
            [SerializeField]
            internal bool instantiateBeamType = true;
            /// <summary>
            /// Defines the beam instance of the barrel.
            /// </summary>
            internal Beam beamInstance;
            /// <summary>
            /// Defines the ejection of the barrel.
            /// </summary>
            [SerializeField] internal Castable ejection;
            /// <summary>
            /// Defines the quantity of ejections the barrel will eject upon a fire.
            /// </summary>
            [SerializeField] internal int ejectionsPerFire = 1;
            /// <summary>
            /// Defines the quantity of 
            /// </summary>
            internal int queuedEjections = 0;
            internal bool HasQueue { get { return queuedEjections != 0; } }

            /// <summary>
            /// Defines the duration in-which the barrel will not be able to eject in range of a recent ejection.
            /// </summary>
            [SerializeField] internal float ejectionRecoveryDuration = 0.01f;
            /// <summary>
            /// Defines the time of which the barrel will be allowed to eject again.
            /// </summary>
            internal float ejectionRecoveredTime;
            internal bool EjectionRecovering { get { return Time.time < ejectionRecoveredTime; } }
        }
        [SerializeField] private Barrel[] _barrels;

        [Serializable]
        internal sealed class Magazine
        {
            /// <summary>
            /// Determines the equipment type that supplies ammunition to the magazine.
            /// </summary>
            [SerializeField] internal EquipmentType pairedEquipment;
            /// <summary>
            /// Defines the maximum ammunition of the magazine.
            /// </summary>
            [SerializeField] internal int maxAmmunition;
            /// <summary>
            /// Defines the ammunition of the magazine.
            /// </summary>
            [SerializeField] internal int ammunition;
            /// <summary>
            /// Defines the maximum ammunition that the clip can hold.
            /// </summary>
            [SerializeField] internal int maxClipAmmunition;
            /// <summary>
            /// Defines the ammuntion of the clip.
            /// </summary>
            [SerializeField] internal int clipAmmuntion;

            internal bool AmmoEmpty { get { return ammunition == 0; } }
            internal bool ClipEmpty { get { return clipAmmuntion == 0; } }
            internal bool AllowReload { get { return !AmmoEmpty && clipAmmuntion != maxClipAmmunition; } }
            internal bool FullAmmo { get { return ammunition == maxAmmunition; } }
            internal bool FullClip { get { return clipAmmuntion == maxClipAmmunition; } }
            internal bool AllFull { get { return FullAmmo && FullClip; } }
            internal int FreeSpaceAmmo { get { return maxAmmunition - ammunition; } }
            internal int FreeSpaceClip { get { return maxClipAmmunition - clipAmmuntion; } }
        }
        [SerializeField] private Magazine[] _magazines;

        [Serializable]
        internal sealed class Battery
        {
            /// <summary>
            /// Determines the equipment type that supplies ammunition to the battery.
            /// </summary>
            [SerializeField] internal EquipmentType pairedEquipment;
            /// <summary>
            /// Defines the maximum energy of the battery.
            /// </summary>
            [SerializeField] internal float maxEnergy;
            /// <summary>
            /// Defines the energy of the battery.
            /// </summary>
            [SerializeField] internal float energy;
            /// <summary>
            /// Defines the maximum heat of the battery.
            /// </summary>
            [SerializeField] internal float maxHeat;
            /// <summary>
            /// Defines the heat of the battery.
            /// </summary>
            [SerializeField] internal float heat;
            /// <summary>
            /// Defines the rate of which the heat of battery will cool.
            /// </summary>
            [SerializeField] internal float heatCoolRate;
            /// <summary>
            /// Determines if the battery is overheated.
            /// </summary>
            internal bool overheated;
            /// <summary>
            /// Defines the duration in-which the battery will be overheated.
            /// </summary>
            [SerializeField] internal float overheatDuration;
            /// <summary>
            /// Defines the time of which the battery will begin to dissipate heat.
            /// </summary>
            internal float overheatRecoveredTime;
            /// <summary>
            /// Determines if the battery was used with in the current frame.
            /// </summary>
            internal bool usedThisFrame;

            internal bool Full { get { return energy == maxEnergy; } }
            internal bool Empty { get { return energy == 0; } }
            internal float FreeSpace { get { return maxEnergy - energy; } }
        }
        [SerializeField] private Battery[] _batteries;

        private Rigidbody _rigidBody;
        private BoxCollider _boxCollider;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (HasMagazine)
            {
                Development.Debug.ShowLabel(this, "reloading", Reloading.ToString());
                for (int i = 0; i < _magazines.Length; i++)
                {
                    Development.Debug.ShowLabel(this, "magazine_[" + i + "]_ammunition", _magazines[i].ammunition.ToString());
                    Development.Debug.ShowLabel(this, "magazine_[" + i + "]_clip_ammunition", _magazines[i].clipAmmuntion.ToString());
                }
            }

            if (HasBattery)
            {
                for (int i = 0; i < _batteries.Length; i++)
                {
                    Development.Debug.ShowLabel(this, "battery_[" + i + "]_overheated", _batteries[i].overheated.ToString());
                    Development.Debug.ShowLabel(this, "battery_[" + i + "]_heat", _batteries[i].heat.ToString());
                    Development.Debug.ShowLabel(this, "battery_[" + i + "]_energy", _batteries[i].energy.ToString());
                }
            }

            Fire();
            UpdateHeat();
            UpdateReloading();
            ResetBatteryUse();
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            _rigidBody = this.gameObject.GetComponent<Rigidbody>();
            _boxCollider = this.gameObject.GetComponent<BoxCollider>();
            Globals.singleton.Contain(this);
            foreach (Barrel barrel in _barrels)
                TryInstantiateBeam(barrel);
        }
        public void SetUnique(int id)
        {
            _uniqueId = id;
        }

        internal void Equipt(Biped biped)
        {
            AllowFire = true;
            _held = true;
            _rigidBody.isKinematic = true;
            SetAspects(biped);
        }
        internal void UnEquipt(Vector3 direction)
        {
            _rigidBody.velocity = direction * Globals.singleton.weaponDefaults.dropForce;
            float weaponDropTorque = UnityEngine.Random.Range(-Globals.singleton.weaponDefaults.dropTorque, Globals.singleton.weaponDefaults.dropTorque);
            _rigidBody.angularVelocity = Vector3.one * weaponDropTorque;

            AllowFire = false;
            _held = false;
            _reloading = false;
            _rigidBody.isKinematic = false;
            ClearAllBarrelQueues();
            Release();
        }
        internal void SetVisibility(bool state)
        {
            this.gameObject.SetActive(state);
        }

        internal void Fire()
        {
            if (!AllowFire)
                return;

            if (Reloading)
                return;

            foreach (Barrel barrel in _barrels)
            {
                if (barrel.UsesMagazine)
                {
                    bool reloading = TryReloadEmptyClip(_magazines[barrel.magazineIndex]);
                    if (reloading)
                        return;
                }

                barrel.firedThisFrame = false;
                bool buttonDown = GetFiringButtonState(barrel.firingButton);
                if (buttonDown)
                {
                    bool allowFire = AllowsFiring(barrel);
                    if (!allowFire)
                        continue;
                    bool charged = ChargeBarrel(barrel);
                    if (!charged)
                        continue;
                    barrel.buttonUpLastFrame = false;
                }
                else
                {
                    barrel.buttonUpLastFrame = true;
                    bool ejectCharged = AllowChargedEjectionOnUp(barrel);
                    if (!ejectCharged && !barrel.HasQueue)
                    {
                        barrel.chargedDuration = 0;
                        continue;
                    }
                }

                Fire(barrel);
            }
        }
        private void Fire(Barrel barrel)
        {
            if (!barrel.HasQueue)
            {
                // Set recovered time, apply fire physical effect to caster, eject projectile
                barrel.fireRecoveredTime = Time.time + barrel.fireRecoveryDuration;

                if (barrel.fireEffect != null)
                    barrel.fireEffect.SetCaster(_player.biped);
            }

            int ejections = !barrel.HasQueue ? barrel.ejectionsPerFire : barrel.queuedEjections;
            for (int i = 0; i < ejections; i++)
            {
                if (barrel.EjectionRecovering)
                {
                    if (!barrel.HasQueue)
                        barrel.queuedEjections = barrel.ejectionsPerFire - i;
                    break;
                }
                barrel.queuedEjections = Mathf.Max(barrel.queuedEjections - 1, 0);
                barrel.ejectionRecoveredTime = Time.time + barrel.ejectionRecoveryDuration;

                Eject(barrel, this.transform.forward);
                if (barrel.ejectionRecoveryDuration != 0 || barrel.ejectionRecoveryDuration == 0 && i == 0)
                    UseResources(barrel);
            }
        }
        internal void Eject(Barrel barrel, Vector3 direction)
        {
            barrel.firedThisFrame = true;
            float spreadX = UnityEngine.Random.Range(-barrel.spread, barrel.spread);
            float spreadY = UnityEngine.Random.Range(-barrel.spread, barrel.spread);
            Vector3 spread = new Vector3(spreadX, spreadY, 0);
            spread = this.transform.rotation * spread;

            // Define position and rotation
            Vector3 position = barrel.barrelMarker.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction + spread);

            // Cast beam instance, or instantiate projectile
            if (barrel.beamInstance != null)
            {
                barrel.beamInstance.transform.position = position;
                barrel.beamInstance.transform.rotation = rotation;
                barrel.beamInstance.SetCaster(null);
                barrel.beamInstance.Cast(range);
            }
            else
            {
                if (_player != null)
                    barrel.ejection.SetCaster(_player.biped);

                GameObject newEjectionGameObject = Instantiate(barrel.ejection.gameObject, position, rotation);
                Impactable impactale = newEjectionGameObject.GetComponent<Impactable>();

                Beam beam = newEjectionGameObject.GetComponent<Beam>();
                if (beam != null)
                    beam.Cast(range);
            }
        }

        private void UpdateHeat()
        {
            foreach (Battery battery in _batteries)
            {
                if (battery.usedThisFrame)
                    continue;
                if (battery.overheatRecoveredTime > Time.time)
                    continue;

                battery.heat = Mathf.Max(battery.heat - (Time.deltaTime * battery.heatCoolRate), 0);
                if (battery.overheated && battery.heat == 0)
                    battery.overheated = false;
            }
        }
        private void ResetBatteryUse()
        {
            foreach (Battery battery in _batteries)
                battery.usedThisFrame = false;
        }
        private bool AllowsFiring(Barrel barrel)
        {
            if (barrel.triggerType == Barrel.TriggerType.Single && !barrel.buttonUpLastFrame && !barrel.HasQueue)
                return false;

            if (barrel.FireRecovering && !barrel.HasQueue)
                return false;

            if (barrel.UsesBattery)
            {
                if (_batteries[barrel.batteryIndex].overheated)
                    return false;
                if (_batteries[barrel.batteryIndex].Empty)
                    return false;
            }

            if (barrel.UsesMagazine)
            {
                if (_magazines[barrel.magazineIndex].clipAmmuntion == 0)
                    return false;
            }

            return true;
        }
        private void UseResources(Barrel barrel)
        {
            if (bottomLessClip)
                return;
            if (barrel.UsesBattery)
            {
                Battery battery = _batteries[barrel.batteryIndex];
                battery.usedThisFrame = true;

                if (!infiniteAmmo)
                    battery.energy = Mathf.Max(battery.energy - barrel.energyUse * Time.deltaTime, 0);

                battery.heat = Mathf.Min(battery.heat + barrel.heatAddition * Time.deltaTime, battery.maxHeat);
                if (battery.heat == battery.maxHeat)
                    battery.overheated = true;
                battery.overheatRecoveredTime = Time.time + battery.overheatDuration;
            }
            if (barrel.UsesMagazine)
                _magazines[barrel.magazineIndex].clipAmmuntion -= 1;
        }
        private bool ChargeBarrel(Barrel barrel)
        {
            // Charging
            if (barrel.triggerType == Barrel.TriggerType.Charged)
            {
                if (!barrel.charged)
                {
                    if (!barrel.HasQueue)
                    {
                        barrel.chargedDuration += Time.deltaTime;
                        return false;
                    }
                }
                else
                {
                    if (barrel.ejectChargedOnUp && !barrel.buttonUpLastFrame)
                        return false;
                    barrel.chargedDuration = 0;
                }
            }
            return true;
        }
        private bool AllowChargedEjectionOnUp(Barrel barrel)
        {
            if (barrel.triggerType != Barrel.TriggerType.Charged)
                return false;

            bool ejectCharged = barrel.charged && barrel.ejectChargedOnUp;
            if (!ejectCharged && barrel.ejectChargedOnUp && barrel.HasQueue)
                ejectCharged = true;
            return ejectCharged;
        }
        private void ClearAllBarrelQueues()
        {
            foreach (Barrel barrel in _barrels)
                barrel.queuedEjections = 0;
        }

        internal void Reload()
        {
            if (_reloading)
                return;
            if (!AllowReload)
                return;
            _reloadingFinishTime = Time.time + _reloadDuraton;
            _reloading = true;
        }
        private void ReloadMagazines()
        {
            foreach (Magazine magazine in _magazines)
            {
                if (magazine.AmmoEmpty)
                    continue;
                if (magazine.FullClip)
                    continue;

                int requiredAmmo = magazine.maxClipAmmunition - magazine.clipAmmuntion;
                if (requiredAmmo == 0)
                    continue;

                if (magazine.ammunition < requiredAmmo)
                    requiredAmmo = magazine.ammunition;

                magazine.clipAmmuntion += requiredAmmo;
                if (!infiniteAmmo)
                    magazine.ammunition -= requiredAmmo;
            }
            _reloading = false;
        }
        private void UpdateReloading()
        {
            if (_reloading && _reloadingFinishTime <= Time.time)
                ReloadMagazines();
        }
        internal bool TryReloadEmptyClip(Magazine magazine)
        {
            if (magazine.ClipEmpty && !magazine.AmmoEmpty)
            {
                Reload();
                return true;
            }
            return false;
        }

        internal void ReceiveResources(Weapon weapon)
        {
            foreach (Magazine externalMagazine in weapon._magazines)
            {
                foreach (Magazine internalMagazine in _magazines)
                {
                    if (externalMagazine.pairedEquipment == internalMagazine.pairedEquipment)
                    {
                        externalMagazine.ammunition = TrySetResources(internalMagazine, externalMagazine.ammunition);
                        if (externalMagazine.ammunition == 0)
                            externalMagazine.clipAmmuntion = TrySetResources(internalMagazine, externalMagazine.clipAmmuntion);
                    }
                }
            }
            foreach (Battery externalBattery in weapon._batteries)
            {
                foreach (Battery internalBattery in _batteries)
                {
                    if (externalBattery.pairedEquipment == internalBattery.pairedEquipment)
                        externalBattery.energy = TrySetResources(internalBattery, Mathf.FloorToInt(externalBattery.energy));
                }
            }
        }
        internal void ReceiveResources(Equipment equipment)
        {
            foreach (Magazine magazine in _magazines)
                if (magazine.pairedEquipment == equipment.EquipmentType)
                    equipment.SetResources(TrySetResources(magazine, equipment.Resources));

            foreach (Battery battery in _batteries)
                equipment.SetResources(TrySetResources(battery, equipment.Resources));
        }
        internal int TrySetResources(Battery battery, int resources)
        {
            float energyResources = Mathf.FloorToInt(resources);
            float freeSpace = battery.FreeSpace;
            if (energyResources >= freeSpace)
            {
                energyResources -= freeSpace;
                battery.energy += freeSpace;
            }
            else if (resources != 0)
            {
                energyResources = 0;
                battery.energy += freeSpace;
            }
            return Mathf.FloorToInt(energyResources);
        }
        internal int TrySetResources(Magazine magazine, int resources)
        {
            int freeSpace = magazine.FreeSpaceAmmo;
            if (resources >= freeSpace)
            {
                resources -= freeSpace;
                magazine.ammunition += freeSpace;
            }
            else if (resources != 0)
            {
                resources = 0;
                magazine.ammunition += freeSpace;
            }
            return resources;
        }

        private void TryInstantiateBeam(Barrel barrel)
        {
            if (!barrel.instantiateBeamType)
                return;
            // Try to instantiate the beam of the projectile
            if (barrel.ejection != null && barrel.ejection.GetType() == typeof(Beam))
            {
                GameObject newBeamGameObject = Instantiate(barrel.ejection.gameObject, this.transform.position, this.transform.rotation);
                barrel.beamInstance = newBeamGameObject.GetComponent<Beam>();
            }
        }

        private bool GetFiringButtonState(Barrel.FiringButton firingButton)
        {
            switch (firingButton)
            {
                case Barrel.FiringButton.AlwaysFire:
                    return true;
                    break;
                case Barrel.FiringButton.ButtonA:
                    return _controller != null &&_controller.fireA;
                    break;
                case Barrel.FiringButton.ButtonB:
                    return _controller != null && _controller.fireB;
                    break;
                case Barrel.FiringButton.ButtonC:
                    return _controller != null && _controller.fireC;
                    break;
                case Barrel.FiringButton.ButtonD:
                    return _controller != null && _controller.fireD;
                    break;
            }
            return false;
        }
        #endregion
    }
}
