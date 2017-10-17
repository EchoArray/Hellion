using Echo.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ObjectProperties))]
    [DisallowMultipleComponent]
    sealed public class Equipment : MonoBehaviour
    {
        // Determines the equipment type of the equipment.
        [SerializeField] private EquipmentType _equipmentType;
        internal EquipmentType EquipmentType { get { return _equipmentType; } }

        [SerializeField] private int resources;
        internal int Resources { get { return resources; } }

        private void Awake()
        {
            Globals.singleton.Contain(this);
            Development.Debug.ShowLabel(this, "resources", resources.ToString());
        }

        internal void SetResources(int value)
        {
            resources = value;
            Development.Debug.ShowLabel(this, "resources", resources.ToString());
            if (resources <= 0)
                Destroy(this.gameObject);
        }
    }
}
