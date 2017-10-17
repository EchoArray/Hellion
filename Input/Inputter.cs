using Echo.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo.Input
{
    sealed public class Inputter : MonoBehaviour
    {
        [SerializeField] internal bool allowInput;

        private int _inputId;
        private bool _invertVerticalLook;

        public enum GameButton
        {
            FireA,
            FireB,
            FireC,
            FireD,
            Jump,
            Crouch,
            Rise,
            Fall,
            Boost,
            Attach,
            Interact,
            SwitchWeapon
        }
        public enum GameAxis
        {
            MovementX,
            MovementY,
            LookingX,
            LookingY
        }

        [SerializeField] internal Controller controller;
        
        internal class Cascade
        {
            [SerializeField] internal string pcName;
            [SerializeField] internal string playstationName;
        }

        private int _activeButtonSet;
        [Serializable]
        internal class ButtonSet
        {
            [SerializeField] string name;
            [Serializable]
            sealed internal class ButtonCascade : Cascade
            {
                [SerializeField] internal Xbox.Button xboxButton;
                internal enum DownType
                {
                    Button,
                    ButtonDown,
                    ButtonUp
                }
                [SerializeField] internal DownType downType;
                [SerializeField] internal GameButton gameButton;
                [Header("NOTE: Hold duration requires a down type of button.")]
                [SerializeField] internal float holdDuration;
                internal float heldDuration;
                internal bool upSinceLastHold;

            }

            [SerializeField] internal List<ButtonCascade> cascades;

        }
        [SerializeField] private List<ButtonSet> _buttonSets;

        private int _activeAxisSet;
        [Serializable]
        internal class AxisSet
        {
            [SerializeField] string name;
            [Serializable]
            internal class AxisCascade : Cascade
            {
                [SerializeField] internal Xbox.Axis xboxAxis;
                [SerializeField] internal GameAxis gameAxis;
            }
            [SerializeField] internal List<AxisCascade> cascades;

        }
        [SerializeField] private List<AxisSet> _axisSets;
        
        private void Update()
        {
            controller.Clear();
            UpdateButtons();
            UpdateAxes();
        }

        internal void SetAspects(Player player)
        {
            SetAspects(player.profile);
        }
        internal void SetAspects(Profile profile)
        {
            _activeButtonSet = profile.inputSettings.buttonSet;
            _activeAxisSet = profile.inputSettings.axisSet;
            _inputId = profile.inputSettings.inputId;
            _invertVerticalLook = profile.inputSettings.invertVerticalLook;
        }
        
        private void UpdateAxes()
        {
            foreach (AxisSet.AxisCascade axisCascade in _axisSets[_activeAxisSet].cascades)
            {
                if (controller.movement.x == 0 && axisCascade.gameAxis == GameAxis.MovementX)
                    controller.movement.x = GetAxis(axisCascade);
                else if (controller.movement.y == 0 && axisCascade.gameAxis == GameAxis.MovementY)
                    controller.movement.y = GetAxis(axisCascade);
                else if (controller.looking.x == 0 && axisCascade.gameAxis == GameAxis.LookingX)
                    controller.looking.x = GetAxis(axisCascade);
                else if (controller.looking.y == 0 && axisCascade.gameAxis == GameAxis.LookingY)
                {
                    float value = GetAxis(axisCascade);
                    controller.looking.y = _invertVerticalLook ? value : -value;
                }
            }
        }
        private void UpdateButtons()
        {
            foreach (ButtonSet.ButtonCascade buttonCascade in _buttonSets[_activeButtonSet].cascades)
            {
                if (!controller.fireA && buttonCascade.gameButton == GameButton.FireA)
                    controller.fireA = GetButtonState(buttonCascade);
                if (!controller.fireB && buttonCascade.gameButton == GameButton.FireB)
                    controller.fireB = GetButtonState(buttonCascade);
                if (!controller.fireC && buttonCascade.gameButton == GameButton.FireC)
                    controller.fireC = GetButtonState(buttonCascade);
                if (!controller.fireD && buttonCascade.gameButton == GameButton.FireD)
                    controller.fireD = GetButtonState(buttonCascade);

                if (!controller.jump && buttonCascade.gameButton == GameButton.Jump)
                    controller.jump = GetButtonState(buttonCascade);
                if (!controller.crouch && buttonCascade.gameButton == GameButton.Crouch)
                    controller.crouch = GetButtonState(buttonCascade);

                if (!controller.rise && buttonCascade.gameButton == GameButton.Rise)
                    controller.rise = GetButtonState(buttonCascade);
                if (!controller.fall && buttonCascade.gameButton == GameButton.Fall)
                    controller.fall = GetButtonState(buttonCascade);
                if (!controller.boost && buttonCascade.gameButton == GameButton.Boost)
                    controller.boost = GetButtonState(buttonCascade);
                if (!controller.attach && buttonCascade.gameButton == GameButton.Attach)
                    controller.attach = GetButtonState(buttonCascade);

                if (!controller.interact && buttonCascade.gameButton == GameButton.Interact)
                    controller.interact = GetButtonState(buttonCascade);
                if (!controller.switchWeapon && buttonCascade.gameButton == GameButton.SwitchWeapon)
                    controller.switchWeapon = GetButtonState(buttonCascade);
            }
        }

        private float GetAxis(AxisSet.AxisCascade axisCascade)
        {
            float value = 0;
            try
            {
                value += UnityEngine.Input.GetAxis(axisCascade.pcName);
            }
            catch (System.Exception)
            {
                try
                {
                    value += UnityEngine.Input.GetButton(axisCascade.pcName) ? 1 : 0;
                }
                catch (System.Exception)
                {

                }
            }

            value += Xbox.GetAxis(_inputId, axisCascade.xboxAxis);

            return Mathf.Min(value, 1);
        }

        private bool GetButtonState(ButtonSet.ButtonCascade buttonCascade)
        {
            if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.Button)
            {
                bool down = GetButton(buttonCascade);
                if (down)
                {
                    if (buttonCascade.upSinceLastHold)
                        buttonCascade.heldDuration += Time.deltaTime;

                    if (buttonCascade.heldDuration >= buttonCascade.holdDuration)
                    {
                        buttonCascade.heldDuration = 0;
                        buttonCascade.upSinceLastHold = false;
                    }
                    else
                        down = false;
                }
                else
                {
                    buttonCascade.upSinceLastHold = true;
                    buttonCascade.heldDuration = 0;
                }
                return down;
            }
            if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.ButtonDown)
            {
                return GetButtonDown(buttonCascade);
            }
            if (buttonCascade.downType == ButtonSet.ButtonCascade.DownType.ButtonUp)
            {
                return GetButtonUp(buttonCascade);
            }
            return false;
        }
        private bool GetButton(ButtonSet.ButtonCascade buttonCascade)
        {
            try
            {
                if (UnityEngine.Input.GetButton(buttonCascade.pcName))
                    return true;
            }
            catch (System.Exception) { }

            if (Xbox.GetButton(_inputId, buttonCascade.xboxButton))
                return true;
            return false;
        }
        private bool GetButtonDown(ButtonSet.ButtonCascade buttonCascade)
        {
            try
            {
                if (UnityEngine.Input.GetButtonDown(buttonCascade.pcName))
                    return true;
            }
            catch (System.Exception) { }

            if (Xbox.GetButtonDown(_inputId, buttonCascade.xboxButton))
                return true;
            return false;
        }
        private bool GetButtonUp(ButtonSet.ButtonCascade buttonCascade)
        {
            try
            {
                if (UnityEngine.Input.GetButtonUp(buttonCascade.pcName))
                    return true;
            }
            catch (System.Exception) { }

            if (Xbox.GetButtonUp(_inputId, buttonCascade.xboxButton))
                return true;
            return false;
        }

        public void AddVibration(Vibration vibration)
        {
            Xbox.AddVibration(vibration);
        }
    }
}
