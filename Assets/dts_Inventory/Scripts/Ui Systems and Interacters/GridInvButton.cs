using System.Collections;
using System.Collections.Generic;
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
    public class GridInvButton : Button
    {
        public GIButtonState GetCurrentButtonState()
        {
            return SelectionToGIButtonState(currentSelectionState);
        }

        public void SetButtonState(GIButtonState newState)
        {
            if (newState == GIButtonState.None)
                return;

            switch (newState)
            {
                case GIButtonState.Disabled:
                    base.DoStateTransition(SelectionState.Disabled,false);
                    break;

                case GIButtonState.Normal:
                    base.DoStateTransition(SelectionState.Normal, false);
                    break;

                case GIButtonState.Highlighted:
                    base.DoStateTransition(SelectionState.Highlighted, false);
                    break;

                case GIButtonState.Selected:
                    base.DoStateTransition(SelectionState.Selected, false);
                    break;

                case GIButtonState.Pressed:
                    base.DoStateTransition(SelectionState.Pressed, false);
                    break;
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

