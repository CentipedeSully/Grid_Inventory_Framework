using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



namespace dtsInventory
{
    public interface ILayoutSubcomponent
    {
        GameObject GetGameObject();
        void ActivateSubcomponent(ILayoutSubcomponent self);
        void DeactivateSubcomponent(ILayoutSubcomponent self);
        void ResetSubcomponent(ILayoutSubcomponent self);

        void RespondToDirectionalInput(Vector2 input);
        void RespondToLeftAction();
        void RespondToRightAction();

        void RespondToConfirmInput();
        void RespondToCancelInput();

        void RespondToJumpHotkey();
        void RespondToEditHotkey();

        void ReadAlphaInput(bool input);
        void ReadBetaInput(bool input);
        void ReadGammaInput(bool input);
    }

    /// <summary>
    /// Feeds inputs to the focused subcomponent. Designed to be UnityEvent driven.
    /// </summary>
    public class LayoutManager : MonoBehaviour
    {
        [Header("State Values")]
        private ILayoutSubcomponent _focusedSubcomponent = null;
        [SerializeField] private bool _isLayoutShowing = false;


        //unity events
        public UnityEvent<ILayoutSubcomponent> OnShowTriggered;
        public UnityEvent<ILayoutSubcomponent> OnHideTriggered;




        //Event methods
        public void TrackSubcomponentActivation(ILayoutSubcomponent subcomponent)
        {
            if (subcomponent != null)
                _focusedSubcomponent = subcomponent;
        }
        public void TrackSubcomponentDeactivation(ILayoutSubcomponent subcomponent)
        {
            if (subcomponent != null)
            {
                if (_focusedSubcomponent == subcomponent)
                    _focusedSubcomponent = null;
            }
                
        }


        public void RespondToInventoryHotkey()
        {
            if (!_isLayoutShowing)
            {
                Debug.Log("Showing Inv");
                _isLayoutShowing = true;
                OnShowTriggered?.Invoke(null);
            }
                
            else
            {
                Debug.Log("Hiding Inv");
                _isLayoutShowing = false;
                OnHideTriggered?.Invoke(null);
            }
                
        }

        public void RelayInputToFocusedSubcomponent(Vector2 directionalInput)
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToDirectionalInput(directionalInput);
        }
        public void RelayLeftActionToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToLeftAction();
        }
        public void RelayRightActionToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToRightAction();
        }

        public void RelayConfirmToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToConfirmInput();
        }
        public void RelayCancelToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToCancelInput();
        }

        public void RelayJumpHotkeyToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToJumpHotkey();
        }
        public void RelayEditHotkeyToFocusedSubcomponent()
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.RespondToEditHotkey();
        }


        public void RelayAlphaToFocusedSubcomponent(bool input)
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.ReadAlphaInput(input);
        }
        public void RelayBetaToFocusedSubcomponent(bool input)
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.ReadBetaInput(input);
        }
        public void RelayGammaToFocusedSubcomponent(bool input)
        {
            if (_focusedSubcomponent == null)
                return;

            _focusedSubcomponent.ReadGammaInput(input);
        }

    }
}



