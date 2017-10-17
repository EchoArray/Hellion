using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Echo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ObjectProperties))]
    public class Toy : Castable
    {
        // Defines the selected function of the toy.
        public int selectedFunction;
        
        [Serializable]
        sealed public class Function
        {
            /// <summary>
            /// Defines the name of the functionm
            /// </summary>
            [SerializeField] internal string name;
            /// <summary>
            /// Determines if the function is currently enabled.
            /// </summary>
            [SerializeField] internal bool enabled;
            /// <summary>
            /// Defines the event that will take place upon the calling of the function.
            /// </summary>
            [SerializeField] internal UnityEvent unityEvent;
        }
        [SerializeField] protected Function[] _functions;

        public virtual void Grab()
        {
        }
        public virtual void Drop()
        {
        }
    }
}