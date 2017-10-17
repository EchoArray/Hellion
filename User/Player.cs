using Echo.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo.User
{
    sealed public class Player : MonoBehaviour
    {
        [SerializeField] internal Profile profile = new Profile();
        [SerializeField] internal Inputter inputter;
        [SerializeField] internal Biped biped;
        [SerializeField] internal CameraController cameraController;

        private void Awake()
        {
            inputter.SetAspects(this);
            biped.SetAspects(this);
            cameraController.SetAspects(this);
        }
    }
}