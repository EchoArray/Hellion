using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo.User
{
    [Serializable]
    sealed internal class Profile
    {
        [Serializable]
        internal class InputSetting
        {
            [SerializeField] internal int inputId;
            [SerializeField] internal int buttonSet;
            [SerializeField] internal int axisSet;
            [SerializeField] internal bool invertVerticalLook;
            [SerializeField] internal int lookSensitivity;
        }
        [SerializeField] internal InputSetting inputSettings;
    }
}