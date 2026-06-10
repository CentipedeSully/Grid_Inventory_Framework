using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace dtsInventory
{
    public enum GIButtonState
    {
        None,
        Disabled,
        Normal,
        Highlighted,
        Pressed,
        Selected
    }

    
    public class GridInvButton: Button
    {
        private GIButtonState _currentState = GIButtonState.None;
        private DisabledOptionFeedback _disabledOptionFeedback;


        public GIButtonState GetCurrentButtonState()
        {
            return _currentState;
        }

        public void SetButtonState(GIButtonState newState)
        {
            if (newState == GIButtonState.None)
                return;

            _currentState = newState;

            switch (newState)
            {
                case GIButtonState.Disabled:
                    if (IsInteractable())
                    {
                        interactable = false;
                        SetDisabledOverlayState(true);
                    }
                    base.DoStateTransition(SelectionState.Disabled,false);
                    break;

                case GIButtonState.Normal:
                    if (!IsInteractable())
                    {
                        interactable = true;
                        SetDisabledOverlayState(false);
                    }
                    base.DoStateTransition(SelectionState.Normal, false);
                    break;

                case GIButtonState.Highlighted:
                    if (!IsInteractable())
                    {
                        interactable = true;
                        SetDisabledOverlayState(false);
                    }
                    base.DoStateTransition(SelectionState.Highlighted, false);
                    break;

                case GIButtonState.Selected:
                    if (!IsInteractable())
                    {
                        interactable = true;
                        SetDisabledOverlayState(false);
                    }
                    base.DoStateTransition(SelectionState.Selected, false);
                    break;

                case GIButtonState.Pressed:
                    if (!IsInteractable())
                    {
                        interactable = true;
                        SetDisabledOverlayState(false);
                    }
                    base.DoStateTransition(SelectionState.Pressed, false);
                    break;
            }
        }
        private void SetDisabledOverlayState(bool newState) 
        {
            //attempt to find the component if it's null
            if (_disabledOptionFeedback == null)
                _disabledOptionFeedback = GetComponent<DisabledOptionFeedback>();

            if (_disabledOptionFeedback != null)
                _disabledOptionFeedback.SetDisabledFeedback(newState);
        }

        public void SetDisabledLabel(string reason)
        {
            //attempt to find the component if it's null
            if (_disabledOptionFeedback == null)
                _disabledOptionFeedback = GetComponent<DisabledOptionFeedback>();

            if ( _disabledOptionFeedback != null)
                _disabledOptionFeedback.SetReasonLabel(reason);
        }
        public void SetDisabledHoverEffect(bool newState)
        {
            //attempt to find the component if it's null
            if (_disabledOptionFeedback == null)
                _disabledOptionFeedback = GetComponent<DisabledOptionFeedback>();

            if (_disabledOptionFeedback != null)
            {
                if (newState)
                    _disabledOptionFeedback.HighlightOverlay();
                else _disabledOptionFeedback.UnHighlightOverlay();
            }
        }

        private GIButtonState SelectionToGIButtonState(SelectionState state)
        {
            switch (state)
            {
                case SelectionState.Disabled:
                    return GIButtonState.Disabled;

                case SelectionState.Normal:
                    return GIButtonState.Normal;

                case SelectionState.Highlighted:
                    return GIButtonState.Highlighted;

                case SelectionState.Selected:
                    return GIButtonState.Selected;

                case SelectionState.Pressed:
                    return GIButtonState.Pressed;

                default:
                    return GIButtonState.None;

                
            }
        }
        
    }
}

