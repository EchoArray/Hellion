using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo.Input
{
    [Serializable]
    sealed public class Controller
    {
        [SerializeField] internal bool fireA;
        [SerializeField] internal bool fireB;
        [SerializeField] internal bool fireC;
        [SerializeField] internal bool fireD;

        [SerializeField] internal bool rise;
        [SerializeField] internal bool fall;
        [SerializeField] internal bool boost;
        [SerializeField] internal bool attach;

        [SerializeField] internal bool jump;
        [SerializeField] internal bool crouch;

        [SerializeField] internal bool interact;
        [SerializeField] internal bool switchWeapon;

        [SerializeField] internal Vector2 movement;
        [SerializeField] internal Vector2 looking;

        internal void Clear()
        {
            fireA = false;
            fireB = false;
            fireC = false;
            fireD = false;

            rise = false;
            fall = false;
            boost = false;
            attach = false;

            jump = false;
            crouch = false;

            interact = false;
            switchWeapon = false;

            movement = Vector2.zero;
            looking = Vector2.zero;
        }
    }
}
