using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    public abstract class Castable : MonoBehaviour
    {
        protected Biped _caster;
        [SerializeField] protected int _uniqueId;
        internal int UniqueId { get { return _uniqueId; } }

        internal virtual void Cast() { }

        public virtual void SetUnique(int id)
        {
            _uniqueId = id;
        }

        internal void SetCaster(Biped caster)
        {
            Castable[] castables = this.gameObject.GetComponents<Castable>();
            foreach (Castable castable in castables)
                castable._caster = caster;
        }
    }
}
