using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    public class Livable : Castable
    {
        [SerializeField] protected bool _livesForever;
        // Defines the duration in-which the effect is allowed to live.
        [SerializeField] protected float _lifespan = 1f;

        protected virtual void Awake()
        {
            SoftStartLifespan();
        }

        internal virtual void SoftStartLifespan()
        {
            if(!_livesForever)
                Destroy(this.gameObject, _lifespan);
        }
        internal virtual void StartLifespan()
        {
            Destroy(this.gameObject, _lifespan);
        }
    }
}
