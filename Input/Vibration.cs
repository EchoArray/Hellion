using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo.Input
{
    public abstract class Vibration
    {
        /// <summary>
        /// Defines the id of the controller that is supposed to recieve vibration.
        /// </summary>
        public int controllerId;
        /// <summary>
        /// Defines the intensity of the vibration.
        /// </summary>
        public float intensity;
        /// <summary>
        /// Defines the duration of the vibration.
        /// </summary>
        public float startTime;
        /// <summary>
        /// Defines the remaining duration of the vibration.
        /// </summary>
        public float killTime;

        public Vibration(int controllerID, float intensity, float duration)
        {
            controllerId = controllerID;
            this.intensity = intensity;
            startTime = Time.time;
            killTime = Time.time + duration;
        }
    }
}
