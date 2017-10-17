using Echo.Input;
using Echo.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    public class Controllable : MonoBehaviour
    {
        /// <summary>
        /// Defines the controlling player of the controllable.
        /// </summary>
        protected Player _player;
        // Defines the controller of the controllable.
        protected Controller _controller;
        internal void Release()
        {
            _player = null;
            _controller = null;
        }

        internal virtual void SetAspects(Player player)
        {
            SetAspects(player, player.inputter.controller);
        }
        internal virtual void SetAspects(Controllable controllable)
        {
            SetAspects(controllable._player, controllable._controller);
        }
        private void SetAspects(Player player, Controller controller)
        {
            this._player = player;
            this._controller = controller;
        }
    }
}
